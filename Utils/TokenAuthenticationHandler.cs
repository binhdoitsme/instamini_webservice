using InstaminiWebService.Database;
using InstaminiWebService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace InstaminiWebService.Utils
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        private readonly DbSet<User> UserDatabase;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            InstaminiContext context)
            : base(options, logger, encoder, clock)
        {
            UserDatabase = context.Users;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var apiKeyQuery = Request.Query["key"];
            Logger.LogInformation($"key={apiKeyQuery}");
            if (string.IsNullOrEmpty(apiKeyQuery))
            {
                Response.Cookies.Delete("Token");
                return AuthenticateResult.Fail("Token is missing!");
            }
            var authResult = JwtUtils.ValidateJWT(apiKeyQuery);
            if (authResult is null)
            {
                Response.Cookies.Delete("Token");
                return AuthenticateResult.Fail("Token is invalid!");
            }
            var authResultUsernameId = int.Parse(authResult.Claims
                                                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                                                    .FirstOrDefault()
                                                    .Value);
            bool isValidUser = await UserDatabase.Where(u => u.Id == authResultUsernameId).FirstOrDefaultAsync() != null;
            if (!isValidUser)
            {
                Response.Cookies.Delete("Token");
                return AuthenticateResult.Fail("Token is invalid!");
            }
            Context.User = authResult;
            return AuthenticateResult.Success(new AuthenticationTicket(authResult, "TokenBased"));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonSerializer.Serialize(new { Err =  "Unauthorized user!" }));
        }
    }
}
