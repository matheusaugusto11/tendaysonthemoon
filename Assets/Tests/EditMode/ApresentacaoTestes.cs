using System.Collections.Generic;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 2.1 (interface-e-minijogos): contraste da paleta, leitura dos custos e faixa da nota.</summary>
    public class ApresentacaoTestes
    {
        // Pares de texto da spec "Contraste de leitura": todo texto com pelo menos 4,5:1.
        static IEnumerable<TestCaseData> ParesDeTexto()
        {
            yield return new TestCaseData(Paleta.Branco, Paleta.Aco).SetName("creme sobre aço");
            yield return new TestCaseData(Paleta.Branco, Paleta.AcoClaro).SetName("creme sobre aço claro");
            yield return new TestCaseData(Paleta.Branco, Paleta.Pagina).SetName("creme sobre o céu");
            yield return new TestCaseData(Paleta.Apoio, Paleta.Aco).SetName("apoio sobre aço");
            yield return new TestCaseData(Paleta.Apoio, Paleta.AcoClaro).SetName("apoio sobre aço claro");
            yield return new TestCaseData(Paleta.Apoio, Paleta.Pagina).SetName("apoio sobre o céu");
            yield return new TestCaseData(Paleta.Tinta, Paleta.Branco).SetName("tinta sobre botão branco");
            yield return new TestCaseData(Paleta.Energia, Paleta.Aco).SetName("energia sobre aço");
            yield return new TestCaseData(Paleta.Branco, Paleta.Terra).SetName("creme sobre azul-Terra (não deu)");
            yield return new TestCaseData(Paleta.Discreto, Paleta.DesativadoFace).SetName("texto de controle desativado");
        }

        [TestCaseSource(nameof(ParesDeTexto))]
        public void TextoTemContrasteDe4e5(string texto, string fundo)
            => Assert.GreaterOrEqual(Apresentacao.Contraste(texto, fundo), 4.5, texto + " sobre " + fundo);

        [Test]
        public void BotaoPrincipalTemContrasteDe7()
            => Assert.GreaterOrEqual(Apresentacao.Contraste(Paleta.Tinta, Paleta.Branco), 7.0);

        [Test]
        public void FormasTemContrasteDe3ComOFundo()
        {
            // Ciano e vermelho só carregam forma (medidor, ícone), nunca texto: pelo menos 3:1.
            foreach (var cor in new[] { Paleta.Ar, Paleta.Alerta, Paleta.Abrigo, Paleta.Estufa, Paleta.Lab, Paleta.Energia })
                Assert.GreaterOrEqual(Apresentacao.Contraste(cor, Paleta.Aco), 3.0, cor);
        }

        [Test]
        public void ContrasteConheceOsExtremos()
        {
            Assert.AreEqual(21.0, Apresentacao.Contraste("#FFFFFF", "#000000"), 0.01);
            Assert.AreEqual(1.0, Apresentacao.Contraste("#2E323B", "#2E323B"), 0.0001);
        }

        [Test]
        public void FaixaDaNotaNosLimites()
        {
            Assert.AreEqual(FaixaNota.Otimo, Apresentacao.FaixaDaNota(1.0f));
            Assert.AreEqual(FaixaNota.Bom, Apresentacao.FaixaDaNota(0.5f));
            Assert.AreEqual(FaixaNota.NaoDeu, Apresentacao.FaixaDaNota(0.49f));
            Assert.AreEqual(FaixaNota.NaoDeu, Apresentacao.FaixaDaNota(0f));
            Assert.AreEqual(FaixaNota.Bom, Apresentacao.FaixaDaNota(0.99f));
            // A nota mostrada é arredondada: 99,6% aparece como 100% e portanto é "Ótimo".
            Assert.AreEqual(FaixaNota.Otimo, Apresentacao.FaixaDaNota(0.996f));
            Assert.AreEqual(FaixaNota.Otimo, Apresentacao.FaixaDaNota(1.3f), "acima de 1 é limitado");
            Assert.AreEqual(FaixaNota.NaoDeu, Apresentacao.FaixaDaNota(-0.2f), "abaixo de 0 é limitado");
        }

        [Test]
        public void CadaCustoDeCadaTarefaGeraChipsComRecurso()
        {
            int n = 0, semEscolha = 0;
            foreach (var id in Tarefas.Todas)
            {
                var d = Tarefas.Def(id);
                // O vazamento não tem "fazer/ficar": a tela é só o botão de remendar, e os custos dele vêm da decisão (teste abaixo).
                if (id == Tarefas.Vazamento)
                {
                    Assert.IsTrue(d.FazerCusto.Vazio && d.FicarCusto.Vazio);
                    semEscolha++;
                    continue;
                }
                foreach (var (nome, custo) in new[] { ("FazerCusto", d.FazerCusto), ("FicarCusto", d.FicarCusto) })
                    foreach (var lingua in new[] { Lingua.Pt, Lingua.En })
                    {
                        var partes = Apresentacao.PartesDoCusto(custo.Em(lingua));
                        n++;
                        Assert.IsNotEmpty(partes, $"{id}.{nome} {lingua}");
                        foreach (var p in partes)
                        {
                            Assert.AreNotEqual(Recurso.Nenhum, p.Recurso, $"{id}.{nome} {lingua}: recurso não reconhecido em \"{p.Texto}\"");
                            Assert.IsNotEmpty(p.Texto, $"{id}.{nome} {lingua}: chip sem texto");
                        }
                    }
            }
            Assert.AreEqual((Tarefas.Todas.Length - semEscolha) * 4, n);
            Assert.AreEqual(1, semEscolha);
        }

        [Test]
        public void CustosDoVazamentoTambemSaoLidos()
        {
            foreach (var t in new[] { Tarefas.VazReforcarCusto, Tarefas.VazDepoisCusto })
                foreach (var lingua in new[] { Lingua.Pt, Lingua.En })
                    foreach (var p in Apresentacao.PartesDoCusto(t.Em(lingua)))
                        Assert.AreNotEqual(Recurso.Nenhum, p.Recurso, t.Em(lingua));
        }

        [Test]
        public void LeCustoComDoisRecursos()
        {
            var p = Apresentacao.PartesDoCusto("O2 -1 a -3 · ciência até +5");
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual(Recurso.Oxigenio, p[0].Recurso);
            Assert.AreEqual("−1 a −3", p[0].Texto);
            Assert.AreEqual(Recurso.Ciencia, p[1].Recurso);
            Assert.AreEqual("até +5", p[1].Texto);
        }

        [Test]
        public void LeCustoComAmanha()
        {
            var pt = Apresentacao.PartesDoCusto("amanhã energia -1");
            Assert.AreEqual(Recurso.Energia, pt[0].Recurso);
            Assert.AreEqual("amanhã −1", pt[0].Texto);
            var en = Apresentacao.PartesDoCusto("energy -1 today · tomorrow energy -1 to +1");
            Assert.AreEqual(2, en.Count);
            Assert.AreEqual("tomorrow −1 to +1", en[1].Texto);
        }

        [Test]
        public void CustoVazioOuDesconhecidoNaoQuebra()
        {
            Assert.IsEmpty(Apresentacao.PartesDoCusto(""));
            Assert.IsEmpty(Apresentacao.PartesDoCusto(null as string));
            var p = Apresentacao.PartesDoCusto("tudo grátis");
            Assert.AreEqual(Recurso.Nenhum, p[0].Recurso);
            Assert.AreEqual("tudo grátis", p[0].Texto);
        }

        [Test]
        public void ZonaDeAlertaDoOxigenioEDe3ParaBaixo()
        {
            Assert.IsTrue(Apresentacao.OxigenioEmAlerta(0));
            Assert.IsTrue(Apresentacao.OxigenioEmAlerta(3));
            Assert.IsFalse(Apresentacao.OxigenioEmAlerta(4));
            Assert.IsFalse(Apresentacao.OxigenioEmAlerta(Missao.MaxO2));
        }

        [Test]
        public void ZonaDeAlertaDaRadiacaoEDe7ParaCima()
        {
            Assert.IsFalse(Apresentacao.RadiacaoEmAlerta(0));
            Assert.IsFalse(Apresentacao.RadiacaoEmAlerta(6));
            Assert.IsTrue(Apresentacao.RadiacaoEmAlerta(7));
            Assert.IsTrue(Apresentacao.RadiacaoEmAlerta(Missao.MaxRad));
        }

        [Test]
        public void AsZonasCabemNosLimitesDoJogo()
        {
            // A zona tem que ser alcançável antes do fim da missão (O2 em 0 e radiação em 10 encerram).
            Assert.Less(Apresentacao.OxigenioAlerta, Missao.MaxO2);
            Assert.Greater(Apresentacao.OxigenioAlerta, 0);
            Assert.Less(Apresentacao.RadiacaoAlerta, Missao.MaxRad);
            Assert.Greater(Apresentacao.RadiacaoAlerta, 0);
        }

        [Test]
        public void PaletaTemHexValido()
        {
            foreach (var f in typeof(Paleta).GetFields())
            {
                var v = (string)f.GetValue(null);
                Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(v, "^#[0-9A-Fa-f]{6}$"), f.Name + " = " + v);
            }
        }
    }
}
