using Android.App;
using Android.Runtime;
using Android.Util;

namespace EuropeanMaui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
            Log.Error("EuropeanMaui", "UnhandledException: " + e.Exception);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
