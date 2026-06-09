
using CRM_API.Options;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Services.ModelServices;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //Add and load env variables from .env file
        DotNetEnv.Env.Load();
        builder.Configuration.AddEnvironmentVariables();

        // Override ASPNETCORE_ENVIRONMENT to Development after loading .env
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        builder.Environment.EnvironmentName = "Development";

        builder.Services.AddOpenApi();
        // Add controllers, JSON, XML
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters();

        //Add DbContext
        var connectionString = builder.Environment.IsProduction() ?
        builder.Configuration["PRODUCTION_SQL_DB_CONNECTION_STRING"] :
        builder.Configuration["DEFAULT_POSTGRESQL_DB_CONNECTION_STRING"];
      

        if(string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
        options.UseNpgsql(connectionString);
        options.UseSnakeCaseNamingConvention();
        });

        
        //Register Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); 


        //Configure Redis
        var redisConnectionString = builder.Environment.IsProduction() ?
        builder.Configuration["PRODUCTION_REDIS_CONNECTION_STRING"] :
        builder.Configuration["DEFAULT_REDIS_CONNECTION_STRING"];

        if(string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        //Idistributed cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "crm:api:";
        });

        //IDatabase
        builder.Services.AddSingleton<IConnectionMultiplexer>(
           ConnectionMultiplexer.Connect(redisConnectionString)
        ); 

        //Configure IOptions for AppSettings
        builder.Services.Configure<AppSettings>(options =>
        {
            options.DefaultSqlDbConnectionString = builder.Configuration["DEFAULT_POSTGRESQL_DB_CONNECTION_STRING"] ?? throw new InvalidOperationException("DEFAULT_POSTGRESQL_DB_CONNECTION_STRING is not configured.");
            options.DefaultRedisConnectionString = builder.Configuration["DEFAULT_REDIS_CONNECTION_STRING"] ?? throw new InvalidOperationException("DEFAULT_REDIS_CONNECTION_STRING is not configured.");
            options.SendGridApiKey = builder.Configuration["SENDGRID_API_KEY"] ?? throw new InvalidOperationException("SENDGRID_API_KEY is not configured.");
            options.FromEmail = builder.Configuration["EMAIL_FROM"] ?? throw new InvalidOperationException("EMAIL_FROM is not configured.");
        });

        //Configure IOptions for JwtOptions
        builder.Services.Configure<JwtOptions>(options =>
        {
            options.Secret = builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
            options.Issuer = builder.Configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER is not configured.");
            options.Audience = builder.Configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE is not configured.");
            options.ExpirationMinutes = int.Parse(builder.Configuration["JWT_EXPIRATION_MINUTES"] ?? throw new InvalidOperationException("JWT_EXPIRATION_MINUTES is not configured."));
        });
        

        // builder.Services.AddCors(options =>
        // {
        //     options.AddPolicy("AllowFrontendOnly", policy =>
        //     {
        //         policy.WithOrigins("https://projsync.vercel.app/")
        //               .AllowAnyMethod()
        //               .AllowAnyHeader();
        //     });
        // });

        // Register model services
        builder.Services.AddScoped<IDeveloperService, DeveloperService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ITeamService, TeamService>();

        // Register utility services
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddScoped<IDistributedRedisCacheService, DistributedRedisCacheService>();
        builder.Services.AddScoped<IRateLimitService, RateLimitService>();
        builder.Services.AddTransient<RoleSeeder>();
       

        try
        {
            var JwtKeySecret = builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
             
            var key = Encoding.UTF8.GetBytes(JwtKeySecret);

            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? throw new InvalidOperationException("Jwt Issuer is not configured."),
                    ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? throw new InvalidOperationException("Jwt Audience is not configured."),
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
         
        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        //Run migrations at startup 
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        //Seed Roles
        // using (var scope = app.Services.CreateScope())
        // {
        //     var roleManager = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        //     await roleManager.SeedRolesAsync();
        // }

        //Create First Admin
        // using (var scope = app.Services.CreateScope())
        // {

        //     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //     if (!db.Users.Any())
        //     {
                
        //         var passwordValue = builder.Configuration["ADMIN_PASSWORD"] ?? throw new InvalidOperationException("ADMIN_PASSWORD is not configured.");

        //         var admin = new ApplicationUser
        //         {
        //             FirstName = "Sei",
        //             SecondName = "Godwin",
        //             UserName = "seigodwin",
        //             Email = "seigodwin65@gmail.com",
        //             PhoneNumber = "0540580393"
        //         };

        //         var results = await userManager.CreateAsync(admin,passwordValue);

        //         if (results.Succeeded)
        //         {  
        //             await userManager.AddToRoleAsync(admin, "Admin");
        //         }  
        //     }
        // }

        // Middleware

        // Configure the HTTP request pipeline.


        app.MapOpenApi();    
        app.MapScalarApiReference( "", options =>
         {
             options.Theme = ScalarTheme.BluePlanet;
             options.WithTitle("CRM API Documentation");
         });
         
        app.UseHttpsRedirection(); 
        app.UseRouting();
        //app.UseCors("AllowFrontendOnly"); 
        app.UseAuthentication(); 
        app.UseAuthorization(); 
        app.MapControllers();
         
        await app.RunAsync();
    }
}
