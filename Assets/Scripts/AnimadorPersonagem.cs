using UnityEngine;

/// <summary>
/// Alimenta o parâmetro "Velocidade" (m/s) do Animator e ajusta a velocidade de reprodução para os pés não deslizarem.
/// Se estiver dentro de um objeto com CharacterController (ex.: o jogador), usa a velocidade real dele;
/// senão use SetVelocidade() (ex.: PersonagemTeste).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimadorPersonagem : MonoBehaviour
{
    [Tooltip("Controller deste personagem (aplicado no Awake se o Animator estiver sem controller)")]
    public RuntimeAnimatorController controller;

    [Header("Passada de referência dos clipes (m/s)")]
    public float passadaCaminhar = 1.5f;
    public float passadaCorrer = 4.5f;
    [Tooltip("Abaixo disto é Idle; acima de 'limiteCorrer' é Correr")]
    public float limiteParado = 0.2f, limiteCorrer = 3.5f;
    public float suavizacao = 0.12f;
    [Tooltip("Limites da velocidade de reprodução, para a animação não ficar exagerada")]
    public Vector2 limitesPlayback = new Vector2(0.7f, 1.5f);

    static readonly int HVelocidade = Animator.StringToHash("Velocidade");
    Animator _anim;
    CharacterController _cc;
    float _alvo, _atual, _vel;
    bool _externo;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        if (_anim.runtimeAnimatorController == null && controller != null) _anim.runtimeAnimatorController = controller;
        _anim.applyRootMotion = false;
        _cc = GetComponentInParent<CharacterController>();
    }

    /// <summary>Informa a velocidade (m/s) por código, em vez de ler do CharacterController.</summary>
    public void SetVelocidade(float v) { _externo = true; _alvo = v; }

    void Update()
    {
        if (!_externo && _cc != null)
        {
            Vector3 v = _cc.velocity; v.y = 0;
            _alvo = v.magnitude;
        }
        _atual = Mathf.SmoothDamp(_atual, _alvo, ref _vel, suavizacao);
        _anim.SetFloat(HVelocidade, _atual);

        // pés sem deslizar: a velocidade de reprodução acompanha a velocidade real
        float speed = 1f;
        if (_atual > limiteParado)
        {
            float referencia = _atual < limiteCorrer ? passadaCaminhar : passadaCorrer;
            speed = Mathf.Clamp(_atual / referencia, limitesPlayback.x, limitesPlayback.y);
        }
        _anim.speed = speed;
    }
}
