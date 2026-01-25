namespace PoolController;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

//        Task<Pentair.StatusMessage> status = client.GetStatusAsync(Pentair.Client.Pump1);
    }

    public PoolService Service => PoolService.Instance;
    public Settings Settings => Settings.Instance;
}
