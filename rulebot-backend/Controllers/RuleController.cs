using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using rulebot_backend.BLL.Definition;
using rulebot_backend.BLL.Implementation;
using rulebot_backend.DAL.Implementation;
using rulebot_backend.Entities;

namespace rulebot_backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]   
    public class RuleController:Controller
    {
        IRuleService _ruleService;
        IConnectionService _connectionService;
        public RuleController(IRuleService ruleService, IConnectionService connectionService)
        {
            _ruleService = ruleService;
            _connectionService = connectionService;
        }

        [HttpGet("get-rules")]
        [Authorize]
        public ActionResult GetRules(int ruleType)
        {
            try
            {
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                var builder = new SqlConnectionStringBuilder(client_db);
                string databaseName = builder.InitialCatalog;

                return Ok(_ruleService.GetRuleDefinitions(databaseName, ruleType, tenant_db));
            }
            catch { return Problem(); }
        }

        [HttpPost("get-dashboard-data")]
        [Authorize]
        public ActionResult GetDashboardData(DashboardDataReq req)
        {
            try
            {
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.GetDashBoardParams(req.ProcessId, req.Pages, 1, client_db, tenant_db));
            }
            catch { return Problem(); }
        }

        [HttpPost("save-rule")]
        [Authorize]
        public ActionResult AddRule(RuleDefinition def)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var builder = new SqlConnectionStringBuilder(client_db);
                def.database = builder.InitialCatalog;

                var isSuccess= _ruleService.SaveEditRule(def, 1, tenant_db);
                if (!isSuccess)
                {
                    return BadRequest("Some of the selected pages are already in use.");
                }
                return Ok();
            }
            catch { return Problem(); }
        }

        [HttpPut("edit-rule")]
        [Authorize]
        public ActionResult EditRule(RuleDefinition def)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");

                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var builder = new SqlConnectionStringBuilder(client_db);
                def.database = builder.InitialCatalog;
                var isSuccess = _ruleService.SaveEditRule(def, 1, tenant_db);
                if (!isSuccess)
                {
                    return BadRequest("Some of the selected pages are already in use.");
                }
                return Ok();
            }
            catch { return Problem(); }
        }
        
        [HttpDelete("delete-rule")]
        [Authorize]
        public ActionResult DeleteRule(int id)
        {
            try
            {
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");

                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.DeleteRule(id, tenant_db));
            }
            catch { return Problem(); }
        }

        [HttpPost("run-rule")]
        [Authorize]
        public ActionResult RunRule(List<RuleDefinition> def)
        {
            try
            {
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");

                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }

                if (string.IsNullOrEmpty(tenant_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.ExecuteRule(def, 1, client_db));
            }
            catch { return Problem(); }
        }
    }
}
