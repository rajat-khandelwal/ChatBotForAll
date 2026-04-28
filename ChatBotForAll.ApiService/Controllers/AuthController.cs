using Microsoft.AspNetCore.Mvc;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ChatBotForAll.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized("Invalid tenant/email/password.");
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync();

            return Ok();
        }

        [HttpPost("users")]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _authService.CreateUserAsync(request);
            if (result is null)
            {
                return Conflict("User with the same email already exists in this tenant.");
            }

            return CreatedAtAction(nameof(GetUser), new { userId = result.UserId, tenantId = result.TenantId }, result);
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<ActionResult<UserResponse>> GetUser(Guid userId, [FromQuery] Guid tenantId)
        {
            var result = await _authService.GetUserAsync(userId, tenantId);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Debug endpoint to inspect token claims (dev only)
        /// </summary>
        [HttpGet("debug/claims")]
        [Authorize]
        public ActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            return Ok(new { Claims = claims, IsAuthenticated = User.Identity?.IsAuthenticated });
        }
    }
}
