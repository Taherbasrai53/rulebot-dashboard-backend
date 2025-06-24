using Microsoft.AspNetCore.Mvc;
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
        public RuleController(IRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        [HttpGet("get-rules")]
        public ActionResult GetRules(int ruleType)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.GetRuleDefinitions(1, ruleType));
            }
            catch { return Problem(); }
        }

        [HttpPost("get-dashboard-data")]
        public ActionResult GetDashboardData(DashboardDataReq req)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.GetDashBoardParams(req.ProcessId, req.Pages, 1));
            }
            catch { return Problem(); }
        }

        [HttpPost("save-rule")]
        public ActionResult AddRule(RuleDefinition def)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.SaveEditRule(def, 1));
            }
            catch { return Problem(); }
        }

        [HttpPut("edit-rule")]
        public ActionResult EditRule(RuleDefinition def)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.SaveEditRule(def, 1));
            }
            catch { return Problem(); }
        }

        [HttpPost("run-rule")]
        public ActionResult RunRule(List<RuleDefinition> def)
        {
            try
            {
                //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(_ruleService.ExecuteRule(def, 1));
            }
            catch { return Problem(); }
        }
    }
}
