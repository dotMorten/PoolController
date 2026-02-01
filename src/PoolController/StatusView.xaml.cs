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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PoolController;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StatusView : Page
{
    public StatusView()
    {
        this.InitializeComponent();

        // if (int.TryParse(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp"), out int temp))
        // {
        //     Console.WriteLine($"CPU Temp: {temp / 1000}°C");
        // }
    }

    public PoolService Service => PoolService.Instance;
}
