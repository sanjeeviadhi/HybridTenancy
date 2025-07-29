namespace HybridTenancy.Application.Models
{
    public class UserRegisterRequest
    {
        public string Identifier { get; set; } = string.Empty; 
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
