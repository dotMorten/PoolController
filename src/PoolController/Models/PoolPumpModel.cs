internal partial class PoolPumpModel : ObservableObject
{
    [ObservableProperty]
    private int _pumpSpeed = 0;

    [ObservableProperty]
    private int _power = 0;

    [ObservableProperty]
    private int _estimatedFlow = 0;

    [ObservableProperty]
    private int _currentTime = 10000;

    [ObservableProperty]
    private bool _isOn;

    [ObservableProperty]
    private PumpState _state = PumpState.Running; // "Running";

    public void ToggleOn(bool on)
    {  
        IsOn = on; 
    }

    public enum PumpState : int
    {
        Running,
        Priming,
        SystemPriming,
        FaultMode
    }
}
