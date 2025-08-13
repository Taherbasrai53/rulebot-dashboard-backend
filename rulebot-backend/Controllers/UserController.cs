using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;

namespace rulebot_backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController: Controller
    {
        IUserService _userService;
        IConnectionService _connectionService;
        public UserController(IUserService userService, IConnectionService connectionService)
        {
            _userService = userService;
            _connectionService = connectionService;
        }

        [HttpPost("login")]
        public IActionResult login(LoginDto req)
        {
            try
            {
                var res = _userService.login(req);
                HttpContext.Session.SetString("Init", "true");
                Console.WriteLine($"[Login] New Session Created: {HttpContext.Session.Id}");

                _connectionService.StoreConnectionString(HttpContext ,res.connectionString, "tenant_db");
                res.connectionString = "";
                return Ok(res);
            }
            catch { return BadRequest("Login Failed"); }
        }
    }
}
