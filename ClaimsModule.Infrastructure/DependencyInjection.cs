using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Infrastructure.Jobs;
using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Infrastructure.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageOptions = new StorageOptions();
        configuration.GetSection(StorageOptions.SectionName).Bind(storageOptions);

        services.Configure<StorageOptions>(
            configuration.GetSection(StorageOptions.SectionName));

        if (storageOptions.Provider == "Azure")
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        else
            services.AddScoped<IStorageService, LocalFileSystemStorageService>();

        services.AddScoped<PostGLReserveChangeJob>();
        services.AddScoped<SlaMonitoringJob>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }

    // Add this extension method
    public static string GetUploadsPath(this IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<StorageOptions>>();
        return options.Value.GetResolvedLocalPath();
    }
}