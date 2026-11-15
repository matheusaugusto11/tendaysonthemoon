# Spec Delta

## Purpose

Faz o dado real da NASA decidir a partida. As erupções solares do jogo são registros reais do
DONKI, e a classe de cada uma define a dose de radiação. A fonte aparece sempre, com o rótulo
de dado arquivado.

## ADDED Requirements

### Requirement: Conjunto curado de erupções reais
O jogo SHALL embutir as 20 erupções do catálogo FLR do NASA DONKI listadas em `Docs/DADOS-NASA.md` §2.3 (5 de classe X, 10 de classe M e 5 de classe C), cada uma com data, classe e link do registro. A intensidade de jogo SHALL ser 0 (fraca) para C, 1 (média) para M e 2 (forte) para X.

#### Scenario: Registro embutido
- **WHEN** o jogo carrega as erupções
- **THEN** existem 20 registros e cada um tem data, classe, intensidade e um link `https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/<número>/-1`

### Requirement: Sorteio dos alertas
No início de cada partida, o jogo SHALL sortear os dias de alerta entre os dias 2 e 10 (4 no Cadete, 6 no Comandante). Os alertas com erupção real SHALL ser 2 no Cadete e 3 no Comandante, sempre com pelo menos uma erupção forte. Os demais alertas SHALL ser alarmes falsos.

#### Scenario: Erupção forte garantida
- **WHEN** uma partida nova começa em qualquer modo
- **THEN** pelo menos um dos alertas com erupção real usa uma erupção de classe X

#### Scenario: Dia 1 sem alerta
- **WHEN** a partida começa
- **THEN** o Dia 1 nunca tem alerta de erupção

### Requirement: Força do alerta
Cada alerta SHALL ser mostrado como forte ou fraco. Um alerta com erupção real SHALL ser forte com probabilidade de 0,8 no Cadete e 0,7 no Comandante. Um alarme falso SHALL ser forte com probabilidade de 0,2 no Cadete e 0,3 no Comandante.

#### Scenario: Alerta no Boletim e na distribuição
- **WHEN** o dia tem alerta
- **THEN** o Boletim e a tela de distribuição mostram "Alerta FORTE" ou "Alerta fraco", e o Boletim lembra que ligar o Abrigo protege, mas custa energia

#### Scenario: Dia sem alerta
- **WHEN** o dia não tem alerta
- **THEN** o Boletim mostra "Sol calmo hoje"

### Requirement: Dose de radiação pela classe real
Quando a erupção do dia acontece, a dose SHALL ser 1, 3 ou 5 para intensidade fraca, média ou forte. Com 2 células no Abrigo, a dose SHALL ser 0. Com 1 célula, metade da dose, arredondada para cima. Sem células, a dose inteira. A Radiação SHALL ter o máximo de 10.

#### Scenario: Abrigo com meia energia contra erupção X
- **WHEN** uma erupção forte acontece com 1 célula no Abrigo
- **THEN** a Radiação sobe 3

#### Scenario: Abrigo ligado
- **WHEN** uma erupção de qualquer classe acontece com 2 células no Abrigo
- **THEN** a Radiação não sobe, e o resultado diz que o Abrigo protegeu a tripulação

### Requirement: Astronauta fora da base durante a erupção
Se a tarefa do dia levou um astronauta para fora da base e a erupção acontece nesse dia, a Radiação SHALL subir 2 a mais, mesmo com o Abrigo ligado.

#### Scenario: Coleta de pedras em dia de erupção
- **GIVEN** o jogador saiu para coletar pedras em um dia com erupção real
- **WHEN** o dia é resolvido com 2 células no Abrigo
- **THEN** a Radiação sobe 2, e o resultado explica que o astronauta estava fora do Abrigo

#### Scenario: Aviso antes de sair
- **WHEN** o dia tem alerta e a tarefa é de fora da base
- **THEN** a tela de escolha da tarefa avisa que quem sai fica sem a proteção do Abrigo

### Requirement: Alarme falso também custa
Quando o alerta do dia é alarme falso, nenhuma radiação SHALL ser aplicada. O resultado SHALL dizer que o alerta não se confirmou e, se o Abrigo recebeu células, que essas células custaram sem fazer falta.

#### Scenario: Abrigo ligado à toa
- **WHEN** um alarme falso é resolvido com 2 células no Abrigo
- **THEN** a Radiação não muda, e o resultado diz que as 2 células do Abrigo não fizeram falta, mas custaram

### Requirement: Cartão "Isso aconteceu de verdade"
No resultado de um dia com erupção real, o jogo SHALL mostrar um cartão com a classe, a data, a intensidade de jogo e um link para o registro. O cartão SHALL trazer o texto "Fonte: NASA DONKI (CCMC). Dado arquivado, consultado em 13/11/2026. Não é dado ao vivo."

#### Scenario: Cartão após erupção
- **WHEN** o resultado de um dia com a erupção X9.3 de 06/09/2017 aparece
- **THEN** o cartão mostra "Erupção solar classe X9.3", a data por extenso, "forte", a fonte e o link do registro

#### Scenario: Nunca como dado ao vivo
- **WHEN** qualquer tela mostra uma erupção
- **THEN** o texto não usa "ao vivo", "agora" nem "hoje no Sol" para descrevê-la

### Requirement: Créditos e fontes
A tela "Créditos e fontes", aberta pela tela inicial e pelo relatório, SHALL listar cada fonte da NASA cujo fato aparece no jogo, com link e o que se usa dela. Também SHALL explicar quais números são valores de jogo e declarar o uso de IA, e SHALL dizer que o jogo não é produto da NASA nem tem o endosso dela.

#### Scenario: Fontes listadas
- **WHEN** o jogador abre "Créditos e fontes"
- **THEN** a lista inclui o NASA DONKI (CCMC) e todas as fontes citadas nas tarefas que estão no sorteio

#### Scenario: Sem marca da NASA
- **WHEN** qualquer tela do jogo é exibida
- **THEN** não aparece logotipo, bandeira nem identificador de missão da NASA. A NASA só é citada em texto
