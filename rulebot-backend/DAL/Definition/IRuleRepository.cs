using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface IRuleRepository
    {
        public List<RuleDefinition> GetRuleDefinitions(int userId, int ruleType);
        public bool AddEditRuleDefinition(RuleDefinition def);
        public List<PageProps> getPageProps(string processId, string pages);
    }
}
