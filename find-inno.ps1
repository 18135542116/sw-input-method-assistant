$ErrorActionPreference = 'Continue'
$roots = @(
  "$env:ProgramFiles\Inno Setup 6",
  "${env:ProgramFiles(x86)}\Inno Setup 6",
  "$env:LOCALAPPDATA\Programs\Inno Setup 6",
  "$env:LOCALAPPDATA\Programs\Inno Setup"
)
foreach ($root in $roots) {
  $candidate = Join-Path $root 'ISCC.exe'
  if (Test-Path -LiteralPath $candidate) { Write-Output $candidate }
}
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall' -ErrorAction SilentlyContinue |
  ForEach-Object { Get-ItemProperty $_.PSPath } |
  Where-Object { $_.DisplayName -like '*Inno*Setup*' } |
  Select-Object DisplayName, InstallLocation, DisplayIcon |
  Format-List
Get-ChildItem 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall' -ErrorAction SilentlyContinue |
  ForEach-Object { Get-ItemProperty $_.PSPath } |
  Where-Object { $_.DisplayName -like '*Inno*Setup*' } |
  Select-Object DisplayName, InstallLocation, DisplayIcon |
  Format-List
