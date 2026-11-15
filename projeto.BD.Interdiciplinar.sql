
DROP DATABASE IF EXISTS delivery_italiano;
CREATE DATABASE delivery_italiano
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE delivery_italiano;

-- -------- CLIENTES---------
CREATE TABLE clientes (
  id          INT UNSIGNED NOT NULL AUTO_INCREMENT,
  nome        VARCHAR(120) NOT NULL,
  email       VARCHAR(150) NOT NULL,
  telefone    VARCHAR(20)  NOT NULL,
  cpf         CHAR(11)     NULL,
  criado_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_clientes_email (email),
  UNIQUE KEY uq_clientes_cpf (cpf)
) ENGINE=InnoDB;

-- ------ ENDERECOS----------------
CREATE TABLE enderecos (
  id           INT UNSIGNED NOT NULL AUTO_INCREMENT,
  cliente_id   INT UNSIGNED NOT NULL,
  cep          CHAR(8)      NOT NULL,
  logradouro   VARCHAR(150) NOT NULL,
  numero       VARCHAR(10)  NOT NULL,
  complemento  VARCHAR(80)  NULL,
  bairro       VARCHAR(80)  NOT NULL,
  cidade       VARCHAR(80)  NOT NULL,
  uf           CHAR(2)      NOT NULL,
  principal    BOOLEAN      NOT NULL DEFAULT FALSE,
  PRIMARY KEY (id),
  KEY idx_enderecos_cliente (cliente_id),
  CONSTRAINT fk_enderecos_cliente
    FOREIGN KEY (cliente_id) REFERENCES clientes (id)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- -------- CATEGORIAS (massas, pizzas, sobremesas, bebidas...)-------------
CREATE TABLE categorias (
  id      INT UNSIGNED NOT NULL AUTO_INCREMENT,
  nome    VARCHAR(60)  NOT NULL,
  ordem   INT          NOT NULL DEFAULT 0,
  ativa   BOOLEAN      NOT NULL DEFAULT TRUE,
  PRIMARY KEY (id),
  UNIQUE KEY uq_categorias_nome (nome)
) ENGINE=InnoDB;

-- -------- PRODUTOS (os pratos do cardápio)------------------
CREATE TABLE produtos (
  id             INT UNSIGNED NOT NULL AUTO_INCREMENT,
  categoria_id   INT UNSIGNED NOT NULL,
  nome           VARCHAR(120) NOT NULL,
  descricao      TEXT         NULL,
  ingredientes   VARCHAR(255) NULL,
  vegetariano    BOOLEAN      NOT NULL DEFAULT FALSE,
  contem_gluten  BOOLEAN      NOT NULL DEFAULT TRUE,
  foto_url       VARCHAR(255) NULL,
  disponivel     BOOLEAN      NOT NULL DEFAULT TRUE,
  PRIMARY KEY (id),
  KEY idx_produtos_categoria (categoria_id),
  CONSTRAINT fk_produtos_categoria
    FOREIGN KEY (categoria_id) REFERENCES categorias (id)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- -----VARIACOES (tamanhos/porções de cada produto, com preço-- --------------------------
CREATE TABLE variacoes (
  id                 INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  produto_id         INT UNSIGNED  NOT NULL,
  nome               VARCHAR(60)   NOT NULL,
  preco              DECIMAL(10,2) NOT NULL,
  tempo_preparo_min  INT UNSIGNED  NOT NULL DEFAULT 20,
  PRIMARY KEY (id),
  UNIQUE KEY uq_variacoes_produto_nome (produto_id, nome),
  CONSTRAINT fk_variacoes_produto
    FOREIGN KEY (produto_id) REFERENCES produtos (id)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT ck_variacoes_preco CHECK (preco >= 0)
) ENGINE=InnoDB;

-- ---------ADICIONAIS (queijo extra, borda recheada, bacon...)---------------
CREATE TABLE adicionais (
  id     INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  nome   VARCHAR(80)   NOT NULL,
  preco  DECIMAL(10,2) NOT NULL,
  ativo  BOOLEAN       NOT NULL DEFAULT TRUE,
  PRIMARY KEY (id),
  UNIQUE KEY uq_adicionais_nome (nome),
  CONSTRAINT ck_adicionais_preco CHECK (preco >= 0)
) ENGINE=InnoDB;

-- ------ CUPONS---------------
CREATE TABLE cupons (
  id             INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  codigo         VARCHAR(30)   NOT NULL,
  tipo           ENUM('percentual','valor_fixo') NOT NULL,
  valor          DECIMAL(10,2) NOT NULL,
  pedido_minimo  DECIMAL(10,2) NOT NULL DEFAULT 0,
  validade       DATE          NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_cupons_codigo (codigo),
  CONSTRAINT ck_cupons_valor CHECK (valor > 0)
) ENGINE=InnoDB;

-- --------PEDIDOS-- ------------
CREATE TABLE pedidos (
  id            INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  cliente_id    INT UNSIGNED  NOT NULL,
  endereco_id   INT UNSIGNED  NOT NULL,
  cupom_id      INT UNSIGNED  NULL,
  status        ENUM('aguardando_pagamento','confirmado','em_preparo',
                     'pronto','saiu_para_entrega','entregue','cancelado')
                NOT NULL DEFAULT 'aguardando_pagamento',
  subtotal      DECIMAL(10,2) NOT NULL DEFAULT 0,
  taxa_entrega  DECIMAL(10,2) NOT NULL DEFAULT 0,
  desconto      DECIMAL(10,2) NOT NULL DEFAULT 0,
  total         DECIMAL(10,2) NOT NULL DEFAULT 0,
  observacao    VARCHAR(255)  NULL,
  criado_em     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_pedidos_cliente (cliente_id),
  KEY idx_pedidos_endereco (endereco_id),
  KEY idx_pedidos_cupom (cupom_id),
  KEY idx_pedidos_status_data (status, criado_em),
  CONSTRAINT fk_pedidos_cliente
    FOREIGN KEY (cliente_id) REFERENCES clientes (id)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_pedidos_endereco
    FOREIGN KEY (endereco_id) REFERENCES enderecos (id)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_pedidos_cupom
    FOREIGN KEY (cupom_id) REFERENCES cupons (id)
    ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT ck_pedidos_valores CHECK (
    subtotal >= 0 AND taxa_entrega >= 0 AND desconto >= 0 AND total >= 0
  )
) ENGINE=InnoDB;

-- --------- ITENS_PEDIDO-----------------------
CREATE TABLE itens_pedido (
  id              INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  pedido_id       INT UNSIGNED  NOT NULL,
  variacao_id     INT UNSIGNED  NOT NULL,
  quantidade      INT UNSIGNED  NOT NULL DEFAULT 1,
  preco_unitario  DECIMAL(10,2) NOT NULL,
  observacao      VARCHAR(255)  NULL,
  PRIMARY KEY (id),
  KEY idx_itens_pedido (pedido_id),
  KEY idx_itens_variacao (variacao_id),
  CONSTRAINT fk_itens_pedido
    FOREIGN KEY (pedido_id) REFERENCES pedidos (id)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_itens_variacao
    FOREIGN KEY (variacao_id) REFERENCES variacoes (id)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT ck_itens_quantidade CHECK (quantidade > 0)
) ENGINE=InnoDB;

-- --------- ITEM_ADICIONAIS (adicionais escolhidos em cada item do pedido)-- ------------
CREATE TABLE item_adicionais (
  id               INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  item_pedido_id   INT UNSIGNED  NOT NULL,
  adicional_id     INT UNSIGNED  NOT NULL,
  preco            DECIMAL(10,2) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_item_adicional (item_pedido_id, adicional_id),
  KEY idx_item_adicionais_adicional (adicional_id),
  CONSTRAINT fk_item_adicionais_item
    FOREIGN KEY (item_pedido_id) REFERENCES itens_pedido (id)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_item_adicionais_adicional
    FOREIGN KEY (adicional_id) REFERENCES adicionais (id)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ---------- PAGAMENTOS-- ----------
CREATE TABLE pagamentos (
  id          INT UNSIGNED  NOT NULL AUTO_INCREMENT,
  pedido_id   INT UNSIGNED  NOT NULL,
  forma       ENUM('pix','cartao_credito','cartao_debito','dinheiro','vale_refeicao')
              NOT NULL,
  status      ENUM('pendente','aprovado','recusado','estornado')
              NOT NULL DEFAULT 'pendente',
  valor       DECIMAL(10,2) NOT NULL,
  troco_para  DECIMAL(10,2) NULL,
  pago_em     DATETIME      NULL,
  PRIMARY KEY (id),
  KEY idx_pagamentos_pedido (pedido_id),
  CONSTRAINT fk_pagamentos_pedido
    FOREIGN KEY (pedido_id) REFERENCES pedidos (id)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT ck_pagamentos_valor CHECK (valor > 0)
) ENGINE=InnoDB;

-- -------ENTREGADORES-- ----------
CREATE TABLE entregadores (
  id        INT UNSIGNED NOT NULL AUTO_INCREMENT,
  nome      VARCHAR(120) NOT NULL,
  telefone  VARCHAR(20)  NOT NULL,
  veiculo   ENUM('moto','bicicleta','carro') NOT NULL DEFAULT 'moto',
  placa     VARCHAR(8)   NULL,
  ativo     BOOLEAN      NOT NULL DEFAULT TRUE,
  PRIMARY KEY (id)
) ENGINE=InnoDB;

-- ------ENTREGAS (uma por pedido)-- -----
CREATE TABLE entregas (
  id               INT UNSIGNED NOT NULL AUTO_INCREMENT,
  pedido_id        INT UNSIGNED NOT NULL,
  entregador_id    INT UNSIGNED NULL,
  status           ENUM('aguardando_entregador','atribuida','a_caminho',
                        'entregue','falhou')
                   NOT NULL DEFAULT 'aguardando_entregador',
  codigo_rastreio  VARCHAR(20)  NOT NULL,
  saida_em         DATETIME     NULL,
  entregue_em      DATETIME     NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_entregas_pedido (pedido_id),
  UNIQUE KEY uq_entregas_rastreio (codigo_rastreio),
  KEY idx_entregas_entregador (entregador_id),
  CONSTRAINT fk_entregas_pedido
    FOREIGN KEY (pedido_id) REFERENCES pedidos (id)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_entregas_entregador
    FOREIGN KEY (entregador_id) REFERENCES entregadores (id)
    ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ------------HISTORICO_ENTREGA (linha do tempo de cada entrega)-- --------------
CREATE TABLE historico_entrega (
  id             INT UNSIGNED NOT NULL AUTO_INCREMENT,
  entrega_id     INT UNSIGNED NOT NULL,
  status         VARCHAR(30)  NOT NULL,
  observacao     VARCHAR(255) NULL,
  registrado_em  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_historico_entrega (entrega_id, registrado_em),
  CONSTRAINT fk_historico_entrega
    FOREIGN KEY (entrega_id) REFERENCES entregas (id)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- =========DADOS DE EXEMPLO=================

INSERT INTO categorias (nome, ordem) VALUES
  ('Massas', 1),
  ('Pizzas', 2),
  ('Sobremesas', 3),
  ('Bebidas', 4);

INSERT INTO produtos (categoria_id, nome, descricao, ingredientes, vegetariano, contem_gluten) VALUES
  (1, 'Spaghetti alla Carbonara', 'Clássico romano com guanciale e pecorino.', 'spaghetti, guanciale, ovos, pecorino, pimenta-do-reino', FALSE, TRUE),
  (1, 'Lasagna alla Bolognese',  'Camadas de massa fresca, ragù e bechamel.',   'massa, carne, tomate, bechamel, parmesão',            FALSE, TRUE),
  (1, 'Gnocchi ao Sugo',         'Nhoque de batata com molho de tomate fresco.', 'batata, farinha, tomate, manjericão',                 TRUE,  TRUE),
  (2, 'Pizza Margherita',        'Molho de tomate, muçarela de búfala e manjericão.', 'tomate, mozzarella di bufala, manjericão',         TRUE,  TRUE),
  (2, 'Pizza Quattro Formaggi',  'Muçarela, gorgonzola, parmesão e provolone.',  'mozzarella, gorgonzola, parmesão, provolone',         TRUE,  TRUE),
  (3, 'Tiramisù',                'Sobremesa de café com mascarpone.',            'mascarpone, café, biscoito champanhe, cacau',         TRUE,  TRUE),
  (3, 'Panna Cotta',             'Creme de nata com calda de frutas vermelhas.', 'nata, açúcar, gelatina, frutas vermelhas',            TRUE,  FALSE),
  (4, 'Água com gás San Pellegrino', 'Garrafa 500 ml.',                          NULL,                                                  TRUE,  FALSE),
  (4, 'Refrigerante Lata',       'Lata 350 ml.',                                 NULL,                                                  TRUE,  FALSE);

INSERT INTO variacoes (produto_id, nome, preco, tempo_preparo_min) VALUES
  (1, 'Individual', 42.90, 20),
  (1, 'Família',    89.90, 25),
  (2, 'Individual', 46.90, 30),
  (2, 'Família',    94.90, 35),
  (3, 'Individual', 39.90, 20),
  (4, 'Broto',      39.90, 25),
  (4, 'Grande',     64.90, 30),
  (5, 'Broto',      44.90, 25),
  (5, 'Grande',     72.90, 30),
  (6, 'Porção',     22.90, 5),
  (7, 'Porção',     19.90, 5),
  (8, 'Garrafa',    14.00, 1),
  (9, 'Lata',        6.50, 1);

INSERT INTO adicionais (nome, preco) VALUES
  ('Queijo parmesão extra', 5.00),
  ('Bacon crocante',        7.00),
  ('Borda recheada',        9.00),
  ('Azeite trufado',        8.00);

INSERT INTO cupons (codigo, tipo, valor, pedido_minimo, validade) VALUES
  ('BENVENUTO10', 'percentual', 10.00, 50.00, '2026-12-31'),
  ('FRETEGRATIS', 'valor_fixo',  8.00, 70.00, '2026-12-31');

INSERT INTO clientes (nome, email, telefone, cpf) VALUES
  ('Mariana Souza', 'mariana@email.com', '31999990000', '12345678901');

INSERT INTO enderecos (cliente_id, cep, logradouro, numero, complemento, bairro, cidade, uf, principal) VALUES
  (1, '30140071', 'Rua da Bahia', '1200', 'Apto 302', 'Centro', 'Belo Horizonte', 'MG', TRUE);

INSERT INTO entregadores (nome, telefone, veiculo, placa) VALUES
  ('Carlos Lima', '31988880000', 'moto', 'ABC1D23');

-- Pedido de exemplo: 1 Carbonara (individual) com parmesão extra + 1 Tiramisù
INSERT INTO pedidos (cliente_id, endereco_id, cupom_id, status, subtotal, taxa_entrega, desconto, total)
VALUES (1, 1, 1, 'entregue', 70.80, 8.00, 7.08, 71.72);

INSERT INTO itens_pedido (pedido_id, variacao_id, quantidade, preco_unitario) VALUES
  (1, 1,  1, 42.90),
  (1, 10, 1, 22.90);

INSERT INTO item_adicionais (item_pedido_id, adicional_id, preco) VALUES
  (1, 1, 5.00);

INSERT INTO pagamentos (pedido_id, forma, status, valor, pago_em) VALUES
  (1, 'pix', 'aprovado', 71.72, NOW());

INSERT INTO entregas (pedido_id, entregador_id, status, codigo_rastreio, saida_em, entregue_em) VALUES
  (1, 1, 'entregue', 'ITA-0001-BH', NOW() - INTERVAL 30 MINUTE, NOW());

INSERT INTO historico_entrega (entrega_id, status, observacao) VALUES
  (1, 'atribuida', 'Entregador Carlos aceitou a corrida'),
  (1, 'a_caminho', 'Saiu da cantina'),
  (1, 'entregue',  'Entregue ao cliente');

-- =========VIEWS ÚTEIS==========

-- Resumo de cada pedido com cliente, entregador e status da entrega
CREATE OR REPLACE VIEW vw_pedidos_resumo AS
SELECT
  p.id              AS pedido_id,
  c.nome            AS cliente,
  p.status          AS status_pedido,
  p.total,
  e.status          AS status_entrega,
  e.codigo_rastreio,
  en.nome           AS entregador,
  p.criado_em
FROM pedidos p
JOIN clientes c          ON c.id = p.cliente_id
LEFT JOIN entregas e     ON e.pedido_id = p.id
LEFT JOIN entregadores en ON en.id = e.entregador_id;

-- Pratos mais vendidos
CREATE OR REPLACE VIEW vw_mais_vendidos AS
SELECT
  pr.nome                        AS produto,
  v.nome                         AS variacao,
  SUM(i.quantidade)              AS qtd_vendida,
  SUM(i.quantidade * i.preco_unitario) AS faturamento
FROM itens_pedido i
JOIN variacoes v  ON v.id = i.variacao_id
JOIN produtos pr  ON pr.id = v.produto_id
JOIN pedidos p    ON p.id = i.pedido_id AND p.status <> 'cancelado'
GROUP BY pr.nome, v.nome
ORDER BY qtd_vendida DESC;

-- Exemplos de consulta:
-- SELECT * FROM vw_pedidos_resumo;
-- SELECT * FROM vw_mais_vendidos;
