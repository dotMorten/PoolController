namespace PoolController;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    public Settings Settings => Settings.Instance;
}
