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
        /// <summary>SEP medido ligado à erupção no DONKI (vazio se não há). Data e hora UTC, link e data da consulta.</summary>
        public string SepId, SepHora, SepUrl, SepConsulta;
        public bool TemSep => !string.IsNullOrEmpty(SepId);
    }

    /// <summary>Uma ejeção de massa coronal real do catálogo CME do NASA DONKI (dado arquivado).</summary>
    public sealed class Cme
    {
        public string Id, Data, Url, FlrLigado, Consulta;
        public int Velocidade;
    }

    /// <summary>Onde a tripulação estava quando tocou o alarme de partículas (spec eventos-do-dia).</summary>
    public enum Alarme { Nenhum, Abrigo, Atrasado, Continuou }

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

        // ---------- Evento do dia (spec eventos-do-dia, design D2) ----------
        /// <summary>Id do evento de hoje (ver <see cref="Eventos"/>).</summary>
        public string Evento;
        /// <summary>true: abre na ida até a tarefa; false: na volta, depois da tarefa.</summary>
        public bool EventoNaIda;
        /// <summary>Segundos de deslocamento até o evento da ida abrir (4 a 12).</summary>
        public float GatilhoSeg;
        public bool EventoResolvido;
        /// <summary>Id da escolha feita no cartão.</summary>
        public string Escolha;
        /// <summary>Células de hoje gastas pelo evento. Fica separado de <see cref="Custo"/>, que as tarefas sobrescrevem.</summary>
        public int CustoEvento;
        public Alarme Alarme;
        public bool SemContatoHoje;
        /// <summary>Multiplica o limite do minijogo de hoje (sono).</summary>
        public float FatorTempoHoje = 1f;
        /// <summary>O risco sorteado na escolha (tremor, fungo) aconteceu. Só aparece no resultado.</summary>
        public bool Risco;
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

        // ---------- Eventos do dia (design D2, D3, D5) ----------
        /// <summary>Sorteios da partida depois do início (evento do dia, momento, riscos).</summary>
        public System.Random Rng;
        public readonly List<string> EventosUsados = new List<string>();
        /// <summary>Células de luz a mais ou a menos num dia futuro, deixadas por um evento, com o motivo para o Boletim.</summary>
        public readonly Dictionary<int, int> AjusteLuz = new Dictionary<int, int>();
        public readonly Dictionary<int, Txt> MotivoAjuste = new Dictionary<int, Txt>();
        /// <summary>Dia do aviso da CME e dia da chegada (0 = sem CME na partida).</summary>
        public int CmeDia, CmeChegada;
        public Cme CmeRegistro;
        /// <summary>O registro da CME é o ligado à erupção da chegada no DONKI.</summary>
        public bool CmeLigada;
        public bool AbrigoPreparado;

        public int AjusteDe(int d) => AjusteLuz.TryGetValue(d, out var v) ? v : 0;

        public void Ajustar(int d, int delta, Txt motivo)
        {
            AjusteLuz[d] = AjusteDe(d) + delta;
            MotivoAjuste[d] = MotivoAjuste.TryGetValue(d, out var t) && !t.Vazio ? t + new Txt(" ", " ") + motivo : motivo;
        }

        public bool EhSombra(int d) => Sombra[0] == d || Sombra[1] == d;
        public Alerta AlertaDe(int d) => Alertas.TryGetValue(d, out var a) ? a : null;
    }
}
