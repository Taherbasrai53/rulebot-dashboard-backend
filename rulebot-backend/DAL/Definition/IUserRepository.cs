namespace rulebot_backend.DAL.Definition
{
    public interface IUserRepository
    {
        public String getClientConnectionString(int userId);
    }
}
