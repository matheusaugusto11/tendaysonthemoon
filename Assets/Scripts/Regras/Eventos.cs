using System;
using System.Collections.Generic;
using System.Linq;

namespace DezDias
{
    /// <summary>Quando o evento interrompe o dia: na ida até a tarefa, na volta ou sorteado entre os dois.</summary>
    public enum MomentoEvento { Ida, Volta, Qualquer }

    /// <summary>Uma escolha do cartão do evento (spec eventos-do-dia).</summary>
    public sealed class EscolhaDef
    {
        public string Id;
        public Txt Titulo, Desc;
        /// <summary>Custo mostrado no cartão (ícone e número). Pode depender do modo.</summary>
        public Func<Missao, Txt> Custo;
        /// <summary>Segundos do relógio que a escolha custa (0 = nenhum). Com o relógio zerado vira 1 célula.</summary>
        public float Segundos;
        public Func<Missao, bool> Disponivel;
        public Txt MotivoIndisponivel;
        /// <summary>Efeito na hora da escolha: células de hoje, dias futuros, flags e sorteio do risco.</summary>
        public Action<Missao> Aplicar;
    }

    /// <summary>Um evento do dia: textos, fonte, momento, condição e escolhas (design D1).</summary>
    public sealed class EventoDef
    {
        public string Id;
        public MomentoEvento Momento;
        public Func<Missao, bool> Condicao;
        public Txt Titulo, Fonte, Dica;
        public Func<Missao, Txt> Texto;
        /// <summary>A fonte do fato foi aberta e registrada em Docs/DADOS-NASA.md §4. Sem isso, o evento sai do sorteio.</summary>
        public bool Conferido;
        /// <summary>Acende a luz de Erupção no cartão.</summary>
        public bool Alarme;
        public readonly List<EscolhaDef> Escolhas = new List<EscolhaDef>();
        public EscolhaDef Escolha(string id) => Escolhas.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>O que o GameManager aplica depois da escolha: tempo do relógio e corrida até o abrigo.</summary>
    public struct ContextoEscolha
    {
        public float SegundosPerdidos;
        public bool IniciarCorrida;
    }

    /// <summary>
    /// Os eventos do dia (spec eventos-do-dia, design D1 a D6). Os custos e as chances são valores de jogo;
    /// o fato de cada cartão tem fonte em Docs/DADOS-NASA.md §4.
    /// </summary>
    public static class Eventos
    {
        public const string Particulas = "particulas", CmeId = "cme", Tremor = "tremor", Micrometeorito = "micrometeorito",
            Poeira = "poeira_eclusa", Frio = "frio", Gelo = "gelo", Terra = "terra", Sono = "sono", Saudade = "saudade", Fungo = "fungo";

        /// <summary>Segundos de espera no abrigo e contagem da corrida (antes do fator do modo).</summary>
        public const float SegundosAbrigo = 45f, ContagemCorrida = 25f;
        public const int PreparoCme = 2, DiasAteChegada = 2;
        public const double ChanceCmeCadete = .6, ChanceCmeComandante = .8;
        public const float FatorSono = .75f;

        static Txt T(string pt, string en) => new Txt(pt, en);
        static Func<Missao, Txt> C(string pt, string en) { var t = T(pt, en); return _ => t; }

        static readonly Dictionary<string, EventoDef> _defs = new Dictionary<string, EventoDef>();
        public static IEnumerable<EventoDef> Defs => _defs.Values;
        public static EventoDef Def(string id) => id != null && _defs.TryGetValue(id, out var d) ? d : null;
        /// <summary>Eventos do sorteio comum (sem partículas e CME, que têm dia certo).</summary>
        public static IEnumerable<EventoDef> Sorteaveis => _defs.Values.Where(d => d.Id != Particulas && d.Id != CmeId);

        static int PctTremor(Missao m) => m.Modo == Modo.Cadete ? 40 : 50;

        // ---------------- Textos fora das escolhas ----------------
        public static readonly Txt AlertaForteTxt = T("Alerta FORTE: é bem provável que as partículas venham.", "STRONG alert: the particles will very likely come.");
        public static readonly Txt AlertaFracoTxt = T("Alerta fraco: pode ser alarme falso, e as partículas podem não vir.", "Weak alert: it may be a false alarm, and the particles may not come.");
        public static readonly Txt CustoTrocado = T("energia -1 hoje", "energy -1 today");
        public static readonly Txt SemTempoNota = T("O tempo do dia acabou: o que custava tempo agora custa 1 célula.", "The day's time is up: what cost time now costs 1 cell.");
        static readonly Txt MomentoGenerico = T("{0}: você escolheu \"{1}\".", "{0}: you chose \"{1}\".");

        static Eventos()
        {
            Add(new EventoDef
            {
                Id = Particulas, Momento = MomentoEvento.Ida, Conferido = true, Alarme = true,
                Condicao = m => m.AlertaDe(m.Dia) != null,
                Titulo = T("Alarme! Partículas do Sol chegando", "Alarm! Particles from the Sun incoming"),
                Texto = m => (m.AlertaDe(m.Dia)?.Forte == true ? AlertaForteTxt : AlertaFracoTxt) + T(
                    " Depois de uma erupção, partículas muito rápidas podem chegar à Lua. Elas atravessam paredes finas, mas não as paredes grossas do abrigo.",
                    " After a flare, very fast particles can reach the Moon. They go through thin walls, but not through the shelter's thick walls."),
                Fonte = T("Fonte: NASA DONKI (CCMC), catálogo SEP. Dado arquivado, não é ao vivo.", "Source: NASA DONKI (CCMC), SEP catalog. Archived data, not live."),
            }, new EscolhaDef
            {
                Id = "abrigo", Segundos = SegundosAbrigo,
                Titulo = T("Correr para o abrigo", "Run to the shelter"),
                Custo = C("tempo -45 s", "time -45 s"),
                Desc = T("Chegue ao Dormitório antes de a contagem acabar. Lá vocês esperam a tempestade passar.", "Reach the Dormitory before the countdown ends. You wait there for the storm to pass."),
            }, new EscolhaDef
            {
                Id = "continuar",
                Titulo = T("Continuar o trabalho", "Keep working"),
                Custo = C("radiação +1 a +3 se vierem", "radiation +1 to +3 if they come"),
                Desc = T("O tempo do dia fica inteiro. Se as partículas vierem, quem está trabalhando leva radiação, e mais ainda fora da base.", "The day's time stays whole. If the particles come, whoever is working takes radiation, even more outside the base."),
                Aplicar = m => m.Hoje.Alarme = Alarme.Continuou,
            });

            Add(new EventoDef
            {
                Id = CmeId, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => m.CmeDia == m.Dia && m.CmeRegistro != null,
                Titulo = T("Uma nuvem do Sol vem aí", "A cloud from the Sun is on its way"),
                Texto = m => (m.CmeLigada
                    ? T("Os satélites viram o Sol soltar uma nuvem gigante de gás quente, a {0} km/s.", "Satellites saw the Sun throw out a giant cloud of hot gas, at {0} km/s.")
                    : T("Os satélites podem ver o Sol soltar nuvens gigantes de gás quente, como uma que saiu a {0} km/s.", "Satellites can see the Sun throw out giant clouds of hot gas, like one that left at {0} km/s."))
                    .F(m.CmeRegistro?.Velocidade ?? 0)
                    + T(" No jogo, ela chega no Dia {0}. Dá para preparar o abrigo antes.", " In the game, it arrives on Day {0}. You can get the shelter ready first.").F(m.CmeChegada),
                Fonte = T("Fonte: NASA DONKI (CCMC), catálogo CME. Dado arquivado, não é ao vivo.", "Source: NASA DONKI (CCMC), CME catalog. Archived data, not live."),
            }, new EscolhaDef
            {
                Id = "preparar",
                Titulo = T("Preparar o abrigo agora", "Get the shelter ready now"),
                Custo = C("energia -2 hoje · radiação 0 com 1 célula no Abrigo", "energy -2 today · radiation 0 with 1 cell in the Shelter"),
                Desc = T("Vocês empilham sacos de água e pedras em volta do abrigo. No dia da chegada, 1 célula no Abrigo já protege todo mundo.", "You stack water bags and rocks around the shelter. On arrival day, 1 cell in the Shelter protects everyone."),
                Aplicar = m => { m.Hoje.CustoEvento += PreparoCme; m.AbrigoPreparado = true; },
            }, new EscolhaDef
            {
                Id = "deixar",
                Titulo = T("Deixar para quando chegar", "Leave it until it arrives"),
                Custo = C("energia -0 hoje · radiação 0 só com 2 células no Abrigo", "energy -0 today · radiation 0 only with 2 cells in the Shelter"),
                Desc = T("A energia de hoje fica inteira. No dia da chegada, o Abrigo vai precisar de 2 células.", "Today's energy stays whole. On arrival day, the Shelter will need 2 cells."),
            });

            Add(new EventoDef
            {
                Id = Tremor, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => !m.Vazamento,
                Titulo = T("A base tremeu!", "The base shook!"),
                Texto = C("A Lua também tem tremores. O mais forte que os sismômetros da Apollo registraram foi perto do polo sul, onde fica a base. Um tremor pode soltar as juntas das paredes.",
                          "The Moon has quakes too. The strongest one the Apollo seismometers recorded was near the south pole, where the base is. A quake can loosen the joints in the walls."),
                Fonte = T("Fonte: NASA, Shrinking Moon Causing Moonquakes and Faults Near Lunar South Pole (2024).", "Source: NASA, Shrinking Moon Causing Moonquakes and Faults Near Lunar South Pole (2024)."),
            }, new EscolhaDef
            {
                Id = "checar", Segundos = 40f,
                Titulo = T("Parar e checar as juntas", "Stop and check the joints"),
                Custo = C("tempo -40 s", "time -40 s"),
                Desc = T("Vocês conferem cada junta. Nada vaza, mas o dia fica mais curto.", "You check every joint. Nothing leaks, but the day gets shorter."),
            }, new EscolhaDef
            {
                Id = "seguir",
                Titulo = T("Seguir o trabalho", "Keep working"),
                Custo = m => T("O2 -1 por dia se vazar (chance de {0}%)", "O2 -1 a day if it leaks ({0}% chance)").F(PctTremor(m)),
                Desc = T("Pode ser que nada tenha soltado. Se soltou, abre um vazamento.", "Maybe nothing came loose. If something did, a leak opens."),
                Aplicar = m => m.Hoje.Risco = Sorteio(m) < PctTremor(m) / 100.0,
            });

            Add(new EventoDef
            {
                Id = Micrometeorito, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = _ => true,
                Titulo = T("Um micrometeorito riscou um painel", "A micrometeorite scratched a panel"),
                Texto = C("Pedrinhas do espaço batem na Lua o tempo todo, e a NASA observa da Terra o clarão desses impactos. Uma delas acertou um painel solar da base.",
                          "Little space rocks hit the Moon all the time, and NASA watches the flashes of these impacts from Earth. One of them hit a solar panel at the base."),
                Fonte = T("Fonte: NASA Meteoroid Environment Office, Lunar Impact Monitoring.", "Source: NASA Meteoroid Environment Office, Lunar Impact Monitoring."),
            }, new EscolhaDef
            {
                Id = "trocar",
                Titulo = T("Sair e trocar a placa", "Go out and swap the plate"),
                Custo = C("O2 -1", "O2 -1"),
                Desc = T("O astronauta usa o ar do traje para trocar a placa riscada.", "The astronaut uses the suit's air to swap the scratched plate."),
                Disponivel = m => m.O2 > 1,
                MotivoIndisponivel = T("Oxigênio baixo demais para sair.", "Oxygen is too low to go outside."),
            }, new EscolhaDef
            {
                Id = "deixar",
                Titulo = T("Deixar riscado", "Leave it scratched"),
                Custo = C("energia -1 amanhã e depois", "energy -1 tomorrow and the day after"),
                Desc = T("Poupa o ar, mas o painel rende 1 célula a menos amanhã e depois de amanhã.", "Saves air, but the panel gives 1 cell fewer tomorrow and the day after."),
                Aplicar = m =>
                {
                    var motivo = T("Painel riscado pelo micrometeorito: 1 célula a menos.", "Panel scratched by the micrometeorite: 1 cell fewer.");
                    m.Ajustar(m.Dia + 1, -1, motivo);
                    m.Ajustar(m.Dia + 2, -1, motivo);
                },
            });

            Add(new EventoDef
            {
                Id = Poeira, Momento = MomentoEvento.Volta, Conferido = true,
                Condicao = _ => true,
                Titulo = T("Poeira lunar grudada nos trajes", "Moon dust stuck to the suits"),
                Texto = C("A poeira da Lua é fina e gruda em tudo. Na missão Apollo 17, ela entrou na nave e irritou os olhos, o nariz e a garganta dos astronautas.",
                          "Moon dust is fine and sticks to everything. On the Apollo 17 mission, it got into the spacecraft and made the astronauts' eyes, noses and throats sore."),
                Fonte = T("Fonte: NASA Science, Moon Dust.", "Source: NASA Science, Moon Dust."),
                Dica = T("Controle da missão: toda escolha custa alguma coisa. Olhe o custo antes: tempo do dia ou oxigênio?", "Mission control: every choice costs something. Check the cost first: the day's time or oxygen?"),
            }, new EscolhaDef
            {
                Id = "aspirar", Segundos = 40f,
                Titulo = T("Aspirar os trajes na eclusa", "Vacuum the suits in the airlock"),
                Custo = C("tempo -40 s", "time -40 s"),
                Desc = T("A poeira fica do lado de fora. Leva um tempinho.", "The dust stays outside. It takes a little while."),
            }, new EscolhaDef
            {
                Id = "entrar",
                Titulo = T("Entrar assim mesmo", "Go in anyway"),
                Custo = C("O2 -1 hoje", "O2 -1 today"),
                Desc = T("Ganha tempo, mas a poeira entope os filtros de ar.", "Saves time, but the dust clogs the air filters."),
            });

            Add(new EventoDef
            {
                Id = Frio, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => m.EhSombra(m.Dia),
                Titulo = T("Frio da sombra", "Cold in the shadow"),
                Texto = C("Com o Sol atrás do morro, a base esfria rápido. No fundo das crateras que ficam sempre na sombra, perto dos polos da Lua, a temperatura chega a -248 °C.",
                          "With the Sun behind the hill, the base cools down fast. At the bottom of the craters that are always in shadow, near the Moon's poles, it gets down to -248 °C."),
                Fonte = T("Fonte: NASA Science, 10 Cool Things NASA's Lunar Reconnaissance Orbiter is Teaching Us About the Moon.", "Source: NASA Science, 10 Cool Things NASA's Lunar Reconnaissance Orbiter is Teaching Us About the Moon."),
            }, new EscolhaDef
            {
                Id = "aquecer",
                Titulo = T("Ligar os aquecedores", "Turn on the heaters"),
                Custo = C("energia -1 hoje", "energy -1 today"),
                Desc = T("A tripulação e as plantas ficam quentinhas.", "The crew and the plants stay warm."),
                Aplicar = m => m.Hoje.CustoEvento += 1,
            }, new EscolhaDef
            {
                Id = "agasalhar",
                Titulo = T("Agasalhar e economizar", "Wrap up and save"),
                Custo = C("estufa -1", "greenhouse -1"),
                Desc = T("A tripulação põe mais roupa, mas as plantas sentem o frio.", "The crew puts on more clothes, but the plants feel the cold."),
            });

            Add(new EventoDef
            {
                Id = Gelo, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = _ => true,
                Titulo = T("Sinal de gelo numa cratera escura", "Signs of ice in a dark crater"),
                Texto = C("Em 2009, a missão LCROSS da NASA bateu numa cratera escura do polo sul e achou água congelada. O radar da base viu um sinal parecido aqui perto!",
                          "In 2009, NASA's LCROSS mission crashed into a dark crater at the south pole and found frozen water. The base's radar saw a similar sign nearby!"),
                Fonte = T("Fonte: NASA Science, LCROSS.", "Source: NASA Science, LCROSS."),
            }, new EscolhaDef
            {
                Id = "rover",
                Titulo = T("Mandar o rover buscar uma amostra", "Send the rover for a sample"),
                Custo = C("energia -2 hoje · ciência +3", "energy -2 today · science +3"),
                Desc = T("O rover vai e volta sozinho. Gelo de água é uma grande descoberta.", "The rover goes and comes back on its own. Water ice is a big discovery."),
                Aplicar = m => m.Hoje.CustoEvento += 2,
            }, new EscolhaDef
            {
                Id = "anotar",
                Titulo = T("Anotar e seguir", "Note it and move on"),
                Custo = C("energia -0 · ciência +0", "energy -0 · science +0"),
                Desc = T("A energia de hoje fica inteira, mas a descoberta fica para outra missão.", "Today's energy stays whole, but the discovery waits for another mission."),
            });

            Add(new EventoDef
            {
                Id = Terra, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => m.Contato,
                Titulo = T("A Terra vai se esconder", "Earth is about to hide"),
                Texto = C("Do polo sul da Lua, a Terra fica baixinha no céu e às vezes some atrás do horizonte. Sem ver a Terra, o rádio da base não fala com o controle da missão.",
                          "From the Moon's south pole, Earth sits low in the sky and sometimes slips below the horizon. Without seeing Earth, the base's radio cannot talk to mission control."),
                Fonte = T("Fontes: NASA SVS 4944; NASA Science, 9 Phenomena NASA Astronauts Will Encounter at Moon's South Pole; NASA, Artemis Base Camp.", "Sources: NASA SVS 4944; NASA Science, 9 Phenomena NASA Astronauts Will Encounter at Moon's South Pole; NASA, Artemis Base Camp."),
            }, new EscolhaDef
            {
                Id = "satelite",
                Titulo = T("Usar o satélite de retransmissão", "Use the relay satellite"),
                Custo = C("energia -1 hoje", "energy -1 today"),
                Desc = T("O satélite repassa as mensagens, e a dica do controle continua.", "The satellite passes the messages on, and mission control's hint stays."),
                Aplicar = m => m.Hoje.CustoEvento += 1,
            }, new EscolhaDef
            {
                Id = "sem",
                Titulo = T("Ficar sem contato hoje", "Go without contact today"),
                Custo = C("contato sem dica hoje", "contact no hint today"),
                Desc = T("A energia fica inteira, mas hoje você distribui a energia sem a dica do controle da missão.", "The energy stays whole, but today you share out the energy without mission control's hint."),
                Aplicar = m => m.Hoje.SemContatoHoje = true,
            });

            Add(new EventoDef
            {
                Id = Sono, Momento = MomentoEvento.Ida, Conferido = true,
                Condicao = m => Tarefas.Def(m.Hoje.Tarefa) != null,
                Titulo = T("Ninguém dormiu direito", "Nobody slept well"),
                Texto = C("No polo sul da Lua há lugares com mais de 200 dias de luz seguida. Com luz o tempo todo o corpo se confunde, e quem dorme mal trabalha pior.",
                          "At the Moon's south pole there are places with more than 200 days of light in a row. With light all the time the body gets confused, and people who sleep badly work worse."),
                Fonte = T("Fontes: NASA Science, 10 Cool Things NASA's Lunar Reconnaissance Orbiter is Teaching Us About the Moon; NASA Human Research Program, Hazard: Isolation and Confinement.", "Sources: NASA Science, 10 Cool Things NASA's Lunar Reconnaissance Orbiter is Teaching Us About the Moon; NASA Human Research Program, Hazard: Isolation and Confinement."),
            }, new EscolhaDef
            {
                Id = "folga", Segundos = 60f,
                Titulo = T("Dar uma folga de manhã", "Take the morning off"),
                Custo = C("tempo -60 s", "time -60 s"),
                Desc = T("A tripulação descansa e depois trabalha bem.", "The crew rests and then works well."),
            }, new EscolhaDef
            {
                Id = "trabalhar",
                Titulo = T("Trabalhar assim mesmo", "Work anyway"),
                Custo = C("tempo -25% no minijogo de hoje", "time -25% in today's minigame"),
                Desc = T("O dia fica inteiro, mas a tarefa de hoje vai ter menos tempo.", "The day stays whole, but today's task will have less time."),
                Aplicar = m => m.Hoje.FatorTempoHoje = FatorSono,
            });

            Add(new EventoDef
            {
                Id = Saudade, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => m.Contato,
                Titulo = T("Saudade de casa", "Missing home"),
                Texto = C("Longe da Terra, os astronautas ficam isolados num lugar pequeno. A NASA estuda como manter a tripulação animada e unida.",
                          "Far from Earth, astronauts are isolated in a small place. NASA studies how to keep the crew cheerful and close."),
                Fonte = T("Fonte: NASA Human Research Program, Hazard: Isolation and Confinement.", "Source: NASA Human Research Program, Hazard: Isolation and Confinement."),
            }, new EscolhaDef
            {
                Id = "chamada",
                Titulo = T("Videochamada com a família", "Video call with family"),
                Custo = C("energia -1 hoje", "energy -1 today"),
                Desc = T("A antena gasta energia, mas todo mundo volta sorrindo.", "The antenna uses energy, but everyone comes back smiling."),
                Aplicar = m => m.Hoje.CustoEvento += 1,
            }, new EscolhaDef
            {
                Id = "seguir",
                Titulo = T("Seguir trabalhando", "Keep working"),
                Custo = C("amanhã energia -1", "tomorrow energy -1"),
                Desc = T("A energia de hoje fica inteira, mas amanhã a tripulação desanimada rende menos.", "Today's energy stays whole, but tomorrow the gloomy crew gets less done."),
                Aplicar = m => m.Ajustar(m.Dia + 1, -1, T("Tripulação desanimada com saudade de casa: 1 célula a menos.", "Crew feeling down and missing home: 1 cell fewer.")),
            });

            Add(new EventoDef
            {
                Id = Fungo, Momento = MomentoEvento.Qualquer, Conferido = true,
                Condicao = m => m.Crescimento >= 2,
                Titulo = T("Fungo na estufa", "Fungus in the greenhouse"),
                Texto = C("Na Estação Espacial, as flores da estufa Veggie pegaram mofo, e o astronauta Scott Kelly salvou as plantas. Aqui, algumas folhas apareceram com manchas.",
                          "On the Space Station, the flowers in the Veggie greenhouse got moldy, and astronaut Scott Kelly saved the plants. Here, some leaves have spots."),
                Fonte = T("Fontes: NASA, Zinnias from Space! (2016); NASA OSDR, OSD-268.", "Sources: NASA, Zinnias from Space! (2016); NASA OSDR, OSD-268."),
            }, new EscolhaDef
            {
                Id = "cortar",
                Titulo = T("Cortar as folhas doentes", "Cut off the sick leaves"),
                Custo = C("estufa -1", "greenhouse -1"),
                Desc = T("Perde um pouco, mas o fungo para.", "You lose a little, but the fungus stops."),
            }, new EscolhaDef
            {
                Id = "deixar",
                Titulo = T("Deixar e torcer", "Leave it and hope"),
                Custo = C("estufa volta a 0 (chance de 50%)", "greenhouse back to 0 (50% chance)"),
                Desc = T("Se o fungo se espalhar, a estufa recomeça do zero.", "If the fungus spreads, the greenhouse starts over from zero."),
                Aplicar = m => m.Hoje.Risco = Sorteio(m) < .5,
            });
        }

        static void Add(EventoDef d, params EscolhaDef[] escolhas)
        {
            d.Escolhas.AddRange(escolhas);
            _defs[d.Id] = d;
        }

        static Random Rng(Missao m) => m.Rng ?? (m.Rng = new Random(0));
        static double Sorteio(Missao m) => Rng(m).NextDouble();

        // ---------------- Sorteio (design D3, D5) ----------------

        /// <summary>Agenda no máximo 1 CME, 2 dias antes de um alerta com erupção real (design D5).</summary>
        public static void AgendarCme(Missao m, IList<Cme> cmes, Random rng)
        {
            if (cmes == null || cmes.Count == 0) return;
            double p = m.Modo == Modo.Cadete ? ChanceCmeCadete : ChanceCmeComandante;
            if (rng.NextDouble() >= p) return;
            foreach (var d in m.Alertas.Keys.OrderBy(x => x))
            {
                var al = m.Alertas[d];
                int aviso = d - DiasAteChegada;
                if (al.Erupcao == null || aviso < 2 || m.Alertas.ContainsKey(aviso)) continue;
                var ligada = cmes.FirstOrDefault(c => c.FlrLigado == al.Erupcao.Id);
                m.CmeDia = aviso;
                m.CmeChegada = d;
                m.CmeLigada = ligada != null;
                m.CmeRegistro = ligada ?? cmes[rng.Next(cmes.Count)];
                al.Forte = true;
                return;
            }
        }

        /// <summary>Escolhe o evento de hoje, o momento e o gatilho (spec "Um evento por dia", "Condições de cada evento").</summary>
        public static void SortearDoDia(Missao m)
        {
            var T = m.Hoje;
            var rng = Rng(m);
            string id;
            if (m.AlertaDe(m.Dia) != null) id = Particulas;
            else if (m.CmeDia == m.Dia && m.CmeRegistro != null) id = CmeId;
            else if (m.Tutorial && m.Dia == 1) id = Poeira;
            else
            {
                var validos = Sorteaveis.Where(d => d.Conferido && d.Condicao(m)).OrderBy(d => d.Id, StringComparer.Ordinal).ToList();
                var novos = validos.Where(d => !m.EventosUsados.Contains(d.Id)).ToList();
                var pool = novos.Count > 0 ? novos : validos;
                id = pool.Count > 0 ? pool[rng.Next(pool.Count)].Id : Micrometeorito;
            }
            var def = Def(id);
            T.Evento = id;
            T.EventoNaIda = def.Momento == MomentoEvento.Ida || (def.Momento == MomentoEvento.Qualquer && rng.Next(2) == 0);
            T.GatilhoSeg = 4f + (float)(rng.NextDouble() * 8.0);
            m.EventosUsados.Add(id);
        }

        // ---------------- Cartão e escolha (design D4) ----------------

        public static bool Disponivel(Missao m, EscolhaDef e) => e != null && (e.Disponivel == null || e.Disponivel(m));

        /// <summary>Custo mostrado no cartão. Com o relógio zerado, o custo em tempo vira 1 célula.</summary>
        public static Txt CustoVisivel(Missao m, EscolhaDef e, bool relogioZerado)
            => relogioZerado && e.Segundos > 0 ? CustoTrocado : e.Custo(m);

        /// <summary>Registra a escolha e aplica o efeito imediato. Devolve o que o GameManager faz com o relógio e a corrida.</summary>
        public static ContextoEscolha Escolher(Missao m, string escolhaId, bool relogioZerado)
        {
            var T = m.Hoje;
            var ctx = new ContextoEscolha();
            var e = Def(T?.Evento)?.Escolha(escolhaId);
            if (e == null || T.EventoResolvido || !Disponivel(m, e)) return ctx;
            T.Escolha = e.Id;
            T.EventoResolvido = true;

            if (T.Evento == Particulas && e.Id == "abrigo")
            {
                // A espera só custa quando a tripulação chega (FimCorrida). Sem tempo, não há corrida: vale 1 célula.
                if (relogioZerado) { T.Alarme = Alarme.Abrigo; T.CustoEvento += 1; }
                else ctx.IniciarCorrida = true;
                return ctx;
            }
            if (e.Segundos > 0)
            {
                if (relogioZerado) T.CustoEvento += 1;
                else ctx.SegundosPerdidos = e.Segundos;
            }
            e.Aplicar?.Invoke(m);
            return ctx;
        }

        /// <summary>Fim da corrida até o abrigo. A tempo: exposição 0 e 45 s do relógio. Atrasado: meia exposição.</summary>
        public static ContextoEscolha FimCorrida(Missao m, bool aTempo)
        {
            m.Hoje.Alarme = aTempo ? Alarme.Abrigo : Alarme.Atrasado;
            return new ContextoEscolha { SegundosPerdidos = aTempo ? SegundosAbrigo : 0f };
        }

        public static float ContagemReal(Missao m) => ContagemCorrida * m.M.FatorTempo;

        // ---------------- Resultado (design D4, D6) ----------------

        /// <summary>Exposição ao alarme com partículas reais: 1, 1 ou 2 por intensidade, +1 fora da base; metade se atrasado; 0 no abrigo.</summary>
        public static int Exposicao(Missao m, Erupcao e)
        {
            if (e == null) return 0;
            var T = m.Hoje;
            int cheia = (e.Intensidade >= 2 ? 2 : 1) + (T.Fora ? 1 : 0);
            switch (T.Alarme)
            {
                case Alarme.Continuou: return cheia;
                case Alarme.Atrasado: return (cheia + 1) / 2;
                default: return 0;
            }
        }

        /// <summary>Linha do alarme no resultado (partículas reais ou alarme falso).</summary>
        public static Txt LinhaAlarme(Missao m, bool vieram, int exposicao)
        {
            var T = m.Hoje;
            switch (T.Alarme)
            {
                case Alarme.Abrigo:
                    return vieram
                        ? T_("Vocês esperaram no abrigo, e as partículas vieram de verdade: ninguém foi atingido.", "You waited in the shelter, and the particles really came: nobody was hit.")
                        : T_("Vocês correram para o abrigo, mas as partículas não vieram. O tempo de espera custou sem fazer falta.", "You ran to the shelter, but the particles did not come. The waiting time cost you without being needed.");
                case Alarme.Atrasado:
                    return vieram
                        ? T_("Vocês chegaram atrasados ao abrigo: radiação +{0}.", "You reached the shelter late: radiation +{0}.").F(exposicao)
                        : T_("Vocês chegaram atrasados ao abrigo, mas as partículas não vieram.", "You reached the shelter late, but the particles did not come.");
                case Alarme.Continuou:
                    if (!vieram) return T_("Vocês continuaram trabalhando e as partículas não vieram. Desta vez deu certo, mas era arriscado.", "You kept working and the particles did not come. It worked this time, but it was risky.");
                    var l = T_("Vocês continuaram trabalhando e as partículas vieram: radiação +{0}.", "You kept working and the particles came: radiation +{0}.").F(exposicao);
                    return T.Fora ? l + T_(" Um astronauta estava fora da base.", " An astronaut was outside the base.") : l;
                default:
                    return Txt.Nada;
            }
        }

        public static Txt MomentoAlarme(Missao m, int exposicao) => m.Hoje.Alarme == Alarme.Atrasado
            ? T_("Partículas do Sol antes de a tripulação chegar ao abrigo. A radiação subiu {0}.", "Particles from the Sun before the crew reached the shelter. Radiation rose {0}.").F(exposicao)
            : T_("Partículas do Sol com a tripulação trabalhando, depois do alarme. A radiação subiu {0}.", "Particles from the Sun while the crew kept working after the alarm. Radiation rose {0}.").F(exposicao);

        public static readonly Txt PreparoValeu = T("A preparação de dois dias antes valeu: 1 célula no Abrigo bastou.", "Getting ready two days earlier paid off: 1 cell in the Shelter was enough.");

        static Txt T_(string pt, string en) => new Txt(pt, en);

        /// <summary>
        /// Aplica no fim do dia o efeito dos eventos que não são de partículas (que ficam no bloco da radiação) e escreve a linha.
        /// Chamado antes da estufa crescer, para o frio e o fungo valerem hoje.
        /// </summary>
        public static void AplicarNoResultado(Missao m, Action<Tom, Txt> linha, ref int dO2, ref int impacto, ref Txt momento)
        {
            var T = m.Hoje;
            if (T == null || !T.EventoResolvido || T.Evento == Particulas) return;
            var def = Def(T.Evento);
            var e = def?.Escolha(T.Escolha);
            if (e == null) return;
            int imp = 1;
            Tom tom = Tom.Laranja;
            Txt l;
            switch (T.Evento + ":" + T.Escolha)
            {
                case CmeId + ":preparar":
                    tom = Tom.Rad; l = T_("Vocês prepararam o abrigo para a nuvem do Dia {0}.", "You got the shelter ready for the Day {0} cloud.").F(m.CmeChegada); break;
                case CmeId + ":deixar":
                    tom = Tom.Rad; l = T_("O abrigo ficou como estava. A nuvem chega no Dia {0}.", "The shelter stayed as it was. The cloud arrives on Day {0}.").F(m.CmeChegada); break;
                case Tremor + ":checar":
                    tom = Tom.O2; l = T_("Vocês checaram as juntas depois do tremor. Nada vazou.", "You checked the joints after the quake. Nothing leaked."); break;
                case Tremor + ":seguir":
                    if (T.Risco)
                    {
                        m.Vazamento = true;
                        tom = Tom.Vermelho; imp = 3;
                        l = T_("O tremor soltou uma junta e abriu um vazamento: oxigênio -1 por dia até o conserto (2 células).", "The quake loosened a joint and opened a leak: oxygen -1 a day until it is repaired (2 cells).");
                    }
                    else { tom = Tom.O2; l = T_("Vocês seguiram o trabalho, e o tremor não soltou nenhuma junta.", "You kept working, and the quake did not loosen any joint."); }
                    break;
                case Micrometeorito + ":trocar":
                    dO2 -= 1; tom = Tom.O2; imp = 2; l = T_("A placa riscada foi trocada: oxigênio -1.", "The scratched plate was swapped: oxygen -1."); break;
                case Micrometeorito + ":deixar":
                    tom = Tom.Energia; imp = 2; l = T_("O painel riscado vai render 1 célula a menos amanhã e depois.", "The scratched panel will give 1 cell fewer tomorrow and the day after."); break;
                case Poeira + ":aspirar":
                    tom = Tom.O2; l = T_("Os trajes foram aspirados na eclusa, e o ar ficou limpo.", "The suits were vacuumed in the airlock, and the air stayed clean."); break;
                case Poeira + ":entrar":
                    dO2 -= 1; tom = Tom.O2; imp = 2; l = T_("A poeira entrou e entupiu os filtros: oxigênio -1.", "The dust got in and clogged the filters: oxygen -1."); break;
                case Frio + ":aquecer":
                    tom = Tom.Energia; l = T_("Os aquecedores esquentaram a base: custou 1 célula.", "The heaters warmed the base: it cost 1 cell."); break;
                case Frio + ":agasalhar":
                    m.Crescimento = Math.Max(0, m.Crescimento - 1);
                    tom = Tom.Verde; l = T_("As plantas sentiram o frio: estufa -1.", "The plants felt the cold: greenhouse -1."); break;
                case Gelo + ":rover":
                    m.Estrelas += 3; tom = Tom.Laranja; imp = 2; l = T_("O rover trouxe gelo da cratera: ciência +3!", "The rover brought back ice from the crater: science +3!"); break;
                case Gelo + ":anotar":
                    tom = Tom.Laranja; l = T_("O sinal de gelo ficou anotado para outra missão.", "The ice signal was noted for another mission."); break;
                case Terra + ":satelite":
                    tom = Tom.O2; l = T_("O satélite manteve o contato com a Terra: custou 1 célula.", "The satellite kept the contact with Earth: it cost 1 cell."); break;
                case Terra + ":sem":
                    tom = Tom.O2; l = T_("A Terra se escondeu, e hoje a base ficou sem a dica do controle da missão.", "Earth hid, and today the base went without mission control's hint."); break;
                case Sono + ":folga":
                    tom = Tom.Verde; l = T_("A folga da manhã fez bem: a tripulação trabalhou descansada.", "The morning off did everyone good: the crew worked rested."); break;
                case Sono + ":trabalhar":
                    tom = Tom.Laranja; l = T_("Com sono, a tarefa de hoje teve menos tempo.", "Sleepy, the crew had less time for today's task."); break;
                case Saudade + ":chamada":
                    tom = Tom.Verde; l = T_("A videochamada animou a tripulação: custou 1 célula.", "The video call cheered the crew up: it cost 1 cell."); break;
                case Saudade + ":seguir":
                    tom = Tom.Energia; l = T_("A tripulação ficou desanimada: amanhã rende 1 célula a menos.", "The crew felt down: tomorrow they give 1 cell fewer."); break;
                case Fungo + ":cortar":
                    m.Crescimento = Math.Max(0, m.Crescimento - 1);
                    tom = Tom.Verde; l = T_("As folhas doentes foram cortadas: estufa -1.", "The sick leaves were cut off: greenhouse -1."); break;
                case Fungo + ":deixar":
                    if (T.Risco)
                    {
                        m.Crescimento = 0;
                        tom = Tom.Vermelho; imp = 3;
                        l = T_("O fungo se espalhou: a estufa voltou a 0.", "The fungus spread: the greenhouse went back to 0.");
                    }
                    else { tom = Tom.Verde; l = T_("O fungo não se espalhou. Desta vez deu certo.", "The fungus did not spread. It worked this time."); }
                    break;
                default:
                    return;
            }
            linha(tom, l);
            if (imp > impacto)
            {
                impacto = imp;
                momento = MomentoGenerico.F(def.Titulo, e.Titulo);
            }
        }
    }
}
