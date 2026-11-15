namespace Ratatouille.Gestao;

internal static class Tema
{
    internal static readonly Color Fundo = Color.FromArgb(43, 15, 10);
    internal static readonly Color Ouro = Color.FromArgb(217, 184, 120);
    internal static void Aplicar(Form form, string titulo)
    {
        form.Text = "Ratatouille — " + titulo; form.BackColor = Fundo; form.ForeColor = Color.WhiteSmoke;
        form.Font = new Font("Segoe UI", 10); form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = new Size(600, 480);
    }
    internal static Button Botao(string texto, EventHandler acao)
    {
        var b = new Button { Text = texto, AutoSize = true, Height = 38, BackColor = Ouro,
            ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Margin = new Padding(6) };
        b.Click += acao; return b;
    }
    internal static void Tentar(Action acao)
    {
        try { acao(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ratatouille", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    internal static DataGridView Grade() => new() {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Fundo,
        DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.Black, BackColor = Color.WhiteSmoke }
    };
}
