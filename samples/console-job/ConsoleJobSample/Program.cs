using System.Diagnostics;

Console.WriteLine("========================================");
Console.WriteLine("ConsoleJobSample");
Console.WriteLine("========================================");
Console.WriteLine($"StartedAt : {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
Console.WriteLine($"Machine   : {Environment.MachineName}");
Console.WriteLine($"OS        : {Environment.OSVersion}");
Console.WriteLine($"ProcessId : {Environment.ProcessId}");
Console.WriteLine($"Message   : {Environment.GetEnvironmentVariable("JOB_MESSAGE") ?? "Hello from Kubernetes Job."}");

if (args.Length > 0)
{
    Console.WriteLine("Args      : " + string.Join(" ", args));
}

Console.WriteLine();
Console.WriteLine("処理を開始します。");

Stopwatch stopwatch = Stopwatch.StartNew();
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] step {i}/5");
    Thread.Sleep(1000);
}

stopwatch.Stop();
Console.WriteLine();
Console.WriteLine($"Finished  : {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
Console.WriteLine($"Elapsed   : {stopwatch.Elapsed}");
