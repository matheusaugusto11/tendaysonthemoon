# Tarefas

Prioridade: os grupos 1 a 6 são **P0** e, juntos, dão uma build completa para submeter. O
grupo 7 é **P1** e só começa depois de uma build P0 funcionando. O grupo 8 é a verificação
integrada. As referências "web:" apontam para `web/index.html`.

## 1. Cena e dados (P0)

- [x] 1.1 Conferir as duas cópias de cada `Estacao` em `Game.unity`. Manter uma por id, a que está dentro da base de `MontarBase`, e desativar a outra. Verificar no Play que um log temporário lista cada id uma vez só.
- [x] 1.2 Criar a estação `console` no centro do hub, junto da mesa central, com raio de cerca de 3 m. Verificar no Play que o botão de ação aparece ao parar junto da mesa.
- [x] 1.3 Criar `Assets/Resources/ErupcoesDONKI.json` com os 20 registros de `Docs/DADOS-NASA.md` §2.3, no formato do §2.2, e corrigir a linha "Arquivo do jogo" do §2.1 para esse caminho. Verificar com um script que há 20 registros (5 X, 10 M e 5 C) e que data, classe e número do link batem com o array `FLARES` da web.

## 2. Regras em C# puro (P0)

- [x] 2.1 Criar `Assets/Scripts/Regras/` com o asmdef `DezDias.Regras` (sem `UnityEngine`, Auto Referenced) e `Assets/Tests/EditMode/` com um asmdef de testes que o referencia. Verificar que um teste vazio passa no Test Runner e que o projeto compila sem erros.
- [x] 2.2 Portar `MODES`, o estado `Missao` e `NovaPartida` (web: `newGame`): dias de sombra, alertas com pelo menos uma erupção X e força do alerta por `hiP`/`loP`. Verificar com testes de 1.000 seeds: sombras entre 4 e 9 a 2 dias ou mais de distância, Dia 1 sem alerta, quantidade de alertas e de erupções reais por modo.
- [x] 2.3 Portar `pickTasks` com a lista reduzida de tarefas e a troca de poeira por estufa nos dias de sombra (web: `startDay`). Verificar com testes de 1.000 seeds: sem repetição em dias seguidos, no máximo 2 usos, composição dos dias 5 a 8 por modo, Dia 1 com coleta de pedras quando há tutorial, nenhuma tarefa excluída.
- [x] 2.4 Portar `IniciarDia`, `CelulasTotais` e `Previa` (web: `startDay`, `totalCells`, `previewLines`), com forma física e contato. Verificar com testes dos cenários "Células disponíveis no dia" e "Dica do controle da missão".
- [x] 2.5 Portar `Resolver` (web: `resolve`): O₂ por Ar e água, vazamento e Conserto, colheita, Laboratório, dose da erupção, astronauta fora, alarme falso, bateria, células perdidas, comida, ordem das condições de fim e o impacto e o momento do histórico. Verificar com um teste por cenário das specs `distribuicao-de-energia`, `clima-espacial-donki` e `ciclo-da-missao` (condições de fim).
- [x] 2.6 Implementar a tabela `Tarefas` com "fazer(nota, tempo, modo)" e "ficar" das 10 tarefas, a marca de "fora da base", o bloqueio com O₂ 1 ou menos, o contato pela antena e a decisão do vazamento. Verificar com testes dos limites de cada tarefa na spec `tarefas-e-minijogos`, incluindo o fator 1,3 do Cadete.
- [x] 2.7 Implementar a nota de 1 a 3 estrelas, os 3 momentos que mais pesaram e a dica pelo motivo do fim (web: `rating`, `vEnd`). Verificar com testes das metas de 3 e 2 estrelas nos dois modos e da dica de cada motivo.

## 3. Idioma e textos (P0)

- [x] 3.1 Criar `Idioma` (Atual, Mudou, `L(pt, en)`, idioma inicial por `Application.systemLanguage`) e `Textos.cs` com os pares PT/EN copiados da web para telas, tarefas, minijogos, dicas, cartões e créditos. Verificar com um teste EditMode que nenhum texto está vazio em nenhum idioma, e com outro que 2017-09-06 sai "6 de setembro de 2017" e "September 6, 2017".

## 4. Minijogos com nota graduada (P0)

- [x] 4.1 Refatorar `Minigame`: trocar `Falhar` por `Terminar(nota)`, adicionar `Progresso()`, o callback `ResultadoMinijogo`, o limite da tarefa vezes 1,3 no Cadete e "SAIR" terminando com o progresso. Verificar no Play que o fim do tempo encerra com nota parcial e que nada recomeça.
- [x] 4.2 Ajustar as 6 mecânicas para a tabela D3 do design: itens, quantidade e botão do `MinigameLixo`; meta do `MinigameEsteira` e errar sem desfazer; trecho molhado do `MinigameCanos`; tempo do `MinigameDisjuntores`; fração limpa da `MinigameRaspadinha`; progresso do `MinigameSegurar`. Verificar cada uma pelo menu de teste (4.4) e conferir a nota mostrada.
- [x] 4.3 Trocar `Minigames.Criar` para escolher o minijogo pela tarefa (tabela D3) e mostrar o cartão de primeira vez, guardando os já vistos em `PlayerPrefs` (`tdm_seen`). Verificar que a segunda abertura do mesmo minijogo começa sem o cartão, mesmo depois de reiniciar o jogo.
- [x] 4.4 Criar o menu de teste aberto quando `Application.absoluteURL` contém "minigame", com escolha de modo, "Jogar de novo" e "Menu de minijogos". Verificar na build WebGL local com `?minigames` que as 10 tarefas abrem e terminam.

## 5. Fases do dia no GameManager (P0)

- [x] 5.1 Trocar o `EstadoJogo` pela máquina `Fase` (design D2). O `GameManager` passa a guardar a `Missao` e a liberar o `Movimento` só em `AteTarefa` e `AteHub`, e a pausa continua funcionando. Verificar no Play um dia inteiro passando pelas fases na ordem da spec, com o personagem parado fora do deslocamento.
- [x] 5.2 Implementar o relógio de 300 s que fica parado no minijogo e na pausa, e o que acontece quando ele acaba antes ou depois da tarefa (no vazamento, "deixar para depois"). Verificar com o relógio em 10 s pelo Inspector os três cenários da spec "Relógio de 5 minutos por dia" e o do vazamento.
- [x] 5.3 Ativar só a estação da tarefa do dia. O console aceita interação só depois da tarefa, a seta muda de destino, e os rótulos do botão de ação ficam nos dois idiomas. Verificar no Play os cenários da spec `exploracao-na-base-3d` (menos a luz).
- [x] 5.4 Remover o fluxo antigo: `DayData.cs`, `DiasPadrao.cs`, `DonkiDados.cs`, `EventosNASA.json`, `Estacao.Regras` e `Estacao.Custos`, e a lógica de cama e do airlock final. Verificar que o Console abre sem erros e que uma busca por `DiasPadrao|DonkiDados|EventosNASA|"cama"` em `Assets/Scripts` não acha nada.

## 6. Telas (P0)

- [ ] 6.1 Montar a tela Início (modo, "Como jogar", tutorial ligado ou desligado, créditos, idioma) e o HUD (dia, O₂, Radiação, Bateria, Estrelas, relógio, idioma, pausa). Verificar as duas telas em PT e EN, em 16:9 e na largura de celular.
- [x] 6.2 Montar Boletim, Escolha (custos, saída desativada com O₂ 1 ou menos, aviso de erupção para quem sai), Decisão do vazamento e Tarefa concluída (com "Você sabia?" na antena). Verificar os cenários das specs com tarefas forçadas pelo menu de teste ou por seed.
- [x] 6.3 Montar a Distribuição: cartões na cor do módulo, + e −, limites, Conserto só com vazamento, sugestão do Cadete, prévia ou "sem previsão" e aviso de células perdidas. Verificar no Play cada cenário da spec `distribuicao-de-energia` que depende da tela.
- [x] 6.4 Montar "O dia acontece" (cerca de 4 s, "Pular", e pular direto quando o navegador pede movimento reduzido, lido por `.jslib` de `prefers-reduced-motion`) e o Resultado (antes e depois, "Por quê?", cartão DONKI com "Ver o registro" por `Application.OpenURL` no clique). Verificar que o link abre o registro certo na build WebGL.
- [x] 6.5 Montar o Relatório: título por motivo, estrelas, chips, momentos, dica, "Jogar de novo" e "Créditos", sem "GAME OVER". Verificar forçando cada motivo de fim (O₂, radiação, comida, completa) com um atalho de depuração só no editor.
- [x] 6.6 Montar "Créditos e fontes": DONKI e as fontes das tarefas no sorteio, valores de jogo, uso de IA e aviso de que não há endosso da NASA. Verificar que a lista bate com as fontes citadas em `Textos.cs` e que não há imagem de logotipo da NASA no projeto.
- [x] 6.7 Ligar a troca de idioma para redesenhar a tela atual. Verificar o cenário "Trocar no meio da distribuição" (3 células mantidas) e a troca durante o deslocamento.

## 7. Tutorial, gráfico e luz (P1)

- [x] 7.1 Montar os 5 slides de "Como jogar", a dica de cada fase do Dia 1 e o desligamento do tutorial depois do Dia 1 (`tdm_tut`). Verificar que a primeira partida mostra slides e dicas e que a segunda vem sem tutorial.
- [x] 7.2 Desenhar o gráfico de Oxigênio, Radiação e Bateria por dia no Relatório, com segmentos de `Image`. Verificar numa missão de 10 dias que as 3 linhas têm 10 pontos e as cores dos medidores.
- [x] 7.3 Ligar o `BaseLightingController` às fases: Dia no Boletim, e Erupção ou Noite em "o dia acontece" e no resultado. Verificar num dia com erupção e num dia sem.

## 8. Verificação integrada

- [ ] 8.1 Gerar a build WebGL e abrir no Chrome e no Brave. Verificar que carrega sem erro no console do navegador, que o idioma inicial segue o navegador e que `?minigames` abre o menu de teste.
- [ ] 8.2 Jogar uma missão inteira em cada modo, uma em PT e outra em EN, e marcar o checklist "Jogo" de `Docs/REGRAS-PARA-GANHAR.md` §5. Verificar que toda escolha mostra o custo, que toda erupção mostra a fonte como dado arquivado e que nenhum texto fica em outro idioma.

## Acompanhamento do fluxo

- Depois da revisão, sincronizar as specs com `openspec/specs/` e arquivar a mudança (`/opsx:archive`).
- Atualizar o "Link to Final Project" da submissão quando a build 3D substituir o protótipo web.
