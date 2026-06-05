using CRM_API.Domain.DTos;
using CRM_API.Domain.Models;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CRM_API.Domain.DTOs.AuthDtos;
using CRM_API.Options;


namespace CRMApi.Utility.Services
{
    public class TokenService : ITokenService
    {
       
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly JwtOptions _jwtOptions;
        public TokenService( IConfiguration config, UserManager<ApplicationUser> userManager
        , IOptions<AppSettings> appSettings, AppDbContext context, IOptions<JwtOptions> jwtOptions)
        {
    
            _userManager = userManager;
            _context = context;
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

            // --- 2. GENERATE REFRESH TOKEN (The missing piece!) ---
                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);
                var refreshTokenValue = Convert.ToBase64String(randomBytes);

                // 3. Save to datbse
                var refreshToken = new RefreshToken
                {
                    JwtId = JwtId,
                    UserId = user.Id,
                    Token = refreshTokenValue,
                    AddedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsUsed = false,
                    IsRevoked = false
                };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

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

            if (!validatedToken.IsValid || validatedToken.SecurityToken is not JsonWebToken jwtToken)
            {
                throw new SecurityTokenException("Invalid access token.");
            }

            string jwtId = validatedToken.SecurityToken.Id;

            // 1. Find the refresh token in the database
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(u => u.Token == request.RefreshToken);
            
            if(storedToken is null)
            {
                throw new SecurityTokenException("Refresh token does not exist.");
            }

            if(storedToken.JwtId != jwtId)
            {
                throw new SecurityTokenException("Jwt ID mismatch.");
            }
            
            // 2. Validate everything
            if (storedToken.IsUsed || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }

            // 3. Get the user
            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user is null)
            {
                throw new SecurityTokenException("User no longer exists.");
            }

            // 4. Token Rotation: Kill the old refresh token!
            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            // 5. Generate a brand new pair and return them
            return await GenerateTokenPairAsync(user);
        }
    }
}
