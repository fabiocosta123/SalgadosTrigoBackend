using System.ComponentModel.DataAnnotations;

namespace SalgadosTrigo.Api.Entities.Identity
{
    public class Role
    {
        public long Id { get; set; }

        [StringLength(40)]
        public required string Code { get; set; }

        [StringLength(80)]
        public required string Name { get; set; }

    }
}
