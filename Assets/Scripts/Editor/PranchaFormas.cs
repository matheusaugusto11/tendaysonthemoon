using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tarefa 2.3 (interface-e-minijogos): prancha de formas do FormaUI, para conferir cada modo em três tamanhos e a nitidez das
/// arestas em 1920 por 1080 e em 1280 por 720. Menu "Dez Dias/Prancha de formas", ou o arquivo Temp/pedido_prancha_formas.txt.
/// Renderiza fora do Play, numa textura, e grava em Docs/05-UI/prancha-formas.png e prancha-formas-720.png.
/// </summary>
public static class PranchaFormas
{
    [MenuItem("Dez Dias/Prancha de formas")]
    public static void Gerar()
    {
        Directory.CreateDirectory("Docs/05-UI");
        Renderizar(1920, 1080, "Docs/05-UI/prancha-formas.png");
        Renderizar(1280, 720, "Docs/05-UI/prancha-formas-720.png");
        Debug.Log("[PranchaFormas] Prancha gerada em Docs/05-UI.");
    }

    static void Renderizar(int w, int h, string saida)
    {
        var raiz = new GameObject("PranchaFormas_tmp");
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
        try
        {
            int ui = LayerMask.NameToLayer("UI");
            var camGo = new GameObject("Cam", typeof(Camera));
            camGo.transform.SetParent(raiz.transform);
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = UiTema.Pagina;
            cam.cullingMask = 1 << ui; cam.targetTexture = rt; cam.enabled = false;

            var cvGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            cvGo.transform.SetParent(raiz.transform);
            var canvas = cvGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 1f;
            var sc = cvGo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; sc.scaleFactor = w / 1920f; // 1280 de largura = 2/3 do referencial
            var pai = (RectTransform)cvGo.transform;

            Montar(pai);
            SetLayer(raiz.transform, ui);
            Canvas.ForceUpdateCanvases();
            cam.Render();

            var antes = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            RenderTexture.active = antes;
            File.WriteAllBytes(saida, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
        finally
        {
            rt.Release(); Object.DestroyImmediate(rt);
            Object.DestroyImmediate(raiz);
        }
    }

    static void SetLayer(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform f in t) SetLayer(f, layer);
    }

    // ---------- Conteúdo (coordenadas do referencial, y para baixo a partir do canto superior esquerdo) ----------

    static RectTransform Caixa(Transform pai, string nome, float x, float y, float w, float h)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static void Rotulo(Transform pai, string s, float x, float y, Color? cor = null)
    {
        var rt = Caixa(pai, "Rotulo", x, y, 900, 30);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = UiTema.BarlowSemi; t.fontSize = 24; t.color = cor ?? UiTema.Apoio; t.text = UiTema.Caixa(s);
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.raycastTarget = false;
    }

    static void Montar(RectTransform pai)
    {
        var fundo = FormaUI.Criar(pai, FormaUI.Modo.Chanfrado, UiTema.Aco, "Fundo");
        fundo.chanfro = 0; fundo.cantoSE = fundo.cantoID = false;
        Rotulo(pai, "FormaUI · prancha de formas · cada modo em três tamanhos", 48, 28, UiTema.Branco);

        // 1) Chanfrado: três tamanhos com a assinatura (SE ID) e as outras combinações de cantos.
        Rotulo(pai, "Chanfrado", 48, 84);
        float x = 48;
        foreach (var (w, h) in new[] { (560f, 100f), (280f, 90f), (120f, 56f) })
        {
            FormaUI.Chanfrado(Caixa(pai, "C", x, 120, w, h), UiTema.AcoClaro, UiTema.ChanfroPainel, "SE ID");
            x += w + 28;
        }
        foreach (var cantos in new[] { "SE SD IE ID", "SD IE", "" })
        {
            FormaUI.Chanfrado(Caixa(pai, "C", x, 120, 90, 90), UiTema.Branco, 18, cantos);
            x += 110;
        }

        // 2) Polígono: manchas de regolito e pedras, com sementes diferentes.
        Rotulo(pai, "Polígono (irregularidade 0,25 · sementes 1 a 6)", 48, 252);
        x = 48;
        foreach (var (d, lados, seed, cor) in new[] { (200f, 7, 1, "#9B98A3"), (120f, 7, 2, "#BCB9C4"), (64f, 6, 3, "#9B98A3"), (200f, 5, 4, "#6E6B79"), (120f, 8, 5, "#9B98A3"), (64f, 7, 6, "#BCB9C4") })
        {
            var f = FormaUI.Criar(Caixa(pai, "P", x, 288, d, d), FormaUI.Modo.Poligono, Cor(cor), "Poligono");
            f.lados = lados; f.semente = seed;
            x += d + 28;
        }

        // 3) Anel e arco.
        Rotulo(pai, "Anel e arco (frações 0,25 · 0,6 · 1)", 48, 520);
        x = 48;
        foreach (var (d, esp, fr, cor) in new[] { (200f, 20f, 0.25f, UiTema.Ar), (120f, 14f, 0.6f, UiTema.Branco), (64f, 8f, 1f, UiTema.Energia) })
        {
            var tr = Caixa(pai, "A", x, 556, d, d);
            var fundoAnel = FormaUI.Criar(tr, FormaUI.Modo.Anel, UiTema.Vazio, "Trilho"); fundoAnel.espessura = esp;
            var a = FormaUI.Criar(tr, FormaUI.Modo.Anel, cor, "Arco"); a.espessura = esp; a.fracao = fr;
            x += d + 28;
        }

        // 4) Triângulo nas quatro direções e em três tamanhos.
        Rotulo(pai, "Triângulo (0° · 90° · 180° · 270°)", 560, 520);
        x = 560;
        foreach (var (d, rot, cor) in new[] { (96f, 0f, UiTema.Branco), (96f, 180f, UiTema.Alerta), (64f, 90f, UiTema.Branco), (64f, 270f, UiTema.Branco), (36f, 0f, UiTema.Energia), (36f, 180f, UiTema.Energia) })
        {
            var t = FormaUI.Criar(Caixa(pai, "T", x, 556, d, d), FormaUI.Modo.Triangulo, cor, "Triangulo"); t.rotacao = rot;
            x += d + 28;
        }

        // 5) Faixa hachurada (zona de alerta): três tamanhos.
        Rotulo(pai, "Faixa hachurada", 48, 780);
        x = 48;
        foreach (var (w, h, cor) in new[] { (420f, 64f, UiTema.Alerta), (210f, 40f, UiTema.Alerta), (100f, 28f, UiTema.Energia) })
        {
            var tr = Caixa(pai, "H", x, 816, w, h);
            FormaUI.Criar(tr, FormaUI.Modo.Chanfrado, UiTema.Vazio, "Fundo").chanfro = 0;
            var f = FormaUI.Criar(tr, FormaUI.Modo.Hachurada, cor, "Hachura"); f.larguraFaixa = 6; f.periodo = 14;
            x += w + 28;
        }

        // 6) Moldura tracejada (foco).
        Rotulo(pai, "Moldura tracejada (foco)", 48, 912);
        x = 48;
        foreach (var (w, h) in new[] { (420f, 88f), (210f, 64f), (100f, 48f) })
        {
            var f = FormaUI.Criar(Caixa(pai, "M", x, 948, w, h), FormaUI.Modo.Moldura, UiTema.Branco, "Moldura");
            f.espessura = 4; f.larguraFaixa = 14; f.periodo = 24;
            x += w + 28;
        }
    }

    static Color Cor(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
}
