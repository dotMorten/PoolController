using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Pentair;

namespace PoolController;

public partial class PoolService : ObservableObject
{
    private PoolService()
    {
        Settings.Instance.PropertyChanged += OnSettingsChanged;
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
        else if(e.PropertyName == nameof(Settings.PumpComPort))
        {
            StartPentairClient();
        }
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

public class Settings : ObservableObject
{
    private ApplicationDataContainer localSettings;
    private Settings()
    {
        localSettings = ApplicationData.Current.LocalSettings;
    }
    public static Settings Instance { get; } = new Settings();

    public T GetSetting<T>(T defaultValue, [CallerMemberName] string? key = null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (localSettings.Values.ContainsKey(key))
        {
            var v = localSettings.Values[key];
            if (typeof(T).IsEnum && v is int)
                return (T)v;
            if (v is T value)
                return value;
        }
        return defaultValue;
    }

    public void SetSetting<T>(T value, [CallerMemberName] string? key = null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (typeof(T).IsEnum)
            localSettings.Values[key] = Convert.ChangeType(value, typeof(int));
        else
            localSettings.Values[key] = value;
        OnPropertyChanged(key);
    }

    private static string GetDefaultPort()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "/dev/serial0";
        return string.Empty;
    }
    public string? PumpComPort
    {
        get => GetSetting(GetDefaultPort());
        set => SetSetting(value ?? string.Empty);
    }

    public string? ChlorinatorComPort
    {
        get => GetSetting(string.Empty);
        set => SetSetting(value ?? string.Empty);
    }


    public bool EnableMqtt 
    {
        get => GetSetting(false);
        set => SetSetting(value);
    }

    public string MqttBrokerAddress
    {
        get => GetSetting("homeassistant");
        set => SetSetting(value ?? string.Empty);
    }
    public string MqttUsername
    {
        get => GetSetting(string.Empty);
        set => SetSetting(value ?? string.Empty);
    }
    public string MqttPassword
    {
        get => GetSetting(string.Empty);
        set => SetSetting(value ?? string.Empty);
    }
}
