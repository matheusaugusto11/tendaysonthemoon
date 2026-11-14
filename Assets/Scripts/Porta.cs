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

    void Preparar()
    {
        if (folhas == null || folhas.Length == 0) return;
        _t01 = 0f; Aberta = false;

        // 1) pose fechada (quadro 1 dos clipes): o FBX guarda as folhas numa pose "neutra" diferente.
        if (raiz != null && clips != null) foreach (var c in clips) if (c != null) c.SampleAnimation(raiz, 0f);

        // 2) centros e direção de deslizamento (ao longo da linha entre as duas folhas, horizontal)
        var centros = new Vector3[folhas.Length]; var larguras = new float[folhas.Length];
        Vector3 media = Vector3.zero;
        for (int i = 0; i < folhas.Length; i++)
        {
            if (folhas[i] == null) continue;
            var r = folhas[i].GetComponentInChildren<Renderer>();
            centros[i] = r != null ? r.bounds.center : folhas[i].position;
            media += centros[i];
        }
        media /= folhas.Length; _centro = media;

        Vector3 d = folhas.Length >= 2 ? centros[0] - centros[1] : Vector3.right;
        d.y = 0;
        if (d.sqrMagnitude < 0.0001f)
        {   // folhas sobrepostas: desliza ao longo da largura da folha
            var r0 = folhas[0].GetComponentInChildren<Renderer>();
            d = r0 != null && r0.bounds.size.x >= r0.bounds.size.z ? Vector3.right : Vector3.forward;
        }
        d.Normalize();

        _fechadaLocal = new Vector3[folhas.Length]; _deslocLocal = new Vector3[folhas.Length];
        for (int i = 0; i < folhas.Length; i++)
        {
            var f = folhas[i]; if (f == null) continue;
            var r = f.GetComponentInChildren<Renderer>();
            float larg = r != null ? (r.bounds.extents.x * Mathf.Abs(d.x) + r.bounds.extents.z * Mathf.Abs(d.z)) * 2f : 0.9f;
            float sinal = Vector3.Dot(centros[i] - media, d) >= 0f ? 1f : -1f; // cada folha vai para o seu lado
            if (folhas.Length == 2 && i == 1) sinal = -Mathf.Sign(Vector3.Dot(centros[0] - media, d) >= 0f ? 1f : -1f);
            Vector3 mundo = d * (sinal * larg * fracaoAbertura);
            _fechadaLocal[i] = f.localPosition;
            _deslocLocal[i] = f.parent != null ? f.parent.InverseTransformVector(mundo) : mundo;
        }
        Aplicar(0f);
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

    void Update()
    {
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
