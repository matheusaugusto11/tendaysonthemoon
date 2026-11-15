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
| Versão jogável publicada | [x] build WebGL no itch.io: https://carambo.itch.io/ten-days-on-the-moon (público, roda no navegador, conferido em 15/11). Ajustes da página: seção 4.1 |
| `LICENSE` na raiz | [x] Apache 2.0, criado em 15/11 (falta commit e push) |
| `README` na raiz | [x] criado em 15/11, em inglês (falta commit e push). Pendente nele: autor e link de cada som do Pixabay |
| Trabalho em `main` | [ ] hoje o trabalho está na branch `teste`; decidir qual branch o link mostra |

### 4.1 Página do itch.io: estado em 15/11/2026

Conta **Carambo Studio**, projeto `https://carambo.itch.io/ten-days-on-the-moon` (id de edição 5373321).
Está **pública**, com o zip WebGL (`Builds/tendays-webgl.zip`, 18 MB, marcado para rodar no navegador),
capa e 3 capturas (`Builds/itch/`, pasta ignorada pelo git). Declaração de IA e fórum já salvos.

Falta, tomando como modelo a página do outro projeto da conta, **Chapéu de Papel**
(`https://itch.io/game/edit/5182324`):

| Item | Modelo (Chapéu de Papel) | Ten Days hoje | Fazer |
| --- | --- | --- | --- |
| Descrição | Dois idiomas (começa com "English below"), seções, controles, aviso, créditos | Um parágrafo em inglês | Reescrever em inglês com versão em português abaixo |
| Frase curta | Duas frases com gancho | "Run a lunar outpost for ten days. Every energy cell has a cost." | Trocar |
| Tags (10) | alzheimers, brazil, emotional... | Nenhuma | `space, nasa, education, kids, moon, resource-management, strategy, science, short, 3d` |
| Tamanho do embed | 960x540, paisagem | 1280x720, sem orientação | **Feito (15/11)**: 960x540, Landscape |
| Status | Em desenvolvimento | Released | Mantido "Released" (jogo completo) |
| Descrição / frase curta | ver acima | um parágrafo | **Feito (15/11)**: bilíngue (EN, "Português abaixo ↓", PT) e frase nova |
| Tema visual (Edit theme) | Fonte Lora, fundo de imagem, tons de papel | Tema claro padrão | **Feito (15/11), sem conferência visual**: ver abaixo |

Tema visual aplicado (botão "Edit theme" na página pública, só para o dono): banner 960x250 (céu, horizonte,
base estilizada e título), fundo de estrelas 1024x1024 que repete, embed BG 960x540, cores BG `#181e34`,
painel `#232841`, texto `#e6e4df`, link `#4dd0e1`, botão `#ffd23f`. As imagens foram desenhadas em canvas
no navegador (a captura do jogo não pôde ser lida pela página). **Conferir na página pública** banner, fundo,
embed e contraste do botão amarelo. Ainda faltam as 10 tags, o campo Genre, o vídeo e os nomes da equipe.

Armadilhas já vividas:
- **Digitar na caixa de tags trava a aba** (diálogo do navegador que a automação não fecha). Digitar as tags à mão.
- Valores colocados por script nos campos **não são salvos**. Usar a interface.
- O botão de upload abre o seletor do Windows. Dá para contornar na aba: trocar `HTMLInputElement.prototype.click` para não abrir o seletor e enviar o arquivo ao campo `input[type=file]` criado.
- O tipo de projeto, o status e a visibilidade são menus personalizados (clicar, não selecionar por valor).
- Imagens do tema (Banner, Background, Embed BG): o botão "Upload" abre o seletor do Windows. Contornar trocando `HTMLInputElement.prototype.click` para não abrir o seletor, guardar o `input[type=file]` criado, montar o `File` na página (canvas, `toBlob`), pôr em `input.files` via `DataTransfer` e disparar `change`. Servidor local (127.0.0.1) não responde à página.
- No editor de tema, o seletor "Repeat" do fundo não guarda valor mudado por script (volta a "repeat"), e o fundo "Fixed" faz as capturas de tela travarem. Por isso o fundo é uma imagem que repete sem emenda.
- Capturas de tela da página pública do jogo costumam dar timeout (renderer travado); conferir pelo HTML ou a olho.
- O jogo abre em português mesmo com o navegador em inglês: conferir `Navegador.LinguaInicial` (o campo de idioma é lido do navegador).

### 4.2 Build WebGL

- Gerar: fechar o Editor do Unity e rodar `Unity.exe -batchmode -nographics -quit -projectPath <pasta> -executeMethod BuildWebGL.Executar -logFile Logs/build-webgl.log` (menu "Dez Dias > Build WebGL" no Editor). Saída em `Builds/WebGL` (ignorada pelo git). Leva uns 4 minutos.
- O script ajusta o `index.html` para o canvas ocupar o iframe todo (o modelo padrão cortava a HUD em 960x600).
- Zip para o itch.io: precisa ter `index.html` na raiz do zip e barras normais nos caminhos (usar Python `zipfile`, não o `Compress-Archive` do PowerShell 5.1).
- Testado: menu, troca de idioma, créditos, início da missão Cadete e dentro da página do itch.io. **Não testado**: missão completa até o dia 10, som e toque no celular.
- O batchmode alterou arquivos de configuração do Unity (`ProjectSettings`, `Assets/Settings/URP-*.asset`, `Piso.mat`): revisar o `git diff` antes de commitar.

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
2. [x] Publicar versão jogável com link público (build web): feito no itch.io, ajustes na seção 4.1
3. [ ] Tornar o repositório público e fazer commit e push de `LICENSE`, `README`, `USO-DE-IA.md`, céu estrelado e `BuildWebGL.cs` (arquivos já criados)
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
