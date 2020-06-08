using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.Controllers
{
    [Route("session")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        [HttpPost]
        public string BeginSession()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public bool ValidateSession([FromBody] string jwt)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public void InvalidateSession([FromBody] string jwt)
        {
            throw new NotImplementedException();
        }
    }
}
