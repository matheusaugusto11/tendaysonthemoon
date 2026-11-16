using System;
using NUnit.Framework;

namespace DezDias.Testes
{
    public class ConversasTestes
    {
        static readonly Momento[] Todos = (Momento[])Enum.GetValues(typeof(Momento));

        [Test]
        public void TodaEtapaDeTodoDiaTemFala_NosDoisIdiomas([Values(Modo.Cadete, Modo.Comandante)] Modo modo, [Values(false, true)] bool tutorial)
        {
            for (int seed = 0; seed < 30; seed++)
            {
                var m = RegrasMissao.NovaPartida(modo, Apoio.Vinte(), new Random(seed), tutorial);
                for (int dia = 1; dia <= Missao.Dias; dia++)
                {
                    RegrasMissao.IniciarDia(m);
                    foreach (var mo in Todos)
                    {
                        var f = Conversas.Para(m, mo, tutorial);
                        Assert.IsNotNull(f);
                        Assert.That(f.Length, Is.InRange(1, 4), $"{mo} dia {dia}");
                        foreach (var (_, fala) in f)
                        {
                            Assert.IsFalse(string.IsNullOrWhiteSpace(fala.Pt), $"{mo} dia {dia} sem PT");
                            Assert.IsFalse(string.IsNullOrWhiteSpace(fala.En), $"{mo} dia {dia} sem EN");
                        }
                    }
                }
            }
        }

        [Test]
        public void Dia1ComTutorial_ComecaComAsFalasDeAberturaDaLia()
        {
            var m = RegrasMissao.NovaPartida(Modo.Cadete, Apoio.Vinte(), new Random(3), true);
            RegrasMissao.IniciarDia(m);
            var f = Conversas.Para(m, Momento.AteTarefa, true);
            for (int i = 0; i < Textos.FalasAbertura.Length; i++)
            {
                Assert.IsFalse(f[i].robo);
                Assert.AreEqual(Textos.FalasAbertura[i].Pt, f[i].fala.Pt);
            }
        }

        [Test]
        public void AlertaForte_OBipAvisaNoBoletim()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Estufa);
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.X(), Forte = true };
            var f = Conversas.Para(m, Momento.Boletim, false);
            Assert.IsTrue(f[0].robo);
            StringAssert.Contains("Alerta forte", f[0].fala.Pt);
        }

        [Test]
        public void TarefaLaForaComAlerta_PedeParaSerRapida()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Amostra);
            m.Alertas[m.Dia] = new Alerta { Erupcao = Apoio.M(), Forte = false };
            var f = Conversas.Para(m, Momento.AteTarefa, false);
            StringAssert.Contains("erupção", f[f.Length - 1].fala.Pt);
        }
    }
}
