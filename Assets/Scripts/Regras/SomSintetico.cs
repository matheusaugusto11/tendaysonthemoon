using System;

namespace DezDias
{
    /// <summary>
    /// Sons de interface sintetizados em código (design D10 da mudança interface-e-minijogos), em amostras de ponto
    /// flutuante, sem UnityEngine, para os testes conferirem duração, volume e altura. SonsUI transforma em AudioClip.
    /// Todo som começa e termina em zero (sem estalo) e nenhum passa de 0,4 de amplitude.
    /// </summary>
    public static class SomSintetico
    {
        public const int Taxa = 44100;
        public enum Onda { Seno, Triangular }

        /// <summary>Uma nota: frequência (Hz) e duração (s).</summary>
        public readonly struct Nota
        {
            public readonly float Hz, Segundos;
            public Nota(float hz, float segundos) { Hz = hz; Segundos = segundos; }
        }

        static float Forma(Onda onda, double fase)
        {
            double f = fase - Math.Floor(fase); // 0..1
            return onda == Onda.Seno
                ? (float)Math.Sin(2.0 * Math.PI * f)
                : (float)(4.0 * Math.Abs(f - 0.5) - 1.0) * -1f; // triangular de -1 a 1 que começa em 0 perto de f=0,25
        }

        /// <summary>Notas em sequência, cada uma com subida curta e descida suave, para não estalar.</summary>
        public static float[] Sequencia(Nota[] notas, Onda onda, float volume, float ataque = 0.004f, float descidaMax = 0.06f)
        {
            int total = 0;
            foreach (var n in notas) total += (int)Math.Round(n.Segundos * Taxa);
            var s = new float[total];
            int pos = 0;
            foreach (var n in notas)
            {
                int len = (int)Math.Round(n.Segundos * Taxa);
                double fase = 0.0, passo = n.Hz / Taxa;
                float desc = Math.Min(descidaMax, n.Segundos * 0.7f);
                for (int i = 0; i < len; i++)
                {
                    float t = i / (float)Taxa;
                    float sobe = Math.Min(1f, t / ataque);
                    float desce = Math.Min(1f, (n.Segundos - t) / desc);
                    float env = sobe * desce * desce; // descida quadrática: some devagar, sem corte
                    s[pos + i] = Forma(onda, fase) * env * volume;
                    fase += passo;
                }
                pos += len;
            }
            return s;
        }

        // ---------- Os cinco sons ----------

        /// <summary>Todo botão: 880 Hz por 40 ms.</summary>
        public static float[] Toque() => Sequencia(new[] { new Nota(880f, 0.04f) }, Onda.Seno, 0.30f, 0.003f, 0.03f);

        /// <summary>Ação certa no minijogo: 660 e depois 990 Hz, 120 ms.</summary>
        public static float[] Acerto() => Sequencia(new[] { new Nota(660f, 0.06f), new Nota(990f, 0.06f) }, Onda.Seno, 0.32f, 0.003f, 0.04f);

        /// <summary>Fim com "Bom!" ou "Ótimo!": arpejo de três notas (dó, mi, sol), 300 ms.</summary>
        public static float[] Concluido() => Sequencia(new[] { new Nota(523.25f, 0.10f), new Nota(659.25f, 0.10f), new Nota(783.99f, 0.10f) }, Onda.Seno, 0.34f);

        /// <summary>Fim com "Não deu": 330 e depois 262 Hz, triangular macia e mais grave, 300 ms. Nunca áspero.</summary>
        /// <summary>Alarme de partículas: duas notas alternadas, três vezes. Firme, mas sem aspereza (onda triangular).</summary>
        public static float[] Alarme() => Sequencia(new[]
        {
            new Nota(784f, 0.12f), new Nota(587.33f, 0.12f), new Nota(784f, 0.12f),
            new Nota(587.33f, 0.12f), new Nota(784f, 0.12f), new Nota(587.33f, 0.16f),
        }, Onda.Triangular, 0.30f, 0.006f, 0.05f);

        public static float[] NaoDeu() => Sequencia(new[] { new Nota(330f, 0.15f), new Nota(262f, 0.15f) }, Onda.Triangular, 0.26f, 0.008f, 0.09f);

        /// <summary>Painel entrando: ruído filtrado de 80 ms, bem baixo.</summary>
        public static float[] Painel()
        {
            int len = (int)Math.Round(0.08 * Taxa);
            var s = new float[len];
            var rnd = new Random(7);
            float y = 0f;
            for (int i = 0; i < len; i++)
            {
                float ruido = (float)(rnd.NextDouble() * 2.0 - 1.0);
                y += 0.06f * (ruido - y); // passa-baixa de um polo: tira o chiado agudo
                float t = i / (float)len;
                float env = (float)(Math.Sin(Math.PI * t)); // sobe e desce, nasce e termina em zero
                s[i] = y * 2.2f * env * 0.12f;
            }
            return s;
        }

        // ---------- Medidas para os testes ----------

        public static float Pico(float[] s) { float p = 0f; foreach (var v in s) p = Math.Max(p, Math.Abs(v)); return p; }

        public static float Segundos(float[] s) => s.Length / (float)Taxa;

        /// <summary>Frequência estimada pelas passagens por zero no trecho [de, ate) em segundos.</summary>
        public static float FrequenciaEstimada(float[] s, float de, float ate)
        {
            int a = Math.Max(0, (int)(de * Taxa)), b = Math.Min(s.Length, (int)(ate * Taxa));
            int cruzes = 0;
            for (int i = a + 1; i < b; i++) if ((s[i - 1] < 0f) != (s[i] < 0f)) cruzes++;
            float dur = (b - a) / (float)Taxa;
            return dur <= 0f ? 0f : cruzes / (2f * dur);
        }
    }
}
