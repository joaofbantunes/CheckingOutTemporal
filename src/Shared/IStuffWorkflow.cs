using Temporalio.Workflows;

namespace Shared;

/*
 * From the docs:
 * [Workflow] attribute must be present on the workflow type.
 * The attribute can have a string argument for the workflow type name.
 * Otherwise the name is defaulted to the unqualified type name (with the I prefix removed if on an interface and has a capital letter following).
 */
[Workflow("StuffWorkflowNameSoTheImplementationMatches")]
public interface IStuffWorkflow
{
    [WorkflowRun]
    Task RunAsync(Guid stuffId);
}