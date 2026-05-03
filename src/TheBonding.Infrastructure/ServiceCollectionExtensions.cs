using Microsoft.Extensions.DependencyInjection;
using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Infrastructure.Data;
using TheBonding.Infrastructure.Repositories;
using TheBonding.Infrastructure.Services;

namespace TheBonding.Infrastructure;

/// <summary>
/// Registers all Infrastructure services.
/// Note: DbConnectionFactory is registered in MauiProgram.cs (requires platform db path).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseInitializer>();

        // Encryption — implemented in Infrastructure (uses System.Security.Cryptography)
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Repositories
        services.AddSingleton<IAppSettingsRepository,         AppSettingsRepository>();
        services.AddSingleton<IUserProfileRepository,         UserProfileRepository>();
        services.AddSingleton<IPartnerRepository,             PartnerRepository>();
        services.AddSingleton<IPartnerHealthStatusRepository, PartnerHealthStatusRepository>();
        services.AddSingleton<IActivityRepository,            ActivityRepository>();
        services.AddSingleton<ILookupRepository,              LookupRepository>();
        services.AddSingleton<IUserHealthStatusRepository,    UserHealthStatusRepository>();
        services.AddSingleton<IDataManagementService,         DataManagementService>();

        return services;
    }
}
