# Proposta: dados da NASA no centro do dia, em um ciclo de 3 minutos

## Por quê

Hoje o único dado da NASA que mexe na jogabilidade são as erupções do DONKI. Os outros fatos
confirmados em `Docs/DADOS-NASA.md` (Sol a 1,5° no polo, sombras de 25 a 50 vezes, painéis
verticais, regolito, atraso de 1,3 s, base no lado da Terra) aparecem em só 3 das 10 tarefas, e
nenhuma decisão da criança depende deles. O critério Relevance do desafio pede o dado da NASA
como parte central, e o critério de sucesso é a criança sair entendendo uma restrição real.
Além disso, o dia atual tem 5 minutos e uma tarefa só, e termina num console sem fechamento
claro. O usuário quer um dia de cerca de 3 minutos, com duas quests e o fim do dia indo para a
cama. A entrega fecha em 15/11/2026, às 23h59.

## O que muda

- **BREAKING (ciclo do dia):** o dia passa a ter **2 quests**. A quest 1 é uma tarefa de rotina
  com minijogo. A quest 2 é um imprevisto (vazamento ou antena), nos dias em que ele foi
  sorteado, ou uma **pergunta de olhar e pensar** com um fato real da NASA. O Dia 1 tem só a
  quest 1.
- **BREAKING (relógio):** o relógio do dia passa de 5 para **3 minutos**. Ele conta do Boletim
  até a chegada ao console depois das duas quests, e para nos minijogos, nas perguntas, nos
  cartões e na pausa. Se acabar, as quests abertas valem "ficar" e o jogo avisa "Hora de dormir!".
- **Fim do dia na cama:** depois da distribuição de energia no console, a seta leva até a cama
  do dormitório. Ao dormir, a fase "o dia acontece" vira **"a noite passa"** e resolve o dia
  (erupção, colheita, bateria). A criança acorda com o resultado e o Boletim seguinte.
- **Perguntas de olhar e pensar:** 7 perguntas, cada uma com um fato confirmado e fonte da NASA.
  São 2 opções com desenho, a resposta está sempre na tela, e a pergunta compara (mais ou menos,
  grosso ou fino, alto ou baixo), nunca pede número nem nome técnico. Acertar dá +1 ciência
  (a pergunta dos painéis dá +1 célula hoje). Errar não tira nada e mostra a explicação.
- **Aviso da véspera de sombra:** o Boletim do dia anterior a um dia de sombra explica por que o
  Sol vai sumir (no polo sul ele nunca sobe mais de 1,5°). A criança escolhe entre guardar
  energia na bateria e aproveitar no laboratório, e essa escolha já arruma a distribuição.
- **Mais "Você sabia?" com fonte:** coleta de pedras (céu preto de dia), poeira nos painéis
  (painéis verticais), transmissão de dados (base no lado da Terra) e um segundo fato na antena
  (a Terra perto do horizonte). Tarefa sem fonte da NASA continua sem cartão.
- **Escala C < M < X** no cartão "Isso aconteceu de verdade", para a criança ler a força da erupção.
- Custos e bônus das duas quests **se somam** no dia. As metas de estrelas sobem para compensar
  os acertos (valores provisórios, ajustados por simulação).

Fora do escopo: o protótipo `web/index.html` não muda. Diminuir o peso da nota dos minijogos fica
para outra mudança. Os efeitos visuais e sonoros dos eventos continuam adiados até a arte final.

## Capacidades

### Novas capacidades
- `perguntas-e-fatos-nasa`: perguntas de olhar e pensar, aviso da véspera de sombra, cartões
  "Você sabia?" e a regra de que todo fato mostrado tem fonte confirmada da NASA.

### Capacidades modificadas
Estas capacidades foram criadas na mudança `fluxo-web-no-3d`, ainda não arquivada. Esta mudança
deve ser arquivada depois dela.
- `ciclo-da-missao`: fases do dia com 2 quests, cama e "a noite passa"; relógio de 3 minutos;
  metas de estrelas.
- `tarefas-e-minijogos`: sequência com quest 1 de rotina e quest 2 de imprevisto ou pergunta;
  custos que se somam; tela de quest concluída.
- `exploracao-na-base-3d`: estação da quest atual, console, cama, seta guia e luz da noite.
- `clima-espacial-donki`: escala de classes no cartão "Isso aconteceu de verdade".

## Impacto

- Regras (`Assets/Scripts/Regras/`): `Missao.cs`, `RegrasMissao.cs`, `Tarefas.cs`, `Textos.cs` e
  um `Perguntas.cs` novo.
- Unity: `GameManager.cs` (fases novas `AteCama` e `Dormir`, relógio de 180 s, quests em
  sequência), `UIJogo.cs` (painel de pergunta, aviso "Hora de dormir!", escala de classes),
  `Estacao.cs` e a `Estacao_cama` já existente na cena `Game.unity`.
- Testes EditMode (`Assets/Tests/`) e a sonda `ProbeJogo.cs`.
- Créditos e fontes: `Textos.FontesNasa` ganha as fontes F2, F5 e F6 (F3, F4, F7, F8 e F10 já estão lá), e `Docs/DADOS-NASA.md`
  registra onde cada fato é usado.
