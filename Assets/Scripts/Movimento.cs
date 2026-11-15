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

    [Header("Câmera: ficar atrás do personagem")]
    [Tooltip("A câmera gira sozinha para ficar sempre nas costas do personagem enquanto ele anda.")]
    public bool cameraSegueAtras = true;
    [Tooltip("Rapidez com que a câmera vai para as costas (maior = mais rápido; ~2 é suave).")]
    [Range(0.2f, 10f)] public float suavizacaoSeguir = 2.5f;
    [Tooltip("Segundos sem seguir depois que o jogador mexe na câmera manualmente.")]
    [Range(0f, 5f)] public float atrasoSeguir = 1.2f;
    [Tooltip("Não gira a câmera quando o personagem anda em direção a ela (recuar), para não dar meia volta.")]
    [Range(60f, 180f)] public float anguloMaxSeguir = 100f;
    [Tooltip("Enquanto o analógico/teclas estão pressionados, o 'para cima' fica travado na direção em que a câmera estava ao começar a andar. Evita andar em círculos quando a câmera gira sozinha.")]
    public bool travarDirecaoAoAndar = true;
    [Tooltip("Se a direção apertada mudar mais que isso (graus), a referência é retravada na câmera atual. Ex.: soltar o D e ficar só no W volta a ser 'para cima da tela'.")]
    [Range(5f, 90f)] public float anguloRetravar = 20f;

    [Header("Controles de toque (só em celular/tablet; criados automaticamente se vazios)")]
    [Tooltip("Automático: analógicos só em celular e tablet. Use SempreToque para testar no editor.")]
    public ControlesMobile.Modo controlesDeToque = ControlesMobile.Modo.Automatico;
    public VirtualJoystick joystick;        // metade esquerda da tela: andar
    public VirtualJoystick joystickCamera;  // metade direita da tela: câmera
    public TouchLook look;                  // opcional: arrastar para girar a câmera
    public float velocidadeCameraAnalogico = 140f; // graus por segundo com o analógico no máximo

    CharacterController _cc;
    float _yaw, _pitch = 20f, _velY;
    bool _congelado;
    /// <summary>Só para testes automáticos: somada à entrada do jogador (zero por padrão).</summary>
    [System.NonSerialized] public Vector2 entradaDepuracao;
    float _yawTravado, _ultimaCameraManual = -99f;
    Vector2 _inputTravado, _inputAtual;
    bool _recentrando;
    const float PitchInicial = 20f;

    /// <summary>Giro da câmera (graus), usado pela CameraTerceiraPessoa.</summary>
    public float Yaw => _yaw;
    /// <summary>Inclinação que o jogador acrescentou à da zona (graus).</summary>
    public float PitchExtra => _pitch - PitchInicial;

    /// <summary>Põe a câmera nas costas do personagem (suave). Também por R, R3 ou toque duplo na metade direita.</summary>
    public void Recentrar() { _recentrando = true; }

    /// <summary>Define o giro da câmera na hora (ex.: ao reaparecer ou em testes).</summary>
    public void DefinirGiro(float yaw) { _yaw = yaw; _yawTravado = yaw; _recentrando = false; }
    bool _andando;

    /// <summary>Congela o personagem (fora da fase de Ação) e esconde os controles de toque.</summary>
    public void Congelar(bool congelar)
    {
        _congelado = congelar;
        if (joystick) joystick.transform.root.gameObject.SetActive(!congelar);
        else if (joystickCamera) joystickCamera.transform.root.gameObject.SetActive(!congelar);
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        ControlesMobile.modo = controlesDeToque;
        if (ControlesMobile.UsarToque && !joystick && !joystickCamera) ControlesMobile.Criar(out joystick, out joystickCamera);
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
        bool manual = false;
        float yawAntes = _yaw;
        // Arrasto de dedo na zona opcional de arrastar (TouchLook).
        Vector2 d = look ? look.ConsumirDelta() : Vector2.zero;
        if (d.sqrMagnitude > 0.01f) manual = true;
        _yaw += d.x * sensibilidadeToque;
        _pitch -= d.y * sensibilidadeToque;

        // Analógico da metade direita: câmera contínua.
        if (joystickCamera)
        {
            Vector2 a = joystickCamera.Valor;
            if (a.sqrMagnitude > 0.0004f) manual = true;
            _yaw += a.x * velocidadeCameraAnalogico * Time.deltaTime;
            _pitch -= a.y * velocidadeCameraAnalogico * Time.deltaTime;
        }

        // Analógico direito do controle (gamepad).
        Vector2 pad = Entrada.Camera;
        if (pad.sqrMagnitude > 0.0004f) manual = true;
        _yaw += pad.x * velocidadeCameraAnalogico * Time.deltaTime;
        _pitch -= pad.y * velocidadeCameraAnalogico * Time.deltaTime;

        // Fallback PC: botão direito do mouse em qualquer lugar.
        // recentrar: R, R3 do controle ou toque duplo na metade direita da tela
        bool duplo = (look != null && look.ConsumirDuploToque()) | (joystickCamera != null && joystickCamera.ConsumirDuploToque());
        if (Entrada.RecentrarDown || duplo) _recentrando = true;
        if (_recentrando)
        {
            float alvoYaw = transform.eulerAngles.y;
            _yaw = Mathf.LerpAngle(_yaw, alvoYaw, 1f - Mathf.Exp(-10f * Time.deltaTime));
            if (Mathf.Abs(Mathf.DeltaAngle(_yaw, alvoYaw)) < 1.5f) _recentrando = false;
            manual = true;
        }

        if (Input.GetMouseButton(1))
        {
            manual = true;
            _yaw += Input.GetAxis("Mouse X") * sensibilidadeMouse;
            _pitch -= Input.GetAxis("Mouse Y") * sensibilidadeMouse;
        }
        _pitch = Mathf.Clamp(_pitch, limitePitch.x, limitePitch.y);

        // giro MANUAL da câmera também leva a referência travada do movimento (só o giro automático não leva)
        _yawTravado += Mathf.DeltaAngle(yawAntes, _yaw);
        if (manual) _ultimaCameraManual = Time.time;
        SeguirPorTras();
    }

    /// <summary>Gira a câmera suavemente para as costas do personagem enquanto ele anda (e o jogador não mexe na câmera).</summary>
    void SeguirPorTras()
    {
        if (!cameraSegueAtras || !_andando) return;
        if (Time.time - _ultimaCameraManual < atrasoSeguir) return;
        float alvo = transform.eulerAngles.y; // a direção para onde o personagem está olhando
        // andando em direção à câmera (recuando): não dá meia volta
        if (Mathf.Abs(Mathf.DeltaAngle(_yaw, alvo)) > anguloMaxSeguir) return;
        // só segue quando o jogador anda PARA A FRENTE: de lado (A/D) ou para trás (S) a câmera fica parada
        float frente = Mathf.Clamp01(_inputAtual.y / Mathf.Max(0.001f, _inputAtual.magnitude));
        frente *= frente;
        if (frente < 0.01f) return;
        _yaw = Mathf.LerpAngle(_yaw, alvo, 1f - Mathf.Exp(-suavizacaoSeguir * frente * Time.deltaTime));
    }

    void Mover()
    {
        Vector2 input = Entrada.Mover;
        if (joystick) input += joystick.Valor;
        input += entradaDepuracao;
        input = Vector2.ClampMagnitude(input, 1f);

        // "para cima" é relativo à câmera. Com a câmera seguindo por trás, trava a referência enquanto o jogador segura o
        // analógico, para a câmera girar sem mudar a direção do movimento (senão o personagem andaria em círculos).
        bool temInput = input.sqrMagnitude > 0.0025f;
        // trava ao começar a andar e RETRAVA quando a direção apertada muda (ex.: W+D → só W), senão o W sairia torto
        if (temInput && (!_andando || Vector2.Angle(input, _inputTravado) > anguloRetravar))
        {
            _yawTravado = _yaw;
            _inputTravado = input;
        }
        _andando = temInput;
        _inputAtual = temInput ? input : Vector2.zero;
        float yawMov = (cameraSegueAtras && travarDirecaoAoAndar) ? _yawTravado : _yaw;
        Quaternion yawRot = Quaternion.Euler(0, yawMov, 0);
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
        if (CameraTerceiraPessoa.Instancia != null && CameraTerceiraPessoa.Instancia.isActiveAndEnabled) return; // a câmera nova cuida
        if (CameraFoco.Instancia != null && CameraFoco.Instancia.Ativo) return; // a interface está dirigindo a câmera
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 foco = transform.position + Vector3.up * altura;
        cameraTransform.position = foco - rot * Vector3.forward * distancia;
        cameraTransform.rotation = rot;
    }
}
