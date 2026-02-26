using Serilog;
using Telemetry.Api.Middleware;
using Telemetry.Api.Services;
using Telemetry.Application.Extensions;
using Telemetry.Infrastructure.Extensions;
using Telemetry.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var logCollector = new InMemoryRingBufferLogCollector(maxEntries: 2000);
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Sink(new InMemoryRingBufferLogSink(logCollector)));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdProvider, HttpContextCorrelationIdProvider>();
// Default policy is permissive for local/demo use. For production, use a named policy with specific origins, e.g.:
// options.AddPolicy("Production", policy => policy.WithOrigins("https://your-frontend.com").AllowAnyMethod().AllowAnyHeader());
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("<required:", StringComparison.Ordinal))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Set it via User Secrets, appsettings.Development.json, or environment. See README.");

builder.Services.AddTelemetryApplication();
builder.Services.AddTelemetryInfrastructure(connectionString, logCollector);

var app = builder.Build();

app.UseCors();
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
