using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Contracts;
using Telemetry.Infrastructure.Data;
using Telemetry.Infrastructure.Repositories;

namespace Telemetry.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TelemetryDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IRunRepository, RunRepository>();
        services.AddScoped<IInstrumentRepository, InstrumentRepository>();
        return services;
    }
}
