using SwInputMethodAssistant.Infrastructure;

namespace SwInputMethodAssistant.Tests;

public sealed class WeChatInputProfileTests
{
    [Fact]
    public void Profile_identity_matches_the_enabled_local_wechat_tip()
    {
        Assert.Equal("86598FB9-66A2-463E-B9C2-AEB906D477AD", WeChatInputProfile.TipClsid.ToString().ToUpperInvariant());
        Assert.Equal("607FDF85-FCC8-4DBD-A365-41296F980C9C", WeChatInputProfile.ProfileGuid.ToString().ToUpperInvariant());
        Assert.Equal(0x0804, WeChatInputProfile.LanguageId);
    }
}
