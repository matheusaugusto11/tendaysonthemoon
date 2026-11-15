using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using DezDias;
using UnityEngine;

/// <summary>
/// Só diagnóstico: com Temp/probe_jogo.flag, percorre o fluxo do jogo no Play (uma missão inteira, o relógio
/// acabando, a troca de idioma e os 10 minijogos), confere as fases contra as specs e grava
/// Temp/probe_jogo.txt e capturas em Temp/probe_capturas/. No fim pede para sair do Play (Temp/pedido_parar_play.txt).
/// </summary>
public class ProbeJogo : MonoBehaviour
{
    static bool Pedido => Application.isEditor && File.Exists(Path.Combine(Application.dataPath, "../Temp/probe_jogo.flag"));

    // Antes da cena: o GameManager lê as preferências no Awake, então a sonda limpa antes dele.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LimparPreferencias()
    {
        if (!Pedido) return;
        PlayerPrefs.DeleteKey("tdm_seen");
        PlayerPrefs.DeleteKey("tdm_tut");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (!Pedido) return;
        new GameObject("ProbeJogo").AddComponent<ProbeJogo>();
    }

    readonly StringBuilder _sb = new StringBuilder();
    int _ok, _falhas;
    string _raiz, _pasta;
    GameManager _gm;

    void Log(string s) => _sb.AppendLine($"[{Time.realtimeSinceStartup:0.0}s] {s}");

    void Checar(bool cond, string o)
    {
        if (cond) _ok++; else _falhas++;
        Log((cond ? "OK     " : "FALHOU ") + o);
    }

    void OnEnable() { Application.logMessageReceived += Capturar; }
    void OnDisable() { Application.logMessageReceived -= Capturar; }
    void Capturar(string msg, string stack, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert) { _falhas++; _sb.AppendLine($"  >> {t}: {msg}\n     {stack.Split('\n')[0]}"); }
        else if (t == LogType.Warning) _sb.AppendLine($"  >> {t}: {msg}");
        else if (msg.StartsWith("[GameManager]")) _sb.AppendLine("  >> " + msg);
    }

    bool Congelado()
    {
        var mov = FindObjectOfType<Movimento>();
        var f = typeof(Movimento).GetField("_congelado", BindingFlags.NonPublic | BindingFlags.Instance);
        return mov != null && f != null && (bool)f.GetValue(mov);
    }

    /// <summary>
    /// Captura a tela depois de <paramref name="espera"/> segundos em tempo real (não para na pausa). A entrada do painel
    /// (250 ms) e os voos da câmera (até 1,2 s) terminam antes da foto; com 0, a foto sai no quadro seguinte.
    /// </summary>
    IEnumerator Foto(string nome, float espera = 1.3f)
    {
        if (espera > 0f) yield return new WaitForSecondsRealtime(espera);
        if (espera > 0f) ConferirModoDaFase(nome);
        yield return null;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(_pasta, nome + ".png"));
        yield return null;
    }

    IEnumerator Teleportar(Estacao alvo)
    {
        var mov = FindObjectOfType<Movimento>();
        if (mov == null || alvo == null) yield break;
        var cc = mov.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        mov.transform.position = alvo.transform.position + new Vector3(0.6f, 0.2f, 0.6f);
        if (cc) cc.enabled = true;
        yield return null;
        yield return null;
    }

    // ---------- Painel, páginas, câmera, pausa e movimento reduzido (grupo 5) ----------

    static Rect MundoRect(RectTransform r) { var c = new Vector3[4]; r.GetWorldCorners(c); return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y); }

    /// <summary>Confere o modo do painel da fase atual: Lateral à direita sem véu, Coluna à direita, Página opaca.</summary>
    void ConferirModoDaFase(string nome)
    {
        string modo;
        switch (_gm.Fase)
        {
            case Fase.Inicio: modo = "coluna"; break;
            case Fase.Relatorio: case Fase.Creditos: case Fase.ComoJogar: case Fase.MenuMinijogos: modo = "pagina"; break;
            case Fase.Boletim: case Fase.Escolha: case Fase.MinijogoInfo: case Fase.DecisaoVazamento: case Fase.TarefaConcluida:
            case Fase.Distribuir: case Fase.DiaAcontece: case Fase.Resultado: modo = "lateral"; break;
            default: return;
        }
        var go = GameObject.Find("Tablet"); var tela = GameObject.Find("Tela");
        if (go == null || tela == null) { Checar(false, $"{nome}: painel ou camada das telas não encontrados"); return; }
        Rect r = MundoRect((RectTransform)go.transform);
        float W = Screen.width;
        var img = tela.GetComponent<UnityEngine.UI.Image>();
        if (modo == "pagina")
        {
            Checar(img.color == UiTema.Pagina && img.color.a >= 0.99f, $"{nome}: página opaca de céu preto, sem a cena atrás");
            var face = go.transform.Find("Fundo");
            Checar(face != null && !face.gameObject.activeSelf, $"{nome}: a página não tem a caixa do painel");
            return;
        }
        float frac = modo == "lateral" ? 840f / 1920f : 640f / 1920f;
        Checar(r.xMin >= W * 0.5f && r.xMax <= W + 0.5f && Mathf.Abs(r.width / W - frac) < 0.02f,
            $"{nome}: painel {modo} à direita com {r.width / W:0.000} da largura (esperado {frac:0.000}), x de {r.xMin:0} a {r.xMax:0} de {W:0}");
        Checar(img.color.a == 0f, $"{nome}: sem véu escuro sobre a cena");
    }

    UnityEngine.UI.Text AchaTexto(string conteudo)
    {
        foreach (var t in FindObjectsOfType<UnityEngine.UI.Text>()) if (t.text == conteudo) return t;
        return null;
    }

    BotaoFisico AchaBotao(string texto)
    {
        foreach (var b in FindObjectsOfType<BotaoFisico>()) if (b.Rotulo != null && b.Rotulo.text == texto) return b;
        return null;
    }

    /// <summary>Pausa: Som com o estado escrito e guardado, e Reiniciar com confirmação em tom de pergunta.</summary>
    IEnumerator TestarPausa()
    {
        var som = AchaBotao(Textos.SomLigado.ToString());
        Checar(som != null, "pausa: o botão de som mostra o estado escrito (\"" + Textos.SomLigado + "\")");
        if (som == null) yield break;
        bool estava = SonsUI.Ligado;
        som.onClick.Invoke();
        yield return null;
        Checar(!SonsUI.Ligado && PlayerPrefs.GetInt("tdm_som", 1) == 0, "pausa: desligar o som guarda a escolha em PlayerPrefs (tdm_som = 0), de onde ela volta depois de recarregar a página");
        Checar(AchaBotao(Textos.SomDesligado.ToString()) != null, "pausa: o botão passa a dizer \"" + Textos.SomDesligado + "\"");
        int tocados = SonsUI.Tocados;
        SonsUI.Toque();
        Checar(SonsUI.Tocados == tocados, "com o som desligado nenhum som é tocado");
        AchaBotao(Textos.SomDesligado.ToString())?.onClick.Invoke();
        yield return null;
        Checar(SonsUI.Ligado == estava, "pausa: ligar de novo devolve o som");

        var gmAntes = GameManager.Instancia;
        var reiniciar = AchaBotao(Textos.Reiniciar.ToString());
        Checar(reiniciar != null, "pausa: botão Reiniciar");
        if (reiniciar == null) yield break;
        reiniciar.onClick.Invoke();
        yield return new WaitForSecondsRealtime(0.3f);
        Checar(AchaTexto(Textos.ReiniciarPergunta.ToString()) != null, "reiniciar pede confirmação em tom de pergunta: \"" + Textos.ReiniciarPergunta + "\"");
        Checar(AchaBotao(Textos.Voltar.ToString()) != null && AchaBotao(Textos.SimVoltarAoInicio.ToString()) != null, "a confirmação tem as opções de voltar e de confirmar");
        Checar(GameManager.Instancia == gmAntes && _gm.Pausado && _gm.Missao != null, "a missão continua intacta até confirmar");
        yield return Foto("26b_pausa_confirmacao");
        AchaBotao(Textos.Voltar.ToString())?.onClick.Invoke();
        yield return null;
        Checar(AchaTexto(Textos.ReiniciarPergunta.ToString()) == null && AchaBotao(Textos.Reiniciar.ToString()) != null, "voltar fecha a pergunta e mostra as opções da pausa");
    }

    /// <summary>
    /// Movimento reduzido: o painel só aparece (sem escorregar), o medidor salta, a variação não deriva, a câmera corta e a
    /// órbita do Início fica parada. Liga e desliga a preferência pelo UiTema, como o navegador faria.
    /// </summary>
    IEnumerator TestarMovimentoReduzido()
    {
        var cf = CameraFoco.Instancia;
        yield return new WaitForSecondsRealtime(1.6f); // termina o voo para a órbita do Início
        Checar(cf != null && cf.Orbitando && cf.EstadoAtual == CameraFoco.Estado.Preso, "início: a câmera orbita a base");
        Vector3 antes = Camera.main.transform.position;
        yield return new WaitForSecondsRealtime(1.0f);
        Checar((Camera.main.transform.position - antes).magnitude > 0.05f, "sem preferência por menos movimento, a órbita do Início anda");

        UiTema.ForcarMovimentoReduzido = true;
        yield return new WaitForSecondsRealtime(0.3f);
        Vector3 p1 = Camera.main.transform.position;
        yield return new WaitForSecondsRealtime(1.0f);
        Checar((Camera.main.transform.position - p1).magnitude < 0.001f, "movimento reduzido: a órbita do Início fica parada");

        // Painel: troca de fase só com opacidade.
        var tab = (RectTransform)GameObject.Find("Tablet").transform;
        _gm.AbrirComoJogar();
        float xMin = float.MaxValue, xMax = float.MinValue, aMin = 2f, aMax = -1f, aAnt = -1f;
        bool crescente = true;
        var tela = GameObject.Find("Tela").GetComponent<CanvasGroup>();
        for (int i = 0; i < 12; i++)
        {
            xMin = Mathf.Min(xMin, tab.anchoredPosition.x); xMax = Mathf.Max(xMax, tab.anchoredPosition.x);
            aMin = Mathf.Min(aMin, tela.alpha); aMax = Mathf.Max(aMax, tela.alpha);
            if (tela.alpha < aAnt - 0.001f) crescente = false;
            aAnt = tela.alpha;
            yield return null;
        }
        Checar(xMax - xMin < 0.01f, $"movimento reduzido: o painel não escorrega (x varia {xMax - xMin:0.00})");
        Checar(aMin < 0.9f && crescente, $"movimento reduzido: o painel entra só pela opacidade (alfa de {aMin:0.00} a {aMax:0.00})");
        yield return new WaitForSecondsRealtime(0.5f);
        Checar(tela.alpha > 0.99f, "movimento reduzido: o painel termina totalmente opaco");
        _gm.PularSlides();
        yield return null;
        UiTema.ForcarMovimentoReduzido = false;
    }

    // ---------- HUD (grupo 4 da mudança interface-e-minijogos) ----------

    static MedidorHud Med(TipoMedidor t)
    {
        foreach (var m in FindObjectsOfType<MedidorHud>()) if (m.tipo == t) return m;
        return null;
    }

    /// <summary>Medidores por forma, zonas de alerta, animação de 600 ms e variação flutuante. Restaura os valores no fim.</summary>
    IEnumerator TestarHud()
    {
        var m = _gm.Missao;
        int o2 = m.O2, rad = m.Rad, bat = m.Bateria;
        var mo = Med(TipoMedidor.Oxigenio); var mr = Med(TipoMedidor.Radiacao); var mb = Med(TipoMedidor.Bateria);
        Checar(mo != null && mr != null && mb != null, "HUD tem os 3 medidores (oxigênio, radiação e bateria)");
        if (mo == null || mr == null || mb == null) yield break;

        // Leitura rápida e zona marcada antes de ser alcançada.
        m.O2 = 6; m.Rad = 2;
        yield return new WaitForSeconds(0.9f);
        Checar(mo.Alvo == 6 && mo.SegmentosCheios == 6 && mo.NumeroTexto == "6" && !mo.EmAlerta,
            $"leitura rápida: oxigênio 6 de 10 = 6 segmentos cheios e o número 6 (cheios {mo.SegmentosCheios}, número {mo.NumeroTexto})");
        Checar(mo.ZonaVisivel, "a zona de alerta do oxigênio está marcada antes de ser alcançada");
        Checar(mr.ZonaVisivel, "a faixa hachurada da radiação está visível fora da zona");

        // Oxigênio na zona: vermelho e triângulo, o resto do HUD não muda.
        m.O2 = 3;
        yield return new WaitForSeconds(0.9f);
        Checar(mo.EmAlerta && mo.AlertaVisivel && mo.CorDoPreenchimento == UiTema.Alerta, "oxigênio 3: segmentos vermelhos e triângulo de alerta");
        Checar(!mr.EmAlerta && mr.CorDoPreenchimento == UiTema.Abrigo && mb.CorDoPreenchimento == UiTema.Energia, "oxigênio em alerta: radiação e bateria não mudam de cor");

        // Radiação alta.
        m.O2 = 6; m.Rad = 7;
        yield return new WaitForSeconds(0.9f);
        Checar(mr.EmAlerta && mr.AlertaVisivel && mr.CorDoPreenchimento == UiTema.Alerta, "radiação 7: barra vermelha com o triângulo de alerta");
        Checar(!mo.EmAlerta && !mo.AlertaVisivel && mo.CorDoPreenchimento == UiTema.Ar && !mb.EmAlerta, "radiação em alerta: o resto do HUD não muda de cor");
        yield return Foto("16_hud_radiacao_alta");

        // Fora da zona: cor normal e a faixa hachurada continua à vista.
        m.Rad = 4;
        yield return new WaitForSeconds(0.9f);
        Checar(!mr.EmAlerta && !mr.AlertaVisivel && mr.CorDoPreenchimento == UiTema.Abrigo && mr.ZonaVisivel, "radiação 4: cor normal e a faixa hachurada continua visível");

        // Custo de sair da base: o oxigênio anda dois segmentos e aparece ▼ 2.
        m.O2 = 8;
        yield return new WaitForSeconds(2.2f);
        m.O2 = 6;
        yield return new WaitForSeconds(0.12f);
        var v = mo.VariacaoAtual;
        Checar(v != null && v.Delta == -2 && v.TrianguloParaBaixo, "custo de 2 de oxigênio: aparece a variação flutuante com o triângulo para baixo e o 2");
        Checar(mo.Mostrado > 6.05f && mo.Mostrado < 7.95f, $"o medidor anda em vez de saltar (mostrado {mo.Mostrado:0.00} entre 8 e 6)");
        yield return Foto("17_hud_variacao", 0f); // no meio da animação, de propósito
        yield return new WaitForSeconds(0.9f);
        Checar(mo.Mostrado == 6f && mo.SegmentosCheios == 6, $"depois de 600 ms o oxigênio chegou a 6 ({mo.SegmentosCheios} segmentos)");
        yield return new WaitForSeconds(1.6f);
        Checar(FindObjectsOfType<VariacaoFlutuante>().Length == 0, "nenhuma variação fica presa na tela");

        // Devolve o jogo ao estado de antes e confere que a volta também não deixa nada preso.
        m.O2 = o2; m.Rad = rad; m.Bateria = bat;
        yield return new WaitForSeconds(3f);
        Checar(FindObjectsOfType<VariacaoFlutuante>().Length == 0 && mo.Alvo == Mathf.Max(0, o2), "valores restaurados, sem variação presa");

        // Movimento reduzido: o medidor salta direto e a variação só aparece e some, sem derivar.
        UiTema.ForcarMovimentoReduzido = true;
        int base2 = Mathf.Max(0, m.O2);
        m.O2 = base2 - 2;
        yield return null; yield return null;
        Checar(mo.Mostrado == base2 - 2 && mo.SegmentosCheios == base2 - 2, $"movimento reduzido: o medidor salta direto para {base2 - 2} (mostrado {mo.Mostrado:0.00})");
        var vr = mo.VariacaoAtual;
        Checar(vr != null && vr.Delta == -2, "movimento reduzido: a variação aparece");
        if (vr != null)
        {
            float y0 = ((RectTransform)vr.transform).anchoredPosition.y;
            yield return new WaitForSeconds(0.4f);
            float y1 = vr != null ? ((RectTransform)vr.transform).anchoredPosition.y : y0;
            Checar(Mathf.Abs(y1 - y0) < 0.01f, $"movimento reduzido: a variação não deriva (y {y0:0.0} para {y1:0.0})");
        }
        UiTema.ForcarMovimentoReduzido = false;
        m.O2 = o2;
        yield return new WaitForSeconds(2.2f);
    }

    /// <summary>Faixa do deslocamento: ícone e tira na cor da tarefa de hoje, e a seta guia na mesma cor.</summary>
    void ConferirFaixa(string tarefa, string quando)
    {
        var faixa = GameObject.Find("FaixaDeslocamento");
        if (faixa == null) { Checar(false, quando + ": faixa do deslocamento não encontrada"); return; }
        var cor = UiTema.CorDaTarefa(tarefa);
        var tira = faixa.transform.Find("Tira");
        Checar(tira != null && tira.GetComponent<FormaUI>().color == cor, $"{quando} ({tarefa}): a tira da faixa usa a cor do módulo de destino");
        Sprite esperado = UiTema.Icone(UiTema.IconeDaTarefa(tarefa)), achado = null;
        foreach (var im in faixa.GetComponentsInChildren<UnityEngine.UI.Image>()) if (im.name == "Icone") achado = im.sprite;
        Checar(achado != null && achado == esperado, $"{quando} ({tarefa}): a faixa mostra o ícone da tarefa");
        var seta = GameObject.Find("Seta");
        if (seta != null)
        {
            var haste = seta.transform.Find("Haste");
            Checar(haste != null && haste.GetComponent<UnityEngine.UI.Image>().color == cor, $"{quando} ({tarefa}): a seta guia usa a cor do módulo de destino");
        }
    }

    /// <summary>Botão de ação junto da estação: do tamanho do botão principal, no canto direito, acima do joystick da câmera.</summary>
    void ConferirBotaoDeAcao()
    {
        var go = GameObject.Find("BotaoAcao");
        var mov = FindObjectOfType<Movimento>();
        if (go == null || mov == null || mov.joystickCamera == null) { Checar(false, "botão de ação ou joystick da câmera não encontrados"); return; }
        var rb = (RectTransform)go.transform; var rj = (RectTransform)mov.joystickCamera.transform;
        Rect Mundo(RectTransform r) { var c = new Vector3[4]; r.GetWorldCorners(c); return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y); }
        Rect b = Mundo(rb), j = Mundo(rj);
        Checar(rb.sizeDelta == new Vector2(400f, 104f), "o botão de ação tem o tamanho do botão principal (400 por 104): " + rb.sizeDelta);
        Checar(!b.Overlaps(j), "o botão de ação não cobre o joystick da câmera");
        Checar(b.yMin >= j.yMax - 0.5f, $"o botão de ação fica acima do joystick da câmera (base do botão {b.yMin:0}, topo do joystick {j.yMax:0})");
        Checar(b.center.x > Screen.width * 0.5f && b.xMax <= Screen.width, "o botão de ação fica no lado direito da tela");
    }

    IEnumerator Start()
    {
        _raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        _pasta = Path.Combine(_raiz, "Temp/probe_capturas");
        Directory.CreateDirectory(_pasta);
        foreach (var f in Directory.GetFiles(_pasta, "*.png")) File.Delete(f);
        yield return new WaitForSeconds(1.5f);
        _gm = GameManager.Instancia;
        if (_gm == null) { Log("GameManager.Instancia é NULL: o jogo não inicia."); Terminar(); yield break; }
        Idioma.Definir(Lingua.Pt);
        yield return null;

        // ---------- Início ----------
        Checar(_gm.Fase == Fase.Inicio, "abre na tela inicial");
        Checar(Congelado(), "personagem parado na tela inicial");
        yield return Foto("01_inicio_pt");
        if (!_gm.TutorialLigado) _gm.AlternarTutorial();

        // ---------- Dia 1 com tutorial (Cadete) ----------
        _gm.Comecar(Modo.Cadete);
        yield return null;
        Checar(_gm.Fase == Fase.ComoJogar, "tutorial: slides antes do Boletim");
        Checar(_gm.Missao.O2 == 9 && _gm.Missao.Comida == 7 && _gm.Missao.Bateria == 1, "valores iniciais do Cadete");
        Checar(_gm.Missao.Hoje.Tarefa == Tarefas.Amostra, "tutorial: Dia 1 é a coleta de pedras");
        yield return Foto("02_como_jogar");
        _gm.PularSlides();
        yield return null;
        Checar(_gm.Fase == Fase.Boletim, "Boletim depois dos slides");
        Checar(Congelado(), "personagem parado no Boletim");
        yield return Foto("03_boletim_dia1");

        _gm.SairDoBoletim();
        yield return new WaitForSeconds(0.6f);
        Checar(_gm.Fase == Fase.AteTarefa && !Congelado(), "deslocamento: personagem liberado");
        float t0 = _gm.TempoRestante;
        yield return new WaitForSeconds(0.5f);
        Checar(_gm.TempoRestante < t0, "relógio corre no deslocamento");
        yield return Foto("04_ate_tarefa");
        var alvo = _gm.Alvo;
        Checar(alvo != null && alvo.id == "airlock", "destino da coleta de pedras é a Eclusa (airlock): " + (alvo ? alvo.id : "null"));
        yield return Teleportar(alvo);
        yield return new WaitForSeconds(0.2f);
        Checar(_gm.Proxima == alvo, "botão de ação perto da estação");
        yield return null;
        ConferirBotaoDeAcao();
        yield return Foto("05_perto_da_estacao");
        _gm.Interagir();
        yield return null;
        Checar(_gm.Fase == Fase.Escolha, "escolha da tarefa");
        Checar(Congelado(), "personagem parado na escolha");
        yield return Foto("06_escolha");
        _gm.EscolherFazer();
        yield return null;
        Checar(_gm.Fase == Fase.MinijogoInfo, "cartão de primeira vez do minijogo");
        yield return Foto("07_minijogo_info");
        _gm.ComecarMinijogo();
        yield return new WaitForSeconds(0.8f);
        Checar(_gm.Fase == Fase.Minijogo, "minijogo aberto");
        float tr = _gm.TempoRestante;
        yield return new WaitForSeconds(0.5f);
        Checar(Mathf.Approximately(tr, _gm.TempoRestante), "relógio do dia parado no minijogo");
        yield return Foto("08_minijogo_pedras");
        int o2 = _gm.Missao.O2;
        _gm.CancelarMinijogo();
        yield return null;
        Checar(_gm.Fase == Fase.TarefaConcluida, "sair do minijogo conclui a tarefa");
        Checar(_gm.Missao.O2 < o2 && _gm.Missao.Hoje.Fora, "sair da coleta cobrou oxigênio e o astronauta saiu da base");
        Log("   msg: " + _gm.Missao.Hoje.MsgTarefa.Pt);
        yield return Foto("09_concluida");
        _gm.VoltarAoHub();
        yield return null;
        Checar(_gm.Fase == Fase.AteHub, "volta ao hub");
        yield return new WaitForSeconds(0.2f);
        Checar(_gm.Alvo != null && _gm.Alvo.id == "console", "seta aponta o console do hub");
        yield return Teleportar(_gm.Alvo);
        yield return new WaitForSeconds(0.2f);
        yield return Foto("10_no_console");
        _gm.Interagir();
        yield return null;
        Checar(_gm.Fase == Fase.Distribuir, "distribuição abre no console");
        var a = _gm.Missao.Hoje.Aloc;
        Checar(a[Modulo.Ar] == 2, "sugestão do Cadete: 2 em Ar e água");
        yield return Foto("11_distribuir_pt");
        {
            var mm = _gm.Missao;
            int livresAntes = RegrasMissao.CelulasLivres(mm);
            Modulo alvoCel = RegrasMissao.PodeMais(mm, Modulo.Bateria) ? Modulo.Bateria : Modulo.Lab;
            int nAntes = a[alvoCel];
            Checar(AchaTexto(Textos.CelulasLivres.F(livresAntes).ToString()) != null, $"bandeja do rodapé mostra \"{Textos.CelulasLivres.F(livresAntes)}\"");
            _gm.Mais(alvoCel);
            yield return null;
            Checar(a[alvoCel] == nAntes + 1 && RegrasMissao.CelulasLivres(mm) == livresAntes - 1, "colocar uma célula: sai da reserva e entra no módulo");
            Checar(AchaTexto(Textos.CelulasLivres.F(livresAntes - 1).ToString()) != null, $"colocar uma célula: a bandeja passa a mostrar \"{Textos.CelulasLivres.F(livresAntes - 1)}\"");
            _gm.Menos(alvoCel);
            yield return null;
        }
        _gm.Mais(Modulo.Lab); _gm.Mais(Modulo.Lab); _gm.Mais(Modulo.Estufa);
        int lab = a[Modulo.Lab], est = a[Modulo.Estufa];
        Idioma.Alternar();
        yield return null;
        Checar(a[Modulo.Lab] == lab && a[Modulo.Estufa] == est && _gm.Fase == Fase.Distribuir, "troca de idioma mantém a distribuição");
        yield return Foto("12_distribuir_en");
        Idioma.Alternar();
        yield return null;
        _gm.ComecarODia();
        yield return null;
        Checar(_gm.Fase == Fase.DiaAcontece, "o dia acontece");
        yield return new WaitForSeconds(0.4f);
        yield return Foto("13_dia_acontece");
        yield return new WaitForSeconds(_gm.duracaoAnimacao);
        Checar(_gm.Fase == Fase.Resultado, "resultado depois da animação");
        yield return Foto("14_resultado_dia1");
        {
            var cf = CameraFoco.Instancia;
            Checar(cf != null && cf.EstadoAtual == CameraFoco.Estado.Preso, "Resultado: a câmera está na pose da base vista de fora");
            Checar(Camera.main.transform.position.y > 50f, $"Resultado: a câmera está alta, vendo a base de fora (y {Camera.main.transform.position.y:0})");
        }
        // Cenário "Oxigênio caiu": com o O2 de antes 2 acima do de agora, a linha mostra a pílula com o triângulo para baixo e o 2.
        var res = _gm.Missao.Hoje.Res;
        int o2Antes = res.O2Antes;
        res.O2Antes = Mathf.Max(0, _gm.Missao.O2) + 2;
        Idioma.Definir(Lingua.En); Idioma.Definir(Lingua.Pt); // redesenha a tela atual
        yield return null;
        bool triBaixo = false;
        foreach (var f in FindObjectsOfType<FormaUI>()) if (f.modo == FormaUI.Modo.Triangulo && Mathf.Abs(f.rotacao - 180f) < 0.1f) triBaixo = true;
        Checar(triBaixo, "oxigênio caiu 2: a pílula do Resultado mostra o triângulo para baixo");
        yield return Foto("14b_resultado_o2_caiu");
        res.O2Antes = o2Antes;
        _gm.Proximo();
        yield return null;
        Checar(_gm.Fase == Fase.Boletim && _gm.Missao.Dia == 2, "Dia 2 começa no Boletim");
        Checar(!_gm.TutorialLigado, "tutorial desliga depois do Dia 1");
        yield return new WaitForSecondsRealtime(1.0f);
        {
            var cf = CameraFoco.Instancia;
            var mov = FindObjectOfType<Movimento>();
            float dist = Vector3.Distance(Camera.main.transform.position, mov.transform.position);
            Checar(cf != null && cf.EstadoAtual == CameraFoco.Estado.Livre && dist < 15f, $"Boletim seguinte: a câmera voltou ao personagem (distância {dist:0.0} m)");
        }
        yield return TestarHud();

        // ---------- Dia 2: o relógio acaba antes da tarefa ----------
        string tarefa2 = _gm.Missao.Hoje.Tarefa;
        _gm.SairDoBoletim();
        yield return null;
        // Último minuto: o relógio fica na cor da energia (nunca vermelho) e não pulsa.
        var relogioGo = GameObject.Find("HudRelogio");
        var relogio = relogioGo != null ? relogioGo.GetComponent<UnityEngine.UI.Text>() : null;
        if (relogio != null)
        {
            _gm.DepurarTempo(100f);
            yield return new WaitForSeconds(0.3f);
            Checar(relogio.color == UiTema.Branco, "com 100 s o relógio ainda está branco");
            _gm.DepurarTempo(45f);
            yield return new WaitForSeconds(0.3f);
            Color c1 = relogio.color;
            Checar(c1 == UiTema.Energia, "faltando 45 s o relógio fica na cor da energia (" + relogio.text + ")");
            Checar(c1 != UiTema.Alerta, "o relógio do dia nunca fica vermelho");
            yield return Foto("15a_relogio_ultimo_minuto");
            yield return new WaitForSeconds(0.3f);
            Checar(relogio.color == c1, "o relógio não pisca nem pulsa");
        }
        _gm.DepurarTempo(0.3f);
        yield return new WaitForSeconds(0.6f);
        Checar(_gm.Fase == Fase.Distribuir && _gm.Missao.Hoje.TarefaResolvida && _gm.Missao.Hoje.TempoEsgotou, "tempo acabou antes da tarefa: vale o 'ficar' e abre a distribuição (" + tarefa2 + ")");
        Checar(Congelado(), "personagem parado na distribuição");
        yield return Foto("15_tempo_acabou");
        _gm.ComecarODia();
        _gm.PularAnim();
        yield return null;
        Checar(_gm.Fase == Fase.Resultado, "pular a animação");
        _gm.Proximo();
        yield return null;

        // ---------- Dias 3 a 10 ----------
        bool fotoErupcao = false;
        int dias = 0;
        while (_gm.Fase == Fase.Boletim && dias++ < 12)
        {
            var m = _gm.Missao;
            _gm.SairDoBoletim();
            yield return null;
            yield return null;
            ConferirFaixa(m.Hoje.Tarefa, "dia " + m.Dia);
            yield return Teleportar(_gm.Alvo);
            yield return new WaitForSeconds(0.15f);
            var estacao = _gm.Alvo;
            var d = Tarefas.Def(m.Hoje.Tarefa);
            Checar(estacao != null && estacao.id == d.Estacao, $"Dia {m.Dia}: destino '{d.Id}' é a estação '{d.Estacao}'");
            _gm.Interagir();
            yield return null;
            Checar(_gm.Fase == Fase.Escolha, $"Dia {m.Dia}: escolha de '{d.Id}'");
            if (d.Id == Tarefas.Vazamento)
            {
                _gm.EscolherFazer();
                yield return null;
                if (_gm.Fase == Fase.MinijogoInfo) { yield return Foto("16_vazamento_info"); _gm.ComecarMinijogo(); }
                yield return new WaitForSeconds(0.5f);
                _gm.CancelarMinijogo();
                yield return null;
                Checar(_gm.Fase == Fase.DecisaoVazamento, "vazamento: decisão depois do minijogo");
                yield return Foto("17_decisao_vazamento");
                _gm.VazamentoDepois();
                yield return null;
            }
            else _gm.EscolherFicar();
            yield return null;
            Checar(_gm.Fase == Fase.TarefaConcluida, $"Dia {m.Dia}: tarefa concluída");
            _gm.VoltarAoHub();
            yield return null;
            yield return Teleportar(_gm.Alvo);
            yield return new WaitForSeconds(0.15f);
            _gm.Interagir();
            yield return null;
            Checar(_gm.Fase == Fase.Distribuir, $"Dia {m.Dia}: distribuição");
            if (m.Vazamento) { _gm.Mais(Modulo.Conserto); _gm.Mais(Modulo.Conserto); }
            while (m.Hoje.Aloc[Modulo.Ar] < 2 && RegrasMissao.PodeMais(m, Modulo.Ar)) _gm.Mais(Modulo.Ar);
            var al = m.AlertaDe(m.Dia);
            if (al != null && al.Forte) { _gm.Mais(Modulo.Abrigo); _gm.Mais(Modulo.Abrigo); }
            for (int i = 0; i < 2; i++) _gm.Mais(Modulo.Estufa);
            for (int i = 0; i < 3; i++) _gm.Mais(Modulo.Lab);
            while (RegrasMissao.CelulasLivres(m) > 0 && RegrasMissao.PodeMais(m, Modulo.Bateria)) _gm.Mais(Modulo.Bateria);
            if (m.Vazamento && !fotoErupcao) yield return Foto("18_distribuir_com_conserto");
            _gm.ComecarODia();
            _gm.PularAnim();
            yield return null;
            var r = m.Hoje.Res;
            Log($"   Dia {m.Dia}: tarefa={d.Id} O2={m.O2} Rad={m.Rad} Bat={m.Bateria} Comida={m.Comida} Estrelas={m.Estrelas} erupção={(r.Erupcao != null ? r.Erupcao.Classe : "-")} falso={r.FalsoAlarme} fim={r.Fim}");
            if (r.Erupcao != null && !fotoErupcao)
            {
                fotoErupcao = true;
                yield return Foto("19_resultado_erupcao");
                bool achou = false;
                foreach (var b in FindObjectsOfType<BotaoLink>()) if (b.url == r.Erupcao.Url) achou = true;
                Checar(achou, "cartão da erupção tem o link do registro certo: " + r.Erupcao.Url);
                bool etiqueta = false, fonte = false;
                foreach (var t in FindObjectsOfType<UnityEngine.UI.Text>())
                {
                    if (t.text == Textos.EtiquetaArquivado.ToString().ToUpperInvariant()) etiqueta = true;
                    if (t.text == Textos.FonteDonki.ToString()) fonte = true;
                }
                Checar(etiqueta, "cartão da erupção mostra a etiqueta de dado arquivado");
                Checar(fonte, "cartão da erupção mostra a fonte NASA DONKI com a data da consulta");
            }
            _gm.Proximo();
            yield return null;
        }
        Checar(_gm.Fase == Fase.Relatorio, "relatório no fim da missão");
        yield return Foto("20_relatorio");
        _gm.AbrirCreditos();
        yield return null;
        Checar(_gm.Fase == Fase.Creditos, "créditos e fontes");
        bool licenca = false;
        foreach (var t in FindObjectsOfType<UnityEngine.UI.Text>())
            if (t.text.Contains("SIL Open Font License 1.1") && t.text.Contains("Barlow Semi Condensed") && t.text.Contains("Nunito")) licenca = true;
        Checar(licenca, "créditos citam as duas fontes (Barlow Semi Condensed e Nunito) e a licença OFL 1.1");
        yield return Foto("21_creditos");
        _gm.FecharCreditos();
        yield return null;
        Checar(_gm.Fase == Fase.Relatorio, "voltar dos créditos ao relatório");
        _gm.JogarDeNovo();
        yield return null;
        Checar(_gm.Fase == Fase.Inicio, "jogar de novo volta ao início");
        Idioma.Definir(Lingua.En);
        yield return null;
        yield return Foto("22_inicio_en");
        yield return TestarMovimentoReduzido();

        // ---------- Menu de teste dos minijogos ----------
        _gm.AbrirMenuMinijogos();
        yield return null;
        Checar(_gm.Fase == Fase.MenuMinijogos, "menu de minijogos");
        yield return Foto("23_menu_minijogos");
        Idioma.Definir(Lingua.Pt);
        foreach (var id in Tarefas.Todas)
        {
            _gm.TestarMinijogo(id);
            yield return new WaitForSeconds(0.7f);
            Checar(_gm.Fase == Fase.Minijogo, "minijogo de teste aberto: " + id);
            yield return Foto("24_mini_" + id);
            _gm.CancelarMinijogo();
            yield return null;
            if (_gm.Fase == Fase.DecisaoVazamento) _gm.VazamentoReforcar();
            yield return null;
            Checar(_gm.Fase == Fase.TarefaConcluida, "minijogo de teste termina: " + id);
            Log("   " + id + ": " + _gm.Missao.Hoje.MsgTarefa.Pt);
        }
        _gm.JogarDeNovo();
        yield return null;
        yield return Extras();
        Terminar();
    }

    // =====================================================================
    // Cenários que a missão normal não garante: estações, console, pausa, relógio depois da tarefa e no
    // vazamento, saída bloqueada, luz por fase, fins forçados, minijogo até o fim do tempo e minijogo já visto.
    // =====================================================================

    static Erupcao ErupcaoX() => new Erupcao { Id = "probe-X", Data = "2017-09-06", Classe = "X9.3", Intensidade = 2, Url = "https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/13010/-1" };

    PresetLuz? Luz() { var l = FindObjectOfType<BaseLightingController>(); return l ? l.PresetAtual : (PresetLuz?)null; }

    Estacao EstacaoPorId(string id) { foreach (var e in FindObjectsOfType<Estacao>()) if (e.id == id) return e; return null; }

    /// <summary>Do Boletim até a distribuição pelo fim do tempo, com a tarefa escolhida.</summary>
    IEnumerator PorTempoAteDistribuir(string tarefa)
    {
        _gm.Missao.Hoje.Tarefa = tarefa;
        _gm.SairDoBoletim();
        yield return null;
        _gm.DepurarTempo(0.1f);
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator FimForcado(Fim esperado, System.Action<Missao> preparar, string foto)
    {
        _gm.Comecar(Modo.Comandante);
        yield return null;
        yield return PorTempoAteDistribuir(Tarefas.Upload);
        var m = _gm.Missao;
        m.Hoje.Aloc.Zerar();
        preparar(m);
        _gm.ComecarODia();
        _gm.PularAnim();
        yield return null;
        Checar(m.Fim == esperado, $"fim forçado: {esperado} (veio {m.Fim})");
        _gm.Proximo();
        yield return null;
        Checar(_gm.Fase == Fase.Relatorio, "relatório depois do fim forçado " + esperado);
        var titulo = RegrasMissao.TituloFim(m).Pt + RegrasMissao.SubtituloFim(m).Pt;
        Checar(!titulo.ToUpperInvariant().Contains("GAME OVER") && (esperado == Fim.Completa || titulo.Contains("interrompida")), "título sem GAME OVER: " + RegrasMissao.TituloFim(m).Pt);
        yield return Foto(foto);
        _gm.JogarDeNovo();
        yield return null;
    }

    IEnumerator Extras()
    {
        Idioma.Definir(Lingua.Pt);
        // Segunda partida: sem tutorial, direto no Boletim, luz de dia.
        _gm.Comecar(Modo.Comandante);
        yield return null;
        Checar(_gm.Fase == Fase.Boletim, "segunda partida começa sem os slides do tutorial");
        Checar(!_gm.TutorialAtivo, "segunda partida sem dicas do tutorial");
        var luz = Luz();
        if (luz.HasValue) Checar(luz.Value == PresetLuz.Dia, "luz de Dia no Boletim");
        var m = _gm.Missao;

        // Só a estação da tarefa ativa; console bloqueado antes da tarefa; rótulo no idioma ativo.
        m.Hoje.Tarefa = Tarefas.Agua;
        _gm.SairDoBoletim();
        yield return null;
        int ativas = 0; string quais = "";
        foreach (var e in FindObjectsOfType<Estacao>()) if (e.Ativa) { ativas++; quais += e.id + " "; }
        Checar(ativas == 1 && quais.Trim() == "reciclador", "só a estação da tarefa fica ativa: " + quais);
        var rec = EstacaoPorId("reciclador");
        var rotulo = rec.transform.Find("Visual/Rotulo").GetComponent<TextMesh>();
        Checar(rotulo.text == Tarefas.Def(Tarefas.Agua).Nome.Pt, "rótulo da estação em português: " + rotulo.text);
        Idioma.Alternar();
        yield return null;
        Checar(rotulo.text == Tarefas.Def(Tarefas.Agua).Nome.En && _gm.Fase == Fase.AteTarefa, "rótulo troca para inglês no deslocamento: " + rotulo.text);
        yield return Foto("25_deslocamento_en");
        Idioma.Alternar();
        yield return Teleportar(EstacaoPorId("console"));
        yield return new WaitForSeconds(0.2f);
        Checar(_gm.Proxima == null, "console não aceita interação antes da tarefa");
        _gm.Interagir();
        yield return null;
        Checar(_gm.Fase == Fase.AteTarefa, "interagir no console antes da tarefa não muda de fase");

        // Pausa para o relógio.
        _gm.Pausar();
        float tp = _gm.TempoRestante;
        yield return new WaitForSecondsRealtime(0.5f);
        Checar(_gm.Pausado && Mathf.Approximately(tp, _gm.TempoRestante) && Congelado(), "pausa para o relógio e o personagem");
        yield return Foto("26_pausa");
        yield return TestarPausa();
        _gm.Retomar();
        yield return null;
        Checar(!_gm.Pausado && Time.timeScale == 1f, "continuar depois da pausa");

        // Tempo acaba depois da tarefa: distribuição sem custo extra.
        yield return Teleportar(rec);
        yield return new WaitForSeconds(0.2f);
        _gm.Interagir();
        yield return null;
        _gm.EscolherFicar();
        yield return null;
        _gm.DepurarTempo(0.1f);
        yield return new WaitForSeconds(0.3f);
        Checar(_gm.Fase == Fase.Distribuir && !m.Hoje.TempoEsgotou, "tempo acaba depois da tarefa: distribuição sem custo extra");

        // Dia com erupção: luz de Erupção.
        m.Alertas[m.Dia] = new Alerta { Erupcao = ErupcaoX(), Forte = true };
        _gm.ComecarODia();
        yield return new WaitForSeconds(0.3f);
        luz = Luz();
        if (luz.HasValue) Checar(luz.Value == PresetLuz.Erupcao, "luz de Erupção no dia da erupção");
        yield return Foto("27_luz_erupcao");
        _gm.PularAnim();
        yield return null;
        _gm.Proximo();
        yield return null;
        luz = Luz();
        if (luz.HasValue) Checar(luz.Value == PresetLuz.Dia, "luz volta para Dia no Boletim seguinte");

        // Relógio acaba antes do vazamento: vira "deixar para depois".
        yield return PorTempoAteDistribuir(Tarefas.Vazamento);
        Checar(_gm.Fase == Fase.Distribuir && m.Vazamento && RegrasMissao.ModulosVisiveis(m).Contains(Modulo.Conserto), "tempo acaba antes do vazamento: vazamento aberto e Conserto na distribuição");
        yield return Foto("28_distribuir_conserto");
        m.Alertas.Remove(m.Dia);
        _gm.ComecarODia();
        yield return new WaitForSeconds(0.3f);
        luz = Luz();
        if (luz.HasValue) Checar(luz.Value == PresetLuz.Noite, "luz de Noite no dia sem erupção");
        _gm.PularAnim();
        yield return null;
        _gm.Proximo();
        yield return null;

        // Saída bloqueada com O2 1, num dia com alerta (aviso de que quem sai fica sem o abrigo).
        m.Hoje.Tarefa = Tarefas.Amostra;
        m.O2 = 1;
        m.Alertas[m.Dia] = new Alerta { Erupcao = null, Forte = false };
        _gm.SairDoBoletim();
        yield return null;
        yield return Teleportar(_gm.Alvo);
        yield return new WaitForSeconds(0.2f);
        _gm.Interagir();
        yield return null;
        yield return Foto("29_saida_bloqueada");
        _gm.EscolherFazer();
        yield return null;
        Checar(_gm.Fase == Fase.Escolha, "com O2 1 a saída fica bloqueada");
        _gm.JogarDeNovo();
        yield return null;

        // Fins forçados (atalho de depuração do editor, via esta sonda).
        yield return FimForcado(Fim.Oxigenio, x => { x.O2 = 1; }, "30_fim_oxigenio");
        yield return FimForcado(Fim.Radiacao, x => { x.Rad = 9; x.O2 = 8; x.Hoje.Aloc[Modulo.Ar] = 2; x.Alertas[x.Dia] = new Alerta { Erupcao = ErupcaoX(), Forte = true }; }, "31_fim_radiacao");
        yield return FimForcado(Fim.Comida, x => { x.Comida = 0; x.O2 = 8; x.Hoje.Aloc[Modulo.Ar] = 2; x.Alertas.Remove(x.Dia); }, "32_fim_comida");

        // Minijogo até acabar o tempo: termina com a nota parcial, sem recomeçar.
        _gm.AbrirMenuMinijogos();
        _gm.DefinirModoTeste(Modo.Comandante);
        _gm.TestarMinijogo(Tarefas.Filtros);
        yield return new WaitForSeconds(Tarefas.Def(Tarefas.Filtros).Limite + 1.6f);
        Checar(_gm.Fase == Fase.TarefaConcluida, "minijogo termina sozinho quando o tempo acaba");
        Log("   filtros no fim do tempo: " + _gm.Missao.Hoje.MsgTarefa.Pt);
        _gm.JogarDeNovo();
        yield return null;

        // Minijogo já visto: depois de recarregar a cena, abre sem o cartão.
        DontDestroyOnLoad(gameObject);
        _gm.Reiniciar();
        yield return new WaitForSeconds(1.5f);
        _gm = GameManager.Instancia;
        Checar(_gm != null && _gm.MinijogoVisto(Tarefas.Amostra), "minijogo visto continua salvo depois de recarregar");
        _gm.Comecar(Modo.Comandante);
        yield return null;
        _gm.Missao.Hoje.Tarefa = Tarefas.Amostra;
        _gm.Missao.O2 = 8;
        _gm.SairDoBoletim();
        yield return null;
        yield return Teleportar(_gm.Alvo);
        yield return new WaitForSeconds(0.2f);
        _gm.Interagir();
        yield return null;
        _gm.EscolherFazer();
        yield return null;
        Checar(_gm.Fase == Fase.Minijogo, "segunda abertura do minijogo começa sem o cartão");
        _gm.CancelarMinijogo();
        yield return null;
        _gm.JogarDeNovo();
        yield return null;
    }

    void Terminar()
    {
        _sb.AppendLine($"\nRESUMO: {_ok} ok, {_falhas} falhas");
        File.WriteAllText(Path.Combine(_raiz, "Temp/probe_jogo.txt"), _sb.ToString());
        File.WriteAllText(Path.Combine(_raiz, "Temp/pedido_parar_play.txt"), "1");
    }
}
