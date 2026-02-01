namespace PoolController.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    public PoolController.Settings Settings => PoolController.Settings.Instance;
}
