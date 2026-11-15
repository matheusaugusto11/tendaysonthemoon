#if UNITY_EDITOR
// Este arquivo inteiro só existe no Editor: numa build (WebGL, mobile, desktop) ele não é compilado e o auto jogar não existe.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DezDias;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// QA automático: joga missões inteiras de 10 dias como um jogador, pela mesma API que os botões usam, e anota o que
/// travou, teleportou, quebrou ou não faz sentido em relação ao conteúdo da NASA. Só no Editor, só quando pedido
/// (menu Tools/QA ou Temp/qa_autojogo.flag). Relatório em Temp/QA/. Plano e lista de checagens: Docs/QA-AUTOJOGO.md.
/// </summary>
public class AutoJogoQA : MonoBehaviour
{
    public const string ChavePedido = "tdm_qa_autojogo";
    const string ArquivoPedido = "Temp/qa_autojogo.flag";
    static readonly string[] ChavesPrefs = { "tdm_seen", "tdm_tut", "tdm_som" };

    enum Perfil { Cuidadoso, Arriscado, Aleatorio }
    enum Nivel { Falha, Alerta, Info }

    sealed class PlanoMissao
    {
        public Modo Modo; public Perfil Perfil; public bool Tutorial;
        public bool TestarPausa, TestarIdioma, TestarFimDoTempo, TestarMovimentoTravado;
    }

    sealed class Registro
    {
        public Nivel Nivel; public string Categoria, Onde, Texto, Foto;
    }

    // =====================================================================
    // Início (só com pedido explícito)
    // =====================================================================

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        string pedido = EditorPrefs.GetString(ChavePedido, "");
        string arq = Path.Combine(Application.dataPath, "..", ArquivoPedido);
        if (pedido == "" && File.Exists(arq))
        {
            pedido = File.ReadAllText(arq).Trim();
            if (pedido == "") pedido = "completo";
            File.Delete(arq);
        }
        if (pedido == "") return;
        EditorPrefs.DeleteKey(ChavePedido);
        if (FindObjectOfType<GameManager>() == null) { Debug.LogWarning("[QA] Esta cena não tem GameManager. Abra Assets/Scenes/Game.unity e rode de novo."); return; }
        var go = new GameObject("AutoJogoQA");
        DontDestroyOnLoad(go);
        go.AddComponent<AutoJogoQA>().Configurar(pedido);
    }

    // =====================================================================
    // Estado
    // =====================================================================

    readonly List<PlanoMissao> _plano = new List<PlanoMissao>();
    readonly List<Registro> _reg = new List<Registro>();
    readonly Dictionary<string, string> _prefsAntes = new Dictionary<string, string>();
    readonly StringBuilder _diario = new StringBuilder();
    System.Random _rng;
    int _semente, _oks, _fotos;
    float _velocidade = 2f;
    string _pasta;
    bool _pararPlayNoFim = true;

    GameManager _gm;
    Movimento _mov;
    CharacterController _cc;
    PlanoMissao _missaoAtual;
    int _nMissao;
    float _inicioReal;

    // vigia
    Vector3 _posAnterior, _posPet;
    bool _teleporteLiberado;
    Fase _faseVigia;
    float _faseDesde, _petLongeDesde = -1f;
    float _ySolo;
    int _petReapareceu;
    Transform _pet;

    void Configurar(string pedido)
    {
        _semente = Environment.TickCount & 0x7fffffff;
        foreach (var parte in pedido.Split(';'))
        {
            var kv = parte.Split('=');
            if (kv.Length == 2 && kv[0].Trim() == "semente" && int.TryParse(kv[1], out var s)) _semente = s;
            if (kv.Length == 2 && kv[0].Trim() == "velocidade" && float.TryParse(kv[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) _velocidade = Mathf.Clamp(v, 1f, 4f);
            if (kv.Length == 2 && kv[0].Trim() == "parar") _pararPlayNoFim = kv[1].Trim() != "0";
        }
        _rng = new System.Random(_semente);

        if (pedido.StartsWith("rapido"))
            _plano.Add(new PlanoMissao { Modo = Modo.Cadete, Perfil = Perfil.Aleatorio, Tutorial = false, TestarPausa = true, TestarIdioma = true, TestarFimDoTempo = true, TestarMovimentoTravado = true });
        else
        {
            _plano.Add(new PlanoMissao { Modo = Modo.Cadete, Perfil = Perfil.Cuidadoso, Tutorial = true, TestarMovimentoTravado = true });
            _plano.Add(new PlanoMissao { Modo = Modo.Comandante, Perfil = Perfil.Arriscado, Tutorial = false, TestarPausa = true, TestarIdioma = true });
            _plano.Add(new PlanoMissao { Modo = Modo.Cadete, Perfil = Perfil.Aleatorio, Tutorial = false, TestarFimDoTempo = true });
        }

        _pasta = Path.Combine(Application.dataPath, "..", "Temp", "QA", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(_pasta);
        foreach (var k in ChavesPrefs) if (PlayerPrefs.HasKey(k)) _prefsAntes[k] = PlayerPrefs.GetString(k, PlayerPrefs.GetInt(k, 0).ToString());
        PlayerPrefs.DeleteKey("tdm_seen"); // os cartões de primeira vez dos minijogos também entram no teste
        Debug.Log($"[QA] Auto jogar começou: {_plano.Count} missão(ões), semente {_semente}, velocidade {_velocidade}x. Relatório em {_pasta}");
    }

    void OnEnable() { Application.logMessageReceived += AoLog; }
    void OnDisable() { Application.logMessageReceived -= AoLog; }

    void AoLog(string msg, string pilha, LogType tipo)
    {
        if (msg.StartsWith("[QA]")) return;
        if (tipo == LogType.Error || tipo == LogType.Exception || tipo == LogType.Assert)
            Anotar(Nivel.Falha, "Erro no console", $"{tipo}: {msg} — {pilha.Split('\n').FirstOrDefault()}");
        else if (tipo == LogType.Warning && !msg.StartsWith("[GameManager] Estações"))
            Anotar(Nivel.Info, "Aviso no console", msg);
    }

    // =====================================================================
    // Registro e relatório
    // =====================================================================

    string Onde()
    {
        var m = _gm != null ? _gm.Missao : null;
        return $"M{_nMissao} {(_missaoAtual != null ? _missaoAtual.Modo + "/" + _missaoAtual.Perfil : "")} · Dia {(m != null ? m.Dia : 0)} · {(_gm != null ? _gm.Fase.ToString() : "-")}";
    }

    void Anotar(Nivel n, string cat, string texto)
    {
        var r = new Registro { Nivel = n, Categoria = cat, Onde = Onde(), Texto = texto };
        _reg.Add(r);
        if (n == Nivel.Falha && _fotos < 25)
        {
            r.Foto = $"falha_{++_fotos:00}.png";
            ScreenCapture.CaptureScreenshot(Path.Combine(_pasta, r.Foto));
        }
        if (n != Nivel.Info) Debug.Log($"[QA] {(n == Nivel.Falha ? "FALHA" : "ALERTA")} [{cat}] {r.Onde}: {texto}");
    }

    bool Checar(bool cond, string cat, string texto, Nivel seFalhar = Nivel.Falha)
    {
        if (cond) _oks++; else Anotar(seFalhar, cat, texto);
        return cond;
    }

    void Diario(string s) => _diario.AppendLine($"- `{Time.realtimeSinceStartup - _inicioReal,6:0.0}s` {Onde()} — {s}");

    void EscreverRelatorio()
    {
        var sb = new StringBuilder();
        int f = _reg.Count(r => r.Nivel == Nivel.Falha), a = _reg.Count(r => r.Nivel == Nivel.Alerta);
        sb.AppendLine("# Relatório do auto jogar (QA)");
        sb.AppendLine();
        sb.AppendLine($"- Data: {DateTime.Now:dd/MM/yyyy HH:mm} · semente `{_semente}` (repita com `semente={_semente}`) · velocidade {_velocidade}x");
        sb.AppendLine($"- Missões: {_nMissao} de {_plano.Count} · tempo real {(Time.realtimeSinceStartup - _inicioReal) / 60f:0.0} min");
        sb.AppendLine($"- **{f} falha(s)**, **{a} alerta(s)**, {_oks} checagens OK");
        sb.AppendLine();
        sb.AppendLine("FALHA = bug (trava, teleporte, erro, regra quebrada). ALERTA = algo que pode não fazer sentido para a criança ou para o conteúdo da NASA; precisa de um olhar humano.");
        foreach (var nivel in new[] { Nivel.Falha, Nivel.Alerta, Nivel.Info })
        {
            var lista = _reg.Where(r => r.Nivel == nivel).ToList();
            if (lista.Count == 0) continue;
            sb.AppendLine();
            sb.AppendLine(nivel == Nivel.Falha ? "## Falhas" : nivel == Nivel.Alerta ? "## Alertas" : "## Informações");
            foreach (var g in lista.GroupBy(r => r.Categoria))
            {
                sb.AppendLine();
                sb.AppendLine($"### {g.Key} ({g.Count()})");
                foreach (var r in g.Take(40))
                    sb.AppendLine($"- {r.Onde}: {r.Texto}{(r.Foto != null ? $" ([captura]({r.Foto}))" : "")}");
                if (g.Count() > 40) sb.AppendLine($"- … mais {g.Count() - 40}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("## Diário da partida");
        sb.AppendLine();
        sb.Append(_diario);
        var caminho = Path.Combine(_pasta, "relatorio.md");
        File.WriteAllText(caminho, sb.ToString());
        File.WriteAllText(Path.Combine(Application.dataPath, "..", "Temp", "QA", "ultimo.txt"), caminho);
        Debug.Log($"[QA] Fim: {f} falha(s), {a} alerta(s), {_oks} OK. Relatório: {Path.GetFullPath(caminho)}");
    }

    // =====================================================================
    // Laço principal
    // =====================================================================

    IEnumerator Start()
    {
        _inicioReal = Time.realtimeSinceStartup;
        yield return null;
        _gm = GameManager.Instancia;
        _mov = FindObjectOfType<Movimento>();
        _cc = _mov != null ? _mov.GetComponent<CharacterController>() : null;
        if (_gm == null || _mov == null || _cc == null) { Anotar(Nivel.Falha, "Cena", "GameManager ou astronauta (Movimento + CharacterController) não encontrado."); Encerrar(); yield break; }
        Idioma.Definir(Lingua.Pt);
        _ySolo = _cc.transform.position.y + _cc.center.y - _cc.height / 2f;
        _posAnterior = _mov.transform.position;

        ChecarConteudoEstatico();
        ConstruirGrade();

        for (_nMissao = 1; _nMissao <= _plano.Count; _nMissao++)
        {
            _missaoAtual = _plano[_nMissao - 1];
            yield return JogarMissao(_missaoAtual);
        }
        Encerrar();
    }

    void Encerrar()
    {
        Time.timeScale = 1f;
        if (_mov) _mov.entradaDepuracao = Vector2.zero;
        foreach (var k in ChavesPrefs) PlayerPrefs.DeleteKey(k);
        foreach (var kv in _prefsAntes)
        {
            if (k_inteiros.Contains(kv.Key) && int.TryParse(kv.Value, out var n)) PlayerPrefs.SetInt(kv.Key, n);
            else PlayerPrefs.SetString(kv.Key, kv.Value);
        }
        PlayerPrefs.Save();
        EscreverRelatorio();
        enabled = false;
        if (_pararPlayNoFim) EditorApplication.isPlaying = false;
    }
    static readonly HashSet<string> k_inteiros = new HashSet<string> { "tdm_som" };

    void Update()
    {
        if (_gm == null || _mov == null) return;
        if (!_gm.Pausado && Time.timeScale > 0f) Time.timeScale = _velocidade;
        Vigiar();
    }

    // =====================================================================
    // Vigia: trava, teleporte, queda, tempo parado, robô, câmera
    // =====================================================================

    static bool FaseAnda(Fase f) => f == Fase.AteTarefa || f == Fase.AteHub || f == Fase.CorridaAbrigo;

    void Vigiar()
    {
        var p = _mov.transform.position;
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        float passoMax = Mathf.Max(_mov.velocidadeCorrida, _mov.velocidade) * dt * 1.6f + 0.6f;
        float salto = Vector3.Distance(p, _posAnterior);
        if (!_teleporteLiberado && salto > passoMax)
            Anotar(Nivel.Falha, "Teleporte do personagem", $"o astronauta pulou {salto:0.0} m num quadro, de {_posAnterior:F1} para {p:F1}");
        if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z)) Anotar(Nivel.Falha, "Posição inválida", "posição do astronauta é NaN");
        if (p.y < _ySolo - 3f)
        {
            Anotar(Nivel.Falha, "Queda pelo chão", $"o astronauta caiu para y={p.y:0.0} (chão em {_ySolo:0.0})");
            Teleportar(PontoLivrePerto(_gm.Alvo != null ? _gm.Alvo.transform.position : _mov.transform.position));
        }
        _teleporteLiberado = false;
        _posAnterior = _mov.transform.position;

        // fase parada demais em fases que andam sozinhas
        if (_gm.Fase != _faseVigia) { _faseVigia = _gm.Fase; _faseDesde = Time.realtimeSinceStartup; }
        float naFase = (Time.realtimeSinceStartup - _faseDesde) * _velocidade;
        if (_gm.Fase == Fase.DiaAcontece && naFase > _gm.duracaoAnimacao + 6f && !_gm.Pausado)
        { Anotar(Nivel.Falha, "Travamento", $"'o dia acontece' passou de {naFase:0} s sem ir para o resultado"); _faseDesde = Time.realtimeSinceStartup; }

        if (!_gm.Pausado && Time.timeScale == 0f) Anotar(Nivel.Falha, "Tempo congelado", "Time.timeScale = 0 sem o jogo estar pausado");

        // robô de estimação
        if (_pet == null) { var rp = FindObjectOfType<RoboPet>(); if (rp) { _pet = rp.transform; _posPet = _pet.position; } }
        if (_pet != null)
        {
            if (Vector3.Distance(_pet.position, _posPet) > 3f) _petReapareceu++;
            _posPet = _pet.position;
            float d = Vector3.Distance(_pet.position, p);
            if (d > 12f) { if (_petLongeDesde < 0f) _petLongeDesde = Time.realtimeSinceStartup; else if (Time.realtimeSinceStartup - _petLongeDesde > 4f) { Anotar(Nivel.Alerta, "Robô de estimação", $"o robô ficou a {d:0} m do astronauta por mais de 4 s"); _petLongeDesde = Time.realtimeSinceStartup + 30f; } }
            else _petLongeDesde = -1f;
        }

        // câmera longe do personagem enquanto ele anda
        if (FaseAnda(_gm.Fase) && Camera.main != null && Vector3.Distance(Camera.main.transform.position, p) > 25f && naFase > 3f)
        { Anotar(Nivel.Alerta, "Câmera", $"câmera a {Vector3.Distance(Camera.main.transform.position, p):0} m do astronauta durante o deslocamento"); _faseDesde = Time.realtimeSinceStartup; }
    }

    void Teleportar(Vector3 destino)
    {
        _teleporteLiberado = true;
        _cc.enabled = false;
        _mov.transform.position = destino;
        _cc.enabled = true;
        _posAnterior = destino;
    }

    bool Congelado()
    {
        var f = typeof(Movimento).GetField("_congelado", BindingFlags.NonPublic | BindingFlags.Instance);
        return f != null && (bool)f.GetValue(_mov);
    }

    // =====================================================================
    // Uma missão
    // =====================================================================

    IEnumerator EsperarFaseMudar(Fase de, float limiteJogo, string oQue)
    {
        float t0 = Time.realtimeSinceStartup;
        while (_gm.Fase == de)
        {
            if ((Time.realtimeSinceStartup - t0) * _velocidade > limiteJogo)
            {
                Anotar(Nivel.Falha, "Travamento", $"{oQue}: a fase {de} não mudou em {limiteJogo:0} s");
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator JogarMissao(PlanoMissao pm)
    {
        if (_gm.Fase != Fase.Inicio) { _gm.JogarDeNovo(); yield return null; }
        if (_gm.TutorialLigado != pm.Tutorial) _gm.AlternarTutorial();
        Diario($"**Começa a missão {_nMissao}**: {pm.Modo}, jogador {pm.Perfil}, tutorial {(pm.Tutorial ? "ligado" : "desligado")}");
        _gm.Comecar(pm.Modo);
        yield return null;
        ChecarSorteioDaPartida(_gm.Missao);

        int diaVisto = 0, voltasMesmaFase = 0;
        Fase ultima = (Fase)(-1);
        bool testouPausa = false, testouIdioma = false, testouTempo = false, testouTravado = false;
        float tMissao = Time.realtimeSinceStartup;

        while (true)
        {
            var m = _gm.Missao;
            var fase = _gm.Fase;
            if (fase == ultima) voltasMesmaFase++; else { voltasMesmaFase = 0; ultima = fase; }
            if (voltasMesmaFase > 3 && !FaseAnda(fase) && fase != Fase.Minijogo && fase != Fase.DiaAcontece)
            { Anotar(Nivel.Falha, "Travamento", $"o botão desta fase não fez o jogo andar ({fase})"); yield break; }
            if ((Time.realtimeSinceStartup - tMissao) * _velocidade > 60f * 60f) { Anotar(Nivel.Falha, "Travamento", "a missão passou de 60 min de jogo sem terminar"); yield break; }

            switch (fase)
            {
                case Fase.ComoJogar:
                    for (int i = 0; i < 20 && _gm.Fase == Fase.ComoJogar; i++) { _gm.SlideProximo(); yield return null; }
                    Checar(_gm.Fase != Fase.ComoJogar, "Fluxo", "os slides de 'Como jogar' não terminam");
                    break;

                case Fase.Boletim:
                    if (m.Dia == diaVisto) { Anotar(Nivel.Falha, "Fluxo", $"o Boletim do Dia {m.Dia} apareceu duas vezes"); }
                    else Checar(m.Dia == diaVisto + 1, "Fluxo", $"o dia pulou de {diaVisto} para {m.Dia}");
                    diaVisto = m.Dia;
                    yield return new WaitForSecondsRealtime(0.4f);
                    ChecarBoletim(m);
                    _gm.SairDoBoletim();
                    yield return null;
                    break;

                case Fase.AteTarefa:
                case Fase.AteHub:
                    if (pm.TestarPausa && !testouPausa && fase == Fase.AteTarefa && m.Dia >= 2) { testouPausa = true; yield return TestarPausa(); break; }
                    if (pm.TestarIdioma && !testouIdioma && m.Dia >= 3) { testouIdioma = true; yield return TestarIdioma(); }
                    if (pm.TestarFimDoTempo && !testouTempo && fase == Fase.AteTarefa && m.Dia >= 4 && !m.Hoje.TarefaResolvida) { testouTempo = true; yield return TestarFimDoTempo(); break; }
                    yield return Andar(fase == Fase.AteTarefa ? "estação da tarefa" : "console do hub", false);
                    break;

                case Fase.CorridaAbrigo:
                    yield return Andar("abrigo (corrida)", true);
                    break;

                case Fase.Evento:
                    yield return new WaitForSecondsRealtime(0.3f);
                    yield return ResolverEvento(m, pm);
                    break;

                case Fase.Escolha:
                    if (pm.TestarMovimentoTravado && !testouTravado) { testouTravado = true; yield return TestarMovimentoTravado(); }
                    yield return new WaitForSecondsRealtime(0.25f);
                    DecidirTarefa(m, pm);
                    yield return null;
                    break;

                case Fase.MinijogoInfo:
                    Checar(TextoNaTela(Tarefas.Def(m.Hoje.Tarefa)?.MiniTitulo.Pt ?? "##"), "Interface", $"o cartão de primeira vez não mostra o título do minijogo de '{m.Hoje.Tarefa}'", Nivel.Alerta);
                    _gm.ComecarMinijogo();
                    yield return null;
                    break;

                case Fase.Minijogo:
                    yield return JogarMinijogo(m, pm);
                    break;

                case Fase.DecisaoVazamento:
                    if (Escolher(pm, 0.6f)) _gm.VazamentoReforcar(); else _gm.VazamentoDepois();
                    Diario("vazamento: " + (m.Vazamento ? "deixou para depois" : "reforçou"));
                    yield return null;
                    break;

                case Fase.TarefaConcluida:
                    ChecarTarefaConcluida(m);
                    yield return new WaitForSecondsRealtime(0.25f);
                    _gm.VoltarAoHub();
                    yield return null;
                    break;

                case Fase.Distribuir:
                    yield return Distribuir(m, pm);
                    break;

                case Fase.DiaAcontece:
                    yield return EsperarFaseMudar(Fase.DiaAcontece, _gm.duracaoAnimacao + 8f, "'o dia acontece'");
                    break;

                case Fase.Resultado:
                    yield return new WaitForSecondsRealtime(0.5f);
                    ChecarResultado(m);
                    _gm.Proximo();
                    yield return null;
                    break;

                case Fase.Relatorio:
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return ChecarRelatorio(m);
                    _gm.JogarDeNovo();
                    yield return null;
                    yield break;

                default:
                    Anotar(Nivel.Falha, "Fluxo", $"fase inesperada no meio da missão: {fase}");
                    yield break;
            }
            yield return null;
        }
    }

    bool Escolher(Perfil p, float chanceCuidadoso) => p == Perfil.Cuidadoso || (p == Perfil.Aleatorio && _rng.NextDouble() < chanceCuidadoso);
    bool Escolher(PlanoMissao pm, float chance) => Escolher(pm.Perfil, chance);

    // =====================================================================
    // Andar de verdade (A* numa grade da base, sem teleporte)
    // =====================================================================

    IEnumerator Andar(string destino, bool corrida)
    {
        var fase = _gm.Fase;
        float t0 = Time.realtimeSinceStartup, ultimoProgresso = t0, contagemInicial = _gm.ContagemCorrida;
        float melhor = float.MaxValue;
        int replanejou = 0;
        List<Vector3> caminho = null;
        int i = 0;
        Estacao alvoPlanejado = null;
        float limiteJogo = corrida ? 60f : 120f;

        while (_gm.Fase == fase)
        {
            var alvo = _gm.Alvo;
            if (alvo == null) { yield return null; if ((Time.realtimeSinceStartup - t0) * _velocidade > 5f) { Anotar(Nivel.Falha, "Navegação", $"sem destino (seta) na fase {fase}"); break; } continue; }

            if (caminho == null || alvo != alvoPlanejado)
            {
                alvoPlanejado = alvo;
                caminho = Caminho(_mov.transform.position, alvo.transform.position);
                i = 0;
                if (caminho == null) { Anotar(Nivel.Alerta, "Navegação", $"não achei caminho andando até {alvo.name} ({destino}); pode ser porta fechada ou estação fora da base"); caminho = new List<Vector3> { alvo.transform.position }; }
                if (corrida && caminho != null)
                {
                    float metros = Comprimento(caminho);
                    float segundosAndando = metros / Mathf.Max(0.1f, _mov.velocidade);
                    Checar(segundosAndando <= contagemInicial, "Conteúdo/jogabilidade",
                        $"corrida até o abrigo: o caminho tem {metros:0} m ({segundosAndando:0} s andando, sem correr) e a contagem dá {contagemInicial:0} s. Uma criança andando não chega a tempo.", Nivel.Alerta);
                }
            }

            if (_gm.Proxima != null)
            {
                _mov.entradaDepuracao = Vector2.zero;
                if (fase != Fase.CorridaAbrigo) _gm.Interagir();
                yield return null;
                if (_gm.Fase == fase && fase == Fase.AteHub && !_gm.Missao.Hoje.TarefaResolvida)
                    Anotar(Nivel.Falha, "Fluxo", "cheguei ao console com a tarefa ainda aberta, e a seta apontava para o console");
                if (_gm.Fase == fase && fase != Fase.CorridaAbrigo) { yield return null; if (_gm.Fase == fase) Anotar(Nivel.Falha, "Fluxo", $"interagir perto de {alvo.name} não abriu nada"); break; }
                continue;
            }

            // segue o próximo ponto do caminho
            while (i < caminho.Count - 1 && Plano(caminho[i] - _mov.transform.position).magnitude < 0.6f) i++;
            Vector3 dir = Plano(caminho[Mathf.Min(i, caminho.Count - 1)] - _mov.transform.position);
            Dirigir(dir);

            float dist = Plano(alvo.transform.position - _mov.transform.position).magnitude;
            if (dist < melhor - 0.3f) { melhor = dist; ultimoProgresso = Time.realtimeSinceStartup; }
            if ((Time.realtimeSinceStartup - ultimoProgresso) * _velocidade > 4f)
            {
                replanejou++;
                ultimoProgresso = Time.realtimeSinceStartup;
                caminho = Caminho(_mov.transform.position, alvo.transform.position) ?? caminho;
                i = 0;
                if (replanejou >= 3)
                {
                    Anotar(Nivel.Falha, "Personagem preso", $"o astronauta ficou preso indo para {alvo.name} ({destino}) perto de {_mov.transform.position:F1}; levei ele até lá para continuar");
                    Teleportar(PontoLivrePerto(alvo.transform.position));
                    replanejou = 0;
                }
            }
            if ((Time.realtimeSinceStartup - t0) * _velocidade > limiteJogo)
            { Anotar(Nivel.Falha, "Travamento", $"não cheguei a {destino} em {limiteJogo:0} s de jogo"); Teleportar(PontoLivrePerto(alvo.transform.position)); t0 = Time.realtimeSinceStartup; }

            if (FaseAnda(_gm.Fase) && Congelado()) { Anotar(Nivel.Falha, "Controles", $"o astronauta está congelado numa fase de andar ({_gm.Fase})"); break; }
            yield return null;
        }
        _mov.entradaDepuracao = Vector2.zero;
        if (corrida) Diario($"corrida até o abrigo terminou com {_gm.ContagemCorrida:0.0} s sobrando de {contagemInicial:0.0} s");
    }

    static Vector3 Plano(Vector3 v) { v.y = 0f; return v; }

    static float Comprimento(List<Vector3> c) { float s = 0; for (int k = 1; k < c.Count; k++) s += Vector3.Distance(c[k - 1], c[k]); return s; }

    /// <summary>Converte a direção no mundo para a entrada do analógico, relativa ao giro que o Movimento usa para andar.</summary>
    void Dirigir(Vector3 dirMundo)
    {
        if (dirMundo.sqrMagnitude < 1e-4f) { _mov.entradaDepuracao = Vector2.zero; return; }
        float yaw = _mov.Yaw;
        if (_mov.cameraSegueAtras && _mov.travarDirecaoAoAndar)
        {
            var f = typeof(Movimento).GetField("_yawTravado", BindingFlags.NonPublic | BindingFlags.Instance);
            var andando = typeof(Movimento).GetField("_andando", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && andando != null && (bool)andando.GetValue(_mov)) yaw = (float)f.GetValue(_mov);
        }
        var local = Quaternion.Euler(0f, -yaw, 0f) * dirMundo.normalized;
        _mov.entradaDepuracao = new Vector2(local.x, local.z);
    }

    // ---------- grade de navegação ----------

    const float Cel = 0.5f;
    Vector3 _origem;
    int _nx, _nz;
    bool[] _livre;

    void ConstruirGrade()
    {
        var pontos = FindObjectsOfType<Estacao>().Select(e => e.transform.position).ToList();
        pontos.Add(_mov.transform.position);
        var min = pontos.Aggregate(Vector3.Min) - new Vector3(12f, 0f, 12f);
        var max = pontos.Aggregate(Vector3.Max) + new Vector3(12f, 0f, 12f);
        _origem = new Vector3(min.x, _ySolo, min.z);
        _nx = Mathf.CeilToInt((max.x - min.x) / Cel); _nz = Mathf.CeilToInt((max.z - min.z) / Cel);
        _livre = new bool[_nx * _nz];
        float r = _cc.radius + 0.05f, h = _cc.height;
        int livres = 0;
        for (int z = 0; z < _nz; z++)
            for (int x = 0; x < _nx; x++)
            {
                var p = Celula(x, z);
                bool chao = Physics.Raycast(p + Vector3.up * 1f, Vector3.down, out var hit, 2.5f, ~0, QueryTriggerInteraction.Ignore) && Ignorar(hit.collider) == false;
                bool ok = chao && !Physics.OverlapCapsule(p + Vector3.up * (r + 0.35f), p + Vector3.up * (h - r), r, ~0, QueryTriggerInteraction.Ignore).Any(c => !Ignorar(c));
                _livre[z * _nx + x] = ok;
                if (ok) livres++;
            }
        Diario($"grade de navegação: {_nx}×{_nz} células de {Cel} m, {livres} livres");
        foreach (var e in FindObjectsOfType<Estacao>())
            if (!string.IsNullOrEmpty(e.id) && Caminho(_mov.transform.position, e.transform.position) == null)
                Anotar(Nivel.Alerta, "Navegação", $"a estação '{e.id}' ({e.name}) parece não ter caminho andando a partir do início");
    }

    bool Ignorar(Collider c) => c == null || c.isTrigger || c.transform.IsChildOf(_mov.transform) || (_pet != null && c.transform.IsChildOf(_pet))
        || c.GetComponentInParent<RoboPet>() != null || c.GetComponentInParent<Porta>() != null || c.GetComponentInParent<PortasAutomaticas>() != null;

    Vector3 Celula(int x, int z) => _origem + new Vector3((x + 0.5f) * Cel, 0f, (z + 0.5f) * Cel);
    bool Dentro(int x, int z) => x >= 0 && z >= 0 && x < _nx && z < _nz;
    bool Livre(int x, int z) => Dentro(x, z) && _livre[z * _nx + x];
    void Indice(Vector3 p, out int x, out int z) { x = Mathf.FloorToInt((p.x - _origem.x) / Cel); z = Mathf.FloorToInt((p.z - _origem.z) / Cel); }

    bool LivreMaisPerto(Vector3 p, out int bx, out int bz)
    {
        Indice(p, out bx, out bz);
        if (Livre(bx, bz)) return true;
        for (int raio = 1; raio < 16; raio++)
            for (int dz = -raio; dz <= raio; dz++)
                for (int dx = -raio; dx <= raio; dx++)
                    if ((Mathf.Abs(dx) == raio || Mathf.Abs(dz) == raio) && Livre(bx + dx, bz + dz)) { bx += dx; bz += dz; return true; }
        return false;
    }

    Vector3 PontoLivrePerto(Vector3 p)
    {
        if (_livre != null && LivreMaisPerto(p, out var x, out var z)) return Celula(x, z) + Vector3.up * (_cc.height / 2f - _cc.center.y + 0.05f);
        return p;
    }

    List<Vector3> Caminho(Vector3 de, Vector3 para)
    {
        if (_livre == null || !LivreMaisPerto(de, out int sx, out int sz) || !LivreMaisPerto(para, out int tx, out int tz)) return null;
        int n = _nx * _nz, ini = sz * _nx + sx, fim = tz * _nx + tx;
        var g = new float[n]; var veio = new int[n]; var fechado = new bool[n];
        for (int k = 0; k < n; k++) { g[k] = float.MaxValue; veio[k] = -1; }
        var aberto = new SortedSet<(float, int)>();
        g[ini] = 0; aberto.Add((0f, ini));
        while (aberto.Count > 0)
        {
            var atual = aberto.Min; aberto.Remove(atual);
            int c = atual.Item2;
            if (c == fim) break;
            if (fechado[c]) continue;
            fechado[c] = true;
            int cx = c % _nx, cz = c / _nx;
            for (int dz = -1; dz <= 1; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0) continue;
                    int nx = cx + dx, nz = cz + dz;
                    if (!Livre(nx, nz) || (dx != 0 && dz != 0 && (!Livre(cx + dx, cz) || !Livre(cx, cz + dz)))) continue;
                    int v = nz * _nx + nx;
                    float custo = g[c] + (dx != 0 && dz != 0 ? 1.414f : 1f);
                    if (custo < g[v])
                    {
                        g[v] = custo; veio[v] = c;
                        float hh = Mathf.Sqrt((nx - tx) * (nx - tx) + (nz - tz) * (nz - tz));
                        aberto.Add((custo + hh + v * 1e-7f, v));
                    }
                }
        }
        if (veio[fim] < 0 && fim != ini) return null;
        var pts = new List<Vector3>();
        for (int k = fim; k >= 0; k = veio[k]) { pts.Add(Celula(k % _nx, k / _nx)); if (k == ini) break; }
        pts.Reverse();
        pts.Add(para);
        return pts;
    }

    // =====================================================================
    // Decisões do jogador
    // =====================================================================

    IEnumerator ResolverEvento(Missao m, PlanoMissao pm)
    {
        var def = Eventos.Def(m.Hoje.Evento);
        if (!Checar(def != null, "Evento", $"evento '{m.Hoje.Evento}' sem definição")) yield break;
        Checar(TextoNaTela(def.Titulo.Pt), "Interface", $"o cartão do evento '{def.Id}' não mostra o título");
        Checar(!def.Fonte.Vazio && def.Fonte.Pt.Contains("NASA"), "Conteúdo NASA", $"o evento '{def.Id}' não cita fonte da NASA no cartão", Nivel.Alerta);
        Checar(TextoNaTela(def.Fonte.Pt.Length > 20 ? def.Fonte.Pt.Substring(0, 20) : def.Fonte.Pt), "Conteúdo NASA", $"a fonte do evento '{def.Id}' não aparece na tela", Nivel.Alerta);
        ChecarSemAoVivo("cartão do evento " + def.Id);

        var disp = def.Escolhas.Where(e => Eventos.Disponivel(m, e)).ToList();
        var indisp = def.Escolhas.Where(e => !Eventos.Disponivel(m, e)).ToList();
        if (!Checar(disp.Count > 0, "Evento", $"o evento '{def.Id}' não tem nenhuma escolha disponível: a criança fica sem saída")) yield break;
        if (indisp.Count > 0)
        {
            _gm.EscolherEvento(indisp[0].Id);
            yield return null;
            Checar(_gm.Fase == Fase.Evento && !m.Hoje.EventoResolvido, "Evento", $"a escolha indisponível '{indisp[0].Id}' do evento '{def.Id}' foi aceita");
            Checar(!indisp[0].MotivoIndisponivel.Vazio, "Interface", $"a escolha indisponível '{indisp[0].Id}' não explica por quê", Nivel.Alerta);
        }

        var escolha = pm.Perfil == Perfil.Cuidadoso ? disp[0] : pm.Perfil == Perfil.Arriscado ? disp[disp.Count - 1] : disp[_rng.Next(disp.Count)];
        float tempoAntes = _gm.TempoRestante;
        _gm.EscolherEvento(escolha.Id);
        yield return null;
        Checar(m.Hoje.EventoResolvido, "Evento", $"escolher '{escolha.Id}' não resolveu o evento '{def.Id}'");
        Checar(_gm.Fase != Fase.Evento, "Travamento", $"o cartão do evento '{def.Id}' não fechou depois da escolha");
        if (escolha.Segundos > 0 && _gm.Fase != Fase.CorridaAbrigo && tempoAntes > escolha.Segundos + 1f)
            Checar(_gm.TempoRestante <= tempoAntes - escolha.Segundos + 1f, "Regras", $"'{escolha.Id}' devia custar {escolha.Segundos:0} s do relógio");
        Diario($"evento **{def.Id}** ({(m.Hoje.EventoNaIda ? "ida" : "volta")}): escolheu '{escolha.Id}'");
    }

    void DecidirTarefa(Missao m, PlanoMissao pm)
    {
        var d = Tarefas.Def(m.Hoje.Tarefa);
        Checar(d != null, "Tarefa", $"tarefa '{m.Hoje.Tarefa}' sem definição");
        if (d == null) { _gm.EscolherFicar(); return; }
        Checar(TextoNaTela(d.Titulo.Pt), "Interface", $"a escolha da tarefa '{d.Id}' não mostra o título", Nivel.Alerta);
        if (!d.FazerCusto.Vazio) Checar(TextoNaTela(d.FazerCusto.Pt), "Interface", $"o custo de 'fazer' da tarefa '{d.Id}' não aparece", Nivel.Alerta);
        if (!d.Fonte.Vazio) Checar(TextoNaTela(d.Fonte.Pt), "Conteúdo NASA", $"a fonte da tarefa '{d.Id}' não aparece na escolha", Nivel.Alerta);
        bool pode = Tarefas.PodeFazer(m);
        if (d.Fora && m.O2 <= 1) Checar(!pode, "Regras", "com Oxigênio 1 ou menos, a saída da base devia ficar bloqueada");
        bool fazer = pode && (pm.Perfil == Perfil.Arriscado || (pm.Perfil == Perfil.Cuidadoso ? !(d.Fora && m.O2 <= 3) : _rng.NextDouble() < 0.65));
        if (fazer) _gm.EscolherFazer(); else _gm.EscolherFicar();
        Diario($"tarefa **{d.Id}**: {(fazer ? "fazer" : "ficar")} (O₂ {m.O2}, rad {m.Rad})");
    }

    IEnumerator JogarMinijogo(Missao m, PlanoMissao pm)
    {
        var mg = _gm.MinigameAtual;
        if (!Checar(mg != null, "Minijogo", "fase de minijogo sem minijogo aberto")) yield break;
        while (mg.EmDemonstracao) yield return null;
        float relogio = _gm.TempoRestante;
        float jogar = Mathf.Min(mg.duracao * 0.6f, 0.5f + (float)_rng.NextDouble() * 2.5f);
        float t0 = Time.realtimeSinceStartup;
        while ((Time.realtimeSinceStartup - t0) * _velocidade < jogar && _gm.Fase == Fase.Minijogo && !mg.Terminando) yield return null;
        Checar(Mathf.Abs(_gm.TempoRestante - relogio) < 0.01f, "Regras", "o relógio do dia andou durante o minijogo");

        float nota = pm.Perfil == Perfil.Cuidadoso ? 0.75f + (float)_rng.NextDouble() * 0.25f
            : pm.Perfil == Perfil.Arriscado ? (float)_rng.NextDouble() * 0.5f : (float)_rng.NextDouble();
        if (!mg.Terminando)
        {
            var encerrar = typeof(Minigame).GetMethod("Encerrar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (encerrar != null) encerrar.Invoke(mg, new object[] { nota, nota >= 0.5f });
            else _gm.CancelarMinijogo();
        }
        int o2 = m.O2, rad = m.Rad;
        yield return EsperarFaseMudar(Fase.Minijogo, Minigame.DuracaoFaixa + 6f, "faixa de resultado do minijogo");
        Checar(m.O2 >= 0 && m.O2 <= Missao.MaxO2, "Regras", $"Oxigênio fora de 0..10 depois do minijogo: {m.O2}");
        Checar(m.Rad == rad, "Regras", "um minijogo mudou a radiação");
        Diario($"minijogo {m.Hoje.Tarefa}: nota {nota:0.00} → {Limpo(m.Hoje.Tarefa == Tarefas.Vazamento ? m.Hoje.MsgVazamento.Pt : m.Hoje.MsgTarefa.Pt)}");
    }

    IEnumerator Distribuir(Missao m, PlanoMissao pm)
    {
        yield return new WaitForSecondsRealtime(0.3f);
        var T = m.Hoje;
        var mods = RegrasMissao.ModulosVisiveis(m);
        Checar(m.Vazamento == mods.Contains(Modulo.Conserto), "Regras", "o módulo Conserto devia aparecer só com vazamento aberto");
        foreach (var mod in mods) for (int k = 0; k < 6 && T.Aloc[mod] > 0; k++) _gm.Menos(mod);
        Checar(T.Aloc.Total == 0, "Distribuição", "não deu para tirar todas as células com o botão −");

        int total = RegrasMissao.CelulasTotais(m);
        bool alerta = m.AlertaDe(m.Dia) != null;
        bool sombraAmanha = m.EhSombra(m.Dia + 1);
        var alvo = new Dictionary<Modulo, int>();
        if (m.Vazamento) alvo[Modulo.Conserto] = 2;
        alvo[Modulo.Ar] = m.O2 <= 4 ? 3 : 2;
        if (alerta) alvo[Modulo.Abrigo] = pm.Perfil == Perfil.Arriscado ? 0 : pm.Perfil == Perfil.Cuidadoso ? (RegrasMissao.AbrigoPreparadoHoje(m) ? 1 : 2) : _rng.Next(3);
        if (pm.Perfil != Perfil.Arriscado) alvo[Modulo.Estufa] = m.Comida <= 3 ? 2 : 1;
        if (sombraAmanha && pm.Perfil == Perfil.Cuidadoso) alvo[Modulo.Bateria] = 2;
        foreach (var kv in alvo) for (int k = 0; k < kv.Value; k++) _gm.Mais(kv.Key);
        for (int k = 0; k < 10 && RegrasMissao.CelulasLivres(m) > 0; k++) { if (RegrasMissao.PodeMais(m, Modulo.Lab)) _gm.Mais(Modulo.Lab); else if (RegrasMissao.PodeMais(m, Modulo.Bateria)) _gm.Mais(Modulo.Bateria); else break; }
        foreach (var mod in mods) Checar(T.Aloc[mod] <= RegrasMissao.Maximo(mod), "Regras", $"o módulo {mod} passou do limite");
        Checar(RegrasMissao.CelulasLivres(m) >= 0, "Regras", "a distribuição usou mais células do que tinha");
        Checar(total >= 0, "Regras", $"células totais negativas: {total}");
        if (m.Contato) Checar(RegrasMissao.Previa(m).Count > 0, "Interface", "a dica do controle da missão está vazia com contato");
        if (alerta && T.Aloc[Modulo.Abrigo] == 0 && m.Contato)
            Checar(RegrasMissao.Previa(m).Any(t => t.Pt == Textos.PreviaAbrigo0.Pt), "Conteúdo/jogabilidade", "dia de alerta com Abrigo 0: a dica não avisa do risco", Nivel.Alerta);

        _antesDoDia = new int[] { m.O2, m.Rad, m.Bateria, m.Estrelas, m.Comida, T.Aloc[Modulo.Abrigo], RegrasMissao.AbrigoEfetivo(m) };
        Diario($"energia: {total} células → " + string.Join(", ", mods.Select(x => $"{x} {T.Aloc[x]}")) + (alerta ? " · alerta hoje" : "") + (sombraAmanha ? " · sombra amanhã" : ""));
        yield return new WaitForSecondsRealtime(0.2f);
        _gm.ComecarODia();
        yield return null;
    }
    int[] _antesDoDia;

    // =====================================================================
    // Testes extras (pausa, idioma, fim do tempo, personagem parado)
    // =====================================================================

    IEnumerator TestarPausa()
    {
        float tr = _gm.TempoRestante;
        _gm.Pausar();
        yield return new WaitForSecondsRealtime(0.6f);
        Checar(_gm.Pausado && Time.timeScale == 0f, "Pausa", "a pausa não parou o tempo do jogo");
        Checar(Mathf.Abs(_gm.TempoRestante - tr) < 0.01f, "Pausa", "o relógio do dia andou com o jogo pausado");
        Checar(Congelado(), "Pausa", "o astronauta não ficou parado na pausa");
        _gm.Retomar();
        yield return null;
        Checar(!_gm.Pausado && Time.timeScale > 0f, "Pausa", "'Continuar' não tirou a pausa");
        Diario("teste de pausa feito");
    }

    IEnumerator TestarIdioma()
    {
        Idioma.Alternar();
        yield return new WaitForSecondsRealtime(0.4f);
        var textos = TextosNaTela();
        var pt = new[] { " você ", " não ", "oxigênio", "energia ", " hoje", "tarefa" };
        var sobra = textos.Where(t => pt.Any(w => (" " + t.ToLowerInvariant() + " ").Contains(w))).Take(5).ToList();
        Checar(sobra.Count == 0, "Idioma", "em inglês ainda aparece texto em português: " + string.Join(" | ", sobra.Select(Limpo)));
        Idioma.Alternar();
        yield return null;
        Diario("teste de idioma feito (inglês e volta)");
    }

    IEnumerator TestarFimDoTempo()
    {
        var m = _gm.Missao;
        _gm.DepurarTempo(1.5f);
        float t0 = Time.realtimeSinceStartup;
        while (_gm.Fase == Fase.AteTarefa && (Time.realtimeSinceStartup - t0) * _velocidade < 8f) yield return null;
        if (_gm.Fase == Fase.Evento) yield return ResolverEvento(m, _missaoAtual);
        Checar(m.Hoje.TarefaResolvida && m.Hoje.TempoEsgotou, "Regras", "o tempo acabou antes da tarefa e ela não valeu como 'ficar'");
        Checar(_gm.Fase == Fase.Distribuir || _gm.Fase == Fase.Evento || _gm.Fase == Fase.CorridaAbrigo, "Fluxo", $"o tempo acabou e o jogo foi para {_gm.Fase} em vez da distribuição");
        Diario("teste de fim do tempo feito");
    }

    IEnumerator TestarMovimentoTravado()
    {
        var p = _mov.transform.position;
        _mov.entradaDepuracao = new Vector2(1f, 0f);
        yield return new WaitForSecondsRealtime(0.4f);
        _mov.entradaDepuracao = Vector2.zero;
        Checar(Plano(_mov.transform.position - p).magnitude < 0.05f, "Controles", "o astronauta andou na tela de escolha (devia estar parado)");
    }

    // =====================================================================
    // Checagens de conteúdo e regras
    // =====================================================================

    static readonly Regex AoVivoRuim = new Regex(@"(?<!não é |não são |not )\b(ao vivo|live data|live feed|em tempo real|real[- ]time|agora no sol|right now on the sun)\b", RegexOptions.IgnoreCase);

    void ChecarSemAoVivo(string onde)
    {
        foreach (var t in TextosNaTela())
            if (AoVivoRuim.IsMatch(t) && !t.ToLowerInvariant().Contains("não é ao vivo") && !t.ToLowerInvariant().Contains("not live"))
                Anotar(Nivel.Falha, "Conteúdo NASA", $"{onde}: texto apresenta dado arquivado como ao vivo: \"{Limpo(t)}\"");
    }

    void ChecarConteudoEstatico()
    {
        // ----- dados do DONKI -----
        var flr = Resources.Load<TextAsset>("ErupcoesDONKI");
        if (Checar(flr != null, "Dados NASA", "Resources/ErupcoesDONKI.json não existe"))
        {
            var ids = new HashSet<string>();
            int c = 0, mm = 0, x = 0;
            foreach (Match rec in Regex.Matches(flr.text, @"\{[^{}]*""classLabel""[^{}]*\}"))
            {
                string Campo(string n) { var mt = Regex.Match(rec.Value, "\"" + n + "\"\\s*:\\s*\"?([^\",}]*)"); return mt.Success ? mt.Groups[1].Value.Trim() : ""; }
                string id = Campo("id"), cls = Campo("classLabel"), ini = Campo("startTime"), url = Campo("sourceUrl"), ret = Campo("retrievedOn"), sep = Campo("sepId"), sepUrl = Campo("sepUrl"), sepTime = Campo("sepTime");
                int.TryParse(Campo("gameIntensity"), out int gi);
                ids.Add(id);
                char letra = cls.Length > 0 ? cls[0] : '?';
                if (letra == 'C') c++; else if (letra == 'M') mm++; else if (letra == 'X') x++;
                int esperado = letra == 'X' ? 2 : letra == 'M' ? 1 : letra == 'C' ? 0 : -1;
                Checar(gi == esperado, "Dados NASA", $"erupção {id}: classe {cls} com intensidade de jogo {gi} (C=0, M=1, X=2)");
                Checar(id.Length >= 10 && ini.StartsWith(id.Substring(0, 10)), "Dados NASA", $"erupção {id}: a data do id não bate com startTime {ini}");
                Checar(url.StartsWith("https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/"), "Dados NASA", $"erupção {id}: link do registro estranho ({url})");
                Checar(!string.IsNullOrEmpty(ret), "Dados NASA", $"erupção {id}: sem data de consulta (retrievedOn)");
                if (DateTime.TryParse(ini.Replace("Z", ""), out var di) && DateTime.TryParse(ret, out var dr))
                    Checar(di <= dr, "Dados NASA", $"erupção {id}: o evento ({di:dd/MM/yyyy}) é depois da data de consulta ({dr:dd/MM/yyyy})");
                if (!string.IsNullOrEmpty(sep))
                {
                    Checar(sepUrl.StartsWith("https://ccmc.gsfc.nasa.gov/DONKI/view/SEP/"), "Dados NASA", $"erupção {id}: SEP sem link do DONKI");
                    if (DateTime.TryParse(ini.Replace("Z", ""), out var a) && DateTime.TryParse(sepTime.Replace("Z", ""), out var b))
                        Checar(b >= a && (b - a).TotalDays <= 4, "Dados NASA", $"erupção {id}: o SEP ligado ({sepTime}) não vem logo depois da erupção ({ini})", Nivel.Alerta);
                }
            }
            Checar(c + mm + x == 20, "Dados NASA", $"o catálogo tem {c + mm + x} erupções (Docs/DADOS-NASA.md fala em 20)", Nivel.Alerta);
            Checar(c == 5 && mm == 10 && x == 5, "Dados NASA", $"proporção C:M:X é {c}:{mm}:{x} (a curadoria diz 5:10:5)", Nivel.Alerta);

            var cme = Resources.Load<TextAsset>("CmesDONKI");
            if (cme != null)
                foreach (Match rec in Regex.Matches(cme.text, @"\{[^{}]*""speed""[^{}]*\}"))
                {
                    string Campo(string n) { var mt = Regex.Match(rec.Value, "\"" + n + "\"\\s*:\\s*\"?([^\",}]*)"); return mt.Success ? mt.Groups[1].Value.Trim() : ""; }
                    string id = Campo("id"), lig = Campo("linkedFlr"), url = Campo("sourceUrl");
                    int.TryParse(Campo("speed"), out int vel);
                    Checar(vel > 100 && vel < 4000, "Dados NASA", $"CME {id}: velocidade {vel} km/s fora do plausível");
                    Checar(url.StartsWith("https://ccmc.gsfc.nasa.gov/DONKI/view/CME/"), "Dados NASA", $"CME {id}: link do registro estranho");
                    if (!string.IsNullOrEmpty(lig)) Checar(ids.Contains(lig), "Dados NASA", $"CME {id}: ligada à erupção {lig}, que não está no catálogo do jogo", Nivel.Alerta);
                }
        }

        // ----- textos: PT e EN, fonte e "ao vivo" -----
        foreach (var tipo in new[] { typeof(Textos), typeof(Tarefas), typeof(Eventos) })
            foreach (var f in tipo.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(f => f.FieldType == typeof(Txt)))
            {
                var t = (Txt)f.GetValue(null);
                if (t.Vazio) continue;
                Checar(!string.IsNullOrWhiteSpace(t.En) && !string.IsNullOrWhiteSpace(t.Pt), "Idioma", $"{tipo.Name}.{f.Name} está sem português ou sem inglês");
                if (t.Pt.Length > 25) Checar(t.Pt != t.En, "Idioma", $"{tipo.Name}.{f.Name} tem o mesmo texto em português e inglês", Nivel.Alerta);
            }

        foreach (var d in Tarefas.Defs)
        {
            foreach (var t in new[] { d.Nome, d.Titulo, d.Texto, d.FazerTitulo, d.FicarTitulo, d.MiniTitulo, d.MiniInstrucao })
                if (!t.Vazio) Checar(!string.IsNullOrWhiteSpace(t.En), "Idioma", $"tarefa '{d.Id}' com texto sem inglês: \"{Limpo(t.Pt)}\"");
            bool citaFato = Regex.IsMatch(d.Texto.Pt ?? "", @"NASA|Estação Espacial|astronautas fazem|\d+\s*(horas|dias|segundos|minutos|%)");
            if (citaFato) Checar(!d.Fonte.Vazio, "Conteúdo NASA", $"a tarefa '{d.Id}' cita um fato real sem fonte: \"{Limpo(d.Texto.Pt)}\"", Nivel.Alerta);
            if (d.Id != Tarefas.Vazamento) Checar(!d.FazerCusto.Vazio && !d.FicarCusto.Vazio, "Regras", $"a tarefa '{d.Id}' não mostra o custo das duas escolhas");
        }

        foreach (var e in Eventos.Defs)
        {
            Checar(e.Conferido, "Conteúdo NASA", $"o evento '{e.Id}' não está marcado como conferido (sai do sorteio)", Nivel.Info);
            Checar(!e.Fonte.Vazio, "Conteúdo NASA", $"o evento '{e.Id}' não tem fonte");
            Checar(e.Escolhas.Count >= 2, "Regras", $"o evento '{e.Id}' tem menos de 2 escolhas: não é uma decisão");
            foreach (var s in e.Escolhas) Checar(s.Custo != null, "Regras", $"a escolha '{e.Id}/{s.Id}' não mostra custo");
            if (e.Escolhas.Count >= 2 && e.Escolhas.All(s => s.Segundos == 0 && s.Aplicar == null))
                Anotar(Nivel.Alerta, "Conteúdo/jogabilidade", $"no evento '{e.Id}' nenhuma escolha muda algo na hora: verifique se a decisão tem peso");
        }

        foreach (var (nome, url, uso) in Textos.FontesNasa)
        {
            Checar(url.StartsWith("https://"), "Créditos", $"fonte '{nome}' sem https");
            Checar(Regex.IsMatch(url, @"^https://([a-z0-9-]+\.)*nasa\.gov/"), "Créditos", $"fonte '{nome}' não é de um site nasa.gov ({url})", Nivel.Alerta);
            Checar(!uso.Vazio && !string.IsNullOrWhiteSpace(uso.En), "Créditos", $"fonte '{nome}' sem explicação do uso em PT e EN");
        }
        foreach (var e in Eventos.Defs.Where(e => !e.Fonte.Vazio))
        {
            var chave = Regex.Match(e.Fonte.Pt, @"Fonte:\s*([^,.(]+)").Groups[1].Value.Trim();
            if (chave.Length > 4) Checar(Textos.FontesNasa.Any(f => f.nome.IndexOf(chave.Split(' ').Last(), StringComparison.OrdinalIgnoreCase) >= 0 || f.nome.IndexOf(chave, StringComparison.OrdinalIgnoreCase) >= 0),
                "Créditos", $"a fonte do evento '{e.Id}' (\"{chave}\") não aparece na tela de créditos", Nivel.Alerta);
        }
        Checar(Missao.Dose[0] < Missao.Dose[1] && Missao.Dose[1] < Missao.Dose[2], "Conteúdo NASA", "a dose da classe C, M e X não é crescente");
        Diario("checagem estática do conteúdo feita");
    }

    void ChecarSorteioDaPartida(Missao m)
    {
        var M = m.M;
        Checar(m.Sombra.All(d => d >= 4 && d <= 9) && Math.Abs(m.Sombra[0] - m.Sombra[1]) >= 2, "Regras", $"dias de sombra inválidos: {m.Sombra[0]} e {m.Sombra[1]}");
        Checar(m.AlertaDe(1) == null, "Regras", "o Dia 1 tem alerta de erupção");
        Checar(m.Alertas.Count == M.Alertas, "Regras", $"{m.Alertas.Count} alertas (o modo pede {M.Alertas})");
        int reais = m.Alertas.Values.Count(a => a.Erupcao != null);
        Checar(reais == Math.Min(M.Reais, M.Alertas), "Regras", $"{reais} erupções reais (o modo pede {M.Reais})");
        Checar(m.Alertas.Values.Any(a => a.Erupcao?.Intensidade == 2), "Regras", "nenhuma erupção forte (X) na partida", Nivel.Alerta);
        for (int i = 1; i < m.Tarefas.Count; i++) Checar(m.Tarefas[i] != m.Tarefas[i - 1], "Regras", $"tarefa repetida nos dias {i} e {i + 1}: {m.Tarefas[i]}");
        foreach (var g in m.Tarefas.GroupBy(t => t)) Checar(g.Count() <= 2, "Regras", $"a tarefa {g.Key} aparece {g.Count()} vezes");
        if (m.Tutorial) Checar(m.Tarefas[0] == Tarefas.Amostra, "Regras", "com tutorial, o Dia 1 devia ser a coleta de pedras");

        foreach (var kv in m.Alertas.Where(k => k.Value.Erupcao != null))
        {
            var e = kv.Value.Erupcao;
            if (e.Intensidade == 2 && !kv.Value.Forte)
                Anotar(Nivel.Alerta, "Conteúdo NASA", $"Dia {kv.Key}: o alerta é FRACO, mas a erupção real é {e.Classe} (classe X, a mais forte). A criança pode achar que o alerta mentiu.");
            if (!e.TemSep)
                Anotar(Nivel.Alerta, "Conteúdo NASA", $"Dia {kv.Key}: a erupção {e.Classe} de {e.Data} não tem SEP ligado no DONKI, mas o jogo vai dizer que as partículas vieram (o cartão cita o catálogo SEP)");
        }
        if (m.CmeRegistro != null)
        {
            Checar(m.CmeChegada == m.CmeDia + Eventos.DiasAteChegada, "Regras", "a CME não chega 2 dias depois do aviso");
            var al = m.AlertaDe(m.CmeChegada);
            Checar(al != null && al.Erupcao != null && al.Forte, "Regras", $"a CME chega no Dia {m.CmeChegada}, que não tem alerta forte com erupção real");
            if (m.CmeLigada && al?.Erupcao != null)
                Checar(m.CmeRegistro.FlrLigado == al.Erupcao.Id, "Conteúdo NASA", $"a CME {m.CmeRegistro.Id} é mostrada como ligada à erupção, mas o DONKI liga ela a {m.CmeRegistro.FlrLigado}, não a {al.Erupcao.Id}");
            if (!m.CmeLigada) Anotar(Nivel.Info, "Conteúdo NASA", $"a CME do Dia {m.CmeDia} não é a ligada à erupção do Dia {m.CmeChegada} (o texto diz \"como uma que saiu\")");
        }
        Diario($"sorteio: sombra nos dias {m.Sombra[0]} e {m.Sombra[1]}; alertas em {string.Join(", ", m.Alertas.Keys.OrderBy(k => k).Select(k => k + (m.Alertas[k].Erupcao != null ? $"({m.Alertas[k].Erupcao.Classe}{(m.Alertas[k].Forte ? " forte" : " fraco")})" : "(falso)")))}"
            + (m.CmeRegistro != null ? $"; CME avisada no dia {m.CmeDia}" : "") + $"; tarefas: {string.Join(", ", m.Tarefas)}");
    }

    void ChecarBoletim(Missao m)
    {
        var T = m.Hoje;
        Checar(TextoNaTela(Textos.Boletim.Pt) || TextoNaTela(Textos.Boletim.En), "Interface", "o Boletim não mostra o título");
        if (m.EhSombra(m.Dia)) Checar(T.Luz <= m.M.Sombra + Math.Max(0, T.Fit) + Math.Max(0, m.AjusteDe(m.Dia)), "Regras", $"dia de sombra com {T.Luz} células de luz");
        if (m.AlertaDe(m.Dia) != null) Checar(T.Evento == Eventos.Particulas, "Regras", $"dia de alerta com evento '{T.Evento}' em vez do alarme de partículas");
        Checar(!string.IsNullOrEmpty(T.Evento) || _gm.semEventos, "Regras", "dia sem evento sorteado", Nivel.Alerta);
        ChecarSemAoVivo("Boletim");
        ChecarRecursos(m, "Boletim");
    }

    void ChecarTarefaConcluida(Missao m)
    {
        var T = m.Hoje;
        Checar(T.TarefaResolvida, "Fluxo", "tela de tarefa concluída com a tarefa não resolvida");
        Checar(!T.MsgTarefa.Vazio, "Interface", $"a tarefa '{T.Tarefa}' terminou sem frase de efeito");
        if (!T.Extra.Vazio)
        {
            Checar(!T.ExtraFonte.Vazio, "Conteúdo NASA", $"o 'Você sabia?' da tarefa '{T.Tarefa}' está sem fonte");
            Checar(TextoNaTela(Textos.VoceSabia.Pt) || TextoNaTela(Textos.VoceSabia.En), "Interface", "o 'Você sabia?' não aparece na tela", Nivel.Alerta);
        }
        ChecarRecursos(m, "tarefa concluída");
    }

    void ChecarResultado(Missao m)
    {
        var r = m.Hoje.Res;
        if (!Checar(r != null, "Fluxo", "resultado do dia sem dados")) return;
        ChecarRecursos(m, "resultado");
        Checar(r.Linhas.Count > 0, "Interface", "o resultado do dia não tem nenhuma linha explicando o que houve");
        int dRad = m.Rad - r.RadAntes;
        Checar(dRad >= 0, "Regras", "a radiação diminuiu de um dia para o outro");
        var al = m.AlertaDe(m.Dia);
        if (al == null) Checar(dRad == 0, "Regras", $"dia sem alerta, mas a radiação subiu {dRad}");
        if (r.FalsoAlarme) Checar(dRad == 0, "Regras", "alarme falso deu radiação");
        if (r.Erupcao != null && _antesDoDia != null)
        {
            var e = r.Erupcao;
            int abrigoEf = _antesDoDia[6];
            bool protegido = abrigoEf >= 2 && (m.Hoje.Alarme == Alarme.Abrigo || m.Hoje.Alarme == Alarme.Nenhum);
            if (protegido) Checar(r.DRad == 0, "Conteúdo/jogabilidade", $"Abrigo com 2 células e a tripulação no abrigo, mas a erupção {e.Classe} deu radiação +{r.DRad}");
            if (abrigoEf == 0 && m.Hoje.Alarme == Alarme.Continuou) Checar(r.DRad >= Missao.Dose[e.Intensidade], "Conteúdo/jogabilidade", $"sem abrigo e continuando o trabalho, a erupção {e.Classe} deu só +{r.DRad}");
            Checar(TextoNaTela(e.Classe), "Conteúdo NASA", $"o cartão 'Isso aconteceu de verdade' não mostra a classe {e.Classe}");
            Checar(TextosNaTela().Any(t => t.Contains("DONKI")), "Conteúdo NASA", "o resultado com erupção não mostra a fonte NASA DONKI");
            Checar(TextosNaTela().Any(t => t.ToLowerInvariant().Contains("arquivado") || t.ToLowerInvariant().Contains("archived")), "Conteúdo NASA", "o cartão da erupção não diz que o dado é arquivado");
            Checar(!string.IsNullOrEmpty(e.Url) && e.Url.StartsWith("https://ccmc.gsfc.nasa.gov/"), "Conteúdo NASA", "o cartão da erupção não tem link do registro no DONKI");
            Diario($"erupção real {e.Classe} ({e.Data}): abrigo {_antesDoDia[5]} (efetivo {abrigoEf}), alarme {m.Hoje.Alarme}, radiação +{r.DRad}");
        }
        ChecarSemAoVivo("resultado");
        var fimEsperado = m.O2 <= 0 ? Fim.Oxigenio : m.Rad >= Missao.MaxRad ? Fim.Radiacao : r.Fim == Fim.Comida ? Fim.Comida : m.Dia >= Missao.Dias ? Fim.Completa : Fim.Nenhum;
        Checar(m.Fim == fimEsperado, "Regras", $"fim da missão {m.Fim}, esperado {fimEsperado} (O₂ {m.O2}, rad {m.Rad}, dia {m.Dia})");
        Checar(m.Hist.Count == m.Dia, "Regras", $"histórico com {m.Hist.Count} dias no Dia {m.Dia}");
    }

    IEnumerator ChecarRelatorio(Missao m)
    {
        Checar(!TextosNaTela().Any(t => t.ToUpperInvariant().Contains("GAME OVER")), "Conteúdo/jogabilidade", "o relatório usa 'GAME OVER' (a regra é falha sem punição)");
        int nota = RegrasMissao.Nota(m);
        if (m.Fim == Fim.Completa) Checar(m.Dia == Missao.Dias, "Regras", $"missão completa no Dia {m.Dia}");
        else Checar(nota == 0, "Regras", "missão interrompida com nota maior que 0");
        Checar(RegrasMissao.Momentos(m).Count <= 3, "Regras", "o relatório tem mais de 3 momentos");
        Checar(!RegrasMissao.DicaFim(m).Vazio, "Interface", "o relatório não dá dica para a próxima missão");
        Diario($"**fim da missão {_nMissao}**: {m.Fim} no Dia {m.Dia}, nota {nota}, estrelas {m.Estrelas}, O₂ {m.O2}, rad {m.Rad}, robô reapareceu {_petReapareceu}x");
        if (_petReapareceu > 15) Anotar(Nivel.Alerta, "Robô de estimação", $"o robô reapareceu {_petReapareceu} vezes nesta missão (fica preso com frequência?)");
        _petReapareceu = 0;

        _gm.AbrirCreditos();
        yield return new WaitForSecondsRealtime(0.5f);
        Checar(TextosNaTela().Any(t => t.Contains("DONKI")), "Créditos", "a tela de créditos não lista o NASA DONKI");
        _gm.FecharCreditos();
        yield return null;
    }

    void ChecarRecursos(Missao m, string onde)
    {
        Checar(m.O2 >= 0 && m.O2 <= Missao.MaxO2, "Regras", $"{onde}: Oxigênio {m.O2} fora de 0..10");
        Checar(m.Rad >= 0 && m.Rad <= Missao.MaxRad, "Regras", $"{onde}: Radiação {m.Rad} fora de 0..10");
        Checar(m.Bateria >= 0 && m.Bateria <= Missao.MaxBat, "Regras", $"{onde}: Bateria {m.Bateria} fora de 0..4");
        Checar(m.Comida >= 0 && m.Estrelas >= 0, "Regras", $"{onde}: Comida ({m.Comida}) ou Estrelas ({m.Estrelas}) negativas");
    }

    // ---------- textos da tela ----------

    static List<string> TextosNaTela() => FindObjectsOfType<Text>().Where(t => t.isActiveAndEnabled && !string.IsNullOrEmpty(t.text)).Select(t => t.text).ToList();

    static bool TextoNaTela(string trecho)
    {
        if (string.IsNullOrEmpty(trecho)) return true;
        string busca = Limpo(trecho);
        if (busca.Length > 30) busca = busca.Substring(0, 30);
        int chave = busca.IndexOf('{');
        if (chave > 0) busca = busca.Substring(0, chave);
        return TextosNaTela().Any(t => Limpo(t).Contains(busca));
    }

    static string Limpo(string s) => Regex.Replace(s ?? "", "<.*?>", "").Replace("\n", " ").Trim();
}
#endif
