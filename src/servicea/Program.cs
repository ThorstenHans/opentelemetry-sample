using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using ServiceA.Configuration;
using ServiceA;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var cfg = new ServiceConfig();
builder.Configuration.GetSection(ServiceConfig.SECTION_NAME).Bind(cfg);
if (cfg == null || !cfg.IsValid())
{
    throw new Exception("Invalid configuration");
}
builder.Services.AddSingleton(cfg);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

Action<ResourceBuilder> buildOpenTelemetryResource = builder => builder
        .AddService("Service A", serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName)
        .Build();

builder.Services.AddOpenTelemetryTracing( builder => {
    builder.ConfigureResource(buildOpenTelemetryResource)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddProcessor<CustomProcessor>()
        .AddJaegerExporter()
        .AddConsoleExporter();
});

builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));

builder.Services.AddOpenTelemetryMetrics( b => 
{
    b.ConfigureResource(buildOpenTelemetryResource)
    .AddRuntimeInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
	.AddPrometheusExporter();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz/readiness");
app.MapHealthChecks("/healthz/liveness");


app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
