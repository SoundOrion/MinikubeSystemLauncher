using SampleBlazorServer.Components;

namespace SampleBlazorServer;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls("http://0.0.0.0:" + port);

        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        WebApplication app = builder.Build();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapGet("/healthz", () => Results.Ok("ok"));
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
