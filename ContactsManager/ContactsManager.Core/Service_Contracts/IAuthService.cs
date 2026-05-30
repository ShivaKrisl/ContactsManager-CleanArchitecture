using Entities_DTO;

namespace Service_Contracts
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDTO registerDTO);

        Task<AuthResult> LoginAsync(LoginDTO loginDTO, string? returnUrl = null);

        Task LogoutAsync();

        Task<bool> IsEmailAlreadyExistsAsync(string? email);

        Task<bool> IsInRoleAsync(string email, string role);
    }
}