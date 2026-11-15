using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.7: nota, momentos e dica do relatório (web: rating, vEnd).</summary>
    public class RelatorioTestes
    {
        static Missao Fim(Modo modo, Fim fim, int estrelas, int rad)
        {
            var m = Apoio.Controlada(modo, Tarefas.Estufa, 10);
            m.Fim = fim; m.Estrelas = estrelas; m.Rad = rad;
            return m;
        }

        [TestCase(Modo.Cadete, 29, 3, 3)]
        [TestCase(Modo.Cadete, 29, 4, 2)]
        [TestCase(Modo.Cadete, 16, 0, 2)]
        [TestCase(Modo.Cadete, 15, 0, 1)]
        [TestCase(Modo.Comandante, 21, 3, 3)]
        [TestCase(Modo.Comandante, 20, 0, 2)]
        [TestCase(Modo.Comandante, 14, 0, 2)]
        [TestCase(Modo.Comandante, 13, 0, 1)]
        public void NotaDaMissaoCompleta(Modo modo, int estrelas, int rad, int nota)
        {
            Assert.AreEqual(nota, RegrasMissao.Nota(Fim(modo, DezDias.Fim.Completa, estrelas, rad)));
        }

        [Test]
        public void MissaoInterrompida_SemNota()
        {
            Assert.AreEqual(0, RegrasMissao.Nota(Fim(Modo.Cadete, DezDias.Fim.Oxigenio, 40, 0)));
        }

        [Test]
        public void DicaPorMotivo()
        {
            StringAssert.StartsWith(Textos.DicaFimO2.Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Oxigenio, 0, 0)).Pt);
            StringAssert.StartsWith(Textos.DicaFimRad.Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Radiacao, 0, 10)).Pt);
            StringAssert.StartsWith(Textos.DicaFimComida.Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Comida, 0, 0)).Pt);
            StringAssert.StartsWith(Textos.DicaFimRadAlta.Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Completa, 20, 5)).Pt);
            StringAssert.StartsWith(Textos.DicaFim3Estrelas.F(29).Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Completa, 20, 1)).Pt);
            StringAssert.StartsWith(Textos.DicaFimExcelente.Pt, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Completa, 30, 1)).Pt);
            StringAssert.EndsWith(Textos.EventosMudam.En, RegrasMissao.DicaFim(Fim(Modo.Cadete, DezDias.Fim.Oxigenio, 0, 0)).En);
        }

        [Test]
        public void TituloENenhumGameOver()
        {
            var m = Fim(Modo.Cadete, DezDias.Fim.Oxigenio, 0, 0);
            m.Dia = 6;
            Assert.AreEqual("Missão interrompida no dia 6", RegrasMissao.TituloFim(m).Pt);
            foreach (var f in new[] { DezDias.Fim.Oxigenio, DezDias.Fim.Radiacao, DezDias.Fim.Comida, DezDias.Fim.Completa })
            {
                m.Fim = f;
                var t = RegrasMissao.TituloFim(m) + RegrasMissao.SubtituloFim(m);
                StringAssert.DoesNotContain("GAME OVER", t.Pt.ToUpperInvariant());
                StringAssert.DoesNotContain("GAME OVER", t.En.ToUpperInvariant());
            }
            StringAssert.Contains("em segurança", RegrasMissao.SubtituloFim(Fim(Modo.Cadete, DezDias.Fim.Comida, 0, 0)).Pt);
        }

        [Test]
        public void Momentos_Ate3EmOrdemDeDia()
        {
            var m = Apoio.Controlada();
            m.Hist.Clear();
            int[] impactos = { 0, 2, 5, 0, 3, 7, 2, 0, 1, 4 };
            for (int i = 0; i < 10; i++) m.Hist.Add(new RegistroDia { Dia = i + 1, Impacto = impactos[i], Momento = new Txt("d" + (i + 1), "d") });
            var mom = RegrasMissao.Momentos(m);
            Assert.AreEqual(3, mom.Count);
            Assert.AreEqual(3, mom[0].Dia);   // impacto 5
            Assert.AreEqual(6, mom[1].Dia);   // impacto 7
            Assert.AreEqual(10, mom[2].Dia);  // impacto 4
        }
    }
}
