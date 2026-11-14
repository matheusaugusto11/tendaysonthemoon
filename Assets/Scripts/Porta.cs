using UnityEngine;

/// <summary>
/// Uma porta da base (duas folhas D/E). Toca os clipes "..._Abrir" do FBX amostrando-os por código.
/// API: Abrir(), Fechar(), Alternar(). Abre sozinha quando o jogador chega perto (desligável).
/// </summary>
[ExecuteAlways]
public class Porta : MonoBehaviour
{
    public string idPorta;
    [Tooltip("Instância de base_portas (raiz da hierarquia dos clipes)")] public GameObject raiz;
    public AnimationClip[] clips;
    public Transform[] folhas;
    public float duracao = 1.6f;

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

    // Pose fechada também no editor: o FBX guarda as folhas numa pose "neutra" diferente do quadro 1 do clipe.
    void OnEnable()
    {
        _centro = CalcularCentro();
        _t01 = 0f;
        Aplicar(0f);
    }

    void Start()
    {
        _centro = CalcularCentro();
        Aplicar(0f);
    }

    Vector3 CalcularCentro()
    {
        Vector3 c = Vector3.zero; int n = 0;
        foreach (var f in folhas)
        {
            if (f == null) continue;
            var r = f.GetComponentInChildren<Renderer>();
            c += r != null ? r.bounds.center : f.position; n++;
        }
        return n > 0 ? c / n : transform.position;
    }

    public void Abrir()
    {
        if (Aberta) return;
        Aberta = true;
        if (somAbrir != null && Application.isPlaying) AudioSource.PlayClipAtPoint(somAbrir, _centro, volumeSom);
    }
    public void Fechar() { Aberta = false; }
    public void Alternar() { if (Aberta) Fechar(); else Abrir(); }

    [ContextMenu("Teste: Alternar")] void TesteAlternar() { _centro = CalcularCentro(); Alternar(); }

    void Update()
    {
        if (!Application.isPlaying) return;
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
        float novo = Mathf.MoveTowards(_t01, alvo, Time.deltaTime / Mathf.Max(0.05f, duracao));
        if (Mathf.Approximately(novo, _t01)) return;
        _t01 = novo;
        Aplicar(Mathf.SmoothStep(0f, 1f, _t01));
    }

    void Aplicar(float t)
    {
        if (raiz == null) return;
        foreach (var c in clips) if (c != null) c.SampleAnimation(raiz, t * c.length);
    }
}
