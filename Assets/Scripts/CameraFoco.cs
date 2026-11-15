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

    // ---------- Comandos ----------

    /// <summary>Vai até a pose e fica nela.</summary>
    public void Focar(Vector3 pos, Quaternion rot, float fov, float duracao = 0.8f)
    {
        Orbitando = false;
        Comecar(pos, rot, fov, duracao);
    }

    /// <summary>Vai até a órbita em volta de <paramref name="centro"/> e continua girando devagar até <see cref="Soltar"/>.</summary>
    public void Orbitar(Vector3 centro, float raio, float altura, float grausPorSegundo, float fov, float deslocamentoLateral, float duracao = 1.2f)
    {
        bool jaOrbitava = Orbitando && EstadoAtual != Estado.Voltando;
        _centro = centro; _raio = raio; _altura = altura; _grausPorS = grausPorSegundo; _deslocamento = deslocamentoLateral;
        if (!jaOrbitava) _angulo = 200f; // começa vendo a base de frente
        Orbitando = true;
        PoseDaOrbita(out var p, out var r);
        Comecar(p, r, fov, duracao);
    }

    /// <summary>Devolve a câmera ao personagem com uma transição suave.</summary>
    public void Soltar(float duracao = 0.6f)
    {
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
