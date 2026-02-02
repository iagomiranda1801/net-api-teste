namespace MinhaApi.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Senha { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    // Construtor para o EF Core
    protected Usuario() { }

    public Usuario(string nome, string email, string senha)
    {
        ValidarDados(nome, email, senha);
        
        Nome = nome;
        Email = email;
        Senha = senha;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        Nome = nome;
        Email = email;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AlterarSenha(string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Senha não pode ser vazia", nameof(novaSenha));

        Senha = novaSenha;
        AtualizadoEm = DateTime.UtcNow;
    }

    private void ValidarDados(string nome, string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));
        
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia", nameof(senha));
    }
}
