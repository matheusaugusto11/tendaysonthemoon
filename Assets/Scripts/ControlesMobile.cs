using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controles de toque, só em celular e tablet. A metade esquerda da tela anda e a metade direita gira a câmera.
/// Os analógicos ficam escondidos: cada um aparece onde o dedo encostar e some ao soltar.
/// No desktop nada disso é criado (teclado, mouse e controle cuidam).
/// </summary>
public static class ControlesMobile
{
    public enum Modo { Automatico, SempreToque, NuncaToque }

    /// <summary>Para testar no editor: force o toque (SempreToque) ou o desktop (NuncaToque).</summary>
    public static Modo modo = Modo.Automatico;

    static Sprite _circulo;

    /// <summary>O jogo está num celular ou tablet (na WebGL, navegador de celular ou tela de toque sem mouse).</summary>
    public static bool UsarToque
    {
        get
        {
            if (modo == Modo.SempreToque) return true;
            if (modo == Modo.NuncaToque) return false;
            return Application.isMobilePlatform || Navegador.TelaDeToque;
        }
    }

    public static void Criar(out VirtualJoystick andar, out VirtualJoystick camera)
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

        andar = NovoAnalogico(canvasGo.transform, "ZonaAndar", new Vector2(0, 0), new Vector2(0.5f, 1));
        camera = NovoAnalogico(canvasGo.transform, "ZonaCamera", new Vector2(0.5f, 0), new Vector2(1, 1));
    }

    /// <summary>Área invisível (metade da tela) que recebe o toque, com o analógico escondido dentro.</summary>
    static VirtualJoystick NovoAnalogico(Transform pai, string nome, Vector2 min, Vector2 max)
    {
        var zona = NovaZona(pai, nome, min, max);

        var baseGo = new GameObject("Analogico", typeof(Image));
        baseGo.transform.SetParent(zona, false);
        var rb = (RectTransform)baseGo.transform;
        rb.anchorMin = rb.anchorMax = rb.pivot = new Vector2(0.5f, 0.5f);
        rb.sizeDelta = new Vector2(300, 300);
        var ib = baseGo.GetComponent<Image>();
        ib.sprite = Circulo();
        ib.color = new Color(1, 1, 1, 0.22f);
        ib.raycastTarget = false;

        var knobGo = new GameObject("Botao", typeof(Image));
        knobGo.transform.SetParent(baseGo.transform, false);
        var rk = (RectTransform)knobGo.transform;
        rk.anchorMin = rk.anchorMax = rk.pivot = new Vector2(0.5f, 0.5f);
        rk.sizeDelta = new Vector2(130, 130);
        var ik = knobGo.GetComponent<Image>();
        ik.sprite = Circulo();
        ik.color = new Color(1, 1, 1, 0.75f);
        ik.raycastTarget = false;

        baseGo.SetActive(false);
        var joy = zona.gameObject.AddComponent<VirtualJoystick>();
        joy.fundo = rb;
        joy.botao = rk;
        joy.flutuante = true;
        joy.raio = 110f;
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
