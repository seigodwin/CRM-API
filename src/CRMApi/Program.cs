using CRMApi.Extentions;

public class Program 
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        DotNetEnv.Env.Load();
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddApplication(builder.Environment, builder.Configuration);
        builder.Services.AddInfracstructure(builder.Configuration, builder.Environment);
        builder.Services.AddPresentation(builder.Environment, builder.Configuration);

        builder.AddHost();

        var app = builder.Build(); 

        app.UseApplication(builder.Environment);
        
        await app.RunAsync();
    }
}
