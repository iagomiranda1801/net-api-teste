using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MinhaApi.Application.DTOs;
using MinhaApi.Application.Interfaces;
using MinhaApi.Domain.Interfaces;

namespace MinhaApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository, 
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        // Buscar usuário por email
        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);
        if (usuario == null)
            return null;

        // Verificar senha
        if (!_passwordHasher.VerifyPassword(dto.Senha, usuario.Senha))
            return null;

        // Gerar tokens
        var token = GenerateJwtToken(usuario.Id, usuario.Email);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpiresInHours"] ?? "2")
        );

        var usuarioDto = new UsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.CriadoEm,
            usuario.AtualizadoEm
        );

        return new LoginResponseDto(token, refreshToken, expiresAt, usuarioDto);
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implementar validação de refresh token armazenado no banco
        // Por enquanto, retorna null (você pode implementar depois)
        return null;
    }

    public string GenerateJwtToken(int userId, string email)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado"))
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpiresInHours"] ?? "2")
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
