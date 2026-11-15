using System.Linq;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.4: células do dia, distribuição e prévia (web: startDay, totalCells, enterAlloc, previewLines).</summary>
    public class DistribuicaoTestes
    {
        [Test]
        public void DiaDeLuzComCustoDeTarefa_SeisCelulas()
        {
            var m = Apoio.Controlada(Modo.Cadete);
            m.Bateria = 1;
            m.Hoje.Custo = 1;
            Assert.AreEqual(6 - 1 + 1, RegrasMissao.CelulasTotais(m));
        }

        [Test]
        public void TripulacaoForaDeForma_SombraComandante_DuasCelulas()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Estufa, 4);
            m.Sombra = new[] { 5, 8 };
            m.FitAmanha = -1;
            RegrasMissao.IniciarDia(m); // dia 5, sombra
            Assert.AreEqual(3 - 1, m.Hoje.Luz);
        }

        [Test]
        public void LuzNuncaMenorQueUm()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Estufa, 1);
            m.Hoje.Luz = 0;
            m.Sombra = new[] { 2, 9 };
            m.FitAmanha = -5;
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(1, m.Hoje.Luz);
        }

        [Test]
        public void SugestaoDoCadete()
        {
            var m = Apoio.Controlada(Modo.Cadete);
            m.Bateria = 1; // 6 de luz + 1 na bateria
            RegrasMissao.PrepararDistribuicao(m);
            Assert.AreEqual(1, m.Hoje.Aloc[Modulo.Bateria]);
            Assert.AreEqual(2, m.Hoje.Aloc[Modulo.Ar]);
            Assert.AreEqual(4, RegrasMissao.CelulasLivres(m));
        }

        [Test]
        public void ComandanteSemSugestao()
        {
            var m = Apoio.Controlada(Modo.Comandante);
            m.Bateria = 2;
            RegrasMissao.PrepararDistribuicao(m);
            Assert.AreEqual(2, m.Hoje.Aloc[Modulo.Bateria]);
            Assert.AreEqual(0, m.Hoje.Aloc[Modulo.Ar]);
        }

        [Test]
        public void LimiteDoModulo_AbrigoCom2()
        {
            var m = Apoio.Controlada();
            Apoio.Alocar(m, abrigo: 2);
            Assert.IsFalse(RegrasMissao.PodeMais(m, Modulo.Abrigo));
            Assert.IsFalse(RegrasMissao.Mais(m, Modulo.Abrigo));
            Assert.IsTrue(RegrasMissao.PodeMais(m, Modulo.Lab));
        }

        [Test]
        public void SemCelulasLivres_TodosOsMaisDesativados()
        {
            var m = Apoio.Controlada();
            m.Bateria = 0;
            Apoio.Alocar(m, ar: 3, estufa: 2, lab: 1); // 6 de 6
            Assert.AreEqual(0, RegrasMissao.CelulasLivres(m));
            foreach (var mod in RegrasMissao.ModulosVisiveis(m)) Assert.IsFalse(RegrasMissao.PodeMais(m, mod), mod.ToString());
            Assert.IsTrue(RegrasMissao.Menos(m, Modulo.Ar));
        }

        [Test]
        public void ConsertoSoComVazamento()
        {
            var m = Apoio.Controlada();
            Assert.IsFalse(RegrasMissao.ModulosVisiveis(m).Contains(Modulo.Conserto));
            Assert.IsFalse(RegrasMissao.PodeMais(m, Modulo.Conserto));
            m.Vazamento = true;
            Assert.IsTrue(RegrasMissao.ModulosVisiveis(m).Contains(Modulo.Conserto));
            Assert.AreEqual(2, RegrasMissao.Maximo(Modulo.Conserto));
        }

        [Test]
        public void LimitesDosModulos()
        {
            Assert.AreEqual(3, RegrasMissao.Maximo(Modulo.Ar));
            Assert.AreEqual(2, RegrasMissao.Maximo(Modulo.Estufa));
            Assert.AreEqual(2, RegrasMissao.Maximo(Modulo.Abrigo));
            Assert.AreEqual(3, RegrasMissao.Maximo(Modulo.Lab));
            Assert.AreEqual(4, RegrasMissao.Maximo(Modulo.Bateria));
        }

        [Test]
        public void Previa_AcompanhaArEAgua()
        {
            var m = Apoio.Controlada();
            Apoio.Alocar(m, ar: 2);
            Assert.AreEqual(Textos.PreviaO2Igual.Pt, RegrasMissao.Previa(m)[0].Pt);
            Apoio.Alocar(m, ar: 1);
            Assert.AreEqual(Textos.PreviaO2Cai.F(1).Pt, RegrasMissao.Previa(m)[0].Pt);
            Assert.AreEqual("Oxygen drops 1.", RegrasMissao.Previa(m)[0].En);
        }

        [Test]
        public void Previa_SemContato()
        {
            var m = Apoio.Controlada();
            m.Contato = false;
            var p = RegrasMissao.Previa(m);
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(Textos.PreviaSemContato.Pt, p[0].Pt);
        }

        [Test]
        public void Previa_AbrigoSoComAlerta()
        {
            var m = Apoio.Controlada();
            Apoio.Alocar(m, ar: 2, abrigo: 1);
            Assert.IsFalse(RegrasMissao.Previa(m).Any(t => t.Pt == Textos.PreviaAbrigo1.Pt));
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            Assert.IsTrue(RegrasMissao.Previa(m).Any(t => t.Pt == Textos.PreviaAbrigo1.Pt));
        }
    }
}
