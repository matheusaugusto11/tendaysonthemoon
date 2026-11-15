using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Entrada unificada: teclado/mouse/toque + controle (Xbox: A confirma, B volta, Start pausa).
/// Os controles de gamepad exigem o pacote Input System com "Active Input Handling = Both".
/// </summary>
public static class Entrada
{
    const float ZonaMorta = 0.15f;

    /// <summary>Movimento: WASD/setas + analógico esquerdo.</summary>
    public static Vector2 Mover
    {
        get
        {
            var v = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null) v += ComZonaMorta(g.leftStick.ReadValue());
#endif
            return v;
        }
    }

    /// <summary>Câmera: analógico direito (−1..1).</summary>
    public static Vector2 Camera
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null) return ComZonaMorta(g.rightStick.ReadValue());
#endif
            return Vector2.zero;
        }
    }

    /// <summary>A (Xbox) ou E: interagir/confirmar durante o jogo.</summary>
    public static bool ConfirmarDown
    {
        get
        {
            bool r = Input.GetKeyDown(KeyCode.E);
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null && g.buttonSouth.wasPressedThisFrame) r = true;
#endif
            return r;
        }
    }

    /// <summary>B (Xbox) ou Backspace: voltar.</summary>
    public static bool VoltarDown
    {
        get
        {
            bool r = Input.GetKeyDown(KeyCode.Backspace);
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null && g.buttonEast.wasPressedThisFrame) r = true;
#endif
            return r;
        }
    }

    /// <summary>Start (Xbox), Esc ou P: pausar/continuar.</summary>
    public static bool PausaDown
    {
        get
        {
            bool r = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null && g.startButton.wasPressedThisFrame) r = true;
#endif
            return r;
        }
    }

    /// <summary>R ou clique no analógico direito (R3): põe a câmera nas costas do personagem.</summary>
    public static bool RecentrarDown
    {
        get
        {
            bool r = Input.GetKeyDown(KeyCode.R);
#if ENABLE_INPUT_SYSTEM
            var g = Gamepad.current;
            if (g != null && g.rightStickButton.wasPressedThisFrame) r = true;
#endif
            return r;
        }
    }

    static Vector2 ComZonaMorta(Vector2 v) => v.magnitude < ZonaMorta ? Vector2.zero : v;

    /// <summary>Garante um EventSystem com o módulo certo (Input System se disponível: toque, mouse, gamepad).</summary>
    public static void GarantirEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        var modulo = go.AddComponent<InputSystemUIInputModule>();
        modulo.AssignDefaultActions(); // Navegar = D-pad/analógico, Confirmar = A, Cancelar = B
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    /// <summary>Seleciona um botão para o controle poder navegar (null = nenhum).</summary>
    public static void Selecionar(GameObject go)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(go);
    }
}
