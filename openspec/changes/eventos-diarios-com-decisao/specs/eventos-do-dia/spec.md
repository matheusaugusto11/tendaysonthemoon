# Spec Delta

## Purpose

Dá a cada dia da missão um momento de decisão diante de um evento ligado a um fato real da NASA.
O evento interrompe o dia na base, oferece escolhas com custo visível e muda os medidores na hora
ou nos dias seguintes.

## ADDED Requirements

### Requirement: Um evento por dia
Cada um dos 10 dias SHALL ter exatamente um evento. Nos dias com alerta de erupção, o evento SHALL ser "Partículas do Sol chegando". Nos outros dias, SHALL ser sorteado entre os eventos válidos ainda não usados na partida. Nenhum evento SHALL se repetir na mesma partida, exceto "Partículas do Sol chegando".

#### Scenario: Dia com alerta
- **WHEN** um dia com alerta de erupção começa
- **THEN** o evento desse dia é "Partículas do Sol chegando"

#### Scenario: Sem repetição
- **WHEN** uma partida chega ao fim
- **THEN** nenhum evento que não seja "Partículas do Sol chegando" aparece em mais de um dia

#### Scenario: Partidas seguidas variam
- **WHEN** o jogador começa várias partidas seguidas no mesmo modo
- **THEN** a sequência de eventos dos dias sem alerta pode mudar de uma partida para a outra

### Requirement: Condições de cada evento
Um evento SHALL entrar no sorteio de um dia só se a condição dele for verdadeira no começo desse dia. Se nenhum evento não usado for válido, o jogo SHALL usar um evento já usado e válido.

#### Scenario: Evento só em dia de sombra
- **WHEN** um dia sem alerta e sem sombra é sorteado
- **THEN** "Frio da sombra" não pode ser o evento desse dia

#### Scenario: Nada válido sobrando
- **GIVEN** todos os eventos válidos para o dia já foram usados na partida
- **WHEN** o dia começa
- **THEN** o evento do dia é um dos eventos válidos já usados, e o dia nunca fica sem evento

### Requirement: Momento da interrupção
Cada evento SHALL acontecer "na ida" ou "na volta". Na ida, o evento abre depois de 4 a 12 segundos de deslocamento até a tarefa, ou quando o jogador chega à estação da tarefa, o que vier primeiro. Na volta, o evento abre quando o jogador fecha a tela de tarefa concluída. O evento SHALL sempre abrir antes da distribuição de energia.

#### Scenario: Evento na ida
- **WHEN** o jogador anda 4 a 12 segundos depois do Boletim, num dia com evento na ida
- **THEN** o movimento para, toca o alarme e abre o cartão do evento

#### Scenario: Chegar à estação antes do gatilho
- **WHEN** o jogador aperta a ação na estação da tarefa antes de o evento da ida abrir
- **THEN** o cartão do evento abre primeiro, e a escolha da tarefa vem depois de o evento ser resolvido

#### Scenario: Tempo do dia acaba antes do evento
- **WHEN** o relógio do dia chega a zero antes de o evento abrir
- **THEN** o cartão do evento abre antes da distribuição de energia

### Requirement: Cartão do evento
O cartão SHALL mostrar o título, uma explicação curta para 7 a 12 anos, de 2 a 3 escolhas, cada uma com o custo em ícone e número, e a fonte do fato em texto. O jogador SHALL escolher uma opção para continuar. O cartão não SHALL ter botão de fechar nem de pular.

#### Scenario: Custo visível em cada escolha
- **WHEN** o cartão de qualquer evento aparece
- **THEN** cada escolha mostra o que custa (por exemplo, "⚡ −1 hoje" ou "O₂ −1") e nenhuma escolha aparece sem custo ou sem risco descrito

#### Scenario: Fonte no cartão
- **WHEN** o cartão de um evento aparece
- **THEN** o rodapé cita a fonte da NASA em texto, sem logotipo

### Requirement: Nenhuma escolha sai de graça
Em todo evento, cada escolha SHALL ter um custo ou um risco real em Oxigênio, Radiação, células, ciência, comida, crescimento da estufa, contato ou tempo do dia. Uma escolha que não gasta nada na hora SHALL ter um risco ou uma perda descrita no cartão.

#### Scenario: Escolha sem gasto imediato
- **WHEN** o jogador escolhe "Seguir o trabalho" no tremor lunar
- **THEN** o cartão já tinha mostrado o risco de abrir um vazamento, e esse risco é sorteado no fim do dia

### Requirement: Custo em tempo com o relógio zerado
Quando o evento abre com o relógio do dia em zero, toda escolha que custa tempo SHALL custar 1 célula de hoje no lugar do tempo, e o cartão SHALL mostrar esse custo trocado.

#### Scenario: Esperar no abrigo depois do tempo
- **WHEN** o evento abre com o relógio em zero e a escolha custava 40 segundos
- **THEN** a escolha mostra e cobra "⚡ −1 hoje"

### Requirement: Evento "Partículas do Sol chegando"
Nos dias de alerta, o evento abre na ida, com a luz de Erupção. As escolhas SHALL ser "Correr para o abrigo", que inicia a corrida até o Dormitório, e "Continuar o trabalho", que expõe a tripulação se as partículas vierem. O cartão SHALL repetir se o alerta é forte ou fraco e SHALL avisar que pode ser alarme falso.

#### Scenario: Escolhas do alarme
- **WHEN** o cartão de partículas aparece num dia de alerta fraco
- **THEN** ele mostra "Alerta fraco", avisa que as partículas podem não vir e oferece "Correr para o abrigo (tempo do dia)" e "Continuar o trabalho (☢️ +1 a +3 se vierem)"

### Requirement: Corrida até o abrigo
Ao escolher "Correr para o abrigo", o jogador SHALL andar até o Dormitório antes do fim de uma contagem de 25 segundos (vezes 1,3 no Cadete). Chegar a tempo SHALL zerar a exposição ao alarme e tirar 45 segundos do relógio do dia, o tempo de esperar a tempestade passar. Chegar atrasado SHALL valer metade da exposição, arredondada para cima.

#### Scenario: Chega a tempo
- **WHEN** o jogador entra no raio do Dormitório antes de a contagem acabar
- **THEN** a tela diz que a tripulação está no abrigo esperando a tempestade passar, o relógio do dia perde 45 segundos e o jogo volta à fase em que estava

#### Scenario: Contagem acaba
- **WHEN** a contagem chega a zero antes de o jogador chegar ao Dormitório
- **THEN** o jogo avisa que as partículas chegaram antes, registra metade da exposição e o jogador volta a andar normalmente

#### Scenario: Espera no abrigo custa a tarefa
- **GIVEN** o relógio do dia tinha menos de 45 segundos e a tarefa ainda não tinha sido feita
- **WHEN** o jogador chega ao abrigo a tempo
- **THEN** o relógio zera, e a tarefa conta como "ficar na base", com o custo dessa escolha

### Requirement: Exposição ao alarme
No fim do dia, se as partículas vieram de verdade, a exposição de quem continuou o trabalho SHALL ser 1 para erupção fraca, 1 para média e 2 para forte, mais 1 se a tarefa do dia levou um astronauta para fora da base. Quem chegou ao abrigo a tempo SHALL ter exposição 0. Num alarme falso, a exposição SHALL ser 0 em qualquer escolha.

#### Scenario: Continuou trabalhando e saiu da base
- **GIVEN** a erupção do dia é forte e o jogador escolheu "Continuar o trabalho"
- **WHEN** o jogador faz a coleta de pedras e o dia é resolvido
- **THEN** a exposição do alarme soma 3 à Radiação, além da dose da noite decidida pelo Abrigo

#### Scenario: Correu à toa
- **GIVEN** o alerta do dia é alarme falso e o jogador chegou ao abrigo a tempo
- **WHEN** o dia é resolvido
- **THEN** a Radiação não sobe pelo alarme, e o resultado diz que as partículas não vieram e o tempo de espera custou sem fazer falta

### Requirement: Evento "Nuvem do Sol a caminho" (CME)
Este evento SHALL acontecer só num dia sem alerta que fica 2 dias antes de um alerta com erupção real. Ele mostra uma CME real do DONKI e diz em que dia ela chega. "Preparar o abrigo agora" SHALL custar 2 células hoje e fazer 1 célula no Abrigo bastar para proteção total no dia da chegada. "Deixar para quando chegar" não custa nada hoje.

#### Scenario: Preparar o abrigo
- **GIVEN** no Dia 4 o jogador escolheu "Preparar o abrigo agora" no aviso da CME
- **WHEN** o Dia 6 é resolvido com 1 célula no Abrigo e a erupção vem
- **THEN** a dose da noite é 0

#### Scenario: Chegada vira alerta forte
- **WHEN** o dia da chegada anunciada começa
- **THEN** o Boletim mostra "Alerta FORTE" e diz que é a nuvem anunciada no dia do aviso

### Requirement: Evento "Tremor lunar"
Válido em qualquer dia sem vazamento aberto, na ida ou na volta. "Parar e checar as juntas" SHALL custar 40 segundos do relógio. "Seguir o trabalho" SHALL ter chance de 40% (Cadete) ou 50% (Comandante) de abrir um vazamento no fim do dia, com a regra de vazamento que já existe.

#### Scenario: Tremor abre vazamento
- **GIVEN** o jogador escolheu "Seguir o trabalho" e o sorteio do risco deu certo para o vazamento
- **WHEN** o dia é resolvido
- **THEN** o resultado diz que o tremor soltou uma junta, e o Boletim seguinte mostra o vazamento aberto

### Requirement: Evento "Micrometeorito no painel"
Válido em qualquer dia, na ida ou na volta. "Sair e trocar a placa" SHALL custar O₂ −1 e só SHALL estar disponível com Oxigênio acima de 1. "Deixar riscado" SHALL custar 1 célula de luz amanhã e depois de amanhã.

#### Scenario: Deixar riscado
- **WHEN** o jogador escolhe "Deixar riscado" no Dia 3
- **THEN** os Boletins dos Dias 4 e 5 mostram 1 célula a menos e explicam o painel riscado

### Requirement: Evento "Poeira lunar nos trajes"
Válido em qualquer dia, na volta. "Aspirar os trajes na eclusa" SHALL custar 40 segundos do relógio. "Entrar assim mesmo" SHALL custar O₂ −1 no fim do dia, porque a poeira entope os filtros. No Dia 1 com o tutorial ligado, este SHALL ser o evento do dia.

#### Scenario: Evento do tutorial
- **WHEN** o Dia 1 começa com o tutorial ligado
- **THEN** o evento do dia é "Poeira lunar nos trajes", e o cartão mostra a dica do controle da missão sobre escolhas com custo

### Requirement: Evento "Frio da sombra"
Válido só em dia de sombra, na ida ou na volta. "Ligar os aquecedores" SHALL custar 1 célula hoje. "Agasalhar e economizar" SHALL fazer a estufa perder 1 de crescimento, sem passar de 0.

#### Scenario: Estufa sente o frio
- **WHEN** o jogador escolhe "Agasalhar e economizar" com a estufa em 2 de 4
- **THEN** a estufa fica em 1 de 4 no fim do dia

### Requirement: Evento "Gelo numa cratera escura"
Válido em qualquer dia, na ida ou na volta. "Mandar o rover buscar uma amostra" SHALL custar 2 células hoje e dar +3 Estrelas de ciência. "Anotar e seguir" não custa nada hoje e perde a descoberta, o que o cartão SHALL dizer.

#### Scenario: Rover busca o gelo
- **WHEN** o jogador manda o rover
- **THEN** o total de células de hoje cai 2 e as Estrelas sobem 3 no fim do dia

### Requirement: Evento "A Terra vai se esconder"
Válido em qualquer dia com contato com a Terra, na ida ou na volta. "Usar o satélite de retransmissão" SHALL custar 1 célula hoje. "Ficar sem contato hoje" SHALL tirar a dica do controle da missão só na distribuição de hoje.

#### Scenario: Sem contato só hoje
- **WHEN** o jogador escolhe "Ficar sem contato hoje"
- **THEN** a distribuição de hoje mostra a mensagem de sem contato no lugar da dica, e a do dia seguinte volta a mostrar a dica

### Requirement: Evento "Ninguém dormiu direito"
Válido em qualquer dia em que a tarefa tem minijogo, sempre na ida. "Dar uma folga de manhã" SHALL custar 60 segundos do relógio. "Trabalhar assim mesmo" SHALL dar 25% menos tempo ao minijogo de hoje.

#### Scenario: Minijogo com sono
- **WHEN** o jogador escolhe "Trabalhar assim mesmo" e depois faz a tarefa
- **THEN** o limite de tempo do minijogo de hoje é 75% do normal do modo

### Requirement: Evento "Saudade de casa"
Válido em qualquer dia com contato com a Terra, na ida ou na volta. "Fazer uma videochamada com a família" SHALL custar 1 célula hoje. "Seguir trabalhando" SHALL custar 1 célula de luz amanhã, porque a tripulação fica desanimada.

#### Scenario: Seguir trabalhando
- **WHEN** o jogador escolhe "Seguir trabalhando"
- **THEN** o Boletim do dia seguinte mostra 1 célula a menos e explica o desânimo da tripulação

### Requirement: Evento "Fungo na estufa"
Válido quando a estufa tem 2 ou mais de crescimento, na ida ou na volta. "Cortar as folhas doentes" SHALL custar 1 de crescimento. "Deixar e torcer" SHALL ter chance de 50% de zerar o crescimento no fim do dia.

#### Scenario: Fungo se espalha
- **GIVEN** o jogador escolheu "Deixar e torcer" com a estufa em 3 de 4
- **WHEN** o sorteio do risco dá errado no fim do dia
- **THEN** a estufa volta a 0 de 4 e o resultado explica que o fungo se espalhou

### Requirement: Fonte de cada evento
Cada evento SHALL citar uma fonte da NASA registrada em `Docs/DADOS-NASA.md`. Um evento cujo fato ainda esteja marcado "a conferir" não SHALL entrar no sorteio da build publicada.

#### Scenario: Fato sem fonte conferida
- **GIVEN** o fato do evento "Saudade de casa" está marcado "a conferir"
- **WHEN** uma partida da build publicada sorteia os eventos
- **THEN** esse evento nunca aparece

### Requirement: Evento no resultado e no relatório
O resultado do dia SHALL ter uma linha com o que a escolha do evento causou, inclusive o resultado de um risco sorteado. O efeito do evento SHALL contar para os "momentos que mais pesaram" do relatório, com uma frase que nomeia a escolha.

#### Scenario: Linha do evento no resultado
- **WHEN** o resultado de um dia com o tremor lunar aparece
- **THEN** há uma linha dizendo se o jogador checou as juntas ou se o tremor abriu, ou não abriu, um vazamento

#### Scenario: Momento no relatório
- **GIVEN** no Dia 5 o jogador continuou o trabalho e as partículas vieram, com Radiação +3
- **WHEN** o relatório aparece
- **THEN** o Dia 5 está entre os momentos, com uma frase sobre ter continuado o trabalho durante o alarme
