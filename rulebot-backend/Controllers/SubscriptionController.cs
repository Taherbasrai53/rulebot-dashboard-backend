using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rulebot_backend.BLL.Definition;
using rulebot_backend.Entities;

namespace rulebot_backend.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class SubscriptionController:Controller
    {
        ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("save-plan")]
        [Authorize]
        public ActionResult AddPlan(SubscriptionDto req)
        {
            try
            {
                _subscriptionService.AddPlanForClient(req);
                return Ok();
            }
            catch { return Problem("There was some error"); }
        }

        [HttpGet("get-plans")]
        [Authorize]
        public ActionResult GetUserPlans()
        {
            try
            {
                return Ok(_subscriptionService.GetClientPlans());
            }
            catch { return Problem("There was some error"); }
        }

        [HttpGet("get-bot-count")]
        [Authorize]
        public ActionResult GetBotCount()
        {
            try
            {
                return Ok(_subscriptionService.GetClientPlansCount());
            }
            catch { return Problem("There was some error"); }
        }
    }
}
