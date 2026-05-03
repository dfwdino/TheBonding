using Microsoft.Extensions.Logging;
using TheBonding.Application;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Infrastructure;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Maui;

public static class MauiProgram
{
    /// <summary>
    /// If startup initialization fails, this message is shown on the Home page
    /// instead of routing to setup/unlock.
    /// </summary>
    public static string? StartupError { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Database path — platform-specific via MAUI FileSystem API
        string dbPath;
        try
        {
            dbPath = Path.Combine(FileSystem.AppDataDirectory, "thebonding.db");
        }
        catch (Exception ex)
        {
            dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "thebonding.db");
            StartupError = $"AppDataDirectory fallback used: {ex.Message}";
        }

        builder.Services.AddSingleton(new DbConnectionFactory(dbPath));

        // Infrastructure (repositories + EncryptionService)
        builder.Services.AddInfrastructureServices();

        // Application (services + AuthService)
        builder.Services.AddApplicationServices();

        var app = builder.Build();

        // Run database schema creation before any UI renders.
        try
        {
            var dbInit = app.Services.GetRequiredService<DatabaseInitializer>();
            dbInit.InitializeAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            StartupError = $"Database init failed: {ex.Message}";
            return app; // let the UI render so the error is visible
        }

        // Load auth state so IsSetup is correct before the first component renders.
        try
        {
            var auth = app.Services.GetRequiredService<IAuthService>();
            auth.LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            StartupError = $"Auth load failed: {ex.Message}";
        }

        return app;
    }
}
