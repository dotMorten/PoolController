using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PoolController;

public class SettingsBase : INotifyPropertyChanged
{
    private ApplicationDataContainer localSettings;
    private string keyPrefix;

    protected SettingsBase(string keyPrefix = "")
    {
        localSettings = ApplicationData.Current.LocalSettings;
        this.keyPrefix = keyPrefix;
    }

    protected void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public T GetSetting<T>(T defaultValue, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null) throw new ArgumentNullException(nameof(propertyName));
        if (localSettings.Values.ContainsKey(keyPrefix + propertyName))
        {
            var v = localSettings.Values[keyPrefix + propertyName];
            if (typeof(T).IsEnum && v is int)
                return (T)v;
            if (v is T value)
                return value;
        }
        return defaultValue;
    }

    public void SetSetting<T>(T value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null) throw new ArgumentNullException(nameof(propertyName));
        if (typeof(T).IsEnum)
            localSettings.Values[keyPrefix + propertyName] = Convert.ChangeType(value, typeof(int));
        else
            localSettings.Values[keyPrefix + propertyName] = value;
        OnPropertyChanged(propertyName);
    }
}

public partial class Settings : SettingsBase
{
    public static Settings Instance { get; } = new Settings();

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

    public TempSensorSettings TempSettings { get; } = new TempSensorSettings();

    public SolarHeatingMode SolarHeatingMode
    {
        get => GetSetting(SolarHeatingMode.Auto);
        set => SetSetting(value);
    }

    public double SolarHeatingTemp
    {
        get => GetSetting(85d);
        set => SetSetting(value);
    }

    public int SolarActuatorId
    {
        get => GetSetting(1);
        set => SetSetting(value);
    }

    public int VacuumActuatorId
    {
        get => GetSetting(2);
        set => SetSetting(value);
    }

    public bool VacuumEnabled
    {
        get => GetSetting(false);
        set => SetSetting(value);
    }
}
public enum SolarHeatingMode : int
{
    Auto = 0,
    On = 1,
    Off = 2
}

public partial class TempSensorSettings : SettingsBase
{
    internal TempSensorSettings() : base("Temp") { }

    public TemperatureSensorType Temp1Type
    {
        get => GetSetting(TemperatureSensorType.Disabled);
        set => SetSetting(value);
    }

    public TemperatureSensorType Temp2Type
    {
        get => GetSetting(TemperatureSensorType.Disabled);
        set => SetSetting(value);
    }

    public TemperatureSensorType Temp3Type
    {
        get => GetSetting(TemperatureSensorType.Disabled);
        set => SetSetting(value);
    }

    public TemperatureSensorType Temp4Type
    {
        get => GetSetting(TemperatureSensorType.Disabled);
        set => SetSetting(value);
    }

    public double Temp1Offset
    {
        get => GetSetting(0d);
        set => SetSetting(value);
    }

    public double Temp2Offset
    {
        get => GetSetting(0d);
        set => SetSetting(value);
    }
    
    public double Temp3Offset
    {
        get => GetSetting(0d);
        set => SetSetting(value);
    }

    public double Temp4Offset
    {
        get => GetSetting(0d);
        set => SetSetting(value);
    }

    public int GetTemperatureSensorId(TemperatureSensorType type)
    {
        if (Temp1Type == type)
            return 1;
        if (Temp2Type == type)
            return 2;
        if (Temp3Type == type)
            return 3;
        if (Temp4Type == type)
            return 4;
        return 0;
    }
}

public enum TemperatureSensorType : int
{
    Disabled = 0,
    WaterTemperature = 1,
    ReturnTemperature = 2,
    AirTemperature = 3,
    SolarAirTemperature = 4,
    Aux1 = 5,
    Aux2 = 6,
    Aux3 = 7,
    Aux4 = 8
}
