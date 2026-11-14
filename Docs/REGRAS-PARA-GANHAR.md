# Regras para ganhar

Lista do que o jogo e a submissão precisam cumprir para ter mais chance no desafio
**Build a Junior Astronaut Mission Trainer** (NASA Space Apps Challenge 2026).

É um resumo para consulta rápida durante o hackathon. O texto completo e traduzido do
enunciado está em [DESAFIO-2026-11-14.md](DESAFIO-2026-11-14.md); as fontes de dados, em [DADOS-NASA-2026-11-14.md](DADOS-NASA-2026-11-14.md).

Atualizado em 14/11/2026, primeiro dia do hackathon. "Hoje", neste arquivo, é 14/11.

**De onde vem cada regra**

| Marca | Origem | Conferido em |
| --- | --- | --- |
| `[E]` | Enunciado do desafio (abas Details e Resources) | 14/11/2026, no navegador. Sem mudanças desde 13/11 |
| `[S]` | Guia de submissão 2026 | 13/11/2026, via DESAFIO.md (não relido hoje) |
| `[J]` | Guia de julgamento e prêmios 2026 | 13/11/2026, via DESAFIO.md (não relido hoje) |
| `[T]` | FAQ do participante e Termos e Condições | 10/11/2026, via DESAFIO.md (não relido hoje) |
| `[N]` | Interpretação nossa, não é regra do evento | |

**Prazo:** a submissão fecha **domingo, 15/11/2026, às 23h59**, no fuso do evento local.

---

## 1. Regras eliminatórias

Descumprir qualquer uma tira o projeto do julgamento ou do prêmio global.

| # | Regra | Origem |
| --- | --- | --- |
| 1 | Clicar em **"Submit for Judging"** antes do prazo, com todos os campos obrigatórios preenchidos. Vale o último envio salvo | `[S]` |
| 2 | Página de submissão e **todo material linkado em inglês**. Vídeo pode ser falado em outra língua, com legenda em inglês | `[S]` |
| 3 | **Demonstração obrigatória**: vídeo de até 30 s **ou** até 7 slides, hospedado fora do site, com link público | `[S]` |
| 4 | **Link do projeto público**, sem senha, permissão nem cadastro | `[S]` |
| 5 | Usar **pelo menos um dado ou recurso da NASA** e declará-lo no campo "NASA Data" | `[S]` |
| 6 | Listar **todo** recurso externo nas referências (código, texto, imagem, áudio), mesmo os livres | `[S]` |
| 7 | Material de terceiros tem que ser livre e público ou open source (OSI). Nada de asset pago ou de licença restritiva da Asset Store | `[T]` |
| 8 | Código e conteúdo originais sob licença open source reconhecida pela OSI (sem escolha, vale Apache 2.0) | `[T]` |
| 9 | Nenhuma imagem, voz, nome ou semelhança de **menor de 18 anos** no vídeo, nos slides ou no jogo | `[S]` |
| 10 | **Não usar logotipo, bandeira nem identificador de missão da NASA**. Citar a fonte em texto é permitido | `[S]` |
| 11 | Declarar o uso de IA. Imagem e vídeo gerados por IA levam **marca d'água visível**; áudio, código e dados gerados por IA levam aviso na descrição e nos metadados | `[S]` |
| 12 | A IA não deve ser usada para "come up with your idea of how to solve the challenge". A ideia do jogo tem que ser da equipe | `[S]` |
| 13 | Equipe de 1 a 6 pessoas, todas confirmadas no mesmo evento local, em um único desafio | `[T]` `[J]` |

---

## 2. Regras de design do jogo

O que o enunciado pede. A ordem é a de peso: as primeiras são as que ele mais repete.

| # | Regra | Como conferir no jogo | Origem |
| --- | --- | --- | --- |
| 1 | **Nenhuma escolha é gratuita.** Toda decisão tem um custo real em outro recurso | Não existe ação "segura" nem estratégia que vença sempre | `[E]` |
| 2 | **A criança sai entendendo uma restrição real** da sobrevivência no espaço, não só tendo se divertido. Este é o critério de sucesso declarado | Depois de uma partida, uma criança consegue dizer o que aprendeu em uma frase | `[E]` |
| 3 | **Mecânica acima de visual.** "A simple game that teaches a real constraint beats a beautiful one that doesn't" | O trade-off funciona mesmo com arte provisória | `[E]` |
| 4 | **Dois ou três recursos centrais** na tela (suporte à vida, energia, blindagem), não cinco ou seis | Os outros sistemas entram como eventos ou contadores secundários | `[E]` |
| 5 | **Consequências ao longo da missão.** O custo se acumula: economizar energia hoje pode custar oxigênio amanhã | Há decisões cujo efeito só aparece turnos depois | `[E]` |
| 6 | **Público de 7 a 12 anos.** Linguagem simples; termo científico explicado na hora em que aparece | Frases curtas, ícones, nenhum termo sem explicação | `[E]` |
| 7 | **Os cinco sistemas do objetivo aparecem de algum jeito**: suporte à vida, blindagem, energia, comida e atraso de comunicação | Cada um tem pelo menos uma decisão ou evento | `[E]` `[N]` |
| 8 | **Fracasso que ensina**, sem parecer punição | O fim da missão explica o que pesou e convida a tentar de novo | `[E]` |
| 9 | **Rejogabilidade.** Eventos que mudam entre partidas | Duas partidas seguidas não têm a mesma ordem de eventos | `[E]` |
| 10 | **Barreira de entrada baixa**: jogável no navegador, sem download e sem conta | Build WebGL em link público, carrega em rede comum | `[E]` |
| 11 | Tema é um **posto** na Lua ou em Marte. Outro local é aceito, desde que o trade-off de engenharia continue no centro | | `[E]` |

Sobre a regra 7: o enunciado lista cinco sistemas no objetivo e recomenda dois ou três
recursos nas considerações. A leitura que atende aos dois é manter três medidores na tela
e tratar comida e atraso de comunicação como eventos.

---

## 3. Regras de dados da NASA

O critério Relevance do julgamento pede que o dado da NASA seja **parte central, não enfeite**.

| # | Regra | Origem |
| --- | --- | --- |
| 1 | Usar dado real em vez de número inventado. Um pouco já vale: uma leitura real de radiação, uma foto real, um achado real de saúde | `[E]` |
| 2 | O dado real tem que **mexer na jogabilidade**, não só decorar a tela | `[J]` |
| 3 | **Citar a fonte dentro do jogo**, à vista do jogador | `[E]` |
| 4 | **Nunca apresentar dado embutido como ao vivo.** Dado embutido e curado é aceito, desde que rotulado como arquivado | `[E]` |
| 5 | Entregar junto uma **explicação curta de quais dados foram usados e por quê**, para o juiz conseguir conferir | `[E]` |
| 6 | Ligar a decisão do jogador a uma consequência do mundo real, por exemplo blindagem e clima espacial atual. É o primeiro item de "o que faz uma solução se destacar" | `[E]` |
| 7 | Em site que não é do governo dos EUA, seguir a regra de uso de dados do próprio site | `[E]` |

**Fontes oficiais do desafio** (aba Resources, sem acréscimos em 14/11/2026):

| Fonte | Serve para |
| --- | --- |
| DONKI | Erupções solares e ejeções de massa coronal reais: eventos de radiação e blindagem |
| OSDR | Pesquisa real de saúde de astronautas: efeitos na tripulação |
| RadLab | Dose de radiação medida na ISS: escala do medidor de radiação |
| NASA Open APIs | Porta única para APOD, fotos dos rovers e DONKI |
| Mars Trek | Terreno e imagens reais de Marte |
| CSA, *Food production on the moon* | Produção de comida no espaço (agência parceira) |

---

## 4. Como os juízes dão nota

Nota de 1 a 5 em cada critério, em todas as fases `[J]`. A coluna da direita é leitura nossa `[N]`.

| Critério | Nota 5 | O que fazer para chegar lá |
| --- | --- | --- |
| Impact | Resolve um problema importante e alcança muita gente | Jogar no navegador, servir também em sala de aula, ter versão em inglês |
| Creativity | Ideia original ou melhora relevante do que já existe | Uma mecânica de assinatura que só faz sentido com o dado real |
| Validity | Base científica, viável, completo e fácil de usar | Protótipo que se joga do começo ao fim; todo fato com fonte |
| Relevance | Responde ao desafio por inteiro; dado da NASA central | Cobrir os cinco sistemas e fazer o DONKI decidir a partida |
| Presentation | Comunicação clara e boa história | Vídeo de 30 s que mostra uma decisão, o custo e a consequência |

O guia pede atenção igual ao critério Presentation: o vídeo de 30 s pesa tanto quanto o jogo.

---

## 5. Checklist antes de enviar

**Jogo**

- [ ] Abre por um link público, no navegador, sem conta
- [ ] Dá para jogar uma missão do começo ao fim
- [ ] Toda decisão mostra o custo
- [ ] Fonte da NASA visível em cada dado real, rotulado como arquivado ou ao vivo
- [ ] Tela de créditos com todas as fontes e assets
- [ ] Sem logotipo, bandeira ou identificador de missão da NASA
- [ ] Texto em inglês disponível
- [ ] Marca d'água em toda imagem gerada por IA

**Submissão**

- [ ] Project Name e High-Level Project Summary
- [ ] Link to Project Demo (vídeo de até 30 s ou até 7 slides, link público)
- [ ] Link to Final Project (sem senha nem cadastro)
- [ ] Detailed Project Description
- [ ] Use of Super Intelligence (SI)
- [ ] NASA Data
- [ ] Space Agency Partner Data and Other References
- [ ] Licença open source no repositório
- [ ] Caixas de termos e de trabalho original marcadas
- [ ] "Submit for Judging" clicado antes de 15/11, 23h59
