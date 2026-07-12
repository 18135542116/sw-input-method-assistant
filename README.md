# SW 输入法助手

Windows 10/11 x64 的 SolidWorks 输入法辅助工具。

## 功能

- SolidWorks 建模主窗口：切换到英文键盘布局。
- SolidWorks 保存、另存为、打开等文件对话框：切换到中文布局，并请求 IME 中文状态。
- 其他应用：请求中文布局与 IME 中文状态。
- 常驻系统托盘，支持暂停、恢复、开机自启和退出。
- 不注册全局快捷键，不模拟 Win+Space、Ctrl+Space 或其他按键，不读取用户输入内容。

## 可靠性策略

- 只在实际检测到目标键盘布局和 IME 状态生效后，才记录“已确认”。
- 失败、超时或焦点变化时不会去重，定时器会继续重试。
- 跨进程窗口请求使用 300ms 超时保护，避免目标应用无响应时卡住托盘程序。
- 只确认中文布局和 IME 状态；Windows 公共接口无法稳定证明某一具体第三方输入法品牌。

## 环境

- Windows 10/11 x64
- .NET SDK 8.0
- SolidWorks
- 已在 Windows 输入法列表中启用中文输入法；本项目针对微信输入法的已启用配置进行检查。
- Inno Setup 6（仅打包安装程序时需要）

## 构建与测试

在 Windows PowerShell 中执行：

```powershell
Set-Location D:\SWInputMethodAssistant
& "C:\Program Files\dotnet\dotnet.exe" test .\SwInputMethodAssistant.sln
& "C:\Program Files\dotnet\dotnet.exe" publish .\src\SwInputMethodAssistant\SwInputMethodAssistant.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
& "C:\Users\张嘉轩\AppData\Local\Programs\Inno Setup 6\ISCC.exe" .\setup.iss
```

构建产物会生成在 `publish/` 和 `installer-output/`；这些属于可再生文件，不提交到源码仓库。

## 安装

使用 `installer-output/` 中生成的安装包安装。安装包默认安装到当前用户的 LocalAppData，并可选开机自启。

## 隐私

项目不联网、不上传数据、不记录键盘输入。仓库不包含用户配置、输入法个人词库、安装产物或任何密钥。

## 许可

暂未指定开源许可证。未经作者明确授权，请勿将代码用于商业分发或二次发布。
