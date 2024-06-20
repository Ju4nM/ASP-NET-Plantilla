using Microsoft.AspNetCore.Mvc;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizeAttributes;
using sigecendis_api.Data.Filters;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.Auth;
using sigecendis_api.Model;
using System.Security.Claims;

namespace sigecendis_api.Controllers {

  [ServiceFilter<ExceptionFilter>]
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase {

    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthRepository authRepository) => _authRepository = authRepository;

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginRequestData) {
      LoginResponse responseData = await _authRepository.Login(loginRequestData);
      return Ok(responseData);
    }

    [RoleAuthorize(ROLES.COORDINATION, ROLES.CENDI_MANAGEMENT, ROLES.GENERAL)]
    [HttpGet("Verificar-token")]
    public IActionResult VerifyToken () {
      return NoContent();
    }

    [RoleAuthorize(ROLES.COORDINATION, ROLES.CENDI_MANAGEMENT, ROLES.GENERAL)]
    [HttpPost("Logout")]
    public async Task<IActionResult> LogOut () {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      await _authRepository.LogOut(userId);
      return NoContent();
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> RefreshToken (RefreshTokenDto refreshTokenDto) {
      RefreshTokenResponse result = await _authRepository.RefreshToken(refreshTokenDto);
      return Ok(result);
    }
  }
}
