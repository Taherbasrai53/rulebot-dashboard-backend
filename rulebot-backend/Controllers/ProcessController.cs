using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rulebot_backend.BLL.Definition;
using System.Security.Claims;

namespace rulebot_backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ProcessController:Controller
    {
        IProcessService _processService;
        public ProcessController(IProcessService processService)
        {
            _processService = processService;
        }

        [HttpGet("get-process-names")]
        //[Authorize]
        public ActionResult GetProcessName()
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_processService.getProcessNames(1));
            }
            catch { return Problem();  }
        }

        [HttpGet("get-process-details")]
        public ActionResult GetProcessDetails(string processId)
        {
            try
            {
                return Ok(_processService.getProcessDetails(processId, 1));
            }
            catch { return Problem(); }
        }
    }
}
