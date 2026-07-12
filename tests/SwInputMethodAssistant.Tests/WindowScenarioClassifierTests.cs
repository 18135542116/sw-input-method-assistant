using SwInputMethodAssistant.Domain;

namespace SwInputMethodAssistant.Tests;

public sealed class WindowScenarioClassifierTests
{
    [Theory]
    [InlineData("SLDWORKS", "SldWorks", "", false, InputTarget.English)]
    [InlineData("SLDWORKS", "#32770", "", true, InputTarget.WeChatChinese)]
    [InlineData("notepad", "Notepad", "", false, InputTarget.WeChatChinese)]
    public void Classify_returns_expected_input_target(
        string processName,
        string className,
        string ownerProcessName,
        bool isFileDialog,
        InputTarget expected)
    {
        var snapshot = new ForegroundWindowSnapshot(
            processName,
            className,
            "ignored",
            ownerProcessName,
            isFileDialog);

        var actual = WindowScenarioClassifier.Classify(snapshot);

        Assert.Equal(expected, actual);
    }
}
