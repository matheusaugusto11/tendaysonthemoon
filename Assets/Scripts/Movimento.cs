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
    public float sensibilidadeMouse = 3f;
    public Vector2 limitePitch = new Vector2(-10f, 70f);

    [Header("Joystick")]
    public VirtualJoystick joystick;

    CharacterController _cc;
    float _yaw, _pitch = 20f, _velY;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        if (!joystick) joystick = VirtualJoystick.Create();
        if (cameraTransform) _yaw = cameraTransform.eulerAngles.y;
    }

    void Update()
    {
        LerMouse();
        Mover();
    }

    void LateUpdate() => AtualizarCamera();

    void LerMouse()
    {
        // Botão direito do mouse (ou esquerdo, se não estiver sobre o joystick) gira a câmera.
        if (Input.GetMouseButton(1) || (Input.GetMouseButton(0) && !Joystick_Ativo()))
        {
            _yaw += Input.GetAxis("Mouse X") * sensibilidadeMouse;
            _pitch -= Input.GetAxis("Mouse Y") * sensibilidadeMouse;
            _pitch = Mathf.Clamp(_pitch, limitePitch.x, limitePitch.y);
        }
    }

    bool Joystick_Ativo() => joystick && joystick.Value.sqrMagnitude > 0.0001f;

    void Mover()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (joystick) input += joystick.Value;
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
