using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{postId}/photos/{photoId}")]
    [ApiController]
    public class PostPhotoController : ControllerBase
    {
        public IEnumerable<PhotoWrapper> GetCommentsByPostId([FromRoute] int id)
        {
            throw new InvalidOperationException();
        }

        [HttpPost]
        public void AddPhotoToPost([FromRoute] int id, [FromBody] IFormFile photo)
        {

        }
    }
}
