using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaminiWebService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("users/{id}/posts")]
    [ApiController]
    public class UserPostController : ControllerBase
    {
        [HttpPost]
        public async Task CreatePost([FromBody] Post post, [FromRoute] int userId)
        {

        }

        [HttpGet] [AllowAnonymous]
        public async Task<IActionResult> GetPostsByUser([FromRoute] int userId)
        {
            throw new NotImplementedException();
        }
    }
}
