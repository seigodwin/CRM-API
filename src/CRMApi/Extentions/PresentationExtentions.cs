
namespace CRMApi.Extentions
{
    public static class PresentationExtentions
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IHostEnvironment environment,
        IConfiguration configuration)
        {
            services.AddOpenApi();
            // Add controllers, JSON, XML
            services.AddControllers()
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();

            //CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

        return services;
        }
    }
}