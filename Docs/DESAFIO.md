# Desafio: Build a Junior Astronaut Mission Trainer

Referência do tema que o jogo precisa seguir, no NASA Space Apps Challenge 2026.
Conteúdo traduzido e organizado a partir das páginas oficiais, lidas em 10/11/2026.
As seções 6, 7.1, 7.3 e 8 foram atualizadas em **13/11/2026** com o que a NASA publicou
nesse dia. A aba Resources foi conferida de novo em **14/11/2026** (primeiro dia do
hackathon), sem acréscimos, e as decisões da seção 9 são de 14/11/2026.

- Página oficial: https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/
- Vídeo oficial do desafio: https://www.youtube.com/watch?v=_R0y4i3iY_M
- Última publicação da página pela NASA: 13/11/2026, às 03:48 UTC (antes: 29/10/2026).
  Em 14/11/2026 continuava a mesma

> **Como ler este documento.** As seções 1 a 7 são o que as páginas oficiais dizem.
> A seção 8 confere o que faltava publicar. A seção 9 é interpretação nossa
> para este projeto em Unity, não é regra do evento.

---

## 1. Ficha rápida

| Item | Valor |
| --- | --- |
| Evento | 2026 NASA Space Apps Challenge, tema "The Next Frontier" |
| Data do hackathon | 14 e 15 de novembro de 2026 |
| Nome do desafio | Build a Junior Astronaut Mission Trainer |
| Nível | Intermediate e Beginner/Youth |
| Áreas (tags) | Games, Planets & Moons, Software, Space Exploration, Sun |
| Público-alvo do jogo | Crianças de 7 a 12 anos |
| Entrega esperada | Protótipo funcional de jogo ou app interativo |
| Plataforma | Livre: navegador, mobile ou desktop |

**O desafio em uma frase:** criar um jogo ou app interativo em que crianças de 7 a 12 anos
administram um posto avançado na Lua ou em Marte, tomando decisões com custo real entre
suporte à vida, blindagem contra radiação, energia, produção de comida e atraso de
comunicação, e vendo as consequências ao longo de uma missão simulada.

---

## 2. Resumo oficial

Conteúdo de STEM com tema espacial costuma simplificar demais os trade-offs de engenharia
que definem uma missão real, ou apresentá-los em um nível complexo demais para prender a
atenção de uma criança. Poucas ferramentas tornam esses trade-offs concretos e divertidos
ao mesmo tempo.

O desafio é projetar e construir um jogo ou app interativo que deixe estudantes comandarem
um posto lunar ou marciano, equilibrando demandas concorrentes, para que vivam na prática
as decisões que determinam se uma missão fracassa ou dá certo.

---

## 3. Contexto (Background)

**O problema real.** Enquanto a NASA trabalha para manter presença contínua na Lua e levar
a exploração humana até Marte, um dos desafios mais persistentes não é só o hardware: é
fazer o público entender o que a sobrevivência exige nesses ambientes. Astronautas em um
posto avançado precisam equilibrar demandas que competem entre si:

- a energia é limitada e tem que ser dividida entre suporte à vida, comunicações e ciência;
- a exposição à radiação tem que ser pesada contra os objetivos da missão;
- toda decisão tem um custo em recursos, e esse custo se acumula com o tempo.

São os mesmos trade-offs que os planejadores de missão da NASA enfrentam todo dia, apoiados
em dados reais (ver seção 6).

**A lacuna.** O problema está na tradução. O conteúdo espacial para crianças ou fica
abstrato demais (pôsteres e fatos gerais) para transmitir trade-offs reais, ou pressupõe
uma base técnica avançada demais. Resultado: muitos estudantes nunca veem a lógica de
decisão por trás de uma missão, só o resultado dela. Conteúdo puramente educativo tem
dificuldade de prender a atenção; conteúdo puramente de entretenimento pode perder a
precisão.

**A direção sugerida.** Priorizar mecânicas, e não só o visual, que espelhem restrições
reais: uma escolha que economiza energia hoje pode custar oxigênio amanhã. Basear a
jogabilidade em dados abertos reais da NASA, em vez de estatísticas inventadas, é
incentivado. Formatos possíveis:

- gerenciamento de recursos no estilo sobrevivência;
- árvore de decisões narrativa;
- simulador de controle de missão em formato de painel (dashboard).

Nenhum desses é obrigatório: vale o formato que melhor ensinar o trade-off.

---

## 4. Objetivos

### 4.1 O que construir

Um jogo ou app interativo em que crianças de 7 a 12 anos gerenciam um posto lunar ou
marciano, tomando decisões reais sobre os cinco sistemas abaixo e vendo como essas
escolhas se desenrolam ao longo de uma missão simulada.

| Sistema | Termo original |
| --- | --- |
| Suporte à vida | life support |
| Blindagem contra radiação | radiation shielding |
| Orçamento de energia | power budgeting |
| Produção de comida | food production |
| Atraso de comunicação | communication delay |

### 4.2 Perguntas que o jogo deve responder

- Como é a sensação de trocar reservas de oxigênio por uma oportunidade extra de pesquisa?
- Como é reforçar a blindagem contra um evento de radiação ao custo da energia desta noite?
- Como representar recursos concorrentes de forma que **nenhuma escolha seja gratuita**?
- Como fazer cada decisão ensinar algo verdadeiro sobre o que astronautas e planejadores
  de missão realmente enfrentam?

### 4.3 Dados reais

A NASA incentiva basear a simulação em dados abertos reais sempre que possível. Usar
conjuntos de dados reais, ao vivo ou arquivados, em vez de números fabricados, deixa a
experiência mais autêntica e mostra ao jogador como decisões críticas de missão são
tomadas de verdade.

### 4.4 Formato e critério de sucesso

- O formato é aberto: sobrevivência com gerenciamento de recursos, simulador narrativo de
  decisões, painel de controle de missão ou outra coisa.
- A plataforma é aberta: navegador, mobile ou desktop.
- A entrega é um **protótipo funcional**.
- **Critério de sucesso declarado:** o protótipo deve demonstrar a mecânica de trade-off
  com clareza suficiente para que a criança saia entendendo uma restrição real da
  sobrevivência no espaço profundo, e não apenas tendo jogado algo divertido.
- Interpretações criativas de "posto", "local" ou "missão" além da Lua e de Marte são
  aceitas, desde que o objetivo educativo continue no centro: ensinar trade-offs reais de
  engenharia por meio do jogo.

### 4.5 Obrigatório x incentivado

| Obrigatório pelo enunciado | Incentivado, não obrigatório |
| --- | --- |
| Ser um jogo ou app interativo | Usar dados abertos reais da NASA |
| Público de 7 a 12 anos | Ser jogável no navegador, sem download nem conta |
| Gerenciar um posto (Lua, Marte ou interpretação criativa) | Citar as fontes de dados dentro do jogo |
| Decisões com trade-off real entre recursos | Rejogabilidade (eventos que variam) |
| Consequências visíveis ao longo de uma missão simulada | Servir também a irmãos mais velhos, pais e sala de aula |
| Protótipo funcional | Explicação curta de quais dados da NASA foram usados e por quê |

---

## 5. Considerações potenciais

A página deixa claro que estes pontos são opcionais ("you may, but are not required to").
Na prática, são a melhor pista do que os avaliadores do desafio valorizam.

### 5.1 Público e acessibilidade

- O alvo é 7 a 12 anos, mas vale pensar se a ferramenta se adapta a irmãos mais velhos,
  pais ou uso em sala de aula.
- Preferir linguagem simples a jargão. Quando um termo científico for inevitável (dose de
  radiação, microgravidade, sol), explicar rapidamente no próprio contexto, sem presumir
  conhecimento prévio.
- Barreira de entrada baixa importa: uma experiência jogável no navegador, sem download e
  sem conta, alcança mais avaliadores durante o julgamento e mais estudantes depois.

### 5.2 Base em dados reais

- Sempre que possível, usar as APIs públicas e os conjuntos de dados abertos da NASA em vez
  de inventar números plausíveis. Mesmo um pouco de dado real (uma leitura real de
  radiação, uma foto real de Marte, um achado real de saúde de astronauta) dá muito mais
  credibilidade do que uma experiência toda simulada.
- Atribuir as fontes de dados dentro da própria ferramenta, para o jogador saber que está
  lidando com ciência real da NASA e não só com um jogo temático.
- Se chamadas de API ao vivo não forem viáveis no tempo de desenvolvimento, embutir um
  conjunto de dados real e curado é uma alternativa aceita e honesta. **Só não apresentar
  dado estático como se fosse ao vivo.**

### 5.3 Armadilhas de design a evitar

- Não criar uma escolha "segura" sem custo. O valor educativo vem de toda decisão envolver
  um trade-off genuíno.
- Não deixar o acabamento visual substituir a precisão da mecânica. Um jogo simples que
  ensina uma restrição real vale mais que um bonito que não ensina.
- Não sobrecarregar a criança com sistemas demais ao mesmo tempo. Dois ou três recursos
  centrais (suporte à vida, energia, blindagem) costumam funcionar melhor que cinco ou seis.

### 5.4 O que faz uma solução se destacar

- Mostrar a consequência no mundo real da escolha feita no jogo, por exemplo ligando uma
  decisão de blindagem a dados atuais de clima espacial.
- Rejogabilidade: mecânicas ou eventos que mudam entre partidas, incentivando a criança a
  testar estratégias diferentes e aprender com o fracasso sem que ele pareça punição.
- Acompanhar o protótipo com uma explicação curta de quais dados da NASA foram usados e
  por quê, deixando a ciência rastreável e verificável pelos avaliadores.

---

## 6. Fontes de dados citadas no enunciado

O texto do desafio cita três fontes pelo nome:

| Fonte | O que oferece, segundo o enunciado | Uso sugerido no jogo |
| --- | --- | --- |
| DONKI (Space Weather Database) | Atividade solar e risco de radiação | Eventos de radiação e decisões de blindagem |
| OSDR (Open Science Data Repository) | Dosimetria de radiação de astronautas e pesquisa biológica | Efeitos na saúde da tripulação |
| Dados das missões Artemis e de superfície de Marte | Condições ambientais | Ambiente do local escolhido para o posto |

### 6.1 Aba Resources (publicada em 13/11/2026)

A página avisa: "NASA does not endorse any non-U.S. Government entity" e, em sites que não
são do governo dos EUA, vale a regra de uso de dados de cada site. Também diz: "More
resources may be added before the hackathon begins". Conferido de novo em 14/11/2026,
com o hackathon já aberto: a lista continua com os mesmos 7 recursos.

| Recurso | Organização | Link | O que a página diz que ele oferece |
| --- | --- | --- | --- |
| NASA Open APIs | NASA | https://api.nasa.gov/ | Portal de APIs públicas, com APOD, fotos dos rovers de Marte e o DONKI. Chave grátis, com limite maior que a chave de demonstração |
| DONKI (Space Weather Database Of Notifications, Knowledge, Information) | NASA (CCMC) | https://ccmc.gsfc.nasa.gov/tools/DONKI/ | Dados reais e atualizados de erupções solares, ejeções de massa coronal e outros eventos que geram risco de radiação |
| OSDR (Open Science Data Repository) | NASA | https://osdr.nasa.gov/bio/repo/ (busca: https://osdr.nasa.gov/osdr/data/search?term=KEYWORD) | Arquivo público de pesquisa em biologia espacial e saúde de astronautas (osso, músculo, imunidade, sono e outros), com busca por palavra |
| RadLab (base de dosimetria de radiação da OSDR) | NASA | https://visualization.osdr.nasa.gov/radlab/gui/data-api/ | Medidas reais de dose de radiação a bordo da ISS, de várias agências (por exemplo, taxa de dose absorvida) |
| Mars Trek | NASA | https://trek.nasa.gov/mars/ | Imagens, relevo e terreno reais de Marte |
| Space resources for STEM and youth organizations | CSA (Canadá), agência parceira | https://www.asc-csa.gc.ca/eng/youth-educators/resources-stem-and-youth-organizations.asp | Atividades bilíngues gratuitas para educadores e grupos de jovens |
| Food production on the moon and in remote areas | CSA (Canadá), agência parceira | https://www.asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/ | Pacote didático sobre produção de comida no espaço e nutrição de astronautas |

O que o jogo usa de cada fonte, com data de consulta, está em [DADOS-NASA-2026-11-14.md](DADOS-NASA-2026-11-14.md).

---

## 7. Regras do hackathon que afetam o projeto

Fontes: FAQ do participante e Termos e Condições 2026.

### 7.1 Cronograma

| Data | O que acontece |
| --- | --- |
| 26/08/2026 | Abertura das inscrições |
| 17/09/2026 | Resumos dos desafios e formação de equipes |
| 28/10/2026 | Enunciados completos dos desafios |
| 03/11/2026 | Abertura do Space Apps Connect (fórum da comunidade) |
| **13/11/2026** | Recursos dos desafios, Global Offers, guia de submissão e guia de julgamento e prêmios |
| **14 e 15/11/2026** | Hackathon. Submissão abre sábado 14/11 às 9h e fecha **domingo 15/11 às 23h59**, no fuso do evento local da equipe |
| 03/12/2026 | Anúncio dos Global Nominees (guia de julgamento, 13/11) |
| 17/12/2026 | Anúncio dos Global Finalists e das menções honrosas |
| 26/01/2027 | Anúncio dos Global Winners |

### 7.2 Equipe

- Máximo de 6 pessoas por equipe.
- Mínimo de 1 pessoa em evento local e de 2 pessoas no Universal Event (virtual), para
  valer no julgamento global.
- Todos precisam estar inscritos no mesmo evento local, ou todos no Universal Event.
- Cada pessoa participa de uma única equipe, e cada equipe trabalha em um único desafio.
- Menores de 18 anos precisam ser inscritos e acompanhados por um responsável.

### 7.3 Submissão

- O projeto tem que ser enviado pela página de projeto da equipe em spaceappschallenge.org.
- O prazo segue o horário de encerramento do evento local em que a equipe está inscrita.
- A submissão precisa incluir uma descrição de como ferramentas de IA foram usadas no
  desenvolvimento. O uso é permitido. O site oficial chama essas ferramentas de
  "Super Intelligence (SI)".
- Nomes, vozes e imagens reconhecíveis de menores de 18 anos não podem aparecer em vídeos
  nem na demonstração do projeto.

Detalhes do guia de submissão de 2026 estão na seção 8.2.

### 7.4 Licença e propriedade intelectual

- Todo projeto enviado fica disponível para outras pessoas baixarem e usarem.
- O conteúdo original criado para o desafio tem que ser livre de restrições ou ter licença
  open source reconhecida pela OSI. Sem licença escolhida, a NASA distribui sob Apache 2.0.
- Material de terceiros que não venha da NASA nem de agências parceiras (código, dados,
  assets) tem que ser livre e público, ou no mínimo open source pela definição da OSI.
- Não é permitido enviar solução sobre a qual a equipe não detenha os direitos.

---

## 8. O que faltava publicar: conferido em 13/11/2026

Leitura feita pelo P.O. em 13/11/2026. As páginas do site montam o conteúdo no navegador;
o texto foi lido do conteúdo que a própria página entrega, sem login.

- [x] **Recursos do desafio.** Publicados: 5 da NASA e 2 da CSA (seção 6.1).
- [x] **Guia de submissão** (*Project Submission Guide*, 2026), seção 8.2.
- [x] **Guia de julgamento e prêmios** (*Judging & Awards Guide*, 2026), seção 8.3.
- [ ] **Global Offers.** Não conferido: a página não apareceu nas leituras de 13/11.
- [ ] **Lista completa de guias em /resources/.** Essa página carrega a lista por uma
      consulta separada que não foi lida. Os dois guias foram abertos pelo endereço direto.

Começar o projeto antes de 14/11: os guias não falam disso. A decisão 17 (o Local Lead
confirmou que pode) continua valendo.

### 8.1 Mudanças em relação ao que o plano previa

| Tema | O plano tinha | O guia de 2026 diz |
| --- | --- | --- |
| Demonstração | Nada previsto | **Obrigatória**: vídeo de até 30 s **ou** até 7 slides (com título e introdução), hospedado fora do site, com link público |
| Link do projeto | Hospedagem decidida no sábado (decisão 60) | Campo "Link to Final Project" com link público, **sem senha, permissão nem cadastro** |
| Idioma | Jogo bilíngue (decisão 8) | Página de submissão e **todo material linkado em inglês**. Vídeo pode ser falado em outra língua, com **legenda em inglês**. Slides em inglês |
| Uso de IA | Registro em USO-DE-IA.md | Imagem e vídeo gerados por IA: **marca d'água visível**. Áudio, código e dados gerados por IA: aviso no texto descritivo e nos metadados. A resposta do formulário não vai aos juízes, mas as marcas dentro do projeto vão |
| Marca da NASA | Nada previsto | Conteúdo gerado não pode usar, conter nem modificar logotipo, bandeira ou identificador de missão da NASA |
| Referências | Lista de dados da NASA | Listar **todo** recurso externo (código, texto, imagem, mesmo os livres). Pelo menos um dado ou recurso da NASA é obrigatório para prêmio global |

### 8.2 Guia de submissão (resumo fiel)

Fonte: https://www.spaceappschallenge.org/resources/project-submission-guide/

**Prazo.** Abre sábado 14/11 às 9h e fecha domingo 15/11 às 23h59, no fuso do evento
local. A aba "Project" da equipe só aparece em 14/11 às 9h. Pode salvar e reenviar quantas
vezes quiser até o fim; vale o último envio salvo.

**Campos do formulário.**

| Campo | O que pede |
| --- | --- |
| Project Name | Nome do projeto |
| High-Level Project Summary | O que foi feito, como responde ao desafio, por que importa |
| Link to Project Demo | Vídeo de até 30 s ou até 7 slides, hospedado fora (YouTube, Drive, GitHub, OneDrive, Dropbox), link público. Conta no critério Presentation |
| Link to Final Project | Link público para o projeto (site, repositório, nuvem), sem senha, permissão nem cadastro |
| Detailed Project Description | O que faz, como funciona, benefícios, objetivo, ferramentas e linguagens usadas |
| Use of Super Intelligence (SI) | Se e como a IA foi usada. A página também diz: "SI should not be used to (come up with your idea of how to solve the challenge)" |
| NASA Data | Que dados e recursos da NASA foram usados. Pelo menos um é obrigatório para prêmio global |
| Space Agency Partner Data and Other References | Todo recurso externo usado, inclusive código, texto e imagem livres |

**Arquivos.** A página aceita imagem (JPG, PNG, GIF, até 10 MB), não vídeo nem documento.
Para site novo, o guia recomenda pedir a classificação no filtro FortiGuard
(https://www.fortiguard.com/faq/wfratingsubmit), para os juízes não serem bloqueados.

**Para ser julgado.** Todos da equipe confirmados no mesmo evento local; todos os campos
obrigatórios preenchidos; tudo em inglês; clicar em "Submit for Judging" antes do prazo;
nenhuma imagem, vídeo, áudio ou semelhança de menor de 18 anos; marcar as caixas de
termos e de "trabalho original" (todo recurso externo listado nas referências e nenhum
material com direito autoral sem permissão).

### 8.3 Guia de julgamento e prêmios (resumo fiel)

Fonte: https://www.spaceappschallenge.org/resources/judging-awards-guide/

Em todas as fases, nota de 1 a 5 em cada critério:

| Critério | Nota 5, em poucas palavras |
| --- | --- |
| Impact | Resolve um problema importante e alcança muita gente |
| Creativity | Ideia original ou melhora relevante do que já existe |
| Validity | Base científica, viável, completo e fácil de usar |
| Relevance | Responde ao desafio por inteiro; **o dado da NASA é parte central, não enfeite** |
| Presentation | Comunicação clara e boa história; os juízes avaliam a demo e o projeto inteiro. O guia pede atenção igual a este critério |

Fases: juízes locais escolhem os Global Nominees (03/12/2026); juízes especialistas
escolhem os Global Finalists e as menções honrosas (17/12/2026); um comitê executivo da
NASA e das agências parceiras escolhe os 10 vencedores (26/01/2027). Prêmios: Best Use of
Science, Best Use of Data, Best Use of Technology, Galactic Impact, Best Mission Concept,
Most Inspirational, Best Storytelling, Global Connection, Art & Technology e Local Impact.

Requisitos mínimos para o julgamento global, além dos da seção 8.2: responder a um desafio
oficial; equipe de 1 a 6 pessoas no mesmo evento local; revisão da regra da NASA sobre
vínculo com a China.

---

## 9. Leitura para este projeto (interpretação nossa)

Esta seção não vem da NASA. São consequências práticas do enunciado para o que temos
hoje: um projeto Unity 2022.3.62f3 recém-criado, só com a cena de exemplo.

**Plataforma.** O enunciado recomenda jogar no navegador, sem download e sem conta. Em
Unity isso significa build WebGL, hospedado em um link público. Vale decidir isso no
início, porque afeta tamanho de assets, uso de rede e desempenho.

**Escopo dos sistemas.** O objetivo lista cinco sistemas, mas as considerações recomendam
dois ou três recursos centrais para não sobrecarregar a criança. Uma leitura que atende
aos dois: dois ou três recursos sempre visíveis (por exemplo oxigênio, energia e
blindagem), com comida e atraso de comunicação entrando como eventos ou regras pontuais.

**Regra de design central.** Toda ação precisa custar alguma coisa. Se existir uma opção
sempre correta, o jogo falha no ponto que o enunciado mais repete.

**Dados reais.** O caminho de menor risco é embutir no jogo um conjunto curado de dados
reais (por exemplo, eventos solares históricos do DONKI) e deixar a chamada ao vivo como
melhoria. Em qualquer caso, mostrar a fonte dentro do jogo e nunca rotular dado embutido
como ao vivo.

**Linguagem.** Texto para 7 a 12 anos: frases curtas, termos científicos explicados na
hora em que aparecem. O idioma do jogo é uma decisão em aberto: o julgamento global é
feito por especialistas da NASA e de agências parceiras, então conteúdo em inglês, ou
bilíngue, tende a ser mais seguro.

**Assets e licenças.** A regra de material de terceiros (seção 7.4) pesa em Unity: assets
pagos ou com licença restritiva da Asset Store não se encaixam em "livre ou open source".
Preferir assets próprios, CC0 ou imagens da própria NASA.

**Entregáveis além do jogo.** Com base no que já está confirmado, separar desde já:
a lista de datasets da NASA usados e o motivo de cada um, e o registro de como a IA foi
usada no desenvolvimento.

### Decisões em aberto

Decididas em 14/11/2026:

- [x] Local: polo sul da Lua, seguindo o plano "Dez Dias na Lua".
- [x] Formato: sobrevivência por turnos, um turno por dia, com uma mini tarefa por dia.
- [x] Duração: 10 dias.
- [x] Dados: erupções reais do DONKI embutidas como dado arquivado; modo ao vivo opcional,
      só se sobrar tempo.
- [x] Atraso de comunicação: evento de apagão de rádio (a criança decide um dia sem a dica
      do controle da missão) e cartão comparando Lua (1,3 s) e Marte (até 20 min).
      **A conferir:** falta fonte da NASA que ligue erupção solar a perda de contato
      Terra–Lua. Sem a fonte, o evento não pode ser atribuído à erupção.

Ainda em aberto:

- [ ] Quais dois ou três recursos ficam no centro?
- [ ] Idioma do jogo.
- [ ] Evento local ou Universal Event, e quem está na equipe.

---

## 10. Fontes

Todas lidas em 10/11/2026.

- Página do desafio: https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/
- Página do evento 2026: https://www.spaceappschallenge.org/2026/
- FAQ do participante: https://www.spaceappschallenge.org/resources/participant-faqs/
- Termos e Condições: https://www.spaceappschallenge.org/legal/
- Prêmios: https://www.spaceappschallenge.org/awards/

Lidas em 13/11/2026:

- Guias do participante (página /resources/, que a NASA indica como atualizada em 13/11;
  a lista completa não foi lida, ver seção 8): https://www.spaceappschallenge.org/resources/
- Página do desafio, aba Resources (publicada em 13/11/2026, 03:48 UTC): https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/
- Guia de submissão 2026: https://www.spaceappschallenge.org/resources/project-submission-guide/
- Guia de julgamento e prêmios 2026: https://www.spaceappschallenge.org/resources/judging-awards-guide/
- Página do DONKI no CCMC: https://ccmc.gsfc.nasa.gov/tools/DONKI/
