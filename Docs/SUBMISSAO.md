# Submissão: como enviar e o que falta

Acompanhamento da submissão de **Dez Dias na Lua** no NASA Space Apps Challenge 2026
(desafio *Build a Junior Astronaut Mission Trainer*).

Fonte: [Project Submission Guide](https://www.spaceappschallenge.org/resources/project-submission-guide/),
página de 13/11/2026, relida na íntegra em **15/11/2026** (último dia). Tudo que está
nas seções 1 a 4 é regra oficial. Observações com **[N]** são nossas.

Estado do projeto conferido em **15/11/2026, à noite** (depois do commit `ed7a70f`): repositório,
página pública do itch.io, slides e código. Novo build WebGL publicado no itch.io no mesmo dia, por volta das 21h40
(seção 4.2).

**Formulário preenchido na aba Project em 15/11, à noite, mas ainda não salvo nem enviado** (seção 2). A demo passou a
ser um **vídeo** no Google Drive (seção 3). Falta: **Save Progress**, conferir o vídeo, marcar as 2 caixas e
**Submit for Judging**.

> **PRAZO: domingo, 15/11/2026, 23h59**, no fuso do **Local Event** do time (no Universal
> Event, vale o fuso de moradia do Team Owner). Projeto salvo ou enviado depois disso
> **não é julgado** e não gera certificado. Confirmar com o Local Lead se a premiação local
> tem horário diferente.

---

## 1. Requisitos eliminatórios (resumo)

- [x] Todos os membros são **participantes confirmados no mesmo Local Event** (quem está em lista de espera precisa ser aceito ou trocar de evento). Confirmado pelo time em 15/11: os 6 estão no time e participaram presencialmente do mesmo evento. Não dá para conferir pelo repositório (a informação está no perfil de cada um)
- [x] **Todos os campos obrigatórios** preenchidos (sem isso o botão não deixa enviar). Preenchidos em 15/11, à noite, na tela do formulário. **Ainda sem Save Progress**. As 2 caixas de aceite continuam desmarcadas (não contam como campo de texto)
- [ ] Página de submissão e **todo material linkado em inglês** (vídeo pode ser em outra língua **com legenda em inglês**; slides só em inglês). Formulário, página do itch.io, jogo (botão EN) e slides: ok. **Vídeo: idioma e legenda não conferidos**
- [ ] Demo dentro do limite: **vídeo de até 30 s OU até 7 slides** (título e introdução contam). Passou do limite = **fora do Global Judging**. A demo enviada no formulário é o **vídeo** (`TenDaysOnTheMoon-Video.mp4`, Drive): **duração não conferida**. Os 7 slides (`Docs/apresentacao/TenDaysOnTheMoon-Demo-v2.pdf`) cumprem o limite, mas não estão no formulário
- [x] Links de demo e projeto **públicos**, sem login, senha, permissão ou cadastro. itch.io: público. Vídeo no Drive: público (resposta 200 sem login, conferido por `curl` em 15/11). **GitHub: privado** (404 sem login), por isso não foi usado no formulário
- [x] Pelo menos **1 dado/recurso da NASA** declarado (exigido para Global Award). DONKI no centro do jogo, mais fontes F2 a F23 em [DADOS-NASA.md](DADOS-NASA.md). **21 fontes** já digitadas no formulário (falta salvar)
- [ ] **Nenhuma imagem, vídeo, áudio ou semelhança de menor de 18 anos**, na demo e no projeto. Conferido em 15/11 no jogo e nos slides: personagens adultos (1,9 m), slides sem foto de pessoa, sem narração. **Falta conferir o vídeo** (rosto, voz, nome)
- [ ] **Nenhum logotipo, bandeira ou identificador de missão da NASA**, inclusive em conteúdo gerado por IA. Conferido em 15/11: slides, página do itch.io, nomes de arquivo, ícones e textura do astronauta. "NASA" aparece só como texto de fonte. **Falta conferir o vídeo**
- [ ] Clicar em **"Submit for Judging"** antes do prazo

---

## 2. Campos do formulário (Project tab)

Acesso: ícone de perfil → **My Team** → aba **Project** (aparece só após 14/11, 9h local) →
botão azul **Edit**.

Status em 15/11, à noite: **[~] digitado na tela, sem Save Progress**. Textos de apoio em
[TEXTOS-FORMULARIO.md](TEXTOS-FORMULARIO.md) e [CAMPO-USO-DE-IA.md](CAMPO-USO-DE-IA.md).

| # | Campo | O que a NASA pede | Status | Observação |
| --- | --- | --- | --- | --- |
| 1 | **Project Name** | Nome do projeto | [~] | **Ten Days on the Moon** (o formulário vinha com "Project Houston, We Have a Game") |
| 2 | **Summary** | O que foi desenvolvido, como responde ao desafio, por que importa. Limite de 1.500 caracteres | [~] | Em inglês, 594 caracteres |
| 3 | **Project Demonstration** | Vídeo ≤ 30 s ou ≤ 7 slides, hospedado fora do site, link público | [~] | `https://drive.google.com/file/d/1Qdm1Bn3QDk1oY1It4kYZkq4SJUPxgccI/view?usp=sharing` (vídeo MP4). **Conferir duração, idioma e legenda** (seção 3) |
| 4 | **Project** (link final) | Link público do projeto (site, repositório, nuvem) | [~] | `https://carambo.itch.io/ten-days-on-the-moon`. O GitHub é privado e não foi usado |
| 5 | **Project Details** | O que faz, como funciona, benefícios, objetivo, ferramentas/linguagens/software usados | [~] | Editor de texto rico, em inglês, com seções (o que é, como funciona, o que a criança leva, dados da NASA, ferramentas, objetivo). Cita 216 testes EditMode |
| 6 | **Use of Super Intelligence (SI)** | Se usou IA e como. Aqui é uma **lista de opções** para marcar, não texto livre | [~] | Marcadas: *Coding, Debugging, or Troubleshooting*; *Writing, Editing, or Translation*; *Visual Design or Image Creation*. **Não** marcadas: *Ideation or Brainstorming* (a ideia é do time), *Presentation, Video, or Audio* (**se o vídeo ou os slides tiveram ajuda de IA, marcar**), *Building SI/ML Into the Project*. Resposta **não vai para os juízes** |
| 7 | **NASA Data** | Pares "Link Text" + "Link Url" com todo dado/recurso da NASA | [~] | **21 linhas**: DONKI (FLR, SEP, CME), OSDR OSD-268 e as páginas dos eventos e das tarefas (F2 a F23 de [DADOS-NASA.md](DADOS-NASA.md)) |
| 8 | **Space Agency Partner & Other Data** | Pares "Link Text" + "Link Url" com **todo** dado, recurso, ferramenta, código, texto e imagem usado, mesmo livre/open source | [~] | **7 linhas**: Unity, Blender, fontes Barlow e Nunito (OFL), sons do Pixabay (uma linha só, com o link da licença), Claude e Gemini. **Falta autor e link de cada um dos 5 sons** ([CREDITOS-AUDIO.md](CREDITOS-AUDIO.md)). Nenhuma agência parceira usada |
| 9 | Imagens (opcional) | Upload/colar JPG, PNG, GIF **até 10 MB** | [ ] | Não enviadas. **[N]** capturas em `Docs/capturas-base` |

Regras do formulário:

- **Não aceita upload de vídeo nem documento.** Só imagem. Vídeo e slides vão para YouTube, Google Drive, GitHub, OneDrive, Dropbox etc., com link público.
- **Salvar sempre** com o botão azul **Save Progress** (à direita). **Atenção:** o texto do próprio formulário diz que o que é salvo fica visível a quem abrir a página do time, antes do envio (a nossa anotação anterior, "só visível ao time", estava errada). Salvar não envia.
- **Submit for Judging** fica logo abaixo do Save Progress, no mesmo painel. Só clicar nele no envio final.
- **Armadilha do campo de links:** em NASA Data e em Referências, cada "Add New Link" cria uma linha vazia. Linha vazia no fim pode travar o envio, então apagar com o X.
- **Lock de edição:** ligar o "Lock" ao editar, para o time saber. Mas qualquer membro pode remover o lock e **sobrescrever** o que não foi publicado. Combinar quem edita.

---

## 3. Demo (vídeo ou slides)

A demo pesa na nota de **Presentation**. A maior parte da avaliação vem do projeto em si.

Escolher **um** formato: [x] vídeo  [ ] slides

**Formato no formulário: vídeo** (decisão de 15/11, à noite): `TenDaysOnTheMoon-Video.mp4` no Google Drive,
`https://drive.google.com/file/d/1Qdm1Bn3QDk1oY1It4kYZkq4SJUPxgccI/view?usp=sharing`. O link abre sem login.
**Eu não assisti ao vídeo**: duração, idioma/legenda, menores de 18 anos, marca da NASA e marca de IA precisam ser
conferidos por alguém do time (checagem abaixo).

Os slides continuam prontos em `Docs/apresentacao/` (`TenDaysOnTheMoon-Demo-v2.pdf` e `.pptx`, 7 slides) e servem se
trocarem de formato. A v1 (`TenDaysOnTheMoon-Demo.pdf`) ficou para trás. O roteiro de vídeo está em
[apresentacao/ROTEIRO-VIDEO-30S.md](apresentacao/ROTEIRO-VIDEO-30S.md). Os itens da lista de slides e a checagem de
marca d'água dos slides abaixo só valem se os slides forem a demo.

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

Checagem final da demo (**vale para o vídeo**; entre parênteses, o que já se sabe dos slides):

- [ ] Duração **≤ 30 s**, conferida (slides: 7, ok)
- [ ] Inglês, ou legenda em inglês no vídeo (slides: ok)
- [ ] Sem menores de 18 anos (rosto, voz, nome, imagem), inclusive na narração (slides: ok)
- [ ] Sem logotipo/bandeira/identificador da NASA (slides: ok)
- [ ] Marca d'água visível em qualquer imagem/vídeo gerado por IA (slides: rótulo "AI-ASSISTED ART" nas capturas dos slides 1, 4 e 7 e "DIAGRAM · AI-ASSISTED" no 3; **no slide 5, o cartão "It really happened" cobre o rótulo**, e outra pessoa cuida disso). Se o vídeo reaproveita essas imagens ou os conceitos de IA, precisam do rótulo
- [x] Link aberto sem pedir login: o do vídeo respondeu 200 sem login por `curl` em 15/11. Abrir também numa **janela anônima** do navegador
- [ ] Se o vídeo ou os slides tiveram ajuda de IA (roteiro, edição, voz, música), marcar *Presentation, Video, or Audio* no campo de IA do formulário

Ajustes opcionais nos slides:
- Slide 5: desencobrir o rótulo "AI-ASSISTED ART".
- Slide 5: "catalog, 2017 to 2026" sugere período contínuo. As 1.982 erupções são de setembro de 2017 e de janeiro de 2024 a novembro de 2026.
- ~~Slide 6 diz que a tela de créditos lista todas as fontes.~~ Resolvido: o build publicado em 15/11 à noite já
  traz os créditos novos (seção 4.2).

---

## 4. Link do projeto final

Estado atual do repositório, conferido em 15/11/2026, à noite:

| Item | Situação |
| --- | --- |
| Remoto | `https://github.com/matheusaugusto11/tendaysonthemoon` (branches `main` e `teste`) |
| Visibilidade do repositório | [ ] **privado**: a página, o arquivo bruto e a API do GitHub dão 404 sem login. Abrir antes de usar como link (Settings → General → Danger Zone → Change visibility) ou usar só o itch.io como "Final Project" |
| Versão jogável publicada | [x] build WebGL no itch.io: https://carambo.itch.io/ten-days-on-the-moon (público, roda no navegador). Build refeito e reenviado em 15/11, à noite, com os créditos novos (seção 4.2). Ajustes da página: seção 4.1 |
| `LICENSE` na raiz | [x] Apache 2.0, na `main` do remoto |
| `README` na raiz | [x] em inglês, na `main` do remoto. Pendente nele: autor e link de cada som do Pixabay |
| `USO-DE-IA.md`, `BuildWebGL.cs` | [x] na `main` do remoto |
| Trabalho em `main` | [x] a `main` tem tudo da `teste`, e o link aponta para a `main`. **A `teste2` não entrou**: tem 3 commits fora da `main` (balões de fala da Lia e do Bip, correção de compilação em `UIJogo` e `BuildWebGL`). Um merge dela foi feito e desfeito em 15/11, à noite, antes do push. O build publicado não inclui a `teste2` |
| Imagens de IA no repositório | [x] `Docs/conceitos` (12) já tinha rótulo. As 24 imagens de referência (repetidas em `Docs/image-ref`, `Assets/refs` e `ArtSource/EstacaoRoda/ref`, 72 arquivos) receberam "AI-GENERATED IMAGE" em 15/11 (**ainda sem commit**). Ver [CAMPO-USO-DE-IA.md](CAMPO-USO-DE-IA.md). Com isso, abrir o repositório deixa de ser problema de marca d'água |
| Restos do skybox removido | `Assets/Real Stars Skybox` ficou só com 3 `.meta` órfãos (o pacote saiu no commit `0bb90f9`). Não pesa, só limpeza |

### 4.1 Página do itch.io: estado em 15/11/2026

Conta **Carambo Studio**, projeto `https://carambo.itch.io/ten-days-on-the-moon` (id de edição 5373321).
Está **pública**, com o zip WebGL (`Builds/tendays-webgl.zip`, 19,2 MB, marcado para rodar no navegador; reenviado em 15/11, à noite),
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
- Testes EditMode por linha de comando (com o Editor fechado): `Unity.exe -batchmode -nographics -projectPath <pasta> -runTests -testPlatform EditMode -testResults Logs/editmode-results.xml -logFile Logs/editmode-tests.log` (sem `-quit`).
- Teste local: `python -m http.server 8765 --bind 127.0.0.1` dentro de `Builds/WebGL` e abrir http://127.0.0.1:8765.
- Testado no build anterior: menu, troca de idioma, créditos, início da missão Cadete e dentro da página do itch.io. **Não testado**: missão completa até o dia 10, som e toque no celular.
- O batchmode já alterou arquivos de configuração do Unity (`ProjectSettings`, `Assets/Settings/URP-*.asset`, `Piso.mat`). No build de 15/11 à noite não alterou nada, mas convém revisar o `git diff` antes de commitar.

**Build atual (15/11/2026, à noite)**, gerado da `main` em `ed7a70f` e enviado ao itch.io pelo time:

- Inclui os créditos novos de `Textos.FontesNasa`: o Artemis Base Camp (citado no evento "A Terra vai se esconder")
  e o post da NASA de 01/04/2026 que dá o treino do Canadarm2 a cada 60 dias. Ver [DADOS-NASA.md](DADOS-NASA.md),
  seção 5. Os dois textos foram encontrados no arquivo de dados do build. Com isso, o slide 6 ("the Credits screen
  lists them all") vale para o jogo publicado.
- Testes EditMode: **216 de 216 passaram**, sem falhas nem testes pulados.
- Conferido sem navegador: o servidor local entrega todos os arquivos e o `index.html` mantém o canvas em 100%.
  **Este build não foi jogado no navegador** (a extensão do Chrome estava desconectada). Vale abrir a página do
  itch.io e conferir menu, idioma e a tela de créditos.

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

- [x] Concepts e imagens feitas no Gemini/IA que apareçam no jogo, na demo ou na página precisam de **marca d'água visível**. Jogo: nenhuma é carregada. Repositório: as 12 de `Docs/conceitos` já tinham rótulo, e as 24 imagens de referência (repetidas em `Docs/image-ref`, `ArtSource/EstacaoRoda/ref` e `Assets/refs`) receberam "AI-GENERATED IMAGE" em 15/11. **Slides: outra pessoa cuida** (falta no slide 5, seção 3)
- [x] Código escrito com ajuda de IA: declarado no [USO-DE-IA.md](../USO-DE-IA.md) (linkado do README), na tela de créditos, no rodapé do jogo e na página do itch.io. Falta só o campo de SI
- [ ] Preencher o campo "Use of SI". **Texto em inglês pronto** em [CAMPO-USO-DE-IA.md](CAMPO-USO-DE-IA.md), escrito a partir do repositório. Ferramentas **confirmadas pelo time em 15/11: Claude e Gemini**. Falta só colar no formulário

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

Atualizado em 15/11/2026, à noite. Legenda: [x] feito, [~] feito em parte ou digitado e ainda não salvo, [ ] a fazer.

1. [x] Confirmar que todo o time está no **mesmo Local Event** e aceito (confirmado pelo time em 15/11: os 6 no time e presentes no evento)
2. [x] Publicar versão jogável com link público (build web): feito no itch.io, ajustes na seção 4.1
3. [x] Commit e push de `LICENSE`, `README`, `USO-DE-IA.md`, céu estrelado e `BuildWebGL.cs`: tudo na `main` do remoto
4. [x] Hospedar a demo com link público: **vídeo** no Drive, link público conferido (seção 3). Os slides não foram hospedados
5. [x] **Link to Final Project**: itch.io (o GitHub é privado e ficou de fora)
6. [x] Redigir em inglês: nome, resumo e descrição detalhada. Digitados no formulário
7. [x] Preencher **NASA Data** (21 linhas). Digitado no formulário
8. [~] Preencher **Referências**: 7 linhas digitadas. **Falta autor e link dos 5 sons do Pixabay**: perguntar ao Daniel, que adicionou os arquivos ([CREDITOS-AUDIO.md](CREDITOS-AUDIO.md)). Sem resposta a tempo, a linha do Pixabay (com o link da licença) basta
9. [x] Campo de IA: 3 opções marcadas (Claude e Gemini confirmados). Conferir *Presentation, Video, or Audio* (seção 3)
10. [~] Varredura: menores de 18 anos e identidade visual da NASA conferidos no itch.io, no jogo e nos slides. **Falta o vídeo**
11. [ ] **Conferir o vídeo**: ≤ 30 s, inglês ou legenda em inglês, sem menor de 18 anos, sem marca da NASA, IA marcada (seção 3)
12. [ ] **Save Progress** (o formulário está só digitado na tela; ver a nota sobre visibilidade na seção 2)
13. [ ] Marcar as 2 caixas e **Submit for Judging**
14. [ ] Baixar o certificado

Opcionais, se sobrar tempo depois do primeiro envio:

- [ ] itch.io: as 10 tags e os nomes da equipe (seção 4.1)
- [x] Refazer o build WebGL com os créditos novos e subir no itch.io (seção 4.2). Feito em 15/11, à noite. Falta só abrir a página e conferir os créditos
- [ ] Slides (só se forem usados como demo): rótulo de IA do slide 5 e período do DONKI (seção 3)
- [ ] Imagens opcionais do formulário (campo 9)
- [ ] Commit e push das 72 imagens com marca de IA e dos documentos novos (`CAMPO-USO-DE-IA.md`, `TEXTOS-FORMULARIO.md`). O repositório não entra no formulário, então não bloqueia o envio
- [ ] README: trocar "209 EditMode tests" por 216

Outros documentos de apoio: [REGRAS-PARA-GANHAR.md](REGRAS-PARA-GANHAR.md) (critérios de
julgamento e regras de design) e [DESAFIO.md](DESAFIO.md) (enunciado traduzido).
