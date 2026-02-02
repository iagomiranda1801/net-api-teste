using Microsoft.AspNetCore.Mvc;
using MinhaApi.Application.DTOs;
using MinhaApi.Application.Interfaces;

namespace MinhaApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> ObterTodos()
    {
        var usuarios = await _service.ObterTodosAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> ObterPorId(int id)
    {
        var usuario = await _service.ObterPorIdAsync(id);
        
        if (usuario is null)
            return NotFound();
        
        return Ok(usuario);
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
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
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto)
    {
        try
        {
            var usuario = await _service.AtualizarAsync(id, dto);
            
            if (usuario is null)
                return NotFound();
            
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um usuário
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        var removido = await _service.RemoverAsync(id);
        
        if (!removido)
            return NotFound();
        
        return NoContent();
    }
}
