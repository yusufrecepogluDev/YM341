using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
 
    /// Kulübe üyelik başvurusu için DTO
 
    public class ClubMembershipApplyDto
    {
        [Required(ErrorMessage = "Kulüp ID'si zorunludur")]
        public int ClubID { get; set; }
    }

    public class ClubMembershipResponseDto
    {
        public int MembershipID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
        public bool? IsApproved { get; set; }
        
 
        /// Üyelik durumu: "Beklemede", "Onaylandı", "Reddedildi"
 
        public string Status => IsApproved switch
        {
            null => "Beklemede",
            true => "Onaylandı",
            false => "Reddedildi"
        };
    }
}