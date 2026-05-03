using Microsoft.Extensions.DependencyInjection;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Services;

namespace TheBonding.Application;

/// <summary>
/// Registers all Application-layer service implementations.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth — singleton so Key2 persists in memory for the app lifetime
        services.AddSingleton<IAuthService,          AuthService>();
        services.AddSingleton<ILookupSeederService,  LookupSeederService>();

        // Data services
        services.AddSingleton<IUserProfileService,        UserProfileService>();
        services.AddSingleton<IPartnerService,            PartnerService>();
        services.AddSingleton<IPartnerHealthStatusService, PartnerHealthStatusService>();
        services.AddSingleton<IActivityService,           ActivityService>();
        services.AddSingleton<ILookupService,             LookupService>();
        services.AddSingleton<IUserHealthStatusService,   UserHealthStatusService>();

        return services;
    }
}
