using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Analógico virtual fixo (círculo na tela). Serve para andar (esquerdo) e para girar a câmera (direito).
/// Multitoque via EventSystem; funciona com mouse no editor.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform botao;          // bolinha interna
    public float raio = 120f;            // alcance da bolinha (unidades do canvas)
    [Range(0f, 0.5f)] public float zonaMorta = 0.1f;

    /// <summary>Valor entre -1 e 1 em cada eixo (x = direita, y = cima).</summary>
    public Vector2 Valor { get; private set; }

    RectTransform _base;
    int _pointerId = int.MinValue;

    void Awake() => _base = (RectTransform)transform;

    public void OnPointerDown(PointerEventData e)
    {
        if (_pointerId != int.MinValue) return;
        _pointerId = e.pointerId;
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
    }

    void Atualizar(PointerEventData e)
    {
        // Posição do dedo relativa ao centro da base (pivô no centro).
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_base, e.position, e.pressEventCamera, out var local);
        Vector2 delta = Vector2.ClampMagnitude(local, raio);
        if (botao) botao.anchoredPosition = delta;

        Vector2 v = delta / raio;
        float mag = v.magnitude;
        Valor = mag < zonaMorta ? Vector2.zero : v.normalized * ((mag - zonaMorta) / (1f - zonaMorta));
    }
}
