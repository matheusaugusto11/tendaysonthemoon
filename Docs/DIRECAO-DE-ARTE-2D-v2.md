# Direção de arte 2D v2: prompts para o Gemini

Substitui [DIRECAO-DE-ARTE-2D.md](DIRECAO-DE-ARTE-2D.md). A primeira rodada saiu "linear": cinco
caixas iguais em fileira, com legenda, parecendo infográfico. Nesta v2 o que muda é a **composição,
a câmera e a profundidade**, não só o desenho.

## O que a v2 corrige (princípios de jogo infantil)

| Problema da v1 | Regra da v2 |
| --- | --- |
| Cinco módulos iguais em fileira | Cada módulo tem **silhueta, altura e função legível pela forma** |
| Tudo no mesmo plano | **Planos de profundidade** (frente, meio, fundo) e luz com direção |
| Sem foco | Um **ponto focal** (o Bip e o hub) e uma leitura em 1 segundo |
| Personagens pequenos e genéricos | **Cabeça grande, olhos grandes, expressões**: criança se apega a rosto |
| Legendas em texto | **Zero texto**: o estado se lê por forma, cor e brilho |
| Mundo parado | **Sinais de vida**: poeira, luzes, rover, plantas, células brilhando |
| Cores sem hierarquia | **Uma cor por módulo**, tudo o mais dessaturado para dar contraste |
| Câmera parada, sempre a mesma | **Câmera como personagem**: enquadramento diferente em cada fase do dia (seção "Câmera dinâmica") |
| Público misto de 7 a 12 | Alvos grandes, rostos grandes, leitura em 1 segundo; o de 12 anos não pode achar infantil demais, o de 7 não pode se perder |

> **IA no projeto.** O enunciado exige marca d'água visível em imagem gerada por IA que entre no
> projeto ([DESAFIO.md](DESAFIO.md), linha 294). Estas imagens são só referência. Se alguma entrar
> no jogo, marcar e registrar em `USO-DE-IA.md`.

| # | Direção | Câmera e composição | Referências |
| --- | --- | --- | --- |
| A | Playset de brinquedo em 3/4 | Vista de cima em ângulo (isométrica suave), hub central e módulos ao redor | Hay Day, Clash of Clans, Brawl Stars, Toca Life World, Monument Valley (clareza) |
| B | Cinema em camadas | Vista lateral baixa, base subindo a borda de uma cratera, 6 planos | Alto's Odyssey, Ori, Hilda, Journey |
| C | Mini-planeta de brinquedo | Radial: base na curva de uma Lua pequena e redonda | O Pequeno Príncipe, Super Mario Galaxy (conceito), Sky: Children of the Light, Sago Mini World |

---

## Como usar no Gemini

1. **Um chat por direção.** O estilo se mantém melhor dentro da mesma conversa.
2. **Mande primeiro a mensagem 0** (bloco de estilo). Depois, um prompt por vez.
3. **Uma imagem por mensagem.** Em folhas (personagens, módulos), tudo em uma imagem.
4. **Quando sair uma imagem boa, anexe-a** e escreva: "Use this image as the exact style and
   camera-angle reference."
5. **Todo prompt já pede "no text".** Se aparecer texto, peça: "Regenerate with absolutely no text,
   no labels, no captions."
6. Cores em hexadecimal são guia, não garantia.
7. Perdeu o estilo no meio? Cole a mensagem 0 de novo e peça "regenerate".

## O jogo em poucas linhas (já embutido nos prompts)

- Base no **polo sul da Lua**, 5 módulos: **Ar e água**, **Estufa**, **Abrigo** (radiação),
  **Laboratório**, **Bateria**.
- Tripulação: **Lia** (engenheira), **Téo** (botânico), **Nina** (médica), **Kai** (cientista) e o
  robô **Bip**, que explica tudo.
- Sol baixo (sombras longas), Terra no céu, poeira nos painéis solares.
- A criança arrasta **células de energia** para os módulos (2 encaixes por módulo).
- Eventos: erupção solar, dia de sombra, vazamento de ar, falha de painel, descoberta.
- Público de 7 a 12 anos.

## Os cinco módulos pela forma (vale para as três direções)

| Módulo | Silhueta | Cor | Pista de estado |
| --- | --- | --- | --- |
| Ar e água | Torre alta com esferas de tanque e tubos | Turquesa | Bolhas e luz azul quando ligado |
| Estufa | Domo de vidro com plantas grandes | Verde | Luz de cultivo quente, plantas eretas; murcham desligada |
| Abrigo | Bunker baixo e gordo com escotilha redonda | Laranja | Placas de proteção brilham quando ligado |
| Laboratório | Observatório com antena parabólica | Violeta | Antena gira e telas acendem |
| Bateria | Farol feito de células empilhadas | Amarelo | Células acesas em quantidade = energia guardada |

---

# Direção A: Playset de brinquedo em 3/4

## A0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D mobile game for children aged 7-12 about running a small Moon base for 10 days.
Lock in this art style for all the images I'll ask next, and just reply "style locked":

STYLE: stylized 2D illustration that looks like premiere mobile-game art (think Hay Day, Clash of
Clans, Brawl Stars). The base is seen from a high 3/4 angle (soft isometric, about 35-45 degrees
from above), like a glossy toy playset. Chunky, plump shapes with beveled edges, glossy plastic
highlights, saturated candy colors, strong warm rim light from a low Sun, long soft cast shadows.
Characters have HUGE heads (about half the body height), huge expressive eyes, simple mittens for
hands. Palette: deep space indigo #1B1F4B, lunar lavender-gray #CBC6D9, turquoise #35C6E8, lime
green #7ED957, orange #FF7A3D, violet #A66BFF, yellow #FFD43B, warm light #FFE9B0. Each module has its
own color and its own silhouette; everything else stays desaturated so the modules pop. Mood: joyful,
curious, adventurous. ABSOLUTELY no text, no letters, no numbers, no labels, no captions, no logos.
```

## A1. Cena principal

```
Using the locked style, create one 16:9 key-art image: a Moon base at the lunar south pole seen from
a high 3/4 angle. In the center, a round hub platform (landing pad with an airlock) where a small
friendly round robot (Bip) waves at the camera. Around the hub, five buildings, each with a unique
silhouette and color, connected by softly glowing paths: a tall turquoise tower with spherical water
tanks and pipes (Air & Water), a green glass dome full of big plants (Greenhouse), a low chunky orange
bunker with a round hatch (Radiation Shelter), a violet observatory with a satellite dish (Laboratory),
and a yellow lighthouse-shaped tower made of stacked battery cells (Battery). Four astronauts with big
heads work around the base. Foreground: big blurred moon rocks and craters framing the shot. Background:
a huge blue Earth and a star-filled sky, the Sun low on the horizon casting long shadows and a warm
rim light on everything. Two dusty solar panels on the side. Small signs of life: dust puffs, a tiny
rover driving, blinking lights. Glowing energy cells float over the hub. No text.
```

## A2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a soft lavender background: five
characters standing in a row, 3/4 view, full body, same scale. Four astronauts with chunky toy-like
suits and HUGE glossy helmets (visor shows a big expressive face, huge eyes): Lia the engineer
(orange accents, tool belt, big wrench), Téo the botanist (green accents, seedling in a pot, a watering
can), Nina the doctor (turquoise accents, medical case with a plus symbol), Kai the scientist (violet
accents, tablet and a glowing sample tube). Fifth: Bip, a round friendly robot, sphere body with a glowing
ring and big LED eyes, little hovering wheel, expressive eyebrow-like light bars. Strong, different
silhouettes, so each is recognizable even as a black shape. No text.
```

## A3. Expressões

```
Using the locked style and the same characters, create one image: an expression sheet on a soft
lavender background, a 3x2 grid of Bip's face and body expressions (happy, worried, surprised, proud,
sleepy, scared), and under each, the same emotion on one astronaut (Lia). Exaggerated, readable,
cute. No text.
```

## A4. Os cinco módulos

```
Using the locked style, create one image: a sheet showing the five buildings at the same scale in 3/4
view on a plain lavender ground. Top row: each ON, glowing and bright. Bottom row: the same buildings OFF,
desaturated, dark windows, droopy plants, empty cells. Each building has two small round empty sockets
at its door where energy cells plug in. Buildings: turquoise tower with tanks (Air & Water), green glass
dome with plants (Greenhouse), orange chunky bunker with round hatch (Radiation Shelter), violet
observatory with dish (Laboratory), yellow lighthouse of stacked battery cells (Battery). The on/off
difference must be obvious in one second. No text.
```

## A5. Eventos

```
Using the locked style, create one 16:9 image divided in three panels by thin lines, same base in each:
1) calm day: warm sunlight, 2) solar flare warning: sky turns orange and magenta, a giant yellow
burst on the horizon, the little robot looks up worried, 3) shadow day: the Sun is hidden by a crater
rim, the base is lit only by its own windows, solar panels dark, purple-blue cold light. Dramatic but
friendly, not scary. No text.
```

## A6. Interface

```
Using the locked style, create one image: a game UI sheet on a dark indigo background, in the same
glossy toy look. Three big chunky dial gauges with thick bevels and icons, no numbers: Energy
(lightning bolt, yellow), Oxygen (bubbles, turquoise), Radiation (sun with rays, orange), each shown
calm (green glow) and warning (pulsing orange glow). Below, a tray with six glowing yellow round
energy cells, a few empty sockets, and a big rounded green "go" button with a check mark. No text.
```

---

# Direção B: Cinema em camadas

## B0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D game for children aged 7-12 about running a small Moon base for 10 days. Lock in
this art style for all the images I'll ask next, and just reply "style locked":

STYLE: cinematic painterly 2D illustration with deep parallax layers, like an animated feature
background (think Alto's Odyssey, Ori and the Blind Forest, Hilda, Journey), but warm and child-
friendly. Side view with a LOW camera angle, composed on a diagonal. Six depth planes: 1) giant
foreground rock in dark silhouette, 2) foreground dust and rover tracks, 3) the base (the focus),
4) bluish misty craters and hills, 5) sky with a soft Milky Way and a huge blue Earth, 6) floating
dust and light rays. LIGHT: low Sun as an orange backlight with a rim light on every edge, cool blue-
violet shadows, and warm glowing windows and energy cells that show which modules are ON.
Palette: sunset orange #FF9A4D, deep teal-navy #1E2A55, violet shadow #4B3F82, warm window #FFC46B, cool
lunar gray-blue #8E98B8, Earth blue #4A90E2, dust haze #B9A7C9. Soft brush edges, no outlines. Mood:
cozy base inside a vast, beautiful universe. No text, no letters, no labels, no logos.
```

## B1. Cena principal

```
Using the locked style, create one 16:9 key-art image: a Moon base climbing the rim of a crater, seen from
a low angle, composed on a diagonal. The base has five distinct buildings at different heights and
scales: a tall tower with spherical tanks on the peak (Air & Water, turquoise light), a glass dome of
big plants glowing warm (Greenhouse), a half-buried chunky bunker with a round hatch (Radiation Shelter,
orange light), an observatory with a satellite dish (Laboratory, violet light), and a lighthouse of
stacked battery cells glowing yellow (Battery). Warm lit windows show tiny astronauts inside. A small
round robot (Bip) stands in the foreground on the dusty path, looking at the viewer. Huge Earth in the
sky, the low Sun backlighting the scene with long shadows and rim light, a giant dark foreground rock on one
side, floating dust in the light. Two dusty solar panels on the ridge. No text.
```

## B2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a dusk-teal background: five characters in
a row, standing in warm rim light, full body, same scale. Four astronauts with soft rounded suits, big
helmets, visors showing a gentle face with big eyes: Lia the engineer (orange accents, tool belt,
wrench), Téo the botanist (green accents, seedling), Nina the doctor (turquoise accents, medical case with a
plus symbol), Kai the scientist (violet accents, tablet). Fifth: Bip, a small round robot with a glowing
ring and big LED eyes. Distinct silhouettes against the light. No text.
```

## B3. Expressões

```
Using the locked style and the same characters, create one image: an expression sheet, a 3x2 grid of
Bip's reactions (happy, worried, surprised, proud, sleepy, scared), with one astronaut (Lia)
mirroring each emotion beneath. Painterly, soft rim light, readable at small size. No text.
```

## B4. Os cinco módulos

```
Using the locked style, create one image: five buildings at the same scale on a dusk-teal ground, side
view. Top row: each powered ON with warm glowing windows, bright accent light, plants alive, cells
glowing. Bottom row: the same five OFF, dark windows, gray-blue, plants drooping, cells empty. Buildings:
tall tower with tanks (Air & Water, turquoise), glass dome with plants (Greenhouse, green), chunky
bunker with round hatch (Radiation Shelter, orange), observatory with dish (Laboratory, violet),
lighthouse of stacked battery cells (Battery, yellow). Two small round sockets at each door for energy
cells. On/off must read from far away through LIGHT. No text.
```

## B5. Eventos

```
Using the locked style, create one 16:9 image in three panels with thin separators, same base in each:
1) calm golden hour, 2) solar flare warning: the sky is orange and magenta, a huge yellow burst
rising behind the crater, the robot looks up worried, 3) shadow day: the Sun hidden by the crater
rim, deep blue-violet cold light, the only warm light coming from the base windows. Dramatic but
friendly. No text.
```

## B6. Interface

```
Using the locked style, create one image: a game UI sheet on a deep teal background, in a soft glowing
painterly style. Three round gauges with warm glass and brass-like rims, no numbers: Energy
(lightning bolt, yellow), Oxygen (bubbles, turquoise), Radiation (sun, orange), each in a calm and a
warning state. Below, a tray with six softly glowing energy cells, a few empty sockets, and a big round
button with a check mark. No text.
```

---

# Direção C: Mini-planeta de brinquedo

## C0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D mobile game for children aged 7-12 about running a small Moon base for 10 days. Lock
in this art style for all the images I'll ask next, and just reply "style locked":

STYLE: a storybook mini-planet world (think The Little Prince, the tiny-planet idea of Super Mario
Galaxy, Sky: Children of the Light, Sago Mini World). The Moon is a SMALL ROUND PLANET floating in space,
cratered, pearly, with the base built along its curve like a toy playset. Radial composition: buildings
spread around the sphere. Soft gouache-like painting with rounded, friendly shapes, gentle glow, a
real light side and shadow side on the sphere, warm Sun-light from one side. Sky: deep indigo with a
soft magenta-teal nebula and stars in three depth layers, a huge blurred blue Earth and a huge warm
Sun in the background. Palette: deep indigo #1A1445, pearl moon #E9E4F2, lilac shadow #A89BC9, candy
accents: turquoise #55D6E8, green #8EE27A, coral #FF8A5C, violet #B68BFF, yellow #FFE066, Earth blue
#4FA3F7. Mood: magical, cozy, curious. No text, no letters, no numbers, no labels, no logos.
```

## C1. Cena principal

```
Using the locked style, create one 16:9 key-art image: a small round Moon-planet in the center of the
frame floating in a starry indigo sky, a huge blue Earth glowing blurred behind it. The base is built along
the curve of the planet: five buildings of different sizes and silhouettes, connected by little glowing
pipes and bridges like toy wires: a tall turquoise tower with tanks (Air & Water), a green glass dome of
plants (Greenhouse), a chunky coral bunker with a round hatch (Radiation Shelter), a violet observatory with
a dish (Laboratory), a yellow lighthouse of stacked battery cells (Battery). Four astronauts with big
helmets and big eyes walk on the surface (feet toward the planet), and a small round robot, Bip, waves from
the front. A tiny rover drives around the curve. Dust puffs, glowing energy cells floating, two
dusty solar panels. Sun on one side, light side and shadow side visible. No text.
```

## C2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a soft indigo background with a faint
glow: five characters in a row, full body, same scale. Four astronauts with soft rounded suits, huge
helmets, visors showing big expressive eyes: Lia the engineer (coral accents, wrench), Téo the botanist
(green accents, seedling), Nina the doctor (turquoise accents, medical case with a plus symbol), Kai the
scientist (violet accents, tablet). Fifth: Bip, a small round robot with a glowing ring and big LED eyes
and little rocket feet. Distinct silhouettes. No text.
```

## C3. Expressões

```
Using the locked style and the same characters, create one image: an expression sheet, a 3x2 grid of Bip's
reactions (happy, worried, surprised, proud, sleepy, scared) with Lia mirroring each emotion
beneath. Soft gouache, gentle glow, readable at small size. No text.
```

## C4. Os cinco módulos

```
Using the locked style, create one image: the five buildings at the same scale on little pieces of lunar
ground, each shown ON (top row) with glow and color and OFF (bottom row) dim, desaturated, dark windows,
drooping plants, empty cells. Tall turquoise tower with tanks (Air & Water), green glass dome with plants
(Greenhouse), chunky coral bunker with a round hatch (Radiation Shelter), violet observatory with dish
(Laboratory), yellow lighthouse of stacked battery cells (Battery). Two small round empty sockets at each
door. On/off must read in one second. No text.
```

## C5. Eventos e o giro do dia

```
Using the locked style, create one 16:9 image in four panels with thin separators, showing the SAME
mini-planet rotated by quarter turns to show four days: 1) sunny, all panels lit, 2) solar flare
warning: the Sun throws a huge golden burst toward the planet, the robot looks up worried, 3) shadow day:
the base side is in darkness, only its own windows glow, 4) calm again. Friendly drama, not scary. No
text.
```

## C6. Interface

```
Using the locked style, create one image: a game UI sheet on a deep indigo background, in a soft glowing
gouache toy look. Three round gauges with rounded frames and icons, no numbers: Energy (lightning bolt,
yellow), Oxygen (bubbles, turquoise), Radiation (sun, coral), each in a calm and a warning state. Below,
a tray with six glowing round energy cells, a few empty sockets, and a big round button with a check
mark. No text.
```

---

---

# Câmera dinâmica (vale para as três direções)

A câmera deixa de ser uma moldura fixa e vira o diretor de cada momento do dia. Cada fase do jogo
pede um enquadramento diferente, e a troca entre eles é o que dá ritmo e "cara de filme".

| Fase do dia | Enquadramento | Por que funciona para criança |
| --- | --- | --- |
| 1. Boletim da manhã | **Revelação**: a câmera chega do espaço, passa pela Terra e pousa na base | Abertura de episódio: cria expectativa |
| 2. Distribuir energia | **Mapa**: câmera mais alta e aberta, todos os módulos visíveis, sem movimento | Precisa de clareza para arrastar; aqui nada se mexe |
| 3. Tarefas | **Close heroico**: aproxima do astronauta, ângulo baixo, fundo desfocado | O personagem vira protagonista; mais emoção e mais alvo para tocar |
| 4. O dia acontece | **Câmera de filme**: voa entre módulos, ângulo inclinado (dutch), tremor leve na erupção | É o momento de espetáculo: a criança vê a consequência |
| 5. Resultado | **Recuo calmo**: afasta, Bip em primeiro plano, olhando para a criança | Fecha o ciclo e prepara a próxima decisão |
| Acerto no minijogo | **Punch-in**: a câmera dá um soco de zoom e volta | Recompensa visual imediata |

**Honestidade de produção.** Em 2D a câmera faz pan, zoom, inclinação, parallax entre camadas e
rotação, mas **não orbita de verdade**. Na Direção A (3/4 com sprites) isso vale: sem giro de 360°,
só zoom, pan e dois ou três ângulos pré-desenhados. Na Direção C o giro da Lua é possível, porque a
base é um anel de sprites que gira. Na Direção B o parallax faz quase todo o trabalho.

## Prompts de câmera

Use no mesmo chat da direção, depois da mensagem 0 e da cena principal. "Locked style" é o da
mensagem 0 de cada direção.

### Direção A: Playset de brinquedo

**A7. Revelação (boletim)**
```
Using the locked style, create one 16:9 image: a cinematic reveal shot. The camera is high above the
Moon base looking down at a steep angle, a huge glowing blue Earth in the top corner, and the playset
base small in the lower third surrounded by big craters. Strong depth: large blurred rocks in the
foreground, soft light rays from the low Sun. Feels like the opening of an animated episode. No text.
```

**A8. Close heroico (tarefa)**
```
Using the locked style, create one 16:9 image: a low-angle hero close-up of the astronaut Lia (big head,
huge expressive eyes, orange accents) brushing dust off a solar panel, the camera at her knee height
looking up, tilted about 10 degrees (dutch angle). The base is a soft blurred background (shallow depth
of field), the Sun is a rim light behind her, dust particles glowing. Determined, heroic, friendly.
No text.
```

**A9. Vista de mapa (distribuir)**
```
Using the locked style, create one 16:9 image: a steep top-down "map" view of the playset base, nearly
straight down, so that all five buildings and the central hub are clearly visible with no overlapping,
each with its glowing socket spots near the door, a tray of six energy cells at the bottom. Calm
lighting, clear and uncluttered, readable on a small screen. No text.
```

**A10. Erupção (dia acontece)**
```
Using the locked style, create one 16:9 image: a dramatic event shot. The camera is low and close to
the ground, tilted about 15 degrees, a giant orange-and-magenta solar flare burst on the horizon, the
turquoise tower and the green dome in front lit by orange rim light, long shadows, dust flying, Bip
looking up with a worried face in the foreground. Exciting, not scary. No text.
```

### Direção B: Cinema em camadas

**B7. Revelação (boletim)**
```
Using the locked style, create one 16:9 image: a cinematic establishing shot from low orbit. We see the
curved horizon of the Moon, a huge blue Earth rising, the Sun a bright rim over the horizon, and far
below, the tiny glowing base on a crater rim. Layers of depth with mist and dust. Feels like the
opening of an animated film. No text.
```

**B8. POV do capacete (tarefa)**
```
Using the locked style, create one 16:9 image: a first-person view from inside an astronaut's
helmet. The curved visor frame at the edges, a faint reflection of Earth and the Sun in the glass, a
gloved hand holding a brush near a dusty solar panel, the base glowing warm in the background, soft
focus on the edges. Feels like you are the astronaut. No text.
```

**B9. Vista de planos (distribuir)**
```
Using the locked style, create one 16:9 image: a calm wide view with the base fully visible, the
camera slightly higher and more frontal than the key art so all five buildings are clear without
overlap, each lit by its own color, glowing sockets near each door, a quiet sky. Clear and uncluttered.
No text.
```

**B10. Erupção (dia acontece)**
```
Using the locked style, create one 16:9 image: a dramatic tilted shot (dutch angle about 12 degrees),
the camera low behind a giant foreground rock in silhouette, a huge orange solar flare burst on the
horizon casting rays across the base, the buildings backlit, dust trembling in the air, a tiny
astronaut running toward the bunker. Exciting, not scary. No text.
```

### Direção C: Mini-planeta de brinquedo

**C7. Revelação (boletim)**
```
Using the locked style, create one 16:9 image: a cinematic reveal. The camera starts far away in space,
we see a huge blue Earth and a huge warm Sun and, tiny and glowing in the middle, the mini-planet with
the base on its curve. Three layers of stars with parallax, a soft nebula. A storybook opening shot.
No text.
```

**C8. Câmera de superfície (tarefa)**
```
Using the locked style, create one 16:9 image: a fisheye "tiny planet" close-up from the planet
surface. The camera is near the ground behind the astronaut Lia (big helmet, coral accents) brushing
dust off a solar panel, the horizon curves strongly around the frame, the base and the whole sky
curving behind, the Earth huge. Playful, wide-angle, a bit exaggerated. No text.
```

**C9. Vista de mapa (distribuir)**
```
Using the locked style, create one 16:9 image: the mini-planet seen straight from the front, filling the
frame, the five buildings spaced evenly around the visible side of the sphere, each clearly separated, with
glowing sockets, a tray of six energy cells below, a clean calm starry background. Readable and
uncluttered. No text.
```

**C10. Giro do dia (dia acontece)**
```
Using the locked style, create one 16:9 image: a dynamic motion shot of the mini-planet mid-spin, with
soft motion trails of stars, one side lit by a giant golden solar flare, the other in shadow with
glowing windows, the camera slightly tilted. Energetic, magical, friendly. No text.
```

---

## Como escolher depois

Compare as três na mesma ordem: **A1/B1/C1** (cena principal), **A2/B2/C2** (personagens) e
**A4/B4/C4** (ligado e desligado).

1. Em 1 segundo, você sabe qual módulo está ligado?
2. Os quatro astronautas e o Bip se distinguem só pela silhueta?
3. Uma criança de 7 anos quer tocar na tela? O Bip dá carinho?
4. A composição tem profundidade, ou voltou a parecer uma fileira?
5. Dá para produzir 5 módulos, 4 astronautas, Bip e os estados de evento em tempo de hackathon?
6. Os enquadramentos de câmera (revelação, close, mapa, erupção) parecem do mesmo mundo e dão
   vontade de ver o próximo?
7. Uma criança de 12 anos acharia bonito, e uma de 7 anos entenderia sem ajuda?
