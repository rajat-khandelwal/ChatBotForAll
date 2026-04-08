using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
    }
}
