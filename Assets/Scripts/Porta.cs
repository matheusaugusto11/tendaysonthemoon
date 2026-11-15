using UnityEngine;

/// <summary>
/// Uma porta da base (duas folhas D/E) de CORRER: cada folha desliza para o seu lado, uma para a esquerda
/// e outra para a direita (sem girar na dobradiça). O clipe "..._Abrir" do FBX só define a pose fechada.
/// API: Abrir(), Fechar(), Alternar(). Abre sozinha quando o jogador chega perto (desligável).
/// </summary>
[ExecuteAlways]
public class Porta : MonoBehaviour
{
    public string idPorta;
    [Tooltip("Instância de base_portas (raiz da hierarquia dos clipes)")] public GameObject raiz;
    [Tooltip("Clipes *_Abrir: usados só para pôr as folhas na pose fechada (quadro 1)")] public AnimationClip[] clips;
    public Transform[] folhas;
    [Tooltip("Segundos para abrir/fechar")] public float tempoAbrir = 1f;
    [Tooltip("Fração da largura da folha que ela desliza (1 = largura inteira)")] public float fracaoAbertura = 1f;

    [Header("Abertura automática")]
    public bool automatica = true;
    public float raioAbrir = 3.2f;
    public float raioFechar = 4.8f;
    public AudioClip somAbrir;
    [Range(0f, 1f)] public float volumeSom = 1f;

    public bool Aberta { get; private set; }

    float _t01;
    Vector3 _centro;
    Transform _jogador;
    Vector3[] _fechadaLocal;
    Vector3[] _deslocLocal;

    // Pose fechada e deslocamentos também no editor.
    void OnEnable() { Preparar(); }
    void Start() { Preparar(); }

    /// <summary>Põe as folhas na pose fechada (quadro 1 do clipe de cada folha) e calcula o deslizamento.</summary>
    public void Preparar()
    {
        if (folhas == null || folhas.Length == 0) return;
        _t01 = 0f; Aberta = false;

        // 1) pose fechada pela GEOMETRIA: cada folha gira em torno da sua dobradiça (o pivô) até apontar
        //    para a dobradiça da outra folha, ficando no plano do vão. Não depende das rotações gravadas no FBX.
        if (folhas.Length >= 2)
            for (int i = 0; i < folhas.Length; i++)
            {
                var f = folhas[i]; var outra = folhas[i == 0 ? 1 : 0];
                if (f == null || outra == null) continue;
                var r = f.GetComponentInChildren<Renderer>();
                if (r == null) continue;
                Vector3 atual = r.bounds.center - f.position; atual.y = 0;      // para onde a folha aponta agora
                Vector3 alvo = outra.position - f.position; alvo.y = 0;         // para onde deve apontar (fechada)
                if (atual.sqrMagnitude < 1e-6f || alvo.sqrMagnitude < 1e-6f) continue;
                float ang = Vector3.SignedAngle(atual, alvo, Vector3.up);
                f.rotation = Quaternion.AngleAxis(ang, Vector3.up) * f.rotation;
            }

        // 2) as dobradiças (pivôs) ficam nas bordas externas do vão: a linha entre elas dá a direção de correr,
        //    e cada folha corre a própria largura (metade da distância entre as dobradiças).
        _fechadaLocal = new Vector3[folhas.Length]; _deslocLocal = new Vector3[folhas.Length];
        Vector3 media = Vector3.zero;
        foreach (var f in folhas) if (f != null) media += f.position;
        media /= folhas.Length; _centro = media;

        Vector3 d = folhas.Length >= 2 && folhas[0] != null && folhas[1] != null ? folhas[0].position - folhas[1].position : Vector3.right;
        d.y = 0;
        float largura = folhas.Length >= 2 ? d.magnitude * 0.5f : 0.9f;
        if (d.sqrMagnitude < 0.0001f) { d = Vector3.right; largura = 0.9f; }
        d.Normalize();

        for (int i = 0; i < folhas.Length; i++)
        {
            var f = folhas[i]; if (f == null) continue;
            float sinal = i == 0 ? 1f : -1f; // folha 0 corre para um lado, a outra para o lado oposto
            Vector3 mundo = d * (sinal * largura * fracaoAbertura);
            _fechadaLocal[i] = f.localPosition;
            _deslocLocal[i] = f.parent != null ? f.parent.InverseTransformVector(mundo) : mundo;
        }
        Aplicar(0f);
    }

    // O clipe de uma folha se chama "<nome da folha>_Abrir" (nas portas da Estufa, sem o "Estufa_").
    AnimationClip ClipDaFolha(Transform folha)
    {
        string a = folha.name + "_Abrir", b = folha.name.Replace("Porta_Estufa_", "Porta_") + "_Abrir";
        foreach (var c in clips) if (c != null && (c.name == a || c.name == b)) return c;
        return null;
    }

    public void Abrir()
    {
        if (Aberta) return;
        Aberta = true;
        if (somAbrir != null && Application.isPlaying) AudioSource.PlayClipAtPoint(somAbrir, _centro, volumeSom);
    }
    public void Fechar() { Aberta = false; }
    public void Alternar() { if (Aberta) Fechar(); else Abrir(); }

    [ContextMenu("Teste: Alternar")] void TesteAlternar() { if (_deslocLocal == null) Preparar(); Alternar(); }

    bool _poseGarantida;

    void Update()
    {
        // Garante a pose fechada no 1º quadro (no editor, a pose do OnEnable pode ser sobrescrita pelo prefab ao carregar a cena).
        if (!_poseGarantida) { _poseGarantida = true; Preparar(); }
        if (!Application.isPlaying || _deslocLocal == null) return;
        if (automatica)
        {
            if (_jogador == null) { var m = FindObjectOfType<Movimento>(); if (m != null) _jogador = m.transform; }
            if (_jogador != null)
            {
                Vector3 d = _centro - _jogador.position; d.y = 0;
                float dist = d.magnitude;
                if (!Aberta && dist < raioAbrir) Abrir();
                else if (Aberta && dist > raioFechar) Fechar();
            }
        }
        float alvo = Aberta ? 1f : 0f;
        float novo = Mathf.MoveTowards(_t01, alvo, Time.deltaTime / Mathf.Max(0.05f, tempoAbrir));
        if (Mathf.Approximately(novo, _t01)) return;
        _t01 = novo;
        Aplicar(Mathf.SmoothStep(0f, 1f, _t01));
    }

    void Aplicar(float t)
    {
        if (_deslocLocal == null) return;
        for (int i = 0; i < folhas.Length; i++)
            if (folhas[i] != null) folhas[i].localPosition = _fechadaLocal[i] + _deslocLocal[i] * t;
    }
}
