// Complementa o site original sem substituir seus textos ou seu layout.
(function () {
    'use strict';
    function centavos(n) { return Math.round(Number(n) * 100); }
    // Remove entradas inválidas antes de calcular ou renderizar o carrinho.
    carrinho = function () {
        var valor = ler('rat_carrinho', []);
        if (!Array.isArray(valor)) return [];
        return valor.filter(function (x) { return x && achar(x.id) && Number.isInteger(x.q) && x.q > 0 && x.q <= 99; });
    };
    var adicionarOriginal = adicionar;
    adicionar = function (id) {
        if (!achar(id)) return;
        if (carrinho().some(function (x) { return x.id === id && x.q >= 99; })) { aviso('Limite de 99 unidades por prato.'); return; }
        adicionarOriginal(id);
    };
    var quantidadeOriginal = mudarQtd;
    mudarQtd = function (id, delta) {
        if (!Number.isInteger(delta) || Math.abs(delta) !== 1) return;
        if (delta > 0 && carrinho().some(function (x) { return x.id === id && x.q >= 99; })) { aviso('Limite de 99 unidades por prato.'); return; }
        quantidadeOriginal(id, delta);
    };
    function codigo() {
        return 'RAT-' + (globalThis.crypto && crypto.randomUUID ? crypto.randomUUID() :
            Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 14));
    }
    // Dados exigidos pela tabela enderecos do banco enviado.
    ['cadastro', 'perfil'].forEach(function (view) {
        var prefixo = view === 'cadastro' ? 'c' : 'p';
        var grade = el('v-' + view).querySelector('.grade-form');
        [['cidade', 'Cidade', 80], ['uf', 'Estado (UF)', 2], ['bairro', 'Bairro', 80]].forEach(function (campo) {
            var div = document.createElement('div'); div.className = 'campo';
            var label = document.createElement('label'); label.htmlFor = prefixo + '-' + campo[0]; label.textContent = campo[1];
            var input = document.createElement('input'); input.id = label.htmlFor; input.maxLength = campo[2];
            div.append(label, input); grade.appendChild(div);
        });
    });
    function enderecoExtra(prefixo) {
        var cidade = el(prefixo + '-cidade').value.trim(), uf = el(prefixo + '-uf').value.trim().toUpperCase();
        if (!cidade || !/^(AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO)$/.test(uf))
            throw new Error('Informe a cidade e uma UF válida no perfil.');
        return { cidade: cidade, uf: uf, bairro: el(prefixo + '-bairro').value.trim() };
    }
    function salvarExtra(dados) {
        var u = usuario(); if (!u) return;
        var lista = usuarios();
        lista.forEach(function (x) { if (x.email === u.email) Object.assign(x, dados); });
        gravar('rat_usuarios', lista);
    }
    var cadastroOriginal = cadastrar;
    cadastrar = function (e) {
        e.preventDefault(); var dados;
        try { dados = enderecoExtra('c'); } catch (erro) { mensagem('cad-msg', erro.message); return; }
        var email = el('c-email').value.trim().toLowerCase();
        var existe = usuarios().some(function (u) { return u.email === email; });
        cadastroOriginal(e);
        if (!existe && usuario() && usuario().email === email) salvarExtra(dados);
    };
    var perfilOriginal = renderPerfil;
    renderPerfil = function () {
        perfilOriginal(); var u = usuario(); if (!u) return;
        ['cidade', 'uf', 'bairro'].forEach(function (k) { el('p-' + k).value = u[k] || ''; });
    };
    var salvarOriginal = salvarPerfil;
    salvarPerfil = function (e) {
        e.preventDefault(); var dados;
        try { dados = enderecoExtra('p'); } catch (erro) { mensagem('perfil-msg', erro.message); return; }
        salvarOriginal(e);
        if (el('perfil-msg').classList.contains('ok')) salvarExtra(dados);
    };
    // Mantém uma fotografia dos dados do pedido para a exportação posterior.
    finalizar = function () {
        var c = carrinho(), u = usuario();
        if (!c.length) { aviso('Seu carrinho está vazio'); return; }
        if (!u) { go('login', 'Faça login para finalizar o pedido.'); return; }
        if (!u.cidade || !u.uf) { go('perfil'); mensagem('perfil-msg', 'Complete a cidade e o estado antes de finalizar.'); return; }
        var itens = [], sub = 0;
        for (var i = 0; i < c.length; i++) {
            var p = achar(c[i].id);
            if (!p || !Number.isInteger(c[i].q) || c[i].q < 1 || c[i].q > 99) { aviso('Revise os itens do carrinho.'); return; }
            itens.push({ produtoId: p.id, nome: p.nome, q: c[i].q, preco: p.preco });
            sub += centavos(p.preco) * c[i].q;
        }
        var agora = new Date(), pedidos = ler('rat_pedidos', []);
        pedidos.push({ num: codigo(), email: u.email, data: agora.toLocaleString('pt-BR'), criadoEm: agora.toISOString(),
            cliente: { nome: u.nome, email: u.email, telefone: u.tel.replace(/\D/g, ''), cpf: u.cpf.replace(/\D/g, '') },
            entrega: { cep: u.cep.replace(/\D/g, ''), logradouro: u.end, numero: u.num, bairro: u.bairro || '', cidade: u.cidade, uf: u.uf },
            itens: itens, subtotal: sub / 100, taxaEntrega: TAXA_ENTREGA, total: (sub + centavos(TAXA_ENTREGA)) / 100,
            endereco: u.end + ', ' + u.num + ' - CEP ' + u.cep, status: 'Pendente' });
        gravar('rat_pedidos', pedidos); gravar('rat_carrinho', []); atualizaBadge();
        aviso('Pedido realizado com sucesso!'); go('pedidos');
    };
    window.exportarPedido = function (num) {
        var u = usuario(); if (!u) return;
        var p = ler('rat_pedidos', []).find(function (x) { return x.num === num && x.email === u.email; });
        if (!p || !p.cliente || !p.entrega) { aviso('Este pedido antigo não possui dados de exportação. Faça um novo pedido.'); return; }
        var json = { versao: 1, pedidoId: p.num, criadoEm: p.criadoEm, cliente: p.cliente, endereco: p.entrega,
            itens: p.itens.map(function (x) { return { produtoId: x.produtoId, nome: x.nome, quantidade: x.q, precoUnitario: x.preco }; }),
            subtotal: p.subtotal, taxaEntrega: p.taxaEntrega, total: p.total };
        // Senha e sessão nunca são incluídas no arquivo JSON.
        var url = URL.createObjectURL(new Blob([JSON.stringify(json, null, 2)], { type: 'application/json' }));
        var a = document.createElement('a'); a.href = url; a.download = p.num.replace(/[^A-Za-z0-9-]/g, '') + '.json';
        document.body.appendChild(a); a.click(); a.remove(); setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
    };
    var pedidosOriginal = renderPedidos;
    renderPedidos = function () {
        pedidosOriginal(); var u = usuario(); if (!u) return;
        var lista = ler('rat_pedidos', []).filter(function (p) { return p.email === u.email; }).reverse();
        document.querySelectorAll('#lista-pedidos .pedido').forEach(function (card, i) {
            var botao = document.createElement('button'); botao.className = 'btn'; botao.textContent = 'Baixar pedido JSON';
            botao.onclick = function () { exportarPedido(lista[i].num); }; card.appendChild(botao);
        });
    };
    // Corrige apenas os caminhos: os arquivos recebidos têm a extensão .jpg.jpeg.
    var imagens = ['1_gdvuMWmCJJRhi_XfGcu8-Q.jpg', 'culinaria-italiana-1.jpg', 'queijo.jpg', 'Panna-cotta.jpg'];
    PRATOS.forEach(function (p) { if (imagens.indexOf(p.img) >= 0) p.img += '.jpeg'; });
    document.querySelectorAll('img').forEach(function (img) {
        if (imagens.indexOf(img.getAttribute('src')) >= 0) img.setAttribute('src', img.getAttribute('src') + '.jpeg');
    });
}());
