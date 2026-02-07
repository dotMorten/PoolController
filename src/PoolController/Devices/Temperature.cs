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
    private readonly Ads1115 adc;

    private Temperature()
    {
        if (!System.OperatingSystem.IsLinux())
            return;
        I2cConnectionSettings settings = new(1, (int)I2cAddress.GND);
        I2cDevice device = I2cDevice.Create(settings);
        device = I2cDevice.Create(settings);
        adc = new Ads1115(device, InputMultiplexer.AIN0, MeasuringRange.FS4096, DataRate.SPS250, DeviceMode.Continuous);
        StartReadLoop();
    }

    public DispatcherQueue? DispatcherQueue { get; set; }

    private async void StartReadLoop()
    {
        while(true)
        {
            double temp1 = ReadTemperatureF(InputMultiplexer.AIN0);
            double temp2 = ReadTemperatureF(InputMultiplexer.AIN1);
            double temp3 = ReadTemperatureF(InputMultiplexer.AIN2);
            double temp4 = ReadTemperatureF(InputMultiplexer.AIN3);
            double avg1 = GetRollingAverage(samples1, temp1);
            if(Math.Abs(Temperature1 - avg1) >= 0.1)
            {
                Temperature1 = avg1;
                Temperature1Changed?.Invoke(this, Temperature1);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature1)));
                });
            }
            double avg2 = GetRollingAverage(samples2, temp2);
            if (Math.Abs(Temperature2 - avg2) >= 0.1)
            {
                Temperature2 = avg2;
                Temperature2Changed?.Invoke(this, Temperature2);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature2)));
                });
            }
            double avg3 = GetRollingAverage(samples3, temp3);
            if (Math.Abs(Temperature3 - avg3) >= 0.1)
            {
                Temperature3 = avg3;
                Temperature3Changed?.Invoke(this, Temperature3);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature3)));
                });
            }
            double avg4 = GetRollingAverage(samples4, temp4);
            if (Math.Abs(Temperature4 - avg4) >= 0.1)
            {
                Temperature4 = avg4;
                Temperature4Changed?.Invoke(this, Temperature4);
                DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature4)));
                });
            }
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    public event EventHandler<double>? Temperature1Changed;
    public event EventHandler<double>? Temperature2Changed;
    public event EventHandler<double>? Temperature3Changed;
    public event EventHandler<double>? Temperature4Changed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double Temperature1 { get; private set; }

    public double Temperature2 { get; private set; }

    public double Temperature3 { get; private set; }

    public double Temperature4 { get; private set; }

    private static double GetRollingAverage(Queue<double> samples, double newSample, int maxSamples = 10)
    {
        samples.Enqueue(newSample);
        if (samples.Count > maxSamples)
            samples.Dequeue();
        return Math.Round(samples.Sum() / samples.Count, 1);
    }

    public static Temperature Instance { get; } = new Temperature();

    private double ReadTemperatureF(InputMultiplexer input)
    {
        ElectricPotential voltage = ReadVoltage(input);
        double resistance = (3300 - voltage.Millivolts) * 10000 / voltage.Millivolts;
        double temperatureC = 1 / (Math.Log(resistance / 10000) / 3950 + 1 / (25 + 273.15)) - 273.15;
        return temperatureC * 9 / 5 + 32;
    }

    private ElectricPotential ReadVoltage(InputMultiplexer input)
    {
        ElectricPotential voltage = adc.ReadVoltage(input);
        return voltage;
    }
}
