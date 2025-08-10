namespace rulebot_backend.Entities
{
    public class SubscriptionDto
    {
        public int Bots { get; set; }
        public DateTime FromDate { get; set; }=DateTime.UtcNow;
        public DateTime ToDate { get; set; } = DateTime.UtcNow;
        public double Billed { get; set; }
    }
}
