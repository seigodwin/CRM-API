using System.Text;
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

var builder = WebApplication.CreateBuilder(args);


//Add Serilog
//var configuration = builder.Configuration;

//Log.Logger = new LoggerConfiguration() 
//    .ReadFrom.Configuration(configuration)
//    .CreateLogger();

//builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();


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

var ConnectionString = builder.Configuration.GetConnectionString("DefaultSQLConnection");
builder.Services.AddDbContext<CRMApiDbContext>(options => options.UseSqlServer(ConnectionString));

//if (builder.Environment.IsDevelopment())
//{
//    var KeyVaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"];

//    var credentials = new ClientSecretCredential(KeyVaultDirectoryId, KeyVaultClientId, KeyVaultClientSecret);
//    var secretClient = new SecretClient(new Uri(KeyVaultUrl), credentials);

//    // Modern way without deprecated overload
//    builder.Configuration.AddAzureKeyVault(new AzureKeyVaultConfigurationOptions 
//    {
//        Vault = new Uri(KeyVaultUrl),
//        Client = secretClient
//    });

//    builder.Services.AddSingleton(sp =>
//    {
//        var secret = secretClient.GetSecret("AzureStorageConnectionString").Value.Value;
//        return new BlobServiceClient(secret);
//    });
//}


// Add JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Inject Models Services
builder.Services.AddScoped<IDeveloperService, DeveloperService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITeamService, TeamService>();


//Inject Blob Storage Service
builder.Services.AddSingleton(sp =>
{
    var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnectionString");
return new BlobServiceClient(storageConnectionString);
});


builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
        c.RoutePrefix = ""; 
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
