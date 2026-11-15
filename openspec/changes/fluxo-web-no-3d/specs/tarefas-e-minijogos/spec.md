# Spec Delta

## Purpose

Define a tarefa de cada dia: quais tarefas existem, como são sorteadas, as duas escolhas com
custo, o efeito de cada uma e como os minijogos dão uma nota graduada que vira consequência.

## ADDED Requirements

### Requirement: Tarefas disponíveis
O sorteio SHALL usar só as tarefas que têm minijogo. As tarefas de rotina são coleta de pedras, poeira nos painéis, estufa, reciclagem de água, transmissão de dados, filtros de ar, exercício e braço robótico. As tarefas de imprevisto são vazamento de ar e antena.

#### Scenario: Tarefa sem minijogo fora do sorteio
- **WHEN** uma partida é sorteada
- **THEN** nenhum dia recebe painel solar, troca de peças, microscópio, rover, filtro de CO₂ ou treino de emergência

### Requirement: Sequência de tarefas da missão
O jogo SHALL sortear uma tarefa por dia para os 10 dias. Os dias 1 a 4 e 9 e 10 recebem tarefas de rotina. Os dias 5 a 8 misturam tarefas: no Cadete, 2 de imprevisto e 2 de rotina; no Comandante, 3 de imprevisto e 1 de rotina. Nenhuma tarefa SHALL repetir em dias seguidos nem aparecer mais de 2 vezes.

#### Scenario: Limites do sorteio
- **WHEN** uma partida nova é sorteada
- **THEN** nenhum par de dias seguidos tem a mesma tarefa, e nenhuma tarefa aparece 3 vezes

#### Scenario: Tutorial fixa o Dia 1
- **WHEN** o tutorial está ligado
- **THEN** a tarefa do Dia 1 é a coleta de pedras

### Requirement: Troca em dia de sombra
Em dia de sombra, a tarefa poeira nos painéis SHALL ser trocada pela tarefa estufa.

#### Scenario: Poeira cai em dia de sombra
- **WHEN** o sorteio põe poeira nos painéis em um dia de sombra
- **THEN** a tarefa desse dia passa a ser a estufa

### Requirement: Escolha com custo visível
Antes de cada tarefa (menos o vazamento), o jogo SHALL mostrar as duas escolhas, "fazer" e "ficar", cada uma com o custo e o ganho em ícones. Nenhuma escolha SHALL ser gratuita: as duas mudam algum recurso, agora ou depois.

#### Scenario: Custos antes de escolher
- **WHEN** a tela de escolha da transmissão de dados aparece
- **THEN** "Transmitir os dados" mostra "⚡ −2 hoje · ⭐ +0 a +4" e "Guardar a energia" mostra "⚡ −0 · ⭐ 0"

#### Scenario: Saída bloqueada sem ar
- **WHEN** o Oxigênio é 1 ou menos e a tarefa é de fora da base (coleta de pedras ou poeira nos painéis)
- **THEN** a escolha "fazer" fica desativada e explica que o oxigênio está baixo demais para sair

### Requirement: Efeito da coleta de pedras
Fazer SHALL custar de 1 a 3 de Oxigênio, conforme o tempo fora, e render de 0 a 5 Estrelas, conforme as pedras trazidas. O astronauta conta como fora da base nesse dia. Se o ar do traje acabar, o custo é o máximo e as pedras se perdem. Ficar não muda nada.

#### Scenario: Ar do traje acaba
- **WHEN** o minijogo termina porque o ar do traje acabou
- **THEN** o Oxigênio cai 3 e as Estrelas não mudam

### Requirement: Efeito da poeira nos painéis
Fazer SHALL custar 1 de Oxigênio e dar de 0 a 2 células extras hoje: 2 se o painel ficar 90% limpo ou mais, 1 se ficar 50% limpo ou mais. O astronauta conta como fora da base. Ficar não muda nada.

#### Scenario: Limpeza completa
- **WHEN** o jogador limpa 95% do painel
- **THEN** o Oxigênio cai 1 e a distribuição de hoje tem 2 células a mais

### Requirement: Efeito da estufa
Fazer SHALL custar 1 célula hoje e somar de 1 a 3 ao crescimento da Estufa: 3 com nota de 85% ou mais, 2 com 55% ou mais, e 1 abaixo disso. Ficar não muda nada.

#### Scenario: Rega média
- **WHEN** a nota do minijogo da estufa é 60%
- **THEN** o crescimento sobe 2 e a distribuição de hoje tem 1 célula a menos

### Requirement: Efeito da reciclagem de água
Fazer SHALL custar 1 célula hoje e dar de 0 a 2 de Oxigênio: 2 com nota de 80% ou mais, 1 com 50% ou mais. Ficar não muda nada.

#### Scenario: Reciclagem bem feita
- **WHEN** a nota do minijogo da água é 85%
- **THEN** o Oxigênio sobe 2, até o máximo de 10

### Requirement: Efeito da transmissão de dados
Fazer SHALL custar 2 células hoje e render Estrelas pelos pacotes enviados: 0, 1 ou 2 pela quantidade, e 4 quando os 3 pacotes chegam. Ficar não muda nada.

#### Scenario: Três pacotes
- **WHEN** o jogador envia os 3 pacotes
- **THEN** as Estrelas sobem 4

### Requirement: Efeito dos filtros de ar
Fazer SHALL custar 1 célula hoje e mudar o Oxigênio em +1 (nota de 85% ou mais), 0 (50% ou mais) ou −1 (abaixo disso). Ficar SHALL custar 1 de Oxigênio. O Oxigênio dessas mudanças nunca fica abaixo de 1.

#### Scenario: Deixar os filtros sujos
- **WHEN** o jogador escolhe ficar com Oxigênio 5
- **THEN** o Oxigênio passa a 4

### Requirement: Efeito do exercício
Fazer SHALL custar 1 célula hoje e mudar a luz de amanhã em +1 (meta cumprida), 0 (60% da meta ou mais) ou −1 (abaixo disso). Ficar SHALL tirar 1 célula da luz de amanhã.

#### Scenario: Pular o exercício
- **WHEN** o jogador pula o exercício
- **THEN** o Boletim de amanhã avisa que a tripulação está mais fraca e a luz de amanhã tem 1 célula a menos

### Requirement: Efeito do braço robótico
Fazer SHALL custar 1 célula hoje e dar Comida: 3 se a caixa for pega rápido, 2 se for pega com atraso, 0 se não for pega. Ficar não muda nada.

#### Scenario: Caixa pega rápido
- **WHEN** o jogador pega a caixa dentro do tempo bom
- **THEN** a Comida sobe 3

### Requirement: Fluxo do vazamento de ar
O vazamento SHALL começar direto no minijogo, sem escolha antes. A perda de Oxigênio SHALL depender de quanto tempo o ar escapou, sem deixar o Oxigênio abaixo de 1. Em seguida, o jogador SHALL escolher entre reforçar agora (custa 2 células hoje) e deixar para depois (abre um vazamento que custa 1 de Oxigênio por dia até o conserto).

#### Scenario: Deixar para depois
- **WHEN** o jogador escolhe deixar o vazamento para depois
- **THEN** o vazamento fica aberto e o módulo Conserto aparece na distribuição dos próximos dias

#### Scenario: Relógio do dia acaba antes do vazamento
- **WHEN** o relógio de 5 minutos acaba antes de o jogador resolver o vazamento
- **THEN** o vazamento é resolvido como "deixar para depois", porque essa é a escolha "ficar" desta tarefa

### Requirement: Fluxo da antena e contato com a Terra
Fazer SHALL custar 1 célula hoje se a comunicação voltar rápido, ou 2 se demorar, e SHALL deixar a base com contato. Ficar SHALL deixar a base sem contato até a antena ser consertada. Ao fazer, o jogo SHALL mostrar o cartão "Você sabia?" sobre o atraso de comunicação, com as fontes.

#### Scenario: Seguir sem contato
- **WHEN** o jogador escolhe seguir sem contato
- **THEN** os Boletins seguintes avisam que não há contato e a distribuição não mostra a prévia

#### Scenario: Atraso de comunicação
- **WHEN** a antena é consertada
- **THEN** o cartão "Você sabia?" diz que uma mensagem leva cerca de 1,3 segundo da Lua à Terra e até 20 minutos de Marte, com as fontes da NASA

### Requirement: Minijogo termina com nota graduada
Todo minijogo SHALL ter limite de tempo e terminar com uma nota de 0 a 100%, sem recomeçar ao falhar. Ao fim do tempo, a nota é a do progresso alcançado, a não ser que a tarefa tenha regra própria (a coleta de pedras tem). Sair do minijogo SHALL contar como terminar com a nota atual, nunca como desistir de graça.

#### Scenario: Tempo do minijogo acaba
- **WHEN** o tempo de um minijogo acaba com metade do objetivo cumprido
- **THEN** o minijogo termina com nota de cerca de 50% e a tarefa aplica o efeito dessa nota

#### Scenario: Sair no meio
- **WHEN** o jogador aperta "Sair" durante um minijogo
- **THEN** a tarefa é resolvida com a nota alcançada até ali, e o custo de "fazer" é cobrado

### Requirement: Mais tempo no Cadete
No modo Cadete, o limite de tempo dos minijogos, e os limites de tempo que decidem a nota, SHALL ser multiplicados por 1,3.

#### Scenario: Antena no Cadete
- **WHEN** o Cadete conserta a antena em 15 segundos
- **THEN** o custo é 1 célula, porque o limite de "rápido" é 12 × 1,3 = 15,6 segundos

### Requirement: Explicação na primeira vez
A primeira vez que o jogador abre cada minijogo, o jogo SHALL mostrar um cartão com o título, a instrução e o botão "Começar". Nas vezes seguintes, a instrução SHALL ficar num botão "?". O jogo SHALL lembrar, entre sessões, quais minijogos já foram vistos.

#### Scenario: Segunda vez no mesmo minijogo
- **WHEN** o jogador abre um minijogo que já viu em outra partida
- **THEN** o minijogo começa sem o cartão de explicação

### Requirement: Tela de tarefa concluída
Depois de cada tarefa, o jogo SHALL mostrar o efeito em uma frase, com os números, e o botão para voltar ao hub e distribuir energia.

#### Scenario: Depois da reciclagem
- **WHEN** a reciclagem de água termina com Oxigênio +1
- **THEN** a tela diz o que mudou e quanto custou, e o botão leva de volta ao deslocamento até o hub

### Requirement: Menu de teste dos minijogos
Quando o endereço da página tem "minigames" ou "minigame", o jogo SHALL abrir um menu com todos os minijogos disponíveis e a escolha de modo. Nesse menu, cada minijogo SHALL poder ser jogado direto, com "Jogar de novo" e "Menu de minijogos" no fim.

#### Scenario: Abrir pelo endereço
- **WHEN** a build WebGL é aberta com `?minigames` no endereço
- **THEN** o menu de teste aparece no lugar da tela inicial
