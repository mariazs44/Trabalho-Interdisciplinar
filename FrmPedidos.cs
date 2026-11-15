namespace Ratatouille.Gestao;

public sealed class FrmPedidos : Form
{
    private readonly DataGridView grade = Tema.Grade();
    private readonly ComboBox status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
    private static readonly string[] Estados = { "Pendente", "aguardando_pagamento", "confirmado", "em_preparo", "pronto", "saiu_para_entrega", "entregue", "cancelado" };
    public FrmPedidos()
    {
        Sessao.Exigir(); Tema.Aplicar(this, "Pedidos"); Size = new Size(1150, 650);
        status.Items.AddRange(Estados); status.SelectedIndex = 0;
        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, AutoScroll = true };
        barra.Controls.Add(Tema.Botao("Importar JSON", (_, _) => Tema.Tentar(Importar)));
        barra.Controls.Add(Tema.Botao("Atualizar", (_, _) => Tema.Tentar(Carregar)));
        barra.Controls.Add(status);
        barra.Controls.Add(Tema.Botao("Salvar status", (_, _) => Tema.Tentar(SalvarStatus)));
        barra.Controls.Add(Tema.Botao("Excluir cancelado", (_, _) => Tema.Tentar(Excluir)));
        barra.Controls.Add(Tema.Botao("Ver itens", (_, _) => Tema.Tentar(VerItens)));
        Controls.Add(grade); Controls.Add(barra);
        grade.SelectionChanged += (_, _) => { if (grade.CurrentRow != null) status.SelectedItem = Convert.ToString(grade.CurrentRow.Cells["status"].Value); };
        Shown += (_, _) => Tema.Tentar(Carregar);
    }
    private long Id() => grade.CurrentRow == null ? throw new InvalidOperationException("Selecione um pedido.") : Convert.ToInt64(grade.CurrentRow.Cells["id"].Value);
    private void Carregar() => grade.DataSource = Conexao.Consultar(
        "SELECT p.id,p.codigo_pedido,c.nome AS cliente,p.total,p.status,p.criado_em FROM pedidos p JOIN clientes c ON c.id=p.cliente_id ORDER BY p.id DESC");
    private void Importar()
    {
        using var selecionar = new OpenFileDialog { Filter = "Pedido JSON (*.json)|*.json", Multiselect = false };
        if (selecionar.ShowDialog(this) != DialogResult.OK) return;
        string codigo = ImportadorPedidos.Importar(selecionar.FileName); Carregar(); MessageBox.Show($"Pedido {codigo} importado com sucesso!");
    }
    private void SalvarStatus()
    {
        Sessao.Exigir(); long id = Id();
        if (status.SelectedItem is not string valor || !Estados.Contains(valor)) throw new InvalidOperationException("Selecione um status.");
        using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, "UPDATE pedidos SET status=@status WHERE id=@id", null, ("@status", valor), ("@id", id));
        cmd.ExecuteNonQuery(); Carregar();
    }
    private void Excluir()
    {
        Sessao.Exigir(); long id = Id();
        if (MessageBox.Show("Excluir definitivamente o pedido cancelado?", "Ratatouille", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var c = Conexao.Abrir(); using var tx = c.BeginTransaction();
        using (var verificar = Conexao.Comando(c, "SELECT status FROM pedidos WHERE id=@id FOR UPDATE", tx, ("@id", id))) {
            if (Convert.ToString(verificar.ExecuteScalar()) != "cancelado") throw new InvalidOperationException("Só é permitido excluir pedidos cancelados.");
        }
        // Respeita todas as FKs do banco original, incluindo dados de exemplo.
        foreach (string sql in new[] {
            "DELETE h FROM historico_entrega h JOIN entregas e ON e.id=h.entrega_id WHERE e.pedido_id=@id",
            "DELETE FROM entregas WHERE pedido_id=@id", "DELETE FROM pagamentos WHERE pedido_id=@id",
            "DELETE FROM pedidos WHERE id=@id AND status='cancelado'" }) {
            using var cmd = Conexao.Comando(c, sql, tx, ("@id", id)); cmd.ExecuteNonQuery();
        }
        tx.Commit(); Carregar();
    }
    private void VerItens()
    {
        long id = Id();
        // ID convertido para inteiro internamente; não provém de entrada SQL livre.
        using var janela = new Form { Width = 850, Height = 500 };
        Tema.Aplicar(janela, "Itens do pedido " + id); var itens = Tema.Grade(); janela.Controls.Add(itens);
        itens.DataSource = Conexao.Consultar("SELECT p.nome,v.nome AS variacao,i.quantidade,i.preco_unitario FROM itens_pedido i JOIN variacoes v ON v.id=i.variacao_id JOIN produtos p ON p.id=v.produto_id WHERE i.pedido_id=" + id);
        janela.ShowDialog(this);
    }
}
