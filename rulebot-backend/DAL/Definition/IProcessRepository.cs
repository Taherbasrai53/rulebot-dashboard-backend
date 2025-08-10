using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface IProcessRepository
    {
        public List<LockedProcess> getProcessNames(string connectionString);
        public String getProcessXML(string connectionString, string processId);
        public void UpdateProcessXml(string connectionString, string processId, string xmlFilePath);
        public List<ProcessItem> getSelectedProcessesByDb(string connectionString, string database);
        public List<LockedProcess> getSelectedProcesses(string connectionString);
        public bool addSelectedProcesses(string connectionString, List<LockedProcess> req, string database);
    }
}
