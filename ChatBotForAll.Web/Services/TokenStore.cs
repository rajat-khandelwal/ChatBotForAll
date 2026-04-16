using ChatBotForAll.Web.Models.Auth;

namespace ChatBotForAll.Web.Services
{
    public class TokenStore
    {
        public LoginResponse? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser is not null;

        public event Action? OnChange;

        public void SetLogin(LoginResponse response)
        {
            CurrentUser = response;
            OnChange?.Invoke();
        }

        public void ClearLogin()
        {
            CurrentUser = null;
            OnChange?.Invoke();
        }
    }
}
