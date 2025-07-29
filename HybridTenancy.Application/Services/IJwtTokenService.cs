namespace HybridTenancy.Application.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(TenantInfo tenant);
    }
}
