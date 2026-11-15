# Spec Delta

## MODIFIED Requirements

### Requirement: Conjunto curado de erupções reais
O jogo SHALL embutir as 20 erupções do catálogo FLR do NASA DONKI listadas em `Docs/DADOS-NASA.md` §2.3 (5 de classe X, 10 de classe M e 5 de classe C), cada uma com data, classe e link do registro. A intensidade de jogo SHALL ser 0 (fraca) para C, 1 (média) para M e 2 (forte) para X. Quando o DONKI liga a erupção a um evento de partículas (SEP) medido por instrumento, o registro SHALL trazer também o identificador, o horário e o link desse SEP. SEP cujo instrumento é um modelo de previsão (texto que começa com "MODEL:") não SHALL ser usado.

#### Scenario: Registro embutido
- **WHEN** o jogo carrega as erupções
- **THEN** existem 20 registros e cada um tem data, classe, intensidade e um link `https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/<número>/-1`

#### Scenario: Erupção com SEP ligado
- **WHEN** o jogo carrega uma erupção que tem SEP ligado na lista curada
- **THEN** o registro tem o horário do SEP e um link `https://ccmc.gsfc.nasa.gov/DONKI/view/SEP/<número>/-1`

#### Scenario: SEP de modelo fica fora
- **WHEN** o único SEP ligado a uma erupção no DONKI vem de um instrumento "MODEL: ..."
- **THEN** a erupção entra na lista sem SEP ligado

### Requirement: Força do alerta
Cada alerta SHALL ser mostrado como forte ou fraco. Um alerta com erupção real SHALL ser forte com probabilidade de 0,8 no Cadete e 0,7 no Comandante. Um alarme falso SHALL ser forte com probabilidade de 0,2 no Cadete e 0,3 no Comandante. O alerta do dia de chegada de uma CME anunciada SHALL ser sempre forte.

#### Scenario: Alerta no Boletim e na distribuição
- **WHEN** o dia tem alerta
- **THEN** o Boletim e a tela de distribuição mostram "Alerta FORTE" ou "Alerta fraco", e o Boletim avisa que as partículas podem chegar durante o dia e que ligar o Abrigo protege a base à noite, mas custa energia

#### Scenario: Dia sem alerta
- **WHEN** o dia não tem alerta
- **THEN** o Boletim mostra "Sol calmo hoje"

#### Scenario: Chegada de CME anunciada
- **GIVEN** o evento "Nuvem do Sol a caminho" anunciou a chegada para o Dia 7
- **WHEN** o Dia 7 começa
- **THEN** o alerta do Dia 7 é forte, qualquer que tenha sido o sorteio

### Requirement: Dose de radiação pela classe real
Quando a erupção do dia acontece, a dose SHALL ser 1, 3 ou 5 para intensidade fraca, média ou forte. Com 2 células no Abrigo, a dose SHALL ser 0. Com 1 célula, metade da dose, arredondada para cima. Sem células, a dose inteira. No dia de chegada de uma CME para a qual o jogador preparou o abrigo, 1 célula no Abrigo SHALL bastar para dose 0. A Radiação SHALL ter o máximo de 10. A exposição ao alarme de partículas soma-se a esta dose.

#### Scenario: Abrigo com meia energia contra erupção X
- **WHEN** uma erupção forte acontece com 1 célula no Abrigo, sem abrigo preparado
- **THEN** a dose da noite é 3

#### Scenario: Abrigo ligado
- **WHEN** uma erupção de qualquer classe acontece com 2 células no Abrigo
- **THEN** a dose da noite é 0, e o resultado diz que o Abrigo protegeu a tripulação

#### Scenario: Abrigo preparado
- **GIVEN** o jogador preparou o abrigo no aviso da CME
- **WHEN** a erupção do dia de chegada acontece com 1 célula no Abrigo
- **THEN** a dose da noite é 0, e o resultado diz que a preparação de dois dias antes valeu

### Requirement: Cartão "Isso aconteceu de verdade"
No resultado de um dia com erupção real, o jogo SHALL mostrar um cartão com a classe, a data, a intensidade de jogo e um link para o registro. Se a erupção tem SEP ligado, o cartão SHALL dizer que as partículas foram medidas e dar o horário e o link do SEP. No dia do aviso de CME, o cartão SHALL mostrar a data, a velocidade e o link da CME. O cartão SHALL trazer o texto "Fonte: NASA DONKI (CCMC). Dado arquivado, consultado em <data da consulta>. Não é dado ao vivo."

#### Scenario: Cartão após erupção
- **WHEN** o resultado de um dia com a erupção X9.3 de 06/09/2017 aparece
- **THEN** o cartão mostra "Erupção solar classe X9.3", a data por extenso, "forte", a fonte e o link do registro

#### Scenario: Cartão com partículas medidas
- **WHEN** o resultado de um dia com uma erupção que tem SEP ligado aparece
- **THEN** o cartão explica, em frase para criança, que primeiro se viu o clarão da erupção e depois as partículas chegaram, com o horário e o link do SEP

#### Scenario: Nunca como dado ao vivo
- **WHEN** qualquer tela mostra uma erupção, um SEP ou uma CME
- **THEN** o texto não usa "ao vivo", "agora" nem "hoje no Sol" para descrevê-los

### Requirement: Créditos e fontes
A tela "Créditos e fontes", aberta pela tela inicial e pelo relatório, SHALL listar cada fonte da NASA cujo fato aparece no jogo, com link e o que se usa dela. Também SHALL explicar quais números são valores de jogo e declarar o uso de IA, e SHALL dizer que o jogo não é produto da NASA nem tem o endosso dela.

#### Scenario: Fontes listadas
- **WHEN** o jogador abre "Créditos e fontes"
- **THEN** a lista inclui o NASA DONKI (CCMC), com os catálogos FLR, SEP e CME, todas as fontes citadas nas tarefas que estão no sorteio e todas as fontes dos eventos do dia que estão no sorteio

#### Scenario: Sem marca da NASA
- **WHEN** qualquer tela do jogo é exibida
- **THEN** não aparece logotipo, bandeira nem identificador de missão da NASA. A NASA só é citada em texto

## ADDED Requirements

### Requirement: Lista curada de CMEs
O jogo SHALL embutir uma lista curada de 6 a 10 CMEs reais do catálogo CME do NASA DONKI, cada uma com data, velocidade em km/s e link do registro. Quando houver uma CME ligada à erupção do dia de chegada, o aviso SHALL usar essa CME. Sem CME ligada, o aviso SHALL usar outra CME da lista e não SHALL dizer que ela causou a erupção.

#### Scenario: Registro de CME
- **WHEN** o jogo carrega as CMEs
- **THEN** cada registro tem data, velocidade e um link `https://ccmc.gsfc.nasa.gov/DONKI/view/CME/<número>/-1`

#### Scenario: CME sem ligação
- **WHEN** a erupção do dia de chegada não tem CME ligada na lista
- **THEN** o cartão do aviso fala de "uma nuvem como esta" e não afirma que é a mesma erupção

### Requirement: Aviso de risco na escolha da tarefa
Num dia com alerta, a escolha de uma tarefa de fora da base SHALL avisar que quem sai fica sem a proteção do abrigo se as partículas vierem.

#### Scenario: Aviso antes de sair
- **WHEN** o dia tem alerta, o jogador escolheu "Continuar o trabalho" no alarme e a tarefa é de fora da base
- **THEN** a tela de escolha da tarefa avisa que sair aumenta a exposição se as partículas vierem

## REMOVED Requirements

### Requirement: Astronauta fora da base durante a erupção
**Reason**: A exposição de quem está fora do abrigo passa a ser decidida pelo jogador no alarme de partículas (capacidade `eventos-do-dia`, requisito "Exposição ao alarme"), e não mais por uma soma fixa de +2.
**Migration**: Sair da base num dia de erupção, depois de escolher "Continuar o trabalho", soma 1 à exposição do alarme. Quem chegou ao abrigo a tempo não recebe exposição, mesmo fazendo uma tarefa de fora depois.
