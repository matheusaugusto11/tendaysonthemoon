using UnityEngine;

/// <summary>
/// Câmera de 3ª pessoa: segue o personagem de forma suave, nunca entra em paredes (SphereCast), adapta o enquadramento
/// à zona (hub, túnel, sala, exterior), olha um pouco à frente para onde ele anda e esconde o personagem se chegar perto
/// demais. A direção (giro/inclinação) vem do Movimento; aqui só se posiciona a câmera. Enquanto o CameraFoco dirige a
/// câmera (telas da interface), não escreve nada.
/// </summary>
[DefaultExecutionOrder(100)] // depois do Movimento (que lê a entrada) e antes do CameraFoco (200)
[RequireComponent(typeof(Camera))]
public class CameraTerceiraPessoa : MonoBehaviour
{
    public static CameraTerceiraPessoa Instancia { get; private set; }

    [Header("Alvo")]
    public Movimento alvo;
    [Tooltip("Altura do ponto que a câmera mira, acima dos pés (m). ~1,5 = ombro.")]
    [Range(0.5f, 3f)] public float alturaPivo = 1.55f;

    [Header("Fora de zonas (área externa)")]
    [Range(1f, 12f)] public float distanciaPadrao = 6.5f;
    [Range(-5f, 60f)] public float inclinacaoPadrao = 20f;
    [Range(35f, 80f)] public float fovPadrao = 58f;
    [Tooltip("Tempo para misturar ao entrar/sair de uma zona (s).")]
    [Range(0.05f, 3f)] public float tempoTrocaZona = 0.6f;

    [Header("Suavidade")]
    [Tooltip("Atraso horizontal do ponto de mira (s). Pequeno = câmera colada; grande = mais 'flutuante'.")]
    [Range(0f, 0.5f)] public float atrasoHorizontal = 0.08f;
    [Tooltip("Atraso vertical do ponto de mira (s). Absorve soleiras e degraus.")]
    [Range(0f, 0.8f)] public float atrasoVertical = 0.18f;
    [Tooltip("Suavização do giro da câmera (s).")]
    [Range(0f, 0.3f)] public float atrasoGiro = 0.05f;
    [Tooltip("Quanto o ponto de mira se adianta para onde o personagem anda (m, na velocidade de corrida).")]
    [Range(0f, 3f)] public float olharAFrente = 0.9f;

    [Header("Colisão (não entrar em paredes)")]
    [Tooltip("Raio da 'bola' usada para testar paredes (m).")]
    [Range(0.05f, 0.6f)] public float raioColisao = 0.28f;
    [Tooltip("Folga entre a câmera e a parede (m).")]
    [Range(0f, 0.5f)] public float margem = 0.1f;
    [Tooltip("Tempo para a câmera voltar à distância normal quando a passagem abre (s). Ao chegar perto é instantâneo.")]
    [Range(0.05f, 2f)] public float tempoVolta = 0.45f;
    [Tooltip("Abaixo desta distância o personagem some, para não encher a tela (m).")]
    [Range(0f, 2f)] public float distanciaEsconder = 1.0f;
    [Tooltip("Plano de corte próximo da câmera (m). Pequeno evita 'cortar' paredes de perto.")]
    [Range(0.01f, 0.5f)] public float planoProximo = 0.08f;

    Camera _cam;
    Vector3 _pivo, _velPivo, _adiante, _velAdiante;
    float _yaw, _pitch, _velYaw, _velPitch;
    float _dist, _velDist;
    float _zDist, _zIncl, _zFov, _vZD, _vZI, _vZF; // parâmetros da zona, misturados
    bool _iniciado, _escondido;
    Renderer[] _rendJogador;
    readonly System.Collections.Generic.List<Renderer> _escondidosPorMim = new System.Collections.Generic.List<Renderer>();
    readonly RaycastHit[] _hits = new RaycastHit[16];

    /// <summary>Nome da zona atual (ou "exterior"), para diagnóstico.</summary>
    public string ZonaAtual { get; private set; } = "exterior";
    /// <summary>Distância real da câmera ao ponto de mira neste quadro.</summary>
    public float DistanciaAtual => _dist;

    void Awake()
    {
        Instancia = this;
        _cam = GetComponent<Camera>();
        _cam.nearClipPlane = planoProximo;
        if (alvo == null) alvo = FindObjectOfType<Movimento>();
        _zDist = distanciaPadrao; _zIncl = inclinacaoPadrao; _zFov = fovPadrao;
    }

    void OnDestroy() { if (Instancia == this) Instancia = null; }

    void OnValidate() { if (_cam != null) _cam.nearClipPlane = planoProximo; }

    /// <summary>Renderizadores do personagem (para esconder de perto), sem a seta guia.</summary>
    void ColetarRenderizadores()
    {
        var lista = new System.Collections.Generic.List<Renderer>();
        foreach (var r in alvo.GetComponentsInChildren<Renderer>(true))
        {
            if (r.transform.IsChildOf(alvo.transform) && !NomeDaSeta(r.transform)) lista.Add(r);
        }
        _rendJogador = lista.ToArray();
    }

    bool NomeDaSeta(Transform t)
    {
        for (var p = t; p != null && p != alvo.transform; p = p.parent) if (p.name == "SetaGuia3D") return true;
        return false;
    }

    void LateUpdate()
    {
        if (alvo == null) { alvo = FindObjectOfType<Movimento>(); if (alvo == null) return; }
        if (_rendJogador == null) ColetarRenderizadores();
        float dt = Mathf.Min(Time.deltaTime, 0.05f); // evita pulo em quedas de quadro
        var foco = CameraFoco.Instancia;
        bool interfaceDirige = foco != null && foco.Ativo;

        // ---------- zona ----------
        Vector3 pe = alvo.transform.position;
        ZonaCamera melhor = null;
        foreach (var z in ZonaCamera.Todas)
            if (z.isActiveAndEnabled && z.Contem(pe) && (melhor == null || z.prioridade > melhor.prioridade || (z.prioridade == melhor.prioridade && z.Volume < melhor.Volume)))
                melhor = z;
        ZonaAtual = melhor != null ? melhor.name : "exterior";
        float zd = melhor ? melhor.distancia : distanciaPadrao, zi = melhor ? melhor.inclinacao : inclinacaoPadrao, zf = melhor ? melhor.fov : fovPadrao;
        if (!_iniciado) { _zDist = zd; _zIncl = zi; _zFov = zf; }
        _zDist = Mathf.SmoothDamp(_zDist, zd, ref _vZD, tempoTrocaZona, Mathf.Infinity, dt);
        _zIncl = Mathf.SmoothDamp(_zIncl, zi, ref _vZI, tempoTrocaZona, Mathf.Infinity, dt);
        _zFov = Mathf.SmoothDamp(_zFov, zf, ref _vZF, tempoTrocaZona, Mathf.Infinity, dt);

        // ---------- ponto de mira (pivô) suavizado + olhar à frente ----------
        var cc = alvo.GetComponent<CharacterController>();
        float yPes = pe.y + (cc != null ? cc.center.y - cc.height * 0.5f : -1f);
        Vector3 vel = cc != null ? cc.velocity : Vector3.zero; vel.y = 0;
        float fracCorrida = Mathf.Clamp01(vel.magnitude / Mathf.Max(0.1f, alvo.velocidadeCorrida));
        Vector3 adianteAlvo = vel.sqrMagnitude > 0.01f ? vel.normalized * (olharAFrente * fracCorrida) : Vector3.zero;
        _adiante = Vector3.SmoothDamp(_adiante, adianteAlvo, ref _velAdiante, 0.35f, Mathf.Infinity, dt);
        Vector3 pivoAlvo = new Vector3(pe.x, yPes + alturaPivo, pe.z) + _adiante;
        if (!_iniciado) _pivo = pivoAlvo;
        float nx = Mathf.SmoothDamp(_pivo.x, pivoAlvo.x, ref _velPivo.x, atrasoHorizontal, Mathf.Infinity, dt);
        float nz = Mathf.SmoothDamp(_pivo.z, pivoAlvo.z, ref _velPivo.z, atrasoHorizontal, Mathf.Infinity, dt);
        float ny = Mathf.SmoothDamp(_pivo.y, pivoAlvo.y, ref _velPivo.y, atrasoVertical, Mathf.Infinity, dt);
        _pivo = new Vector3(nx, ny, nz);

        // ---------- direção: giro do Movimento + inclinação da zona (o jogador ainda pode inclinar) ----------
        float yawAlvo = alvo.Yaw;
        float pitchAlvo = Mathf.Clamp(_zIncl + alvo.PitchExtra, -5f, 75f);
        if (!_iniciado) { _yaw = yawAlvo; _pitch = pitchAlvo; }
        _yaw = Mathf.SmoothDampAngle(_yaw, yawAlvo, ref _velYaw, atrasoGiro, Mathf.Infinity, dt);
        _pitch = Mathf.SmoothDampAngle(_pitch, pitchAlvo, ref _velPitch, atrasoGiro, Mathf.Infinity, dt);
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);

        // ---------- colisão: chega perto na hora, volta devagar ----------
        float desejada = _zDist;
        float permitida = DistanciaLivre(_pivo, rot * Vector3.back, desejada);
        if (!_iniciado) _dist = permitida;
        if (permitida < _dist) { _dist = permitida; _velDist = 0f; }
        else _dist = Mathf.SmoothDamp(_dist, permitida, ref _velDist, tempoVolta, Mathf.Infinity, dt);

        // ---------- personagem some se a câmera chegar perto demais ----------
        bool esconder = _dist < distanciaEsconder;
        if (esconder != _escondido)
        {
            _escondido = esconder;
            if (esconder)
            {
                // só esconde o que está visível agora e lembra disso (a cápsula provisória desligada fica como está)
                _escondidosPorMim.Clear();
                foreach (var r in _rendJogador) if (r != null && r.enabled) { r.enabled = false; _escondidosPorMim.Add(r); }
            }
            else
            {
                foreach (var r in _escondidosPorMim) if (r != null) r.enabled = true;
                _escondidosPorMim.Clear();
            }
        }

        _iniciado = true;
        if (interfaceDirige) return; // o CameraFoco está com a câmera; quando soltar, mistura até esta pose

        transform.SetPositionAndRotation(_pivo + rot * Vector3.back * _dist, rot);
        if (foco == null || foco.EstadoAtual == CameraFoco.Estado.Livre) _cam.fieldOfView = _zFov;
    }

    /// <summary>Até onde a câmera pode ir do pivô na direção dada sem encostar em nada (ignora o personagem e gatilhos).</summary>
    float DistanciaLivre(Vector3 de, Vector3 dir, float desejada)
    {
        float livre = desejada;
        int n = Physics.SphereCastNonAlloc(de, raioColisao, dir, _hits, desejada + margem, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var h = _hits[i];
            if (h.collider == null || h.collider.transform.IsChildOf(alvo.transform)) continue; // o próprio personagem
            if (h.collider.attachedRigidbody != null && !h.collider.attachedRigidbody.isKinematic) continue; // caixas soltas
            float d = h.distance <= 0f ? 0f : h.distance - margem;     // distance 0 = começou encostado
            if (d < livre) livre = d;
        }
        return Mathf.Clamp(livre, 0.1f, desejada); // parede manda: nunca atravessa (de muito perto o personagem some)
    }

    /// <summary>Teleporta a câmera direto para a pose atual (sem suavizar), ex.: ao trocar de cena ou reaparecer.</summary>
    public void Cortar() { _iniciado = false; }
}
