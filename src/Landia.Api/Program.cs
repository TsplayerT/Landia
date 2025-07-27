using Landia.Api.Startup;

namespace Landia.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Builder.Create(args);
        var app = App.Create(builder);

        await app.RunAsync();
    }
}