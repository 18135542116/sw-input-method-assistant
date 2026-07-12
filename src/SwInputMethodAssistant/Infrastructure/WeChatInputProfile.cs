namespace SwInputMethodAssistant.Infrastructure;

public static class WeChatInputProfile
{
    // 来自本机已启用 WeType 文本服务的注册表身份：
    // HKLM\SOFTWARE\Microsoft\CTF\TIP\{86598FB9-66A2-463E-B9C2-AEB906D477AD}
    // \LanguageProfile\0x00000804\{607FDF85-FCC8-4DBD-A365-41296F980C9C}
    public static readonly Guid TipClsid = new("86598FB9-66A2-463E-B9C2-AEB906D477AD");
    public static readonly Guid ProfileGuid = new("607FDF85-FCC8-4DBD-A365-41296F980C9C");
    public const short LanguageId = 0x0804;
}
