

using Scalar.AspNetCore;
using Serilog;

namespace CRMApi.Extentions
{
    public static class MiddlewareExtentions
    {
        public static WebApplication UseApplication(this WebApplication app, IHostEnvironment environment)
        {
            app.UseSerilogRequestLogging();

        app.MapOpenApi();

        if (environment.IsProduction())
        {
            app.MapScalarApiReference("", options =>
            {
                options.Theme = ScalarTheme.BluePlanet;
                options.WithTitle("CRM API Documentation");

                options.Servers = new[]
                {
                    new ScalarServer("https://crm-api-47oi.onrender.com")
                };
            });
        }
        else
        {
            app.MapScalarApiReference( "", options =>
         {
             options.Theme = ScalarTheme.BluePlanet;
             options.WithTitle("CRM API Documentation");
         });
        }

        app.UseRouting();
        app.UseHttpsRedirection(); 
        app.UseCors("AllowAll");
        app.UseAuthentication(); 
        app.UseAuthorization(); 
        app.MapControllers();
         
         return app;
        }
    }
}