using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using ServiceB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

Action<ResourceBuilder> buildOpenTelemetryResource = builder => builder
        .AddService("Service B", serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName)
        .Build();

builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry( options =>
{
    options.ConfigureResource(buildOpenTelemetryResource);
    options.AddConsoleExporter();
});

var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

logger.LogInformation("Configured Logging");
builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
{
    opt.IncludeScopes = true;
    opt.ParseStateValues = true;
    opt.IncludeFormattedMessage = true;
    
});

builder.Services.AddOpenTelemetryTracing( builder => {
    builder.ConfigureResource(buildOpenTelemetryResource)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessor<CustomProcessor>()
        .AddSource(CustomTraces.Default.Name)
        .AddJaegerExporter()
        .AddConsoleExporter();
});
builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));
logger.LogInformation("Configured Traces");

builder.Services.AddOpenTelemetryMetrics( b => 
{
    b.ConfigureResource(buildOpenTelemetryResource)
    .AddRuntimeInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter(CustomMetrics.Default.Name)
    .AddView(CustomMetrics.PingDelay.Name, CustomMetrics.PingDelayView)
	.AddPrometheusExporter();
});
logger.LogInformation("Configured Metrics");
logger.LogInformation("Registering Middlewares");
var app = builder.Build();
logger.LogInformation(" - Swagger");
app.UseSwagger();
logger.LogInformation(" - SwaggerUI");
app.UseSwaggerUI();

logger.LogInformation(" - Authorization");
app.UseAuthorization();
logger.LogInformation(" - API Controllers");
app.MapControllers();
logger.LogInformation(" - HealthChecks");
app.MapHealthChecks("/healthz/readiness");
app.MapHealthChecks("/healthz/liveness");

logger.LogInformation(" - Prometheus Scraping Endpoint");
app.UseOpenTelemetryPrometheusScrapingEndpoint();

logger.LogInformation("Middlewares registered");
logger.LogInformation("Starting API...");
app.Run();
