using HybridTenancy.Application.Models;
using HybridTenancy.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HybridTenancy.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(ITenantService tenantService, IUserService userService, ILogger<UserController> logger)
        {
            _tenantService = tenantService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdentifierAsync(request.Identifier);
                if (tenant == null || tenant.ValidTill < DateTime.UtcNow)
                    return BadRequest("Invalid or expired tenant.");

                HttpContext.Items["TenantInfo"] = tenant;
                await _userService.RegisterAsync(request);

                _logger.LogInformation("User {Username} registered under tenant {TenantId}", request.Username, tenant.TenantId);
                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User registration failed.");
                return StatusCode(500, "User registration failed.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdentifierAsync(request.Identifier);
                if (tenant == null || tenant.ValidTill < DateTime.UtcNow)
                    return BadRequest("Invalid or expired tenant.");

                HttpContext.Items["TenantInfo"] = tenant;
                var result = await _userService.LoginAsync(request);

                _logger.LogInformation("User {Username} logged in under tenant {TenantId}", result.Username, tenant.TenantId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid username or password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User login failed.");
                return StatusCode(500, "Login error occurred.");
            }
        }
    }
}
