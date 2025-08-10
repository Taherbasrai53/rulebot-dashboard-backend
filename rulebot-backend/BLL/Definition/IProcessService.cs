using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface IProcessService
    {
        //public List<ProcessItem> getProcessNames(int userId, string connectionString);
        public List<String> getProcessDetails(string processId, int userId, string connectionString);
        public LokScreenData getSelectedProcesses(int userId, string tenant_db, string client_db);
        public List<ProcessItem> getSelectedProcessDb(string database, string tenant_db);
        public void addSelectedProcesses(string tenant_db, List<LockedProcess> selectedProcesses, string database);
    }
}
