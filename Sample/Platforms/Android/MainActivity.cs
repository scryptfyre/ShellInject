using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content.Res;
using AndroidX.Core.View;

namespace Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        UpdateStatusBarIcons();
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        UpdateStatusBarIcons();
    }

    private void UpdateStatusBarIcons()
    {
        if (Window is not { } window) return;
        var isDark = (Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;
        var background = Android.Graphics.Color.ParseColor(isDark ? "#101923" : "#F3F6FA");
        window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(background));
        // Android 15+ draws edge-to-edge; older releases still have a separate status-bar color.
        if (!OperatingSystem.IsAndroidVersionAtLeast(35)) window.SetStatusBarColor(background);
        if (WindowCompat.GetInsetsController(window, window.DecorView) is { } controller)
        {
            controller.AppearanceLightStatusBars = !isDark;
        }
    }
}
