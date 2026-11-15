using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.6: efeitos de "fazer" e "ficar" de cada tarefa (spec tarefas-e-minijogos).</summary>
    public class TarefasEfeitoTestes
    {
        static ResultadoMinijogo R(float nota, float tempo = 3f, bool concluiu = true) => new ResultadoMinijogo(nota, tempo, concluiu);

        [Test]
        public void ColetaDePedras_ArDoTrajeAcaba()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Amostra);
            m.O2 = 6; m.Estrelas = 0;
            Tarefas.Fazer(m, R(0.75f, 15f, false));
            Assert.AreEqual(3, m.O2);
            Assert.AreEqual(0, m.Estrelas);
            Assert.IsTrue(m.Hoje.Fora);
            Assert.IsTrue(m.Hoje.TarefaResolvida);
        }

        [Test]
        public void ColetaDePedras_RapidaEComTudo()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Amostra);
            m.O2 = 6;
            Tarefas.Fazer(m, R(1f, 4f));
            Assert.AreEqual(5, m.O2);
            Assert.AreEqual(5, m.Estrelas);
        }

        [Test]
        public void ColetaDePedras_CustoPeloTempo()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Amostra);
            m.O2 = 6;
            Tarefas.Fazer(m, R(0.5f, 9f)); // limite 15: entre 1/3 e 2/3 → custa 2
            Assert.AreEqual(4, m.O2);
            Assert.AreEqual(2, m.Estrelas);
        }

        [Test]
        public void Poeira_LimpezaCompleta()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Poeira);
            m.O2 = 6; m.Bateria = 0;
            int antes = RegrasMissao.CelulasTotais(m);
            Tarefas.Fazer(m, R(0.95f));
            Assert.AreEqual(5, m.O2);
            Assert.AreEqual(antes + 2, RegrasMissao.CelulasTotais(m));
            Assert.IsTrue(m.Hoje.Fora);
        }

        [TestCase(0.95f, 2)]
        [TestCase(0.6f, 1)]
        [TestCase(0.3f, 0)]
        public void Poeira_Bonus(float nota, int bonus)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Poeira);
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(bonus, m.Hoje.Bonus);
        }

        [TestCase(0.9f, 3)]
        [TestCase(0.6f, 2)]
        [TestCase(0.1f, 1)]
        public void Estufa_Crescimento(float nota, int n)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Estufa);
            m.Crescimento = 0;
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(n, m.Crescimento);
            Assert.AreEqual(1, m.Hoje.Custo);
        }

        [Test]
        public void Agua_BemFeita_AteOMaximo()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Agua);
            m.O2 = 9;
            Tarefas.Fazer(m, R(0.85f));
            Assert.AreEqual(10, m.O2);
            Assert.AreEqual(1, m.Hoje.Custo);
        }

        [TestCase(0.85f, 2)]
        [TestCase(0.55f, 1)]
        [TestCase(0.2f, 0)]
        public void Agua_Escala(float nota, int n)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Agua);
            m.O2 = 5;
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(5 + n, m.O2);
        }

        [TestCase(1f, 4)]
        [TestCase(0.67f, 2)]
        [TestCase(0.34f, 1)]
        [TestCase(0f, 0)]
        public void Upload_EstrelasPorPacote(float nota, int estrelas)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Upload);
            m.Estrelas = 0;
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(estrelas, m.Estrelas);
            Assert.AreEqual(2, m.Hoje.Custo);
        }

        [Test]
        public void Filtros_DeixarSujos()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Filtros);
            m.O2 = 5;
            Tarefas.Ficar(m);
            Assert.AreEqual(4, m.O2);
        }

        [Test]
        public void Filtros_NuncaAbaixoDe1()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Filtros);
            m.O2 = 1;
            Tarefas.Ficar(m);
            Assert.AreEqual(1, m.O2);
            Tarefas.Fazer(m, R(0.1f));
            Assert.AreEqual(1, m.O2);
        }

        [TestCase(0.9f, 1)]
        [TestCase(0.6f, 0)]
        [TestCase(0.3f, -1)]
        public void Filtros_Fazer(float nota, int delta)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Filtros);
            m.O2 = 5;
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(5 + delta, m.O2);
        }

        [Test]
        public void Exercicio_PularTiraUmaCelulaAmanha()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Exercicio, 2);
            Tarefas.Ficar(m);
            Assert.AreEqual(-1, m.FitAmanha);
            RegrasMissao.IniciarDia(m);
            Assert.AreEqual(6 - 1, m.Hoje.Luz);
            Assert.AreEqual(-1, m.Hoje.Fit);
        }

        [TestCase(1f, 1)]
        [TestCase(0.6f, 0)]
        [TestCase(0.4f, -1)]
        public void Exercicio_Fazer(float nota, int fit)
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Exercicio);
            Tarefas.Fazer(m, R(nota));
            Assert.AreEqual(fit, m.FitAmanha);
            Assert.AreEqual(1, m.Hoje.Custo);
        }

        [Test]
        public void Braco_CaixaPegaRapido()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Braco);
            m.Comida = 2;
            Tarefas.Fazer(m, R(1f, 5f));
            Assert.AreEqual(5, m.Comida);
        }

        [Test]
        public void Braco_ComAtrasoENaoPega()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Braco);
            m.Comida = 2;
            Tarefas.Fazer(m, R(1f, 9f));
            Assert.AreEqual(4, m.Comida);
            var m2 = Apoio.Controlada(Modo.Comandante, Tarefas.Braco);
            m2.Comida = 2;
            Tarefas.Fazer(m2, R(0f, 14f, false));
            Assert.AreEqual(2, m2.Comida);
        }

        [Test]
        public void Vazamento_DeixarParaDepois()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Vazamento);
            m.O2 = 6;
            Tarefas.Fazer(m, R(1f, 3f));
            Assert.IsFalse(m.Hoje.TarefaResolvida, "falta a decisão");
            Assert.AreEqual(6, m.O2);
            Tarefas.VazamentoDepois(m);
            Assert.IsTrue(m.Vazamento);
            Assert.IsTrue(m.Hoje.TarefaResolvida);
            Assert.IsTrue(RegrasMissao.ModulosVisiveis(m).Contains(Modulo.Conserto));
        }

        [Test]
        public void Vazamento_ReforcarCusta2()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Vazamento);
            m.O2 = 6;
            Tarefas.Fazer(m, R(0.3f, 13f, false)); // sem selar: -2
            Assert.AreEqual(4, m.O2);
            Tarefas.VazamentoReforcar(m);
            Assert.AreEqual(2, m.Hoje.Custo);
            Assert.IsFalse(m.Vazamento);
        }

        [Test]
        public void Vazamento_NuncaDeixaO2AbaixoDe1()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Vazamento);
            m.O2 = 2;
            Tarefas.Fazer(m, R(0f, 13f, false));
            Assert.AreEqual(1, m.O2);
        }

        [Test]
        public void Antena_SeguirSemContato()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Antena);
            Tarefas.Ficar(m);
            Assert.IsFalse(m.Contato);
            Assert.AreEqual(Textos.PreviaSemContato.Pt, RegrasMissao.Previa(m)[0].Pt);
        }

        [Test]
        public void Antena_CadeteEm15s_Custa1()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Antena);
            m.Contato = false;
            Tarefas.Fazer(m, R(1f, 15f));
            Assert.AreEqual(1, m.Hoje.Custo);
            Assert.IsTrue(m.Contato);
            Assert.AreEqual(Tarefas.AtrasoComunicacao.Pt, m.Hoje.Extra.Pt);
        }

        [Test]
        public void Antena_ComandanteEm15s_Custa2()
        {
            var m = Apoio.Controlada(Modo.Comandante, Tarefas.Antena);
            Tarefas.Fazer(m, R(1f, 15f));
            Assert.AreEqual(2, m.Hoje.Custo);
        }

        [Test]
        public void SaidaBloqueadaSemAr()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Amostra);
            m.O2 = 1;
            Assert.IsFalse(Tarefas.PodeFazer(m));
            m.O2 = 2;
            Assert.IsTrue(Tarefas.PodeFazer(m));
            var dentro = Apoio.Controlada(Modo.Cadete, Tarefas.Agua);
            dentro.O2 = 1;
            Assert.IsTrue(Tarefas.PodeFazer(dentro));
        }

        [Test]
        public void TempoEsgotado_VazamentoViraDeixarParaDepois()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Vazamento);
            Tarefas.TempoEsgotado(m);
            Assert.IsTrue(m.Vazamento);
            Assert.IsTrue(m.Hoje.TarefaResolvida);
        }

        [Test]
        public void TempoEsgotado_AplicaOFicar()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Exercicio);
            Tarefas.TempoEsgotado(m);
            Assert.AreEqual(-1, m.FitAmanha);
            StringAssert.StartsWith(Textos.TempoAcabouTarefa.Pt, m.Hoje.MsgTarefa.Pt);
        }

        [Test]
        public void TempoEsgotado_DepoisDaTarefa_NaoCobraNada()
        {
            var m = Apoio.Controlada(Modo.Cadete, Tarefas.Exercicio);
            Tarefas.Fazer(m, R(1f));
            Tarefas.TempoEsgotado(m);
            Assert.AreEqual(1, m.FitAmanha);
        }

        [Test]
        public void LimiteDoCadete_1Virgula3()
        {
            var cad = Apoio.Controlada(Modo.Cadete, Tarefas.Antena);
            var com = Apoio.Controlada(Modo.Comandante, Tarefas.Antena);
            Assert.AreEqual(15f * 1.3f, Tarefas.LimiteReal(cad, Tarefas.Antena), 0.001f);
            Assert.AreEqual(15f, Tarefas.LimiteReal(com, Tarefas.Antena), 0.001f);
        }
    }
}
