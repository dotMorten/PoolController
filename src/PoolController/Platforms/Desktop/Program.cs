using Uno.UI.Hosting;
using Uno.UI.Runtime.Skia.Linux.FrameBuffer;

namespace PoolController;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer(hostBuilder => hostBuilder.Orientation(Windows.Graphics.Display.DisplayOrientations.Portrait).DisableKMSDRM())
            .UseMacOS()
            .UseWin32()
            .Build();

        if (host is FrameBufferHost fbh)
        {
            fbh.DisplayScale = 2;
            MainPage.TurnOnScreen();
        }
        host.Run();
    }
}
