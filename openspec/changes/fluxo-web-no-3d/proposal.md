# Proposta: fluxo da versão web dentro da base 3D

## Por quê

Hoje o projeto descreve dois jogos diferentes. O protótipo web (`web/index.html`, publicado em
tendaysonthemoon-coral.vercel.app) segue a decisão de 14/11 do `Docs/DESAFIO.md`: um turno por
dia, uma tarefa por dia, células de energia divididas entre cinco módulos, e radiação decidida
por erupções reais do NASA DONKI. O Unity roda um loop próprio: dia de 5 minutos com várias
estações, Energia, Oxigênio e Comida de 0 a 100, sem distribuição de energia, sem radiação e
com o dado da NASA só como etiqueta. Isso fere duas regras do desafio
(`Docs/REGRAS-PARA-GANHAR.md`): o dado da NASA tem que mexer na jogabilidade, e todo material
tem que estar em inglês. A entrega fecha em 15/11/2026, às 23h59, e nenhuma regra de jogo está
escrita como especificação. Esta mudança fixa o fluxo da web como a regra oficial e leva esse
fluxo para o visual 3D do Unity.

## O que muda

- **BREAKING (regras do Unity):** o loop atual do `GameManager` (dia de 5 minutos com várias
  estações, recursos de 0 a 100, cama, 10 dias com roteiro fixo, tela de "GAME OVER") é
  substituído pelo fluxo da web: Boletim da manhã, uma tarefa com minijogo, distribuição de
  energia no hub, o dia acontece, resultado. Depois do Dia 10, ou se um limite for atingido,
  vem o relatório da missão.
- O relógio de 5 minutos continua: ele conta do Boletim até o jogador chegar ao console do hub.
  Se o tempo acabar antes da tarefa, ela conta como "ficar na base", com o custo dessa escolha.
  Distribuir energia não tem relógio.
- Novos recursos com a escala da web: Oxigênio de 0 a 10, Radiação de 0 a 10, Bateria de 0 a 4,
  e Comida e Estrelas de ciência como contadores secundários.
- Distribuição de energia em células para Ar e água, Estufa, Abrigo, Laboratório, Bateria e
  Conserto (este só aparece com vazamento aberto), com a dica do controle da missão.
- Clima espacial com as 20 erupções reais do catálogo FLR do DONKI. Os alertas são fortes ou
  fracos e podem ser alarme falso, e a classe C, M ou X decide a dose. O cartão "Isso aconteceu
  de verdade" mostra a fonte e o rótulo de dado arquivado. Os eventos CME, SEP e RBE de
  `EventosNASA.json`, presos a dias fixos, saem do jogo.
- Tarefas sorteadas a cada partida, cada uma com as escolhas "fazer" e "ficar", e um custo em
  cada uma. As 6 mecânicas de minijogo que o Unity já tem passam a cobrir 10 tarefas da web, com
  resultado graduado em vez de passou ou falhou. As outras 6 tarefas da web saem do sorteio por
  enquanto.
- Modos Cadete (7 a 9 anos) e Comandante (10 a 12 anos), com os números da tabela `MODES` da web.
- Interface em português e inglês, com botão de troca. O idioma inicial vem do navegador.
- Telas de créditos e fontes e de relatório da missão iguais às da web. "Missão interrompida"
  substitui o "GAME OVER".

## Capacidades

### Novas capacidades

- `ciclo-da-missao`: os 10 dias, as fases de cada dia, o relógio de 5 minutos, os modos de
  dificuldade, as condições de fim e o relatório da missão.
- `distribuicao-de-energia`: as células do dia, os módulos, a dica do controle da missão e as
  regras de resolução do dia (oxigênio, colheita, ciência, radiação, bateria, comida, células
  perdidas).
- `clima-espacial-donki`: o conjunto curado de erupções reais, o sorteio de alertas, os alarmes
  falsos, a dose pela classe, o cartão de evento real e a atribuição das fontes.
- `tarefas-e-minijogos`: o sorteio das tarefas, as escolhas com custo, as consequências que
  duram vários dias (vazamento, sem contato, exercício), os minijogos, a nota graduada e o
  menu de teste.
- `exploracao-na-base-3d`: andar pela Estação-Roda, a estação de cada tarefa, a seta guia, o
  console de energia no hub e a luz da base mostrando o estado do dia.
- `idioma-pt-en`: textos em português e inglês, idioma inicial e botão de troca.

### Capacidades modificadas

Nenhuma. `openspec/specs/` ainda está vazio.

## Impacto

- **Código Unity reescrito:** `GameManager.cs`, `Recursos.cs`, `DayData.cs`, `DiasPadrao.cs`,
  `UIJogo.cs`, `Estacao.cs` (custos e regras), `Minigame.cs` (nota graduada no lugar de
  recomeçar ao falhar), `Minigames.cs` (fábrica por tarefa) e `DonkiDados.cs`.
- **Dados:** novo `Resources/ErupcoesDONKI.json`, com as 20 erupções de `Docs/DADOS-NASA.md`
  §2.3. `EventosNASA.json` deixa de ser usado.
- **Cena:** `Assets/Scenes/Game.unity` mantém o cenário, as portas, as luzes e as estações. A
  cama e o airlock final saem do fluxo.
- **Mantido:** `Movimento`, `Entrada`, controles de toque, `PortasAutomaticas`,
  `BaseLightingController` (agora acionado pelo estado do dia) e os modelos de `Assets/Models`.
- **Testes:** primeiros testes EditMode (Unity Test Framework 1.1.33, já instalado) para as
  regras puras.
- **Fora desta mudança:** as 6 tarefas sem minijogo no Unity (painel, falha, micro, rover, co2,
  treino), a andada automática "Ir sozinho", o Cinemachine e as câmeras por fase do Art Bible.
  O protótipo web não muda.
