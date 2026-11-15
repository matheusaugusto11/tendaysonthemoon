# Spec Delta

## RENAMED Requirements

- FROM: `### Requirement: Relógio de 5 minutos por dia`
- TO: `### Requirement: Relógio de 3 minutos por dia`

- FROM: `### Requirement: Fase "o dia acontece"`
- TO: `### Requirement: Fase "a noite passa"`

## MODIFIED Requirements

### Requirement: Missão de 10 dias em fases fixas
A partida SHALL ter 10 dias. Cada dia SHALL passar, nesta ordem, por: Boletim da manhã, quest 1, quest 2 (a partir do Dia 2), deslocamento até o console do hub, distribuição de energia, deslocamento até a cama, "a noite passa" e resultado do dia. Cada quest SHALL ter o deslocamento até a sua estação e a sua resolução.

#### Scenario: Ordem das fases em um dia comum
- **WHEN** o jogador confirma o Boletim da manhã de um dia entre 2 e 10
- **THEN** o jogo libera o movimento para levar o astronauta até a estação da quest 1
- **AND** depois de a quest 1 terminar, a seta leva até a estação da quest 2
- **AND** a distribuição de energia só abre depois que as duas quests terminam ou o relógio acaba

#### Scenario: Dia 1 com uma quest
- **WHEN** o jogador confirma o Boletim do Dia 1
- **THEN** o dia tem só a quest 1, e depois dela a seta leva ao console do hub

#### Scenario: Dormir fecha o dia
- **WHEN** o jogador confirma a distribuição de energia
- **THEN** o movimento volta, a seta aponta para a cama do dormitório, e o dia só é resolvido quando o jogador interage com a cama

#### Scenario: Passagem para o dia seguinte
- **WHEN** o jogador confirma o resultado de um dia menor que 10 e nenhuma condição de fim foi atingida
- **THEN** o contador avança um dia e o jogo mostra o Boletim da manhã desse dia

### Requirement: Relógio de 3 minutos por dia
Cada dia SHALL ter um relógio de 3 minutos, visível na tela. Ele começa quando o jogador sai do Boletim e para quando o jogador chega ao console depois das quests. O relógio SHALL ficar parado durante um minijogo, uma pergunta, um cartão "Você sabia?" e a pausa. A distribuição de energia, o caminho até a cama e "a noite passa" não têm relógio.

#### Scenario: Tempo acaba com quest aberta
- **WHEN** o relógio chega a zero antes de alguma quest do dia ser escolhida ou terminada
- **THEN** cada quest de tarefa aberta é resolvida como a escolha "ficar", com o custo dessa escolha, e cada quest de pergunta aberta passa sem bônus e sem custo
- **AND** o jogo mostra "Hora de dormir!" e a seta leva ao console do hub

#### Scenario: Tempo acaba depois das quests
- **WHEN** o relógio chega a zero depois de as duas quests terminarem, antes de o jogador chegar ao console do hub
- **THEN** o jogo mostra "Hora de dormir!", sem nenhum custo extra, e a seta continua apontando para o console

#### Scenario: Minijogo e pergunta não consomem o relógio do dia
- **WHEN** um minijogo ou uma pergunta está na tela
- **THEN** o relógio do dia fica parado até a tela fechar

### Requirement: Fase "a noite passa"
Depois que o jogador interage com a cama, a tela SHALL escurecer e o jogo SHALL mostrar uma fase curta, de cerca de 4 segundos, com a base à noite, os módulos que receberam energia e uma frase sobre o que aconteceu durante a noite. O dia SHALL ser resolvido nessa fase. O jogador SHALL poder pular essa fase.

#### Scenario: Pular a noite
- **WHEN** o jogador aperta "Pular" durante "a noite passa"
- **THEN** o resultado do dia aparece na hora

#### Scenario: Erupção durante a noite
- **WHEN** o dia tem erupção real
- **THEN** a frase de "a noite passa" fala da erupção e de como o abrigo segurou ou não

#### Scenario: Movimento reduzido
- **WHEN** o sistema pede movimento reduzido
- **THEN** o jogo vai direto ao resultado do dia, sem o escurecer animado

### Requirement: Relatório da missão
Ao fim da missão, o jogo SHALL mostrar um relatório com o modo, as Estrelas, a Radiação e o Oxigênio finais, um gráfico de Oxigênio, Radiação e Bateria por dia, e até 3 momentos que mais pesaram, em ordem de dia. Ao lado, SHALL vir uma dica para a próxima missão e os botões "Jogar de novo" e "Créditos e fontes".

#### Scenario: Nota da missão completa
- **WHEN** a missão termina completa
- **THEN** a nota é 3 estrelas se as Estrelas de ciência atingem a meta de 3 estrelas do modo (32 no Cadete, 24 no Comandante) com Radiação 3 ou menos
- **AND** é 2 estrelas se atingem a meta de 2 estrelas (18 no Cadete, 16 no Comandante), e 1 estrela nos outros casos

#### Scenario: Missão interrompida sem punição
- **WHEN** a missão é interrompida por oxigênio, radiação ou comida
- **THEN** o título diz em que dia a missão foi interrompida e explica que a tripulação voltou para casa em segurança
- **AND** a dica sugere uma estratégia ligada ao motivo, e nenhuma tela usa o texto "GAME OVER"

### Requirement: Tutorial do primeiro dia
Na primeira partida, o jogo SHALL mostrar os slides de "Como jogar" e uma dica do controle da missão em cada fase do Dia 1, incluindo o caminho até a cama e "a noite passa". Nas partidas seguintes, o tutorial SHALL vir desligado. O jogador SHALL poder ligar ou desligar o tutorial na tela inicial.

#### Scenario: Primeira partida
- **WHEN** o jogador começa a primeira partida com o tutorial ligado
- **THEN** os slides aparecem antes do Boletim do Dia 1, a quest 1 do Dia 1 é a coleta de pedras e cada fase do Dia 1 mostra a dica correspondente

#### Scenario: Dica da cama
- **WHEN** o jogador confirma a distribuição no Dia 1 com o tutorial
- **THEN** a dica explica que a tripulação dorme para o dia terminar e que a noite mostra o que aconteceu

#### Scenario: Partida seguinte
- **WHEN** o jogador termina o Dia 1 com o tutorial e começa outra partida
- **THEN** o tutorial fica desligado, a não ser que o jogador o ligue de novo
