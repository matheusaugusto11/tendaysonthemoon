# Tarefas

Pré-requisito: a build da `interface-e-minijogos` (tarefa 6.2 daquela mudança) já publicada. Ela
continua sendo a entrega se esta mudança não terminar até 15/11/2026, às 23h59. Cada grupo
termina com os testes EditMode passando e o jogo rodando no Play, e é um **ponto de commit**. As
referências "D" apontam para `design.md`. As specs estão em `specs/`.

## 1. Dados da NASA

- [x] 1.1 Escrever `Docs/donki/curar.py` (D9), que baixa do DONKI (`ccmc.gsfc.nasa.gov/DONKI/WS/get`, sem chave, blocos de até 60 dias) os FLR dos períodos das 20 erupções curadas, os SEP e as CMEs, salva as respostas brutas em `Docs/donki/` e acrescenta `sepId`, `sepTime` e `sepUrl` em `Assets/Resources/ErupcoesDONKI.json`, usando só SEP cujo instrumento não começa com `MODEL:`. Verificar rodando o script duas vezes: a segunda execução não muda o JSON, os 20 registros continuam lá e cada `sepUrl` presente segue `https://ccmc.gsfc.nasa.gov/DONKI/view/SEP/<n>/-1`.
- [x] 1.2 Gerar com o mesmo script `Assets/Resources/CmesDONKI.json`, com 6 a 10 CMEs (`id`, `startTime`, `speed`, `sourceUrl`, `linkedFlr`, `retrievedOn`), priorizando as ligadas às 20 erupções. Verificar abrindo 2 links de CME e 2 de SEP no navegador e conferindo data e velocidade com o JSON.
- [x] 1.3 Abrir a página da NASA de cada fato dos 9 eventos sem DONKI (D10) e registrar em `Docs/DADOS-NASA.md` uma nova seção 4, "Eventos do dia", com a fonte, o link, a frase exata e a data de consulta. Na mesma passada, acrescentar ao §1 as linhas de SEP e CME e ao §2 a curadoria de SEP e CME. Verificar que cada evento tem a situação "Confirmado" ou "a conferir", e que nenhum fato confirmado depende de página não aberta.
- [x] 1.4 Procurar na OSDR, com limite de 20 minutos, um estudo de plantas ou de sono que sustente o evento do fungo ou o do sono. Registrar o achado, ou a ausência dele, no §4 de `Docs/DADOS-NASA.md`. Verificar que a pergunta em aberto do `design.md` foi respondida no doc.

## 2. Regras dos eventos (assembly `DezDias.Regras`)

- [x] 2.1 Acrescentar o estado do D2 em `Missao.cs` (`DiaMissao`: evento, momento, gatilho, escolha, `CustoEvento`, `Alarme`, `SemContatoHoje`, `FatorTempoHoje`, `Risco`, linha e momento do evento; `Missao`: `Rng`, `EventosUsados`, `AjusteLuz`, campos da CME, `AbrigoPreparado`) e o registro de CME. Fazer `CelulasTotais` subtrair `CustoEvento`, e `IniciarDia`, somar `AjusteLuz[dia]` à luz. Verificar com os testes EditMode existentes passando sem mudança, além de 2 testes novos: `CustoEvento` não é apagado por `Custo` de tarefa, e `AjusteLuz` muda as células do dia certo.
- [x] 2.2 Criar `Eventos.cs` (D1) com os 11 eventos da spec `eventos-do-dia`, cada um com momento, condição, `Conferido` (vindo da 1.3), textos PT e EN, custo em texto e fonte. Verificar com um teste EditMode que, para cada evento, cada escolha tem título e custo não vazios nos dois idiomas, que todo custo é reconhecido por `Apresentacao.PartesDoCusto` e que todo evento tem fonte.
- [x] 2.3 Implementar o sorteio do D3 em `NovaPartida` e `IniciarDia`: partículas no dia de alerta, CME no dia agendado, poeira no Dia 1 com tutorial, e sem repetição, com condição e com fallback nos outros dias. Ajustar `Apoio.Controlada` para limpar a CME. Verificar com testes EditMode: 200 partidas com seeds diferentes não repetem evento fora de partículas; "Frio da sombra" só cai em dia de sombra; o tremor não cai com vazamento aberto; o dia nunca fica sem evento; e o Dia 1 com tutorial tem a poeira.
- [x] 2.4 Implementar o agendamento da CME (D5): chance por modo, dia `d − 2 ≥ 2` sem alerta, alerta da chegada forçado a forte e escolha da CME ligada, se houver. Verificar com testes EditMode: com seed que agenda, a chegada é alerta real e forte, e o número de alertas por modo não muda.
- [x] 2.5 Implementar `Eventos.Escolher` (D4) com os efeitos imediatos, o risco sorteado na escolha, a troca de tempo por 1 célula com o relógio zerado e o `ContextoEscolha` (segundos perdidos, iniciar corrida). Verificar com 1 teste EditMode por escolha de cada evento, cobrindo o cenário correspondente da spec `eventos-do-dia`, e com o cenário "Esperar no abrigo depois do tempo".
- [x] 2.6 Mudar `Resolver` (D4, D6): tirar `if (T.Fora) dRad += 2`, aplicar a exposição do alarme, o risco sorteado, o O₂ da poeira e o abrigo preparado, e escrever a linha e o momento do evento. Trocar `AstronautaForaNaErupcao_MaisDois` pelos testes dos cenários "Continuou trabalhando e saiu da base", "Correu à toa", "Abrigo preparado", "Tremor abre vazamento", "Fungo se espalha" e "Momento no relatório". Verificar os testes passando.
- [x] 2.7 Escrever um teste EditMode de balanceamento com 500 partidas por modo e duas políticas fixas: "corre sempre e liga 2 no Abrigo nos fortes" e "nunca corre e nunca liga". Verificar que a primeira completa a missão em pelo menos 80% das partidas no Cadete e 60% no Comandante, e que a segunda termina por radiação em mais partidas que a primeira. Se falhar, ajustar a exposição pelo risco do D6 e registrar o número final no `design.md`.

## 3. Textos e Boletim

- [x] 3.1 Acrescentar em `Textos.cs` os textos novos em PT e EN: o Boletim com a CME a caminho e as células a menos por evento, o aviso de alarme que separa "agora" de "à noite", as linhas de resultado e os momentos de cada evento, o cartão de SEP e CME e as fontes novas em Créditos. Verificar que o teste EditMode de textos não vazios cobre os novos e passa, e que nenhum texto de SEP ou CME usa "ao vivo", "agora no Sol" ou "hoje no Sol" (teste de busca nos textos).

## 4. Fases do evento e corrida no `GameManager`

- [x] 4.1 Acrescentar as fases `Evento` e `CorridaAbrigo` (D7) e o `_retornoEvento`; ligar o gatilho por tempo em `AteTarefa`, e os gatilhos em `Interagir`, `VoltarAoHub`, `AbrirDistribuicao` e `TempoAcabou`; deixar `Evento` fora de `FaseComRelogio` e `CorridaAbrigo` dentro, e as duas em `PodePausar` conforme a spec. Verificar no Play os cenários "Evento na ida", "Chegar à estação antes do gatilho", "Tempo do dia acaba antes do evento" (com `DepurarTempo`) e "Cartão do evento não consome o relógio".
- [x] 4.2 Implementar a corrida: ativar só a `cama` com o rótulo "Abrigo", a seta e a contagem de 25 s × `FatorTempo`, a chegada por raio sem ação, os 45 s tirados do relógio e o retorno à fase anterior, e o fallback para o console quando não houver `cama`. Verificar no Play os cenários "Chega a tempo", "Contagem acaba", "Espera no abrigo custa a tarefa" e "Pausar na corrida".
- [x] 4.3 Ligar a luz `Erupcao` no cartão de partículas e na corrida, e voltar para `Dia` no fim, e aplicar `FatorTempoHoje` no limite do minijogo (`Tarefas.LimiteReal`). Verificar no Play os cenários "Alarme de partículas" e "Minijogo com sono".
- [ ] 4.4 Medir na cena o caminho mais longo de qualquer estação de tarefa até a `cama`, andando no Play nos dois modos. Verificar que ele cabe em 70% da contagem, ou ajustar a contagem e registrar o valor no `design.md` (D7).
- [x] 4.5 Acrescentar ao `GameManager` um campo de depuração `forcarEvento` (id), que troca o evento do dia ao iniciar. Verificar abrindo cada um dos 11 eventos por esse campo no Play.

## 5. Interface do evento

- [ ] 5.1 Montar em `UIJogo` o cartão do evento (D8) no painel lateral, com título, texto, escolhas com chips de custo, escolha desativada com motivo (O₂ baixo no micrometeorito), rodapé de fonte, dica do tutorial no Dia 1 e nenhum botão de fechar. Verificar em PT e EN, em 16:9 e na largura de celular, os cenários "Custo visível em cada escolha", "Fonte no cartão", "Escolhas do alarme" e "Evento do tutorial".
- [x] 5.2 Mostrar na faixa do deslocamento a contagem da corrida e o "Abrigo" na seta, e a tela curta de chegada ("a tripulação espera a tempestade passar") ou de atraso. Tocar o alarme com `SomSintetico` (D8). Verificar no Play os cenários "Destino na corrida", "Volta ao destino de antes" e "Personagem anda na corrida".
- [x] 5.3 Mostrar no Boletim as notas da CME e do `AjusteLuz`, no resultado as linhas do evento e o cartão "Isso aconteceu de verdade" com SEP e CME, e em Créditos as fontes novas. Verificar os cenários "CME a caminho", "Boletim com consequência de dia anterior", "Cartão com partículas medidas", "CME sem ligação", "Linha do evento no resultado" e "Fontes listadas".

## 6. Integração e entrega

- [ ] 6.1 Jogar uma missão inteira no Cadete em PT e outra no Comandante em EN. Verificar que todo dia teve um evento, que toda escolha mostrou o custo, que a corrida funcionou pelo menos uma vez a tempo e uma vez atrasada, que todo SEP e toda CME aparecem como dado arquivado com fonte, e que o relatório citou pelo menos uma decisão de evento.
- [ ] 6.2 Gerar a build WebGL, abrir no Chrome e repetir a 6.1 em versão curta (até o Dia 4 com `forcarEvento` desligado). Verificar que não há erro no console do navegador, que o checklist "Jogo" de `Docs/REGRAS-PARA-GANHAR.md` §5 continua marcado e publicar. Verificar que o link publicado abre a versão nova.

## Workflow follow-up

- Arquivar a `fluxo-web-no-3d` antes desta: os deltas MODIFIED de `ciclo-da-missao`, `clima-espacial-donki` e `exploracao-na-base-3d` precisam das specs principais que aquela mudança cria.
- Atualizar o campo "NASA Data" da submissão com SEP, CME e as fontes dos eventos.
