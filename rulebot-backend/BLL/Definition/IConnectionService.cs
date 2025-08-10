namespace rulebot_backend.BLL.Definition
{
    public interface IConnectionService
    {
        public Boolean checkConnection(string connectionString);
        public void StoreConnectionString(HttpContext context, string connectionString, string key);
        public string? GetDecryptedConnectionString(HttpContext context, string key);
    }
}
