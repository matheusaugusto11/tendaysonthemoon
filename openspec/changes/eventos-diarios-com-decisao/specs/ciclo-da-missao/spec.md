# Spec Delta

## MODIFIED Requirements

### Requirement: Missão de 10 dias em fases fixas
A partida SHALL ter 10 dias. Cada dia SHALL passar, nesta ordem, por: Boletim da manhã, deslocamento até a estação da tarefa, tarefa do dia, deslocamento até o console do hub, distribuição de energia, "o dia acontece" e resultado do dia. O evento do dia SHALL interromper uma dessas fases, na ida ou na volta, sempre depois do Boletim e antes da distribuição.

#### Scenario: Ordem das fases em um dia comum
- **WHEN** o jogador confirma o Boletim da manhã
- **THEN** o jogo libera o movimento para levar o astronauta até a estação da tarefa
- **AND** a distribuição de energia só abre depois que a tarefa do dia termina ou o relógio acaba, e depois que o evento do dia foi resolvido

#### Scenario: Passagem para o dia seguinte
- **WHEN** o jogador confirma o resultado de um dia menor que 10 e nenhuma condição de fim foi atingida
- **THEN** o contador avança um dia e o jogo mostra o Boletim da manhã desse dia

#### Scenario: Evento antes da distribuição
- **WHEN** o jogador chega ao console do hub e o evento do dia ainda não abriu
- **THEN** o cartão do evento abre antes da distribuição de energia

### Requirement: Boletim da manhã
O Boletim SHALL mostrar a parte da missão (1: dias 1 a 4, 2: dias 5 a 8, 3: dias 9 e 10), as células de hoje (luz ou sombra), as células guardadas na bateria e o clima espacial do dia. Quando se aplicarem, SHALL mostrar também o vazamento aberto, a forma da tripulação, a falta de contato com a Terra, uma CME a caminho, com o dia da chegada, e as células a menos por um evento de dia anterior.

#### Scenario: Boletim em dia de sombra com alerta
- **WHEN** o dia é de sombra e tem alerta de erupção
- **THEN** o Boletim diz que o Sol ficou atrás de um morro, mostra quantas células os painéis rendem e mostra o alerta, forte ou fraco

#### Scenario: Boletim com consequência de dia anterior
- **WHEN** existe um vazamento aberto, a tripulação pulou o exercício no dia anterior ou um evento anterior tirou células de hoje
- **THEN** o Boletim mostra uma nota explicando a consequência e o custo

#### Scenario: CME a caminho
- **GIVEN** o aviso da CME apareceu no Dia 4
- **WHEN** o Boletim do Dia 5 aparece
- **THEN** ele diz que a nuvem chega no Dia 6 e se o abrigo foi preparado

### Requirement: Relógio de 5 minutos por dia
Cada dia SHALL ter um relógio de 5 minutos, visível na tela. Ele começa quando o jogador sai do Boletim e para quando a distribuição de energia abre. O relógio SHALL ficar parado durante um minijogo, durante o cartão do evento e durante a pausa. Ele SHALL continuar correndo na corrida até o abrigo, e a espera no abrigo SHALL tirar tempo dele.

#### Scenario: Tempo acaba antes da tarefa
- **WHEN** o relógio chega a zero antes de a tarefa do dia ser escolhida ou terminada
- **THEN** a tarefa é resolvida como a escolha "ficar na base", com o custo dessa escolha
- **AND** o jogo avisa que o tempo acabou e, depois do evento do dia, se ainda faltar, abre a distribuição de energia

#### Scenario: Tempo acaba depois da tarefa
- **WHEN** o relógio chega a zero depois de a tarefa terminar, antes de o jogador chegar ao console do hub
- **THEN** a distribuição de energia abre sem nenhum custo extra, depois do evento do dia, se ainda faltar

#### Scenario: Minijogo não consome o relógio do dia
- **WHEN** um minijogo está em andamento
- **THEN** o relógio do dia fica parado até o minijogo terminar

#### Scenario: Cartão do evento não consome o relógio
- **WHEN** o cartão do evento está aberto
- **THEN** o relógio do dia fica parado até o jogador escolher

### Requirement: Tutorial do primeiro dia
Na primeira partida, o jogo SHALL mostrar os slides de "Como jogar" e uma dica do controle da missão em cada fase do Dia 1, inclusive no cartão do evento. Nas partidas seguintes, o tutorial SHALL vir desligado. O jogador SHALL poder ligar ou desligar o tutorial na tela inicial.

#### Scenario: Primeira partida
- **WHEN** o jogador começa a primeira partida com o tutorial ligado
- **THEN** os slides aparecem antes do Boletim do Dia 1, a tarefa do Dia 1 é a coleta de pedras, o evento do Dia 1 é "Poeira lunar nos trajes" e cada fase do Dia 1 mostra a dica correspondente

#### Scenario: Partida seguinte
- **WHEN** o jogador termina o Dia 1 com o tutorial e começa outra partida
- **THEN** o tutorial fica desligado, a não ser que o jogador o ligue de novo

### Requirement: Pausa
Durante o deslocamento, durante a corrida até o abrigo e durante os minijogos, o jogador SHALL poder pausar o jogo com "Continuar" e "Reiniciar". Enquanto o jogo estiver pausado, o relógio do dia, o do minijogo e a contagem da corrida SHALL ficar parados.

#### Scenario: Pausar no deslocamento
- **WHEN** o jogador pausa enquanto anda pela base
- **THEN** o personagem e o relógio do dia ficam parados até o jogador escolher "Continuar"

#### Scenario: Pausar na corrida
- **WHEN** o jogador pausa durante a corrida até o abrigo
- **THEN** a contagem da corrida e o relógio do dia ficam parados até o jogador escolher "Continuar"
