using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace InstaminiWebService.ExceptionHandler
{
    [Route("error")]
    [ApiController]
    public class ExceptionHandlerController : ControllerBase
    {
        private readonly IDictionary<int, string> ERROR_CODE_MESSAGE_MAPPING = new Dictionary<int, string> {
            { 400, "Invalid request!" },
            { 401, "You do not have permission to perform this operation!" },
            { 404, "The requested resource cannot be found!"},
            { 500, "The server encountered an error, please try again!" }
        };

        [Route("{errCode}")]
        public IActionResult GetExceptionHandlerResult([FromRoute] int errCode) {
            return new JsonResult(new {
                Err = ERROR_CODE_MESSAGE_MAPPING[errCode]
            });
        }
    }
}