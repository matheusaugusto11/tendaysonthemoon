# Dados da NASA usados no jogo

Registro de cada fonte da NASA usada em **Dez Dias na Lua**: o que se usa, o link, o que a
fonte diz sobre o uso e quando foi consultada. Esse registro serve de base para a tela
"Créditos e fontes" (GDD 8.4) e para os campos "NASA Data" e "References" da submissão.

Mantido pelo P.O. Consulta feita em **13/11/2026**, salvo indicação na linha.

> **Regra 9 do CLAUDE.md.** Todo dado real aparece com fonte visível e como **dado
> arquivado**, nunca como "ao vivo". Fato sem fonte da NASA continua marcado "a conferir".

---

## 1. Fontes

| # | Fonte | O que o jogo usa | Link | Uso e licença, segundo a fonte | Consulta |
| --- | --- | --- | --- | --- | --- |
| F1 | DONKI, catálogo de erupções solares (FLR), do CCMC/NASA Goddard | 20 erupções reais, embutidas como dado arquivado (seção 2) | https://ccmc.gsfc.nasa.gov/tools/DONKI/ | Dados "as is", sem garantia; o uso não implica endosso da NASA. O DONKI se declara "Experimental Research Information" de "prototyping quality"; a previsão oficial de clima espacial é da NOAA SWPC. Para agradecimento em publicação, a página remete à política do CCMC: https://ccmc.gsfc.nasa.gov/publication-policy/ (não lida em 13/11) | 13/11/2026 |
| F2 | NASA SVS 4893, *Shadows near the Moon's South Pole* (01/04/2021) | Altura do Sol no polo (fato 1) | https://svs.gsfc.nasa.gov/4893 | Página pública da NASA. Termos de uso de imagem não conferidos: o jogo usa só o fato, não a imagem | 13/11/2026 |
| F3 | NASA SVS 4944, *Earth and Sun from the Moon's South Pole* (16/10/2021) | Sol rente ao horizonte; Terra vista do polo (fatos 1, 2 e 3) | https://svs.gsfc.nasa.gov/4944 | Idem F2 | 13/11/2026 |
| F4 | NASA Science, *9 Phenomena NASA Astronauts Will Encounter at Moon's South Pole* (11/09/2024, atualizada em 15/05/2025) | Sol baixo andando pelo horizonte, sombras longas, céu preto de dia, Terra perto do horizonte (fatos 2, 3, 4 e 5) | https://science.nasa.gov/solar-system/moon/phenomena-nasa-astronauts-will-encounter-at-moons-south-pole/ | Idem F2 | 13/11/2026 |
| F5 | NASA, *NASA's Artemis Base Camp on the Moon Will Need Light, Water, Elevation* (27/01/2021) | Inclinação de 1,5° do eixo da Lua; base no lado voltado para a Terra (fatos 1 e 9) | https://www.nasa.gov/humans-in-space/nasas-artemis-base-camp-on-the-moon-will-need-light-water-elevation/ | Idem F2 | 13/11/2026 |
| F6 | NASA Space Technology, *NASA, Industry to Mature Vertical Solar Array Technologies for Lunar Surface* (23/03/2021, atualizada em 22/06/2026) | Painéis solares verticais no polo sul (fato 7) | https://www.nasa.gov/technology/nasa-industry-to-mature-vertical-solar-array-technologies-for-lunar-surface/ | Idem F2 | 13/11/2026 |
| F7 | NASA Science, *How Does Webb See Back in Time?* (atualizada em 14/01/2026) | Luz leva cerca de 1,3 s da Lua à Terra (fato 6, cartão do atraso, GDD 8.5) | https://science.nasa.gov/mission/webb/science-overview/science-explainers/how-does-webb-see-back-in-time/ | Idem F2 | 13/11/2026 |
| F8 | NASA Human Research Program, *Hazard: Distance From Earth* (atualizada em 01/08/2024) | Atraso até Marte (cartão do atraso, GDD 8.5) | https://www.nasa.gov/hrp/hazard-distance-from-earth/ | Idem F2 | 13/11/2026 |
| F9 | NASA NSSDC, *Moon Fact Sheet* (atualizada em 11/01/2024) | Distância média, perigeu, apogeu e período sinódico, para as contas dos fatos 1, 2 e 6 | https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html | Idem F2 | 13/11/2026 |
| F10 | Grande et al., *Protecting Crew and Surface Systems with a Long-Duration Lunar Safe Haven*, NASA Langley, Kennedy e Marshall, ASCEND 2021 (NTRS 20210022066) | Regolito cobrindo o habitat (fato 8) | https://ntrs.nasa.gov/citations/20210022066 | Estudo técnico público no NTRS. É estudo de conceito, não projeto adotado do Artemis | 13/11/2026 |

### Recursos oficiais do desafio que o jogo ainda não usa

Publicados na aba Resources em 13/11/2026 (ver [DESAFIO-2026-11-14.md](DESAFIO-2026-11-14.md), seção 6).

| Recurso | Situação no jogo |
| --- | --- |
| NASA Open APIs (https://api.nasa.gov/) | Não usado. O caminho do DONKI por essa porta devolveu erro 404 em 13/11/2026 (seção 2.1) |
| OSDR (https://osdr.nasa.gov/bio/repo/) | Previsto para os 2 cartões do GDD 8.2. Nenhum estudo escolhido ainda: **a conferir** |
| RadLab, dosimetria da ISS na OSDR (https://visualization.osdr.nasa.gov/radlab/gui/data-api/) | Não usado. Opção para o cartão de radiação do GDD 8.2 (pergunta ao usuário no relatório) |
| Mars Trek (https://trek.nasa.gov/mars/) | Não se aplica: a base fica na Lua |
| CSA, recursos de STEM para jovens e *Food production on the moon* | Não usados. Agência parceira, não NASA |

---

## 2. DONKI: lista real de erupções

### 2.1 Como foi obtida

| Item | Valor |
| --- | --- |
| Porta usada | `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=AAAA-MM-DD&endDate=AAAA-MM-DD`, pública, sem chave |
| Porta pedida no brief | `https://api.nasa.gov/DONKI/FLR?...&api_key=DEMO_KEY` devolveu **404** (`"path":"/DONKI-API/get/FLR/"`) em 13/11/2026, às 11:01 UTC. A rota de notificações do DONKI, na mesma porta, respondeu normalmente |
| Porta antiga | `kauai.ccmc.gsfc.nasa.gov` não aceitou conexão. A página do DONKI informa que o endereço antigo foi trocado por `ccmc.gsfc.nasa.gov`, com os mesmos caminhos |
| Limite | O CCMC recusa intervalo maior que 60 dias ("Date range cannot exceed 60 days") |
| Períodos baixados | Setembro de 2017, e de 01/01/2024 a 12/11/2026 em blocos de até 60 dias |
| Resposta bruta | 19 arquivos JSON, sem nenhuma edição, em [donki/](donki/) (`FLR_<início>_<fim>.json`). Ao todo, 1.982 erupções: 89 X, 1.603 M, 283 C e 7 B |
| Arquivo do jogo | `Assets/Resources/ErupcoesDONKI.json` (Unity), com os 20 registros da lista curada (seção 2.3) no formato abaixo. A mesma lista está embutida no protótipo web (`web/index.html`, array `FLARES`) |

### 2.2 Regra de curadoria

| Regra | Como foi aplicada |
| --- | --- |
| Tamanho | 20 registros (o pedido era de 12 a 20). Com 20, as 2 erupções sorteadas variam mais entre partidas |
| Classe para intensidade de jogo | **C = fraca, M = média, X = forte.** A classe mede o brilho em raios X, não a dose que chega à Lua (GDD 8.1). A dose em pontos continua sendo número de jogo |
| Proporção 1 : 2 : 1 (GDD 6.2, passo 6) | 5 C, 10 M e 5 X |
| Eventos marcantes | A maior erupção do ciclo 24 que está no DONKI (X9.3, 06/09/2017); as erupções da região ativa 13664 em maio de 2024; a X9.0 de 03/10/2024; a X5.1 de 11/11/2025 |
| Variedade | Anos de 2017, 2024, 2025 e 2026; classes de M1.0 a M9.8; 15 regiões ativas diferentes |
| Campos (GDD 8.1) | `id` = `flrID`; `isTest` = false; `eventType` = `FLR` (tipo como o DONKI publica); `startTime` = `beginTime` (UTC); `classLabel` = `classType`; `gameIntensity` = 0, 1 ou 2; `sourceUrl` = `link` do evento; `retrievedOn` = 2026-11-13 |
| Formato do arquivo | `{"records":[...]}` com `gameIntensity` em número (0 fraca, 1 média, 2 forte), o mesmo formato de `Tests/Fixtures/flares-test.json`, que o `GameDataTools` lê com `JsonUtility` |

**Maio de 2024.** O DONKI separa as erupções (FLR) das tempestades geomagnéticas (GST).
A lista traz **erupções** da região ativa 13664 de 08 a 14/05/2024, não a tempestade em
si. O texto do cartão não pode chamar esses eventos de "a tempestade de maio" sem conferir
o registro GST correspondente.

### 2.3 Lista curada

| # | `id` (flrID) | Início (UTC) | Classe | Intensidade | Região ativa | Link do evento |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 2017-09-06T11:53:00-FLR-001 | 06/09/2017 11:53 | X9.3 | forte | 12673 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/13010/-1 |
| 2 | 2024-05-11T01:10:00-FLR-001 | 11/05/2024 01:10 | X5.8 | forte | 13664 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30711/-1 |
| 3 | 2024-05-14T16:46:00-FLR-001 | 14/05/2024 16:46 | X8.7 | forte | 13664 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30849/-1 |
| 4 | 2024-10-03T12:08:00-FLR-001 | 03/10/2024 12:08 | X9.0 | forte | 13842 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/33693/-1 |
| 5 | 2025-11-11T09:49:00-FLR-001 | 11/11/2025 09:49 | X5.1 | forte | 14274 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/42610/-1 |
| 6 | 2017-09-04T20:15:00-FLR-001 | 04/09/2017 20:15 | M5.5 | média | 12673 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/12979/-1 |
| 7 | 2024-05-08T22:05:00-FLR-001 | 08/05/2024 22:05 | M9.8 | média | 13664 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30603/-1 |
| 8 | 2024-05-10T13:58:00-FLR-001 | 10/05/2024 13:58 | M5.9 | média | 13664 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30680/-1 |
| 9 | 2024-05-13T09:36:00-FLR-001 | 13/05/2024 09:36 | M6.6 | média | 13664 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30803/-1 |
| 10 | 2024-06-08T01:23:00-FLR-001 | 08/06/2024 01:23 | M9.7 | média | 13697 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/31335/-1 |
| 11 | 2025-02-24T21:50:00-FLR-001 | 24/02/2025 21:50 | M3.9 | média | 13990 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/37523/-1 |
| 12 | 2025-03-17T19:25:00-FLR-001 | 17/03/2025 19:25 | M1.0 | média | 14033 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/37870/-1 |
| 13 | 2025-05-30T23:31:00-FLR-001 | 30/05/2025 23:31 | M8.1 | média | 14100 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/39182/-1 |
| 14 | 2025-08-10T14:57:00-FLR-001 | 10/08/2025 14:57 | M2.2 | média | 14168 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/40506/-1 |
| 15 | 2026-06-06T13:40:00-FLR-001 | 06/06/2026 13:40 | M1.8 | média | 14461 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/46688/-1 |
| 16 | 2017-09-12T07:23:00-FLR-001 | 12/09/2017 07:23 | C3.0 | fraca | 12680 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/13124/-1 |
| 17 | 2024-01-21T00:35:00-FLR-001 | 21/01/2024 00:35 | C5.3 | fraca | 13559 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/28668/-1 |
| 18 | 2025-01-04T14:24:00-FLR-001 | 04/01/2025 14:24 | C9.3 | fraca | 13939 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/36258/-1 |
| 19 | 2025-06-16T23:53:00-FLR-001 | 16/06/2025 23:53 | C5.5 | fraca | 14114 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/39559/-1 |
| 20 | 2026-01-10T19:31:00-FLR-001 | 10/01/2026 19:31 | C2.1 | fraca | 14339 | https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/43894/-1 |

Os links 1 e 2 foram abertos em 13/11/2026: respondem e mostram a data e a classe do evento.

### 2.4 Para o cartão "Isso aconteceu de verdade"

- Rodapé (GDD 8.1): "Fonte: NASA DONKI (CCMC). Evento de [data]. Dado arquivado, consultado
  em 13/11/2026."
- O registro do DONKI informa como instrumento o sensor de raios X do satélite GOES
  (`GOES-P: EXIS 1.0-8.0`, por exemplo). A frase "a classe mede o brilho em raios X" ainda
  precisa de uma página da NASA que a diga com essas palavras: **a conferir**.
- Regra do evento (guia de submissão): o jogo não pode usar logotipo, bandeira nem
  identificador de missão da NASA. Citar a fonte em texto é diferente de usar a marca.

---

## 3. Fatos que o GDD marcou "a conferir"

| # | Fato no plano | Situação | O que a fonte diz, na íntegra | Fonte |
| --- | --- | --- | --- | --- |
| 1 | Sol a ~1,5° no polo sul (decisões 53 e 65) | **Confirmado** | "At the Moon's North and South Poles, the Sun is never more than 1.5° above or below the horizon." / "the Moon is tilted only 1.5 degrees on its axis" | F2, F3, F5 |
| 2 | Sol anda ~12° por dia (decisão 69) | **Confirmado por cálculo nosso** a partir de dado da NASA. A frase "12° por dia" não aparece escrita em nenhuma página lida | Período sinódico de 29,53 dias (F9): 360° ÷ 29,53 = **12,2° por dia**. O Sol "glides around the horizon" (F3) e "moves horizontally across the sky" (F4) | F3, F4, F9 |
| 3 | Terra baixa no horizonte (decisão 53) | **Parcial.** As fontes mostram a Terra perto do horizonte e às vezes abaixo dele, mas não dão a altura em graus | Legenda: "Earth emerging below the horizon as seen from the Moon's South Pole" (F4). A Terra "bobs up and down" e "appears to be upside-down and rotating backwards" (F3) | F3, F4 |
| 4 | Sombras muito longas (decisão 53) | **Confirmado**, com número | "shadows that are 25 to 50 times longer than the objects casting them" | F4 |
| 5 | Céu sem estrelas de dia (decisão 53) | **Parcial.** Céu preto confirmado; "sem estrelas" não aparece na página | "the daytime sky is black" | F4 |
| 6 | Atraso Terra–Lua ~1,3 s (decisão 37, GDD 8.5) | **Confirmado** | "Light takes around 1.3 seconds to travel the distance to Earth." Pelo F9, de 1,21 s no perigeu (363.300 km) a 1,35 s no apogeu (405.500 km): conta nossa, com c = 299.792 km/s. Isso confirma "varia um pouco ao longo do mês" | F7, F9 |
| 6b | Atraso até Marte "às vezes mais de 20 minutos" (GDD 8.5) | **Diverge da fonte citável.** A página da NASA diz "até 20 minutos" | "Facing a communication delay of up to 20 minutes one way" | F8 |
| 7 | Painéis verticais no polo sul | **Confirmado como tecnologia em desenvolvimento** pela NASA, ainda não instalada | "When low-angled light hits rocky formations like hills and slopes in these areas, it casts a shadow over the surface." / "The vertical position and height of these new designs will help prevent loss of power at the lunar poles." Até 32 pés (cerca de 10 m) de altura | F6 |
| 8 | Habitats cilíndricos cobertos de regolito (decisão 48) | **Regolito: confirmado só como conceito de estudo.** **Cilindro deitado: sem fonte.** No estudo, a base é uma cúpula metálica coberta de regolito, e a alternativa 1.1B é um **cilindro vertical** | "a structure would be built around and over the habitat, and regolith would be piled around and on top" / "Concept 1.1B was a variation to the Baseline but eschewed the concept of a regolith dome in favor of a cylinder" | F10 |
| 9 | Base no lado voltado para a Terra | **Confirmado** (fato a mais, útil para o cartão do atraso) | "The Artemis Base Camp has to be on the Earth-facing side to make it easier for engineers to use radio waves to communicate" | F5 |

### Continua sem fonte

- Altura da Terra no céu do polo sul, em graus.
- Formato de cilindro deitado para os módulos (decisão 48).
- "Sem estrelas no céu de dia" (decisão 53).
- Sombra previsível e calendário de luz de um ponto real (GDD 5.2 e 8.3).
- Estudos da OSDR para os 2 cartões do GDD 8.2.
- Frase da NASA que defina a classe da erupção como brilho em raios X (seção 2.4).
- Termos de uso de imagem das páginas F2 a F8 (o jogo usa só o texto do fato).
