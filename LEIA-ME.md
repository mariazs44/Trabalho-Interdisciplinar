# Ratatouille - complementos do trabalho

## O que este pacote contém

- Site original, com os mesmos textos e CSS, acrescido de exportação de pedidos JSON.
- Correção dos caminhos das imagens terminadas em `.jpg.jpeg`, do idioma do HTML e do escape do código do pedido.
- Campos complementares de cidade, UF e bairro para compatibilidade com a tabela de endereços existente.
- Projeto C# complementar para Windows Forms: login do gerente, importação JSON, pedidos, produtos e CRUDs de clientes e entregadores.
- SQL incremental que mantém os dados existentes, cadastra os nove pratos do site e relaciona os IDs do site às variações do banco.
- Exemplo de pedido e documento do uso de IA.

## Limites da revisão

Abra Ratatouille.Gestao.sln para executar o projeto independente em Windows Forms.

As fotos originais estão incluídas na pasta Site. Abra Site/index.html para visualizar o restaurante.

## Preparar o banco

1. Se `delivery_italiano` já existe, execute apenas `Banco/02_complemento.sql` no MySQL Workbench.
2. Se for uma instalação nova, o SQL original `projeto.BD.Interdiciplinar.sql` está preservado na pasta Banco. ATENÇÃO: ele contém `DROP DATABASE`; execute-o somente quando não houver dados a preservar. Depois execute `02_complemento.sql`.
3. Não é necessário apagar ou substituir as tabelas originais. O complemento pode ser executado novamente sem duplicar o catálogo ou redefinir os preços já alterados.

## Executar o C# complementar

1. Abra `Gestao/Ratatouille.Gestao.csproj` no Visual Studio com a carga de trabalho de desenvolvimento desktop .NET e SDK .NET 8.
2. Restaure o pacote NuGet `MySql.Data` especificado no projeto.
3. Preencha servidor, porta, usuário e senha do MySQL em `Gestao/conexao.json`. O banco é `delivery_italiano`.
4. Compile e execute. Na primeira abertura, configure um e-mail e uma senha de pelo menos 8 caracteres para o gerente. Não há senha padrão.
5. Entre com essa conta. A senha é armazenada no banco com PBKDF2, salt aleatório e comparação de hash.

## Fluxo completo de JSON

1. Abra `Site/index.html`, cadastre um cliente e preencha também cidade e UF. Cadastros antigos podem completar esses dados em Perfil.
2. Adicione pratos e finalize o pedido.
3. Em Pedidos, clique em `Baixar pedido JSON`.
4. No C#, entre como gerente e abra `Pedidos e importação JSON`.
5. Importe o arquivo. Cliente, endereço, pedido e itens são gravados juntos em uma transação. O status inicial é `Pendente`.
6. A importação rejeita código duplicado, cliente inconsistente, itens inválidos, produto ausente e valores divergentes. O exemplo `exemplo-pedido.json` totaliza R$ 116,70.

A integração exigida pelo material é feita por arquivo JSON. O site continua com os cadastros e pedidos locais do navegador; não há API web nem atualização automática dos status do C# de volta para o navegador. O login do cliente que já existia no site é uma demonstração local; o acesso ao banco é feito pelo aplicativo do gerente. Pedidos antigos, feitos antes deste complemento, não têm fotografia completa dos dados para exportação e precisam ser refeitos para testar esse fluxo.

## Regras das telas

- Pedidos: importar, consultar itens, alterar status e excluir apenas quando cancelado. Não existe cadastro manual de pedido.
- Produtos: consultar, alterar preço da variação e excluir quando as relações do banco permitirem. Não existe cadastro de produto pelo C#.
- Clientes e entregadores: inserir, consultar, alterar e excluir. Clientes com pedidos permanecem protegidos pelas chaves estrangeiras.
- Alterações de preço também precisam ser copiadas para o valor correspondente em `PRATOS`, no `script.js`, antes de gerar outro pedido: a importação confere o preço do banco. Produtos excluídos no C# também precisam ser removidos do cardápio web manualmente; o importador rejeita um produto removido ou indisponível.

## Verificação realizada e pendente

PASSOU: verificação sintática do JavaScript e testes executáveis de exportação, soma em centavos, dados do cliente e endereço, ausência de senha no JSON, códigos distintos, separação de usuários, login obrigatório para finalizar, endereço incompleto, carrinho inválido e limite de quantidade. O texto visível original do HTML foi comparado e preservado; o CSS é igual ao recebido.

Teste no Chrome: cadastro com máscaras, pedido de R$ 50,90, conteúdo do JSON, imagens do cardápio, busca e largura móvel de 390 px verificados. A gravação do download no disco foi cancelada pelo navegador de teste. Faltam a compilação do Windows Forms, o teste com MySQL e a publicação real na VM. Veja Ambientes-Computacionais.md.

## Conferência final no computador do grupo

1. Compilar sem erros e conferir conexão e primeiro cadastro do gerente.
2. Testar senha errada, correta, logout e reabertura do aplicativo.
3. Importar o exemplo e conferir cliente, endereço, dois itens e total R$ 116,70.
4. Reimportar e confirmar que a duplicidade é bloqueada.
5. Alterar total ou quantidade no JSON e confirmar que não entra nenhum registro parcial.
6. Fazer um pedido novo no site e importar o arquivo baixado.
7. Alterar status, cancelar e excluir, verificando as relações no MySQL.
8. Conferir alteração de preço, exclusão de produto e CRUDs completos de cliente e entregador.
9. Repetir a migração e conferir ausência de duplicações no catálogo.
10. Testar visualmente o site em celular e desktop, com todas as imagens originais presentes.

## Base usada

Materiais recebidos: `Projeto_Integrador_Csharp_Tarde.pdf`, `Parte 5 - Tela de Pedidos e Importação JSON(2).pdf`, site Ratatouille e SQL enviados. O trabalho anterior Kalzona Sabores serviu como referência do fluxo de entrega; não foi usado para trocar os textos ou o cardápio do Ratatouille. Não houve revisão direta de outras atividades do Classroom nesta sessão.
