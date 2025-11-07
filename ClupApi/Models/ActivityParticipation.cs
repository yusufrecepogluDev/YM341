using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("ActivityParticipation")]
    public class ActivityParticipation
    {
        [Key]
        public int ParticipationID { get; set; }

        [ForeignKey("Activity")]
        public int ActivityID { get; set; }

        [ForeignKey("Student")]
        public int StudentID { get; set; }

        public DateTime? JoinDate { get; set; }

        public int? Rating { get; set; }

        public Activity Activity { get; set; }
        public Student Student { get; set; }

    }
}
