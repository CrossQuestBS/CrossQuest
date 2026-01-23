namespace CrossQuestUI.Models
{
    public record struct ModInfo()
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DownloadUrl { get; set; }
        public bool Required { get; set; }
    }
}