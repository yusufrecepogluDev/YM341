namespace KampusEtkinlik.Data.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Categories { get; set; } = string.Empty;
        public string CategoriesColor { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class Category
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
