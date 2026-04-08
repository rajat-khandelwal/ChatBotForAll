using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(Guid tenantId, string email);
        Task<AppUser?> GetByIdAsync(Guid tenantId, Guid userId);
        Task<AppUser> AddAsync(AppUser user);
    }
}
