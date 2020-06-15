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
    [Route("users/{id}/avatar")]
    public class UserAvatarController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly InstaminiContext DbContext;
        private readonly string AvatarServingPath;

        public UserAvatarController(IConfiguration configuration, InstaminiContext context)
        {
            Configuration = configuration;
            DbContext = context;
            AvatarServingPath = configuration.GetValue<string>("AvatarServingAbsolutePath");
        }

        [HttpPatch] [Authorize]
        public async Task<IActionResult> UpdateAvatar([FromRoute] int id, [FromForm] IFormFile newAvatar)
        {
            string jwt = Request.Cookies["Token"];
            if (string.IsNullOrEmpty(jwt))
            {
                return BadRequest(new { Err = "Unauthorized user!" });
            }
            int userId = int.Parse(JwtUtils.ValidateJWT(jwt)?.Claims
                                .Where(claim => claim.Type == ClaimTypes.NameIdentifier)
                                .FirstOrDefault().Value);
            if (userId != id)
            {
                return BadRequest(new { Err = "You cannot update others' avatars!" });
            }

            // get avatar record
            var avatarRecord = await DbContext.AvatarPhotos.Where(a => a.UserId == id).FirstOrDefaultAsync();

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
