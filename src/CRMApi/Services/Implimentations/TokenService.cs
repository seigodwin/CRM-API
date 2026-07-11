using CRM_API.Domain.DTos;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CRM_API.Domain.DTOs.AuthDtos;
using CRM_API.Options;
using StackExchange.Redis;
using System.Security;


namespace CRMApi.Utility.Services
{
    public class TokenService : ITokenService
    {
      
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly IDatabase _cache;
        public TokenService(UserManager<ApplicationUser> userManager
        , IConnectionMultiplexer redis, IOptions<JwtOptions> jwtOptions) 
        {
            _cache = redis.GetDatabase();
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value; 
        }

        public async Task<AuthenticatedUsertDto> GenerateTokenPairAsync(ApplicationUser user)
        {
            var tokenHandler = new JsonWebTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);

            // 1. Generate a brand new unique ID right here
            string JwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, JwtId)  
            };

            //check and include roles
            var roles = await _userManager.GetRolesAsync(user);
            if (roles is not null)
            {
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwtOptions.Audience,
                Issuer = _jwtOptions.Issuer,
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
          };

            string accessToken = tokenHandler.CreateToken(tokenDescriptor);

            //  2. GENERATE REFRESH TOKEN 
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create(); 
                rng.GetBytes(randomBytes);
                var refreshTokenValue = Convert.ToBase64String(randomBytes);

                var redisKey = $"refresh:{user.Id}";

                await _cache.StringSetAsync(
                    redisKey,
                    refreshTokenValue,
                    TimeSpan.FromDays(7)
                );

            // --- 4. RETURN THE PAIR ---
            
            return new AuthenticatedUsertDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiration = tokenDescriptor.Expires.GetValueOrDefault()
            };
        }


        public async Task<AuthenticatedUsertDto> RefreshTokenAsync(RefreshRequestDto request)
        {
            if(request is null || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new SecurityTokenException("Invalid client request.");
            }

            var tokenHandler = new JsonWebTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };

            var validatedToken = await tokenHandler.ValidateTokenAsync(request.AccessToken, tokenValidationParameters);

            if (validatedToken.SecurityToken is not JsonWebToken jwtToken)
            {
                throw new SecurityTokenException("Invalid access token.");
            }

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            // 1. Find the refresh token in the database
            var key = $"refresh:{userId}";
            var storedToken = await _cache.StringGetAsync(key);
            
            if(storedToken.IsNullOrEmpty)
            {
                throw new SecurityTokenException("Refresh token does not exist.");
            }

            if(storedToken != request.RefreshToken)
            {
                throw new SecurityException("Refresh token mismatch");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityException("User not found");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new SecurityTokenException("User no longer exists.");
            }

            // 5. Generate a brand new pair and return them
            return await GenerateTokenPairAsync(user);
        }
    }
}
