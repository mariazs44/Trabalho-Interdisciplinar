var PRATOS = [
    { id: 1, cat: 'Massas', nome: 'Spaghetti al Pomodoro Fresco e Basilico', desc: 'Tomate italiano, azeite extravirgem, manjericão e massa trefilada em bronze.', preco: 42.90, img: '1_gdvuMWmCJJRhi_XfGcu8-Q.jpg' },
    { id: 2, cat: 'Massas', nome: 'Fettuccine Alfredo', desc: 'Massa fresca com molho cremoso de manteiga, filé de frango e parmesão.', preco: 49.90, img: 'picture.webp' },
    { id: 3, cat: 'Massas', nome: 'Ravioli ai Funghi', desc: 'Ravioli recheado, cogumelos salteados e lascas de parmesão.', preco: 54.90, img: 'post.webp' },
    { id: 4, cat: 'Pizzas', nome: 'Pizza Margherita con Burrata e Pomodorini', desc: 'Massa de longa fermentação, muçarela de búfala, tomate e folhas de manjericão.', preco: 59.90, img: 'pizza-5275191_1920.webp' },
    { id: 5, cat: 'Pizzas', nome: 'Pizza Calabresa', desc: 'Molho de tomate, muçarela, calabresa fatiada e cebola.', preco: 52.90, img: 'calabresa.png' },
    { id: 6, cat: 'Pizzas', nome: 'Pizza Quattro Formaggi', desc: 'Muçarela, gorgonzola, parmesão e provolone.', preco: 64.90, img: 'queijo.jpg' },
    { id: 7, cat: 'Sobremesas', nome: 'Cannoli Siciliani Tradizionali', desc: 'Massa artesanal crocante com recheio cremoso de ricota doce e gotas de chocolate.', preco: 24.90, img: 'receitas-de-doces-italianos-1-730x449.webp' },
    { id: 8, cat: 'Sobremesas', nome: 'Tiramisù', desc: 'Camadas de biscoito champagne, café, creme de mascarpone e cacau.', preco: 26.90, img: 'tiramissu.webp' },
    { id: 9, cat: 'Sobremesas', nome: 'Panna Cotta', desc: 'Creme gelado de baunilha com calda de frutas vermelhas.', preco: 22.90, img: 'Panna-cotta.jpg' }
];
var CATEGORIAS = ['Todos', 'Massas', 'Pizzas', 'Sobremesas'];
var TAXA_ENTREGA = 8;

var catAtual = 'Todos';
var termo = '';

function ler(chave, padrao) {
    try { return JSON.parse(localStorage.getItem(chave)) || padrao; }
    catch (e) { return padrao; }
}
function gravar(chave, valor) { localStorage.setItem(chave, JSON.stringify(valor)); }
function moeda(v) { return 'R$ ' + v.toFixed(2).replace('.', ','); }
function el(id) { return document.getElementById(id); }
function esc(s) {
    return String(s).replace(/[&<>"']/g, function (c) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
}
function semAcento(s) { return s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase(); }
function achar(id) {
    for (var i = 0; i < PRATOS.length; i++) if (PRATOS[i].id === id) return PRATOS[i];
}
function mensagem(id, texto, ok) {
    var m = el(id);
    m.textContent = texto;
    m.className = 'msg' + (ok ? ' ok' : '');
}
function aviso(texto) {
    var t = document.createElement('div');
    t.className = 'toast';
    t.textContent = texto;
    document.body.appendChild(t);
    setTimeout(function () { t.remove(); }, 2200);
}

function go(v, msg) {
    if ((v === 'pedidos' || v === 'perfil') && !usuario()) {
        msg = 'Faça login para acessar essa página.';
        v = 'login';
    }
    var views = document.querySelectorAll('.view');
    for (var i = 0; i < views.length; i++) views[i].classList.add('oculto');
    el('v-' + v).classList.remove('oculto');

    var links = document.querySelectorAll('#links a');
    for (var j = 0; j < links.length; j++) {
        links[j].classList.toggle('on', links[j].getAttribute('data-v') === v);
    }

    if (v === 'login') mensagem('login-msg', msg || '');
    if (v === 'produtos') renderProdutos();
    if (v === 'carrinho') renderCarrinho();
    if (v === 'pedidos') renderPedidos();
    if (v === 'perfil') renderPerfil();
    window.scrollTo(0, 0);
}

function busca(valor) {
    termo = semAcento(valor.trim());
    if (el('v-produtos').classList.contains('oculto')) go('produtos');
    else renderProdutos();
}

function renderProdutos() {
    var f = '';
    for (var i = 0; i < CATEGORIAS.length; i++) {
        f += '<button class="filtro' + (CATEGORIAS[i] === catAtual ? ' on' : '') +
            '" onclick="filtrar(\'' + CATEGORIAS[i] + '\')">' + CATEGORIAS[i] + '</button>';
    }
    el('filtros').innerHTML = f;

    var html = '', achados = 0;
    for (var k = 0; k < PRATOS.length; k++) {
        var p = PRATOS[k];
        if (catAtual !== 'Todos' && p.cat !== catAtual) continue;
        if (termo && semAcento(p.nome + ' ' + p.desc).indexOf(termo) === -1) continue;
        achados++;
        html += '<div class="card-prato">' +
            '<div class="container-icone"><img src="' + p.img + '" alt="' + esc(p.nome) + '"></div>' +
            '<h3>' + esc(p.nome) + '</h3>' +
            '<p>' + esc(p.desc) + '</p>' +
            '<span class="preco">' + moeda(p.preco) + '</span>' +
            '<button class="btnfill" onclick="adicionar(' + p.id + ')">Adicionar</button>' +
            '</div>';
    }
    el('lista-produtos').innerHTML = html;
    el('sem-resultado').classList.toggle('oculto', achados > 0);
}
function filtrar(cat) { catAtual = cat; renderProdutos(); }

function carrinho() { return ler('rat_carrinho', []); }

function atualizaBadge() {
    var total = 0, c = carrinho();
    for (var i = 0; i < c.length; i++) total += c[i].q;
    el('quantidade').textContent = total;
}
function adicionar(id) {
    var c = carrinho(), achou = false;
    for (var i = 0; i < c.length; i++) {
        if (c[i].id === id) { c[i].q++; achou = true; }
    }
    if (!achou) c.push({ id: id, q: 1 });
    gravar('rat_carrinho', c);
    atualizaBadge();
    aviso('Adicionado ao carrinho');
}
function mudarQtd(id, delta) {
    var c = carrinho();
    for (var i = 0; i < c.length; i++) {
        if (c[i].id === id) c[i].q += delta;
    }
    c = c.filter(function (x) { return x.q > 0; });
    gravar('rat_carrinho', c);
    atualizaBadge();
    renderCarrinho();
}
function remover(id) {
    gravar('rat_carrinho', carrinho().filter(function (x) { return x.id !== id; }));
    atualizaBadge();
    renderCarrinho();
}
function esvaziar() {
    gravar('rat_carrinho', []);
    atualizaBadge();
    renderCarrinho();
}
function renderCarrinho() {
    var c = carrinho(), html = '', sub = 0;
    if (!c.length) {
        el('itens-carrinho').innerHTML = '<div class="vazio">Seu carrinho está vazio.<br>' +
            '<button class="btnfill" onclick="go(\'produtos\')">Ver cardápio</button></div>';
        el('r-subtotal').textContent = moeda(0);
        el('r-entrega').textContent = moeda(0);
        el('r-total').textContent = moeda(0);
        return;
    }
    for (var i = 0; i < c.length; i++) {
        var p = achar(c[i].id), linha = p.preco * c[i].q;
        sub += linha;
        html += '<div class="item-carrinho">' +
            '<img src="' + p.img + '" alt="' + esc(p.nome) + '">' +
            '<div><h4>' + esc(p.nome) + '</h4><small>' + moeda(p.preco) + ' cada</small>' +
            '<div class="qtd"><button onclick="mudarQtd(' + p.id + ',-1)">−</button><span>' + c[i].q +
            '</span><button onclick="mudarQtd(' + p.id + ',1)">+</button></div></div>' +
            '<div class="item-direita"><span>' + moeda(linha) + '</span>' +
            '<button class="remover" onclick="remover(' + p.id + ')">Remover</button></div>' +
            '</div>';
    }
    el('itens-carrinho').innerHTML = html;
    el('r-subtotal').textContent = moeda(sub);
    el('r-entrega').textContent = moeda(TAXA_ENTREGA);
    el('r-total').textContent = moeda(sub + TAXA_ENTREGA);
}
function finalizar() {
    var c = carrinho();
    if (!c.length) { aviso('Seu carrinho está vazio'); return; }
    var u = usuario();
    if (!u) { go('login', 'Faça login para finalizar o pedido.'); return; }

    var itens = [], sub = 0;
    for (var i = 0; i < c.length; i++) {
        var p = achar(c[i].id);
        itens.push({ nome: p.nome, q: c[i].q, preco: p.preco });
        sub += p.preco * c[i].q;
    }
    var pedidos = ler('rat_pedidos', []);
    pedidos.push({
        num: String(Date.now()).slice(-6),
        email: u.email,
        data: new Date().toLocaleString('pt-BR'),
        itens: itens,
        total: sub + TAXA_ENTREGA,
        endereco: u.end + ', ' + u.num + ' - CEP ' + u.cep,
        status: 'Em preparo'
    });
    gravar('rat_pedidos', pedidos);
    gravar('rat_carrinho', []);
    atualizaBadge();
    aviso('Pedido realizado com sucesso!');
    go('pedidos');
}

function renderPedidos() {
    var u = usuario();
    if (!u) return;
    var lista = ler('rat_pedidos', []).filter(function (p) { return p.email === u.email; }).reverse();
    if (!lista.length) {
        el('lista-pedidos').innerHTML = '<div class="vazio">Você ainda não fez nenhum pedido.<br>' +
            '<button class="btnfill" onclick="go(\'produtos\')">Fazer pedido</button></div>';
        return;
    }
    var html = '';
    for (var i = 0; i < lista.length; i++) {
        var p = lista[i], itens = '';
        for (var j = 0; j < p.itens.length; j++) {
            itens += '<li><span>' + p.itens[j].q + 'x ' + esc(p.itens[j].nome) + '</span><span>' +
                moeda(p.itens[j].preco * p.itens[j].q) + '</span></li>';
        }
        html += '<div class="pedido">' +
            '<div class="pedido-topo"><strong>Pedido #' + esc(p.num) + '</strong><span class="status">' + esc(p.status) + '</span></div>' +
            '<small>' + p.data + '</small><ul>' + itens + '</ul>' +
            '<small>Entrega: ' + esc(p.endereco) + '</small>' +
            '<div class="total-pedido">Total: ' + moeda(p.total) + '</div></div>';
    }
    el('lista-pedidos').innerHTML = html;
}

function mascara(campo) {
    var tipo = campo.getAttribute('data-mask');
    var v = campo.value, d = v.replace(/\D/g, '');
    if (tipo === 'cpf') {
        d = d.slice(0, 11);
        v = d.replace(/^(\d{3})(\d{0,3})(\d{0,3})(\d{0,2}).*/, function (m, a, b, c, e) {
            return a + (b ? '.' + b : '') + (c ? '.' + c : '') + (e ? '-' + e : '');
        });
    } else if (tipo === 'tel') {
        d = d.slice(0, 11);
        if (d.length > 2) {
            v = '(' + d.slice(0, 2) + ') ' +
                (d.length > 10 ? d.slice(2, 7) + '-' + d.slice(7) : d.slice(2, 6) + (d.length > 6 ? '-' + d.slice(6) : ''));
        } else {
            v = d.length ? '(' + d : '';
        }
    } else if (tipo === 'cep') {
        d = d.slice(0, 8);
        v = d.length > 5 ? d.slice(0, 5) + '-' + d.slice(5) : d;
    } else if (tipo === 'num') {
        v = d;
    } else if (tipo === 'nome') {
        v = v.replace(/[^A-Za-zÀ-ÿ' ]/g, '');
    }
    campo.value = v;
}
document.addEventListener('input', function (e) {
    if (e.target.getAttribute && e.target.getAttribute('data-mask')) mascara(e.target);
});

function cpfValido(c) {
    c = c.replace(/\D/g, '');
    if (c.length !== 11 || /^(\d)\1+$/.test(c)) return false;
    for (var t = 9; t < 11; t++) {
        var s = 0;
        for (var i = 0; i < t; i++) s += Number(c[i]) * (t + 1 - i);
        if (((s * 10) % 11) % 10 !== Number(c[t])) return false;
    }
    return true;
}
function emailValido(e) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(e); }

function usuarios() { return ler('rat_usuarios', []); }
function usuario() {
    var email = localStorage.getItem('rat_sessao');
    if (!email) return null;
    var l = usuarios();
    for (var i = 0; i < l.length; i++) if (l[i].email === email) return l[i];
    return null;
}

function cadastrar(e) {
    e.preventDefault();
    var u = {
        nome: el('c-nome').value.trim(),
        email: el('c-email').value.trim().toLowerCase(),
        cpf: el('c-cpf').value,
        tel: el('c-tel').value,
        cep: el('c-cep').value,
        num: el('c-num').value,
        end: el('c-end').value.trim(),
        senha: el('c-senha').value
    };
    var erro = '';
    if (u.nome.length < 3) erro = 'Informe seu nome completo.';
    else if (!emailValido(u.email)) erro = 'E-mail inválido.';
    else if (!cpfValido(u.cpf)) erro = 'CPF inválido.';
    else if (u.tel.length < 14) erro = 'Telefone incompleto.';
    else if (u.cep.length !== 9) erro = 'CEP incompleto.';
    else if (!u.num) erro = 'Informe o número do endereço.';
    else if (u.end.length < 3) erro = 'Informe o endereço de entrega.';
    else if (u.senha.length < 6) erro = 'A senha deve ter no mínimo 6 caracteres.';
    else if (u.senha !== el('c-senha2').value) erro = 'As senhas não conferem.';
    else {
        var l = usuarios();
        for (var i = 0; i < l.length; i++) {
            if (l[i].email === u.email) erro = 'Este e-mail já está cadastrado.';
            else if (l[i].cpf === u.cpf) erro = 'Este CPF já está cadastrado.';
        }
    }
    if (erro) { mensagem('cad-msg', erro); return; }

    var lista = usuarios();
    lista.push(u);
    gravar('rat_usuarios', lista);
    localStorage.setItem('rat_sessao', u.email);
    e.target.reset();
    mensagem('cad-msg', '');
    renderAuth();
    aviso('Cadastro realizado! Bem-vindo(a)');
    go('home');
}

function entrar(e) {
    e.preventDefault();
    var email = el('l-email').value.trim().toLowerCase(), senha = el('l-senha').value;
    var l = usuarios();
    for (var i = 0; i < l.length; i++) {
        if (l[i].email === email && l[i].senha === senha) {
            localStorage.setItem('rat_sessao', email);
            e.target.reset();
            renderAuth();
            go('home');
            return;
        }
    }
    mensagem('login-msg', 'E-mail ou senha incorretos.');
}

function sair() {
    localStorage.removeItem('rat_sessao');
    renderAuth();
    go('home');
}

function renderPerfil() {
    var u = usuario();
    if (!u) return;
    el('p-nome').value = u.nome;
    el('p-email').value = u.email;
    el('p-cpf').value = u.cpf;
    el('p-tel').value = u.tel;
    el('p-cep').value = u.cep;
    el('p-num').value = u.num;
    el('p-end').value = u.end;
    mensagem('perfil-msg', '');
}

function salvarPerfil(e) {
    e.preventDefault();
    var u = usuario();
    var nome = el('p-nome').value.trim(), tel = el('p-tel').value, cep = el('p-cep').value,
        num = el('p-num').value, end = el('p-end').value.trim();
    var erro = '';
    if (nome.length < 3) erro = 'Informe seu nome completo.';
    else if (tel.length < 14) erro = 'Telefone incompleto.';
    else if (cep.length !== 9) erro = 'CEP incompleto.';
    else if (!num) erro = 'Informe o número do endereço.';
    else if (end.length < 3) erro = 'Informe o endereço de entrega.';
    if (erro) { mensagem('perfil-msg', erro); return; }

    var l = usuarios();
    for (var i = 0; i < l.length; i++) {
        if (l[i].email === u.email) {
            l[i].nome = nome; l[i].tel = tel; l[i].cep = cep; l[i].num = num; l[i].end = end;
        }
    }
    gravar('rat_usuarios', l);
    renderAuth();
    mensagem('perfil-msg', 'Dados atualizados com sucesso!', true);
}

function excluirConta() {
    var u = usuario();
    if (!u || !confirm('Tem certeza que deseja excluir sua conta? Essa ação não pode ser desfeita.')) return;
    gravar('rat_usuarios', usuarios().filter(function (x) { return x.email !== u.email; }));
    gravar('rat_pedidos', ler('rat_pedidos', []).filter(function (p) { return p.email !== u.email; }));
    sair();
    aviso('Conta excluída');
}

function renderAuth() {
    var u = usuario();
    var claro = document.body.classList.contains('claro');
    var html = '<button class="tema" onclick="tema()" aria-label="Alternar modo claro/escuro">' +
        (claro ? '☾' : '☀') + '</button>';
    if (u) {
        html += '<span>Olá, ' + esc(u.nome.split(' ')[0]) + '</span>' +
            '<button class="btn" onclick="sair()">Sair</button>';
    } else {
        html += '<button class="btn" onclick="go(\'login\')">Entrar</button>' +
            '<button class="btnfill" onclick="go(\'cadastro\')">Cadastrar</button>';
    }
    el('auth').innerHTML = html;
}

function tema() {
    document.body.classList.toggle('claro');
    localStorage.setItem('rat_tema', document.body.classList.contains('claro') ? 'claro' : 'escuro');
    renderAuth();
}

if (localStorage.getItem('rat_tema') === 'claro') document.body.classList.add('claro');
renderAuth();
atualizaBadge();
go('home');
