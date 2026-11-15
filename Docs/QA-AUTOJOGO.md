# QA automático: auto jogar os 10 dias

Script que joga missões inteiras sozinho, como uma criança jogaria, e anota o que quebrou, travou ou não faz sentido em
relação ao conteúdo da NASA. Código: `Assets/Scripts/QA/AutoJogoQA.cs` e o menu em `Assets/Scripts/Editor/AutoJogoQAMenu.cs`.

## Só no Editor

- O arquivo inteiro está dentro de `#if UNITY_EDITOR`: **nenhuma build (WebGL, mobile, desktop) inclui o auto jogar**.
- Mesmo no Editor, ele só liga quando pedido pelo menu **Tools/QA** (ou pelo arquivo `Temp/qa_autojogo.flag`).
  Um Play normal não é afetado.
- As preferências do jogador (minijogos vistos, tutorial, som) são guardadas antes e devolvidas no fim.

## Como rodar

1. No Unity: **Tools/QA/Auto jogar: 3 missões completas (10 dias cada)**, ou **1 missão rápida**.
2. O jogo entra no Play e joga sozinho, em velocidade 2x. Dá para assistir.
3. No fim ele sai do Play e escreve o relatório em `Temp/QA/<data>/relatorio.md`, com capturas de cada falha.
   **Tools/QA/Abrir o último relatório** mostra a pasta.
4. Para repetir exatamente a mesma partida, use a semente do relatório:
   `echo "completo;semente=12345" > Temp/qa_autojogo.flag` e aperte Play na cena Game.

Opções do pedido, separadas por `;`: `completo` ou `rapido`, `semente=N`, `velocidade=1..4`, `parar=0` (não sai do Play).

## O que ele joga

| Missão | Modo | Jogador | Testes extras |
| --- | --- | --- | --- |
| 1 | Cadete | Cuidadoso: faz as tarefas com nota alta, corre para o abrigo, liga o Abrigo em dia de alerta e guarda bateria antes da sombra | Tutorial ligado; personagem parado nas telas de escolha |
| 2 | Comandante | Arriscado: faz tudo com nota baixa, continua trabalhando no alarme, não liga o Abrigo | Pausa; troca para inglês e volta |
| 3 | Cadete | Aleatório | O relógio do dia acaba antes da tarefa |

A missão rápida é uma missão Cadete com jogador aleatório e todos os testes extras.

Em cada dia, ele lê o Boletim, **anda de verdade** até a estação (caminho calculado numa grade da base, sem teleporte),
resolve o evento do dia, escolhe fazer ou ficar, joga o minijogo, volta ao console, divide a energia, vê "o dia
acontece" e o resultado.

## O que ele confere

**Bugs (FALHA)**
- Travamento: uma fase que não muda, um botão que não faz o jogo andar, uma missão que não termina.
- Personagem: teleporte inesperado (salto maior que o passo possível num quadro), queda pelo chão, posição inválida,
  ficar preso no caminho, ficar congelado numa fase de andar ou andar numa tela que devia estar parada.
- Erros e exceções no console.
- Tempo: `timeScale` em 0 sem pausa, relógio do dia andando no minijogo ou na pausa.
- Regras: recursos fora de 0..10 (Bateria 0..4), radiação sem alerta, alarme falso dando radiação, Abrigo com 2
  células e mesmo assim radiação, dias de sombra e alertas fora da regra, tarefas repetidas, fim da missão errado,
  "GAME OVER" no relatório, escolha indisponível sendo aceita, Conserto aparecendo sem vazamento.

**Conteúdo da NASA (FALHA ou ALERTA)**
- Catálogo do DONKI: classe C/M/X bate com a intensidade de jogo; data do id bate com a data do evento; link do
  registro é do DONKI; evento anterior à data de consulta; proporção 5:10:5; SEP logo depois da erupção; CME ligada a
  uma erupção que está no catálogo e com velocidade plausível.
- Na tela: o cartão da erupção mostra a classe, a fonte DONKI e "dado arquivado"; nenhum texto apresenta dado
  arquivado como ao vivo; todo evento cita fonte da NASA; todo "Você sabia?" tem fonte; tarefa que cita fato real
  (NASA, Estação Espacial, números) tem fonte; toda fonte de evento aparece nos créditos; créditos só com sites nasa.gov.
- Coerência que uma criança perceberia:
  - um alerta **fraco** com erupção real **classe X**;
  - partículas que "vieram" numa erupção **sem SEP** ligado no DONKI, quando o cartão cita o catálogo SEP;
  - uma CME mostrada como ligada à erupção errada;
  - corrida até o abrigo que **uma criança andando não consegue cumprir** (comprimento do caminho real ÷ velocidade
    de andar × contagem);
  - dia de alerta com Abrigo 0 sem aviso na dica do controle da missão;
  - evento cujas escolhas não mudam nada.

**Outros (ALERTA)**
- Idioma: texto sem inglês, texto igual nas duas línguas, palavras em português na tela depois de trocar para inglês.
- Robô de estimação longe do astronauta por mais de 4 s, ou reaparecendo demais.
- Câmera longe do personagem durante o deslocamento.
- Estação sem caminho andando a partir do início.

## Como ler o relatório

- **FALHA** é bug: precisa de correção.
- **ALERTA** é para alguém olhar: pode ser decisão de design, mas pode confundir a criança ou o juiz.
- **Informações** são avisos do console e notas, como uma CME que não é a ligada à erupção.
- O **diário** no fim conta a partida dia a dia: escolhas, notas, energia e erupções. Serve para reproduzir um problema.

## Limites

- Os minijogos não são jogados com o dedo: o script espera alguns segundos e termina com uma nota sorteada pelo tipo de
  jogador. Para testar a mecânica de cada minijogo, use o menu de minijogos (`?minigames`) e a sonda (`ProbeJogo`).
- O caminho usa uma grade de 0,5 m calculada na hora. As portas contam como passagem, porque abrem quando o astronauta
  chega perto.
