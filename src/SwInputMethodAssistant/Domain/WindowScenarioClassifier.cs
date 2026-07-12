namespace SwInputMethodAssistant.Domain;

public enum InputTarget
{
    English,
    WeChatChinese
}

public sealed record ForegroundWindowSnapshot(
    string ProcessName,
    string ClassName,
    string Title,
    string OwnerProcessName,
    bool IsFileDialog);

public static class WindowScenarioClassifier
{
    private const string SolidWorksProcessName = "SLDWORKS";

    public static InputTarget Classify(ForegroundWindowSnapshot snapshot)
    {
        var belongsToSolidWorks = string.Equals(
            snapshot.ProcessName,
            SolidWorksProcessName,
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                snapshot.OwnerProcessName,
                SolidWorksProcessName,
                StringComparison.OrdinalIgnoreCase);

        return belongsToSolidWorks && !snapshot.IsFileDialog
            ? InputTarget.English
            : InputTarget.WeChatChinese;
    }
}
