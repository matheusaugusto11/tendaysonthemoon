# Tarefas

Ordem: primeiro **todas as interfaces** (grupos 1 a 6), depois **os minijogos** (grupos 7 e 8).
Cada grupo termina com o jogo rodando no Play e é um **ponto de commit**: o usuário commita ao
fim de cada grupo, para que as etapas fiquem em commits separados. O grupo 6 gera e publica uma
build com todas as telas novas e os minijogos antigos. Se os minijogos novos não ficarem prontos
até 15/11/2026, às 23h59, essa build é a entrega. O P2 está no fim, fora da contagem. As
referências "D" apontam para `design.md`. As specs estão em `specs/`.

## 1. Direção e ícones

- [x] 1.1 Escrever um script no scratchpad que extraia os `<symbol>` do artifact "Wireframes UI", troque as cores pelo mapa da paleta v3 (D2, D5) e rasterize cada ícone em 128 px com Chrome headless, gerando `Assets/Resources/Icones/<nome>.png` (energia, oxigenio, radiacao, radiacao_alerta, comida, descoberta, mod_ar, mod_estufa, mod_abrigo, mod_lab, mod_bateria, tarefa_poeira, tarefa_estufa, tarefa_remendo, tarefa_expedicao, relogio, estrela, estrela_apagada, globo, pausa, som, check, alerta, cadeado, ajuda, sol, sombra, fonte, terra, seta, pular, mao). Verificar com uma prancha de contato PNG no scratchpad, conferida a olho, e com o Unity importando todos como Sprite sem erro no Console.
- [x] 1.2 Publicar o artifact novo "Direção de Interface" (D13), com os slides de direção (conceito, assinatura, o que fica de fora, paleta com contrastes, fontes aplicadas a textos reais, ícones da 1.1), os mocapes das telas (HUD, painel lateral, Distribuição, Resultado, Relatório, Início e Pausa) e os dos minijogos (o molde do painel com a câmera na estação e uma prancha para cada uma das 6 mecânicas), sobre capturas de `Docs/capturas-base/`. Escrever o mesmo conteúdo em texto em `Docs/05-UI/DIRECAO-DE-INTERFACE.md`. Verificar que o artifact abre sem texto cortado em 16:9 e na largura de celular, e pedir a aprovação do usuário. A aprovação libera o grupo 3. O grupo 2 segue com os valores do D2, que mudam só em `UiTema` se a direção mudar.

## 2. Base do tema

- [x] 2.1 Criar `Assets/Scripts/Regras/Apresentacao.cs` (D12): a paleta em hex por papel, `FaixaDaNota(nota)` e `PartesDoCusto(texto)`, com o recurso reconhecido nos dois idiomas. Verificar com testes EditMode novos: pares de texto da paleta com contraste de pelo menos 4,5:1 e o botão principal com pelo menos 7:1; cada `FazerCusto` e `FicarCusto` das 10 tarefas, em PT e em EN, gera pelo menos um chip com recurso reconhecido; limites da faixa em 1,0, 0,5 e 0,49.
- [x] 2.2 Baixar do Google Fonts os TTF de Barlow Semi Condensed (SemiBold e Bold) e de Nunito (Bold) para `Assets/Resources/Fontes/`, com o `OFL.txt` de cada família. Criar `UiTema` (D2, D3), que lê a paleta de `Apresentacao` e expõe `Fonte(papel)`, `Tamanho(papel)`, os espaços, os chanfros, os tempos e `Icone(nome)`. Verificar no Play, com uma tela de teste só do editor, que "Ação, coração, não deu desta vez, órgão, maçã, O₂ −6" aparece nas três fontes sem glifo trocado.
- [x] 2.3 Criar `FormaUI` (D4), um `Graphic` com os modos chanfrado (cantos escolhidos), polígono (lados, rotação, irregularidade por semente), anel e arco, triângulo e faixa hachurada. Verificar com uma prancha do editor (menu "Dez Dias/Prancha de formas") que desenha cada modo em 3 tamanhos, salva uma captura em `Docs/05-UI/prancha-formas.png` e mostra arestas nítidas a 1280 por 720 e a 1920 por 1080.
- [x] 2.4 Criar `SonsUI` (D10), com toque, acerto, concluído, não deu e painel gerados por `AudioClip.Create`, mais o volume e o mudo em `PlayerPrefs` (`tdm_som`). Verificar no Play, pela tela de teste da 2.2, que cada som toca, é curto e que o "não deu" soa grave e suave, sem aspereza.
- [x] 2.5 Criar `BotaoFisico` (D6), com os estilos principal, secundário e escolha e os estados normal, pressionado (desce 6 e perde a sombra em 80 ms), desativado (cinza, cadeado e motivo) e foco (moldura tracejada). Verificar na tela de teste com mouse, toque (simulador do editor) e controle, e conferir que o layout não pula ao pressionar.

## 3. Pele das telas atuais

- [x] 3.1 Trocar os helpers de `UIJogo` (`Rot`, `Btn`, `Horizontal` e `Vertical` com fundo, `Nota`, `Alerta`, `Escolha`, `Chip`, `Celulas`) para usar `UiTema`, `FormaUI` e `BotaoFisico`: painéis opacos chanfrados com a tira de luz, os 5 papéis de texto e nenhum vermelho em botão ou título (o botão do vazamento e o "ficar" ruim saem do vermelho). Verificar que uma busca por `Hex("#`, `new Color(` e `fontSize` fora de `UiTema` em `UIJogo.cs` só acha os usos justificados em comentário, e passear no Play do Início ao Relatório, em PT e em EN, sem texto cortado.
- [x] 3.2 Criar o rodapé fixo do painel e `BotaoPrincipal(texto, ação)` (D7), e mover para ele a ação que avança em todas as telas (Começar, Continuar, Começar o dia, Pular, Próximo dia, Ver relatório, Voltar ao hub, Jogar de novo). Verificar o cenário "Do Boletim ao Resultado" da spec `hud-e-moldura` e que, num Boletim longo com rolagem, o botão continua visível.
- [x] 3.3 Pôr ícones e forma nas linhas de informação: Boletim com uma linha por item, `LinhaDelta` com ícone e pílula ▲ ▼ =, "Por quê?" com ícone por causa e chips de custo na Escolha pela `Apresentacao.PartesDoCusto`. Verificar os cenários "Boletim da manhã", "Custo antes da escolha" e "Oxigênio caiu" da spec `telas-da-missao` e o cenário "Botão desativado" da spec `sistema-visual-da-interface`, forçando O₂ em 1 pelo menu de teste ou por seed.
- [x] 3.4 Dar ao cartão "Isso aconteceu de verdade" o molde próprio (ícone de fonte, título fixo, dado, rótulo de arquivado e fonte no rodapé) e acrescentar em Créditos as duas fontes tipográficas com a licença OFL 1.1, com o par de textos PT e EN em `Textos.cs`. Verificar os cenários "Erupção do dia" e "Licença das fontes", e que o teste EditMode de textos não vazios continua passando.

## 4. HUD e deslocamento

- [x] 4.1 Montar os medidores do HUD (D11): Oxigênio em 10 segmentos com zona de 3 ou menos, Radiação em barra com ponteiro e faixa hachurada de 7 a 10, Bateria em 4 células, Ciência e relógio, todos com ícone e número. Verificar os cenários "Leitura rápida", "Radiação alta" e "Fora da zona" da spec `hud-e-moldura` com um atalho só do editor que define O₂ e Radiação.
- [x] 4.2 Animar a troca de valor em 600 ms e mostrar a variação flutuante junto do medidor. Verificar o cenário "Custo de sair da base" e que um dia inteiro não deixa variação presa na tela.
- [x] 4.3 Deixar o relógio do dia amarelo no último minuto, sem pulso. Pôr na faixa do deslocamento o ícone da tarefa ou do console, e na seta guia a cor do módulo de destino. Mover o botão de ação para o lugar do botão principal, acima do joystick da câmera quando `ControlesMobile` estiver ativo. Verificar com `duracaoDia` em 70 s os cenários "Último minuto", "Indo para a Estufa" e "Perto da estação no celular" (com o simulador de dispositivo do editor).

## 5. Painel lateral, páginas e pausa

- [x] 5.1 Implementar os modos Lateral, Página e Coluna do painel (D7) e passar Boletim, Escolha, Decisão do vazamento, Tarefa concluída, O dia acontece e Resultado para o Lateral, e a Distribuição para lista vertical no Lateral. Tirar o véu escuro de 45%. Verificar os cenários "Resultado com a base à vista" e "Colocar uma célula" da spec `telas-da-missao` em 16:9 e em 1280 por 720.
- [x] 5.2 Enquadrar a base de fora no Resultado, com a câmera parada atrás do painel, voltando ao personagem no Boletim seguinte. Verificar que a luz de erupção ou de noite fica visível à esquerda do painel.
- [x] 5.3 Passar o Relatório para página opaca `#05060C`, com estrelas (a apagada usa `estrela_apagada`), mosaicos de números com ícone e o gráfico com ponto em círculo, triângulo e quadrado. Verificar o cenário "Gráfico sem cor" com a captura convertida em cinza, e os quatro motivos de fim pelo atalho de depuração da `fluxo-web-no-3d`.
- [x] 5.4 Montar o Início no modo Coluna, com a base ao fundo numa órbita lenta de câmera. Verificar o cenário "Abrir o jogo" e que "Como jogar", Créditos e Menu de minijogos usam o modo Página.
- [x] 5.5 Passar a Pausa para cartão central sobre a cena escurecida, com "Continuar" como principal, "Som" ligado ou desligado (estado escrito) e "Reiniciar" com confirmação em tom de pergunta, com os textos PT e EN em `Textos.cs`. Verificar o cenário "Reiniciar pela pausa" e que o mudo continua depois de recarregar a página.
- [x] 5.6 Ligar `Navegador` (`prefers-reduced-motion`) ao `UiTema`: painéis, troca de fase, medidores e a órbita do Início passam a usar só troca de opacidade ou ficam parados. Verificar o cenário "Movimento reduzido" no Play forçando a preferência pelo editor.

## 6. Build das interfaces (verificação integrada)

- [ ] 6.1 Gerar a build WebGL e abrir no Chrome a 1920 por 1080 e a 1280 por 720, e num celular deitado. Jogar uma missão inteira em PT no Cadete e outra em EN no Comandante. Verificar: nenhum erro no console do navegador; nenhum texto cortado; todas as telas no modo certo (Lateral, Página, Coluna, Pausa central); os minijogos antigos ainda funcionando dentro do painel; som depois do primeiro clique; o movimento reduzido na build; os 60 fps de antes em 1080p (profiler ou contador); e o tamanho da build comprimida comparado ao da anterior, anotando o aumento.
- [ ] 6.2 Conferir o checklist "Jogo" de `Docs/REGRAS-PARA-GANHAR.md` §5 com a interface nova (custo visível em toda escolha, fonte e "dado arquivado" em toda erupção, nenhum texto no idioma errado) e publicar a build. Verificar que o link publicado abre a versão nova. Esta build é a entrega se o grupo 7 não terminar a tempo.

## 7. Minijogos na cena

- [x] 7.1 Usar o `CameraFoco` (D8), já criado no grupo 5 com `Movimento.AtualizarCamera` respeitando `CameraFoco.Ativo`. `Estacao` ganha `pontoCamera` e esconde o marcador, o rótulo e a máquina provisória no close. `GameManager` chama `Focar` em `Escolha`, `DecisaoVazamento`, `MinijogoInfo` e `Minijogo`, e `Soltar` ao sair de `TarefaConcluida`. Com movimento reduzido, a câmera já corta (feito no 5.6). Verificar no Play os cenários "Reciclagem de água" e "Fim do minijogo" da spec `apresentacao-dos-minijogos`, que Escolha, minijogo e Tarefa concluída ficam no mesmo close sem a câmera ir e voltar, e que o personagem e a câmera voltam ao ângulo de antes.
- [x] 7.2 Ajustar o `pontoCamera` das 8 estações de tarefa (`airlock`, `estufa`, `reciclador`, `filtro`, `backup`, `esteira`, `vazamento`, `disjuntores`) em `Game.unity`, sem câmera dentro de parede e com a estação na metade esquerda. Verificar com uma captura por estação em `Docs/05-UI/capturas-minijogos/`, com o painel aberto, e o cenário "Menu de teste" (vista geral quando não há estação).
- [x] 7.3 Refazer o molde do `Minigame` (D9) sobre o modo Lateral do painel: cabeçalho, instrução, relógio sem vermelho, `Area` de 760 por 560, barra de progresso contínua, rodapé com "?" e "Sair", os helpers `Pulso`, `Flutua` e sons, e `FaixaResultado` pela `Apresentacao.FaixaDaNota`, com "Ótimo!", "Bom!" e "Não deu desta vez" em PT e EN em `Textos.cs`. Verificar os cenários "Leitura do painel", "Últimos segundos" e "Tempo acabou com pouco progresso", e que os testes EditMode continuam passando.
- [x] 7.4 Redesenhar a Raspadinha (poeira e filtros) e o Segurar (vazamento) com `FormaUI`, reposicionados na `Area` nova, seguindo as pranchas aprovadas na 1.2 e o retorno da tabela D9. Verificar pelo `?minigames`, nos dois modos, o cenário "Poeira nos painéis" e que a nota com a mesma jogada fica a até 10 pontos da versão anterior.
- [x] 7.5 Redesenhar a Esteira (exercício e transmissão) e o Arrastar (pedras e braço), mantendo a zona alvo na mesma fração da largura. Verificar pelo `?minigames`, nos dois modos, o cenário "Acerto no ritmo", com pedras e caixas com pelo menos 88 unidades.
- [x] 7.6 Redesenhar os Canos (reciclagem) e os Disjuntores (estufa e antena), com a sobrecarga em amarelo e sem vermelho. O tanque de água usada passa do amarelo para o mostarda (`Paleta.AguaUsada`), e o texto "tanque amarelo" da instrução dos canos em `Tarefas.cs` (PT e EN) muda junto. Verificar pelo `?minigames` os cenários "Sobrecarga nos disjuntores" e "Canos no celular" (simulador do editor), e que a antena ainda cobra 1 célula quando resolvida dentro do limite rápido do Cadete (cenário "Antena no Cadete" da `fluxo-web-no-3d`).
- [x] 7.7 Refazer o cartão de primeira vez com o ícone da tarefa e a ilustração do gesto (arrastar, ritmo, segurar, girar ou ligar, feita com `FormaUI` e o ícone da mão), com a câmera já no close. Verificar o cenário "Primeira vez nos canos" depois de apagar `tdm_seen` nos PlayerPrefs, e que a segunda abertura começa sem o cartão.
- [x] 7.8 Mostrar a mão fantasma (ícone `mao`) repetindo o gesto por 3 s no primeiro minijogo do Dia 1 com tutorial, antes de o relógio começar. Verificar o cenário "Coleta de pedras no Dia 1" e que, com o tutorial desligado, o relógio começa na hora.

## 8. Build final (verificação integrada)

- [ ] 8.1 Gerar a build WebGL e repetir a 6.1 e a 6.2, agora com os 6 minijogos novos com close e painel, nos dois modos. Verificar que nada da lista da 6.1 piorou e publicar.

## Depois desta mudança (P2, fora da contagem)

- Migrar `Text` para TextMeshPro, com atlas estáticos das duas fontes (D3).
- Layout de celular em pé (as 3 telas mais usadas) com a folha de baixo do artifact antigo.
- Cartões de termo científico (radiação, erupção, dia de sombra, Abrigo) explicados na hora.
- Custos estruturados em `Tarefas.cs` no lugar da leitura do texto (D12).
- Trocar os sons sintetizados por sons de um Sound Designer.

## Workflow follow-up

- Marcar a tarefa 6.1 da `fluxo-web-no-3d` (Início e HUD) como coberta por esta mudança depois do grupo 4.
- Arquivar primeiro a `fluxo-web-no-3d` e depois esta mudança (`/opsx:archive`), para as specs entrarem em `openspec/specs/` na ordem certa.
- Atualizar o "Link to Final Project" da submissão quando cada build nova for publicada (grupos 6 e 8).
