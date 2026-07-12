using SwInputMethodAssistant.Domain;

namespace SwInputMethodAssistant.Services;

public sealed class SwitchingDecisionEngine
{
    private IntPtr _lastVerifiedWindow;
    private InputTarget? _lastVerifiedTarget;

    public bool TryGetTarget(
        bool isPaused,
        ForegroundWindowSnapshot? snapshot,
        IntPtr foregroundWindow,
        out InputTarget target)
    {
        target = default;
        if (isPaused || snapshot is null || foregroundWindow == IntPtr.Zero)
        {
            return false;
        }

        target = WindowScenarioClassifier.Classify(snapshot);
        return _lastVerifiedWindow != foregroundWindow || _lastVerifiedTarget != target;
    }

    public void RecordVerified(IntPtr foregroundWindow, InputTarget target)
    {
        if (foregroundWindow == IntPtr.Zero)
        {
            return;
        }

        _lastVerifiedWindow = foregroundWindow;
        _lastVerifiedTarget = target;
    }

    public void Reset()
    {
        _lastVerifiedWindow = IntPtr.Zero;
        _lastVerifiedTarget = null;
    }
}
