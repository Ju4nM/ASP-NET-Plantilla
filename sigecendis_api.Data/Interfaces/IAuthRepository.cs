using sigecendis_api.DTOs.Auth;
using sigecendis_api.Model;

namespace sigecendis_api.Data.Interfaces {
  public interface IAuthRepository {

    public Task<LoginResponse> Login(LoginDto loginDto);
    public Task<bool> LogOut(int userId);
    public Task<RefreshTokenResponse> RefreshToken(RefreshTokenDto refreshTokenDto);
  }
}
