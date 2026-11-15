# Spec Delta

## MODIFIED Requirements

### Requirement: Sequência de tarefas da missão
O jogo SHALL sortear as quests dos 10 dias no início da partida. A quest 1 de todo dia SHALL ser uma tarefa de rotina, sem repetir em dias seguidos e sem aparecer mais de 2 vezes como quest 1. A quest 2 existe do Dia 2 ao Dia 10. Nos dias 5 a 8, SHALL ser um imprevisto em 2 dias no Cadete e em 3 no Comandante. Nos outros dias, SHALL ser uma pergunta de olhar e pensar.

#### Scenario: Limites do sorteio
- **WHEN** uma partida nova é sorteada
- **THEN** nenhum par de dias seguidos tem a mesma quest 1, e nenhuma tarefa aparece 3 vezes como quest 1

#### Scenario: Imprevistos na quest 2
- **WHEN** uma partida nova é sorteada no modo Comandante
- **THEN** exatamente 3 dias entre 5 e 8 têm vazamento ou antena como quest 2, e a quest 1 desses dias é de rotina

#### Scenario: Sem pergunta disponível
- **WHEN** a quest 2 de um dia deveria ser uma pergunta, mas nenhuma pergunta serve para esse dia
- **THEN** a quest 2 vira uma tarefa de rotina diferente da quest 1 do mesmo dia

#### Scenario: Tutorial fixa o Dia 1
- **WHEN** o tutorial está ligado
- **THEN** a quest 1 do Dia 1 é a coleta de pedras

### Requirement: Fluxo do vazamento de ar
O vazamento SHALL começar direto no minijogo, sem escolha antes. A perda de Oxigênio SHALL depender de quanto tempo o ar escapou, sem deixar o Oxigênio abaixo de 1. Em seguida, o jogador SHALL escolher entre reforçar agora (custa 2 células hoje) e deixar para depois (abre um vazamento que custa 1 de Oxigênio por dia até o conserto).

#### Scenario: Deixar para depois
- **WHEN** o jogador escolhe deixar o vazamento para depois
- **THEN** o vazamento fica aberto e o módulo Conserto aparece na distribuição dos próximos dias

#### Scenario: Relógio do dia acaba antes do vazamento
- **WHEN** o relógio de 3 minutos acaba antes de o jogador resolver o vazamento
- **THEN** o vazamento é resolvido como "deixar para depois", porque essa é a escolha "ficar" desta tarefa

### Requirement: Tela de tarefa concluída
Depois de cada quest, o jogo SHALL mostrar o efeito em uma frase, com os números, e o cartão "Você sabia?" quando a tarefa tiver um. O botão SHALL levar ao deslocamento até a próxima quest ou, depois da última quest do dia, até o console do hub.

#### Scenario: Depois da quest 1
- **WHEN** a reciclagem de água termina como quest 1 com Oxigênio +1
- **THEN** a tela diz o que mudou e quanto custou, e o botão leva ao deslocamento até a estação da quest 2

#### Scenario: Depois da última quest
- **WHEN** a quest 2 termina
- **THEN** o botão leva ao deslocamento até o console do hub

## ADDED Requirements

### Requirement: Efeitos das duas quests se somam
Os efeitos das duas quests do mesmo dia SHALL se somar: células gastas, células extras, Oxigênio, Estrelas, Comida, crescimento da Estufa e o efeito do exercício para amanhã. Se alguma das quests levar o astronauta para fora da base, ele SHALL contar como fora nesse dia.

#### Scenario: Duas quests que gastam células
- **WHEN** a quest 1 é a reciclagem de água (1 célula) e a quest 2 é a antena consertada com demora (2 células)
- **THEN** a distribuição de hoje tem 3 células a menos

#### Scenario: Bônus de pergunta somado
- **WHEN** a quest 1 é a poeira nos painéis com 2 células extras e a quest 2 é a pergunta dos painéis respondida certo
- **THEN** a distribuição de hoje tem 3 células a mais

### Requirement: Quests do mesmo dia em estações diferentes
As duas quests de um dia SHALL abrir em estações diferentes da base. A pergunta SHALL abrir numa estação ligada ao seu fato e diferente da estação da quest 1.

#### Scenario: Estação ocupada pela quest 1
- **WHEN** a quest 1 abre na Eclusa e a pergunta sorteada prefere a Eclusa
- **THEN** a pergunta abre na próxima estação da lista dela, ou outra pergunta é escolhida
