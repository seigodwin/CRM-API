using APIWeaver;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
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
using Microsoft.OpenApi.Models;
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


        // Add controllers, JSON, XML
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters();

        builder.Services.AddOpenApi();

       // Configure OpenApi with ApiWeaver.OpenApi
        builder.Services.AddOpenApi(options =>
        {
            options.AddSecurityScheme(JwtBearerDefaults.AuthenticationScheme, scheme =>
            {
                scheme.In = ParameterLocation.Header;
                scheme.Type = SecuritySchemeType.Http;
                scheme.Scheme = JwtBearerDefaults.AuthenticationScheme;
                scheme.BearerFormat = "JWT";
            });
            options.AddAuthResponse();
        });



        if (builder.Environment.IsProduction())
        {
            string VaultUrl = builder.Configuration["KEY_VAULT_URI"]!;
            var keyVaultClient = new SecretClient(new Uri(VaultUrl), new DefaultAzureCredential());

            // Fetch production secrets from Key Vault 
            var prodSqlSecret = await keyVaultClient.GetSecretAsync("ProdDbConString");
            var prodStorageSecret = await keyVaultClient.GetSecretAsync("AzureStorageConnectionString");

            var prodConnectionString = prodSqlSecret.Value.Value;
            var prodStorageConnectionString = prodStorageSecret.Value.Value;

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(prodConnectionString));


            builder.Services.AddSingleton(sp =>
                new BlobServiceClient(prodStorageConnectionString));

            //Configure Serilog for logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.MSSqlServer(
                    connectionString: prodConnectionString,
                    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true }
                )
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        else
        {

            var devStorageSecret = builder.Configuration["BLOB_STORAGE_CONNECTION_STRING"];

            var devDbConnection = builder.Configuration.GetConnectionString("DefaultSQLConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(devDbConnection));

            
            builder.Services.AddSingleton(sp =>
                new BlobServiceClient(devStorageSecret));

        }


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendOnly", policy =>
            {
                policy.WithOrigins("https://projsync.vercel.app/")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

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

        //Development
        //var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:Key"]!);

        //Production
        try
        {
            var keyVaultUrl = builder.Configuration["KEY_VAULT_URI"]!;
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            var JwtKeySecret = await client.GetSecretAsync("JwtKey");
            var JwtKeyValue = JwtKeySecret.Value.Value;
             
            var key = Encoding.UTF8.GetBytes(JwtKeyValue);

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
                var VaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"]!;
                var client = new SecretClient(new Uri(VaultUrl), new DefaultAzureCredential());

                var passwordSecret = await client.GetSecretAsync("FirstAdminPassword");
                var passwordValue = passwordSecret.Value.Value;

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
            
         app.MapScalarApiReference("",options =>
         {
             options.Theme = ScalarTheme.BluePlanet;
         }
         );   

       
        app.UseHttpsRedirection(); 
        app.UseRouting();
        app.UseCors("AllowFrontendOnly"); 
        app.UseAuthentication();
        app.UseAuthorization(); 
        app.MapControllers();
         
        await app.RunAsync();
    }
}
