namespace PoolController.Models;

public partial class PoolControllerModel : ObservableObject
{
    public PoolControllerModel()
    {
    }

    internal void Init()
    {
        PoolService.Instance.PropertyChanged += Service_PropertyChanged;
        Settings.Instance.PropertyChanged += Settings_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.SolarHeatingMode):
                SolarHeatingMode = Settings.Instance.SolarHeatingMode; break;
            case nameof(Settings.SolarHeatingTemp):
                SolarTargetTemperature = Settings.Instance.SolarHeatingTemp; break;
            case nameof(Settings.VacuumEnabled):
                VacuumEnabled = Settings.Instance.VacuumEnabled; break;
        }
    }

    private void Service_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PoolService.AirTemperature):
                AirTemperature = PoolService.Instance.AirTemperature; break;
            case nameof(PoolService.SolarAirTemperature):
                SolarAirTemperature = PoolService.Instance.SolarAirTemperature; break;
            case nameof(PoolService.WaterTemperature):
                WaterTemperature = PoolService.Instance.WaterTemperature; break;
            case nameof(PoolService.IsSolarHeating):
                IsSolarHeating = PoolService.Instance.IsSolarHeating; break;
            case nameof(PoolService.IsPumpInServiceMode):
                PumpServiceMode = PoolService.Instance.IsPumpInServiceMode; break;
        }
    }

    internal void UpdatePumpStatus(Pentair.StatusMessage statusMessage)
    {
        Power = statusMessage.Power;
        PumpSpeed = statusMessage.Rpm;
        EstimatedFlow = statusMessage.Gpm;
        // PumpStatus.Ppc = statusMessage.Ppc;
        // PumpStatus.Error = statusMessage.Error;
        Clock = statusMessage.Clock;
        State = statusMessage.State;
        Running = statusMessage.Run;
        Mode = statusMessage.Mode;
        Timer = statusMessage.Timer;
    }

    #region Pump

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
    private TimeSpan _timer;

    [ObservableProperty]
    private Pentair.PumpState _state = Pentair.PumpState.Normal;

    [ObservableProperty]
    private Pentair.PumpRunning _running = Pentair.PumpRunning.Stopped;

    [ObservableProperty]
    private bool _pumpServiceMode = false;

    public void ToggleOn(bool on)
    {  
        IsOn = on; 
    }

    [ObservableProperty]
    private bool _vacuumEnabled = false;

    partial void OnVacuumEnabledChanged(bool value)
    {
        Settings.Instance.VacuumEnabled = value;
    }

    #endregion

    #region Temperature/Heating

    [ObservableProperty]
    private double _waterTemperature = double.NaN;

    [ObservableProperty]
    private double _airTemperature = double.NaN;

    [ObservableProperty]
    private double _solarAirTemperature = double.NaN;

    [ObservableProperty]
    private double _returnWaterTemperature = double.NaN;

    [ObservableProperty]
    private bool _isSolarHeating = false;

    [ObservableProperty]
    private double _solarTargetTemperature = double.NaN;

    [ObservableProperty]
    private SolarHeatingMode _solarHeatingMode = SolarHeatingMode.Off;

    #endregion

    #region Chlorinator

    [ObservableProperty]
    private double _chlorinatorSaltLevel = 3500;

    [ObservableProperty]
    private int _chlorinatorPercentage = 50;

    [ObservableProperty]
    private double _chlorinatorTemperature = double.NaN;

    #endregion
}
