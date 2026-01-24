namespace PoolController;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        var client = new Pentair.Client("COM3");
        client.MessageReceived += (s, e) => Console.WriteLine(e);

        Task<Pentair.StatusMessage> status = client.GetStatusAsync(Pentair.Client.Pump1);
    }
}
