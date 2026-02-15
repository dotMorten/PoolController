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
            if (double.IsNaN(SolarAirTemperature)) // If we don't have a solar air sensor, fallback to air temp
            {
                airTemp = AirTemperature;
            }
            if (!double.IsNaN(airTemp) && !double.IsNaN(WaterTemperature)) // If we don't have the necessary temperatures, don't change the current state
            {
                if(airTemp < WaterTemperature - 1) // If the air temp is significantly lower than the water temp, don't turn on solar heating to avoid cooling the pool
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

    public double SolarAirTemperature => GetTemperature(TemperatureSensorType.SolarAirTemperature);

    private double GetTemperature(TemperatureSensorType type)
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
        else if (e.PropertyName?.StartsWith("Solar") == true)
        {
            CheckHeatingState();
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
            CheckHeatingState();
        }
        else if (e.PropertyName == "Temperature" + Settings.Instance.TempSettings.GetTemperatureSensorId(TemperatureSensorType.SolarAirTemperature))
        {
            CheckHeatingState();
        }
        else if (e.PropertyName == nameof(Settings.SolarHeatingMode) || e.PropertyName == nameof(Settings.SolarHeatingTemp))
        {
            CheckHeatingState();
        }
        else if (e.PropertyName == nameof(Settings.VacuumEnabled))
        {
            if (Settings.Instance.VacuumActuatorId > 0)
            {
                Actuators.Instance.SetActuator(Settings.Instance.VacuumActuatorId, Settings.Instance.VacuumEnabled);
            }
            Log.LogMessage($"Turning {(Settings.Instance.VacuumEnabled ? "on" : "off")} vacuum.");
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

    private CancellationTokenSource? pentairCts;

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
                if (IsPumpInServiceMode) // turn panel control back on after getting status
                    await PentairClient.SendCommandAsync(0x60, Client.PanelControlOn);
            }
            catch
            {
                // Ignore errors for now
            }
            await Task.Delay(IsPumpInServiceMode ? 60000 : 10000);
        }
    }

    private void PentairClient_MessageReceived(object? sender, Pentair.Message e)
    {
        if (e is StatusMessage statusMessage)
        {
            // Handle status message
            if (e.Source == 0x60) // Pump 1
            {
               DispatcherQueue?.TryEnqueue(() =>
               {
                   PumpStatus.Power = statusMessage.Power;
                   PumpStatus.PumpSpeed = statusMessage.Rpm;
                   PumpStatus.EstimatedFlow = statusMessage.Gpm;
                   // PumpStatus.Ppc = statusMessage.Ppc;
                   // PumpStatus.Error = statusMessage.Error;
                   PumpStatus.Clock = statusMessage.Clock;
                   PumpStatus.State = statusMessage.State;
                   PumpStatus.Running = statusMessage.Run;
                   PumpStatus.Mode = statusMessage.Mode;
                   PumpStatus.Timer = statusMessage.Timer;
                   if(PumpStatus.Clock.Hour != DateTime.Now.Hour || Math.Abs(PumpStatus.Clock.Minute - DateTime.Now.Minute - DateTime.Now.Second / 60d) > 1.5)
                   {
                       // Clock is off, update it
                       _ = PentairClient?.SetPumpClock(Pentair.Client.Pump1, (byte)DateTime.Now.Hour, (byte)DateTime.Now.Minute);
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

    public Models.PoolPumpModel PumpStatus { get; } = new Models.PoolPumpModel();

    public Models.ChlorinatorModel ChlorinatorStatus { get; } = new Models.ChlorinatorModel();

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
        catch
        {
            Settings.Instance.EnableMqtt = false;
        }
    }

    public static PoolService Instance { get; }

    public Mqtt.MqttServer? MqttServer { get; private set; }

    public Pentair.Client? PentairClient { get; private set; }
}
