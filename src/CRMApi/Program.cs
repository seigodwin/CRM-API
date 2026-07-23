
using CRM_API.Options;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Extentions;
using CRMApi.Options;
using CRMApi.Services.Implimentations;
using CRMApi.Services.Interfaces;
using CRMApi.Services.ModelServices;
using CRMApi.Services.Services;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using Scalar.AspNetCore;
using Serilog;
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

        builder.Services.AddApplication(builder.Environment, builder.Configuration);
        builder.Services.AddInfracstructure(builder.Configuration, builder.Environment);
        builder.Services.AddPresentation(builder.Environment, builder.Configuration);

        builder.AddHost();

        var app = builder.Build(); 

        app.UseApplication(builder.Environment);
        
        await app.RunAsync();
    }
}
