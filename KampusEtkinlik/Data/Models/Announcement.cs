namespace KampusEtkinlik.Data.Models
{
    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public string AnnouncementTitle { get; set; }
        public string AnnouncementContent { get; set; }
        public int ClubID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DeletionDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        // UI için yardımcı özellikler
        public string Title => AnnouncementTitle;
        public string Content => AnnouncementContent;
        public DateTime CreatedAt => CreationDate ?? StartDate;
    }
}
