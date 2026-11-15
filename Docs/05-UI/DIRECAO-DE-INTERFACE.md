# Direção de Interface: o painel de bordo da Estação-Roda

Versão 1, 15/11/2026, **aprovada pelo usuário em 15/11/2026** (as cinco decisões da seção 8). A versão visual, com os mocapes de
todas as telas e dos seis minijogos, está no artifact "Direção de Interface"
(https://claude.ai/artifact/TrKcXnL6LiZ7cj7QapM5Nv). Este arquivo é a referência escrita para
agentes. Os valores exatos e as decisões técnicas estão em
`openspec/changes/interface-e-minijogos/design.md` (D1 a D13).

Origem: as regras de interface do artifact "Wireframes UI" (moldura, botão principal fixo, falha
sem punição, estados, movimento) continuam valendo. A estética de massinha e o diorama saem,
porque o Art Bible v3 trocou o estilo para low-poly estruturado.

## 1. Conceito

- **Tensão:** brinquedo para 7 a 12 anos contra engenharia espacial real. Formas simples e
  grandes, com acabamento de equipamento de verdade.
- **Conceito:** a interface é feita das mesmas peças da base. Painéis de aço escuro com bordas
  chanfradas, botões de pintura branca e a cor de cada sistema numa tira de luz.
- **Assinatura:** a tira de luz. Todo painel que fala de um sistema tem uma faixa fina na cor
  dele no topo, e o canto superior esquerdo e o inferior direito chanfrados.
- **Fica de fora:** holograma, neon e brilho; painel translúcido sobre a cena; vermelho, tremor
  ou som de erro para falha; emoji; fonte arredondada de massinha; botão verde (o verde é da
  Estufa).

## 2. Cores

| Papel | Hex |
| --- | --- |
| Painel (aço escuro) | `#2E323B` |
| Secundário, chips (aço claro) | `#3A3F4A` |
| Sombra dura | `#1A1C22` |
| Página opaca (Relatório, Créditos) | `#05060C` |
| Botão principal (pintura branca) e texto | `#E6E4DF` |
| Texto sobre botão branco (tinta) | `#14161C` |
| Texto de apoio / discreto | `#A9ADB8` / `#7A7F8C` |
| Ar e água, Oxigênio | `#4DD0E1` |
| Estufa | `#6BD425` |
| Abrigo, Radiação | `#C77DFF` |
| Laboratório | `#FF9F43` |
| Energia, Bateria | `#FFD23F` |
| Alerta (só zona de perigo) | `#FF5D5D` |
| "Não deu desta vez" (azul-Terra) | `#5C6E8C` |

Contrastes medidos: creme sobre aço 10,1:1; apoio sobre aço 5,7:1; tinta sobre branco 14,2:1;
energia sobre aço 8,9:1. Ciano e vermelho só carregam forma, nunca texto.

## 3. Tipografia (Canvas 1920 por 1080)

| Papel | Fonte | Tamanho |
| --- | --- | --- |
| Título de tela | Barlow Semi Condensed Bold | 60 |
| Título de painel, botão | Barlow Semi Condensed SemiBold | 40 |
| Número de medidor | Barlow Semi Condensed Bold | 36 |
| Corpo | Nunito Bold | 30 |
| Rótulo, caixa alta | Barlow Semi Condensed SemiBold | 24 |

As duas famílias são OFL 1.1, do Google Fonts.

## 4. Sistema

- Espaços: 4, 8, 12, 16, 24, 32, 48 e 64. Chanfro: 14 nos painéis, 10 nos botões e 5 nas células.
- Sombra dura, 6 para baixo, sem desfoque.
- Moldura: HUD de 100 no topo; painel lateral de 840 à direita; botão principal de 400 por 104 no
  canto inferior direito; alvo mínimo de 88 por 88.
- Movimento: toque 80 ms, painel 250 ms, medidor 600 ms, faixa de resultado 300 ms. Nada pisca.
- Botões: principal (branco, texto em tinta, seta), secundário (aço claro), desativado (cinza,
  cadeado e motivo), foco (moldura tracejada creme). Pressionado desce 6 e perde a sombra.

## 5. Ícones

São 35 ícones em `Assets/Resources/Icones/` (128 px, Sprite), recoloridos dos SVG do
"Wireframes UI" pelo script descrito na tarefa 1.1. Os ícones creme têm versão `*_tinta` para o
botão branco. A radiação usa o violeta do Abrigo.

## 6. Telas

| Tela | Modo | Destaques |
| --- | --- | --- |
| Deslocamento | Sem painel | Faixa do destino com ícone e cor do módulo; seta guia na mesma cor; botão de ação no lugar do principal |
| Boletim | Lateral | Uma linha por informação, com ícone e rótulo; tarefa do dia anunciada |
| Escolha | Lateral | Custo e ganho em chips; duas escolhas no mesmo nível, sem botão principal |
| Distribuição | Lateral | Uma linha por módulo, com faixa, ícone, vagas, − e + de 88 |
| Resultado | Lateral | Antes e depois com pílula ▲ ▼ =; "Por quê?" com ícone; cartão de dado real em papel branco |
| Relatório | Página | Estrelas, mosaicos, gráfico com ponto em círculo, triângulo e quadrado |
| Início | Coluna | Base ao fundo; os dois modos como ação principal |
| Pausa | Central | Única tela que cobre o centro; reiniciar pede confirmação |

## 7. Minijogos

Molde comum no painel lateral, com a câmera em close na estação à esquerda: ícone e título,
instrução, relógio creme (nunca vermelho), área do gesto de 760 por 560, barra de progresso sem
nota de corte, "?" e "Sair". O fim mostra "Ótimo!", "Bom!" ou "Não deu desta vez" (azul-Terra).

| Mecânica | Arte |
| --- | --- |
| Raspadinha | Painel solar azul em grade; manchas de regolito em polígonos irregulares |
| Esteira | Trilho chanfrado; zona alvo na cor do sistema; ponteiro triangular; pips de acerto |
| Arrastar | Chão de regolito; pedras facetadas; caixa de amostras com tira laranja |
| Canos | Peças de aço 3 por 3; trecho ligado em ciano; tanque de água usada em mostarda apagado |
| Disjuntores | Alavancas brancas, LEDs, etiquetas com ícone; limite de carga hachurado em amarelo |
| Segurar | Parede com rachadura e vapor; botão redondo com anel de progresso |

## 8. Decisões aprovadas (15/11/2026)

1. O conceito e a tira de luz.
2. O par Barlow Semi Condensed e Nunito (o plano B continua sendo só Nunito).
3. O botão principal em pintura branca.
4. O ícone vermelho de alerta também no aviso de erupção do Boletim, só no ícone. A spec
   `sistema-visual-da-interface` ("Cor com papel fixo") já foi ajustada.
5. As pranchas dos seis minijogos, e a mesma linha para o restante.
