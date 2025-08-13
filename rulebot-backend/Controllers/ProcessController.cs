using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rulebot_backend.BLL.Definition;
using rulebot_backend.BLL.Implementation;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using rulebot_backend.Entities;


namespace rulebot_backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ProcessController:Controller
    {
        IProcessService _processService;
        IConnectionService _connectionService;
        public ProcessController(IProcessService processService, IConnectionService connectionService)
        {
            _processService = processService;
            _connectionService = connectionService;
        }

        [HttpGet("get-process-names")]
        [Authorize]
        //[Authorize]
        public ActionResult GetProcessName()
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                if (string.IsNullOrEmpty(client_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                //return Ok(_processService.getProcessNames(1, client_db));
                return Ok();
            }
            catch { return Problem();  }
        }

        [HttpGet("get-selected-processes")]
        [Authorize]
        //[Authorize]
        public ActionResult GetSelectedProcesses()
        {
            try
            {
                Console.WriteLine("In Get Selected Process");
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                Console.WriteLine($"clienDb {client_db} tenantDb {tenant_db}");

                if (string.IsNullOrEmpty(tenant_db))
                {
                    Console.WriteLine($"tenantdb ");
                    return Unauthorized(new { message = "Session expired" });
                }
                //var builder = new SqlConnectionStringBuilder(client_db);

                //string databaseName = builder.InitialCatalog;
                return Ok(_processService.getSelectedProcesses(1, tenant_db, client_db));
            }
            catch { return Problem(); }
        }

        [HttpGet("get-selected-processes-by-db")]
        [Authorize]
        //[Authorize]
        public ActionResult GetSelectedProcessesByDB()
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine("Inside Get Selected Process");
                var sessionId = HttpContext.Session.Id;
                var initValue = HttpContext.Session.GetString("Init");
                Console.WriteLine($"[Controller] Session ID: {sessionId}, Init Value: {initValue ?? "null"}");

                var tenant_db = _connectionService.GetDecryptedConnectionString(HttpContext, "tenant_db");
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                Console.WriteLine($"tenant db : {tenant_db} client db : {client_db}");
                if (string.IsNullOrEmpty(tenant_db))
                {
                    Console.WriteLine("tenat_db is null");
                    return Unauthorized(new { message = "Session expired" });
                }
                var builder = new SqlConnectionStringBuilder(client_db);

                string databaseName = builder.InitialCatalog;
                return Ok(_processService.getSelectedProcessDb(databaseName, tenant_db));
            }
            catch { return Problem(); }
        }


        [HttpPost("add-selected-processes")]
        [Authorize]
        //[Authorize]
        public ActionResult AddSelectedProcesses(List<LockedProcess> req)
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
                var builder = new SqlConnectionStringBuilder(client_db);

                string databaseName = builder.InitialCatalog;
                _processService.addSelectedProcesses(tenant_db, req, databaseName);
                return Ok();
            }
            catch { return Problem(); }
        }

        [HttpGet("get-process-details")]
        [Authorize]
        public ActionResult GetProcessDetails(string processId)
        {
            try
            {
                var client_db = _connectionService.GetDecryptedConnectionString(HttpContext, "client_db");
                if (string.IsNullOrEmpty(client_db))
                {
                    return Unauthorized(new { message = "Session expired" });
                }
                return Ok(_processService.getProcessDetails(processId, 1, client_db));
            }
            catch { return Problem(); }
        }
    }
}
