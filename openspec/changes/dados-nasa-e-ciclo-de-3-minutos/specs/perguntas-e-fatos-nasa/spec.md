# Spec Delta

## Purpose

Leva os fatos confirmados da NASA para dentro do dia: perguntas que a criança consegue responder
olhando a tela, um aviso de véspera que muda uma decisão real, e cartões "Você sabia?", sempre
com a fonte à vista.

## ADDED Requirements

### Requirement: Todo fato mostrado tem fonte confirmada
Toda pergunta, aviso e cartão desta capacidade SHALL usar só fatos confirmados em `Docs/DADOS-NASA.md` (de um fato "parcial", só a parte que tem citação) e SHALL mostrar a fonte da NASA na mesma tela. Fato marcado "a conferir" ou sem fonte SHALL ficar fora do jogo. Todo texto SHALL existir em português e em inglês.

#### Scenario: Fonte junto com o fato
- **WHEN** qualquer pergunta, aviso de véspera ou cartão "Você sabia?" aparece
- **THEN** a tela mostra a linha "Fonte: ..." com o nome da página da NASA

#### Scenario: Fato sem fonte fica de fora
- **WHEN** uma tarefa não tem fato confirmado (estufa, reciclagem de água, filtros de ar e vazamento, nesta versão)
- **THEN** a tela de quest concluída dessa tarefa não mostra cartão "Você sabia?"

#### Scenario: Troca de idioma
- **WHEN** o jogador troca para inglês com uma pergunta aberta
- **THEN** o fato, a pergunta, as opções, a explicação e a fonte aparecem em inglês

### Requirement: Pergunta que a criança consegue responder
Cada pergunta SHALL ter exatamente 2 opções, cada uma com desenho e texto curto. A informação que leva à resposta SHALL estar na mesma tela, em desenho ou em texto. A pergunta SHALL pedir comparação (mais ou menos, grosso ou fino, alto ou baixo, perto ou longe) e SHALL NOT pedir número decorado nem nome técnico.

#### Scenario: Resposta visível
- **WHEN** a pergunta "Com o Sol baixinho assim, qual painel pega mais luz?" aparece
- **THEN** a tela mostra o Sol desenhado rente ao chão e os dois painéis, um deitado e um em pé

#### Scenario: Sem pergunta de memória
- **WHEN** a pergunta do atraso de comunicação aparece
- **THEN** ela pergunta se a mensagem de Marte demora mais ou menos, e não quantos segundos ela leva

### Requirement: Acerto dá bônus e erro não tira nada
Acertar uma pergunta SHALL dar +1 Estrela de ciência, a não ser que a pergunta tenha bônus próprio. Errar SHALL NOT mudar nenhum recurso. Nos dois casos, o jogo SHALL mostrar uma explicação de uma frase com a fonte. O bônus SHALL ser aplicado uma vez só.

#### Scenario: Acerto
- **WHEN** o jogador acerta a pergunta da parede contra a radiação
- **THEN** as Estrelas sobem 1 e a tela explica que uma camada grossa de regolito segura mais radiação, com a fonte

#### Scenario: Erro sem punição
- **WHEN** o jogador erra uma pergunta
- **THEN** Oxigênio, Radiação, Bateria, Comida, Estrelas e células de hoje ficam iguais
- **AND** a tela mostra a resposta certa com a explicação, sem som ou texto de punição

### Requirement: Banco de perguntas
O jogo SHALL ter as 7 perguntas da tabela do cenário, cada uma ligada ao seu fato e à sua fonte. Uma pergunta SHALL aparecer no máximo uma vez por partida.

#### Scenario: Perguntas e fontes
- **WHEN** o banco de perguntas é carregado
- **THEN** existem estas perguntas:

| Id | Fato | Fonte | Pergunta | Certa | Bônus |
| --- | --- | --- | --- | --- | --- |
| painel | No polo sul o Sol fica baixinho; a NASA testa painéis em pé, de até 10 m | F2, F6 | Qual painel pega mais luz desse Sol? | em pé | +1 célula hoje |
| regolito | Um estudo da NASA cobre o abrigo com regolito contra a radiação | F10 | Qual parede segura mais a radiação? | grossa de pedra e poeira | +1 Estrela |
| marte | A mensagem leva cerca de 1,3 s da Lua à Terra e até 20 min de Marte | F7, F8 | Marte fica muito mais longe. A mensagem de lá demora mais ou menos? | mais | +1 Estrela |
| classe | As erupções do DONKI têm classe C (fraca), M (média) e X (forte) | F1 | A classe M é mais forte ou mais fraca que a X? | mais fraca | +1 Estrela |
| sombra | As sombras no polo sul ficam de 25 a 50 vezes maiores que as coisas | F4 | Por que a sombra do astronauta ficou tão comprida? | o Sol está baixinho | +1 Estrela |
| lado | A base fica no lado da Lua virado para a Terra, para o rádio funcionar | F5 | Para falar com a Terra, a base fica de que lado da Lua? | o lado virado para a Terra | +1 Estrela |
| terra | Do polo sul, a Terra aparece perto do horizonte | F3, F4 | Da base, onde aparece a Terra? | perto do chão | +1 Estrela |

#### Scenario: Sem repetição na partida
- **WHEN** uma partida de 10 dias termina
- **THEN** nenhuma pergunta apareceu mais de uma vez

### Requirement: Perguntas ligadas ao momento da missão
Algumas perguntas SHALL aparecer só quando o momento combina com o fato: "sombra" só em dia de sombra, "regolito" só em dia com alerta de erupção, e "classe" só depois de o jogador ter visto um cartão "Isso aconteceu de verdade". As outras podem aparecer em qualquer dia com quest de pergunta.

#### Scenario: Pergunta da sombra
- **WHEN** a quest 2 de um dia de sombra é uma pergunta
- **THEN** a pergunta é "sombra", se ela ainda não apareceu na partida

#### Scenario: Pergunta da classe antes de alguma erupção
- **WHEN** nenhuma erupção real aconteceu ainda na partida
- **THEN** a pergunta "classe" não é sorteada

### Requirement: Aviso da véspera de sombra
No Boletim do dia anterior a um dia de sombra, o jogo SHALL avisar que amanhã o Sol fica atrás do morro e explicar que no polo sul ele nunca sobe mais de 1,5° (fontes F2 e F5). O jogador SHALL escolher entre "Guardar energia na bateria" e "Aproveitar no laboratório". A escolha SHALL só arrumar a distribuição de hoje, que o jogador ainda pode mudar.

#### Scenario: Escolher guardar
- **WHEN** o jogador escolhe "Guardar energia na bateria" no aviso da véspera
- **THEN** a distribuição de hoje abre com o máximo possível de células no módulo Bateria, respeitando o que o modo já põe em Ar e água

#### Scenario: Escolher aproveitar
- **WHEN** o jogador escolhe "Aproveitar no laboratório"
- **THEN** a distribuição de hoje abre com até 3 células no Laboratório, respeitando o que o modo já põe em Ar e água

#### Scenario: Consequência no dia de sombra
- **WHEN** chega o dia de sombra
- **THEN** o Boletim mostra as células de sombra mais as guardadas na bateria, e a escolha da véspera pesa no total

### Requirement: Cartões "Você sabia?" das tarefas
Depois de uma quest com fato confirmado, a tela de quest concluída SHALL mostrar o cartão "Você sabia?" com o fato e a fonte. As tarefas com cartão são: coleta de pedras (céu preto de dia, F4), poeira nos painéis (painéis em pé, F6), transmissão de dados (base no lado da Terra, F5), exercício, braço robótico e antena (atraso de 1,3 s e Terra perto do horizonte, F3, F4, F7 e F8).

#### Scenario: Cartão depois da coleta
- **WHEN** a coleta de pedras termina, com "fazer" ou com "ficar"
- **THEN** o cartão diz que lá fora o céu é preto mesmo de dia, com a fonte NASA Science

#### Scenario: Antena com dois fatos
- **WHEN** a antena é consertada
- **THEN** o cartão mostra o atraso de cerca de 1,3 s e diz que, do polo sul, a Terra aparece perto do horizonte, com as fontes

### Requirement: Créditos com todas as fontes usadas
A tela "Créditos e fontes" SHALL listar todas as fontes usadas pelas perguntas, pelo aviso de véspera e pelos cartões, com o link e o que o jogo usa de cada uma.

#### Scenario: Fontes novas nos créditos
- **WHEN** o jogador abre "Créditos e fontes"
- **THEN** a lista inclui NASA SVS 4893, o artigo da NASA sobre a Artemis Base Camp e o artigo da NASA sobre painéis solares verticais
