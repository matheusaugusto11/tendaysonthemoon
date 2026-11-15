using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Zona da tela (metade direita) onde arrastar o dedo gira a câmera. Também funciona com o mouse.</summary>
public class TouchLook : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    int _pointerId = int.MinValue;
    Vector2 _acumulado;
    float _ultimoToque = -9f;
    bool _duploToque;

    /// <summary>Toque duplo na metade direita da tela (recentrar a câmera). Zera ao ler.</summary>
    public bool ConsumirDuploToque() { bool d = _duploToque; _duploToque = false; return d; }

    /// <summary>Devolve o arrasto (em pixels) desde a última chamada e zera.</summary>
    public Vector2 ConsumirDelta()
    {
        var d = _acumulado;
        _acumulado = Vector2.zero;
        return d;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (Time.unscaledTime - _ultimoToque < 0.3f) _duploToque = true;
        _ultimoToque = Time.unscaledTime;
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
