using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Joystick virtual na tela. Funciona com toque e com mouse (arrastar).</summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform handle;
    public float radius = 70f;

    /// <summary>Valor entre -1 e 1 em cada eixo.</summary>
    public Vector2 Value { get; private set; }

    RectTransform _rect;

    void Awake() => _rect = (RectTransform)transform;

    public void OnPointerDown(PointerEventData e) => OnDrag(e);

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, e.position, e.pressEventCamera, out var local);
        Vector2 clamped = Vector2.ClampMagnitude(local, radius);
        if (handle) handle.anchoredPosition = clamped;
        Value = clamped / radius;
    }

    public void OnPointerUp(PointerEventData e)
    {
        Value = Vector2.zero;
        if (handle) handle.anchoredPosition = Vector2.zero;
    }

    /// <summary>Cria um joystick pronto (Canvas + EventSystem) no canto inferior esquerdo.</summary>
    public static VirtualJoystick Create()
    {
        var canvasGo = new GameObject("JoystickCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var baseGo = new GameObject("Joystick", typeof(Image), typeof(VirtualJoystick));
        baseGo.transform.SetParent(canvasGo.transform, false);
        var baseRect = (RectTransform)baseGo.transform;
        baseRect.anchorMin = baseRect.anchorMax = baseRect.pivot = Vector2.zero;
        baseRect.sizeDelta = new Vector2(300, 300);
        baseRect.anchoredPosition = new Vector2(80, 80);
        baseGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.25f);

        var knobGo = new GameObject("Handle", typeof(Image));
        knobGo.transform.SetParent(baseGo.transform, false);
        var knob = (RectTransform)knobGo.transform;
        knob.anchorMin = knob.anchorMax = knob.pivot = new Vector2(0.5f, 0.5f);
        knob.sizeDelta = new Vector2(120, 120);
        knobGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.7f);
        knobGo.GetComponent<Image>().raycastTarget = false;

        // Centro do RectTransform base é o pivô para o cálculo local.
        baseRect.pivot = new Vector2(0.5f, 0.5f);
        baseRect.anchoredPosition = new Vector2(80 + 150, 80 + 150);

        var joy = baseGo.GetComponent<VirtualJoystick>();
        joy.handle = knob;
        joy.radius = 90f;
        return joy;
    }
}
