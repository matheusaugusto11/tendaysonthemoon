# Spec Delta

## Purpose

Define a estrutura de uma partida de Dez Dias na Lua: os 10 dias, a ordem das fases de cada dia,
o relógio de 5 minutos, os modos de dificuldade, as condições de fim e o relatório da missão.

## ADDED Requirements

### Requirement: Missão de 10 dias em fases fixas
A partida SHALL ter 10 dias. Cada dia SHALL passar, nesta ordem, por: Boletim da manhã, deslocamento até a estação da tarefa, tarefa do dia, deslocamento até o console do hub, distribuição de energia, "o dia acontece" e resultado do dia.

#### Scenario: Ordem das fases em um dia comum
- **WHEN** o jogador confirma o Boletim da manhã
- **THEN** o jogo libera o movimento para levar o astronauta até a estação da tarefa
- **AND** a distribuição de energia só abre depois que a tarefa do dia termina ou o relógio acaba

#### Scenario: Passagem para o dia seguinte
- **WHEN** o jogador confirma o resultado de um dia menor que 10 e nenhuma condição de fim foi atingida
- **THEN** o contador avança um dia e o jogo mostra o Boletim da manhã desse dia

### Requirement: Modos Cadete e Comandante
Antes do Dia 1, o jogador SHALL escolher o modo: Cadete (7 a 9 anos) ou Comandante (10 a 12 anos). O modo SHALL definir os valores iniciais e os limites da partida, conforme a tabela dos cenários.

#### Scenario: Valores iniciais do Cadete
- **WHEN** o jogador começa no modo Cadete
- **THEN** a partida começa com Oxigênio 9 de 10, Comida 7, Radiação 0, Bateria 1 de 4 e Estrelas 0
- **AND** os dias de luz rendem 6 células, os dias de sombra rendem 4, e há 4 alertas de erupção, 2 deles com erupção real

#### Scenario: Valores iniciais do Comandante
- **WHEN** o jogador começa no modo Comandante
- **THEN** a partida começa com Oxigênio 8 de 10, Comida 6, Radiação 0, Bateria 1 de 4 e Estrelas 0
- **AND** os dias de luz rendem 6 células, os dias de sombra rendem 3, e há 6 alertas de erupção, 3 deles com erupção real

### Requirement: Dias de sombra
No início de cada partida, o jogo SHALL sortear 2 dias de sombra entre os dias 4 e 9, com pelo menos 2 dias de distância entre eles.

#### Scenario: Sorteio dos dias de sombra
- **WHEN** uma partida nova começa
- **THEN** exatamente 2 dias entre 4 e 9 são marcados como sombra, e a diferença entre eles é de 2 ou mais

#### Scenario: Partidas seguidas variam
- **WHEN** o jogador começa várias partidas seguidas
- **THEN** os dias de sombra, os dias de alerta e a sequência de tarefas podem mudar de uma partida para a outra

### Requirement: Boletim da manhã
O Boletim SHALL mostrar a parte da missão (1: dias 1 a 4, 2: dias 5 a 8, 3: dias 9 e 10), as células de hoje (luz ou sombra), as células guardadas na bateria e o clima espacial do dia. Quando se aplicarem, SHALL mostrar também o vazamento aberto, a forma da tripulação e a falta de contato com a Terra.

#### Scenario: Boletim em dia de sombra com alerta
- **WHEN** o dia é de sombra e tem alerta de erupção
- **THEN** o Boletim diz que o Sol ficou atrás de um morro, mostra quantas células os painéis rendem e mostra o alerta, forte ou fraco

#### Scenario: Boletim com consequência de dia anterior
- **WHEN** existe um vazamento aberto ou a tripulação pulou o exercício no dia anterior
- **THEN** o Boletim mostra uma nota explicando a consequência e o custo

### Requirement: Relógio de 5 minutos por dia
Cada dia SHALL ter um relógio de 5 minutos, visível na tela. Ele começa quando o jogador sai do Boletim e para quando a distribuição de energia abre. O relógio SHALL ficar parado durante um minijogo e durante a pausa.

#### Scenario: Tempo acaba antes da tarefa
- **WHEN** o relógio chega a zero antes de a tarefa do dia ser escolhida ou terminada
- **THEN** a tarefa é resolvida como a escolha "ficar na base", com o custo dessa escolha
- **AND** o jogo avisa que o tempo acabou e abre a distribuição de energia

#### Scenario: Tempo acaba depois da tarefa
- **WHEN** o relógio chega a zero depois de a tarefa terminar, antes de o jogador chegar ao console do hub
- **THEN** a distribuição de energia abre sem nenhum custo extra

#### Scenario: Minijogo não consome o relógio do dia
- **WHEN** um minijogo está em andamento
- **THEN** o relógio do dia fica parado até o minijogo terminar

### Requirement: Fase "o dia acontece"
Depois que o jogador confirma a distribuição, o jogo SHALL mostrar uma fase curta, de cerca de 4 segundos, com os módulos que receberam energia e uma frase sobre o que aconteceu. O jogador SHALL poder pular essa fase.

#### Scenario: Pular a fase do dia
- **WHEN** o jogador aperta "Pular" durante "o dia acontece"
- **THEN** o resultado do dia aparece na hora

#### Scenario: Movimento reduzido
- **WHEN** o sistema pede movimento reduzido
- **THEN** o jogo vai direto ao resultado do dia

### Requirement: Condições de fim da missão
Depois de resolver um dia, o jogo SHALL encerrar a missão na primeira condição verdadeira, nesta ordem: Oxigênio em 0, Radiação em 10, Comida acabou, Dia 10 concluído.

#### Scenario: Oxigênio acaba
- **WHEN** o Oxigênio chega a 0 na resolução de um dia
- **THEN** a missão é interrompida pelo motivo "oxigênio", mesmo que outra condição também seja verdadeira

#### Scenario: Missão completa
- **WHEN** o Dia 10 é resolvido sem nenhum limite atingido
- **THEN** a missão termina como completa

### Requirement: Relatório da missão
Ao fim da missão, o jogo SHALL mostrar um relatório com o modo, as Estrelas, a Radiação e o Oxigênio finais, um gráfico de Oxigênio, Radiação e Bateria por dia, e até 3 momentos que mais pesaram, em ordem de dia. Ao lado, SHALL vir uma dica para a próxima missão e os botões "Jogar de novo" e "Créditos e fontes".

#### Scenario: Nota da missão completa
- **WHEN** a missão termina completa
- **THEN** a nota é 3 estrelas se as Estrelas de ciência atingem a meta de 3 estrelas do modo (29 no Cadete, 21 no Comandante) com Radiação 3 ou menos
- **AND** é 2 estrelas se atingem a meta de 2 estrelas (16 no Cadete, 14 no Comandante), e 1 estrela nos outros casos

#### Scenario: Missão interrompida sem punição
- **WHEN** a missão é interrompida por oxigênio, radiação ou comida
- **THEN** o título diz em que dia a missão foi interrompida e explica que a tripulação voltou para casa em segurança
- **AND** a dica sugere uma estratégia ligada ao motivo, e nenhuma tela usa o texto "GAME OVER"

### Requirement: Tutorial do primeiro dia
Na primeira partida, o jogo SHALL mostrar os slides de "Como jogar" e uma dica do controle da missão em cada fase do Dia 1. Nas partidas seguintes, o tutorial SHALL vir desligado. O jogador SHALL poder ligar ou desligar o tutorial na tela inicial.

#### Scenario: Primeira partida
- **WHEN** o jogador começa a primeira partida com o tutorial ligado
- **THEN** os slides aparecem antes do Boletim do Dia 1, a tarefa do Dia 1 é a coleta de pedras e cada fase do Dia 1 mostra a dica correspondente

#### Scenario: Partida seguinte
- **WHEN** o jogador termina o Dia 1 com o tutorial e começa outra partida
- **THEN** o tutorial fica desligado, a não ser que o jogador o ligue de novo

### Requirement: Pausa
Durante o deslocamento e durante os minijogos, o jogador SHALL poder pausar o jogo com "Continuar" e "Reiniciar". Enquanto o jogo estiver pausado, o relógio do dia e o do minijogo SHALL ficar parados.

#### Scenario: Pausar no deslocamento
- **WHEN** o jogador pausa enquanto anda pela base
- **THEN** o personagem e o relógio do dia ficam parados até o jogador escolher "Continuar"
