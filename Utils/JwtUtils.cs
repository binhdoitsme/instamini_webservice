using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InstaminiWebService.Utils
{
    public static class JwtUtils
    {
        private const string SECRET = "xx_INSTA_X_MINI_SECRET_xx";
        private const string ISSUER = "INSTAMINI_WEB_SERVICE";
        private const int EXPIRATION_IN_MINUTES = 20;

        private static readonly IList<string> JwtBlacklist = new List<string>();

        public static string CreateJwt(string username, 
                                        int userId,
                                        string issuer = ISSUER, 
                                        string secret = SECRET, 
                                        int expireTimeInMinutes = EXPIRATION_IN_MINUTES)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var secToken = new JwtSecurityToken(
                signingCredentials: credentials,
                issuer: issuer,
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                },
                expires: DateTime.UtcNow.AddMinutes(expireTimeInMinutes)
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }

        public static ClaimsPrincipal ValidateJWT(string token)
        {
            if (JwtBlacklist.Contains(token)) return null;
            if (token is null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var issuerSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET));
            var validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = ISSUER,
                IssuerSigningKey = issuerSecurityKey,
                ValidateAudience = false
            };
            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            } catch (SecurityTokenValidationException)
            {
                return null;
            } catch (Exception)
            {
                throw;
            }
        }

        public static void InvalidateJWT(string jwt)
        {
            JwtBlacklist.Add(jwt);
        }
    }
}
