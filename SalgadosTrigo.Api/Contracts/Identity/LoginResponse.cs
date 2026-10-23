namespace SalgadosTrigo.Api.Contracts.Identity
{
    public class LoginResponse
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public long RoleId { get; set; }
        public required string RoleCode { get; set; }
    }
}
