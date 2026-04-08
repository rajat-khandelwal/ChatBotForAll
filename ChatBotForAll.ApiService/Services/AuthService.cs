using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace ChatBotForAll.ApiService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher<AppUser> passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.TenantId, request.Email);
            if (user is null || !user.IsActive)
            {
                return null;
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var token = _tokenService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                TenantId = user.TenantId
            };
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<UserResponse?> CreateUserAsync(CreateUserRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.TenantId, request.Email);
            if (existing is not null)
            {
                return null;
            }

            var user = new AppUser
            {
                UserId = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = string.Empty,
                Role = request.Role,
                IsActive = true,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var created = await _userRepository.AddAsync(user);
            return MapToResponse(created);
        }

        public async Task<UserResponse?> GetUserAsync(Guid userId, Guid tenantId)
        {
            var user = await _userRepository.GetByIdAsync(tenantId, userId);
            return user is null ? null : MapToResponse(user);
        }

        private static UserResponse MapToResponse(AppUser user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                TenantId = user.TenantId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDateTime = user.CreatedDateTime,
                UpdatedDateTime = user.UpdatedDateTime
            };
        }
    }
}
