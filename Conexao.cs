using System.Data;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace Ratatouille.Gestao;

public sealed class Configuracao
{
    public string Servidor { get; set; } = "localhost";
    public uint Porta { get; set; } = 3306;
    public string Banco { get; set; } = "delivery_italiano";
    public string Usuario { get; set; } = "root";
    public string Senha { get; set; } = "";
}

// A mesma conexão é usada pelo login, pela importação e pelas telas de gestão.
public static class Conexao
{
    public static MySqlConnection Abrir()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "conexao.json");
        var config = JsonSerializer.Deserialize<Configuracao>(File.ReadAllText(caminho))
            ?? throw new InvalidDataException("Configure o arquivo conexao.json.");
        var cs = new MySqlConnectionStringBuilder {
            Server = config.Servidor, Port = config.Porta, Database = config.Banco,
            UserID = config.Usuario, Password = config.Senha, CharacterSet = "utf8mb4",
            ConnectionTimeout = 5, DefaultCommandTimeout = 15, SslMode = MySqlSslMode.Preferred
        };
        var conexao = new MySqlConnection(cs.ConnectionString);
        try { conexao.Open(); return conexao; }
        catch { conexao.Dispose(); throw; }
    }

    public static MySqlCommand Comando(MySqlConnection c, string sql,
        MySqlTransaction? transacao = null, params (string Nome, object? Valor)[] parametros)
    {
        var cmd = new MySqlCommand(sql, c, transacao);
        foreach (var p in parametros) cmd.Parameters.AddWithValue(p.Nome, p.Valor ?? DBNull.Value);
        return cmd;
    }

    public static DataTable Consultar(string sql)
    {
        Sessao.Exigir();
        using var c = Abrir(); using var cmd = new MySqlCommand(sql, c);
        using var adapter = new MySqlDataAdapter(cmd); var dados = new DataTable();
        adapter.Fill(dados); return dados;
    }
}

public static class Sessao
{
    public static string? Email { get; private set; }
    internal static void Entrar(string email) => Email = email;
    internal static void Sair() => Email = null;
    public static void Exigir()
    {
        if (Email == null) throw new UnauthorizedAccessException("Entre como gerente para continuar.");
    }
}
