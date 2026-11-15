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

    IEnumerator Foto(string nome)
    {
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
        _gm.Proximo();
        yield return null;
        Checar(_gm.Fase == Fase.Boletim && _gm.Missao.Dia == 2, "Dia 2 começa no Boletim");
        Checar(!_gm.TutorialLigado, "tutorial desliga depois do Dia 1");

        // ---------- Dia 2: o relógio acaba antes da tarefa ----------
        string tarefa2 = _gm.Missao.Hoje.Tarefa;
        _gm.SairDoBoletim();
        yield return null;
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
            }
            _gm.Proximo();
            yield return null;
        }
        Checar(_gm.Fase == Fase.Relatorio, "relatório no fim da missão");
        yield return Foto("20_relatorio");
        _gm.AbrirCreditos();
        yield return null;
        Checar(_gm.Fase == Fase.Creditos, "créditos e fontes");
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
