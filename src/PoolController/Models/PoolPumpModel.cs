using Iot.Device.Gpio.Drivers;
using PoolController.Devices;

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
        Settings.Instance.TempSettings.PropertyChanged += TempSettings_PropertyChanged;
        Temperature.Instance.PropertyChanged += Temperature_PropertyChanged;
    }

    private void Temperature_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        TemperatureSensorType type = TemperatureSensorType.Disabled;
        var temp = double.NaN;
        switch (e.PropertyName)
        {
            case nameof(Temperature.Temperature1):
                temp = Temperature.Instance.Temperature1;
                type = Settings.Instance.TempSettings.Temp1Type;
                break;
            case nameof(Temperature.Temperature2):
                temp = Temperature.Instance.Temperature2;
                type = Settings.Instance.TempSettings.Temp2Type; break;
            case nameof(Temperature.Temperature3):
                temp = Temperature.Instance.Temperature3;
                type = Settings.Instance.TempSettings.Temp3Type; break;
            case nameof(Temperature.Temperature4):
                temp = Temperature.Instance.Temperature4;
                type = Settings.Instance.TempSettings.Temp4Type; break;
        }
        switch (type)
        {
            case TemperatureSensorType.AirTemperature:
                AirTemperature = temp; break;
            case TemperatureSensorType.SolarAirTemperature:
                SolarAirTemperature = temp; break;
            case TemperatureSensorType.WaterTemperature:
                WaterTemperature = temp; break;
            case TemperatureSensorType.ReturnTemperature:
                ReturnWaterTemperature = temp; break;
        }
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
    private void TempSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // If settings change, refresh all
        WaterTemperature = PoolService.Instance.GetTemperature(TemperatureSensorType.WaterTemperature);
        ReturnWaterTemperature = PoolService.Instance.GetTemperature(TemperatureSensorType.ReturnTemperature);
        AirTemperature= PoolService.Instance.GetTemperature(TemperatureSensorType.AirTemperature);
        SolarAirTemperature = PoolService.Instance.GetTemperature(TemperatureSensorType.SolarAirTemperature);
        // Aux1Temperature = PoolService.Instance.GetTemperature(TemperatureSensorType.Aux1);
        // Aux2Temperature = PoolService.Instance.GetTemperature(TemperatureSensorType.Aux2);
        // Aux3Temperature = PoolService.Instance.GetTemperature(TemperatureSensorType.Aux3);
        // Aux4Temperature = PoolService.Instance.GetTemperature(TemperatureSensorType.Aux4);
    }

    private void Service_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
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

    public void SetSolarTargetTemperature(double temp) => SolarTargetTemperature = temp;

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
