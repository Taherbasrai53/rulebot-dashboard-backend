using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Implementation
{
    public class SubscriptionService: ISubscriptionService
    {
        public ISubscriptionRepository _repo { get; set; }
        public SubscriptionService(ISubscriptionRepository repo)
        {
            _repo = repo;
        }

        public bool AddPlanForClient(SubscriptionDto req)
        {
            return _repo.AddPlanForClient(req);
        }

        public List<SubscriptionDto> GetClientPlans()
        {
            return _repo.GetAllPlansForClient();
        }

        public int GetClientPlansCount()
        {
            return _repo.GetTodayBots();
        }
    }
}
