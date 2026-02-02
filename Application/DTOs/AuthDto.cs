namespace MinhaApi.Application.DTOs;

public record LoginDto(
    string Email,
    string Senha
);

public record LoginResponseDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UsuarioDto Usuario
);

public record RefreshTokenDto(
    string RefreshToken
);
