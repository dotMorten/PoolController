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

namespace PoolController.Controls;

public sealed partial class PumpControl : UserControl
{
    public PumpControl()
    {
        this.InitializeComponent();
        serviceModeButton.IsChecked = Service.IsPumpInServiceMode;
        serviceModeButton.Content = Service.IsPumpInServiceMode ? "Disable Service Mode" : "Enable Service Mode";
        UpdateStopButton();
        program1Button.IsEnabled = stopButton.IsEnabled = !Service.IsPumpInServiceMode;
    }

    public PoolService Service => PoolService.Instance;

    private void serviceModeButton_Click(object sender, RoutedEventArgs e)
    {
        Service.IsPumpInServiceMode = !Service.IsPumpInServiceMode;
        serviceModeButton.Content = Service.IsPumpInServiceMode ? "Disable Service Mode" : "Enable Service Mode";
        program1Button.IsEnabled = stopButton.IsEnabled = !Service.IsPumpInServiceMode;
    }

    private async void stopButton_Click(object sender, RoutedEventArgs e)
    {
        if (Service.PentairClient is null)
            return;
        if (Service.PumpStatus.Running == Pentair.PumpRunning.Stopped)
        {
            stopButton.Content = "Starting...";
            await Service.PentairClient.Start(Pentair.Client.Pump1);
            await Service.PentairClient.GetStatusAsync(Pentair.Client.Pump1);
        }
        else
        {
            stopButton.Content = "Stopping...";
            await Service.PentairClient.Stop(Pentair.Client.Pump1);
            await Service.PentairClient.GetStatusAsync(Pentair.Client.Pump1);
        }
        UpdateStopButton();
    }

    private void UpdateStopButton()
    {
        stopButton.Content = Service.PumpStatus.Running == Pentair.PumpRunning.Stopped ? "Start" : "Stop";
    }

    private void program1Button_Click(object sender, RoutedEventArgs e)
    {
        Service.PentairClient?.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.StartProgram2);
    }
}
