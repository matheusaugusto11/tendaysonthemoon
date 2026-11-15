# Direção de arte 2D: prompts para o Gemini

Três direções 2D aprovadas em 14/11/2026 para *Dez Dias na Lua*. O objetivo destas imagens é
**referência visual**: escolher um caminho antes de produzir qualquer arte final.

| # | Direção | Em uma frase | Referências |
| --- | --- | --- | --- |
| A | Pôster da Corrida Espacial | Vetor chapado retrofuturista, sem contorno, uma cor forte por módulo | Pôsteres NASA/JPL "Visions of the Future", Alto's Odyssey, Mini Metro, Fallout Shelter (corte) |
| B | Cartoon de contorno grosso | Formas gordinhas, contorno escuro, cor chapada com uma sombra em degrau e visor brilhante | Among Us (só o espírito do estilo), Fall Guys, Oxygen Not Included |
| C | Papel recortado em camadas | Colagem de papel com sombras entre camadas, céu de papel furado, livro-maquete | Eric Carle, Lois Ehlert, Tearaway, Yoshi's Crafted World |

> **Importante sobre IA.** O enunciado exige marca d'água visível em imagem gerada por IA que
> entre no projeto ([DESAFIO.md](DESAFIO.md), linha 294; [REGRAS-PARA-GANHAR.md](REGRAS-PARA-GANHAR.md),
> regra 11). Estas imagens são só referência. Se alguma entrar no jogo, marcar e registrar em
> `USO-DE-IA.md`.

---

## Como usar no Gemini

1. **Um chat por direção.** O Gemini mantém o estilo melhor dentro da mesma conversa.
2. **Primeiro a mensagem 0** (o bloco de estilo da direção). Depois, um prompt por vez, na ordem.
3. **Peça uma imagem por mensagem.** Em folhas (personagens, módulos), uma imagem com tudo.
4. **Quando uma imagem sair boa, anexe-a** nas próximas mensagens e escreva: "Use this image as the
   exact style reference."
5. **Não confie em texto dentro da imagem.** Os prompts já pedem "no text"; se aparecerem letras
   estranhas, ignore, elas não fazem parte da referência.
6. **Os códigos de cor são guia, não garantia.** O Gemini chega perto, não exato. Ajuste depois.
7. Se perder o estilo no meio, cole de novo a mensagem 0 e peça "regenerate".

## O jogo em 6 linhas (já embutido nos prompts)

- Base de 5 módulos no **polo sul da Lua**, vista em **corte lateral** (formigueiro, casa de bonecas).
- Módulos: **Ar e água**, **Estufa**, **Abrigo** (radiação), **Laboratório**, **Bateria**.
- Tripulação: **Lia** (engenheira), **Téo** (botânico), **Nina** (médica), **Kai** (cientista) e o
  robô **Bip**, que explica tudo.
- Sol baixo no horizonte (sombras longas), Terra pequena no céu, poeira nos painéis solares.
- A criança arrasta **células de energia** para os módulos. O estado de cada módulo (ligado ou
  desligado) precisa ser claro à distância.
- Público de 7 a 12 anos.

---

# Direção A: Pôster da Corrida Espacial

## A0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D children's educational game (ages 7-12) about running a small Moon base for 10
days. Please lock in this art style for all the images I'll ask next, and just reply "style locked":

STYLE: flat vector illustration inspired by 1960s-70s space-age travel posters (NASA/JPL
"Visions of the Future" feel). Pure flat color shapes, NO outlines, simple geometric forms with
softly rounded corners. Shadows are hard-edged offset blocks (no blur, no gradients). Only
texture allowed: a subtle halftone dot pattern in the sky and a faint screen-print paper grain.
Limited palette: deep navy #14213D, cream #F4E9D2, warm lunar grays #B9B2A5 and #6E6A64, rocket
orange #F26B3A, sky blue #3FA7D6, leaf green #6BBF59, violet #8E6BD1, sun yellow #F6BD3B.
Mood: clean, optimistic, friendly, highly readable at a glance. No text, no letters, no logos, no
real NASA insignia.
```

## A1. Cena principal: a base em corte

```
Using the locked style, create one 16:9 image: a side-view cross-section ("ant farm" cutaway) of a
small Moon base at the lunar south pole. Five rooms sit side by side inside the lunar ground, each
a different bold color so they read from far away: Air & Water (blue, tanks and pipes), Greenhouse
(green, rows of plants), Radiation Shelter (orange, thick rounded walls), Laboratory (violet,
screens and flasks), Battery (yellow, stacked glowing energy cells). Small tubes connect the rooms.
Four astronauts and one small round robot are working inside different rooms. On the surface above:
two solar panels with a little dust on them, long hard-edged shadows, the Sun very low on the
horizon, and a small blue-and-white Earth in the navy halftone sky. Straight-on camera, no
perspective. No text.
```

## A2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a plain cream background: five
characters standing in a row, front view, full body, same scale. Four astronauts with chunky rounded
spacesuits, big helmets with smooth flat visors, each with ONE accent color: Lia the engineer (orange
accents, tool belt, wrench), Téo the botanist (green accents, seedling in a small pot), Nina the doctor
(teal accents, small first-aid case with a plus symbol, no text), Kai the scientist (violet accents,
tablet and a sample tube). The fifth is Bip, a small friendly round robot: a sphere body with a glowing
ring and two light-dot eyes, short stubby arms. Each character must be recognizable by silhouette alone.
No text.
```

## A3. Poses de tarefa

```
Using the locked style and the same characters, create one image: a 2x2 grid of four small scenes on a
cream background, each showing one astronaut doing a chore. 1) Lia brushing dust off a solar panel
outside, 2) Téo watering plants in the greenhouse, 3) Kai running a hand-held leak detector along a
module wall with little floating particles, 4) Nina and a crewmate sitting inside the orange radiation
shelter. Flat shapes, hard block shadows, no outlines, no text.
```

## A4. Os cinco módulos, ligado e desligado

```
Using the locked style, create one image: a sheet with five room cutaways in a row. Top row: each room
powered ON with bright colors and glowing details. Bottom row: the same five rooms powered OFF: dimmed
to desaturated navy and gray with no glow. Rooms: Air & Water (blue, tanks, pipes), Greenhouse (green
plants), Radiation Shelter (orange, thick walls), Laboratory (violet, screens, flasks), Battery
(yellow, stacked cells). Each room has two small empty round sockets on the wall where energy cells
plug in. The on/off difference must be obvious at a glance. No text.
```

## A5. Superfície e erupção solar

```
Using the locked style, create one 16:9 image with two halves split by a thin cream line. Left half:
the lunar surface at the south pole, dusty solar panels in a row, the Sun low and big on the horizon
casting long hard-edged shadows, small Earth in the sky, calm. Right half: the same view during a solar
flare warning: the sky turns orange and magenta halftone, a big sun-yellow burst with straight rays
rises over the horizon, the panels look the same. No text.
```

## A6. Interface (ícones e medidores)

```
Using the locked style, create one image: a game UI sheet on a navy background. Three large round gauges
with simple icons and no numbers: Energy (lightning bolt, yellow), Oxygen (air bubbles, sky blue),
Radiation (trefoil-like sun symbol, orange), each shown in a calm state and a warning state. Below:
a tray with six round energy cells (glowing yellow), a few empty sockets, and a big rounded "done"
button without text (a check mark). Flat shapes, no outlines, hard block shadows. No text.
```

---

# Direção B: Cartoon de contorno grosso

## B0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D children's educational game (ages 7-12) about running a small Moon base for 10
days. Please lock in this art style for all the images I'll ask next, and just reply "style locked":

STYLE: chunky cartoon game art in the spirit of casual mobile games like Among Us and Fall Guys, but
with ORIGINAL characters (do not copy any existing game's character design). Thick, uniform dark
outlines (#1B1B2B). Flat saturated colors with exactly ONE step of cel shadow and ONE small glossy
highlight per shape. No gradients, no textures, no fine detail. Very plump, rounded shapes (capsules,
beans, pills). Palette: void navy #14182E, lunar gray #C9C3B8 with shadow #8C8799, cyan #4FB6E8, green
#5FD068, coral #FF6B4A, purple #B66CF0, yellow #FFD23F, visor light-blue #A9E6FF. Mood: playful,
friendly, instantly readable. No text, no letters, no logos.
```

## B1. Cena principal: a base em corte

```
Using the locked style, create one 16:9 image: a side-view cross-section of a small Moon base at the
lunar south pole, like a cartoon spaceship interior. Five chunky rooms side by side, each a distinct
color: Air & Water (cyan, big tanks and pipes), Greenhouse (green, plants in planters), Radiation
Shelter (coral, thick padded walls), Laboratory (purple, monitors and flasks), Battery (yellow, glowing
energy cells in a rack). Short tubes link the rooms. Four little astronauts and one small round robot
walk around inside. Above ground: two solar panels with some dust, the Sun low at the horizon, a small
Earth in the dark sky. Straight-on camera, everything has thick dark outlines. No text.
```

## B2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a plain light-gray background: five
characters in a row, front view, full body, same scale. Four original astronauts with plump capsule-
shaped suits, small backpacks and big glossy visors that show a simple cute face (two dot eyes and
a small eyebrow; make each face slightly different so they have personality). Each has one vibrant
suit color: Lia the engineer (coral, wrench and tool belt), Téo the botanist (green, small seedling),
Nina the doctor (cyan, small medical case with a plus symbol), Kai the scientist (purple, tablet and
test tube). The fifth is Bip, a small round robot: sphere body, glowing ring around the middle, two LED
eyes, tiny stubby arms. Distinct silhouettes. No text.
```

## B3. Poses de tarefa

```
Using the locked style and the same characters, create one image: a 2x2 grid of four small scenes on a
light-gray background. 1) Lia brushing dust off a solar panel outside, 2) Téo watering plants in the
greenhouse, 3) Kai sweeping a leak detector along a module wall while bubbles float toward a tiny crack,
4) Nina and a crewmate huddled in the coral radiation shelter. Thick outlines, one-step cel shadows,
exaggerated friendly expressions. No text.
```

## B4. Os cinco módulos, ligado e desligado

```
Using the locked style, create one image: a sheet with five room cutaways in a row. Top row: each room
powered ON, bright and glowing. Bottom row: the same rooms powered OFF: desaturated and dark with the
glow gone, lights dimmed. Rooms: Air & Water (cyan, tanks), Greenhouse (green, plants droop when OFF),
Radiation Shelter (coral, thick walls), Laboratory (purple, monitors dark when OFF), Battery (yellow,
cells empty when OFF). Each room has two round empty sockets on the wall for energy cells. The on/off
difference must be obvious at a glance. No text.
```

## B5. Superfície e erupção solar

```
Using the locked style, create one 16:9 image split in two halves by a thin line. Left: the lunar
surface at the south pole, a row of dusty solar panels, the Sun low at the horizon, long hard-edged
cel shadows, small Earth in the sky, calm. Right: the same scene during a solar flare
warning: the sky glows orange and magenta, a bright sun-yellow burst with a wavy ring shoots out, a
small round robot points up with a worried face. No text.
```

## B6. Interface (ícones e medidores)

```
Using the locked style, create one image: a game UI sheet on a navy background. Three chunky round gauges
with thick outlines and simple icons and no numbers: Energy (lightning bolt, yellow), Oxygen (bubbles,
cyan), Radiation (sun with rays, coral), each in a calm and a warning state. Below: a tray with six
glossy round energy cells, some empty sockets, and a big rounded green button with a check mark and no
text. No text.
```

---

# Direção C: Papel recortado em camadas

## C0. Mensagem de estilo (mandar primeiro)

```
I'm designing a 2D children's educational game (ages 7-12) about running a small Moon base for 10
days. Please lock in this art style for all the images I'll ask next, and just reply "style locked":

STYLE: layered paper-cut collage, like a handmade children's picture-book diorama (think Eric Carle
and Lois Ehlert collage, and the papercraft look of Tearaway and Yoshi's Crafted World). Every element
is a separate cut-paper layer with slightly irregular scissor-cut edges. Soft, realistic drop shadows
between layers show depth. Visible paper-fiber texture, faint fold creases. Modules look like cardboard
and folded paper; the sky is indigo paper with punched star holes glowing warm from behind. Palette:
indigo #2B2D6E, lunar lilac #CFC6E8, cream paper #F7F0E0, coral #F2766B, mustard #F2B84B, mint
#7CCBA2, sky blue #6FB3E0. Mood: cozy, handmade, warm. No text, no letters, no logos.
```

## C1. Cena principal: a base em corte

```
Using the locked style, create one 16:9 image: a side-view paper diorama cross-section of a small Moon
base at the lunar south pole, like a shoebox theater with several paper layers in depth. Five rooms
side by side cut into the lilac-paper ground, each its own color: Air & Water (sky blue, tanks and
pipes), Greenhouse (mint, folded-paper plants), Radiation Shelter (coral, thick cardboard walls),
Laboratory (lilac and indigo, tiny paper screens), Battery (mustard, stacked paper cells). Four paper-
doll astronauts and one small paper-and-tin-can robot are inside. Above: two paper solar panels with a
little dust, the paper Sun low at the horizon, a small paper Earth in the indigo sky with punched stars.
Layers cast soft shadows. No text.
```

## C2. Folha de personagens

```
Using the locked style, create one image, a character sheet on a plain cream paper background: five
paper-craft characters in a row, front view, same scale. Four astronaut paper dolls with folded-paper
suits, button-like eyes visible through clear acetate visors, small joint rivets at the elbows and
knees: Lia the engineer (coral, paper wrench), Téo the botanist (mint, tiny paper seedling), Nina the
doctor (sky blue, small paper medical case with a plus symbol), Kai the scientist (lilac, paper
tablet). The fifth is Bip, a small friendly robot built from a recycled tin can, a bottle cap, and a
paper ring with two button eyes. Distinct silhouettes. No text.
```

## C3. Poses de tarefa

```
Using the locked style and the same characters, create one image: a 2x2 grid of four small paper-
diorama scenes on a cream background. 1) Lia brushing dust off a paper solar panel outside, 2) Téo
watering folded-paper plants in the greenhouse, 3) Kai moving a little detector along a cardboard wall
while tiny paper puffs drift toward a crack, 4) Nina and a crewmate sitting in the coral cardboard
radiation shelter. Soft shadows between layers. No text.
```

## C4. Os cinco módulos, ligado e desligado

```
Using the locked style, create one image: a sheet with five room cutaways in a row. Top row: each room
powered ON: warm paper-glow behind translucent parts, bright colors. Bottom row: the same rooms powered
OFF: colors faded to gray-lilac, no glow, paper plants drooping. Rooms: Air & Water (sky blue, tanks),
Greenhouse (mint plants), Radiation Shelter (coral, thick walls), Laboratory (lilac, screens), Battery
(mustard, stacked cells). Each room has two small round empty sockets on the wall for energy cells. The
on/off difference must be obvious at a glance. No text.
```

## C5. Superfície e erupção solar

```
Using the locked style, create one 16:9 paper diorama image split in two halves by a thin fold line.
Left: the lunar south-pole surface in lilac paper, dusty paper solar panels, the Sun low on the horizon,
small paper Earth in the indigo punched-star sky, calm. Right: the same scene during a solar flare
warning: the sky turns coral and mustard paper, a big jagged sun-burst of layered yellow paper rises
over the horizon, soft shadows between layers. No text.
```

## C6. Interface (ícones e medidores)

```
Using the locked style, create one image: a game UI sheet on an indigo paper background. Three large
round paper-cut gauges with simple icons and no numbers: Energy (lightning bolt, mustard), Oxygen
(bubbles, sky blue), Radiation (sun with rays, coral), each in a calm and a warning state. Below: a
tray with six round glowing paper energy cells, some empty sockets, and a big rounded mint paper
button with a check mark and no text. Soft shadows between layers. No text.
```

---

## Como escolher depois

Compare as três direções na mesma ordem: **A1/B1/C1** (cena principal), **A2/B2/C2** (personagens)
e **A4/B4/C4** (ligado e desligado). Perguntas que decidem:

1. Dá para saber, em 1 segundo, qual módulo está ligado?
2. Os quatro astronautas e o Bip se distinguem só pela silhueta?
3. O estilo se mantém bonito quando alguém está com pressa (criança de 7 anos)?
4. Dá para produzir os 5 módulos, 4 astronautas, Bip e 10 dias de eventos em tempo de hackathon?
