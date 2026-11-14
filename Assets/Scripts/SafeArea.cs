using UnityEngine;

/// <summary>Ajusta este RectTransform à área segura da tela (entalhe do iPhone, cantos arredondados, barra de gestos).</summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    Rect _ultima;
    Vector2Int _tela;

    void Update()
    {
        if (Screen.safeArea == _ultima && _tela.x == Screen.width && _tela.y == Screen.height) return;
        _ultima = Screen.safeArea;
        _tela = new Vector2Int(Screen.width, Screen.height);
        if (Screen.width <= 0 || Screen.height <= 0) return;

        var rt = (RectTransform)transform;
        Vector2 min = _ultima.position, max = _ultima.position + _ultima.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
