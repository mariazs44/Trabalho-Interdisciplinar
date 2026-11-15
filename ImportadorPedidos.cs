using System.Text.Json;
using MySql.Data.MySqlClient;

namespace Ratatouille.Gestao;

public static class ImportadorPedidos
{
    // Todas as gravações pertencem à mesma transação: um erro desfaz tudo.
    public static string Importar(string caminho)
    {
        Sessao.Exigir();
        var arquivo = new FileInfo(caminho);
        if (!arquivo.Exists || arquivo.Length > 2 * 1024 * 1024)
            throw new InvalidDataException("Selecione um JSON de até 2 MB.");
        var pedido = JsonSerializer.Deserialize<PedidoJson>(File.ReadAllText(caminho),
            new JsonSerializerOptions { MaxDepth = 16 }) ?? throw new InvalidDataException("JSON vazio.");
        Validacao.Pedido(pedido);
        var cliente = pedido.Cliente!; var endereco = pedido.Endereco!;
        using var c = Conexao.Abrir(); using var tx = c.BeginTransaction();
        using (var duplicado = Conexao.Comando(c, "SELECT COUNT(*) FROM pedidos WHERE codigo_pedido=@codigo", tx, ("@codigo", pedido.PedidoId))) {
            if (Convert.ToInt32(duplicado.ExecuteScalar()) > 0) throw new InvalidDataException("Este pedido já foi importado.");
        }
        long clienteId;
        // Não confunde identidades se CPF e e-mail apontarem para pessoas diferentes.
        using (var consultar = Conexao.Comando(c,
            "SELECT id,email,cpf FROM clientes WHERE email=@email OR cpf=@cpf FOR UPDATE", tx,
            ("@email", cliente.Email), ("@cpf", cliente.Cpf))) {
            using var r = consultar.ExecuteReader(); clienteId = 0;
            while (r.Read()) {
                if (clienteId != 0 || !string.Equals(r.GetString("email"), cliente.Email, StringComparison.OrdinalIgnoreCase) ||
                    Convert.ToString(r["cpf"]) != cliente.Cpf)
                    throw new InvalidDataException("CPF ou e-mail já pertencem a outro cadastro. Revise os clientes.");
                clienteId = Convert.ToInt64(r["id"]);
            }
        }
        if (clienteId == 0) {
            using var inserir = Conexao.Comando(c,
                "INSERT INTO clientes(nome,email,telefone,cpf) VALUES(@nome,@email,@tel,@cpf)", tx,
                ("@nome", cliente.Nome), ("@email", cliente.Email.Trim().ToLowerInvariant()), ("@tel", cliente.Telefone), ("@cpf", cliente.Cpf));
            inserir.ExecuteNonQuery(); clienteId = inserir.LastInsertedId;
        }
        long enderecoId;
        using (var inserir = Conexao.Comando(c,
            "INSERT INTO enderecos(cliente_id,cep,logradouro,numero,bairro,cidade,uf,principal) " +
            "VALUES(@cliente,@cep,@rua,@numero,@bairro,@cidade,@uf,0)", tx,
            ("@cliente", clienteId), ("@cep", endereco.Cep), ("@rua", endereco.Logradouro), ("@numero", endereco.Numero),
            ("@bairro", endereco.Bairro), ("@cidade", endereco.Cidade), ("@uf", endereco.Uf))) {
            inserir.ExecuteNonQuery(); enderecoId = inserir.LastInsertedId;
        }
        long pedidoId;
        using (var inserir = Conexao.Comando(c,
            "INSERT INTO pedidos(codigo_pedido,cliente_id,endereco_id,status,subtotal,taxa_entrega,total,criado_em) " +
            "VALUES(@codigo,@cliente,@endereco,'Pendente',@subtotal,@taxa,@total,@data)", tx,
            ("@codigo", pedido.PedidoId), ("@cliente", clienteId), ("@endereco", enderecoId),
            ("@subtotal", pedido.Subtotal), ("@taxa", pedido.TaxaEntrega), ("@total", pedido.Total), ("@data", pedido.CriadoEm.UtcDateTime))) {
            inserir.ExecuteNonQuery(); pedidoId = inserir.LastInsertedId;
        }
        foreach (var item in pedido.Itens!) {
            long variacaoId;
            using (var buscar = Conexao.Comando(c,
                "SELECT v.id,p.nome,v.preco,p.disponivel FROM mapa_produtos_site m " +
                "JOIN variacoes v ON v.id=m.variacao_id JOIN produtos p ON p.id=v.produto_id WHERE m.site_id=@id FOR UPDATE", tx,
                ("@id", item.ProdutoId))) {
                using var r = buscar.ExecuteReader();
                if (!r.Read() || !Convert.ToBoolean(r["disponivel"]) || r.GetString("nome") != item.Nome)
                    throw new InvalidDataException($"Produto {item.ProdutoId} ausente, indisponível ou diferente do cardápio.");
                if (Convert.ToDecimal(r["preco"]) != item.PrecoUnitario)
                    throw new InvalidDataException($"O preço de {item.Nome} mudou. Atualize o preço no site antes de gerar um novo pedido.");
                variacaoId = Convert.ToInt64(r["id"]);
            }
            using var inserir = Conexao.Comando(c,
                "INSERT INTO itens_pedido(pedido_id,variacao_id,quantidade,preco_unitario) VALUES(@pedido,@variacao,@qtd,@preco)", tx,
                ("@pedido", pedidoId), ("@variacao", variacaoId), ("@qtd", item.Quantidade), ("@preco", item.PrecoUnitario));
            inserir.ExecuteNonQuery();
        }
        tx.Commit(); return pedido.PedidoId;
    }
}
