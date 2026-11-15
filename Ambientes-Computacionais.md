# Publicação do Ratatouille na VM Ubuntu

O Apache serve os arquivos de `Site`. O cliente finaliza a compra e baixa o JSON em Pedidos. No Windows, o gerente abre o aplicativo Windows Forms, importa o arquivo e consulta a venda no MySQL. O navegador e o aplicativo não precisam compartilhar uma pasta.

## Preparar a VM de laboratório

Na VM Ubuntu indicada pela turma, verifique o IP com `hostname -I`. Instale os serviços se ainda não estiverem disponíveis:

```sh
sudo apt update
sudo apt install apache2 vsftpd mysql-server
sudo systemctl enable --now apache2 vsftpd mysql
```

Use a conta FTP e a configuração de rede indicadas na apostila da turma. Para upload de uma conta local com vsftpd, confira em `/etc/vsftpd.conf` se `local_enable=YES` e `write_enable=YES` estão ativos e `anonymous_enable=NO`. Depois de ajustar a configuração, execute `sudo systemctl restart vsftpd`. Se a turma configurou FTPS, mantenha o modo de criptografia correspondente no FileZilla.

## Transferir o site pelo FileZilla

1. Na conta de laboratório da VM, crie uma pasta de recebimento com `mkdir -p ~/ratatouille-site`.
2. No Windows, extraia o ZIP e abra o FileZilla. Conecte-se ao IP da VM usando o usuário FTP da atividade. No painel local, abra `Ratatouille/Site`. No painel remoto, abra `ratatouille-site` na pasta pessoal da mesma conta.
3. Envie todo o conteúdo de `Site`, incluindo `index.html`, CSS, JavaScript e fotos. Confira a fila de transferências concluídas.
4. Na VM, entre com essa mesma conta, copie os arquivos para uma pasta própria do projeto e verifique a resposta do Apache:

```sh
sudo install -d -m 755 /var/www/html/ratatouille
sudo cp -R "$HOME/ratatouille-site/." /var/www/html/ratatouille/
curl -I http://localhost/ratatouille/
```

5. No navegador do Windows, abra `http://IP-DA-VM/ratatouille/`, usando o IP obtido no Ubuntu. Confira a foto de abertura, os pratos e o tema. Cadastre um cliente de teste, finalize um pedido e baixe o JSON. As contas e o histórico do site pertencem ao endereço em que o navegador os criou.

## Ligar o Windows Forms ao MySQL

Para um banco novo, use o script original apenas em uma instância de laboratório vazia, pois ele contém `DROP DATABASE`. Para o banco existente do grupo, use somente `Banco/02_complemento.sql`. Configure `Gestao/conexao.json` com o servidor e as credenciais MySQL da atividade. Se o banco estiver na VM e o frontend estiver no Windows, o servidor indicado no aplicativo é o IP da VM. A conta MySQL precisa aceitar a conexão do computador Windows. Mantenha senhas e arquivos de configuração fora da pasta publicada pelo Apache.

Abra `Ratatouille.Gestao.sln` no Visual Studio e execute o Windows Forms. Faça login como gerente, importe o JSON baixado do site publicado e confira código, cliente, total e status no aplicativo. Reimporte o mesmo arquivo para conferir o bloqueio de duplicidade.

## Evidências da execução

Registre a transferência concluída no FileZilla, a página aberta pelo IP da VM no Windows, o pedido finalizado no site e o mesmo pedido importado no Windows Forms. Se necessário, consulte `SELECT codigo_pedido, total, status FROM pedidos WHERE codigo_pedido IS NOT NULL ORDER BY id DESC;` no banco. Evite exibir senhas e use dados fictícios na demonstração. Confira a ordem e os campos de entrega no PDF da turma. Esse PDF não ficou acessível nesta revisão. As capturas devem ser feitas na publicação real.
