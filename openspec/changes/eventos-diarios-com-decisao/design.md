# Design: um evento com decisão em cada dia

## Contexto

Ver `proposal.md` (Por quê). Estado atual relevante:

- As regras puras ficam no assembly `DezDias.Regras` (`Missao.cs`, `RegrasMissao.cs`,
  `Tarefas.cs`, `Textos.cs`) e têm testes EditMode. O `GameManager` só troca de fase, conta o
  relógio do dia (`TempoRestante`, que não existe nas regras) e chama a UI.
- As tarefas gravam o custo em células com atribuição (`T.Custo = 1`, `= 2`) e o efeito de
  amanhã em `m.FitAmanha = ...`. Um evento que somasse nesses campos seria apagado pela tarefa,
  ou a apagaria.
- `RegrasMissao.Resolver` não recebe `Random`. A regra "fora da base +2" usa `Hoje.Fora`, que
  as tarefas de fora ligam em `Tarefas.Fazer`.
- A cena tem as estações `airlock, antena, backup, cama, disjuntores, esteira, estufa, filtro,
  lixo, painel, reciclador, vazamento` e o console do hub. A `cama` fica no Dormitório, que a
  spec 3D já liga ao módulo Abrigo.
- `BaseLightingController` tem os presets `Dia`, `Erupcao` e `Noite`. `SomSintetico` gera os
  sons de interface.
- `Docs/donki/` (respostas brutas citadas em `Docs/DADOS-NASA.md` §2.1) não está no
  repositório. A consulta de 15/11/2026 confirmou que o DONKI responde para SEP e CME, que os SEP
  trazem `linkedEvents` com o FLR e a CME de origem, e que parte dos SEP vem de modelo
  (`"MODEL: REleASE:..."`).

## Objetivos / Fora do escopo

**Objetivos:** a tabela de eventos e os efeitos dela ficam nas regras puras, testáveis sem
cena; o `GameManager` ganha só o gatilho, duas fases e o relógio; os efeitos dos eventos não
interferem nos efeitos das tarefas; a build atual continua jogável se a mudança parar no meio.

**Fora do escopo:** editor de eventos, eventos em JSON editável, mais de um evento por dia,
animação de personagem correndo, áudio gravado, mudanças no protótipo web.

## Decisões

### D1. Tabela de eventos em código, em `Eventos.cs`

Cada evento é um `EventoDef` com: `Id`, `Momento` (`Ida`, `Volta`, `Qualquer`), uma condição
`Func<Missao, bool>`, título, texto, fonte, `Conferido` (bool) e de 2 a 3 `EscolhaDef`, cada uma
com título, custo (texto), descrição e um efeito `Action<Missao, ContextoEscolha>`. É o mesmo
padrão de `Tarefas.cs` (dicionário estático com textos PT e EN via `Txt`).

*Alternativa:* JSON em `Resources/`, como as erupções. Foi descartada porque os efeitos são
código, e o JSON só levaria os textos, dividindo um evento em dois lugares, com o prazo de hoje.

### D2. Estado novo isolado do estado das tarefas

Em `DiaMissao`: `Evento` (id), `EventoNaIda`, `GatilhoSeg` (4 a 12 s, sorteado),
`EventoResolvido`, `Escolha` (id), `CustoEvento` (células de hoje), `Alarme` (`Nenhum`,
`Abrigo`, `Atrasado`, `Continuou`), `SemContatoHoje`, `FatorTempoHoje` (padrão 1),
`Risco` (o risco sorteado aconteceu?) e `LinhaEvento`/`MomentoEvento` (`Txt`).

Em `Missao`: `Rng` (o `Random` da partida), `EventosUsados`, `AjusteLuz`
(`Dictionary<int,int>`, as células a mais ou a menos por dia, somadas em `IniciarDia`),
`CmeDia`, `CmeChegada`, `CmeRegistro`, `AbrigoPreparado`.

`CelulasTotais` passa a subtrair `CustoEvento` junto de `Custo`. A tarefa continua escrevendo
`Custo` com `=`, e o evento escreve só `CustoEvento`. *Alternativa:* trocar todos os `Custo =`
das tarefas por `+=`. Foi descartada porque mexe em 8 efeitos testados por um ganho só estético.

### D3. Sorteio por dia, em `IniciarDia`, com o `Random` guardado na partida

`NovaPartida` guarda `m.Rng = rng` e agenda a CME (D5). `IniciarDia` escolhe o evento: no dia
de alerta, `particulas`; no dia `CmeDia`, `cme`; no Dia 1 com tutorial, `poeira_eclusa`; nos
outros, um evento válido, conferido e não usado, ou um já usado se não sobrar nenhum. A
condição é avaliada no começo do dia porque depende do estado (vazamento, contato, crescimento,
sombra), o que não daria para saber no começo da partida. Nos eventos `Qualquer`, sorteia ida ou
volta com chance de 50%.

`IniciarDia(Missao)` mantém a assinatura. Os testes que criam `Missao` à mão sem `Rng` recebem
um `Random` fixo criado na hora, para não quebrar `Apoio.cs`.

### D4. Efeitos aplicados na escolha; exposição e risco revelados no resultado

`Eventos.Escolher(Missao m, string escolha, bool relogioZerado)` aplica o efeito imediato
(células, O₂, Estrelas, contato só hoje, fator de tempo) e sorteia agora o risco
(tremor, fungo), guardando o resultado em `Hoje.Risco`, sem mostrar. A função devolve um
`ContextoEscolha` com `SegundosPerdidos` e `IniciarCorrida`, para o `GameManager` aplicar no
relógio, que só ele tem. Com `relogioZerado`, o custo em tempo vira `CustoEvento += 1`, conforme
a spec.

`Resolver` ganha um passo logo antes da radiação da noite: aplica o risco sorteado (vazamento,
crescimento zerado), a exposição do alarme (D6) e o O₂ −1 da poeira, e escreve
`LinhaEvento`. O impacto do evento entra na disputa pelo "momento" do dia com a mesma regra
`impacto = max(...)` que já existe. Sortear na escolha deixa `Resolver` sem `Random` e
determinístico nos testes.

### D5. CME pendurada num alerta real que já existe

Em `NovaPartida`, depois dos alertas: com chance de 0,6 (Cadete) ou 0,8 (Comandante), procura o
primeiro alerta com erupção real no dia `d` tal que `d − 2 ≥ 2` e o dia `d − 2` não tenha alerta.
Se achar, marca `CmeDia = d − 2` e `CmeChegada = d` e força `Forte = true` no alerta de `d`. A
CME vem da lista curada: a ligada à erupção de `d`, se existir, ou outra da lista, e nesse caso
o texto não afirma ligação. Os 2 dias são valor de jogo. O cartão mostra a velocidade real e diz
que, no jogo, a nuvem chega em 2 dias. A conta de trânsito (de 18 a 37 h para as CMEs curadas,
`Docs/DADOS-NASA.md` §2.5) não aparece no jogo, porque a distância Sol–Lua não tem fonte
registrada.

*Alternativa:* a CME criar um alerta novo. Foi descartada porque muda o número de alertas por
modo, que os testes e a dificuldade já calibram.

### D6. Exposição do alarme separada da dose da noite

Exposição do alarme (só com erupção real): `Continuou` vale 1, 1 e 2 para fraca, média e
forte, mais 1 se `Hoje.Fora`; `Atrasado` vale metade disso, arredondada para cima; `Abrigo`
vale 0. A dose da noite pelo Abrigo da distribuição (1, 3 e 5) não muda, a não ser com o abrigo
preparado (1 célula basta). A regra fixa `if (T.Fora) dRad += 2` sai.

Pior caso por dia: continuar, sair e deixar o Abrigo sem células numa forte dá 3 + 5 = 8.
Antes, sair numa forte com o Abrigo sem células dava 5 + 2 = 7. A diferença é pequena e agora
pode ser evitada correndo para o abrigo. A calibragem é conferida pelo teste de simulação
(tarefa 2.7).

Resultado da simulação (500 partidas por modo, 15/11/2026), sem ajuste nos números:

| Política | Cadete | Comandante |
| --- | --- | --- |
| Corre no alarme e põe 2 no Abrigo nos fortes | completa 99% (radiação 1%) | completa 95% (radiação 5%) |
| Continua no alarme e põe 2 no Abrigo nos fortes | completa 95% (radiação 5%) | completa 57% (radiação 42%) |
| Nunca se protege | radiação em 80% | radiação em 100% |

### D7. Duas fases novas no `GameManager`: `Evento` e `CorridaAbrigo`

- **Gatilho:** em `AteTarefa`, um acumulador de tempo de fase chega a `GatilhoSeg` e abre
  `Evento`. `Interagir()` na estação da tarefa com evento da ida pendente abre `Evento` antes de
  `Escolha`. `VoltarAoHub()` com evento da volta pendente abre `Evento` antes de `AteHub`.
  `AbrirDistribuicao()` e `TempoAcabou()` com evento pendente abrem `Evento` antes. Ao resolver,
  o jogo volta para a fase guardada em `_retornoEvento`.
- **`Evento`:** não está em `FaseComRelogio` (relógio parado). Personagem congelado. Luz
  `Erupcao` se o evento é `particulas`, e `Dia` nos outros.
- **`CorridaAbrigo`:** está em `FaseComRelogio` e em `PodePausar`. O personagem anda.
  `RefrescarEstacoes` ativa só a `cama`, com o rótulo "Abrigo". `AtualizarGuia` aponta para
  ela. Uma contagem própria de 25 s × `FatorTempo` corre fora da pausa. Entrar no raio da `cama`
  encerra a corrida a tempo, sem ação: `Alarme = Abrigo` e `TempoRestante -= 45`, e se zerar
  vale a regra de `TempoAcabou`. Se a contagem zera, `Alarme = Atrasado`. Nos dois casos, a luz
  volta para `Dia` e o jogo volta para `_retornoEvento`.
- Se a cena não tiver a `cama`, a corrida usa o console do hub, com aviso no log, como já
  acontece com as tarefas sem estação.
- **Estimativa da corrida (15/11/2026, das posições em `Temp/estacoes.txt`):** a `cama` fica a
  21,4 m do centro do hub. O pior caso é a Estufa, a 31,2 m do centro: 52,6 m passando pelo hub,
  cerca de 10,5 s a 5 m/s andando, ou 13,7 s com 30% a mais de desvios e portas. Isso cabe nos 70%
  da contagem (17,5 s no Comandante e 22,8 s no Cadete). Falta confirmar andando no Play (tarefa 4.4).

*Alternativa:* resolver "correr para o abrigo" só no cartão, sem andar. Foi descartada porque o
usuário pediu a interrupção na base 3D, e a corrida é o que dá peso à escolha. O custo de
implementar é baixo, porque seta, estações e movimento já existem.

### D8. Interface: cartão no painel lateral, contagem na faixa do deslocamento

O cartão do evento reaproveita o layout da tela Escolha da `interface-e-minijogos` (painel à
direita, escolhas com custo em ícone e número, rodapé de fonte), sem botão de fechar. A corrida
usa a faixa do deslocamento e a seta guia, com a contagem em destaque. O alarme usa um som de
`SomSintetico` com os parâmetros de "não deu" repetidos 3 vezes: é um som novo de parâmetro,
não de código. No Boletim, a CME a caminho e o `AjusteLuz` viram notas do mesmo tipo das notas
de vazamento e exercício.

### D9. Dados: curadoria reprodutível em `Docs/donki/`

Um script Python curto em `Docs/donki/curar.py` baixa do `ccmc.gsfc.nasa.gov/DONKI/WS/get`
(sem chave, blocos de até 60 dias) os FLR das 20 erupções curadas, os SEP e as CMEs dos mesmos
períodos. Ele salva as respostas brutas e escreve:

- em `ErupcoesDONKI.json`, os campos opcionais `sepId`, `sepTime` e `sepUrl`, com o primeiro
  SEP de `linkedEvents` cujo instrumento não começa com `MODEL:`;
- `CmesDONKI.json`, com 6 a 10 CMEs (`id`, `startTime`, `speed`, `sourceUrl`, `linkedFlr`,
  `retrievedOn`), priorizando as ligadas às 20 erupções.

`RegistroFLR` ganha os campos novos. `JsonUtility` deixa os campos ausentes vazios, então o
arquivo antigo continua carregando.

### D10. Fontes: só entra no sorteio o evento conferido

Cada `EventoDef` tem `Conferido`. Antes da build, cada fato é aberto na página da NASA e
registrado em `Docs/DADOS-NASA.md` (nova seção 4), com a frase exata, como no §3. O evento sem
fonte aberta fica com `Conferido = false` e sai do sorteio. Fontes candidatas: Terra no
horizonte (F3, F4 e F5, já conferidas); tremor (sismômetros da Apollo, NASA Science);
micrometeorito (NASA Meteoroid Environment Office, monitoramento de impactos na Lua); poeira
(NASA, poeira lunar nas missões Apollo); frio (LRO Diviner); gelo (LCROSS, 2009); sono e
saudade (NASA Human Research Program, páginas de risco); fungo (Veggie na ISS). Para a OSDR,
procurar na tarefa 1.4 um estudo de plantas ou de sono que sustente o fungo ou o sono. Se não
achar a tempo, fica a página da NASA, e a OSDR continua "a conferir".

## Riscos / Trade-offs

- [Prazo de hoje, 23h59] → Os grupos de tarefas terminam com o jogo rodando. A build da
  `interface-e-minijogos` é publicada antes. Se a corrida não ficar pronta, o "Correr para o
  abrigo" resolve no cartão como "chegou a tempo", e a spec `exploracao-na-base-3d` é ajustada
  antes do arquivamento. Se faltarem eventos
  conferidos, o sorteio usa os conferidos com repetição.
- [Dois canais de radiação confundem a criança] → O texto separa "agora" (partículas durante o
  trabalho → correr ou não) e "à noite" (Abrigo da distribuição). O resultado mostra as duas
  linhas, cada uma com o seu ícone.
- [Dificuldade sobe no Comandante] → Teste de simulação com 500 partidas por política. Se a
  política "corre sempre e liga o Abrigo nos fortes" completar menos que hoje, a exposição de
  "Continuou" na fraca cai para 0.
- [Corrida impossível por geometria] → 25 s foi pensado para atravessar o hub. A tarefa 4.4
  mede o caminho mais longo até a `cama` na cena. Se passar de 70% da contagem, a contagem sobe.
- [O jogador espera o relógio zerar para não pagar tempo] → Com o relógio zerado, o custo em
  tempo vira 1 célula.
- [Unity diverge do protótipo web] → É aceito e fica registrado na proposta. A web segue como
  referência das regras antigas.

## Plano de migração

Não há dado salvo de partida, só preferências (`tdm_seen`, `tdm_tut`), que não mudam. O teste
`AstronautaForaNaErupcao_MaisDois` é substituído pelos testes da exposição do alarme. Para
voltar atrás, basta reverter os commits, porque o JSON novo é compatível com o leitor antigo.

## Perguntas em aberto

- ~~Qual estudo da OSDR citar (D10).~~ Respondida em 15/11/2026: OSD-268 (micróbios em alfaces
  da estufa Veggie), como fonte complementar do fungo. Ver `Docs/DADOS-NASA.md` §4.
