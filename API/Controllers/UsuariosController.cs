using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MinhaApi.Application.DTOs;
using MinhaApi.Application.Interfaces;

namespace MinhaApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todos os endpoints deste controller
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtém todos os usuários
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UsuarioDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> ObterTodos()
    {
        var usuarios = await _service.ObterTodosAsync();
        var response = new ApiResponse<IEnumerable<UsuarioDto>>(
            Sucesso: true,
            Mensagem: "Usuários obtidos com sucesso",
            Dados: usuarios
        );
        return Ok(response);
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObterPorId(int id)
    {
        var usuario = await _service.ObterPorIdAsync(id);
        
        if (usuario is null)
        {
            var errorResponse = new ApiResponse<UsuarioDto>(
                Sucesso: false,
                Mensagem: "Usuário não encontrado",
                Dados: null
            );
            return NotFound(errorResponse);
        }
        
        var response = new ApiResponse<UsuarioDto>(
            Sucesso: true,
            Mensagem: "Usuário obtido com sucesso",
            Dados: usuario
        );
        return Ok(response);
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // Permite criar usuário sem autenticação (registro)
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Criar([FromBody] CriarUsuarioDto dto)
    {
        try
        {
            var usuario = await _service.CriarAsync(dto);
            var response = new ApiResponse<UsuarioDto>(
                Sucesso: true,
                Mensagem: "Usuário criado com sucesso",
                Dados: usuario
            );
            return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            var errorResponse = new ApiResponse<UsuarioDto>(
                Sucesso: false,
                Mensagem: ex.Message,
                Dados: null
            );
            return BadRequest(errorResponse);
        }
        catch (ArgumentException ex)
        {
            var errorResponse = new ApiResponse<UsuarioDto>(
                Sucesso: false,
                Mensagem: ex.Message,
                Dados: null
            );
            return BadRequest(errorResponse);
        }
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto)
    {
        try
        {
            var usuario = await _service.AtualizarAsync(id, dto);
            
            if (usuario is null)
            {
                var errorResponse = new ApiResponse<UsuarioDto>(
                    Sucesso: false,
                    Mensagem: "Usuário não encontrado",
                    Dados: null
                );
                return NotFound(errorResponse);
            }
            
            var response = new ApiResponse<UsuarioDto>(
                Sucesso: true,
                Mensagem: "Usuário atualizado com sucesso",
                Dados: usuario
            );
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            var errorResponse = new ApiResponse<UsuarioDto>(
                Sucesso: false,
                Mensagem: ex.Message,
                Dados: null
            );
            return BadRequest(errorResponse);
        }
        catch (ArgumentException ex)
        {
            var errorResponse = new ApiResponse<UsuarioDto>(
                Sucesso: false,
                Mensagem: ex.Message,
                Dados: null
            );
            return BadRequest(errorResponse);
        }
    }

    /// <summary>
    /// Remove um usuário
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Remover(int id)
    {
        var removido = await _service.RemoverAsync(id);
        
        if (!removido)
        {
            var errorResponse = new ApiResponse<object>(
                Sucesso: false,
                Mensagem: "Usuário não encontrado",
                Dados: null
            );
            return NotFound(errorResponse);
        }
        
        var response = new ApiResponse<object>(
            Sucesso: true,
            Mensagem: "Usuário removido com sucesso",
            Dados: null
        );
        return Ok(response);
    }
}
