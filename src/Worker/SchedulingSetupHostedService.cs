using Temporalio.Client;
using Temporalio.Client.Schedules;
using Temporalio.Exceptions;

namespace Worker;

public class SchedulingSetupHostedService(ITemporalClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedule = new Schedule(
            ScheduleActionStartWorkflow.Create(
                (CronLoggerWorkflow w) => w.RunAsync(),
                new WorkflowOptions
                {
                    Id = "CronLogger",
                    TaskQueue = "default"
                }),
            new ScheduleSpec
            {
                Intervals = [new ScheduleIntervalSpec(TimeSpan.FromSeconds(15))]
            });
    
        try
        {
            await client.CreateScheduleAsync("CronLoggerEvery15Seconds", schedule);
        }
        catch (ScheduleAlreadyRunningException)
        {
            // TODO: is there a better way to "upsert" a schedule?
            var scheduleHandle = client.GetScheduleHandle("CronLoggerEvery15Seconds");
            await scheduleHandle.UpdateAsync(_ => new ScheduleUpdate(schedule));
        }
    }
}