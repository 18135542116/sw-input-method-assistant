$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
  throw "未找到 Inno Setup 编译器：$compiler"
}
& $compiler 'D:\SWInputMethodAssistant\setup.iss'
exit $LASTEXITCODE
