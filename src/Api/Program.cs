using Shared;
using Temporalio.Client;
using Temporalio.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTemporalClient("localhost:7233", "default");
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost(
    "/stuff/{stuffId}",
    async (Guid stuffId, ITemporalClient client, ILogger<Program> logger) =>
    {
        try
        {
            await client.StartWorkflowAsync<IStuffWorkflow>(
                w => w.RunAsync(stuffId),
                new WorkflowOptions
                {
                    Id = $"Stuff_{stuffId}",
                    TaskQueue = "default"
                });
        }
        catch (WorkflowAlreadyStartedException)
        {
            logger.LogInformation("Workflow for {StuffId} already started", stuffId);
        }

        return Results.Accepted();
    });

app.Run();