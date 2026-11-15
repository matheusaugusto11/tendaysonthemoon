using System;
using System.Collections.Generic;

namespace DezDias
{
    /// <summary>Resultado de um minijogo: nota de 0 a 1, tempo usado (s) e se o objetivo foi concluído.</summary>
    public struct ResultadoMinijogo
    {
        public float Nota, Tempo;
        public bool Concluiu;
        public ResultadoMinijogo(float nota, float tempo, bool concluiu) { Nota = nota; Tempo = tempo; Concluiu = concluiu; }
    }

    /// <summary>As 6 mecânicas de minijogo que o Unity já tem.</summary>
    public enum Mecanica { Lixo, Esteira, Canos, Disjuntores, Raspadinha, Segurar }

    /// <summary>Uma tarefa do dia: textos, estação na base, minijogo e as duas escolhas com custo.</summary>
    public sealed class TarefaDef
    {
        public string Id, Estacao;
        public bool Fora;
        /// <summary>Limite de tempo do minijogo em segundos, antes do fator do modo.</summary>
        public float Limite;
        public Mecanica Mec;
        public Txt Nome, Titulo, Texto, Fonte;
        public Txt FazerTitulo, FazerCusto, FazerDesc;
        public Txt FicarTitulo, FicarCusto, FicarDesc;
        public bool FicarRuim;
        public Txt MiniTitulo, MiniInstrucao;
    }

    /// <summary>Tabela das tarefas e seus efeitos (web: textos de vTask, finish de cada MG e as ações *Stay).</summary>
    public static class Tarefas
    {
        public const string Amostra = "amostra", Poeira = "poeira", Estufa = "estufa", Agua = "agua", Upload = "upload",
            Filtros = "filtros", Exercicio = "exercicio", Braco = "braco", Vazamento = "vazamento", Antena = "antena";

        /// <summary>Tarefas de rotina com minijogo no Unity. Painel, falha, micro, rover, co2 e treino ficam fora por enquanto.</summary>
        public static readonly string[] Rotina = { Amostra, Estufa, Agua, Poeira, Upload, Filtros, Exercicio, Braco };
        public static readonly string[] Imprevistos = { Vazamento, Antena };
        public static readonly string[] Todas = { Amostra, Poeira, Estufa, Agua, Upload, Filtros, Exercicio, Braco, Vazamento, Antena };

        static Txt T(string pt, string en) => new Txt(pt, en);
        static readonly Dictionary<string, TarefaDef> _defs = new Dictionary<string, TarefaDef>();
        public static IEnumerable<TarefaDef> Defs => _defs.Values;
        public static TarefaDef Def(string id) => _defs.TryGetValue(id, out var d) ? d : null;

        // Textos da decisão do vazamento e do cartão "Você sabia?" da antena.
        public static readonly Txt VazDecisaoTitulo = T("Furo tapado por enquanto. E agora?", "Hole plugged for now. Now what?");
        public static readonly Txt VazDecisaoTexto = T("Os remendos rápidos não seguram por muito tempo.", "The quick patches will not hold for long.");
        public static readonly Txt VazReforcarTitulo = T("Reforçar os remendos agora", "Reinforce the patches now");
        public static readonly Txt VazReforcarCusto = T("energia -2 hoje", "energy -2 today");
        public static readonly Txt VazReforcarDesc = T("Dois astronautas param o trabalho para terminar o conserto.", "Two astronauts stop their work to finish the repair.");
        public static readonly Txt VazDepoisTitulo = T("Deixar para depois", "Leave it for later");
        public static readonly Txt VazDepoisCusto = T("O2 -1 por dia", "O2 -1 per day");
        public static readonly Txt VazDepoisDesc = T("A energia de hoje fica inteira, mas o ar vai embora até consertar.", "Today's energy stays whole, but air keeps escaping until it is fixed.");
        public static readonly Txt VazBotao = T("Pegar os remendos!", "Grab the patches!");
        public static readonly Txt AtrasoComunicacao = T("Uma mensagem leva cerca de 1,3 segundo para ir da Lua à Terra. De Marte, pode levar até 20 minutos.", "A message takes about 1.3 seconds to go from the Moon to Earth. From Mars, it can take up to 20 minutes.");
        public static readonly Txt AtrasoFontes = T("Fontes: NASA Science (Webb explainers); NASA Human Research Program, Hazard: Distance From Earth; NASA SVS 4944.", "Sources: NASA Science (Webb explainers); NASA Human Research Program, Hazard: Distance From Earth; NASA SVS 4944.");
        static readonly Txt CustouUma = T(" Custou 1 célula de hoje.", " It cost 1 of today's cells.");

        static Tarefas()
        {
            Add(new TarefaDef
            {
                Id = Amostra, Estacao = "airlock", Fora = true, Limite = 15f, Mec = Mecanica.Lixo,
                Nome = T("Saída para coletar pedras", "Rock-collecting walk"),
                Titulo = T("Tem pedras interessantes lá fora", "There are interesting rocks out there"),
                Texto = T("Lá fora o astronauta respira o ar do traje. Quanto mais longe e mais tempo, mais ciência e menos oxigênio. Você decide até onde vale ir.", "Outside, the astronaut breathes the air in the suit. The farther and the longer, the more science and the less oxygen. You decide how far is worth going."),
                FazerTitulo = T("Sair para buscar pedras", "Go out to fetch rocks"),
                FazerCusto = T("O2 -1 a -3 · ciência até +5", "O2 -1 to -3 · science up to +5"),
                FazerDesc = T("Arraste as pedras para a caixa e volte antes de o ar do traje acabar.", "Drag the rocks into the box and come back before the suit's air runs out."),
                FicarTitulo = T("Ficar na base", "Stay at base"),
                FicarCusto = T("O2 0 · ciência 0", "O2 0 · science 0"),
                FicarDesc = T("Guarda o ar, perde a descoberta.", "Saves air, misses the discovery."),
                MiniTitulo = T("Coleta de pedras", "Rock collecting"),
                MiniInstrucao = T("Arraste as pedras para a caixa e aperte VOLTAR. Quanto mais tempo lá fora, mais oxigênio. Se o ar do traje acabar, as pedras se perdem.", "Drag the rocks into the box and press BACK. The longer you stay out, the more oxygen it costs. If the suit runs out of air, the rocks are lost."),
            });
            Add(new TarefaDef
            {
                Id = Poeira, Estacao = "airlock", Fora = true, Limite = 12f, Mec = Mecanica.Raspadinha,
                Nome = T("Painéis solares", "Solar panels"),
                Titulo = T("Os painéis estão cobertos de poeira", "The panels are covered in dust"),
                Texto = T("A poeira da Lua é fina e gruda em tudo. Com poeira em cima, o painel pega menos luz.", "Moon dust is fine and sticks to everything. Under dust, a panel catches less light."),
                FazerTitulo = T("Sair e limpar os painéis", "Go out and clean the panels"),
                FazerCusto = T("O2 -1 · energia +0 a +2 hoje", "O2 -1 · energy +0 to +2 today"),
                FazerDesc = T("Quanto mais limpo, mais energia hoje.", "The cleaner the panel, the more energy today."),
                FicarTitulo = T("Ficar dentro", "Stay inside"),
                FicarCusto = T("O2 0 · energia +0", "O2 0 · energy +0"),
                FicarDesc = T("Economiza ar, os painéis seguem sujos.", "Saves air, the panels stay dirty."),
                MiniTitulo = T("Limpar os painéis", "Clean the panels"),
                MiniInstrucao = T("Clique ou toque e arraste (ou segure A e mova o analógico) para esfregar a poeira!", "Click or tap and drag (or hold A and move the stick) to scrub off the dust!"),
            });
            Add(new TarefaDef
            {
                Id = Estufa, Estacao = "estufa", Limite = 15f, Mec = Mecanica.Disjuntores,
                Nome = T("Estufa", "Greenhouse"),
                Titulo = T("As plantas pedem cuidado", "The plants need care"),
                Texto = T("Planta dá comida e um pouco de ar, mas demora. Ligar a luz e a rega na hora certa faz ela crescer mais rápido.", "Plants give food and a little air, but they take time. Switching on light and water at the right time makes them grow faster."),
                FazerTitulo = T("Cuidar das plantas", "Look after the plants"),
                FazerCusto = T("energia -1 hoje · estufa +1 a +3", "energy -1 today · greenhouse +1 to +3"),
                FazerDesc = T("Quanto mais rápido você ligar a estufa sem estourar a carga, mais ela adianta.", "The faster you switch on the greenhouse without overloading, the further it gets."),
                FicarTitulo = T("Deixar para depois", "Leave it for later"),
                FicarCusto = T("energia -0 · estufa +0", "energy -0 · greenhouse +0"),
                FicarDesc = T("A energia de hoje fica inteira, a comida demora mais.", "Today's energy stays whole, food takes longer."),
                MiniTitulo = T("Quadro da estufa", "Greenhouse switchboard"),
                MiniInstrucao = T("Ligue a ESTUFA arrastando a alavanca para cima. Cuidado com a carga máxima!", "Switch on the GREENHOUSE by dragging its lever up. Watch the maximum load!"),
            });
            Add(new TarefaDef
            {
                Id = Agua, Estacao = "reciclador", Limite = 15f, Mec = Mecanica.Canos,
                Nome = T("Reciclagem de água", "Water recycling"),
                Titulo = T("A máquina de reciclar água pede ajuda", "The water recycler needs a hand"),
                Texto = T("Na base nenhuma gota vai fora: a máquina limpa até o xixi e o suor dos astronautas para virar água de beber. E com água a base também faz oxigênio.", "At the base no drop is wasted: the machine cleans even the astronauts' pee and sweat into drinking water. And with water the base also makes oxygen."),
                FazerTitulo = T("Ligar os canos da reciclagem", "Connect the recycler pipes"),
                FazerCusto = T("energia -1 hoje · O2 +0 a +2", "energy -1 today · O2 +0 to +2"),
                FazerDesc = T("Ligue o tanque de água usada ao de água limpa girando os canos.", "Link the used-water tank to the clean-water tank by turning the pipes."),
                FicarTitulo = T("Deixar para depois", "Leave it for later"),
                FicarCusto = T("energia -0 · O2 +0", "energy -0 · O2 +0"),
                FicarDesc = T("A energia de hoje fica inteira, mas a água de hoje se perde.", "Today's energy stays whole, but today's water is lost."),
                MiniTitulo = T("Canos da reciclagem", "Recycler pipes"),
                MiniInstrucao = T("Clique nos canos para girar e ligue o tanque mostarda (água usada) ao azul (água limpa).", "Click the pipes to turn them and link the mustard tank (used water) to the blue one (clean water)."),
            });
            Add(new TarefaDef
            {
                Id = Upload, Estacao = "backup", Limite = 12f, Mec = Mecanica.Esteira,
                Nome = T("Transmissão de dados", "Data upload"),
                Titulo = T("Hora de mandar as descobertas para a Terra", "Time to send the discoveries to Earth"),
                Texto = T("Enviar um arquivo grande gasta bastante energia da antena. Cada pacote que chega inteiro vale ciência.", "Sending a big file uses a lot of the antenna's energy. Each packet that arrives whole is worth science."),
                FazerTitulo = T("Transmitir os dados", "Upload the data"),
                FazerCusto = T("energia -2 hoje · ciência +0 a +4", "energy -2 today · science +0 to +4"),
                FazerDesc = T("Aperte Enviar quando o sinal estiver na faixa verde.", "Press Send when the signal is in the green band."),
                FicarTitulo = T("Guardar a energia", "Save the energy"),
                FicarCusto = T("energia -0 · ciência 0", "energy -0 · science 0"),
                FicarDesc = T("A base usa a energia em outra coisa. Os dados ficam para depois.", "The base uses the energy on something else. The data waits."),
                MiniTitulo = T("Transmissão de dados", "Data upload"),
                MiniInstrucao = T("Aperte ESPAÇO, A ou toque quando o cursor estiver na faixa verde. Cada acerto envia 1 pacote: são 3 pacotes!", "Press SPACE, A or tap when the cursor is in the green band. Each hit sends 1 packet: there are 3 packets!"),
            });
            Add(new TarefaDef
            {
                Id = Filtros, Estacao = "filtro", Limite = 12f, Mec = Mecanica.Raspadinha,
                Nome = T("Filtros de ar", "Air filters"),
                Titulo = T("Os filtros de ar estão cheios de poeira", "The air filters are full of dust"),
                Texto = T("A poeira da Lua entra na base grudada nos trajes e entope os filtros. Com o filtro sujo, o ar da base piora.", "Moon dust gets into the base stuck to the suits and clogs the filters. With a dirty filter, the air in the base gets worse."),
                FazerTitulo = T("Limpar os filtros", "Clean the filters"),
                FazerCusto = T("energia -1 hoje · O2 -1 a +1", "energy -1 today · O2 -1 to +1"),
                FazerDesc = T("Quanto mais poeira você tirar, melhor fica o ar.", "The more dust you remove, the better the air."),
                FicarTitulo = T("Deixar sujo hoje", "Leave them dirty today"),
                FicarCusto = T("energia -0 · O2 -1", "energy -0 · O2 -1"),
                FicarDesc = T("A energia fica inteira, mas o ar piora hoje.", "The energy stays whole, but the air gets worse today."),
                FicarRuim = true,
                MiniTitulo = T("Filtros de ar", "Air filters"),
                MiniInstrucao = T("Clique ou toque e arraste (ou segure A e mova o analógico) para esfregar a poeira do filtro!", "Click or tap and drag (or hold A and move the stick) to scrub the dust off the filter!"),
            });
            Add(new TarefaDef
            {
                Id = Exercicio, Estacao = "esteira", Limite = 12f, Mec = Mecanica.Esteira,
                Nome = T("Exercício", "Exercise"),
                Titulo = T("Hora do exercício", "Exercise time"),
                Texto = T("No espaço o corpo perde músculo e osso. Na Estação Espacial, os astronautas fazem até 10 horas de exercício por semana para continuar fortes.", "In space the body loses muscle and bone. On the Space Station, astronauts exercise for up to 10 hours a week to stay strong."),
                Fonte = T("Fonte: NASA, Astronaut Exercise.", "Source: NASA, Astronaut Exercise."),
                FazerTitulo = T("Correr na esteira", "Run on the treadmill"),
                FazerCusto = T("energia -1 hoje · amanhã energia -1 a +1", "energy -1 today · tomorrow energy -1 to +1"),
                FazerDesc = T("Uma tripulação em forma trabalha melhor amanhã.", "A crew in good shape works better tomorrow."),
                FicarTitulo = T("Pular o exercício", "Skip the exercise"),
                FicarCusto = T("amanhã energia -1", "tomorrow energy -1"),
                FicarDesc = T("A energia de hoje fica inteira, mas a tripulação fica mais fraca amanhã.", "Today's energy stays whole, but the crew is weaker tomorrow."),
                FicarRuim = true,
                MiniTitulo = T("Esteira", "Treadmill"),
                MiniInstrucao = T("Aperte ESPAÇO, A ou toque quando o cursor estiver na zona verde. São 5 acertos! Errar desfaz um acerto.", "Press SPACE, A or tap when the cursor is in the green zone. You need 5 hits! A miss undoes one hit."),
            });
            Add(new TarefaDef
            {
                Id = Braco, Estacao = "airlock", Limite = 14f, Mec = Mecanica.Lixo,
                Nome = T("Braço robótico", "Robotic arm"),
                Titulo = T("Chegou uma caixa de suprimentos", "A box of supplies has arrived"),
                Texto = T("Na Estação Espacial, os astronautas capturam as naves de carga com um braço robótico, o Canadarm2, e treinam isso a cada 60 dias. Aqui, o braço da base pega a caixa de comida.", "On the Space Station, astronauts capture cargo ships with a robotic arm, Canadarm2, and practise it every 60 days. Here, the base's arm picks up the food box."),
                Fonte = T("Fonte: NASA, blog da Estação Espacial.", "Source: NASA, Space Station blog."),
                FazerTitulo = T("Operar o braço robótico", "Operate the robotic arm"),
                FazerCusto = T("energia -1 hoje · comida +0 a +3 dias", "energy -1 today · food +0 to +3 days"),
                FazerDesc = T("Arraste a caixa até a garra e aperte PEGAR.", "Drag the box to the claw and press GRAB."),
                FicarTitulo = T("Deixar a caixa lá fora", "Leave the box outside"),
                FicarCusto = T("energia -0 · comida +0", "energy -0 · food +0"),
                FicarDesc = T("A energia fica inteira. Hoje não entra comida nova.", "The energy stays whole. No new food comes in today."),
                MiniTitulo = T("Braço robótico", "Robotic arm"),
                MiniInstrucao = T("Arraste a caixa que flutua até a garra e aperte PEGAR. Quanto mais rápido, mais comida.", "Drag the floating box to the claw and press GRAB. The faster you are, the more food."),
            });
            Add(new TarefaDef
            {
                Id = Vazamento, Estacao = "vazamento", Limite = 10f, Mec = Mecanica.Segurar,
                Nome = T("Vazamento de ar", "Air leak"),
                Titulo = T("Alarme! Tem ar escapando do abrigo", "Alarm! Air is escaping from the shelter"),
                Texto = T("Um furo abriu na parede. Segure o remendo até selar, o mais rápido que puder.", "A hole has opened in the wall. Hold the patch until it seals, as fast as you can."),
                MiniTitulo = T("Vazamento", "Air leak"),
                MiniInstrucao = T("SEGURE o clique, o toque ou o botão A para selar o vazamento!", "HOLD the click, the touch or the A button to seal the leak!"),
            });
            Add(new TarefaDef
            {
                Id = Antena, Estacao = "disjuntores", Limite = 15f, Mec = Mecanica.Disjuntores,
                Nome = T("Comunicação com a Terra", "Contact with Earth"),
                Titulo = T("A comunicação com a Terra caiu", "Contact with Earth has dropped"),
                Texto = T("A base perdeu o contato com a Terra. Sem ele, você não recebe a dica do controle da missão antes de cada dia.", "The base has lost contact with Earth. Without it, you do not get mission control's hint before each day."),
                FazerTitulo = T("Religar a comunicação", "Restore communications"),
                FazerCusto = T("energia -1 ou -2 hoje", "energy -1 or -2 today"),
                FazerDesc = T("Ligue a COMUNICAÇÃO no quadro sem estourar a carga. Quanto mais rápido, menos energia gasta.", "Switch on COMMS on the board without overloading it. The faster you are, the less energy it costs."),
                FicarTitulo = T("Seguir sem contato", "Carry on without contact"),
                FicarCusto = T("energia -0", "energy -0"),
                FicarDesc = T("Guarda a energia, mas você decide no escuro até a comunicação voltar.", "Saves energy, but you decide in the dark until communications are back."),
                FicarRuim = true,
                MiniTitulo = T("Quadro de energia", "Power switchboard"),
                MiniInstrucao = T("Ligue a COMUNICAÇÃO (gasta muita energia). Desligue algo para liberar carga.", "Switch on COMMS (it uses a lot of power). Switch something off to free up load."),
            });
        }

        static void Add(TarefaDef d) => _defs[d.Id] = d;

        /// <summary>Tarefa de fora só com O2 acima de 1.</summary>
        public static bool PodeFazer(Missao m)
        {
            var d = Def(m.Hoje.Tarefa);
            return d == null || !d.Fora || m.O2 > 1;
        }

        /// <summary>Limite real do minijogo (segundos), com o fator do modo.</summary>
        public static float LimiteReal(Missao m, string id) => (Def(id)?.Limite ?? 15f) * m.M.FatorTempo * (m.Hoje?.FatorTempoHoje ?? 1f);

        static void Concluir(Missao m, Txt msg)
        {
            m.Hoje.MsgTarefa = msg;
            m.Hoje.TarefaResolvida = true;
        }

        /// <summary>Aplica o efeito de "fazer" com o resultado do minijogo.</summary>
        public static void Fazer(Missao m, ResultadoMinijogo r)
        {
            var T = m.Hoje;
            float f = m.M.FatorTempo;
            float lim = LimiteReal(m, T.Tarefa);
            float nota = Math.Max(0f, Math.Min(1f, r.Nota));
            switch (T.Tarefa)
            {
                case Amostra:
                {
                    int usado, est;
                    T.Fora = true;
                    if (!r.Concluiu)
                    {
                        usado = 3; est = 0;
                        m.O2 -= usado;
                        Concluir(m, Tx("O ar do traje acabou! O astronauta voltou correndo e largou as pedras. Oxigênio -{0}.", "The suit ran out of air! The astronaut rushed back and dropped the rocks. Oxygen -{0}.").F(usado));
                        return;
                    }
                    usado = r.Tempo <= lim / 3f ? 1 : r.Tempo <= lim * 2f / 3f ? 2 : 3;
                    int pedras = (int)Math.Round(nota * 4f);
                    est = pedras >= 4 ? 5 : pedras;
                    m.O2 -= usado;
                    m.Estrelas += est;
                    Concluir(m, est == 0
                        ? Tx("O astronauta voltou sem pedras. Oxigênio -{0}.", "The astronaut came back without rocks. Oxygen -{0}.").F(usado)
                        : Tx("Pedras na base! Ciência +{0}, oxigênio -{1}.", "Rocks back at base! Science +{0}, oxygen -{1}.").F(est, usado));
                    return;
                }
                case Poeira:
                {
                    int n = nota >= .9f ? 2 : nota >= .5f ? 1 : 0;
                    m.O2 -= 1;
                    T.Fora = true;
                    T.Bonus = n;
                    Concluir(m, Tx("Painéis {0}% limpos: energia +{1} hoje. Oxigênio -1.", "Panels {0}% clean: energy +{1} today. Oxygen -1.").F(Pct(nota), n));
                    return;
                }
                case Estufa:
                {
                    int n = nota >= .85f ? 3 : nota >= .55f ? 2 : 1;
                    T.Custo = 1;
                    m.Crescimento += n;
                    Concluir(m, Tx("As plantas foram cuidadas: a estufa adiantou {0}.", "The plants were looked after: the greenhouse moved {0} ahead.").F(n) + CustouUma);
                    return;
                }
                case Agua:
                {
                    int n = nota >= .8f ? 2 : nota >= .5f ? 1 : 0;
                    T.Custo = 1;
                    m.O2 = Math.Min(Missao.MaxO2, m.O2 + n);
                    Concluir(m, (n > 0
                        ? Tx("A água voltou limpa: oxigênio +{0}.", "The water came back clean: oxygen +{0}.").F(n)
                        : Tx("A água não chegou à máquina hoje: oxigênio +0.", "The water did not reach the machine today: oxygen +0.")) + CustouUma);
                    return;
                }
                case Upload:
                {
                    int enviados = (int)Math.Round(nota * 3f);
                    int est = enviados == 3 ? 4 : enviados;
                    T.Custo = 2;
                    m.Estrelas += est;
                    Concluir(m, Tx("{0} pacote(s) chegaram inteiros: ciência +{1}. Custou 2 células de hoje.", "{0} packet(s) arrived whole: science +{1}. It cost 2 of today's cells.").F(enviados, est));
                    return;
                }
                case Filtros:
                {
                    int n = nota >= .85f ? 1 : nota >= .5f ? 0 : -1;
                    int antes = m.O2;
                    T.Custo = 1;
                    m.O2 = Math.Max(1, Math.Min(Missao.MaxO2, m.O2 + n));
                    int d = m.O2 - antes;
                    Concluir(m, (d > 0 ? Tx("Filtros limpos: oxigênio +{0}.", "Filters clean: oxygen +{0}.").F(d)
                        : d == 0 ? Tx("Filtros meio limpos: o oxigênio ficou igual.", "Filters half clean: oxygen stayed the same.")
                        : Tx("Ficou poeira nos filtros: oxigênio -{0}.", "Dust stayed in the filters: oxygen -{0}.").F(-d)) + CustouUma);
                    return;
                }
                case Exercicio:
                {
                    int fit = nota >= .999f ? 1 : nota >= .6f ? 0 : -1;
                    T.Custo = 1;
                    m.FitAmanha = fit;
                    Concluir(m, (fit > 0 ? Tx("Treino completo! Amanhã a tripulação rende 1 célula a mais.", "Full workout! Tomorrow the crew gives 1 extra cell.")
                        : fit == 0 ? Tx("Treino pela metade: amanhã fica igual.", "Half a workout: tomorrow stays the same.")
                        : Tx("O treino foi fraco: amanhã a tripulação rende 1 célula a menos.", "A weak workout: tomorrow the crew gives 1 cell fewer.")) + CustouUma);
                    return;
                }
                case Braco:
                {
                    int n = !r.Concluiu ? 0 : r.Tempo <= 8f * f ? 3 : 2;
                    T.Custo = 1;
                    m.Comida += n;
                    Concluir(m, (n == 0 ? Tx("A caixa escapou da garra. Hoje não entra comida nova.", "The box slipped past the claw. No new food comes in today.")
                        : Tx("Caixa pega! Comida +{0} dias.", "Box caught! Food +{0} days.").F(n)) + CustouUma);
                    return;
                }
                case Vazamento:
                {
                    int base0 = r.Concluiu && r.Tempo <= lim / 2f ? 0 : r.Concluiu ? 1 : 2;
                    int perdido = Math.Max(0, Math.Min(m.O2 - 1, base0));
                    m.O2 -= perdido;
                    T.MsgVazamento = perdido == 0
                        ? Tx("Que rapidez! Quase nenhum ar escapou.", "So quick! Almost no air escaped.")
                        : Tx("Escapou ar enquanto o furo ficou aberto: oxigênio -{0}.", "Air escaped while the hole was open: oxygen -{0}.").F(perdido);
                    return; // falta a decisão: reforçar agora ou deixar para depois
                }
                case Antena:
                {
                    int custo = r.Concluiu && r.Tempo <= 12f * f ? 1 : 2;
                    T.Custo = custo;
                    m.Contato = true;
                    T.Extra = AtrasoComunicacao;
                    T.ExtraFonte = AtrasoFontes;
                    Concluir(m, custo == 1
                        ? Tx("Contato recuperado bem rápido! Custou 1 célula de hoje.", "Contact restored really fast! It cost 1 of today's cells.")
                        : Tx("Contato recuperado, mas demorou: custou 2 células de hoje.", "Contact restored, but it took a while: it cost 2 of today's cells."));
                    return;
                }
            }
        }

        /// <summary>Aplica o efeito de "ficar" (web: as ações *Stay).</summary>
        public static void Ficar(Missao m)
        {
            switch (m.Hoje.Tarefa)
            {
                case Amostra: Concluir(m, Tx("A tripulação ficou na base. O ar foi poupado, a descoberta ficou para outro dia.", "The crew stayed at base. Air was saved, the discovery waits for another day.")); return;
                case Poeira: Concluir(m, Tx("Os painéis seguem com poeira. Ar poupado, energia normal hoje.", "The panels stay dusty. Air saved, normal energy today.")); return;
                case Estufa: Concluir(m, Tx("As plantas seguem no ritmo delas. A energia de hoje ficou inteira.", "The plants carry on at their own pace. Today's energy stays whole.")); return;
                case Agua: Concluir(m, Tx("A máquina ficou parada hoje. A energia ficou inteira.", "The machine stayed off today. The energy stays whole.")); return;
                case Upload: Concluir(m, Tx("Os dados ficaram guardados no computador. A energia de hoje ficou inteira.", "The data stays saved on the computer. Today's energy stays whole.")); return;
                case Filtros:
                {
                    int antes = m.O2;
                    m.O2 = Math.Max(1, m.O2 - 1);
                    Concluir(m, Tx("Os filtros ficaram sujos: oxigênio -{0}. A energia de hoje ficou inteira.", "The filters stayed dirty: oxygen -{0}. Today's energy stays whole.").F(antes - m.O2));
                    return;
                }
                case Exercicio:
                    m.FitAmanha = -1;
                    Concluir(m, Tx("Sem exercício hoje. Amanhã a tripulação rende 1 célula a menos.", "No exercise today. Tomorrow the crew gives 1 cell fewer."));
                    return;
                case Braco: Concluir(m, Tx("A caixa ficou lá fora. A energia de hoje ficou inteira.", "The box stays outside. The energy stays whole.")); return;
                case Vazamento: VazamentoDepois(m); return;
                case Antena:
                    m.Contato = false;
                    Concluir(m, Tx("A base segue sem contato com a Terra. A energia de hoje ficou inteira.", "The base carries on without contact with Earth. Today's energy stays whole."));
                    return;
            }
        }

        public static void VazamentoReforcar(Missao m)
        {
            m.Hoje.Custo = 2;
            Concluir(m, Tx("Remendos reforçados! Custou 2 células de hoje.", "Patches reinforced! It cost 2 of today's cells."));
        }

        public static void VazamentoDepois(Missao m)
        {
            m.Vazamento = true;
            Concluir(m, Tx("Os remendos rápidos não seguraram: oxigênio -1 por dia até o conserto (2 células).", "The quick patches did not hold: oxygen -1 per day until repaired (2 cells)."));
        }

        /// <summary>O relógio do dia acabou antes de a tarefa ser resolvida: vale a escolha "ficar" dela.</summary>
        public static void TempoEsgotado(Missao m)
        {
            if (m.Hoje.TarefaResolvida) return;
            Ficar(m);
            m.Hoje.TempoEsgotou = true;
            m.Hoje.MsgTarefa = Textos.TempoAcabouTarefa + m.Hoje.MsgTarefa;
        }

        static Txt Tx(string pt, string en) => new Txt(pt, en);
        static int Pct(float nota) => (int)Math.Round(nota * 100f);
    }
}
