using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Robozinho que segue o astronauta pela base, como um bichinho de estimação. É criado sozinho quando a cena carrega
/// (configuração em Resources/RoboPet), então a cena não precisa ser editada. Anda com um CharacterController próprio,
/// para não atravessar paredes, mas não colide com o astronauta. Se ficar para trás ou preso, reaparece atrás dele.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RoboPet : MonoBehaviour
{
    public ConfigRoboPet config;
    public Transform alvo;

    const float Gravidade = -20f, Aceleracao = 14f, GiroSuave = 8f;

    CharacterController _cc;
    Vector3 _velH;
    float _velY, _tempoPreso;
    AudioSource _som;
    float _proximoSom, _fimSom;

    // ---------- Criação automática ----------

    static bool _inscrito;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Zerar() { _inscrito = false; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AoIniciar()
    {
        if (!_inscrito) { SceneManager.sceneLoaded += (_, __) => Criar(); _inscrito = true; }
        Criar();
    }

    static void Criar()
    {
        if (FindObjectOfType<RoboPet>() != null) return;
        var cfg = Resources.Load<ConfigRoboPet>("RoboPet");
        if (cfg == null || !cfg.ativo || cfg.prefab == null) return;
        if (!string.IsNullOrEmpty(cfg.somenteNaCena) && SceneManager.GetActiveScene().name != cfg.somenteNaCena) return;
        var jogador = FindObjectOfType<Movimento>();
        if (jogador == null) return;

        var raiz = new GameObject("RoboPet");
        var cc = raiz.AddComponent<CharacterController>();
        cc.height = 1.3f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.65f, 0f);
        cc.stepOffset = 0.3f; cc.skinWidth = 0.04f; cc.minMoveDistance = 0f;

        var modelo = Instantiate(cfg.prefab, raiz.transform);
        modelo.name = "Modelo";
        modelo.transform.localPosition = Vector3.zero;
        modelo.transform.localRotation = Quaternion.identity;
        foreach (var c in modelo.GetComponentsInChildren<Collider>()) c.enabled = false; // quem colide é o CharacterController

        var pet = raiz.AddComponent<RoboPet>();
        pet.config = cfg;
        pet.alvo = jogador.transform;
    }

    // ---------- Seguir ----------

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        if (alvo == null) { enabled = false; return; }
        foreach (var c in alvo.GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(_cc, c, true);
        Reaparecer();
        PrepararSom();
    }

    // ---------- Som de "conversa" ----------

    void PrepararSom()
    {
        var clip = SonsUI.Arquivo("RobotSound");
        if (clip == null || !config.somAtivo) return;
        _som = gameObject.AddComponent<AudioSource>();
        _som.clip = clip;
        _som.playOnAwake = false;
        _som.loop = false;
        _som.spatialBlend = 0.6f;            // sai do robô, mas sem sumir quando a câmera se afasta
        _som.minDistance = 2f; _som.maxDistance = 20f;
        _som.rolloffMode = AudioRolloffMode.Linear;
        _som.volume = 0f;
        _proximoSom = Time.time + Random.Range(3f, config.intervaloSom.x);
    }

    void AtualizarSom()
    {
        if (_som == null) return;
        if (_som.isPlaying)
        {
            // entra e sai suave (0,25 s), para não cortar seco no meio do áudio
            float resta = _fimSom - Time.time, tocou = config.duracaoSom - resta;
            _som.volume = config.volumeSom * Mathf.Clamp01(Mathf.Min(tocou, resta) / 0.25f);
            if (resta <= 0f) _som.Stop();
            return;
        }
        if (Time.time < _proximoSom || !SonsUI.Ligado || Time.timeScale <= 0f) return;

        float dur = Mathf.Min(config.duracaoSom, _som.clip.length);
        _som.time = Random.Range(0f, Mathf.Max(0f, _som.clip.length - dur));   // um trecho diferente a cada vez
        _som.pitch = Random.Range(0.95f, 1.08f);
        _som.volume = 0f;
        _som.Play();
        _fimSom = Time.time + dur;
        _proximoSom = _fimSom + Random.Range(config.intervaloSom.x, config.intervaloSom.y);
    }

    Vector3 PontoDeSeguir()
    {
        Vector3 frente = alvo.forward; frente.y = 0f;
        if (frente.sqrMagnitude < 1e-4f) frente = Vector3.forward;
        frente.Normalize();
        Vector3 direita = Vector3.Cross(Vector3.up, frente);
        return alvo.position + direita * config.posicao.x + frente * config.posicao.y;
    }

    void Update()
    {
        if (alvo == null) return;
        AtualizarSom();
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 aoJogador = alvo.position - transform.position; aoJogador.y = 0f;
        if (aoJogador.magnitude > config.distanciaReaparecer) { Reaparecer(); return; }

        Vector3 para = PontoDeSeguir() - transform.position; para.y = 0f;
        float dist = para.magnitude;

        // anda mais rápido quanto mais longe, e freia chegando no ponto
        Vector3 desejada = Vector3.zero;
        if (dist > config.distanciaParar)
        {
            float vel = Mathf.Lerp(config.velocidade, config.velocidadeAlcancar, Mathf.InverseLerp(2f, 6f, dist));
            vel = Mathf.Min(vel, (dist - config.distanciaParar) * 4f);
            desejada = para / dist * vel;
        }
        _velH = Vector3.MoveTowards(_velH, desejada, Aceleracao * dt);

        if (_cc.isGrounded && _velY < 0f) _velY = -2f;
        _velY += Gravidade * dt;

        Vector3 antes = transform.position;
        _cc.Move((_velH + Vector3.up * _velY) * dt);

        // preso (parede, porta fechada): reaparece depois de um tempo sem conseguir andar
        Vector3 andou = transform.position - antes; andou.y = 0f;
        bool tentando = _velH.magnitude > 1f && dist > 1.5f;
        _tempoPreso = tentando && andou.magnitude < _velH.magnitude * dt * 0.2f ? _tempoPreso + dt : 0f;
        if (_tempoPreso > 1.5f) { Reaparecer(); return; }

        // olha para onde anda; parado, vira devagar para o mesmo lado do astronauta
        Vector3 olhar = _velH.sqrMagnitude > 0.05f ? _velH : alvo.forward;
        olhar.y = 0f;
        if (olhar.sqrMagnitude > 1e-4f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(olhar), GiroSuave * dt);
    }

    /// <summary>Põe o robô no ponto de seguir (ou logo atrás do astronauta, se o ponto estiver dentro de uma parede).</summary>
    public void Reaparecer()
    {
        Vector3 pos = NoChao(PontoDeSeguir());
        if (Bloqueado(pos)) pos = NoChao(alvo.position - alvo.forward * 1f);
        if (Bloqueado(pos)) pos = NoChao(alvo.position);
        _cc.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(alvo.forward, Vector3.up).normalized + Vector3.forward * 1e-4f);
        _cc.enabled = true;
        _velH = Vector3.zero; _velY = 0f; _tempoPreso = 0f;
    }

    Vector3 NoChao(Vector3 p)
    {
        var origem = new Vector3(p.x, alvo.position.y + 1.5f, p.z);
        foreach (var h in Physics.RaycastAll(origem, Vector3.down, 6f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(alvo) && !h.transform.IsChildOf(transform)) return h.point + Vector3.up * 0.05f;
        return new Vector3(p.x, alvo.position.y - 0.9f, p.z);
    }

    bool Bloqueado(Vector3 pe)
    {
        Vector3 a = pe + Vector3.up * (0.3f + 0.1f), b = pe + Vector3.up * (1.3f - 0.3f);
        foreach (var c in Physics.OverlapCapsule(a, b, 0.28f, ~0, QueryTriggerInteraction.Ignore))
            if (!c.transform.IsChildOf(alvo) && !c.transform.IsChildOf(transform)) return true;
        return false;
    }
}
