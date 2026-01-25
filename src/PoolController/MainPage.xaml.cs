
using Windows.Devices.AllJoyn;

namespace PoolController;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        NavFrame.Navigate(typeof(StatusView));
        UpdateClock();
    }

    private async void UpdateClock()
    {
        while (true)
        {
            ClockText.Text = DateTime.Now.ToString("T");
            await Task.Delay(1000 - DateTime.Now.Millisecond);
        }
    }

    public PoolService Service => PoolService.Instance;

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Navigate(typeof(SettingsPage));
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        NavFrame.Navigate(typeof(StatusView));
    }
}
