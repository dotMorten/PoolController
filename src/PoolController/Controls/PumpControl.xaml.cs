using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Dispatching;
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
        this.Loaded += (s, e) =>
        {
            Service.PumpStatus.PropertyChanged += OnPumpPropertyChanged;
        };
        this.Unloaded += (s, e) =>
        {
            Service.PumpStatus.PropertyChanged -= OnPumpPropertyChanged;
        };
    }

    private void OnPumpPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Service.PumpStatus.Running))
        {
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                UpdateStopButton();
            });
        }
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
        try {
            if (Service.PumpStatus.Running == Pentair.PumpRunning.Stopped)
            {
                stopButton.Content = "Starting...";
                await Service.PentairClient.Start(Pentair.Client.Pump1);
            }
            else
            {
                stopButton.Content = "Stopping...";
                await Service.PentairClient.Stop(Pentair.Client.Pump1);
            }
            await Task.Delay(500);
            await Service.PentairClient.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.RequestStatus);
        }
        catch (Exception ex)
        {
            stopButton.Content = ex.Message;
            return;
        }
    }

    private void UpdateStopButton()
    {
        stopButton.Content = Service.PumpStatus.Running == Pentair.PumpRunning.Stopped ? "Start" : "Stop";
    }

    private async void program1Button_Click(object sender, RoutedEventArgs e)
    {
        await Service.PentairClient.StartLocalProgram(Pentair.Client.Pump1, 1);
        //Service.PentairClient?.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.StartProgram2);
        await Task.Delay(500);

        _ = Service.PentairClient.SendCommandAsync(Pentair.Client.Pump1, Pentair.Client.RequestStatus);
    }
}
