namespace HybridTenancy.Persistence.Entities
{
    public class User
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
