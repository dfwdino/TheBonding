using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

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

        // Dark navy status bar colour
#pragma warning disable CA1422
        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1b1b2f"));
#pragma warning restore CA1422

        // Keep content between the status bar and navigation bar on ALL Android versions.
        // Android 15 (API 35) forces edge-to-edge and ignores SetDecorFitsSystemWindows(true),
        // so we use a ViewCompat insets listener that pads the root view on every API level.
        ViewCompat.SetOnApplyWindowInsetsListener(Window.DecorView, new SystemBarInsetListener());

        // White icons on the dark status bar
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
#pragma warning disable CA1416, CA1422
            Window.InsetsController?.SetSystemBarsAppearance(
                0,
                (int)WindowInsetsControllerAppearance.LightStatusBars);
#pragma warning restore CA1416, CA1422
        }
        else
        {
#pragma warning disable CS0618
            var flags = (int)Window.DecorView.SystemUiVisibility;
            flags &= ~(int)SystemUiFlags.LightStatusBar;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)flags;
#pragma warning restore CS0618
        }
    }

    /// <summary>
    /// Pads the root decor view by exactly the system bar insets so app content
    /// is always visible between the status bar and the navigation bar.
    /// </summary>
    private sealed class SystemBarInsetListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, WindowInsetsCompat insets)
        {
            var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
            v.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);
            return insets;
        }
    }
}
