using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("AcademicEvents")]
    public class AcademicEvents
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
        
        public string Category { get; set; } = string.Empty;
    }
}
