using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface ISubscriptionRepository
    {
        public int GetTodayBots();
        public bool AddPlanForClient(SubscriptionDto req);
        public List<SubscriptionDto> GetAllPlansForClient();
    }
}
