using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Identity
{
    public class User
    {
        public long Id { get; set; }
        
        [StringLength(160)]
        public required string Name { get; set; }
        
        [StringLength(180)]
        public required string Email { get; set; }
        
        [StringLength(500)]
        public required string PasswordHash { get; set; }
        
        public long RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
