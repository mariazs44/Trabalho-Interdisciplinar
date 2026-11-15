using System.Text.RegularExpressions;

namespace Ratatouille.Gestao;

// Dois CRUDs completos: clientes e entregadores, sem duplicação das telas comuns.
public sealed class FrmCadastro : Form
{
    private readonly bool entregadores;
    private readonly DataGridView grade = Tema.Grade();
    private readonly TextBox nome = new() { Width = 260, MaxLength = 120 };
    private readonly TextBox contato = new() { Width = 230, MaxLength = 150 };
    private readonly TextBox telefone = new() { Width = 170, MaxLength = 20 };
    private readonly TextBox documento = new() { Width = 170, MaxLength = 14 };
    private readonly ComboBox veiculo = new() { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox ativo = new() { Text = "Ativo", Checked = true, AutoSize = true };
    private long? selecionado;
    private string Tabela => entregadores ? "entregadores" : "clientes";
    public FrmCadastro(bool usarEntregadores)
    {
        Sessao.Exigir(); entregadores = usarEntregadores;
        Tema.Aplicar(this, entregadores ? "Entregadores" : "Clientes"); Size = new Size(1100, 650);
        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 185, AutoScroll = true };
        void Campo(string titulo, Control campo) {
            var painel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Width = campo.Width + 12, Height = 68 };
            painel.Controls.Add(new Label { Text = titulo, AutoSize = true }); painel.Controls.Add(campo); barra.Controls.Add(painel);
        }
        Campo("Nome", nome); Campo("Telefone", telefone);
        if (entregadores) {
            veiculo.Items.AddRange(new[] { "moto", "bicicleta", "carro" }); veiculo.SelectedIndex = 0;
            Campo("Veículo", veiculo); Campo("Placa (opcional)", documento); barra.Controls.Add(ativo);
        } else { Campo("E-mail", contato); Campo("CPF", documento); }
        barra.Controls.Add(Tema.Botao("Novo", (_, _) => Limpar()));
        barra.Controls.Add(Tema.Botao("Salvar", (_, _) => Tema.Tentar(Salvar)));
        barra.Controls.Add(Tema.Botao("Excluir", (_, _) => Tema.Tentar(Excluir)));
        barra.Controls.Add(Tema.Botao("Atualizar", (_, _) => Tema.Tentar(Carregar)));
        Controls.Add(grade); Controls.Add(barra); grade.SelectionChanged += (_, _) => Selecionar();
        Shown += (_, _) => Tema.Tentar(Carregar);
    }
    private void Carregar()
    {
        grade.DataSource = Conexao.Consultar(entregadores ?
            "SELECT id,nome,telefone,veiculo,placa,ativo FROM entregadores ORDER BY nome" :
            "SELECT id,nome,email,telefone,cpf FROM clientes ORDER BY nome");
        Limpar();
    }
    private void Limpar()
    {
        selecionado = null; nome.Clear(); telefone.Clear(); contato.Clear(); documento.Clear(); ativo.Checked = true;
        if (entregadores) veiculo.SelectedIndex = 0;
        grade.ClearSelection();
    }
    private void Selecionar()
    {
        if (grade.SelectedRows.Count == 0) return;
        var r = grade.SelectedRows[0]; selecionado = Convert.ToInt64(r.Cells["id"].Value);
        string Valor(string coluna) => Convert.ToString(r.Cells[coluna].Value) ?? "";
        nome.Text = Valor("nome"); telefone.Text = Valor("telefone");
        if (entregadores) { documento.Text = Valor("placa"); veiculo.SelectedItem = Valor("veiculo"); ativo.Checked = Convert.ToBoolean(r.Cells["ativo"].Value); }
        else { contato.Text = Valor("email"); documento.Text = Valor("cpf"); }
    }
    private void Salvar()
    {
        Sessao.Exigir(); Validacao.Texto(nome.Text, 3, 120, "Nome");
        string tel = Regex.Replace(telefone.Text, @"\D", ""), doc = Regex.Replace(documento.Text, @"\D", "");
        if (!Regex.IsMatch(tel, @"^\d{10,11}$")) throw new InvalidDataException("Informe um telefone com DDD.");
        string sql;
        if (entregadores) {
            if (veiculo.SelectedItem == null) throw new InvalidDataException("Escolha o veículo.");
            string placa = documento.Text.Replace("-", "").Trim().ToUpperInvariant();
            if (placa.Length > 0 && !Regex.IsMatch(placa, @"^[A-Z]{3}\d[A-Z0-9]\d{2}$")) throw new InvalidDataException("Placa inválida.");
            sql = selecionado == null ? "INSERT INTO entregadores(nome,telefone,veiculo,placa,ativo) VALUES(@nome,@tel,@veiculo,@placa,@ativo)" :
                "UPDATE entregadores SET nome=@nome,telefone=@tel,veiculo=@veiculo,placa=@placa,ativo=@ativo WHERE id=@id";
            using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, sql, null,
                ("@nome", nome.Text.Trim()), ("@tel", tel), ("@veiculo", veiculo.SelectedItem), ("@placa", placa.Length == 0 ? null : placa), ("@ativo", ativo.Checked), ("@id", selecionado));
            cmd.ExecuteNonQuery();
        } else {
            var cliente = new ClienteJson { Nome = nome.Text.Trim(), Email = contato.Text.Trim().ToLowerInvariant(), Telefone = tel, Cpf = doc };
            Validacao.Cliente(cliente);
            sql = selecionado == null ? "INSERT INTO clientes(nome,email,telefone,cpf) VALUES(@nome,@email,@tel,@cpf)" :
                "UPDATE clientes SET nome=@nome,email=@email,telefone=@tel,cpf=@cpf WHERE id=@id";
            using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, sql, null,
                ("@nome", cliente.Nome), ("@email", cliente.Email), ("@tel", tel), ("@cpf", doc), ("@id", selecionado)); cmd.ExecuteNonQuery();
        }
        Carregar();
    }
    private void Excluir()
    {
        Sessao.Exigir(); if (selecionado == null) throw new InvalidOperationException("Selecione um cadastro.");
        if (MessageBox.Show("Excluir o cadastro selecionado?", "Ratatouille", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        // Tabela é escolhida somente pelo construtor; os valores são parametrizados.
        using var c = Conexao.Abrir(); using var cmd = Conexao.Comando(c, "DELETE FROM " + Tabela + " WHERE id=@id", null, ("@id", selecionado));
        cmd.ExecuteNonQuery(); Carregar();
    }
}
