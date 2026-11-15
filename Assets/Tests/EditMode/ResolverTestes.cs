using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.5: resolução do dia (web: resolve), um teste por cenário das specs.</summary>
    public class ResolverTestes
    {
        [Test]
        public void ArEAguaSemEnergia_OxigenioCai2()
        {
            var m = Apoio.Controlada();
            m.O2 = 8;
            Apoio.Alocar(m, ar: 0);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(6, m.O2);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.ArSem));
        }

        [TestCase(3, 1)]
        [TestCase(2, 0)]
        [TestCase(1, -1)]
        [TestCase(0, -2)]
        public void ArEAgua_Escala(int celulas, int delta)
        {
            var m = Apoio.Controlada();
            m.O2 = 5;
            Apoio.Alocar(m, ar: celulas);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(5 + delta, m.O2);
        }

        [Test]
        public void OxigenioFicaEntre0e10()
        {
            var m = Apoio.Controlada();
            m.O2 = 10;
            Apoio.Alocar(m, ar: 3);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(10, m.O2);
        }

        [Test]
        public void VazamentoConsertado()
        {
            var m = Apoio.Controlada();
            m.Vazamento = true; m.O2 = 6;
            Apoio.Alocar(m, ar: 2, conserto: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.IsFalse(m.Vazamento);
            Assert.AreEqual(6, m.O2);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.VazamentoConsertado));
            Assert.IsFalse(RegrasMissao.ModulosVisiveis(m).Contains(Modulo.Conserto));
        }

        [Test]
        public void VazamentoAberto_PerdeUmAMais()
        {
            var m = Apoio.Controlada();
            m.Vazamento = true; m.O2 = 6;
            Apoio.Alocar(m, ar: 2, conserto: 1);
            var r = RegrasMissao.Resolver(m);
            Assert.IsTrue(m.Vazamento);
            Assert.AreEqual(5, m.O2);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.VazamentoContinua));
        }

        [Test]
        public void DiaDeColheita()
        {
            var m = Apoio.Controlada();
            m.Crescimento = 3; m.O2 = 5; m.Comida = 3;
            Apoio.Alocar(m, ar: 2, estufa: 1);
            var r = RegrasMissao.Resolver(m);
            Assert.IsTrue(r.Colheita);
            Assert.AreEqual(0, m.Crescimento);
            Assert.AreEqual(6, m.O2);
            Assert.AreEqual(3 + 4 - 1, m.Comida); // colheita +4, a tripulação come 1
        }

        [Test]
        public void LaboratorioCom3_TresEstrelas()
        {
            var m = Apoio.Controlada();
            m.Estrelas = 2;
            Apoio.Alocar(m, ar: 2, lab: 3);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(5, m.Estrelas);
        }

        [Test]
        public void CelulasSemUso_SePerdem()
        {
            var m = Apoio.Controlada();
            m.Bateria = 1; // 6 + 1 = 7 células
            Apoio.Alocar(m, ar: 2, lab: 2, bat: 1); // 5 usadas, 2 livres
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(2, r.Perdidas);
            Assert.AreEqual(1, m.Bateria);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.CelulasPerdidas.F(2)));
        }

        [Test]
        public void BateriaPassaParaOutroDia_Maximo4()
        {
            var m = Apoio.Controlada();
            m.Bateria = 0;
            Apoio.Alocar(m, ar: 2, bat: 4);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(4, m.Bateria);
        }

        [Test]
        public void ConsumoDeComida()
        {
            var m = Apoio.Controlada();
            m.Comida = 3;
            Apoio.Alocar(m, ar: 2);
            RegrasMissao.Resolver(m);
            Assert.AreEqual(2, m.Comida);
            Assert.AreEqual(Fim.Nenhum, m.Fim);
        }

        [Test]
        public void ComidaAcabou_EncerraPorComida()
        {
            var m = Apoio.Controlada();
            m.Comida = 0;
            Apoio.Alocar(m, ar: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(Fim.Comida, r.Fim);
        }

        [Test]
        public void AbrigoComMeiaEnergia_ContraX_Radiacao3()
        {
            var m = Apoio.Controlada();
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Apoio.Alocar(m, ar: 2, abrigo: 1);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(3, m.Rad);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.ErupcaoMeia.F(3)));
            Assert.AreSame(r.Erupcao, m.AlertaDe(m.Dia).Erupcao);
        }

        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 3)]
        [TestCase(2, 0, 5)]
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(0, 2, 0)]
        [TestCase(2, 2, 0)]
        public void DosePelaClasseEPeloAbrigo(int intensidade, int abrigo, int dose)
        {
            var m = Apoio.Controlada();
            var e = intensidade == 2 ? Apoio.X() : intensidade == 1 ? Apoio.M() : Apoio.C();
            m.Alertas[m.Dia] = new Alerta { Erupcao = e, Forte = false };
            Apoio.Alocar(m, ar: 2, abrigo: abrigo);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(dose, r.DRad);
            Assert.AreEqual(dose, m.Rad);
        }

        [Test]
        public void AbrigoLigado_Protege()
        {
            var m = Apoio.Controlada();
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Apoio.Alocar(m, ar: 2, abrigo: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(0, m.Rad);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.ErupcaoProtegeu));
        }

        [Test]
        public void AlarmeFalso_AbrigoLigadoAToa()
        {
            var m = Apoio.Controlada();
            m.Alertas[m.Dia] = new Alerta { Erupcao = null, Forte = true };
            Apoio.Alocar(m, ar: 2, abrigo: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(0, m.Rad);
            Assert.IsTrue(r.FalsoAlarme);
            Assert.IsNull(r.Erupcao);
            Assert.IsTrue(Apoio.TemLinha(r, Textos.FalsoComAbrigo.F(2)));
        }

        [Test]
        public void RadiacaoMaximo10()
        {
            var m = Apoio.Controlada();
            m.Rad = 8;
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Apoio.Alocar(m, ar: 2);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(10, m.Rad);
            Assert.AreEqual(Fim.Radiacao, r.Fim);
        }

        [Test]
        public void OrdemDoFim_OxigenioAntesDeTudo()
        {
            var m = Apoio.Controlada();
            m.O2 = 1; m.Rad = 9; m.Comida = 0;
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Apoio.Alocar(m, ar: 0);
            var r = RegrasMissao.Resolver(m);
            Assert.AreEqual(Fim.Oxigenio, r.Fim);
        }

        [Test]
        public void OrdemDoFim_RadiacaoAntesDaComida()
        {
            var m = Apoio.Controlada();
            m.O2 = 5; m.Rad = 9; m.Comida = 0;
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Apoio.Alocar(m, ar: 2);
            Assert.AreEqual(Fim.Radiacao, RegrasMissao.Resolver(m).Fim);
        }

        [Test]
        public void MissaoCompleta_NoDia10()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Estufa, 10);
            m.O2 = 5; m.Comida = 3;
            Apoio.Alocar(m, ar: 2);
            Assert.AreEqual(Fim.Completa, RegrasMissao.Resolver(m).Fim);
        }

        [Test]
        public void Historico_RegistraODia()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Estufa, 4);
            m.O2 = 6;
            Apoio.Alocar(m, ar: 0);
            RegrasMissao.Resolver(m);
            var h = m.Hist[m.Hist.Count - 1];
            Assert.AreEqual(4, h.Dia);
            Assert.AreEqual(4, h.O2);
            Assert.AreEqual(3, h.Impacto); // caiu 2 → impacto 3
            Assert.AreEqual(Textos.MomentoO2Caiu.F(2).Pt, h.Momento.Pt);
        }
    }
}
