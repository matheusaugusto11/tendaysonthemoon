using UnityEngine;

/// <summary>
/// Um ponto de interação no mundo 3D (estação de uma tarefa, ou o console de energia do hub).
/// Mostra marcador flutuante e rótulo quando está ativa; o GameManager decide qual fica ativa a cada fase.
/// </summary>
public class Estacao : MonoBehaviour
{
    public string id;               // ex.: "reciclador" (liga com TarefaDef.Estacao)
    public string nome;             // nome de reserva; o rótulo mostra o nome da tarefa no idioma ativo
    public Color cor = Color.cyan;
    public float raio = 3.2f;       // distância para poder interagir
    [Tooltip("Só marcador e rótulo, sem a base e a máquina provisórias (ex.: console sobre a mesa do hub).")]
    public bool semMaquina;
    [Tooltip("De onde a câmera olha a estação no close do minijogo (design D8). Só a posição conta: a câmera olha para a " +
             "estação e a põe na metade esquerda da tela. Vazio: 3,2 m da estação, do lado do hub.")]
    public Transform pontoCamera;

    public bool Ativa { get; private set; }
    public bool Concluida { get; private set; }
    /// <summary>A câmera está no close desta estação: marcador, rótulo e máquina provisória ficam escondidos.</summary>
    public bool EmClose { get; private set; }

    Transform _marcador, _visual;
    TextMesh _rotulo;
    Renderer[] _corpos;
    float _fase;

    /// <summary>Ponto que a câmera enquadra no close: a altura de uma bancada.</summary>
    public Vector3 PontoDeFoco => transform.position + Vector3.up * 1.1f;

    /// <summary>Close do minijogo: esconde todo o visual provisório (marcador, rótulo, base e máquina). Ao sair, volta como estava.</summary>
    public void DefinirClose(bool close)
    {
        EmClose = close;
        if (_visual != null) _visual.gameObject.SetActive(!close);
    }

    void Awake()
    {
        Montar();
        Definir(false, false);
    }

    /// <summary>Cria a parte visual (idempotente). Também chamado pelo editor para ficar salvo na cena.</summary>
    public void Montar()
    {
        var existente = transform.Find("Visual");
        if (existente != null)
        {
            if (Achar()) return;
            // Visual incompleto (parte faltando): descarta e refaz.
            existente.name = "Visual_descartado";
            existente.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(existente.gameObject); else DestroyImmediate(existente.gameObject);
        }
        var visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        if (!semMaquina)
        {
            Prim(PrimitiveType.Cylinder, "Base", visual, new Vector3(0, 0.05f, 0), new Vector3(2.6f, 0.05f, 2.6f));
            Prim(PrimitiveType.Cube, "Maquina", visual, new Vector3(0, 0.9f, 0), new Vector3(0.9f, 1.7f, 0.9f));
        }
        var marc = Prim(PrimitiveType.Cube, "Marcador", visual, new Vector3(0, 3.2f, 0), Vector3.one * 0.6f);
        marc.transform.localRotation = Quaternion.Euler(45, 45, 0);

        var rot = new GameObject("Rotulo");
        rot.transform.SetParent(visual, false);
        rot.transform.localPosition = new Vector3(0, 2.4f, 0);
        var tm = rot.AddComponent<TextMesh>();
        tm.text = nome;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.08f;
        tm.color = Color.white;
        if (!Achar()) Debug.LogError("[Estacao] não consegui montar o visual de " + name);
    }

    /// <summary>Liga as referências às partes do visual. Devolve false se alguma parte obrigatória estiver faltando.</summary>
    bool Achar()
    {
        var v = transform.Find("Visual");
        if (v == null) return false;
        _visual = v;
        _marcador = v.Find("Marcador");
        var r = v.Find("Rotulo");
        if (_marcador == null || r == null) return false;
        _rotulo = r.GetComponent<TextMesh>();
        var rMarc = _marcador.GetComponent<Renderer>();
        if (_rotulo == null || r.GetComponent<MeshRenderer>() == null || rMarc == null) return false;
        var baseT = v.Find("Base"); var maquinaT = v.Find("Maquina");
        if (!semMaquina && (baseT == null || maquinaT == null)) return false;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            _rotulo.font = font;
            r.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        if (string.IsNullOrEmpty(_rotulo.text)) _rotulo.text = nome;
        var corpos = new System.Collections.Generic.List<Renderer> { rMarc };
        if (baseT && baseT.GetComponent<Renderer>()) corpos.Add(baseT.GetComponent<Renderer>());
        if (maquinaT && maquinaT.GetComponent<Renderer>()) corpos.Add(maquinaT.GetComponent<Renderer>());
        _corpos = corpos.ToArray();
        return true;
    }

    static GameObject Prim(PrimitiveType t, string n, Transform pai, Vector3 pos, Vector3 escala)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = n;
        Object.DestroyImmediate(go.GetComponent<Collider>()); // não bloqueia o personagem
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        return go;
    }

    void Pintar(Color c)
    {
        if (_corpos == null) return;
        foreach (var r in _corpos)
        {
            if (r == null) continue;
            var m = new Material(r.sharedMaterial);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            r.material = m;
        }
    }

    /// <summary>Ativa = é o destino da fase atual (mostra marcador e rótulo e aceita interação).</summary>
    public void Definir(bool ativa, bool concluida)
    {
        Ativa = ativa;
        Concluida = concluida;
        if (_marcador == null) return;
        Pintar(concluida ? new Color(0.3f, 0.8f, 0.4f) : (ativa ? cor : new Color(0.35f, 0.35f, 0.4f)));
        _marcador.gameObject.SetActive(ativa && !concluida);
        _rotulo.gameObject.SetActive(ativa);
    }

    /// <summary>Troca o texto do rótulo flutuante (o nome da tarefa no idioma ativo).</summary>
    public void DefinirRotulo(string texto)
    {
        if (_rotulo != null) _rotulo.text = texto;
    }

    void Update()
    {
        if (_marcador == null) return;
        _fase += Time.deltaTime;
        if (_marcador.gameObject.activeSelf)
        {
            _marcador.localPosition = new Vector3(0, 3.2f + Mathf.Sin(_fase * 3f) * 0.25f, 0);
            _marcador.Rotate(0, 90f * Time.deltaTime, 0, Space.World);
        }
        var cam = Camera.main;
        if (cam && _rotulo.gameObject.activeSelf)
            _rotulo.transform.rotation = Quaternion.LookRotation(_rotulo.transform.position - cam.transform.position);
    }

    public float DistanciaAte(Vector3 p)
    {
        Vector3 d = transform.position - p; d.y = 0;
        return d.magnitude;
    }
}
