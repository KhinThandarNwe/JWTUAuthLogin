using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTUAuthLogin.Infrastructure.Repository.TokenModule
{
    public class JwtAuth
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public int TokenLifeTime { get; set; }
    }

    public interface ITokenManager
    {
        string GenerateToken(string user_id, string license_id);
        ServiceActionResult ValidateToken(IHeaderDictionary Header);
        AuthorizedUser GetAuthorizedUser();
        (string requestId, string licenseId) GetUserIdFromToken();
    }

    public class AuthorizedUser
    {
        public string userID { get; set; }
        public string license { get; set; }
    }

    public class TokenManager : ITokenManager
    {
        private readonly JwtAuth _JwtAuth;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenManager(IOptions<JwtAuth> JwtAuth, IHttpContextAccessor httpContextAccessor)
        {
            _JwtAuth = JwtAuth.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateToken(string user_id, string license_id)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtAuth.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //claim is used to add identity to JWT token
            var claims = new[]
            {
                new Claim("userID", user_id),
                new Claim("license", license_id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _JwtAuth.Issuer,
                _JwtAuth.Issuer,
                claims, //null original value
                expires: DateTime.Now.AddMinutes(_JwtAuth.TokenLifeTime),
                signingCredentials: credentials
            );

            string response_token = new JwtSecurityTokenHandler().WriteToken(token); //return access token

            return response_token;
        }

        public ServiceActionResult ValidateToken(IHeaderDictionary Header)
        {
            try
            {
                string token_data = Header["Authorization"]
                    .First()
                    .Replace("Bearer ", string.Empty);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_JwtAuth.Key);
                tokenHandler.ValidateToken(
                    token_data,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = _JwtAuth.Issuer,
                        ValidAudience = _JwtAuth.Issuer,
                        RequireExpirationTime = true
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken = (JwtSecurityToken)validatedToken;
                var claims = jwtToken.Claims.First();
                var userId = jwtToken.Claims.First(x => x.Type == "userID").Value;

                return new ServiceActionResult(ReturnStatus.success, "", userId);
            }
            catch (Exception ex)
            {
                return new ServiceActionResult(ReturnStatus.Unauthorized);
            }
        }

        public AuthorizedUser GetAuthorizedUser()
        {
            AuthorizedUser user = new AuthorizedUser();
            user.userID = _httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userID")
                ?.Value;
            user.license = _httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "license")
                ?.Value;
            return user;
        }

        public (string requestId, string licenseId) GetUserIdFromToken()
        {
            return (
                _httpContextAccessor
                    .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userID")
                    ?.Value,
                _httpContextAccessor
                    .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "license")
                    ?.Value
            );
        }
    }
}

