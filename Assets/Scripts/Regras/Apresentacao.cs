using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DezDias
{
    /// <summary>
    /// Paleta da interface em hex, por papel (design D2 da mudança interface-e-minijogos; Art Bible v3 §6).
    /// Fica no assembly de regras, sem UnityEngine, para os testes conferirem contraste. UiTema converte para Color.
    /// </summary>
    public static class Paleta
    {
        // Superfícies
        public const string Aco = "#2E323B", AcoClaro = "#3A3F4A", Sombra = "#1A1C22", Pagina = "#05060C", Vazio = "#23262E";
        public const string Branco = "#E6E4DF", BrancoSombra = "#8E8C86", Tinta = "#14161C";
        // Texto: creme, apoio e o discreto (só controles desativados, sobre DesativadoFace)
        public const string Apoio = "#A9ADB8", Discreto = "#949AA7", DesativadoFace = "#26282F", DesativadoSombra = "#1B1D22";
        // Um acento por sistema (iguais aos do módulo 3D)
        public const string Ar = "#4DD0E1", Estufa = "#6BD425", Abrigo = "#C77DFF", Lab = "#FF9F43", Energia = "#FFD23F";
        // Alerta (só medidor e ícone de aviso) e "não deu" (azul-Terra, calmo)
        public const string Alerta = "#FF5D5D", Terra = "#52638A";
        // Tanque de água usada nos canos: mostarda apagado, porque o amarelo puro é só da energia
        public const string AguaUsada = "#D9B45A";
        // Arte dos minijogos (design D9): painel solar, filtro, regolito e pedras, no estilo low-poly da base
        public const string PainelSolar = "#2B5C9E", PainelSolarJunta = "#1E3F6E", Filtro = "#C9D3DC", FiltroTrama = "#A9B6C2";
        public const string Regolito = "#8C8170", Chao = "#4A4743", ChaoClaro = "#5A5650", Pedra = "#9E9AA6", PedraClara = "#BDB9C4";
    }

    /// <summary>Recurso reconhecido num texto de custo (vira o ícone do chip).</summary>
    public enum Recurso { Nenhum, Oxigenio, Energia, Ciencia, Estufa, Comida, Radiacao, Bateria, Tempo, Contato }

    /// <summary>Uma parte de um custo: o recurso (para o ícone) e o que sobra do texto (para o chip).</summary>
    public readonly struct ParteCusto
    {
        public readonly Recurso Recurso;
        public readonly string Texto;
        public ParteCusto(Recurso recurso, string texto) { Recurso = recurso; Texto = texto; }
    }

    /// <summary>Faixa do resultado de um minijogo.</summary>
    public enum FaixaNota { Otimo, Bom, NaoDeu }

    /// <summary>Lógica de apresentação sem UnityEngine: contraste, faixa da nota e leitura dos custos (design D12).</summary>
    public static class Apresentacao
    {
        /// <summary>"Ótimo!" com 100%, "Bom!" a partir de 50%, senão "Não deu desta vez". A nota é a mostrada (arredondada).</summary>
        public static FaixaNota FaixaDaNota(float nota)
        {
            int pct = (int)Math.Round(Math.Max(0f, Math.Min(1f, nota)) * 100.0, MidpointRounding.AwayFromZero);
            return pct >= 100 ? FaixaNota.Otimo : pct >= 50 ? FaixaNota.Bom : FaixaNota.NaoDeu;
        }

        // ---------- Zonas de alerta dos medidores (spec hud-e-moldura) ----------

        /// <summary>Oxigênio de 0 a 10: de 3 para baixo é a zona de alerta.</summary>
        public const int OxigenioAlerta = 3;
        /// <summary>Radiação de 0 a 10: de 7 para cima é a zona de alerta.</summary>
        public const int RadiacaoAlerta = 7;

        public static bool OxigenioEmAlerta(int oxigenio) => oxigenio <= OxigenioAlerta;
        public static bool RadiacaoEmAlerta(int radiacao) => radiacao >= RadiacaoAlerta;

        // ---------- Contraste (WCAG) ----------

        static double Luminancia(string hex)
        {
            string h = hex.TrimStart('#');
            double Canal(int i)
            {
                double c = int.Parse(h.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
                return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Canal(0) + 0.7152 * Canal(2) + 0.0722 * Canal(4);
        }

        /// <summary>Razão de contraste entre duas cores em hex (de 1 a 21).</summary>
        public static double Contraste(string a, string b)
        {
            double la = Luminancia(a), lb = Luminancia(b);
            if (la < lb) { var t = la; la = lb; lb = t; }
            return (la + 0.05) / (lb + 0.05);
        }

        // ---------- Custos ----------

        static readonly Dictionary<string, Recurso> Palavras = new Dictionary<string, Recurso>(StringComparer.OrdinalIgnoreCase)
        {
            ["o2"] = Recurso.Oxigenio, ["oxigênio"] = Recurso.Oxigenio, ["oxigenio"] = Recurso.Oxigenio, ["oxygen"] = Recurso.Oxigenio,
            ["energia"] = Recurso.Energia, ["energy"] = Recurso.Energia,
            ["ciência"] = Recurso.Ciencia, ["ciencia"] = Recurso.Ciencia, ["science"] = Recurso.Ciencia,
            ["estufa"] = Recurso.Estufa, ["greenhouse"] = Recurso.Estufa,
            ["comida"] = Recurso.Comida, ["food"] = Recurso.Comida,
            ["radiação"] = Recurso.Radiacao, ["radiacao"] = Recurso.Radiacao, ["radiation"] = Recurso.Radiacao,
            ["bateria"] = Recurso.Bateria, ["battery"] = Recurso.Bateria,
            ["tempo"] = Recurso.Tempo, ["time"] = Recurso.Tempo,
            ["contato"] = Recurso.Contato, ["contact"] = Recurso.Contato,
        };

        static readonly string[] Prefixos = { "amanhã", "tomorrow" };

        /// <summary>Lê um texto de custo como "O2 -1 a -3 · ciência até +5" em partes (recurso e texto do chip).</summary>
        public static List<ParteCusto> PartesDoCusto(Txt custo) => PartesDoCusto(custo.Em(Idioma.Atual));

        public static List<ParteCusto> PartesDoCusto(string custo)
        {
            var partes = new List<ParteCusto>();
            if (string.IsNullOrWhiteSpace(custo)) return partes;
            foreach (var bruto in custo.Split(new[] { '·' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string seg = bruto.Trim();
                if (seg.Length == 0) continue;
                string prefixo = "";
                foreach (var p in Prefixos)
                    if (seg.StartsWith(p + " ", StringComparison.OrdinalIgnoreCase)) { prefixo = p + " "; seg = seg.Substring(prefixo.Length).TrimStart(); break; }
                int esp = seg.IndexOf(' ');
                string palavra = esp < 0 ? seg : seg.Substring(0, esp);
                string resto = esp < 0 ? "" : seg.Substring(esp + 1).Trim();
                if (Palavras.TryGetValue(palavra, out var rec))
                    partes.Add(new ParteCusto(rec, Menos(prefixo + resto).Trim()));
                else
                    partes.Add(new ParteCusto(Recurso.Nenhum, Menos(prefixo + seg)));
            }
            return partes;
        }

        /// <summary>Troca o hífen antes de número pelo sinal de menos (U+2212), que as fontes do jogo têm.</summary>
        static string Menos(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                bool inicio = i == 0 || s[i - 1] == ' ';
                bool antesDeNumero = i + 1 < s.Length && char.IsDigit(s[i + 1]);
                sb.Append(s[i] == '-' && inicio && antesDeNumero ? '−' : s[i]);
            }
            return sb.ToString();
        }
    }
}
