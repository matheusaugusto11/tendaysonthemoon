using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Formas vetoriais da interface, desenhadas em malha (design D4 da mudança interface-e-minijogos): chanfrado, polígono,
/// anel e arco, triângulo, faixa hachurada e moldura tracejada. Nítidas em qualquer tamanho, sem textura, com uma franja
/// de 1 unidade de alfa para suavizar as diagonais (o canvas em Overlay não tem MSAA). Herda de MaskableGraphic para ser
/// recortada por RectMask2D (as áreas de rolagem); com Graphic puro o fundo de um painel vazava por cima do rodapé.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class FormaUI : MaskableGraphic
{
    public enum Modo { Chanfrado, Poligono, Anel, Triangulo, Hachurada, Moldura }

    public Modo modo = Modo.Chanfrado;

    [Header("Chanfrado")]
    public float chanfro = UiTema.ChanfroPainel;
    [Tooltip("Cantos cortados. O padrão é a assinatura do jogo: superior esquerdo e inferior direito.")]
    public bool cantoSE = true, cantoSD = false, cantoIE = false, cantoID = true;

    [Header("Polígono e triângulo")]
    public int lados = 7;
    [Range(0f, 0.6f)] public float irregularidade = 0.25f;
    public int semente = 1;
    [Tooltip("Graus, no sentido anti-horário. O triângulo aponta para cima com 0.")]
    public float rotacao;

    [Header("Anel e arco (começa no topo, sentido horário)")]
    [Range(0f, 1f)] public float fracao = 1f;
    public float espessura = 12f;

    [Header("Faixa hachurada e moldura tracejada")]
    [Tooltip("Distância entre o início de uma faixa (ou traço) e o da seguinte.")]
    public float periodo = 14f;
    [Tooltip("Espessura da faixa na perpendicular, ou comprimento do traço da moldura.")]
    public float larguraFaixa = 6f;

    [Header("Suavização")]
    [Tooltip("Largura da franja de alfa nas bordas, em unidades do canvas. 0 desliga.")]
    public float suavizacao = 1f;

    /// <summary>Cria uma forma como filha de <paramref name="pai"/>, esticada nele. Não recebe toques.</summary>
    public static FormaUI Criar(Transform pai, Modo modo, Color cor, string nome = "Forma")
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(FormaUI));
        go.transform.SetParent(pai, false);
        var f = go.GetComponent<FormaUI>();
        f.modo = modo; f.color = cor; f.raycastTarget = false;
        var rt = f.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        return f;
    }

    /// <summary>Chanfrado já configurado. <paramref name="cantos"/> usa as letras SE, SD, IE, ID (ex.: "SE ID").</summary>
    public static FormaUI Chanfrado(Transform pai, Color cor, float chanfro, string cantos = "SE ID", string nome = "Chanfrado")
    {
        var f = Criar(pai, Modo.Chanfrado, cor, nome);
        f.chanfro = chanfro;
        f.cantoSE = cantos.Contains("SE"); f.cantoSD = cantos.Contains("SD");
        f.cantoIE = cantos.Contains("IE"); f.cantoID = cantos.Contains("ID");
        return f;
    }

    public void Redesenhar() => SetVerticesDirty();

    // ---------------------------------------------------------------------------------------------------------

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();
        if (r.width <= 0f || r.height <= 0f) return;
        Color32 cor = color;
        switch (modo)
        {
            case Modo.Chanfrado: Preencher(vh, PontosChanfrado(r), cor, true); break;
            case Modo.Poligono: Preencher(vh, PontosPoligono(r, Mathf.Max(3, lados), irregularidade, rotacao + 90f), cor, true); break;
            case Modo.Triangulo: Preencher(vh, PontosPoligono(r, 3, 0f, rotacao + 90f), cor, true); break;
            case Modo.Anel: DesenharAnel(vh, r, cor); break;
            case Modo.Hachurada: DesenharHachurada(vh, r, cor); break;
            case Modo.Moldura: DesenharMoldura(vh, r, cor); break;
        }
    }

    // ---------- Pontos de cada forma (sempre anti-horário, y para cima) ----------

    List<Vector2> PontosChanfrado(Rect r)
    {
        float c = Mathf.Clamp(chanfro, 0f, Mathf.Min(r.width, r.height) * 0.5f);
        float x0 = r.xMin, x1 = r.xMax, y0 = r.yMin, y1 = r.yMax;
        var p = new List<Vector2>(8);
        // Parte da base, andando para a direita.
        if (cantoIE) { p.Add(new Vector2(x0, y0 + c)); p.Add(new Vector2(x0 + c, y0)); } else p.Add(new Vector2(x0, y0));
        if (cantoID) { p.Add(new Vector2(x1 - c, y0)); p.Add(new Vector2(x1, y0 + c)); } else p.Add(new Vector2(x1, y0));
        if (cantoSD) { p.Add(new Vector2(x1, y1 - c)); p.Add(new Vector2(x1 - c, y1)); } else p.Add(new Vector2(x1, y1));
        if (cantoSE) { p.Add(new Vector2(x0 + c, y1)); p.Add(new Vector2(x0, y1 - c)); } else p.Add(new Vector2(x0, y1));
        return p;
    }

    List<Vector2> PontosPoligono(Rect r, int n, float irreg, float grausInicio)
    {
        var rnd = new System.Random(semente);
        var p = new List<Vector2>(n);
        Vector2 c = r.center;
        float rx = r.width * 0.5f, ry = r.height * 0.5f;
        for (int k = 0; k < n; k++)
        {
            float a = (grausInicio + 360f * k / n) * Mathf.Deg2Rad + (irreg > 0f ? (float)(rnd.NextDouble() - 0.5) * 0.35f : 0f);
            float f = 1f - irreg * (float)rnd.NextDouble();
            p.Add(c + new Vector2(Mathf.Cos(a) * rx * f, Mathf.Sin(a) * ry * f));
        }
        return p;
    }

    // ---------- Desenho ----------

    static Color32 SemAlfa(Color32 c) => new Color32(c.r, c.g, c.b, 0);

    /// <summary>Preenche um polígono em leque a partir do centro, com franja de alfa opcional.</summary>
    void Preencher(VertexHelper vh, List<Vector2> p, Color32 cor, bool franja)
    {
        int n = p.Count;
        if (n < 3) return;
        float area = 0f;
        for (int i = 0; i < n; i++) { var a = p[i]; var b = p[(i + 1) % n]; area += a.x * b.y - b.x * a.y; }
        if (area < 0f) p.Reverse();
        Vector2 centro = Vector2.zero;
        foreach (var v in p) centro += v;
        centro /= n;

        int b0 = vh.currentVertCount;
        vh.AddVert(centro, cor, Vector2.zero);
        for (int i = 0; i < n; i++) vh.AddVert(p[i], cor, Vector2.zero);
        for (int i = 0; i < n; i++) vh.AddTriangle(b0, b0 + 1 + i, b0 + 1 + (i + 1) % n);

        if (!franja || suavizacao <= 0f) return;
        int o0 = vh.currentVertCount;
        var sem = SemAlfa(cor);
        for (int i = 0; i < n; i++)
        {
            Vector2 antes = p[(i + n - 1) % n], depois = p[(i + 1) % n];
            Vector2 e1 = (p[i] - antes).normalized, e2 = (depois - p[i]).normalized;
            Vector2 n1 = new Vector2(e1.y, -e1.x), n2 = new Vector2(e2.y, -e2.x); // para fora, em polígono anti-horário
            Vector2 nm = n1 + n2;
            nm = nm.sqrMagnitude < 1e-6f ? n1 : nm.normalized;
            float k = suavizacao / Mathf.Max(0.5f, Vector2.Dot(nm, n1));
            vh.AddVert(p[i] + nm * k, sem, Vector2.zero);
        }
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            vh.AddTriangle(b0 + 1 + i, o0 + i, o0 + j);
            vh.AddTriangle(b0 + 1 + i, o0 + j, b0 + 1 + j);
        }
    }

    void DesenharAnel(VertexHelper vh, Rect r, Color32 cor)
    {
        float f = Mathf.Clamp01(fracao);
        if (f <= 0f) return;
        float ext = Mathf.Min(r.width, r.height) * 0.5f;
        float inte = Mathf.Max(0f, ext - Mathf.Max(1f, espessura));
        Vector2 c = r.center;
        int seg = Mathf.Max(6, Mathf.CeilToInt(72 * f));
        float a0 = 90f * Mathf.Deg2Rad, total = -360f * Mathf.Deg2Rad * f; // do topo, sentido horário
        var sem = SemAlfa(cor);
        float s = suavizacao;
        int b0 = vh.currentVertCount;
        // por ponto: 0 interno, 1 externo, 2 franja externa, 3 franja interna
        for (int i = 0; i <= seg; i++)
        {
            float a = a0 + total * i / seg;
            Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            vh.AddVert(c + d * inte, cor, Vector2.zero);
            vh.AddVert(c + d * ext, cor, Vector2.zero);
            vh.AddVert(c + d * (ext + s), sem, Vector2.zero);
            vh.AddVert(c + d * Mathf.Max(0f, inte - s), sem, Vector2.zero);
        }
        for (int i = 0; i < seg; i++)
        {
            int a = b0 + i * 4, b = b0 + (i + 1) * 4;
            vh.AddTriangle(a, a + 1, b + 1); vh.AddTriangle(a, b + 1, b);
            if (s <= 0f) continue;
            vh.AddTriangle(a + 1, a + 2, b + 2); vh.AddTriangle(a + 1, b + 2, b + 1);
            if (inte > 0f) { vh.AddTriangle(a + 3, a, b); vh.AddTriangle(a + 3, b, b + 3); }
        }
    }

    /// <summary>Corta um polígono convexo mantendo o lado onde dot(n, p) >= c (Sutherland-Hodgman, um plano).</summary>
    static List<Vector2> Recortar(List<Vector2> p, Vector2 n, float c)
    {
        var saida = new List<Vector2>(p.Count + 2);
        for (int i = 0; i < p.Count; i++)
        {
            Vector2 a = p[i], b = p[(i + 1) % p.Count];
            float da = Vector2.Dot(n, a) - c, db = Vector2.Dot(n, b) - c;
            if (da >= 0f) saida.Add(a);
            if ((da >= 0f) != (db >= 0f)) saida.Add(a + (b - a) * (da / (da - db)));
        }
        return saida;
    }

    void DesenharHachurada(VertexHelper vh, Rect r, Color32 cor)
    {
        float p = Mathf.Max(2f, periodo), w = Mathf.Clamp(larguraFaixa, 0.5f, p) * Mathf.Sqrt(2f); // w é a espessura na perpendicular
        var ret = new List<Vector2> { new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax) };
        // Faixas na direção (1,1): a região a <= x - y <= a + w.
        float inicio = Mathf.Floor((r.xMin - r.yMax) / (p * Mathf.Sqrt(2f))) * p * Mathf.Sqrt(2f);
        float fim = r.xMax - r.yMin;
        for (float a = inicio; a <= fim; a += p * Mathf.Sqrt(2f))
        {
            var poly = Recortar(ret, new Vector2(1f, -1f), a);
            poly = Recortar(poly, new Vector2(-1f, 1f), -(a + w));
            if (poly.Count >= 3) Preencher(vh, poly, cor, false);
        }
    }

    void DesenharMoldura(VertexHelper vh, Rect r, Color32 cor)
    {
        float e = Mathf.Max(1f, espessura), traco = Mathf.Max(2f, larguraFaixa), passo = Mathf.Max(traco + 2f, periodo);
        // quatro lados, com os traços alinhados nos cantos
        Traco(vh, cor, r.xMin, r.yMax - e, r.width, e, true, traco, passo);
        Traco(vh, cor, r.xMin, r.yMin, r.width, e, true, traco, passo);
        Traco(vh, cor, r.xMin, r.yMin + e, e, r.height - 2f * e, false, traco, passo);
        Traco(vh, cor, r.xMax - e, r.yMin + e, e, r.height - 2f * e, false, traco, passo);
    }

    static void Traco(VertexHelper vh, Color32 cor, float x, float y, float w, float h, bool horizontal, float traco, float passo)
    {
        float comp = horizontal ? w : h;
        int n = Mathf.Max(1, Mathf.RoundToInt(comp / passo));
        float p = comp / n, t = Mathf.Min(traco, p - 2f);
        for (int i = 0; i < n; i++)
        {
            float d = i * p + (p - t) * 0.5f;
            float qx = horizontal ? x + d : x, qy = horizontal ? y : y + d;
            float qw = horizontal ? t : w, qh = horizontal ? h : t;
            int b = vh.currentVertCount;
            vh.AddVert(new Vector2(qx, qy), cor, Vector2.zero);
            vh.AddVert(new Vector2(qx + qw, qy), cor, Vector2.zero);
            vh.AddVert(new Vector2(qx + qw, qy + qh), cor, Vector2.zero);
            vh.AddVert(new Vector2(qx, qy + qh), cor, Vector2.zero);
            vh.AddTriangle(b, b + 1, b + 2); vh.AddTriangle(b, b + 2, b + 3);
        }
    }
}
