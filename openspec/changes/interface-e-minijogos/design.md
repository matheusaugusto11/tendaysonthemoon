# Design: interface e minijogos com cara de jogo

## Contexto

O motivo está em `proposal.md`, em "Por quê". Os comportamentos estão nas quatro specs desta
mudança. Este documento cobre a direção e como implementá-la.

Estado atual que molda a abordagem:

- **UI montada em código.** `UIJogo.cs` (1.155 linhas) cria um Canvas uGUI Screen Space Overlay
  (referência 1920 por 1080, match 0,5) com `UnityEngine.UI.Text` e a fonte embutida
  `LegacyRuntime.ttf`. Tudo passa por poucos helpers: `Rot`, `Btn`, `Horizontal`, `Vertical`,
  `Nota`, `Alerta`, `Escolha`, `Chip`, `LinhaDelta` e `Celulas`. As cores são constantes no topo,
  copiadas da web (`#0E1326` de painel, verde como botão principal).
- **Tablet único.** As telas são montadas num `ScrollRect` centralizado de até 1600 de largura,
  sobre um véu escuro de 45%. Os botões ficam dentro do conteúdo que rola.
- **Minijogos.** `Minigame.cs` desenha numa `Area` de 1400 por 520 dentro de uma caixa de 1500 por
  860 com fundo preto de 75%. As 6 mecânicas (`Minigames.cs`) usam só `Image` retangular com cor
  lisa e posições fixas pensadas para essa área.
- **Câmera.** `Movimento.AtualizarCamera` posiciona a câmera atrás do personagem em todo
  `LateUpdate`. Não há Cinemachine. O Timeline 1.7.7 está instalado.
- **Estações.** `Estacao` cria um cilindro e um cubo provisórios (`Base`, `Maquina`), a não ser que
  `semMaquina` esteja ligado, mais um marcador flutuante e um `TextMesh`.
- **Pacotes.** TextMeshPro 3.0.7 está no manifesto, mas não é usado. Não há fontes nem sons de
  interface no projeto. O único áudio é `Assets/Audios/doorOpening.mp3`.
- **Movimento reduzido.** `Navegador` já lê `prefers-reduced-motion` por `DezDias.jslib`.
- **Prazo.** Entrega em 15/11/2026, às 23h59. O protótipo web continua como plano B.

## Metas e não metas

**Metas:**
- Um único lugar com cores, fontes, tamanhos, espaços e tempos. Nenhum valor solto nas telas.
- Mudar a cara do jogo inteiro trocando os helpers, sem reescrever cada tela.
- Minijogos com arte vetorial desenhada em código, sem pipeline de arte nova.
- Cada grupo de tarefas deixa o jogo rodando e é um ponto de commit, para dar para parar entre um
  grupo e outro.

**Não metas:**
- TextMeshPro (P2), layout de celular em pé (P2), Cinemachine, minijogos em 3D no mundo.
- Ilustrações ou renders novos. A arte da interface é forma vetorial mais ícones.
- Qualquer mudança de regra, número, texto de jogo ou fluxo de fases.

## Decisões

### D1. Direção: o painel de bordo da Estação-Roda

O método da direção criativa (contexto, conceito, assinatura e o que fica de fora) aplicado ao jogo:

- **Tensão:** brinquedo para 7 a 12 anos contra engenharia espacial real. A direção equilibra os
  dois: formas simples e grandes, com acabamento de equipamento de verdade.
- **Conceito:** *a interface é feita das mesmas peças da base.* Painéis de aço escuro com bordas
  chanfradas, botões de pintura branca como as paredes dos módulos e a cor de cada sistema numa tira
  de luz, como nos corredores.
- **Assinatura:** a **tira de luz**. Todo painel que fala de um sistema tem uma faixa fina na cor
  dele, no topo, e o canto superior esquerdo chanfrado. O jogador reconhece o sistema pela cor da
  tira antes de ler.
- **Fica de fora:** holograma, neon e brilho (regra 6 do Art Bible: nada futurista); painel
  translúcido sobre a cena; vermelho para falha; emoji; fonte arredondada de massinha (Fredoka, do
  artifact antigo).

*Por quê:* o Art Bible v3 trocou a massinha pelo low-poly estruturado. O artifact antigo tem
regras de interface boas, mas a pele dele é de outra estética. O conceito novo liga cada decisão
visual a algo que já está na cena.

### D2. Tema central (`UiTema`)

Uma classe estática nova guarda todos os valores. `UIJogo` e os minijogos passam a ler só dela.

| Papel | Valor | Origem |
| --- | --- | --- |
| Superfície do painel | `#2E323B` (aço escuro), face clara `#3A3F4A` | Art Bible §6 |
| Sombra dura | `#1A1C22`, deslocada 6 para baixo, sem desfoque | — |
| Página opaca (Relatório) | `#05060C` | Céu do Art Bible |
| Botão principal | face `#E6E4DF` (pintura branca), texto `#14161C` | Art Bible §6 |
| Texto | tinta `#E6E4DF`, apoio `#A9ADB8`; discreto `#949AA7` só em controle desativado, sobre `#26282F` | ajustado na 2.1 para cumprir 4,5:1 |
| Ar e água / O₂ | `#4DD0E1` | Art Bible e web |
| Estufa | `#6BD425` | idem |
| Abrigo / Radiação | `#C77DFF` | idem |
| Laboratório | `#FF9F43` | idem |
| Energia e Bateria | `#FFD23F` | idem |
| Alerta | `#FF5D5D` | web |
| "Não deu" | `#52638A` (azul-Terra) | artifact (`#5C6E8C`), escurecido na 2.1: creme sobre ele passou de 4,1:1 para 4,7:1 |

Contrastes previstos: `#E6E4DF` sobre `#2E323B`, cerca de 11:1; `#14161C` sobre `#E6E4DF`, cerca
de 15:1; `#A9ADB8` sobre `#2E323B`, cerca de 6:1. Conferir com uma calculadora na tarefa do tema.

**Tipografia (5 papéis, referência 1920 por 1080):**

| Papel | Fonte | Tamanho |
| --- | --- | --- |
| Título de tela | Barlow Semi Condensed Bold | 60 |
| Título de painel, botão | Barlow Semi Condensed SemiBold | 40 |
| Número de medidor | Barlow Semi Condensed Bold | 36 |
| Corpo, falas, cartões | Nunito Bold | 30 |
| Rótulo (caixa alta) | Barlow Semi Condensed SemiBold | 24 |

**Espaços:** 4, 8, 12, 16, 24, 32, 48 e 64. **Chanfro:** 14 nos painéis e 10 nos botões.
**Tempos:** toque 80 ms, painel 250 ms, medidor 600 ms, faixa de resultado 300 ms.

*Por quê:* hoje há cerca de 20 tamanhos de texto e cores repetidas em vários lugares. Com poucos
valores bem distantes a hierarquia fica clara, e um ajuste de direção vira uma linha. O botão
principal sai do verde, porque o verde é a cor da Estufa.
*Alternativa:* ScriptableObject de tema, descartado por enquanto. A UI é toda em código e não há
Inspector para editar. Vira SO se a UI for para prefabs.

### D3. Fontes: Barlow Semi Condensed e Nunito, ainda em `Text`

Barlow Semi Condensed vem da sinalização de estrada: técnica, condensada e boa para números de
painel. Nunito é arredondada e aberta, legível pequena e para crianças. As duas são OFL 1.1, do
Google Fonts, com Latin-1 completo. Os TTF ficam em `Assets/Resources/Fontes/` e `UiTema` os
carrega por papel (`UiTema.Fonte(Papel)`). `UIJogo` troca `_fonte` e o `FontStyle.Bold` sintético
pelo arquivo do peso certo.

**O que a implementação (2.2) mostrou:**
- O Google Fonts só publica a Nunito variável (`Nunito[wght].ttf`), e o `Font` da Unity não escolhe o peso de uma fonte variável. O `Nunito-Bold.ttf` do projeto é a instância estática com `wght=700`, gerada com fontTools. O OFL da Nunito não reserva nome de fonte, então isso é permitido. Os dois OFL ficam em `Assets/Resources/Fontes/`.
- Nenhuma das fontes tem `▲ ▼ →`, e a Barlow não tem o subscrito `₂`. O Editor completa o glifo com uma fonte do sistema, que **não existe no WebGL**, e por isso o `HasCharacter` engana. Regra: triângulos e setas são formas (`FormaUI`) ou ícones, e todo texto com `₂` usa a Nunito (`UiTema.Fonte(papel, texto)`). Todo caractere não ASCII dos textos atuais (acentos, `·`) existe nas duas famílias.

*Por quê ainda `Text` e não TMP:* a troca de fonte em `Text` é uma mudança só nos helpers e cabe
no prazo. Migrar para TMP mexe em todo helper, em todo minijogo e na geração dos atlas, e fica
como P2.
*Alternativa:* só Nunito (uma família). Funciona, mas perde a leitura de painel técnico dos números
e títulos. Fica como plano B se a Barlow pesar ou não renderizar bem no WebGL.

### D4. Formas vetoriais desenhadas em malha (`FormaUI`)

Um `Graphic` novo (`OnPopulateMesh`) desenha, em qualquer tamanho e sem textura:
- **Chanfrado:** retângulo com cantos cortados (os quatro, ou só o superior esquerdo e o inferior
  direito, que é a assinatura);
- **Polígono:** N lados, rotação e irregularidade por semente (pedras, manchas de regolito);
- **Anel e arco:** com fração preenchida (botão de segurar, relógio);
- **Triângulo:** ponteiro e alerta;
- **Faixa hachurada:** zona de alerta da barra de radiação e limite de carga.

A sombra dura é uma segunda forma, 6 unidades abaixo da face (não o componente `Shadow`, que não acompanha o afundar do botão). Cada forma tem uma franja de 1 unidade com alfa 0 nas bordas, porque o canvas em Overlay não tem MSAA e as diagonais dos chanfros serrilhariam. A faixa hachurada fica sem franja, de propósito.

*Por quê:* o estilo low-poly é feito de faces planas e arestas vivas, que é exatamente o que uma
malha 2D desenha bem. Sai nítido em qualquer resolução, dá para animar a cor e a escala, e não
precisa de arquivo de arte.
*Alternativa:* sprites 9-slice em PNG, descartados porque cada forma nova pediria arte nova.
Sprite gerado em `Texture2D` também foi descartado: serrilha nos chanfros e não faz polígono.

### D5. Ícones em PNG a partir dos SVG do artifact

Os símbolos SVG do artifact "Wireframes UI" (energia, oxigênio, radiação, alerta, módulos,
tarefas, relógio, estrela, globo, pausa, som, fonte, Terra, cadeado, seta) são recoloridos com a
paleta v3, rasterizados em 128 px e salvos em `Assets/Resources/Icones/<nome>.png`. A
rasterização é feita por script no scratchpad, com Chrome headless. `UiTema.Icone(nome)` devolve
o sprite.

*Por quê:* os ícones já existem, desenhados em dois tons com sombra dura, o que conversa com as
faces chanfradas. Só a paleta muda.
*Alternativa:* desenhar os ícones com `FormaUI`, descartada para ícones complexos (frasco, folha,
radiação). A mão fantasma também usa PNG.

### D6. Botões com corpo (`BotaoFisico`)

Um componente novo (`IPointerDownHandler`, `IPointerUpHandler`, `ISelectHandler`) põe a face sobre
a sombra e, ao pressionar, desce a face 6 unidades e esconde a sombra em 80 ms. O som de toque sai
junto. O foco desenha uma moldura tracejada clara por fora. Há três estilos:
- **Principal:** pintura branca com texto escuro e seta à direita;
- **Secundário:** aço claro `#3A3F4A` com texto claro;
- **Escolha:** cartão com a tira na cor do sistema.

Desativado vira cinza-chumbo, com cadeado e o motivo.

**O que a implementação (2.5) mostrou:** o `BotaoFisico` é uma subclasse de `Button`, então mouse, toque e controle passam pelos mesmos eventos e pelo `Entrada.Selecionar` atual. A área de toque é um `Image` invisível com `cullTransparentMesh = false` (sem isso o raio ignora a malha transparente), e a face e a sombra se movem dentro dela, então o layout não pula. A moldura de foco só aparece se a seleção não veio de um ponteiro: clicar com o mouse não a mostra, navegar com teclado ou controle mostra. O texto não usa o `resizeTextForBestFit`, porque ele ignora as tags `<size>` do texto rico e quebra a linha em vez de reduzir: o rótulo escolhe o maior tamanho fixo entre 40, 34, 30 e 24 que cabe numa linha.

*Por quê:* o toque que afunda é o que mais dá "cara de jogo" por linha de código, e responde
igual no mouse, no toque e no controle (o `Entrada.Selecionar` atual continua).

### D7. Moldura: HUD, painel e rodapé fixo

O `_tablet` vira um painel com três modos, escolhidos pela fase:

| Modo | Onde | Fases |
| --- | --- | --- |
| Lateral | Direita, 840 de largura, do HUD até a margem de baixo | Boletim, Escolha, MinijogoInfo, Minijogo, DecisaoVazamento, TarefaConcluida, Distribuir, DiaAcontece, Resultado |
| Página | Tela cheia opaca `#05060C` | Relatório, Créditos, ComoJogar, MenuMinijogos |
| Coluna | Direita, 640 de largura, com a base ao fundo | Início |

O painel tem cabeçalho fixo (tira de luz e título), corpo com rolagem e rodapé fixo, onde o
`BotaoPrincipal(texto, ação)` coloca a ação que avança, sempre no canto inferior direito. Com
`ControlesMobile` ativo, o rodapé e o botão de ação do deslocamento sobem acima do joystick da
câmera. O véu escuro de 45% sai; só a Pausa escurece a cena.

**O que a implementação (grupo 3) mostrou:**
- O rodapé **não fica dentro do painel**. Ele é ancorado no canto inferior direito da tela, e o painel ocupa a faixa livre acima dele. Dentro do painel o botão subia e descia a cada tela, porque a altura do painel muda; assim ele fica no mesmo ponto em toda fase, como a spec pede. O botão principal tem sempre 400 por 104: texto longo quebra em duas linhas em vez de alargar o botão.
- `FormaUI` herda de `MaskableGraphic`, não de `Graphic`. Com `Graphic` o `RectMask2D` da rolagem cortava o texto e os ícones, mas não os fundos de painel, que vazavam por cima do rodapé.
- O painel mantém a posição de rolagem quando a mesma fase é redesenhada (clicar em + ou −, trocar o idioma) e volta ao topo só ao mudar de fase.
- A moldura de foco aparece só depois de teclado ou controle, e some quando o mouse ou o dedo se mexem. Como `Entrada.Selecionar` seleciona botões por código, a seleção sozinha não diz quem está jogando.
- Na Distribuição, e no Resultado de um dia de erupção, o conteúdo ainda passa da altura da tela em alguns casos e rola. O cartão do dado real vem antes do "Por quê?" para a fonte da NASA ficar visível sem rolar. A lista vertical e o painel lateral do grupo 5 resolvem o resto.

**O que a implementação (grupo 5) mostrou:**
- Os três modos saem de `ModoDaFase`. A Página não tem caixa nem tira: é uma tela cheia opaca `#05060C` com o conteúdo centralizado. A Coluna tem 640 de largura, margem de 64 à direita e margens verticais de 24, para a nota de rodapé do Início caber.
- O cabeçalho é fixo. Na Distribuição ele leva o título e a prévia do dia; no Resultado, o título e as três linhas de variação. O resto rola por baixo. A reserva de células ("Células livres: N") fica numa bandeja ao lado do botão principal, no rodapé da tela.
- O painel entra em 250 ms (desliza 40 e aparece, com `CanvasGroup`), com o tempo real, para a pausa não travar a animação. Com movimento reduzido, só aparece, em 200 ms.
- Os cartões de Escolha e Decisão do vazamento ficaram empilhados na vertical (230 de altura com chips, 200 sem). Uma linha de dois cartões ficava estreita demais em 840.
- A dica do tutorial usa o corpo de rótulo (menor), porque o painel lateral é estreito. No Dia 1, Escolha, Resultado e Distribuição ainda rolam um pouco por causa dela.
- A Pausa é um cartão central de 760 por 700 com tira branca. "Reiniciar" abre uma pergunta X com "Voltar" como principal e "Sim, voltar ao início" como secundário. "Som" mostra o estado escrito e grava em `PlayerPrefs` (`tdm_som`).
- Verificado em 1920 por 1080 pelo probe (244 conferências, 0 falhas: modo de cada fase, sem véu, página opaca, pausa, movimento reduzido) e, em 1280 por 720, só por captura: o probe travou na parte dos minijogos nessa resolução (o painel de minijogo ainda é o antigo, do grupo 7), sem relação com o grupo 5.

A Distribuição vira lista vertical (uma linha de cerca de 112 por módulo, até 6 linhas), com a
reserva de células e "Começar o dia" no rodapé.

**Ordem:** os três modos são feitos na etapa de interfaces, antes de qualquer minijogo, primeiro
com a pele nova (D2 a D6) e o rodapé fixo, depois com cada fase no seu modo. O minijogo chega
depois e só reaproveita o modo Lateral. Assim todas as telas ficam prontas e publicáveis mesmo se
os minijogos novos atrasarem.

*Por quê:* "a interface emoldura" é a regra principal do artifact, e no jogo 3D é o que deixa a
base aparecer. O rodapé fixo resolve o botão principal que hoje some na rolagem.

### D8. Câmera de foco na estação (`CameraFoco`)

Um componente novo na câmera principal. `GameManager` chama `CameraFoco.Focar(estacao)` ao entrar
em `MinijogoInfo` ou `Minijogo` e `Soltar()` ao sair de `TarefaConcluida`. O foco começa já
em `Escolha` e `DecisaoVazamento`, para que a escolha, o minijogo e a tarefa concluída fiquem no
mesmo close. Enquanto o foco está ativo, `Movimento` não mexe na
câmera (checa `CameraFoco.Ativo` em `AtualizarCamera`) e guarda yaw e pitch para a volta.

**Enquadramento:** cada `Estacao` ganha um `Transform pontoCamera` opcional. Sem ele, a câmera fica
a 3,2 m da estação, a 1,7 m de altura, do lado do hub, e olha para um ponto deslocado à direita da
estação, para que ela caia no centro da metade esquerda da tela (a metade direita é do painel). A
transição usa `SmoothStep` em 0,6 s na ida e em 0,5 s na volta. No close, o marcador, o rótulo e a
máquina provisória da estação ficam escondidos. Com movimento reduzido, a câmera corta com fade de
0,2 s.

*Por quê:* foi a escolha do usuário (painel mais câmera na estação). Liga o minijogo ao lugar real
da tarefa sem modelo novo e sem Cinemachine.
*Alternativa:* Cinemachine com uma câmera virtual por estação, descartado agora pelo prazo e por ser
pacote novo. Pode substituir o `CameraFoco` depois sem mudar a spec.

**O que a implementação (grupo 5) mostrou:** o `CameraFoco` nasceu no grupo 5, antes dos
minijogos, porque o Resultado e o Início precisam dele. Tem quatro estados (Livre, Indo, Preso,
Voltando) e três chamadas: `Focar(pos, rot, fov)`, `Orbitar(centro, raio, altura, graus por
segundo, fov, deslocamento)` e `Soltar()`. Ele roda depois do `Movimento` (ordem 200), e
`Movimento.AtualizarCamera` sai cedo enquanto `Ativo`. `GameManager.AjustarCamera(fase)` decide:
- **Resultado:** pose aérea, de (0, 110, 52) para (2, 0, 2), campo de visão 60, base deslocada 58 para a esquerda do painel;
- **Início:** órbita de raio 70 e altura 75 em torno de (2, 0, 2), 3 graus por segundo, campo de visão 55;
- **Como jogar, Créditos, Relatório e Menu de minijogos:** a câmera não muda;
- **as outras fases:** `Soltar()`, e o personagem volta ao ângulo de antes.

Falta para o grupo 7 só `Focar(Estacao)`, o `pontoCamera` e o close em Escolha, Decisão do
vazamento e minijogo. Movimento reduzido: a câmera corta, e a órbita do Início fica parada.

**O que a implementação (grupo 7) mostrou:**
- `FocarEstacao(estacao, personagem)` calcula a pose: a câmera fica no `pontoCamera` (só a posição conta) e gira para a direita o bastante para a estação cair no meio da faixa livre à esquerda do painel (`CameraFoco.FracaoLivre`, que a `UIJogo` atualiza com a largura real do painel). Campo de visão do close: 50. Pedir a mesma pose de novo não refaz a transição, então Escolha, cartão, minijogo e Tarefa concluída ficam no mesmo close; a `TarefaConcluida` também chama o foco e as fases seguintes soltam a câmera.
- No close, todo o `Visual` provisório da estação some (marcador, rótulo, base e máquina). Se o personagem tapar a estação, os renderers dele que estavam ligados ficam escondidos e só esses voltam depois (o personagem tem uma cápsula desligada de propósito, que reaparecia se todos fossem religados).
- Os 8 `pontoCamera` são gerados por "Dez Dias/Pontos de câmera das estações" (`EstacoesCamera`): a 3,2 m e 1,7 m de altura, na direção sem parede no caminho nem na linha de visão até a estação, longe do corpo provisório das outras estações e, entre as boas, a mais voltada ao hub. Um filho `PontoCamera_manual` não é sobrescrito. O primeiro gerador só testava o caminho da esfera e pôs a câmera do filtro atrás de um painel; a linha de visão corrigiu.
- A cena não tem tanques nem esteira como objetos separados perto das estações (o interior dos módulos é uma malha só). O close enquadra o ambiente do módulo, como previsto em Riscos.
- Sem estação na cena, o menu de teste usa a vista aérea do Resultado; na missão, a tarefa sem estação abre no console do hub e o close é nele.

### D9. Molde comum do minijogo

`Minigame` passa a montar o painel inteiro (D7, modo Lateral): cabeçalho (ícone e título da
tarefa), instrução, relógio (ícone, número e barra que esvazia), `Area` de 760 por 560, barra de
progresso lida de `Progresso()` a cada frame, e rodapé com "?" e "Sair". Os helpers novos da base
são:
- `Pulso(alvo)`: escala de 1 para 1,08 e de volta, em 120 ms;
- `Flutua(texto, pos)`: texto que sobe e some;
- `Sons.Acerto()`, `Sons.Toque()` e `Sons.Erro()` (este é só o som grave suave);
- `FaixaResultado(nota, concluiu)`: "Ótimo!", "Bom!" ou "Não deu desta vez", no lugar de
  `UI.Mensagem`.

Cada mecânica é reposicionada para a `Area` nova e redesenhada com `FormaUI`:

| Mecânica | Tarefas | Arte | Retorno |
| --- | --- | --- | --- |
| Raspadinha | Poeira, Filtros | Painel solar `#2B5C9E` com grade de células, ou grade de filtro clara; manchas de regolito em polígonos irregulares (6 por 4) | Mancha clareia e some; brilho rápido na célula limpa |
| Esteira | Exercício, Transmissão | Trilho chanfrado; zona alvo com a tira da cor do sistema; ponteiro triangular; pips de acerto | Pip acende com pulso; erro só apaga o pip, sem cor de erro |
| Arrastar | Coleta de pedras, Braço | Chão de regolito; pedras em polígono facetado; caixa de amostras chanfrada com tira laranja; garra | Pedra encaixa com pulso; contador "2 de 4" |
| Canos | Reciclagem de água | Placas de aço chanfradas; canos grossos; tanques com ícone (água usada, limpa) | Peça gira em 120 ms; trecho molhado fica ciano |
| Disjuntores | Estufa, Antena | Quadro com alavancas, LEDs e etiquetas com ícone; barra de carga com faixa hachurada no limite | LED acende; sobrecarga devolve a alavanca, com o limite em amarelo |
| Segurar | Vazamento | Parede com jato de vapor (partículas de `FormaUI`); botão redondo grande com anel de progresso | Anel enche; o vapor diminui com o progresso |

Os alvos de cada área de gesto respeitam o mínimo de 88 (spec).

**O que a implementação (grupo 7) mostrou:**
- O painel do minijogo ocupa o lugar do Lateral (840 de largura, do HUD até a margem de baixo), sem véu. A Area guarda 760 por 560 e só encolhe por escala quando a altura não dá (celular deitado de 20:9: cerca de 0,82, com as peças dos canos em cerca de 107). A instrução usa o papel de rótulo (24, em frase), para caber em duas ou três linhas; com o corpo (30) a Area não cabia em 16:9 com o HUD.
- "?" e "Sair" ficam no rodapé do painel, à direita da barra de progresso. Como a instrução fica sempre visível, o "?" mostra de novo a mão fantasma por 3 s, sem parar o relógio (o cartão de primeira vez diz que o aviso "fica no botão ?").
- O resultado é aplicado na hora em que o minijogo termina (`Tarefas.Fazer`), para a faixa mostrar a nota e o efeito. A faixa fica 1,6 s e só então o jogo segue. "Sair" também passa pela faixa.
- A barra de progresso lê `ProgressoVisivel`, que nos canos é o trecho molhado sem o fator 0,79 e nos disjuntores é a carga liberada para o sistema pedido, para andar de forma contínua sem mostrar a nota de corte. A nota continua a mesma.
- A zona alvo da esteira continua verde, porque a instrução diz "zona verde"; a cor do sistema vai na tira por cima do trilho. Zona com 15% da largura e ponteiro a 0,65 larguras por segundo, como antes.
- Raspadinha: 6 por 4 células de 120, com raio na mesma proporção. O esforço por célula passou de 350 para 640 e passou a ser medido em unidades da Area, não em pixels: numa simulação de vaivém (900 a 2000 por segundo, 3 a 8 s), a nota nova fica a até 4 pontos da antiga (com 350 ficaria até 32 acima). O Segurar usa a mesma fórmula de antes.
- A sobrecarga dos disjuntores devolve a alavanca, deixa a faixa hachurada do limite amarela e mostra "Sobrecarga" por 1,2 s, com o som grave e suave. As cores de arte (painel solar, filtro, regolito, pedra) entraram na `Paleta`.
- A mão fantasma (`DemoGesto`) serve ao cartão de primeira vez (com cenário próprio, numa caixa de 560 por 230) e ao Dia 1 (por cima do minijogo, nos pontos que ele indica). No Dia 1 com tutorial, os toques durante os 3 s não contam e o relógio só começa depois.

*Por quê:* o molde comum garante que todo minijogo tenha a mesma leitura (spec "Painel do
minijogo"). Redesenhar sobre a mesma lógica de nota mantém as regras e os testes da
`fluxo-web-no-3d`.
*Risco ligado:* área menor muda o ritmo de algumas mecânicas. Ver Riscos.

### D10. Sons sintetizados (`SonsUI`)

Os clipes são gerados em código com `AudioClip.Create` (PCM, 44,1 kHz): senoides e triangulares
com envelope curto.

| Som | Forma | Uso |
| --- | --- | --- |
| Toque | 880 Hz, 40 ms | Todo botão |
| Acerto | 660 e depois 990 Hz, 120 ms | Ação certa no minijogo |
| Concluído | Arpejo de 3 notas, 300 ms | Fim com "Bom!" ou "Ótimo!" |
| Não deu | 330 e depois 262 Hz, triangular suave, 300 ms | Fim com "Não deu" |
| Painel | Ruído filtrado de 80 ms, bem baixo | Painel entra |

Um `AudioSource` no objeto da UI. O volume e o mudo ficam em `PlayerPrefs` (`tdm_som`). O botão
"Som" entra na Pausa nova, na etapa de interfaces.

*Por quê:* não há sons no projeto, e sintetizar evita licença e download. No WebGL o áudio só
começa depois do primeiro gesto, o que não pesa aqui, porque todo som vem de um gesto.

### D11. Medidores do HUD

Os widgets são montados com `FormaUI`:
- **Oxigênio:** 10 segmentos chanfrados; os da zona de 3 ou menos têm um traço vermelho fino na
  base;
- **Radiação:** barra com ponteiro triangular e faixa hachurada de 7 a 10;
- **Bateria:** 4 células;
- **Ciência:** ícone e número;
- **Relógio:** ícone e `mm:ss`.

O HUD guarda o último valor mostrado e anima até o novo em 600 ms. Quando o valor muda, aparece a
variação flutuante. `AtualizarHud` continua sendo o ponto de entrada.

**O que a implementação (grupo 4) mostrou:**
- Cada medidor é um componente (`MedidorHud`, em `MedidoresHud.cs`) que anima sozinho no `Update` com o tempo real, então a pausa não congela a animação nem prende uma variação na tela. A variação flutuante (`VariacaoFlutuante`) se apaga sozinha em 1,3 s, e há uma por medidor: a nova substitui a anterior.
- Os medidores saltam direto, sem animar nem soltar variação, na primeira leitura e quando a missão é outra (Jogar de novo). Sem isso, toda partida nova começaria com três variações falsas.
- As zonas de alerta (Oxigênio 3 ou menos, Radiação 7 ou mais) ficam em `Apresentacao`, testadas no assembly de regras. O Oxigênio marca a zona com um traço vermelho sob os três primeiros segmentos; a Radiação, com a faixa hachurada.
- O ícone vermelho de alerta ocupa um lugar reservado, para o HUD não pular quando ele aparece.
- A faixa, a seta guia e o nome do destino usam `UiTema.CorDaTarefa`, a mesma cor da tira dos painéis. O exercício passou do verde para o laranja: a esteira fica no Laboratório. O botão de idioma mostra só o código (EN ou PT), o do idioma para o qual a troca leva.
- A dica do tutorial dizia "siga a seta amarela", e a seta agora tem a cor do módulo. O texto virou "siga a seta", em português e em inglês.
- O botão de ação tem o tamanho do botão principal (400 por 104). Com o joystick da câmera na tela, ele sobe para 16 acima do topo do joystick. Os joysticks aparecem sempre que o personagem pode andar, inclusive no PC, então na prática ele fica nessa altura em todo deslocamento.
- Movimento reduzido (`UiTema.MovimentoReduzido`, de `Navegador.PrefereMenosMovimento` ou forçado pelo editor para teste): os medidores saltam, a variação flutuante aparece sem subir e o painel só aparece. O valor é lido de novo a cada 1 s, para acompanhar a mudança de preferência do sistema.

### D12. Custo em ícones, a partir do texto atual

Os custos de `TarefaDef` são textos como `"O2 -1 a -3 · ciência até +5"`. A tela de escolha divide
o texto em " · " e reconhece o recurso pela palavra inicial nos dois idiomas (O2; energia/energy;
ciência/science; estufa/greenhouse; comida/food; radiação/radiation; bateria/battery). Cada parte
vira um chip com ícone. Se a palavra não for reconhecida, o chip sai só com o texto.

*Por quê:* não mexe nas regras nem nos textos já conferidos com a web.
*Alternativa:* campos estruturados de custo em `Tarefas.cs`, melhor a longo prazo, mas mexe na
tabela de tarefas e nos testes dela. Fica para depois.

**Onde fica a lógica pura:** o leitor de custos, a escolha da faixa pela nota ("Ótimo!" com 100%,
"Bom!" a partir de 50%, senão "Não deu") e a paleta em hex ficam numa classe estática
`Apresentacao`, em `Assets/Scripts/Regras/`. Ela não depende de `UnityEngine`, como o resto do
assembly `DezDias.Regras`. Assim os testes EditMode que já existem podem cobrir essas partes
(contraste dos pares da paleta, leitura de todos os custos nos dois idiomas, limites da faixa).
`UiTema` converte os hex em `Color`.

### D13. Referência visual aprovada

Antes de aplicar o tema às telas, a direção vira um **novo artifact, "Direção de Interface"**, no
formato do documento de direção criativa: conceito, assinatura, o que fica de fora, paleta com
contrastes, as duas fontes aplicadas a textos reais do jogo e os ícones recoloridos. Em seguida
vêm os mocapes sobre capturas reais da cena: as telas (HUD, painel lateral, Distribuição,
Resultado, Relatório, Início e Pausa) e os minijogos (o molde do painel com a câmera na estação e
uma prancha para cada uma das 6 mecânicas). Os minijogos entram já neste artifact, embora sejam
implementados por último, para que a direção seja aprovada uma vez só.
O texto da direção também vai para `Docs/05-UI/DIRECAO-DE-INTERFACE.md`. O artifact "Wireframes
UI" fica como está, citado como origem das regras.

*Por quê:* o método pede a direção aprovada antes da construção. Atualizar o artifact antigo
apagaria os wireframes do diorama, que ainda servem de histórico.

## Riscos e trade-offs

- **[Prazo de cerca de 24 h, com os minijogos por último]** → A ordem é tema, pele das telas, HUD,
  painel lateral e páginas, build das interfaces e só então os minijogos. Cada grupo deixa o jogo
  rodando e é um ponto de commit. A build das interfaces é publicada com os minijogos antigos
  dentro do painel e vira a entrega se os minijogos novos não ficarem prontos a tempo. O site web
  continua como plano B.
- **[Fonte dinâmica de `Text` no WebGL com serrilha ou corte de glifo]** → Testar a Barlow e a
  Nunito na primeira build. Se falhar, ficar só com a Nunito (D3) e, no P2, ir para TMP com atlas
  estático.
- **[Área menor muda a dificuldade dos minijogos]** → Manter a proporção dos alvos (por exemplo,
  a zona da esteira com a mesma fração da largura) e jogar cada mecânica pelo menu de teste nos
  dois modos, comparando com a tabela D3 da `fluxo-web-no-3d`. Ajustar só posição e tamanho, nunca
  a fórmula da nota.
- **[Câmera de foco atravessa parede dentro dos módulos]** → `pontoCamera` manual nas 8 estações,
  conferido no Play. Se ainda atravessar, aproximar a câmera e usar FOV de 50.
- **[Máquina provisória feia no close]** → A máquina provisória fica escondida no close. Onde a
  estação não tem equipamento real por perto, enquadrar o ambiente do módulo (tanques, canteiros,
  racks).
- **[Muitos `Graphic` refazendo a malha a cada frame]** → Só os elementos que mudam chamam
  `SetVerticesDirty`. Medir no WebGL com o profiler. O alvo é manter os 60 fps atuais em 1080p.
- **[Ícones do artifact com a cor errada]** → A recoloração é feita por mapa de cores no script de
  exportação e conferida numa prancha de contato.
- **[Mudança grande em `UIJogo.cs` sem testes de UI]** → Cada grupo termina com um passeio
  completo no Play (Início, um dia inteiro e Relatório) em PT e EN, mais uma build WebGL nos pontos
  marcados.

## Plano de migração

Não há dado salvo para migrar. A chave nova é `tdm_som`. Cada grupo de tarefas é um commit
próprio, e para voltar atrás basta `git revert` do grupo. A build anterior e o protótipo web
continuam publicados até a build nova passar na verificação.

## Perguntas em aberto

- O par de fontes (Barlow Semi Condensed e Nunito) e o botão principal em pintura branca podem ser
  trocados na revisão do artifact "Direção de Interface" sem mudar specs nem tarefas: é só trocar
  arquivo e valor em `UiTema`.
- Os sons sintetizados podem ser trocados depois por sons de um Sound Designer, sem mudar a spec.
