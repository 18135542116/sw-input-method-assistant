using System.Runtime.InteropServices;
using System.Text;
using SwInputMethodAssistant.Domain;

namespace SwInputMethodAssistant.Infrastructure;

public sealed class ForegroundWindowReader
{
    public ForegroundWindowSnapshot? Read(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        NativeMethods.GetWindowThreadProcessId(handle, out var processId);
        var processName = GetProcessName(processId);
        var className = ReadClassName(handle);
        var title = ReadWindowText(handle);
        var ownerProcessName = GetOwnerChainProcessName(handle);

        return new ForegroundWindowSnapshot(
            processName,
            className,
            title,
            ownerProcessName,
            IsStandardFileDialog(className, ownerProcessName, processName));
    }

    private static string GetProcessName(uint processId)
    {
        try
        {
            return System.Diagnostics.Process.GetProcessById((int)processId).ProcessName;
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
    }

    private static string GetOwnerChainProcessName(IntPtr handle)
    {
        var visited = new HashSet<IntPtr>();
        var owner = NativeMethods.GetWindow(handle, NativeMethods.GwOwner);
        while (owner != IntPtr.Zero && visited.Add(owner))
        {
            NativeMethods.GetWindowThreadProcessId(owner, out var ownerProcessId);
            var ownerProcessName = GetProcessName(ownerProcessId);
            if (string.Equals(ownerProcessName, "SLDWORKS", StringComparison.OrdinalIgnoreCase))
            {
                return ownerProcessName;
            }

            owner = NativeMethods.GetWindow(owner, NativeMethods.GwOwner);
        }

        return string.Empty;
    }

    private static string ReadClassName(IntPtr handle)
    {
        var buffer = new StringBuilder(256);
        NativeMethods.GetClassName(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string ReadWindowText(IntPtr handle)
    {
        var length = NativeMethods.GetWindowTextLength(handle);
        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static bool IsStandardFileDialog(
        string className,
        string ownerProcessName,
        string processName)
    {
        var isOwnedBySolidWorks = string.Equals(ownerProcessName, "SLDWORKS", StringComparison.OrdinalIgnoreCase);
        var isSolidWorksProcess = string.Equals(processName, "SLDWORKS", StringComparison.OrdinalIgnoreCase);

        return string.Equals(className, "#32770", StringComparison.Ordinal)
            && (isOwnedBySolidWorks || isSolidWorksProcess);
    }
}

internal static class NativeMethods
{
    internal const uint GwOwner = 4;

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
}
