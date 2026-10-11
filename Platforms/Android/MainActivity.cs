using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WasteTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        Java.Lang.Thread.DefaultUncaughtExceptionHandler = new CustomUncaughtExceptionHandler();
    }
}

public class CustomUncaughtExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
{
    public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e)
    {
        Android.Util.Log.Error("MAUI_CRASH", $"Message: {e.Message}");
        Android.Util.Log.Error("MAUI_CRASH", $"LocalizedMessage: {e.LocalizedMessage}");
        Android.Util.Log.Error("MAUI_CRASH", $"Cause: {e.Cause}");
    }
}