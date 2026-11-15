# Spec Delta

## Purpose

Define como cada tela da missão e fora dela apresenta a informação: o que aparece primeiro, em que
forma e em que lugar, sem mudar as regras nem os textos de jogo.

## ADDED Requirements

### Requirement: Um ponto focal por tela
Cada tela SHALL ter um título curto, uma informação principal claramente maior que o resto e o botão
principal. Informações de apoio SHALL aparecer em linhas ou cartões menores, com ícone, nunca como
um parágrafo único.

#### Scenario: Boletim da manhã
- **WHEN** o Boletim abre num dia com alerta de erupção
- **THEN** o título do dia vem primeiro, as células de luz e o alerta aparecem em linhas próprias com ícone, e o botão principal fica no rodapé

### Requirement: Telas curtas no painel lateral
O Boletim, a Escolha, a Decisão do vazamento, a Tarefa concluída, O dia acontece e o Resultado SHALL
abrir num painel à direita, com a cena 3D visível à esquerda. A Distribuição SHALL usar o mesmo
painel, com os módulos em lista vertical.

#### Scenario: Resultado com a base à vista
- **WHEN** o Resultado de um dia com erupção aparece
- **THEN** o painel fica à direita e a base, com a luz de erupção, continua visível à esquerda

### Requirement: Custo antes da escolha
Cada cartão de escolha SHALL mostrar o custo em ícone e número antes do toque, com a mesma forma dos
medidores do HUD. Uma escolha bloqueada SHALL continuar visível, em cinza, com cadeado e o motivo.

#### Scenario: Escolha com custo
- **WHEN** a Escolha mostra "fazer" a coleta de pedras
- **THEN** o cartão mostra o ícone de Oxigênio com o custo, e o ícone de Ciência com o ganho, antes do toque

### Requirement: Distribuição por módulo
Na Distribuição, cada módulo SHALL aparecer como uma linha com a faixa da cor do módulo, o ícone, o
nome, as vagas de célula (cheias e vazias), os botões de mais e menos e a dica do que as células
farão. As células livres SHALL aparecer como células, não só como número.

#### Scenario: Colocar uma célula
- **WHEN** o jogador toca em mais no Ar e água
- **THEN** uma célula sai da reserva, preenche a primeira vaga do Ar e água, e a dica da linha muda na hora

### Requirement: Antes e depois estruturado
O Resultado SHALL mostrar uma linha por medidor com ícone, nome, valor de antes, valor de depois e a
variação em pílula com ▲, ▼ ou =. O "Por quê?" SHALL listar cada causa em uma linha com ícone.

#### Scenario: Oxigênio caiu
- **WHEN** o Oxigênio foi de 7 para 5 no dia
- **THEN** a linha mostra o ícone de Oxigênio, "7 → 5" e a pílula "▼ 2"

### Requirement: Cartão de dado real em destaque
O cartão "Isso aconteceu de verdade" SHALL ter o ícone de fonte, o título fixo, o dado (classe, data
e intensidade), o rótulo de dado arquivado e a fonte no rodapé, num molde próprio que o diferencia
dos outros cartões.

#### Scenario: Erupção do dia
- **WHEN** o Resultado de um dia com a erupção X9.3 aparece
- **THEN** o cartão de dado real aparece com seu molde próprio, a fonte no rodapé e o botão "Ver o registro"

### Requirement: Relatório como página própria
O Relatório SHALL ser uma página opaca de céu preto, sem a cena por trás, com estrelas (apagada
quando não conquistada), números em mosaicos com ícone, o gráfico por dia e as ações no rodapé. O
gráfico SHALL distinguir as séries também por forma do ponto.

#### Scenario: Gráfico sem cor
- **WHEN** o jogador vê o gráfico do Relatório em escala de cinza
- **THEN** Oxigênio, Radiação e Bateria se distinguem pela forma dos pontos (círculo, triângulo, quadrado)

### Requirement: Início com a base ao fundo
A tela Início SHALL mostrar a base 3D ao fundo, com o título do jogo e uma coluna de opções à
direita: a escolha de modo como ação principal, e "Como jogar", tutorial, idioma e créditos como
ações secundárias.

#### Scenario: Abrir o jogo
- **WHEN** o jogo termina de carregar
- **THEN** a base aparece ao fundo e a coluna de opções fica à direita, sem cobrir o centro

### Requirement: Créditos e fontes legíveis
A tela de Créditos SHALL listar cada fonte num cartão com nome, para que o jogo usou e o botão para
abrir, e SHALL citar as fontes tipográficas usadas com a licença OFL.

#### Scenario: Licença das fontes
- **WHEN** o jogador abre Créditos e fontes
- **THEN** aparecem os nomes das duas fontes tipográficas e a licença SIL Open Font License 1.1
