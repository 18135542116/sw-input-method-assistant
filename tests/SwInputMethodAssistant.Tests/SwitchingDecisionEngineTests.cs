using SwInputMethodAssistant.Domain;
using SwInputMethodAssistant.Services;

namespace SwInputMethodAssistant.Tests;

public sealed class SwitchingDecisionEngineTests
{
    [Fact]
    public void TryGetTarget_retries_until_the_switch_is_recorded_as_verified()
    {
        var engine = new SwitchingDecisionEngine();
        var snapshot = new ForegroundWindowSnapshot("SLDWORKS", "SldWorks", "", "", false);

        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)101, out var first));
        Assert.Equal(InputTarget.English, first);
        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)101, out _));

        engine.RecordVerified((IntPtr)101, first);

        Assert.False(engine.TryGetTarget(false, snapshot, (IntPtr)101, out _));
    }

    [Fact]
    public void TryGetTarget_retries_when_the_foreground_window_changes_even_if_target_is_same()
    {
        var engine = new SwitchingDecisionEngine();
        var snapshot = new ForegroundWindowSnapshot("notepad", "Notepad", "", "", false);

        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)101, out var target));
        engine.RecordVerified((IntPtr)101, target);

        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)202, out var afterWindowChanged));
        Assert.Equal(InputTarget.WeChatChinese, afterWindowChanged);
    }

    [Fact]
    public void TryGetTarget_does_not_suppress_a_window_when_its_prior_switch_was_not_verified()
    {
        var engine = new SwitchingDecisionEngine();
        var snapshot = new ForegroundWindowSnapshot("SLDWORKS", "#32770", "另存为", "", true);

        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)101, out var target));
        Assert.Equal(InputTarget.WeChatChinese, target);

        Assert.True(engine.TryGetTarget(false, snapshot, (IntPtr)101, out var retryTarget));
        Assert.Equal(InputTarget.WeChatChinese, retryTarget);
    }

    [Fact]
    public void TryGetTarget_returns_false_when_paused()
    {
        var engine = new SwitchingDecisionEngine();
        var snapshot = new ForegroundWindowSnapshot("SLDWORKS", "SldWorks", "", "", false);

        Assert.False(engine.TryGetTarget(true, snapshot, (IntPtr)101, out _));
    }
}
