using Temporalio.Activities;
using Temporalio.Workflows;

namespace Worker;

[Workflow]
public class CronLoggerWorkflow
{
    [WorkflowRun]
    public async Task RunAsync()
    {
        await Workflow.ExecuteActivityAsync(
            (CronLoggerActivity activity) => activity.LogAsync(),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });
    }
}

public class CronLoggerActivity(ILogger<CronLoggerActivity> logger)
{
    [Activity]
    public Task LogAsync()
    {
        logger.LogInformation("Hello from CronLoggerActivity {UtcNow}", DateTime.UtcNow);
        return Task.CompletedTask;
    }
}