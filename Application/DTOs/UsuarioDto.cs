namespace MinhaApi.Application.DTOs;

public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    DateTime CriadoEm,
    DateTime AtualizadoEm
);

public record CriarUsuarioDto(
    string Nome,
    string Email,
    string Senha
);

public record AtualizarUsuarioDto(
    string Nome,
    string Email
);

public record AlterarSenhaDto(
    string NovaSenha
);

// Resposta padrão da API com mensagem
public record ApiResponse<T>(
    bool Sucesso,
    string Mensagem,
    T? Dados
);
