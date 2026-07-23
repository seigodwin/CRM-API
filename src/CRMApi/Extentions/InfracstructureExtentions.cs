
using System.Security.Claims;
using System.Text;
using CRM_API.Options;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using StackExchange.Redis;

namespace CRMApi.Extentions
{
    public static class InfracstructureExtentions
    {
        public static IServiceCollection AddInfracstructure(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
        {
              //Add DbContext
            var connectionString = environment.IsProduction() ?
            configuration["SUPABASE_CONNECTION_STRING"] :
            configuration["DEFAULT_POSTGRESQL_DB_CONNECTION_STRING"];

            if(string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured.");
            }

            services.AddDbContext<AppDbContext>(options =>
            {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
            });

            
            //Register Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); 

                //Configure Redis
        var redisConnectionString = environment.IsProduction() ?
        configuration["PRODUCTION_REDIS_CONNECTION_STRING"] :
        configuration["DEFAULT_REDIS_CONNECTION_STRING"];

        if(string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        //Idistributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "crm:api:";
        });

        //IDatabase
        services.AddSingleton<IConnectionMultiplexer>(
           ConnectionMultiplexer.Connect(redisConnectionString)
        );

                //Configure Resend Mail client

        var resendAPIKEY = configuration["RESEND_API_KEY"] ?? throw new InvalidOperationException("Resend client api key has not been configured");

        services.AddResend(o =>
        {
            o.ApiToken = resendAPIKEY;
        });

        //Configure IOptions for EmailOptions
        services.Configure<EmailOptions>(options =>
        {
            options.FromEmail = configuration["FROM_EMAIL"] ?? throw new InvalidOperationException("FROM_EMAIL is not configured");
        });

        //Configure IOptions for JwtOptions
        services.Configure<JwtOptions>(options =>
        {
            options.Secret = configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
            options.Issuer = configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER is not configured.");
            options.Audience = configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE is not configured.");
            options.ExpirationMinutes = int.Parse(configuration["JWT_EXPIRATION_MINUTES"] ?? throw new InvalidOperationException("JWT_EXPIRATION_MINUTES is not configured."));
        });

          try
        {
            var JwtKeySecret = configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
             
            var key = Encoding.UTF8.GetBytes(JwtKeySecret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("Jwt Issuer is not configured."),
                    ValidAudience = configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("Jwt Audience is not configured."),
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role,
                    
                };
            });
        }

        catch(Exception ex)
        {
            Console.WriteLine($"Key Vault error: {ex.Message}");
            throw;
        }
        
        services.AddAuthorization();  

        services.AddEndpointsApiExplorer(); 
         
        return services;
        }
    }
}