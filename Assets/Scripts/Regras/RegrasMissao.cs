using System;
using System.Collections.Generic;
using System.Linq;

namespace DezDias
{
    /// <summary>
    /// Regras da missão, traduzidas de web/index.html (funções newGame, pickTasks, startDay, totalCells,
    /// enterAlloc, previewLines, resolve, rating e vEnd). Todos os números são valores de jogo.
    /// </summary>
    public static class RegrasMissao
    {
        public static void Embaralhar<T>(IList<T> a, Random r)
        {
            for (int i = a.Count - 1; i > 0; i--)
            {
                int j = r.Next(i + 1);
                (a[i], a[j]) = (a[j], a[i]);
            }
        }

        // ---------------- Partida (web: newGame) ----------------

        public static Missao NovaPartida(Modo modo, IList<Erupcao> erupcoes, Random rng, bool tutorial)
        {
            var M = ParametrosModo.De(modo);
            var m = new Missao
            {
                Modo = modo, M = M, Tutorial = tutorial,
                O2 = M.O2, Rad = 0, Comida = M.Comida, Crescimento = 0, Bateria = 1, Estrelas = 0,
            };

            // Dois dias de sombra entre 4 e 9, com 2 ou mais dias de distância.
            int[] pool = { 4, 5, 6, 7, 8, 9 };
            int s1 = pool[rng.Next(6)], s2;
            do { s2 = pool[rng.Next(6)]; } while (Math.Abs(s2 - s1) < 2);
            m.Sombra = new[] { s1, s2 };

            // Dias de alerta entre 2 e 10; os primeiros "Reais" têm erupção real, sempre com uma forte (X).
            var dias = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Embaralhar(dias, rng);
            dias = dias.Take(M.Alertas).ToList();
            var picks = new List<Erupcao>(erupcoes ?? new List<Erupcao>());
            Embaralhar(picks, rng);
            var forte = picks.FirstOrDefault(f => f.Intensidade == 2) ?? picks.FirstOrDefault();
            var reais = new List<Erupcao>();
            if (forte != null)
            {
                reais.Add(forte);
                reais.AddRange(picks.Where(f => f != forte).Take(M.Reais - 1));
                Embaralhar(reais, rng);
            }
            for (int i = 0; i < dias.Count; i++)
            {
                var e = i < M.Reais && i < reais.Count ? reais[i] : null;
                m.Alertas[dias[i]] = new Alerta { Erupcao = e, Forte = rng.NextDouble() < (e != null ? M.ProbForte : M.ProbForteFalso) };
            }

            m.Tarefas.AddRange(SortearTarefas(modo, rng, tutorial));
            return m;
        }

        /// <summary>
        /// Sorteia as 10 tarefas (web: pickTasks): rotina nos dias 1 a 4 e 9 e 10, mistura nos dias 5 a 8.
        /// Nunca repete em dias seguidos e usa cada tarefa no máximo 2 vezes.
        /// </summary>
        public static List<string> SortearTarefas(Modo modo, Random rng, bool tutorial)
        {
            var usos = new Dictionary<string, int>();
            var saida = new List<string>();
            int Usos(string t) => usos.TryGetValue(t, out var n) ? n : 0;
            void Pegar(string[] pool)
            {
                var ok = pool.ToList();
                Embaralhar(ok, rng);
                ok = ok.Where(t => Usos(t) < 2 && (saida.Count == 0 || t != saida[saida.Count - 1])).ToList();
                var t0 = ok.Count > 0 ? ok[0] : Tarefas.Amostra;
                usos[t0] = Usos(t0) + 1;
                saida.Add(t0);
            }

            if (tutorial) { saida.Add(Tarefas.Amostra); usos[Tarefas.Amostra] = 1; }   // o tutorial do Dia 1 foi escrito para a coleta de pedras
            else Pegar(Tarefas.Rotina);
            for (int i = 0; i < 3; i++) Pegar(Tarefas.Rotina);
            var mix = modo == Modo.Comandante ? new List<bool> { true, true, true, false } : new List<bool> { true, true, false, false };
            Embaralhar(mix, rng);
            foreach (var imprevisto in mix) Pegar(imprevisto ? Tarefas.Imprevistos : Tarefas.Rotina);
            Pegar(Tarefas.Rotina);
            Pegar(Tarefas.Rotina);
            return saida;
        }

        // ---------------- Dia (web: startDay, totalCells) ----------------

        public static void IniciarDia(Missao m)
        {
            m.Dia++;
            var tarefa = m.Tarefas[m.Dia - 1];
            if (m.EhSombra(m.Dia) && tarefa == Tarefas.Poeira) tarefa = Tarefas.Estufa;
            int fit = m.FitAmanha;
            m.FitAmanha = 0;
            m.Hoje = new DiaMissao
            {
                Fit = fit,
                Luz = Math.Max(1, (m.EhSombra(m.Dia) ? m.M.Sombra : m.M.Luz) + fit),
                Tarefa = tarefa,
            };
        }

        public static int CelulasTotais(Missao m) => Math.Max(0, m.Hoje.Luz + m.Hoje.Bonus - m.Hoje.Custo) + m.Bateria;
        public static int CelulasUsadas(Missao m) => m.Hoje.Aloc.Total;
        public static int CelulasLivres(Missao m) => CelulasTotais(m) - CelulasUsadas(m);

        public static Txt ParteDaMissao(int dia) => dia <= 4 ? Textos.Parte1 : dia <= 8 ? Textos.Parte2 : Textos.Parte3;

        // ---------------- Distribuição (web: modules, enterAlloc, plus, minus) ----------------

        public static int Maximo(Modulo mod)
        {
            switch (mod)
            {
                case Modulo.Ar: return 3;
                case Modulo.Estufa: return 2;
                case Modulo.Abrigo: return 2;
                case Modulo.Lab: return 3;
                case Modulo.Bateria: return Missao.MaxBat;
                default: return 2;
            }
        }

        public static List<Modulo> ModulosVisiveis(Missao m)
        {
            var l = new List<Modulo> { Modulo.Ar, Modulo.Estufa, Modulo.Abrigo, Modulo.Lab, Modulo.Bateria };
            if (m.Vazamento) l.Add(Modulo.Conserto);
            return l;
        }

        public static Txt DicaModulo(Missao m, Modulo mod)
        {
            switch (mod)
            {
                case Modulo.Ar: return Textos.DicaAr;
                case Modulo.Estufa: return Textos.DicaEstufa.F(Math.Max(0, Missao.Crescer - m.Crescimento));
                case Modulo.Abrigo: return Textos.DicaAbrigo;
                case Modulo.Lab: return Textos.DicaLab;
                case Modulo.Bateria: return Textos.DicaBateria;
                default: return Textos.DicaConserto;
            }
        }

        /// <summary>Abre a distribuição: a bateria começa no módulo Bateria; no Cadete, até 2 em Ar e água.</summary>
        public static void PrepararDistribuicao(Missao m)
        {
            var a = m.Hoje.Aloc;
            a.Zerar();
            int total = CelulasTotais(m);
            a[Modulo.Bateria] = Math.Min(m.Bateria, total);
            if (m.Modo == Modo.Cadete) a[Modulo.Ar] = Math.Min(2, total - a[Modulo.Bateria]);
        }

        public static bool PodeMais(Missao m, Modulo mod)
            => ModulosVisiveis(m).Contains(mod) && m.Hoje.Aloc[mod] < Maximo(mod) && CelulasLivres(m) > 0;

        public static bool Mais(Missao m, Modulo mod)
        {
            if (!PodeMais(m, mod)) return false;
            m.Hoje.Aloc[mod]++;
            return true;
        }

        public static bool Menos(Missao m, Modulo mod)
        {
            if (m.Hoje.Aloc[mod] <= 0) return false;
            m.Hoje.Aloc[mod]--;
            return true;
        }

        static int DeltaAr(int celulas) => celulas >= 3 ? 1 : celulas == 2 ? 0 : celulas == 1 ? -1 : -2;

        /// <summary>Dica do controle da missão (web: previewLines).</summary>
        public static List<Txt> Previa(Missao m)
        {
            var a = m.Hoje.Aloc;
            var saida = new List<Txt>();
            if (!m.Contato) { saida.Add(Textos.PreviaSemContato); return saida; }
            int dO = DeltaAr(a[Modulo.Ar]) - (m.Vazamento && a[Modulo.Conserto] < 2 ? 1 : 0);
            saida.Add(dO == 0 ? Textos.PreviaO2Igual : dO > 0 ? Textos.PreviaO2Sobe.F(dO) : Textos.PreviaO2Cai.F(-dO));
            if (m.AlertaDe(m.Dia) != null)
                saida.Add(a[Modulo.Abrigo] >= 2 ? Textos.PreviaAbrigo2 : a[Modulo.Abrigo] == 1 ? Textos.PreviaAbrigo1 : Textos.PreviaAbrigo0);
            if (a[Modulo.Lab] > 0) saida.Add(Textos.PreviaCiencia.F(a[Modulo.Lab]));
            if (a[Modulo.Estufa] > 0 || m.Crescimento >= Missao.Crescer)
                saida.Add(m.Crescimento + a[Modulo.Estufa] >= Missao.Crescer ? Textos.PreviaColheita
                    : Textos.PreviaCresce.F(Missao.Crescer - m.Crescimento - a[Modulo.Estufa]));
            if (m.Comida <= 1) saida.Add(Textos.PreviaComidaAcabando);
            saida.Add(Textos.PreviaGuardadas.F(a[Modulo.Bateria]));
            return saida;
        }

        // ---------------- Resolução (web: resolve) ----------------

        public static ResultadoDia Resolver(Missao m)
        {
            var T = m.Hoje;
            var a = T.Aloc;
            var r = new ResultadoDia { O2Antes = m.O2, RadAntes = m.Rad, BatAntes = m.Bateria, EstrelasAntes = m.Estrelas, ComidaAntes = m.Comida };
            void Linha(Tom tom, Txt t) => r.Linhas.Add(new LinhaResultado(tom, t));
            int impacto = 0;
            Txt momento = Txt.Nada;

            int ar = a[Modulo.Ar];
            int dO2 = DeltaAr(ar);
            Linha(Tom.O2, ar >= 3 ? Textos.ArTotal : ar == 2 ? Textos.ArLigado : ar == 1 ? Textos.ArPouca : Textos.ArSem);

            if (m.Vazamento)
            {
                if (a[Modulo.Conserto] >= 2) { m.Vazamento = false; Linha(Tom.Vermelho, Textos.VazamentoConsertado); }
                else { dO2 -= 1; Linha(Tom.Vermelho, Textos.VazamentoContinua); }
            }

            m.Crescimento += a[Modulo.Estufa];
            if (m.Crescimento >= Missao.Crescer)
            {
                m.Crescimento -= Missao.Crescer;
                m.Comida += 4;
                dO2 += 1;
                r.Colheita = true;
                Linha(Tom.Verde, Textos.Colheita);
                momento = Textos.MomentoColheita;
                impacto = 2;
            }
            else if (a[Modulo.Estufa] > 0) Linha(Tom.Verde, Textos.EstufaCresceu.F(Missao.Crescer - m.Crescimento));

            if (a[Modulo.Lab] > 0)
            {
                m.Estrelas += a[Modulo.Lab];
                Linha(Tom.Laranja, Textos.LabRendeu.F(a[Modulo.Lab]));
            }

            int dRad = 0;
            var al = m.AlertaDe(m.Dia);
            if (al != null)
            {
                if (al.Erupcao != null)
                {
                    r.Erupcao = al.Erupcao;
                    int cheia = Missao.Dose[Math.Max(0, Math.Min(2, al.Erupcao.Intensidade))];
                    int abrigo = a[Modulo.Abrigo];
                    dRad = abrigo >= 2 ? 0 : abrigo == 1 ? (cheia + 1) / 2 : cheia;
                    Linha(Tom.Rad, dRad == 0 ? Textos.ErupcaoProtegeu : abrigo == 1 ? Textos.ErupcaoMeia.F(dRad) : Textos.ErupcaoSem.F(dRad));
                    if (T.Fora) { dRad += 2; Linha(Tom.Rad, Textos.AstronautaFora); }
                    if (dRad > 0)
                    {
                        impacto = Math.Max(impacto, dRad + 2);
                        momento = T.Fora && abrigo >= 2 ? Textos.MomentoFora.F(dRad) : Textos.MomentoAbrigoFraco.F(dRad);
                    }
                    else
                    {
                        impacto = Math.Max(impacto, 2);
                        momento = Textos.MomentoAbrigoLigado;
                    }
                }
                else
                {
                    r.FalsoAlarme = true;
                    Linha(Tom.Rad, a[Modulo.Abrigo] > 0 ? Textos.FalsoComAbrigo.F(a[Modulo.Abrigo]) : Textos.FalsoSemAbrigo);
                }
            }

            int perdidas = CelulasTotais(m) - a.Total;
            if (perdidas > 0) Linha(Tom.Energia, Textos.CelulasPerdidas.F(perdidas));

            m.Bateria = Math.Min(Missao.MaxBat, a[Modulo.Bateria]);
            m.O2 = Math.Max(0, Math.Min(Missao.MaxO2, m.O2 + dO2));
            m.Rad = Math.Min(Missao.MaxRad, m.Rad + dRad);
            bool fome = false;
            if (m.Comida > 0) m.Comida--; else fome = true;

            int caiu = r.O2Antes - m.O2;
            if (caiu >= 2 && impacto < caiu + 1) { impacto = caiu + 1; momento = Textos.MomentoO2Caiu.F(caiu); }
            if (m.EhSombra(m.Dia) && impacto < 2) { impacto = 2; momento = Textos.MomentoSombra; }

            var fim = m.O2 <= 0 ? Fim.Oxigenio
                : m.Rad >= Missao.MaxRad ? Fim.Radiacao
                : fome ? Fim.Comida
                : m.Dia >= Missao.Dias ? Fim.Completa
                : Fim.Nenhum;

            r.DRad = dRad;
            r.Perdidas = Math.Max(0, perdidas);
            r.Fim = fim;
            T.Res = r;
            m.Hist.Add(new RegistroDia { Dia = m.Dia, O2 = m.O2, Rad = m.Rad, Bat = m.Bateria, Impacto = impacto, Momento = momento });
            m.Fim = fim;
            return r;
        }

        /// <summary>Frase da fase "o dia acontece" (web: animCaption).</summary>
        public static Txt Legenda(Missao m, int passo)
        {
            var r = m.Hoje.Res;
            if (passo == 0 || r == null) return Textos.AnimComeco;
            if (r.Erupcao != null) return r.DRad > 0 ? Textos.AnimErupcaoExposta : Textos.AnimErupcaoSegurou;
            if (r.FalsoAlarme) return Textos.AnimFalso;
            if (r.Colheita) return Textos.AnimColheita;
            if (m.EhSombra(m.Dia)) return Textos.AnimSombra;
            return Textos.AnimTranquilo;
        }

        // ---------------- Relatório (web: rating, vEnd) ----------------

        public static int Nota(Missao m)
        {
            if (m.Fim != Fim.Completa) return 0;
            if (m.Estrelas >= m.M.Meta3 && m.Rad <= 3) return 3;
            return m.Estrelas >= m.M.Meta2 ? 2 : 1;
        }

        public static Txt TituloFim(Missao m) => m.Fim == Fim.Completa ? Textos.MissaoCompleta : Textos.MissaoInterrompida.F(m.Dia);

        public static Txt SubtituloFim(Missao m)
        {
            switch (m.Fim)
            {
                case Fim.Completa: return Textos.SubCompleta;
                case Fim.Oxigenio: return Textos.SubO2;
                case Fim.Radiacao: return Textos.SubRad;
                default: return Textos.SubComida;
            }
        }

        public static Txt DicaFim(Missao m)
        {
            int n = Nota(m);
            Txt dica = m.Fim == Fim.Oxigenio ? Textos.DicaFimO2
                : m.Fim == Fim.Radiacao ? Textos.DicaFimRad
                : m.Fim == Fim.Comida ? Textos.DicaFimComida
                : n < 3 && m.Rad > 3 ? Textos.DicaFimRadAlta
                : n < 3 ? Textos.DicaFim3Estrelas.F(m.M.Meta3)
                : Textos.DicaFimExcelente;
            return dica + Textos.EventosMudam;
        }

        /// <summary>Até 3 dias de maior impacto, em ordem de dia.</summary>
        public static List<RegistroDia> Momentos(Missao m)
            => m.Hist.Where(p => p.Impacto > 0).OrderByDescending(p => p.Impacto).Take(3).OrderBy(p => p.Dia).ToList();
    }
}
