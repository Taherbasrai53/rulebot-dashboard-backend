using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface IRuleService
    {
        public List<RuleDefinition> GetRuleDefinitions(int userId, int ruleType);
        public bool SaveEditRule(RuleDefinition ruleDefinition, int userId);
        public bool ExecuteRule(List<RuleDefinition> ruleDefinitions, int userId);
        public List<DashboardData> GetDashBoardParams(string processId, string pages, int userId);
    }
}
