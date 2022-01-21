using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WebCore
{
    /// <summary>
    /// Workflow manager.
    /// </summary>
    public class Workflow
    {
    }
    public class Workflow1 : IWorkflow
    {
        public string Id { get; set; } = "1";

        public int Version { get; set; } = 1;

        public void Build(IWorkflowBuilder<object> builder)
        {
            builder.StartWith(context => ExecutionResult.Next())
                .UserTask("Do you approve", data => @"domain\bob")
                .WithOption("yes", "I approve").Do(then => then.StartWith(context => Console.WriteLine("You approved")))
                .WithOption("no", "I do not approve").Do(then => then.StartWith(context => Console.WriteLine("You did not approve")))
                .WithEscalation(x => TimeSpan.FromSeconds(20), x => @"domain\frank", action => action.StartWith(context => Console.WriteLine("Escalated task")).Then(context => Console.WriteLine("Sending notification...")))
                .Then(context => Console.WriteLine("end"));
        }
    }
}
