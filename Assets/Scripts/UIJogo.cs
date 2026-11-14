using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Telas do jogo (título, intro, briefing, HUD, resumo, fim), montadas em código no Canvas.</summary>
public class UIJogo
{
    readonly GameManager _gm;
    readonly Font _fonte;
    Transform _raiz;

    GameObject _painelMinigame, _pausa, _titulo, _intro, _briefing, _hud, _resumo, _pontuacao, _gameOver;
    Text _hudTarefas, _briefingTexto, _resumoTexto, _pontuacaoTexto, _hudDia, _hudTempo;
    RectTransform _barraEnergia, _barraOxigenio, _barraComida;
    Text _txtEnergia, _txtOxigenio, _txtComida;
    Image _fundoBriefing;
    Button _btnProximo, _btnAcao, _btnJogar, _btnComecar, _btnIniciarDia, _btnPontuacao, _btnTentar, _btnContinuar;
    Text _btnAcaoTexto, _avisoTexto;
    RectTransform _seta;
    Text _setaTexto;
    float _avisoAte, _flashAte;
    Image _alertaFundo, _flash;
    Text _alertaTexto, _nasaTag;
    Image _nasaFundo;
    Image[] _setaPartes;
    Text _miniTitulo, _miniInstrucao;
    RectTransform _miniArea, _miniBarra;
    Text _miniTempo, _miniMsg;
    Image _miniCursor;
    Text _btnProximoTexto;

    public UIJogo(GameManager gm)
    {
        _gm = gm;
        _fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_fonte == null) _fonte = Resources.GetBuiltinResource<Font>("Arial.ttf");
        Construir();
    }

    // ---------- Construção ----------

    void Construir()
    {
        var go = new GameObject("UI_Jogo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // acima dos controles
        var sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        _raiz = go.transform;
        Entrada.GarantirEventSystem();

        // Título
        _titulo = Painel("Titulo", new Color(0.02f, 0.03f, 0.08f, 1));
        Texto(_titulo.transform, "DEZ DIAS NA LUA", 110, new Vector2(0.5f, 0.62f), new Vector2(1500, 160));
        Texto(_titulo.transform, "Sobreviver no espaço é escolher o que desligar.", 44, new Vector2(0.5f, 0.48f), new Vector2(1500, 80));
        _btnJogar = Botao(_titulo.transform, "JOGAR", new Vector2(0.5f, 0.3f), new Vector2(420, 130), _gm.Jogar);

        // Intro
        _intro = Painel("Intro", new Color(0.02f, 0.03f, 0.08f, 1));
        Texto(_intro.transform, "Você pousou na Lua.\nSua base precisa de você por 10 dias.\nGerencie Energia, Oxigênio e Comida.", 56,
            new Vector2(0.5f, 0.6f), new Vector2(1500, 400));
        _btnComecar = Botao(_intro.transform, "COMEÇAR", new Vector2(0.5f, 0.25f), new Vector2(420, 130), _gm.ComecarJogo);

        // Briefing (pop-up)
        _briefing = Painel("Briefing", new Color(0, 0, 0, 0.7f));
        var caixa = Caixa(_briefing.transform, new Vector2(1600, 940));
        _fundoBriefing = caixa.GetComponent<Image>();
        _briefingTexto = Texto(caixa, "", 33, new Vector2(0.5f, 0.57f), new Vector2(1500, 740));
        _btnIniciarDia = Botao(caixa, "COMEÇAR O DIA", new Vector2(0.5f, 0.12f), new Vector2(480, 120), _gm.ComecarAcao);

        // HUD
        _hud = new GameObject("HUD", typeof(RectTransform));
        _hud.transform.SetParent(_raiz, false);
        Esticar((RectTransform)_hud.transform);
        _hud.AddComponent<SafeArea>(); // HUD respeita o entalhe do celular
        _hudDia = Texto(_hud.transform, "", 40, new Vector2(0.5f, 0.96f), new Vector2(600, 60));
        _hudTempo = Texto(_hud.transform, "", 40, new Vector2(0.5f, 0.915f), new Vector2(600, 60));
        // Tarefas do dia (sempre visíveis durante a ação)
        var fundoTarefas = new GameObject("TarefasFundo", typeof(Image));
        fundoTarefas.transform.SetParent(_hud.transform, false);
        var rtf = (RectTransform)fundoTarefas.transform;
        rtf.anchorMin = rtf.anchorMax = new Vector2(0.5f, 0.80f);
        rtf.sizeDelta = new Vector2(1100, 130);
        rtf.anchoredPosition = Vector2.zero;
        var imgf = fundoTarefas.GetComponent<Image>();
        imgf.color = new Color(0, 0, 0, 0.55f);
        imgf.raycastTarget = false;
        _hudTarefas = Texto(fundoTarefas.transform, "", 34, new Vector2(0.5f, 0.5f), new Vector2(1060, 120));
        _barraEnergia = Barra(_hud.transform, "Energia", 0.12f, new Color(1f, 0.85f, 0.2f), out _txtEnergia);
        _barraOxigenio = Barra(_hud.transform, "Oxigênio", 0.25f, new Color(0.4f, 0.8f, 1f), out _txtOxigenio);
        _barraComida = Barra(_hud.transform, "Comida", 0.38f, new Color(0.5f, 0.9f, 0.4f), out _txtComida);
        // Botão AÇÃO (aparece perto de uma estação)
        _btnAcao = Botao(_hud.transform, "AÇÃO", new Vector2(0.86f, 0.48f), new Vector2(420, 170), () => { _gm.Interagir(); Entrada.Selecionar(null); });
        Cores(_btnAcao, new Color(0.95f, 0.6f, 0.1f, 1));
        _btnAcao.navigation = new Navigation { mode = Navigation.Mode.None };
        _btnAcaoTexto = _btnAcao.GetComponentInChildren<Text>();
        _btnAcao.gameObject.SetActive(false);

        // Aviso rápido (tarefa concluída)
        _avisoTexto = Texto(_hud.transform, "", 56, new Vector2(0.5f, 0.28f), new Vector2(1200, 100));
        _avisoTexto.color = new Color(0.5f, 1f, 0.5f);

        // Banner de alerta (evento em andamento) + flash vermelho de tela
        _alertaFundo = Retangulo(_hud.transform, Vector2.zero, new Vector2(1500, 170), new Color(0.6f, 0.05f, 0.05f, 0.9f));
        var rta = _alertaFundo.rectTransform;
        rta.anchorMin = rta.anchorMax = new Vector2(0.5f, 0.66f);
        rta.anchoredPosition = Vector2.zero;
        _alertaTexto = Texto(_alertaFundo.transform, "", 38, new Vector2(0.5f, 0.5f), new Vector2(1460, 160));
        _alertaFundo.gameObject.SetActive(false);
        _flash = Retangulo(_raiz, Vector2.zero, Vector2.zero, new Color(1f, 0f, 0f, 0f));
        Esticar(_flash.rectTransform);
        _flash.transform.SetSiblingIndex(_hud.transform.GetSiblingIndex() + 1);

        // Etiqueta do evento NASA do dia (sempre visível durante a ação)
        _nasaFundo = Retangulo(_hud.transform, Vector2.zero, new Vector2(760, 110), new Color(0.05f, 0.15f, 0.35f, 0.8f));
        var rtn = _nasaFundo.rectTransform;
        rtn.anchorMin = rtn.anchorMax = new Vector2(0f, 0.72f);
        rtn.pivot = new Vector2(0f, 0.5f);
        rtn.anchoredPosition = new Vector2(40, 0);
        _nasaTag = Texto(_nasaFundo.transform, "", 28, new Vector2(0.5f, 0.5f), new Vector2(740, 100));
        _nasaTag.alignment = TextAnchor.MiddleLeft;

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
        foreach (var im in _setaPartes)
        { im.color = new Color(1f, 0.9f, 0.2f, 0.95f); im.raycastTarget = false; }
        _setaTexto = Texto(_hud.transform, "", 34, new Vector2(0.5f, 0.5f), new Vector2(400, 50));
        _setaTexto.color = new Color(1f, 0.9f, 0.2f);
        _seta.gameObject.SetActive(false);
        _setaTexto.gameObject.SetActive(false);

        // Botão de pausa (toque/mouse). No controle: Start.
        var bp = Botao(_hud.transform, "II", new Vector2(0.95f, 0.94f), new Vector2(110, 90), _gm.Pausar);
        bp.navigation = new Navigation { mode = Navigation.Mode.None };

        // Painel de minigame (conteúdo montado por cada Minigame)
        _painelMinigame = Painel("Minigame", new Color(0, 0, 0, 0.75f));
        var cm = Caixa(_painelMinigame.transform, new Vector2(1500, 860));
        _miniTitulo = Texto(cm, "", 62, new Vector2(0.5f, 0.93f), new Vector2(1300, 100));
        _miniInstrucao = Texto(cm, "", 36, new Vector2(0.5f, 0.84f), new Vector2(1300, 70));
        var areaGo = new GameObject("Area", typeof(RectTransform));
        areaGo.transform.SetParent(cm, false);
        _miniArea = (RectTransform)areaGo.transform;
        _miniArea.anchorMin = _miniArea.anchorMax = new Vector2(0.5f, 0.45f);
        _miniArea.sizeDelta = new Vector2(1400, 520);
        _miniArea.anchoredPosition = Vector2.zero;
        // Barra e número do tempo
        var fundoTempo = Retangulo(cm, new Vector2(0, 232), new Vector2(1300, 18), new Color(0, 0, 0, 0.5f));
        var preencheT = Retangulo(fundoTempo.transform, Vector2.zero, new Vector2(1292, 12), new Color(0.4f, 0.85f, 1f));
        _miniBarra = preencheT.rectTransform;
        _miniBarra.pivot = new Vector2(0, 0.5f);
        _miniBarra.anchoredPosition = new Vector2(-646, 0);
        _miniTempo = Texto(cm, "", 44, new Vector2(0.93f, 0.93f), new Vector2(200, 70));
        _miniMsg = Texto(cm, "", 80, new Vector2(0.5f, 0.5f), new Vector2(1300, 160));
        _miniMsg.color = new Color(1f, 0.9f, 0.3f);
        Botao(cm, "SAIR (B)", new Vector2(0.14f, 0.07f), new Vector2(300, 90), _gm.CancelarMinigame);
        // Cursor virtual (controle) por cima de tudo no painel
        _miniCursor = Retangulo(_painelMinigame.transform, Vector2.zero, new Vector2(48, 48), new Color(1f, 0.85f, 0.2f, 0.95f));
        _miniCursor.sprite = ControlesMobile.Circulo();
        _miniCursor.gameObject.SetActive(false);
        _painelMinigame.SetActive(false);

        // Tela de pausa
        _pausa = Painel("Pausa", new Color(0, 0, 0, 0.8f));
        Texto(_pausa.transform, "PAUSA", 100, new Vector2(0.5f, 0.68f), new Vector2(900, 150));
        _btnContinuar = Botao(_pausa.transform, "CONTINUAR (B)", new Vector2(0.5f, 0.5f), new Vector2(560, 120), _gm.Retomar);
        Botao(_pausa.transform, "REINICIAR", new Vector2(0.5f, 0.35f), new Vector2(560, 120), _gm.Reiniciar);
        Texto(_pausa.transform, "A: confirmar   B: voltar   Start: pausa", 36, new Vector2(0.5f, 0.2f), new Vector2(1200, 60));
        _pausa.SetActive(false);

        // Resumo do dia (só informa)
        _resumo = Painel("Resumo", new Color(0, 0, 0, 0.85f));
        var cr = Caixa(_resumo.transform, new Vector2(1100, 700));
        _resumoTexto = Texto(cr, "", 46, new Vector2(0.5f, 0.6f), new Vector2(1000, 500));
        _btnProximo = Botao(cr, "PRÓXIMO DIA", new Vector2(0.5f, 0.13f), new Vector2(480, 120), _gm.ProximoDia);
        _btnProximoTexto = _btnProximo.GetComponentInChildren<Text>();

        // Pontuação
        _pontuacao = Painel("Pontuacao", new Color(0.02f, 0.03f, 0.08f, 1));
        _pontuacaoTexto = Texto(_pontuacao.transform, "", 56, new Vector2(0.5f, 0.6f), new Vector2(1500, 500));
        _btnPontuacao = Botao(_pontuacao.transform, "JOGAR DE NOVO", new Vector2(0.5f, 0.22f), new Vector2(520, 130), _gm.Reiniciar);

        // Game Over
        _gameOver = Painel("GameOver", new Color(0.15f, 0, 0, 1));
        Texto(_gameOver.transform, "GAME OVER\nO oxigênio da base acabou.", 70, new Vector2(0.5f, 0.6f), new Vector2(1500, 300));
        _btnTentar = Botao(_gameOver.transform, "TENTAR DE NOVO", new Vector2(0.5f, 0.25f), new Vector2(520, 130), _gm.Reiniciar);

        _gm.RecursosMudaram += AtualizarHud;
    }

    // ---------- Estados ----------

    public void Mostrar(EstadoJogo e)
    {
        _titulo.SetActive(e == EstadoJogo.Titulo);
        _intro.SetActive(e == EstadoJogo.Intro);
        _briefing.SetActive(e == EstadoJogo.Briefing);
        _hud.SetActive(e == EstadoJogo.Acao || e == EstadoJogo.Minigame);
        _resumo.SetActive(e == EstadoJogo.Resumo);
        _pontuacao.SetActive(e == EstadoJogo.Pontuacao);
        _gameOver.SetActive(e == EstadoJogo.GameOver);

        _pausa.SetActive(false);
        _painelMinigame.SetActive(e == EstadoJogo.Minigame);
        if (e != EstadoJogo.Acao && e != EstadoJogo.Minigame) { _alertaFundo.gameObject.SetActive(false); _flash.color = new Color(1, 0, 0, 0); }
        if (e != EstadoJogo.Acao) { _btnAcao.gameObject.SetActive(false); _seta.gameObject.SetActive(false); _setaTexto.gameObject.SetActive(false); }
        Button primeiro = null;
        switch (e)
        {
            case EstadoJogo.Titulo: primeiro = _btnJogar; break;
            case EstadoJogo.Intro: primeiro = _btnComecar; break;
            case EstadoJogo.Briefing: primeiro = _btnIniciarDia; break;
            case EstadoJogo.Resumo: primeiro = _btnProximo; break;
            case EstadoJogo.Pontuacao: primeiro = _btnPontuacao; break;
            case EstadoJogo.GameOver: primeiro = _btnTentar; break;
        }
        Entrada.Selecionar(primeiro != null ? primeiro.gameObject : null);

        if (e == EstadoJogo.Briefing) PreencherBriefing();
        if (e == EstadoJogo.Resumo) PreencherResumo();
        if (e == EstadoJogo.Pontuacao) _pontuacaoTexto.text = "FIM DOS 10 DIAS\n\n" + _gm.TextoPontuacao() +
            $"\n\nOxigênio {_gm.Recursos.oxigenio:0}%   Comida {_gm.Recursos.comida:0}%\n\n{DonkiDados.Creditos}";
        if (e == EstadoJogo.Acao)
        {
            AtualizarTarefas();
            AtualizarNasa();
            AtualizarHud();
        }
    }

    public void AbrirMinigame(Minigame m, System.Action<bool> aoTerminar)
    {
        _miniTitulo.text = m.titulo;
        _miniInstrucao.text = m.instrucao;
        _miniMsg.text = "";
        m.Abrir(this, _miniArea, aoTerminar);
        Entrada.Selecionar(m.PrimeiroBotao != null ? m.PrimeiroBotao.gameObject : null);
    }

    public void LimparArea()
    {
        foreach (Transform filho in _miniArea) Object.Destroy(filho.gameObject);
        // Destroy só age no fim do frame: esconde já para não aparecer duplicado.
        foreach (Transform filho in _miniArea) filho.gameObject.SetActive(false);
    }

    public void Mensagem(string txt) => _miniMsg.text = txt;

    public void Tempo(float fracao, float segundos)
    {
        _miniBarra.sizeDelta = new Vector2(1292f * Mathf.Clamp01(fracao), 12f);
        _miniBarra.GetComponent<Image>().color = fracao < 0.25f ? new Color(1f, 0.35f, 0.3f) : new Color(0.4f, 0.85f, 1f);
        _miniTempo.text = Mathf.CeilToInt(segundos) + " s";
    }

    public void Cursor(bool visivel, Vector2 posTela)
    {
        _miniCursor.gameObject.SetActive(visivel);
        if (visivel) _miniCursor.transform.position = posTela;
    }

    /// <summary>Retângulo centrado (âncora no centro do pai) em posição/tamanho do canvas.</summary>
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

    public void MostrarPausa(bool mostrar)
    {
        _pausa.SetActive(mostrar);
        Entrada.Selecionar(mostrar ? _btnContinuar.gameObject : null);
    }

    void PreencherBriefing()
    {
        var d = _gm.DadosDoDia;
        _fundoBriefing.color = d.alertaVermelho ? new Color(0.45f, 0.05f, 0.05f, 0.97f) : new Color(0.08f, 0.12f, 0.2f, 0.97f);
        string nasa = DonkiDados.Do(d.numero) is EventoNasa en ? "\n\n" + en.Card
            : (string.IsNullOrEmpty(d.alertaNasa) ? "" : $"\n\nALERTA NASA: {d.alertaNasa}");
        var nomes = new System.Collections.Generic.List<string>();
        foreach (var id in _gm.TarefasHoje) nomes.Add(_gm.NomeDe(id));
        var r = _gm.Recursos;
        _briefingTexto.text = $"DIA {d.numero} — {d.titulo}\n\n{d.briefing}\n\nTarefas: {string.Join(", ", nomes)}{nasa}\n\n" +
            $"Energia {r.energia:0}%  |  Oxigênio {r.oxigenio:0}%  |  Comida {r.comida:0}%";
    }

    void PreencherResumo()
    {
        var d = _gm.DadosDoDia;
        var r = _gm.Recursos;
        _resumoTexto.text = $"RESUMO DO DIA {d.numero}\n\n" +
            $"Comida: {(-_gm.ResumoComida >= 0 ? "+" : "")}{-_gm.ResumoComida:0}\n" +
            $"Oxigênio: -{_gm.ResumoOxigenio:0}\n" +
            $"Energia restante: {_gm.ResumoEnergiaRestante:0}%\n\n" +
            (_gm.ResumoLinhas.Count > 0 ? string.Join("\n", _gm.ResumoLinhas) + "\n\n" : "") +
            $"Agora: Oxigênio {r.oxigenio:0}%  |  Comida {r.comida:0}%";
        _btnProximoTexto.text = _gm.DiaAtual >= _gm.TotalDias ? "VER RESULTADO" : "PRÓXIMO DIA";
    }

    public void AtualizarHud()
    {
        if (_hud == null || !_hud.activeSelf) return;
        _hudDia.text = $"Dia {_gm.DiaAtual}/{_gm.TotalDias}";
        int t = Mathf.CeilToInt(_gm.TempoRestante);
        _hudTempo.text = $"{t / 60}:{t % 60:00}";
        var r = _gm.Recursos;
        Preencher(_barraEnergia, _txtEnergia, "Energia", r.energia);
        Preencher(_barraOxigenio, _txtOxigenio, "Oxigênio", r.oxigenio);
        Preencher(_barraComida, _txtComida, "Comida", r.comida);
    }

    /// <summary>Etiqueta "NASA DONKI: qual evento está acontecendo hoje".</summary>
    public void AtualizarNasa()
    {
        var en = DonkiDados.Do(_gm.DiaAtual);
        _nasaFundo.gameObject.SetActive(en != null);
        if (en == null) return;
        _nasaFundo.color = en.perigo == "EXTREMO" ? new Color(0.5f, 0.05f, 0.05f, 0.85f) : new Color(0.05f, 0.15f, 0.35f, 0.8f);
        _nasaTag.text = $"NASA DONKI · {en.data}\n{en.nome}\n{en.Linha}";
    }

    public void AtualizarTarefas()
    {
        var d = _gm.DadosDoDia;
        string lista = "";
        foreach (var id in _gm.TarefasHoje)
            lista += (_gm.EstaConcluida(id) ? "[x] " : "[ ] ") + _gm.NomeDe(id) + "    ";
        _hudTarefas.text = $"TAREFAS DO DIA {d.numero}:\n{lista}";
    }

    public void Aviso(string texto)
    {
        _avisoTexto.text = texto;
        _avisoAte = Time.time + 2f;
    }

    public void Alerta(string titulo, string texto)
    {
        _flashAte = Time.time + 1.6f;
        Aviso(titulo);
    }

    public void AtualizarAlerta(EventoDia ev, float restante)
    {
        // Flash vermelho pulsante logo após o alarme
        float a = Time.time < _flashAte ? Mathf.PingPong(Time.time * 4f, 1f) * 0.35f : 0f;
        _flash.color = new Color(1f, 0f, 0f, a);

        _alertaFundo.gameObject.SetActive(ev != null);
        if (ev == null) return;
        string tempo = ev.limite > 0f ? $"   ({Mathf.CeilToInt(Mathf.Max(0f, restante))} s)" : "";
        var en = DonkiDados.Do(_gm.DiaAtual);
        string real = en != null ? $"\nEvento real NASA: {en.nome} ({en.Linha})" : "";
        _alertaTexto.text = $"{ev.titulo}{tempo}\n{ev.texto}{real}";
        _alertaFundo.color = new Color(0.6f, 0.05f, 0.05f, 0.75f + Mathf.PingPong(Time.time * 2f, 0.2f));
    }

    /// <summary>Atualiza seta guia, botão AÇÃO e aviso a cada frame.</summary>
    public void AtualizarGuia(Estacao alvo, Estacao proxima)
    {
        if (_avisoTexto.text != "" && Time.time > _avisoAte) _avisoTexto.text = "";

        _btnAcao.gameObject.SetActive(proxima != null);
        if (proxima != null)
        {
            string extra = Estacao.Custos(proxima.id).aviso;
            _btnAcaoTexto.text = proxima.id == "cama" ? "DORMIR (A / E)"
                : $"{proxima.nome.ToUpper()}\nINICIAR (A / E)" + (extra != "" ? "\n" + extra : "");
        }

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
        _setaTexto.transform.position = pos + Vector2.down * 70f;
        var corSeta = _gm.AlvoUrgente ? new Color(1f, 0.25f, 0.2f, 1f) : new Color(1f, 0.9f, 0.2f, 0.95f);
        foreach (var im in _setaPartes) im.color = corSeta;
        _setaTexto.color = corSeta;
        _setaTexto.text = $"{alvo.nome}  {alvo.DistanciaAte(_gm.jogador.transform.position):0} m";
    }

    static void Preencher(RectTransform barra, Text txt, string nome, float valor)
    {
        barra.anchorMax = new Vector2(Mathf.Clamp01(valor / Recursos.Max), 1);
        txt.text = $"{nome} {valor:0}%";
    }

    // ---------- Helpers de UI ----------

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

    public Button Botao(Transform pai, string rotulo, Vector2 ancora, Vector2 tamanho, UnityEngine.Events.UnityAction acao)
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
        c.selectedColor = new Color(1f, 0.85f, 0.2f, 1f); // amarelo = selecionado
        c.pressedColor = Color.Lerp(normal, Color.black, 0.3f);
        c.colorMultiplier = 1f;
        btn.colors = c;
    }

    RectTransform Barra(Transform pai, string nome, float yMin, Color cor, out Text txt)
    {
        // Barras empilhadas no canto superior esquerdo (yMin só ordena: 0.12 → mais abaixo)
        float y = 0.96f - (yMin - 0.12f) / 0.13f * 0.07f;
        var fundo = new GameObject("Barra_" + nome, typeof(Image));
        fundo.transform.SetParent(pai, false);
        var rf = (RectTransform)fundo.transform;
        rf.anchorMin = rf.anchorMax = new Vector2(0, y);
        rf.pivot = new Vector2(0, 0.5f);
        rf.sizeDelta = new Vector2(520, 50);
        rf.anchoredPosition = new Vector2(40, 0);
        fundo.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);

        var preenche = new GameObject("Preenchimento", typeof(Image));
        preenche.transform.SetParent(fundo.transform, false);
        var rp = (RectTransform)preenche.transform;
        rp.anchorMin = Vector2.zero; rp.anchorMax = Vector2.one;
        rp.offsetMin = new Vector2(4, 4); rp.offsetMax = new Vector2(-4, -4);
        preenche.GetComponent<Image>().color = cor;

        txt = Texto(fundo.transform, nome, 30, new Vector2(0.5f, 0.5f), new Vector2(500, 50));
        txt.color = Color.black;
        return rp;
    }
}
