using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.4 (interface-e-minijogos): cada som é curto, suave e o "não deu" é mais grave que o de acerto.</summary>
    public class SomSinteticoTestes
    {
        static (string nome, float[] amostras, float maxSegundos)[] Todos() => new[]
        {
            ("toque", SomSintetico.Toque(), 0.05f), ("acerto", SomSintetico.Acerto(), 0.13f),
            ("concluido", SomSintetico.Concluido(), 0.31f), ("naodeu", SomSintetico.NaoDeu(), 0.31f), ("painel", SomSintetico.Painel(), 0.09f),
        };

        [Test]
        public void TodoSomEhCurtoEBaixo()
        {
            foreach (var (nome, s, max) in Todos())
            {
                Assert.Greater(s.Length, 0, nome);
                Assert.LessOrEqual(SomSintetico.Segundos(s), max, nome + " longo demais");
                Assert.LessOrEqual(SomSintetico.Pico(s), 0.4f, nome + " alto demais (sem agudo estridente)");
                Assert.Greater(SomSintetico.Pico(s), 0.02f, nome + " inaudível");
            }
        }

        [Test]
        public void TodoSomComecaETerminaEmZeroSemEstalo()
        {
            foreach (var (nome, s, _) in Todos())
            {
                Assert.That(System.Math.Abs(s[0]), Is.LessThan(0.01f), nome + " começa com estalo");
                Assert.That(System.Math.Abs(s[s.Length - 1]), Is.LessThan(0.01f), nome + " termina com estalo");
            }
        }

        [Test]
        public void NaoDeuEhMaisGraveQueAcerto()
        {
            // Acerto: 660 Hz na primeira metade. Não deu: 330 Hz na primeira metade e 262 Hz na segunda.
            var acerto = SomSintetico.Acerto();
            var naoDeu = SomSintetico.NaoDeu();
            float fAcerto = SomSintetico.FrequenciaEstimada(acerto, 0.01f, 0.05f);
            float f1 = SomSintetico.FrequenciaEstimada(naoDeu, 0.02f, 0.14f);
            float f2 = SomSintetico.FrequenciaEstimada(naoDeu, 0.17f, 0.29f);
            Assert.That(fAcerto, Is.EqualTo(660f).Within(60f));
            Assert.That(f1, Is.EqualTo(330f).Within(40f));
            Assert.That(f2, Is.EqualTo(262f).Within(40f));
            Assert.Less(f1, fAcerto);
            Assert.Less(f2, f1, "o não deu desce, nunca sobe");
        }

        [Test]
        public void ConcluidoSobe()
        {
            var s = SomSintetico.Concluido();
            float f1 = SomSintetico.FrequenciaEstimada(s, 0.01f, 0.09f);
            float f3 = SomSintetico.FrequenciaEstimada(s, 0.21f, 0.29f);
            Assert.Less(f1, f3, "o arpejo de concluído sobe");
        }

        [Test]
        public void NaoDeuNaoTemPicoMaiorQueOConcluido()
        {
            Assert.LessOrEqual(SomSintetico.Pico(SomSintetico.NaoDeu()), SomSintetico.Pico(SomSintetico.Concluido()));
        }

        [Test]
        public void PainelEhRuidoSuaveSemAgudoLongo()
        {
            var s = SomSintetico.Painel();
            Assert.LessOrEqual(SomSintetico.Pico(s), 0.15f);
            Assert.AreEqual(0.08f, SomSintetico.Segundos(s), 0.002f);
        }
    }
}
