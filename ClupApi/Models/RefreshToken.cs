using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("RefreshToken")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string UserType { get; set; } = string.Empty; // "student" or "club"

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        [MaxLength(100)]
        public string? RevokedReason { get; set; }
    }
}
