# Design: fluxo da versão web dentro da base 3D

## Contexto

O motivo está em `proposal.md`, em "Por quê". As regras estão nas specs desta mudança. Este
documento cobre só como portar as regras.

Estado atual que molda a abordagem:

- **Fonte das regras:** `web/index.html`, igual byte a byte ao site publicado. As regras estão
  em `newGame`, `pickTasks`, `startDay`, `resolve`, `MODES`, `FLARES` e no `finish` de cada
  minijogo (`MG.*`). Os textos em português e em inglês estão nas chamadas `L(pt,en)`.
- **Unity 2022.3.62f3, URP, WebGL.** A interface é montada em código num Canvas uGUI
  (`UIJogo.cs`, sem prefabs). O `GameManager` mistura máquina de estados, regras e eventos. Os
  minijogos (`Minigame.cs`, `Minigames.cs`) são 6 mecânicas que passam ou falham e recomeçam
  quando falham.
- **Cena `Game.unity`:** tem o cenário, as portas automáticas, as luzes com
  `BaseLightingController` (Dia, Erupção, Noite), o personagem com `Movimento` e as estações
  (`Estacao`), cada id com **duas cópias** na cena (por exemplo, `reciclador` nas linhas 537 e
  19384 do YAML).
- **Ainda não há testes nem asmdef.** O Unity Test Framework 1.1.33 já está instalado.
- **Prazo:** 15/11/2026, às 23h59. O protótipo web já cumpre as regras do desafio e continua
  publicado como plano B.

## Metas e não metas

**Metas:**
- As regras portadas da web com os mesmos números, testáveis fora da cena.
- O mínimo de código novo na cena: reaproveitar `Movimento`, `Entrada`, as portas, as luzes,
  as estações e as 6 mecânicas de minijogo.
- Textos copiados da web em PT/EN, para não reescrever conteúdo que já foi conferido com as
  fontes.

**Não metas:**
- As 6 tarefas sem minijogo no Unity, a andada automática "Ir sozinho", o Cinemachine e as
  câmeras por fase do Art Bible.
- Estados de luz por módulo (0, 1 ou 2 células acendendo cada módulo, Art Bible §5). Fica como
  extra se sobrar tempo.
- Mexer no protótipo web.

## Decisões

### D1. Regras em C# puro, num assembly próprio

As regras vão para `Assets/Scripts/Regras/`, com o asmdef `DezDias.Regras`, sem dependência de
`UnityEngine`. Elas recebem um `System.Random` injetado e a lista de erupções como entrada. O
assembly expõe três peças:
- `Missao`: o estado (dia, O₂, radiação, comida, crescimento, bateria, estrelas, vazamento,
  contato, forma física, dias de sombra, alertas, tarefas e histórico);
- `RegrasMissao`: `NovaPartida(modo, rng, erupcoes, tutorial)`, `IniciarDia()`,
  `CelulasTotais()`, `Previa(alocacao)` e `Resolver(alocacao)`, que devolve um `ResultadoDia`;
- `Tarefas`: a tabela das 10 tarefas, com o efeito de "fazer(nota, tempo)" e de "ficar".

Cada função é tradução direta da função da web com o mesmo nome. O comentário cita a linha de
`web/index.html` de onde veio.

*Por quê:* as regras são o que os juízes avaliam (Validity) e o ponto mais fácil de errar no
porte. Em C# puro dá para testar em EditMode, com a seed fixa, contra casos calculados na mão.
Também separa as regras do `GameManager`, que hoje mistura tudo.
*Alternativas:* manter as regras no `GameManager` foi descartado, porque não dá para testar e o
acoplamento com a interface já existe. ScriptableObjects por dia foram descartados, porque os
dias agora são sorteados e não roteirizados. `DayData` e `DiasPadrao` deixam de existir.
*Por que um asmdef só para as regras:* pôr um asmdef em `Assets/Scripts/` puxaria
`Assets/Scripts/Editor/` para dentro dele. O asmdef isolado em `Regras/` evita isso, e o
Assembly-CSharp o referencia sozinho (Auto Referenced).

### D2. O GameManager vira máquina de fases

O `EstadoJogo` dá lugar a `Fase { Inicio, ComoJogar, Boletim, AteTarefa, Escolha, Minijogo,
DecisaoVazamento, TarefaConcluida, AteHub, Distribuir, DiaAcontece, Resultado, Relatorio,
Creditos, MenuMinijogos }`. O `GameManager` guarda a `Missao`, troca a fase, congela ou libera o
`Movimento` (só `AteTarefa` e `AteHub` andam), avisa a interface e ajusta a luz. A pausa
continua como está, com `Time.timeScale`.

**Relógio do dia:** 300 segundos, que contam em `AteTarefa`, `Escolha`, `DecisaoVazamento`,
`TarefaConcluida` e `AteHub`, e ficam parados em `Minijogo` e na pausa. Ao chegar a zero:
- se a tarefa não foi resolvida, aplica o "ficar" da tarefa (no vazamento, "deixar para
  depois"), mostra "O tempo acabou" e vai para `Distribuir`;
- se a tarefa já foi resolvida, vai direto para `Distribuir`.

### D3. Tarefa, estação e minijogo

Cada tarefa aponta para uma estação que já existe na cena. Mais de uma tarefa pode usar a mesma
estação, porque só uma fica ativa por dia. Falta criar uma estação nova, `console`, no centro
do hub.

| Tarefa | Estação (id) | Lugar | Mecânica atual | Como a nota sai |
| --- | --- | --- | --- | --- |
| Coleta de pedras | `airlock` | Eclusa | `MinigameLixo` (4 pedras, botão VOLTAR) | Estrelas = pedras na caixa, mais 1 se forem 4 (de 0 a 5). O₂ pelo tempo: até ⅓ do limite custa 1, até ⅔ custa 2, depois custa 3. Fim do tempo sem VOLTAR = ar acabou |
| Poeira nos painéis | `airlock` | Eclusa | `MinigameRaspadinha` | Fração limpa |
| Braço robótico | `airlock` | Eclusa | `MinigameLixo` (1 caixa, botão PEGAR) | Pegou em até 8 s × fator do modo: Comida +3. Pegou depois: +2. Não pegou: 0 |
| Estufa | `estufa` | Estufa | `MinigameDisjuntores(Estufa)` | Resolvido em até ⅓ do limite = 100%, até ⅔ = 70%, depois = 40%, sem resolver = 0 |
| Reciclagem de água | `reciclador` | Água | `MinigameCanos` | Resolvido = 100% menos o tempo usado (mínimo 80%). Sem resolver = trecho molhado ÷ caminho |
| Filtros de ar | `filtro` | Energia | `MinigameRaspadinha` | Fração limpa |
| Transmissão de dados | `backup` | Laboratório | `MinigameEsteira` (3 pacotes, errar não desfaz) | Pacotes ÷ 3 |
| Exercício | `esteira` | Laboratório | `MinigameEsteira` (5 acertos) | Acertos ÷ 5 |
| Vazamento | `vazamento` | Hub | `MinigameSegurar` | Selado até ½ do limite: O₂ −0. Selado depois: −1. Sem selar: −2. Mínimo de 1 |
| Antena | `disjuntores` | Energia | `MinigameDisjuntores(Antena)` | Resolvido em até 12 s × fator do modo: custa 1 célula. Senão: custa 2 |

*Por quê:* foi a escolha do usuário (reaproveitar as 6 mecânicas). Os modos Estufa e Antena do
`MinigameDisjuntores` já existem, o que mantém comida e comunicação, dois dos cinco sistemas do
desafio. As tarefas de fora (pedras e poeira) começam na Eclusa, porque andar no terreno
externo não foi testado com colisores e tempo de travessia.
*Alternativa:* o "disjuntores→falha" da pergunta inicial foi trocado. A tarefa de falha só
existe no Comandante e não cobre um sistema que a antena já não cubra. O texto das tarefas
continua o da web, com uma frase adaptada onde a mecânica mudou. Por exemplo, na antena:
"religue a comunicação no quadro sem estourar a carga".

### D4. Minijogo com nota graduada

`Minigame` deixa de usar `Falhar`, que recomeçava o minijogo, e passa a usar
`Terminar(nota 0..1)`. Cada subclasse implementa `Progresso()`, que é usado quando o tempo
acaba ou o jogador sai. O callback passa a ser `Action<ResultadoMinijogo>`, com nota, tempo
usado e se o objetivo foi concluído. O limite vem da tarefa (de 10 a 15 s, como na web)
multiplicado por 1,3 no Cadete. "SAIR (B)" termina com `Progresso()`.

*Por quê:* a spec pede nota graduada, sem desistir de graça. Recomeçar ao falhar deixava a
tarefa sem custo.

### D5. Interface: o UIJogo ganha telas, sem trocar de tecnologia

O Canvas continua montado em código, com os helpers que já existem (`Painel`, `Caixa`,
`Texto`, `Botao`, `Barra`). As telas novas seguem a web, tela por tela:
- **Início:** escolha de modo, "Como jogar", tutorial ligado ou desligado, créditos e idioma.
- **Como jogar:** 5 slides.
- **Boletim.**
- **Escolha:** dois cartões com custo.
- **Concluída:** inclui o "Você sabia?".
- **Distribuição:** 5 ou 6 cartões na cor do módulo, ícones de célula, botões + e −, contador
  de células livres e prévia.
- **O dia acontece.**
- **Resultado:** linhas de antes e depois, "Por quê?" e o cartão DONKI com um botão "Ver o
  registro", que chama `Application.OpenURL` no clique.
- **Relatório:** gráfico de 3 linhas desenhado com segmentos de `Image` girados, momentos e
  dica.
- **Créditos:** ScrollRect.
- **Menu de minijogos.**

O HUD mostra dia, Oxigênio, Radiação, Bateria, Estrelas, relógio, idioma e pausa. A barra de
Comida sai do HUD e fica no Boletim e no relatório, como na web.

*Alternativa:* UI Toolkit ou prefabs foram descartados por causa do prazo, e porque o padrão do
projeto já é o uGUI montado em código.

### D6. Idioma

Uma classe estática `Idioma` guarda `Atual` (Pt ou En) e o evento `Mudou`, e oferece
`L(pt, en)`. Os textos ficam em `Textos.cs`, como pares copiados da web. O idioma inicial vem
de `Application.systemLanguage == SystemLanguage.Portuguese`. Na troca, a interface chama de
novo o `Mostrar(fase)` da fase atual, que reconstrói os textos a partir do estado e por isso não
perde nada. As datas são formatadas com `CultureInfo("pt-BR")` ou `("en-US")` sobre a data
UTC do registro.

### D7. Dados reais

O arquivo novo `Assets/Resources/ErupcoesDONKI.json` segue o formato do `DADOS-NASA.md` §2.2:
`{"records":[{id, eventType, startTime, classLabel, gameIntensity, sourceUrl, retrievedOn}]}`.
Os 20 registros saem da tabela §2.3 e conferem com o array `FLARES` da web. O `GameManager` lê o
arquivo com `JsonUtility` e entrega a lista às regras. `EventosNASA.json` e `DonkiDados.cs`
são removidos. As mensagens deles ligavam CME a poeira e RBE a comunicações sem fonte, o que
fere a regra de "fato real com fonte".

### D8. Luz e persistência

O `GameManager` chama `BaseLightingController.SetPreset` ao entrar em `Boletim` (Dia) e em
`DiaAcontece` (Erupção se a erupção do dia acontece, Noite nos outros casos). Os minijogos já
vistos e o tutorial desligado ficam em `PlayerPrefs` (IndexedDB no WebGL), com as chaves
`tdm_seen` e `tdm_tut`, os mesmos nomes da web. O menu de teste abre quando
`Application.absoluteURL` contém "minigame".

## Riscos e trade-offs

- **[Prazo de cerca de 30 h]** → as tarefas estão em ordem de prioridade. O bloco P0 (regras,
  fases, distribuição, resultado, relatório, PT/EN, créditos) gera um jogo completo e
  submetível. O bloco P1 (tutorial, gráfico, luz, menu de teste) só entra depois de uma build
  que funcione. O site web continua sendo o plano B do "Link to Final Project".
- **[Notas dos minijogos diferentes das da web]** → as regras de recurso usam os números da
  web. Só o cálculo da nota de cada mecânica é novo (tabela D3). Uma partida de teste por modo
  antes da build final.
- **[Estações duplicadas na cena]** → a primeira tarefa confere as duas cópias e mantém uma por
  id, a que está dentro da base montada por `MontarBase`. A busca por id passa a avisar no
  console se achar duas.
- **[`Application.systemLanguage` no WebGL]** → conferir na build. Se não refletir o
  navegador, usar um `.jslib` de uma linha que leia `navigator.language`.
- **[`Application.OpenURL` bloqueado como pop-up]** → chamar no clique (pointer up). Se o
  navegador bloquear mesmo assim, mostrar o link em texto para copiar.
- **[Textos longos em PT/EN não cabem nos cartões]** → usar tamanho de fonte com Best Fit nos
  cartões de escolha e de distribuição, e conferir em 16:9 e na largura de celular.

## Plano de migração

Não há dado salvo de jogador para migrar. A cena continua a mesma, e só as estações e o
`GameManager` mudam. Para voltar atrás, basta `git revert` desta mudança. Enquanto ela não
estiver pronta, o protótipo web segue publicado como versão jogável.

## Perguntas em aberto

- A volta das 6 tarefas sem minijogo (painel, falha, micro, rover, co2, treino) e dos estados
  de luz por módulo fica para uma mudança futura.
- O ajuste fino dos limites de cada minijogo (por exemplo, quantos acertos o exercício pede)
  pode mudar depois da primeira partida de teste, sem mexer nas specs.
