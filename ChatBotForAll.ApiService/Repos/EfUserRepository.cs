using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatBotForAll.ApiService.Repos
{
    public class EfUserRepository : IUserRepository
    {
        private readonly ChatBotDbContext _dbContext;

        public EfUserRepository(ChatBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<AppUser?> GetByEmailAsync(Guid tenantId, string email)
        {
            return _dbContext.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email);
        }

        public Task<AppUser?> GetByIdAsync(Guid tenantId, Guid userId)
        {
            return _dbContext.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId);
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            _dbContext.AppUsers.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}
