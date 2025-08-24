using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface IRuleService
    {
        public List<RuleDefinition> GetRuleDefinitions(string database, int ruleType, string connectionString);
        public bool SaveEditRule(RuleDefinition ruleDefinition, int userId, string connectionString);
        public Dictionary<String, String> ExecuteRule(List<RuleDefinition> ruleDefinitions, int userId, string connectionString);
        public List<DashboardData> GetDashBoardParams(string processId, string pages, int userId, string clients_db, string tenant_db);
        public bool DeleteRule(int id, string connectionString);
    }
}
