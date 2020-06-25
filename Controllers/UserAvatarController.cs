using InstaminiWebService.Database;
using InstaminiWebService.Models;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaminiWebService.Controllers
{
    [Route("users/{username}/avatar")]
    public class UserAvatarController : ControllerBase
    {
        private readonly InstaminiContext DbContext;
        private readonly string AvatarServingPath;

        public UserAvatarController(IConfiguration configuration, InstaminiContext context)
        {
            DbContext = context;
            AvatarServingPath = configuration.GetValue<string>("AvatarServingAbsolutePath");
        }

        [HttpPatch] [Authorize]
        public async Task<IActionResult> UpdateAvatar([FromRoute] string username, [FromForm] IFormFile newAvatar)
        {
            string jwtUsername = User.FindFirstValue(ClaimTypes.Name);
            if (jwtUsername != username)
            {
                return BadRequest(new { Err = "You cannot update others' avatars!" });
            }

            // get avatar record
            var avatarRecord = await DbContext.AvatarPhotos
                                        .Include(a => a.User)
                                        .Where(a => a.User.Username == username)
                                        .FirstOrDefaultAsync();

            // upload photo
            try
            {
                string avatarPath = await PhotoUtils.UploadPhotoAsync(newAvatar, AvatarServingPath);
                DbContext.Entry(avatarRecord).CurrentValues.SetValues(new { FileName = avatarPath });
                await DbContext.SaveChangesAsync();
                return Ok();
            } catch (InvalidDataException)
            {
                return BadRequest(new { Err = "Photo is invalid!" });
            }
        }
    }
}
