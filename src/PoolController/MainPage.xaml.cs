
using Microsoft.UI.Xaml.Input;
using Windows.Devices.AllJoyn;

namespace PoolController;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        NavFrame.Navigate(typeof(StatusView));
        UpdateClock();
        screenOffTimer = new DispatcherTimer();
        screenOffTimer.Interval = TimeSpan.FromSeconds(60);
        screenOffTimer.Tick += (s, e) =>
        {
            TurnOffScreen();
        };
        LayoutRoot.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(RootPointerMoved), true);
        LayoutRoot.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(RootPointerMoved), true);
        screenOffTimer.Start();
        TurnOnScreen();
    }

    private async void UpdateClock()
    {
        while (true)
        {
            ClockText.Text = DateTime.Now.ToString("t");
            // Update at the start of the next minute
            await Task.Delay(TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(DateTime.Now.Second) - TimeSpan.FromMilliseconds(DateTime.Now.Millisecond));
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Navigate(typeof(SettingsPage));
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Navigate(typeof(StatusView));
    }

    private DispatcherTimer screenOffTimer;
    
    private void RootPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        screenOffTimer.Stop();
        screenOffTimer.Start();
        TurnOnScreen();
    }


    public static void TurnOffScreen() => SetBrightness(0);

    public static void TurnOnScreen() => SetBrightness(255);
    
    static byte brightness = 0;
    public static void SetBrightness(byte b)
    {
        if(brightness == b)
            return;
        brightness = b;
        string command = $"echo {b} | sudo tee /sys/class/backlight/*/brightness";   
        System.Diagnostics.Process.Start("bash", $"-c \"{command}\"");
    }
}
