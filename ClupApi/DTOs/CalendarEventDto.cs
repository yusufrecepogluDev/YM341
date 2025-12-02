namespace ClupApi.DTOs
{
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Categories { get; set; } // "AkademikOlay", "KulupEtkinligi", "Duyuru"
        public string CategoriesColor { get; set; }
        public bool IsAllDay { get; set; }
        public string Location { get; set; }
    }

    public class CategoryDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
    }
}
