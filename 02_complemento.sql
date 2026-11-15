-- Execute no banco existente. Este complemento não apaga tabelas nem dados.
USE delivery_italiano;

CREATE TABLE IF NOT EXISTS gerentes (
  id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(150) NOT NULL UNIQUE,
  senha_hash VARBINARY(32) NOT NULL,
  salt VARBINARY(16) NOT NULL,
  iteracoes INT NOT NULL,
  ativo BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;

-- O exemplo didático usa Pendente; preservamos também todos os status originais.
ALTER TABLE pedidos MODIFY status ENUM('aguardando_pagamento','confirmado','em_preparo',
  'pronto','saiu_para_entrega','entregue','cancelado','Pendente') NOT NULL DEFAULT 'Pendente';

DELIMITER $$
DROP PROCEDURE IF EXISTS rat_completar_codigo_pedido$$
CREATE PROCEDURE rat_completar_codigo_pedido()
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='pedidos' AND COLUMN_NAME='codigo_pedido') THEN
    ALTER TABLE pedidos ADD codigo_pedido VARCHAR(70) NULL, ADD UNIQUE KEY uq_codigo_pedido(codigo_pedido);
  END IF;
END$$
CALL rat_completar_codigo_pedido()$$
DROP PROCEDURE rat_completar_codigo_pedido$$
DELIMITER ;

CREATE TABLE IF NOT EXISTS mapa_produtos_site (
  site_id INT NOT NULL PRIMARY KEY,
  variacao_id INT UNSIGNED NOT NULL UNIQUE,
  CONSTRAINT fk_mapa_variacao FOREIGN KEY(variacao_id) REFERENCES variacoes(id) ON DELETE CASCADE
) ENGINE=InnoDB;

START TRANSACTION;
-- Cadastra os pratos que faltavam, com os nomes e descrições exatos do site.

INSERT INTO categorias(nome,ordem) SELECT 'Massas',1
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Massas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Massas' LIMIT 1),'Spaghetti al Pomodoro Fresco e Basilico','Tomate italiano, azeite extravirgem, manjericão e massa trefilada em bronze.','1_gdvuMWmCJJRhi_XfGcu8-Q.jpg.jpeg'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Spaghetti al Pomodoro Fresco e Basilico');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',42.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Spaghetti al Pomodoro Fresco e Basilico')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 1,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Spaghetti al Pomodoro Fresco e Basilico') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=1);

INSERT INTO categorias(nome,ordem) SELECT 'Massas',1
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Massas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Massas' LIMIT 1),'Fettuccine Alfredo','Massa fresca com molho cremoso de manteiga, filé de frango e parmesão.','picture.webp'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Fettuccine Alfredo');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',49.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Fettuccine Alfredo')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 2,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Fettuccine Alfredo') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=2);

INSERT INTO categorias(nome,ordem) SELECT 'Massas',1
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Massas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Massas' LIMIT 1),'Ravioli ai Funghi','Ravioli recheado, cogumelos salteados e lascas de parmesão.','post.webp'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Ravioli ai Funghi');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',54.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Ravioli ai Funghi')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 3,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Ravioli ai Funghi') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=3);

INSERT INTO categorias(nome,ordem) SELECT 'Pizzas',2
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Pizzas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Pizzas' LIMIT 1),'Pizza Margherita con Burrata e Pomodorini','Massa de longa fermentação, muçarela de búfala, tomate e folhas de manjericão.','pizza-5275191_1920.webp'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Pizza Margherita con Burrata e Pomodorini');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',59.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Margherita con Burrata e Pomodorini')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 4,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Margherita con Burrata e Pomodorini') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=4);

INSERT INTO categorias(nome,ordem) SELECT 'Pizzas',2
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Pizzas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Pizzas' LIMIT 1),'Pizza Calabresa','Molho de tomate, muçarela, calabresa fatiada e cebola.','calabresa.png'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Pizza Calabresa');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',52.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Calabresa')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 5,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Calabresa') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=5);

INSERT INTO categorias(nome,ordem) SELECT 'Pizzas',2
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Pizzas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Pizzas' LIMIT 1),'Pizza Quattro Formaggi','Muçarela, gorgonzola, parmesão e provolone.','queijo.jpg.jpeg'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Pizza Quattro Formaggi');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',64.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Quattro Formaggi')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 6,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Pizza Quattro Formaggi') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=6);

INSERT INTO categorias(nome,ordem) SELECT 'Sobremesas',3
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Sobremesas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Sobremesas' LIMIT 1),'Cannoli Siciliani Tradizionali','Massa artesanal crocante com recheio cremoso de ricota doce e gotas de chocolate.','receitas-de-doces-italianos-1-730x449.webp'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Cannoli Siciliani Tradizionali');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',24.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Cannoli Siciliani Tradizionali')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 7,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Cannoli Siciliani Tradizionali') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=7);

INSERT INTO categorias(nome,ordem) SELECT 'Sobremesas',3
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Sobremesas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Sobremesas' LIMIT 1),'Tiramisù','Camadas de biscoito champagne, café, creme de mascarpone e cacau.','tiramissu.webp'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Tiramisù');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',26.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Tiramisù')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 8,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Tiramisù') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=8);

INSERT INTO categorias(nome,ordem) SELECT 'Sobremesas',3
WHERE NOT EXISTS (SELECT 1 FROM categorias WHERE nome='Sobremesas');
INSERT INTO produtos(categoria_id,nome,descricao,foto_url)
SELECT (SELECT id FROM categorias WHERE nome='Sobremesas' LIMIT 1),'Panna Cotta','Creme gelado de baunilha com calda de frutas vermelhas.','Panna-cotta.jpg.jpeg'
WHERE NOT EXISTS (SELECT 1 FROM produtos WHERE nome='Panna Cotta');
INSERT INTO variacoes(produto_id,nome,preco)
SELECT p.id,'Cardápio do site',22.90 FROM produtos p
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Panna Cotta')
AND NOT EXISTS (SELECT 1 FROM variacoes v WHERE v.produto_id=p.id AND v.nome='Cardápio do site');
INSERT INTO mapa_produtos_site(site_id,variacao_id)
SELECT 9,v.id FROM variacoes v JOIN produtos p ON p.id=v.produto_id
WHERE p.id=(SELECT MIN(pr.id) FROM produtos pr WHERE pr.nome='Panna Cotta') AND v.nome='Cardápio do site'
AND NOT EXISTS (SELECT 1 FROM mapa_produtos_site WHERE site_id=9);

COMMIT;
