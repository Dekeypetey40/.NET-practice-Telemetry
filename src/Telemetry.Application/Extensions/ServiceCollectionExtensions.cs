using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Services;

namespace Telemetry.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryApplication(this IServiceCollection services)
    {
        services.AddScoped<IRunService, RunService>();
        services.AddScoped<IInstrumentService, InstrumentService>();
        return services;
    }
}
