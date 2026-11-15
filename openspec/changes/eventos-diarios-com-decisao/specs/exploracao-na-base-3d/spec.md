# Spec Delta

## MODIFIED Requirements

### Requirement: Movimento só nas fases de deslocamento
O astronauta SHALL andar pela base, com teclado, controle ou toque, só no deslocamento até a tarefa, no deslocamento até o hub e na corrida até o abrigo. Nas outras fases, SHALL ficar parado, e os controles de toque SHALL sumir.

#### Scenario: Personagem parado no Boletim
- **WHEN** o Boletim, a escolha da tarefa, um minijogo, o cartão do evento, a distribuição ou o resultado estão na tela
- **THEN** as entradas de movimento não movem o astronauta

#### Scenario: Personagem anda na corrida
- **WHEN** o jogador escolhe "Correr para o abrigo"
- **THEN** o astronauta volta a andar e os controles de toque reaparecem

### Requirement: Seta guia
Durante o deslocamento, uma seta na tela SHALL apontar para o destino atual, com o nome e a distância em metros. O destino é a estação da tarefa antes de resolvê-la e o console do hub depois. Durante a corrida até o abrigo, o destino SHALL ser o Dormitório, com a contagem da corrida ao lado da seta.

#### Scenario: Destino muda depois da tarefa
- **WHEN** a tela de tarefa concluída é fechada
- **THEN** a seta passa a apontar para o console do hub

#### Scenario: Destino na corrida
- **WHEN** a corrida até o abrigo começa
- **THEN** a seta aponta para o Dormitório, mostra "Abrigo" e a contagem em segundos, e nenhuma outra estação aceita interação

#### Scenario: Volta ao destino de antes
- **WHEN** a corrida termina, a tempo ou atrasada
- **THEN** a seta volta a apontar para o destino da fase de antes da corrida

### Requirement: Luz da base conta o dia
A iluminação da base SHALL usar o cenário Dia no Boletim e no deslocamento. Durante o cartão de partículas e a corrida até o abrigo, SHALL usar Erupção. Em "o dia acontece" e no resultado, SHALL usar Erupção quando a erupção do dia acontece, e Noite nos outros casos.

#### Scenario: Dia com erupção
- **WHEN** "o dia acontece" começa em um dia com erupção real
- **THEN** a base troca para a iluminação de Erupção, com as luzes de emergência pulsando

#### Scenario: Alarme de partículas
- **WHEN** o cartão "Partículas do Sol chegando" abre
- **THEN** a base troca para a iluminação de Erupção e volta para Dia quando a corrida termina ou quando o jogador escolhe continuar o trabalho

#### Scenario: Novo dia
- **WHEN** o Boletim do dia seguinte aparece
- **THEN** a iluminação volta para Dia

## ADDED Requirements

### Requirement: Dormitório como abrigo
A estação do Dormitório (a cama) SHALL servir de destino da corrida até o abrigo. Ela SHALL ficar ativa, com marcador e o nome "Abrigo", só durante a corrida. Entrar no raio dela durante a corrida SHALL encerrar a corrida a tempo, sem precisar apertar a ação.

#### Scenario: Chegar ao abrigo
- **WHEN** o astronauta entra no raio do Dormitório durante a corrida
- **THEN** a corrida termina a tempo, sem botão de ação

#### Scenario: Dormitório fora da corrida
- **WHEN** não há corrida em andamento
- **THEN** o Dormitório não mostra marcador nem aceita interação
