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

    static bool FaseComRelogio(Fase f) => f == Fase.AteTarefa || f == Fase.Escolha || f == Fase.DecisaoVazamento || f == Fase.TarefaConcluida || f == Fase.AteHub;
    /// <summary>O relógio do dia anda (parado no minijogo e na pausa).</summary>
    public bool RelogioCorre => !Pausado && !Teste && Missao != null && FaseComRelogio(Fase);

    UIJogo _ui;
    Minigame _minigame;
    BaseLightingController _luz;
    readonly Dictionary<string, Estacao> _estacoes = new Dictionary<string, Estacao>();
    Estacao _console;
    readonly List<Erupcao> _erupcoes = new List<Erupcao>();
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

        if (Fase == Fase.AteTarefa || Fase == Fase.AteHub)
        {
            AtualizarGuia();
            if (Proxima != null && Entrada.ConfirmarDown) Interagir();
        }
        _ui.AtualizarHud();
    }

    // =====================================================================
    // Dados e preferências
    // =====================================================================

    [Serializable] class RegistroFLR { public string id, eventType, startTime, classLabel, sourceUrl, retrievedOn; public bool isTest; public int gameIntensity; }
    [Serializable] class ArquivoErupcoes { public string fonte; public RegistroFLR[] records; }

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
                Data = string.IsNullOrEmpty(r.startTime) || r.startTime.Length < 10 ? r.startTime : r.startTime.Substring(0, 10),
            });
        }
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
    }

    void AtualizarGuia()
    {
        Alvo = Fase == Fase.AteHub ? _console : EstacaoDaTarefa();
        Proxima = null;
        if (Alvo != null && jogador != null && Alvo.DistanciaAte(jogador.transform.position) <= Alvo.raio) Proxima = Alvo;
        var rotulo = Fase == Fase.AteHub ? Textos.BotaoDistribuir : Textos.BotaoIniciar.F(Tarefas.Def(Missao.Hoje.Tarefa).Nome);
        _ui.AtualizarGuia(Alvo, Proxima, rotulo);
    }

    /// <summary>Botão AÇÃO / tecla E / botão A perto do destino.</summary>
    public void Interagir()
    {
        if (Pausado || Missao == null) return;
        if (Fase == Fase.AteTarefa || Fase == Fase.AteHub) AtualizarGuia();
        if (Proxima == null) return;
        if (Fase == Fase.AteTarefa) MudarFase(Fase.Escolha);
        else if (Fase == Fase.AteHub && Missao.Hoje.TarefaResolvida) AbrirDistribuicao();
    }

    // =====================================================================
    // Fluxo da missão
    // =====================================================================

    public void Comecar(Modo modo)
    {
        Teste = false;
        Missao = RegrasMissao.NovaPartida(modo, _erupcoes, _rng, TutorialLigado);
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
        TempoRestante = Mathf.Max(1f, duracaoDia);
        DefinirLuz(PresetLuz.Dia);
        if (mostrar) MudarFase(Fase.Boletim);
    }

    public void SairDoBoletim() { if (Fase == Fase.Boletim) MudarFase(Fase.AteTarefa); }

    public void EscolherFazer()
    {
        if (Fase != Fase.Escolha || !Tarefas.PodeFazer(Missao)) return;
        var id = Missao.Hoje.Tarefa;
        if (!Teste && !_vistos.Contains(id)) MudarFase(Fase.MinijogoInfo);
        else IniciarMinijogo(Teste || !_vistos.Contains(id));
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
        IniciarMinijogo(false);
    }

    void IniciarMinijogo(bool mostrarInstrucao)
    {
        var id = Missao.Hoje.Tarefa;
        _minigame = Minigames.Criar(id, Tarefas.LimiteReal(Missao, id));
        MudarFase(Fase.Minijogo);
        _ui.AbrirMinigame(_minigame, mostrarInstrucao, AoTerminarMinijogo);
    }

    /// <summary>SAIR / B: termina o minijogo com o progresso atual.</summary>
    public void CancelarMinijogo()
    {
        if (Fase == Fase.Minijogo) _minigame?.Cancelar();
    }

    void AoTerminarMinijogo(ResultadoMinijogo r)
    {
        _minigame = null;
        _ui.FecharMinigame();
        Tarefas.Fazer(Missao, r);
        if (Missao.Hoje.Tarefa == Tarefas.Vazamento && !Missao.Hoje.TarefaResolvida) MudarFase(Fase.DecisaoVazamento);
        else MudarFase(Fase.TarefaConcluida);
    }

    public void VazamentoReforcar() { if (Fase != Fase.DecisaoVazamento) return; Tarefas.VazamentoReforcar(Missao); MudarFase(Fase.TarefaConcluida); }
    public void VazamentoDepois() { if (Fase != Fase.DecisaoVazamento) return; Tarefas.VazamentoDepois(Missao); MudarFase(Fase.TarefaConcluida); }

    public void VoltarAoHub()
    {
        if (Fase != Fase.TarefaConcluida || Teste) return;
        if (TempoRestante <= 0f) AbrirDistribuicao();
        else MudarFase(Fase.AteHub);
    }

    void TempoAcabou()
    {
        bool antesDaTarefa = !Missao.Hoje.TarefaResolvida;
        if (antesDaTarefa) Tarefas.TempoEsgotado(Missao);
        AbrirDistribuicao();
        _ui.Aviso(Textos.TempoAcabou);
    }

    void AbrirDistribuicao()
    {
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
        IniciarMinijogo(true);
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
        if (jogador) jogador.Congelar(pausar || !(Fase == Fase.AteTarefa || Fase == Fase.AteHub));
        _ui.MostrarPausa(pausar);
    }

    // ---------------- Troca de fase ----------------

    void MudarFase(Fase nova)
    {
        Fase = nova;
        if (Pausado) { Pausado = false; Time.timeScale = 1f; _ui?.MostrarPausa(false); }
        // O personagem só anda nas fases de deslocamento.
        if (jogador) jogador.Congelar(!(nova == Fase.AteTarefa || nova == Fase.AteHub));
        Alvo = Proxima = null;
        RefrescarEstacoes();
        _ui.Render();
    }

    void DefinirLuz(PresetLuz p)
    {
        if (_luz) _luz.SetPreset(p);
    }

    /// <summary>Só para testes e depuração: muda o tempo restante do dia.</summary>
    public void DepurarTempo(float segundos) => TempoRestante = segundos;
}
