using System;
using System.Collections.Generic;
using DezDias;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Fases de um dia e as telas fora da missão (spec ciclo-da-missao).</summary>
public enum Fase
{
    Inicio, ComoJogar, Boletim, AteTarefa, Escolha, MinijogoInfo, Minijogo, DecisaoVazamento,
    TarefaConcluida, AteHub, Distribuir, DiaAcontece, Resultado, Relatorio, Creditos, MenuMinijogos,
    /// <summary>Cartão do evento do dia (spec eventos-do-dia). Relógio parado.</summary>
    Evento,
    /// <summary>Corrida até o abrigo depois do alarme de partículas. Relógio correndo.</summary>
    CorridaAbrigo,
}

/// <summary>
/// Máquina de fases do jogo (design D2 da mudança fluxo-web-no-3d). As regras ficam no assembly DezDias.Regras;
/// aqui só se troca de fase, libera ou congela o personagem, conta o relógio do dia, ativa as estações e chama a UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [Header("Referências (achadas sozinhas se vazias)")]
    public Movimento jogador;

    [Header("Dia")]
    [Tooltip("Segundos de cada dia, do Boletim até chegar ao console do hub (300 = 5 min). Baixe para testar o fim do tempo.")]
    public float duracaoDia = 300f;
    [Tooltip("Segundos da fase 'o dia acontece' antes do resultado.")]
    public float duracaoAnimacao = 3.6f;

    [Header("Console de energia do hub (criado se a cena não tiver uma Estacao 'console')")]
    public Vector3 posicaoConsole = Vector3.zero;
    public float raioConsole = 3.5f;

    [Header("Depuração")]
    [Tooltip("Abre o menu de teste dos minijogos, como ?minigames na build WebGL.")]
    public bool abrirMenuMinijogos;
    [Tooltip("Id de um evento do dia (particulas, cme, tremor, micrometeorito, poeira_eclusa, frio, gelo, terra, sono, saudade, fungo) para forçar em todos os dias. Vazio = sorteio normal.")]
    public string forcarEvento = "";
    [Tooltip("Dias sem evento: só para a sonda e depuração do fluxo antigo.")]
    public bool semEventos;

    public Fase Fase { get; private set; } = Fase.Inicio;
    public Missao Missao { get; private set; }
    /// <summary>Modo do menu de teste dos minijogos (sem HUD e sem dias).</summary>
    public bool Teste { get; private set; }
    public Modo ModoTeste { get; private set; } = Modo.Cadete;
    public bool Pausado { get; private set; }
    public float TempoRestante { get; private set; }
    public int Slide { get; private set; }
    public int PassoAnim { get; private set; }
    public bool TutorialLigado { get; private set; } = true;
    public Estacao Alvo { get; private set; }
    public Estacao Proxima { get; private set; }

    /// <summary>Dicas do tutorial: só no Dia 1 da partida que começou com o tutorial ligado.</summary>
    public bool TutorialAtivo => Missao != null && Missao.Tutorial && Missao.Dia == 1 && !Teste;

    static bool FaseComRelogio(Fase f) => f == Fase.AteTarefa || f == Fase.Escolha || f == Fase.DecisaoVazamento || f == Fase.TarefaConcluida || f == Fase.AteHub
        || f == Fase.CorridaAbrigo;
    /// <summary>Fases em que o astronauta anda (deslocamento e corrida até o abrigo).</summary>
    static bool FaseComMovimento(Fase f) => f == Fase.AteTarefa || f == Fase.AteHub || f == Fase.CorridaAbrigo;
    /// <summary>O relógio do dia anda (parado no minijogo e na pausa).</summary>
    public bool RelogioCorre => !Pausado && !Teste && Missao != null && FaseComRelogio(Fase);

    UIJogo _ui;
    Minigame _minigame;
    BaseLightingController _luz;
    readonly Dictionary<string, Estacao> _estacoes = new Dictionary<string, Estacao>();
    Estacao _console;
    readonly List<Erupcao> _erupcoes = new List<Erupcao>();
    readonly List<Cme> _cmes = new List<Cme>();
    /// <summary>Fase para onde o jogo volta depois do evento (Distribuir = abrir a distribuição).</summary>
    Fase _retornoEvento = Fase.AteTarefa;
    float _tempoNaIda;
    bool _avisouSemCama;

    /// <summary>Segundos que faltam na corrida até o abrigo.</summary>
    public float ContagemCorrida { get; private set; }
    /// <summary>O relógio do dia já acabou (os custos em tempo viram 1 célula).</summary>
    public bool RelogioZerado => TempoRestante <= 0f;
    /// <summary>O evento de hoje ainda não foi resolvido.</summary>
    public bool EventoPendente => !Teste && Missao?.Hoje != null && !string.IsNullOrEmpty(Missao.Hoje.Evento) && !Missao.Hoje.EventoResolvido;
    readonly System.Random _rng = new System.Random();
    readonly HashSet<string> _vistos = new HashSet<string>();
    Fase _retornoComoJogar = Fase.Inicio, _retornoCreditos = Fase.Inicio;
    float _animT;
    string _tarefaTeste;

    const string ChaveVistos = "tdm_seen", ChaveTutorial = "tdm_tut";

    // =====================================================================
    // Ciclo de vida
    // =====================================================================

    void Awake()
    {
        Instancia = this;
        Idioma.Definir(Navegador.LinguaInicial());
        CarregarErupcoes();
        CarregarPreferencias();
    }

    void Start()
    {
        if (!jogador) jogador = FindObjectOfType<Movimento>();
        _luz = FindObjectOfType<BaseLightingController>();
        RegistrarEstacoes();
        _ui = new UIJogo(this);
        Idioma.Mudou += AoMudarIdioma;
        if (abrirMenuMinijogos || Navegador.PediuMenuMinijogos()) AbrirMenuMinijogos();
        else MudarFase(Fase.Inicio);
    }

    void OnDestroy()
    {
        Idioma.Mudou -= AoMudarIdioma;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (_ui == null) return;
        if (PodePausar)
        {
            if (Entrada.PausaDown) { DefinirPausa(!Pausado); return; }
            if (Pausado && Entrada.VoltarDown) { Retomar(); return; }
        }
        if (Pausado) return;

        if (Fase == Fase.Minijogo)
        {
            _minigame?.Tick();
            if (Entrada.VoltarDown) CancelarMinijogo();
        }
        else if (Fase == Fase.DiaAcontece)
        {
            _animT += Time.deltaTime;
            if (PassoAnim == 0 && _animT >= 1.5f) { PassoAnim = 1; _ui.AtualizarLegenda(); }
            if (_animT >= duracaoAnimacao) IrParaResultado();
        }

        if (RelogioCorre)
        {
            TempoRestante -= Time.deltaTime;
            if (TempoRestante <= 0f) { TempoRestante = 0f; TempoAcabou(); }
        }

        if (Fase == Fase.AteTarefa && EventoPendente && Missao.Hoje.EventoNaIda)
        {
            _tempoNaIda += Time.deltaTime;
            if (_tempoNaIda >= Missao.Hoje.GatilhoSeg) { AbrirEvento(Fase.AteTarefa); _ui.AtualizarHud(); return; }
        }

        if (Fase == Fase.AteTarefa || Fase == Fase.AteHub)
        {
            AtualizarGuia();
            if (Proxima != null && Entrada.ConfirmarDown) Interagir();
        }
        else if (Fase == Fase.CorridaAbrigo)
        {
            ContagemCorrida = Mathf.Max(0f, ContagemCorrida - Time.deltaTime);
            AtualizarGuia();
            if (Proxima != null) FimCorrida(true);
            else if (ContagemCorrida <= 0f) FimCorrida(false);
        }
        _ui.AtualizarHud();
    }

    // =====================================================================
    // Dados e preferências
    // =====================================================================

    [Serializable] class RegistroFLR { public string id, eventType, startTime, classLabel, sourceUrl, retrievedOn, sepId, sepTime, sepUrl, sepRetrievedOn; public bool isTest; public int gameIntensity; }
    [Serializable] class ArquivoErupcoes { public string fonte; public RegistroFLR[] records; }
    [Serializable] class RegistroCME { public string id, startTime, sourceUrl, linkedFlr, retrievedOn; public int speed; }
    [Serializable] class ArquivoCmes { public string fonte; public RegistroCME[] records; }

    static string SoData(string iso) => string.IsNullOrEmpty(iso) || iso.Length < 10 ? iso : iso.Substring(0, 10);
    static string SoHora(string iso) => string.IsNullOrEmpty(iso) || iso.Length < 16 ? "" : iso.Substring(11, 5);

    void CarregarCmes()
    {
        _cmes.Clear();
        var arq = Resources.Load<TextAsset>("CmesDONKI");
        if (arq == null) { Debug.LogWarning("[GameManager] Resources/CmesDONKI.json não encontrado: a partida fica sem aviso de CME."); return; }
        var dados = JsonUtility.FromJson<ArquivoCmes>(arq.text);
        if (dados?.records == null) return;
        foreach (var r in dados.records)
            _cmes.Add(new Cme { Id = r.id, Data = SoData(r.startTime), Velocidade = r.speed, Url = r.sourceUrl, FlrLigado = r.linkedFlr, Consulta = r.retrievedOn });
    }

    void CarregarErupcoes()
    {
        _erupcoes.Clear();
        var arq = Resources.Load<TextAsset>("ErupcoesDONKI");
        if (arq == null) { Debug.LogError("[GameManager] Resources/ErupcoesDONKI.json não encontrado: o jogo fica sem erupções."); return; }
        var dados = JsonUtility.FromJson<ArquivoErupcoes>(arq.text);
        if (dados?.records == null) return;
        foreach (var r in dados.records)
        {
            if (r.isTest) continue;
            _erupcoes.Add(new Erupcao
            {
                Id = r.id, Classe = r.classLabel, Url = r.sourceUrl, Intensidade = r.gameIntensity,
                Data = SoData(r.startTime),
                SepId = r.sepId, SepUrl = r.sepUrl, SepConsulta = r.sepRetrievedOn,
                SepHora = string.IsNullOrEmpty(r.sepTime) ? "" : SoData(r.sepTime) + " " + SoHora(r.sepTime),
            });
        }
        CarregarCmes();
    }

    void CarregarPreferencias()
    {
        try
        {
            foreach (var id in PlayerPrefs.GetString(ChaveVistos, "").Split(','))
                if (!string.IsNullOrEmpty(id)) _vistos.Add(id);
            TutorialLigado = PlayerPrefs.GetString(ChaveTutorial, "") != "1";
        }
        catch (Exception e) { Debug.LogWarning("[GameManager] Preferências indisponíveis: " + e.Message); }
    }

    void SalvarPreferencias()
    {
        try
        {
            PlayerPrefs.SetString(ChaveVistos, string.Join(",", _vistos));
            PlayerPrefs.SetString(ChaveTutorial, TutorialLigado ? "" : "1");
            PlayerPrefs.Save();
        }
        catch (Exception e) { Debug.LogWarning("[GameManager] Não deu para salvar as preferências: " + e.Message); }
    }

    public bool MinijogoVisto(string id) => _vistos.Contains(id);

    // =====================================================================
    // Estações
    // =====================================================================

    void RegistrarEstacoes()
    {
        _estacoes.Clear();
        foreach (var e in FindObjectsOfType<Estacao>())
        {
            if (string.IsNullOrEmpty(e.id)) continue;
            if (_estacoes.ContainsKey(e.id))
            {
                Debug.LogWarning($"[GameManager] Duas estações ativas com o id '{e.id}': {_estacoes[e.id].name} e {e.name}. Uso a primeira.");
                continue;
            }
            _estacoes[e.id] = e;
        }
        if (!_estacoes.TryGetValue("console", out _console)) _console = CriarConsole();
        foreach (var d in Tarefas.Defs)
            if (!_estacoes.ContainsKey(d.Estacao))
                Debug.LogWarning($"[GameManager] A estação '{d.Estacao}' da tarefa '{d.Id}' não está na cena: a tarefa abre no console do hub.");
        Debug.Log("[GameManager] Estações: " + string.Join(", ", _estacoes.Keys) + " + console");
    }

    Estacao CriarConsole()
    {
        var go = new GameObject("Estacao_console");
        go.SetActive(false); // preenche os campos antes do Awake montar o visual
        go.transform.position = posicaoConsole;
        var e = go.AddComponent<Estacao>();
        e.id = "console";
        e.nome = Textos.Console.ToString();
        e.cor = new Color(1f, 0.82f, 0.25f);
        e.raio = raioConsole;
        e.semMaquina = true;
        go.SetActive(true);
        return e;
    }

    /// <summary>Estação da tarefa de hoje (ou o console, se a cena não tiver a estação).</summary>
    Estacao EstacaoDaTarefa()
    {
        if (Missao?.Hoje == null) return null;
        var d = Tarefas.Def(Missao.Hoje.Tarefa);
        return d != null && _estacoes.TryGetValue(d.Estacao, out var e) ? e : _console;
    }

    /// <summary>Só o destino da fase atual fica ativo: a estação da tarefa (indo) ou o console (voltando).</summary>
    void RefrescarEstacoes()
    {
        foreach (var e in _estacoes.Values) e.Definir(false, false);
        if (_console) _console.Definir(false, false);
        if (Missao == null || Teste) return;
        if (Fase == Fase.AteTarefa)
        {
            var e = EstacaoDaTarefa();
            if (e != null) { e.Definir(true, false); e.DefinirRotulo(Tarefas.Def(Missao.Hoje.Tarefa).Nome.ToString()); }
        }
        else if (Fase == Fase.AteHub && _console)
        {
            _console.Definir(true, false);
            _console.DefinirRotulo(Textos.Console.ToString());
        }
        else if (Fase == Fase.CorridaAbrigo)
        {
            var abrigo = EstacaoAbrigo();
            if (abrigo != null) { abrigo.Definir(true, false); abrigo.DefinirRotulo(Textos.Abrigo.ToString()); }
        }
    }

    /// <summary>O Dormitório (estação "cama") é o abrigo da corrida. Sem ele na cena, a corrida vai até o console do hub.</summary>
    Estacao EstacaoAbrigo()
    {
        if (_estacoes.TryGetValue("cama", out var e)) return e;
        if (!_avisouSemCama) { _avisouSemCama = true; Debug.LogWarning("[GameManager] A estação 'cama' (Dormitório) não está na cena: a corrida até o abrigo vai até o console do hub."); }
        return _console;
    }

    void AtualizarGuia()
    {
        Alvo = Fase == Fase.CorridaAbrigo ? EstacaoAbrigo() : Fase == Fase.AteHub ? _console : EstacaoDaTarefa();
        Proxima = null;
        if (Alvo != null && jogador != null && Alvo.DistanciaAte(jogador.transform.position) <= Alvo.raio) Proxima = Alvo;
        var rotulo = Fase == Fase.CorridaAbrigo ? Textos.Abrigo
            : Fase == Fase.AteHub ? Textos.BotaoDistribuir : Textos.BotaoIniciar.F(Tarefas.Def(Missao.Hoje.Tarefa).Nome);
        _ui.AtualizarGuia(Alvo, Proxima, rotulo);
    }

    /// <summary>Botão AÇÃO / tecla E / botão A perto do destino.</summary>
    public void Interagir()
    {
        if (Pausado || Missao == null) return;
        if (Fase == Fase.AteTarefa || Fase == Fase.AteHub) AtualizarGuia();
        if (Proxima == null) return;
        if (Fase == Fase.AteTarefa && EventoPendente && Missao.Hoje.EventoNaIda) AbrirEvento(Fase.Escolha);
        else if (Fase == Fase.AteTarefa) MudarFase(Fase.Escolha);
        else if (Fase == Fase.AteHub && Missao.Hoje.TarefaResolvida) AbrirDistribuicao();
    }

    // =====================================================================
    // Fluxo da missão
    // =====================================================================

    public void Comecar(Modo modo)
    {
        Teste = false;
        Missao = RegrasMissao.NovaPartida(modo, _erupcoes, _rng, TutorialLigado, _cmes);
        if (Missao.Tutorial)
        {
            IniciarDia(false);
            Slide = 0;
            _retornoComoJogar = Fase.Boletim;
            MudarFase(Fase.ComoJogar);
        }
        else IniciarDia(true);
    }

    void IniciarDia(bool mostrar)
    {
        RegrasMissao.IniciarDia(Missao);
        ForcarEventoDeDepuracao();
        if (semEventos) Missao.Hoje.EventoResolvido = true;
        TempoRestante = Mathf.Max(1f, duracaoDia);
        _tempoNaIda = 0f;
        DefinirLuz(PresetLuz.Dia);
        if (mostrar) MudarFase(Fase.Boletim);
    }

    /// <summary>Campo de depuração forcarEvento (tarefa 4.5): troca o evento sorteado do dia.</summary>
    void ForcarEventoDeDepuracao()
    {
        if (string.IsNullOrEmpty(forcarEvento)) return;
        var def = Eventos.Def(forcarEvento.Trim());
        if (def == null) { Debug.LogWarning("[GameManager] forcarEvento: evento desconhecido '" + forcarEvento + "'."); return; }
        var T = Missao.Hoje;
        T.Evento = def.Id;
        T.EventoNaIda = def.Momento != MomentoEvento.Volta;
        if (def.Id == Eventos.CmeId && Missao.CmeRegistro == null && _cmes.Count > 0)
        {
            Missao.CmeRegistro = _cmes[0];
            Missao.CmeDia = Missao.Dia;
            Missao.CmeChegada = Missao.Dia + Eventos.DiasAteChegada;
        }
        Debug.Log("[GameManager] forcarEvento: Dia " + Missao.Dia + " com " + def.Id + (T.EventoNaIda ? " (ida)" : " (volta)"));
    }

    public void SairDoBoletim() { if (Fase == Fase.Boletim) { _tempoNaIda = 0f; MudarFase(Fase.AteTarefa); } }

    // ---------------- Evento do dia e corrida até o abrigo (design D7) ----------------

    void AbrirEvento(Fase retorno)
    {
        _retornoEvento = retorno;
        bool alarme = Eventos.Def(Missao.Hoje.Evento)?.Alarme == true;
        if (alarme) { DefinirLuz(PresetLuz.Erupcao); SonsUI.Alarme(); }
        else SonsUI.Painel();
        MudarFase(Fase.Evento);
    }

    /// <summary>Botão de uma escolha do cartão do evento.</summary>
    public void EscolherEvento(string escolha)
    {
        if (Fase != Fase.Evento) return;
        var ctx = Eventos.Escolher(Missao, escolha, RelogioZerado);
        if (!Missao.Hoje.EventoResolvido) return; // escolha indisponível
        if (ctx.IniciarCorrida) { IniciarCorrida(); return; }
        if (Eventos.Def(Missao.Hoje.Evento)?.Alarme == true) DefinirLuz(PresetLuz.Dia);
        PerderTempo(ctx.SegundosPerdidos);
        VoltarDoEvento();
    }

    void IniciarCorrida()
    {
        // Saiu do lugar: depois da corrida o jogador anda de novo até a tarefa, sem pular para a escolha.
        if (_retornoEvento == Fase.Escolha) _retornoEvento = Fase.AteTarefa;
        ContagemCorrida = Eventos.ContagemReal(Missao);
        MudarFase(Fase.CorridaAbrigo);
    }

    void FimCorrida(bool aTempo)
    {
        if (Fase != Fase.CorridaAbrigo) return;
        var ctx = Eventos.FimCorrida(Missao, aTempo);
        DefinirLuz(PresetLuz.Dia);
        PerderTempo(ctx.SegundosPerdidos);
        VoltarDoEvento();
        _ui.Aviso(aTempo ? Textos.ChegouAbrigo : Textos.AtrasouAbrigo);
    }

    /// <summary>Tira tempo do relógio. Se zerar, o Update aplica a regra do tempo esgotado no próximo quadro.</summary>
    void PerderTempo(float segundos)
    {
        if (segundos > 0f) TempoRestante = Mathf.Max(0f, TempoRestante - segundos);
    }

    void VoltarDoEvento()
    {
        if (_retornoEvento == Fase.Distribuir) AbrirDistribuicao();
        else MudarFase(_retornoEvento);
    }

    public void EscolherFazer()
    {
        if (Fase != Fase.Escolha || !Tarefas.PodeFazer(Missao)) return;
        var id = Missao.Hoje.Tarefa;
        if (!Teste && !_vistos.Contains(id)) MudarFase(Fase.MinijogoInfo);
        else IniciarMinijogo();
    }

    public void EscolherFicar()
    {
        if (Fase != Fase.Escolha) return;
        Tarefas.Ficar(Missao);
        MudarFase(Fase.TarefaConcluida);
    }

    /// <summary>Botão "Começar!" do cartão de primeira vez do minijogo.</summary>
    public void ComecarMinijogo()
    {
        if (Fase != Fase.MinijogoInfo) return;
        _vistos.Add(Missao.Hoje.Tarefa);
        SalvarPreferencias();
        IniciarMinijogo();
    }

    void IniciarMinijogo()
    {
        var id = Missao.Hoje.Tarefa;
        _minigame = Minigames.Criar(id, Tarefas.LimiteReal(Missao, id));
        // Dia 1 com tutorial: a mão fantasma mostra o gesto por 3 s antes de o relógio começar (tarefa 7.8).
        _minigame.demo = TutorialAtivo ? Minigame.DuracaoDemo : 0f;
        MudarFase(Fase.Minijogo);
        _ui.AbrirMinigame(_minigame, AoResultadoMinijogo, AoFecharMinijogo);
    }

    /// <summary>Minijogo aberto agora (null fora dele). Para a sonda.</summary>
    public Minigame MinigameAtual => _minigame;

    /// <summary>SAIR / B: termina o minijogo com o progresso atual.</summary>
    public void CancelarMinijogo()
    {
        if (Fase == Fase.Minijogo) _minigame?.Cancelar();
    }

    /// <summary>Fim do minijogo: aplica o resultado na hora e devolve o efeito, que a faixa de resultado mostra com a nota.</summary>
    Txt AoResultadoMinijogo(ResultadoMinijogo r)
    {
        Tarefas.Fazer(Missao, r);
        return Missao.Hoje.Tarefa == Tarefas.Vazamento ? Missao.Hoje.MsgVazamento : Missao.Hoje.MsgTarefa;
    }

    /// <summary>A faixa de resultado saiu: segue para a decisão do vazamento ou para a tarefa concluída.</summary>
    void AoFecharMinijogo()
    {
        _minigame = null;
        _ui.FecharMinigame();
        if (Missao.Hoje.Tarefa == Tarefas.Vazamento && !Missao.Hoje.TarefaResolvida) MudarFase(Fase.DecisaoVazamento);
        else MudarFase(Fase.TarefaConcluida);
    }

    public void VazamentoReforcar() { if (Fase != Fase.DecisaoVazamento) return; Tarefas.VazamentoReforcar(Missao); MudarFase(Fase.TarefaConcluida); }
    public void VazamentoDepois() { if (Fase != Fase.DecisaoVazamento) return; Tarefas.VazamentoDepois(Missao); MudarFase(Fase.TarefaConcluida); }

    public void VoltarAoHub()
    {
        if (Fase != Fase.TarefaConcluida || Teste) return;
        if (TempoRestante <= 0f) AbrirDistribuicao();
        else if (EventoPendente) AbrirEvento(Fase.AteHub);
        else MudarFase(Fase.AteHub);
    }

    void TempoAcabou()
    {
        // O relógio acabou no meio da corrida: a tripulação já estava a caminho e fica no abrigo, sem custo extra.
        if (Fase == Fase.CorridaAbrigo) { Eventos.FimCorrida(Missao, true); DefinirLuz(PresetLuz.Dia); }
        bool antesDaTarefa = !Missao.Hoje.TarefaResolvida;
        if (antesDaTarefa) Tarefas.TempoEsgotado(Missao);
        AbrirDistribuicao();
        _ui.Aviso(Textos.TempoAcabou);
    }

    void AbrirDistribuicao()
    {
        // O evento do dia sempre abre antes da distribuição (spec eventos-do-dia, "Momento da interrupção").
        if (EventoPendente) { AbrirEvento(Fase.Distribuir); return; }
        RegrasMissao.PrepararDistribuicao(Missao);
        MudarFase(Fase.Distribuir);
    }

    public void Mais(Modulo mod) { if (Fase == Fase.Distribuir && RegrasMissao.Mais(Missao, mod)) _ui.Render(); }
    public void Menos(Modulo mod) { if (Fase == Fase.Distribuir && RegrasMissao.Menos(Missao, mod)) _ui.Render(); }

    public void ComecarODia()
    {
        if (Fase != Fase.Distribuir) return;
        var r = RegrasMissao.Resolver(Missao);
        DefinirLuz(r.Erupcao != null ? PresetLuz.Erupcao : PresetLuz.Noite);
        if (Navegador.PrefereMenosMovimento) { IrParaResultado(); return; }
        PassoAnim = 0;
        _animT = 0f;
        MudarFase(Fase.DiaAcontece);
    }

    public void PularAnim() { if (Fase == Fase.DiaAcontece) IrParaResultado(); }
    void IrParaResultado() => MudarFase(Fase.Resultado);

    public void Proximo()
    {
        if (Fase != Fase.Resultado) return;
        if (TutorialAtivo && TutorialLigado) { TutorialLigado = false; SalvarPreferencias(); }
        if (Missao.Fim != Fim.Nenhum) MudarFase(Fase.Relatorio);
        else IniciarDia(true);
    }

    public void JogarDeNovo()
    {
        Missao = null;
        Teste = false;
        DefinirLuz(PresetLuz.Dia);
        MudarFase(Fase.Inicio);
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------- Como jogar, tutorial, créditos, idioma ----------------

    public void AbrirComoJogar() { Slide = 0; _retornoComoJogar = Fase.Inicio; MudarFase(Fase.ComoJogar); }

    public void SlideProximo()
    {
        if (Slide >= Textos.SlideTitulo.Length - 1) MudarFase(_retornoComoJogar);
        else { Slide++; _ui.Render(); }
    }

    public void SlideAnterior() { if (Slide > 0) { Slide--; _ui.Render(); } }
    public void PularSlides() => MudarFase(_retornoComoJogar);

    public void AlternarTutorial()
    {
        TutorialLigado = !TutorialLigado;
        SalvarPreferencias();
        _ui.Render();
    }

    public void AbrirCreditos() { _retornoCreditos = Fase; MudarFase(Fase.Creditos); }
    public void FecharCreditos() => MudarFase(_retornoCreditos == Fase.Creditos ? Fase.Inicio : _retornoCreditos);

    public void AlternarIdioma() => Idioma.Alternar();

    void AoMudarIdioma()
    {
        if (_ui == null) return;
        _ui.Render();
        RefrescarEstacoes();
    }

    // ---------------- Menu de teste dos minijogos ----------------

    public void AbrirMenuMinijogos()
    {
        Teste = true;
        Missao = null;
        MudarFase(Fase.MenuMinijogos);
    }

    public void DefinirModoTeste(Modo m) { ModoTeste = m; _ui.Render(); }

    public void TestarMinijogo(string id)
    {
        _tarefaTeste = id;
        Teste = true;
        Missao = RegrasMissao.NovaPartida(ModoTeste, _erupcoes, _rng, false);
        Missao.Alertas.Clear();
        Missao.Sombra = new[] { -1, -1 };
        Missao.Tarefas[0] = id;
        RegrasMissao.IniciarDia(Missao);
        Missao.Hoje.Tarefa = id;
        Missao.O2 = Missao.MaxO2;
        IniciarMinijogo();
    }

    public void TestarDeNovo() { if (!string.IsNullOrEmpty(_tarefaTeste)) TestarMinijogo(_tarefaTeste); }

    // ---------------- Pausa ----------------

    bool PodePausar => FaseComRelogio(Fase) || Fase == Fase.Minijogo;

    public void Pausar() => DefinirPausa(true);
    public void Retomar() => DefinirPausa(false);

    void DefinirPausa(bool pausar)
    {
        if (pausar && !PodePausar) return;
        Pausado = pausar;
        Time.timeScale = pausar ? 0f : 1f;
        if (jogador) jogador.Congelar(pausar || !FaseComMovimento(Fase));
        _ui.MostrarPausa(pausar);
    }

    // ---------------- Troca de fase ----------------

    void MudarFase(Fase nova)
    {
        Fase = nova;
        if (Pausado) { Pausado = false; Time.timeScale = 1f; _ui?.MostrarPausa(false); }
        // O personagem só anda nas fases de deslocamento.
        if (jogador) jogador.Congelar(!FaseComMovimento(nova));
        Alvo = Proxima = null;
        RefrescarEstacoes();
        AjustarCamera(nova);
        _ui.Render();
    }

    // ---------------- Câmera por fase (grupo 5 da mudança interface-e-minijogos) ----------------

    /// <summary>Base vista de cima, de fora: mesma pose das capturas aéreas (MontarBase), com a cena empurrada para a esquerda, fora do painel.</summary>
    static readonly Vector3 AereaDe = new Vector3(0f, 110f, 52f), AereaPara = new Vector3(2f, 0f, 2f);
    const float AereaFov = 60f, AereaDeslocamento = 58f;

    /// <summary>
    /// Resultado: a câmera sobe e mostra a base de fora, para a luz do dia (calma, erupção ou noite) aparecer à esquerda do painel.
    /// Início: órbita lenta em volta da base. As páginas opacas (Como jogar, Créditos, Relatório) não mexem na câmera.
    /// Nas demais fases a câmera volta ao personagem.
    /// </summary>
    void AjustarCamera(Fase nova)
    {
        var cf = CameraFoco.Garantir();
        if (cf == null) return;
        switch (nova)
        {
            case Fase.Resultado:
                CameraFoco.PoseOlhando(AereaDe, AereaPara, AereaDeslocamento, out var pos, out var rot);
                cf.Focar(pos, rot, AereaFov);
                break;
            case Fase.Inicio:
                cf.Orbitar(AereaPara, 70f, 75f, 3f, 55f, 52f);
                break;
            // Close na estação da tarefa (design D8): começa na escolha e fica o mesmo até sair da tarefa concluída,
            // sem a câmera ir e voltar entre a escolha, o cartão, o minijogo e a tela de concluída.
            case Fase.Escolha: case Fase.DecisaoVazamento: case Fase.MinijogoInfo: case Fase.Minijogo: case Fase.TarefaConcluida:
                FocarNaTarefa(cf);
                break;
            case Fase.ComoJogar: case Fase.Creditos: case Fase.Relatorio: case Fase.MenuMinijogos:
                break;
            default:
                cf.Soltar();
                break;
        }
    }

    /// <summary>
    /// Câmera na estação da tarefa de hoje. No menu de teste, sem a estação na cena, a câmera mostra a base de cima
    /// (vista geral); na missão, sem a estação, a tarefa abre no console do hub e o close é nele.
    /// </summary>
    void FocarNaTarefa(CameraFoco cf)
    {
        var d = Missao != null ? Tarefas.Def(Missao.Hoje.Tarefa) : null;
        Estacao e = null;
        if (d != null) _estacoes.TryGetValue(d.Estacao, out e);
        if (e == null && !Teste) e = _console;
        if (e != null) { cf.FocarEstacao(e, jogador != null ? jogador.transform : null); return; }
        CameraFoco.PoseOlhando(AereaDe, AereaPara, AereaDeslocamento, out var pos, out var rot);
        cf.Focar(pos, rot, AereaFov);
    }

    /// <summary>Estação registrada com o id (null se a cena não tiver). Para a sonda e as capturas.</summary>
    public Estacao EstacaoPorId(string id) => id != null && _estacoes.TryGetValue(id, out var e) ? e : null;

    void DefinirLuz(PresetLuz p)
    {
        if (_luz) _luz.SetPreset(p);
    }

    /// <summary>Só para testes e depuração: muda o tempo restante do dia.</summary>
    public void DepurarTempo(float segundos) => TempoRestante = segundos;
}
