using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace DezDias.Testes
{
    /// <summary>Tarefa 3.1: todo texto nos dois idiomas, idioma inicial e datas.</summary>
    public class IdiomaTestes
    {
        static IEnumerable<(string nome, Txt t)> TextosDe(System.Type tipo)
        {
            foreach (var f in tipo.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var v = f.GetValue(null);
                if (v is Txt t) yield return (f.Name, t);
                else if (v is Txt[] arr) for (int i = 0; i < arr.Length; i++) yield return (f.Name + "[" + i + "]", arr[i]);
                else if (v is System.ValueTuple<string, string, Txt>[] fontes)
                    for (int i = 0; i < fontes.Length; i++) yield return (f.Name + "[" + i + "]", fontes[i].Item3);
            }
        }

        [Test]
        public void TodoTextoTemPtEEn()
        {
            int n = 0;
            foreach (var tipo in new[] { typeof(Textos), typeof(Tarefas) })
                foreach (var (nome, t) in TextosDe(tipo))
                {
                    n++;
                    Assert.IsFalse(string.IsNullOrWhiteSpace(t.Pt), tipo.Name + "." + nome + " sem PT");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(t.En), tipo.Name + "." + nome + " sem EN");
                }
            Assert.Greater(n, 150);
        }

        [Test]
        public void TodaTarefaTemTextosNosDoisIdiomas()
        {
            foreach (var id in Tarefas.Todas)
            {
                var d = Tarefas.Def(id);
                Assert.IsNotNull(d, id);
                var obrig = new List<(string, Txt)> { ("Nome", d.Nome), ("Titulo", d.Titulo), ("Texto", d.Texto), ("MiniTitulo", d.MiniTitulo), ("MiniInstrucao", d.MiniInstrucao) };
                if (id != Tarefas.Vazamento)
                    obrig.AddRange(new[] { ("FazerTitulo", d.FazerTitulo), ("FazerCusto", d.FazerCusto), ("FazerDesc", d.FazerDesc), ("FicarTitulo", d.FicarTitulo), ("FicarCusto", d.FicarCusto), ("FicarDesc", d.FicarDesc) });
                foreach (var (nome, t) in obrig)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(t.Pt), id + "." + nome + " sem PT");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(t.En), id + "." + nome + " sem EN");
                }
                Assert.IsFalse(string.IsNullOrEmpty(d.Estacao), id + " sem estação");
            }
        }

        [Test]
        public void TextosSoComLatin1()
        {
            foreach (var tipo in new[] { typeof(Textos), typeof(Tarefas) })
                foreach (var (nome, t) in TextosDe(tipo))
                    foreach (var s in new[] { t.Pt, t.En })
                        foreach (var c in s)
                            Assert.That(c <= 'ÿ', tipo.Name + "." + nome + " tem o caractere U+" + ((int)c).ToString("X4"));
        }

        [Test]
        public void Datas()
        {
            Assert.AreEqual("6 de setembro de 2017", Idioma.FormatarData("2017-09-06", Lingua.Pt));
            Assert.AreEqual("September 6, 2017", Idioma.FormatarData("2017-09-06", Lingua.En));
            Assert.AreEqual("1 de março de 2025", Idioma.FormatarData("2025-03-01T00:00Z", Lingua.Pt));
        }

        [Test]
        public void IdiomaInicialPeloCodigo()
        {
            Assert.AreEqual(Lingua.Pt, Idioma.PorCodigo("pt-BR"));
            Assert.AreEqual(Lingua.Pt, Idioma.PorCodigo("PT"));
            Assert.AreEqual(Lingua.En, Idioma.PorCodigo("en-US"));
            Assert.AreEqual(Lingua.En, Idioma.PorCodigo("es"));
            Assert.AreEqual(Lingua.En, Idioma.PorCodigo(""));
        }

        [Test]
        public void TrocaDeIdioma_AvisaEMostraOOutro()
        {
            var inicial = Idioma.Atual;
            int avisos = 0;
            void Ouvir() => avisos++;
            Idioma.Mudou += Ouvir;
            try
            {
                Idioma.Definir(Lingua.Pt);
                Assert.AreEqual("Dez Dias na Lua", Textos.Titulo.ToString());
                Idioma.Alternar();
                Assert.AreEqual("Ten Days on the Moon", Textos.Titulo.ToString());
                Assert.GreaterOrEqual(avisos, 1);
            }
            finally
            {
                Idioma.Mudou -= Ouvir;
                Idioma.Definir(inicial);
            }
        }

        [Test]
        public void FormatoComTxtDentro()
        {
            var t = Textos.ValeComo.F(Textos.Intensidade[2]);
            Assert.AreEqual("No jogo ela vale como erupção forte.", t.Pt);
            Assert.AreEqual("In the game it counts as a strong flare.", t.En);
        }
    }
}
