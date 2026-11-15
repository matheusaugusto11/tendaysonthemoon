using System.Collections;
using System.IO;
using System.Text;
using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Só diagnóstico do Editor (tarefas 2.2 a 2.5 da mudança interface-e-minijogos): com Temp/tema_teste.flag, monta uma tela
/// de teste do tema por cima do jogo (fontes, cores, formas, botões), confere os glifos, captura a tela em
/// Temp/tema_capturas/ e escreve Temp/tema_teste.txt. No fim pede para sair do Play (Temp/pedido_parar_play.txt).
/// Não faz nada fora do Editor nem sem o arquivo.
/// </summary>
public class TemaTeste : MonoBehaviour
{
    static bool Pedido => Application.isEditor && File.Exists(Path.Combine(Application.dataPath, "../Temp/tema_teste.flag"));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (!Pedido) return;
        new GameObject("TemaTeste").AddComponent<TemaTeste>();
    }

    readonly StringBuilder _sb = new StringBuilder();
    RectTransform _raiz;
    string _raizProjeto, _pasta;

    void Log(string s) => _sb.AppendLine(s);

    /// <summary>Progresso em arquivo, gravado na hora: se o Play travar, dá para ver onde.</summary>
    void Passo(string s) => File.AppendAllText(Path.Combine(_raizProjeto, "Temp/tema_progresso.txt"), $"[{Time.realtimeSinceStartup:0.0}s t={Time.time:0.0} escala={Time.timeScale}] {s}{'\n'}");

    IEnumerator Start()
    {
        Application.runInBackground = true; // o Editor sem foco estrangula o loop de Play
        _raizProjeto = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        File.Delete(Path.Combine(_raizProjeto, "Temp/tema_progresso.txt"));
        Passo("início");
        _pasta = Path.Combine(_raizProjeto, "Temp/tema_capturas");
        Directory.CreateDirectory(_pasta);
        foreach (var f in Directory.GetFiles(_pasta, "*.png")) File.Delete(f);
        Application.logMessageReceived += Capturar;
        yield return new WaitForSecondsRealtime(1.0f);
        Passo("depois da espera de 1 s");
        Log($"Tela: {Screen.width}x{Screen.height}");

        MontarCanvas(); Passo("canvas montado");
        ConferirFontes(); Passo("fontes conferidas");
        MontarFontes(); Passo("fontes montadas");
        MontarCores(); Passo("cores montadas");
        MontarBotoes(); Passo("botões montados");
        yield return null;
        yield return new WaitForSecondsRealtime(0.4f);
        Passo("vai capturar");
        Capturar("tema"); Passo("capturou");
        yield return null;
        yield return TestarBotoes(); Passo("botões testados");
        Log($"RESULTADO: {_ok} OK, {_falhas} FALHARAM");
        Terminar(); Passo("terminou");
    }

    // A janela Game do Editor é pequena e ScreenCapture com superSize travou: renderiza o canvas numa textura de 1920x1080.
    Camera _cam;
    RenderTexture _rt;

    static void NaCamadaUI(Transform t)
    {
        t.gameObject.layer = LayerMask.NameToLayer("UI");
        foreach (Transform filho in t) NaCamadaUI(filho);
    }

    void Capturar(string nome)
    {
        NaCamadaUI(_raiz); // objetos criados por código nascem na camada Default; a câmera de captura só enxerga a UI
        Canvas.ForceUpdateCanvases();
        _cam.Render();
        var antes = RenderTexture.active;
        RenderTexture.active = _rt;
        var tex = new Texture2D(_rt.width, _rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = antes;
        File.WriteAllBytes(Path.Combine(_pasta, nome + ".png"), tex.EncodeToPNG());
        Destroy(tex);
    }

    void Capturar(string msg, string stack, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert) Log($"ERRO {t}: {msg}");
        else if (t == LogType.Warning && msg.StartsWith("[UiTema]")) Log("AVISO " + msg);
    }

    void Terminar()
    {
        Application.logMessageReceived -= Capturar;
        File.WriteAllText(Path.Combine(_raizProjeto, "Temp/tema_teste.txt"), _sb.ToString());
        File.Delete(Path.Combine(_raizProjeto, "Temp/tema_teste.flag"));
        File.WriteAllText(Path.Combine(_raizProjeto, "Temp/pedido_parar_play.txt"), "1");
    }

    // ---------- Montagem ----------

    void MontarCanvas()
    {
        var go = new GameObject("UI_TemaTeste", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        _rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var camGo = new GameObject("CamTemaTeste");
        _cam = camGo.AddComponent<Camera>();
        _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = UiTema.Pagina;
        _cam.cullingMask = 1 << LayerMask.NameToLayer("UI"); _cam.orthographic = true; _cam.targetTexture = _rt; _cam.enabled = false;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = _cam;
        canvas.planeDistance = 1f;
        // Textura de 1920x1080 exatos: sem CanvasScaler, 1 unidade do canvas = 1 pixel = 1 unidade do referencial.
        go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        _raiz = (RectTransform)go.transform;
        var fundo = Rect("Fundo", _raiz, Vector2.zero, new Vector2(1920, 1080), UiTema.Pagina);
        fundo.anchorMin = Vector2.zero; fundo.anchorMax = Vector2.one; fundo.offsetMin = fundo.offsetMax = Vector2.zero;
    }

    /// <summary>Retângulo com canto superior esquerdo em (x, y) do referencial (y para baixo).</summary>
    RectTransform Rect(string nome, Transform pai, Vector2 pos, Vector2 tam, Color cor)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(pos.x, -pos.y);
        rt.sizeDelta = tam;
        var im = go.GetComponent<Image>(); im.color = cor; im.raycastTarget = false;
        return rt;
    }

    Text Texto(string s, Font fonte, int tamanho, Color cor, Vector2 pos, Vector2 tam, TextAnchor ancora = TextAnchor.UpperLeft)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(_raiz, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(pos.x, -pos.y);
        rt.sizeDelta = tam;
        var t = go.GetComponent<Text>();
        t.font = fonte; t.fontSize = tamanho; t.color = cor; t.text = s; t.alignment = ancora;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // ---------- 2.2: fontes ----------

    const string Amostra = "Ação, coração, não deu desta vez, órgão, maçã";

    void ConferirFontes()
    {
        var teste = Amostra + " ÁÀÂÃÉÊÍÓÔÕÚÇ · −6 0123456789";
        foreach (var (nome, fonte) in new[] { ("Barlow Bold", UiTema.BarlowBold), ("Barlow SemiBold", UiTema.BarlowSemi), ("Nunito Bold", UiTema.Nunito) })
        {
            if (fonte == null) { Log($"FONTE {nome}: NULL"); continue; }
            fonte.RequestCharactersInTexture(teste, 40, FontStyle.Normal);
            var faltam = new StringBuilder();
            foreach (var c in teste) if (!fonte.HasCharacter(c)) faltam.Append(c);
            Log($"FONTE {nome}: dinâmica={fonte.dynamic} nome interno={fonte.name} faltando=\"{faltam}\"");
        }
        // O subscrito ₂ só existe na Nunito (a Barlow não tem).
        UiTema.Nunito.RequestCharactersInTexture("₂", 40, FontStyle.Normal);
        UiTema.BarlowBold.RequestCharactersInTexture("₂", 40, FontStyle.Normal);
        Log($"SUBSCRITO ₂: Nunito={UiTema.Nunito.HasCharacter('₂')} Barlow={UiTema.BarlowBold.HasCharacter('₂')}");
    }

    void MontarFontes()
    {
        Texto("TEMA · FONTES", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(48, 32), new Vector2(900, 30));
        float y = 80;
        foreach (var p in new[] { Papel.TituloTela, Papel.TituloPainel, Papel.Numero, Papel.Corpo, Papel.Rotulo })
        {
            var f = UiTema.Fonte(p);
            Texto($"{p} · {f.name} · {UiTema.Tamanho(p)}", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(48, y), new Vector2(1000, 30));
            y += 32;
            string s = p == Papel.Rotulo ? UiTema.Caixa("Dado arquivado · não é ao vivo · maçã") : Amostra;
            Texto(s, f, UiTema.Tamanho(p), UiTema.Branco, new Vector2(48, y), new Vector2(1300, 80));
            y += UiTema.Tamanho(p) + 30;
        }
        // Números de medidor e o "O₂" na Nunito.
        Texto("O₂ −6 · 3:12 · 2/4 · 0:41", UiTema.Nunito, 36, UiTema.Branco, new Vector2(48, y), new Vector2(900, 50));
        Texto("O₂ −6 · 3:12 · 2/4 · 0:41", UiTema.BarlowBold, 36, UiTema.Branco, new Vector2(760, y), new Vector2(900, 50));
        Texto("Nunito Bold (tem ₂)", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(48, y + 48), new Vector2(500, 30));
        Texto("Barlow Bold (sem ₂: a conferir no desenho)", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(760, y + 48), new Vector2(700, 30));
    }

    // ---------- 2.5: botões ----------

    int _ok, _falhas;

    void Checar(bool cond, string o)
    {
        if (cond) _ok++; else _falhas++;
        Log((cond ? "OK     " : "FALHOU ") + o);
    }

    BotaoFisico Botao(BotaoFisico.Estilo e, string texto, string icone, float x, float y, Vector2? tam = null)
    {
        var b = BotaoFisico.Criar(_raiz, e, texto, icone, null, UiTema.Ar);
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        if (tam.HasValue) rt.sizeDelta = tam.Value;
        return b;
    }

    void MontarBotoes()
    {
        Texto("BOTÕES · NORMAL · PRESSIONADO · SECUNDÁRIO · DESATIVADO · FOCO · ESCOLHA", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(48, 690), new Vector2(1500, 30));
        Botao(BotaoFisico.Estilo.Principal, "Começar o dia", "seta_tinta", 48, 740);
        var p = Botao(BotaoFisico.Estilo.Principal, "Começar o dia", "seta_tinta", 488, 740); p.ForcarPressionado(true);
        Botao(BotaoFisico.Estilo.Secundario, "Como jogar", "ajuda", 928, 740);
        var d = Botao(BotaoFisico.Estilo.Secundario, "Sair da base", "ajuda", 1226, 740, new Vector2(400, 96)); d.Desativar("Oxigênio em 1");
        var f = Botao(BotaoFisico.Estilo.Secundario, "Créditos e fontes", "fonte", 48, 900); f.ForcarFoco(true);
        Botao(BotaoFisico.Estilo.Escolha, "Sair para buscar pedras\n<size=30>O₂ −1 a −3 · ciência até +5</size>", null, 488, 880, new Vector2(760, 150));
        var pr = Botao(BotaoFisico.Estilo.Principal, "Pronto", "seta_tinta", 1300, 900, new Vector2(400, 104)); d.interactable = false;
        pr.Desativar("Falta uma célula");
    }

    static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

    IEnumerator TestarBotoes()
    {
        Entrada.GarantirEventSystem();
        BotaoFisico.IgnorarEntradaReal = true; // o mouse de quem está no computador não pode mudar o resultado do teste
        var es = UnityEngine.EventSystems.EventSystem.current;
        Checar(es != null, "EventSystem presente");
        int cliques = 0, cliquesDes = 0;
        var b = BotaoFisico.Criar(_raiz, BotaoFisico.Estilo.Principal, "Começar o dia", "seta_tinta", () => cliques++, null, "Teste");
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(1500, -20);
        var des = BotaoFisico.Criar(_raiz, BotaoFisico.Estilo.Secundario, "Sair da base", "ajuda", () => cliquesDes++, null, "Teste_desativado");
        ((RectTransform)des.transform).anchoredPosition = new Vector2(1500, -200);
        des.Desativar("Oxigênio em 1");
        yield return null;

        // Mouse ou dedo: o mesmo PointerEventData.
        Rect r0 = rt.rect; float y0 = b.Face.anchoredPosition.y; Color cor0 = b.Face.GetComponent<FormaUI>().color;
        var ped = new UnityEngine.EventSystems.PointerEventData(es) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left, pointerId = -1 };
        UnityEngine.EventSystems.ExecuteEvents.Execute(b.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
        float t0 = Time.realtimeSinceStartup;
        float tAfundou = -1f;
        while (Time.realtimeSinceStartup - t0 < 0.4f) { if (tAfundou < 0f && b.Afundado) tAfundou = Time.realtimeSinceStartup - t0; yield return null; }
        float y1 = b.Face.anchoredPosition.y; Color cor1 = b.Face.GetComponent<FormaUI>().color;
        Checar(rt.rect == r0, "layout não pula: o retângulo do botão é o mesmo pressionado");
        Checar(Mathf.Abs(y1 - (y0 - UiTema.Sombra_Y)) < 0.01f, $"pressionado: a face desce {UiTema.Sombra_Y} (de {y0} para {y1})");
        Checar(b.Sombra.color.a < 0.01f, $"pressionado: a sombra some (alfa {b.Sombra.color.a:0.00})");
        Checar(Lum(cor1) < Lum(cor0), $"pressionado: a face escurece um degrau (luminância {Lum(cor0):0.000} → {Lum(cor1):0.000})");
        Checar(tAfundou >= 0f && tAfundou <= 0.2f, $"afunda em {(tAfundou * 1000f):0} ms (alvo 80 ms; o resto é granularidade do quadro)");

        int tocados0 = SonsUI.Tocados;
        UnityEngine.EventSystems.ExecuteEvents.Execute(b.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
        UnityEngine.EventSystems.ExecuteEvents.Execute(b.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        Checar(cliques == 1, "clique do mouse ou do dedo chama a ação (1 chamada)");
        Checar(!SonsUI.Ligado || SonsUI.Tocados == tocados0 + 1, $"o clique toca o som de toque (Tocados {tocados0} → {SonsUI.Tocados}, último \"{SonsUI.Ultimo}\")");
        t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 0.3f) yield return null;
        Checar(Mathf.Abs(b.Face.anchoredPosition.y - y0) < 0.01f && b.Sombra.color.a > 0.99f, "solto: a face e a sombra voltam");
        Checar(!b.FocoVisivel, "clique de mouse ou dedo não mostra a moldura de foco");

        // Controle ou teclado: seleção e envio sem ponteiro.
        es.SetSelectedGameObject(null);
        yield return null;
        BotaoFisico.NavegacaoPorTeclado = false; // quem joga de mouse: o jogo seleciona botões por código e a moldura não deve aparecer
        es.SetSelectedGameObject(b.gameObject, new UnityEngine.EventSystems.BaseEventData(es));
        yield return null;
        Checar(!b.FocoVisivel, "seleção por código com o mouse em uso não mostra a moldura de foco");
        BotaoFisico.NavegacaoPorTeclado = true; // teclado ou controle em uso
        yield return null;
        Checar(b.FocoVisivel, "com teclado ou controle em uso, a seleção mostra a moldura de foco");
        BotaoFisico.NavegacaoPorTeclado = false;
        yield return null;
        Checar(!b.FocoVisivel, "voltar ao mouse esconde a moldura");
        BotaoFisico.NavegacaoPorTeclado = true;
        yield return null;
        UnityEngine.EventSystems.ExecuteEvents.Execute(b.gameObject, new UnityEngine.EventSystems.BaseEventData(es), UnityEngine.EventSystems.ExecuteEvents.submitHandler);
        Checar(cliques == 2, "enviar pelo controle chama a mesma ação (2 chamadas)");
        es.SetSelectedGameObject(null);
        yield return null;
        Checar(!b.FocoVisivel, "tirar a seleção esconde a moldura");

        // Desativado: cinza, cadeado, motivo escrito, sem ação.
        UnityEngine.EventSystems.ExecuteEvents.Execute(des.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
        UnityEngine.EventSystems.ExecuteEvents.Execute(des.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
        UnityEngine.EventSystems.ExecuteEvents.Execute(des.gameObject, ped, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        yield return null;
        Checar(cliquesDes == 0, "botão desativado não chama a ação");
        Checar(des.Rotulo.text.Contains("Oxigênio em 1"), "botão desativado escreve o motivo");
        Checar(des.GetComponentsInChildren<UnityEngine.UI.Image>(true).Length > 0 && FindIcone(des) == UiTema.Icone("cadeado"), "botão desativado mostra o cadeado");
        Checar(!des.Afundado, "botão desativado não afunda");
        BotaoFisico.IgnorarEntradaReal = false; BotaoFisico.NavegacaoPorTeclado = false;
    }

    static Sprite FindIcone(BotaoFisico b)
    {
        var t = b.Face.Find("Icone");
        return t != null ? t.GetComponent<UnityEngine.UI.Image>().sprite : null;
    }

    // ---------- Cores ----------

    void MontarCores()
    {
        Texto("CORES", UiTema.BarlowSemi, 24, UiTema.Apoio, new Vector2(1380, 32), new Vector2(400, 30));
        var itens = new (string, Color)[]
        {
            ("Aço", UiTema.Aco), ("Aço claro", UiTema.AcoClaro), ("Branco", UiTema.Branco), ("Tinta", UiTema.Tinta),
            ("Ar / O₂", UiTema.Ar), ("Estufa", UiTema.Estufa), ("Abrigo / Rad.", UiTema.Abrigo), ("Laboratório", UiTema.Lab),
            ("Energia", UiTema.Energia), ("Alerta", UiTema.Alerta), ("Terra", UiTema.Terra), ("Água usada", UiTema.AguaUsada),
        };
        for (int i = 0; i < itens.Length; i++)
        {
            float x = 1380 + (i % 2) * 260, y = 80 + (i / 2) * 76;
            Rect("Amostra", _raiz, new Vector2(x, y), new Vector2(60, 60), itens[i].Item2);
            Texto(itens[i].Item1, UiTema.Nunito, 24, UiTema.Branco, new Vector2(x + 72, y + 14), new Vector2(180, 30));
        }
    }
}
