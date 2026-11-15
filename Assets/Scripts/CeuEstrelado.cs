using UnityEngine;

/// <summary>
/// Céu de estrelas gerado em código (sem textura de terceiros). Cria as 6 faces do skybox "6 Sided"
/// com pontos de luz sorteados com semente fixa, para o céu ser sempre igual.
/// </summary>
public static class CeuEstrelado
{
    const int Tamanho = 512, EstrelasPorFace = 420, Semente = 20261114;
    static readonly string[] Faces = { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" };

    /// <summary>Põe as estrelas nas 6 faces do material (que usa o shader Skybox/6 Sided).</summary>
    public static void Aplicar(Material m)
    {
        if (m == null) return;
        var rng = new System.Random(Semente);
        for (int f = 0; f < Faces.Length; f++)
        {
            if (!m.HasProperty(Faces[f])) continue;
            m.SetTexture(Faces[f], Face(rng));
        }
    }

    static Texture2D Face(System.Random rng)
    {
        var px = new Color32[Tamanho * Tamanho];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(2, 3, 8, 255);   // fundo quase preto, azulado
        for (int s = 0; s < EstrelasPorFace; s++)
        {
            int cx = rng.Next(4, Tamanho - 4), cy = rng.Next(4, Tamanho - 4);
            double sorte = rng.NextDouble();
            float brilho = sorte < 0.04 ? 1f : sorte < 0.2 ? 0.7f : 0.25f + 0.3f * (float)rng.NextDouble();
            float raio = brilho > 0.9f ? 2.2f : brilho > 0.6f ? 1.5f : 1f;
            float quente = (float)rng.NextDouble();   // 0 azulada, 1 amarelada
            var cor = new Color(0.8f + 0.2f * quente, 0.85f + 0.05f * quente, 1f - 0.3f * quente);
            int r = Mathf.CeilToInt(raio * 2f);
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    float d2 = (dx * dx + dy * dy) / (raio * raio);
                    float v = brilho * Mathf.Exp(-d2 * 1.4f);
                    if (v < 0.02f) continue;
                    int i = (cy + dy) * Tamanho + cx + dx;
                    var a = px[i];
                    px[i] = new Color32(
                        (byte)Mathf.Min(255, a.r + cor.r * v * 255f),
                        (byte)Mathf.Min(255, a.g + cor.g * v * 255f),
                        (byte)Mathf.Min(255, a.b + cor.b * v * 255f), 255);
                }
        }
        var t = new Texture2D(Tamanho, Tamanho, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        t.SetPixels32(px);
        t.Apply(false, true);
        return t;
    }
}
