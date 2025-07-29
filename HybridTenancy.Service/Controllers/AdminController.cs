using Microsoft.AspNetCore.Mvc;
using HybridTenancy.Application.Services;

namespace HybridTenancy.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ITenantService tenantService,
            IJwtTokenService jwtTokenService,
            ILogger<AdminController> logger)
        {
            _tenantService = tenantService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterTenant([FromBody] TenantInfo tenant)
        {
            try
            {
                _logger.LogInformation("Registering tenant with Identifier: {Identifier}", tenant.Identifier);

                await _tenantService.RegisterTenantAsync(tenant);
                var token = _jwtTokenService.GenerateToken(tenant);

                _logger.LogInformation("Tenant registered successfully. TenantId: {TenantId}", tenant.TenantId);

                return Ok(new { token, tenant.TenantId, tenant.Mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering tenant with Identifier: {Identifier}", tenant.Identifier);
                return StatusCode(500, "An error occurred while registering the tenant.");
            }
        }

        [HttpGet("tenants")]
        public async Task<IActionResult> GetTenants()
        {
            try
            {
                _logger.LogInformation("Fetching all tenants...");
                var tenants = await _tenantService.GetAllTenantsAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tenants.");
                return StatusCode(500, "An error occurred while fetching tenants.");
            }
        }
    }
}
