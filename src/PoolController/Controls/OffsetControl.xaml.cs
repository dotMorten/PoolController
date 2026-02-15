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

public sealed partial class OffsetControl : UserControl
{
    public OffsetControl()
    {
        this.InitializeComponent();
        OffsetText.Text = "+0.0";
    }

    private void IncreaseOffset_Click(object sender, RoutedEventArgs e)
    {
        Offset+= 0.1;
    }

    private void DecreaseOffset_Click(object sender, RoutedEventArgs e)
    {
        Offset -= 0.1;
    }


    public double Offset
    {
        get { return (double)GetValue(OffsetProperty); }
        set { SetValue(OffsetProperty, value); }
    }

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register(nameof(Offset), typeof(double), typeof(OffsetControl), new PropertyMetadata(0d, (s, e) => ((OffsetControl)s).OnOffsetPropertyChanged()));

    private void OnOffsetPropertyChanged()
    {
        var str = Offset.ToString("0.0");
        if(Offset >= 0)
            str = "+" + str;
        OffsetText.Text = str;
    }
}
