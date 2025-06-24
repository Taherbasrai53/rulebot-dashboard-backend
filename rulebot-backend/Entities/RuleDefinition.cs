namespace rulebot_backend.Entities
{
    public class RuleDefinition
    {
        public int Id { get; set; }
        public string ProcessId { get; set; }
        public int UserId { get; set; }
        public string Pages { get; set; }
        public string Stage { get; set; }
        public string Parameters { get; set; }
        public int RuleType { get; set; }
    }
}
