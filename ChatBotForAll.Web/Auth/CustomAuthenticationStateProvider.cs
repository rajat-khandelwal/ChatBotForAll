using ChatBotForAll.Web.Constants;
using ChatBotForAll.Web.Models.Auth;
using ChatBotForAll.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace ChatBotForAll.Web.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private static readonly AuthenticationState AnonymousState =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly TokenStore _tokenStore;
        private readonly ProtectedLocalStorage _localStorage;
        private bool _initialized;
        private AuthenticationState? _cachedState;

        public CustomAuthenticationStateProvider(TokenStore tokenStore, ProtectedLocalStorage localStorage)
        {
            _tokenStore = tokenStore;
            _localStorage = localStorage;
            _tokenStore.OnChange += OnTokenStoreChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Try to restore from localStorage if the circuit token is empty (e.g. after page refresh)
            if (!_initialized && !_tokenStore.IsAuthenticated)
            {
                _initialized = true;
                try
                {
                    var stored = await _localStorage.GetAsync<LoginResponse>(CookiesConstant.AuthCookieName);
                    if (stored.Success && stored.Value is not null)
                    {
                        _tokenStore.SetLogin(stored.Value);
                    }
                }
                catch
                {
                    // JS interop not available during SSR or prerender phase — safe to ignore
                }
            }

            if (_tokenStore.CurrentUser is null)
            {
                return AnonymousState;
            }

            return _cachedState ??= BuildAuthenticationState(_tokenStore.CurrentUser);
        }

        private static AuthenticationState BuildAuthenticationState(LoginResponse user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenant_id", user.TenantId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private void OnTokenStoreChanged()
        {
            _cachedState = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Dispose()
        {
            _tokenStore.OnChange -= OnTokenStoreChanged;
        }
    }
}
