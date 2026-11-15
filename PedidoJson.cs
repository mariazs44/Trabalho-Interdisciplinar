using System.Text.Json.Serialization;

namespace Ratatouille.Gestao;

// Os nomes abaixo correspondem exatamente ao JSON baixado pelo site.
public sealed class PedidoJson
{
    [JsonPropertyName("versao")] public int Versao { get; set; }
    [JsonPropertyName("pedidoId")] public string PedidoId { get; set; } = "";
    [JsonPropertyName("criadoEm")] public DateTimeOffset CriadoEm { get; set; }
    [JsonPropertyName("cliente")] public ClienteJson? Cliente { get; set; }
    [JsonPropertyName("endereco")] public EnderecoJson? Endereco { get; set; }
    [JsonPropertyName("itens")] public List<ItemJson>? Itens { get; set; }
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
    [JsonPropertyName("taxaEntrega")] public decimal TaxaEntrega { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
}
public sealed class ClienteJson
{
    [JsonPropertyName("nome")] public string Nome { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("telefone")] public string Telefone { get; set; } = "";
    [JsonPropertyName("cpf")] public string Cpf { get; set; } = "";
}
public sealed class EnderecoJson
{
    [JsonPropertyName("cep")] public string Cep { get; set; } = "";
    [JsonPropertyName("logradouro")] public string Logradouro { get; set; } = "";
    [JsonPropertyName("numero")] public string Numero { get; set; } = "";
    [JsonPropertyName("bairro")] public string Bairro { get; set; } = "";
    [JsonPropertyName("cidade")] public string Cidade { get; set; } = "";
    [JsonPropertyName("uf")] public string Uf { get; set; } = "";
}
public sealed class ItemJson
{
    [JsonPropertyName("produtoId")] public int ProdutoId { get; set; }
    [JsonPropertyName("nome")] public string Nome { get; set; } = "";
    [JsonPropertyName("quantidade")] public int Quantidade { get; set; }
    [JsonPropertyName("precoUnitario")] public decimal PrecoUnitario { get; set; }
}
