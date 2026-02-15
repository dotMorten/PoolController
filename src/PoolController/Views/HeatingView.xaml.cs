namespace PoolController.Views;

public sealed partial class HeatingView : Page
{
    public HeatingView()
    {
        this.InitializeComponent();
        SolarHeatingSelector.SelectedIndex = (int)PoolController.Settings.Instance.SolarHeatingMode;
    }

    public PoolController.Settings Settings => PoolController.Settings.Instance;

    private void SolarHeatingModeChanged(object sender, SelectionChangedEventArgs e)
    {
        PoolController.Settings.Instance.SolarHeatingMode = (SolarHeatingMode)SolarHeatingSelector.SelectedIndex;
    }
}
