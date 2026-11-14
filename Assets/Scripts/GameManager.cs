using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EstadoJogo { Titulo, Intro, Briefing, Acao, Minigame, Resumo, Pontuacao, GameOver }

/// <summary>
/// Controla o ciclo de 10 dias: Título → Intro → [Briefing → Ação → Resumo] x10 → Pontuação.
/// Tudo numa cena só: este script liga/desliga o movimento do personagem e as telas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [Header("Dias (vazio = usa os 10 padrões do roteiro)")]
    public DayData[] dias;

    [Header("Referências (achadas sozinhas se vazias)")]
    public Movimento jogador;

    public EstadoJogo Estado { get; private set; }
    public int DiaAtual { get; private set; }
    public float TempoRestante { get; private set; }
    public Recursos Recursos { get; private set; } = new Recursos();
    public DayData DadosDoDia => dias[DiaAtual - 1];
    public int TotalDias => dias.Length;

    /// <summary>Resultado da noite para a tela de resumo.</summary>
    public float ResumoComida, ResumoOxigenio, ResumoEnergiaRestante;

    public event Action<EstadoJogo> EstadoMudou;
    public event Action RecursosMudaram;

    UIJogo _ui;
    bool _painelAvisado;
    Estacao[] _estacoes;
    readonly HashSet<string> _concluidas = new HashSet<string>();

    /// <summary>Estação para onde a seta aponta (próxima pendente, ou a Cama quando tudo foi feito).</summary>
    public Estacao Alvo { get; private set; }
    /// <summary>Estação ao alcance do jogador (mostra botão AÇÃO).</summary>
    public Estacao Proxima { get; private set; }
    public bool TudoFeito { get; private set; }
    public bool AlvoUrgente { get; private set; }

    /// <summary>Painéis solares cobertos de poeira: bateria menor até limpar (começa no Dia 4).</summary>
    public bool PainelSujo { get; private set; }
    public const float EnergiaPainelSujo = 60f;
    public const float ComidaInicial = 60f;
    public const float ConsumoComidaPorDia = 6f;
    public const float SemComidaPerdaOxigenio = 10f;

    /// <summary>Linhas explicando o que mudou na noite (para a tela de resumo).</summary>
    public readonly List<string> ResumoLinhas = new List<string>();

    void Awake()
    {
        Instancia = this;
        if (dias == null || dias.Length == 0) dias = DiasPadrao.Criar();
    }

    void Start()
    {
        if (!jogador) jogador = FindObjectOfType<Movimento>();
        _estacoes = FindObjectsOfType<Estacao>();
        _ui = new UIJogo(this);
        MudarEstado(EstadoJogo.Titulo);
    }

    public bool Pausado { get; private set; }

    public void Pausar() => DefinirPausa(true);
    public void Retomar() => DefinirPausa(false);

    void DefinirPausa(bool pausar)
    {
        if (pausar && Estado != EstadoJogo.Acao && Estado != EstadoJogo.Minigame) return;
        Pausado = pausar;
        Time.timeScale = pausar ? 0f : 1f;
        if (jogador) jogador.Congelar(pausar || Estado != EstadoJogo.Acao);
        _ui.MostrarPausa(pausar);
    }

    void OnDestroy() => Time.timeScale = 1f;

    void Update()
    {
        // Controle: Start pausa/continua, B volta.
        if (Estado == EstadoJogo.Acao || Estado == EstadoJogo.Minigame)
        {
            if (Entrada.PausaDown) { DefinirPausa(!Pausado); return; }
            if (Pausado && Entrada.VoltarDown) { Retomar(); return; }
        }
        if (Estado == EstadoJogo.Intro && Entrada.VoltarDown) MudarEstado(EstadoJogo.Titulo);
        if (Estado == EstadoJogo.Minigame && !Pausado)
        {
            _minigame?.Tick();
            if (Entrada.VoltarDown) CancelarMinigame();
            return;
        }
        if (Pausado) return;
        if (Estado != EstadoJogo.Acao) return;
        TempoRestante -= Time.deltaTime;
        if (TempoRestante <= 0f) { TempoRestante = 0f; TerminarDia(); return; }
        AtualizarEventos();
        if (Estado != EstadoJogo.Acao) return;
        AtualizarEstacoes();
        _ui.AtualizarHud();
        _ui.AtualizarGuia(Alvo, Proxima);
        _ui.AtualizarAlerta(_evento, _eventoRestante);
        if (Proxima != null && Entrada.ConfirmarDown) Interagir();
    }

    // ---------- Estações / tarefas ----------

    static readonly string[] PoolPadrao = { "lixo", "filtro", "esteira", "estufa", "reciclador" };
    readonly List<string> _tarefasHoje = new List<string>();
    /// <summary>ids das tarefas do dia (fixas + sorteadas + painel se sujo).</summary>
    public IReadOnlyList<string> TarefasHoje => _tarefasHoje;

    // Evento do dia em andamento (alarme/vazamento)
    EventoDia _evento;
    float _eventoRestante;
    bool[] _disparados = new bool[0];
    public EventoDia EventoAtivo => _evento;
    public float EventoRestante => _eventoRestante;
    public bool BackupFeito { get; private set; }

    bool EstacaoDoDia(Estacao e)
    {
        if (e.id == "cama") return true;
        if (_evento != null && _evento.estacao == e.id) return true;
        if (e.id == "airlock") return _tarefasHoje.Contains(e.id) && _concluidas.Contains("backup");
        return _tarefasHoje.Contains(e.id);
    }

    void RefrescarEstacoes()
    {
        foreach (var e in _estacoes) e.Definir(EstacaoDoDia(e), _concluidas.Contains(e.id));
    }

    void AtualizarEventos()
    {
        var eventos = DadosDoDia.eventos;
        if (_evento == null && eventos != null)
        {
            float decorrido = Mathf.Max(DadosDoDia.duracaoSegundos, 1f) - TempoRestante;
            for (int i = 0; i < eventos.Length; i++)
            {
                if (_disparados[i] || decorrido < eventos[i].segundo || !ExisteEstacao(eventos[i].estacao)) continue;
                _disparados[i] = true;
                _evento = eventos[i];
                _eventoRestante = _evento.limite;
                RefrescarEstacoes();
                _ui.Alerta(_evento.titulo, _evento.texto);
                break;
            }
        }
        if (_evento == null) return;

        if (_evento.drenoOxigenioPorSeg > 0f)
        {
            AplicarRecursos(0, -_evento.drenoOxigenioPorSeg * Time.deltaTime, 0);
            if (Estado != EstadoJogo.Acao) return; // game over durante o dreno
        }
        if (_evento.limite > 0f)
        {
            _eventoRestante -= Time.deltaTime;
            if (_eventoRestante <= 0f)
            {
                var ev = _evento;
                _evento = null;
                AplicarRecursos(-ev.penalEnergia, -ev.penalOxigenio, 0);
                RefrescarEstacoes();
                _ui.Aviso("Você demorou demais! A base sofreu danos.");
            }
        }
    }

    public string NomeDe(string id)
    {
        foreach (var e in _estacoes) if (e.id == id) return e.nome;
        return id;
    }

    /// <summary>Monta a lista de tarefas do dia: fixas do DayData + sorteadas (sem repetir) + painel se estiver sujo.</summary>
    void SortearTarefas()
    {
        _tarefasHoje.Clear();
        var d = DadosDoDia;
        foreach (var id in d.estacoes ?? new string[0]) _tarefasHoje.Add(id);
        if (PainelSujo && !_tarefasHoje.Contains("painel")) _tarefasHoje.Add("painel");

        var pool = new List<string>(d.poolAleatorio != null && d.poolAleatorio.Length > 0 ? d.poolAleatorio : PoolPadrao);
        pool.RemoveAll(id => _tarefasHoje.Contains(id) || !ExisteEstacao(id));
        for (int i = 0; i < d.tarefasAleatorias && pool.Count > 0; i++)
        {
            int k = UnityEngine.Random.Range(0, pool.Count);
            _tarefasHoje.Add(pool[k]);
            pool.RemoveAt(k);
        }
    }

    bool ExisteEstacao(string id)
    {
        foreach (var e in _estacoes) if (e.id == id) return true;
        return false;
    }

    void PrepararEstacoes()
    {
        _concluidas.Clear();
        foreach (var e in _estacoes) e.Definir(EstacaoDoDia(e), false);
    }

    void AtualizarEstacoes()
    {
        Alvo = Proxima = null;
        if (!jogador) return;
        Vector3 p = jogador.transform.position;
        float melhorAlvo = float.MaxValue, melhorProx = float.MaxValue;
        Estacao cama = null;
        TudoFeito = true;
        foreach (var e in _estacoes)
        {
            if (!EstacaoDoDia(e)) continue;
            if (e.id == "cama") { cama = e; continue; }
            if (e.Concluida) continue;
            TudoFeito = false;
            float d = e.DistanciaAte(p);
            if (d < melhorAlvo) { melhorAlvo = d; Alvo = e; }
        }
        // Evento urgente tem prioridade na seta
        AlvoUrgente = false;
        if (_evento != null)
            foreach (var e in _estacoes)
                if (e.id == _evento.estacao && !e.Concluida) { Alvo = e; AlvoUrgente = true; TudoFeito = false; }
        if (Alvo == null) Alvo = cama; // tudo feito → vá dormir
        foreach (var e in _estacoes)
        {
            if (!EstacaoDoDia(e) || e.Concluida) continue;
            if (e.id == "cama" && !TudoFeito) continue;
            float d = e.DistanciaAte(p);
            if (d <= e.raio && d < melhorProx) { melhorProx = d; Proxima = e; }
        }
    }

    /// <summary>Botão AÇÃO / tecla E. Por enquanto conclui direto; as próximas tasks abrem o minigame aqui.</summary>
    public void Interagir()
    {
        var e = Proxima;
        if (e == null || Estado != EstadoJogo.Acao) return;
        if (e.id == "cama") { TerminarDia(); return; }
        AbrirMinigame(e);
    }

    Minigame _minigame;
    Estacao _estacaoDoMinigame;

    void AbrirMinigame(Estacao e)
    {
        _estacaoDoMinigame = e;
        _minigame = Minigames.Criar(e);
        MudarEstado(EstadoJogo.Minigame);          // congela o personagem
        _ui.AbrirMinigame(_minigame, sucesso => FecharMinigame(sucesso));
    }

    void FecharMinigame(bool sucesso)
    {
        var e = _estacaoDoMinigame;
        _minigame = null; _estacaoDoMinigame = null;
        MudarEstado(EstadoJogo.Acao);
        if (sucesso && e != null) ConcluirEstacao(e);
    }

    /// <summary>Botão SAIR / tecla B: fecha o minigame sem concluir a tarefa.</summary>
    public void CancelarMinigame()
    {
        if (Estado != EstadoJogo.Minigame || _minigame == null) return;
        _minigame.Cancelar();
    }

    public void ConcluirEstacao(Estacao e)
    {
        _concluidas.Add(e.id);
        if (e.id == "painel") PainelSujo = false;
        if (e.id == "backup") BackupFeito = true;
        bool eraEvento = _evento != null && _evento.estacao == e.id;
        if (eraEvento) _evento = null;
        TempoRestante = Mathf.Max(0f, TempoRestante - e.custoTempo);
        var (cEnergia, cOxigenio, _) = Estacao.Custos(e.id);
        AplicarRecursos(-cEnergia, -cOxigenio, 0);
        if (Estado == EstadoJogo.GameOver) return;
        RefrescarEstacoes();

        // Última tarefa do jogo: a comporta fecha e o dia 10 termina.
        if (e.id == "airlock") { TerminarDia(); return; }

        bool tudo = _evento == null;
        foreach (var id in _tarefasHoje) if (!_concluidas.Contains(id)) tudo = false;
        string aviso = eraEvento ? "Crise resolvida!" : $"{e.nome} concluída!";
        if (tudo) aviso = "Tudo pronto! Volte para a cama para dormir.";
        _ui.Aviso(aviso);
        _ui.AtualizarTarefas();
    }

    public bool EstaConcluida(string id) => _concluidas.Contains(id);

    // ---------- Fluxo ----------

    public void Jogar() => MudarEstado(EstadoJogo.Intro);

    public void ComecarJogo()
    {
        DiaAtual = 1;
        PainelSujo = false;
        BackupFeito = false;
        Recursos = new Recursos { comida = ComidaInicial };
        IniciarBriefing();
    }

    void IniciarBriefing()
    {
        if (DiaAtual >= 4 && !_painelAvisado) { PainelSujo = true; _painelAvisado = true; }
        SortearTarefas();
        _evento = null;
        _disparados = new bool[DadosDoDia.eventos != null ? DadosDoDia.eventos.Length : 0];
        Recursos.energia = PainelSujo ? Mathf.Min(EnergiaPainelSujo, DadosDoDia.energiaInicial) : DadosDoDia.energiaInicial;
        RecursosMudaram?.Invoke();
        MudarEstado(EstadoJogo.Briefing);
    }

    public void ComecarAcao()
    {
        TempoRestante = Mathf.Max(DadosDoDia.duracaoSegundos, 1f);
        PrepararEstacoes();
        MudarEstado(EstadoJogo.Acao);
        var nasa = DonkiDados.Do(DiaAtual);
        if (nasa != null) _ui.Aviso("Evento NASA hoje: " + nasa.nome);
    }

    /// <summary>Chamado pela cama (Task 3), pelo botão temporário "Dormir" ou quando o tempo zera.</summary>
    public void TerminarDia()
    {
        if (Estado != EstadoJogo.Acao && Estado != EstadoJogo.Minigame) return;

        _evento = null;
        // Consumo e consequências da noite
        float o2Antes = Recursos.oxigenio, comidaAntes = Recursos.comida;
        ResumoLinhas.Clear();
        float dO2 = -DadosDoDia.perdaOxigenioNoite, dComida = -ConsumoComidaPorDia;
        foreach (var e in _estacoes)
        {
            if (e.id == "cama" || !EstacaoDoDia(e)) continue;
            var (comidaFeita, o2Ignorada, motivo) = Estacao.Regras(e.id);
            if (e.Concluida) dComida += comidaFeita;
            else
            {
                dO2 -= o2Ignorada;
                if (motivo != "") ResumoLinhas.Add(motivo);
            }
        }
        if (PainelSujo) ResumoLinhas.Add("Painéis sujos: pouca energia amanhã");
        if (Recursos.comida + dComida <= 0f)
        {
            dO2 -= SemComidaPerdaOxigenio;
            ResumoLinhas.Add("Sem comida: a base enfraqueceu (oxigênio caiu mais)");
        }
        Recursos.Aplicar(0, dO2, dComida);
        ResumoOxigenio = o2Antes - Recursos.oxigenio;
        ResumoComida = comidaAntes - Recursos.comida; // negativo = comida produzida
        ResumoEnergiaRestante = Recursos.energia;
        RecursosMudaram?.Invoke();

        MudarEstado(Recursos.oxigenio <= 0f ? EstadoJogo.GameOver : EstadoJogo.Resumo);
    }

    public void ProximoDia()
    {
        if (DiaAtual >= TotalDias) { MudarEstado(EstadoJogo.Pontuacao); return; }
        DiaAtual++;
        IniciarBriefing();
    }

    public void Reiniciar() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    /// <summary>API para minigames/eventos (próximas tasks): aplica custo de recursos.</summary>
    public void AplicarRecursos(float dEnergia, float dOxigenio, float dComida)
    {
        Recursos.Aplicar(dEnergia, dOxigenio, dComida);
        RecursosMudaram?.Invoke();
        if (Recursos.oxigenio <= 0f) MudarEstado(EstadoJogo.GameOver);
    }

    public string TextoPontuacao()
    {
        float o2 = Recursos.oxigenio, comida = Recursos.comida;
        if (!BackupFeito) return "Você sobreviveu, mas deixou a pesquisa para trás. Muita ciência foi sacrificada.";
        if (o2 >= 70f && comida >= 5f) return "Missão Perfeita! Você entendeu o peso das suas escolhas!";
        if (o2 >= 30f) return "Você sobreviveu, mas sacrificou muita ciência.";
        return "Resgate no limite. A base quase não aguentou.";
    }

    void MudarEstado(EstadoJogo novo)
    {
        Estado = novo;
        if (Pausado) { Pausado = false; Time.timeScale = 1f; }
        // O personagem só se mexe na fase de Ação.
        if (jogador) jogador.Congelar(novo != EstadoJogo.Acao);
        _ui.Mostrar(novo);
        EstadoMudou?.Invoke(novo);
    }
}
