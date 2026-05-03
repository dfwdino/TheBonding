using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace TheBonding.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Window is null) return;

        // Dark navy status bar colour (obsoleted in API 35 but still works; suppress warning)
#pragma warning disable CA1422
        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1b1b2f"));
#pragma warning restore CA1422

        // Push content below the status bar (Android 30+; below 30 the system already does this)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
#pragma warning disable CA1416, CA1422
            Window.SetDecorFitsSystemWindows(true);

            // White (light) icons on the dark status bar
            Window.InsetsController?.SetSystemBarsAppearance(
                0,
                (int)WindowInsetsControllerAppearance.LightStatusBars);
#pragma warning restore CA1416, CA1422
        }
        else
        {
            // Android < 30: clear LightStatusBar so icons are white
#pragma warning disable CS0618
            var flags = (int)Window.DecorView.SystemUiVisibility;
            flags &= ~(int)SystemUiFlags.LightStatusBar;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)flags;
#pragma warning restore CS0618
        }
    }
}
