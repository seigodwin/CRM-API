using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Services.Interfaces;
using CRMApi.Services.ModelServices;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
 
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers()
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();


// Configure Swagger (Fixing AddOpenApi) 


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1",
        Description = "API for managing CRM functionalities",
    });

    // Add JWT Authentication to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your_token}'",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



if (builder.Environment.IsDevelopment())
{
    var KeyVaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"];
    var KeyVaultClient = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCridential());

    var prodConnectionStringSecret = await KeyVaultClient.GetSecretAsync("ProductionConnectionString");
    string prodConnectionStringValue = prodConnectionStringSecret.Value.Value;

    var prodStorageServiceSecret = await KeyVaultClient.GetSecretAsync("AzureStorageConnection");
    string prodStorageServiceValue = prodStorageServiceSecret.Value.Value;

    builder.Services.AddDbContext<CRMApiDbContext>(options => options.UseSqlServer(prodConnectionStringValue));

    builder.Services.AddSingleton(sp =>
    {
        return new BlobServiceClient(prodStorageServiceValue);
    });

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
    var ConnectionString = builder.Configuration.GetConnectionString("DefaultSQLConnection");
    builder.Services.AddDbContext<CRMApiDbContext>(options => options.UseSqlServer(ConnectionString));

    //Inject Blob Storage Service
    builder.Services.AddSingleton(sp =>
    {
        var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnectionString");
        return new BlobServiceClient(storageConnectionString);
    });
}

// Add JWT Authentication
var KeyVaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"];
var KeyVaultClient = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCridential());

var sengridApiKeySecret = await KeyVaultClient.GetSecretAsync("SengridApiKey");
string sengridApiKeyValue = sengridSecret.Value.Value;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sengridApiKeyValue))
        };
    });

// Inject Models Services
builder.Services.AddScoped<IDeveloperService, DeveloperService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITeamService, TeamService>();


builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer(); 

var app = builder.Build();

// Configure the HTTP request pipeline.

    
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
    c.RoutePrefix = ""; 
});


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
