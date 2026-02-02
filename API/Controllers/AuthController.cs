using Microsoft.AspNetCore.Mvc;
using MinhaApi.Application.DTOs;
using MinhaApi.Application.Interfaces;

namespace MinhaApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza login e retorna token JWT
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);

        if (resultado == null)
        {
            var errorResponse = new ApiResponse<LoginResponseDto>(
                Sucesso: false,
                Mensagem: "Email ou senha inválidos",
                Dados: null
            );
            return Unauthorized(errorResponse);
        }

        var response = new ApiResponse<LoginResponseDto>(
            Sucesso: true,
            Mensagem: "Login realizado com sucesso",
            Dados: resultado
        );
        return Ok(response);
    }

    /// <summary>
    /// Renova o token JWT usando refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var resultado = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (resultado == null)
        {
            var errorResponse = new ApiResponse<LoginResponseDto>(
                Sucesso: false,
                Mensagem: "Refresh token inválido ou expirado",
                Dados: null
            );
            return Unauthorized(errorResponse);
        }

        var response = new ApiResponse<LoginResponseDto>(
            Sucesso: true,
            Mensagem: "Token renovado com sucesso",
            Dados: resultado
        );
        return Ok(response);
    }
}
