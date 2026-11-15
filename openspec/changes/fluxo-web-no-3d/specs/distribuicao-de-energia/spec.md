# Spec Delta

## Purpose

Define a decisão central do dia, que é dividir as poucas células de energia entre os módulos da
base, e as regras que transformam essa divisão em mudanças de Oxigênio, Comida, Ciência,
Radiação e Bateria.

## ADDED Requirements

### Requirement: Células disponíveis no dia
As células do dia SHALL ser: o máximo entre 0 e (luz do dia + bônus de hoje − custo de hoje), mais as células da Bateria. A luz do dia SHALL ser o valor de luz ou de sombra do modo, somado ao efeito do exercício do dia anterior (−1, 0 ou +1), e nunca menos que 1.

#### Scenario: Dia de luz com custo de tarefa
- **GIVEN** um dia de luz no modo Cadete, com Bateria 1 e uma tarefa que custou 1 célula
- **WHEN** a distribuição abre
- **THEN** há 6 − 1 + 1 = 6 células disponíveis

#### Scenario: Tripulação fora de forma
- **GIVEN** a tripulação pulou o exercício no dia anterior
- **WHEN** a distribuição abre em um dia de sombra no modo Comandante
- **THEN** a luz do dia é 3 − 1 = 2 células

### Requirement: Módulos e limites
A distribuição SHALL ter estes módulos, com estes limites de células: Ar e água 3, Estufa 2, Abrigo 2, Laboratório 3 e Bateria 4. O módulo Conserto, com limite 2, SHALL aparecer só quando há vazamento aberto.

#### Scenario: Limite do módulo
- **WHEN** o Abrigo já tem 2 células
- **THEN** o botão "+" do Abrigo fica desativado

#### Scenario: Sem células livres
- **WHEN** todas as células do dia já foram colocadas
- **THEN** todos os botões "+" ficam desativados e os botões "−" continuam ativos

#### Scenario: Conserto só com vazamento
- **WHEN** não há vazamento aberto
- **THEN** o módulo Conserto não aparece

### Requirement: Estado inicial da distribuição
Ao abrir a distribuição, as células da Bateria SHALL começar no módulo Bateria. No modo Cadete, o jogo SHALL colocar também até 2 células em Ar e água como sugestão. O jogador pode mudar as duas.

#### Scenario: Sugestão do Cadete
- **GIVEN** o modo Cadete, com 6 células no dia e 1 na Bateria
- **WHEN** a distribuição abre
- **THEN** a Bateria começa com 1 célula, Ar e água começa com 2, e sobram 4 livres

### Requirement: Dica do controle da missão
Enquanto o jogador distribui, o jogo SHALL mostrar uma prévia do resultado: o efeito no Oxigênio, a proteção do Abrigo se houver alerta, a ciência, o crescimento ou a colheita da Estufa, a falta de comida e as células guardadas. Sem contato com a Terra, a prévia SHALL ser trocada por um aviso de "sem previsão".

#### Scenario: Prévia acompanha a escolha
- **WHEN** o jogador muda Ar e água de 2 para 1 célula
- **THEN** a prévia passa de "o oxigênio se mantém" para "o oxigênio cai 1"

#### Scenario: Sem contato com a Terra
- **WHEN** o contato com a Terra foi perdido
- **THEN** a prévia mostra só "Sem contato com a Terra: sem previsão hoje"

#### Scenario: Aviso de células perdidas
- **WHEN** sobram células livres
- **THEN** o jogo avisa quantas células vão se perder se não forem usadas ou guardadas

### Requirement: Oxigênio pelo módulo Ar e água
Na resolução do dia, o Oxigênio SHALL mudar assim: +1 com 3 células em Ar e água, 0 com 2, −1 com 1 e −2 com 0. O resultado SHALL ficar entre 0 e 10.

#### Scenario: Ar e água sem energia
- **WHEN** o dia é resolvido com 0 células em Ar e água
- **THEN** o Oxigênio cai 2, e o resultado explica que Ar e água ficou sem energia

### Requirement: Vazamento aberto
Enquanto houver vazamento aberto, o dia SHALL perder 1 de Oxigênio a mais, a não ser que o Conserto receba 2 células. Com 2 células no Conserto, o vazamento SHALL ser fechado.

#### Scenario: Conserto feito
- **GIVEN** um vazamento aberto
- **WHEN** o dia é resolvido com 2 células no Conserto
- **THEN** o vazamento fecha, o dia não perde o Oxigênio extra e o módulo Conserto some nos dias seguintes

### Requirement: Estufa e colheita
Cada célula na Estufa SHALL somar 1 ao crescimento. Quando o crescimento chega a 4, o jogo SHALL subtrair 4 do crescimento e dar 4 de Comida e 1 de Oxigênio.

#### Scenario: Dia de colheita
- **GIVEN** a Estufa com crescimento 3
- **WHEN** o dia é resolvido com 1 célula na Estufa
- **THEN** há colheita: Comida +4, Oxigênio +1, e o crescimento volta a 0

### Requirement: Ciência pelo Laboratório
Cada célula no Laboratório SHALL render 1 Estrela de ciência no dia.

#### Scenario: Laboratório com 3 células
- **WHEN** o dia é resolvido com 3 células no Laboratório
- **THEN** as Estrelas aumentam 3

### Requirement: Bateria e células perdidas
As células colocadas na Bateria SHALL passar para o dia seguinte, com o máximo de 4. As células que não forem usadas nem guardadas SHALL se perder, e o resultado do dia SHALL dizer quantas foram.

#### Scenario: Células sem uso
- **WHEN** o dia é resolvido com 2 células livres
- **THEN** a Bateria do dia seguinte não recebe essas 2 células, e o resultado diz que 2 células se perderam

### Requirement: Consumo de comida
Na resolução de cada dia, a tripulação SHALL comer 1 de Comida. Se a Comida já estiver em 0, a tripulação SHALL ficar com fome, e isso encerra a missão pelo motivo "comida".

#### Scenario: Comida acabou
- **GIVEN** a Comida em 0 no início da resolução
- **WHEN** o dia é resolvido
- **THEN** a missão é interrompida pelo motivo "comida", a não ser que Oxigênio ou Radiação tenham encerrado antes

### Requirement: Resultado do dia explicado
O resultado do dia SHALL mostrar o valor de antes e o de depois de Oxigênio, Radiação e Bateria. Logo abaixo, uma lista "Por quê?" SHALL ter uma linha curta para cada regra que mudou alguma coisa.

#### Scenario: Leitura do resultado
- **WHEN** o resultado aparece depois de um dia com erupção e Laboratório ligado
- **THEN** a lista "Por quê?" tem uma linha para o Ar e água, uma para o Laboratório e uma para a erupção
