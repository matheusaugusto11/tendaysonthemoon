using System;
using System.Collections.Generic;

namespace DezDias.Testes
{
    /// <summary>Monta missões previsíveis para os testes.</summary>
    static class Apoio
    {
        public static Erupcao X(string data = "2017-09-06", string classe = "X9.3") => new Erupcao { Id = data + "-X", Data = data, Classe = classe, Intensidade = 2, Url = "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/13010/-1" };
        public static Erupcao M(string data = "2024-05-08") => new Erupcao { Id = data + "-M", Data = data, Classe = "M9.8", Intensidade = 1, Url = "u" };
        public static Erupcao C(string data = "2024-01-21") => new Erupcao { Id = data + "-C", Data = data, Classe = "C5.3", Intensidade = 0, Url = "u" };

        /// <summary>As 20 erupções curadas (5 X, 10 M, 5 C), só com o que as regras usam.</summary>
        public static List<Erupcao> Vinte()
        {
            var l = new List<Erupcao>();
            for (int i = 0; i < 5; i++) l.Add(X("2024-05-1" + i, "X" + (5 + i)));
            for (int i = 0; i < 10; i++) l.Add(M("2025-" + (i + 1).ToString("00") + "-01"));
            for (int i = 0; i < 5; i++) l.Add(C("2026-0" + (i + 1) + "-02"));
            return l;
        }

        /// <summary>Missão no dia indicado, sem sombra e sem alerta, com a tarefa escolhida.</summary>
        public static Missao Controlada(Modo modo = Modo.Cadete, string tarefa = Tarefas.Estufa, int dia = 3)
        {
            var m = RegrasMissao.NovaPartida(modo, Vinte(), new Random(1), false);
            m.Sombra = new[] { -1, -1 };
            m.Alertas.Clear();
            m.Tarefas.Clear();
            for (int i = 0; i < Missao.Dias; i++) m.Tarefas.Add(tarefa);
            m.Dia = dia - 1;
            RegrasMissao.IniciarDia(m);
            return m;
        }

        public static void Alocar(Missao m, int ar = 0, int estufa = 0, int abrigo = 0, int lab = 0, int bat = 0, int conserto = 0)
        {
            var a = m.Hoje.Aloc;
            a.Zerar();
            a[Modulo.Ar] = ar; a[Modulo.Estufa] = estufa; a[Modulo.Abrigo] = abrigo;
            a[Modulo.Lab] = lab; a[Modulo.Bateria] = bat; a[Modulo.Conserto] = conserto;
        }

        public static bool TemLinha(ResultadoDia r, Txt modelo)
        {
            foreach (var l in r.Linhas) if (l.Texto.Pt == modelo.Pt) return true;
            return false;
        }

        public static bool TemLinhaComeco(ResultadoDia r, string prefixoPt)
        {
            foreach (var l in r.Linhas) if (l.Texto.Pt.StartsWith(prefixoPt)) return true;
            return false;
        }
    }
}
