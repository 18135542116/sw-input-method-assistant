using SwInputMethodAssistant.Infrastructure;
using SwInputMethodAssistant.Services;

namespace SwInputMethodAssistant;

public sealed class MainForm : Form
{
    private readonly ForegroundWindowReader _windowReader = new();
    private readonly InputLanguageController _inputLanguageController = new();
    private readonly SwitchingDecisionEngine _decisionEngine = new();
    private readonly StartupRegistration _startupRegistration = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 200 };
    private readonly NotifyIcon _notifyIcon = new();
    private readonly ToolStripMenuItem _pauseMenuItem = new();
    private readonly ToolStripMenuItem _startupMenuItem = new();
    private readonly Label _statusLabel = new();
    private readonly TextBox _logTextBox = new();
    private bool _isPaused;
    private bool _allowClose;

    public MainForm()
    {
        Text = "SW 输入法助手";
        ClientSize = new Size(620, 360);
        MinimumSize = new Size(620, 360);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        _statusLabel.Dock = DockStyle.Top;
        _statusLabel.Height = 48;
        _statusLabel.Padding = new Padding(16, 14, 16, 0);
        _statusLabel.Font = new Font(Font, FontStyle.Bold);

        _logTextBox.Dock = DockStyle.Fill;
        _logTextBox.Multiline = true;
        _logTextBox.ReadOnly = true;
        _logTextBox.ScrollBars = ScrollBars.Vertical;
        _logTextBox.Font = new Font("Consolas", 10);

        Controls.Add(_logTextBox);
        Controls.Add(_statusLabel);

        var menu = new ContextMenuStrip();
        _pauseMenuItem.Click += (_, _) => TogglePaused();
        _startupMenuItem.CheckOnClick = true;
        _startupMenuItem.CheckedChanged += OnStartupCheckedChanged;
        var showMenuItem = new ToolStripMenuItem("打开主窗口", null, (_, _) => ShowMainWindow());
        var exitMenuItem = new ToolStripMenuItem("退出", null, (_, _) => ExitApplication());
        menu.Items.AddRange([showMenuItem, new ToolStripSeparator(), _pauseMenuItem, _startupMenuItem, new ToolStripSeparator(), exitMenuItem]);

        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Text = "SW 输入法助手";
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
        _notifyIcon.Visible = true;

        _timer.Tick += (_, _) => CheckAndSwitch();
        Load += (_, _) => Initialize();
        FormClosing += OnFormClosing;
    }

    private void Initialize()
    {
        _startupMenuItem.Text = "开机自启";
        _startupMenuItem.Checked = _startupRegistration.IsEnabled();
        _pauseMenuItem.Text = "暂停自动切换";
        UpdateStatus();
        Log("程序已启动：不注册快捷键，也不会模拟 Win+空格。");
        _timer.Start();
        CheckAndSwitch();
    }

    private void CheckAndSwitch()
    {
        if (_isPaused)
        {
            return;
        }

        var foregroundWindow = NativeWindowHandle.GetForegroundWindow();
        var snapshot = _windowReader.Read(foregroundWindow);
        if (!_decisionEngine.TryGetTarget(_isPaused, snapshot, foregroundWindow, out var target))
        {
            return;
        }

        if (_inputLanguageController.TryApply(foregroundWindow, target, out var result))
        {
            if (NativeWindowHandle.GetForegroundWindow() == foregroundWindow)
            {
                Log($"{DescribeTarget(target)}：{result}");
                _decisionEngine.RecordVerified(foregroundWindow, target);
            }
            else
            {
                Log("前台窗口已变化，本次不记录切换结果，将按新窗口重新判断。");
                _decisionEngine.Reset();
            }
        }
        else
        {
            Log($"切换未执行：{result}");
            _decisionEngine.Reset();
        }
    }

    private void TogglePaused()
    {
        _isPaused = !_isPaused;
        _decisionEngine.Reset();
        _pauseMenuItem.Text = _isPaused ? "恢复自动切换" : "暂停自动切换";
        UpdateStatus();
        Log(_isPaused ? "已暂停自动切换。" : "已恢复自动切换。");
        if (!_isPaused)
        {
            CheckAndSwitch();
        }
    }

    private void SetStartup(bool enabled)
    {
        try
        {
            _startupRegistration.SetEnabled(enabled);
            Log(enabled ? "已开启当前用户开机自启。" : "已关闭当前用户开机自启。");
        }
        catch (Exception exception)
        {
            _startupMenuItem.CheckedChanged -= OnStartupCheckedChanged;
            _startupMenuItem.Checked = !enabled;
            _startupMenuItem.CheckedChanged += OnStartupCheckedChanged;
            Log($"修改开机自启失败：{exception.Message}");
        }
    }

    private void OnStartupCheckedChanged(object? sender, EventArgs eventArgs)
    {
        SetStartup(_startupMenuItem.Checked);
    }

    private void UpdateStatus()
    {
        _statusLabel.Text = _isPaused
            ? "当前状态：已暂停（不会修改输入法）"
            : "当前状态：自动切换中（SW 主界面英文，文件对话框和其他程序中文）";
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _allowClose = true;
        Close();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs eventArgs)
    {
        if (!_allowClose && eventArgs.CloseReason == CloseReason.UserClosing)
        {
            eventArgs.Cancel = true;
            Hide();
            _notifyIcon.ShowBalloonTip(1500, "SW 输入法助手", "程序仍在托盘运行。右键托盘图标可暂停或退出。", ToolTipIcon.Info);
            return;
        }

        _timer.Stop();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private void Log(string text)
    {
        _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
    }

    private static string DescribeTarget(Domain.InputTarget target) => target == Domain.InputTarget.English ? "SW 建模窗口" : "中文输入场景";
}

internal static class NativeWindowHandle
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();
}
