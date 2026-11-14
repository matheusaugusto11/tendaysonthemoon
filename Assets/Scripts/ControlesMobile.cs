using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Cria o Canvas de controles de toque: analógico esquerdo (andar), analógico direito (câmera)
/// e uma zona de arrasto na metade direita (alternativa para girar a câmera).
/// </summary>
public static class ControlesMobile
{
    static Sprite _circulo;

    public static void Criar(out VirtualJoystick andar, out VirtualJoystick camera, out TouchLook look)
    {
        var canvasGo = new GameObject("ControlesCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Entrada.GarantirEventSystem();

        // Arrastar na metade direita (por baixo dos analógicos)
        var zonaDir = NovaZona(canvasGo.transform, "ZonaCamera", new Vector2(0.5f, 0), new Vector2(1, 1));
        look = zonaDir.gameObject.AddComponent<TouchLook>();

        // Analógicos dentro da área segura (entalhe/cantos do celular)
        var seguro = new GameObject("AreaSegura", typeof(RectTransform), typeof(SafeArea));
        seguro.transform.SetParent(canvasGo.transform, false);
        var rs = (RectTransform)seguro.transform;
        rs.anchorMin = Vector2.zero; rs.anchorMax = Vector2.one; rs.offsetMin = rs.offsetMax = Vector2.zero;

        andar = NovoAnalogico(seguro.transform, "AnalogicoAndar", new Vector2(0, 0), new Vector2(280, 280));
        camera = NovoAnalogico(seguro.transform, "AnalogicoCamera", new Vector2(1, 0), new Vector2(-280, 280));
    }

    static VirtualJoystick NovoAnalogico(Transform pai, string nome, Vector2 ancora, Vector2 pos)
    {
        var baseGo = new GameObject(nome, typeof(Image));
        baseGo.transform.SetParent(pai, false);
        var rb = (RectTransform)baseGo.transform;
        rb.anchorMin = rb.anchorMax = ancora;
        rb.pivot = new Vector2(0.5f, 0.5f);
        rb.sizeDelta = new Vector2(340, 340);
        rb.anchoredPosition = pos;
        var ib = baseGo.GetComponent<Image>();
        ib.sprite = Circulo();
        ib.color = new Color(1, 1, 1, 0.22f);
        ib.raycastTarget = true;

        var knobGo = new GameObject("Botao", typeof(Image));
        knobGo.transform.SetParent(baseGo.transform, false);
        var rk = (RectTransform)knobGo.transform;
        rk.anchorMin = rk.anchorMax = rk.pivot = new Vector2(0.5f, 0.5f);
        rk.sizeDelta = new Vector2(140, 140);
        var ik = knobGo.GetComponent<Image>();
        ik.sprite = Circulo();
        ik.color = new Color(1, 1, 1, 0.75f);
        ik.raycastTarget = false;

        var joy = baseGo.AddComponent<VirtualJoystick>();
        joy.botao = rk;
        joy.raio = 120f;
        return joy;
    }

    static RectTransform NovaZona(Transform pai, string nome, Vector2 min, Vector2 max)
    {
        var go = new GameObject(nome, typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0); // invisível, mas recebe toque
        img.raycastTarget = true;
        return rt;
    }

    /// <summary>Sprite de círculo branco com borda suave, gerado em código.</summary>
    public static Sprite Circulo()
    {
        if (_circulo != null) return _circulo;
        const int n = 128;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        float c = (n - 1) / 2f, r = n / 2f - 1f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = Mathf.Clamp01(r - d + 1f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        _circulo = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return _circulo;
    }
}
