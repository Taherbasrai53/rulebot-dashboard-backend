namespace rulebot_backend.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Client { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
    }
}
