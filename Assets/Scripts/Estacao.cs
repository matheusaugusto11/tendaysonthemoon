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

    // ---------- visual: totem low-poly no estilo da base (painel branco/escuro, faixa e tela acesas na cor da tarefa) ----------
    const string Versao = "Totem_v2";
    const float AlturaMarcador = 2.05f, AlturaRotulo = 1.6f;

    [Header("Materiais do totem (preenchidos pelo editor ao montar)")]
    public Material matCorpo;      // Painel branco
    public Material matEscuro;     // Painel escuro
    public Material matDestaque;   // emissivo, recebe a cor da tarefa

    MaterialPropertyBlock _mpb;

    /// <summary>Cria a parte visual (idempotente). Também chamado pelo editor para ficar salvo na cena.</summary>
    public void Montar()
    {
        var existente = transform.Find("Visual");
        if (existente != null)
        {
            if (Achar()) return;
            // Visual antigo ou incompleto: descarta e refaz.
            existente.name = "Visual_descartado";
            existente.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(existente.gameObject); else DestroyImmediate(existente.gameObject);
        }
        var visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        new GameObject(Versao).transform.SetParent(visual, false); // marca da versão do visual
        // a tela fica virada para o hub (de onde o jogador costuma chegar)
        Vector3 paraHub = -transform.position; paraHub.y = 0;
        if (paraHub.sqrMagnitude > 0.25f) visual.rotation = Quaternion.LookRotation(paraHub.normalized, Vector3.up);

        if (!semMaquina)
        {
            // pedestal octogonal baixo
            Peca("Base", visual, MalhaPrisma(8, 0.55f, 0.5f, 0.08f), Vector3.zero, Quaternion.identity, matEscuro);
            // coluna afunilada
            Peca("Maquina", visual, MalhaPrisma(8, 0.24f, 0.19f, 0.92f), new Vector3(0, 0.08f, 0), Quaternion.identity, matCorpo);
            // faixa acesa na cor da tarefa
            Peca("Faixa", visual, MalhaPrisma(8, 0.255f, 0.245f, 0.07f), new Vector3(0, 0.34f, 0), Quaternion.identity, matDestaque);
            // console inclinado no topo, com a tela acesa
            var console = Peca("Console", visual, MalhaPrisma(4, 0.36f, 0.32f, 0.07f), new Vector3(0, 1.0f, 0),
                                Quaternion.Euler(22f, 0, 0), matEscuro); // tela inclinada para a frente do Visual
            Peca("Tela", console.transform, MalhaPrisma(4, 0.27f, 0.27f, 0.012f), new Vector3(0, 0.07f, 0), Quaternion.identity, matDestaque);
        }
        var marc = Peca("Marcador", visual, MalhaOctaedro(0.16f, 0.24f), new Vector3(0, AlturaMarcador, 0), Quaternion.identity, matDestaque);

        var rot = new GameObject("Rotulo");
        rot.transform.SetParent(visual, false);
        rot.transform.localPosition = new Vector3(0, AlturaRotulo, 0);
        var tm = rot.AddComponent<TextMesh>();
        tm.text = nome;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.05f;
        tm.color = Color.white;
        if (!Achar()) Debug.LogError("[Estacao] não consegui montar o visual de " + name);
    }

    /// <summary>Liga as referências às partes do visual. Devolve false se alguma parte obrigatória estiver faltando (ou se o visual for da versão antiga).</summary>
    bool Achar()
    {
        var v = transform.Find("Visual");
        if (v == null || v.Find(Versao) == null) return false;
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
        // só as partes acesas recebem a cor da tarefa; corpo e console mantêm o material da base
        var corpos = new System.Collections.Generic.List<Renderer> { rMarc };
        foreach (var n in new[] { "Faixa", "Console/Tela" })
        {
            var t = v.Find(n);
            if (t && t.GetComponent<Renderer>()) corpos.Add(t.GetComponent<Renderer>());
        }
        _corpos = corpos.ToArray();
        return true;
    }

    static GameObject Peca(string n, Transform pai, Mesh malha, Vector3 pos, Quaternion rot, Material mat)
    {
        var go = new GameObject(n);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        go.AddComponent<MeshFilter>().sharedMesh = malha;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat != null ? mat : PadraoLit();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // barato no celular
        return go;
    }

    static Material _padrao;
    static Material PadraoLit()
    {
        if (_padrao == null)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            _padrao = new Material(sh != null ? sh : Shader.Find("Standard")) { name = "Totem (reserva)" };
        }
        return _padrao;
    }

    /// <summary>Prisma de N lados com faces chapadas (visual facetado), base em y=0. Raios de baixo e de cima podem diferir.</summary>
    static Mesh MalhaPrisma(int lados, float rBaixo, float rTopo, float altura)
    {
        var v = new System.Collections.Generic.List<Vector3>();
        var t = new System.Collections.Generic.List<int>();
        float a0 = Mathf.PI / lados; // aresta virada para a frente
        Vector3 P(int i, float r, float y) { float a = a0 + i * 2f * Mathf.PI / lados; return new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r); }
        for (int i = 0; i < lados; i++)
        {
            Vector3 b0 = P(i, rBaixo, 0), b1 = P(i + 1, rBaixo, 0), c0 = P(i, rTopo, altura), c1 = P(i + 1, rTopo, altura);
            int k = v.Count; v.AddRange(new[] { b0, c0, c1, b1 }); t.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3 });
        }
        int topo = v.Count; v.Add(new Vector3(0, altura, 0));
        for (int i = 0; i <= lados; i++) v.Add(P(i, rTopo, altura));
        for (int i = 0; i < lados; i++) t.AddRange(new[] { topo, topo + 1 + i, topo + 2 + i });
        int fundo = v.Count; v.Add(Vector3.zero);
        for (int i = 0; i <= lados; i++) v.Add(P(i, rBaixo, 0));
        for (int i = 0; i < lados; i++) t.AddRange(new[] { fundo, fundo + 2 + i, fundo + 1 + i });
        return Chapada(v, t, "Prisma" + lados);
    }

    /// <summary>Losango de 4 lados (octaedro alongado), centrado na origem.</summary>
    static Mesh MalhaOctaedro(float raio, float meiaAltura)
    {
        var v = new System.Collections.Generic.List<Vector3>();
        var t = new System.Collections.Generic.List<int>();
        Vector3 cima = Vector3.up * meiaAltura, baixo = Vector3.down * meiaAltura;
        for (int i = 0; i < 4; i++)
        {
            float a = i * Mathf.PI / 2f, b = (i + 1) * Mathf.PI / 2f;
            Vector3 p0 = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * raio, p1 = new Vector3(Mathf.Sin(b), 0, Mathf.Cos(b)) * raio;
            int k = v.Count; v.AddRange(new[] { cima, p0, p1 }); t.AddRange(new[] { k, k + 1, k + 2 });
            k = v.Count; v.AddRange(new[] { baixo, p1, p0 }); t.AddRange(new[] { k, k + 1, k + 2 });
        }
        return Chapada(v, t, "Losango");
    }

    /// <summary>Desfaz o compartilhamento de vértices para cada triângulo ter normal própria (sombreado facetado).</summary>
    static Mesh Chapada(System.Collections.Generic.List<Vector3> v, System.Collections.Generic.List<int> t, string nome)
    {
        var vv = new Vector3[t.Count]; var tt = new int[t.Count];
        for (int i = 0; i < t.Count; i++) { vv[i] = v[t[i]]; tt[i] = i; }
        var m = new Mesh { name = nome, vertices = vv, triangles = tt };
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }

    void Pintar(Color c)
    {
        if (_corpos == null) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        foreach (var r in _corpos)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            _mpb.SetColor("_EmissionColor", c * 1.6f); // acende na cor da tarefa
            r.SetPropertyBlock(_mpb);
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
            _marcador.localPosition = new Vector3(0, AlturaMarcador + Mathf.Sin(_fase * 3f) * 0.12f, 0);
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
