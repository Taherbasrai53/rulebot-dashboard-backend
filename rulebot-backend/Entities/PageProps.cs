namespace rulebot_backend.Entities
{
    public class PageProps
    {
        public string Page { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class DashboardData
    {
        public string Page { get; set; }
        public int StageCompliantNum { get; set; }
        public int StageUnCompliantNum { get; set; }
        public int DataItemCompliantNum { get; set; }
        public int DataItemUnCompliantNum { get; set; }
    }
}
