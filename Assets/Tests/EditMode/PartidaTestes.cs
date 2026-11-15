using System;
using System.Linq;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.1 e 2.2: assembly de regras e sorteio da partida (web: newGame).</summary>
    public class PartidaTestes
    {
        [Test]
        public void Sanidade_AssemblyDeRegrasCarrega()
        {
            Assert.AreEqual(10, Missao.Dias);
        }

        [Test]
        public void ValoresIniciais_Cadete()
        {
            var m = RegrasMissao.NovaPartida(Modo.Cadete, Apoio.Vinte(), new Random(7), false);
            Assert.AreEqual(9, m.O2); Assert.AreEqual(7, m.Comida); Assert.AreEqual(0, m.Rad);
            Assert.AreEqual(1, m.Bateria); Assert.AreEqual(0, m.Estrelas);
            Assert.AreEqual(6, m.M.Luz); Assert.AreEqual(4, m.M.Sombra);
            Assert.AreEqual(4, m.Alertas.Count);
            Assert.AreEqual(2, m.Alertas.Values.Count(a => a.Erupcao != null));
        }

        [Test]
        public void ValoresIniciais_Comandante()
        {
            var m = RegrasMissao.NovaPartida(Modo.Comandante, Apoio.Vinte(), new Random(7), false);
            Assert.AreEqual(8, m.O2); Assert.AreEqual(6, m.Comida); Assert.AreEqual(0, m.Rad);
            Assert.AreEqual(1, m.Bateria); Assert.AreEqual(0, m.Estrelas);
            Assert.AreEqual(6, m.M.Luz); Assert.AreEqual(3, m.M.Sombra);
            Assert.AreEqual(6, m.Alertas.Count);
            Assert.AreEqual(3, m.Alertas.Values.Count(a => a.Erupcao != null));
        }

        [Test]
        public void MilSeeds_SombraAlertasEErupcoes([Values(Modo.Cadete, Modo.Comandante)] Modo modo)
        {
            var M = ParametrosModo.De(modo);
            for (int seed = 0; seed < 1000; seed++)
            {
                var m = RegrasMissao.NovaPartida(modo, Apoio.Vinte(), new Random(seed), false);
                Assert.That(m.Sombra.All(d => d >= 4 && d <= 9), "sombra fora de 4..9, seed " + seed);
                Assert.That(Math.Abs(m.Sombra[0] - m.Sombra[1]) >= 2, "sombras perto demais, seed " + seed);
                Assert.IsFalse(m.Alertas.ContainsKey(1), "Dia 1 com alerta, seed " + seed);
                Assert.That(m.Alertas.Keys.All(d => d >= 2 && d <= 10));
                Assert.AreEqual(M.Alertas, m.Alertas.Count);
                var reais = m.Alertas.Values.Where(a => a.Erupcao != null).ToList();
                Assert.AreEqual(M.Reais, reais.Count, "seed " + seed);
                Assert.That(reais.Any(a => a.Erupcao.Intensidade == 2), "sem erupção X, seed " + seed);
                Assert.AreEqual(reais.Count, reais.Select(a => a.Erupcao.Id).Distinct().Count(), "erupção repetida, seed " + seed);
            }
        }

        [Test]
        public void ForcaDoAlerta_SegueAsProbabilidades()
        {
            int fortesReais = 0, reais = 0, fortesFalsos = 0, falsos = 0;
            for (int seed = 0; seed < 2000; seed++)
            {
                var m = RegrasMissao.NovaPartida(Modo.Cadete, Apoio.Vinte(), new Random(seed), false);
                foreach (var a in m.Alertas.Values)
                {
                    if (a.Erupcao != null) { reais++; if (a.Forte) fortesReais++; }
                    else { falsos++; if (a.Forte) fortesFalsos++; }
                }
            }
            Assert.AreEqual(0.8, fortesReais / (double)reais, 0.04);
            Assert.AreEqual(0.2, fortesFalsos / (double)falsos, 0.04);
        }
    }
}
