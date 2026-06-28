using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Extensions.Hosting;
using Temporalio.Workflows;

string mode = args.Length == 0 ? "help" : args[0].Trim().ToLowerInvariant();
string temporalAddress = Environment.GetEnvironmentVariable("TEMPORAL_ADDRESS") ?? "localhost:7233";
string temporalNamespace = Environment.GetEnvironmentVariable("TEMPORAL_NAMESPACE") ?? "default";
string taskQueue = Environment.GetEnvironmentVariable("TEMPORAL_TASK_QUEUE") ?? "sample-task-queue";

if (mode == "worker")
{
    Console.WriteLine($"Starting Temporal worker: address={temporalAddress}, namespace={temporalNamespace}, taskQueue={taskQueue}");

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    builder.Services
        .AddHostedTemporalWorker(temporalAddress, temporalNamespace, taskQueue)
        .AddScopedActivities<GreetingActivities>()
        .AddWorkflow<GreetingWorkflow>();

    IHost host = builder.Build();
    await host.RunAsync();
    return;
}

if (mode == "start")
{
    string name = args.Length >= 2 ? args[1] : "Temporal";
    string workflowId = "sample-greeting-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    Console.WriteLine($"Starting workflow: address={temporalAddress}, namespace={temporalNamespace}, taskQueue={taskQueue}, workflowId={workflowId}");

    TemporalClient client = await TemporalClient.ConnectAsync(new TemporalClientConnectOptions(temporalAddress)
    {
        Namespace = temporalNamespace,
    });

    string result = await client.ExecuteWorkflowAsync(
        (GreetingWorkflow workflow) => workflow.RunAsync(name),
        new WorkflowOptions(workflowId, taskQueue));

    Console.WriteLine("Workflow result:");
    Console.WriteLine(result);
    return;
}

Console.WriteLine("TemporalWorkerSample");
Console.WriteLine();
Console.WriteLine("Usage:");
Console.WriteLine("  dotnet run -- worker");
Console.WriteLine("  dotnet run -- start <name>");
Console.WriteLine();
Console.WriteLine("Environment variables:");
Console.WriteLine("  TEMPORAL_ADDRESS     default: localhost:7233");
Console.WriteLine("  TEMPORAL_NAMESPACE   default: default");
Console.WriteLine("  TEMPORAL_TASK_QUEUE  default: sample-task-queue");

public class GreetingActivities
{
    [Activity]
    public string ComposeGreeting(string name)
    {
        Console.WriteLine($"Activity ComposeGreeting called. name={name}");
        return $"Hello, {name}. This message came from a Temporal Activity at {DateTimeOffset.Now:O}.";
    }
}

[Workflow]
public class GreetingWorkflow
{
    [WorkflowRun]
    public async Task<string> RunAsync(string name)
    {
        string greeting = await Workflow.ExecuteActivityAsync(
            (GreetingActivities activities) => activities.ComposeGreeting(name),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(30),
            });

        return greeting;
    }
}
