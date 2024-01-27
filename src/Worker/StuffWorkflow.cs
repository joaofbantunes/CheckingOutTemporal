using Bogus;
using Shared;
using Temporalio.Activities;
using Temporalio.Common;
using Temporalio.Workflows;

namespace Worker;

[Workflow("StuffWorkflowNameSoTheImplementationMatches")]
public class StuffWorkflow : IStuffWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(Guid stuffId)
    {
        var randomString = await Workflow.ExecuteActivityAsync<StuffWorkflowActivities, string>(
            a => a.FetchRandomStringAsync(),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });

        var newRandomString = await Workflow.ExecuteActivityAsync<StuffWorkflowActivities, string>(
            a => a.ReturnRandomStringWhenNotFailingAsync(randomString),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10),
                RetryPolicy = new RetryPolicy
                {
                    // using the default
                }
            });

        await Workflow.ExecuteActivityAsync<StuffWorkflowActivities>(
            a => a.WrapUpAsync(stuffId, newRandomString),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10)
            });
    }
}

public class StuffWorkflowActivities(
    Faker faker,
    ILogger<StuffWorkflowActivities> logger)
{
    [Activity]
    public Task<string> FetchRandomStringAsync()
        => Task.FromResult(faker.Hacker.IngVerb());

    [Activity]
    public Task<string> ReturnRandomStringWhenNotFailingAsync(string randomString)
    {
        logger.LogInformation("Got string \"{RandomString}\"", randomString);
        var randomValue = Random.Shared.Next(0, 10);
        if (randomValue is not 5)
        {
            logger.LogInformation("Hoped for 5, got {RandomValue}", randomValue);
            throw new Exception("Kaboom!");
        }
        return Task.FromResult($"{randomString} + ¯\\_(ツ)_/¯");
    }

    [Activity]
    public Task WrapUpAsync(Guid stuffId, string randomString)
    {
        logger.LogInformation(
            "Got to the end of workflow for stuff {StuffId}, with random string {RandomString}",
            stuffId, 
            randomString);
        return Task.CompletedTask;
    }
}