using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace PoolController.Views.Settings;

public sealed partial class SensorSettings : UserControl
{
    public SensorSettings()
    {
        this.InitializeComponent();
        Sensor1Selector.SelectedIndex = (int)Settings.Temp1Type;
        Sensor2Selector.SelectedIndex = (int)Settings.Temp2Type;
        Sensor3Selector.SelectedIndex = (int)Settings.Temp3Type;
        Sensor4Selector.SelectedIndex = (int)Settings.Temp4Type;
    }

    public PoolController.TempSensorSettings Settings => PoolController.Settings.Instance.TempSettings;

    public Devices.Temperature Sensors => Devices.Temperature.Instance;

    private void Sensor1SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.Temp1Type = (TemperatureSensorType)Sensor1Selector.SelectedIndex;
    }
    private void Sensor2SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.Temp2Type = (TemperatureSensorType)Sensor2Selector.SelectedIndex;
    }
    private void Sensor3SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.Temp3Type = (TemperatureSensorType)Sensor3Selector.SelectedIndex;
    }
    private void Sensor4SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.Temp4Type = (TemperatureSensorType)Sensor4Selector.SelectedIndex;
    }
}
