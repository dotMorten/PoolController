using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.Web.WebView2.Core;
using Pentair;
using PoolController.Devices;

namespace PoolController;

public partial class PoolService : ObservableObject
{
    private PoolService()
    {
        Settings.Instance.PropertyChanged += OnSettingsChanged;
        Settings.Instance.TempSettings.PropertyChanged += OnTemperatureSettingsChanged;
        Devices.Temperature.Instance.PropertyChanged += Temperature_PropertyChanged;

        CheckHeatingState();
        // Ensure actuators are in the correct state on startup
        if (Settings.Instance.SolarActuatorId > 0)
            Actuators.Instance.SetActuator(Settings.Instance.SolarActuatorId, IsSolarHeating);
        if (Settings.Instance.VacuumActuatorId > 0)
            Actuators.Instance.SetActuator(Settings.Instance.VacuumActuatorId, Settings.Instance.VacuumEnabled);
    }

    /// <summary>
    /// Called when water and air temperature changes, or if there are changes to the solar heating configuration
    /// </summary>
    private void CheckHeatingState()
    {
        bool isOn = false;
        var solarHeatingState = Settings.Instance.SolarHeatingMode;
        double solarHeatingTemp = Settings.Instance.SolarHeatingTemp;
        if (solarHeatingState == SolarHeatingMode.On)
        {
            isOn = true;
        }
        else if (solarHeatingState == SolarHeatingMode.Auto)
        {
            var airTemp = SolarAirTemperature;
            if (!double.IsNaN(airTemp) && !double.IsNaN(WaterTemperature)) // If we don't have the necessary temperatures, don't change the current state
            {
                if (airTemp - 1 < WaterTemperature) // If the air temp is isn't warmer than the water, don't turn on solar heating to avoid cooling the pool
                {
                    isOn = false;
                }
                else
                {
                    if (WaterTemperature < solarHeatingTemp && WaterTemperature < airTemp)
                        isOn = true;
                    if (isOn != IsSolarHeating)
                    {
                        // Ensure that we are past a threshold of a full degree before flipping back to reduce frequent cycling from
                        // noise in the sensors
                        if (IsSolarHeating && WaterTemperature > solarHeatingTemp + 1)
                        {
                            isOn = false;
                        }
                        else if (!IsSolarHeating && WaterTemperature < solarHeatingTemp - 1 && WaterTemperature < airTemp - 1)
                        {
                            isOn = true;
                        }
                    }
                }
            }
        }
        IsSolarHeating = isOn;
        
        // TODO: Check gas heater
    }

    private bool _isSolarHeating;

    public bool IsSolarHeating
    {
        get { return _isSolarHeating; }
        private set
        {
            if (_isSolarHeating != value)
            {
                _isSolarHeating = value;
                if (Settings.Instance.SolarActuatorId > 0)
                    Actuators.Instance.SetActuator(Settings.Instance.SolarActuatorId, value);
                Log.LogMessage($"Turning {(value ? "on" : "off")} solar heating.");
                OnPropertyChanged();
            }
        }
    }

    public double WaterTemperature => GetTemperature(TemperatureSensorType.WaterTemperature);

    public double AirTemperature => GetTemperature(TemperatureSensorType.AirTemperature);

    public double SolarAirTemperature
    {
        get
        {
            var temp = GetTemperature(TemperatureSensorType.SolarAirTemperature);
            if (double.IsNaN(temp))
            {
                // Fall back to air temp
                temp = GetTemperature(TemperatureSensorType.AirTemperature);
            }
            return temp;
        }
    }

    public double GetTemperature(TemperatureSensorType type)
    {
        if (Settings.Instance.TempSettings.Temp1Type == type)
        {
            return Devices.Temperature.Instance.Temperature1;
        }
        else if (Settings.Instance.TempSettings.Temp2Type == type)
        {
            return Devices.Temperature.Instance.Temperature2;
        }
        else if (Settings.Instance.TempSettings.Temp3Type == type)
        {
            return Devices.Temperature.Instance.Temperature3;
        }
        else if (Settings.Instance.TempSettings.Temp4Type == type)
        {
            return Devices.Temperature.Instance.Temperature4;
        }
        return double.NaN;
    }

    static PoolService()
    {
        Instance = new PoolService();
        Instance.Init();
    }

    private void Init()
    {
        MqttModel.Init();
        StartMqtt();
        StartPentairClient();
    }

    public DispatcherQueue? DispatcherQueue { get; set; }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.EnableMqtt) || e.PropertyName == nameof(Settings.MqttUsername) ||
            e.PropertyName == nameof(Settings.MqttPassword) || e.PropertyName == nameof(Settings.MqttBrokerAddress))
        {
            StartMqtt();
        }
        else if (e.PropertyName == nameof(Settings.PumpComPort))
        {
            StartPentairClient();
        }
        else if (e.PropertyName == nameof(Settings.SolarActuatorId))
        {
            if (Settings.Instance.SolarActuatorId > 0)
            {
                Actuators.Instance.SetActuator(Settings.Instance.SolarActuatorId, IsSolarHeating);
            }
        }
        else if (e.PropertyName == nameof(Settings.SolarHeatingMode) || e.PropertyName == nameof(Settings.SolarHeatingTemp))
        {
            CheckHeatingState();
        }
        else if (e.PropertyName == nameof(Settings.VacuumEnabled) || e.PropertyName == nameof(Settings.VacuumActuatorId))
        {
            if (Settings.Instance.VacuumActuatorId > 0)
            {
                Actuators.Instance.SetActuator(Settings.Instance.VacuumActuatorId, Settings.Instance.VacuumEnabled);
            }
            Log.LogMessage($"Turning {(Settings.Instance.VacuumEnabled ? "on" : "off")} vacuum.");
        }
    }

    private void Temperature_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Temperature" + Settings.Instance.TempSettings.GetTemperatureSensorId(TemperatureSensorType.WaterTemperature))
        {
            OnPropertyChanged(nameof(WaterTemperature));
            CheckHeatingState();
        }
        else if (e.PropertyName == "Temperature" + Settings.Instance.TempSettings.GetTemperatureSensorId(TemperatureSensorType.AirTemperature))
        {
            OnPropertyChanged(nameof(AirTemperature));
            OnPropertyChanged(nameof(SolarAirTemperature));
            CheckHeatingState();
        }
        else if (e.PropertyName == "Temperature" + Settings.Instance.TempSettings.GetTemperatureSensorId(TemperatureSensorType.SolarAirTemperature))
        {
            OnPropertyChanged(nameof(SolarAirTemperature));
            CheckHeatingState();
        }
    }

    private void OnTemperatureSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AirTemperature));
        OnPropertyChanged(nameof(WaterTemperature));
        CheckHeatingState();
    }

    private void StartPentairClient()
    {
        pentairCts?.Cancel();
        pentairCts = null;
        if (PentairClient is not null)
        {
            PentairClient.MessageReceived -= PentairClient_MessageReceived;
            PentairClient?.Dispose();
            PentairClient = null;
        }
        if (string.IsNullOrWhiteSpace(Settings.Instance.PumpComPort))
        {
            return;
        }
        if (!string.IsNullOrEmpty(Settings.Instance.PumpComPort))
        {
            PentairClient = new Pentair.Client(Settings.Instance.PumpComPort);
            PentairClient.MessageReceived += PentairClient_MessageReceived;
            pentairCts = new CancellationTokenSource();
            PentairClientLoop(pentairCts.Token);
        }
    }

    public bool IsPumpRemoteControlled
    {
        get
        {
            if (IsPumpInServiceMode)
                return false;
            return IsUserActive || IsProgramRunning;
        }
    }

    public bool IsProgramRunning => _activeProgram != null && _activeProgram.TimeRemaining > TimeSpan.Zero;

    public bool IsUserActive {get;set;}

    private CancellationTokenSource? pentairCts;

    private  class PumpProgram
    {
        public byte ProgramId { get; set; } = 1;
        public bool IsLocalProgram { get; set; }
        public DateTime EndTime {get; set;}
        public TimeSpan TimeRemaining => EndTime - DateTime.Now;

        public override string ToString()
        {
            if (IsLocalProgram)
                return $"Speed {ProgramId}";
            return $"Program {ProgramId})";
        }
    }

    private PumpProgram? _activeProgram;

    public string ActiveProgram => _activeProgram?.ToString() ?? "";

    public string TimeRemaining
    {
        get
        {
            var time = _activeProgram?.TimeRemaining ?? MqttModel.Timer;
            if(time <= TimeSpan.Zero)
                return "";
            if (time.TotalHours < 1)
                return $"{time.Minutes}m";
                return $"{(int)time.Hours}h {time.Minutes}m";
        }
    }

    private async void PentairClientLoop(CancellationToken cancellationToken)
    {
        if (PentairClient is null)
        {
            return;
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await PentairClient.SendCommandAsync(0x60, Client.RequestStatus);
                if(_activeProgram != null)
                {
                    if (_activeProgram?.TimeRemaining > TimeSpan.FromMinutes(1))
                    {
                        RunProgram(_activeProgram);
                        OnPropertyChanged(nameof(TimeRemaining));
                    }
                    else
                    {
                        _activeProgram = null;
                        OnPropertyChanged(nameof(ActiveProgram));
                        OnPropertyChanged(nameof(TimeRemaining));
                        await PentairClient.SendCommandAsync(0x60, Client.PanelControlOn);
                    }
                }
                if (!IsPumpRemoteControlled) // turn panel control back on after getting status
                    await PentairClient.SendCommandAsync(0x60, Client.PanelControlOn);
            }
            catch
            {
                // Ignore errors for now
            }
            await Task.Delay(IsProgramRunning ? 2000 : IsPumpRemoteControlled ? 10000 : 60000);
        }
    }

    public async void StartLocalProgram(byte programId, TimeSpan duration)
    {
        _activeProgram = new PumpProgram
        {
            ProgramId = programId,
            IsLocalProgram = true,
            EndTime = DateTime.Now.Add(duration)
        };
        PentairClient?.SendCommandAsync(Client.Pump1, Client.StartCommand);
        RunProgram(_activeProgram);
        OnPropertyChanged(nameof(ActiveProgram));
        OnPropertyChanged(nameof(TimeRemaining));
        await Task.Delay(500);
        _ = PentairClient?.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.RequestStatus);
    }

    public async void StartExternalProgram(byte programId, TimeSpan duration)
    {
        _activeProgram = new PumpProgram
        {
            ProgramId = programId,
            IsLocalProgram = false,
            EndTime = DateTime.Now.Add(duration)
        };
        PentairClient?.SendCommandAsync(Client.Pump1, Client.StartCommand);
        RunProgram(_activeProgram);
        OnPropertyChanged(nameof(ActiveProgram));
        OnPropertyChanged(nameof(TimeRemaining));
        await Task.Delay(500);
        _ = PentairClient?.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.RequestStatus);
    }

    public void CancelProgram()
    {
        _activeProgram = null;
        PentairClient?.SendCommandAsync(Client.Pump1, Client.StopCommand);
        OnPropertyChanged(nameof(ActiveProgram));
        OnPropertyChanged(nameof(TimeRemaining));
    }

    private void RunProgram(PumpProgram activeProgram)
    {
        if(PentairClient is null)
            return;
        if (activeProgram.IsLocalProgram)
        {
            _ = PentairClient.StartLocalProgram(Client.Pump1, activeProgram.ProgramId);
        }
        else
        {
            _ = PentairClient.StartExternalProgram(Client.Pump1, activeProgram.ProgramId);
        }
    }

    private void PentairClient_MessageReceived(object? sender, Pentair.Message e)
    {
        if (e is StatusMessage statusMessage)
        {
            // Handle status message
            if (e.Source == 0x60) // Pump 1
            {
               var time = MqttModel.Timer;
               MqttModel.UpdatePumpStatus(statusMessage);
               MqttModel.Timer = _activeProgram?.TimeRemaining ?? statusMessage.Timer;
               DispatcherQueue?.TryEnqueue(() =>
               {
                   if (MqttModel.Clock.Hour != DateTime.Now.Hour || Math.Abs(MqttModel.Clock.Minute - DateTime.Now.Minute - DateTime.Now.Second / 60d) > 1.5)
                   {
                       // Clock is off, update it
                       _ = PentairClient?.SetPumpClock(Pentair.Client.Pump1, (byte)DateTime.Now.Hour, (byte)DateTime.Now.Minute);
                   }
                   if (time != statusMessage.Timer)
                   {
                        OnPropertyChanged(nameof(TimeRemaining));
                   }
               });
            }
        }
    }

    [ObservableProperty]
    private bool _isPumpInServiceMode;

    partial void OnIsPumpInServiceModeChanged(bool value)
    {
        if(value)
            _ = PentairClient?.SendCommandAsync(0x60, Client.PanelControlOn);
        else {
            _ = PentairClient?.SendCommandAsync(0x60, Client.PanelControlOff);
            _ = PentairClient?.SendCommandAsync(0x60, Client.RequestStatus);
        }
    }

    public Models.PoolControllerModel MqttModel { get; } = new Models.PoolControllerModel();


    private async void StartMqtt()
    {
        _ = MqttServer?.StopAsync();
        MqttServer = null;
        if (!Settings.Instance.EnableMqtt)
        {
            return;
        }
        try
        {
            MqttServer = await PoolController.Mqtt.MqttServer.StartServer(Settings.Instance.MqttBrokerAddress, Settings.Instance.MqttUsername, Settings.Instance.MqttPassword);
        }
        catch(System.Exception ex)
        {
            Log.LogError("Failed to start MQTT Server: " + ex.Message);
            Settings.Instance.EnableMqtt = false;
        }
    }

    public static PoolService Instance { get; }

    public Mqtt.MqttServer? MqttServer { get; private set; }

    public Pentair.Client? PentairClient { get; private set; }
}
