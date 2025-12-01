namespace KampusEtkinlik.Data.DTOs
{
    public class AnnouncementCrateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public AnnouncementCrateDto() { }

    }
}
