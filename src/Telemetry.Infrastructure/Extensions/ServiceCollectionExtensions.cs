using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Contracts;
using Telemetry.Infrastructure.Data;
using Telemetry.Infrastructure.Repositories;
using Telemetry.Infrastructure.Services;

namespace Telemetry.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryInfrastructure(this IServiceCollection services, string connectionString, ISupportBundleLogCollector? logCollector = null)
    {
        services.AddDbContext<TelemetryDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IRunRepository, RunRepository>();
        services.AddScoped<IInstrumentRepository, InstrumentRepository>();
        if (logCollector != null)
            services.AddSingleton<ISupportBundleLogCollector>(logCollector);
        services.AddScoped<ISupportBundleService, SupportBundleService>();
        return services;
    }
}
