using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.ResponseModels;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbSet<User> UserContext;
        private readonly InstaminiContext DbContext;
        private readonly IResponseModelFactory ResponseModelFactory;
        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;
        private readonly string DefaultAvatarPath;

        public UserController(InstaminiContext context, 
                              IResponseModelFactory responseModelFactory,
                              ILogger<UserController> logger,
                              IConfiguration configuration)
        {
            DbContext = context;
            UserContext = context.Users;
            ResponseModelFactory = responseModelFactory;
            Logger = logger;
            Configuration = configuration;
            DefaultAvatarPath = Configuration.GetValue<string>("DefaultAvatar");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FindUsersByQueryOrId([FromQuery(Name = "q")] string query,
                                                        [FromQuery] int? id)
        {
            if ((query is null && id is null) || (!(query is null) && !(id is null)))
            {
                return BadRequest(new { Err = "You must only supply id or query!" });
            }

            if (query != null)
            {
                var result = await UserContext.Include(u => u.AvatarPhoto)
                                    .Where(u => u.Username.Contains(query))
                                    .Select(u => (UserResponse)ResponseModelFactory.Create(u))
                                    .ToListAsync();
                return Ok(result);
            }
            else
            {
                var result = await UserContext.Include(u => u.AvatarPhoto)
                                        .Where(u => u.Id == id)
                                        .FirstOrDefaultAsync();
                return Ok(ResponseModelFactory.Create(result));
            }
            
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            bool hasDuplicate = UserContext.Any(u => u.Username == user.Username);
            if (hasDuplicate)
            {
                return BadRequest(new { Err =  "Username is duplicated!" });
            }

            string originalPass = user.Password;
            string salt = PasswordUtils.GenerateSalt();
            string hashedPass = PasswordUtils.HashPasswordWithSalt(originalPass, salt);
            var now = DateTimeOffset.UtcNow;

            user.Username = user.Username.Trim();
            user.DisplayName = user.DisplayName.Trim();
            user.Password = hashedPass;
            user.Salt = salt;
            user.Created = now;
            user.LastUpdate = now;

            await UserContext.AddAsync(user);
            await DbContext.SaveChangesAsync();

            // create new avatar record
            var photo = new AvatarPhoto
            {
                UserId = user.Id,
                FileName = DefaultAvatarPath
            };
            DbContext.AvatarPhotos.Add(photo);
            await DbContext.SaveChangesAsync();

            // format output
            user.AvatarPhoto = photo;
            return Created(Url.Action("GetUserByUsername", new { username = user.Username }), ResponseModelFactory.Create(user));
        }

        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            var retrievedUser = await UserContext.Include(u => u.AvatarPhoto)
                                        .Where(u => u.Username == username).FirstOrDefaultAsync();
            if (retrievedUser == null)
            {
                return NotFound();
            }

            // format output
            return Ok(ResponseModelFactory.Create(retrievedUser));
        }

        [HttpPatch("{username}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] User user, 
                                                    [FromRoute] string username)
        {
            var retrievedUser = await DbContext.Users
                                    .Include(u => u.AvatarPhoto)
                                    .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (retrievedUser.Username != username)
            {
                return BadRequest(new { Err =  "You are trying to update an account of another!" });
            }
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            string jwtUsername = JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.Name)
                                .FirstOrDefault().Value;
            if (jwtUsername != username)
            {
                return BadRequest(new { Err = "You cannot delete others' accounts!" });
            }

            // check username
            if (DbContext.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest(new { Err = "Username is duplicated!" });
            }

            // Perform password update right here
            // ----------------------------------

            if (!string.IsNullOrEmpty(user.Password))
            {
                string salt = PasswordUtils.GenerateSalt();
                string hashedPassword = PasswordUtils.HashPasswordWithSalt(user.Password, salt);
                user.Salt = salt;
                user.Password = hashedPassword;
            } else
            {
                user.Password = retrievedUser.Password;
                user.Salt = retrievedUser.Salt;
            }

            if (user.Created == DateTimeOffset.MinValue)
            {
                user.Created = retrievedUser.Created;
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                user.Username = retrievedUser.Username;
            }
            if (string.IsNullOrEmpty(user.DisplayName))
            {
                user.DisplayName = retrievedUser.DisplayName;
            }

            var now = DateTime.UtcNow;
            user.LastUpdate = now;

            // After password update
            DbContext.Entry(retrievedUser).CurrentValues.SetValues(user);
            await DbContext.SaveChangesAsync();
            return Ok(ResponseModelFactory.Create(retrievedUser));
        }

        [HttpDelete("{username}")]
        [Authorize]
        public async Task<IActionResult> RemoveUser([FromRoute] string username)
        {
            // TODO: 
            // CAN ONLY REMOVE THE LOGGED IN USER WITH THE SAME ID, ELSE MUST FORBID
            // ---------------------------------------------
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err =  "Unauthorized user!" });
            }
            string jwtUsername = JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.Name)
                                .FirstOrDefault().Value;
            if (jwtUsername != username)
            {
                return BadRequest(new { Err =  "You cannot delete others' accounts!" });
            }
            // ---------------------------------------------
            var retrievedUser = await UserContext.Where(u => u.Username == username).FirstOrDefaultAsync();
            if (retrievedUser == null)
            {
                return NotFound();
            }

            UserContext.Remove(retrievedUser);
            await DbContext.SaveChangesAsync();
            // remove token in response
            Response.Cookies.Delete("Token");
            return Ok();
        }
    }
}
