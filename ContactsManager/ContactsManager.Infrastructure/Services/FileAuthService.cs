using System.Security.Claims;
using System.Text.Json;
using ContactsManager.Core.Domain.IdentityEntities;
using Entities_DTO;
using Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Service_Contracts;
using Storage;

namespace Service_Classes
{
    public class FileAuthService : IAuthService
    {
        private readonly JsonFileStore _fileStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FileStorageOptions _options;

        public FileAuthService(JsonFileStore fileStore, IHttpContextAccessor httpContextAccessor, IOptions<FileStorageOptions> options)
        {
            _fileStore = fileStore;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDTO registerDTO)
        {
            AuthResult validationResult = ValidateRegisterRequest(registerDTO);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            List<ApplicationUser> users = await ReadUsersAsync();
            if (users.Any(user => string.Equals(user.Email, registerDTO.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return new AuthResult { Errors = new List<string> { "Email already exists!!" } };
            }

            List<ApplicationRole> roles = await ReadRolesAsync();
            string roleName = registerDTO.UserType.ToString();
            EnsureRoleExists(roles, roleName);

            ApplicationUser user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = registerDTO.FirstName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                PasswordHash = PasswordHelper.HashPassword(registerDTO.Password!),
                Roles = new List<string> { roleName }
            };

            users.Add(user);
            await SaveUsersAsync(users);
            await SaveRolesAsync(roles);
            await SignInAsync(user);

            return new AuthResult
            {
                Succeeded = true,
                IsAdmin = roleName == Enums.UserRoles.ADMIN.ToString()
            };
        }

        public async Task<AuthResult> LoginAsync(LoginDTO loginDTO, string? returnUrl = null)
        {
            if (!ValidationHelper.IsModelValid(loginDTO))
            {
                return new AuthResult { Errors = ValidationHelper.Errors.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList() };
            }

            List<ApplicationUser> users = await ReadUsersAsync();
            ApplicationUser? user = users.FirstOrDefault(item => string.Equals(item.Email, loginDTO.Email, StringComparison.OrdinalIgnoreCase));

            if (user == null || !PasswordHelper.VerifyPassword(loginDTO.Password!, user.PasswordHash ?? string.Empty))
            {
                return new AuthResult { Errors = new List<string> { "Invalid Email Id or Password attempt" } };
            }

            await SignInAsync(user);

            return new AuthResult
            {
                Succeeded = true,
                IsAdmin = user.Roles.Any(role => string.Equals(role, Enums.UserRoles.ADMIN.ToString(), StringComparison.OrdinalIgnoreCase))
            };
        }

        public async Task LogoutAsync()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        public async Task<bool> IsEmailAlreadyExistsAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            List<ApplicationUser> users = await ReadUsersAsync();
            return !users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> IsInRoleAsync(string email, string role)
        {
            List<ApplicationUser> users = await ReadUsersAsync();
            ApplicationUser? user = users.FirstOrDefault(item => string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase));
            return user?.Roles.Any(userRole => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private AuthResult ValidateRegisterRequest(RegisterDTO registerDTO)
        {
            if (registerDTO == null)
            {
                return new AuthResult { Errors = new List<string> { "Register request cannot be null." } };
            }

            bool isValid = ValidationHelper.IsModelValid(registerDTO);
            if (!isValid)
            {
                return new AuthResult { Errors = ValidationHelper.Errors.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList() };
            }

            return new AuthResult { Succeeded = true };
        }

        private async Task SignInAsync(ApplicationUser user)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (string role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });
        }

        private async Task<List<ApplicationUser>> ReadUsersAsync()
        {
            return await _fileStore.ReadListAsync(_options.UsersFileName, () => new List<ApplicationUser>());
        }

        private async Task SaveUsersAsync(List<ApplicationUser> users)
        {
            await _fileStore.WriteListAsync(_options.UsersFileName, users);
        }

        private async Task<List<ApplicationRole>> ReadRolesAsync()
        {
            return await _fileStore.ReadListAsync(_options.RolesFileName, GetDefaultRoles);
        }

        private async Task SaveRolesAsync(List<ApplicationRole> roles)
        {
            await _fileStore.WriteListAsync(_options.RolesFileName, roles);
        }

        private static List<ApplicationRole> GetDefaultRoles()
        {
            return new List<ApplicationRole>
            {
                new ApplicationRole { Id = Guid.NewGuid(), Name = Enums.UserRoles.USER.ToString(), NormalizedName = Enums.UserRoles.USER.ToString().ToUpperInvariant() },
                new ApplicationRole { Id = Guid.NewGuid(), Name = Enums.UserRoles.ADMIN.ToString(), NormalizedName = Enums.UserRoles.ADMIN.ToString().ToUpperInvariant() }
            };
        }

        private static void EnsureRoleExists(List<ApplicationRole> roles, string roleName)
        {
            if (!roles.Any(role => string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase)))
            {
                roles.Add(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }
    }
}