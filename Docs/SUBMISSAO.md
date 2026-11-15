# Submissão: como enviar e o que falta

Acompanhamento da submissão de **Dez Dias na Lua** no NASA Space Apps Challenge 2026
(desafio *Build a Junior Astronaut Mission Trainer*).

Fonte: [Project Submission Guide](https://www.spaceappschallenge.org/resources/project-submission-guide/),
página de 13/11/2026, relida na íntegra em **15/11/2026** (último dia). Tudo que está
nas seções 1 a 4 é regra oficial. Observações com **[N]** são nossas.

> **PRAZO: domingo, 15/11/2026, 23h59**, no fuso do **Local Event** do time (no Universal
> Event, vale o fuso de moradia do Team Owner). Projeto salvo ou enviado depois disso
> **não é julgado** e não gera certificado. Confirmar com o Local Lead se a premiação local
> tem horário diferente.

---

## 1. Requisitos eliminatórios (resumo)

- [ ] Todos os membros são **participantes confirmados no mesmo Local Event** (quem está em lista de espera precisa ser aceito ou trocar de evento)
- [ ] **Todos os campos obrigatórios** preenchidos (sem isso o botão não deixa enviar)
- [ ] Página de submissão e **todo material linkado em inglês** (vídeo pode ser em outra língua **com legenda em inglês**; slides só em inglês)
- [ ] Demo dentro do limite: **vídeo de até 30 s OU até 7 slides** (título e introdução contam). Passou do limite = **fora do Global Judging**
- [ ] Links de demo e projeto **públicos**, sem login, senha, permissão ou cadastro
- [ ] Pelo menos **1 dado/recurso da NASA** declarado (exigido para Global Award)
- [ ] **Nenhuma imagem, vídeo, áudio ou semelhança de menor de 18 anos**, na demo e no projeto
- [ ] **Nenhum logotipo, bandeira ou identificador de missão da NASA**, inclusive em conteúdo gerado por IA
- [ ] Clicar em **"Submit for Judging"** antes do prazo

---

## 2. Campos do formulário (Project tab)

Acesso: ícone de perfil → **My Team** → aba **Project** (aparece só após 14/11, 9h local) →
botão azul **Edit**.

| # | Campo | O que a NASA pede | Status | Observação |
| --- | --- | --- | --- | --- |
| 1 | **Project Name** | Nome do projeto | [ ] | **[N]** decidir se vai "Ten Days on the Moon" (inglês) ou "Dez Dias na Lua" |
| 2 | **High-Level Project Summary** | O que foi desenvolvido, como responde ao desafio, por que importa | [ ] | Em inglês. Curto e direto |
| 3 | **Link to Project Demo** | Vídeo ≤ 30 s ou ≤ 7 slides, hospedado fora do site, link público | [ ] | Ver seção 3 |
| 4 | **Link to Final Project** | Link público do projeto (site, repositório, nuvem) | [ ] | Ver seção 4 |
| 5 | **Detailed Project Description** | O que faz, como funciona, benefícios, objetivo, ferramentas/linguagens/software usados | [ ] | Em inglês. Citar Unity, versão web, C#, etc. |
| 6 | **Use of Super Intelligence (SI)** | Se usou IA (ML, chatbots, geração de imagem, LLM) e como | [ ] | Ver seção 5. Resposta **não vai para os juízes**, só coleta de dados |
| 7 | **NASA Data** | Detalhes de quais dados/recursos da NASA foram usados | [ ] | Base pronta em [DADOS-NASA.md](DADOS-NASA.md) (fontes F1 a F10, DONKI etc.) |
| 8 | **Space Agency Partner Data and Other References** | **Todo** dado, recurso, ferramenta, código, texto e imagem usado, mesmo livre/open source | [ ] | Incluir também Unity, pacotes, fontes, sons. Agências parceiras são incentivadas (ex.: Brazilian Space Agency, ESA, JAXA...) |
| 9 | Imagens (opcional) | Upload/colar JPG, PNG, GIF **até 10 MB** | [ ] | **[N]** colocar capturas de `Docs/capturas-base` |

Regras do formulário:

- **Não aceita upload de vídeo nem documento.** Só imagem. Vídeo e slides vão para YouTube, Google Drive, GitHub, OneDrive, Dropbox etc., com link público.
- **Salvar sempre** com o botão azul **Save Progress** (à direita). O que está salvo só é visível ao time até o envio.
- **Lock de edição:** ligar o "Lock" ao editar, para o time saber. Mas qualquer membro pode remover o lock e **sobrescrever** o que não foi publicado. Combinar quem edita.

---

## 3. Demo (vídeo ou slides)

A demo pesa na nota de **Presentation**. A maior parte da avaliação vem do projeto em si.

Escolher **um** formato: [ ] vídeo  [ ] slides

**Vídeo (≤ 30 s)**, roteiro sugerido pela NASA:

| Tempo | Conteúdo |
| --- | --- |
| 0:00 | Seu nome e o nome do time |
| 0:02 | Qual desafio o projeto aborda e o que inspirou a escolha |
| 0:05 | Nome e slogan do projeto |
| 0:10 | Como funciona: imagens/protótipo, experiência do usuário, como dados da NASA e de agências parceiras tornaram possível |
| 0:20 a 0:30 | Futuro: o que a solução pode fazer pelas pessoas, pelo mundo e além |

**Slides (≤ 7, contando título e introdução)** precisam ter:

- [ ] Nome do time
- [ ] Nomes dos membros
- [ ] Desafio escolhido
- [ ] Imagem do projeto
- [ ] Experiência do usuário
- [ ] Como dados e tecnologia da NASA/agências viabilizam a solução
- [ ] Fonte legível, poucas palavras por slide, visuais fortes

Checagem final da demo:

- [ ] Duração ≤ 30 s ou ≤ 7 slides
- [ ] Inglês (ou legenda em inglês no vídeo)
- [ ] Sem menores de 18 anos (rosto, voz, nome, imagem)
- [ ] Sem logotipo/bandeira/identificador da NASA
- [ ] Marca d'água visível em qualquer imagem/vídeo gerado por IA
- [ ] Link aberto em **janela anônima** sem pedir login

---

## 4. Link do projeto final

Estado atual do repositório, conferido em 15/11/2026:

| Item | Situação |
| --- | --- |
| Remoto | `https://github.com/matheusaugusto11/tendaysonthemoon` (branches `main` e `teste`) |
| Visibilidade do repositório | [ ] **conferir se está público** (abrir em janela anônima) |
| Versão jogável publicada | [ ] **falta**: build WebGL ou `web/index.html` hospedado (GitHub Pages, itch.io etc.) |
| `LICENSE` na raiz | [ ] **falta**. A regra de termos pede licença open source reconhecida pela OSI (Apache 2.0 vale) |
| `README` na raiz | [ ] **falta**. Explicar o jogo, como rodar, créditos e fontes |
| Trabalho em `main` | [ ] hoje o trabalho está na branch `teste`; decidir qual branch o link mostra |

Se o jogo for hospedado em site novo, a NASA **recomenda** pedir a classificação do site no
**FortiGuard Web Filter** para evitar bloqueio no acesso dos juízes (opcional):

1. Preencher o formulário FortiGuard Web Filter Classification.
2. Categoria: a que melhor descreve o projeto (ex.: *web-based application* ou *education*). A NASA não escolhe por você.
3. Company Name: `N/A`.
4. Enviar. A decisão final é do FortiGuard.

---

## 5. Uso de IA (SI): regras de marcação

IA é permitida, **mas não pode ser usada para criar a ideia de como resolver o desafio**.
Conteúdo gerado **não pode usar nem modificar** identidade visual da NASA. Marcar assim:

| Tipo | Obrigação |
| --- | --- |
| Imagens e vídeos | **Marca d'água visível** indicando geração por IA |
| Áudio | Declarar no texto descritivo **e nos metadados** |
| Código e dados | Declarar no texto descritivo **e nos metadados** |

Os juízes veem as marcas, textos e metadados e avaliam originalidade, intenção e execução
dos elementos feitos com IA.

**[N] O que isso significa para o projeto:**

- [ ] Concepts e imagens feitas no Gemini/IA (`Docs/conceitos`, `ArtSource`, capturas de personagens) que apareçam no jogo, na demo ou na página precisam de **marca d'água visível**
- [ ] Código escrito com ajuda de IA: declarar no README e no campo de SI
- [ ] Preencher o campo "Use of SI" descrevendo onde e como a IA entrou (ferramentas, arte, código)

---

## 6. Passo a passo do envio

1. [ ] Todos os campos preenchidos e **Save Progress** feito
2. [ ] Links de demo e projeto testados em janela anônima
3. [ ] Revisão: tudo em inglês, sem menores de 18 anos, sem marca da NASA, IA marcada
4. [ ] Marcar a caixa **"Terms and Conditions"**
5. [ ] Marcar a caixa **"Confirm All Work is Original"**
6. [ ] Clicar em **Submit for Judging** (sem campo obrigatório, o botão não funciona)
7. [ ] Conferir o **banner de confirmação** no topo da tela
8. [ ] Pode editar e reenviar quantas vezes quiser até 23h59; **vale o último salvo**

**[N]** Enviar uma versão completa cedo e refinar depois. Não deixar o primeiro envio para
os últimos minutos.

---

## 7. Depois do envio: certificado

Só quem está em time que enviou no prazo recebe.

1. Perfil (ícone) → **Profile** → **Participant Information** → "2026 NASA Space Apps Challenge"
2. Botão amarelo **Download Participant Certificate**
3. Papel: **Letter** (8,5x11 pol). Só existe em PDF. Preferir notebook/desktop
4. O nome vem do **Full Name** do perfil (muda em *Account Information* → *Change*; pode mudar depois do envio)
5. Problema técnico: web@spaceappschallenge.org, com usuário e nome do time

---

## 8. Pendências, em ordem de prioridade

1. [ ] Confirmar que todo o time está no **mesmo Local Event** e aceito
2. [ ] Publicar versão jogável com link público (build web)
3. [ ] Tornar o repositório público e adicionar `LICENSE` (Apache 2.0) e `README`
4. [ ] Produzir a **demo** (vídeo ≤ 30 s ou ≤ 7 slides), em inglês, e hospedar publicamente
5. [ ] Redigir em inglês: nome, resumo, descrição detalhada
6. [ ] Preencher **NASA Data** a partir de [DADOS-NASA.md](DADOS-NASA.md)
7. [ ] Preencher **Referências**: todo código, texto, imagem, áudio, pacote e fonte, mesmo livres
8. [ ] Marca d'água em imagens/vídeos de IA; declarar IA em áudio/código; preencher campo SI
9. [ ] Varredura final: menores de 18 anos e identidade visual da NASA
10. [ ] Marcar as 2 caixas e **Submit for Judging**
11. [ ] Baixar o certificado

Outros documentos de apoio: [REGRAS-PARA-GANHAR.md](REGRAS-PARA-GANHAR.md) (critérios de
julgamento e regras de design) e [DESAFIO.md](DESAFIO.md) (enunciado traduzido).
