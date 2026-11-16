# Ten Days on the Moon (Dez Dias na Lua)

A mission trainer for kids aged 7 to 12, made for the **NASA Space Apps Challenge 2026**
(*Build a Junior Astronaut Mission Trainer*). You run a small outpost at the Moon's south pole
for ten days. Each day you split a few energy cells between air and water, a greenhouse, a
radiation shelter, a science lab and a battery. **No choice is free:** every cell you spend in
one place is missing somewhere else, and the cost shows up days later.

**What a child should take away:** on the Moon, energy is limited, and protecting the crew from
solar radiation costs energy that you also need for air, food and science.

Languages: English and Portuguese (switch in the game). Not a NASA product and not endorsed by NASA.

## How to play

Play it in the browser, with no download and no account: **https://carambo.itch.io/ten-days-on-the-moon**

Walk the astronaut to the day's task station, do the minigame, then go back to the energy
console and place your cells. Watch oxygen, radiation and the battery.
Rules in short: oxygen at 0 ends the mission, radiation at 10 ends the mission, and food runs out
if the greenhouse is neglected. Two difficulty modes: Cadet and Commander.

## NASA data

| Source | Use in the game |
| --- | --- |
| [NASA DONKI](https://ccmc.gsfc.nasa.gov/tools/DONKI/) (CCMC): solar flares (FLR), solar energetic particles (SEP), coronal mass ejections (CME) | 20 real solar flares, with real date and class. The class decides how hard radiation hits the base. The CME gives a two-day warning. |
| [NASA OSDR](https://osdr.nasa.gov/bio/repo/data/studies/OSD-268) OSD-268 | The "fungus" greenhouse event. |
| Other NASA pages (moon dust, moonquakes, ice, Earth hiding, isolation, exercise, Canadarm2 robotic arm) | Facts shown on event and task cards. Each card shows its source. |

The data is **archived, not live**, and the game labels it that way. Full list with links and
query dates: [Docs/DADOS-NASA.md](Docs/DADOS-NASA.md) (in Portuguese) and the in-game
credits screen. Costs, chances and the 0 to 10 scales are game values, not measurements.

## Use of AI

See [USO-DE-IA.md](USO-DE-IA.md).

## Other references

- Unity 2022.3 (engine).
- Fonts Barlow Semi Condensed and Nunito, SIL Open Font License 1.1 (licenses in `Assets/Resources/Fontes`).
- The star sky is generated in code (`Assets/Scripts/CeuEstrelado.cs`), with no third-party texture.
- Sound effects: five files downloaded from [Pixabay](https://pixabay.com/sound-effects/), used under the
  [Pixabay Content License](https://pixabay.com/service/license-summary/) (free use, attribution not required;
  we credit them anyway). They are not made by the team.

  | File | Where it plays | Source |
  | --- | --- | --- |
  | `Assets/Audios/doorOpening.mp3` | Base doors opening | Sound Effect by freesound_community from Pixabay |
  | `Assets/Resources/Sons/Alarme.mp3` | Red lights during a solar event | Pixabay sound effect |
  | `Assets/Resources/Sons/ClickUI.mp3` | Interface buttons | Pixabay sound effect |
  | `Assets/Resources/Sons/RobotSound.mp3` | Pet robot | Pixabay sound effect |
  | `Assets/Resources/Sons/SomDePassos.mp3` | Astronaut footsteps | Pixabay sound effect |

- The other interface sounds (success, done, panel) are generated in code (`Assets/Scripts/Regras/SomSintetico.cs`).
- AI-generated reference images (Gemini) used to model the station and characters are in `Docs/image-ref`,
  `ArtSource/EstacaoRoda/ref` and `Assets/refs`. Each one carries a visible "AI-generated image" watermark.
  See [USO-DE-IA.md](USO-DE-IA.md).

## Run and test

Open the folder with Unity 2022.3.62f3 and load `Assets/Scenes/Game.unity`.
The game rules are plain C# in `Assets/Scripts/Regras` (no Unity dependency) with 209 EditMode
tests in `Assets/Tests/EditMode` (Test Runner, EditMode).

## License

Original code and content: [Apache License 2.0](LICENSE). Third-party items keep their own licenses, listed above.
