using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Analógico virtual. No modo flutuante, este objeto é uma área invisível que recebe o toque: o círculo (fundo)
/// aparece onde o dedo encostou, a bolinha segue o dedo e tudo some ao soltar. No modo fixo, o círculo fica sempre
/// na tela. Serve para andar (metade esquerda) e para girar a câmera (metade direita). Multitoque via EventSystem.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform botao;          // bolinha interna
    [Tooltip("Círculo de fundo. Vazio = este próprio objeto (modo fixo).")]
    public RectTransform fundo;
    [Tooltip("O círculo aparece onde o dedo tocar e some ao soltar.")]
    public bool flutuante;
    public float raio = 120f;            // alcance da bolinha (unidades do canvas)
    [Range(0f, 0.5f)] public float zonaMorta = 0.1f;

    /// <summary>Valor entre -1 e 1 em cada eixo (x = direita, y = cima).</summary>
    public Vector2 Valor { get; private set; }

    RectTransform _area;
    int _pointerId = int.MinValue;
    float _ultimoToque = -9f;
    bool _duploToque;

    /// <summary>Dois toques rápidos nesta área (recentrar a câmera). Zera ao ler.</summary>
    public bool ConsumirDuploToque() { bool d = _duploToque; _duploToque = false; return d; }

    void Awake()
    {
        _area = (RectTransform)transform;
        if (!fundo) fundo = _area;
        MostrarFundo(!flutuante);
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (_pointerId != int.MinValue) return;
        _pointerId = e.pointerId;
        if (Time.unscaledTime - _ultimoToque < 0.3f) _duploToque = true;
        _ultimoToque = Time.unscaledTime;

        if (flutuante && fundo != _area &&
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_area, e.position, e.pressEventCamera, out var mundo))
        {
            fundo.position = mundo;
            MostrarFundo(true);
        }
        Atualizar(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId == _pointerId) Atualizar(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != _pointerId) return;
        Soltar();
    }

    void OnDisable() => Soltar();

    void Soltar()
    {
        _pointerId = int.MinValue;
        Valor = Vector2.zero;
        if (botao) botao.anchoredPosition = Vector2.zero;
        if (flutuante) MostrarFundo(false);
    }

    void MostrarFundo(bool v)
    {
        if (fundo && fundo != _area && fundo.gameObject.activeSelf != v) fundo.gameObject.SetActive(v);
    }

    void Atualizar(PointerEventData e)
    {
        // Posição do dedo relativa ao centro do círculo (pivô no centro).
        RectTransformUtility.ScreenPointToLocalPointInRectangle(fundo, e.position, e.pressEventCamera, out var local);
        Vector2 delta = Vector2.ClampMagnitude(local, raio);
        if (botao) botao.anchoredPosition = delta;

        Vector2 v = delta / raio;
        float mag = v.magnitude;
        Valor = mag < zonaMorta ? Vector2.zero : v.normalized * ((mag - zonaMorta) / (1f - zonaMorta));
    }
}
