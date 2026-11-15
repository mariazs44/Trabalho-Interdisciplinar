namespace Ratatouille.Gestao;

public sealed class FrmLogin : Form
{
    private readonly TextBox email = new() { Width = 360, MaxLength = 150 };
    private readonly TextBox senha = new() { Width = 360, MaxLength = 128, UseSystemPasswordChar = true };
    private readonly TextBox confirmar = new() { Width = 360, MaxLength = 128, UseSystemPasswordChar = true };
    private readonly Label aviso = new() { Text = "Login exclusivo do gerente", AutoSize = true, MaximumSize = new Size(460, 0) };
    private readonly Label confirmarLabel = new() { Text = "Confirmar senha", AutoSize = true, Visible = false };
    private readonly Button criar;
    private readonly Button entrar;
    public FrmLogin()
    {
        Tema.Aplicar(this, "Entrar"); Size = new Size(600, 500);
        var painel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, Padding = new Padding(70, 30, 30, 30), AutoScroll = true };
        Controls.Add(painel);
        painel.Controls.Add(new Label { Text = "Ratatouille", AutoSize = true, Font = new Font("Georgia", 28), ForeColor = Tema.Ouro });
        painel.Controls.Add(aviso);
        painel.Controls.Add(new Label { Text = "E-mail", AutoSize = true }); painel.Controls.Add(email);
        painel.Controls.Add(new Label { Text = "Senha", AutoSize = true }); painel.Controls.Add(senha);
        confirmar.Visible = false; painel.Controls.Add(confirmarLabel); painel.Controls.Add(confirmar);
        entrar = Tema.Botao("Entrar", (_, _) => Tema.Tentar(Login));
        criar = Tema.Botao("Configurar primeiro gerente", (_, _) => Tema.Tentar(Criar)); criar.Visible = false;
        painel.Controls.Add(entrar); painel.Controls.Add(criar);
        painel.Controls.Add(Tema.Botao("Verificar conexão", (_, _) => Verificar()));
        AcceptButton = entrar; Shown += (_, _) => Verificar();
    }
    private void Verificar() => Tema.Tentar(() => {
        bool primeiro = Autenticacao.PrecisaGerente();
        criar.Visible = confirmar.Visible = confirmarLabel.Visible = primeiro; entrar.Enabled = !primeiro;
        aviso.Text = primeiro ? "Configure o primeiro gerente. A senha deve ter pelo menos 8 caracteres." : "Login exclusivo do gerente";
        AcceptButton = primeiro ? criar : entrar;
    });
    private void Criar()
    {
        if (senha.Text != confirmar.Text) throw new InvalidDataException("As senhas não conferem.");
        Autenticacao.CriarPrimeiro(email.Text, senha.Text); senha.Clear(); confirmar.Clear(); Verificar();
        MessageBox.Show("Gerente configurado. Faça login.");
    }
    private void Login()
    {
        if (!Autenticacao.Entrar(email.Text, senha.Text)) { senha.Clear(); throw new InvalidDataException("E-mail ou senha incorretos."); }
        senha.Clear(); Hide();
        try { using var principal = new FrmPrincipal(); principal.ShowDialog(this); }
        finally { Sessao.Sair(); Show(); }
    }
}
