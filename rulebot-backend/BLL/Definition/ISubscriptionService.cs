using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface ISubscriptionService
    {
        public bool AddPlanForClient(SubscriptionDto req);
        public List<SubscriptionDto> GetClientPlans();
        public int GetClientPlansCount();
    }
}
