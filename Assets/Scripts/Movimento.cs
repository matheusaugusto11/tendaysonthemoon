using UnityEngine;

/// <summary>
/// Move o personagem com WASD/setas + joystick virtual na tela.
/// A câmera orbita com o mouse (segure botão direito ou esquerdo fora do joystick) e segue o personagem.
/// O movimento é relativo à direção da câmera.
/// Troque o modelo depois: basta colocar o novo asset como filho deste objeto.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Movimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;
    public float velocidadeCorrida = 8f;
    public float suavizacaoRotacao = 12f;
    public float gravidade = -20f;
    public float forcaEmpurrar = 1.5f;

    [Header("Câmera")]
    public Transform cameraTransform;
    public float distancia = 6f;
    public float altura = 1.5f;
    public float sensibilidadeToque = 0.15f;   // graus por pixel arrastado
    public float sensibilidadeMouse = 3f;      // botão direito (só PC/editor)
    public Vector2 limitePitch = new Vector2(-10f, 70f);

    [Header("Controles de toque (criados automaticamente se vazios)")]
    public VirtualJoystick joystick;        // analógico esquerdo: andar
    public VirtualJoystick joystickCamera;  // analógico direito: câmera
    public TouchLook look;                  // arrastar na metade direita (extra)
    public float velocidadeCameraAnalogico = 140f; // graus por segundo com o analógico no máximo

    CharacterController _cc;
    float _yaw, _pitch = 20f, _velY;
    bool _congelado;

    /// <summary>Congela o personagem (fora da fase de Ação) e esconde os controles de toque.</summary>
    public void Congelar(bool congelar)
    {
        _congelado = congelar;
        if (joystick) joystick.transform.root.gameObject.SetActive(!congelar);
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        if (!joystick || !joystickCamera || !look) ControlesMobile.Criar(out joystick, out joystickCamera, out look);
        if (cameraTransform) _yaw = cameraTransform.eulerAngles.y;
        Congelar(_congelado);
    }

    void Update()
    {
        if (_congelado) return;
        LerCamera();
        Mover();
    }

    void LateUpdate() { AtualizarCamera(); }

    void LerCamera()
    {
        // Arrasto de dedo (ou mouse esquerdo) na metade direita da tela.
        Vector2 d = look ? look.ConsumirDelta() : Vector2.zero;
        _yaw += d.x * sensibilidadeToque;
        _pitch -= d.y * sensibilidadeToque;

        // Analógico direito: câmera contínua.
        if (joystickCamera)
        {
            Vector2 a = joystickCamera.Valor;
            _yaw += a.x * velocidadeCameraAnalogico * Time.deltaTime;
            _pitch -= a.y * velocidadeCameraAnalogico * Time.deltaTime;
        }

        // Analógico direito do controle (gamepad).
        Vector2 pad = Entrada.Camera;
        _yaw += pad.x * velocidadeCameraAnalogico * Time.deltaTime;
        _pitch -= pad.y * velocidadeCameraAnalogico * Time.deltaTime;

        // Fallback PC: botão direito do mouse em qualquer lugar.
        if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * sensibilidadeMouse;
            _pitch -= Input.GetAxis("Mouse Y") * sensibilidadeMouse;
        }
        _pitch = Mathf.Clamp(_pitch, limitePitch.x, limitePitch.y);
    }

    void Mover()
    {
        Vector2 input = Entrada.Mover;
        if (joystick) input += joystick.Valor;
        input = Vector2.ClampMagnitude(input, 1f);

        Quaternion yawRot = Quaternion.Euler(0, _yaw, 0);
        Vector3 dir = yawRot * new Vector3(input.x, 0, input.y);

        float vel = Input.GetKey(KeyCode.LeftShift) ? velocidadeCorrida : velocidade;
        Vector3 move = dir * vel;

        if (_cc.isGrounded && _velY < 0) _velY = -2f;
        _velY += gravidade * Time.deltaTime;
        move.y = _velY;
        _cc.Move(move * Time.deltaTime);

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion alvo = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, suavizacaoRotacao * Time.deltaTime);
        }
    }

    // CharacterController não empurra Rigidbodies sozinho: aplicamos o empurrão aqui.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.3f) return;
        Vector3 empurrao = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForceAtPosition(empurrao * forcaEmpurrar, hit.point, ForceMode.Impulse);
    }

    void AtualizarCamera()
    {
        if (!cameraTransform) return;
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 foco = transform.position + Vector3.up * altura;
        cameraTransform.position = foco - rot * Vector3.forward * distancia;
        cameraTransform.rotation = rot;
    }
}
