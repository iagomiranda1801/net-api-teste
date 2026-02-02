using MinhaApi.Application.DTOs;
using MinhaApi.Application.Interfaces;
using MinhaApi.Domain.Entities;
using MinhaApi.Domain.Interfaces;

namespace MinhaApi.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(IUsuarioRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(int id)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        return usuario == null ? null : MapearParaDto(usuario);
    }

    public async Task<IEnumerable<UsuarioDto>> ObterTodosAsync()
    {
        var usuarios = await _repository.ObterTodosAsync();
        return usuarios.Select(MapearParaDto);
    }

    public async Task<UsuarioDto> CriarAsync(CriarUsuarioDto dto)
    {
        // Validação de negócio
        if (await _repository.EmailExisteAsync(dto.Email))
            throw new InvalidOperationException("Email já cadastrado");

        // Hash da senha antes de salvar
        var senhaHash = _passwordHasher.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, dto.Email, senhaHash);
        await _repository.AdicionarAsync(usuario);
        
        return MapearParaDto(usuario);
    }

    public async Task<UsuarioDto?> AtualizarAsync(int id, AtualizarUsuarioDto dto)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        if (usuario == null)
            return null;

        // Verifica se o email já existe em outro usuário
        var usuarioComEmail = await _repository.ObterPorEmailAsync(dto.Email);
        if (usuarioComEmail != null && usuarioComEmail.Id != id)
            throw new InvalidOperationException("Email já cadastrado");

        usuario.Atualizar(dto.Nome, dto.Email);
        await _repository.AtualizarAsync(usuario);
        
        return MapearParaDto(usuario);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var usuario = await _repository.ObterPorIdAsync(id);
        if (usuario == null)
            return false;

        await _repository.RemoverAsync(usuario);
        return true;
    }

    private static UsuarioDto MapearParaDto(Usuario usuario)
    {
        return new UsuarioDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.CriadoEm,
            usuario.AtualizadoEm
        );
    }
}
