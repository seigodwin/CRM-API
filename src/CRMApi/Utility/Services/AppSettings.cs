namespace CRMApi.Utility.Services
{
    public class AppSettings
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public string DefaultSqlDbConnectionString { get; set; } = string.Empty;
        public string DefaultRedisConnectionString { get; set; } = string.Empty;
        public string SendGridApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
    }
}