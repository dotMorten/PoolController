
using Microsoft.UI.Xaml.Input;
using Windows.Devices.AllJoyn;

namespace PoolController;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        NavFrame.Navigate(typeof(Views.StatusView));
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

    public PoolService PoolService => PoolService.Instance;

    private async void UpdateClock()
    {
        while (true)
        {
            ClockText.Text = DateTime.Now.ToString("t");
            // Update at the start of the next minute
            await Task.Delay(TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(DateTime.Now.Second) - TimeSpan.FromMilliseconds(DateTime.Now.Millisecond));
        }
    }

    private readonly Views.SettingsPage settingsView = new Views.SettingsPage();
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Content = settingsView;
    }


    private readonly Views.StatusView statusView = new Views.StatusView();
    private void Home_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Content = statusView;
    }

    private readonly Views.HeatingView heatingView = new Views.HeatingView();
    private void Heating_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Content = heatingView;
    }

    private DispatcherTimer screenOffTimer;
    
    private void RootPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
        screenOffTimer.Stop();
        screenOffTimer.Start();
        TurnOnScreen();
    }


    public void TurnOffScreen()
    {
        TouchLayer.Visibility = Visibility.Visible;
        SetBrightness(0f);
    }

    public void TurnOnScreen()
    {
        TouchLayer.Visibility = Visibility.Collapsed;
        SetBrightness(1f);
    }

   
    static float brightness = 0;
    static int maxBrightness = -1;
    public static void SetBrightness(float b)
    {
        if(brightness == b)
            return;
        brightness = b;
        if (!System.OperatingSystem.IsLinux())
            return;
        byte bb = 0;
        if (b > 0)
        {
            if (maxBrightness <= -1)
            {
                if (File.Exists("/sys/class/backlight/*/max_brightness") &&
                int.TryParse(File.ReadAllText("/sys/class/backlight/*/max_brightness"), out int max))
                {
                    maxBrightness = max;
                }
            }
            if (maxBrightness > 0)
            {
                bb = (byte)(maxBrightness * b);
            }
            else
            {
                bb = 31; // This is max for Raspberry Pi Touch 2 screen.
            } 
        }
        string command = $"echo {bb} | sudo tee /sys/class/backlight/*/brightness";   
        System.Diagnostics.Process.Start("bash", $"-c \"{command}\"");
    }
}
