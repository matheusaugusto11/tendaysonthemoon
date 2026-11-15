# Proposta: interface e minijogos com cara de jogo

## Por quê

A mudança `fluxo-web-no-3d` levou as regras da web para a Unity, mas a interface ficou no nível de
graybox. É um "tablet" azul-marinho semitransparente centralizado por cima da cena, com a fonte
padrão da Unity, cerca de 20 tamanhos de texto, medidores só em texto e barra, e botões chapados
sem estado de toque. Os minijogos são retângulos lisos numa caixa preta de 75% da tela, sem
relação com o lugar da base onde a tarefa acontece. A cena 3D (Art Bible v3, low-poly estruturado)
já tem identidade, e a interface não conversa com ela. A entrega do desafio fecha em 15/11/2026,
às 23h59. Esta mudança define a direção de interface do jogo e a ordem de trabalho: todas as
interfaces primeiro, depois os minijogos.

O artifact "Wireframes UI" (Docs/05-UI, Etapa U1) foi desenhado para outra versão do jogo
(diorama com câmera fixa, Bip com bandeja, paleta massinha). Por decisão do usuário, ele é
**adaptado**: as regras dele continuam valendo (moldura que não cobre a cena, medidores lidos por
forma, botão principal fixo, falha sem punição, estados, movimento, termos explicados na hora), e
o que é do diorama sai.

## O que muda

- **Sistema visual único da interface**, no lugar das cores e tamanhos soltos de `UIJogo.cs`:
  paleta por papel tirada do Art Bible v3, duas fontes livres (OFL), escala tipográfica fechada,
  painéis opacos com cantos chanfrados e sombra dura (as mesmas "peças" da base), ícones próprios,
  estados de botão e regras de movimento.
- **HUD novo:** dia, Oxigênio, Radiação, Bateria, Ciência e relógio, cada medidor com ícone, forma
  própria e zona de alerta. O vermelho só aparece na zona de alerta.
- **Botão principal fixo** no canto inferior direito em toda tela da missão. O botão de ação do
  deslocamento passa a ocupar o mesmo lugar.
- **Minijogos dentro da cena:** a câmera faz close na estação da tarefa, e o minijogo abre num painel
  à direita, com instrução, relógio, área do gesto, barra de progresso e faixa de resultado. Os 6
  tipos de minijogo ganham arte vetorial no estilo da base, retorno a cada acerto (escala, cor e
  som) e o fim "Não deu desta vez" sem vermelho, sem tremor e sem som de erro.
- **Sons de interface** sintetizados em código: toque, acerto, concluído e "não deu" (mais grave).
- **Telas da missão no novo estilo:** Boletim, Escolha, Tarefa concluída, Distribuição, O dia
  acontece, Resultado, Relatório, Início, Créditos e Pausa usam o tema. As linhas de antes e depois
  ganham ícone e variação por forma (▲ ▼ =).
- **Painel lateral e páginas:** as telas curtas passam para um painel à direita, com a cena
  visível à esquerda. O Relatório vira uma página opaca. A tela Início mostra a base ao fundo. A
  Pausa vira um cartão central com "Som".
- **Ordem de trabalho:** todas as interfaces vêm antes dos minijogos, cada etapa em commit
  próprio. Uma build só com as interfaces novas é publicada antes dos minijogos e vira a entrega
  se eles não ficarem prontos a tempo. A mão fantasma do Dia 1 entra com os minijogos.
- **Depois desta mudança (P2):** troca de `Text` por TextMeshPro, layout de celular em pé e
  cartões de termo científico.
- A direção adaptada vira um artifact novo, "Direção de Interface", que já inclui os minijogos. O
  "Wireframes UI" fica como está, como origem das regras.

Nenhuma regra de jogo muda. Custos, notas, limites de tempo e textos continuam os da
`fluxo-web-no-3d`.

## Capacidades

### Novas capacidades

- `sistema-visual-da-interface`: paleta por papel, tipografia, forma dos painéis e botões, ícones,
  estados de interação, movimento, contraste e alvos de toque. Vale para todas as telas.
- `hud-e-moldura`: a moldura da tela durante a missão (topo, botão principal fixo, painel lateral),
  o HUD com os medidores por forma, a faixa do deslocamento e a pausa.
- `telas-da-missao`: como cada tela da missão e fora dela apresenta a informação (Boletim, Escolha,
  Concluída, Distribuição, O dia acontece, Resultado, Relatório, Início, Créditos, Pausa).
- `apresentacao-dos-minijogos`: câmera na estação, painel do minijogo, retorno visual e sonoro,
  fim do minijogo e a primeira vez de cada um.

### Capacidades modificadas

Nenhuma. `openspec/specs/` ainda está vazio. As specs da `fluxo-web-no-3d` (ciclo-da-missao,
tarefas-e-minijogos, clima-espacial-donki, idioma-pt-en) continuam valendo como estão: esta
mudança só acrescenta como a informação aparece e não altera nenhum requisito delas.

## Impacto

- **Código:** `UIJogo.cs` (helpers de painel, botão, texto, HUD, telas), `Minigame.cs` e
  `Minigames.cs` (layout e arte das 6 mecânicas), `GameManager.cs` (câmera na entrada e na saída do
  minijogo), `Estacao.cs` (ponto de câmera e esconder o marcador no close), `Movimento.cs` (deixar a
  câmera para outro componente quando pedido).
- **Novos arquivos:** tema da interface, componente de forma vetorial (chanfro, polígono, anel),
  botão com estados, sons sintetizados, câmera de foco e fontes em `Assets/Resources/Fontes/`.
- **Assets:** duas fontes TTF do Google Fonts (OFL 1.1) e um atlas de ícones PNG feito a partir dos
  SVG do artifact, recoloridos com a paleta v3. Nenhuma imagem gerada por IA entra na interface.
- **Build WebGL:** cerca de 300 KB a mais (fontes e ícones). A conferir no tamanho final.
- **Créditos:** as licenças OFL das fontes entram em "Créditos e fontes".
- **Dependência:** parte da `fluxo-web-no-3d` como está (6.1 e 8.x ainda abertas). A tarefa 6.1
  daquela mudança (Início e HUD) é coberta aqui com o visual novo.
- **Fora desta mudança:** Cinemachine, câmeras por fase do Art Bible §7, minijogos em 3D no mundo,
  mudança de regra ou de número de jogo e o protótipo web.
