using System;
using System.Globalization;

namespace DezDias
{
    public enum Lingua { Pt, En }

    /// <summary>Um texto nos dois idiomas. Mostra o do idioma ativo (ToString).</summary>
    public readonly struct Txt
    {
        public readonly string Pt, En;
        public Txt(string pt, string en) { Pt = pt ?? ""; En = en ?? ""; }

        public string Em(Lingua l) => l == Lingua.Pt ? Pt : En;
        public override string ToString() => Em(Idioma.Atual);
        public bool Vazio => string.IsNullOrEmpty(Pt) && string.IsNullOrEmpty(En);

        /// <summary>Preenche {0}, {1}... nos dois idiomas. Um argumento Txt entra no idioma certo.</summary>
        public Txt F(params object[] args)
        {
            return new Txt(Formatar(Pt, Lingua.Pt, args), Formatar(En, Lingua.En, args));
        }

        static string Formatar(string modelo, Lingua l, object[] args)
        {
            var a = new object[args.Length];
            for (int i = 0; i < args.Length; i++) a[i] = args[i] is Txt t ? t.Em(l) : args[i];
            return string.Format(CultureInfo.InvariantCulture, modelo, a);
        }

        public static Txt operator +(Txt a, Txt b) => new Txt(a.Pt + b.Pt, a.En + b.En);
        public static readonly Txt Nada = new Txt("", "");
    }

    /// <summary>Idioma ativo do jogo (português ou inglês) e formatação de datas sem depender da cultura do sistema.</summary>
    public static class Idioma
    {
        public static Lingua Atual { get; private set; } = Lingua.Pt;
        public static event Action Mudou;

        public static void Definir(Lingua l)
        {
            if (l == Atual) return;
            Atual = l;
            Mudou?.Invoke();
        }

        public static void Alternar() => Definir(Atual == Lingua.Pt ? Lingua.En : Lingua.Pt);

        public static string L(string pt, string en) => Atual == Lingua.Pt ? pt : en;

        /// <summary>"pt", "pt-BR", "PT_br" → português; qualquer outro código → inglês.</summary>
        public static Lingua PorCodigo(string codigo)
            => !string.IsNullOrEmpty(codigo) && codigo.Trim().ToLowerInvariant().StartsWith("pt") ? Lingua.Pt : Lingua.En;

        static readonly string[] MesesPt = { "janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };
        static readonly string[] MesesEn = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>"2017-09-06" → "6 de setembro de 2017" ou "September 6, 2017". Lê só a data, sem fuso.</summary>
        public static string FormatarData(string iso, Lingua l)
        {
            if (string.IsNullOrEmpty(iso) || iso.Length < 10) return iso ?? "";
            int ano = int.Parse(iso.Substring(0, 4), CultureInfo.InvariantCulture);
            int mes = int.Parse(iso.Substring(5, 2), CultureInfo.InvariantCulture);
            int dia = int.Parse(iso.Substring(8, 2), CultureInfo.InvariantCulture);
            return l == Lingua.Pt ? $"{dia} de {MesesPt[mes - 1]} de {ano}" : $"{MesesEn[mes - 1]} {dia}, {ano}";
        }

        public static Txt Data(string iso) => new Txt(FormatarData(iso, Lingua.Pt), FormatarData(iso, Lingua.En));
    }
}
