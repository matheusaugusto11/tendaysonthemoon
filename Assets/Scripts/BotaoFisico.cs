using DezDias;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Botão com corpo (design D6 da mudança interface-e-minijogos). É um Button do uGUI, então mouse, toque e controle usam os mesmos
/// eventos. A face fica sobre uma sombra dura; ao pressionar, a face desce 6, a sombra some e a cor escurece um degrau em 80 ms.
/// O retângulo do botão (e a área de toque) nunca muda, então o layout não pula. Estados: normal, pressionado, desativado
/// (cinza, cadeado e o motivo em texto) e foco (moldura tracejada, só quando a seleção veio de teclado ou controle).
/// </summary>
public class BotaoFisico : Button
{
    public enum Estilo { Principal, Secundario, Escolha }

    public Estilo estilo;
    FormaUI _face, _sombra, _foco, _tira;
    RectTransform _faceRt;
    Text _rotulo;
    Image _icone;
    Image _area; // área de toque fixa, invisível
    string _texto = "", _motivo = "";
    string _nomeIcone;
    Color _corTira;
    float _afundo;                  // 0 normal, 1 pressionado
    bool _pressionado, _selecionado, _forcaFoco, _focoMostrado, _ajustar;

    /// <summary>Onde pôr conteúdo próprio (ícones, chips) dentro do botão; desce junto com a face.</summary>
    public RectTransform Face => _faceRt;
    public Text Rotulo => _rotulo;
    /// <summary>A moldura de foco está visível agora.</summary>
    public bool FocoVisivel => _foco != null && _foco.gameObject.activeSelf;

    // ---------- Último dispositivo: a moldura de foco é para teclado e controle, não para mouse e dedo ----------

    /// <summary>
    /// Verdadeiro depois de uma tecla, do D-pad ou do analógico; falso depois de o mouse se mexer ou clicar e de um toque.
    /// O jogo seleciona botões por código (Entrada.Selecionar), então só a seleção não diz quem está jogando.
    /// </summary>
    public static bool NavegacaoPorTeclado { get; set; }
    static int _frameEntrada = -1;
    static Vector3 _mouseAntes;

    /// <summary>Só para testes: ignora o mouse, o teclado e o controle de verdade, para o teste decidir sozinho quem está jogando.</summary>
    public static bool IgnorarEntradaReal { get; set; }

    static void LerEntrada()
    {
        if (IgnorarEntradaReal || _frameEntrada == Time.frameCount) return;
        _frameEntrada = Time.frameCount;
        Vector3 mouse = Input.mousePosition;
        bool mouseAtivo = (mouse - _mouseAntes).sqrMagnitude > 4f || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
        _mouseAntes = mouse;
        if (mouseAtivo) { NavegacaoPorTeclado = false; return; }
        bool teclado = (Input.anyKeyDown && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)) || Entrada.Mover.sqrMagnitude > 0.25f;
#if ENABLE_INPUT_SYSTEM
        var g = UnityEngine.InputSystem.Gamepad.current;
        if (g != null && (g.buttonSouth.wasPressedThisFrame || g.dpad.ReadValue().sqrMagnitude > 0.1f)) teclado = true;
#endif
        if (teclado) NavegacaoPorTeclado = true;
    }

    bool DeveMostrarFoco => _selecionado && interactable && (_forcaFoco || NavegacaoPorTeclado);
    public float Afundo => _afundo;
    public bool Afundado => _afundo > 0.99f;
    public string Motivo => _motivo;

    bool _semCadeado;
    /// <summary>Botões de passo (−, +) desativados só ficam cinza: o motivo já está escrito no cartão deles.</summary>
    public bool SemCadeado { get => _semCadeado; set { _semCadeado = value; Aplicar(true); } }
    public FormaUI Sombra => _sombra;

    // ---------- Criação ----------

    public static Vector2 TamanhoPadrao(Estilo e) => e == Estilo.Principal ? new Vector2(400f, 104f) : e == Estilo.Secundario ? new Vector2(266f, 96f) : new Vector2(760f, 210f);

    public static BotaoFisico Criar(Transform pai, Estilo estilo, string texto, string icone = null, UnityAction aoClicar = null, Color? tira = null, string nome = null)
    {
        var go = new GameObject(nome ?? "Botao_" + estilo, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = TamanhoPadrao(estilo);

        // Área de toque: invisível, mas sem corte de malha transparente para continuar recebendo o raio.
        var area = go.GetComponent<Image>();
        area.color = new Color(0, 0, 0, 0);
        go.GetComponent<CanvasRenderer>().cullTransparentMesh = false;

        var b = go.AddComponent<BotaoFisico>();
        b.estilo = estilo; b._area = area; b._nomeIcone = icone; b._corTira = tira ?? UiTema.Branco; b._texto = texto ?? "";
        b.targetGraphic = area;
        b.transition = Transition.None;
        var nav = b.navigation; nav.mode = Navigation.Mode.Automatic; b.navigation = nav;
        b.Montar();
        if (aoClicar != null) b.onClick.AddListener(aoClicar);
        b.Aplicar(true);
        return b;
    }

    static RectTransform Filho(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    void Montar()
    {
        float c = UiTema.ChanfroBotao;
        // Sombra dura: a mesma forma da face, 6 abaixo. Fica fora do retângulo do botão (não entra no layout).
        var s = Filho("Sombra", transform);
        s.offsetMin = new Vector2(0, -UiTema.Sombra_Y); s.offsetMax = new Vector2(0, -UiTema.Sombra_Y);
        _sombra = s.gameObject.AddComponent<FormaUI>(); Preparar(_sombra, c);

        _faceRt = Filho("Face", transform);
        _face = _faceRt.gameObject.AddComponent<FormaUI>(); Preparar(_face, c);

        if (estilo == Estilo.Escolha)
        {
            // Tira de luz da cor do sistema, no topo, a partir do canto chanfrado.
            var t = Filho("Tira", _faceRt);
            t.anchorMin = new Vector2(0, 1); t.anchorMax = new Vector2(1, 1); t.pivot = new Vector2(0.5f, 1);
            t.offsetMin = new Vector2(c - 2f, -8f); t.offsetMax = new Vector2(0, 0);
            _tira = t.gameObject.AddComponent<FormaUI>(); _tira.chanfro = 0; _tira.cantoSE = _tira.cantoID = false; _tira.raycastTarget = false;
        }
        else
        {
            var ic = new GameObject("Icone", typeof(RectTransform), typeof(Image));
            ic.transform.SetParent(_faceRt, false);
            _icone = ic.GetComponent<Image>(); _icone.preserveAspect = true; _icone.raycastTarget = false;
            var r = (RectTransform)ic.transform;
            bool direita = estilo == Estilo.Principal;
            r.anchorMin = r.anchorMax = new Vector2(direita ? 1f : 0f, 0.5f); r.pivot = new Vector2(direita ? 1f : 0f, 0.5f);
            r.sizeDelta = new Vector2(direita ? 52f : 48f, direita ? 52f : 48f);
            r.anchoredPosition = new Vector2(direita ? -UiTema.E24 : UiTema.E24, 0f);
        }

        var tx = new GameObject("Rotulo", typeof(RectTransform), typeof(Text));
        tx.transform.SetParent(_faceRt, false);
        _rotulo = tx.GetComponent<Text>();
        var rr = (RectTransform)tx.transform;
        rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one;
        if (estilo == Estilo.Escolha) { rr.offsetMin = new Vector2(UiTema.E32, UiTema.E12); rr.offsetMax = new Vector2(-UiTema.E32, -UiTema.E16); }
        else
        {
            float esq = estilo == Estilo.Principal ? 30f : (_nomeIcone != null ? 24f + 48f + 12f : 24f);
            float dir = estilo == Estilo.Principal ? 24f + 52f + 12f : 24f;
            rr.offsetMin = new Vector2(esq, 0f); rr.offsetMax = new Vector2(-dir, 0f);
        }
        _rotulo.raycastTarget = false;
        _rotulo.alignment = estilo == Estilo.Escolha ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
        // Sem best fit: ele ignora as tags <size> do texto rico e quebra a linha em vez de reduzir. AjustarTamanho escolhe entre tamanhos fixos.
        _rotulo.horizontalOverflow = estilo == Estilo.Escolha ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        _rotulo.verticalOverflow = VerticalWrapMode.Overflow;
        _rotulo.supportRichText = true;
        _rotulo.resizeTextForBestFit = false;

        // Moldura de foco: fora da face, a 12 de distância, só aparece com seleção por teclado ou controle.
        var f = Filho("Foco", transform);
        f.offsetMin = new Vector2(-12f, -12f); f.offsetMax = new Vector2(12f, 12f);
        _foco = f.gameObject.AddComponent<FormaUI>();
        _foco.modo = FormaUI.Modo.Moldura; _foco.color = UiTema.Branco; _foco.raycastTarget = false;
        _foco.espessura = 4f; _foco.larguraFaixa = 14f; _foco.periodo = 24f;
        _foco.gameObject.SetActive(false);
    }

    static void Preparar(FormaUI f, float chanfro)
    {
        f.modo = FormaUI.Modo.Chanfrado; f.chanfro = chanfro; f.cantoSE = true; f.cantoID = true; f.cantoSD = f.cantoIE = false;
        f.raycastTarget = false;
    }

    // ---------- Aparência ----------

    Color FaceNormal => !interactable ? UiTema.DesativadoFace : estilo == Estilo.Principal ? UiTema.Branco : UiTema.AcoClaro;
    Color SombraCor => !interactable ? UiTema.DesativadoSombra : estilo == Estilo.Principal ? UiTema.BrancoSombra : UiTema.Sombra;
    Color TextoCor => !interactable ? UiTema.Discreto : estilo == Estilo.Principal ? UiTema.Tinta : UiTema.Branco;
    /// <summary>Um degrau mais escuro, para o botão pressionado.</summary>
    Color FacePressionada => Color.Lerp(FaceNormal, Color.black, 0.14f);

    /// <summary>Texto do botão. Desativado com motivo mostra o motivo numa segunda linha.</summary>
    public void Texto(string texto) { _texto = texto ?? ""; Aplicar(true); }

    /// <summary>Desativa o botão e escreve o motivo nele (nunca some sem explicação).</summary>
    public void Desativar(string motivo) { _motivo = motivo ?? ""; interactable = false; Aplicar(true); }

    public void Ativar() { _motivo = ""; interactable = true; Aplicar(true); }

    /// <summary>Só para capturas e testes: fixa o botão como pressionado (ou solto) na hora, sem animação.</summary>
    public void ForcarPressionado(bool sim) { _pressionado = sim; _afundo = sim ? 1f : 0f; Aplicar(true); }

    /// <summary>Só para capturas e testes: mostra a moldura de foco como se a seleção viesse do teclado.</summary>
    public void ForcarFoco(bool sim) { _selecionado = sim; _forcaFoco = sim; Aplicar(true); }

    void Aplicar(bool tudo)
    {
        if (_face == null) return;
        float t = _afundo;
        _faceRt.anchoredPosition = new Vector2(0f, -UiTema.Sombra_Y * t);
        _face.color = Color.Lerp(FaceNormal, FacePressionada, t);
        var sc = SombraCor; sc.a = 1f - t;
        _sombra.color = sc;
        if (!tudo) return;
        bool des = !interactable;
        string linha = _texto;
        if (des && !string.IsNullOrEmpty(_motivo))
            linha = $"{_texto}\n<size={UiTema.Tamanho(Papel.Rotulo)}>{_motivo}</size>";
        _rotulo.text = linha;
        _rotulo.font = UiTema.Fonte(Papel.TituloPainel, _texto + _motivo);
        _rotulo.color = TextoCor;
        bool temIcone = false;
        if (_icone != null)
        {
            string nomeIcone = des ? (_semCadeado ? null : "cadeado") : _nomeIcone;
            _icone.sprite = UiTema.Icone(nomeIcone);
            _icone.enabled = _icone.sprite != null;
            _icone.color = Color.white;
            temIcone = _icone.enabled;
            if (estilo == Estilo.Secundario)
            {
                bool soIcone = string.IsNullOrEmpty(_texto) && temIcone;
                var r = _icone.rectTransform;
                r.anchorMin = r.anchorMax = r.pivot = new Vector2(soIcone ? 0.5f : 0f, 0.5f);
                r.anchoredPosition = new Vector2(soIcone ? 0f : UiTema.E24, 0f);
                // O texto abre espaço para o ícone quando ele existe (o cadeado aparece depois de o botão ser criado).
                var rr = (RectTransform)_rotulo.transform;
                rr.offsetMin = new Vector2(temIcone && !soIcone ? UiTema.E24 + 48f + UiTema.E12 : UiTema.E24, rr.offsetMin.y);
            }
        }
        AjustarTamanho();
        if (_tira != null) _tira.color = des ? UiTema.Discreto : _corTira;
        _foco.gameObject.SetActive(_focoMostrado = DeveMostrarFoco);
    }

    /// <summary>Maior tamanho de um passo fixo (40, 34, 30, 24) em que o texto cabe numa linha. Nunca abaixo de 24.</summary>
    static readonly int[] Passos = { 40, 34, 30, 24 };

    int _tamanhoMaximo = 40;
    /// <summary>Maior tamanho de texto que o botão usa (a barra do HUD usa 30, para não passar dos números dos medidores).</summary>
    public int TamanhoMaximo { get => _tamanhoMaximo; set { _tamanhoMaximo = value; AjustarTamanho(); } }

    /// <summary>Largura do botão (e portanto do rótulo) ainda não resolvida pelo layout: o chamador diz quanto vale.</summary>
    public void AjustarPara(float larguraDoBotao)
    {
        if (_rotulo == null || estilo == Estilo.Escolha) return;
        _larguraDada = larguraDoBotao;
        AjustarTamanho();
    }

    float _larguraDada;

    void AjustarTamanho()
    {
        if (_rotulo == null) return;
        if (estilo == Estilo.Escolha) { if (_rotulo.fontSize != UiTema.Tamanho(Papel.TituloPainel)) _rotulo.fontSize = UiTema.Tamanho(Papel.TituloPainel); return; }
        var rr = (RectTransform)_rotulo.transform;
        // Com a largura dita pelo chamador usa-se ela (o layout ainda não resolveu); senão, a que o layout já resolveu.
        // A margem esquerda (offsetMin.x) é positiva para dentro e a direita (offsetMax.x) é negativa para dentro.
        Escolher(_larguraDada > 0f ? _larguraDada - rr.offsetMin.x + rr.offsetMax.x : rr.rect.width);
    }

    /// <summary>
    /// Mede com o gerador de layout do próprio Text, sem mudar o Text enquanto testa: mexer no tamanho dentro de uma
    /// reconstrução de layout faz a Unity acusar um loop de reconstrução.
    /// </summary>
    void Escolher(float larg)
    {
        if (larg <= 0f) return;
        var gen = _rotulo.cachedTextGeneratorForLayout;
        var cfg = _rotulo.GetGenerationSettings(Vector2.zero);
        cfg.resizeTextForBestFit = false;
        int escolhido = -1;
        foreach (int p in Passos)
        {
            if (p > _tamanhoMaximo) continue;
            cfg.fontSize = p;
            if (gen.GetPreferredWidth(_rotulo.text, cfg) / _rotulo.pixelsPerUnit <= larg) { escolhido = p; break; }
        }
        // Não cabe numa linha nem no menor passo: quebra em duas linhas no tamanho de corpo, sem alargar o botão.
        var modo = escolhido < 0 ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        if (escolhido < 0) escolhido = UiTema.Tamanho(Papel.Corpo);
        if (_rotulo.horizontalOverflow != modo) _rotulo.horizontalOverflow = modo;
        if (_rotulo.fontSize != escolhido) _rotulo.fontSize = escolhido;
    }

    // Não ajusta aqui: este evento chega durante a reconstrução do layout. O Update (antes da reconstrução seguinte) ajusta.
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        _ajustar = true;
    }

    void Update()
    {
        LerEntrada();
        if (_ajustar) { _ajustar = false; AjustarTamanho(); }
        if (DeveMostrarFoco != _focoMostrado && _foco != null) _foco.gameObject.SetActive(_focoMostrado = DeveMostrarFoco);
        float alvo = _pressionado && interactable ? 1f : 0f;
        if (Mathf.Approximately(_afundo, alvo)) return;
        _afundo = Mathf.MoveTowards(_afundo, alvo, Time.unscaledDeltaTime / UiTema.TempoToque);
        Aplicar(false);
    }

    // ---------- Eventos (os mesmos para mouse, toque e controle) ----------

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        _pressionado = state == SelectionState.Pressed;
        Aplicar(true);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        _selecionado = true;
        Aplicar(true);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        _selecionado = false;
        Aplicar(true);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable()) SonsUI.Toque();
        base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (IsActive() && IsInteractable()) SonsUI.Toque();
        base.OnSubmit(eventData);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _pressionado = false; _afundo = 0f;
        Aplicar(true);
    }
}
