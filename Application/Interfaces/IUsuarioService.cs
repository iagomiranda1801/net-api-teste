using MinhaApi.Application.DTOs;

namespace MinhaApi.Application.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<UsuarioDto>> ObterTodosAsync();
    Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto);
    Task<UsuarioDto?> AtualizarAsync(int id, AtualizarUsuarioDto dto);
    Task<bool> RemoverAsync(int id);
}
