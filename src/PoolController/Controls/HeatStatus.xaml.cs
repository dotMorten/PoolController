using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PoolController.Controls;

public sealed partial class HeatStatus : UserControl
{
    public HeatStatus()
    {
        this.InitializeComponent();
        UpdateHeatingState();
        Settings.PropertyChanged += OnSettingsChanged;
        Service.PropertyChanged += OnServiceChanged;
    }

    private void OnServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Service.IsSolarHeating) || e.PropertyName == nameof(Service.SolarAirTemperature) || e.PropertyName == nameof(Service.WaterTemperature))
        {
            UpdateHeatingState();
        }
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.SolarHeatingTemp) || e.PropertyName == nameof(Settings.SolarHeatingMode))
        {
            UpdateHeatingState();
        }
    }

    public PoolService Service => PoolService.Instance;

    public Settings Settings => Settings.Instance;

    private void UpdateHeatingState()
    {
        if (Settings.SolarHeatingMode == SolarHeatingMode.On)
        {
            StatusText.Text = $"Solar Heating On";
            StatusText.Visibility = Visibility.Visible;
        }
        else if(Settings.SolarHeatingMode == SolarHeatingMode.Auto)
        {
            if (Service.IsSolarHeating)
            {
                StatusText.Text = $"Heating to {Settings.SolarHeatingTemp}°F";
            }
            else
            {
                StatusText.Text = $"Waiting for solar to reach at least {(Service.WaterTemperature+1).ToString("0.0")}°F (Currently {Service.SolarAirTemperature.ToString("0.0")}°F)";
            }
            StatusText.Visibility = Visibility.Visible;
        }
        else
        {
            StatusText.Visibility = Visibility.Collapsed;
            StatusText.Text = "";
        }
    }
}