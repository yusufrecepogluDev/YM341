using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("ClubMembership")]
    public class ClubMembership
    {
        [Key]
        public int MembershipID { get; set; }

        [ForeignKey("Student")]
        public int StudentID { get; set; }

        [ForeignKey("Club")]
        public int ClubID { get; set; }

        public DateTime? JoinDate { get; set; }

        public bool? IsApproved { get; set; }

        public Club Club { get; set; }
        public Student Student { get; set; }
    }
}
