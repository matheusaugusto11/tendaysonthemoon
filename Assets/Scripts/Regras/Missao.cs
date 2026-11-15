using System.Collections.Generic;

namespace DezDias
{
    public enum Modo { Cadete, Comandante }
    public enum Modulo { Ar = 0, Estufa = 1, Abrigo = 2, Lab = 3, Bateria = 4, Conserto = 5 }
    public enum Fim { Nenhum, Oxigenio, Radiacao, Comida, Completa }
    /// <summary>Cor de destaque de uma linha de resultado (igual às cores dos medidores da web).</summary>
    public enum Tom { O2, Verde, Rad, Laranja, Energia, Vermelho }

    /// <summary>Valores de jogo por modo (web: const MODES). Não são dados reais.</summary>
    public sealed class ParametrosModo
    {
        public int Luz, Sombra, O2, Comida, Alertas, Reais, Meta3, Meta2;
        public double ProbForte, ProbForteFalso;
        /// <summary>Multiplica o tempo dos minijogos e os limites de "rápido" (web: TFAC).</summary>
        public float FatorTempo;

        public static readonly ParametrosModo Cadete = new ParametrosModo
        { Luz = 6, Sombra = 4, O2 = 9, Comida = 7, Alertas = 4, Reais = 2, ProbForte = .8, ProbForteFalso = .2, Meta3 = 29, Meta2 = 16, FatorTempo = 1.3f };
        public static readonly ParametrosModo Comandante = new ParametrosModo
        { Luz = 6, Sombra = 3, O2 = 8, Comida = 6, Alertas = 6, Reais = 3, ProbForte = .7, ProbForteFalso = .3, Meta3 = 21, Meta2 = 14, FatorTempo = 1f };

        public static ParametrosModo De(Modo m) => m == Modo.Cadete ? Cadete : Comandante;
    }

    /// <summary>Uma erupção real do catálogo FLR do NASA DONKI (dado arquivado).</summary>
    public sealed class Erupcao
    {
        public string Id, Data, Classe, Url;
        /// <summary>0 fraca (classe C), 1 média (M), 2 forte (X). Mapeamento nosso.</summary>
        public int Intensidade;
    }

    public sealed class Alerta
    {
        /// <summary>null = alarme falso.</summary>
        public Erupcao Erupcao;
        public bool Forte;
    }

    /// <summary>Células colocadas em cada módulo no dia.</summary>
    public sealed class Alocacao
    {
        readonly int[] _c = new int[6];
        public int this[Modulo m] { get => _c[(int)m]; set => _c[(int)m] = value; }
        public int Total { get { int s = 0; foreach (var v in _c) s += v; return s; } }
        public void Zerar() { for (int i = 0; i < _c.Length; i++) _c[i] = 0; }
    }

    public sealed class LinhaResultado
    {
        public Tom Tom;
        public Txt Texto;
        public LinhaResultado(Tom tom, Txt texto) { Tom = tom; Texto = texto; }
    }

    public sealed class ResultadoDia
    {
        public int O2Antes, RadAntes, BatAntes, EstrelasAntes, ComidaAntes;
        public readonly List<LinhaResultado> Linhas = new List<LinhaResultado>();
        public Erupcao Erupcao;
        public bool FalsoAlarme, Colheita;
        public int DRad, Perdidas;
        public Fim Fim;
    }

    public sealed class RegistroDia
    {
        public int Dia, O2, Rad, Bat, Impacto;
        public Txt Momento;
    }

    /// <summary>Estado do dia em andamento (web: G.T).</summary>
    public sealed class DiaMissao
    {
        public int Fit, Luz, Bonus, Custo;
        public string Tarefa;
        /// <summary>Algum astronauta saiu da base hoje (fica fora do abrigo se a erupção vier).</summary>
        public bool Fora;
        public bool TarefaResolvida;
        /// <summary>O relógio do dia acabou antes da tarefa (ela valeu como "ficar").</summary>
        public bool TempoEsgotou;
        public readonly Alocacao Aloc = new Alocacao();
        public ResultadoDia Res;
        public Txt MsgTarefa, MsgVazamento;
        /// <summary>Cartão "Você sabia?" depois da tarefa (vazio = sem cartão).</summary>
        public Txt Extra, ExtraFonte;
    }

    /// <summary>Estado da partida inteira (web: G).</summary>
    public sealed class Missao
    {
        public const int MaxO2 = 10, MaxRad = 10, MaxBat = 4, Dias = 10, Crescer = 4;
        public static readonly int[] Dose = { 1, 3, 5 };

        public Modo Modo;
        public ParametrosModo M;
        public bool Tutorial;
        public int Dia;
        public int O2, Rad, Comida, Crescimento, Bateria, Estrelas;
        public bool Vazamento, Contato = true;
        /// <summary>Efeito do exercício para o próximo dia: −1, 0 ou +1 célula.</summary>
        public int FitAmanha;
        public int[] Sombra = new int[2];
        public readonly Dictionary<int, Alerta> Alertas = new Dictionary<int, Alerta>();
        public readonly List<string> Tarefas = new List<string>();
        public readonly List<RegistroDia> Hist = new List<RegistroDia>();
        public DiaMissao Hoje;
        public Fim Fim = Fim.Nenhum;

        public bool EhSombra(int d) => Sombra[0] == d || Sombra[1] == d;
        public Alerta AlertaDe(int d) => Alertas.TryGetValue(d, out var a) ? a : null;
    }
}
