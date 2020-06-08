using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{id}")]
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet]
        public PostWrapper GetPostById([FromRoute] int id)
        {
            throw new NotImplementedException();
        }

        [HttpPatch]
        public PostWrapper UpdatePost([FromRoute] int id,[FromBody] Post postToBeUpdated)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public void DeletePost([FromRoute] int id)
        {

        }
    }
}
