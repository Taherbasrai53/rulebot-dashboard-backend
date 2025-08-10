using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface IRuleRepository
    {
        public List<RuleDefinition> GetRuleDefinitions(string database, int ruleType, string connectionString);
        public bool AddEditRuleDefinition(RuleDefinition def, string connectionString);
        public bool DeleteRuleDefinition(int id, string connectionString);
        public List<PageProps> getPageProps(string processId, string pages, string connectionString);
        public List<String> getRulePages(string processId, int ruleType, int ruleId, string connectionString);
    }
}
