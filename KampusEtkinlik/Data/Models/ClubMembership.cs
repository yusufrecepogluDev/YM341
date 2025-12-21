using KampusEtkinlik.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KampusEtkinlik.Data.Models
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

        // Navigation properties
        public Club? Club { get; set; }
        public Student? Student { get; set; }

 
        /// Üyelik durumu: "Beklemede", "Onaylandı", "Reddedildi"
 
        public string Status => IsApproved switch
        {
            null => "Beklemede",
            true => "Onaylandı",
            false => "Reddedildi"
        };
    }
}