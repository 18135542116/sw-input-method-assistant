using SwInputMethodAssistant.Infrastructure;

namespace SwInputMethodAssistant.Tests;

public sealed class ChineseModeVerificationTests
{
    [Fact]
    public void Accepts_file_dialog_fallback_when_chinese_layout_is_active_without_a_legacy_imm_context()
    {
        Assert.True(ChineseModeVerification.IsApplied(
            hasExpectedLayout: true,
            ChineseModeStatus.LegacyImmContextUnavailable,
            allowTsfFallback: true));
    }

    [Fact]
    public void Rejects_an_unverifiable_context_outside_a_file_dialog()
    {
        Assert.False(ChineseModeVerification.IsApplied(
            hasExpectedLayout: true,
            ChineseModeStatus.LegacyImmContextUnavailable,
            allowTsfFallback: false));
    }

    [Fact]
    public void Rejects_a_legacy_imm_context_that_is_not_in_native_mode()
    {
        Assert.False(ChineseModeVerification.IsApplied(
            hasExpectedLayout: true,
            ChineseModeStatus.NotNative,
            allowTsfFallback: true));
    }

    [Fact]
    public void Accepts_a_legacy_imm_context_in_native_mode()
    {
        Assert.True(ChineseModeVerification.IsApplied(
            hasExpectedLayout: true,
            ChineseModeStatus.Native,
            allowTsfFallback: false));
    }

    [Fact]
    public void Rejects_a_file_dialog_fallback_when_the_expected_layout_is_not_active()
    {
        Assert.False(ChineseModeVerification.IsApplied(
            hasExpectedLayout: false,
            ChineseModeStatus.LegacyImmContextUnavailable,
            allowTsfFallback: true));
    }

    [Fact]
    public void Rejects_native_mode_when_the_expected_layout_is_not_active()
    {
        Assert.False(ChineseModeVerification.IsApplied(
            hasExpectedLayout: false,
            ChineseModeStatus.Native,
            allowTsfFallback: false));
    }
}
