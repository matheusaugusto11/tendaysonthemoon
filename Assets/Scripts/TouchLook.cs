using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Zona da tela (metade direita) onde arrastar o dedo gira a câmera. Também funciona com o mouse.</summary>
public class TouchLook : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    int _pointerId = int.MinValue;
    Vector2 _acumulado;

    /// <summary>Devolve o arrasto (em pixels) desde a última chamada e zera.</summary>
    public Vector2 ConsumirDelta()
    {
        var d = _acumulado;
        _acumulado = Vector2.zero;
        return d;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (_pointerId == int.MinValue) _pointerId = e.pointerId;
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId == _pointerId) _acumulado += e.delta;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId == _pointerId) _pointerId = int.MinValue;
    }

    void OnDisable()
    {
        _pointerId = int.MinValue;
        _acumulado = Vector2.zero;
    }
}
