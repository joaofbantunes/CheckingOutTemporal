using Bogus;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Temporalio.Extensions.Hosting;
using Temporalio.Extensions.OpenTelemetry;
using Worker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Faker>();
builder.Services.AddHostedService<SchedulingSetupHostedService>();
builder.Services.AddTemporalClient("localhost:7233", "default");
builder.Services
    .AddHostedTemporalWorker("localhost:7233", "default", "default")
    .AddWorkflow<CronLoggerWorkflow>()
    .AddWorkflow<StuffWorkflow>()
    .AddSingletonActivities<CronLoggerActivity>()
    .AddSingletonActivities<StuffWorkflowActivities>()
    .ConfigureOptions(options => { options.Interceptors = [new TracingInterceptor()]; });

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
    {
        resourceBuilder.AddService(
            serviceName: "worker",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
            serviceInstanceId: Environment.MachineName);
    })
    .WithTracing(b =>
    {
        b
            .AddAspNetCoreInstrumentation()
            .AddSource(
                typeof(Program).Assembly.GetName().Name!,
                // Temporal stuff
                TracingInterceptor.ClientSource.Name,
                TracingInterceptor.WorkflowsSource.Name,
                TracingInterceptor.ActivitiesSource.Name)
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();