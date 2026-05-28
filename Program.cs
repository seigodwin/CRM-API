
using Azure.Identity;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Services.ModelServices;
using CRMApi.Services.Services;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
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

        builder.Services.AddOpenApi();
        // Add controllers, JSON, XML
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters();

        //Add DbContext
        var connectionString = builder.Environment.IsProduction() ?
        builder.Configuration["PRODUCTION_SQL_DB_CONNECTION_STRING"] :
        builder.Configuration["DEFAULT_SQL_DB_CONNECTION_STRING"];

        if(string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
        options.UseNpgsql(connectionString);
        options.UseSnakeCaseNamingConvention();
        });

        // if (builder.Environment.IsProduction())
        // {

        //     // //Configure Serilog for logging
        //     // Log.Logger = new LoggerConfiguration()
        //     //     .MinimumLevel.Information()
        //     //     .WriteTo.MSSqlServer(
        //     //         connectionString: prodConnectionString,
        //     //         sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true }
        //     //     )
        //     //     .CreateLogger();

        //     // builder.Host.UseSerilog();
        // }


        //Configure IOptions for AppSettings
        builder.Services.Configure<AppSettings>(options =>
        {
            options.JwtSecret = builder.Configuration["JWT_SECRET"]!;
            options.JwtIssuer = builder.Configuration["JWT_ISSUER"]!;
            options.JwtAudience = builder.Configuration["JWT_AUDIENCE"]!;
            options.DefaultSqlDbConnectionString = builder.Configuration["DEFAULT_SQL_DB_CONNECTION_STRING"]!;
            options.DefaultRedisConnectionString = builder.Configuration["DEFAULT_REDIS_CONNECTION_STRING"]!;
            options.SendGridApiKey = builder.Configuration["SENDGRID_API_KEY"]!;
            options.FromEmail = builder.Configuration["EMAIL_FROM"]!;
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
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITeamService, TeamService>();

        // Register utility services
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddTransient<RoleSeeder>();

        //Register Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); 


        //Register Jwt
        builder.Services.AddAuthorization();

        //Production
        try
        {
     
            var JwtKeySecret = builder.Configuration["JWT_SECRET"]!;
             
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
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


        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        //Run migrations at startup 
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

        }

        //Seed Roles
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
            await roleManager.SeedRolesAsync();
        }

        //Create First Admin
        using (var scope = app.Services.CreateScope())
        {

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!db.Users.Any())
            {
                
                var passwordValue = builder.Configuration["ADMIN_PASSWORD"]!;

                var admin = new ApplicationUser
                {
                    FirstName = "Sei",
                    SecondName = "Godwin",
                    UserName = "seigodwin",
                    Email = "seigodwin65@gmail.com",
                    PhoneNumber = "0540580393"
                };

                var results = await userManager.CreateAsync(admin,passwordValue);

                if (results.Succeeded)
                {  
                    await userManager.AddToRoleAsync(admin, "Admin");
                }  
            }
        }

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
        //app.UseAuthentication();
        //app.UseAuthorization(); 
        app.MapControllers();
         
        await app.RunAsync();
    }
}
