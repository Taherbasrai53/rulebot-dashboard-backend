using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rulebot_backend.BLL.Definition;
using rulebot_backend.BLL.Implementation;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;

namespace rulebot_backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ConnectionController:Controller
    {
        IConnectionService _connService;        
        public ConnectionController(IConnectionService connService)
        {
            _connService = connService;
        }

        [HttpPost("check-connection")]
        [Authorize]
        public IActionResult checkConnection(ConnectionRequest req)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(_connService.checkConnection(req.connectionString))
                {
                    HttpContext.Session.SetString("ConnectionString", req.connectionString);
                    _connService.StoreConnectionString(HttpContext, req.connectionString, "client_db");
                    return Ok();
                }
                else
                {
                    return BadRequest("Connection Failed");
                }
            }
            catch { return Problem(); }
        }
    }
}
