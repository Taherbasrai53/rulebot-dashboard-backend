using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface IUserRepository
    {
        public String getClientConnectionString(string RID);
        public User ValidateLogin(LoginDto req, string connectionString);
    }
}
