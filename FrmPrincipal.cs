namespace Ratatouille.Gestao;

public sealed class FrmPrincipal : Form
{
    public FrmPrincipal()
    {
        Sessao.Exigir(); Tema.Aplicar(this, "Gestão"); Size = new Size(700, 500);
        var menu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(30) };
        Controls.Add(menu);
        menu.Controls.Add(new Label { Text = "Ratatouille — Cucina Italiana", Font = new Font("Georgia", 22), AutoSize = true, ForeColor = Tema.Ouro });
        menu.Controls.Add(new Label { Text = "Gerente: " + Sessao.Email, AutoSize = true });
        menu.Controls.Add(Tema.Botao("Pedidos e importação JSON", (_, _) => Tema.Tentar(() => { using var f = new FrmPedidos(); f.ShowDialog(this); })));
        menu.Controls.Add(Tema.Botao("Produtos e preços", (_, _) => Tema.Tentar(() => { using var f = new FrmProdutos(); f.ShowDialog(this); })));
        menu.Controls.Add(Tema.Botao("Clientes", (_, _) => Tema.Tentar(() => { using var f = new FrmCadastro(false); f.ShowDialog(this); })));
        menu.Controls.Add(Tema.Botao("Entregadores", (_, _) => Tema.Tentar(() => { using var f = new FrmCadastro(true); f.ShowDialog(this); })));
        menu.Controls.Add(Tema.Botao("Sair", (_, _) => Close()));
    }
}
