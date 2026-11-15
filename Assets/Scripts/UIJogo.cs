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
    ScrollRect _scroll;
    Button _primeiro;

    // HUD
    Text _hudDia, _hudRelogio, _hudEstrelas;
    BotaoFisico _btnIdioma;
    RectTransform _fillO2, _fillRad, _fillBat;
    Text _rotO2, _rotRad, _rotBat;
    // Faixa do deslocamento, botão de ação, seta e aviso
    Text _faixaTexto, _faixaDica;
    BotaoFisico _btnAcao;
    RectTransform _seta;
    Text _setaTexto;
    Image[] _setaPartes;
    Text _avisoTexto;
    float _avisoAte;
    // Minigame
    Text _miniTitulo, _miniInstrucao, _miniTempo, _miniMsg;
    BotaoFisico _btnMiniSair;
    RectTransform _miniArea, _miniBarra;
    Image _miniCursor, _miniBarraImg;
    Minigame _minigameAtual;
    bool _miniInstrucaoVisivel;
    // Pausa
    BotaoFisico _btnContinuar, _btnReiniciar;
    Text _pausaTitulo, _pausaAjuda;
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

        // Barra de 108: cabem os botões de 88 com 10 de folga. O HUD completo (medidores por forma) vem no grupo 4.
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

        _fillO2 = Medidor(barra, UiTema.Ar, "oxigenio", out _rotO2);
        _fillRad = Medidor(barra, UiTema.Abrigo, "radiacao", out _rotRad);
        _fillBat = Medidor(barra, UiTema.Energia, "mod_bateria", out _rotBat);

        var est = Horizontal(barra, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
        Tam(est, 190, -1);
        Icone(est, "estrela", 44);
        _hudEstrelas = Rot(est, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);
        var rel = Horizontal(barra, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
        Tam(rel, 150, -1);
        Icone(rel, "relogio", 44);
        _hudRelogio = Rot(rel, "", Papel.Numero, UiTema.Branco, TextAnchor.MiddleLeft);

        _btnIdioma = Sec(barra, "", "globo", () => _gm.AlternarIdioma(), 270, UiTema.AlvoMinimo);
        _btnIdioma.TamanhoMaximo = 30;
        _btnIdioma.navigation = new Navigation { mode = Navigation.Mode.None };
        var bp = Sec(barra, "", "pausa", () => _gm.Pausar(), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
        bp.navigation = new Navigation { mode = Navigation.Mode.None };

        // Aviso rápido (tarefa concluída, tempo acabou), sobre a cena
        _avisoTexto = Texto(_hud.transform, "", 46, new Vector2(0.5f, 0.24f), new Vector2(1500, 140));
        _avisoTexto.color = UiTema.Branco;
        _avisoTexto.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
    }

    RectTransform Medidor(Transform pai, Color cor, string icone, out Text rotulo)
    {
        var h = Horizontal(pai, UiTema.E12, false, 0, null, TextAnchor.MiddleLeft);
        var le = h.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 300; le.flexibleWidth = 1;
        Icone(h, icone, 52);
        var v = Vertical(h, 6, 0, null, TextAnchor.MiddleLeft);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        rotulo = Rot(v, "", Papel.Rotulo, UiTema.Branco);
        var fundo = RT("Barra", v);
        fundo.gameObject.AddComponent<Image>().color = UiTema.Vazio;
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
        rt.anchoredPosition = new Vector2(0, -136);
        var v = _faixa.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(UiTema.E32, UiTema.E32, UiTema.E16, UiTema.E16); v.spacing = UiTema.E4;
        v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
        var f = _faixa.AddComponent<ContentSizeFitter>(); f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rt.sizeDelta = new Vector2(1180, 0);
        Fundo(rt, UiTema.Aco);
        _faixaTexto = Rot(rt, "", Papel.Corpo, UiTema.Branco, TextAnchor.MiddleCenter);
        _faixaDica = Rot(rt, "", Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter);

        // Botão AÇÃO (aparece perto da estação de destino). Vai para o canto do botão principal no grupo 4.
        _btnAcao = BotaoFisico.Criar(_hud.transform, BotaoFisico.Estilo.Principal, "", "seta_tinta", () => { _gm.Interagir(); Entrada.Selecionar(null); }, null, "BotaoAcao");
        var ra = (RectTransform)_btnAcao.transform;
        ra.anchorMin = ra.anchorMax = new Vector2(0.86f, 0.42f); ra.pivot = new Vector2(0.5f, 0.5f);
        ra.sizeDelta = new Vector2(440, 120); ra.anchoredPosition = Vector2.zero;
        _btnAcao.AjustarPara(440);
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
    }

    void ConstruirTablet()
    {
        _fundoTela = new GameObject("Tela", typeof(Image));
        _fundoTela.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_fundoTela.transform);
        var veu = UiTema.Pagina; veu.a = 0.45f; // sai no grupo 5, com o painel lateral
        _fundoTela.GetComponent<Image>().color = veu;

        _tablet = RT("Tablet", _fundoTela.transform);
        _tablet.anchorMin = _tablet.anchorMax = new Vector2(0.5f, 0.5f);
        _tablet.pivot = new Vector2(0.5f, 0.5f);
        _tablet.anchoredPosition = new Vector2(0, -30);
        Fundo(_tablet, UiTema.Aco);

        // Tira de luz: a cor do sistema de que a tela fala, no topo, a partir do canto chanfrado.
        var tiraRt = RT("Tira", _tablet);
        tiraRt.anchorMin = new Vector2(0, 1); tiraRt.anchorMax = new Vector2(1, 1); tiraRt.pivot = new Vector2(0.5f, 1);
        tiraRt.offsetMin = new Vector2(UiTema.ChanfroPainel - 2f, -8f); tiraRt.offsetMax = Vector2.zero;
        _tira = tiraRt.gameObject.AddComponent<FormaUI>();
        _tira.chanfro = 0; _tira.cantoSE = _tira.cantoID = false; _tira.raycastTarget = false;

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
        // fase, e o painel ocupa o espaço acima dele. Ações secundárias entram à esquerda do botão principal.
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

    void ConstruirMinigame()
    {
        var veu = UiTema.Pagina; veu.a = 0.75f;
        _painelMinigame = Painel("Minigame", veu);
        var cm = Caixa(_painelMinigame.transform, new Vector2(1500, 860));
        _miniTitulo = Texto(cm, "", 58, new Vector2(0.5f, 0.93f), new Vector2(1250, 100));
        _miniInstrucao = Texto(cm, "", 30, new Vector2(0.5f, 0.835f), new Vector2(1300, 90));
        _miniInstrucao.color = UiTema.Apoio;
        var areaGo = new GameObject("Area", typeof(RectTransform));
        areaGo.transform.SetParent(cm, false);
        _miniArea = (RectTransform)areaGo.transform;
        _miniArea.anchorMin = _miniArea.anchorMax = new Vector2(0.5f, 0.45f);
        _miniArea.sizeDelta = new Vector2(1400, 520);
        _miniArea.anchoredPosition = Vector2.zero;
        var fundoTempo = Retangulo(cm, new Vector2(0, 232), new Vector2(1300, 18), UiTema.Vazio);
        var preencheT = Retangulo(fundoTempo.transform, Vector2.zero, new Vector2(1292, 12), UiTema.Branco);
        _miniBarra = preencheT.rectTransform;
        _miniBarraImg = preencheT;
        _miniBarra.pivot = new Vector2(0, 0.5f);
        _miniBarra.anchoredPosition = new Vector2(-646, 0);
        _miniTempo = Texto(cm, "", 44, new Vector2(0.93f, 0.93f), new Vector2(200, 70));
        _miniMsg = Texto(cm, "", 64, new Vector2(0.5f, 0.5f), new Vector2(1300, 160));
        _miniMsg.color = UiTema.Branco;
        _miniMsg.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);
        _btnMiniSair = Sec(cm, "", null, () => _gm.CancelarMinijogo(), 300, UiTema.AlvoMinimo);
        Ancorar(_btnMiniSair, new Vector2(0.14f, 0.07f));
        var ajuda = Sec(cm, "?", null, () =>
        {
            _miniInstrucaoVisivel = !_miniInstrucaoVisivel;
            _miniInstrucao.gameObject.SetActive(_miniInstrucaoVisivel);
        }, UiTema.AlvoMinimo, UiTema.AlvoMinimo);
        ajuda.Rotulo.alignment = TextAnchor.MiddleCenter;
        Ancorar(ajuda, new Vector2(0.06f, 0.93f));
        _miniCursor = Retangulo(_painelMinigame.transform, Vector2.zero, new Vector2(48, 48), UiTema.Energia);
        _miniCursor.sprite = ControlesMobile.Circulo();
        _miniCursor.gameObject.SetActive(false);
        _painelMinigame.SetActive(false);
    }

    void ConstruirPausa()
    {
        var veu = UiTema.Pagina; veu.a = 0.8f;
        _pausa = Painel("Pausa", veu);
        var cartao = RT("Cartao", _pausa.transform);
        cartao.anchorMin = cartao.anchorMax = cartao.pivot = new Vector2(0.5f, 0.5f);
        cartao.sizeDelta = new Vector2(760, 700);
        Fundo(cartao, UiTema.Aco);
        _pausaTitulo = Texto(cartao, "", 60, new Vector2(0.5f, 0.86f), new Vector2(700, 100));
        _btnContinuar = BotaoFisico.Criar(cartao, BotaoFisico.Estilo.Principal, "", "seta_tinta", () => _gm.Retomar(), null, "Continuar");
        ComTamanho(_btnContinuar, 600, 120); Ancorar(_btnContinuar, new Vector2(0.5f, 0.62f));
        _btnReiniciar = Sec(cartao, "", null, () => _gm.Reiniciar(), 600, UiTema.AlvoMinimo + 8);
        Ancorar(_btnReiniciar, new Vector2(0.5f, 0.4f));
        _pausaAjuda = Texto(cartao, "", 24, new Vector2(0.5f, 0.17f), new Vector2(680, 120));
        _pausaAjuda.color = UiTema.Apoio;
        _pausa.SetActive(false);
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
                return _gm.Missao != null && !_gm.Teste ? CorDoTarefa(_gm.Missao.Hoje.Tarefa) : UiTema.Lab;
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
        _btnContinuar.Texto(Textos.ContinuarB.ToString());
        _btnReiniciar.Texto(Textos.Reiniciar.ToString());
        _pausaAjuda.text = Textos.AjudaControle.ToString();
        _btnMiniSair.Texto(Textos.Sair.ToString());
        _btnIdioma.Texto(Textos.BotaoIdioma.ToString());
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

    void MontarTablet(Fase f)
    {
        Limpar(_conteudo);
        Limpar(_rodape);
        _primeiro = null;
        _telaW = Screen.width; _telaH = Screen.height;
        _tira.color = TiraDaFase(f);
        _tira.Redesenhar();
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

        // O painel ocupa a faixa livre entre o HUD e o rodapé (se a tela tem ação), centrado nela.
        bool temRodape = TemAtivos(_rodape); // childCount ainda conta os filhos destruídos (Destroy é adiado)
        _rodape.gameObject.SetActive(temRodape);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_conteudo);
        if (temRodape) LayoutRebuilder.ForceRebuildLayoutImmediate(_rodape);
        float h = LayoutUtility.GetPreferredHeight(_conteudo);
        float topo = _hud.activeSelf ? 128f : 30f;
        float reservado = temRodape ? MargemRodapeBaixo + AlturaPrincipal + UiTema.E24 : 30f;
        float livre = tam.y - topo - reservado;
        _tablet.sizeDelta = new Vector2(largura, Mathf.Min(h + 56f, livre));
        _tablet.anchoredPosition = new Vector2(0, tam.y * 0.5f - (topo + livre * 0.5f));
        LayoutRebuilder.ForceRebuildLayoutImmediate(_conteudo);
        // Redesenhar a mesma fase (clicar em + ou −, trocar o idioma) mantém a rolagem; mudar de fase volta ao topo.
        if (f != _fasePintada) _scroll.verticalNormalizedPosition = 1f;
        _fasePintada = f;
        Entrada.Selecionar(_primeiro != null ? _primeiro.gameObject : null);
    }

    // ---------------- Início ----------------

    void TelaInicio()
    {
        Rot(_conteudo, Textos.Titulo.ToString(), Papel.TituloTela);
        Rot(_conteudo, Textos.IntroTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        foreach (var t in new[] { Textos.IntroItem1, Textos.IntroItem2, Textos.IntroItem3 })
            Rot(_conteudo, "·  " + t, Papel.Corpo, UiTema.Apoio);
        Secao(Textos.EscolhaMissao.ToString());
        var linha = Horizontal(_conteudo, UiTema.E24, true);
        var c = Escolha(linha, UiTema.Estufa, Textos.Cadete.ToString(), Textos.CadeteIdade.ToString(), Textos.CadeteDesc.ToString(), () => _gm.Comecar(Modo.Cadete), true, false);
        _primeiro = c;
        Escolha(linha, UiTema.Lab, Textos.Comandante.ToString(), Textos.ComandanteIdade.ToString(), Textos.ComandanteDesc.ToString(), () => _gm.Comecar(Modo.Comandante), true, false);
        var botoes = Horizontal(_conteudo, UiTema.E16, false);
        Sec(botoes, Textos.ComoJogar.ToString(), "ajuda", () => _gm.AbrirComoJogar(), 300);
        Sec(botoes, Textos.TutorialDia1.F(_gm.TutorialLigado ? Textos.Ligado : Textos.Desligado).ToString(), _gm.TutorialLigado ? "check" : null, () => _gm.AlternarTutorial(), 480);
        Sec(botoes, Textos.Creditos.ToString(), "fonte", () => _gm.AbrirCreditos(), 360);
        Sec(botoes, Textos.BotaoIdioma.ToString(), "globo", () => _gm.AlternarIdioma(), 280);
        Rot(_conteudo, Textos.Rodape.ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter);
    }

    void TelaComoJogar()
    {
        int i = Mathf.Clamp(_gm.Slide, 0, Textos.SlideTitulo.Length - 1);
        Rot(_conteudo, $"{i + 1} / {Textos.SlideTitulo.Length}", Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter, true);
        Rot(_conteudo, Textos.SlideTitulo[i].ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.SlideTexto[i].ToString(), Papel.Corpo, UiTema.Apoio, TextAnchor.MiddleCenter);
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
        var linha = Horizontal(_conteudo, UiTema.E24, true);
        bool pode = Tarefas.PodeFazer(m);
        var cor = CorDoTarefa(d.Id);
        _primeiro = Escolha(linha, cor, d.FazerTitulo.ToString(), d.FazerCusto.ToString(), (pode ? d.FazerDesc : Textos.OxigenioBaixo).ToString(), () => _gm.EscolherFazer(), pode);
        // "Ficar" é do mesmo nível que "fazer", sempre sem vermelho: a tira neutra diz que é a escolha calma.
        var ficar = Escolha(linha, d.FicarRuim ? UiTema.Apoio : UiTema.Branco, d.FicarTitulo.ToString(), d.FicarCusto.ToString(), d.FicarDesc.ToString(), () => _gm.EscolherFicar(), true);
        if (!pode) _primeiro = ficar;
        if (!d.Fonte.Vazio) Rot(_conteudo, d.Fonte.ToString(), Papel.Rotulo, UiTema.Apoio);
    }

    static Color CorDoTarefa(string id)
    {
        switch (id)
        {
            case Tarefas.Estufa: case Tarefas.Exercicio: return UiTema.Estufa;
            case Tarefas.Agua: case Tarefas.Filtros: case Tarefas.Upload: case Tarefas.Antena: case Tarefas.Vazamento: return UiTema.Ar;
            case Tarefas.Poeira: return UiTema.Energia;
            default: return UiTema.Lab;
        }
    }

    void TelaMinijogoInfo()
    {
        var d = Tarefas.Def(_gm.Missao.Hoje.Tarefa);
        if (d.Id == Tarefas.Amostra) DicaTutorial(Textos.DicaTutMinijogo);
        Rot(_conteudo, d.MiniTitulo.ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleCenter);
        Rot(_conteudo, d.MiniInstrucao.ToString(), Papel.Corpo, UiTema.Apoio, TextAnchor.MiddleCenter);
        Rot(_conteudo, Textos.PrimeiraVez.ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.MiddleCenter);
        Principal(Textos.Comecar.ToString(), () => _gm.ComecarMinijogo());
    }

    void TelaDecisaoVazamento()
    {
        var T = _gm.Missao.Hoje;
        Rot(_conteudo, Tarefas.VazDecisaoTitulo.ToString(), Papel.TituloPainel);
        Linha(_conteudo, "tarefa_remendo", null, T.MsgVazamento.ToString());
        Rot(_conteudo, Tarefas.VazDecisaoTexto.ToString(), Papel.Corpo, UiTema.Apoio);
        var linha = Horizontal(_conteudo, UiTema.E24, true);
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
        DicaTutorial(Textos.DicaTutDistribuir);
        var topo = Horizontal(_conteudo, UiTema.E24, false);
        var esq = Vertical(topo, 6, 0);
        esq.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Rot(esq, Textos.Distribuir.ToString(), Papel.TituloPainel);
        Rot(esq, (Textos.DistribuirTexto + (m.Modo == Modo.Cadete ? Textos.SugestaoCadete : Txt.Nada)).ToString(), Papel.Corpo, UiTema.Apoio);
        var dir = Vertical(topo, 6, 0);
        Tam(dir, 380, -1);
        Rot(dir, Textos.CelulasLivres.F(livres).ToString(), Papel.Corpo, UiTema.Branco, TextAnchor.MiddleRight);
        var bandeja = Horizontal(dir, UiTema.E8, false, 0, null, TextAnchor.MiddleRight);
        Celulas(bandeja, livres, 0);
        if (m.Hoje.TempoEsgotou) Linha(_conteudo, "relogio", Textos.TempoAcabou.ToString(), m.Hoje.MsgTarefa.ToString());
        var al = m.AlertaDe(m.Dia);
        if (al != null) Alerta(_conteudo, (al.Forte ? Textos.AlertaForteHoje : Textos.AlertaFracoHoje).ToString(), al.Forte);

        var linha = Horizontal(_conteudo, UiTema.E12, true);
        linha.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = UiTema.CorDoModulo(mod);
            var card = Vertical(linha, UiTema.E8, 14, null, TextAnchor.UpperCenter);
            Fundo(card, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
            card.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var tira = RT("Tira", card); tira.gameObject.AddComponent<Image>().color = cor; Tam(tira, -1, 8);
            var cab = Horizontal(card, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
            Icone(cab, UiTema.IconeDoModulo(mod), 48);
            // Rótulo (Barlow condensada, 24): com 6 cartões o nome "Laboratório" em corpo ou título não cabe numa linha.
            Rot(cab, Textos.NomeModulo[(int)mod].ToString(), Papel.Rotulo, UiTema.Branco, TextAnchor.MiddleLeft);
            var slots = Horizontal(card, 6, false, 0, null, TextAnchor.MiddleCenter);
            Tam(slots, -1, 44);
            Celulas(slots, a[mod], RegrasMissao.Maximo(mod) - a[mod]);
            var pm = Horizontal(card, UiTema.E12, false, 0, null, TextAnchor.MiddleCenter);
            var Mod = mod;
            var menos = Sec(pm, "−", null, () => _gm.Menos(Mod), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
            menos.SemCadeado = true;
            menos.Rotulo.alignment = TextAnchor.MiddleCenter;
            menos.interactable = a[mod] > 0;
            var mais = Sec(pm, "+", null, () => _gm.Mais(Mod), UiTema.AlvoMinimo, UiTema.AlvoMinimo);
            mais.SemCadeado = true;
            mais.Rotulo.alignment = TextAnchor.MiddleCenter;
            mais.interactable = RegrasMissao.PodeMais(m, mod);
            if (_primeiro == null && mais.interactable) _primeiro = mais;
            var hint = Rot(card, RegrasMissao.DicaModulo(m, mod).ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperCenter);
            var hl = hint.gameObject.AddComponent<LayoutElement>(); hl.minHeight = 96;
        }

        var dica = Vertical(_conteudo, UiTema.E8, 18, null);
        Fundo(dica, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        Rot(dica, Textos.DicaControle.ToString(), Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);
        foreach (var l in RegrasMissao.Previa(m)) Rot(dica, "·  " + l, Papel.Corpo, UiTema.Branco);
        if (livres > 0) Rot(dica, Textos.CelulasVaoSePerder.F(livres).ToString(), Papel.Corpo, UiTema.Energia);
        var comecar = Principal(Textos.ComecarDia.ToString(), () => _gm.ComecarODia());
        if (_primeiro == null) _primeiro = comecar;
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
        _legenda = Rot(_conteudo, RegrasMissao.Legenda(m, _gm.PassoAnim).ToString(), Papel.TituloPainel, UiTema.Branco, TextAnchor.MiddleCenter);
        var faixa = Horizontal(_conteudo, UiTema.E12, true);
        foreach (var mod in RegrasMissao.ModulosVisiveis(m))
        {
            var cor = UiTema.CorDoModulo(mod);
            bool ligado = a[mod] > 0;
            var v = Vertical(faixa, UiTema.E8, 12, null, TextAnchor.UpperCenter);
            Fundo(v, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
            v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var tira = RT("Tira", v); tira.gameObject.AddComponent<Image>().color = ligado ? cor : UiTema.Vazio; Tam(tira, -1, 8);
            Rot(v, Textos.NomeModulo[(int)mod].ToString(), Papel.Corpo, ligado ? UiTema.Branco : UiTema.Apoio, TextAnchor.MiddleCenter);
            var s = Horizontal(v, 6, false, 0, null, TextAnchor.MiddleCenter);
            Tam(s, -1, 44);
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
        DicaTutorial(Textos.DicaTutResultado);
        Rot(_conteudo, Textos.FimDoDia.F(m.Dia).ToString(), Papel.TituloPainel);
        LinhaDelta("oxigenio", Textos.Oxigenio, r.O2Antes, Math.Max(0, m.O2));
        LinhaDelta("radiacao", Textos.Radiacao, r.RadAntes, m.Rad);
        LinhaDelta("mod_bateria", Textos.Bateria, r.BatAntes, m.Bateria);
        // O cartão do dado real vem antes do "Por quê?": a fonte da NASA fica visível sem rolar (o painel lateral reordena depois).
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
    void LinhaDelta(string icone, Txt nome, int antes, int depois)
    {
        // Uma linha só: ícone, nome, "{antes} para {depois}" e a pílula, para o resultado caber sem rolar.
        var h = Horizontal(_conteudo, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
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
        Tam(p, 150, 56);
        if (delta == 0) { Rot(p, "=", Papel.Numero, UiTema.Branco, TextAnchor.MiddleCenter); return; }
        var tri = FormaUI.Criar(p, FormaUI.Modo.Triangulo, UiTema.Branco, "Sentido");
        tri.rotacao = delta > 0 ? 0f : 180f;
        var le = tri.gameObject.AddComponent<LayoutElement>(); le.minWidth = le.preferredWidth = 26; le.minHeight = le.preferredHeight = 24;
        Rot(p, Math.Abs(delta).ToString(), Papel.Numero, UiTema.Branco, TextAnchor.MiddleCenter);
    }

    /// <summary>
    /// Cartão "Isso aconteceu de verdade": papel branco, ícone de fonte, título fixo, o dado, a etiqueta de arquivado e a
    /// fonte. Compacto, para caber no painel sem rolar.
    /// </summary>
    void CartaoErupcao(Erupcao e)
    {
        var card = Vertical(_conteudo, UiTema.E8, 20, null);
        Fundo(card, UiTema.Branco, UiTema.ChanfroPainel, "SE ID", true);
        // A sombra do papel é o cinza do papel, não o aço.
        card.GetChild(0).GetComponent<FormaUI>().color = UiTema.BrancoSombra;
        var cab = Horizontal(card, UiTema.E12, false, 0, null, TextAnchor.MiddleLeft);
        Icone(cab, "fonte_tinta", 44);
        var titulo = Rot(cab, Textos.AconteceuDeVerdade.ToString(), Papel.TituloPainel, UiTema.Tinta, TextAnchor.MiddleLeft);
        titulo.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        // Etiqueta escura: dado arquivado, nunca "agora" nem "ao vivo".
        var et = Horizontal(cab, 0, false, 12, null, TextAnchor.MiddleCenter);
        Fundo(et, UiTema.Tinta, UiTema.ChanfroCelula, "SE ID", false);
        Rot(et, Textos.EtiquetaArquivado.ToString(), Papel.Rotulo, UiTema.Branco, TextAnchor.MiddleCenter, true);
        Rot(card, $"{Textos.ErupcaoClasse.F(e.Classe)} · {Idioma.Data(e.Data)}", Papel.TituloPainel, UiTema.Tinta);
        Rot(card, Textos.ValeComo.F(Textos.Intensidade[Mathf.Clamp(e.Intensidade, 0, 2)]).ToString(), Papel.Corpo, UiTema.Tinta);
        var rodape = Horizontal(card, UiTema.E16, false, 0, null, TextAnchor.MiddleLeft);
        var fonte = Rot(rodape, Textos.FonteDonki.ToString(), Papel.Rotulo, UiTema.Tinta);
        fonte.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        Link(rodape, Textos.VerRegistro.ToString(), e.Url, 300, UiTema.AlvoMinimo);
    }

    // ---------------- Relatório ----------------

    void TelaRelatorio()
    {
        var m = _gm.Missao;
        int n = RegrasMissao.Nota(m);
        Rot(_conteudo, RegrasMissao.TituloFim(m).ToString(), Papel.TituloTela);
        Rot(_conteudo, RegrasMissao.SubtituloFim(m).ToString(), Papel.Corpo, UiTema.Apoio);
        if (m.Fim == Fim.Completa)
        {
            var est = Horizontal(_conteudo, UiTema.E12, false, 0, null, TextAnchor.MiddleLeft);
            for (int i = 0; i < 3; i++) Icone(est, i < n ? "estrela" : "estrela_apagada", 64);
            Rot(est, Textos.EstrelasDe3.F(n).ToString(), Papel.TituloPainel, UiTema.Branco);
        }
        var chips = Horizontal(_conteudo, UiTema.E12, false);
        Chip(chips, null, (m.Modo == Modo.Cadete ? Textos.Cadete : Textos.Comandante).ToString());
        Chip(chips, "estrela", $"{Textos.Ciencia}: {m.Estrelas}");
        Chip(chips, "radiacao", $"{Textos.Radiacao}: {m.Rad}/{Missao.MaxRad}");
        Chip(chips, "oxigenio", $"O₂: {Math.Max(0, m.O2)}/{Missao.MaxO2}");
        Chip(chips, "comida", $"{Textos.Comida}: {m.Comida}");
        Grafico(m);
        var mom = RegrasMissao.Momentos(m);
        if (mom.Count > 0)
        {
            Secao(Textos.OQueMaisPesou.ToString());
            foreach (var p in mom) Linha(_conteudo, "tarefa_expedicao", Textos.DiaN.F(p.Dia).ToString(), p.Momento.ToString());
        }
        Linha(_conteudo, "mod_estufa", Textos.ParaProxima.ToString(), RegrasMissao.DicaFim(m).ToString());
        Secundario(Textos.Creditos.ToString(), "fonte", () => _gm.AbrirCreditos());
        Principal(Textos.JogarDeNovo.ToString(), () => _gm.JogarDeNovo());
    }

    /// <summary>Gráfico de Oxigênio, Radiação e Bateria por dia (web: chart), desenhado com segmentos de Image.</summary>
    void Grafico(Missao m)
    {
        Secao(Textos.GraficoTitulo.ToString());
        const float W = 900f, H = 280f, margemX = 40f, margemY = 34f;
        var linhaG = Horizontal(_conteudo, 0, false, 0, null, TextAnchor.MiddleCenter);
        var g = RT("Grafico", linhaG);
        g.gameObject.AddComponent<Image>().color = UiTema.Vazio;
        Tam(g, W, H);
        float X(int i) => margemX + (W - margemX - 20f) * i / (Missao.Dias - 1);
        float Y(float v) => margemY + (H - margemY - 16f) * v;
        for (int i = 0; i < Missao.Dias; i++)
        {
            Segmento(g, new Vector2(X(i), margemY), new Vector2(X(i), H - 16f), 2f, UiTema.AcoClaro);
            var t = Texto(g, (i + 1).ToString(), 24, Vector2.zero, new Vector2(40, 26));
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = Vector2.zero;
            t.rectTransform.anchoredPosition = new Vector2(X(i), 14);
            t.color = UiTema.Apoio;
        }
        Serie(g, m, p => p.O2 / (float)Missao.MaxO2, UiTema.Ar, X, Y);
        Serie(g, m, p => p.Rad / (float)Missao.MaxRad, UiTema.Abrigo, X, Y);
        Serie(g, m, p => p.Bat / (float)Missao.MaxBat, UiTema.Energia, X, Y);
        var leg = Horizontal(_conteudo, 30, false, 0, null, TextAnchor.MiddleCenter);
        foreach (var (nome, cor) in new[] { (Textos.Oxigenio, UiTema.Ar), (Textos.Radiacao, UiTema.Abrigo), (Textos.Bateria, UiTema.Energia) })
        {
            var item = Horizontal(leg, UiTema.E8, false, 0, null, TextAnchor.MiddleCenter);
            var q = RT("Cor", item); q.gameObject.AddComponent<Image>().color = cor; Tam(q, 30, 8);
            Rot(item, nome.ToString(), Papel.Corpo, UiTema.Branco);
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
        bool tut = _gm.TutorialAtivo;
        _faixaDica.gameObject.SetActive(tut);
        if (tut) _faixaDica.text = (_gm.Fase == Fase.AteHub ? Textos.DicaTutAteHub : Textos.DicaTutAteTarefa).ToString();
    }

    void DicaTutorial(Txt t)
    {
        if (!_gm.TutorialAtivo) return;
        var caixa = Vertical(_conteudo, 0, 16, null);
        Fundo(caixa, UiTema.AcoClaro, UiTema.ChanfroBotao, "SE ID", false);
        Linha(caixa, "ajuda", Textos.ControleDaMissao.ToString(), t.ToString());
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
        _hudEstrelas.text = m.Estrelas.ToString();
        int t = Mathf.CeilToInt(Mathf.Max(0f, _gm.TempoRestante));
        _hudRelogio.text = $"{t / 60}:{t % 60:00}";
        // Sem vermelho nem pulso: no último minuto o relógio só muda para a cor da energia.
        _hudRelogio.color = _gm.RelogioCorre ? (t <= 60 ? UiTema.Energia : UiTema.Branco) : UiTema.Apoio;
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
        if (proxima != null) _btnAcao.Texto(rotuloAcao.ToString());

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
        foreach (var im in _setaPartes) im.color = UiTema.Energia;
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
        _miniBarraImg.color = UiTema.Branco; // o relógio nunca fica vermelho nem pulsa
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
    // Helpers de UI (tudo sai de UiTema, FormaUI e BotaoFisico)
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
    static FormaUI Fundo(RectTransform rt, Color cor, float chanfro = UiTema.ChanfroPainel, string cantos = "SE ID", bool sombra = true)
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

    RectTransform Vertical(Transform pai, float espaco, int pad, Color? fundo = null, TextAnchor alin = TextAnchor.UpperLeft)
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

    RectTransform Horizontal(Transform pai, float espaco, bool expandir, int pad = 0, Color? fundo = null, TextAnchor alin = TextAnchor.MiddleLeft)
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
    Text Rot(Transform pai, string s, Papel papel, Color? cor = null, TextAnchor alin = TextAnchor.UpperLeft, bool caixa = false)
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
    Image Icone(Transform pai, string nome, float tam)
    {
        var rt = RT("Icone", pai);
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = UiTema.Icone(nome); im.enabled = im.sprite != null; im.preserveAspect = true; im.raycastTarget = false;
        Tam(rt, tam, tam);
        return im;
    }

    /// <summary>Uma informação por linha: ícone, rótulo curto em caixa alta (opcional) e a frase.</summary>
    RectTransform Linha(Transform pai, string icone, string rotulo, string texto)
    {
        var h = Horizontal(pai, UiTema.E16, false, 0, null, TextAnchor.UpperLeft);
        Icone(h, icone, 56);
        var v = Vertical(h, 2, 0);
        v.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        if (!string.IsNullOrEmpty(rotulo)) Rot(v, rotulo, Papel.Rotulo, UiTema.Apoio, TextAnchor.UpperLeft, true);
        if (!string.IsNullOrEmpty(texto)) Rot(v, texto, Papel.Corpo, UiTema.Branco);
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
        le.flexibleWidth = 1; le.minHeight = le.preferredHeight = custoEmChips ? 270 : 210;
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
