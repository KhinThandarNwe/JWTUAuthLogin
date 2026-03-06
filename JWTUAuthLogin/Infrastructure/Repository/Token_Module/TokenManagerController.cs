using JWTUAuthLogin.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace JWTUAuthLogin.Infrastructure.Repository.Token_Module
{
    public class JwtAuth
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public int TokenLifeTime { get; set; }
    }

    public interface ITokenManager
    {
        string GenerateToken(string user_id, string user_name);
        ServiceActionResult ValidateToken(IHeaderDictionary Header);
        AuthorizedUser GetAuthorizedUser();
        (string userId, string userName) GetUserIdFromToken();

    }

    public class AuthorizedUser
    {
        public string userId { get; set; }
        public string userName { get; set; }
    }
    public class TokenManagerController : ITokenManager
    {
        private readonly JwtAuth _JwtAuth;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenManagerController(IOptions<JwtAuth> jwtAuth, IHttpContextAccessor httpContextAccessor)
        {
            _JwtAuth = jwtAuth.Value;
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateToken(string user_id, string user_name)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtAuth.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                 new Claim("user_id", user_id),
                new Claim("user_name", user_name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };
            var token = new JwtSecurityToken(
                issuer: _JwtAuth.Issuer,
                audience: _JwtAuth.Issuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_JwtAuth.TokenLifeTime),
                signingCredentials: credentials
            );
            string response_token = new JwtSecurityTokenHandler().WriteToken(token);
            return response_token;
        }

        public ServiceActionResult ValidateToken(IHeaderDictionary Header)
        {
            try
            {
                string token_data = Header["Authorization"].First().Replace("Bearer ", string.Empty);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_JwtAuth.Key);
                tokenHandler.ValidateToken(
                    token_data,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _JwtAuth.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _JwtAuth.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    },
                    out SecurityToken validatedToken
                );
                var jwtToken = (JwtSecurityToken)validatedToken;
                var claims = jwtToken.Claims;
                var userId = claims.First(x => x.Type == "user_id").Value;
                return new ServiceActionResult(Shared.Enums.ReturnStatus.success, "", userId);
            }
            catch (Exception ex)
            {
                return new ServiceActionResult(Shared.Enums.ReturnStatus.Unauthorized, "Unauthorized", ex.Message);
            }
        }
    
        public AuthorizedUser GetAuthorizedUser()
        {
            AuthorizedUser user = new AuthorizedUser();
           user.userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            user.userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "user_name")?.Value;
            return user;
        }

        public (string userId, string userName) GetUserIdFromToken()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "user_name")?.Value;
            return (userId, userName);
        }   
    }
}
