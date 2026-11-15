using UnityEngine;

/// <summary>
/// Som de passos do jogador (Resources/Sons/SomDePassos) em loop enquanto ele anda no chão. Correndo, o som fica mais
/// rápido. Para quando o personagem para, quando o jogo é pausado ou com o som desligado. Adicionado pelo Movimento.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SomPassos : MonoBehaviour
{
    [Range(0f, 1f)] public float volume = 0.5f;
    [Tooltip("Velocidade (m/s) a partir da qual os passos tocam.")]
    public float velocidadeMinima = 0.3f;
    [Tooltip("Velocidade (m/s) a partir da qual conta como correr.")]
    public float velocidadeCorrida = 6.5f;
    [Tooltip("Rapidez do som ao correr (1 = normal).")]
    public float pitchCorrida = 1.3f;

    CharacterController _cc;
    AudioSource _fonte;
    bool _comecou;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        var clip = SonsUI.Arquivo("SomDePassos");
        if (!clip) { enabled = false; return; }
        _fonte = gameObject.AddComponent<AudioSource>();
        _fonte.clip = clip;
        _fonte.loop = true;
        _fonte.playOnAwake = false;
        _fonte.spatialBlend = 0f;
        _fonte.volume = 0f;
    }

    void Update()
    {
        Vector3 v = _cc.velocity; v.y = 0f;
        float vel = v.magnitude;
        bool andando = vel > velocidadeMinima && _cc.isGrounded && Time.timeScale > 0f && SonsUI.Ligado;

        _fonte.volume = Mathf.MoveTowards(_fonte.volume, andando ? volume : 0f, Time.unscaledDeltaTime * 6f);
        _fonte.pitch = Mathf.MoveTowards(_fonte.pitch, vel >= velocidadeCorrida ? pitchCorrida : 1f, Time.unscaledDeltaTime * 2f);

        if (andando && !_fonte.isPlaying)
        {
            if (_comecou) _fonte.UnPause(); else { _fonte.Play(); _comecou = true; }
        }
        else if (!andando && _fonte.isPlaying && _fonte.volume <= 0.001f) _fonte.Pause();
    }

    void OnDisable() { if (_fonte) _fonte.Pause(); }
}
