using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Portas de correr da estufa: quando o jogador chega perto, cada porta desliza para o seu lado
/// (uma para a esquerda, a outra para a direita); ao se afastar, elas voltam e fecham.
/// Funciona com as portas "Porta_Entrada_*" e "Porta_Fundo_*" do modelo, sem usar animações.
/// Coloque este componente na raiz do modelo da estufa.
/// </summary>
public class PortasAutomaticas : MonoBehaviour
{
    public float raioAbrir = 4.5f;
    public float raioFechar = 6f;
    [Tooltip("Segundos para abrir/fechar")] public float tempoAbrir = 0.8f;
    [Tooltip("Som tocado quando as portas começam a abrir (preenchido automaticamente com Assets/Audios/doorOpening)")] public AudioClip somAbrir;
    [Range(0f, 1f)] public float volumeSom = 1f;
    [Tooltip("Fração da largura da porta que ela desliza (1 = largura inteira)")] public float fracaoAbertura = 1f;

    class Porta
    {
        public Transform t;
        public Vector3 fechadaLocal;
        public Vector3 deslocLocal;   // deslocamento local até a posição aberta
    }

    class Lado
    {
        public string nome;
        public List<Porta> portas = new List<Porta>();
        public Vector3 centro;
        public bool aberto;
        public float f; // 0 = fechada, 1 = aberta
    }

    Lado[] _lados;
    Transform _jogador;

    void Start()
    {
        var entrada = new Lado { nome = "Entrada" };
        var fundo = new Lado { nome = "Fundo" };
        var porEntrada = new List<Transform>();
        var porFundo = new List<Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains("Abrir")) continue;
            if (t.name.Contains("Porta_Entrada")) porEntrada.Add(t);
            else if (t.name.Contains("Porta_Fundo")) porFundo.Add(t);
        }
        Vector3 cEntrada = Centro(porEntrada), cFundo = Centro(porFundo);
        Vector3 eixo = cFundo - cEntrada; eixo.y = 0;
        if (eixo.sqrMagnitude < 0.0001f) eixo = transform.forward;
        eixo.Normalize();

        Montar(entrada, porEntrada, eixo);     // olhando da entrada para dentro
        Montar(fundo, porFundo, -eixo);        // olhando do fundo para dentro
        _lados = new[] { entrada, fundo };
        if (porEntrada.Count + porFundo.Count == 0)
            Debug.LogWarning("[PortasAutomaticas] Não achei Porta_Entrada*/Porta_Fundo* em " + name);
    }

    static Bounds Limites(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    static Vector3 Centro(List<Transform> lista)
    {
        if (lista.Count == 0) return Vector3.zero;
        Vector3 c = Vector3.zero;
        foreach (var t in lista) c += Limites(t).center;
        return c / lista.Count;
    }

    // "paraDentro" = direção de quem está do lado de fora olhando para a estufa.
    void Montar(Lado lado, List<Transform> portas, Vector3 paraDentro)
    {
        if (portas.Count == 0) return;
        lado.centro = Centro(portas);
        Vector3 direita = Vector3.Cross(Vector3.up, paraDentro).normalized; // direita de quem entra

        foreach (var t in portas)
        {
            var b = Limites(t);
            float lado_ = Vector3.Dot(b.center - lado.centro, direita);
            float sinal = lado_ >= 0f ? 1f : -1f;                         // porta da direita vai p/ direita
            float largura = Mathf.Abs(b.size.x * direita.x) + Mathf.Abs(b.size.z * direita.z);
            Vector3 desloc = direita * (sinal * largura * fracaoAbertura);
            var p = new Porta
            {
                t = t,
                fechadaLocal = t.localPosition,
                deslocLocal = t.parent != null ? t.parent.InverseTransformVector(desloc) : desloc,
            };
            lado.portas.Add(p);
        }
        // Se só uma porta caiu de cada lado (centros iguais), garante sentidos opostos.
        if (lado.portas.Count == 2 && Vector3.Dot(lado.portas[0].deslocLocal, lado.portas[1].deslocLocal) > 0f)
            lado.portas[1].deslocLocal = -lado.portas[1].deslocLocal;
    }

    void Update()
    {
        if (_lados == null) return;
        if (_jogador == null)
        {
            var m = FindObjectOfType<Movimento>();
            if (m == null) return;
            _jogador = m.transform;
        }

        foreach (var lado in _lados)
        {
            if (lado.portas.Count == 0) continue;
            Vector3 delta = lado.centro - _jogador.position; delta.y = 0;
            float d = delta.magnitude;
            if (!lado.aberto && d < raioAbrir)
            {
                lado.aberto = true;
                // Som 3D na posição das portas, só quando elas começam a abrir.
                if (somAbrir != null) AudioSource.PlayClipAtPoint(somAbrir, lado.centro, volumeSom);
            }
            else if (lado.aberto && d > raioFechar) lado.aberto = false;

            float alvo = lado.aberto ? 1f : 0f;
            float novo = Mathf.MoveTowards(lado.f, alvo, Time.deltaTime / Mathf.Max(0.05f, tempoAbrir));
            if (Mathf.Approximately(novo, lado.f)) continue;
            lado.f = novo;
            float suave = Mathf.SmoothStep(0f, 1f, lado.f);
            foreach (var p in lado.portas) p.t.localPosition = p.fechadaLocal + p.deslocLocal * suave;
        }
    }
}
