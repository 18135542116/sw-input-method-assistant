$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class ForegroundImeProbe {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
  [DllImport("user32.dll")] public static extern IntPtr GetKeyboardLayout(uint threadId);
  [DllImport("imm32.dll")] public static extern bool ImmIsIME(IntPtr hKL);
  [DllImport("imm32.dll")] public static extern IntPtr ImmGetContext(IntPtr hWnd);
  [DllImport("imm32.dll")] public static extern bool ImmGetOpenStatus(IntPtr hIMC);
  [DllImport("imm32.dll")] public static extern bool ImmGetConversionStatus(IntPtr hIMC, out int conversion, out int sentence);
  [DllImport("imm32.dll")] public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
}
'@

$hwnd = [ForegroundImeProbe]::GetForegroundWindow()
$thread = [ForegroundImeProbe]::GetWindowThreadProcessId($hwnd, [ref]([uint32]0))
$hkl = [ForegroundImeProbe]::GetKeyboardLayout($thread)
$ime = [ForegroundImeProbe]::ImmIsIME($hkl)
$himc = [ForegroundImeProbe]::ImmGetContext($hwnd)
$open = $false
$conversion = 0
$sentence = 0
if ($himc -ne [IntPtr]::Zero) {
  $open = [ForegroundImeProbe]::ImmGetOpenStatus($himc)
  [void][ForegroundImeProbe]::ImmGetConversionStatus($himc, [ref]$conversion, [ref]$sentence)
  [void][ForegroundImeProbe]::ImmReleaseContext($hwnd, $himc)
}
Write-Output ("HWND=" + $hwnd)
Write-Output ("IS_IME=" + $ime)
Write-Output ("IME_OPEN=" + $open)
Write-Output ("CONVERSION=0x" + $conversion.ToString('X8'))
Write-Output ("NATIVE_CHINESE=" + (($conversion -band 1) -ne 0))
