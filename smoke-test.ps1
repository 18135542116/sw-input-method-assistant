$ErrorActionPreference = 'Stop'
$exe = 'D:\SWInputMethodAssistant\publish\SwInputMethodAssistant.exe'
$process = Start-Process -FilePath $exe -PassThru
Start-Sleep -Seconds 3
$alive = -not $process.HasExited
Write-Output ("STARTED=" + $alive)
if ($alive) {
  Stop-Process -Id $process.Id -Force
  Write-Output 'STOPPED=True'
}
