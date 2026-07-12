$ErrorActionPreference = 'Stop'
$installer = 'D:\SWInputMethodAssistant\installer-output\SW输入法助手安装包_v1.0.0.exe'
if (-not (Test-Path -LiteralPath $installer)) { throw 'Installer missing.' }
$arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/TASKS="autostart"')
$process = Start-Process -FilePath $installer -ArgumentList $arguments -PassThru -Wait
Write-Output ("INSTALL_EXIT=" + $process.ExitCode)
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\SW输入法助手'
$exe = Join-Path $installDir 'SwInputMethodAssistant.exe'
Write-Output ("EXE_EXISTS=" + (Test-Path -LiteralPath $exe))
$runValue = (Get-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'SwInputMethodAssistant' -ErrorAction SilentlyContinue).SwInputMethodAssistant
Write-Output ("AUTOSTART_EXISTS=" + (-not [string]::IsNullOrWhiteSpace($runValue)))
if (Test-Path -LiteralPath $exe) {
  $app = Start-Process -FilePath $exe -PassThru
  Start-Sleep -Seconds 2
  Write-Output ("INSTALLED_APP_STARTED=" + (-not $app.HasExited))
  if (-not $app.HasExited) { Stop-Process -Id $app.Id -Force }
}
$uninstaller = Join-Path $installDir 'unins000.exe'
if (Test-Path -LiteralPath $uninstaller) {
  $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -PassThru -Wait
  Write-Output ("UNINSTALL_EXIT=" + $uninstall.ExitCode)
  Write-Output ("EXE_REMOVED=" + (-not (Test-Path -LiteralPath $exe)))
}
