using UnityEngine;

/// <summary>
/// Seta guia em 3D: um aro de marcas no chão ao redor dos pés do personagem e uma seta que orbita nesse aro,
/// sempre apontando (na horizontal) para o destino. Este componente fica no objeto _Player; o desenho fica num
/// filho "SetaGuia3D" que não gira junto com o personagem. Quase tudo abaixo vale ao vivo (também no editor).
/// </summary>
public class SetaGuia3D : MonoBehaviour
{
    [Header("Posição")]
    [Tooltip("Altura do desenho acima dos pés do personagem (m).")]
    [Range(0f, 2f)] public float alturaDosPes = 0.05f;
    [Tooltip("Distância da SETA ao centro do corpo (m).")]
    [Range(0.3f, 4f)] public float raio = 1.5f;

    [Header("Seta")]
    public bool mostrarSeta = true;
    [Tooltip("Tamanho da seta (1 = padrão).")]
    [Range(0.3f, 4f)] public float tamanhoSeta = 1f;
    [Tooltip("Espessura (altura) da seta (1 = padrão).")]
    [Range(0.2f, 6f)] public float espessuraSeta = 1f;

    [Header("Aro (círculo de marcas)")]
    public bool mostrarAro = true;
    [Tooltip("Raio do aro (m).")]
    [Range(0.3f, 4f)] public float raioAro = 1.5f;
    [Tooltip("Quantidade de marcas do aro.")]
    [Range(8, 96)] public int quantidadeMarcas = 28;
    [Tooltip("Largura de cada marca (m).")]
    [Range(0.02f, 0.5f)] public float larguraMarca = 0.09f;
    [Tooltip("Comprimento de cada marca, ao longo do círculo (m). Valores grandes fecham o aro.")]
    [Range(0.05f, 1.5f)] public float comprimentoMarca = 0.32f;
    [Tooltip("Espessura (altura) das marcas (m).")]
    [Range(0.01f, 0.3f)] public float espessuraAro = 0.04f;

    [Header("Cor e brilho")]
    [Tooltip("Usar sempre a cor abaixo, em vez da cor do módulo de destino.")]
    public bool usarCorFixa = false;
    public Color corFixa = new Color(1f, 0.82f, 0.2f);
    [Tooltip("O quanto a cor do módulo é clareada em direção ao branco, só na seta (0 = cor pura).")]
    [Range(0f, 1f)] public float clarearSeta = 0.3f;
    [Tooltip("Brilho (emissão) da seta.")]
    [Range(0f, 8f)] public float brilhoSeta = 2.6f;
    [Tooltip("Brilho (emissão) do aro.")]
    [Range(0f, 8f)] public float brilhoAro = 1.1f;

    [Header("Movimento")]
    [Tooltip("Velocidade do 'respirar' (pulso).")]
    [Range(0f, 12f)] public float pulsoVelocidade = 4f;
    [Tooltip("Quanto a seta vai e volta ao respirar (m).")]
    [Range(0f, 0.6f)] public float pulsoDistancia = 0.1f;
    [Tooltip("Quanto a seta cresce ao respirar (fração).")]
    [Range(0f, 0.5f)] public float pulsoEscala = 0.08f;
    [Tooltip("Variação extra do brilho da seta ao respirar.")]
    [Range(0f, 6f)] public float pulsoBrilho = 1.2f;
    [Tooltip("Rapidez com que a seta gira para o novo destino (maior = mais rápido).")]
    [Range(1f, 40f)] public float suavizacaoGiro = 14f;

    const string NomeRaiz = "SetaGuia3D";
    const float BaseHasteY = 0.06f;

    Transform _raiz, _seta, _anel;
    Renderer[] _rendSeta, _rendAnel;
    Material _matSeta, _matAnel;
    Vector3 _dir = Vector3.forward;
    bool _temDir;
    float[] _ultimos;

    /// <summary>Posição da seta no mundo (para o texto do HUD ficar logo abaixo).</summary>
    public Vector3 Posicao => _seta != null ? _seta.position : transform.position;

    /// <summary>Usa o componente que está no _Player; se não houver, põe um no jogador (ou cria um solto).</summary>
    public static SetaGuia3D Criar()
    {
        var existente = FindObjectOfType<SetaGuia3D>(true);
        if (existente == null)
        {
            var jog = FindObjectOfType<Movimento>();
            var go = jog != null ? jog.gameObject : new GameObject("SetaGuia3D_solta");
            existente = go.AddComponent<SetaGuia3D>();
        }
        existente.Preparar();
        existente.Esconder();
        return existente;
    }

    void Awake()
    {
        if (!Application.isPlaying) return;
        Preparar();
        Esconder(); // só aparece quando há um destino (UIJogo chama Atualizar)
    }

    // ---------------- Construção ----------------

    static GameObject Peca(PrimitiveType tipo, string nome, Transform pai, Vector3 pos, Vector3 escala, float yaw = 0f)
    {
        var g = GameObject.CreatePrimitive(tipo);
        g.name = nome;
        var col = g.GetComponent<Collider>();            // nunca bloqueia nem empurra nada
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        g.transform.SetParent(pai, false);
        g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        g.transform.localScale = escala;
        var r = g.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return g;
    }

    /// <summary>Constrói o desenho (filho "SetaGuia3D" com Seta + Aro). Também chamado pelo editor para ficar salvo na cena.</summary>
    public void Montar()
    {
        var velho = transform.Find(NomeRaiz);
        if (velho != null) { if (Application.isPlaying) Destroy(velho.gameObject); else DestroyImmediate(velho.gameObject); }

        var raiz = new GameObject(NomeRaiz).transform; raiz.SetParent(transform, false);

        // Seta deitada no chão apontando para +Z: haste + ponta em losango (tamanhos de referência; o ajuste fino é por escala).
        var seta = new GameObject("Seta").transform; seta.SetParent(raiz, false);
        Peca(PrimitiveType.Cube, "Haste", seta, new Vector3(0, 0, -0.05f), new Vector3(0.26f, BaseHasteY, 0.75f));
        Peca(PrimitiveType.Cube, "Ponta", seta, new Vector3(0, 0, 0.5f), new Vector3(0.62f, BaseHasteY, 0.62f), 45f);

        var anel = new GameObject("Anel").transform; anel.SetParent(raiz, false);
        ReconstruirAro(anel);

        _ultimos = null;
        _raiz = raiz; _seta = seta; _anel = anel;
        AplicarGeometria();
    }

    void ReconstruirAro(Transform anel)
    {
        for (int i = anel.childCount - 1; i >= 0; i--)
        {
            var f = anel.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(f); else DestroyImmediate(f);
        }
        int n = Mathf.Max(8, quantidadeMarcas);
        for (int i = 0; i < n; i++) Peca(PrimitiveType.Cube, "Marca", anel, Vector3.zero, Vector3.one);
    }

    float PesLocalY()
    {
        var cc = GetComponent<CharacterController>();
        return cc != null ? cc.center.y - cc.height * 0.5f : -1f;
    }

    /// <summary>Liga as referências ao desenho (monta se faltar) e cria os materiais de execução.</summary>
    void Preparar()
    {
        var r = transform.Find(NomeRaiz);
        if (r == null || r.Find("Seta") == null || r.Find("Anel") == null) { Montar(); r = transform.Find(NomeRaiz); }
        Ligar(r);
        AplicarGeometria();
        if (Application.isPlaying) CriarMateriais();
    }

    void Ligar(Transform r)
    {
        _raiz = r; _seta = r.Find("Seta"); _anel = r.Find("Anel");
        _rendSeta = _seta.GetComponentsInChildren<Renderer>();
        _rendAnel = _anel.GetComponentsInChildren<Renderer>();
    }

    void CriarMateriais()
    {
        _matSeta = new Material(_rendSeta[0].sharedMaterial);
        _matAnel = new Material(_rendSeta[0].sharedMaterial);
        foreach (var r in _rendSeta) r.sharedMaterial = _matSeta;
        foreach (var r in _rendAnel) r.sharedMaterial = _matAnel;
    }

    /// <summary>Só para capturas no editor, em um objeto temporário (nunca na seta salva na cena).</summary>
    public void PrepararParaCaptura() { Preparar(); CriarMateriais(); }

    // ---------------- Ajustes ao vivo ----------------

    /// <summary>Aplica tamanhos, quantidade de marcas e visibilidade; só refaz o que mudou.</summary>
    void AplicarGeometria()
    {
        if (_raiz == null || _seta == null || _anel == null) return;
        var atual = new float[] { raio, tamanhoSeta, espessuraSeta, raioAro, quantidadeMarcas, larguraMarca, comprimentoMarca, espessuraAro, alturaDosPes, mostrarSeta ? 1 : 0, mostrarAro ? 1 : 0 };
        if (_ultimos != null && _ultimos.Length == atual.Length)
        {
            bool igual = true;
            for (int i = 0; i < atual.Length; i++) if (!Mathf.Approximately(_ultimos[i], atual[i])) { igual = false; break; }
            if (igual) return;
        }
        _ultimos = atual;

        // quantidade de marcas mudou → reconstrói o aro (e religa os renderers)
        int n = Mathf.Max(8, quantidadeMarcas);
        if (_anel.childCount != n)
        {
            ReconstruirAro(_anel);
            _rendAnel = _anel.GetComponentsInChildren<Renderer>();
            if (Application.isPlaying && _matAnel != null) foreach (var r in _rendAnel) r.sharedMaterial = _matAnel;
        }

        _seta.localScale = new Vector3(tamanhoSeta, tamanhoSeta * espessuraSeta, tamanhoSeta);
        _seta.localPosition = new Vector3(0, 0, raio);
        _seta.gameObject.SetActive(mostrarSeta);
        _anel.gameObject.SetActive(mostrarAro);

        for (int i = 0; i < _anel.childCount; i++)
        {
            float a = i * Mathf.PI * 2f / n;
            var m = _anel.GetChild(i);
            m.localPosition = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * raioAro;
            m.localRotation = Quaternion.Euler(0, a * Mathf.Rad2Deg + 90f, 0);
            m.localScale = new Vector3(larguraMarca, espessuraAro, comprimentoMarca);
        }
        _raiz.localPosition = new Vector3(0, PesLocalY() + alturaDosPes, 0);
    }

    // Mudanças no Inspector (editor): aplicadas na hora para ver o resultado sem dar Play.
    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null || Application.isPlaying && _raiz == null) return;
            var r = transform.Find(NomeRaiz);
            if (r == null || r.Find("Seta") == null || r.Find("Anel") == null) return;
            if (_raiz != r) Ligar(r);
            AplicarGeometria();
        };
#endif
    }

    static void Pintar(Material m, Color cor, float brilho)
    {
        if (m == null) return; // fora do Play a seta da cena não tem materiais de execução
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
        if (m.HasProperty("_Color")) m.SetColor("_Color", cor);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", cor * brilho);
    }

    // ---------------- Execução ----------------

    /// <summary>Mostra/atualiza a seta: 'jogador' é o personagem, 'alvo' o ponto de destino no mundo.</summary>
    public void Atualizar(Transform jogador, Vector3 alvo, Color cor)
    {
        if (jogador == null) { Esconder(); return; }
        if (_raiz == null) Preparar();
        AplicarGeometria(); // vale ao vivo, também durante o Play
        if (!_raiz.gameObject.activeSelf) { _raiz.gameObject.SetActive(true); _temDir = false; }

        Vector3 para = alvo - jogador.position; para.y = 0;
        if (para.sqrMagnitude > 0.0001f)
        {
            Vector3 nova = para.normalized;
            _dir = _temDir ? Vector3.Slerp(_dir, nova, 1f - Mathf.Exp(-suavizacaoGiro * Time.deltaTime)) : nova;
            _temDir = true;
        }

        // pés do personagem (base do CharacterController) + a altura configurada; sem girar com o corpo
        float yPes = jogador.position.y;
        var cc = jogador.GetComponent<CharacterController>();
        if (cc != null) yPes += cc.center.y - cc.height * 0.5f;
        _raiz.position = new Vector3(jogador.position.x, yPes + alturaDosPes, jogador.position.z);
        _raiz.rotation = Quaternion.identity;

        // a seta orbita no aro e aponta para fora (para o destino), com um leve "respirar"
        float pulso = 0.5f + 0.5f * Mathf.Sin(Time.time * pulsoVelocidade);
        _seta.localPosition = _dir * (raio + pulsoDistancia * pulso);
        _seta.localRotation = Quaternion.LookRotation(_dir, Vector3.up);
        float esc = 1f + pulsoEscala * pulso;
        _seta.localScale = new Vector3(tamanhoSeta * esc, tamanhoSeta * espessuraSeta * esc, tamanhoSeta * esc);

        Color baseCor = usarCorFixa ? corFixa : cor;
        var corSeta = Color.Lerp(baseCor, Color.white, clarearSeta); corSeta.a = 1f;
        Pintar(_matSeta, corSeta, brilhoSeta + pulsoBrilho * pulso);
        var corAnel = baseCor; corAnel.a = 1f;
        Pintar(_matAnel, corAnel, brilhoAro);
    }

    public void Esconder()
    {
        var r = _raiz != null ? _raiz : transform.Find(NomeRaiz);
        if (r != null && r.gameObject.activeSelf) r.gameObject.SetActive(false);
    }
}
