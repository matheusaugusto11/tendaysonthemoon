using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Ponteiro unificado para os minigames: mouse/toque OU cursor virtual movido pelo analógico esquerdo
/// (botão A = clicar/segurar). Posição em pixels de tela.
/// </summary>
public static class Ponteiro
{
    public static Vector2 Pos { get; private set; }
    public static bool Down { get; private set; }
    public static bool Held { get; private set; }
    public static bool Up { get; private set; }
    /// <summary>true quando o controle está guiando (mostra o cursor virtual).</summary>
    public static bool ModoCursor { get; private set; }
    /// <summary>Quanto o ponteiro andou neste frame (pixels).</summary>
    public static float Movimento { get; private set; }

    static bool _antes;
    static Vector2 _mouseAntes;

    public static void Iniciar()
    {
        Pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        _antes = false; Down = Up = Held = false; Movimento = 0f;
        _mouseAntes = Input.mousePosition;
    }

    public static void Atualizar()
    {
        Vector2 mouse = Input.mousePosition;
        Vector2 stick = Vector2.zero;
        bool padA = false;
#if ENABLE_INPUT_SYSTEM
        var g = Gamepad.current;
        if (g != null)
        {
            stick = g.leftStick.ReadValue();
            if (stick.magnitude < 0.15f) stick = Vector2.zero;
            padA = g.buttonSouth.isPressed;
        }
#endif
        if (stick != Vector2.zero || padA) ModoCursor = true;
        if ((mouse - _mouseAntes).sqrMagnitude > 4f || Input.GetMouseButton(0)) ModoCursor = false;
        _mouseAntes = mouse;

        bool segurando;
        Vector2 anterior = Pos;
        if (ModoCursor)
        {
            Pos += stick * Screen.height * 0.9f * Time.deltaTime;
            Pos = new Vector2(Mathf.Clamp(Pos.x, 0, Screen.width), Mathf.Clamp(Pos.y, 0, Screen.height));
            segurando = padA;
        }
        else
        {
            Pos = mouse;
            segurando = Input.GetMouseButton(0);
        }
        Movimento = (Pos - anterior).magnitude;
        Down = segurando && !_antes;
        Up = !segurando && _antes;
        Held = segurando;
        _antes = segurando;
    }
}
