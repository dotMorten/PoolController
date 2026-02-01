namespace PoolController.Models;

public partial class PoolPumpModel : ObservableObject
{
    [ObservableProperty]
    private int _pumpSpeed = 0;

    [ObservableProperty]
    private int _power = 0;

    [ObservableProperty]
    private int _estimatedFlow = 0;

    [ObservableProperty]
    private TimeOnly _clock;

    [ObservableProperty]
    private bool _isOn;

    [ObservableProperty]
    private Pentair.PumpMode _mode = Pentair.PumpMode.Unknown;

    [ObservableProperty]
    private Pentair.PumpState _state = Pentair.PumpState.Normal;

    [ObservableProperty]
    private Pentair.PumpRunning _running = Pentair.PumpRunning.Stopped;

    public void ToggleOn(bool on)
    {  
        IsOn = on; 
    }
}
