namespace rulebot_backend.Entities
{
    public class LoginResponseDTO
    {
        public string token { get; set; }
        public User user { get; set; }
        public string connectionString { get; set; }
    }
}
