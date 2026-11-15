using System.Security.Cryptography;

namespace Ratatouille.Gestao;

public static class Autenticacao
{
    private const int Iteracoes = 210000;
    public static bool PrecisaGerente()
    {
        using var c = Conexao.Abrir();
        using var cmd = Conexao.Comando(c, "SELECT COUNT(*) FROM gerentes");
        return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
    }

    public static void CriarPrimeiro(string email, string senha)
    {
        email = email.Trim().ToLowerInvariant();
        if (!Validacao.Email(email) || senha.Length < 8 || senha.Length > 128)
            throw new InvalidDataException("Informe um e-mail válido e senha de 8 a 128 caracteres.");
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, 32);
        using var c = Conexao.Abrir();
        // ID fixo impede que duas configurações iniciais concorrentes criem gerentes.
        using var cmd = Conexao.Comando(c,
            "INSERT INTO gerentes (id,email,senha_hash,salt,iteracoes) " +
            "SELECT 1,@email,@hash,@salt,@iteracoes WHERE NOT EXISTS (SELECT 1 FROM gerentes)", null,
            ("@email", email), ("@hash", hash), ("@salt", salt), ("@iteracoes", Iteracoes));
        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("O gerente já foi configurado. Faça login.");
    }

    public static bool Entrar(string email, string senha)
    {
        Sessao.Sair();
        email = email.Trim().ToLowerInvariant();
        if (!Validacao.Email(email) || senha.Length > 128) return false;
        using var c = Conexao.Abrir();
        using var cmd = Conexao.Comando(c,
            "SELECT senha_hash,salt,iteracoes FROM gerentes WHERE email=@email AND ativo=1", null, ("@email", email));
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return false;
        var esperado = (byte[])r["senha_hash"];
        var obtido = Rfc2898DeriveBytes.Pbkdf2(senha, (byte[])r["salt"], Convert.ToInt32(r["iteracoes"]), HashAlgorithmName.SHA256, 32);
        if (!CryptographicOperations.FixedTimeEquals(esperado, obtido)) return false;
        Sessao.Entrar(email); return true;
    }
}
