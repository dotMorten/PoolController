using Uno.UI.Hosting;
using Uno.UI.Runtime.Skia.Linux.FrameBuffer;

namespace PoolController;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Log.PurgeLogFile();
        Log.LogMessage("**********************************\nStarting up Pool Controller\n**********************************");
        System.Console.WriteLine("Writing log to " + Log.LogFileName);


        AppDomain.CurrentDomain.UnhandledException += (sender, e) => FatalExceptionObject(e.ExceptionObject);

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
        }
        host.Run();
    }

    private static void FatalExceptionObject(object exceptionObject)
    {
        if (exceptionObject is Exception ex)
        {
            Log.LogError($"{ex.Message}\n{ex.StackTrace}");
        }
        else
        {
            Log.LogError($"Unknown Fatal exception:{exceptionObject}");
        }
    }
}
