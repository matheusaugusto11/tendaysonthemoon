# Modelagem no Blender: Estação-Roda

Guia de produção, passo a passo, de cada modelo que já tem referência em `Docs/image-ref`. Segue o método
da skill `blender-producao` (6 etapas, uma por vez, cada uma com aprovação) e a bíblia visual aprovada em
[ART-BIBLE.md](ART-BIBLE.md).

**Estado:** rascunho para avaliação, atualizado em 14/11/2026 com as suas três respostas. Nenhum arquivo foi criado no Blender. Nada daqui está aprovado.
**Estilo:** low-poly estruturado e realista, nada futurista, nada de massinha.
**Fonte das imagens:** `Docs/image-ref/` (índice em [INDICE.md](../image-ref/INDICE.md)). Gerar por IA: se uma
imagem entrar no jogo, leva marca d'água e registro em `USO-DE-IA.md`.

---

## 0. Resumo e o que preciso que você decida

O documento tem 18 receitas (R00 a R17), uma por modelo, e as leis que valem para todas: dimensões, conexões e
organização. A ordem de trabalho segue a regra do projeto: **personagens, depois props, depois cenário**.

### Decisões registradas (14/11/2026)

| # | Pergunta | Resposta | O que muda |
| --- | --- | --- | --- |
| 1 | Versão do Blender | **5.3** | O arquivo precisa abrir no 5.3. Ver a nota técnica abaixo: o gerador roda no **5.2.1**, e o `.blend` abre no 5.3 |
| 2 | Quem modela | **Eu, por script** (`fonte/construir.py`) | A seção 6.5 descreve o caminho por script. As receitas da seção 7 viram a especificação que o código implementa |
| 3 | Acompanhar o processo técnico | **Sim, em detalhe** | Cada processo abre com duas linhas de como vai ser feito; a tabela 4.9 traz o processo técnico de cada peça |

**Nota técnica sobre a versão (testado hoje nesta máquina).** O Blender 5.3 (versão da Microsoft Store, `5.3.0`) roda o
auditor e o exportador sem problema, mas **trava (segfault) ao renderizar prévias em segundo plano**, em qualquer
modo. O 5.2.1 renderiza normalmente, inclusive um arquivo salvo no 5.3. Sem prévia eu não "vejo" o resultado, então
**decidi gerar com o 5.2.1**: um `.blend` salvo no 5.2 abre no 5.3 (versão mais nova abre arquivo mais velho), e
a regra da skill é gerar na versão mais antiga que a equipe usa. Cada entrega diz a versão ("Hub.blend, Blender 5.2.1,
abre no 5.3"). Se você preferir salvar no 5.3, o auditor e o export funcionam lá, mas fico sem prévia.

### Perguntas que continuam em aberto

1. **Aprova os refinamentos A a F e as duas divergências da seção 3.4?** (a tabela abaixo)
2. **Atlas do Bip e das telas** (`T_BipFace_BC`, `T_Screens_BC`): eu gero por script, com ícones simples e sem texto, ou você traz imagens?
3. **Aparelho-alvo do WebGL** (celular de entrada, tablet ou PC) para confirmar o orçamento.

### Refinamentos de arquitetura que as referências pedem (a aprovar)

As referências novas mostram detalhes que a bíblia ainda não fixava. Proponho estes ajustes, todos medidos
na seção 4.6:

| # | Ajuste | Motivo |
| --- | --- | --- |
| A | O **pivô do módulo é o centro da escotilha de ligação, no chão**, a 16,0 m do centro do hub, e o corredor tem 10,0 m (de 6,0 a 16,0). Antes: módulo a 19 m do centro | Todo módulo e todo corredor se encaixam com a mesma transformação, sem conta por módulo |
| B | O **hub é um decágono de apótema 6,0 m** (10 faces de 3,9 m). Cada corredor sai do centro de uma face | 5 corredores a 72° e a eclusa a 180° caem exatamente no centro de faces, e o teto pentagonal fecha com 5 vidros (referência `cena-04`) |
| C | **Eclusa** como anexo do hub a 180° (de frente para a câmera), sem corredor | Precisa existir para as tarefas externas (`int-08-eclusa`); 180° é o único ponto livre entre Bateria (144°) e Abrigo (216°) |
| D | **Ar e água é uma casa de máquinas fechada**, com 3 tanques atravessando o teto, tanque horizontal e ponte de tubos por fora | `ext-01` mostra pátio aberto e `int-03` mostra sala fechada; juntei os dois |
| E | **Bateria = 4 contêineres**: 1 de controle (entrável) e 3 de bateria (um por célula guardada, 0 a 3, com LED) | `ext-05` e `int-07` batem com a regra de 0 a 3 células guardadas do GDD |
| F | **Piso interno a z = +0,40 m e eixo das escotilhas a z = 1,78 m em todo o projeto** | Corredor, hub e módulos alinham sem degrau |

---

## 1. O que está aprovado (a bíblia em uma página)

| Tema | Regra |
| --- | --- |
| Estilo | Low-poly limpo, faces grandes e planas, bordas chanfradas (1 segmento), sem ruído de textura |
| Realismo | Equipamento plausível: módulos pressurizados, túneis, tanques, painéis, escotilhas, corrimãos. **Sem naves, hologramas ou objetos flutuando** |
| Cor | Corpo branco e cinza, estrutura em aço escuro, **um acento por módulo** em faixas, portas e tiras de luz |
| Acentos (cores da interface) | Ar e água `#4DD0E1`, Estufa `#6BD425`, Abrigo `#C77DFF`, Laboratório `#FF9F43`, Bateria `#FFD23F` |
| Estados (tabela 3.3 do GDD) | Desligado (0 células): acento apagado e tiras escuras. Ligado (1): tiras acesas, 1 encaixe aceso. Turbo (2): tudo aceso, 2 encaixes |
| Terreno | Facetado (low-poly talhado), cinza `#807D8B` a `#55525F`, rochas e espigões facetados |
| Luz | Sol baixo duro, 12° de altura (licença artística; o real no polo sul é até 1,5°, NASA F2, F3, F5), sombra definida, preenchimento frio |
| Escala | 1 unidade = 1 m. Astronauta 1,8 m, 4 cabeças de altura, traje EVA branco com viseira dourada, **sem rosto** |
| Bip | Robô de esteiras com tela de rosto. A expressão vive no Bip e nos retratos da interface |
| Proibido | Massinha, vetor, contorno preto, futurismo, foto realista com textura pesada |

---

## 2. Etapa 1: Escopo (devolvido para confirmação)

- **O que é:** a base lunar do jogo *Dez Dias na Lua*: hub central, 5 corredores, 5 módulos, eclusa, área de apoio
  e terreno. O jogador **anda por dentro** de tudo.
- **Onde roda:** Unity 2022.3.62f3, URP 14.0.12, build WebGL, para crianças de 7 a 12 anos.
- **O que se mexe:** astronauta e Bip (animados), prato do laboratório (gira), plantas (crescem), painéis e LEDs
  (estados). O resto é estático.
- **O que já existe:** referências geradas (`Docs/image-ref`), bíblia visual e documento de câmeras.
- **O que não pode mudar:** cores da interface, os 5 módulos, o hub com corredores, o trajeto de no máximo 5 s.
- **Aparelho e fps:** navegador, alvo de 60 fps (a confirmar no aparelho-alvo; ver pendência 1).

Perguntas abertas: as três do início. O resto do escopo está respondido pelo projeto.

---

## 3. Etapa 2: Referências

### 3.1 Imagem por modelo

| Modelo | Referências |
| --- | --- |
| Astronauta (4 tripulantes) | `char-01-tripulacao` |
| Bip | `char-02-bip` |
| Painel de energia e célula | `int-09-painel-energia-3-estados` |
| Parede de vazamento | `int-10-parede-vazamento` |
| Corredor | `int-02-corredor-ciano`, `cena-03-hub-exterior` |
| Hub | `cena-03-hub-exterior`, `int-01-hub`, `cena-04-hub-malha-wireframe` (topologia) |
| Eclusa | `int-08-eclusa` |
| Ar e água | `ext-01-ar-e-agua-on-off`, `int-03-ar-e-agua` |
| Estufa | `ext-02-estufa`, `int-04-estufa` |
| Abrigo | `ext-03-abrigo-on-off`, `int-05-abrigo` |
| Laboratório | `ext-04-laboratorio-on-off`, `int-06-laboratorio` |
| Bateria | `ext-05-bateria-niveis`, `int-07-bateria` |
| Apoio (solar, pouso, rover, prato, contêineres, dutos) | `cena-05-apoio-solar-pouso-rover` |
| Terreno, rochas, espigões | `cena-06-terreno-formacoes` |
| Montagem e câmeras | `cena-01-geral-base` (versão a usar), `cena-02-geral-alternativa` |
| Luz por clima (para o motor) | `cena-07-tres-climas` |

### 3.2 Princípios observáveis

1. **Blocos chanfrados e poucos vértices**: faces grandes, bordas com 1 segmento de chanfro, quinas duras.
2. **Branco, preto de estrutura e um acento** por peça; o acento aparece em faixa, moldura de porta e tira de luz.
3. **Detalhe legível**: escada, corrimão, escotilha com roda, tanque com cinta, antena, venezianas. Nada fino
   demais para a tela.
4. **Interior com faixa escura no pé da parede**, juntas de painel, luz dura entrando pela janela e a Terra
   visível lá fora.
5. **Terreno facetado** com planos grandes e rochas talhadas.

### 3.3 O que fica de fora

Detalhe abaixo de 2 a 3 px na tela do aparelho, texto e símbolos dentro da geometria, parafusos pequenos (só os
de 4 faces onde a câmera chega perto), cabos soltos modelados um a um (viram trilho de cabos).

### 3.4 Divergências entre referências (a resolver antes do blockout)

- `cena-01` e `cena-02` divergem no teto do hub (cinza e laranja) e na posição do abrigo. **Proposta:** usar o
  teto pentagonal de `cena-04` com moldura cinza, e o abrigo na posição de 216° (`cena-01`).
- `int-01` mostra o teto do hub com mais janelas que o pentágono de `cena-04`. **Proposta:** seguir `cena-04`
  (centro pentagonal e 5 vidros), por ser a referência de malha.

---

## 4. Etapa 3: Plano

### 4.1 Intenção

Uma base que o jogador **entende de longe pela silhueta e pela cor** e que **convence de perto** como estrutura de
verdade: cada módulo se lê por componentes reais, cada estado se lê pela luz.

### 4.2 Orçamento

Ponto de partida: perfil `web` da skill (200 mil triângulos, 20 mil por objeto, 60 fps), apertado porque o
WebGL pesa também no download.

```json
{
  "nome": "Estação-Roda (WebGL)",
  "observacoes": "UV0 colapsada no atlas de paleta: densidade de texel não se aplica. Sem lightmap: luz direcional em tempo real com sombra.",
  "fps_alvo": 60,
  "tris_total": 120000,
  "tris_por_objeto": 12000,
  "materiais_por_objeto": 4,
  "textura_max_px": 256,
  "textura_px": 256,
  "ossos_max": 24,
  "influencias_max": 4,
  "shape_keys_max": 0,
  "exigir_uv2": false,
  "nomes": { "MESH": "^(SM|SK|COL)_", "ARMATURE": "^RIG_", "CAMERA": "^CAM_" }
}
```

Origem dos números e dois estouros **justificados** (vão para o LEIAME):

- **120 mil no quadro:** estrutura, módulos, apoio e terreno cerca de 87 mil e personagens cerca de 20 mil, com folga de 13 mil (tabela abaixo).
- **4 materiais por objeto (perfil pede 2):** `Palette` (todas as cores neutras, uma textura de 256²), `Accent_<Módulo>`
  (a cor do módulo, que **muda em tempo de execução** para o estado desligado), `Glow_<Módulo>` (luz emissiva do
  estado) e `Glass`. Sem isso o estado não se controla por módulo. São materiais de cor chapada, quase sem custo.
- **UV1 não exigida:** sem luz assada; o jogo usa uma direcional com sombra.

| Item | Triângulos | Cópias visíveis | Total |
| --- | --- | --- | --- |
| Hub (com plaza) | 8.000 | 1 | 8.000 |
| Corredor | 1.500 | 5 | 7.500 |
| Eclusa | 2.500 | 1 | 2.500 |
| Ar e água | 8.000 | 1 | 8.000 |
| Estufa (com 6 canteiros e plantas) | 12.000 | 1 | 12.000 |
| Abrigo | 5.000 | 1 | 5.000 |
| Laboratório | 8.000 | 1 | 8.000 |
| Bateria (4 contêineres, transformador) | 9.000 | 1 | 9.000 |
| Painéis solares | 120 | 40 | 4.800 |
| Pouso e módulo de carga | 2.100 | 1 | 2.100 |
| Prato, rover, contêineres, dutos | 6.000 | 1 | 6.000 |
| Terreno, rochas, espigões | 14.000 | 1 | 14.000 |
| Astronauta | 4.500 | 4 | 18.000 |
| Bip | 2.500 | 1 | 2.500 |
| **Total** | | | **≈ 107.000** |

Interiores **não somam** com o exterior: ao entrar, o teto e o exterior do módulo saem do quadro.

### 4.3 Superfície: paleta, materiais e estados

**Caminho:** atlas de paleta (cor pela UV), o caminho de "paleta" da skill. Motivo: low-poly de cor chapada em
WebGL, quase zero de memória, e o detalhe vem da forma e da luz.

**Atlas:** `T_Palette_BC.png`, 256×256, grade de 8×8 ladrilhos de 32 px. Cada ladrilho é uma cor chapada. A
UV0 de cada face é **colapsada** no centro do ladrilho (o auditor lê isso como atlas). Você modela com
materiais nomeados `MAT_Pt_<nn>_<Nome>`; um script (`fonte/paleta_uv.py`, a escrever na etapa 5) converte o
material em UV de ladrilho. Assim você modela pensando em cor, sem desdobrar nada.

| Ladrilho | Nome | Cor | Uso |
| --- | --- | --- | --- |
| 01 | Branco_Pintura | `#E6E4DF` | Cascos, trajes, painéis |
| 02 | Cinza_Claro | `#BFC0C4` | Equipamentos, mochila, caixas |
| 03 | Cinza_Medio | `#9A98A0` | Concreto, bases |
| 04 | Aco_Claro | `#7C828C` | Tubos, corrimãos, escadas |
| 05 | Aco_Escuro | `#2E323B` | Estrutura, faixa do pé da parede |
| 06 | Borracha | `#1E2026` | Esteiras, rodas |
| 07 | Concreto_Piso | `#8E8C95` | Plaza, pisos |
| 10 | Rocha_A | `#85828F` | Rochas |
| 11 | Rocha_B | `#9692A2` | Rochas |
| 12 | Rocha_C | `#75727F` | Rochas |
| 13 | Regolito_Berma | `#77747F` | Berma do abrigo |
| 20 | Solo_Canteiro | `#4A3A2E` | Terra dos canteiros |
| 21 a 23 | Planta_1, 2, 3 | `#2F7A22`, `#4FA832`, `#7FD14A` | Cultivo |
| 24 | Espiga | `#F2B84B` | Trigo |
| 30 | Painel_Solar | `#2B5C9E` | Células do painel |
| 32 | Dourado_Viseira | `#C9A24A` | Viseira |
| 33 | Agua_Tanque | `#3FA9E0` | Tanques de água do abrigo |
| 34 | Caixa_Verde | `#5C7A3A` | Caixas de suprimento |
| 37 | Amarelo_Seguranca | `#E8B923` | Faixas de segurança |
| 40 | Bip_Banda | `#8FB4FF` | Banda do Bip |
| 41 | Tela_Escura | `#14182E` | Moldura de telas |

**Materiais fora do atlas** (cor muda em tempo de execução ou precisa de efeito):

| Material | O que é | Notas |
| --- | --- | --- |
| `MAT_Accent_Air`, `_Green`, `_Shelter`, `_Lab`, `_Battery` | Tinta de acento do módulo | Cor sólida. No estado desligado o Unity dessatura |
| `MAT_Accent_Crew` | Acento do tripulante | Uma cor por prefab (ciano, verde, violeta, laranja) |
| `MAT_Glow_<Módulo>` | Tiras, janelas e LEDs acesos | Emissivo. Estado muda a intensidade e a cor |
| `MAT_Glow_Bateria_C1`, `_C2`, `_C3` | LED de cada contêiner | Um por contêiner (0 a 3 células guardadas) |
| `MAT_Glass` | Vidro teal `#5FB3C4`, 35% | Único transparente. Origem do objeto no centro dele |
| `MAT_Terrain_VC` | Terreno por cor de vértice | `#807D8B` a `#55525F` |
| `MAT_Screen` | Telas de monitor (atlas `T_Screens_BC`, 256², sem texto) | P1 |
| `MAT_Bip_Screen` | Tela do Bip (atlas `T_BipFace_BC`, 256², 6 expressões em grade 3×2) | Emissivo |

### 4.4 Movimento

| Quem | Tipo | Clipes (30 fps) | Root motion |
| --- | --- | --- | --- |
| Astronauta (4 variantes, 1 rig) | **Humanoid** (reaproveita animações entre os 4) | `Idle` 60 loop, `Run` 24 loop, `Interact` 20, `Task_Dust` 40 loop, `Task_Water` 40 loop, `Task_Leak` 40 loop, `Celebrate` 30, `Shrug` 24, `Sit` 60 loop, `Climb` 24 loop (P2) | No lugar; o código move |
| Bip | **Generic** | `Idle` 60 loop, `Talk` 30 loop, `Point` 24, `Alert` 30 loop, `Spin_Happy` 36, `Scan` 48 loop | No lugar |
| Prato do laboratório | Animação de objeto | `Lab_Prato_Girar` 120 loop | No lugar |
| Escotilhas | Animação de objeto | `Escotilha_Abrir` 30, `Escotilha_Fechar` 30 | No lugar |
| Plantas | Escala por estágio no Unity | sem clipe | |
| Rover (P2) | Rodas giram no Unity | `Rover_Rodar` 24 loop | No lugar |

Esteiras do Bip: textura rolando no Unity (sem ossos). Pontas e dedos: luva-luva em bloco, sem ossos de dedo.

### 4.5 Estrutura: arquivos, coleções, nomes, pivôs

**Pasta do projeto:** `ArtSource/EstacaoRoda/` (fora de `Assets`, como no padrão do projeto anterior).

```
ArtSource/EstacaoRoda/
├── perfil.json            orçamento (seção 4.2)
├── LEIAME.md              andamento, escopo, referências, plano, orçamento × atual
├── fonte/                 construir.py, config.json, _comum.py, paleta_uv.py, kit.py
├── ref/                   cópia dos nomes de Docs/image-ref usados
├── tex/                   T_Palette_BC.png, T_Screens_BC.png, T_BipFace_BC.png
├── export/                FBX por asset (_v001)
└── renders/               prévias e folhas
```

**Um `.blend` por grupo** (o skill pede um por grupo de peças; kits compartilhados são *linkados*):

| Arquivo | Conteúdo |
| --- | --- |
| `Kit_Comum.blend` | escotilha, corrimão, escada, grade de piso, janelas, armário, bancada, bandeja de cabos, válvula, poste de luz, respiro, caixa |
| `Props_Energia.blend` | painel de energia, célula, parede de vazamento |
| `Personagens.blend` | astronauta (+ acessórios), Bip |
| `Hub.blend`, `Corredor.blend`, `Eclusa.blend` | estrutura central |
| `Mod_Air.blend`, `Mod_Green.blend`, `Mod_Shelter.blend`, `Mod_Lab.blend`, `Mod_Battery.blend` | os 5 módulos |
| `Apoio.blend` | painel solar, pouso, módulo de carga, prato, rover, contêiner, duto |
| `Terreno.blend` | terreno, rochas, espigões |
| `Montagem.blend` | só instâncias de coleção, na posição final. **Não exporta**; serve para conferir a arquitetura |

**Coleções (Outliner)**, em todo `.blend`:

```
00 Referencia     planos de imagem e medidas — nunca exporta
01 Blockout       primitivas e boneco de escala — some quando a arte entra
02 <Asset>        o asset exportável (ex.: 02 Hub)
03 Colisao        COL_*
04 Sockets        empties SOCKET_* (pontos de jogo)
_Teste            prefixo _ = fora do export
```

**Nomes** (padrão da skill; sem acento, sem espaço, PascalCase, nada de `.001`):

| Tipo | Padrão | Exemplo |
| --- | --- | --- |
| Malha estática | `SM_<Asset>_<Parte>` | `SM_Hub_Casco`, `SM_Estufa_Vidro` |
| Malha com rig | `SK_<Asset>` | `SK_Astronaut`, `SK_Bip` |
| Armadura | `RIG_<Asset>` | `RIG_Astronaut` |
| Colisão | `COL_<Asset>_<Parte>` | `COL_Hub_Piso` |
| Material | `MAT_<Nome>` | `MAT_Pt_01_Branco_Pintura` |
| Textura | `T_<Asset>_<Mapa>` | `T_Palette_BC` |
| Ponto de jogo | `SOCKET_<Asset>_<Função>` | `SOCKET_Hub_Porta_Air` |
| Câmera | `CAM_<nn>_<Plano>` | `CAM_02_Mapa` |
| Luz | `LUZ_<Função>` | `LUZ_Sol` |
| Ação | `<Asset>_<Ação>` | `Astronaut_Run` |

Peças que o Unity esconde ou troca viram **objetos separados**: `_Teto` (some ao entrar), `_Vidro`, `_Glow`
(estado), `_Interior`. O nome da malha (data) acompanha o do objeto.

**Eixos e transformações:** 1 unidade = 1 m; Z para cima; frente do asset para **−Y** (vira +Z no Unity); escala
aplicada (1,1,1); nenhuma escala negativa. **Pivô** na escotilha de ligação (módulos, eclusa e corredor), no
centro do hub (hub e plaza), no centro da base no chão (props soltos) e no eixo de giro (prato, escotilha).

### 4.6 Lei arquitetônica: dimensões e conexões

Tudo o que for modelado obedece esta seção. O auditor e a Montagem conferem.

**Níveis e eixos comuns**

| Item | Medida |
| --- | --- |
| Chão do terreno | z = 0 |
| Piso interno de tudo (hub, corredores, módulos, eclusa) | **z = +0,40** |
| Eixo de corredores e escotilhas | **z = 1,78** |
| Escotilha | vão livre Ø 2,2; moldura 0,25 |
| Corredor | interno Ø 3,0; parede 0,15; externo Ø 3,3; **comprimento 10,0** (de r = 6,0 a r = 16,0) |
| Piso do corredor | grade de 1,2 m de largura a z = 0,40 |
| Passagem livre | ≥ 1,2 m de largura e ≥ 2,2 m de altura em todo caminho; corredor de 2,0 m nos salões |
| Corrimão | tubo Ø 0,05 a 1,0 m acima do piso |
| Escada de degraus | 0,20 de altura × 0,25 de profundidade, largura 1,0 |
| Escada vertical | degraus a cada 0,30, largura 0,5 |
| Janela | peitoril a 1,1 m acima do piso |
| Parede | 0,30 nos módulos; 0,50 no hub |
| Grade modular | 0,5 m (medidas em múltiplos de 0,05) |

**Hub**

| Item | Medida |
| --- | --- |
| Forma | decágono; **apótema 6,0 externo e 5,5 interno** (circunraio 6,31 e 5,78) |
| Altura | parede de z 0,4 a 3,4 por fora; interno livre de **2,6 nas paredes** (forro a z = 3,0) e **4,3 no centro** (forro a z = 4,7) |
| Teto | tronco de pirâmide de 10 lados, claraboia pentagonal no centro, 5 vidros trapezoidais |
| Plaza | disco de 24 lados, **R = 10,0**, espessura 0,4 |
| Faces | 0°, 72°, 144°, 216°, 288° = corredores (Ar, Estufa, Bateria, Abrigo, Laboratório); **180° = eclusa**; 36°, 108°, 252°, 324° = janela redonda Ø 0,8 |

Ângulos medidos no sentido horário a partir do fundo da cena (+Y). **Posição = (R·sen θ, R·cos θ)**, **rotação Z = −θ**.

| Peça | θ | Pivô (R = 16,0) | Rotação Z |
| --- | --- | --- | --- |
| Ar e água | 0° | (0,000; 16,000) | 0° |
| Estufa | 72° | (15,217; 4,944) | −72° |
| Bateria | 144° | (9,405; −12,944) | −144° |
| Abrigo | 216° | (−9,405; −12,944) | −216° (= +144°) |
| Laboratório | 288° | (−15,217; 4,944) | −288° (= +72°) |
| Eclusa (R = 6,0) | 180° | (0,000; −6,000) | −180° |

O **corredor tem o mesmo pivô e a mesma transformação do módulo** e se estende para −Y local, de y = 0 a y = −10. O
corpo do módulo se estende para +Y local.

**Envelopes dos módulos** (cheio × livre por dentro)

| Peça | Externo X × Y × Z | Interno livre X × Y × Z | Observação |
| --- | --- | --- | --- |
| Ar e água | 7,6 × 5,6 × 5,3 (tanques até z = 7,6) | 7,0 × 5,0 × 4,6 | Mezanino a z = 2,8; 3 tanques Ø 2,6 atravessando o teto |
| Estufa | 12,2 × 7,2 × 5,0 (cumeeira) | 11,6 × 6,6 × 2,8 laterais e 4,6 na cumeeira | 6 canteiros de 3,0 × 1,3 |
| Abrigo | 6,6 × 8,6 × 3,5, berma até z = 4,4 | 6,0 × 8,0 × 2,8 na coroa | Abóbada semienterrada |
| Laboratório | 7,6 × 5,0 × 3,8 (equipamentos até z = 6,5) | 7,0 × 4,4 × 3,0 | Rampa de serviço lateral decorativa |
| Bateria, controle | 3,8 × 6,1 × 3,0 | 3,4 × 5,7 × 2,6 | Entrável |
| Bateria, 3 contêineres | 2,44 × 6,1 × 2,6 cada | não entrável | LED de estado |
| Eclusa | 3,3 × 3,0 × 3,0 | 2,7 × 2,5 × 2,6 | Escotilha externa a 3,0 m do hub |

**Verificações de encaixe** (rodar na Montagem): nenhuma interpenetração entre módulos vizinhos; distância de
64 cm entre a eclusa e os corredores vizinhos no hub; todo piso interno a z = 0,40 ±0,005; todo eixo de escotilha
a z = 1,78 ±0,005; passagem de 1,2 m livre em todo caminho.

### 4.7 Entrega

FBX por asset (um arquivo por coleção `02 <Asset>`), Unity URP, opções do `exportar.py` (seção 8). Texturas em
`tex/`, normal não usada. Versão de entrega numerada `_v001`.

### 4.8 Processo técnico (vale para todas as receitas)

- **Tipo de modelagem:** *box modeling* a partir do blockout (arquitetura e hard surface). Terreno e rochas por
  deslocamento de malha. Personagens por box modeling com `Mirror`.
- **Quads, nenhum N-gon.** Triângulo só em tampa pequena escondida, fundo que a câmera não vê. Nenhum vértice
  sem motivo (silhueta, deformação ou costura). Nenhuma face interna, nenhum vértice duplicado.
- **Chanfro:** `Bevel` de **1 segmento**, largura 0,02 a 0,05 m (peças de perto), `Limit Method: Angle` 30°.
  Chanfro maior só em peças grandes (0,08 a 0,12).
- **Sombreamento:** `Shade Smooth` por ângulo de 30° (no 4.3, *Shade Auto Smooth* ou *Smooth by Angle*, conforme
  o menu da versão) mais `Weighted Normal` (Keep Sharp ligado). **Terreno e rochas: `Shade Flat`.**
- **Cilindros:** 8 lados para peças pequenas, 12 para médias, 16 para tanques; acima de 16 só com motivo no LEIAME.
- **Loops de deformação** (só os personagens): articulação que dobra leva 3 loops; polos fora das juntas.
- **Costuras de UV:** como o UV0 é colapsado no atlas, não há costura de textura. Só o **Bip** (tela) e as **telas**
  de monitor têm UV desdobrada.
- **Repetição:** `Array` para corrimão, ripas, painéis, venezianas e canteiros. Aplicar os modificadores na cópia
  de entrega, nunca no arquivo de trabalho sem avisar.

### 4.9 Processo técnico em detalhe, por peça

Como a skill manda: tipo de modelagem e ponto de partida, segmentos e o motivo, loops e polos, onde fica triângulo,
UV e a estimativa de triângulos. Em todas, o **UV0 é colapsado no atlas de paleta** (sem costuras de textura),
salvo onde dito.

| Peça | Ponto de partida | Segmentos (motivo) | Loops e polos | Triângulos aceitos | UV |
| --- | --- | --- | --- | --- | --- |
| Astronauta | *Box modeling* com `Mirror`, torso em caixa | Capacete 12 × 8 (silhueta redonda de perto); membros 8 lados | 3 loops em ombro, cotovelo, punho, quadril e joelho; polos do capacete no topo e no queixo, fora da viseira | Ponta da bota e topo do capacete (~120) | Colapsada |
| Bip | Esfera UV 14 × 10 escalada | Corpo 14 × 10 (lê como ovo a 10 m); braços 8 lados | 3 loops no cotovelo; polos da esfera na antena e embaixo | Ponta da antena, fundo | **Tela desdobrada** (atlas 3 × 2); resto colapsada |
| Kit comum | Primitivas e *box modeling* | Cilindros de 8 a 12 lados conforme a distância | Sem articulação | Tampas pequenas e escondidas | Colapsada |
| Painel e célula | Caixa com `Inset` | Soquete 12 lados; célula 8 lados | Sem articulação | Tampa dos parafusos | Colapsada |
| Parede de vazamento | Caixa e peças do kit | Flange 12 lados, parafuso 4 faces | Sem articulação | Nenhum | Colapsada |
| Corredor | Tubo extrudado de um círculo | 12 lados (a câmera chega a 1,6 m por dentro) | Anéis a cada 2,0 m; sem polo no tubo | Nenhum (as pontas fecham no anel de escotilha) | Colapsada |
| Hub | Decágono extrudado, teto por `Scale` | 10 lados (6 conexões caem no centro de faces); plaza 24 | Polos do pentágono central, escondidos sob a viga da claraboia | Nenhum visível | Colapsada |
| Ar e água | Caixa e perfil de revolução | Tanque 16 lados (peça grande, vista de perto) | Polo da calota no topo, coberto pela escotilha de visita | Nenhum | Colapsada |
| Estufa | Caixas e vigas finas, `Array` | Vigas em caixa; vidro em plano | Sem polos relevantes | Nenhum | Colapsada |
| Abrigo | Meia elipse de 12 segmentos extrudada | 12 (abóbada lê lisa a 8 m) | Sem polos | **Regolito em triângulos, por design** (facetado, `Shade Flat`) | Colapsada |
| Laboratório | Caixa chanfrada | Prato 12 × 4 | Polo da calota coberto pelo alimentador | Nenhum | Colapsada |
| Bateria | Caixas chanfradas e `Array` | Ventilador 10 lados | Sem polos | Nenhum | Colapsada |
| Eclusa | Caixa chanfrada | Roda de 6 raios | Sem polos | Nenhum | Colapsada |
| Campo solar, pouso, rover, dutos | Primitivas e *box modeling* | Rodas 10 lados, postes 8 | Sem articulação | Nenhum | Colapsada |
| Prato de antena | Calota 12 × 4 | 12 (borda lê circular) | Polo no centro, coberto | Nenhum | Colapsada |
| Terreno | Grade 64 × 64 com `Displace` e `Decimate` | Triângulos pelo `Decimate` | Sem deformação | **Triângulos por design** (facetado, `Shade Flat`, cor por vértice) | Sem UV de textura |
| Rochas e espigões | Icosfera nível 1 e cilindro de 6 a 8 lados | 6 a 8 lados | Sem deformação | **Triângulos por design** | Colapsada |

---

## 5. Etapa 4: Blockout

**Por quê:** forma, escala e composição se resolvem aqui, com primitivas, antes de qualquer detalhe.

**Antes do primeiro `.blend`:** versão do Blender gravada em `fonte/config.json` (pendência 1), pasta do projeto
montada e `perfil.json` na raiz.

**Fazer, em todo `.blend`:**

1. Cena nova, unidade métrica, `scale_length = 1`. Crie as coleções da seção 4.5.
2. Em `01 Blockout`: um **boneco de escala** de 1,8 m (cilindro de 0,5 de largura) e um **cubo de 1 m**.
3. Primitivas em escala real, **na posição e rotação finais**, usando só as medidas da seção 4.6.
4. Câmeras do jogo, como objetos `CAM_*` na coleção `01 Blockout`, com **FOV vertical** (*Sensor Fit: Vertical*):
   `CAM_01_Andar` atrás e acima do boneco a 2,5 m, 25° de inclinação, FOV 45; `CAM_02_Mapa` 62°, FOV 45;
   `CAM_03_Close` baixa, três quartos, FOV 35; `CAM_04_Interior` a 1,6 m de altura, 28 mm.
5. Registre no log os triângulos por bloco, com o orçamento ao lado.

**Conferir** (cada checagem com imagem aberta e crítica): `previa.py --modo solido --folha`, `--cameras` e uma prévia
no tamanho da tela do aparelho. Perguntas: a silhueta diz o que é? A escala bate com o boneco? Os encaixes
(escotilhas, pisos) estão alinhados?

**Fechamento:** peça aprovação do blockout. Aprovado, a forma trava: mudar proporção depois refaz paleta, rig e
pesos.

---

## 6. Etapa 5: Produção

### 6.1 Ordem dos lotes

| Lote | Conteúdo | Motivo |
| --- | --- | --- |
**Decisão de 14/11/2026: o cenário vem antes dos personagens.** O blockout dos personagens (`Personagens.blend`) fica guardado e parado.

| Lote | Conteúdo | Motivo |
| --- | --- | --- |
| C (blockout) | Cenário inteiro em caixas: hub, 5 corredores, eclusa, 5 módulos, apoio, terreno | Confere a arquitetura antes de gastar triângulo |
| L3 | Estrutura: R05 corredor, R06 hub, R12 eclusa | A espinha da arquitetura; tudo se mede contra ela |
| L2 | Props e kit: R00 kit, R01 painel e célula, R02 parede de vazamento, R13 a R15 apoio | Os módulos reaproveitam o kit |
| L4 | Módulos: R07 a R11 | Entram sobre o kit e a estrutura |
| L5 | Terreno: R16 | Depende da pegada da base |
| L6 | Montagem e export: R17 | Confere a arquitetura inteira |
| L1 | Personagens: R03 astronauta, R04 Bip | Em espera; retomam depois do cenário |

### 6.2 Os processos de cada lote

1. **Modelagem** (seção 4.8 e a receita). 2. **Paleta e UV.** 3. **Materiais e estados.** 4. **Rig e pesos**
(só personagens). 5. **Animação** (só personagens, prato e escotilhas). 6. **Cenário, luz e câmera** (só na Montagem).

### 6.3 O ciclo dentro de cada processo

rodar → `previa.py --folha` → **abrir a imagem** → criticar como diretor → `auditar.py --perfil perfil.json` →
corrigir → repetir. Só mostre quando o auditor não tiver ERRO.

| Processo | O que mostrar ao fechar |
| --- | --- |
| Modelagem | Folha `solido`, `arame` e `--cameras`; triângulos por peça contra o orçamento; nenhum N-gon |
| Paleta e UV | Folha `xadrez`; UV0 colapsada nos ladrilhos certos; telas e Bip com UV desdobrada |
| Materiais e estados | Folha `textura` e `eevee`; os 4 materiais por objeto; estados ligado, desligado e turbo |
| Rig e pesos | Poses extremas em `solido` e `arame`; ossos e influências contra o perfil |
| Animação | Animatic para você e tira para mim; lista de clipes com duração e loop |
| Montagem | Storyboard `--cameras todas --folha` e as verificações de encaixe |

**Cada processo fecha com a sua confirmação.** Uma confirmação não autoriza o processo seguinte. Mudar a forma
depois do UV refaz o UV; mudar a malha depois dos pesos refaz os pesos.

### 6.4 Sequência padrão de modelagem de um módulo

As receitas só dizem o que é específico de cada peça. O resto é sempre isto:

1. **Blockout aprovado** como guia (`01 Blockout`).
2. **Casco:** caixa ou cilindro com as medidas externas; `Solidify` de 0,30 (ou o valor da receita) para a parede.
3. **Aberturas:** escotilha (círculo de 12 lados, vão Ø 2,2), janelas e portas com `Inset` + `Extrude` para dentro.
4. **Estrutura:** vigas, quadros e treliças com *edge extrude*, `Skin` ou caixas finas; repetição com `Array`.
5. **Chanfro:** `Bevel` de 1 segmento nas arestas que aparecem; `Weighted Normal`.
6. **Interior:** piso a z = 0,40, faixa escura no pé da parede (inset de 0,60 de altura), equipamentos do kit.
7. **Separar** em objetos: `_Casco`, `_Teto`, `_Interior`, `_Vidro`, `_Glow`, e as peças móveis.
8. **Materiais:** atribuir `MAT_Pt_*`, `MAT_Accent_*`, `MAT_Glow_*`, `MAT_Glass` por face ou objeto.
9. **Sockets e colisão:** empties `SOCKET_*` e malhas simples `COL_*`.
10. **Conferir:** auditor, prévia e a tabela de triângulos.

### 6.5 Caminho por script (decidido: eu construo)

O `.blend` é **resultado**; o código é a fonte da verdade. Refazer um modelo é rodar de novo; mudar uma medida é
mudar o `config.json`. Nada fica "feito à mão" sem estar no código.

```
ArtSource/EstacaoRoda/fonte/
├── config.json          medidas, quantidades, cores, versão ("blender": "5.2"), semente
├── _comum.py            da skill (args, log, etapa, material_novo, ...)
├── kit.py               formas: caixa chanfrada, cilindro chanfrado, tanque de revolução, calota, anel, Array
├── paleta.py            tabela de ladrilhos e a conversão MAT_Pt_* → UV do atlas (paleta_uv)
├── hierarquia.py        coleções, nomes, pivôs, empties de socket, malhas de colisão
├── personagens.py       astronauta e Bip (malha, rig, pesos, ações)
├── estrutura.py         corredor, hub, eclusa
├── modulos.py           os 5 módulos (uma função por módulo)
├── apoio.py             painel solar, pouso, prato, rover, contêiner, duto
├── terreno.py           terreno, rochas, espigões (semente fixa)
├── montagem.py          instâncias nas posições da seção 4.6 e as verificações de encaixe
└── construir.py         orquestra por lote: `--lote L1` ... `L6`
```

- Geometria com **bmesh** (controle total, quads garantidos); `bpy.ops` só onde não há alternativa (UV, pesos
  automáticos, export). Aleatoriedade com semente fixa (`random.Random(11)`).
- Cada lote gera os seus `.blend` (um por grupo, seção 4.5) e registra no log os triângulos por peça contra o orçamento.
- Rodar: `BLENDER_VERSAO=5.2 rodar_blender.sh fonte/construir.py -- --cfg fonte/config.json --lote L1`.
- Conferir: `auditar.py --perfil perfil.json`, `previa.py --folha` (eu abro a imagem e critico) e, no fim, `exportar.py --conferir`.
- Nunca gravo por cima de arquivo seu: se você editar um `.blend` à mão, a próxima rodada vira `_v002` e eu te pergunto antes
  de refazer a parte editada.

---

## 7. Receitas por modelo

Cada receita tem: referências, medidas, objetos, passos específicos, materiais, sockets e colisão, estados,
triângulos e critério de aceite. Os passos 1 a 10 da seção 6.4 valem em todas; a receita só descreve a
diferença.

### R00 Kit comum (`Kit_Comum.blend`)

**Referências:** `int-02`, `int-03`, `int-06`, `int-08`, `cena-03`. **Papel:** peças reutilizadas em todos os módulos; um
objeto por peça, **exportado uma vez** e montado no Unity como prefab.

| Peça | Medida | Como modelar | Tri |
| --- | --- | --- | --- |
| `SM_Kit_Escotilha` | Ø externo 2,7, vão Ø 2,2, profundidade 0,3 | Anel de 12 lados com `Solidify` e `Bevel`; 4 parafusos de 4 faces; acento no anel | 300 |
| `SM_Kit_Escotilha_Porta` | Ø 2,1 | Disco de 12 lados + roda de 6 raios; pivô na dobradiça; clipe `Escotilha_Abrir` | 250 |
| `SM_Kit_Corrimao_1m` | altura 1,0 | 2 montantes 0,05 × 0,05 + tubo Ø 0,05 de 8 lados; `Array` para 2 m | 80 |
| `SM_Kit_Escada_Vertical` | 2,4 × 0,5 | 2 trilhos + 8 degraus; `Array` | 140 |
| `SM_Kit_Escada_Degrau` | 5 degraus | Rampa em degraus de 0,20 × 0,25 com corrimão | 160 |
| `SM_Kit_Grade_Piso_1x1` | 1 × 1 | Plano com `Inset` em retícula (ripas), faixa de borda escura | 60 |
| `SM_Kit_Bandeja_Cabos_1m` e `_Curva` | 0,4 × 0,08 | Calha com 2 tubos Ø 0,06 | 70 |
| `SM_Kit_Valvula` | Ø 0,5 | Roda de 8 raios em tubo; acento | 90 |
| `SM_Kit_Poste_Luz` | 3,2 de altura | Mastro de 8 lados + cabeça em caixa chanfrada; `_Glow` separado | 70 |
| `SM_Kit_Respiro` | 0,6 × 0,6 | Caixa com 6 venezianas | 60 |
| `SM_Kit_Janela_Redonda` | Ø 0,7 | Anel + vidro | 90 |
| `SM_Kit_Janela_Retangular` | 1,5 × 1,0 | Moldura de aço + vidro | 70 |
| `SM_Kit_Armario` | 0,9 × 0,5 × 2,0 | Caixa chanfrada, 2 portas, 4 venezianas | 120 |
| `SM_Kit_Bancada` | 2,0 × 0,8 × 0,9 | Tampo + gavetas + pés | 110 |
| `SM_Kit_Caixa_P/M/G` | 0,6 / 1,0 / 1,6 | Caixa chanfrada com 2 cintas | 40 a 80 |

**Materiais:** `MAT_Pt_01`, `MAT_Pt_04`, `MAT_Pt_05`; peças com acento usam `MAT_Accent_Neutral` (troca no
prefab). **Colisão:** nenhuma (usa-se a do módulo). **Aceite:** cada peça no tamanho da tabela; pivô no ponto
de montagem; nenhum N-gon.

### R01 Painel de energia e célula (`Props_Energia.blend`)

**Referências:** `int-09-painel-energia-3-estados`. **Papel:** ponto onde o jogador entrega as células. **Um por ambiente.**

**Medidas:** 0,9 (L) × 0,8 (A) × 0,18 (P). Montado a 1,2 m do piso, ao lado da porta.

**Objetos:** `SM_Painel_Corpo`, `SM_Painel_Soquete_1`, `SM_Painel_Soquete_2` (cada um com `_Glow` próprio),
`SM_Painel_Led`, `SM_Celula`.

**Modelagem:**
1. **Corpo:** caixa chanfrada (0,02) de aço escuro; moldura branca 0,04 mais larga por cima (`Inset` e
   `Extrude`); faixa de acento de 0,16 de altura no meio (modelada em **faces separadas** com `MAT_Accent_*`).
2. **Soquetes:** dois círculos de 12 lados, Ø 0,34, afundados 0,08; anel de borda de aço (Ø 0,40). O fundo é
   escuro; o `_Glow` é um disco fino a 0,03 de profundidade.
3. **Alavanca** embaixo e **4 parafusos** de 4 faces nos cantos.
4. **LEDs de status:** 2 caixas de 0,03 no canto superior (verde e vermelho).
5. **Célula:** cilindro de **8 lados**, Ø 0,30 × 0,16, com aro escuro e faces amarelas (`MAT_Glow_Bateria`).
   Encaixa no soquete; é o mesmo objeto que voa na bandeja do hub e na mão do astronauta.

**Estados** (3 da imagem): ambos soquetes apagados; 1 célula, 1 LED verde; 2 células, brilho forte.
**Sockets:** `SOCKET_Painel_Encaixe_1`, `_2` (no centro dos soquetes). **Triângulos:** 300 (painel) e 60 (célula).
**Aceite:** a célula entra no soquete com 0,01 de folga; os dois estados se leem a 5 m.

### R02 Parede de vazamento (`Props_Energia.blend`)

**Referências:** `int-10-parede-vazamento`. **Papel:** a tarefa Remendar vazamento (GDD 4.5): 6 pontos suspeitos, um solta vapor.

**Medidas:** seção de parede 3,0 × 2,4 × 0,3, ligada ao piso a z = 0,40 como as paredes dos módulos.

**Objetos:** `SM_Parede_Vazamento` (casco e faixa escura no pé), mais as 6 peças nos pontos:

| Ponto | Peça | Modelagem |
| --- | --- | --- |
| 1 | Junta de painel | Placa de 0,5 × 0,6 com 8 parafusos de 4 faces |
| 2 | Vedação de janela | Moldura retangular 1,2 × 0,8 com borracha (anel preto) |
| 3 | Flange de tubo | Disco de 12 lados Ø 0,5 + tubo Ø 0,14 + 6 parafusos |
| 4 | Vedação de escotilha | Anel Ø 0,6 com roda pequena |
| 5 | Painel de rebites | Placa 0,6 × 0,5 com 2 × 4 rebites |
| 6 | Respiro | Grade com 5 venezianas |

**Sockets:** `SOCKET_Vazamento_01` a `_06` (empties no centro de cada ponto). O jato de vapor é **VFX do Unity**, não
malha. **Triângulos:** 1.000. **Aceite:** os 6 pontos se distinguem a 2 m; nenhum depende de texto.

### R03 Astronauta (`Personagens.blend`)

**Referências:** `char-01-tripulacao`. **Papel:** a tripulação (Lia, Téo, Nina, Kai). **1 malha e 1 rig, 4 prefabs** (só o acento muda).

**Medidas:** 1,8 m, **4 cabeças** (capacete Ø 0,46); A-pose; pés em z = 0; origem no chão entre os pés; frente −Y.

**Objetos:** `SK_Astronaut` (malha única), `RIG_Astronaut`, acessórios como `SM_Crew_*` filhos de ossos.

**Modelagem** (box modeling com `Mirror` no eixo X):
1. **Torso:** caixa 0,46 × 0,30 × 0,55; *loops:* 1 no peito, 1 no abdômen, 1 na cintura (3 para o quadril).
2. **Capacete:** esfera de 12 × 8; viseira = calota separada por `Duplicate` e `Separate`, deslocada 0,01, `MAT_Pt_32_Dourado`;
   aro do pescoço (anel 12 lados) em `MAT_Accent_Crew`.
3. **Mochila:** caixa chanfrada 0,40 × 0,22 × 0,50 com 2 tubos laterais; faixa de acento.
4. **Braços:** 2 segmentos, **3 loops no ombro, 3 no cotovelo, 3 no punho**; mão em *mitten* (caixa 0,10) com polegar.
5. **Pernas:** 2 segmentos, **3 loops no quadril e 3 no joelho**; bota em caixa com sola escura.
6. **Painéis do traje:** `Inset` raso nas placas do peito, ombro e coxa; o acento do tripulante vai na placa do ombro e
   no aro do pescoço.
7. **Acessórios** (todos objetos separados, filhos do osso da mão ou da cintura): Lia: cinto de ferramentas, chave
   inglesa nas costas, 2 lâmpadas no capacete. Téo: maleta de amostras no quadril. Nina: mochila médica com cruz.
   Kai: tablet no antebraço e antena de prato no capacete.

**Rig (Humanoid):** `Hips`, `Spine`, `Chest`, `Neck`, `Head`, `UpperArm.L/R`, `LowerArm.L/R`, `Hand.L/R`,
`UpperLeg.L/R`, `LowerLeg.L/R`, `Foot.L/R`, `Toes.L/R`, `Root` no chão (**20 ossos**, perfil 24). Pesos por
`Armature With Automatic Weights`, limpeza com `Limit Total` 4 e `Normalize All`. Teste de poses: braço para cima,
joelho a 120°, pescoço virado, em `solido` e `arame`.

**Materiais:** `Palette` (traje, mochila, bota, viseira), `MAT_Accent_Crew`, `MAT_Glow_Crew` (lâmpadas e tela do
tablet). **Triângulos:** 4.500 (capacete 400, viseira 200, torso 700, mochila 600, braços 1.000, pernas 1.100, botas
400, detalhes 100). **Animação:** lista da seção 4.4.
**Aceite:** 4 prefabs se distinguem pela silhueta e pelo acento; auditor sem peso solto; clipes em loop.

### R04 Bip (`Personagens.blend`)

**Referências:** `char-02-bip`. **Papel:** o robô ajudante, dá as falas. **Rig Generic.**

**Medidas:** altura 1,1 m (antena até 1,25), largura 0,7; pivô no chão entre as esteiras; frente −Y.

**Objetos:** `SK_Bip`, `RIG_Bip`, `SM_Bip_Tela` (UV desdobrada), `SM_Bip_Esteira_L/R`.

**Modelagem:**
1. **Corpo ovoide:** esfera UV 14 × 10 escalada (0,62 × 0,55 × 0,95). Duas faixas de painel por `Inset` (cintura e
   peito); a banda azul (`MAT_Pt_40_Bip_Banda`) é uma faixa de faces inteira.
2. **Tela** no terço superior: placa chanfrada 0,34 × 0,22, ligeiramente saliente; UV desdobrada para o atlas
   `T_BipFace_BC` (3 × 2 expressões); material `MAT_Bip_Screen` emissivo.
3. **Esteiras:** caixa chanfrada 0,22 × 0,8 com 5 rodas de 10 lados à vista; sem ossos (a textura rola no Unity).
4. **Braços:** 2 segmentos de cilindro de 8 lados, 3 loops no cotovelo; **garra** de 3 caixas.
5. **Antena:** cilindro de 6 lados e ponta de esfera 6 × 4 emissiva (`_Glow`).

**Rig:** `Root`, `Body`, `Head`, `Antenna`, `UpperArm.L/R`, `LowerArm.L/R`, `Claw.L/R` (**11 ossos**).
**Triângulos:** 2.500. **Animação:** `Idle`, `Talk`, `Point`, `Alert`, `Spin_Happy`, `Scan`.
**Aceite:** lê-se como Bip a 10 m de distância; a tela mostra as 6 expressões sem texto.

### R05 Corredor (`Corredor.blend`)

**Referências:** `int-02-corredor-ciano`, `cena-03-hub-exterior`. **Papel:** o deslocamento. **1 malha, 5 prefabs** (muda só o acento).

**Medidas** (seção 4.6): interno Ø 3,0, externo Ø 3,3, comprimento 10,0 de y = 0 a y = −10. Piso a z = 0,40, eixo a z = 1,78.

**Objetos:** `SM_Corredor_Casco`, `SM_Corredor_Interior`, `SM_Corredor_Vidro` (vigias), `SM_Corredor_Glow` (tira de luz).

**Modelagem:**
1. **Tubo:** círculo de 12 lados, `Extrude` 10 m, `Solidify` 0,15.
2. **Anéis de reforço:** 5 anéis de 0,20 de largura e 0,10 de saliência (`Loop Cut` + `Extrude` ou `Array`), a cada 2,0 m.
3. **Piso:** grade de 1,2 m a z = 0,40 (kit), trilho de cabos ao lado esquerdo (kit).
4. **Corrimãos** dos dois lados a 1,4 m do eixo (kit).
5. **Tira de luz** de 0,30 de largura no teto, 9,6 m (objeto `_Glow`).
6. **Vigias:** 2 de cada lado (y = −2,5 e −7,5), Ø 0,7 (kit), com vidro.
7. **Extremidades:** moldura de escotilha (kit) nas duas pontas; a do lado do módulo em `MAT_Accent_*`.
8. **Apoios:** 3 cavaletes em A (caixas finas) sob o tubo, a cada 3 m. Calha de cabos sob o piso.

**Materiais:** `Palette`, `MAT_Accent_*` (moldura e anel), `MAT_Glow_*`, `MAT_Glass`.
**Sockets:** `SOCKET_Corredor_Modulo` (y = 0) e `SOCKET_Corredor_Hub` (y = −10). **Colisão:** `COL_Corredor_Piso` (plano) e 2
paredes laterais em caixa. **Triângulos:** 1.500. **Estados:** a tira de luz acende com o módulo ligado.
**Aceite:** comprimento 10,0 ±0,01; o piso a z = 0,40; a escotilha com vão Ø 2,2.

### R06 Hub (`Hub.blend`)

**Referências:** `cena-03-hub-exterior`, `int-01-hub`, `cena-04-hub-malha-wireframe` (topologia). **Papel:** ponto de partida; as células saem
daqui e o Bip fala. **Pivô no centro, no chão.**

**Medidas:** seção 4.6 (decágono de apótema 6,0 e 5,5; piso a z = 0,40; teto até 4,7 no centro; plaza R = 10,0).

**Objetos:** `SM_Hub_Plaza`, `SM_Hub_Casco`, `SM_Hub_Teto`, `SM_Hub_Vidro`, `SM_Hub_Interior`, `SM_Hub_Console`,
`SM_Hub_Piso_Guia` (5 faixas coloridas), `SM_Hub_Glow` (tiras de teto e janelas).

**Modelagem:**
1. **Plaza:** círculo de 24 lados, `Extrude` 0,4; `Bevel` 0,06.
2. **Casco:** decágono de apótema 6,0; `Extrude` 3,0; `Solidify` 0,5 para dentro. As **faces em 0°, 72°, 144°, 216°,
   288° e 180°** recebem a escotilha (kit), com anel de acento nos 5 corredores e anel neutro na eclusa. As faces
   restantes recebem janela redonda de Ø 0,8.
3. **Teto:** da borda do casco (z 3,4) sobe até z = 4,7: `Extrude` + `Scale` do anel até um **pentágono central** de
   raio 2,0. Os 5 trapézios entre o anel e o pentágono viram **vidros** (objeto `_Vidro`); as divisórias são vigas escuras.
   Siga a malha de `cena-04`: pentágono mais 5 trapézios.
4. **Parapeito do teto:** corrimão (kit) em volta da borda de z = 3,4, interrompido sobre as escotilhas.
5. **Interior:** piso decagonal a z = 0,40 com juntas de 1 × 1 (inset); **faixa escura** de 0,60 no pé da parede; 5
   armários (kit) nas 4 faces de janela e 1 ao lado da eclusa; 10 vigas de teto.
6. **Console central:** anel de 20 lados (R externo 1,9, interno 1,1, altura 0,95), tampo `Bevel`, **tela larga** em arco
   no lado norte (`MAT_Screen`), **bandeja** no centro com 6 soquetes de célula (octógonos, `SM_Celula`); degrau de
   R 2,4 e 0,15 de altura em volta.
7. **Guias de piso:** 5 faixas de 0,25 de largura saindo do console até cada escotilha, em `MAT_Accent_*`; inset de
   0,01 sobre o piso.
8. **Luzes:** 5 tiras de teto brancas (`SM_Hub_Glow`) e as janelas redondas.
9. **Por fora:** 2 postes de luz (kit) na plaza; calha de cabos saindo de cada corredor (kit).

**Sockets:** `SOCKET_Hub_Spawn` (centro, z = 0,40), `SOCKET_Hub_Bandeja_1` a `_6`, `SOCKET_Hub_Porta_Air`, `_Green`,
`_Battery`, `_Shelter`, `_Lab`, `_Eclusa` (centro de cada escotilha, z = 1,78, sentido para fora).
**Colisão:** `COL_Hub_Piso`, `COL_Hub_Parede` (10 caixas), `COL_Hub_Console` (cilindro).
**Triângulos:** 8.000 (casco 3.200, teto 1.200, vidro 120, interior 2.300, console 900, plaza 280).
**Estados:** as tiras acendem com o dia; o resto é fixo.
**Aceite:** 6 escotilhas centradas nas faces; apótema 6,0 ±0,01; teto pentagonal igual a `cena-04`; passagem livre ao redor do console de 1,5 m.

### R07 Ar e água (`Mod_Air.blend`)

**Referências:** `ext-01-ar-e-agua-on-off`, `int-03-ar-e-agua`. **Papel:** oxigênio; vazamento. **Acento:** `#4DD0E1`.

**Medidas:** seção 4.6. Casa de máquinas externa 7,6 × 5,6 × 5,3; interna 7,0 × 5,0 × 4,6; mezanino a z = 2,8; 3 tanques Ø 2,6.

**Objetos:** `SM_Air_Casco`, `SM_Air_Teto`, `SM_Air_Interior`, `SM_Air_Tanques`, `SM_Air_Equipamento`, `SM_Air_Ponte`,
`SM_Air_Vidro`, `SM_Air_Glow`.

**Modelagem:**
1. **Casco:** caixa 7,6 × 5,6; `Solidify` 0,30; escotilha na parede da frente (y = 0), centralizada (kit). **Teto** em
   laje de 0,30 com 3 furos Ø 2,7.
2. **Tanques:** perfil de revolução (cilindro de 16 lados com calota arredondada, 2 anéis de loop) `Ø 2,6`, altura de z = 0,40 até 7,6;
   atravessam o teto. **Faixa de acento** de 0,60 a z = 3,0; **2 cintas** escuras; escotilha de visita no topo.
3. **Interior:** piso a z = 0,40; as bases dos tanques sobem pelo piso com colar de aço; **mezanino** de 7,0 × 1,0 a z = 2,8
   no fundo, corrimão e escada (kit); **tubulação** (cilindros de 8 lados, em `MAT_Accent_Air`) ligando os tanques,
   com 4 válvulas (kit); **2 bombas** e **2 filtros** em quadros de aço; **armário de controle** (kit) com tela
   `MAT_Screen`; 3 janelas altas na parede direita; respiros.
4. **Exterior** (como `ext-01`): **tanque horizontal** Ø 2,1 × 4,6 em 2 selas, à frente à esquerda; **ponte de tubos**
   horizontal a z = 6,4 ligando os 3 tanques; escada de serviço até o teto; **chaminé** Ø 0,5 × 1,7 com `SOCKET_Air_Vapor`.
5. **Painel de energia** (R01) na parede esquerda perto da porta e **parede de vazamento** (R02) na parede direita.

**Sockets:** `SOCKET_Air_Porta`, `SOCKET_Air_Painel`, `SOCKET_Air_Vazamento`, `SOCKET_Air_Vapor`, `SOCKET_Air_Spawn`.
**Colisão:** `COL_Air_Piso`, `COL_Air_Mezanino`, `COL_Air_Parede`, `COL_Air_Tanques`.
**Estados:** desligado: acento escuro, tiras apagadas, sem vapor; ligado: tiras ciano acesas e vapor lento; turbo: tudo
aceso e vapor forte (VFX). **Triângulos:** 8.000. **Aceite:** o interior bate com `int-03` (tanques atravessando o teto, mezanino, bombas);
tanques com 3 cintas legíveis de longe.

### R08 Estufa e plantas (`Mod_Green.blend`)

**Referências:** `ext-02-estufa`, `int-04-estufa`. **Papel:** regar e colher. **Acento:** `#6BD425`.

**Medidas:** externo 12,2 × 7,2; paredes laterais a z = 3,2 (2,8 acima do piso); cumeeira a z = 5,0 (refinamento G: a verga da escotilha chega a z = 2,88, então a parede de 2,6 não fechava); 5 vãos de 2,4.

**Objetos:** `SM_Estufa_Casco`, `SM_Estufa_Estrutura`, `SM_Estufa_Vidro`, `SM_Estufa_Interior` (canteiros), `SM_Estufa_Glow`,
mais as plantas `SM_Planta_E0` a `E4` (estágios).

**Modelagem:**
1. **Piso e paredes baixas:** laje de 0,4; paredes de 0,26 e **2,2 de altura**, faixa de acento verde de 0,30 a z = 1,0;
   **faixa escura** no pé (z 0,4 a 1,0), como `int-04`.
2. **Estrutura:** 6 quadros em A (colunas, caibros e tirante) em x = −5,9, −4,8, −2,4, 0, 2,4, 4,8 (+ extremos), vigas
   de 0,14; **cumeeira** de 0,22 × 0,34; inclinação de 28,6° (rise 1,8 sobre 3,3 de vão).
3. **Vidro:** 10 painéis (2 águas × 5 vãos) em objeto separado `_Vidro` (`MAT_Glass`), moldura escura de 0,06.
4. **Empenas** triangulares nas extremidades, com porta dupla escura na extremidade direita.
5. **Canteiros:** 6 caixas de 3,0 × 1,3 × 0,55 (3 de cada lado de um corredor central de 2,0), tampo de terra
   (`MAT_Pt_20`), placa de acento verde na ponta; `Array`.
6. **Luz:** tiras de 11,0 m ao longo do alto das duas paredes (`_Glow`, verde) e 6 barras de cultivo sobre os canteiros.
7. **Plantas:** `SM_Planta_E0` (semente), `E1` (broto), `E2` (folhas), `E3` (madura), `E4` (florada): alface = 3 camadas
   de pétalas de 8 lados (E2 a E4), trigo = 4 hastes de caixa fina com espiga. Cada estágio 40 a 120 triângulos.
8. **Painel de energia** (R01) junto à porta e **parede de vazamento** (R02) na parede de fundo.

**Sockets:** `SOCKET_Estufa_Porta`, `SOCKET_Estufa_Painel`, `SOCKET_Estufa_Vazamento`, `SOCKET_Estufa_Canteiro_1` a `_6` (a
planta nasce aqui), `SOCKET_Estufa_Spawn`.
**Colisão:** `COL_Estufa_Piso`, `COL_Estufa_Paredes`, `COL_Estufa_Canteiros`.
**Estados:** desligado: tiras escuras, plantas murchas (escala 0,75 no Unity); ligado: tiras acesas; turbo: brilho forte.
**Triângulos:** 10.000 (hall) mais 2.000 (plantas). **Aceite:** a vista interior se lê como `int-04` (caibros de aço, vidro, canteiros); 6 canteiros; corredor de 2,0.

### R09 Abrigo (`Mod_Shelter.blend`)

**Referências:** `ext-03-abrigo-on-off`, `int-05-abrigo`. **Papel:** proteção da erupção. **Acento:** `#C77DFF`.

**Medidas:** casco 6,6 × 8,6 × 3,5; interno 6,0 × 8,0 com **coroa a 2,8** acima do piso; berma até z = 4,4. O comprimento fica
ao longo de +Y.

**Objetos:** `SM_Abrigo_Casco`, `SM_Abrigo_Interior`, `SM_Abrigo_Regolito`, `SM_Abrigo_Gabioes`, `SM_Abrigo_Glow`.

**Modelagem:**
1. **Abóbada:** perfil de meia elipse de 12 segmentos, extrudado 8,6 em +Y; `Solidify` 0,30; 5 **nervuras** de 0,12 sobre o
   teto (acento violeta nas bordas).
2. **Escotilha blindada** na parede da frente (kit), com moldura violeta grande e porta aberta; **porta interna** violeta
   no fundo (fechada, decorativa).
3. **Regolito:** malha facetada separada que cobre as laterais e o topo, subindo a z = 4,4; `Displace` leve e `Decimate`;
   `Shade Flat`; `MAT_Pt_13`.
4. **Gabiões:** 2 muros de 1,0 × 1,8 × 2,6 em ângulo na entrada, feitos de **blocos chanfrados** 3 × 6 (`Array`).
5. **Respiros:** 2 tubos curvos Ø 0,3 no topo.
6. **Interior:** nervuras de arco; **4 beliches** (2 vãos de 2,0 × 2 níveis) na parede esquerda; **8 tanques de água**
   (Ø 0,7 × 1,2, `MAT_Pt_33`) empilhados 2 × 4 na parede direita (blindagem); **estação médica** (armário + maca); **banco redondo** de 12 lados (R 0,8, h 0,45) no centro;
   **dosímetro** (tela `MAT_Screen`, sem texto) sobre a porta interna; tiras de teto violeta (`_Glow`).
7. **Painel de energia** (R01) ao lado da porta.

**Sockets:** `SOCKET_Abrigo_Porta`, `SOCKET_Abrigo_Painel`, `SOCKET_Abrigo_Reuniao` (banco), `SOCKET_Abrigo_Spawn`.
**Colisão:** `COL_Abrigo_Piso`, `COL_Abrigo_Casco`, `COL_Abrigo_Beliches`, `COL_Abrigo_Tanques`.
**Estados:** desligado: violeta apagado, escotilha escura; ligado: tiras e anel acesos; turbo: nervuras acesas em onda (VFX). **Triângulos:** 5.000.
**Aceite:** o exterior reproduz `ext-03` (abóbada semienterrada, escotilha violeta, gabiões); o interior reproduz `int-05` (beliches, tanques azuis, banco).

### R10 Laboratório (`Mod_Lab.blend`)

**Referências:** `ext-04-laboratorio-on-off`, `int-06-laboratorio`. **Papel:** descobertas; Expedição. **Acento:** `#FF9F43`.

**Medidas:** externo 7,6 × 5,0 × 3,8; interno 7,0 × 4,4 × 3,0; equipamentos de teto até z = 6,5.

**Objetos:** `SM_Lab_Casco`, `SM_Lab_Interior`, `SM_Lab_Equipamento`, `SM_Lab_Prato` (gira), `SM_Lab_Rampa`, `SM_Lab_Tanque`,
`SM_Lab_Vidro`, `SM_Lab_Glow`.

**Modelagem:**
1. **Casco:** caixa chanfrada (0,06) com `Solidify` 0,30; **faixa de acento laranja** de 0,55 em volta; escotilha na frente.
2. **Janelas:** 3 na lateral (kit retangular, 1,5 × 1,0) e 2 altas por dentro; vidro separado; `_Glow` âmbar para o interior aceso.
3. **Rampa de serviço** lateral (kit: rampa 2,6 × 1,9 + corrimão); porta lateral fechada, decorativa.
4. **Teto:** unidade de climatização 1,8 × 0,9 × 1,5 com 2 ventiladores (kit), mastro com 3 sensores (caixas), **prato** em
   tripé: calota de 12 × 4 com braço e alimentador; objeto próprio com pivô no eixo, clipe `Lab_Prato_Girar`.
5. **Tanque lateral:** Ø 1,2 × 2,4 com faixa laranja.
6. **Interior:** bancadas ao longo das duas paredes (kit), **microscópio** (caixa 0,25), **espectrômetro** (caixa 0,5),
   **freezer de amostras** (caixa 0,8 × 0,7 × 1,9), **câmara de luvas** (1,2 × 0,7 × 1,0 com vidro), **prateleiras** com caixinhas coloridas,
   **eclusa de amostras** quadrada de 0,6 na parede do fundo, com moldura laranja; 3 monitores (`MAT_Screen`); dutos de teto
   (cilindros de 8 lados); **faixa laranja** na altura dos olhos; faixa escura no pé.
7. **Painel de energia** (R01) ao lado da porta.

**Sockets:** `SOCKET_Lab_Porta`, `SOCKET_Lab_Painel`, `SOCKET_Lab_Bancada`, `SOCKET_Lab_Spawn`.
**Colisão:** `COL_Lab_Piso`, `COL_Lab_Paredes`, `COL_Lab_Bancadas`. **Estados:** desligado: janelas escuras, prato parado; ligado: janelas acesas e prato lento; turbo: prato rápido (clipe a 2×).
**Triângulos:** 8.000. **Aceite:** silhueta de contêiner com rampa, prato e HVAC como `ext-04`; interior com bancadas, câmara de luvas e faixa laranja como `int-06`.

### R11 Bateria (`Mod_Battery.blend`)

**Referências:** `ext-05-bateria-niveis`, `int-07-bateria`. **Papel:** guarda as células (0 a 3). **Acento:** `#FFD23F`.

**Medidas:** contêiner de controle 3,8 × 6,1 × 3,0 (entrável, interno 3,4 × 5,7 × 2,6), no eixo da escotilha. 3 contêineres de bateria
de 2,44 × 6,1 × 2,6: **esquerdo** em x = −3,6, **direito** em x = +3,6 (ambos de y = 0 a 6,1) e **traseiro** em y = 6,5 a 12,6. Transformador em x = 6,8.

**Objetos:** `SM_Bateria_Controle_Casco`, `SM_Bateria_Controle_Interior`, `SM_Bateria_Cont_C1`, `_C2`, `_C3`,
`SM_Bateria_LED_C1`, `_C2`, `_C3` (cada LED com material próprio), `SM_Bateria_Transformador`, `SM_Bateria_Glow`.

**Modelagem:**
1. **Contêineres de bateria:** caixa chanfrada com 6 nervuras verticais; **faixa amarela** de 0,35; **venezianas** (5 caixas finas em cada lado);
   **ventilador** (disco de 10 lados em grade) numa cabeceira; **barra de LED** vertical de 3 segmentos na cabeceira. Cada contêiner usa **um material
   `MAT_Glow_Bateria_Cn`** que o Unity acende pela célula guardada (C1 com 1, C2 com 2, C3 com 3).
2. **Contêiner de controle (interior):** **racks** de 0,9 de profundidade nos dois lados, com portas venezianadas e barras de LED amarelas
   (acendem com o mesmo material dos contêineres); **corredor central** de 1,6 com grade de piso e **faixas de segurança**
   amarelas nas bordas; bandejas de cabos e tubulação de resfriamento no teto; **console** no fundo com tela e **3 barras de LED grandes**
   (`MAT_Glow_Bateria_C1` a `C3`, as mesmas dos contêineres externos).
3. **Transformador:** caixa 1,9 × 1,9 × 1,5 com 3 isoladores (cilindro de 8 lados); calhas de cabos ligando os contêineres (kit).
4. **Poste de luz** (kit) no canto da frente.
5. **Painel de energia** (R01) ao lado da porta do controle.

**Sockets:** `SOCKET_Bateria_Porta`, `SOCKET_Bateria_Painel`, `SOCKET_Bateria_Console`, `SOCKET_Bateria_Spawn`.
**Colisão:** `COL_Bateria_Piso`, `COL_Bateria_Paredes`, `COL_Bateria_Racks`, `COL_Bateria_Conteineres` (3 caixas). **Estados:** 0 células guardadas: tudo escuro; 1: C1 aceso; 2: C1 e C2; 3: os três.
**Triângulos:** 9.000. **Aceite:** o LED externo e o do console sempre mostram o mesmo número; as quatro imagens de `ext-05` se reproduzem.

### R12 Eclusa (`Eclusa.blend`)

**Referências:** `int-08-eclusa`. **Papel:** saída para as tarefas externas (Limpar poeira, Expedição). **Pivô** no plano da face do hub a 180° (r = 6,0).

**Medidas:** externo 3,3 × 3,0 × 3,0; interno 2,7 × 2,5 × 2,6; escotilha ao hub em y = 0; escotilha externa em y = 3,0.

**Objetos:** `SM_Eclusa_Casco`, `SM_Eclusa_Interior`, `SM_Eclusa_Trajes`, `SM_Eclusa_Vidro`, `SM_Eclusa_Glow`.

**Modelagem:**
1. **Casco:** caixa chanfrada, `Solidify` 0,30; duas escotilhas (kit): a do hub neutra, a externa com **roda** grande e moldura de aço.
2. **Interior:** faixa escura de 0,60 no pé; banco (kit bancada) e **cabide de ferramentas** (chave, escova) na parede direita;
   **2 nichos de traje** na esquerda com **trajes de manequim** (`SM_Eclusa_Trajes`: 3 bonecos fixos de 500 triângulos, versão simplificada do R03, sem rig);
   janela com a Terra.
3. **Luzes de status:** 3 LEDs sobre a escotilha externa (verde, âmbar, vermelho; `_Glow`).
4. **Soleira:** rampa de 0,4 → 0 do lado de fora, entre r = 9,6 e r = 10,0.

**Sockets:** `SOCKET_Eclusa_Porta_Hub`, `SOCKET_Eclusa_Porta_Externa`, `SOCKET_Eclusa_Spawn`.
**Colisão:** `COL_Eclusa_Piso`, `COL_Eclusa_Paredes`. **Triângulos:** 2.500.
**Aceite:** reproduz `int-08`; a escotilha externa abre sobre a plaza a z = 0,40 sem degrau; 64 cm livres até o corredor vizinho no hub.

### R13 Campo solar (`Apoio.blend`)

**Referências:** `cena-05-apoio-solar-pouso-rover`. **Papel:** a fonte de energia; poeira nos painéis (0 a 6) é decal do Unity.

**Objetos:** `SM_SolarPanel` (um painel com suporte); o campo é montado no Unity.

**Modelagem:** painel 3,3 × 2,3 × 0,14 (caixa chanfrada), moldura branca e **células** em placa recuada de 3,05 × 2,05 em `MAT_Pt_30`, com 3 divisões
(*Inset* em retícula); suporte: poste de 8 lados Ø 0,24 × 1,3 e ponteira; **inclinação de 28° já na malha**, voltada para o Sol baixo (−X).
**Triângulos:** 120. **Aceite:** o campo de 40 painéis (2 × 5 × 4 grupos) cabe nos 4.800 triângulos.

### R14 Pouso e módulo de carga (`Apoio.blend`)

**Referências:** `cena-05-apoio-solar-pouso-rover`. **Objetos:** `SM_Pouso_Plataforma`, `SM_Modulo_Carga`.

**Plataforma:** octógono de R 7,5 e 0,35, com **octógono interno** recuado em `MAT_Pt_03`, 4 marcadores triangulares nos cantos e meio-fio em `MAT_Pt_05`.
**Módulo de carga:** corpo em caixa chanfrada 3,2 × 3,2 × 2,2 sobre **4 pernas** com sapatas, sino de motor de 8 lados, escada curta, painéis
chanfrados com 2 faixas de acento neutro. **Triângulos:** 600 e 1.500. **Aceite:** plausível como módulo de carga de pouso, sem asas, sem propulsor futurista.

### R15 Prato, rover, contêineres e dutos (`Apoio.blend`)

**Referências:** `cena-05-apoio-solar-pouso-rover`.

| Peça | Modelagem | Tri |
| --- | --- | --- |
| `SM_Prato_Antena` | Base em caixa 2,4 × 2,4 × 1,4, tripé de 3 pernas, **prato** (calota de 12 × 4, Ø 6,8) com braço e alimentador. `SM_Prato_Disco` separado, com pivô no eixo | 900 |
| `SM_Rover` | Chassi em caixa chanfrada 3,6 × 2,2 × 1,3, cabine com vidro teal, 3 pares de rodas de 10 lados Ø 1,1 (`SM_Rover_Roda` instanciada, clipe `Rover_Rodar`), antena e faixa laranja | 3.000 |
| `SM_Container_Suprimento` | Caixa 2,44 × 6,1 × 2,6 com 6 nervuras (a mesma da Bateria; **um só modelo** reaproveitado) | 400 |
| `SM_Duto_Reto_4m`, `_Curva`, `_T`, `_Fim` | 2 tubos de 8 lados Ø 0,16 com 2 braçadeiras; uso: seguir o chão entre módulos e o campo solar | 150 cada |

**Aceite:** o rover lê-se como veículo pressurizado de 6 rodas; o prato se distingue de longe; os dutos seguem o chão.

### R16 Terreno, rochas e espigões (`Terreno.blend`)

**Referências:** `cena-06-terreno-formacoes`. **Papel:** o chão e o horizonte; planos grandes facetados.

**Objetos:** `SM_Ground_Nucleo`, `SM_Ground_Borda`, `SM_Rock_01` a `_06`, `SM_Spire_01` a `_06`.

**Modelagem:**
1. **Núcleo:** grade de 64 × 64 sobre um disco de Ø 80 m; `Displace` (textura Clouds, força 1,2 m) e `Voronoi` fraco; **crateras** esculpidas por
   `Proportional Edit` (8 crateras de 5 a 11 m); área de 34 m de raio da base **achatada** (z = 0, sem relevo); `Decimate` (Collapse 0,5), `Shade Flat`.
2. **Borda:** anel de r 80 a 130 que sobe até 14 m (muralha de cratera), grade de 48 × 16 decimada, `Shade Flat`.
3. **Cor por vértice:** gradiente de `#807D8B` (claro) a `#55525F` (fundo de cratera), por script ou `Vertex Paint`.
4. **Rochas:** icosfera de nível 1, escala não uniforme, `Displace`, `Decimate` até 100 a 250 triângulos; **6 variantes**.
5. **Espigões:** cilindro de 6 a 8 lados com 4 segmentos de altura, vértices deslocados por ruído (±0,25 do raio), altura de 8 a 26 m; **6 variantes**.
**Triângulos:** 10.000 (terreno), 1.200 (rochas), 1.500 (espigões). **Aceite:** igual a `cena-06` na escala; a base fica sobre chão plano; as sombras do Sol a 12° caem limpas.

### R17 Montagem e câmeras (`Montagem.blend`)

**Referências:** `cena-01-geral-base` (versão a usar), `cena-02-geral-alternativa`. **Papel:** conferir a arquitetura inteira; **não exporta**.

1. Coleção `02 Estacao` com **instâncias de coleção** de cada asset, nas posições da tabela da seção 4.6.
2. Hub no centro; 5 pares módulo + corredor com a **mesma transformação**; eclusa a 180°.
3. Plaza, campo solar (2 grupos 2 × 5), pouso, prato, rover, contêineres, dutos, terreno, rochas e espigões.
4. Câmeras `CAM_01` a `CAM_04` (seção 5) e `LUZ_Sol` (direcional, 12°, da esquerda).
5. Rodar as **verificações de encaixe** (4.6) e `previa.py --cameras todas --folha`.
**Aceite:** nenhuma interpenetração; pisos e eixos alinhados; a vista geral reproduz `cena-01`.

---

## 8. Etapa 6: Revisão no motor e entrega

**Exportar** (por asset, com `exportar.py`):

| Opção | Valor | Por quê |
| --- | --- | --- |
| Formato | FBX por coleção `02 <Asset>` | um prefab por asset |
| Apply Scalings | FBX All | escala 1 no Unity |
| Forward / Up | −Z / Y | frente (−Y) vira +Z |
| Apply Transform | só sem rig | com rig entorta |
| Smoothing | Face | o Unity lê as arestas duras |
| Rig | só ossos que deformam, sem leaf bones, ações assadas | clipes limpos |
| Texturas | Copy, sem embutir, em `tex/` | importadas à parte |

**Antes de exportar:** `auditar.py --perfil perfil.json` sem ERRO; `exportar.py … --conferir` reimporta e confere tamanho
em metros, escala e ações. Versão de entrega `_v001`.

**No Unity (conferir, e eu peço):** objeto com rotação (0,0,0) e escala (1,1,1) ao lado de um cubo de 1 m; frente para +Z;
materiais `Palette` (sRGB ligado), `Accent`, `Glow`, `Glass` ligados; clipes com Loop Time; o personagem com Avatar Humanoid;
**fps e draw calls no momento mais pesado** (Profiler); leitura no aparelho, no tamanho real da tela.
Depois, uma pergunta aberta: como ficou lá, o que incomoda, o que não parece com o que você imaginou.

**Fechamento:** a etapa só fecha com a sua aprovação depois de ver no motor. Antes disso, "exportado, aguardando conferência no motor".

---

## 9. Checklists

### 9.1 Conformidade arquitetônica (rodar na Montagem)

- [ ] Hub: decágono, apótema 6,0 externo e 5,5 interno, teto pentagonal, plaza R 10,0
- [ ] 5 corredores de 10,0 m, interno Ø 3,0, no centro de faces a 0°, 72°, 144°, 216°, 288°
- [ ] Eclusa a 180°, escotilha externa a 3,0 m do hub, sem degrau para a plaza
- [ ] Pivôs dos módulos em R = 16,0 com as posições e rotações da tabela
- [ ] Piso interno a z = 0,40 em **todos** os espaços; eixo das escotilhas a z = 1,78
- [ ] Passagem livre ≥ 1,2 m × 2,2 m em todo caminho; corredor de 2,0 m nos salões
- [ ] Nenhuma interpenetração entre módulos; 64 cm livres entre a eclusa e os corredores vizinhos
- [ ] Cada módulo tem painel de energia com 2 soquetes e, onde pedido, parede de vazamento com 6 pontos

### 9.2 Aceite por asset

- [ ] Reproduz a(s) referência(s) da tabela 3.1 na silhueta, na estrutura e na cor de acento
- [ ] Triângulos dentro da tabela 4.2; nenhum objeto acima de 12.000
- [ ] **Nenhum N-gon**, nenhum vértice duplicado ou solto, nenhuma face interna, normais para fora, escala aplicada
- [ ] Nomes no padrão 4.5, nada com `.001`, nada solto, pivô e frente (−Y) corretos
- [ ] Materiais: só `MAT_Pt_*`, `MAT_Accent_*`, `MAT_Glow_*`, `MAT_Glass` (até 4 por objeto)
- [ ] `SOCKET_*` e `COL_*` presentes onde a receita pede
- [ ] Estados (desligado, ligado, turbo) se leem pela luz
- [ ] `auditar.py` sem ERRO; previa aberta e criticada

### 9.3 O que eu faço em cada processo (como na skill)

Abro o processo com o número e o motivo; rodo o ciclo de conferência; mostro **o que fiz, a imagem (caminho completo), os números do auditor e o que
vem depois**; peço a confirmação. Uma confirmação não autoriza o processo seguinte, e resposta a pergunta não é confirmação.

---

## 10. Pendências e aprovação

**Decidido** (seção 0): versão 5.3 como destino (gerado no 5.2.1), construção por script, processo técnico em detalhe.

**Em aberto (com a etapa que cada uma bloqueia):**

1. Aprovar os refinamentos A a F da seção 0 e as duas divergências da 3.4 → bloqueia o **blockout**.
2. Atlas do Bip e das telas: por script (padrão que sigo se você não responder) ou imagens suas → bloqueia só os **materiais** do Bip e das telas.
3. Aparelho-alvo do WebGL → confirma o orçamento; sem isso o perfil `web` vale como estimativa e o gargalo real aparece na etapa 6.

**Pergunta direta:** você aprova este plano, com o processo técnico da tabela 4.9 e a arquitetura da seção 4.6, para começar
o blockout? Aprovando, o próximo passo é a **Etapa 4**: criar `ArtSource/EstacaoRoda/` com o LEIAME, o `perfil.json` e o
`config.json` (com `"blender": "5.2"`), e gerar o primeiro `.blend` do **lote L1 (personagens)**, em primitivas à escala real
com as câmeras do jogo, para você avaliar a forma.
