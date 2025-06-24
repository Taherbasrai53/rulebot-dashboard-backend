using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface IProcessService
    {
        public List<ProcessItem> getProcessNames(int userId);
        public List<String> getProcessDetails(string processId, int userId);
    }
}
