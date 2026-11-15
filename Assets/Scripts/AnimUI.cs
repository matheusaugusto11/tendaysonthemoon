using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retornos curtos dos minijogos (design D9): pulso de escala e texto que sobe e some. Usam o tempo real, para a pausa
/// não prender nada na tela. Com movimento reduzido o pulso não acontece e o texto só some, sem subir.
/// </summary>
public static class AnimUI
{
    /// <summary>Escala de 1 para <paramref name="escala"/> e de volta, em <paramref name="duracao"/> segundos (120 ms no padrão).</summary>
    public static void Pulso(Transform alvo, float escala = 1.08f, float duracao = 0.12f)
    {
        if (alvo == null || UiTema.MovimentoReduzido) return;
        var p = alvo.GetComponent<Pulsar>();
        if (p == null) p = alvo.gameObject.AddComponent<Pulsar>();
        p.Comecar(escala, duracao);
    }

    /// <summary>Texto que sobe 40 e some em 0,8 s, na posição <paramref name="pos"/> do <paramref name="pai"/>.</summary>
    public static Text Flutua(Transform pai, string texto, Vector2 pos, Color cor)
    {
        var go = new GameObject("Flutua", typeof(RectTransform), typeof(Text), typeof(Flutuante));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(260, 60);
        rt.anchoredPosition = pos;
        var t = go.GetComponent<Text>();
        t.font = UiTema.Fonte(Papel.Numero, texto); t.fontSize = UiTema.Tamanho(Papel.Numero);
        t.text = texto; t.color = cor; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        go.AddComponent<Shadow>().effectDistance = new Vector2(0, -3);
        go.GetComponent<Flutuante>().Iniciar(pos);
        return t;
    }
}

/// <summary>Pulso de escala de um objeto (ver <see cref="AnimUI.Pulso"/>).</summary>
public class Pulsar : MonoBehaviour
{
    float _t0, _dur, _escala;
    bool _ativo;

    public void Comecar(float escala, float duracao)
    {
        _t0 = Time.unscaledTime; _dur = Mathf.Max(0.01f, duracao); _escala = escala; _ativo = true;
    }

    void Update()
    {
        if (!_ativo) return;
        float k = (Time.unscaledTime - _t0) / _dur;
        if (k >= 1f) { transform.localScale = Vector3.one; _ativo = false; return; }
        transform.localScale = Vector3.one * (1f + (_escala - 1f) * Mathf.Sin(k * Mathf.PI));
    }

    void OnDisable() { transform.localScale = Vector3.one; _ativo = false; }
}

/// <summary>Texto flutuante: sobe e some (ver <see cref="AnimUI.Flutua"/>).</summary>
public class Flutuante : MonoBehaviour
{
    public const float Duracao = 0.8f, Subida = 40f;
    float _t0;
    Vector2 _pos;
    Text _t;

    public void Iniciar(Vector2 pos) { _pos = pos; _t0 = Time.unscaledTime; _t = GetComponent<Text>(); }

    void Update()
    {
        float k = (Time.unscaledTime - _t0) / Duracao;
        if (k >= 1f) { Destroy(gameObject); return; }
        float sobe = UiTema.MovimentoReduzido ? 0f : Subida * (1f - (1f - k) * (1f - k));
        ((RectTransform)transform).anchoredPosition = _pos + Vector2.up * sobe;
        var c = _t.color; c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f; _t.color = c;
    }
}
