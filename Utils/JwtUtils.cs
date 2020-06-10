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
                                        string userId,
                                        string issuer = ISSUER, 
                                        string secret = SECRET, 
                                        int expireTimeInMinutes = EXPIRATION_IN_MINUTES)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.Sha512);

            var secToken = new JwtSecurityToken(
                signingCredentials: credentials,
                issuer: issuer,
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, userId)
                },
                expires: DateTime.UtcNow.AddMinutes(expireTimeInMinutes)
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(secToken);
        }

        public static IPrincipal ValidateJWT(string jwt)
        {
            if (JwtBlacklist.Contains(jwt)) return null;

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

            IPrincipal principal = tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);

            return principal;
        }
    }
}
