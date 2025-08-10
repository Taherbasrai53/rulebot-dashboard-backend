namespace rulebot_backend.Entities
{
    public class ProcessItem
    {
        public string ProcessId { get; set; }
        public string ProcessType { get; set; }
        public string ProcessName { get; set; }
    }
    
    public class LockedProcess
    {
        public string ProcessId { get; set; }
        public string ProcessType { get; set; }
        public string ProcessName { get; set; }
        public string Database { get; set; }
        public int Rules { get; set; }
    }

    public class LokScreenData
    {
        public List<LockedProcess> locked { get; set; }
        public List<LockedProcess> available { get; set; }
    }
}
