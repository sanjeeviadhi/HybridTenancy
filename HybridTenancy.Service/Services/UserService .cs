using Dapper;
using HybridTenancy.Application.Models;
using HybridTenancy.Application.Services;
using HybridTenancy.Persistence.Entities;
using HybridTenancy.Service.Multitenancy;
using BCrypt.Net;

namespace HybridTenancy.Service.Services
{
    public class UserService : IUserService
    {
        private readonly TenantConnectionFactory _connectionFactory;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            TenantConnectionFactory connectionFactory,
            IJwtTokenService jwtTokenService,
            ILogger<UserService> logger)
        {
            _connectionFactory = connectionFactory;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task RegisterAsync(UserRegisterRequest request)
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection();
                var tenant = _connectionFactory.CreateDbContext().Tenant;

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var sql = @"
                    INSERT INTO ""Users"" (""TenantId"", ""Username"", ""Password"", ""Role"")
                    VALUES (@TenantId, @Username, @Password, @Role)";

                var user = new User
                {
                    TenantId = tenant.TenantId!.Value,
                    Username = request.Username,
                    Password = hashedPassword,
                    Role = "User"
                };

                await conn.ExecuteAsync(sql, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                throw;
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection();
                var tenant = _connectionFactory.CreateDbContext().Tenant;

                var sql = @"SELECT * FROM ""Users"" WHERE ""Username"" = @Username AND ""TenantId"" = @TenantId";

                var user = await conn.QuerySingleOrDefaultAsync<User>(sql, new
                {
                    Username = request.Username,
                    TenantId = tenant.TenantId
                });

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }

                var token = _jwtTokenService.GenerateToken(new TenantInfo
                {
                    TenantId = tenant.TenantId,
                    Mode = tenant.Mode
                });

                return new LoginResponse
                {
                    Token = token,
                    Username = user.Username
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed.");
                throw new Exception("Login error occurred.");
            }
        }
    }
}
