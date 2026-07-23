using Serilog;

namespace CRMApi.Extentions
{
    public static class HostExtentions
    {
        public static WebApplicationBuilder AddHost(this WebApplicationBuilder builder)
        {
            //Serilog
        var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console();

        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.WriteTo.Seq(
                builder.Configuration["SEQ_CONNECTION_STRING"]
                ?? throw new InvalidOperationException("SEQ_CONNECTION_STRING is not configured."));
        }

        Serilog.Log.Logger = loggerConfig.CreateLogger();
        
        builder.Host.UseSerilog();

        return builder;
        }
    }
}