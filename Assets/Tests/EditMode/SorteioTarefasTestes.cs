using System;
using System.Linq;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.3: sequência de tarefas (web: pickTasks) e troca em dia de sombra.</summary>
    public class SorteioTarefasTestes
    {
        static readonly string[] Excluidas = { "painel", "falha", "micro", "rover", "co2", "treino" };

        [Test]
        public void MilSeeds_LimitesDoSorteio([Values(Modo.Cadete, Modo.Comandante)] Modo modo, [Values(false, true)] bool tutorial)
        {
            int imprevistosEsperados = modo == Modo.Cadete ? 2 : 3;
            for (int seed = 0; seed < 1000; seed++)
            {
                var t = RegrasMissao.SortearTarefas(modo, new Random(seed), tutorial);
                Assert.AreEqual(10, t.Count);
                for (int i = 1; i < t.Count; i++) Assert.AreNotEqual(t[i - 1], t[i], "repetiu em dias seguidos, seed " + seed);
                foreach (var g in t.GroupBy(x => x)) Assert.That(g.Count() <= 2, g.Key + " apareceu " + g.Count() + " vezes, seed " + seed);
                Assert.That(t.All(x => !Excluidas.Contains(x)), "tarefa excluída no sorteio, seed " + seed);
                Assert.That(t.All(x => Tarefas.Def(x) != null), "tarefa sem definição, seed " + seed);
                int imprevistos = t.Skip(4).Take(4).Count(x => Tarefas.Imprevistos.Contains(x));
                Assert.AreEqual(imprevistosEsperados, imprevistos, "dias 5 a 8, seed " + seed);
                Assert.That(t.Take(4).Concat(t.Skip(8)).All(x => Tarefas.Rotina.Contains(x)), "imprevisto fora dos dias 5 a 8, seed " + seed);
                if (tutorial) Assert.AreEqual(Tarefas.Amostra, t[0]);
            }
        }

        [Test]
        public void PoeiraEmDiaDeSombra_ViraEstufa()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Poeira, 1);
            m.Dia = 0;
            m.Sombra = new[] { 1, 6 };
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(Tarefas.Estufa, m.Hoje.Tarefa);
        }

        [Test]
        public void PoeiraEmDiaDeLuz_Continua()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Poeira, 2);
            Assert.AreEqual(Tarefas.Poeira, m.Hoje.Tarefa);
        }
    }
}
