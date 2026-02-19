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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PoolController.Controls;

public sealed partial class PumpStatus : UserControl
{
    public PumpStatus()
    {
        this.InitializeComponent();
        Settings.Instance.PropertyChanged += Instance_PropertyChanged;
        UpdateVacuumButtons();
    }

    private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.VacuumEnabled))
        {
            DispatcherQueue.TryEnqueue(UpdateVacuumButtons);
        }
    }

    private void UpdateVacuumButtons()
    {
        VacuumButton.IsChecked = Settings.Instance.VacuumEnabled;
        SkimmerButton.IsChecked = !Settings.Instance.VacuumEnabled;
    }

    public PoolService Service => PoolService.Instance;

    private void serviceModeButton_Click(object sender, RoutedEventArgs e)
    {
        Service.IsPumpInServiceMode = !Service.IsPumpInServiceMode;
        //serviceModeButton.Content = Service.IsPumpInServiceMode ? "Disable Service Mode" : "Enable Service Mode";
    }

    private void Border_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ContentDialog cd = new ContentDialog();
        cd.Title = "Pump";
        cd.Content = new PumpControl();
        cd.XamlRoot = this.XamlRoot;
        cd.PrimaryButtonText = "OK";
        cd.IsPrimaryButtonEnabled = true;
        
        _ = cd.ShowAsync();
    }

    private void SkimmerClick(object sender, RoutedEventArgs e)
    {
        Settings.Instance.VacuumEnabled = false;
        SkimmerButton.IsChecked = true;
        VacuumButton.IsChecked = false;
    }

    private void VacuumClick(object sender, RoutedEventArgs e)
    {
        Settings.Instance.VacuumEnabled = true;
        SkimmerButton.IsChecked = false;
        VacuumButton.IsChecked = true;
    }
}
