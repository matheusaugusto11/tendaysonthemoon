# Textos do formulário de submissão (Project tab)

Texto em inglês, pronto para colar, campo por campo, na ordem do formulário
(ver [SUBMISSAO.md](SUBMISSAO.md), seção 2). Cada bloco entre ``` é o que vai no campo.
Escrito em 15/11/2026 a partir do README, de [DADOS-NASA.md](DADOS-NASA.md) e de [CAMPO-USO-DE-IA.md](CAMPO-USO-DE-IA.md).

Pendências que **nenhum texto resolve**: o link dos slides (campo 3) e o autor e o link de cada som do Pixabay (campo 8).

---

## 1. Project Name

```
Ten Days on the Moon
```

## 2. High-Level Project Summary

```
Ten Days on the Moon is a browser game that trains kids aged 7 to 12 to think like a junior astronaut. You run a small outpost at the Moon's south pole for ten days. Each day you split a few energy cells between air and water, a greenhouse, a radiation shelter, a science lab and a battery. You cannot power everything, and no choice is free: the cost shows up days later. Real NASA solar flare data (DONKI) sets how hard radiation hits the base, so a child learns by playing that on the Moon energy is limited and protecting the crew costs energy that is also needed for air, food and science.
```

## 3. Link to Project Demo

```
(PENDENTE: link público dos 7 slides, Docs/apresentacao/TenDaysOnTheMoon-Demo-v2.pdf, testado em janela anônima)
```

## 4. Link to Final Project

```
https://carambo.itch.io/ten-days-on-the-moon
```

## 5. Detailed Project Description

```
WHAT IT IS
Ten Days on the Moon (Dez Dias na Lua) is a mission trainer for kids aged 7 to 12, built for the challenge "Build a Junior Astronaut Mission Trainer". It runs in the browser with no download and no account, in English and Portuguese.

HOW IT WORKS
The child walks an astronaut around a 3D station to the day's task, plays a short minigame, then goes back to the energy console and places that day's energy cells. Six cells are available on a clear day, for ten slots: air and water, greenhouse, shelter, science lab and battery. The child watches oxygen, radiation and food. Oxygen at 0 or radiation at 10 ends the mission, and food runs out if the greenhouse is neglected. If a limit is hit, the crew goes home early, safely. There are two difficulty modes, Cadet and Commander.

WHAT A CHILD TAKES AWAY
On the Moon, energy is limited. Protecting the crew from solar radiation costs energy that is also needed for air, food and science. Every choice has a cost, and the cost can show up days later. Shadow days also teach why batteries matter at the poles.

HOW NASA DATA IS USED
Space weather events come from real NASA DONKI data. The game uses 20 real solar flares, each with its real date and class (C, M or X). The class decides how hard radiation hits the base. Real coronal mass ejections (CME) give a two-day warning, and real solar energetic particle (SEP) events are shown when DONKI links one to the flare. Event and task cards show real facts, each with its NASA source visible: moonquakes, moon dust, lunar ice, the Earth hiding below the horizon, isolation, exercise in space, the Canadarm2 robotic arm and a greenhouse mold event backed by NASA OSDR study OSD-268. The data is archived, not live, and the game labels it that way. Costs, chances and the 0 to 10 scales are game values, not measurements.

TOOLS
Unity 2022.3 (C#, URP) with a WebGL build, published on itch.io. 3D art modeled in Blender. The game rules are plain C# with no Unity dependency, covered by 216 EditMode tests. Fonts Barlow Semi Condensed and Nunito. The star sky is generated in code. AI was used as an assistant (Claude and Gemini), as declared in the field above and in the project.

BENEFITS AND GOAL
The goal is to make real space data and the hard trade-offs of living on the Moon something a child can feel, not memorize. The code is open under Apache 2.0. Not a NASA product and not endorsed by NASA.
```

## 6. Use of Super Intelligence (SI)

Usar o texto de [CAMPO-USO-DE-IA.md](CAMPO-USO-DE-IA.md), seção "Texto para colar".

## 7. NASA Data

```
NASA DONKI (CCMC, NASA Goddard), https://ccmc.gsfc.nasa.gov/tools/DONKI/
- Solar flares (FLR): 20 real flares curated from 1,982 records (Sept 2017 and Jan 2024 to Nov 2026), with real date, class and event link. The class sets how hard radiation hits the base. Includes the X9.3 of 06/09/2017, the X9.0 of 03/10/2024 and the X5.1 of 11/11/2025.
- Solar energetic particles (SEP): 13 measured events linked to the flares.
- Coronal mass ejections (CME): 10 events linked to the flares; they drive the two-day warning.
Retrieved 13 and 15 Nov 2026 from the public CCMC endpoint (no key). Raw JSON is kept unedited in the repository.

NASA OSDR, study OSD-268, "Microbiological and nutritional analysis of lettuce crops grown on the International Space Station": https://osdr.nasa.gov/bio/repo/data/studies/OSD-268 (greenhouse mold event).

NASA pages used as facts on event and task cards (source shown on each card):
- Sun height at the south pole, long shadows, black daytime sky, Earth near the horizon: NASA SVS 4893 and 4944; NASA Science "9 Phenomena NASA Astronauts Will Encounter at Moon's South Pole"; NASA "Artemis Base Camp on the Moon Will Need Light, Water, Elevation".
- Vertical solar arrays: NASA Space Technology, "NASA, Industry to Mature Vertical Solar Array Technologies for Lunar Surface".
- Light delay Moon to Earth and up to 20 min to Mars: NASA Science "How Does Webb See Back in Time?"; NASA HRP "Hazard: Distance From Earth"; NASA NSSDC Moon Fact Sheet.
- Regolith-covered habitat: NASA NTRS 20210022066 (Grande et al., ASCEND 2021).
- Moonquakes; meteoroid impacts; moon dust; cold in shadowed craters (LRO); ice in a dark crater (LCROSS): NASA pages cited in the repository.
- Isolation and confinement; astronaut exercise; zinnias from space: NASA HRP and NASA pages.
- Canadarm2 capture of Cygnus XL (NASA Space Station blog, 13/04/2026) and the 60-day crew training (NASA blog, 01/04/2026).

Full list with links, quotes and query dates (23 sources, F1 to F23): Docs/DADOS-NASA.md in the repository. All sources are also listed on the in-game Credits screen.
```

## 8. Space Agency Partner Data and Other References

```
No data from partner space agencies was used. (If the form asks for one: none; the game uses NASA data only.)

Engine and tools: Unity 2022.3.62f3 (Unity Technologies), Universal Render Pipeline; Blender (Blender Foundation) for 3D modeling, with Python scripts; Python and Pillow for build and image tools; itch.io for hosting the WebGL build.

Fonts: Barlow Semi Condensed and Nunito, SIL Open Font License 1.1 (licenses in the repository, Assets/Resources/Fontes).

Sound effects (Pixabay, Pixabay Content License, https://pixabay.com/service/license-summary/): doorOpening (freesound_community), Alarme, ClickUI, RobotSound, SomDePassos. [COMPLETAR: autor e link de cada som]. Other interface sounds are synthesized in code.

Other: the star sky is generated in code with no third-party texture. All code and content are original, under Apache 2.0.

AI tools: Claude (Anthropic) and Gemini (Google), as declared in the SI field and in USO-DE-IA.md.

Repository (private until opened): https://github.com/matheusaugusto11/tendaysonthemoon
```

## 9. Imagens (opcional)

Capturas do jogo em `Docs/capturas-base` (JPG ou PNG, até 10 MB cada). Evitar imagens de IA sem marca visível.
