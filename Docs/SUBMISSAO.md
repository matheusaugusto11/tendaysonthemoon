# Submissão: como enviar e o que falta

Acompanhamento da submissão de **Dez Dias na Lua** no NASA Space Apps Challenge 2026
(desafio *Build a Junior Astronaut Mission Trainer*).

Fonte: [Project Submission Guide](https://www.spaceappschallenge.org/resources/project-submission-guide/),
página de 13/11/2026, relida na íntegra em **15/11/2026** (último dia). Tudo que está
nas seções 1 a 4 é regra oficial. Observações com **[N]** são nossas.

Estado do projeto conferido em **15/11/2026, à noite** (depois do commit `2afe10f`, slides v2):
repositório, página pública do itch.io, slides e código.

> **PRAZO: domingo, 15/11/2026, 23h59**, no fuso do **Local Event** do time (no Universal
> Event, vale o fuso de moradia do Team Owner). Projeto salvo ou enviado depois disso
> **não é julgado** e não gera certificado. Confirmar com o Local Lead se a premiação local
> tem horário diferente.

---

## 1. Requisitos eliminatórios (resumo)

- [ ] Todos os membros são **participantes confirmados no mesmo Local Event** (quem está em lista de espera precisa ser aceito ou trocar de evento). Não dá para conferir pelo repositório: cada um olha no próprio perfil
- [ ] **Todos os campos obrigatórios** preenchidos (sem isso o botão não deixa enviar)
- [ ] Página de submissão e **todo material linkado em inglês** (vídeo pode ser em outra língua **com legenda em inglês**; slides só em inglês). Slides, página do itch.io e jogo (botão EN): ok. Falta o formulário
- [x] Demo dentro do limite: **vídeo de até 30 s OU até 7 slides** (título e introdução contam). Passou do limite = **fora do Global Judging**. São **7 slides**: `Docs/apresentacao/TenDaysOnTheMoon-Demo-v2.pdf`
- [ ] Links de demo e projeto **públicos**, sem login, senha, permissão ou cadastro. itch.io: público. **GitHub: privado** (404 sem login em 15/11, à noite). **Slides: sem link público ainda**
- [x] Pelo menos **1 dado/recurso da NASA** declarado (exigido para Global Award). DONKI no centro do jogo, mais fontes F2 a F23 em [DADOS-NASA.md](DADOS-NASA.md). Falta copiar para o formulário
- [x] **Nenhuma imagem, vídeo, áudio ou semelhança de menor de 18 anos**, na demo e no projeto. Conferido em 15/11: personagens adultos (1,9 m), slides sem foto de pessoa, sem narração
- [x] **Nenhum logotipo, bandeira ou identificador de missão da NASA**, inclusive em conteúdo gerado por IA. Conferido em 15/11: slides, página do itch.io, nomes de arquivo, ícones e textura do astronauta. "NASA" aparece só como texto de fonte
- [ ] Clicar em **"Submit for Judging"** antes do prazo

---

## 2. Campos do formulário (Project tab)

Acesso: ícone de perfil → **My Team** → aba **Project** (aparece só após 14/11, 9h local) →
botão azul **Edit**.

| # | Campo | O que a NASA pede | Status | Observação |
| --- | --- | --- | --- | --- |
| 1 | **Project Name** | Nome do projeto | [ ] | **[N]** usar **Ten Days on the Moon**: é o nome dos slides e da página do itch.io |
| 2 | **High-Level Project Summary** | O que foi desenvolvido, como responde ao desafio, por que importa | [ ] | Em inglês. Curto e direto. O primeiro parágrafo do README serve de base |
| 3 | **Link to Project Demo** | Vídeo ≤ 30 s ou ≤ 7 slides, hospedado fora do site, link público | [ ] | Slides prontos; **falta hospedar** (ver seção 3) |
| 4 | **Link to Final Project** | Link público do projeto (site, repositório, nuvem) | [ ] | **[N]** o itch.io já serve (público, jogável). O GitHub só depois de aberto (ver seção 4) |
| 5 | **Detailed Project Description** | O que faz, como funciona, benefícios, objetivo, ferramentas/linguagens/software usados | [ ] | Em inglês. Citar Unity, versão web, C#, etc. |
| 6 | **Use of Super Intelligence (SI)** | Se usou IA (ML, chatbots, geração de imagem, LLM) e como | [ ] | Ver seção 5. Resposta **não vai para os juízes**, só coleta de dados |
| 7 | **NASA Data** | Detalhes de quais dados/recursos da NASA foram usados | [ ] | Base pronta em [DADOS-NASA.md](DADOS-NASA.md) (fontes F1 a F23: DONKI FLR/SEP/CME, OSDR OSD-268, páginas dos eventos e das tarefas) |
| 8 | **Space Agency Partner Data and Other References** | **Todo** dado, recurso, ferramenta, código, texto e imagem usado, mesmo livre/open source | [ ] | Incluir Unity 2022.3, Blender, fontes Barlow e Nunito (OFL), sons. **Falta autor e link dos 5 sons do Pixabay** ([CREDITOS-AUDIO.md](CREDITOS-AUDIO.md)). Agências parceiras são incentivadas (ex.: Brazilian Space Agency, ESA, JAXA...) |
| 9 | Imagens (opcional) | Upload/colar JPG, PNG, GIF **até 10 MB** | [ ] | **[N]** colocar capturas de `Docs/capturas-base` |

Regras do formulário:

- **Não aceita upload de vídeo nem documento.** Só imagem. Vídeo e slides vão para YouTube, Google Drive, GitHub, OneDrive, Dropbox etc., com link público.
- **Salvar sempre** com o botão azul **Save Progress** (à direita). O que está salvo só é visível ao time até o envio.
- **Lock de edição:** ligar o "Lock" ao editar, para o time saber. Mas qualquer membro pode remover o lock e **sobrescrever** o que não foi publicado. Combinar quem edita.

---

## 3. Demo (vídeo ou slides)

A demo pesa na nota de **Presentation**. A maior parte da avaliação vem do projeto em si.

Escolher **um** formato: [ ] vídeo  [x] slides

**Formato escolhido: slides.** Arquivos em `Docs/apresentacao/`: `TenDaysOnTheMoon-Demo-v2.pdf` (o que vai para
o link) e `.pptx`. A v1 (`TenDaysOnTheMoon-Demo.pdf`) ficou para trás. O roteiro de vídeo
([apresentacao/ROTEIRO-VIDEO-30S.md](apresentacao/ROTEIRO-VIDEO-30S.md)) só vale se trocarem de formato.

**Vídeo (≤ 30 s)**, roteiro sugerido pela NASA:

| Tempo | Conteúdo |
| --- | --- |
| 0:00 | Seu nome e o nome do time |
| 0:02 | Qual desafio o projeto aborda e o que inspirou a escolha |
| 0:05 | Nome e slogan do projeto |
| 0:10 | Como funciona: imagens/protótipo, experiência do usuário, como dados da NASA e de agências parceiras tornaram possível |
| 0:20 a 0:30 | Futuro: o que a solução pode fazer pelas pessoas, pelo mundo e além |

**Slides (≤ 7, contando título e introdução)** precisam ter:

- [x] Nome do time (slides 1 e 2: Houston, We Have a Game!)
- [x] Nomes dos membros (slide 2: os 6)
- [x] Desafio escolhido (slides 1 e 2)
- [x] Imagem do projeto (slides 1, 4, 5 e 7: capturas do jogo)
- [x] Experiência do usuário (slides 3 e 4: ciclo do dia e escolha com custo)
- [x] Como dados e tecnologia da NASA/agências viabilizam a solução (slides 5 e 6: DONKI e tabela de fontes)
- [x] Fonte legível, poucas palavras por slide, visuais fortes (o slide 6, em tabela, é o mais denso)

Os números e as regras citados nos slides foram conferidos no código em 15/11: 6 células em dia claro, Abrigo
2/1/0, custos da coleta de pedras, alarmes falsos, 1,3 s e 20 min, 1.982 erupções, X9.0 de 03/10/2024 e a tripulação
voltando em segurança.

Checagem final da demo:

- [x] Duração ≤ 30 s ou ≤ 7 slides
- [x] Inglês (ou legenda em inglês no vídeo)
- [x] Sem menores de 18 anos (rosto, voz, nome, imagem)
- [x] Sem logotipo/bandeira/identificador da NASA
- [x] Marca d'água visível em qualquer imagem/vídeo gerado por IA. Rótulo "AI-ASSISTED ART" nas capturas dos slides 1, 4, 5 e 7 e "DIAGRAM · AI-ASSISTED" no 3. No slide 5 os dois rótulos ficam juntos à esquerda, fora do alcance do cartão "It really happened"
- [ ] Link aberto em **janela anônima** sem pedir login (ainda não hospedado)

Ajustes opcionais nos slides:
- Slide 5: "catalog, 2017 to 2026" sugere período contínuo. As 1.982 erupções são de setembro de 2017 e de janeiro de 2024 a novembro de 2026.
- Slide 6 diz que a tela de créditos lista todas as fontes. Isso só vale depois do novo build (seção 4.2).

---

## 4. Link do projeto final

Estado atual do repositório, conferido em 15/11/2026, à noite:

| Item | Situação |
| --- | --- |
| Remoto | `https://github.com/matheusaugusto11/tendaysonthemoon` (branches `main` e `teste`) |
| Visibilidade do repositório | [ ] **privado**: a página, o arquivo bruto e a API do GitHub dão 404 sem login. Abrir antes de usar como link (Settings → General → Danger Zone → Change visibility) ou usar só o itch.io como "Final Project" |
| Versão jogável publicada | [x] build WebGL no itch.io: https://carambo.itch.io/ten-days-on-the-moon (público, roda no navegador, conferido em 15/11). Ajustes da página: seção 4.1 |
| `LICENSE` na raiz | [x] Apache 2.0, na `main` do remoto |
| `README` na raiz | [x] em inglês, na `main` do remoto. Pendente nele: autor e link de cada som do Pixabay |
| `USO-DE-IA.md`, `BuildWebGL.cs` | [x] na `main` do remoto |
| Trabalho em `main` | [x] a `main` tem tudo: a `teste` não tem nenhum commit fora da `main`. O link aponta para a `main` |
| Imagens de IA no repositório | [x] 15/11: as 72 referências do Gemini (`Docs/image-ref`, `ArtSource/EstacaoRoda/ref`, `Assets/refs`) receberam marca d'água visível "AI-generated image (Gemini)". O zip do WhatsApp em `Assets/refs` (cópias sem marca) ficou fora do repositório. `Docs/conceitos` já tinha a marca. **Atenção:** os commits antigos ainda guardam as versões sem marca |
| Restos do skybox removido | `Assets/Real Stars Skybox` ficou só com 3 `.meta` órfãos (o pacote saiu no commit `0bb90f9`). Não pesa, só limpeza |

### 4.1 Página do itch.io: estado em 15/11/2026

Conta **Carambo Studio**, projeto `https://carambo.itch.io/ten-days-on-the-moon` (id de edição 5373321).
Está **pública**, com o zip WebGL (`Builds/tendays-webgl.zip`, 18 MB, marcado para rodar no navegador),
capa e 3 capturas (`Builds/itch/`, pasta ignorada pelo git). Declaração de IA e fórum já salvos.

Falta, tomando como modelo a página do outro projeto da conta, **Chapéu de Papel**
(`https://itch.io/game/edit/5182324`):

| Item | Modelo (Chapéu de Papel) | Ten Days hoje | Fazer |
| --- | --- | --- | --- |
| Descrição | Dois idiomas (começa com "English below"), seções, controles, aviso, créditos | Bilíngue (EN, "Português abaixo ↓", PT), com seções, controles, dados da NASA, uso de IA e licença | **Feito (15/11)**, conferido na página pública |
| Frase curta | Duas frases com gancho | Frase nova | **Feito (15/11)** |
| Tags (10) | alzheimers, brazil, emotional... | **Nenhuma** (conferido na página pública, 15/11 à noite) | `space, nasa, education, kids, moon, resource-management, strategy, science, short, 3d` |
| Genre | | Educational | **Feito** |
| Declaração de IA | | AI Assisted: Code, Graphics, Text, com texto | **Feito**, conferido na página pública |
| Tamanho do embed | 960x540, paisagem | 1280x720, sem orientação | **Feito (15/11)**: 960x540, Landscape |
| Status | Em desenvolvimento | Released | Mantido "Released" (jogo completo) |
| Equipe | | Só "Developed by Carambo Studio" | Pôr os 6 nomes e o nome do time (Houston, We Have a Game!), como nos slides |
| Tema visual (Edit theme) | Fonte Lora, fundo de imagem, tons de papel | Tema claro padrão | **Feito (15/11), sem conferência visual**: ver abaixo |

Tema visual aplicado (botão "Edit theme" na página pública, só para o dono): banner 960x250 (céu, horizonte,
base estilizada e título), fundo de estrelas 1024x1024 que repete, embed BG 960x540, cores BG `#181e34`,
painel `#232841`, texto `#e6e4df`, link `#4dd0e1`, botão `#ffd23f`. As imagens foram desenhadas em canvas
no navegador (a captura do jogo não pôde ser lida pela página). **Conferir na página pública** banner, fundo,
embed e contraste do botão amarelo. Ainda faltam as 10 tags e os nomes da equipe. O vídeo da página é
opcional, já que a demo da submissão é em slides.

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
- **Créditos mudaram depois do build publicado (15/11, à noite):** `Textos.FontesNasa` ganhou o Artemis Base Camp
  (citado no evento "A Terra vai se esconder") e o post da NASA de 01/04/2026 que dá o treino do Canadarm2 a cada
  60 dias (o post de 13/04, único citado antes, não fala dos 60 dias). Ver [DADOS-NASA.md](DADOS-NASA.md), seção 5.
  Para isso aparecer no itch.io: refazer o build, rodar os testes EditMode e subir o zip de novo. Sem isso, o
  slide 6 ("the Credits screen lists them all") fica um pouco à frente do jogo.

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

- [ ] Concepts e imagens feitas no Gemini/IA (`Docs/conceitos`, `ArtSource`, capturas de personagens) que apareçam no jogo, na demo ou na página precisam de **marca d'água visível**. Slides: rótulo em quase todas as imagens (falta no slide 5, seção 3). Repositório: ver seção 4
- [x] Código escrito com ajuda de IA: declarado no [USO-DE-IA.md](../USO-DE-IA.md) (linkado do README), na tela de créditos, no rodapé do jogo e na página do itch.io. Falta só o campo de SI
- [ ] Preencher o campo "Use of SI" descrevendo onde e como a IA entrou (ferramentas, arte, código). Base: [USO-DE-IA.md](../USO-DE-IA.md), que ainda pede para o time **confirmar quais ferramentas de IA foram usadas**

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

Atualizado em 15/11/2026, à noite.

1. [ ] Confirmar que todo o time está no **mesmo Local Event** e aceito
2. [x] Publicar versão jogável com link público (build web): feito no itch.io, ajustes na seção 4.1
3. [x] Commit e push de `LICENSE`, `README`, `USO-DE-IA.md`, céu estrelado e `BuildWebGL.cs`: tudo na `main` do remoto
4. [ ] **Hospedar os slides** (`Docs/apresentacao/TenDaysOnTheMoon-Demo-v2.pdf`) com link público (Google Drive "qualquer pessoa com o link", por exemplo) e testar em janela anônima. Os 7 slides estão prontos (seção 3)
5. [ ] Decidir o **Link to Final Project**: itch.io (já público) ou GitHub. Para o GitHub, abrir o repositório e antes resolver as imagens de IA sem marca d'água (seção 4)
6. [ ] Redigir em inglês: nome (Ten Days on the Moon), resumo, descrição detalhada
7. [ ] Preencher **NASA Data** a partir de [DADOS-NASA.md](DADOS-NASA.md) (F1 a F23)
8. [ ] Preencher **Referências**: todo código, texto, imagem, áudio, pacote e fonte, mesmo livres. **Falta autor e link dos 5 sons do Pixabay**: perguntar ao Daniel, que adicionou os arquivos ([CREDITOS-AUDIO.md](CREDITOS-AUDIO.md))
9. [ ] Confirmar com o time as ferramentas de IA ([USO-DE-IA.md](../USO-DE-IA.md)) e preencher o campo SI
10. [x] Varredura: menores de 18 anos e identidade visual da NASA (conferido em 15/11 nos slides, na página do itch.io e nos arquivos do jogo)
11. [ ] Marcar as 2 caixas e **Submit for Judging**
12. [ ] Baixar o certificado

Opcionais, se sobrar tempo depois do primeiro envio:

- [ ] itch.io: as 10 tags e os nomes da equipe (seção 4.1)
- [ ] Refazer o build WebGL com os créditos novos e subir no itch.io (seção 4.2)
- [ ] Slides: rótulo de IA do slide 5 e período do DONKI (seção 3)

Outros documentos de apoio: [REGRAS-PARA-GANHAR.md](REGRAS-PARA-GANHAR.md) (critérios de
julgamento e regras de design) e [DESAFIO.md](DESAFIO.md) (enunciado traduzido).
