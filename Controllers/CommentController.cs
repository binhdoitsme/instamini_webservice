using InstaminiWebService.Models;
using InstaminiWebService.ModelWrappers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InstaminiWebService.Controllers
{
    [Route("/comments/{id}")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        [HttpGet]
        public CommentWrapper GetCommentById([FromRoute] int id)
        {
            throw new InvalidOperationException();
        }

        [HttpPatch]
        public void UpdateComment([FromRoute] int id, [FromBody] Comment newComment)
        {

        }

        [HttpDelete]
        public void RemoveCommentById([FromRoute] int id)
        {

        }
    }
}