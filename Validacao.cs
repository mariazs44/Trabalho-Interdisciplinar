using System.Text.RegularExpressions;

namespace Ratatouille.Gestao;

public static class Validacao
{
    public static bool Email(string? texto) => texto != null && texto.Length <= 150 && Regex.IsMatch(texto, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
    public static void Texto(string? texto, int minimo, int maximo, string campo)
    {
        if (texto == null || texto.Trim().Length < minimo || texto.Length > maximo)
            throw new InvalidDataException($"{campo}: informe de {minimo} a {maximo} caracteres.");
    }
    public static bool Cpf(string? cpf)
    {
        if (cpf == null || !Regex.IsMatch(cpf, @"^\d{11}$") || cpf.Distinct().Count() == 1) return false;
        for (int t = 9; t < 11; t++) {
            int soma = 0; for (int i = 0; i < t; i++) soma += (cpf[i] - '0') * (t + 1 - i);
            if ((soma * 10 % 11) % 10 != cpf[t] - '0') return false;
        }
        return true;
    }
    public static void Cliente(ClienteJson? c)
    {
        if (c == null) throw new InvalidDataException("Cliente ausente.");
        Texto(c.Nome, 3, 120, "Nome");
        if (!Email(c.Email) || !Cpf(c.Cpf) || c.Telefone == null || !Regex.IsMatch(c.Telefone, @"^\d{10,11}$"))
            throw new InvalidDataException("Confira o e-mail, CPF e telefone do cliente.");
    }
    public static void Pedido(PedidoJson p)
    {
        if (p.Versao != 1 || p.PedidoId == null || !Regex.IsMatch(p.PedidoId, @"^RAT-[A-Za-z0-9-]{10,60}$"))
            throw new InvalidDataException("Versão ou código do pedido inválido.");
        if (p.CriadoEm == default || p.CriadoEm > DateTimeOffset.UtcNow.AddMinutes(10))
            throw new InvalidDataException("Data do pedido inválida.");
        Cliente(p.Cliente);
        var e = p.Endereco ?? throw new InvalidDataException("Endereço ausente.");
        Texto(e.Logradouro, 3, 150, "Logradouro"); Texto(e.Numero, 1, 10, "Número");
        Texto(e.Cidade, 1, 80, "Cidade"); Texto(e.Bairro, 0, 80, "Bairro");
        if (e.Cep == null || !Regex.IsMatch(e.Cep, @"^\d{8}$") || e.Uf == null ||
            !"AC AL AP AM BA CE DF ES GO MA MT MS MG PA PB PR PE PI RJ RN RS RO RR SC SP SE TO".Split(' ').Contains(e.Uf))
            throw new InvalidDataException("CEP ou UF inválidos.");
        if (p.Itens == null || p.Itens.Count == 0 || p.Itens.Count > 100 || p.Itens.Any(i => i == null))
            throw new InvalidDataException("O pedido deve ter de 1 a 100 itens.");
        decimal subtotal = 0;
        foreach (var i in p.Itens) {
            Texto(i.Nome, 1, 120, "Produto");
            if (i.ProdutoId < 1 || i.Quantidade < 1 || i.Quantidade > 99 || i.PrecoUnitario <= 0 || i.PrecoUnitario > 100000 ||
                decimal.Round(i.PrecoUnitario, 2) != i.PrecoUnitario) throw new InvalidDataException("Item inválido.");
            subtotal += i.PrecoUnitario * i.Quantidade;
        }
        if (p.Subtotal != subtotal || p.TaxaEntrega != 8m || p.Total != subtotal + p.TaxaEntrega || p.Total > 99999999.99m)
            throw new InvalidDataException("Subtotal, entrega ou total não conferem com os itens.");
    }
}
