namespace Ratatouille.Gestao;

public sealed class FrmProdutos : Form
{
    private readonly DataGridView grade = Tema.Grade();
    private readonly NumericUpDown preco = new() { DecimalPlaces = 2, Minimum = 0.01m, Maximum = 100000, Width = 130 };
    public FrmProdutos()
    {
        Sessao.Exigir(); Tema.Aplicar(this, "Produtos"); Size = new Size(1050, 620);
        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 90 };
        barra.Controls.Add(new Label { Text = "Preço (R$)", AutoSize = true }); barra.Controls.Add(preco);
        barra.Controls.Add(Tema.Botao("Salvar preço", (_, _) => Tema.Tentar(Salvar)));
        barra.Controls.Add(Tema.Botao("Excluir produto", (_, _) => Tema.Tentar(Excluir)));
        barra.Controls.Add(Tema.Botao("Atualizar", (_, _) => Tema.Tentar(Carregar)));
        Controls.Add(grade); Controls.Add(barra);
        grade.SelectionChanged += (_, _) => {
            if (grade.CurrentRow != null) preco.Value = Math.Clamp(Convert.ToDecimal(grade.CurrentRow.Cells["preco"].Value), preco.Minimum, preco.Maximum);
        };
        Shown += (_, _) => Tema.Tentar(Carregar);
    }
    private void Carregar() => grade.DataSource = Conexao.Consultar(
        "SELECT p.id,v.id AS variacao_id,p.nome,c.nome AS categoria,v.nome AS porcao,v.preco,p.disponivel " +
        "FROM produtos p JOIN categorias c ON c.id=p.categoria_id JOIN variacoes v ON v.produto_id=p.id ORDER BY p.nome,v.id");
    private long Id(string coluna) => grade.CurrentRow == null ? throw new InvalidOperationException("Selecione um produto.") : Convert.ToInt64(grade.CurrentRow.Cells[coluna].Value);
    private void Salvar()
    {
        Sessao.Exigir(); long id = Id("variacao_id");
        using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, "UPDATE variacoes SET preco=@preco WHERE id=@id", null, ("@preco", preco.Value), ("@id", id));
        cmd.ExecuteNonQuery(); Carregar();
        MessageBox.Show("Preço salvo. Atualize também o preço correspondente em PRATOS, no script.js do site, antes de gerar novos pedidos.");
    }
    private void Excluir()
    {
        Sessao.Exigir(); long id = Id("id");
        if (MessageBox.Show("Excluir o produto selecionado? Produtos utilizados em pedidos são protegidos pelo banco.", "Ratatouille", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, "DELETE FROM produtos WHERE id=@id", null, ("@id", id));
        cmd.ExecuteNonQuery(); Carregar();
    }
    // Não há botão nem método de cadastro: produtos são incluídos somente via SQL.
}
