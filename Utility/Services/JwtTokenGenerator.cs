using CRMApi.Domain.Models;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace CRMApi.Utility.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
       
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        public JwtTokenGenerator( IConfiguration config, UserManager<ApplicationUser> userManager
        , IOptions<AppSettings> appSettings)
        {
            _config = config;
            _userManager = userManager;
            _appSettings = appSettings.Value;

        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JsonWebTokenHandler();

            var JwtKeyValue = _appSettings.JwtSecret;

            var key = Encoding.UTF8.GetBytes(JwtKeyValue);

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!)   
            };

            //check and include roles
            var roles = await _userManager.GetRolesAsync(user);
            if (roles is not null)
            {
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _config["Jwt:Audience"],
                Issuer = _config["Jwt:Issuer"],
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
          };

            string token = tokenHandler.CreateToken(tokenDescriptor);
            return token;  
        }
    }
}
