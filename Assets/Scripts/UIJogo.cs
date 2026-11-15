using System;
using System.Collections.Generic;
using DezDias;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Telas do jogo, montadas em código num Canvas (uGUI). Cada fase redesenha o "tablet da missão" por cima da
/// base 3D (igual ao protótipo web). Trocar o idioma só redesenha: o estado fica no GameManager e na Missao.
/// </summary>
public class UIJogo
{
    // ---------- Cores (web: :root) ----------
    public static readonly Color CorEnergia = Hex("#FFD23F"), CorO2 = Hex("#4DD0E1"), CorRad = Hex("#C77DFF"), CorVerde = Hex("#6BD425"),
        CorLaranja = Hex("#FF9F43"), CorVermelho = Hex("#FF5D5D"), CorPainel = Hex("#0E1326"), CorPainel2 = Hex("#171D38"),
        CorLinha = Hex("#3A4578"), CorSuave = Hex("#CFD5F5"), CorBotaoAlt = Hex("#39405A"), CorEscura = Hex("#10300A");

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    /// <summary>Cor de acento de cada módulo (a mesma do módulo 3D correspondente).</summary>
    public static Color CorDoModulo(Modulo m) => m switch
    {
        Modulo.Ar => CorO2, Modulo.Estufa => CorVerde, Modulo.Abrigo => CorRad,
        Modulo.Lab => CorLaranja, Modulo.Bateria => CorEnergia, _ => CorVermelho,
    };

    public static Color CorDoTom(Tom t) => t switch
    {
        Tom.O2 => CorO2, Tom.Verde => CorVerde, Tom.Rad => CorRad, Tom.Laranja => CorLaranja, Tom.Energia => CorEnergia, _ => CorVermelho,
    };

    readonly GameManager _gm;
    readonly Font _fonte;
    RectTransform _raiz;

    GameObject _hud, _fundoTela, _painelMinigame, _pausa, _faixa;
    RectTransform _tablet, _conteudo;
    ScrollRect _scroll;
    Button _primeiro;

    // HUD
    Text _hudDia, _hudRelogio, _hudEstrelas, _hudIdioma;
    RectTransform _fillO2, _fillRad, _fillBat;
    Text _rotO2, _rotRad, _rotBat;
    // Faixa do deslocamento, botão de ação, seta e aviso
    Text _faixaTexto, _faixaDica;
    Button _btnAcao;
    Text _btnAcaoTexto;
    RectTransform _seta;
    Text _setaTexto;
    Image[] _setaPartes;
    Text _avisoTexto;
    float _avisoAte;
    // Minigame
    Text _miniTitulo, _miniInstrucao, _miniTempo, _miniMsg, _miniSairTexto;
    RectTransform _miniArea, _miniBarra;
    Image _miniCursor, _miniBarraImg;
    Minigame _minigameAtual;
    bool _miniInstrucaoVisivel;
    // Pausa
    Button _btnContinuar;
    Text _pausaTitulo, _pausaContinuar, _pausaReiniciar, _pausaAjuda;
    // O dia acontece
    Text _legenda;

    public UIJogo(GameManager gm)
    {
        _gm = gm;
        _fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_fonte == null) _fonte = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

        var barra = Horizontal(_hud.transform, 18, false, 10, new Color(0.055f, 0.075f, 0.15f, 0.92f));
        barra.anchorMin = new Vector2(0, 1); barra.anchorMax = new Vector2(1, 1); barra.pivot = new Vector2(0.5f, 1);
        barra.sizeDelta = new Vector2(-24, 84); barra.anchoredPosition = new Vector2(0, -10);
        var hl = barra.GetComponent<HorizontalLayoutGroup>();
        hl.childControlHeight = true; hl.childForceExpandHeight = true; hl.childAlignment = TextAnchor.MiddleLeft;

        var pill = Horizontal(barra, 0, false, 0, CorLaranja, TextAnchor.MiddleCenter);
        Tam(pill, 230, -1);
        _hudDia = Rot(pill, "", 30, new Color(0.16f, 0.08f, 0f), FontStyle.Bold, TextAnchor.MiddleCenter);

        _fillO2 = Medidor(barra, CorO2, out _rotO2);
        _fillRad = Medidor(barra, CorRad, out _rotRad);
        _fillBat = Medidor(barra, CorEnergia, out _rotBat);

        _hudEstrelas = Rot(barra, "", 28, CorEnergia, FontStyle.Bold, TextAnchor.MiddleCenter);
        Tam(_hudEstrelas, 210, -1);
        _hudRelogio = Rot(barra, "", 30, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        Tam(_hudRelogio, 170, -1);
        var bi = Btn(barra, "", CorPainel2, () => _gm.AlternarIdioma(), 64, 190, 26);
        _hudIdioma = bi.GetComponentInChildren<Text>();
        bi.navigation = new Navigation { mode = Navigation.Mode.None };
        var bp = Btn(barra, "II", CorPainel2, () => _gm.Pausar(), 64, 90, 30);
        bp.navigation = new Navigation { mode = Navigation.Mode.None };

        // Aviso rápido (tarefa concluída, tempo acabou)
        _avisoTexto = Texto(_hud.transform, "", 46, new Vector2(0.5f, 0.24f), new Vector2(1500, 140));
        _avisoTexto.color = new Color(0.6f, 1f, 0.55f);
        var sombra = _avisoTexto.gameObject.AddComponent<Shadow>();
        sombra.effectDistance = new Vector2(2, -2);
    }

    RectTransform Medidor(Transform pai, Color cor, out Text rotulo)
    {
        var v = Vertical(pai, 6, 0);
        var le = v.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 260; le.flexibleWidth = 1;
        var vl = v.GetComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleLeft; vl.padding = new RectOffset(0, 0, 10, 10);
        rotulo = Rot(v, "", 24, Hex("#DFE4FF"), FontStyle.Bold);
        var fundo = RT("Barra", v);
        fundo.gameObject.AddComponent<Image>().color = Hex("#252C4D");
        Tam(fundo, -1, 16);
        var fill = RT("Preenche", fundo);
        fill.gameObject.AddComponent<Image>().color = cor;
        fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0.5f, 1); fill.offsetMin = fill.offsetMax = Vector2.zero;
        return fill;
    }

    void ConstruirFaixa()
    {
        _faixa = new GameObject("FaixaDeslocamento", typeof(RectTransform));
        _faixa.transform.SetParent(_hud.transform, false);
        var rt = (RectTransform)_faixa.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1); rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -108);
        var img = _faixa.AddComponent<Image>(); img.color = new Color(0.055f, 0.075f, 0.15f, 0.9f); img.raycastTarget = false;
        var v = _faixa.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(26, 26, 14, 14); v.spacing = 6; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
        var f = _faixa.AddComponent<ContentSizeFitter>(); f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rt.sizeDelta = new Vector2(1180, 0);
        _faixaTexto = Rot(rt, "", 30, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        _faixaDica = Rot(rt, "", 24, CorEnergia, FontStyle.Normal, TextAnchor.MiddleCenter);

        // Botão AÇÃO (aparece perto da estação de destino)
        _btnAcao = Botao(_hud.transform, "", new Vector2(0.86f, 0.42f), new Vector2(440, 180), () => { _gm.Interagir(); Entrada.Selecionar(null); });
        Cores(_btnAcao, new Color(0.95f, 0.6f, 0.1f, 1));
        _btnAcao.navigation = new Navigation { mode = Navigation.Mode.None };
        _btnAcaoTexto = _btnAcao.GetComponentInChildren<Text>();
        _btnAcaoTexto.fontSize = 34;
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
        foreach (var im in _setaPartes) { im.color = new Color(1f, 0.9f, 0.2f, 0.95f); im.raycastTarget = false; }
        _setaTexto = Texto(_hud.transform, "", 30, new Vector2(0.5f, 0.5f), new Vector2(520, 50));
        _setaTexto.color = new Color(1f, 0.9f, 0.2f);
        _setaTexto.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
        _seta.gameObject.SetActive(false);
        _setaTexto.gameObject.SetActive(false);
    }

    void ConstruirTablet()
    {
        _fundoTela = new GameObject("Tela", typeof(Image));
        _fundoTela.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_fundoTela.transform);
        _fundoTela.GetComponent<Image>().color = new Color(0.01f, 0.015f, 0.04f, 0.45f);

        _tablet = RT("Tablet", _fundoTela.transform);
        _tablet.anchorMin = _tablet.anchorMax = new Vector2(0.5f, 0.5f);
        _tablet.pivot = new Vector2(0.5f, 0.5f);
        _tablet.anchoredPosition = new Vector2(0, -30);
        var img = _tablet.gameObject.AddComponent<Image>();
        img.color = new Color(CorPainel.r, CorPainel.g, CorPainel.b, 0.97f);
        var ol = _tablet.gameObject.AddComponent<Outline>();
        ol.effectColor = CorLinha; ol.effectDistance = new Vector2(3, -3);

        var viewport = RT("Viewport", _tablet);
        Esticar(viewport);
        viewport.offsetMin = new Vector2(30, 24); viewport.offsetMax = new Vector2(-30, -24);
        viewport.gameObject.AddComponent<RectMask2D>();
        var vi = viewport.gameObject.AddComponent<Image>(); vi.color = new Color(0, 0, 0, 0); // recebe o arrasto da rolagem

        _conteudo = Vertical(viewport, 16, 0);
        _conteudo.anchorMin = new Vector2(0, 1); _conteudo.anchorMax = new Vector2(1, 1); _conteudo.pivot = new Vector2(0.5f, 1);
        _conteudo.offsetMin = _conteudo.offsetMax = Vector2.zero;
        var fit = _conteudo.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _scroll = _tablet.gameObject.AddComponent<ScrollRect>();
        _scroll.viewport = viewport; _scroll.content = _conteudo;
        _scroll.horizontal = false; _scroll.vertical = true;
        _scroll.movementType = ScrollRect.MovementType.Clamped;
        _scroll.scrollSensitivity = 40f;
    }

    void ConstruirMinigame()
    {
        _painelMinigame = Painel("Minigame", new Color(0, 0, 0, 0.75f));
        var cm = Caixa(_painelMinigame.transform, new Vector2(1500, 860));
        _miniTitulo = Texto(cm, "", 58, new Vector2(0.5f, 0.93f), new Vector2(1250, 100));
        _miniInstrucao = Texto(cm, "", 30, new Vector2(0.5f, 0.835f), new Vector2(1300, 90));
        var areaGo = new GameObject("Area", typeof(RectTransform));
        areaGo.transform.SetParent(cm, false);
        _miniArea = (RectTransform)areaGo.transform;
        _miniArea.anchorMin = _miniArea.anchorMax = new Vector2(0.5f, 0.45f);
        _miniArea.sizeDelta = new Vector2(1400, 520);
        _miniArea.anchoredPosition = Vector2.zero;
        var fundoTempo = Retangulo(cm, new Vector2(0, 232), new Vector2(1300, 18), new Color(0, 0, 0, 0.5f));
        var preencheT = Retangulo(fundoTempo.transform, Vector2.zero, new Vector2(1292, 12), new Color(0.4f, 0.85f, 1f));
        _miniBarra = preencheT.rectTransform;
        _miniBarraImg = preencheT;
        _miniBarra.pivot = new Vector2(0, 0.5f);
        _miniBarra.anchoredPosition = new Vector2(-646, 0);
        _miniTempo = Texto(cm, "", 44, new Vector2(0.93f, 0.93f), new Vector2(200, 70));
        _miniMsg = Texto(cm, "", 64, new Vector2(0.5f, 0.5f), new Vector2(1300, 160));
        _miniMsg.color = new Color(1f, 0.9f, 0.3f);
        _miniMsg.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);
        var sair = Botao(cm, "", new Vector2(0.14f, 0.07f), new Vector2(300, 90), () => _gm.CancelarMinijogo());
        _miniSairTexto = sair.GetComponentInChildren<Text>();
        var ajuda = Botao(cm, "?", new Vector2(0.06f, 0.93f), new Vector2(90, 90), () =>
        {
            _miniInstrucaoVisivel = !_miniInstrucaoVisivel;
            _miniInstrucao.gameObject.SetActive(_miniInstrucaoVisivel);
        });
        Cores(ajuda, CorBotaoAlt);
        _miniCursor = Retangulo(_painelMinigame.transform, Vector2.zero, new Vector2(48, 48), new Color(1f, 0.85f, 0.2f, 0.95f));
        _miniCursor.sprite = ControlesMobile.Circulo();
        _miniCursor.gameObject.SetActive(false);
        _painelMinigame.SetActive(false);
    }

    void ConstruirPausa()
    {
        _pausa = Painel("Pausa", new Color(0, 0, 0, 0.8f));
        _pausaTitulo = Texto(_pausa.transform, "", 100, new Vector2(0.5f, 0.68f), new Vector2(900, 150));
        _btnContinuar = Botao(_pausa.transform, "", new Vector2(0.5f, 0.5f), new Vector2(560, 120), () => _gm.Retomar());
        _pausaContinuar = _btnContinuar.GetComponentInChildren<Text>();
        var r = Botao(_pausa.transform, "", new Vector2(0.5f, 0.35f), new Vector2(560, 120), () => _gm.Reiniciar());
        _pausaReiniciar = r.GetComponentInChildren<Text>();
        _pausaAjuda = Texto(_pausa.transform, "", 36, new Vector2(0.5f, 0.2f), new Vector2(1200, 60));
        _pausa.SetActive(false);
    }

    // =====================================================================
    // Redesenho por fase
    // =====================================================================

    static bool FaseDeMissao(Fase f) => f == Fase.Boletim || f == Fase.AteTarefa || f == Fase.Escolha || f == Fase.MinijogoInfo
        || f == Fase.Minijogo || f == Fase.DecisaoVazamento || f == Fase.TarefaConcluida || f == Fase.AteHub
        || f == Fase.Distribuir || f == Fase.DiaAcontece || f == Fase.Resultado;

    static bool FaseSemTablet(Fase f) => f == Fase.AteTarefa || f == Fase.AteHub || f == Fase.Minijogo;

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
        }
        TextosFixos();
        if (!FaseSemTablet(f)) MontarTablet(f);
        if (f == Fase.AteTarefa || f == Fase.AteHub) MontarFaixa();
        if (f == Fase.Minijogo && _minigameAtual != null)
        {
            _miniTitulo.text = _minigameAtual.titulo.ToString();
            _miniInstrucao.text = _minigameAtual.instrucao.ToString();
        }
        AtualizarHud();
    }

    void TextosFixos()
    {
        _pausaTitulo.text = Textos.Pausa.ToString();
        _pausaContinuar.text = Textos.ContinuarB.ToString();
        _pausaReiniciar.text = Textos.Reiniciar.ToString();
        _pausaAjuda.text = Textos.AjudaControle.ToString();
        _miniSairTexto.text = Textos.Sair.ToString();
        _hudIdioma.text = Textos.BotaoIdioma.ToString();
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

    /// <summary>Redesenha a tela atual se a janela mudou de tamanho (tela cheia, giro do celular).</summary>
    void VerificarTamanhoDaTela()
    {
        if (Screen.width == _telaW && Screen.height == _telaH) return;
        bool primeira = _telaW == 0;
        _telaW = Screen.width; _telaH = Screen.height;
        if (!primeira) Render();
    }

    void MontarTablet(Fase f)
    {
        Limpar(_conteudo);
        _primeiro = null;
        _telaW = Screen.width; _telaH = Screen.height;
        var tam = TamanhoReferencia();
        float largura = Mathf.Min(1600f, tam.x - 40f);
        _tablet.sizeDelta = new Vector2(largura, 400f);

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

        // Altura do tablet: a do conteúdo, até o máximo da tela (o resto rola).
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_conteudo);
        float h = LayoutUtility.GetPreferredHeight(_conteudo);
        float max = tam.y - (_hud.activeSelf ? 150f : 60f);
        _tablet.sizeDelta = new Vector2(largura, Mathf.Min(h + 48f, max));
        _tablet.anchoredPosition = new Vector2(0, _hud.activeSelf ? -45f : 0f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_conteudo);
        _scroll.verticalNormalizedPosition = 1f;
        Entrada.Selecionar(_primeiro != null ? _primeiro.gameObject : null);
    }

    // ---------------- Início ----------------

    void TelaInicio()
    {
        Rot(_conteudo, Textos.Titulo.ToString(), 60, Color.white, FontStyle.Bold);
        Rot(_conteudo, Textos.IntroTexto.ToString(), 30, CorSuave);
        foreach (var t in new[] { Textos.IntroItem1, Textos.IntroItem2, Textos.IntroItem3 })
            Rot(_conteudo, "-  " + t, 28, CorSuave);
        Rot(_conteudo, Textos.EscolhaMissao.ToString(), 36, Color.white, FontStyle.Bold);
        var linha = Horizontal(_conteudo, 18, true);
        var c = Escolha(linha, CorVerde, Textos.Cadete.ToString(), Textos.CadeteIdade.ToString(), Textos.CadeteDesc.ToString(), () => _gm.Comecar(Modo.Cadete), true);
        _primeiro = c;
        Escolha(linha, CorLaranja, Textos.Comandante.ToString(), Textos.ComandanteIdade.ToString(), Textos.ComandanteDesc.ToString(), () => _gm.Comecar(Modo.Comandante), true);
        var botoes = Horizontal(_conteudo, 14, false);
        Btn(botoes, Textos.ComoJogar.ToString(), CorBotaoAlt, () => _gm.AbrirComoJogar(), 70, 300, 28);
        Btn(botoes, Textos.TutorialDia1.F(_gm.TutorialLigado ? Textos.Ligado : Textos.Desligado).ToString(), CorBotaoAlt, () => _gm.AlternarTutorial(), 70, 400, 28);
        Btn(botoes, Textos.Creditos.ToString(), CorBotaoAlt, () => _gm.AbrirCreditos(), 70, 330, 28);
        Btn(botoes, Textos.BotaoIdioma.ToString(), CorBotaoAlt, () => _gm.AlternarIdioma(), 70, 220, 28);
        Rot(_conteudo, Textos.Rodape.ToString(), 20, new Color(0.6f, 0.64f, 0.8f), FontStyle.Normal, TextAnchor.MiddleCenter);
    }

    void TelaComoJogar()
    {
        int i = Mathf.Clamp(_gm.Slide, 0, Textos.SlideTitulo.Length - 1);
        Rot(_conteudo, $"{i + 1} / {Textos.SlideTitulo.Length}", 26, CorSuave, FontStyle.Bold, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.SlideTitulo[i].ToString(), 50, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.SlideTexto[i].ToString(), 32, CorSuave, FontStyle.Normal, TextAnchor.MiddleCenter);
        var b = Horizontal(_conteudo, 16, false, 0, null, TextAnchor.MiddleCenter);
        Btn(b, Textos.Voltar.ToString(), CorBotaoAlt, () => _gm.SlideAnterior(), 76, 240, 30).interactable = i > 0;
        bool ultimo = i >= Textos.SlideTitulo.Length - 1;
        _primeiro = Btn(b, (ultimo ? Textos.Comecar : Textos.Proximo).ToString(), CorVerde, () => _gm.SlideProximo(), 76, 280, 32, CorEscura);
        Btn(b, Textos.Pular.ToString(), CorBotaoAlt, () => _gm.PularSlides(), 76, 220, 30);
    }

    // ---------------- Boletim ----------------

    void TelaBoletim()
    {
        var m = _gm.Missao; var T = m.Hoje;
        DicaTutorial(Textos.DicaTutBoletim);
        Rot(_conteudo, Textos.Boletim.ToString(), 50, Color.white, FontStyle.Bold);
        Rot(_conteudo, RegrasMissao.ParteDaMissao(m.Dia).ToString(), 24, CorSuave);
        var luz = (m.EhSombra(m.Dia) ? Textos.LuzSombra : Textos.LuzNormal).F(T.Luz);
        if (m.Bateria > 0) luz = luz + Textos.NaBateria.F(m.Bateria);
        Nota(_conteudo, CorEnergia, Textos.LuzDeHoje.ToString(), luz.ToString());
        var al = m.AlertaDe(m.Dia);
        if (al != null)
        {
            Alerta(_conteudo, (al.Forte ? Textos.AlertaForte : Textos.AlertaFraco).ToString(), al.Forte);
            Rot(_conteudo, (al.Forte ? Textos.AlertaForteTexto : Textos.AlertaFracoTexto).ToString(), 28, CorSuave);
            Rot(_conteudo, Textos.AbrigoCusta.ToString(), 28, CorSuave);
        }
        else Nota(_conteudo, CorVerde, Textos.ClimaEspacial.ToString(), Textos.SolCalmo.ToString());
        if (m.Vazamento) Nota(_conteudo, CorVermelho, Textos.VazamentoAberto.ToString(), Textos.VazamentoAbertoTexto.ToString());
        if (T.Fit != 0) Nota(_conteudo, CorVerde, Textos.Tripulacao.ToString(), (T.Fit > 0 ? Textos.TripulacaoForte : Textos.TripulacaoFraca).ToString());
        if (!m.Contato) Nota(_conteudo, CorO2, Textos.SemContato.ToString(), Textos.SemContatoTexto.ToString());
        var b = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleRight);
        _primeiro = Btn(b, Textos.Continuar.ToString(), CorVerde, () => _gm.SairDoBoletim(), 80, 320, 34, CorEscura);
    }

    // ---------------- Tarefa ----------------

    void TelaEscolha()
    {
        var m = _gm.Missao; var d = Tarefas.Def(m.Hoje.Tarefa);
        if (d.Id == Tarefas.Amostra) DicaTutorial(Textos.DicaTutEscolha);
        Rot(_conteudo, d.Titulo.ToString(), 46, Color.white, FontStyle.Bold);
        Rot(_conteudo, d.Texto.ToString(), 30, CorSuave);
        if (d.Id == Tarefas.Vazamento)
        {
            var bv = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleRight);
            _primeiro = Btn(bv, Tarefas.VazBotao.ToString(), CorVermelho, () => _gm.EscolherFazer(), 84, 420, 34);
            return;
        }
        if (m.AlertaDe(m.Dia) != null && d.Fora) Alerta(_conteudo, Textos.RiscoErupcao.ToString(), false);
        var linha = Horizontal(_conteudo, 18, true);
        bool pode = Tarefas.PodeFazer(m);
        var cor = d.Fora ? Hex("#7DE3F0") : CorDoTarefa(d.Id);
        _primeiro = Escolha(linha, cor, d.FazerTitulo.ToString(), d.FazerCusto.ToString(), (pode ? d.FazerDesc : Textos.OxigenioBaixo).ToString(), () => _gm.EscolherFazer(), pode);
        var ficar = Escolha(linha, d.FicarRuim ? CorVermelho : CorLinha, d.FicarTitulo.ToString(), d.FicarCusto.ToString(), d.FicarDesc.ToString(), () => _gm.EscolherFicar(), true);
        if (!pode) _primeiro = ficar;
        if (!d.Fonte.Vazio) Rot(_conteudo, d.Fonte.ToString(), 22, new Color(0.67f, 0.7f, 0.87f));
    }

    static Color CorDoTarefa(string id)
    {
        switch (id)
        {
            case Tarefas.Estufa: case Tarefas.Exercicio: return CorVerde;
            case Tarefas.Agua: case Tarefas.Filtros: case Tarefas.Upload: case Tarefas.Antena: return CorO2;
            case Tarefas.Poeira: return CorEnergia;
            case Tarefas.Braco: return CorLaranja;
            default: return CorVermelho;
        }
    }

    void TelaMinijogoInfo()
    {
        var d = Tarefas.Def(_gm.Missao.Hoje.Tarefa);
        if (d.Id == Tarefas.Amostra) DicaTutorial(Textos.DicaTutMinijogo);
        Rot(_conteudo, d.MiniTitulo.ToString(), 50, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        Rot(_conteudo, d.MiniInstrucao.ToString(), 32, CorSuave, FontStyle.Normal, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.PrimeiraVez.ToString(), 22, new Color(0.67f, 0.7f, 0.87f), FontStyle.Normal, TextAnchor.MiddleCenter);
        var b = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleCenter);
        _primeiro = Btn(b, Textos.Comecar.ToString(), CorVerde, () => _gm.ComecarMinijogo(), 84, 320, 34, CorEscura);
    }

    void TelaDecisaoVazamento()
    {
        var T = _gm.Missao.Hoje;
        Rot(_conteudo, Tarefas.VazDecisaoTitulo.ToString(), 46, Color.white, FontStyle.Bold);
        Nota(_conteudo, CorO2, T.MsgVazamento.ToString(), null);
        Rot(_conteudo, Tarefas.VazDecisaoTexto.ToString(), 30, CorSuave);
        var linha = Horizontal(_conteudo, 18, true);
        _primeiro = Escolha(linha, CorVerde, Tarefas.VazReforcarTitulo.ToString(), Tarefas.VazReforcarCusto.ToString(), Tarefas.VazReforcarDesc.ToString(), () => _gm.VazamentoReforcar(), true);
        Escolha(linha, CorVermelho, Tarefas.VazDepoisTitulo.ToString(), Tarefas.VazDepoisCusto.ToString(), Tarefas.VazDepoisDesc.ToString(), () => _gm.VazamentoDepois(), true);
    }

    void TelaConcluida()
    {
        var T = _gm.Missao.Hoje; var d = Tarefas.Def(T.Tarefa);
        DicaTutorial(Textos.DicaTutConcluida);
        Rot(_conteudo, d.Titulo.ToString(), 46, Color.white, FontStyle.Bold);
        Nota(_conteudo, CorVerde, T.MsgTarefa.ToString(), null);
        if (!T.Extra.Vazio) Nota(_conteudo, CorO2, Textos.VoceSabia.ToString(), (T.Extra + new Txt("\n", "\n") + T.ExtraFonte).ToString());
        var b = Horizontal(_conteudo, 16, false, 0, null, TextAnchor.MiddleRight);
        if (_gm.Teste)
        {
            _primeiro = Btn(b, Textos.JogarDeNovo.ToString(), CorVerde, () => _gm.TestarDeNovo(), 80, 320, 32, CorEscura);
            Btn(b, Textos.MenuMinijogos.ToString(), CorBotaoAlt, () => _gm.AbrirMenuMinijogos(), 80, 360, 30);
        }
        else _primeiro = Btn(b, Textos.VoltarAoHub.ToString(), CorVerde, () => _gm.VoltarAoHub(), 80, 620, 32, CorEscura);
    }

    // ---------------- Distribuição ----------------

    void TelaDistribuir()
    {
        var m = _gm.Missao; var a = m.Hoje.Aloc;
        int livres = RegrasMissao.CelulasLivres(m);
        DicaTutorial(Textos.DicaTutDistribuir);
        var topo = Horizontal(_conteudo, 20, false);
        var esq = Vertical(topo, 6, 0);
        esq.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Rot(esq, Textos.Distribuir.ToString(), 46, Color.white, FontStyle.Bold);
        Rot(esq, (Textos.DistribuirTexto + (m.Modo == Modo.Cadete ? Textos.SugestaoCadete : Txt.Nada)).ToString(), 26, CorSuave);
        var dir = Vertical(topo, 6, 0);
        Tam(dir, 360, -1);
        Rot(dir, Textos.CelulasLivres.F(livres).ToString(), 30, Color.white, FontStyle.Bold, TextAnchor.MiddleRight);
        var bandeja = Horizontal(dir, 6, false, 0, null, TextAnchor.MiddleRight);
        Celulas(bandeja, livres, 0);
        if (m.Hoje.TempoEsgotou) Nota(_conteudo, CorVermelho, Textos.TempoAcabou.ToString(), m.Hoje.MsgTarefa.ToString());
        var al = m.AlertaDe(m.Dia);
        if (al != null) Alerta(_conteudo, (al.Forte ? Textos.AlertaForteHoje : Textos.AlertaFracoHoje).ToString(), al.Forte);

        var linha = Horizontal(_conteudo, 12, true);
        linha.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = CorDoModulo(mod);
            var card = Vertical(linha, 10, 0, a[mod] > 0 ? Color.Lerp(CorPainel, cor, 0.18f) : CorPainel);
            card.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 12);
            var ol = card.gameObject.AddComponent<Outline>(); ol.effectColor = cor; ol.effectDistance = new Vector2(3, -3);
            card.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var cab = Horizontal(card, 0, true, 8, cor, TextAnchor.MiddleCenter);
            Rot(cab, Textos.NomeModulo[(int)mod].ToString(), 26, CorPainel, FontStyle.Bold, TextAnchor.MiddleCenter);
            var slots = Horizontal(card, 6, false, 0, null, TextAnchor.MiddleCenter);
            Tam(slots, -1, 48);
            Celulas(slots, a[mod], RegrasMissao.Maximo(mod) - a[mod]);
            var pm = Horizontal(card, 12, false, 0, null, TextAnchor.MiddleCenter);
            var Mod = mod;
            var menos = Btn(pm, "-", CorBotaoAlt, () => _gm.Menos(Mod), 64, 84, 40);
            menos.interactable = a[mod] > 0;
            var mais = Btn(pm, "+", CorEnergia, () => _gm.Mais(Mod), 64, 84, 40, new Color(0.23f, 0.16f, 0f));
            mais.interactable = RegrasMissao.PodeMais(m, mod);
            if (_primeiro == null && mais.interactable) _primeiro = mais;
            var hint = Rot(card, RegrasMissao.DicaModulo(m, mod).ToString(), 21, Hex("#DFE4FF"), FontStyle.Normal, TextAnchor.UpperCenter);
            var hl = hint.gameObject.AddComponent<LayoutElement>(); hl.minHeight = 60;
        }

        var dica = Vertical(_conteudo, 8, 18, CorPainel2);
        Rot(dica, Textos.DicaControle.ToString(), 32, Color.white, FontStyle.Bold);
        foreach (var l in RegrasMissao.Previa(m)) Rot(dica, "-  " + l, 26, CorSuave);
        if (livres > 0) Rot(dica, Textos.CelulasVaoSePerder.F(livres).ToString(), 26, new Color(1f, 0.94f, 0.69f), FontStyle.Bold);
        var b = Horizontal(dica, 0, false, 0, null, TextAnchor.MiddleRight);
        var comecar = Btn(b, Textos.ComecarDia.ToString(), CorVerde, () => _gm.ComecarODia(), 80, 380, 34, CorEscura);
        if (_primeiro == null) _primeiro = comecar;
    }

    void Celulas(Transform pai, int ligadas, int vazias)
    {
        for (int i = 0; i < ligadas + vazias; i++)
        {
            var c = RT("Celula", pai);
            var im = c.gameObject.AddComponent<Image>();
            im.color = i < ligadas ? CorEnergia : new Color(CorLinha.r, CorLinha.g, CorLinha.b, 0.9f);
            Tam(c, 28, 40);
        }
    }

    // ---------------- O dia acontece ----------------

    void TelaDiaAcontece()
    {
        var m = _gm.Missao; var a = m.Hoje.Aloc;
        _legenda = Rot(_conteudo, RegrasMissao.Legenda(m, _gm.PassoAnim).ToString(), 40, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        var faixa = Horizontal(_conteudo, 12, true);
        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = CorDoModulo(mod);
            var v = Vertical(faixa, 8, 12, a[mod] > 0 ? Color.Lerp(CorPainel, cor, 0.35f) : CorPainel2);
            v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Rot(v, Textos.NomeModulo[(int)mod].ToString(), 26, a[mod] > 0 ? Color.white : CorSuave, FontStyle.Bold, TextAnchor.MiddleCenter);
            var s = Horizontal(v, 6, false, 0, null, TextAnchor.MiddleCenter);
            Tam(s, -1, 44);
            Celulas(s, a[mod], 0);
        }
        var b = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleRight);
        _primeiro = Btn(b, Textos.Pular.ToString(), CorBotaoAlt, () => _gm.PularAnim(), 72, 240, 30);
    }

    public void AtualizarLegenda()
    {
        if (_legenda != null && _gm.Missao != null) _legenda.text = RegrasMissao.Legenda(_gm.Missao, _gm.PassoAnim).ToString();
    }

    // ---------------- Resultado ----------------

    void TelaResultado()
    {
        var m = _gm.Missao; var r = m.Hoje.Res;
        DicaTutorial(Textos.DicaTutResultado);
        Rot(_conteudo, Textos.FimDoDia.F(m.Dia).ToString(), 48, Color.white, FontStyle.Bold);
        LinhaDelta("O2", Textos.Oxigenio, r.O2Antes, Math.Max(0, m.O2), CorO2);
        LinhaDelta("RAD", Textos.Radiacao, r.RadAntes, m.Rad, CorRad);
        LinhaDelta("BAT", Textos.Bateria, r.BatAntes, m.Bateria, CorEnergia);
        Rot(_conteudo, Textos.PorQue.ToString(), 34, Color.white, FontStyle.Bold);
        foreach (var l in r.Linhas) Nota(_conteudo, CorDoTom(l.Tom), l.Texto.ToString(), null);
        if (r.Erupcao != null) CartaoErupcao(r.Erupcao);
        var b = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleRight);
        _primeiro = Btn(b, (r.Fim != Fim.Nenhum ? Textos.VerRelatorio : Textos.ProximoDia).ToString(), CorVerde, () => _gm.Proximo(), 80, 480, 34, CorEscura);
    }

    void LinhaDelta(string sigla, Txt nome, int antes, int depois, Color cor)
    {
        var h = Horizontal(_conteudo, 16, false, 10, CorPainel2);
        var ic = Horizontal(h, 0, false, 0, cor, TextAnchor.MiddleCenter);
        Tam(ic, 90, 60);
        Rot(ic, sigla, 24, CorPainel, FontStyle.Bold, TextAnchor.MiddleCenter);
        var v = Vertical(h, 2, 0);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Rot(v, nome.ToString(), 28, Color.white, FontStyle.Bold);
        Rot(v, Textos.DeAte.F(antes, depois).ToString(), 24, CorSuave);
        string tag = depois == antes ? Textos.Igual.ToString() : (depois > antes ? "+" : "-") + Math.Abs(depois - antes);
        var t = Horizontal(h, 0, false, 0, depois == antes ? CorBotaoAlt : cor, TextAnchor.MiddleCenter);
        Tam(t, 140, 56);
        Rot(t, tag, 28, depois == antes ? Color.white : CorPainel, FontStyle.Bold, TextAnchor.MiddleCenter);
    }

    void CartaoErupcao(Erupcao e)
    {
        var card = Vertical(_conteudo, 8, 0, CorPainel2);
        var ol = card.gameObject.AddComponent<Outline>(); ol.effectColor = CorEnergia; ol.effectDistance = new Vector2(3, -3);
        var cab = Horizontal(card, 0, true, 10, CorEnergia);
        Rot(cab, Textos.AconteceuDeVerdade.ToString(), 28, new Color(0.23f, 0.16f, 0f), FontStyle.Bold);
        var corpo = Vertical(card, 8, 18);
        Rot(corpo, Textos.ErupcaoClasse.F(e.Classe).ToString(), 36, Color.white, FontStyle.Bold);
        Rot(corpo, Idioma.Data(e.Data).ToString(), 28, CorSuave);
        Rot(corpo, Textos.ValeComo.F(Textos.Intensidade[Mathf.Clamp(e.Intensidade, 0, 2)]).ToString(), 28, CorSuave);
        Rot(corpo, Textos.FonteDonki.ToString(), 22, new Color(0.67f, 0.7f, 0.87f));
        var l = Horizontal(corpo, 14, false);
        Link(l, Textos.VerRegistro.ToString(), e.Url);
        Rot(l, e.Url, 20, new Color(0.67f, 0.7f, 0.87f));
    }

    // ---------------- Relatório ----------------

    void TelaRelatorio()
    {
        var m = _gm.Missao;
        int n = RegrasMissao.Nota(m);
        Rot(_conteudo, RegrasMissao.TituloFim(m).ToString(), 54, Color.white, FontStyle.Bold);
        Rot(_conteudo, RegrasMissao.SubtituloFim(m).ToString(), 30, CorSuave);
        if (m.Fim == Fim.Completa)
        {
            var est = Horizontal(_conteudo, 18, false, 0, null, TextAnchor.MiddleLeft);
            for (int i = 0; i < 3; i++)
            {
                var s = RT("Estrela", est);
                s.gameObject.AddComponent<Image>().color = i < n ? CorEnergia : CorLinha;
                s.localRotation = Quaternion.Euler(0, 0, 45);
                Tam(s, 46, 46);
            }
            Rot(est, Textos.EstrelasDe3.F(n).ToString(), 32, CorEnergia, FontStyle.Bold);
        }
        var chips = Horizontal(_conteudo, 12, false);
        Chip(chips, (m.Modo == Modo.Cadete ? Textos.Cadete : Textos.Comandante).ToString());
        Chip(chips, $"{Textos.Ciencia}: {m.Estrelas}");
        Chip(chips, $"{Textos.Radiacao}: {m.Rad}/{Missao.MaxRad}");
        Chip(chips, $"O2: {Math.Max(0, m.O2)}/{Missao.MaxO2}");
        Chip(chips, $"{Textos.Comida}: {m.Comida}");
        Grafico(m);
        var mom = RegrasMissao.Momentos(m);
        if (mom.Count > 0)
        {
            Rot(_conteudo, Textos.OQueMaisPesou.ToString(), 34, Color.white, FontStyle.Bold);
            foreach (var p in mom) Nota(_conteudo, CorRad, Textos.DiaN.F(p.Dia).ToString(), p.Momento.ToString());
        }
        Nota(_conteudo, CorVerde, Textos.ParaProxima.ToString(), RegrasMissao.DicaFim(m).ToString());
        var b = Horizontal(_conteudo, 16, false);
        _primeiro = Btn(b, Textos.JogarDeNovo.ToString(), CorVerde, () => _gm.JogarDeNovo(), 80, 340, 32, CorEscura);
        Btn(b, Textos.Creditos.ToString(), CorBotaoAlt, () => _gm.AbrirCreditos(), 80, 360, 30);
    }

    void Chip(Transform pai, string s)
    {
        var c = Horizontal(pai, 0, false, 12, CorPainel2, TextAnchor.MiddleCenter);
        Rot(c, s, 26, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
    }

    /// <summary>Gráfico de Oxigênio, Radiação e Bateria por dia (web: chart), desenhado com segmentos de Image.</summary>
    void Grafico(Missao m)
    {
        Rot(_conteudo, Textos.GraficoTitulo.ToString(), 30, Color.white, FontStyle.Bold);
        const float W = 900f, H = 280f, margemX = 40f, margemY = 34f;
        var linhaG = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleCenter);
        var g = RT("Grafico", linhaG);
        g.gameObject.AddComponent<Image>().color = CorPainel2;
        Tam(g, W, H);
        float X(int i) => margemX + (W - margemX - 20f) * i / (Missao.Dias - 1);
        float Y(float v) => margemY + (H - margemY - 16f) * v;
        for (int i = 0; i < Missao.Dias; i++)
        {
            Segmento(g, new Vector2(X(i), margemY), new Vector2(X(i), H - 16f), 2f, CorLinha);
            var t = Texto(g, (i + 1).ToString(), 20, Vector2.zero, new Vector2(40, 26));
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = Vector2.zero;
            t.rectTransform.anchoredPosition = new Vector2(X(i), 14);
            t.color = new Color(0.67f, 0.7f, 0.87f);
        }
        Serie(g, m, p => p.O2 / (float)Missao.MaxO2, CorO2, X, Y);
        Serie(g, m, p => p.Rad / (float)Missao.MaxRad, CorRad, X, Y);
        Serie(g, m, p => p.Bat / (float)Missao.MaxBat, CorEnergia, X, Y);
        var leg = Horizontal(_conteudo, 30, false, 0, null, TextAnchor.MiddleCenter);
        foreach (var (nome, cor) in new[] { (Textos.Oxigenio, CorO2), (Textos.Radiacao, CorRad), (Textos.Bateria, CorEnergia) })
        {
            var item = Horizontal(leg, 8, false, 0, null, TextAnchor.MiddleCenter);
            var q = RT("Cor", item); q.gameObject.AddComponent<Image>().color = cor; Tam(q, 30, 8);
            Rot(item, nome.ToString(), 24, cor, FontStyle.Bold);
        }
    }

    void Serie(RectTransform g, Missao m, Func<RegistroDia, float> f, Color cor, Func<int, float> X, Func<float, float> Y)
    {
        Vector2? ant = null;
        for (int i = 0; i < m.Hist.Count; i++)
        {
            var p = new Vector2(X(i), Y(Mathf.Clamp01(f(m.Hist[i]))));
            if (ant.HasValue) Segmento(g, ant.Value, p, 5f, cor);
            var ponto = RT("Ponto", g);
            ponto.gameObject.AddComponent<Image>().color = cor;
            ponto.anchorMin = ponto.anchorMax = Vector2.zero;
            ponto.sizeDelta = new Vector2(12, 12);
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
        Rot(_conteudo, Textos.Creditos.ToString(), 50, Color.white, FontStyle.Bold);
        Rot(_conteudo, Textos.DadosNasa.ToString(), 34, Color.white, FontStyle.Bold);
        foreach (var (nome, url, uso) in Textos.FontesNasa)
        {
            var n = Vertical(_conteudo, 6, 14, CorPainel2);
            Rot(n, nome, 26, Color.white, FontStyle.Bold);
            Rot(n, uso.ToString(), 24, CorSuave);
            var l = Horizontal(n, 12, false);
            Link(l, Idioma.L("Abrir", "Open"), url);
            Rot(l, url, 20, new Color(0.67f, 0.7f, 0.87f));
        }
        Rot(_conteudo, Textos.NumeroDeJogo.ToString(), 34, Color.white, FontStyle.Bold);
        Rot(_conteudo, Textos.NumeroDeJogoTexto.ToString(), 26, CorSuave);
        Rot(_conteudo, Textos.UsoDeIA.ToString(), 34, Color.white, FontStyle.Bold);
        Rot(_conteudo, Textos.UsoDeIATexto.ToString(), 26, CorSuave);
        var b = Horizontal(_conteudo, 0, false);
        _primeiro = Btn(b, Textos.Voltar.ToString(), CorVerde, () => _gm.FecharCreditos(), 76, 260, 32, CorEscura);
    }

    void Link(Transform pai, string rotulo, string url)
    {
        var b = Btn(pai, rotulo, CorO2, null, 56, 240, 24, CorPainel);
        b.gameObject.AddComponent<BotaoLink>().url = url;
    }

    // ---------------- Menu de minijogos ----------------

    void TelaMenuMinijogos()
    {
        Rot(_conteudo, Textos.TesteMinijogos.ToString(), 50, Color.white, FontStyle.Bold);
        Rot(_conteudo, Textos.TesteMinijogosTexto.ToString(), 28, CorSuave);
        var modos = Horizontal(_conteudo, 14, false);
        Btn(modos, Textos.CadeteMaisTempo.ToString(), _gm.ModoTeste == Modo.Cadete ? CorVerde : CorBotaoAlt, () => _gm.DefinirModoTeste(Modo.Cadete), 72, 380, 28, _gm.ModoTeste == Modo.Cadete ? CorEscura : Color.white);
        Btn(modos, Textos.Comandante.ToString(), _gm.ModoTeste == Modo.Comandante ? CorVerde : CorBotaoAlt, () => _gm.DefinirModoTeste(Modo.Comandante), 72, 300, 28, _gm.ModoTeste == Modo.Comandante ? CorEscura : Color.white);
        var grade = RT("Grade", _conteudo);
        var gl = grade.gameObject.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(350, 96); gl.spacing = new Vector2(14, 14);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 4;
        foreach (var id in Tarefas.Todas)
        {
            var d = Tarefas.Def(id);
            var Id = id;
            var b = Btn(grade, d.MiniTitulo + "\n" + id, CorPainel2, () => _gm.TestarMinijogo(Id), 96, -1, 26);
            var ol = b.gameObject.AddComponent<Outline>(); ol.effectColor = CorLaranja; ol.effectDistance = new Vector2(2, -2);
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
        bool tut = _gm.TutorialAtivo;
        _faixaDica.gameObject.SetActive(tut);
        if (tut) _faixaDica.text = (_gm.Fase == Fase.AteHub ? Textos.DicaTutAteHub : Textos.DicaTutAteTarefa).ToString();
    }

    void DicaTutorial(Txt t)
    {
        if (!_gm.TutorialAtivo) return;
        var n = Horizontal(_conteudo, 14, false, 14, new Color(0.16f, 0.2f, 0.36f, 1f));
        var ol = n.gameObject.AddComponent<Outline>(); ol.effectColor = CorO2; ol.effectDistance = new Vector2(2, -2);
        var v = Vertical(n, 4, 0);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Rot(v, Textos.ControleDaMissao.ToString(), 26, CorO2, FontStyle.Bold);
        Rot(v, t.ToString(), 26, Color.white);
    }

    // =====================================================================
    // Atualização por frame
    // =====================================================================

    public void AtualizarHud()
    {
        VerificarTamanhoDaTela();
        if (_avisoTexto.text != "" && Time.time > _avisoAte) _avisoTexto.text = "";
        if (!_hud.activeSelf || _gm.Missao == null) return;
        var m = _gm.Missao;
        _hudDia.text = Textos.DiaDe.F(Mathf.Max(1, m.Dia), Missao.Dias).ToString();
        Preencher(_fillO2, _rotO2, Textos.Oxigenio, Mathf.Max(0, m.O2), Missao.MaxO2);
        Preencher(_fillRad, _rotRad, Textos.Radiacao, m.Rad, Missao.MaxRad);
        Preencher(_fillBat, _rotBat, Textos.Bateria, m.Bateria, Missao.MaxBat);
        _hudEstrelas.text = $"{Textos.Ciencia}: {m.Estrelas}";
        int t = Mathf.CeilToInt(Mathf.Max(0f, _gm.TempoRestante));
        _hudRelogio.text = $"{t / 60}:{t % 60:00}";
        _hudRelogio.color = _gm.RelogioCorre ? (t <= 30 ? CorVermelho : Color.white) : CorSuave;
    }

    static void Preencher(RectTransform fill, Text rot, Txt nome, int valor, int max)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(valor / (float)max), 1);
        rot.text = $"{nome}  {valor}/{max}";
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
        if (proxima != null) _btnAcaoTexto.text = rotuloAcao.ToString();

        var cam = Camera.main;
        bool mostrar = alvo != null && proxima == null && cam != null && _gm.jogador != null;
        _seta.gameObject.SetActive(mostrar);
        _setaTexto.gameObject.SetActive(mostrar);
        if (!mostrar) return;

        Vector3 origem = cam.WorldToScreenPoint(_gm.jogador.transform.position + Vector3.up * 1.5f);
        Vector3 destino = cam.WorldToScreenPoint(alvo.transform.position + Vector3.up * 1f);
        Vector2 dir = (Vector2)(destino - origem);
        if (destino.z < 0) dir = -dir;
        if (dir.sqrMagnitude < 1f) dir = Vector2.up;
        dir.Normalize();
        float raio = Screen.height * 0.2f;
        Vector2 pos = (Vector2)origem + dir * raio;
        pos.x = Mathf.Clamp(pos.x, 80, Screen.width - 80);
        pos.y = Mathf.Clamp(pos.y, 80, Screen.height - 80);
        _seta.position = pos;
        _seta.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        _setaTexto.transform.position = pos + Vector2.down * 64f;
        var corSeta = new Color(1f, 0.9f, 0.2f, 0.95f);
        foreach (var im in _setaPartes) im.color = corSeta;
        _setaTexto.text = $"{(_gm.Fase == Fase.AteHub ? Textos.Console : Tarefas.Def(_gm.Missao.Hoje.Tarefa).Nome)}  {alvo.DistanciaAte(_gm.jogador.transform.position):0} m";
    }

    // ---------------- Minigame ----------------

    public void AbrirMinigame(Minigame m, bool mostrarInstrucao, Action<ResultadoMinijogo> aoTerminar)
    {
        _minigameAtual = m;
        _miniTitulo.text = m.titulo.ToString();
        _miniInstrucao.text = m.instrucao.ToString();
        _miniInstrucaoVisivel = mostrarInstrucao;
        _miniInstrucao.gameObject.SetActive(mostrarInstrucao);
        _miniMsg.text = "";
        m.Abrir(this, _miniArea, aoTerminar);
        Entrada.Selecionar(m.PrimeiroBotao != null ? m.PrimeiroBotao.gameObject : null);
    }

    public void FecharMinigame() { _minigameAtual = null; _miniCursor.gameObject.SetActive(false); }

    public void LimparArea()
    {
        foreach (Transform filho in _miniArea) UnityEngine.Object.Destroy(filho.gameObject);
        foreach (Transform filho in _miniArea) filho.gameObject.SetActive(false);
    }

    public void Mensagem(string txt) => _miniMsg.text = txt;

    public void Tempo(float fracao, float segundos)
    {
        _miniBarra.sizeDelta = new Vector2(1292f * Mathf.Clamp01(fracao), 12f);
        _miniBarraImg.color = fracao < 0.25f ? new Color(1f, 0.35f, 0.3f) : new Color(0.4f, 0.85f, 1f);
        _miniTempo.text = Mathf.CeilToInt(segundos) + " s";
    }

    public void Cursor(bool visivel, Vector2 posTela)
    {
        _miniCursor.gameObject.SetActive(visivel);
        if (visivel) _miniCursor.transform.position = posTela;
    }

    public void MostrarPausa(bool mostrar)
    {
        TextosFixos();
        _pausa.SetActive(mostrar);
        _pausa.transform.SetAsLastSibling();
        Entrada.Selecionar(mostrar ? _btnContinuar.gameObject : (_primeiro != null && _primeiro ? _primeiro.gameObject : null));
    }

    // =====================================================================
    // Helpers de UI
    // =====================================================================

    static RectTransform RT(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return (RectTransform)go.transform;
    }

    static void Tam(Component c, float largura, float altura)
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

    RectTransform Vertical(Transform pai, float espaco, int pad, Color? fundo = null, TextAnchor alin = TextAnchor.UpperLeft)
    {
        var rt = RT("V", pai);
        if (fundo.HasValue) { var im = rt.gameObject.AddComponent<Image>(); im.color = fundo.Value; }
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = espaco; v.padding = new RectOffset(pad, pad, pad, pad);
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        v.childAlignment = alin;
        return rt;
    }

    RectTransform Horizontal(Transform pai, float espaco, bool expandir, int pad = 0, Color? fundo = null, TextAnchor alin = TextAnchor.MiddleLeft)
    {
        var rt = RT("H", pai);
        if (fundo.HasValue) { var im = rt.gameObject.AddComponent<Image>(); im.color = fundo.Value; }
        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = espaco; h.padding = new RectOffset(pad, pad, pad, pad);
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = expandir; h.childForceExpandHeight = false;
        h.childAlignment = alin;
        return rt;
    }

    Text Rot(Transform pai, string s, int tam, Color? cor = null, FontStyle estilo = FontStyle.Normal, TextAnchor alin = TextAnchor.UpperLeft)
    {
        var rt = RT("T", pai);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = _fonte; t.text = s; t.fontSize = tam; t.fontStyle = estilo;
        t.color = cor ?? Color.white; t.alignment = alin;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 1.05f;
        t.raycastTarget = false;
        return t;
    }

    Button Btn(Transform pai, string s, Color fundo, UnityAction acao, float altura, float largura, int tam, Color? corTexto = null)
    {
        var rt = RT("Botao", pai);
        rt.gameObject.AddComponent<Image>();
        var btn = rt.gameObject.AddComponent<Button>();
        if (acao != null) btn.onClick.AddListener(acao);
        Cores(btn, fundo);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.minHeight = altura; le.preferredHeight = altura;
        if (largura > 0) { le.preferredWidth = largura; le.minWidth = largura; }
        var t = Rot(rt, s, tam, corTexto ?? Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = new Vector2(10, 4); trt.offsetMax = new Vector2(-10, -4);
        return btn;
    }

    Button Escolha(Transform pai, Color borda, string titulo, string custo, string desc, UnityAction acao, bool ativo)
    {
        var rt = RT("Escolha", pai);
        rt.gameObject.AddComponent<Image>();
        var btn = rt.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(acao);
        btn.interactable = ativo;
        Cores(btn, CorPainel2);
        var ol = rt.gameObject.AddComponent<Outline>(); ol.effectColor = borda; ol.effectDistance = new Vector2(3, -3);
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(20, 20, 16, 16); v.spacing = 6;
        v.childControlWidth = v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
        var le = rt.gameObject.AddComponent<LayoutElement>(); le.flexibleWidth = 1; le.minHeight = 150;
        Rot(rt, titulo, 32, Color.white, FontStyle.Bold);
        Rot(rt, custo, 26, borda, FontStyle.Bold);
        Rot(rt, desc, 24, CorSuave);
        return btn;
    }

    void Nota(Transform pai, Color cor, string titulo, string texto)
    {
        var h = Horizontal(pai, 14, false, 0, CorPainel2);
        h.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = true;
        var barra = RT("Barra", h); barra.gameObject.AddComponent<Image>().color = cor; Tam(barra, 8, -1);
        var v = Vertical(h, 4, 12);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Rot(v, titulo, 26, Color.white, FontStyle.Bold);
        if (!string.IsNullOrEmpty(texto)) Rot(v, texto, 26, CorSuave);
    }

    void Alerta(Transform pai, string s, bool forte)
    {
        var h = Horizontal(pai, 0, true, 14, forte ? CorVermelho : Hex("#B8742A"));
        Rot(h, "!  " + s, 30, Color.white, FontStyle.Bold);
    }

    GameObject Painel(string nome, Color cor)
    {
        var go = new GameObject(nome, typeof(Image));
        go.transform.SetParent(_raiz, false);
        Esticar((RectTransform)go.transform);
        go.GetComponent<Image>().color = cor;
        return go;
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    Transform Caixa(Transform pai, Vector2 tamanho)
    {
        var go = new GameObject("Caixa", typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamanho;
        go.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.97f);
        return go.transform;
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

    /// <summary>Texto com âncora fixa (usado pelos minigames e pelas camadas fixas).</summary>
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
        t.font = _fonte; t.text = conteudo; t.fontSize = tamanho;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public Button Botao(Transform pai, string rotulo, Vector2 ancora, Vector2 tamanho, UnityAction acao)
    {
        var go = new GameObject("Botao_" + rotulo, typeof(Image), typeof(Button));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tamanho;
        rt.anchoredPosition = Vector2.zero;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(acao);
        Cores(btn, new Color(0.2f, 0.45f, 0.85f, 1));
        Texto(go.transform, rotulo, 44, new Vector2(0.5f, 0.5f), tamanho);
        return btn;
    }

    /// <summary>Define as cores do botão, com destaque claro quando selecionado pelo controle.</summary>
    static void Cores(Button btn, Color normal)
    {
        btn.targetGraphic.color = Color.white;
        var c = btn.colors;
        c.normalColor = normal;
        c.highlightedColor = Color.Lerp(normal, Color.white, 0.25f);
        c.selectedColor = Color.Lerp(normal, new Color(1f, 0.85f, 0.2f, 1f), 0.4f); // puxa para o amarelo sem apagar o texto
        c.pressedColor = Color.Lerp(normal, Color.black, 0.3f);
        c.disabledColor = new Color(normal.r, normal.g, normal.b, 0.35f);
        c.colorMultiplier = 1f;
        c.fadeDuration = 0.08f;
        btn.colors = c;
        // Sem isto o botão nasce branco e esmaece até a cor em 0,1 s: cada redesenho da tela piscaria.
        btn.targetGraphic.CrossFadeColor(btn.interactable ? c.normalColor : c.disabledColor, 0f, true, true);
    }
}
