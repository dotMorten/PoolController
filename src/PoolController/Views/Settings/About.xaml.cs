using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

public sealed partial class About : UserControl
{
    public About()
    {
        this.InitializeComponent();
        Version.Text = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
    }

    protected override void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
    {
        if (newValue == Visibility.Visible)
        {
            double temp = GetCPUTemperature();
            if (!double.IsNaN(temp))
            {
                CpuTemp.Text = $"CPU Temperature: {temp:F1} °C";
            }
            else
            {
                CpuTemp.Text = "";
            }
        }
        var ethernet = GetLocalIPv4(NetworkInterfaceType.Ethernet);
        var wifi = GetLocalIPv4(NetworkInterfaceType.Wireless80211);
        Network.Text = $"Ethernet: {(string.IsNullOrEmpty(ethernet) ? "N/A" : ethernet)}\nWi-Fi: {(string.IsNullOrEmpty(wifi) ? "N/A" : wifi)}";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private static double GetCPUTemperature()
    {
        try
        {
            if (OperatingSystem.IsLinux())
            {
                if (File.Exists("/sys/class/thermal/thermal_zone0/temp") &&
                    int.TryParse(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp"), out int temp))
                {
                    return temp / 1000d;
                }
            }
            else if(OperatingSystem.IsWindows())
            {
                return 35.3; // Just simulate a value on Windows for testing purposes
            }
        }
        catch { }
        return double.NaN;
    }

    private static string GetLocalIPv4(NetworkInterfaceType type)
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == type && ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        return ip.Address.ToString();
                }
            }
        }
        return string.Empty;
    }
}
