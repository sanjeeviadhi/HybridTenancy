using HybridTenancy.Application.Models;

public interface IUserService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(UserRegisterRequest request);
}
