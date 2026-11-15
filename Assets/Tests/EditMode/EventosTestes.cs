using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Random que sempre devolve o mesmo valor: deixa o sorteio do risco previsível.</summary>
    sealed class RandomFixo : Random
    {
        readonly double _v;
        public RandomFixo(double v) { _v = v; }
        protected override double Sample() => _v;
        public override double NextDouble() => _v;
        public override int Next(int max) => (int)(_v * max);
    }

    /// <summary>Spec eventos-do-dia e deltas de clima-espacial-donki (mudança eventos-diarios-com-decisao).</summary>
    public class EventosTestes
    {
        static List<Cme> Cmes() => new List<Cme>
        {
            new Cme { Id = "cme-ligada", Data = "2024-05-10", Velocidade = 1263, Url = "https://ccmc.gsfc.nasa.gov/DONKI/view/CME/1/-1", FlrLigado = Apoio.Vinte()[0].Id },
            new Cme { Id = "cme-solta", Data = "2025-05-31", Velocidade = 2371, Url = "https://ccmc.gsfc.nasa.gov/DONKI/view/CME/2/-1", FlrLigado = "" },
        };

        static Missao ComEvento(string evento, Modo modo = Modo.Cadete, string tarefa = Tarefas.Estufa, int dia = 3)
        {
            var m = Apoio.Controlada(modo, tarefa, dia);
            Apoio.ComEvento(m, evento);
            return m;
        }

        // ================= 2.1 Estado =================

        [Test]
        public void CustoEvento_NaoEApagadoPeloCustoDaTarefa()
        {
            var m = ComEvento(Eventos.Gelo, tarefa: Tarefas.Agua);
            int antes = RegrasMissao.CelulasTotais(m);
            Eventos.Escolher(m, "rover", false);
            Tarefas.Fazer(m, new ResultadoMinijogo(1f, 1f, true)); // a reciclagem grava Custo = 1
            Assert.AreEqual(1, m.Hoje.Custo);
            Assert.AreEqual(2, m.Hoje.CustoEvento);
            Assert.AreEqual(antes - 3, RegrasMissao.CelulasTotais(m));
        }

        [Test]
        public void AjusteLuz_MudaSoODiaCerto()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Estufa, 3);
            m.Ajustar(4, -1, new Txt("motivo", "reason"));
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(4, m.Dia);
            Assert.AreEqual(m.M.Luz - 1, m.Hoje.Luz);
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(m.M.Luz, m.Hoje.Luz);
        }

        // ================= 2.2 Tabela =================

        [Test]
        public void Tabela_OnzeEventosCompletosNosDoisIdiomas()
        {
            var ids = new[] { Eventos.Particulas, Eventos.CmeId, Eventos.Tremor, Eventos.Micrometeorito, Eventos.Poeira, Eventos.Frio,
                              Eventos.Gelo, Eventos.Terra, Eventos.Sono, Eventos.Saudade, Eventos.Fungo };
            Assert.AreEqual(11, Eventos.Defs.Count());
            foreach (var modo in new[] { Modo.Cadete, Modo.Comandante })
            {
                var m = Apoio.Controlada(modo);
                m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
                m.CmeRegistro = Cmes()[0]; m.CmeDia = m.Dia; m.CmeChegada = m.Dia + 2;
                foreach (var id in ids)
                {
                    var d = Eventos.Def(id);
                    Assert.IsNotNull(d, id);
                    Assert.IsTrue(d.Conferido, id);
                    Assert.That(d.Escolhas.Count, Is.InRange(2, 3), id);
                    foreach (var l in new[] { Lingua.Pt, Lingua.En })
                    {
                        Assert.IsNotEmpty(d.Titulo.Em(l), id);
                        Assert.IsNotEmpty(d.Texto(m).Em(l), id);
                        Assert.IsNotEmpty(d.Fonte.Em(l), id);
                        StringAssert.Contains("NASA", d.Fonte.Em(l), id);
                        foreach (var e in d.Escolhas)
                        {
                            Assert.IsNotEmpty(e.Titulo.Em(l), $"{id}.{e.Id}");
                            Assert.IsNotEmpty(e.Desc.Em(l), $"{id}.{e.Id}");
                            foreach (var zerado in new[] { false, true })
                            {
                                var custo = Eventos.CustoVisivel(m, e, zerado).Em(l);
                                Assert.IsNotEmpty(custo, $"{id}.{e.Id}");
                                foreach (var p in Apresentacao.PartesDoCusto(custo))
                                    Assert.AreNotEqual(Recurso.Nenhum, p.Recurso, $"{id}.{e.Id} {l}: \"{p.Texto}\" em \"{custo}\"");
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void NenhumTextoDeDonkiDizAoVivo()
        {
            var m = Apoio.Controlada();
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = false };
            m.CmeRegistro = Cmes()[1]; m.CmeDia = m.Dia; m.CmeChegada = m.Dia + 2;
            foreach (var id in new[] { Eventos.Particulas, Eventos.CmeId })
            {
                var d = Eventos.Def(id);
                foreach (var t in new[] { d.Titulo, d.Texto(m) })
                {
                    StringAssert.DoesNotContain("ao vivo", t.Pt.ToLowerInvariant());
                    StringAssert.DoesNotContain("hoje no sol", t.Pt.ToLowerInvariant());
                    StringAssert.DoesNotContain("agora no sol", t.Pt.ToLowerInvariant());
                    StringAssert.DoesNotContain(" live", t.En.ToLowerInvariant());
                }
                StringAssert.Contains("não é ao vivo", d.Fonte.Pt);
                StringAssert.Contains("not live", d.Fonte.En);
            }
        }

        // ================= 3.1 Textos =================

        static IEnumerable<(string nome, Txt t)> TextosDosEventos(Missao m)
        {
            foreach (var f in typeof(Eventos).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                if (f.GetValue(null) is Txt t) yield return ("Eventos." + f.Name, t);
            foreach (var d in Eventos.Defs)
            {
                yield return (d.Id + ".Titulo", d.Titulo);
                yield return (d.Id + ".Texto", d.Texto(m));
                yield return (d.Id + ".Fonte", d.Fonte);
                if (!d.Dica.Vazio) yield return (d.Id + ".Dica", d.Dica);
                foreach (var e in d.Escolhas)
                {
                    yield return ($"{d.Id}.{e.Id}.Titulo", e.Titulo);
                    yield return ($"{d.Id}.{e.Id}.Desc", e.Desc);
                    yield return ($"{d.Id}.{e.Id}.Custo", e.Custo(m));
                    if (!e.MotivoIndisponivel.Vazio) yield return ($"{d.Id}.{e.Id}.Motivo", e.MotivoIndisponivel);
                }
            }
            foreach (var a in new[] { Alarme.Abrigo, Alarme.Atrasado, Alarme.Continuou })
            {
                m.Hoje.Alarme = a;
                yield return ("LinhaAlarme." + a + ".vieram", Eventos.LinhaAlarme(m, true, 2));
                yield return ("LinhaAlarme." + a + ".falso", Eventos.LinhaAlarme(m, false, 0));
                yield return ("MomentoAlarme." + a, Eventos.MomentoAlarme(m, 2));
            }
        }

        static Missao MissaoComTudo()
        {
            var m = Apoio.Controlada();
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            m.CmeRegistro = Cmes()[0]; m.CmeLigada = true; m.CmeDia = m.Dia; m.CmeChegada = m.Dia + 2;
            return m;
        }

        [Test]
        public void TextosDosEventos_PtEEnSoComLatin1()
        {
            int n = 0;
            foreach (var (nome, t) in TextosDosEventos(MissaoComTudo()))
            {
                n++;
                Assert.IsFalse(string.IsNullOrWhiteSpace(t.Pt), nome + " sem PT");
                Assert.IsFalse(string.IsNullOrWhiteSpace(t.En), nome + " sem EN");
                foreach (var s in new[] { t.Pt, t.En })
                    foreach (var c in s)
                        Assert.That(c <= 'ÿ', nome + " tem o caractere U+" + ((int)c).ToString("X4"));
            }
            Assert.Greater(n, 80);
        }

        [Test]
        public void TextosDeSepECme_NuncaComoAoVivo()
        {
            var campos = new[] { Textos.AbrigoCusta, Textos.CmeACaminho, Textos.CmeChegaNoDia, Textos.CmeChegou, Textos.CmePreparado,
                                 Textos.CmeNaoPreparado, Textos.SepMedido, Textos.CmeCartao, Textos.CmeCartaoTexto, Textos.FonteDonkiData,
                                 Textos.ChegouAbrigo, Textos.AtrasouAbrigo, Textos.RiscoContinuou };
            foreach (var t in campos)
            {
                var pt = t.Pt.ToLowerInvariant().Replace("não é dado ao vivo", "");
                var en = t.En.ToLowerInvariant().Replace("not live data", "");
                StringAssert.DoesNotContain("ao vivo", pt, t.Pt);
                StringAssert.DoesNotContain("agora no sol", pt, t.Pt);
                StringAssert.DoesNotContain("hoje no sol", pt, t.Pt);
                StringAssert.DoesNotContain("live", en, t.En);
            }
            StringAssert.Contains("15 de novembro de 2026", Textos.FonteDonkiData.F(Idioma.Data("2026-11-15")).Pt);
        }

        [Test]
        public void Creditos_TemDonkiComSepECmeEAsFontesDosEventos()
        {
            var urls = Textos.FontesNasa.Select(f => f.url).ToList();
            foreach (var u in new[]
            {
                "https://www.nasa.gov/solar-system/moon/shrinking-moon-causing-moonquakes-and-faults-near-lunar-south-pole/",
                "https://www.nasa.gov/meteoroid-environment-office/lunar-impact-monitoring/",
                "https://science.nasa.gov/moon/moon-dust/",
                "https://science.nasa.gov/solar-system/moon/10-cool-things-nasas-lunar-reconnaissance-orbiter-is-teaching-us-about-the-moon/",
                "https://science.nasa.gov/mission/lcross/",
                "https://www.nasa.gov/hrp/hazard-isolation-and-confinement/",
                "https://www.nasa.gov/humans-in-space/zinnias-from-space-nasa-studies-the-multiple-benefits-of-gardening/",
                "https://osdr.nasa.gov/bio/repo/data/studies/OSD-268",
                "https://svs.gsfc.nasa.gov/4944",
            }) CollectionAssert.Contains(urls, u);
            var donki = Textos.FontesNasa.First(f => f.nome.Contains("DONKI")).uso;
            StringAssert.Contains("SEP", donki.Pt);
            StringAssert.Contains("CME", donki.Pt);
        }

        // ================= 2.3 Sorteio =================

        static List<Missao> Partidas(Modo modo, int n, bool tutorial = false)
        {
            var l = new List<Missao>();
            for (int s = 0; s < n; s++)
            {
                var m = RegrasMissao.NovaPartida(modo, Apoio.Vinte(), new Random(s), tutorial, Cmes());
                var dias = new List<DiaMissao>();
                for (int d = 1; d <= Missao.Dias; d++) { RegrasMissao.IniciarDia(m); dias.Add(m.Hoje); }
                l.Add(m);
                _dias[m] = dias;
            }
            return l;
        }

        static readonly Dictionary<Missao, List<DiaMissao>> _dias = new Dictionary<Missao, List<DiaMissao>>();

        [TestCase(Modo.Cadete)]
        [TestCase(Modo.Comandante)]
        public void Sorteio_SemRepeticaoComCondicaoESemDiaVazio(Modo modo)
        {
            foreach (var m in Partidas(modo, 200))
            {
                var dias = _dias[m];
                var comuns = dias.Select(d => d.Evento).Where(e => e != Eventos.Particulas).ToList();
                CollectionAssert.AllItemsAreUnique(comuns, "evento repetido");
                for (int i = 0; i < dias.Count; i++)
                {
                    int dia = i + 1;
                    var ev = dias[i].Evento;
                    Assert.IsNotNull(ev, $"dia {dia} sem evento");
                    Assert.AreEqual(m.AlertaDe(dia) != null, ev == Eventos.Particulas, $"dia {dia}: partículas só em dia de alerta");
                    if (ev == Eventos.Frio) Assert.IsTrue(m.EhSombra(dia), "frio fora da sombra");
                    if (ev == Eventos.CmeId) Assert.AreEqual(m.CmeDia, dia);
                    if (ev == Eventos.Sono || ev == Eventos.Particulas) Assert.IsTrue(dias[i].EventoNaIda, ev);
                    if (ev == Eventos.Poeira) Assert.IsFalse(dias[i].EventoNaIda, ev);
                    Assert.That(dias[i].GatilhoSeg, Is.InRange(4f, 12f));
                }
            }
        }

        [Test]
        public void Sorteio_PartidasSeguidasVariam()
        {
            var seqs = Partidas(Modo.Cadete, 20).Select(m => string.Join(",", _dias[m].Select(d => d.Evento))).Distinct().Count();
            Assert.Greater(seqs, 10);
        }

        [Test]
        public void Sorteio_TremorNuncaComVazamentoAberto()
        {
            for (int s = 0; s < 100; s++)
            {
                var m = RegrasMissao.NovaPartida(Modo.Comandante, Apoio.Vinte(), new Random(s), false);
                m.Vazamento = true;
                for (int d = 1; d <= Missao.Dias; d++)
                {
                    RegrasMissao.IniciarDia(m);
                    Assert.AreNotEqual(Eventos.Tremor, m.Hoje.Evento);
                }
            }
        }

        [Test]
        public void Sorteio_FungoSoComEstufaEmDoisOuMais()
        {
            var m = Apoio.Controlada();
            m.EventosUsados.Clear();
            m.EventosUsados.AddRange(Eventos.Sorteaveis.Where(d => d.Id != Eventos.Fungo).Select(d => d.Id));
            m.Crescimento = 1;
            RegrasMissao.IniciarDia(m);
            Assert.AreNotEqual(Eventos.Fungo, m.Hoje.Evento);
            m.EventosUsados.Clear();
            m.EventosUsados.AddRange(Eventos.Sorteaveis.Where(d => d.Id != Eventos.Fungo).Select(d => d.Id));
            m.Crescimento = 2;
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(Eventos.Fungo, m.Hoje.Evento);
        }

        [Test]
        public void Sorteio_SemNadaNovo_UsaUmJaUsadoValido()
        {
            var m = Apoio.Controlada();
            m.EventosUsados.AddRange(Eventos.Sorteaveis.Select(d => d.Id));
            RegrasMissao.IniciarDia(m);
            var d = Eventos.Def(m.Hoje.Evento);
            Assert.IsNotNull(d);
            Assert.IsTrue(d.Condicao(m));
        }

        [Test]
        public void Tutorial_Dia1ComPoeira()
        {
            var m = RegrasMissao.NovaPartida(Modo.Cadete, Apoio.Vinte(), new Random(3), true, Cmes());
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(Eventos.Poeira, m.Hoje.Evento);
            Assert.IsFalse(Eventos.Def(Eventos.Poeira).Dica.Vazio);
        }

        // ================= 2.4 CME =================

        [TestCase(Modo.Cadete)]
        [TestCase(Modo.Comandante)]
        public void Cme_ChegadaEAlertaRealForteSemMudarContagem(Modo modo)
        {
            int agendadas = 0;
            for (int s = 0; s < 200; s++)
            {
                var m = RegrasMissao.NovaPartida(modo, Apoio.Vinte(), new Random(s), false, Cmes());
                Assert.AreEqual(m.M.Alertas, m.Alertas.Count);
                Assert.AreEqual(m.M.Reais, m.Alertas.Values.Count(a => a.Erupcao != null));
                if (m.CmeDia == 0) continue;
                agendadas++;
                Assert.AreEqual(m.CmeDia + 2, m.CmeChegada);
                Assert.GreaterOrEqual(m.CmeDia, 2);
                Assert.IsNull(m.AlertaDe(m.CmeDia), "o dia do aviso não tem alerta");
                var al = m.AlertaDe(m.CmeChegada);
                Assert.IsNotNull(al?.Erupcao, "a chegada é alerta com erupção real");
                Assert.IsTrue(al.Forte, "a chegada é alerta forte");
                Assert.AreEqual(m.CmeLigada, m.CmeRegistro.FlrLigado == al.Erupcao.Id);
            }
            Assert.Greater(agendadas, 50);
        }

        [Test]
        public void Cme_SemListaNaoAgenda()
        {
            for (int s = 0; s < 50; s++)
                Assert.AreEqual(0, RegrasMissao.NovaPartida(Modo.Comandante, Apoio.Vinte(), new Random(s), false).CmeDia);
        }

        [Test]
        public void Cme_TextoSemLigacaoNaoAfirmaQueEAMesma()
        {
            var m = ComEvento(Eventos.CmeId);
            m.CmeRegistro = Cmes()[1]; m.CmeLigada = false; m.CmeChegada = m.Dia + 2;
            var t = Eventos.Def(Eventos.CmeId).Texto(m);
            StringAssert.Contains("como uma que saiu", t.Pt);
            StringAssert.Contains("2371", t.Pt);
            StringAssert.Contains("Dia " + (m.Dia + 2), t.Pt);
        }

        // ================= 2.5 Escolhas =================

        [Test]
        public void Particulas_Abrigo_IniciaCorrida_EFimDaCorrida()
        {
            var m = ComEvento(Eventos.Particulas);
            var c = Eventos.Escolher(m, "abrigo", false);
            Assert.IsTrue(c.IniciarCorrida);
            Assert.AreEqual(0f, c.SegundosPerdidos);
            Assert.AreEqual(Alarme.Nenhum, m.Hoje.Alarme);
            var fim = Eventos.FimCorrida(m, true);
            Assert.AreEqual(Alarme.Abrigo, m.Hoje.Alarme);
            Assert.AreEqual(Eventos.SegundosAbrigo, fim.SegundosPerdidos);
            Assert.AreEqual(0f, Eventos.FimCorrida(m, false).SegundosPerdidos);
            Assert.AreEqual(Alarme.Atrasado, m.Hoje.Alarme);
        }

        [Test]
        public void Particulas_AbrigoComRelogioZerado_CustaUmaCelulaSemCorrida()
        {
            var m = ComEvento(Eventos.Particulas);
            var c = Eventos.Escolher(m, "abrigo", true);
            Assert.IsFalse(c.IniciarCorrida);
            Assert.AreEqual(Alarme.Abrigo, m.Hoje.Alarme);
            Assert.AreEqual(1, m.Hoje.CustoEvento);
        }

        [Test]
        public void Particulas_Continuar()
        {
            var m = ComEvento(Eventos.Particulas);
            Eventos.Escolher(m, "continuar", false);
            Assert.AreEqual(Alarme.Continuou, m.Hoje.Alarme);
        }

        [Test]
        public void Particulas_CorridaComFatorDoModo()
        {
            Assert.AreEqual(25f * 1.3f, Eventos.ContagemReal(Apoio.Controlada(Modo.Cadete)), 0.01f);
            Assert.AreEqual(25f, Eventos.ContagemReal(Apoio.Controlada(Modo.Comandante)), 0.01f);
        }

        [Test]
        public void Cme_PrepararEDeixar()
        {
            var m = ComEvento(Eventos.CmeId);
            Eventos.Escolher(m, "preparar", false);
            Assert.AreEqual(2, m.Hoje.CustoEvento);
            Assert.IsTrue(m.AbrigoPreparado);
            var n = ComEvento(Eventos.CmeId);
            Eventos.Escolher(n, "deixar", false);
            Assert.AreEqual(0, n.Hoje.CustoEvento);
            Assert.IsFalse(n.AbrigoPreparado);
        }

        [TestCase(Eventos.Tremor, "checar", 40f)]
        [TestCase(Eventos.Poeira, "aspirar", 40f)]
        [TestCase(Eventos.Sono, "folga", 60f)]
        public void CustoEmTempo_EComRelogioZeradoViraCelula(string ev, string escolha, float seg)
        {
            var m = ComEvento(ev);
            Assert.AreEqual(seg, Eventos.Escolher(m, escolha, false).SegundosPerdidos);
            Assert.AreEqual(0, m.Hoje.CustoEvento);
            var z = ComEvento(ev);
            var e = Eventos.Def(ev).Escolha(escolha);
            Assert.AreEqual(Eventos.CustoTrocado.Pt, Eventos.CustoVisivel(z, e, true).Pt);
            Assert.AreEqual(0f, Eventos.Escolher(z, escolha, true).SegundosPerdidos);
            Assert.AreEqual(1, z.Hoje.CustoEvento);
        }

        [TestCase(Modo.Cadete, 0.39, true)]
        [TestCase(Modo.Cadete, 0.41, false)]
        [TestCase(Modo.Comandante, 0.49, true)]
        [TestCase(Modo.Comandante, 0.51, false)]
        public void Tremor_Seguir_SorteiaORiscoPorModo(Modo modo, double sorteio, bool vaza)
        {
            var m = ComEvento(Eventos.Tremor, modo);
            m.Rng = new RandomFixo(sorteio);
            Eventos.Escolher(m, "seguir", false);
            Assert.AreEqual(vaza, m.Hoje.Risco);
        }

        [Test]
        public void Micrometeorito_TrocarIndisponivelComOxigenio1()
        {
            var m = ComEvento(Eventos.Micrometeorito);
            m.O2 = 1;
            var e = Eventos.Def(Eventos.Micrometeorito).Escolha("trocar");
            Assert.IsFalse(Eventos.Disponivel(m, e));
            Eventos.Escolher(m, "trocar", false);
            Assert.IsFalse(m.Hoje.EventoResolvido);
        }

        [Test]
        public void Micrometeorito_DeixarRiscado_TiraUmaCelulaNosDoisDiasSeguintes()
        {
            var m = ComEvento(Eventos.Micrometeorito);
            Eventos.Escolher(m, "deixar", false);
            Assert.AreEqual(-1, m.AjusteDe(4));
            Assert.AreEqual(-1, m.AjusteDe(5));
            Assert.AreEqual(0, m.AjusteDe(6));
            Assert.IsFalse(m.MotivoAjuste[4].Vazio);
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(m.M.Luz - 1, m.Hoje.Luz);
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(m.M.Luz - 1, m.Hoje.Luz);
        }

        [TestCase(Eventos.Frio, "aquecer", 1)]
        [TestCase(Eventos.Gelo, "rover", 2)]
        [TestCase(Eventos.Terra, "satelite", 1)]
        [TestCase(Eventos.Saudade, "chamada", 1)]
        [TestCase(Eventos.Gelo, "anotar", 0)]
        public void CustoEmCelulasDeHoje(string ev, string escolha, int celulas)
        {
            var m = ComEvento(ev);
            int antes = RegrasMissao.CelulasTotais(m);
            Eventos.Escolher(m, escolha, false);
            Assert.AreEqual(antes - celulas, RegrasMissao.CelulasTotais(m));
        }

        [Test]
        public void Terra_SemContatoSoHoje()
        {
            var m = ComEvento(Eventos.Terra);
            Eventos.Escolher(m, "sem", false);
            var p = RegrasMissao.Previa(m);
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(Textos.PreviaSemContato.Pt, p[0].Pt);
            RegrasMissao.IniciarDia(m);
            Assert.IsFalse(m.Hoje.SemContatoHoje);
            Assert.AreNotEqual(Textos.PreviaSemContato.Pt, RegrasMissao.Previa(m)[0].Pt);
        }

        [Test]
        public void Sono_TrabalharDiminuiOMinijogo()
        {
            var m = ComEvento(Eventos.Sono, Modo.Comandante, Tarefas.Agua);
            float normal = Tarefas.LimiteReal(m, Tarefas.Agua);
            Eventos.Escolher(m, "trabalhar", false);
            Assert.AreEqual(0.75f, m.Hoje.FatorTempoHoje, 0.001f);
            Assert.AreEqual(normal * 0.75f, Tarefas.LimiteReal(m, Tarefas.Agua), 0.001f);
        }

        [Test]
        public void Saudade_SeguirTiraUmaCelulaAmanha()
        {
            var m = ComEvento(Eventos.Saudade);
            Eventos.Escolher(m, "seguir", false);
            Assert.AreEqual(-1, m.AjusteDe(4));
            Assert.AreEqual(0, m.AjusteDe(5));
        }

        [TestCase(0.49, true)]
        [TestCase(0.5, false)]
        public void Fungo_Deixar_SorteiaMetade(double sorteio, bool espalha)
        {
            var m = ComEvento(Eventos.Fungo);
            m.Rng = new RandomFixo(sorteio);
            Eventos.Escolher(m, "deixar", false);
            Assert.AreEqual(espalha, m.Hoje.Risco);
        }

        [Test]
        public void Escolher_SoUmaVez()
        {
            var m = ComEvento(Eventos.Gelo);
            Eventos.Escolher(m, "rover", false);
            Eventos.Escolher(m, "rover", false);
            Assert.AreEqual(2, m.Hoje.CustoEvento);
            Assert.AreEqual("rover", m.Hoje.Escolha);
        }

        // ================= 2.6 Resultado =================

        static Missao DiaDeAlerta(Erupcao e, Alarme alarme, bool fora = false, string tarefa = Tarefas.Estufa, int dia = 3)
        {
            var m = Apoio.Controlada(Modo.Cadete, tarefa, dia);
            m.Alertas[m.Dia] = new Alerta { Erupcao = e, Forte = true };
            Apoio.ComEvento(m, Eventos.Particulas);
            m.Hoje.EventoResolvido = true;
            m.Hoje.Alarme = alarme;
            m.Hoje.Fora = fora;
            return m;
        }

        [Test]
        public void ContinuouTrabalhandoESaiuDaBase_SomaTres()
        {
            var m = DiaDeAlerta(Apoio.X(), Alarme.Continuou, fora: true, tarefa: Tarefas.Amostra);
            Apoio.Alocar(m, ar: 2, abrigo: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(3, m.Rad);
            Assert.IsTrue(Apoio.TemLinhaComeco(r, "Vocês continuaram trabalhando e as partículas vieram: radiação +3."));
        }

        [TestCase(0, Alarme.Continuou, false, 1)]
        [TestCase(1, Alarme.Continuou, false, 1)]
        [TestCase(2, Alarme.Continuou, false, 2)]
        [TestCase(1, Alarme.Continuou, true, 2)]
        [TestCase(2, Alarme.Atrasado, false, 1)]
        [TestCase(2, Alarme.Atrasado, true, 2)]
        [TestCase(2, Alarme.Abrigo, true, 0)]
        [TestCase(2, Alarme.Nenhum, true, 0)]
        public void ExposicaoAoAlarme(int intensidade, Alarme alarme, bool fora, int exposicao)
        {
            var e = intensidade == 2 ? Apoio.X() : intensidade == 1 ? Apoio.M() : Apoio.C();
            var m = DiaDeAlerta(e, alarme, fora);
            Apoio.Alocar(m, ar: 2, abrigo: 2);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(exposicao, m.Rad);
        }

        [Test]
        public void ExposicaoSomaComADoseDaNoite()
        {
            var m = DiaDeAlerta(Apoio.X(), Alarme.Continuou);
            Apoio.Alocar(m, ar: 2, abrigo: 1);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(3 + 2, m.Rad);
        }

        [Test]
        public void CorreuAToa()
        {
            var m = DiaDeAlerta(null, Alarme.Abrigo);
            Apoio.Alocar(m, ar: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(0, m.Rad);
            Assert.IsTrue(r.FalsoAlarme);
            Assert.IsTrue(Apoio.TemLinha(r, Eventos.LinhaAlarme(m, false, 0)));
            StringAssert.Contains("custou sem fazer falta", Eventos.LinhaAlarme(m, false, 0).Pt);
        }

        [Test]
        public void AbrigoPreparado_UmaCelulaBasta()
        {
            var m = DiaDeAlerta(Apoio.X(), Alarme.Abrigo, dia: 6);
            m.AbrigoPreparado = true; m.CmeDia = 4; m.CmeChegada = 6;
            Apoio.Alocar(m, ar: 2, abrigo: 1);
            Assert.AreEqual(Textos.PreviaAbrigo2.Pt, RegrasMissao.Previa(m).First(t => t.Pt.StartsWith("🛡") || t.Pt == Textos.PreviaAbrigo2.Pt || t.Pt == Textos.PreviaAbrigo1.Pt).Pt);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(0, m.Rad);
            Assert.IsTrue(Apoio.TemLinha(r, Eventos.PreparoValeu));
        }

        [Test]
        public void AbrigoPreparado_NaoValeEmOutroDia()
        {
            var m = DiaDeAlerta(Apoio.X(), Alarme.Abrigo, dia: 5);
            m.AbrigoPreparado = true; m.CmeDia = 4; m.CmeChegada = 6;
            Apoio.Alocar(m, ar: 2, abrigo: 1);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(3, m.Rad);
        }

        static ResultadoDia ResolverEvento(Missao m, string escolha, bool risco, Action<Missao> antes = null)
        {
            Eventos.Escolher(m, escolha, false);
            m.Hoje.Risco = risco;
            antes?.Invoke(m);
            Apoio.Alocar(m, ar: 2);
            return RegrasMissao.Resolver(m);
        }

        [Test]
        public void TremorAbreVazamento()
        {
            var m = ComEvento(Eventos.Tremor);
            var r = ResolverEvento(m, "seguir", true);
            Assert.IsTrue(m.Vazamento);
            Assert.IsTrue(Apoio.TemLinhaComeco(r, "O tremor soltou uma junta"));
            RegrasMissao.IniciarDia(m);
            Assert.Contains(Modulo.Conserto, RegrasMissao.ModulosVisiveis(m));
        }

        [Test]
        public void TremorSemSorte_NaoAbre()
        {
            var m = ComEvento(Eventos.Tremor);
            var r = ResolverEvento(m, "seguir", false);
            Assert.IsFalse(m.Vazamento);
            Assert.IsTrue(Apoio.TemLinhaComeco(r, "Vocês seguiram o trabalho, e o tremor não soltou"));
        }

        [Test]
        public void FungoSeEspalha()
        {
            var m = ComEvento(Eventos.Fungo);
            m.Crescimento = 3;
            var r = ResolverEvento(m, "deixar", true);
            Assert.AreEqual(0, m.Crescimento);
            Assert.IsTrue(Apoio.TemLinhaComeco(r, "O fungo se espalhou"));
        }

        [Test]
        public void FrioAgasalhar_EstufaPerdeUm()
        {
            var m = ComEvento(Eventos.Frio);
            m.Crescimento = 2;
            ResolverEvento(m, "agasalhar", false);
            Assert.AreEqual(1, m.Crescimento);
        }

        [Test]
        public void GeloRover_TresEstrelas()
        {
            var m = ComEvento(Eventos.Gelo);
            var r = ResolverEvento(m, "rover", false);
            Assert.AreEqual(r.EstrelasAntes + 3, m.Estrelas);
        }

        [TestCase(Eventos.Poeira, "entrar")]
        [TestCase(Eventos.Micrometeorito, "trocar")]
        public void OxigenioMenosUm(string ev, string escolha)
        {
            var m = ComEvento(ev);
            m.O2 = 6;
            var r = ResolverEvento(m, escolha, false); // ar 2 → O₂ igual
            Assert.AreEqual(5, m.O2);
        }

        [Test]
        public void MomentoNoRelatorio_DecisaoDoAlarme()
        {
            var m = DiaDeAlerta(Apoio.X(), Alarme.Continuou, fora: true, tarefa: Tarefas.Amostra, dia: 5);
            Apoio.Alocar(m, ar: 2, abrigo: 2);
            RegrasMissao.Resolver(m);
            var mom = RegrasMissao.Momentos(m);
            var d5 = mom.FirstOrDefault(h => h.Dia == 5);
            Assert.IsNotNull(d5);
            Assert.AreEqual(Eventos.MomentoAlarme(m, 3).Pt, d5.Momento.Pt);
            StringAssert.Contains("trabalhando, depois do alarme", d5.Momento.Pt);
        }

        [Test]
        public void MomentoNoRelatorio_NomeiaAEscolha()
        {
            var m = ComEvento(Eventos.Tremor);
            ResolverEvento(m, "seguir", true);
            var h = m.Hist.Last();
            StringAssert.Contains("Seguir o trabalho", h.Momento.Pt);
            Assert.AreEqual(3, h.Impacto);
        }

        // ================= 2.7 Balanceamento =================

        /// <summary>Joga uma partida inteira com uma política fixa: "corre" no alarme e "liga" 2 no Abrigo nos alertas fortes.</summary>
        static Missao Simular(Modo modo, int semente, bool corre, bool liga)
        {
            var rng = new Random(semente);
            var m = RegrasMissao.NovaPartida(modo, Apoio.Vinte(), rng, false, Cmes());
            while (m.Fim == Fim.Nenhum)
            {
                RegrasMissao.IniciarDia(m);
                var T = m.Hoje;
                // Evento: partículas pela política; os outros, a primeira escolha disponível.
                var ev = Eventos.Def(T.Evento);
                if (T.Evento == Eventos.Particulas)
                {
                    if (corre) { Eventos.Escolher(m, "abrigo", false); Eventos.FimCorrida(m, true); }
                    else Eventos.Escolher(m, "continuar", false);
                }
                else
                {
                    var e = ev.Escolhas.First(x => Eventos.Disponivel(m, x));
                    Eventos.Escolher(m, e.Id, false);
                }
                // Tarefa: faz com nota média se puder.
                if (Tarefas.PodeFazer(m))
                {
                    Tarefas.Fazer(m, new ResultadoMinijogo(0.7f, Tarefas.LimiteReal(m, T.Tarefa) * 0.5f, true));
                    if (!T.TarefaResolvida) Tarefas.VazamentoReforcar(m);
                }
                else Tarefas.Ficar(m);
                // Distribuição gulosa.
                RegrasMissao.PrepararDistribuicao(m);
                var a = T.Aloc;
                a.Zerar();
                void Por(Modulo mod, int n) { for (int i = 0; i < n; i++) RegrasMissao.Mais(m, mod); }
                Por(Modulo.Ar, m.O2 <= 5 ? 3 : 2);
                if (m.Vazamento) Por(Modulo.Conserto, 2);
                var al = m.AlertaDe(m.Dia);
                if (liga && al != null && al.Forte) Por(Modulo.Abrigo, 2);
                Por(Modulo.Estufa, m.Comida <= 3 ? 2 : 1);
                Por(Modulo.Lab, 3);
                Por(Modulo.Bateria, Missao.MaxBat);
                RegrasMissao.Resolver(m);
            }
            return m;
        }

        [TestCase(Modo.Cadete, 0.80)]
        [TestCase(Modo.Comandante, 0.60)]
        public void Balanceamento(Modo modo, double minimoCompleta)
        {
            const int n = 500;
            int completaA = 0, radA = 0, radB = 0, completaC = 0, radC = 0;
            for (int s = 0; s < n; s++)
            {
                var a = Simular(modo, s, true, true);
                if (a.Fim == Fim.Completa) completaA++;
                if (a.Fim == Fim.Radiacao) radA++;
                if (Simular(modo, s, false, false).Fim == Fim.Radiacao) radB++;
                var c = Simular(modo, s, false, true); // só o alarme muda em relação à A
                if (c.Fim == Fim.Completa) completaC++;
                if (c.Fim == Fim.Radiacao) radC++;
            }
            TestContext.WriteLine($"{modo}: corre e liga completa {completaA}/{n} (radiação {radA}); continua e liga completa {completaC}/{n} (radiação {radC}); nunca protege termina por radiação {radB}/{n}");
            Assert.GreaterOrEqual(completaA / (double)n, minimoCompleta);
            Assert.Greater(radB, radA);
        }
    }
}
