using rulebot_backend.Entities;

namespace rulebot_backend.DAL.Definition
{
    public interface IProcessRepository
    {
        public List<ProcessItem> getProcessNames(string connectionString);
        public String getProcessXML(string connectionString, string processId);
        public void UpdateProcessXml(string connectionString, string processId, string xmlFilePath);
    }
}
