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

public sealed partial class ChlorinatorStatus : UserControl
{
    public ChlorinatorStatus()
    {
        this.InitializeComponent();
        this.Loaded += ChlorinatorStatus_Loaded;
        this.Unloaded += ChlorinatorStatus_Unloaded;
    }

    private void ChlorinatorStatus_Unloaded(object sender, RoutedEventArgs e)
    {
        Service.ChlorinatorStatus.PropertyChanged -= Service_PropertyChanged;
    }

    private void ChlorinatorStatus_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSaltIndicatorLine();
        Service.ChlorinatorStatus.PropertyChanged += Service_PropertyChanged;
    }

    private void Service_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(Service.ChlorinatorStatus.SaltLevel))
        {
            UpdateSaltIndicatorLine();
        }
    }

    public PoolService Service => PoolService.Instance;

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        UpdateSaltIndicatorLine();
    }
    private void UpdateSaltIndicatorLine()
    { 
        if (Service != null)
        {
            double min = 2600;
            double max = 4700;
            double range = max - min;
            var fraction = (Service.ChlorinatorStatus.SaltLevel - min) / range;
            SaltIndicatorLine.Margin = new Thickness(fraction * (SaltIndicatorGrid.ActualWidth) - SaltIndicatorLine.ActualWidth*.5, -2, 0, -2);
        }
    }
}
