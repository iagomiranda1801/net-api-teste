using MinhaApi.Application.DTOs;

namespace MinhaApi.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    string GenerateJwtToken(int userId, string email);
    string GenerateRefreshToken();
}
