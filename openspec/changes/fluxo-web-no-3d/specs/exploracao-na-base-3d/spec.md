# Spec Delta

## Purpose

Leva o fluxo do dia para dentro da Estação-Roda em 3D. O jogador anda pela base até a estação
da tarefa e depois até o console do hub, e a luz da base mostra o que está acontecendo.

## ADDED Requirements

### Requirement: Movimento só nas fases de deslocamento
O astronauta SHALL andar pela base, com teclado, controle ou toque, só no deslocamento até a tarefa e no deslocamento até o hub. Nas outras fases, SHALL ficar parado, e os controles de toque SHALL sumir.

#### Scenario: Personagem parado no Boletim
- **WHEN** o Boletim, a escolha da tarefa, um minijogo, a distribuição ou o resultado estão na tela
- **THEN** as entradas de movimento não movem o astronauta

### Requirement: Estação da tarefa do dia
Cada tarefa SHALL ter uma estação fixa na base, e só a estação da tarefa de hoje SHALL ficar ativa, com marcador e nome. A coleta de pedras, a poeira nos painéis e o braço robótico começam na Eclusa. Ao chegar ao raio da estação, o jogador SHALL ver o botão de ação, que abre a escolha da tarefa.

#### Scenario: Chegar à estação da tarefa
- **WHEN** o astronauta entra no raio da estação da tarefa de hoje
- **THEN** aparece o botão de ação com o nome da tarefa, e apertá-lo (botão, tecla E ou botão A) abre a escolha

#### Scenario: Outras estações apagadas
- **WHEN** a tarefa de hoje é a reciclagem de água
- **THEN** nenhuma outra estação mostra marcador nem aceita interação

### Requirement: Console de energia no hub
A distribuição de energia SHALL abrir no console do centro do hub. O console SHALL aceitar interação só depois que a tarefa do dia foi resolvida.

#### Scenario: Console antes da tarefa
- **WHEN** o astronauta chega ao console do hub antes de resolver a tarefa
- **THEN** o console não abre a distribuição, e a seta continua apontando para a estação da tarefa

#### Scenario: Console depois da tarefa
- **WHEN** a tarefa foi resolvida e o astronauta entra no raio do console
- **THEN** o botão de ação diz "Distribuir energia" e abre a distribuição

### Requirement: Seta guia
Durante o deslocamento, uma seta na tela SHALL apontar para o destino atual, com o nome e a distância em metros. O destino é a estação da tarefa antes de resolvê-la e o console do hub depois.

#### Scenario: Destino muda depois da tarefa
- **WHEN** a tela de tarefa concluída é fechada
- **THEN** a seta passa a apontar para o console do hub

### Requirement: Módulos 3D correspondem aos módulos de energia
Cada módulo de energia SHALL corresponder a um módulo da Estação-Roda e usar a mesma cor: Ar e água ao módulo Água (#4DD0E1), Estufa à Estufa (#6BD425), Abrigo ao Dormitório (#C77DFF), Laboratório ao Laboratório (#FF9F43) e Bateria ao módulo Energia (#FFD23F).

#### Scenario: Cor do módulo na distribuição
- **WHEN** a distribuição de energia aparece
- **THEN** cada cartão de módulo usa a cor de acento do módulo 3D correspondente

### Requirement: Luz da base conta o dia
A iluminação da base SHALL usar o cenário Dia no Boletim e no deslocamento. Em "o dia acontece" e no resultado, SHALL usar Erupção quando a erupção do dia acontece, e Noite nos outros casos.

#### Scenario: Dia com erupção
- **WHEN** "o dia acontece" começa em um dia com erupção real
- **THEN** a base troca para a iluminação de Erupção, com as luzes de emergência pulsando

#### Scenario: Novo dia
- **WHEN** o Boletim do dia seguinte aparece
- **THEN** a iluminação volta para Dia
