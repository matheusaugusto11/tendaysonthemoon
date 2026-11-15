using System;
using System.Collections.Generic;
using DezDias;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Telas do jogo, montadas em código num Canvas (uGUI). Cada fase redesenha o painel da missão por cima da base 3D
/// (igual ao protótipo web). Trocar o idioma só redesenha: o estado fica no GameManager e na Missao.
/// Visual (mudança interface-e-minijogos): nenhuma cor, fonte ou tamanho é escrito aqui; tudo vem de UiTema, e botões
/// e formas vêm de BotaoFisico e FormaUI. A ação que avança fica sempre no rodapé fixo do painel (BotaoPrincipal).
/// </summary>
public class UIJogo
{
    /// <summary>Cor de acento de cada módulo (a mesma do módulo 3D correspondente).</summary>
    public static Color CorDoModulo(Modulo m) => UiTema.CorDoModulo(m);

    readonly GameManager _gm;
    RectTransform _raiz;

    GameObject _hud, _fundoTela, _painelMinigame, _pausa, _faixa;
    RectTransform _tablet, _conteudo, _viewport, _rodape;
    FormaUI _tira;
    RectTransform _cabecalho;
    GameObject _tabletSombra, _tabletFace;
    CanvasGroup _grupoTela;
    enum ModoPainel { Lateral, Pagina, Coluna }
    ModoPainel _modo;
    Vector2 _posBase;
    float _animInicio = -10f;
    ScrollRect _scroll;
    Button _primeiro;

    // HUD
    Text _hudDia, _hudRelogio, _hudEstrelas;
    BotaoFisico _btnIdioma;
    MedidorHud _medO2, _medRad, _medBat;
    Missao _missaoVista; // trocou a missão: os medidores saltam direto, sem andar nem soltar variação
    // Faixa do deslocamento, botão de ação, seta e aviso
    Text _faixaTexto, _faixaDica;
    Image _faixaIcone;
    FormaUI _faixaTira;
    Color _corDestino = UiTema.Energia; // cor do módulo de destino: a da faixa e a da seta guia
    BotaoFisico _btnAcao;
    RectTransform _seta;
    SetaGuia3D _seta3D;   // seta no chão, ao redor do corpo do personagem
    Text _setaTexto;
    Image[] _setaPartes;
    Text _avisoTexto;
    float _avisoAte;
    // Minigame (painel lateral do molde D9)
    RectTransform _miniPainel, _miniArea, _miniAreaFundo, _miniTempoTrilho, _miniTempoBarra, _miniProgTrilho, _miniProgBarra, _miniFaixa;
    FormaUI _miniTira, _miniProgForma, _miniFaixaFace;
    Text _miniTitulo, _miniInstrucao, _miniTempo, _miniFaixaTitulo, _miniFaixaTexto;
    Image _miniIcone, _miniCursor, _miniFaixaIcone, _miniTempoIcone, _miniProgIcone;
    BotaoFisico _btnMiniSair, _btnMiniAjuda;
    CanvasGroup _miniFaixaGrupo;
    Minigame _minigameAtual;
    float _miniProgAlvo, _miniProgVis, _miniFaixaInicio = -10f;
    // Pausa
    BotaoFisico _btnContinuar, _btnReiniciar, _btnSom, _btnSimVoltar, _btnNaoVoltar;
    Text _pausaTitulo, _pausaAjuda, _pausaPergunta;
    GameObject _pausaOpcoes, _pausaConfirma;
    // O dia acontece
    Text _legenda;

    public UIJogo(GameManager gm)
    {
        _gm = gm;
        Construir();
    }

    // =====================================================================
    // Construção das camadas fixas
    // =====================================================================

    void Construir()
    {
        var go = new GameObject("UI_Jogo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // acima dos controles de toque
        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        _raiz = (RectTransform)go.transform;
        Entrada.GarantirEventSystem();

        ConstruirHud();
        ConstruirFaixa();
        ConstruirTablet();
        ConstruirMinigame();
        ConstruirPausa();
    }

    void ConstruirHud()
    {
        _hud = new GameObject("HUD", typeof(RectTransform));
        _hud.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_hud.transform);
        _hud.AddComponent<SafeArea>(); // respeita o entalhe do celular

        // Barra de 108: cabem os botões de 88 com 10 de folga.
        var barra = Horizontal(_hud.transform, UiTema.E16, false, 10, null, TextAnchor.MiddleLeft);
        Fundo(barra, UiTema.Aco);
        barra.anchorMin = new Vector2(0, 1); barra.anchorMax = new Vector2(1, 1); barra.pivot = new Vector2(0.5f, 1);
        barra.sizeDelta = new Vector2(-24, 108); barra.anchoredPosition = new Vector2(0, -10);
        var hl = barra.GetComponent<HorizontalLayoutGroup>();
        hl.childControlHeight = true; hl.childForceExpandHeight = true;

        var pill = Horizontal(barra, 0, false, 0, null, TextAnchor.MiddleCenter);
        Fundo(pill, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        Tam(pill, 220, -1);
        _hudDia = Rot(pill, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleCenter);

        _medO2 = MedidorHud.Criar(this, barra, TipoMedidor.Oxigenio);
        _medRad = MedidorHud.Criar(this, barra, TipoMedidor.Radiacao);
        _medBat = MedidorHud.Criar(this, barra, TipoMedidor.Bateria);

        var est = Horizontal(barra, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
        Tam(est, 120, -1);
        Icone(est, "estrela", 44);
        _hudEstrelas = Rot(est, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        var rel = Horizontal(barra, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
        Tam(rel, 150, -1);
        Icone(rel, "relogio", 44);
        _hudRelogio = Rot(rel, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        _hudRelogio.name = "HudRelogio";

        _btnIdioma = Sec(barra, "", "globo", () => _gm.AlternarIdioma(), 150, UiTema.AlvoMinimo); // só o código: EN ou PT
        _btnIdioma.TamanhoMaximo = 30;
        _btnIdioma.navigation = new Navigation { mode = Navigation.Mode.None };
        var bp = Sec(barra, "", "pausa", () => _gm.Pausar(), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
        bp.navigation = new Navigation { mode = Navigation.Mode.None };

        // Aviso rápido (tarefa concluída, tempo acabou), sobre a cena
        _avisoTexto = Texto(_hud.transform, "", 46, new Vector2(0.5f, 0.24f), new Vector2(1500, 140));
        _avisoTexto.color = UiTema.Branco;
        _avisoTexto.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
    }

    void ConstruirFaixa()
    {
        _faixa = new GameObject("FaixaDeslocamento", typeof(RectTransform));
        _faixa.transform.SetParent(_hud.transform, false);
        var rt = (RectTransform)_faixa.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -136);
        var v = _faixa.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(UiTema.E32, UiTema.E32, UiTema.E24, UiTema.E16); v.spacing = UiTema.E4;
        v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
        var f = _faixa.AddComponent<ContentSizeFitter>(); f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rt.sizeDelta = new Vector2(1180, 0);
        Fundo(rt, UiTema.Aco);
        // Tira de luz na cor do módulo de destino, como nos painéis.
        var tiraRt = RT("Tira", rt);
        tiraRt.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        tiraRt.anchorMin = new Vector2(0, 1); tiraRt.anchorMax = new Vector2(1, 1); tiraRt.pivot = new Vector2(0.5f, 1);
        tiraRt.offsetMin = new Vector2(UiTema.ChanfroPainel - 2f, -8f); tiraRt.offsetMax = Vector2.zero;
        _faixaTira = tiraRt.gameObject.AddComponent<FormaUI>();
        _faixaTira.chanfro = 0; _faixaTira.cantoSE = _faixaTira.cantoID = false; _faixaTira.raycastTarget = false;
        var linha = Horizontal(rt, UiTema.E12, false, 0, null, TextAnchor.MiddleCenter);
        _faixaIcone = Icone(linha, "ajuda", 52);
        _faixaTexto = Rot(linha, "", Papel.Corpo, UiTema.Branco, TextAnchor.MiddleCenter);
        _faixaDica = Rot(rt, "", Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter);

        // Botão de AÇÃO junto da estação: é o botão principal do deslocamento, do mesmo tamanho e estilo e no canto inferior
        // direito. Com os joysticks na tela ele sobe para ficar acima do joystick da câmera (ver AtualizarGuia).
        _btnAcao = BotaoFisico.Criar(_hud.transform, BotaoFisico.Estilo.Principal, "", "seta_tinta", () => { _gm.Interagir(); Entrada.Selecionar(null); }, null, "BotaoAcao");
        var ra = (RectTransform)_btnAcao.transform;
        ra.anchorMin = ra.anchorMax = new Vector2(1, 0); ra.pivot = new Vector2(1, 0);
        ra.sizeDelta = new Vector2(400f, AlturaPrincipal); ra.anchoredPosition = new Vector2(-MargemRodape, MargemRodapeBaixo);
        _btnAcao.AjustarPara(400f);
        _btnAcao.navigation = new Navigation { mode = Navigation.Mode.None };
        _btnAcao.gameObject.SetActive(false);

        // Seta guia
        var seta = new GameObject("Seta", typeof(RectTransform));
        seta.transform.SetParent(_hud.transform, false);
        _seta = (RectTransform)seta.transform;
        _seta.sizeDelta = new Vector2(120, 60);
        var haste = new GameObject("Haste", typeof(Image));
        haste.transform.SetParent(_seta, false);
        var rh = (RectTransform)haste.transform;
        rh.sizeDelta = new Vector2(60, 18); rh.anchoredPosition = new Vector2(-18, 0);
        var ponta = new GameObject("Ponta", typeof(Image));
        ponta.transform.SetParent(_seta, false);
        var rpn = (RectTransform)ponta.transform;
        rpn.sizeDelta = new Vector2(38, 38); rpn.anchoredPosition = new Vector2(24, 0);
        rpn.localRotation = Quaternion.Euler(0, 0, 45);
        _setaPartes = new[] { haste.GetComponent<Image>(), ponta.GetComponent<Image>() };
        foreach (var im in _setaPartes) { im.color = UiTema.Energia; im.raycastTarget = false; }
        _setaTexto = Texto(_hud.transform, "", 30, new Vector2(0.5f, 0.5f), new Vector2(520, 50));
        _setaTexto.color = UiTema.Energia;
        _setaTexto.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
        _seta.gameObject.SetActive(false);
        _setaTexto.gameObject.SetActive(false);
        _seta3D = SetaGuia3D.Criar();
    }

    void ConstruirTablet()
    {
        // A camada das telas: transparente nos modos Lateral e Coluna (a cena aparece), opaca no modo Página.
        _fundoTela = new GameObject("Tela", typeof(Image), typeof(CanvasGroup));
        _fundoTela.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_fundoTela.transform);
        var imgTela = _fundoTela.GetComponent<Image>();
        imgTela.color = Color.clear; imgTela.raycastTarget = false;
        _grupoTela = _fundoTela.GetComponent<CanvasGroup>();

        _tablet = RT("Tablet", _fundoTela.transform);
        var face = Fundo(_tablet, UiTema.Aco);
        _tabletFace = face.gameObject;
        _tabletSombra = _tablet.GetChild(0).gameObject;

        // Tira de luz: a cor do sistema de que a tela fala, no topo, a partir do canto chanfrado.
        var tiraRt = RT("Tira", _tablet);
        tiraRt.anchorMin = new Vector2(0, 1); tiraRt.anchorMax = new Vector2(1, 1); tiraRt.pivot = new Vector2(0.5f, 1);
        tiraRt.offsetMin = new Vector2(UiTema.ChanfroPainel - 2f, -8f); tiraRt.offsetMax = Vector2.zero;
        _tira = tiraRt.gameObject.AddComponent<FormaUI>();
        _tira.chanfro = 0; _tira.cantoSE = _tira.cantoID = false; _tira.raycastTarget = false;

        // Cabeçalho fixo: o que precisa ficar à vista enquanto o corpo rola (ex.: a prévia da distribuição).
        _cabecalho = RT("Cabecalho", _tablet);
        _cabecalho.anchorMin = new Vector2(0, 1); _cabecalho.anchorMax = new Vector2(1, 1); _cabecalho.pivot = new Vector2(0.5f, 1);
        _cabecalho.sizeDelta = new Vector2(-60f, 0f); _cabecalho.anchoredPosition = new Vector2(0f, -32f);
        var cv = _cabecalho.gameObject.AddComponent<VerticalLayoutGroup>();
        cv.spacing = UiTema.E8; cv.childControlWidth = cv.childControlHeight = true;
        cv.childForceExpandWidth = true; cv.childForceExpandHeight = false;
        var cf = _cabecalho.gameObject.AddComponent<ContentSizeFitter>();
        cf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _viewport = RT("Viewport", _tablet);
        Esticar(_viewport);
        _viewport.offsetMin = new Vector2(30, 24); _viewport.offsetMax = new Vector2(-30, -32);
        _viewport.gameObject.AddComponent<RectMask2D>();
        var vi = _viewport.gameObject.AddComponent<Image>(); vi.color = new Color(0, 0, 0, 0); // recebe o arrasto da rolagem

        _conteudo = Vertical(_viewport, UiTema.E16, 0);
        _conteudo.anchorMin = new Vector2(0, 1); _conteudo.anchorMax = new Vector2(1, 1); _conteudo.pivot = new Vector2(0.5f, 1);
        _conteudo.offsetMin = _conteudo.offsetMax = Vector2.zero;
        var fit = _conteudo.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Rodapé: a ação que avança fica sempre no canto inferior direito da TELA, no mesmo lugar e tamanho em toda
        // fase, e o painel ocupa o espaço acima dele. Ações secundárias (e a bandeja de células) entram à esquerda dele.
        _rodape = RT("Rodape", _fundoTela.transform);
        _rodape.anchorMin = _rodape.anchorMax = new Vector2(1, 0); _rodape.pivot = new Vector2(1, 0);
        _rodape.anchoredPosition = new Vector2(-MargemRodape, MargemRodapeBaixo);
        var hl = _rodape.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = UiTema.E16; hl.childAlignment = TextAnchor.LowerRight;
        hl.childControlWidth = hl.childControlHeight = true; hl.childForceExpandWidth = hl.childForceExpandHeight = false;
        var fr = _rodape.gameObject.AddComponent<ContentSizeFitter>();
        fr.horizontalFit = fr.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scroll = _tablet.gameObject.AddComponent<ScrollRect>();
        _scroll.viewport = _viewport; _scroll.content = _conteudo;
        _scroll.horizontal = false; _scroll.vertical = true;
        _scroll.movementType = ScrollRect.MovementType.Clamped;
        _scroll.scrollSensitivity = 40f;
    }

    /// <summary>Filho de <paramref name="pai"/> preso no canto superior esquerdo (posição em y para baixo, como num texto).</summary>
    static RectTransform NoTopo(string nome, Transform pai)
    {
        var rt = RT(nome, pai);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        return rt;
    }

    static void Colocar(RectTransform rt, float x, float y, float w, float h)
    {
        rt.sizeDelta = new Vector2(w, h); rt.anchoredPosition = new Vector2(x, -y);
    }

    /// <summary>
    /// Painel do minijogo (design D9): no lugar do painel lateral (D7), à direita, com a estação em close à esquerda.
    /// De cima para baixo: ícone e título da tarefa, instrução em uma frase, relógio (ícone, segundos e barra que
    /// esvazia), a Area de 760 por 560 e o rodapé com a barra de progresso, "?" e "Sair". Sem véu sobre a cena.
    /// </summary>
    void ConstruirMinigame()
    {
        _painelMinigame = new GameObject("Minigame", typeof(RectTransform));
        _painelMinigame.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_painelMinigame.transform);

        _miniPainel = RT("PainelMinijogo", _painelMinigame.transform);
        _miniPainel.anchorMin = new Vector2(1, 0); _miniPainel.anchorMax = new Vector2(1, 1); _miniPainel.pivot = new Vector2(1, 0.5f);
        Fundo(_miniPainel, UiTema.Aco);
        var tiraRt = RT("Tira", _miniPainel);
        tiraRt.anchorMin = new Vector2(0, 1); tiraRt.anchorMax = new Vector2(1, 1); tiraRt.pivot = new Vector2(0.5f, 1);
        tiraRt.offsetMin = new Vector2(UiTema.ChanfroPainel - 2f, -8f); tiraRt.offsetMax = Vector2.zero;
        _miniTira = tiraRt.gameObject.AddComponent<FormaUI>();
        _miniTira.chanfro = 0; _miniTira.cantoSE = _miniTira.cantoID = false; _miniTira.raycastTarget = false;

        // Cabeçalho: ícone e título da tarefa.
        var ic = NoTopo("Icone", _miniPainel);
        _miniIcone = ic.gameObject.AddComponent<Image>(); _miniIcone.preserveAspect = true; _miniIcone.raycastTarget = false;
        _miniTitulo = Rot(_miniPainel, "", Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleLeft);
        _miniTitulo.name = "TituloMinijogo";
        Topo(_miniTitulo.rectTransform);
        // Instrução em uma frase.
        _miniInstrucao = Rot(_miniPainel, "", Papel.Rotulo, UiTema.Branco, TextAnchor.UpperLeft);
        _miniInstrucao.name = "Instrucao";
        Topo(_miniInstrucao.rectTransform);
        // Relógio: ícone, segundos e barra que esvazia. Nunca fica vermelho, não pulsa nem toca som.
        var ri = NoTopo("RelogioIcone", _miniPainel);
        _miniTempoIcone = ri.gameObject.AddComponent<Image>(); _miniTempoIcone.sprite = UiTema.Icone("relogio"); _miniTempoIcone.preserveAspect = true; _miniTempoIcone.raycastTarget = false;
        _miniTempo = Rot(_miniPainel, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        _miniTempo.name = "RelogioMinijogo";
        Topo(_miniTempo.rectTransform);
        _miniTempoTrilho = NoTopo("RelogioTrilho", _miniPainel);
        FormaUI.Chanfrado(_miniTempoTrilho, UiTema.Vazio, 4f, "SE ID", "Trilho");
        _miniTempoBarra = RT("Barra", _miniTempoTrilho);
        _miniTempoBarra.anchorMin = Vector2.zero; _miniTempoBarra.anchorMax = new Vector2(1, 1); _miniTempoBarra.pivot = new Vector2(0, 0.5f);
        _miniTempoBarra.offsetMin = Vector2.zero; _miniTempoBarra.offsetMax = Vector2.zero;
        FormaUI.Chanfrado(_miniTempoBarra, UiTema.Branco, 4f, "SE ID", "Face");

        // Área do gesto: tela escura chanfrada, com a Area (760 por 560) por cima, na escala que couber.
        _miniAreaFundo = RT("AreaFundo", _miniPainel);
        _miniAreaFundo.anchorMin = _miniAreaFundo.anchorMax = new Vector2(0, 1); _miniAreaFundo.pivot = new Vector2(0.5f, 0.5f);
        FormaUI.Chanfrado(_miniAreaFundo, UiTema.Vazio, UiTema.ChanfroBotao, "SE ID", "Fundo");
        _miniArea = RT("Area", _miniPainel);
        _miniArea.anchorMin = _miniArea.anchorMax = new Vector2(0, 1); _miniArea.pivot = new Vector2(0.5f, 0.5f);
        _miniArea.sizeDelta = Minigame.TamanhoArea;

        // Rodapé: barra de progresso contínua (sem nota de corte), "?" e "Sair".
        var pi = NoTopo("ProgressoIcone", _miniPainel);
        _miniProgIcone = pi.gameObject.AddComponent<Image>(); _miniProgIcone.sprite = UiTema.Icone("check"); _miniProgIcone.preserveAspect = true; _miniProgIcone.raycastTarget = false;
        _miniProgTrilho = NoTopo("ProgressoTrilho", _miniPainel);
        FormaUI.Chanfrado(_miniProgTrilho, UiTema.Vazio, 4f, "SE ID", "Trilho");
        _miniProgBarra = RT("Barra", _miniProgTrilho);
        _miniProgBarra.anchorMin = Vector2.zero; _miniProgBarra.anchorMax = new Vector2(0, 1); _miniProgBarra.pivot = new Vector2(0, 0.5f);
        _miniProgBarra.offsetMin = Vector2.zero; _miniProgBarra.offsetMax = Vector2.zero;
        _miniProgForma = FormaUI.Chanfrado(_miniProgBarra, UiTema.Ar, 4f, "SE ID", "Face");
        _btnMiniAjuda = Sec(_miniPainel, "?", null, () => { _minigameAtual?.MostrarDemo(); Entrada.Selecionar(null); }, UiTema.AlvoMinimo, UiTema.AlvoMinimo);
        _btnMiniAjuda.Rotulo.alignment = TextAnchor.MiddleCenter;
        _btnMiniAjuda.name = "BotaoAjuda";
        _btnMiniSair = Sec(_miniPainel, "", null, () => _gm.CancelarMinijogo(), 240, UiTema.AlvoMinimo);
        _btnMiniSair.name = "BotaoSair";
        foreach (var b in new[] { _btnMiniAjuda, _btnMiniSair })
        {
            var r = (RectTransform)b.transform;
            r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
            b.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        // Faixa de resultado: "Ótimo!", "Bom!" ou "Não deu desta vez", com a nota e o efeito, por cima da área.
        _miniFaixa = RT("FaixaResultado", _miniPainel);
        _miniFaixa.anchorMin = _miniFaixa.anchorMax = new Vector2(0, 1); _miniFaixa.pivot = new Vector2(0.5f, 0.5f);
        _miniFaixaGrupo = _miniFaixa.gameObject.AddComponent<CanvasGroup>();
        _miniFaixaGrupo.blocksRaycasts = false;
        _miniFaixaFace = Fundo(_miniFaixa, UiTema.Branco);
        var fi = RT("Icone", _miniFaixa);
        fi.anchorMin = fi.anchorMax = new Vector2(0, 0.5f); fi.pivot = new Vector2(0, 0.5f);
        fi.sizeDelta = new Vector2(72, 72); fi.anchoredPosition = new Vector2(UiTema.E32, 0);
        _miniFaixaIcone = fi.gameObject.AddComponent<Image>(); _miniFaixaIcone.preserveAspect = true; _miniFaixaIcone.raycastTarget = false;
        _miniFaixaTitulo = Rot(_miniFaixa, "", Papel.TituloTela, UiTema.Tinta, TextAnchor.LowerLeft);
        _miniFaixaTitulo.name = "FaixaTitulo";
        Topo(_miniFaixaTitulo.rectTransform);
        _miniFaixaTexto = Rot(_miniFaixa, "", Papel.Corpo, UiTema.Tinta, TextAnchor.UpperLeft);
        _miniFaixaTexto.name = "FaixaTexto";
        Topo(_miniFaixaTexto.rectTransform);
        _miniFaixa.gameObject.SetActive(false);

        _miniCursor = Retangulo(_painelMinigame.transform, Vector2.zero, new Vector2(48, 48), UiTema.Energia);
        _miniCursor.sprite = ControlesMobile.Circulo();
        _miniCursor.gameObject.SetActive(false);
        _painelMinigame.SetActive(false);
    }

    static void Topo(RectTransform rt) { rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); }

    /// <summary>
    /// Posiciona o painel do minijogo no lugar do lateral e distribui as partes de cima para baixo. A Area guarda o
    /// tamanho de 760 por 560 e só encolhe (escala) se a tela não tiver altura para ela, como num celular deitado.
    /// </summary>
    void PosicionarMinigame()
    {
        if (_minigameAtual == null) return;
        var tam = TamanhoReferencia();
        float topo = _hud.activeSelf ? 128f : 40f, baixo = 40f;
        float W = Mathf.Min(840f, tam.x - 96f), H = tam.y - topo - baixo;
        _miniPainel.sizeDelta = new Vector2(W, -(topo + baixo));
        _miniPainel.anchoredPosition = new Vector2(-MargemRodape, (baixo - topo) * 0.5f);
        float pad = 30f, wi = W - 2f * pad;

        float y = 26f;
        Colocar((RectTransform)_miniIcone.transform, pad, y, 56, 56);
        Colocar(_miniTitulo.rectTransform, pad + 72f, y, wi - 72f, 56);
        y += 56f + 10f;
        Colocar(_miniInstrucao.rectTransform, pad, y, wi, 10);
        float hi = Mathf.Ceil(_miniInstrucao.preferredHeight) + 2f;
        _miniInstrucao.rectTransform.sizeDelta = new Vector2(wi, hi);
        y += hi + 12f;
        Colocar((RectTransform)_miniTempoIcone.transform, pad, y, 40, 40);
        Colocar(_miniTempo.rectTransform, pad + 50f, y, 90, 40);
        Colocar(_miniTempoTrilho, pad + 150f, y + 14f, wi - 150f, 12);
        y += 40f + 12f;

        const float rodape = 88f;
        float livre = H - y - 12f - rodape - 20f;
        float s = Mathf.Clamp(livre / Minigame.TamanhoArea.y, 0.5f, 1f);
        s = Mathf.Min(s, wi / Minigame.TamanhoArea.x);
        float cy = y + Mathf.Max(Minigame.TamanhoArea.y * s, livre) * 0.5f;
        _miniArea.localScale = new Vector3(s, s, 1f);
        _miniArea.anchoredPosition = new Vector2(W * 0.5f, -cy);
        _miniAreaFundo.sizeDelta = Minigame.TamanhoArea * s;
        _miniAreaFundo.anchoredPosition = _miniArea.anchoredPosition;

        float yr = H - 20f - rodape;
        float xAjuda = W - pad - 240f - UiTema.E12 - UiTema.AlvoMinimo;
        Colocar((RectTransform)_miniProgIcone.transform, pad, yr + 24f, 40, 40);
        Colocar(_miniProgTrilho, pad + 52f, yr + 34f, xAjuda - UiTema.E24 - (pad + 52f), 20);
        ((RectTransform)_btnMiniAjuda.transform).anchoredPosition = new Vector2(xAjuda, -yr);
        ((RectTransform)_btnMiniSair.transform).anchoredPosition = new Vector2(W - pad - 240f, -yr);

        _miniFaixa.sizeDelta = new Vector2(wi, _miniFaixa.sizeDelta.y < 1f ? 190f : _miniFaixa.sizeDelta.y);
        _miniFaixa.anchoredPosition = _miniArea.anchoredPosition;
        _miniPainel.ForceUpdateRectTransforms();
        // A estação vai para o meio da faixa livre à esquerda do painel.
        CameraFoco.FracaoLivre = Mathf.Clamp01((tam.x - W - MargemRodape) / tam.x);
    }

    /// <summary>Animação do painel do minijogo: a barra de progresso anda contínua e a faixa entra em 300 ms.</summary>
    void AnimarMinigame()
    {
        if (!_painelMinigame.activeSelf) return;
        float dt = Time.unscaledDeltaTime;
        _miniProgVis = Mathf.MoveTowards(_miniProgVis, _miniProgAlvo, dt * 1.5f);
        _miniProgBarra.anchorMax = new Vector2(Mathf.Clamp01(_miniProgVis), 1f);
        _miniProgBarra.gameObject.SetActive(_miniProgVis > 0.001f);
        if (_miniFaixa.gameObject.activeSelf)
        {
            float k = Mathf.Clamp01((Time.unscaledTime - _miniFaixaInicio) / UiTema.TempoFaixa);
            float e = 1f - (1f - k) * (1f - k) * (1f - k);
            _miniFaixaGrupo.alpha = e;
            _miniFaixa.localScale = Vector3.one * (UiTema.MovimentoReduzido ? 1f : Mathf.Lerp(0.92f, 1f, e));
        }
    }

    void ConstruirPausa()
    {
        // A pausa é a única tela que cobre o centro: o jogo está parado, então a cena escurece atrás do cartão.
        var veu = UiTema.Pagina; veu.a = 0.8f;
        _pausa = Painel("Pausa", veu);
        var cartao = RT("Cartao", _pausa.transform);
        cartao.anchorMin = cartao.anchorMax = cartao.pivot = new Vector2(0.5f, 0.5f);
        cartao.sizeDelta = new Vector2(760, 700);
        Fundo(cartao, UiTema.Aco);
        var tiraRt = RT("Tira", cartao);
        tiraRt.anchorMin = new Vector2(0, 1); tiraRt.anchorMax = new Vector2(1, 1); tiraRt.pivot = new Vector2(0.5f, 1);
        tiraRt.offsetMin = new Vector2(UiTema.ChanfroPainel - 2f, -8f); tiraRt.offsetMax = Vector2.zero;
        var tira = tiraRt.gameObject.AddComponent<FormaUI>();
        tira.chanfro = 0; tira.cantoSE = tira.cantoID = false; tira.raycastTarget = false; tira.color = UiTema.Branco;
        _pausaTitulo = Texto(cartao, "", 60, new Vector2(0.5f, 0.89f), new Vector2(700, 100));

        // Opções: Continuar é o principal; Som e Reiniciar são secundários.
        var opcoes = RT("Opcoes", cartao); Esticar(opcoes);
        _pausaOpcoes = opcoes.gameObject;
        _btnContinuar = BotaoFisico.Criar(opcoes, BotaoFisico.Estilo.Principal, "", "seta_tinta", () => _gm.Retomar(), null, "Continuar");
        ComTamanho(_btnContinuar, 600, 120); Ancorar(_btnContinuar, new Vector2(0.5f, 0.71f));
        _btnSom = Sec(opcoes, "", "som", () => AlternarSom(), 600, UiTema.AlvoMinimo + 8);
        Ancorar(_btnSom, new Vector2(0.5f, 0.53f));
        _btnReiniciar = Sec(opcoes, "", null, () => PedirConfirmacao(), 600, UiTema.AlvoMinimo + 8);
        Ancorar(_btnReiniciar, new Vector2(0.5f, 0.38f));
        _pausaAjuda = Texto(opcoes, "", 24, new Vector2(0.5f, 0.16f), new Vector2(680, 100));
        _pausaAjuda.color = UiTema.Apoio;

        // Confirmação de "reiniciar", em tom de pergunta (não de aviso): voltar é o principal e fácil de achar.
        var confirma = RT("Confirma", cartao); Esticar(confirma);
        _pausaConfirma = confirma.gameObject;
        _pausaPergunta = Texto(confirma, "", 40, new Vector2(0.5f, 0.64f), new Vector2(640, 240));
        _btnNaoVoltar = BotaoFisico.Criar(confirma, BotaoFisico.Estilo.Principal, "", "seta_tinta", () => FecharConfirmacao(), null, "NaoSair");
        ComTamanho(_btnNaoVoltar, 600, 120); Ancorar(_btnNaoVoltar, new Vector2(0.5f, 0.38f));
        _btnSimVoltar = Sec(confirma, "", null, () => _gm.Reiniciar(), 600, UiTema.AlvoMinimo + 8);
        Ancorar(_btnSimVoltar, new Vector2(0.5f, 0.22f));
        _pausaConfirma.SetActive(false);
        _pausa.SetActive(false);
    }

    void AlternarSom()
    {
        SonsUI.Ligado = !SonsUI.Ligado; // guardado em PlayerPrefs (tdm_som): continua depois de recarregar a página
        TextosFixos();
    }

    void PedirConfirmacao()
    {
        _pausaOpcoes.SetActive(false);
        _pausaConfirma.SetActive(true);
        Entrada.Selecionar(_btnNaoVoltar.gameObject);
    }

    void FecharConfirmacao()
    {
        _pausaConfirma.SetActive(false);
        _pausaOpcoes.SetActive(true);
        Entrada.Selecionar(_btnContinuar.gameObject);
    }

    // =====================================================================
    // Redesenho por fase
    // =====================================================================

    static bool FaseDeMissao(Fase f) => f == Fase.Boletim || f == Fase.AteTarefa || f == Fase.Escolha || f == Fase.MinijogoInfo
        || f == Fase.Minijogo || f == Fase.DecisaoVazamento || f == Fase.TarefaConcluida || f == Fase.AteHub
        || f == Fase.Distribuir || f == Fase.DiaAcontece || f == Fase.Resultado;

    static bool FaseSemTablet(Fase f) => f == Fase.AteTarefa || f == Fase.AteHub || f == Fase.Minijogo;

    /// <summary>Cor da tira de luz de cada tela: a do sistema de que ela fala.</summary>
    Color TiraDaFase(Fase f)
    {
        switch (f)
        {
            case Fase.Boletim: case Fase.Distribuir: case Fase.DiaAcontece: return UiTema.Energia;
            case Fase.Escolha: case Fase.MinijogoInfo: case Fase.TarefaConcluida:
                return _gm.Missao != null && !_gm.Teste ? UiTema.CorDaTarefa(_gm.Missao.Hoje.Tarefa) : UiTema.Lab;
            case Fase.DecisaoVazamento: return UiTema.Ar;
            case Fase.MenuMinijogos: return UiTema.Lab;
            default: return UiTema.Branco;
        }
    }

    public void Render()
    {
        var f = _gm.Fase;
        _hud.SetActive(_gm.Missao != null && !_gm.Teste && FaseDeMissao(f));
        _faixa.SetActive(f == Fase.AteTarefa || f == Fase.AteHub);
        _painelMinigame.SetActive(f == Fase.Minijogo);
        _fundoTela.SetActive(!FaseSemTablet(f));
        if (f != Fase.AteTarefa && f != Fase.AteHub)
        {
            _btnAcao.gameObject.SetActive(false);
            _seta.gameObject.SetActive(false);
            _setaTexto.gameObject.SetActive(false);
            if (_seta3D != null) _seta3D.Esconder();
        }
        TextosFixos();
        if (!FaseSemTablet(f)) MontarTablet(f);
        if (f == Fase.AteTarefa || f == Fase.AteHub) MontarFaixa();
        if (f == Fase.Minijogo && _minigameAtual != null)
        {
            TextosDoMinigame();
            PosicionarMinigame();
        }
        AtualizarHud();
    }

    void TextosFixos()
    {
        _pausaTitulo.text = Textos.Pausa.ToString();
        _btnContinuar.Texto(Textos.ContinuarB.ToString());
        _btnReiniciar.Texto(Textos.Reiniciar.ToString());
        _btnSom.Texto((SonsUI.Ligado ? Textos.SomLigado : Textos.SomDesligado).ToString()); // o estado vai escrito, não só na posição
        _pausaPergunta.text = Textos.ReiniciarPergunta.ToString();
        _btnNaoVoltar.Texto(Textos.Voltar.ToString());
        _btnSimVoltar.Texto(Textos.SimVoltarAoInicio.ToString());
        _pausaAjuda.text = Textos.AjudaControle.ToString();
        _btnMiniSair.Texto(Textos.Sair.ToString());
        _btnIdioma.Texto(Idioma.Atual == Lingua.Pt ? "EN" : "PT"); // o código do idioma para o qual a troca leva
    }

    /// <summary>
    /// Tamanho do canvas em unidades de referência (1920x1080, match 0,5), calculado da tela. O rect do canvas só
    /// é atualizado pelo CanvasScaler no frame seguinte: no primeiro desenho da WebGL ele ainda vinha em pixels.
    /// </summary>
    static Vector2 TamanhoReferencia()
    {
        float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
        float log = Mathf.Lerp(Mathf.Log(w / 1920f, 2f), Mathf.Log(h / 1080f, 2f), 0.5f);
        float s = Mathf.Pow(2f, log);
        return new Vector2(w / s, h / s);
    }

    int _telaW, _telaH;
    Fase _fasePintada = (Fase)(-1);

    /// <summary>Redesenha a tela atual se a janela mudou de tamanho (tela cheia, giro do celular).</summary>
    void VerificarTamanhoDaTela()
    {
        if (Screen.width == _telaW && Screen.height == _telaH) return;
        bool primeira = _telaW == 0;
        _telaW = Screen.width; _telaH = Screen.height;
        if (!primeira) Render();
    }

    const float MargemRodape = 48f, MargemRodapeBaixo = 32f, AlturaPrincipal = 104f;

    /// <summary>Lateral: à direita, com a cena à esquerda. Página: a tela toda, opaca. Coluna: à direita, com a base ao fundo.</summary>
    static ModoPainel ModoDaFase(Fase f)
    {
        switch (f)
        {
            case Fase.Inicio: return ModoPainel.Coluna;
            case Fase.Relatorio: case Fase.Creditos: case Fase.ComoJogar: case Fase.MenuMinijogos: return ModoPainel.Pagina;
            default: return ModoPainel.Lateral;
        }
    }

    /// <summary>Põe o painel no lugar do modo atual, entre o HUD e a margem de baixo (ou o rodapé, se a tela tem ação).</summary>
    void PosicionarPainel(Vector2 tam, bool rodape)
    {
        float margemVazia = _modo == ModoPainel.Coluna ? 24f : 40f;
        float topo = _hud.activeSelf ? 128f : margemVazia;
        float baixo = rodape ? MargemRodapeBaixo + AlturaPrincipal + UiTema.E24 : margemVazia;
        if (_modo == ModoPainel.Pagina)
        {
            float mx = Mathf.Max(48f, (tam.x - 1500f) * 0.5f);
            _tablet.anchorMin = Vector2.zero; _tablet.anchorMax = Vector2.one; _tablet.pivot = new Vector2(0.5f, 0.5f);
            _tablet.sizeDelta = new Vector2(-2f * mx, -(topo + baixo));
            _posBase = new Vector2(0f, (baixo - topo) * 0.5f);
        }
        else
        {
            bool lateral = _modo == ModoPainel.Lateral;
            float w = Mathf.Min(lateral ? 840f : 640f, tam.x - 96f);
            float margem = lateral ? MargemRodape : 64f;
            _tablet.anchorMin = new Vector2(1, 0); _tablet.anchorMax = new Vector2(1, 1); _tablet.pivot = new Vector2(1, 0.5f);
            _tablet.sizeDelta = new Vector2(w, -(topo + baixo));
            _posBase = new Vector2(-margem, (baixo - topo) * 0.5f);
        }
        _tablet.anchoredPosition = _posBase;
    }

    /// <summary>Entrada do painel: 250 ms, desacelerando, escorregando 40 e aparecendo. Com movimento reduzido, só a opacidade.</summary>
    void AnimarPainel()
    {
        if (_grupoTela == null || !_fundoTela.activeSelf) return;
        bool reduzido = UiTema.MovimentoReduzido;
        float dur = reduzido ? 0.2f : UiTema.TempoPainel;
        float t = Mathf.Clamp01((Time.unscaledTime - _animInicio) / dur);
        float e = 1f - (1f - t) * (1f - t) * (1f - t);
        _grupoTela.alpha = e;
        _tablet.anchoredPosition = _posBase + new Vector2(reduzido ? 0f : 40f * (1f - e), 0f);
    }

    void MontarTablet(Fase f)
    {
        Limpar(_conteudo);
        Limpar(_cabecalho);
        Limpar(_rodape);
        _primeiro = null;
        _telaW = Screen.width; _telaH = Screen.height;
        _modo = ModoDaFase(f);
        bool pagina = _modo == ModoPainel.Pagina;
        _tabletFace.SetActive(!pagina); _tabletSombra.SetActive(!pagina); _tira.gameObject.SetActive(!pagina);
        _fundoTela.GetComponent<Image>().color = pagina ? UiTema.Pagina : Color.clear; // a página é opaca, de céu preto
        _tira.color = TiraDaFase(f);
        _tira.Redesenhar();
        var tam = TamanhoReferencia();
        PosicionarPainel(tam, true); // provisório: o texto já quebra na largura certa enquanto a tela é montada

        switch (f)
        {
            case Fase.Inicio: TelaInicio(); break;
            case Fase.ComoJogar: TelaComoJogar(); break;
            case Fase.Boletim: TelaBoletim(); break;
            case Fase.Escolha: TelaEscolha(); break;
            case Fase.MinijogoInfo: TelaMinijogoInfo(); break;
            case Fase.DecisaoVazamento: TelaDecisaoVazamento(); break;
            case Fase.TarefaConcluida: TelaConcluida(); break;
            case Fase.Distribuir: TelaDistribuir(); break;
            case Fase.DiaAcontece: TelaDiaAcontece(); break;
            case Fase.Resultado: TelaResultado(); break;
            case Fase.Relatorio: TelaRelatorio(); break;
            case Fase.Creditos: TelaCreditos(); break;
            case Fase.MenuMinijogos: TelaMenuMinijogos(); break;
        }

        // Rodapé e cabeçalho só existem se a tela pôs algo neles (childCount ainda conta os filhos destruídos).
        bool temRodape = TemAtivos(_rodape);
        bool temCabecalho = TemAtivos(_cabecalho);
        _rodape.gameObject.SetActive(temRodape);
        _cabecalho.gameObject.SetActive(temCabecalho);
        PosicionarPainel(tam, temRodape);
        Canvas.ForceUpdateCanvases();
        if (temCabecalho) LayoutRebuilder.ForceRebuildLayoutImmediate(_cabecalho);
        float alturaCabecalho = temCabecalho ? LayoutUtility.GetPreferredHeight(_cabecalho) + UiTema.E8 : 0f;
        _viewport.offsetMin = new Vector2(30f, 24f);
        _viewport.offsetMax = new Vector2(-30f, -(32f + alturaCabecalho));
        LayoutRebuilder.ForceRebuildLayoutImmediate(_conteudo);
        if (temRodape) LayoutRebuilder.ForceRebuildLayoutImmediate(_rodape);
        // Redesenhar a mesma fase (clicar em + ou −, trocar o idioma) mantém a rolagem e não repete a entrada do painel.
        if (f != _fasePintada)
        {
            _scroll.verticalNormalizedPosition = 1f;
            _animInicio = Time.unscaledTime;
            AnimarPainel(); // já no primeiro quadro, sem um relance do painel inteiro
        }
        _fasePintada = f;
        Entrada.Selecionar(_primeiro != null ? _primeiro.gameObject : null);
    }

    // ---------------- Início ----------------

    void TelaInicio()
    {
        // Coluna à direita, com a base ao fundo. As três dicas de "como jogar" estão nos slides, não aqui.
        Rot(_conteudo, Textos.Titulo.ToString(), Papel.TituloTela);
        Rot(_conteudo, Textos.IntroTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        Secao(Textos.EscolhaMissao.ToString());
        var cartoes = Vertical(_conteudo, UiTema.E16, 0);
        _primeiro = Escolha(cartoes, UiTema.Estufa, Textos.Cadete.ToString(), Textos.CadeteIdade.ToString(), Textos.CadeteDesc.ToString(), () => _gm.Comecar(Modo.Cadete), true, false);
        Escolha(cartoes, UiTema.Lab, Textos.Comandante.ToString(), Textos.ComandanteIdade.ToString(), Textos.ComandanteDesc.ToString(), () => _gm.Comecar(Modo.Comandante), true, false);
        var grade = RT("Grade", _conteudo);
        var gl = grade.gameObject.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(282, 96); gl.spacing = new Vector2(UiTema.E16, UiTema.E16);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 2;
        Sec(grade, Textos.ComoJogar.ToString(), "ajuda", () => _gm.AbrirComoJogar(), 282);
        Sec(grade, Textos.TutorialDia1.F(_gm.TutorialLigado ? Textos.Ligado : Textos.Desligado).ToString(), _gm.TutorialLigado ? "check" : null, () => _gm.AlternarTutorial(), 282);
        Sec(grade, Textos.Creditos.ToString(), "fonte", () => _gm.AbrirCreditos(), 282);
        Sec(grade, Textos.BotaoIdioma.ToString(), "globo", () => _gm.AlternarIdioma(), 282);
        Rot(_conteudo, Textos.Rodape.ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter);
    }

    void TelaComoJogar()
    {
        int i = Mathf.Clamp(_gm.Slide, 0, Textos.SlideTitulo.Length - 1);
        Tam(RT("Espaco", _conteudo), -1, 160); // a página começa mais abaixo, para o slide ficar no meio da tela
        Rot(_conteudo, $"{i + 1} / {Textos.SlideTitulo.Length}", Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter, true);
        Rot(_conteudo, Textos.SlideTitulo[i].ToString(), Papel.TituloTela, UiTema.Branco, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.SlideTexto[i].ToString(), Papel.TituloPainel, UiTema.Apoio, TextAnchor.MiddleCenter);
        bool ultimo = i >= Textos.SlideTitulo.Length - 1;
        var voltar = Secundario(Textos.Voltar.ToString(), null, () => _gm.SlideAnterior());
        voltar.SemCadeado = true; // no primeiro slide não há para onde voltar: só fica cinza
        voltar.interactable = i > 0;
        Secundario(Textos.Pular.ToString(), "pular", () => _gm.PularSlides());
        Principal((ultimo ? Textos.Comecar : Textos.Proximo).ToString(), () => _gm.SlideProximo());
    }

    // ---------------- Boletim ----------------

    void TelaBoletim()
    {
        var m = _gm.Missao; var T = m.Hoje;
        DicaTutorial(Textos.DicaTutBoletim);
        Rot(_conteudo, Textos.Boletim.ToString(), Papel.TituloPainel);
        Rot(_conteudo, RegrasMissao.ParteDaMissao(m.Dia).ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);
        var luz = (m.EhSombra(m.Dia) ? Textos.LuzSombra : Textos.LuzNormal).F(T.Luz);
        if (m.Bateria > 0) luz = luz + Textos.NaBateria.F(m.Bateria);
        Linha(_conteudo, m.EhSombra(m.Dia) ? "sombra" : "sol", Textos.LuzDeHoje.ToString(), luz.ToString());
        var al = m.AlertaDe(m.Dia);
        if (al != null)
        {
            Alerta(_conteudo, (al.Forte ? Textos.AlertaForte : Textos.AlertaFraco).ToString(), al.Forte);
            Rot(_conteudo, (al.Forte ? Textos.AlertaForteTexto : Textos.AlertaFracoTexto).ToString(), Papel.Corpo, UiTema.Apoio);
            Rot(_conteudo, Textos.AbrigoCusta.ToString(), Papel.Corpo, UiTema.Apoio);
        }
        else Linha(_conteudo, "sol", Textos.ClimaEspacial.ToString(), Textos.SolCalmo.ToString());
        if (m.Vazamento) Linha(_conteudo, "tarefa_remendo", Textos.VazamentoAberto.ToString(), Textos.VazamentoAbertoTexto.ToString());
        if (T.Fit != 0) Linha(_conteudo, "check", Textos.Tripulacao.ToString(), (T.Fit > 0 ? Textos.TripulacaoForte : Textos.TripulacaoFraca).ToString());
        if (!m.Contato) Linha(_conteudo, "terra", Textos.SemContato.ToString(), Textos.SemContatoTexto.ToString());
        Principal(Textos.Continuar.ToString(), () => _gm.SairDoBoletim());
    }

    // ---------------- Tarefa ----------------

    void TelaEscolha()
    {
        var m = _gm.Missao; var d = Tarefas.Def(m.Hoje.Tarefa);
        if (d.Id == Tarefas.Amostra) DicaTutorial(Textos.DicaTutEscolha);
        Rot(_conteudo, d.Titulo.ToString(), Papel.TituloPainel);
        Rot(_conteudo, d.Texto.ToString(), Papel.Corpo, UiTema.Apoio);
        if (d.Id == Tarefas.Vazamento)
        {
            Principal(Tarefas.VazBotao.ToString(), () => _gm.EscolherFazer());
            return;
        }
        if (m.AlertaDe(m.Dia) != null && d.Fora) Alerta(_conteudo, Textos.RiscoErupcao.ToString(), false);
        var linha = Vertical(_conteudo, UiTema.E16, 0); // empilhados: o painel lateral é estreito
        bool pode = Tarefas.PodeFazer(m);
        var cor = UiTema.CorDaTarefa(d.Id);
        _primeiro = Escolha(linha, cor, d.FazerTitulo.ToString(), d.FazerCusto.ToString(), (pode ? d.FazerDesc : Textos.OxigenioBaixo).ToString(), () => _gm.EscolherFazer(), pode);
        // "Ficar" é do mesmo nível que "fazer", sempre sem vermelho: a tira neutra diz que é a escolha calma.
        var ficar = Escolha(linha, d.FicarRuim ? UiTema.Apoio : UiTema.Branco, d.FicarTitulo.ToString(), d.FicarCusto.ToString(), d.FicarDesc.ToString(), () => _gm.EscolherFicar(), true);
        if (!pode) _primeiro = ficar;
        if (!d.Fonte.Vazio) Rot(_conteudo, d.Fonte.ToString(), Papel.Rotulo, UiTema.Apoio);
    }

    void TelaMinijogoInfo()
    {
        // Cartão de primeira vez (tarefa 7.7): ícone e título da tarefa, a instrução e a ilustração do gesto, com a câmera
        // já no close da estação, à esquerda.
        var d = Tarefas.Def(_gm.Missao.Hoje.Tarefa);
        if (d.Id == Tarefas.Amostra) DicaTutorial(Textos.DicaTutMinijogo);
        var cab = Horizontal(_conteudo, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
        Icone(cab, UiTema.IconeDaTarefa(d.Id), 64);
        Rot(cab, d.MiniTitulo.ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleLeft);
        Rot(_conteudo, d.MiniInstrucao.ToString(), Papel.Corpo, UiTema.Apoio);
        var ilu = RT("Ilustracao", _conteudo);
        Tam(ilu, -1, 230);
        Fundo(ilu, UiTema.Vazio, UiTema.ChanfroBotao, "SE ID", false);
        var palco = RT("Palco", ilu);
        palco.anchorMin = palco.anchorMax = palco.pivot = new Vector2(0.5f, 0.5f);
        palco.sizeDelta = new Vector2(560, 230);
        DemoGesto.Criar(palco, d.Mec, Vector2.zero, Vector2.zero, true, UiTema.CorDaTarefa(d.Id));
        var leg = Horizontal(_conteudo, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
        Icone(leg, "mao", 36);
        Rot(leg, DemoGesto.Legenda(d.Mec).ToString(), Papel.Rotulo, UiTema.Branco, TextAnchor.MiddleLeft, true);
        Rot(_conteudo, Textos.PrimeiraVez.ToString(), Papel.Rotulo, UiTema.Apoio);
        Principal(Textos.Comecar.ToString(), () => _gm.ComecarMinijogo());
    }

    void TelaDecisaoVazamento()
    {
        var T = _gm.Missao.Hoje;
        Rot(_conteudo, Tarefas.VazDecisaoTitulo.ToString(), Papel.TituloPainel);
        Linha(_conteudo, "tarefa_remendo", null, T.MsgVazamento.ToString());
        Rot(_conteudo, Tarefas.VazDecisaoTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        var linha = Vertical(_conteudo, UiTema.E16, 0);
        _primeiro = Escolha(linha, UiTema.Ar, Tarefas.VazReforcarTitulo.ToString(), Tarefas.VazReforcarCusto.ToString(), Tarefas.VazReforcarDesc.ToString(), () => _gm.VazamentoReforcar(), true);
        Escolha(linha, UiTema.Apoio, Tarefas.VazDepoisTitulo.ToString(), Tarefas.VazDepoisCusto.ToString(), Tarefas.VazDepoisDesc.ToString(), () => _gm.VazamentoDepois(), true);
    }

    void TelaConcluida()
    {
        var T = _gm.Missao.Hoje; var d = Tarefas.Def(T.Tarefa);
        DicaTutorial(Textos.DicaTutConcluida);
        Rot(_conteudo, d.Titulo.ToString(), Papel.TituloPainel);
        Linha(_conteudo, "check", null, T.MsgTarefa.ToString());
        if (!T.Extra.Vazio) Linha(_conteudo, "ajuda", Textos.VoceSabia.ToString(), (T.Extra + new Txt("\n", "\n") + T.ExtraFonte).ToString());
        if (_gm.Teste)
        {
            Secundario(Textos.MenuMinijogos.ToString(), null, () => _gm.AbrirMenuMinijogos());
            Principal(Textos.JogarDeNovo.ToString(), () => _gm.TestarDeNovo());
        }
        else Principal(Textos.VoltarAoHub.ToString(), () => _gm.VoltarAoHub());
    }

    // ---------------- Distribuição ----------------

    void TelaDistribuir()
    {
        var m = _gm.Missao; var a = m.Hoje.Aloc;
        int livres = RegrasMissao.CelulasLivres(m);
        // Cabeçalho fixo: o título e a prévia do que as células vão fazer ficam sempre à vista enquanto a lista rola.
        Rot(_cabecalho, Textos.Distribuir.ToString(), Papel.TituloPainel);
        var dica = Vertical(_cabecalho, UiTema.E4, 14, null);
        Fundo(dica, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        Rot(dica, Textos.DicaControle.ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);
        foreach (var l in RegrasMissao.Previa(m)) Rot(dica, "·  " + l, Papel.Rotulo, UiTema.Branco);
        if (livres > 0) Rot(dica, Textos.CelulasVaoSePerder.F(livres).ToString(), Papel.Rotulo, UiTema.Energia);

        // Corpo: a lista vertical de módulos.
        DicaTutorial(Textos.DicaTutDistribuir);
        Rot(_conteudo, (Textos.DistribuirTexto + (m.Modo == Modo.Cadete ? Textos.SugestaoCadete : Txt.Nada)).ToString(), Papel.Rotulo, UiTema.Apoio);
        if (m.Hoje.TempoEsgotou) Linha(_conteudo, "relogio", Textos.TempoAcabou.ToString(), m.Hoje.MsgTarefa.ToString());
        var al = m.AlertaDe(m.Dia);
        if (al != null) Alerta(_conteudo, (al.Forte ? Textos.AlertaForteHoje : Textos.AlertaFracoHoje).ToString(), al.Forte);

        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = UiTema.CorDoModulo(mod);
            var linha = Horizontal(_conteudo, UiTema.E12, false, 12, null, TextAnchor.MiddleLeft);
            Fundo(linha, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
            linha.gameObject.AddComponent<LayoutElement>().minHeight = 104;
            TiraLateral(linha, cor);
            Icone(linha, UiTema.IconeDoModulo(mod), 48);
            var texto = Vertical(linha, 2, 0, null, TextAnchor.MiddleLeft);
            texto.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Rot(texto, Textos.NomeModulo[(int)mod].ToString(), Papel.Corpo, UiTema.Branco);
            Rot(texto, RegrasMissao.DicaModulo(m, mod).ToString(), Papel.Rotulo, UiTema.Apoio);
            var slots = Horizontal(linha, 6, false, 0, null, TextAnchor.MiddleCenter);
            Celulas(slots, a[mod], RegrasMissao.Maximo(mod) - a[mod]);
            var Mod = mod;
            var menos = Sec(linha, "\u2212", null, () => _gm.Menos(Mod), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
            menos.SemCadeado = true;
            menos.Rotulo.alignment = TextAnchor.MiddleCenter;
            menos.interactable = a[mod] > 0;
            var mais = Sec(linha, "+", null, () => _gm.Mais(Mod), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
            mais.SemCadeado = true;
            mais.Rotulo.alignment = TextAnchor.MiddleCenter;
            mais.interactable = RegrasMissao.PodeMais(m, mod);
            if (_primeiro == null && mais.interactable) _primeiro = mais;
        }

        // Rodapé: a bandeja das células livres, fixa embaixo, e o botão principal.
        var bandeja = Horizontal(_rodape, UiTema.E12, false, 14, null, TextAnchor.MiddleLeft);
        Fundo(bandeja, UiTema.Aco, UiTema.ChanfroBotao, "SE ID", true);
        Rot(bandeja, Textos.CelulasLivres.F(livres).ToString(), Papel.Corpo, UiTema.Branco, TextAnchor.MiddleLeft);
        var livresSlots = Horizontal(bandeja, 6, false, 0, null, TextAnchor.MiddleCenter);
        Celulas(livresSlots, livres, 0);
        Tam(bandeja, -1, UiTema.AlvoMinimo);
        var comecar = Principal(Textos.ComecarDia.ToString(), () => _gm.ComecarODia());
        if (_primeiro == null) _primeiro = comecar;
    }

    /// <summary>Faixa vertical na cor do módulo, na borda esquerda de uma linha (ignora o layout do pai).</summary>
    static void TiraLateral(RectTransform linha, Color cor)
    {
        var t = RT("Tira", linha);
        t.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        t.anchorMin = new Vector2(0, 0); t.anchorMax = new Vector2(0, 1); t.pivot = new Vector2(0, 0.5f);
        t.sizeDelta = new Vector2(8f, -2f * UiTema.ChanfroBotao); t.anchoredPosition = Vector2.zero;
        var im = t.gameObject.AddComponent<Image>(); im.color = cor; im.raycastTarget = false;
    }

    void Celulas(Transform pai, int ligadas, int vazias)
    {
        for (int i = 0; i < ligadas + vazias; i++)
        {
            var c = RT("Celula", pai);
            var f = c.gameObject.AddComponent<FormaUI>();
            f.chanfro = UiTema.ChanfroCelula; f.cantoSE = f.cantoID = true; f.cantoSD = f.cantoIE = false; f.raycastTarget = false;
            f.color = i < ligadas ? UiTema.Energia : UiTema.Vazio;
            Tam(c, 26, 40);
        }
    }

    // ---------------- O dia acontece ----------------

    void TelaDiaAcontece()
    {
        var m = _gm.Missao; var a = m.Hoje.Aloc;
        _legenda = Rot(_conteudo, RegrasMissao.Legenda(m, _gm.PassoAnim).ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.UpperLeft);
        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = UiTema.CorDoModulo(mod);
            bool ligado = a[mod] > 0;
            var linha = Horizontal(_conteudo, UiTema.E12, false, 12, null, TextAnchor.MiddleLeft);
            Fundo(linha, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
            linha.gameObject.AddComponent<LayoutElement>().minHeight = 72;
            TiraLateral(linha, ligado ? cor : UiTema.Vazio);
            Icone(linha, UiTema.IconeDoModulo(mod), 44);
            var nome = Rot(linha, Textos.NomeModulo[(int)mod].ToString(), Papel.Corpo, ligado ? UiTema.Branco : UiTema.Apoio, TextAnchor.MiddleLeft);
            nome.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var s = Horizontal(linha, 6, false, 0, null, TextAnchor.MiddleCenter);
            Celulas(s, a[mod], 0);
        }
        Principal(Textos.Pular.ToString(), () => _gm.PularAnim());
    }

    public void AtualizarLegenda()
    {
        if (_legenda != null && _gm.Missao != null) _legenda.text = RegrasMissao.Legenda(_gm.Missao, _gm.PassoAnim).ToString();
    }

    // ---------------- Resultado ----------------

    void TelaResultado()
    {
        var m = _gm.Missao; var r = m.Hoje.Res;
        // Cabeçalho fixo: o antes e o depois de cada medidor ficam sempre à vista; o resto rola.
        Rot(_cabecalho, Textos.FimDoDia.F(m.Dia).ToString(), Papel.TituloPainel);
        LinhaDelta(_cabecalho, "oxigenio", Textos.Oxigenio, r.O2Antes, Math.Max(0, m.O2));
        LinhaDelta(_cabecalho, "radiacao", Textos.Radiacao, r.RadAntes, m.Rad);
        LinhaDelta(_cabecalho, "mod_bateria", Textos.Bateria, r.BatAntes, m.Bateria);
        DicaTutorial(Textos.DicaTutResultado);
        // O cartão do dado real vem antes do "Por quê?": a fonte da NASA fica visível sem rolar.
        if (r.Erupcao != null) CartaoErupcao(r.Erupcao);
        Secao(Textos.PorQue.ToString());
        foreach (var l in r.Linhas) Linha(_conteudo, IconeDoTom(l.Tom), null, l.Texto.ToString());
        Principal((r.Fim != Fim.Nenhum ? Textos.VerRelatorio : Textos.ProximoDia).ToString(), () => _gm.Proximo());
    }

    static string IconeDoTom(Tom t) => t switch
    {
        Tom.O2 => "oxigenio", Tom.Verde => "mod_estufa", Tom.Rad => "radiacao", Tom.Laranja => "mod_lab",
        Tom.Energia => "energia", _ => "tarefa_remendo",
    };

    /// <summary>Antes e depois de um medidor: ícone, nome, "{antes} para {depois}" e a variação em pílula (▲ ▼ =, por forma).</summary>
    void LinhaDelta(Transform pai, string icone, Txt nome, int antes, int depois)
    {
        var h = Horizontal(pai, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
        Icone(h, icone, 56);
        Rot(h, nome.ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleLeft).horizontalOverflow = HorizontalWrapMode.Overflow;
        var de = Rot(h, Textos.DeAte.F(antes, depois).ToString(), Papel.Corpo, UiTema.Apoio, TextAnchor.MiddleLeft);
        de.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Pilula(h, depois - antes);
    }

    /// <summary>Pílula escura com triângulo para cima, para baixo ou "=" e o número. O sentido vem da forma, nunca da cor.</summary>
    void Pilula(Transform pai, int delta)
    {
        var p = Horizontal(pai, UiTema.E8, false, 12, null, TextAnchor.MiddleCenter);
        Fundo(p, UiTema.Sombra, UiTema.ChanfroBotao, "SE ID", false);
        Tam(p, 130, 56);
        if (delta == 0) { Rot(p, "=", Papel.Numero, UiTema.Branco, TextAnchor.MiddleCenter); return; }
        var tri = FormaUI.Criar(p, FormaUI.Modo.Triangulo, UiTema.Branco, "Sentido");
        tri.rotacao = delta > 0 ? 0f : 180f;
        var le = tri.gameObject.AddComponent<LayoutElement>(); le.minWidth = le.preferredWidth = 26; le.minHeight = le.preferredHeight = 24;
        Rot(p, Math.Abs(delta).ToString(), Papel.Numero, UiTema.Branco, TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// Cartão "Isso aconteceu de verdade": papel branco, ícone de fonte, título fixo, a etiqueta de arquivado, o dado e a
    /// fonte. No painel estreito cada coisa fica na sua linha.
    /// </summary>
    void CartaoErupcao(Erupcao e)
    {
        var card = Vertical(_conteudo, UiTema.E8, 20, null);
        Fundo(card, UiTema.Branco, UiTema.ChanfroPainel, "SE ID", true);
        // A sombra do papel é o cinza do papel, não o aço.
        card.GetChild(0).GetComponent<FormaUI>().color = UiTema.BrancoSombra;
        var cab = Horizontal(card, UiTema.E12, false, 0, null, TextAnchor.MiddleLeft);
        Icone(cab, "fonte_tinta", 44);
        Rot(cab, Textos.AconteceuDeVerdade.ToString(), Papel.TituloPainel, UiTema.Tinta, TextAnchor.MiddleLeft);
        // Etiqueta escura: dado arquivado, nunca "agora" nem "ao vivo".
        var linhaEtiqueta = Horizontal(card, 0, false, 0, null, TextAnchor.MiddleLeft);
        var et = Horizontal(linhaEtiqueta, 0, false, 12, null, TextAnchor.MiddleCenter);
        Fundo(et, UiTema.Tinta, UiTema.ChanfroCelula, "SE ID", false);
        Rot(et, Textos.EtiquetaArquivado.ToString(), Papel.Rotulo, UiTema.Branco, TextAnchor.MiddleCenter, true);
        Rot(card, $"{Textos.ErupcaoClasse.F(e.Classe)} · {Idioma.Data(e.Data)}", Papel.TituloPainel, UiTema.Tinta);
        Rot(card, Textos.ValeComo.F(Textos.Intensidade[Mathf.Clamp(e.Intensidade, 0, 2)]).ToString(), Papel.Corpo, UiTema.Tinta);
        var rodape = Horizontal(card, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
        var fonte = Rot(rodape, Textos.FonteDonki.ToString(), Papel.Rotulo, UiTema.Tinta);
        fonte.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Link(rodape, Textos.VerRegistro.ToString(), e.Url, 260, UiTema.AlvoMinimo);
    }

    // ---------------- Relatório ----------------

    void TelaRelatorio()
    {
        var m = _gm.Missao;
        int n = RegrasMissao.Nota(m);
        // Página opaca de céu preto, em duas colunas: o resumo à esquerda e o que pesou à direita.
        var colunas = Horizontal(_conteudo, UiTema.E48, false, 0, null, TextAnchor.UpperLeft);
        var esq = Vertical(colunas, UiTema.E16, 0);
        esq.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        var dir = Vertical(colunas, UiTema.E16, 0);
        Tam(dir, 520, -1);

        Rot(esq, RegrasMissao.TituloFim(m).ToString(), Papel.TituloTela);
        Rot(esq, RegrasMissao.SubtituloFim(m).ToString(), Papel.Corpo, UiTema.Apoio);
        if (m.Fim == Fim.Completa)
        {
            // A estrela apagada é só cinza: não conquistada, sem mensagem negativa.
            var est = Horizontal(esq, UiTema.E12, false, 0, null, TextAnchor.MiddleLeft);
            for (int i = 0; i < 3; i++) Icone(est, i < n ? "estrela" : "estrela_apagada", 88);
            Rot(est, Textos.EstrelasDe3.F(n).ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleLeft);
        }
        // Mosaicos: ícone, número e a palavra.
        var mosaicos = Horizontal(esq, UiTema.E12, true, 0, null, TextAnchor.UpperLeft);
        Mosaico(mosaicos, "estrela", m.Estrelas.ToString(), Textos.Ciencia.ToString());
        Mosaico(mosaicos, "oxigenio", $"{Math.Max(0, m.O2)}/{Missao.MaxO2}", Textos.Oxigenio.ToString());
        Mosaico(mosaicos, "radiacao", $"{m.Rad}/{Missao.MaxRad}", Textos.Radiacao.ToString());
        Mosaico(mosaicos, "comida", m.Comida.ToString(), Textos.Comida.ToString());
        Mosaico(mosaicos, "mod_bateria", $"{m.Bateria}/{Missao.MaxBat}", Textos.Bateria.ToString());
        Grafico(esq, m);

        var mom = RegrasMissao.Momentos(m);
        if (mom.Count > 0)
        {
            Rot(dir, UiTema.Caixa(Textos.OQueMaisPesou.ToString()), Papel.Rotulo, UiTema.Apoio);
            foreach (var p in mom) Linha(dir, "tarefa_expedicao", Textos.DiaN.F(p.Dia).ToString(), p.Momento.ToString());
        }
        Rot(dir, UiTema.Caixa(Textos.ParaProxima.ToString()), Papel.Rotulo, UiTema.Apoio);
        Linha(dir, "mod_estufa", null, RegrasMissao.DicaFim(m).ToString());
        Secundario(Textos.Creditos.ToString(), "fonte", () => _gm.AbrirCreditos());
        Principal(Textos.JogarDeNovo.ToString(), () => _gm.JogarDeNovo());
    }

    void Mosaico(Transform pai, string icone, string numero, string palavra)
    {
        var v = Vertical(pai, UiTema.E4, 14, null, TextAnchor.UpperLeft);
        Fundo(v, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Icone(v, icone, 48);
        Rot(v, numero, Papel.TituloPainel, UiTema.Branco);
        Rot(v, UiTema.Caixa(palavra), Papel.Rotulo, UiTema.Apoio);
    }

    /// <summary>
    /// Gráfico de Oxigênio, Radiação e Bateria por dia (web: chart), desenhado com segmentos de Image. As três linhas se
    /// distinguem também pela forma do ponto (círculo, triângulo, quadrado): o gráfico se lê sem cor.
    /// </summary>
    void Grafico(Transform pai, Missao m)
    {
        Rot(pai, UiTema.Caixa(Textos.GraficoTitulo.ToString()), Papel.Rotulo, UiTema.Apoio);
        const float W = 860f, H = 260f, margemX = 40f, margemY = 34f;
        var g = RT("Grafico", pai);
        g.gameObject.AddComponent<Image>().color = UiTema.Vazio;
        Tam(g, -1, H);
        float larg = W;
        float X(int i) => margemX + (larg - margemX - 20f) * i / (Missao.Dias - 1);
        float Y(float v) => margemY + (H - margemY - 16f) * v;
        for (int i = 0; i < Missao.Dias; i++)
        {
            Segmento(g, new Vector2(X(i), margemY), new Vector2(X(i), H - 16f), 2f, UiTema.AcoClaro);
            var t = Texto(g, (i + 1).ToString(), 24, Vector2.zero, new Vector2(40, 26));
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = Vector2.zero;
            t.rectTransform.anchoredPosition = new Vector2(X(i), 14);
            t.color = UiTema.Apoio;
        }
        Serie(g, m, p => p.O2 / (float)Missao.MaxO2, UiTema.Ar, FormaDoPonto.Circulo, X, Y);
        Serie(g, m, p => p.Rad / (float)Missao.MaxRad, UiTema.Abrigo, FormaDoPonto.Triangulo, X, Y);
        Serie(g, m, p => p.Bat / (float)Missao.MaxBat, UiTema.Energia, FormaDoPonto.Quadrado, X, Y);
        var leg = Horizontal(pai, 30, false, 0, null, TextAnchor.MiddleLeft);
        foreach (var (nome, cor, forma) in new[] { (Textos.Oxigenio, UiTema.Ar, FormaDoPonto.Circulo), (Textos.Radiacao, UiTema.Abrigo, FormaDoPonto.Triangulo), (Textos.Bateria, UiTema.Energia, FormaDoPonto.Quadrado) })
        {
            var item = Horizontal(leg, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
            var q = Ponto(item, forma, cor, 22f);
            Tam(q, 22, 22);
            Rot(item, nome.ToString(), Papel.Corpo, cor == UiTema.Energia ? UiTema.Energia : UiTema.Branco);
        }
    }

    enum FormaDoPonto { Circulo, Triangulo, Quadrado }

    /// <summary>Ponto de uma série: a forma é o que distingue a linha quando não se vê a cor.</summary>
    static RectTransform Ponto(Transform pai, FormaDoPonto forma, Color cor, float tam)
    {
        FormaUI f;
        switch (forma)
        {
            case FormaDoPonto.Triangulo: f = FormaUI.Criar(pai, FormaUI.Modo.Triangulo, cor, "PontoTriangulo"); break;
            case FormaDoPonto.Quadrado:
                f = FormaUI.Criar(pai, FormaUI.Modo.Chanfrado, cor, "PontoQuadrado");
                f.chanfro = 0; f.cantoSE = f.cantoID = false; break;
            default:
                f = FormaUI.Criar(pai, FormaUI.Modo.Poligono, cor, "PontoCirculo");
                f.lados = 20; f.irregularidade = 0f; break;
        }
        var rt = f.rectTransform;
        rt.sizeDelta = new Vector2(tam, tam);
        return rt;
    }

    void Serie(RectTransform g, Missao m, Func<RegistroDia, float> f, Color cor, FormaDoPonto forma, Func<int, float> X, Func<float, float> Y)
    {
        Vector2? ant = null;
        for (int i = 0; i < m.Hist.Count; i++)
        {
            var p = new Vector2(X(i), Y(Mathf.Clamp01(f(m.Hist[i]))));
            if (ant.HasValue) Segmento(g, ant.Value, p, 5f, cor);
            var ponto = Ponto(g, forma, cor, 20f);
            ponto.anchorMin = ponto.anchorMax = Vector2.zero;
            ponto.pivot = new Vector2(0.5f, 0.5f);
            ponto.anchoredPosition = p;
            ant = p;
        }
    }

    void Segmento(RectTransform pai, Vector2 a, Vector2 b, float espessura, Color cor)
    {
        var s = RT("Seg", pai);
        var im = s.gameObject.AddComponent<Image>(); im.color = cor; im.raycastTarget = false;
        s.anchorMin = s.anchorMax = Vector2.zero;
        s.pivot = new Vector2(0, 0.5f);
        var d = b - a;
        s.sizeDelta = new Vector2(d.magnitude, espessura);
        s.anchoredPosition = a;
        s.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    // ---------------- Créditos ----------------

    void TelaCreditos()
    {
        Rot(_conteudo, Textos.Creditos.ToString(), Papel.TituloPainel);
        Secao(Textos.DadosNasa.ToString());
        foreach (var (nome, url, uso) in Textos.FontesNasa)
        {
            var n = Vertical(_conteudo, UiTema.E8, 16, null);
            Fundo(n, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
            Rot(n, nome, Papel.Corpo, UiTema.Branco);
            Rot(n, uso.ToString(), Papel.Corpo, UiTema.Apoio);
            var l = Horizontal(n, UiTema.E12, false);
            Link(l, Idioma.L("Abrir", "Open"), url, 240);
            Rot(l, url, Papel.Rotulo, UiTema.Apoio);
        }
        Secao(Textos.NumeroDeJogo.ToString());
        Rot(_conteudo, Textos.NumeroDeJogoTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        Secao(Textos.UsoDeIA.ToString());
        Rot(_conteudo, Textos.UsoDeIATexto.ToString(), Papel.Corpo, UiTema.Apoio);
        Secao(Textos.Tipografia.ToString());
        Rot(_conteudo, Textos.TipografiaTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        Principal(Textos.Voltar.ToString(), () => _gm.FecharCreditos());
    }

    void Link(Transform pai, string rotulo, string url, float largura, float altura = 96f)
    {
        var b = Sec(pai, rotulo, "seta", null, largura, altura);
        b.gameObject.AddComponent<BotaoLink>().url = url;
    }

    // ---------------- Menu de minijogos ----------------

    void TelaMenuMinijogos()
    {
        Rot(_conteudo, Textos.TesteMinijogos.ToString(), Papel.TituloPainel);
        Rot(_conteudo, Textos.TesteMinijogosTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        var modos = Horizontal(_conteudo, UiTema.E16, false);
        Sec(modos, Textos.CadeteMaisTempo.ToString(), _gm.ModoTeste == Modo.Cadete ? "check" : null, () => _gm.DefinirModoTeste(Modo.Cadete), 460);
        Sec(modos, Textos.Comandante.ToString(), _gm.ModoTeste == Modo.Comandante ? "check" : null, () => _gm.DefinirModoTeste(Modo.Comandante), 360);
        var grade = RT("Grade", _conteudo);
        var gl = grade.gameObject.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(350, 96); gl.spacing = new Vector2(UiTema.E16, UiTema.E16);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 4;
        foreach (var id in Tarefas.Todas)
        {
            var d = Tarefas.Def(id);
            var Id = id;
            var b = Sec(grade, d.MiniTitulo.ToString(), UiTema.IconeDaTarefa(id), () => _gm.TestarMinijogo(Id), 350);
            var le = b.GetComponent<LayoutElement>(); if (le != null) UnityEngine.Object.Destroy(le); // a grade manda no tamanho
            if (_primeiro == null) _primeiro = b;
        }
    }

    // ---------------- Faixa do deslocamento ----------------

    void MontarFaixa()
    {
        var m = _gm.Missao;
        if (m == null) return;
        Txt destino = _gm.Fase == Fase.AteHub ? Textos.ConsoleHub : Tarefas.Def(m.Hoje.Tarefa).Nome;
        _faixaTexto.text = Textos.LeveAte.F(destino).ToString();
        // Ícone e cor do destino: a tarefa do dia, ou o console de energia no hub.
        bool console = _gm.Fase == Fase.AteHub;
        _corDestino = console ? UiTema.Energia : UiTema.CorDaTarefa(m.Hoje.Tarefa);
        _faixaIcone.sprite = UiTema.Icone(console ? "energia" : UiTema.IconeDaTarefa(m.Hoje.Tarefa));
        _faixaIcone.enabled = _faixaIcone.sprite != null;
        _faixaTira.color = _corDestino;
        bool tut = _gm.TutorialAtivo;
        _faixaDica.gameObject.SetActive(tut);
        if (tut) _faixaDica.text = (_gm.Fase == Fase.AteHub ? Textos.DicaTutAteHub : Textos.DicaTutAteTarefa).ToString();
    }

    void DicaTutorial(Txt t)
    {
        if (!_gm.TutorialAtivo) return;
        var caixa = Vertical(_conteudo, 0, 16, null);
        Fundo(caixa, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        Linha(caixa, "ajuda", Textos.ControleDaMissao.ToString(), t.ToString(), Papel.Rotulo); // menor: o painel lateral é estreito
    }

    // =====================================================================
    // Atualização por frame
    // =====================================================================

    public void AtualizarHud()
    {
        VerificarTamanhoDaTela();
        AnimarPainel();
        AnimarMinigame();
        if (_avisoTexto.text != "" && Time.time > _avisoAte) _avisoTexto.text = "";
        if (!_hud.activeSelf || _gm.Missao == null) return;
        var m = _gm.Missao;
        _hudDia.text = Textos.DiaDe.F(Mathf.Max(1, m.Dia), Missao.Dias).ToString();
        bool salta = !ReferenceEquals(m, _missaoVista); // missão nova: sem animação
        _missaoVista = m;
        _medO2.Definir(Mathf.Max(0, m.O2), salta);
        _medRad.Definir(m.Rad, salta);
        _medBat.Definir(m.Bateria, salta);
        _hudEstrelas.text = m.Estrelas.ToString();
        int t = Mathf.CeilToInt(Mathf.Max(0f, _gm.TempoRestante));
        _hudRelogio.text = $"{t / 60}:{t % 60:00}";
        // Sem vermelho nem pulso: no último minuto o relógio só muda para a cor da energia.
        _hudRelogio.color = _gm.RelogioCorre ? (t <= 60 ? UiTema.Energia : UiTema.Branco) : UiTema.Apoio;
    }

    public void Aviso(Txt texto)
    {
        _avisoTexto.text = texto.ToString();
        _avisoAte = Time.time + 2.6f;
    }

    /// <summary>Seta para o destino e botão de ação quando o astronauta está no raio dele.</summary>
    public void AtualizarGuia(Estacao alvo, Estacao proxima, Txt rotuloAcao)
    {
        _btnAcao.gameObject.SetActive(proxima != null);
        if (proxima != null)
        {
            _btnAcao.Texto(rotuloAcao.ToString());
            // Com os joysticks na tela, o botão fica no canto direito, logo acima do joystick da câmera, sem cobri-lo.
            var joy = _gm.jogador != null ? _gm.jogador.joystickCamera : null;
            float y = MargemRodapeBaixo;
            if (joy != null && joy.gameObject.activeInHierarchy)
            {
                var jr = (RectTransform)joy.transform;
                y = Mathf.Max(y, jr.anchoredPosition.y + jr.sizeDelta.y * (1f - jr.pivot.y) + UiTema.E16);
            }
            ((RectTransform)_btnAcao.transform).anchoredPosition = new Vector2(-MargemRodape, y);
        }

        var cam = Camera.main;
        bool mostrar = alvo != null && proxima == null && cam != null && _gm.jogador != null;
        _seta.gameObject.SetActive(false); // a seta agora é 3D, ao redor do corpo (SetaGuia3D)
        _setaTexto.gameObject.SetActive(mostrar);
        if (!mostrar) { if (_seta3D != null) _seta3D.Esconder(); return; }

        // seta no chão: aponta para o destino, na cor do módulo de destino
        _seta3D.Atualizar(_gm.jogador.transform, alvo.transform.position, _corDestino);

        // o texto (nome + distância) fica logo abaixo da seta 3D, na tela
        Vector3 sp = cam.WorldToScreenPoint(_seta3D.Posicao);
        Vector2 pos = sp.z > 0f ? new Vector2(sp.x, sp.y) : new Vector2(Screen.width * 0.5f, Screen.height * 0.3f);
        pos.x = Mathf.Clamp(pos.x, 160, Screen.width - 160);
        pos.y = Mathf.Clamp(pos.y, 80, Screen.height * (1f - 300f / 1080f)); // abaixo do HUD e da faixa
        _setaTexto.transform.position = pos + Vector2.down * 44f;
        _setaTexto.color = _corDestino;
        _setaTexto.text = $"{(_gm.Fase == Fase.AteHub ? Textos.Console : Tarefas.Def(_gm.Missao.Hoje.Tarefa).Nome)}  {alvo.DistanciaAte(_gm.jogador.transform.position):0} m";
    }

    // ---------------- Minigame ----------------

    /// <summary>Abre o minijogo no painel lateral. O resultado passa por <paramref name="aoResultado"/> (que devolve o efeito, para a faixa) e o jogo segue em <paramref name="aoFechar"/>.</summary>
    public void AbrirMinigame(Minigame m, Func<ResultadoMinijogo, Txt> aoResultado, Action aoFechar)
    {
        _minigameAtual = m;
        _painelMinigame.SetActive(true);
        _miniFaixa.gameObject.SetActive(false);
        TextosDoMinigame();
        PosicionarMinigame();
        _miniProgVis = _miniProgAlvo = 0f;
        m.Abrir(this, _miniArea, aoResultado, aoFechar);
        Entrada.Selecionar(m.PrimeiroBotao != null ? m.PrimeiroBotao.gameObject : null);
    }

    void TextosDoMinigame()
    {
        var m = _minigameAtual;
        if (m == null) return;
        _miniTitulo.text = m.titulo.ToString();
        _miniTitulo.font = UiTema.Fonte(Papel.TituloPainel, _miniTitulo.text);
        _miniInstrucao.text = m.instrucao.ToString();
        _miniInstrucao.font = UiTema.Fonte(Papel.Rotulo, _miniInstrucao.text);
        _miniIcone.sprite = UiTema.Icone(UiTema.IconeDaTarefa(m.tarefa));
        _miniIcone.enabled = _miniIcone.sprite != null;
        var cor = UiTema.CorDaTarefa(m.tarefa);
        _miniTira.color = cor; _miniTira.Redesenhar();
        _miniProgForma.color = cor;
        _btnMiniSair.Texto(Textos.Sair.ToString());
    }

    public void FecharMinigame()
    {
        _minigameAtual = null;
        _miniCursor.gameObject.SetActive(false);
        _miniFaixa.gameObject.SetActive(false);
    }

    /// <summary>Minijogo aberto agora (para a sonda).</summary>
    public Minigame MinigameAtual => _minigameAtual;

    public void LimparArea()
    {
        foreach (Transform filho in _miniArea) { filho.gameObject.SetActive(false); UnityEngine.Object.Destroy(filho.gameObject); }
    }

    /// <summary>Relógio do minijogo: segundos e barra que esvazia. Sempre na mesma cor, sem pulso e sem som.</summary>
    public void Tempo(float fracao, float segundos)
    {
        _miniTempoBarra.anchorMax = new Vector2(Mathf.Clamp01(fracao), 1f);
        _miniTempo.text = Mathf.CeilToInt(segundos) + " s";
        _miniTempo.color = UiTema.Branco;
    }

    /// <summary>Progresso do objetivo (0 a 1). A barra anda até ele de forma contínua; <paramref name="saltar"/> põe direto.</summary>
    public void Progresso(float valor, bool saltar)
    {
        _miniProgAlvo = Mathf.Clamp01(valor);
        if (saltar) _miniProgVis = _miniProgAlvo;
    }

    /// <summary>Faixa de resultado no painel: "Ótimo!" e "Bom!" em pintura branca; "Não deu desta vez" no azul-Terra calmo.</summary>
    public void FaixaResultado(FaixaNota faixa, float nota, Txt efeito)
    {
        bool naoDeu = faixa == FaixaNota.NaoDeu;
        var corTexto = naoDeu ? UiTema.Branco : UiTema.Tinta;
        _miniFaixaFace.color = naoDeu ? UiTema.Terra : UiTema.Branco;
        _miniFaixaTitulo.text = (faixa == FaixaNota.Otimo ? Textos.Otimo : faixa == FaixaNota.Bom ? Textos.Bom : Textos.NaoDeu).ToString();
        _miniFaixaTitulo.color = corTexto;
        string detalhe = Textos.NotaCurta.F(Mathf.RoundToInt(nota * 100f)).ToString();
        if (!efeito.Vazio) detalhe += "  ·  " + efeito;
        _miniFaixaTexto.text = detalhe;
        _miniFaixaTexto.font = UiTema.Fonte(Papel.Corpo, detalhe);
        _miniFaixaTexto.color = corTexto;
        _miniFaixaIcone.sprite = UiTema.Icone(faixa == FaixaNota.Otimo ? "estrela" : faixa == FaixaNota.Bom ? "check_tinta" : "terra");
        _miniFaixaIcone.enabled = _miniFaixaIcone.sprite != null;

        float w = _miniFaixa.sizeDelta.x, xTexto = UiTema.E32 + 72f + UiTema.E24, wt = w - xTexto - UiTema.E32;
        Colocar(_miniFaixaTitulo.rectTransform, xTexto, UiTema.E24, wt, 70);
        Colocar(_miniFaixaTexto.rectTransform, xTexto, UiTema.E24 + 70f, wt, 10);
        float ht = Mathf.Ceil(_miniFaixaTexto.preferredHeight) + 2f;
        _miniFaixaTexto.rectTransform.sizeDelta = new Vector2(wt, ht);
        _miniFaixa.sizeDelta = new Vector2(w, Mathf.Max(170f, UiTema.E24 * 2f + 70f + ht));
        _miniFaixa.gameObject.SetActive(true);
        _miniFaixa.SetAsLastSibling();
        _miniFaixaInicio = Time.unscaledTime;
        AnimarMinigame();
    }

    public void Cursor(bool visivel, Vector2 posTela)
    {
        _miniCursor.gameObject.SetActive(visivel);
        if (visivel) _miniCursor.transform.position = posTela;
    }

    public void MostrarPausa(bool mostrar)
    {
        TextosFixos();
        _pausaConfirma.SetActive(false);
        _pausaOpcoes.SetActive(true);
        _pausa.SetActive(mostrar);
        _pausa.transform.SetAsLastSibling();
        Entrada.Selecionar(mostrar ? _btnContinuar.gameObject : (_primeiro != null && _primeiro ? _primeiro.gameObject : null));
    }

    // =====================================================================
    // Helpers de UI (tudo sai de UiTema, FormaUI e BotaoFisico)
    // =====================================================================

    internal static RectTransform RT(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return (RectTransform)go.transform;
    }

    internal static void Tam(Component c, float largura, float altura)
    {
        var le = c.GetComponent<LayoutElement>();
        if (le == null) le = c.gameObject.AddComponent<LayoutElement>(); // sem "??": o Unity devolve um "null falso"
        if (largura >= 0) { le.preferredWidth = largura; le.minWidth = largura; le.flexibleWidth = 0; }
        if (altura >= 0) { le.preferredHeight = altura; le.minHeight = altura; }
    }

    static void Limpar(RectTransform rt)
    {
        foreach (Transform filho in rt) { filho.gameObject.SetActive(false); UnityEngine.Object.Destroy(filho.gameObject); }
    }

    static bool TemAtivos(RectTransform rt)
    {
        foreach (Transform filho in rt) if (filho.gameObject.activeSelf) return true;
        return false;
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Fundo chanfrado opaco com sombra dura (6 abaixo, sem desfoque) por trás de um painel. As formas ignoram o layout
    /// do pai, então servem a qualquer grupo de layout. Devolve a face.
    /// </summary>
    internal static FormaUI Fundo(RectTransform rt, Color cor, float chanfro = UiTema.ChanfroPainel, string cantos = "SE ID", bool sombra = true)
    {
        if (sombra)
        {
            var s = FormaUI.Chanfrado(rt, UiTema.Sombra, chanfro, cantos, "Sombra");
            s.rectTransform.offsetMin = new Vector2(0, -UiTema.Sombra_Y); s.rectTransform.offsetMax = new Vector2(0, -UiTema.Sombra_Y);
            s.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            s.transform.SetAsFirstSibling();
        }
        var face = FormaUI.Chanfrado(rt, cor, chanfro, cantos, "Fundo");
        face.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        face.transform.SetSiblingIndex(sombra ? 1 : 0);
        return face;
    }

    internal RectTransform Vertical(Transform pai, float espaco, int pad, Color? fundo = null, TextAnchor alin = TextAnchor.UpperLeft)
    {
        var rt = RT("V", pai);
        if (fundo.HasValue) Fundo(rt, fundo.Value, UiTema.ChanfroBotao, "SE ID", false);
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = espaco; v.padding = new RectOffset(pad, pad, pad, pad);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        v.childAlignment = alin;
        return rt;
    }

    internal RectTransform Horizontal(Transform pai, float espaco, bool expandir, int pad = 0, Color? fundo = null, TextAnchor alin = TextAnchor.MiddleLeft)
    {
        var rt = RT("H", pai);
        if (fundo.HasValue) Fundo(rt, fundo.Value, UiTema.ChanfroBotao, "SE ID", false);
        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = espaco; h.padding = new RectOffset(pad, pad, pad, pad);
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = expandir; h.childForceExpandHeight = false;
        h.childAlignment = alin;
        return rt;
    }

    /// <summary>Texto de um dos cinco papéis do tema (fonte, tamanho e cor padrão vêm do papel).</summary>
    internal Text Rot(Transform pai, string s, Papel papel, Color? cor = null, TextAnchor alin = TextAnchor.UpperLeft, bool caixa = false)
    {
        var rt = RT("T", pai);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = UiTema.Fonte(papel, s); t.fontSize = UiTema.Tamanho(papel); t.fontStyle = FontStyle.Normal;
        t.text = caixa ? UiTema.Caixa(s) : s;
        t.color = cor ?? (papel == Papel.Rotulo ? UiTema.Apoio : UiTema.Branco);
        t.alignment = alin;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 1.05f;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>Rótulo de seção: caixa alta, apoio, como nos mocapes da direção.</summary>
    void Secao(string s) => Rot(_conteudo, s, Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);

    /// <summary>Ícone do tema com tamanho fixo no layout. Sem ícone, deixa o espaço vazio.</summary>
    internal Image Icone(Transform pai, string nome, float tam)
    {
        var rt = RT("Icone", pai);
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = UiTema.Icone(nome); im.enabled = im.sprite != null; im.preserveAspect = true; im.raycastTarget = false;
        Tam(rt, tam, tam);
        return im;
    }

    /// <summary>Uma informação por linha: ícone, rótulo curto em caixa alta (opcional) e a frase.</summary>
    RectTransform Linha(Transform pai, string icone, string rotulo, string texto, Papel corpo = Papel.Corpo)
    {
        var h = Horizontal(pai, UiTema.E16, false, 0, null, TextAnchor.UpperLeft);
        Icone(h, icone, 56);
        var v = Vertical(h, 2, 0);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        if (!string.IsNullOrEmpty(rotulo)) Rot(v, rotulo, Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);
        if (!string.IsNullOrEmpty(texto)) Rot(v, texto, corpo, UiTema.Branco);
        return h;
    }

    /// <summary>Aviso de erupção: o ícone vermelho de alerta e o texto em creme (o vermelho nunca vai no texto).</summary>
    void Alerta(Transform pai, string s, bool forte)
    {
        var h = Horizontal(pai, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
        Icone(h, "alerta", 56);
        Rot(h, s, forte ? Papel.TituloPainel : Papel.Corpo, UiTema.Branco, TextAnchor.MiddleLeft);
    }

    /// <summary>Chip: ícone opcional e texto curto, em aço escuro. Usado para custos e números.</summary>
    void Chip(Transform pai, string icone, string texto, Color? fundo = null)
    {
        var h = Horizontal(pai, UiTema.E8, false, 8, null, TextAnchor.MiddleLeft);
        Fundo(h, fundo ?? UiTema.AcoClaro, UiTema.ChanfroCelula + 3f, "SE ID", false);
        if (!string.IsNullOrEmpty(icone)) Icone(h, icone, 40);
        Rot(h, texto, Papel.Corpo, UiTema.Branco, TextAnchor.MiddleLeft);
        Tam(h, -1, 56);
    }

    /// <summary>Um cartão por escolha, com tira na cor do sistema, título, custo em chips (antes do toque) e descrição.</summary>
    BotaoFisico Escolha(Transform pai, Color tira, string titulo, string custo, string desc, UnityAction acao, bool ativo, bool custoEmChips = true)
    {
        var b = BotaoFisico.Criar(pai, BotaoFisico.Estilo.Escolha, "", null, acao, tira, "Escolha");
        var le = b.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1; le.minHeight = le.preferredHeight = custoEmChips ? 230 : 200;
        if (!ativo) b.Desativar("");
        var cont = Vertical(b.Face, UiTema.E8, 0);
        cont.anchorMin = Vector2.zero; cont.anchorMax = Vector2.one;
        cont.offsetMin = new Vector2(UiTema.E32, UiTema.E16); cont.offsetMax = new Vector2(-UiTema.E32, -UiTema.E32);
        Rot(cont, titulo, Papel.TituloPainel, ativo ? UiTema.Branco : UiTema.Discreto);
        if (custoEmChips)
        {
            var chips = Horizontal(cont, UiTema.E12, false);
            foreach (var p in Apresentacao.PartesDoCusto(custo))
                Chip(chips, UiTema.IconeDoRecurso(p.Recurso), p.Texto, UiTema.Aco);
        }
        else Rot(cont, custo, Papel.Rotulo, ativo ? UiTema.Apoio : UiTema.Discreto, TextAnchor.UpperLeft, true);
        Rot(cont, desc, Papel.Corpo, ativo ? UiTema.Apoio : UiTema.Discreto);
        if (!ativo)
        {
            var cad = Icone(b.Face, "cadeado", 48);
            var r = cad.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(1, 1);
            r.anchoredPosition = new Vector2(-UiTema.E24, -UiTema.E32);
            cad.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }
        return b;
    }

    // ---------- Botões ----------

    static void ComTamanho(BotaoFisico b, float largura, float altura)
    {
        var le = b.GetComponent<LayoutElement>();
        if (le == null) le = b.gameObject.AddComponent<LayoutElement>();
        le.minWidth = le.preferredWidth = largura; le.minHeight = le.preferredHeight = altura;
        ((RectTransform)b.transform).sizeDelta = new Vector2(largura, altura);
        b.AjustarPara(largura);
    }

    /// <summary>Botão secundário (aço claro), de 96 de altura por padrão, com tamanho fixo no layout.</summary>
    BotaoFisico Sec(Transform pai, string texto, string icone, UnityAction acao, float largura = 300, float altura = 96)
    {
        var b = BotaoFisico.Criar(pai, BotaoFisico.Estilo.Secundario, texto, icone, acao);
        ComTamanho(b, largura, altura);
        return b;
    }

    /// <summary>A ação que avança: sempre no rodapé fixo do painel, no canto inferior direito, no mesmo tamanho e estilo.</summary>
    BotaoFisico Principal(string texto, UnityAction acao)
    {
        var b = BotaoFisico.Criar(_rodape, BotaoFisico.Estilo.Principal, texto, "seta_tinta", acao);
        ComTamanho(b, 400f, AlturaPrincipal); // o mesmo tamanho em toda fase; texto longo quebra em duas linhas
        _primeiro = b;
        return b;
    }

    /// <summary>Ação secundária no rodapé, à esquerda do botão principal.</summary>
    BotaoFisico Secundario(string texto, string icone, UnityAction acao)
    {
        var b = BotaoFisico.Criar(_rodape, BotaoFisico.Estilo.Secundario, texto, icone, acao);
        ComTamanho(b, 360f, 96f);
        return b;
    }

    static void Ancorar(BotaoFisico b, Vector2 ancora)
    {
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = ancora; rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = Vector2.zero;
    }

    // ---------- Camadas e minijogos (API pública usada pelos minijogos) ----------

    GameObject Painel(string nome, Color cor)
    {
        var go = new GameObject(nome, typeof(Image));
        go.transform.SetParent(_raiz, false);
        Esticar((RectTransform)go.transform);
        go.GetComponent<Image>().color = cor;
        return go;
    }

    Transform Caixa(Transform pai, Vector2 tamanho)
    {
        var rt = RT("Caixa", pai);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamanho;
        Fundo(rt, UiTema.Aco);
        return rt;
    }

    /// <summary>Retângulo centrado (âncora no centro do pai) em posição/tamanho do canvas. Usado pelos minigames.</summary>
    public Image Retangulo(Transform pai, Vector2 pos, Vector2 tam, Color cor)
    {
        var go = new GameObject("Ret", typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;
        var im = go.GetComponent<Image>();
        im.color = cor;
        im.raycastTarget = false;
        return im;
    }

    /// <summary>
    /// Texto com âncora fixa (usado pelos minigames e pelas camadas fixas). Nenhum texto abaixo de 24; de 40 para cima
    /// usa a Barlow (título), abaixo disso a Nunito (corpo). Os minijogos ganham o molde novo no grupo 7.
    /// </summary>
    public Text Texto(Transform pai, string conteudo, int tamanho, Vector2 ancora, Vector2 area)
    {
        var go = new GameObject("Texto", typeof(Text));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = area;
        rt.anchoredPosition = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.font = UiTema.Fonte(tamanho >= 40 ? Papel.TituloPainel : Papel.Corpo, conteudo);
        t.text = conteudo; t.fontSize = Mathf.Max(24, tamanho);
        t.alignment = TextAnchor.MiddleCenter;
        t.color = UiTema.Branco;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
