using System.ComponentModel;
using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.UI.Dispatching;
using UnitsNet;

namespace PoolController.Devices;

public class Temperature : INotifyPropertyChanged
{
    private readonly Queue<double> samples1 = new Queue<double>();
    private readonly Queue<double> samples2 = new Queue<double>();
    private readonly Queue<double> samples3 = new Queue<double>();
    private readonly Queue<double> samples4 = new Queue<double>();
    private readonly Ads1115? adc;

    private Temperature()
    {
        Settings.Instance.TempSettings.PropertyChanged += TempSettings_PropertyChanged;
        try
        {
            I2cConnectionSettings settings = new(1, (int)I2cAddress.GND);
            I2cDevice device = I2cDevice.Create(settings);
            device = I2cDevice.Create(settings);
            adc = new Ads1115(device, InputMultiplexer.AIN0, MeasuringRange.FS4096, DataRate.SPS250, DeviceMode.Continuous);
            StartReadLoop();
        }
        catch(System.Exception ex)
        {
            Log.LogError("Failed to start temperature sensor read loop: " + ex.Message);
        }
    }

    private void TempSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TempSensorSettings.Temp1Offset))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature1)));
        else if (e.PropertyName == nameof(TempSensorSettings.Temp2Offset))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature2)));
        else if (e.PropertyName == nameof(TempSensorSettings.Temp3Offset))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature3)));
        else if (e.PropertyName == nameof(TempSensorSettings.Temp4Offset))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature4)));

    }

    public DispatcherQueue? DispatcherQueue { get; set; }

    private async void StartReadLoop()
    {
        Log.LogMessage("Temperature sensor read loop started.");
        while(true)
        {
            double temp1 = ReadTemperatureF(InputMultiplexer.AIN0);
            double temp2 = ReadTemperatureF(InputMultiplexer.AIN1);
            double temp3 = ReadTemperatureF(InputMultiplexer.AIN2);
            double temp4 = ReadTemperatureF(InputMultiplexer.AIN3);
            double avg1 = GetRollingAverage(samples1, temp1);
            if(double.IsNaN(_temperature1) || Math.Abs(Temperature1 - avg1) >= 0.1)
            {
                this._temperature1 = avg1;
                Temperature1Changed?.Invoke(this, _temperature1);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature1)));
                });
            }
            double avg2 = GetRollingAverage(samples2, temp2);
            if (double.IsNaN(_temperature2) || Math.Abs(_temperature2 - avg2) >= 0.1)
            {
                this._temperature2 = avg2;
                Temperature2Changed?.Invoke(this, Temperature2);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature2)));
                });
            }
            double avg3 = GetRollingAverage(samples3, temp3);
            if (double.IsNaN(_temperature3) || Math.Abs(_temperature3 - avg3) >= 0.1)
            {
                this._temperature3 = avg3;
                Temperature3Changed?.Invoke(this, Temperature3);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature3)));
                });
            }
            double avg4 = GetRollingAverage(samples4, temp4);
            if (double.IsNaN(_temperature4) || Math.Abs(_temperature4 - avg4) >= 0.1)
            {
                this._temperature4 = avg4;
                Temperature4Changed?.Invoke(this, Temperature4);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature4)));
                });
            }
            await Task.Delay(5000).ConfigureAwait(false);
        }
    }

    public event EventHandler<double>? Temperature1Changed;
    public event EventHandler<double>? Temperature2Changed;
    public event EventHandler<double>? Temperature3Changed;
    public event EventHandler<double>? Temperature4Changed;

    public event PropertyChangedEventHandler? PropertyChanged;

    private double _temperature1 = double.NaN;
    public double Temperature1 { get => _temperature1 + Settings.Instance.TempSettings.Temp1Offset; }

    private double _temperature2 = double.NaN;
    public double Temperature2 { get => _temperature2 + Settings.Instance.TempSettings.Temp2Offset; }

    private double _temperature3 = double.NaN;
    public double Temperature3 { get => _temperature3 + Settings.Instance.TempSettings.Temp3Offset; }

    private double _temperature4 = double.NaN;
    public double Temperature4 { get => _temperature4 + Settings.Instance.TempSettings.Temp4Offset; }

    private static double GetRollingAverage(Queue<double> samples, double newSample, int maxSamples = 30)
    {
        samples.Enqueue(newSample);
        if (samples.Count > maxSamples)
            samples.Dequeue();

        // Oldest item weight = 1, newest item weight = Count
        double weightedSum = 0.0;
        int weightTotal = 0;
        int i = 1; // weight starts at 1 for oldest

        foreach (double s in samples)
        {
            weightedSum += s * i;
            weightTotal += i;
            i++;
        }

        double avg = weightedSum / weightTotal;
        return Math.Round(avg, 1);
    }

    public static Temperature Instance { get; } = new Temperature();

    // Constants for thermistor calculation
    private const double SupplyVoltageMv = 5100.0; // millivolts
    private const double FixedResistorOhms = 10000.0; // ohms
    private const double ReferenceResistanceOhms = 10000.0; // ohms
    private const double BetaCoefficient = 3950.0; // beta value
    private const double ReferenceTemperatureC = 25.0; // Celsius

    private double ReadTemperatureF(InputMultiplexer input)
    {
        ElectricPotential voltage = ReadVoltage(input);
        double measuredVoltageMv = voltage.Millivolts;

        // Calculate thermistor resistance using voltage divider formula
        double resistance = (SupplyVoltageMv - measuredVoltageMv) * FixedResistorOhms / measuredVoltageMv;

        // Convert resistance to temperature in Celsius using Steinhart-Hart equation
        double referenceTempK = ReferenceTemperatureC + 273.15;
        double temperatureC = 1 / (Math.Log(resistance / ReferenceResistanceOhms) / BetaCoefficient + 1 / referenceTempK) - 273.15;

        // Convert Celsius to Fahrenheit
        return temperatureC * 9 / 5 + 32;
    }

    private ElectricPotential ReadVoltage(InputMultiplexer input)
    {
        ElectricPotential voltage = adc?.ReadVoltage(input) ?? new ElectricPotential(double.NaN, UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        return voltage;
    }
}
