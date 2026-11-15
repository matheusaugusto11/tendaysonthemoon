using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Os três medidores do HUD, cada um com a sua forma (design D11 da mudança interface-e-minijogos).</summary>
public enum TipoMedidor { Oxigenio, Radiacao, Bateria }

/// <summary>
/// Medidor do HUD: ícone, forma própria, sigla e número. Oxigênio em 10 segmentos, Radiação em barra com ponteiro e faixa
/// hachurada na zona de alerta, Bateria em 4 células. Quando o valor muda ele anda até o novo em 600 ms (os segmentos
/// acendem ou apagam um a um e o número conta junto) e solta uma variação flutuante. Entrando na zona de alerta
/// (Oxigênio 3 ou menos, Radiação 7 ou mais) a forma fica vermelha e aparece o triângulo de alerta, sem piscar.
/// </summary>
public class MedidorHud : MonoBehaviour
{
    public TipoMedidor tipo;
    public int maximo;

    // Estado, também lido pela sonda de teste.
    public int Alvo { get; private set; }
    public float Mostrado { get; private set; }
    public bool EmAlerta { get; private set; }
    public bool Iniciado { get; private set; }
    public Color CorDoPreenchimento { get; private set; }
    public bool AlertaVisivel => _alerta != null && _alerta.enabled;
    public bool ZonaVisivel => _zona != null && _zona.gameObject.activeInHierarchy && _zona.color.a > 0.05f;
    public string NumeroTexto => _numero != null ? _numero.text : "";
    public VariacaoFlutuante VariacaoAtual => _variacao;
    public int SegmentosCheios
    {
        get
        {
            int n = 0;
            if (_celulas != null) foreach (var c in _celulas) if (c.color != UiTema.Vazio) n++;
            return n;
        }
    }

    UIJogo _ui;
    FormaUI[] _celulas;
    FormaUI _preench, _zona;
    RectTransform _preenchRt, _ponteiroRt;
    Text _numero;
    Image _alerta, _iconeImg;
    VariacaoFlutuante _variacao;
    float _de, _inicio;

    string IconeNormal => tipo == TipoMedidor.Oxigenio ? "oxigenio" : tipo == TipoMedidor.Radiacao ? "radiacao" : "mod_bateria";
    Color CorNormal => tipo == TipoMedidor.Oxigenio ? UiTema.Ar : tipo == TipoMedidor.Radiacao ? UiTema.Abrigo : UiTema.Energia;

    // ---------- Montagem ----------

    public static MedidorHud Criar(UIJogo ui, Transform pai, TipoMedidor tipo)
    {
        var h = ui.Horizontal(pai, UiTema.E8, false, 0, null, TextAnchor.MiddleLeft);
        h.name = "Medidor_" + tipo;
        var m = h.gameObject.AddComponent<MedidorHud>();
        m._ui = ui;
        m.tipo = tipo;
        m.maximo = tipo == TipoMedidor.Oxigenio ? Missao.MaxO2 : tipo == TipoMedidor.Radiacao ? Missao.MaxRad : Missao.MaxBat;
        m._iconeImg = ui.Icone(h, m.IconeNormal, 48);

        // Coluna: a forma em cima, a sigla embaixo (O₂ usa a Nunito, que tem o subscrito; a Barlow não tem).
        var col = ui.Vertical(h, 6, 0, null, TextAnchor.MiddleLeft);
        string sigla = tipo == TipoMedidor.Oxigenio ? "O₂" : tipo == TipoMedidor.Radiacao ? "RAD" : "BAT";
        switch (tipo)
        {
            case TipoMedidor.Oxigenio: m.CriarCelulas(col, 10, 18f, true); break;
            case TipoMedidor.Bateria: m.CriarCelulas(col, 4, 28f, false); break;
            default: m.CriarBarra(col, 200f); break;
        }
        ui.Rot(col, sigla, Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleLeft);

        m._numero = ui.Rot(h, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        UIJogo.Tam(m._numero, 44, -1);
        if (tipo != TipoMedidor.Bateria) m._alerta = ui.Icone(h, "alerta", 40); // o lugar fica reservado: o HUD não pula

        float forma = tipo == TipoMedidor.Oxigenio ? 234f : tipo == TipoMedidor.Radiacao ? 200f : 130f;
        UIJogo.Tam(h, 48f + 8f + forma + 8f + 44f + (tipo == TipoMedidor.Bateria ? 0f : 8f + 40f), -1);
        m.Aplicar();
        return m;
    }

    /// <summary>Fileira de células chanfradas de 44 de altura. No Oxigênio, um traço vermelho fino marca as 3 últimas (zona de alerta).</summary>
    void CriarCelulas(RectTransform col, int n, float larg, bool comZona)
    {
        var linha = _ui.Horizontal(col, 6, false, 0, null, TextAnchor.MiddleLeft);
        UIJogo.Tam(linha, -1, 44);
        _celulas = new FormaUI[n];
        for (int i = 0; i < n; i++)
        {
            var c = FormaUI.Chanfrado(linha, UiTema.Vazio, UiTema.ChanfroCelula, "SE ID", "Celula");
            UIJogo.Tam(c, larg, 44);
            _celulas[i] = c;
        }
        if (!comZona) return;
        // A zona de alerta fica marcada mesmo antes de ser alcançada: o traço fica sob as três primeiras (valores 1 a 3).
        _zona = FormaUI.Criar(linha, FormaUI.Modo.Chanfrado, UiTema.Alerta, "ZonaDeAlerta");
        _zona.chanfro = 0; _zona.cantoSE = _zona.cantoID = false;
        _zona.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        var r = _zona.rectTransform;
        r.anchorMin = r.anchorMax = Vector2.zero; r.pivot = new Vector2(0, 1);
        r.sizeDelta = new Vector2(3 * larg + 2 * 6f, 4f);
        r.anchoredPosition = new Vector2(0, -2f);
    }

    /// <summary>Barra com fundo, faixa hachurada na zona de alerta (7 a 10), preenchimento e ponteiro em triângulo.</summary>
    void CriarBarra(RectTransform col, float larg)
    {
        var barra = UIJogo.RT("Barra", col);
        UIJogo.Tam(barra, larg, 44);
        var fundo = FormaUI.Criar(barra, FormaUI.Modo.Chanfrado, UiTema.Vazio, "Fundo");
        fundo.chanfro = 0; fundo.cantoSE = fundo.cantoID = false;
        Ancorar(fundo.rectTransform, 0f, 0.25f, 1f, 0.75f);

        float inicioZona = Apresentacao.RadiacaoAlerta / (float)Missao.MaxRad;
        _zona = FormaUI.Criar(barra, FormaUI.Modo.Hachurada, UiTema.Alerta, "ZonaDeAlerta");
        _zona.larguraFaixa = 5f; _zona.periodo = 12f;
        Ancorar(_zona.rectTransform, inicioZona, 0.25f, 1f, 0.75f);

        _preench = FormaUI.Criar(barra, FormaUI.Modo.Chanfrado, UiTema.Abrigo, "Preenchimento");
        _preench.chanfro = 0; _preench.cantoSE = _preench.cantoID = false;
        _preenchRt = _preench.rectTransform;
        Ancorar(_preenchRt, 0f, 0.25f, 0f, 0.75f);

        var ponteiro = FormaUI.Criar(barra, FormaUI.Modo.Triangulo, UiTema.Branco, "Ponteiro");
        ponteiro.rotacao = 180f; // aponta para baixo, para a barra
        _ponteiroRt = ponteiro.rectTransform;
        _ponteiroRt.pivot = new Vector2(0.5f, 0f);
        _ponteiroRt.sizeDelta = new Vector2(20f, 16f);
        _ponteiroRt.anchoredPosition = new Vector2(0f, 1f);
    }

    static void Ancorar(RectTransform r, float x0, float y0, float x1, float y1)
    {
        r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    // ---------- Valor ----------

    /// <summary>
    /// Pede o valor do medidor. Na primeira vez (ou com <paramref name="instantaneo"/>, numa missão nova) ele salta direto;
    /// depois, cada mudança anda até o novo valor e solta a variação flutuante.
    /// </summary>
    public void Definir(int valor, bool instantaneo = false)
    {
        valor = Mathf.Clamp(valor, 0, maximo);
        // Primeira leitura ou missão nova: salta direto, sem andar e sem variação.
        if (!Iniciado || instantaneo)
        {
            Alvo = valor; Mostrado = _de = valor; Iniciado = true;
            if (_variacao != null) Destroy(_variacao.gameObject);
            Aplicar();
            return;
        }
        if (valor == Alvo) return; // nada mudou: não mexe na variação que ainda está na tela
        int delta = valor - Alvo;
        if (UiTema.MovimentoReduzido)
        {
            // Movimento reduzido: o medidor salta direto para o valor novo; a variação só aparece e some.
            Alvo = valor; Mostrado = _de = valor;
            Aplicar();
        }
        else
        {
            _de = Mostrado; _inicio = Time.unscaledTime; Alvo = valor;
        }
        if (_variacao != null) Destroy(_variacao.gameObject); // uma por medidor: a nova substitui a anterior
        _variacao = VariacaoFlutuante.Criar(_ui, (RectTransform)transform, IconeNormal, delta, tipo);
    }

    void Update()
    {
        if (!Iniciado || Mathf.Approximately(Mostrado, Alvo)) return;
        float t = Mathf.Clamp01((Time.unscaledTime - _inicio) / UiTema.TempoMedidor);
        float suave = t * t * (3f - 2f * t); // entra e sai devagar
        Mostrado = t >= 1f ? Alvo : Mathf.Lerp(_de, Alvo, suave);
        Aplicar();
    }

    void Aplicar()
    {
        int cheio = Mathf.RoundToInt(Mostrado); // o número e os segmentos andam juntos, de um em um
        bool alerta = tipo == TipoMedidor.Oxigenio ? Apresentacao.OxigenioEmAlerta(cheio)
                    : tipo == TipoMedidor.Radiacao ? Apresentacao.RadiacaoEmAlerta(cheio) : false;
        EmAlerta = alerta;
        Color cor = alerta ? UiTema.Alerta : CorNormal;
        CorDoPreenchimento = cor;
        if (_celulas != null)
            for (int i = 0; i < _celulas.Length; i++) _celulas[i].color = i < cheio ? cor : UiTema.Vazio;
        if (_preenchRt != null)
        {
            float f = Mathf.Clamp01(Mostrado / maximo);
            _preenchRt.anchorMax = new Vector2(f, 0.75f);
            _preench.color = cor;
            _ponteiroRt.anchorMin = _ponteiroRt.anchorMax = new Vector2(f, 0.75f);
        }
        if (_numero != null) _numero.text = cheio.ToString();
        if (_alerta != null) _alerta.enabled = alerta;
        if (tipo == TipoMedidor.Radiacao && _iconeImg != null) _iconeImg.sprite = UiTema.Icone(alerta ? "radiacao_alerta" : "radiacao");
    }
}

/// <summary>
/// Variação que sobe e some junto de um medidor: ícone, triângulo para cima ou para baixo e o número. Dura pouco mais de
/// 1 segundo (tempo real, não para na pausa) e se apaga sozinha; nada fica preso na tela.
/// </summary>
public class VariacaoFlutuante : MonoBehaviour
{
    public const float Duracao = 1.3f, Deriva = 28f, Inicio = -20f;

    public int Delta { get; private set; }
    public TipoMedidor Medidor { get; private set; }
    public bool TrianguloParaBaixo { get; private set; }

    RectTransform _rt;
    CanvasGroup _grupo;
    float _t0;

    public static VariacaoFlutuante Criar(UIJogo ui, RectTransform pai, string icone, int delta, TipoMedidor medidor)
    {
        var rt = ui.Horizontal(pai, UiTema.E8, false, 12, null, TextAnchor.MiddleCenter);
        rt.name = "Variacao_" + medidor;
        UIJogo.Fundo(rt, UiTema.Aco, UiTema.ChanfroBotao, "SE ID", true);
        var v = rt.gameObject.AddComponent<VariacaoFlutuante>();
        v.Delta = delta; v.Medidor = medidor; v._rt = rt; v._t0 = Time.unscaledTime;

        var le = rt.gameObject.AddComponent<LayoutElement>(); le.ignoreLayout = true;
        var fit = rt.gameObject.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        v._grupo = rt.gameObject.AddComponent<CanvasGroup>();
        v._grupo.blocksRaycasts = false; v._grupo.interactable = false;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, Inicio);

        ui.Icone(rt, icone, 36);
        var tri = FormaUI.Criar(rt, FormaUI.Modo.Triangulo, UiTema.Branco, "Sentido");
        tri.rotacao = delta > 0 ? 0f : 180f; // o sentido vem da forma, nunca da cor
        v.TrianguloParaBaixo = delta < 0;
        var tl = tri.gameObject.AddComponent<LayoutElement>(); tl.minWidth = tl.preferredWidth = 22; tl.minHeight = tl.preferredHeight = 20;
        ui.Rot(rt, Mathf.Abs(delta).ToString(), Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        return v;
    }

    void Update()
    {
        float t = (Time.unscaledTime - _t0) / Duracao;
        if (t >= 1f) { Destroy(gameObject); return; }
        float sai = 1f - (1f - t) * (1f - t); // desacelera ao sair
        float deriva = UiTema.MovimentoReduzido ? 0f : Deriva; // com movimento reduzido ela só some, parada
        _rt.anchoredPosition = new Vector2(0f, Inicio - deriva * sai);
        _grupo.alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
    }
}
