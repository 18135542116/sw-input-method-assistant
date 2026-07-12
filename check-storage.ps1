$paths = @(
  (Join-Path $env:LOCALAPPDATA 'SW输入法助手'),
  (Join-Path $env:APPDATA 'SW输入法助手')
)
foreach ($path in $paths) {
  Write-Output ("PATH=$path; EXISTS=" + (Test-Path -LiteralPath $path))
  if (Test-Path -LiteralPath $path) {
    Get-ChildItem -LiteralPath $path -Force -Recurse | ForEach-Object {
      Write-Output ("FILE=" + $_.FullName + "; SIZE=" + $_.Length)
    }
  }
}
$run = (Get-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name SwInputMethodAssistant -ErrorAction SilentlyContinue).SwInputMethodAssistant
Write-Output "AUTOSTART=$run"
