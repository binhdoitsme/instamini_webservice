using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaminiWebService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("users/{id}/follows")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<User> GetFollowersByUser(int id)
        {
            throw new InvalidOperationException();
        }

        [HttpPost]
        public void MakeFollowRelationship([FromRoute] int id, [FromQuery] int followedUser)
        {

        }

        [HttpDelete]
        public void StopFollowing([FromRoute] int id, [FromQuery] int followedUser)
        {

        }
    }
}
