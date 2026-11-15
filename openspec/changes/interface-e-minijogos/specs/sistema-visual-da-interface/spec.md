# Spec Delta

## Purpose

Define a linguagem visual única da interface do jogo (cor, tipografia, forma, ícones, estados,
movimento e acessibilidade), para que toda tela pareça feita das mesmas peças da Estação-Roda.

## ADDED Requirements

### Requirement: Cor com papel fixo
A interface SHALL usar cor forte só para três papéis: o acento de cada módulo (a mesma cor do
módulo 3D), a energia e o alerta. O vermelho SHALL aparecer só em alerta de medidor e no ícone do
aviso de erupção, nunca em texto. Superfícies e textos SHALL usar os neutros da base (aço escuro,
pintura branca, cinza).

#### Scenario: Cartão de um módulo
- **WHEN** a tela mostra o módulo Estufa
- **THEN** o acento do cartão usa a mesma cor verde das tiras de luz da Estufa na cena 3D

#### Scenario: Aviso de erupção no Boletim
- **WHEN** o Boletim mostra um alerta de erupção
- **THEN** a linha usa o ícone vermelho de alerta, e o texto da linha continua na cor creme

#### Scenario: Nenhum botão vermelho
- **WHEN** qualquer tela da missão é exibida
- **THEN** nenhum botão, título ou fundo de painel usa o vermelho de alerta

### Requirement: Contraste de leitura
Todo texto SHALL ter contraste de pelo menos 4,5:1 com o fundo, e o texto do botão principal de
pelo menos 7:1. Nenhuma informação SHALL depender só da cor: estado e sentido também aparecem por
forma, ícone ou palavra.

#### Scenario: Variação lida sem cor
- **WHEN** o jogador vê um valor que subiu e outro que caiu em escala de cinza
- **THEN** cada valor mostra ▲, ▼ ou = junto do número

### Requirement: Tipografia fechada
A interface SHALL usar no máximo duas famílias de fonte livres (licença OFL) com os acentos do
português, e no máximo cinco tamanhos de texto, cada um com papel fixo. Nenhum texto SHALL ficar
abaixo de 24 unidades no referencial de 1920 por 1080.

#### Scenario: Acentos do português
- **WHEN** a tela mostra "Ação, coração, não deu, órgão, maçã"
- **THEN** todos os caracteres aparecem na fonte do jogo, sem quadrado vazio nem fonte trocada

#### Scenario: Menor texto
- **WHEN** o jogo roda numa janela de 1280 por 720
- **THEN** o menor rótulo aparece com pelo menos 16 pixels de altura

### Requirement: Painéis opacos e chanfrados
Todo painel e botão SHALL seguir um único molde: superfície opaca, cantos chanfrados e sombra dura
deslocada para baixo, sem contorno brilhante, sem transparência sobre a cena e sem caixa dentro de
caixa.

#### Scenario: Painel sobre a cena
- **WHEN** um painel aparece sobre a base 3D
- **THEN** a cena não aparece através do painel e o texto fica legível em qualquer estado de luz

### Requirement: Hierarquia de ações
Cada tela SHALL ter no máximo um botão principal, no estilo mais forte da interface. Ações
secundárias SHALL ter estilo visivelmente mais fraco (contorno ou superfície neutra), nunca um
segundo botão cheio na mesma cor.

#### Scenario: Escolha da tarefa
- **WHEN** a tela de escolha mostra "fazer" e "ficar"
- **THEN** as duas escolhas são cartões do mesmo nível, e só o botão de confirmar do rodapé, se houver, usa o estilo principal

### Requirement: Estados de botão
Todo elemento tocável SHALL ter os estados normal, pressionado, desativado e foco. Pressionado SHALL
parecer afundar (desce e perde a sombra) em até 100 ms. Desativado SHALL mostrar o motivo em texto.
Foco SHALL ter moldura visível para teclado e controle.

#### Scenario: Botão desativado
- **WHEN** "Sair da base" está bloqueado porque o Oxigênio está em 1
- **THEN** o botão aparece em cinza, com cadeado e o motivo escrito, em vez de sumir

#### Scenario: Navegação por controle
- **WHEN** o jogador navega com o controle ou o teclado
- **THEN** o botão selecionado mostra a moldura de foco

### Requirement: Alvos de toque
Todo alvo tocável SHALL ter pelo menos 88 por 88 unidades no referencial de 1920 por 1080, com pelo
menos 12 unidades entre alvos vizinhos.

#### Scenario: Botões de mais e menos
- **WHEN** a Distribuição mostra os botões de mais e menos de cada módulo
- **THEN** cada botão tem pelo menos 88 por 88 unidades

### Requirement: Ícones próprios
Cada medidor, módulo, tarefa e ação recorrente SHALL ter um ícone próprio, da mesma família visual
(forma cheia em dois tons, sem contorno), e o ícone SHALL acompanhar a palavra, nunca a substituir.
A interface SHALL NOT usar emoji como ícone.

#### Scenario: Medidor de Oxigênio
- **WHEN** o HUD mostra o Oxigênio
- **THEN** aparecem o ícone de oxigênio, a palavra ou sigla "O₂" e o valor

### Requirement: Movimento suave e curto
Animações de interface SHALL durar até 500 ms, com desaceleração na entrada. Nada SHALL piscar,
tremer ou repetir mais de 3 vezes por segundo. Quando o navegador pede movimento reduzido,
deslocamentos e escalas SHALL ser trocados por troca de opacidade.

#### Scenario: Movimento reduzido
- **WHEN** o navegador informa preferência por movimento reduzido
- **THEN** os painéis entram só com troca de opacidade, sem deslizar
