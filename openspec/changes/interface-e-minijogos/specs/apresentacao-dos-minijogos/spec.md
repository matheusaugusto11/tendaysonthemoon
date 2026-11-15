# Spec Delta

## Purpose

Define como os minijogos aparecem e respondem ao jogador: dentro da cena, em close na estação da
tarefa, com arte no estilo da base, retorno a cada ação e fim sem punição.

## ADDED Requirements

### Requirement: Close na estação da tarefa
Ao abrir o minijogo, a câmera SHALL sair da visão de terceira pessoa e, em até 0,8 s, enquadrar a
estação da tarefa na metade esquerda da tela. O marcador flutuante e o rótulo da estação SHALL ficar
escondidos durante o close. Ao fechar o minijogo, a câmera SHALL voltar suavemente ao personagem.

#### Scenario: Reciclagem de água
- **WHEN** o jogador começa a reciclagem de água no módulo Ar e água
- **THEN** a câmera enquadra os tanques do módulo à esquerda, e o minijogo abre à direita

#### Scenario: Fim do minijogo
- **WHEN** o minijogo termina e a tela de tarefa concluída fecha
- **THEN** a câmera volta para trás do personagem sem corte seco

#### Scenario: Menu de teste
- **WHEN** um minijogo é aberto pelo menu de teste dos minijogos
- **THEN** o minijogo abre no mesmo painel, com a câmera na estação da tarefa ou, sem estação, na vista geral da base

### Requirement: Painel do minijogo
O minijogo SHALL abrir num painel à direita da tela com, de cima para baixo: o ícone e o título da
tarefa, a instrução em uma frase, o relógio, a área do gesto, a barra de progresso e as ações
secundárias ("?" e "Sair"). O painel SHALL NOT cobrir a estação enquadrada.

#### Scenario: Leitura do painel
- **WHEN** o minijogo de disjuntores abre
- **THEN** o título, a instrução, o relógio e a barra de carga aparecem no painel à direita, e o quadro elétrico da estação aparece à esquerda

### Requirement: Arte no estilo da base
Cada um dos 6 tipos de minijogo SHALL ter arte própria feita de formas chapadas e facetadas, nas
cores da paleta, representando o objeto da tarefa (painel solar, filtro, pedras e caixa, canos e
tanques, quadro de disjuntores, esteira ou sinal, parede com vazamento), em vez de retângulos
genéricos.

#### Scenario: Poeira nos painéis
- **WHEN** o minijogo da poeira abre
- **THEN** a área mostra um painel solar azul com manchas de regolito irregulares por cima, e não uma grade de quadrados cinza

### Requirement: Retorno a cada ação
Cada ação certa no minijogo SHALL dar retorno imediato visível e sonoro: o objeto reage com escala ou
cor em até 150 ms e toca um som curto e suave. A barra de progresso SHALL andar de forma contínua,
sem mostrar a nota de corte.

#### Scenario: Acerto no ritmo
- **WHEN** o jogador acerta a zona no exercício
- **THEN** o marcador de acerto acende com um pulso curto, toca o som de acerto e a barra avança

### Requirement: Relógio do minijogo sem pressão
O relógio do minijogo SHALL mostrar o ícone de relógio, o número de segundos e uma barra que esvazia.
Ele SHALL NOT ficar vermelho, pulsar nem tocar som quando faltar pouco tempo.

#### Scenario: Últimos segundos
- **WHEN** faltam 3 segundos no minijogo
- **THEN** o relógio continua na mesma cor e sem som

### Requirement: Fim sem punição
O fim do minijogo SHALL mostrar uma faixa de resultado no painel: "Ótimo!" com nota de 100%, "Bom!"
a partir de 50% e "Não deu desta vez" abaixo de 50%, sempre com a nota e o efeito. "Não deu" SHALL
usar o azul-Terra calmo, sem vermelho, sem tremor e com um som grave e suave, nunca de erro.

#### Scenario: Tempo acabou com pouco progresso
- **WHEN** o tempo da reciclagem de água acaba com 30% do caminho molhado
- **THEN** a faixa azul-Terra mostra "Não deu desta vez" e a nota, toca o som grave suave, e o jogo segue para a tarefa concluída

#### Scenario: Sobrecarga nos disjuntores
- **WHEN** o jogador tenta ligar um disjuntor que passaria do limite de carga
- **THEN** a alavanca volta, a barra de carga mostra a faixa do limite em amarelo e o texto "Sobrecarga", sem vermelho e sem tremor

### Requirement: Primeira vez de cada minijogo
O cartão de primeira vez SHALL mostrar o ícone da tarefa, o título, a instrução e uma ilustração do
gesto (arrastar, tocar no ritmo, segurar, girar ou ligar), com "Começar" como botão principal. A
câmera SHALL já estar no close da estação enquanto o cartão está aberto.

#### Scenario: Primeira vez nos canos
- **WHEN** o jogador abre os canos pela primeira vez
- **THEN** o cartão mostra a ilustração de girar uma peça, e a câmera já mostra os tanques do módulo

### Requirement: Mão fantasma no Dia 1
No Dia 1 com o tutorial ligado, o minijogo SHALL mostrar uma mão fantasma repetindo o gesto por 3
segundos antes de o relógio começar.

#### Scenario: Coleta de pedras no Dia 1
- **WHEN** o primeiro minijogo do Dia 1 abre com o tutorial ligado
- **THEN** a mão fantasma arrasta uma pedra até a caixa duas vezes e só então o relógio começa

### Requirement: Mesmo gesto com mouse, toque e controle
Todo minijogo SHALL funcionar com mouse, toque e controle. Cada área de gesto SHALL ter alvos de pelo
menos 88 unidades no referencial de 1920 por 1080.

#### Scenario: Canos no celular
- **WHEN** o jogador gira as peças dos canos com o dedo
- **THEN** cada peça responde ao toque com pelo menos 88 unidades de lado
