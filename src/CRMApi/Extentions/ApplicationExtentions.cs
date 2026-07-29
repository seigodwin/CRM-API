using CRM_API.Options;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Exceptions;
using CRMApi.Options;
using CRMApi.Services.Implimentations;
using CRMApi.Services.Interfaces;
using CRMApi.Services.ModelServices;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;
using StackExchange.Redis;

namespace CRMApi.Extentions
{
    public static class ApplicationExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IHostEnvironment environment,
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

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Register app services
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITeamService, TeamService>();

        // Register utility services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDistributedRedisCacheService, DistributedRedisCacheService>();
        services.AddScoped<IRateLimitService, RateLimitService>();
        services.AddTransient<RoleSeeder>();

        return services;
       
        }
    }
}