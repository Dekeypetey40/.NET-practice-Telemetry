using Serilog;
using Telemetry.Api.Middleware;
using Telemetry.Api.Services;
using Telemetry.Application.Extensions;
using Telemetry.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdProvider, HttpContextCorrelationIdProvider>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Set it in appsettings, User Secrets, or environment.");

builder.Services.AddTelemetryApplication();
builder.Services.AddTelemetryInfrastructure(connectionString);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect to HTTPS when we have an HTTPS port (e.g. launch profile "https"); avoids breaking Swagger on http-only.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// Expose for integration tests (WebApplicationFactory<Program>)
public partial class Program { }
