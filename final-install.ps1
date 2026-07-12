$ErrorActionPreference = 'Stop'
$installer = Get-ChildItem -LiteralPath 'D:\SWInputMethodAssistant\installer-output' -Filter '*.exe' | Select-Object -First 1
if ($null -eq $installer) { throw 'Installer missing.' }
$arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/TASKS="autostart,desktopicon"')
$process = Start-Process -FilePath $installer.FullName -ArgumentList $arguments -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Install failed: $($process.ExitCode)" }
Write-Output 'FINAL_INSTALL_OK=True'
