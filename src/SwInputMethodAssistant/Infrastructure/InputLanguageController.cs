using System.Runtime.InteropServices;
using Microsoft.Win32;
using SwInputMethodAssistant.Domain;

namespace SwInputMethodAssistant.Infrastructure;

public sealed class InputLanguageController
{
    public const string EnglishLayout = "00000409";
    // 当前用户已启用的中文（简体）键盘布局；在本机该布局唯一启用的 TIP 为微信输入法。
    public const string ChineseSimplifiedLayout = "00000804";

    private const uint WmInputLangChangeRequest = 0x0050;
    private const string WeChatTipRegistryPath =
        @"SOFTWARE\Microsoft\CTF\TIP\{86598FB9-66A2-463E-B9C2-AEB906D477AD}\LanguageProfile\0x00000804\{607FDF85-FCC8-4DBD-A365-41296F980C9C}";

    public bool IsWeChatInputMethodAvailable()
    {
        using var key = Registry.LocalMachine.OpenSubKey(WeChatTipRegistryPath, writable: false);
        return key?.GetValue("Enable") is int enabled && enabled == 1;
    }

    public bool TryApply(
        IntPtr foregroundWindow,
        InputTarget target,
        bool allowTsfFallback,
        out string message)
    {
        if (foregroundWindow == IntPtr.Zero)
        {
            message = "没有可切换的前台窗口。";
            return false;
        }

        if (target == InputTarget.WeChatChinese && !IsWeChatInputMethodAvailable())
        {
            message = "未检测到已启用的微信输入法，已停止切换请求。";
            return false;
        }

        var layout = NativeMethods.LoadKeyboardLayout(
            target == InputTarget.English ? EnglishLayout : ChineseSimplifiedLayout,
            0);

        if (layout == IntPtr.Zero)
        {
            message = target == InputTarget.WeChatChinese
                ? "无法载入中文（简体）键盘布局，请先在 Windows 输入法列表启用微信输入法。"
                : "无法载入英文键盘布局。";
            return false;
        }

        var inputWindow = NativeMethods.GetFocusedWindow(foregroundWindow);
        if (!NativeMethods.TrySendInputLanguageRequest(
            inputWindow,
            WmInputLangChangeRequest,
            IntPtr.Zero,
            layout))
        {
            message = "输入法请求超时或被窗口拒绝，将继续重试。";
            return false;
        }

        if (NativeMethods.GetForegroundWindow() != foregroundWindow
            || NativeMethods.GetFocusedWindow(foregroundWindow) != inputWindow)
        {
            message = "切换期间焦点已变化，将按当前焦点重新判断。";
            return false;
        }

        var hasExpectedLayout = HasExpectedLayout(inputWindow, target);
        if (!hasExpectedLayout)
        {
            message = target == InputTarget.WeChatChinese
                ? "中文输入法布局尚未生效，将继续重试。"
                : "英文键盘布局尚未生效，将继续重试。";
            return false;
        }

        if (target == InputTarget.WeChatChinese)
        {
            var modeStatus = TryEnableChineseMode(inputWindow);
            if (!ChineseModeVerification.IsApplied(hasExpectedLayout, modeStatus, allowTsfFallback))
            {
                message = "微信输入法切换未确认：该窗口的输入法状态尚未生效，将继续重试。";
                return false;
            }

            message = modeStatus == ChineseModeStatus.LegacyImmContextUnavailable
                ? "已确认中文布局生效；当前输入框无法通过传统 IMM 接口验证中文模式。"
                : "已确认中文布局和 IME 状态生效。";
            return true;
        }

        message = "已确认英文键盘布局生效。";
        return true;
    }

    private static bool HasExpectedLayout(IntPtr inputWindow, InputTarget target)
    {
        var threadId = NativeMethods.GetWindowThreadProcessId(inputWindow, out _);
        var activeLayout = NativeMethods.GetKeyboardLayout(threadId).ToInt64();
        var languageId = (ushort)(activeLayout & 0xffff);
        var expectedLanguageId = target == InputTarget.English ? 0x0409 : 0x0804;
        return languageId == expectedLanguageId;
    }

    private static ChineseModeStatus TryEnableChineseMode(IntPtr foregroundWindow)
    {
        var inputContext = NativeMethods.ImmGetContext(foregroundWindow);
        if (inputContext == IntPtr.Zero)
        {
            return ChineseModeStatus.LegacyImmContextUnavailable;
        }

        try
        {
            if (!NativeMethods.ImmSetOpenStatus(inputContext, true)
                || !NativeMethods.ImmGetConversionStatus(inputContext, out var conversion, out var sentence))
            {
                return ChineseModeStatus.NotNative;
            }

            var desired = conversion | NativeMethods.ImeCmodeNative;
            if (!NativeMethods.ImmSetConversionStatus(inputContext, desired, sentence)
                || !NativeMethods.ImmGetConversionStatus(inputContext, out var actual, out _))
            {
                return ChineseModeStatus.NotNative;
            }

            return (actual & NativeMethods.ImeCmodeNative) != 0
                ? ChineseModeStatus.Native
                : ChineseModeStatus.NotNative;
        }
        finally
        {
            NativeMethods.ImmReleaseContext(foregroundWindow, inputContext);
        }
    }

    private static class NativeMethods
    {
        internal const int ImeCmodeNative = 0x0001;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint flags);

        private const uint SmtoAbortIfHung = 0x0002;
        private const uint SendTimeoutMilliseconds = 300;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SendMessageTimeout(
            IntPtr hWnd,
            uint msg,
            IntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo lpgui);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        internal static bool TrySendInputLanguageRequest(
            IntPtr inputWindow,
            uint message,
            IntPtr wParam,
            IntPtr lParam) => SendMessageTimeout(
                inputWindow,
                message,
                wParam,
                lParam,
                SmtoAbortIfHung,
                SendTimeoutMilliseconds,
                out _);

        internal static IntPtr GetFocusedWindow(IntPtr foregroundWindow)
        {
            var threadId = GetWindowThreadProcessId(foregroundWindow, out _);
            var info = new GuiThreadInfo { cbSize = (uint)Marshal.SizeOf<GuiThreadInfo>() };
            return GetGUIThreadInfo(threadId, ref info) && info.hwndFocus != IntPtr.Zero
                ? info.hwndFocus
                : foregroundWindow;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct GuiThreadInfo
        {
            internal uint cbSize;
            internal uint flags;
            internal IntPtr hwndActive;
            internal IntPtr hwndFocus;
            internal IntPtr hwndCapture;
            internal IntPtr hwndMenuOwner;
            internal IntPtr hwndMoveSize;
            internal IntPtr hwndCaret;
            internal RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;
        }

        [DllImport("imm32.dll")]
        internal static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmSetOpenStatus(
            IntPtr hIMC,
            [MarshalAs(UnmanagedType.Bool)] bool fOpen);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmGetConversionStatus(
            IntPtr hIMC,
            out int lpfdwConversion,
            out int lpfdwSentence);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmSetConversionStatus(
            IntPtr hIMC,
            int fdwConversion,
            int fdwSentence);

        [DllImport("imm32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
    }
}

internal enum ChineseModeStatus
{
    LegacyImmContextUnavailable,
    NotNative,
    Native,
}

internal static class ChineseModeVerification
{
    public static bool IsApplied(
        bool hasExpectedLayout,
        ChineseModeStatus modeStatus,
        bool allowTsfFallback) =>
        hasExpectedLayout
        && (modeStatus == ChineseModeStatus.Native
            || allowTsfFallback && modeStatus == ChineseModeStatus.LegacyImmContextUnavailable);
}
