using System;
using System.Collections.Generic;
using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("posts/{id}/comments")]
    [ApiController]
    public class PostCommentController : ControllerBase
    {
        public IEnumerable<CommentWrapper> GetCommentsByPostId([FromRoute] int id)
        {
            throw new InvalidOperationException();
        }

        [HttpPost]
        public void CreateCommentForPost([FromRoute] int id, [FromBody] Comment comment)
        {

        }
    }
}