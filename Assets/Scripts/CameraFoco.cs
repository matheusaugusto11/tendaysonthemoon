using UnityEngine;

/// <summary>
/// Câmera dirigida pela interface (design D8 da mudança interface-e-minijogos): leva a câmera principal a uma pose fixa
/// (a base vista de fora no Resultado) ou a uma órbita lenta em volta da base (a tela Início) e a devolve ao personagem
/// com uma transição suave. Enquanto está <see cref="Ativo"/>, o Movimento não mexe na câmera.
/// Com movimento reduzido (navegador ou UiTema.ForcarMovimentoReduzido) não há transição nem órbita: a câmera corta direto
/// para a pose e fica parada.
/// </summary>
[DefaultExecutionOrder(200)] // depois do Movimento, que posiciona a câmera de seguir no LateUpdate
[RequireComponent(typeof(Camera))]
public class CameraFoco : MonoBehaviour
{
    public enum Estado { Livre, Indo, Preso, Voltando }

    public static CameraFoco Instancia { get; private set; }

    public Estado EstadoAtual { get; private set; } = Estado.Livre;
    /// <summary>A câmera está sendo guiada por aqui (indo ou presa na pose): o Movimento fica de fora.</summary>
    public bool Ativo => EstadoAtual == Estado.Indo || EstadoAtual == Estado.Preso;
    public bool Orbitando { get; private set; }

    Camera _cam;
    Vector3 _dePos, _alvoPos;
    Quaternion _deRot, _alvoRot;
    float _deFov, _alvoFov, _fovLivre;
    float _t, _dur;
    // órbita
    Vector3 _centro;
    float _raio, _altura, _grausPorS, _angulo, _deslocamento;

    /// <summary>Acha a câmera principal e garante o componente nela.</summary>
    public static CameraFoco Garantir()
    {
        if (Instancia != null) return Instancia;
        var cam = Camera.main;
        if (cam == null) return null;
        var cf = cam.GetComponent<CameraFoco>();
        if (cf == null) cf = cam.gameObject.AddComponent<CameraFoco>();
        Instancia = cf;
        return cf;
    }

    void Awake()
    {
        Instancia = this;
        _cam = GetComponent<Camera>();
        _fovLivre = _cam.fieldOfView;
    }

    void OnDestroy() { if (Instancia == this) Instancia = null; }

    // ---------- Poses ----------

    /// <summary>
    /// Pose de olhar um ponto de um ponto: a câmera é deslocada para o lado (sem girar), o que empurra a cena para o lado oposto
    /// no quadro. É assim que a base fica à esquerda, fora do painel lateral.
    /// </summary>
    public static void PoseOlhando(Vector3 de, Vector3 para, float deslocamentoLateral, out Vector3 pos, out Quaternion rot)
    {
        rot = Quaternion.LookRotation(para - de, Vector3.up);
        pos = de + rot * Vector3.right * deslocamentoLateral;
    }

    // ---------- Close na estação (tarefa 7.1) ----------

    /// <summary>Campo de visão do close: um pouco mais fechado que o da câmera de seguir, para não pegar parede de lado.</summary>
    public const float FovClose = 50f;
    /// <summary>Sem pontoCamera: distância e altura da câmera em relação à estação, do lado do hub.</summary>
    public const float DistanciaClose = 3.2f, AlturaClose = 1.7f;
    static readonly Vector3 Hub = new Vector3(2f, 0f, 2f);

    /// <summary>Estação no close agora (o visual provisório dela fica escondido até soltar ou focar outra coisa).</summary>
    public Estacao EstacaoEmClose { get; private set; }
    Renderer[] _escondidos;

    /// <summary>
    /// Fração da largura da tela, a partir da esquerda, que o painel lateral deixa livre. A estação vai para o meio dela.
    /// Definida pela interface (UIJogo); o padrão é o do referencial 1920 por 1080 (painel de 840 com margem de 48).
    /// </summary>
    public static float FracaoLivre = (1920f - 840f - 48f) / 1920f;

    /// <summary>
    /// Pose do close: a câmera fica no pontoCamera da estação (ou a 3,2 m, 1,7 m de altura, do lado do hub) e gira para a
    /// direita o bastante para a estação cair no centro da faixa livre à esquerda do painel.
    /// </summary>
    public static void PoseDoClose(Estacao e, float aspecto, out Vector3 pos, out Quaternion rot)
    {
        Vector3 alvo = e.PontoDeFoco;
        if (e.pontoCamera != null) pos = e.pontoCamera.position;
        else
        {
            Vector3 paraHub = Hub - e.transform.position; paraHub.y = 0f;
            if (paraHub.sqrMagnitude < 0.01f) paraHub = Vector3.back;
            pos = e.transform.position + paraHub.normalized * DistanciaClose + Vector3.up * AlturaClose;
        }
        Quaternion olhar = Quaternion.LookRotation(alvo - pos, Vector3.up);
        // Um ponto a θ graus à esquerda do centro aparece em x = −tan θ / tan(meio campo horizontal), em NDC.
        float ndc = 1f - 2f * Mathf.Clamp(FracaoLivre * 0.5f, 0.15f, 0.5f);
        float tanH = Mathf.Tan(FovClose * 0.5f * Mathf.Deg2Rad) * Mathf.Max(0.5f, aspecto);
        float giro = Mathf.Atan(ndc * tanH) * Mathf.Rad2Deg;
        rot = Quaternion.Euler(0f, giro, 0f) * olhar;
    }

    /// <summary>Close na estação: esconde o visual provisório dela e, se o personagem tapar a vista, esconde o personagem.</summary>
    public void FocarEstacao(Estacao e, Transform personagem, float duracao = 0.8f)
    {
        if (e == null) return;
        PoseDoClose(e, _cam.aspect, out var pos, out var rot);
        if (EstacaoEmClose != e) LiberarEstacao();
        EstacaoEmClose = e;
        e.DefinirClose(true);
        EsconderSeTapar(personagem, pos, e.PontoDeFoco);
        Focar(pos, rot, FovClose, duracao);
    }

    void EsconderSeTapar(Transform personagem, Vector3 de, Vector3 para)
    {
        MostrarPersonagem();
        if (personagem == null) return;
        Vector3 c = personagem.position + Vector3.up * 1f;
        Vector3 seg = para - de;
        float t = Mathf.Clamp01(Vector3.Dot(c - de, seg) / Mathf.Max(0.0001f, seg.sqrMagnitude));
        bool tapa = Vector3.Distance(c, de + seg * t) < 0.8f || Vector3.Distance(c, de) < 1.2f;
        if (!tapa) return;
        // Só os que estão ligados: ao voltar, religa exatamente esses (o personagem tem peças desligadas de propósito).
        var lista = new System.Collections.Generic.List<Renderer>();
        foreach (var r in personagem.GetComponentsInChildren<Renderer>()) if (r && r.enabled) { r.enabled = false; lista.Add(r); }
        _escondidos = lista.ToArray();
    }

    void MostrarPersonagem()
    {
        if (_escondidos == null) return;
        foreach (var r in _escondidos) if (r) r.enabled = true;
        _escondidos = null;
    }

    void LiberarEstacao()
    {
        if (EstacaoEmClose != null) EstacaoEmClose.DefinirClose(false);
        EstacaoEmClose = null;
        MostrarPersonagem();
    }

    // ---------- Comandos ----------

    /// <summary>Vai até a pose e fica nela. Pedir de novo a pose em que já está (ou para onde já vai) não refaz a transição.</summary>
    public void Focar(Vector3 pos, Quaternion rot, float fov, float duracao = 0.8f)
    {
        bool mesma = Ativo && !Orbitando && (_alvoPos - pos).sqrMagnitude < 0.0025f && Quaternion.Angle(_alvoRot, rot) < 0.5f
                     && Mathf.Abs(_alvoFov - fov) < 0.1f;
        if (mesma) return;
        if (EstacaoEmClose != null)
        {
            // Outra pose que não o close desta estação: devolve o visual dela.
            PoseDoClose(EstacaoEmClose, _cam.aspect, out var pc, out _);
            if ((pc - pos).sqrMagnitude > 0.0025f) LiberarEstacao();
        }
        Orbitando = false;
        Comecar(pos, rot, fov, duracao);
    }

    /// <summary>Vai até a órbita em volta de <paramref name="centro"/> e continua girando devagar até <see cref="Soltar"/>.</summary>
    public void Orbitar(Vector3 centro, float raio, float altura, float grausPorSegundo, float fov, float deslocamentoLateral, float duracao = 1.2f)
    {
        bool jaOrbitava = Orbitando && EstadoAtual != Estado.Voltando;
        LiberarEstacao();
        _centro = centro; _raio = raio; _altura = altura; _grausPorS = grausPorSegundo; _deslocamento = deslocamentoLateral;
        if (!jaOrbitava) _angulo = 200f; // começa vendo a base de frente
        Orbitando = true;
        PoseDaOrbita(out var p, out var r);
        Comecar(p, r, fov, duracao);
    }

    /// <summary>Devolve a câmera ao personagem com uma transição suave.</summary>
    public void Soltar(float duracao = 0.6f)
    {
        LiberarEstacao();
        if (EstadoAtual == Estado.Livre || EstadoAtual == Estado.Voltando) return;
        Orbitando = false;
        _dePos = transform.position; _deRot = transform.rotation; _deFov = _cam.fieldOfView;
        _t = 0f; _dur = UiTema.MovimentoReduzido ? 0f : duracao;
        EstadoAtual = Estado.Voltando;
        if (_dur <= 0f) { EstadoAtual = Estado.Livre; _cam.fieldOfView = _fovLivre; }
    }

    void Comecar(Vector3 pos, Quaternion rot, float fov, float duracao)
    {
        if (EstadoAtual == Estado.Livre) _fovLivre = _cam.fieldOfView;
        _dePos = transform.position; _deRot = transform.rotation; _deFov = _cam.fieldOfView;
        _alvoPos = pos; _alvoRot = rot; _alvoFov = fov;
        _t = 0f; _dur = UiTema.MovimentoReduzido ? 0f : duracao;
        EstadoAtual = Estado.Indo;
        if (_dur <= 0f) { Aplicar(1f); EstadoAtual = Estado.Preso; }
    }

    // ---------- Quadro a quadro ----------

    void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;
        switch (EstadoAtual)
        {
            case Estado.Indo:
                if (Orbitando) AtualizarOrbita(dt);
                _t += dt;
                Aplicar(_dur <= 0f ? 1f : Suave(_t / _dur));
                if (_t >= _dur) EstadoAtual = Estado.Preso;
                break;
            case Estado.Preso:
                if (Orbitando) AtualizarOrbita(dt);
                Aplicar(1f);
                break;
            case Estado.Voltando:
                // O Movimento já pôs a câmera na pose de seguir neste quadro (ele roda antes). Mistura da pose de saída até ela.
                _t += dt;
                float s = _dur <= 0f ? 1f : Suave(_t / _dur);
                transform.SetPositionAndRotation(Vector3.Lerp(_dePos, transform.position, s), Quaternion.Slerp(_deRot, transform.rotation, s));
                _cam.fieldOfView = Mathf.Lerp(_deFov, _fovLivre, s);
                if (_t >= _dur) { EstadoAtual = Estado.Livre; _cam.fieldOfView = _fovLivre; }
                break;
        }
    }

    static float Suave(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    void Aplicar(float s)
    {
        transform.SetPositionAndRotation(Vector3.Lerp(_dePos, _alvoPos, s), Quaternion.Slerp(_deRot, _alvoRot, s));
        _cam.fieldOfView = Mathf.Lerp(_deFov, _alvoFov, s);
    }

    void AtualizarOrbita(float dt)
    {
        if (!UiTema.MovimentoReduzido) _angulo += _grausPorS * dt;
        PoseDaOrbita(out _alvoPos, out _alvoRot);
    }

    void PoseDaOrbita(out Vector3 pos, out Quaternion rot)
    {
        Vector3 de = _centro + Quaternion.Euler(0f, _angulo, 0f) * (Vector3.back * _raio) + Vector3.up * _altura;
        PoseOlhando(de, _centro, _deslocamento, out pos, out rot);
    }
}
