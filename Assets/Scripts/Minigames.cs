using System.Collections.Generic;
using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Fábrica: devolve o minigame de cada tarefa (tabela D3 do design da mudança fluxo-web-no-3d).</summary>
public static class Minigames
{
    static Txt T(string pt, string en) => new Txt(pt, en);

    public static Minigame Criar(string tarefa, float limite)
    {
        Minigame m;
        switch (tarefa)
        {
            case Tarefas.Amostra:
                m = new MinigameLixo(4, T("VOLTAR", "BACK"), T("Caixa de amostras", "Sample box"), pedras: true, botaoSempreAtivo: true);
                break;
            case Tarefas.Braco:
                m = new MinigameLixo(1, T("PEGAR", "GRAB"), T("Garra", "Claw"), pedras: false, botaoSempreAtivo: false);
                break;
            case Tarefas.Poeira:
                m = new MinigameRaspadinha(filtro: false);
                break;
            case Tarefas.Filtros:
                m = new MinigameRaspadinha(filtro: true);
                break;
            case Tarefas.Agua:
                m = new MinigameCanos();
                break;
            case Tarefas.Upload:
                m = new MinigameEsteira(3, errarDesfaz: false, rotulo: T("Pacotes enviados", "Packets sent"), sinal: true);
                break;
            case Tarefas.Exercicio:
                m = new MinigameEsteira(5, errarDesfaz: true, rotulo: T("Passos no ritmo", "Steps in rhythm"), sinal: false);
                break;
            case Tarefas.Estufa:
                m = new MinigameDisjuntores(ModoDisjuntor.Estufa);
                break;
            case Tarefas.Antena:
                m = new MinigameDisjuntores(ModoDisjuntor.Antena);
                break;
            default:
                m = new MinigameSegurar();
                break;
        }
        var d = Tarefas.Def(tarefa);
        if (d != null) { m.titulo = d.MiniTitulo; m.instrucao = d.MiniInstrucao; }
        m.tarefa = tarefa;
        m.duracao = limite;
        return m;
    }
}

// =====================================================================================
// 1) ESTEIRA — ritmo: apertar no momento certo (ESPAÇO / A / toque) na zona verde.
//    Exercício (5 acertos, errar desfaz) e transmissão de dados (3 pacotes, errar não desfaz).
//    A zona e a velocidade guardam as frações da versão antiga: zona com 15% da largura, ponteiro a 0,65 larguras/s.
// =====================================================================================
public class MinigameEsteira : Minigame
{
    public override Mecanica Mecanica => Mecanica.Esteira;

    const float Largura = 680f;               // trilho (era 1000)
    const float ZonaLarg = Largura * 0.15f;   // 150 de 1000
    const float Vel = Largura * 0.65f;        // 650 de 1000 por segundo
    const float YTrilho = 70f, AltoTrilho = 96f;
    readonly int _alvo;
    readonly bool _errarDesfaz, _sinal;
    readonly Txt _rotulo;
    int _acertos;
    float _x, _dir = 1f, _zonaX;
    RectTransform _cursor, _zona;
    FormaUI[] _pips;
    Text _contador;
    Color _cor;

    public MinigameEsteira(int acertos, bool errarDesfaz, Txt rotulo, bool sinal)
    {
        _alvo = acertos; _errarDesfaz = errarDesfaz; _rotulo = rotulo; _sinal = sinal;
    }

    protected override float Progresso() => _acertos / (float)_alvo;

    protected override void Construir()
    {
        _acertos = 0; _x = -Largura / 2f; _dir = 1f;
        _cor = UiTema.CorDaTarefa(tarefa);

        // Cenário: a Terra que recebe os pacotes, ou a lona da esteira.
        if (_sinal)
        {
            Icone("terra", new Vector2(Largura / 2f - 40f, 210f), 96);
            for (int i = 0; i < 3; i++)
            {
                var onda = Forma(FormaUI.Modo.Anel, new Vector2(-Largura / 2f + 60f, 210f), Vector2.one * (60f + i * 46f), UiTema.Apoio);
                onda.espessura = 6; onda.fracao = 0.25f; onda.rotacao = 0f;
                onda.rectTransform.localRotation = Quaternion.Euler(0, 0, -45f);
            }
            Icone("mod_lab", new Vector2(-Largura / 2f + 60f, 210f), 56);
        }

        Placa(new Vector2(0, YTrilho), new Vector2(Largura + 28f, AltoTrilho + 28f), UiTema.AcoClaro, UiTema.ChanfroBotao);
        Chanfro(new Vector2(0, YTrilho), new Vector2(Largura, AltoTrilho), UiTema.Vazio, 6f);
        var lona = Forma(FormaUI.Modo.Hachurada, new Vector2(0, YTrilho), new Vector2(Largura, AltoTrilho), UiTema.Aco);
        lona.periodo = _sinal ? 18f : 30f; lona.larguraFaixa = _sinal ? 4f : 9f;
        // Tira de luz na cor do sistema, por cima do trilho (a assinatura dos painéis).
        Chanfro(new Vector2(0, YTrilho + AltoTrilho / 2f + 14f - 4f), new Vector2(Largura, 8f), _cor, 0f);
        // A zona alvo é verde, como diz a instrução.
        _zona = Chanfro(Vector2.zero, new Vector2(ZonaLarg, AltoTrilho), UiTema.Estufa, 4f).rectTransform;
        _zona.anchoredPosition = new Vector2(0, YTrilho);

        _cursor = No("Ponteiro", new Vector2(_x, YTrilho), new Vector2(44f, 170f));
        Chanfro(new Vector2(0, -6f), new Vector2(8f, 128f), UiTema.Branco, 0f, _cursor);
        var tri = Forma(FormaUI.Modo.Triangulo, new Vector2(0, 70f), new Vector2(44f, 36f), UiTema.Branco, _cursor);
        tri.rotacao = 180f;

        _pips = new FormaUI[_alvo];
        float passo = 92f;
        for (int i = 0; i < _alvo; i++)
        {
            var pos = new Vector2((i - (_alvo - 1) / 2f) * passo, -110f);
            var sombra = Poligono(pos + new Vector2(0, -5f), new Vector2(70, 70), UiTema.Sombra, 6, 0f, 1);
            sombra.rotacao = 30f;
            _pips[i] = Poligono(pos, new Vector2(70, 70), UiTema.AcoClaro, 6, 0f, 1);
            _pips[i].rotacao = 30f;
        }
        _contador = Rotulo("", Papel.Rotulo, new Vector2(0, -190f), new Vector2(700, 40), UiTema.Apoio);
        AtualizarContador();
        NovaZona();
    }

    void AtualizarContador() => _contador.text = UiTema.Caixa(_rotulo + "  " + Textos.XDeY.F(_acertos, _alvo));

    void NovaZona()
    {
        _zonaX = Random.Range(-Largura / 2f + ZonaLarg, Largura / 2f - ZonaLarg);
        _zona.anchoredPosition = new Vector2(_zonaX, YTrilho);
    }

    protected override void PontosDemo(out Vector2 de, out Vector2 para) { de = new Vector2(0, -20f); para = de; }

    protected override void Atualizar()
    {
        _x += _dir * Vel * Time.deltaTime;
        if (_x > Largura / 2f) { _x = Largura / 2f; _dir = -1f; }
        if (_x < -Largura / 2f) { _x = -Largura / 2f; _dir = 1f; }
        _cursor.anchoredPosition = new Vector2(_x, YTrilho);
        if (Input.GetKeyDown(KeyCode.Space) || Ponteiro.Down) Tocar();
    }

    /// <summary>Só para a sonda: põe o ponteiro no meio da zona alvo.</summary>
    public void PorNaZona() { _x = _zonaX; _cursor.anchoredPosition = new Vector2(_x, YTrilho); }

    /// <summary>O toque (ESPAÇO, A, clique): acerto se o ponteiro está na zona.</summary>
    public void Tocar()
    {
        if (Terminando) return;
        if (Mathf.Abs(_x - _zonaX) <= ZonaLarg / 2f)
        {
            var pip = _pips[_acertos];
            pip.color = _cor;
            Pulso(pip);
            Pulso(_zona);
            Sons.Acerto();
            Flutua("+1", new Vector2(_zonaX, YTrilho + 90f), _cor);
            _acertos++;
            AtualizarContador();
            if (_acertos >= _alvo) { Vencer(); return; }
            NovaZona();
        }
        else
        {
            // Erro: só apaga o último pip (no exercício). Sem cor nem som de erro.
            Sons.Toque();
            if (_errarDesfaz && _acertos > 0)
            {
                _acertos--;
                _pips[_acertos].color = UiTema.AcoClaro;
                AtualizarContador();
            }
        }
    }

    public FormaUI Pip(int i) => _pips[i];
    public int Acertos => _acertos;
}

// =====================================================================================
// 2) RASPADINHA — esfregar a poeira (painéis solares / filtros de ar). Nota = fração limpa.
//    6 por 4 células de 120 (eram 10 por 4 de 130). O esforço por célula foi recalculado para a mesma jogada dar a
//    mesma nota da versão anterior (simulação de vaivém: diferença de até 4 pontos).
// =====================================================================================
public class MinigameRaspadinha : Minigame
{
    public override Mecanica Mecanica => Mecanica.Raspadinha;

    const int Cols = 6, Lins = 4;
    const float Celula = 120f, Raio = 97f;    // o raio guarda a proporção da célula (105 de 130)
    const float Esforco = 640f;               // unidades esfregadas dentro do raio para limpar uma célula
    readonly bool _filtro;
    float[] _sujeira;
    bool[] _limpa;
    Vector2[] _centros;
    FormaUI[][] _manchas;
    Color[][] _corBase;
    Vector2 _posAntes;
    bool _segurava;

    public MinigameRaspadinha(bool filtro) { _filtro = filtro; }

    protected override float Progresso()
    {
        if (_sujeira == null) return 0f;
        float s = 0f;
        foreach (var v in _sujeira) s += v;
        return 1f - s / _sujeira.Length;
    }

    protected override void Construir()
    {
        int n = Cols * Lins;
        _sujeira = new float[n]; _limpa = new bool[n]; _centros = new Vector2[n];
        _manchas = new FormaUI[n][]; _corBase = new Color[n][];
        float w = Cols * Celula, h = Lins * Celula;
        Placa(Vector2.zero, new Vector2(w + 30f, h + 30f), UiTema.AcoClaro, UiTema.ChanfroPainel);
        Chanfro(Vector2.zero, new Vector2(w, h), _filtro ? UiTema.FiltroTrama : UiTema.PainelSolarJunta, 6f);
        for (int i = 0; i < n; i++)
        {
            int c = i % Cols, l = i / Cols;
            _centros[i] = new Vector2((c - (Cols - 1) / 2f) * Celula, (l - (Lins - 1) / 2f) * Celula);
            _sujeira[i] = 1f;
            if (_filtro)
            {
                Chanfro(_centros[i], new Vector2(Celula - 6f, Celula - 6f), UiTema.Filtro, 3f);
                var trama = Forma(FormaUI.Modo.Hachurada, _centros[i], new Vector2(Celula - 22f, Celula - 22f), UiTema.FiltroTrama);
                trama.periodo = 12f; trama.larguraFaixa = 3f;
            }
            else
            {
                Chanfro(_centros[i], new Vector2(Celula - 6f, Celula - 6f), UiTema.PainelSolar, 3f);
                Chanfro(_centros[i] + new Vector2(0, Celula / 2f - 16f), new Vector2(Celula - 26f, 3f), UiTema.PainelSolarJunta, 0f);
            }
        }
        // Manchas de regolito: polígonos irregulares, um grande e um pequeno por célula, que se sobrepõem.
        for (int i = 0; i < n; i++)
        {
            var g = Poligono(_centros[i] + new Vector2(Random.Range(-12f, 12f), Random.Range(-12f, 12f)),
                new Vector2(Random.Range(128f, 150f), Random.Range(116f, 140f)), UiTema.Regolito, Random.Range(7, 10), 0.35f, Random.Range(1, 9999),
                null, Random.Range(0f, 360f));
            var p = Poligono(_centros[i] + new Vector2(Random.Range(-36f, 36f), Random.Range(-36f, 36f)),
                new Vector2(Random.Range(46f, 70f), Random.Range(40f, 60f)), UiTema.Chao, Random.Range(5, 8), 0.3f, Random.Range(1, 9999),
                null, Random.Range(0f, 360f));
            _manchas[i] = new[] { g, p };
            _corBase[i] = new[] { g.color, p.color };
        }
        _posAntes = PosLocal; _segurava = false;
    }

    protected override void PontosDemo(out Vector2 de, out Vector2 para) { de = new Vector2(-250f, 150f); para = new Vector2(250f, -150f); }

    protected override void Atualizar()
    {
        Vector2 p = PosLocal;
        if (!Ponteiro.Held) { _segurava = false; _posAntes = p; return; }
        float mov = _segurava ? (p - _posAntes).magnitude : 0f; // só esfregar (mover) limpa
        _segurava = true; _posAntes = p;
        float limpa = mov / Esforco;
        int sujas = 0;
        bool acertou = false;
        for (int i = 0; i < _sujeira.Length; i++)
        {
            if (_sujeira[i] > 0f && (p - _centros[i]).magnitude < Raio)
            {
                _sujeira[i] = Mathf.Max(0f, _sujeira[i] - limpa);
                // A mancha clareia enquanto some.
                for (int k = 0; k < 2; k++)
                {
                    var c = Color.Lerp(_corBase[i][k], UiTema.PedraClara, (1f - _sujeira[i]) * 0.6f);
                    c.a = _limpa[i] ? 0f : _sujeira[i];
                    _manchas[i][k].color = c;
                }
                if (!_limpa[i] && _sujeira[i] <= 0.08f)
                {
                    _limpa[i] = true;
                    foreach (var m in _manchas[i]) { var c = m.color; c.a = 0f; m.color = c; }
                    Brilho(_centros[i]);
                    acertou = true;
                }
            }
            if (_sujeira[i] > 0.08f) sujas++;
        }
        if (acertou) Sons.Acerto();
        if (sujas == 0) Vencer();
    }

    /// <summary>Brilho rápido na célula que acabou de ficar limpa.</summary>
    void Brilho(Vector2 pos)
    {
        var f = Chanfro(pos, new Vector2(Celula - 6f, Celula - 6f), new Color(1f, 1f, 1f, 0.55f), 3f);
        f.gameObject.AddComponent<Esvai>().Iniciar(0.25f);
        Pulso(f);
    }
}

/// <summary>Some (alfa até 0) e se destrói. Para o brilho das células limpas.</summary>
public class Esvai : MonoBehaviour
{
    float _t0, _dur, _a0;
    Graphic _g;
    public void Iniciar(float dur) { _t0 = Time.unscaledTime; _dur = dur; _g = GetComponent<Graphic>(); _a0 = _g.color.a; }
    void Update()
    {
        float k = (Time.unscaledTime - _t0) / _dur;
        if (k >= 1f) { Destroy(gameObject); return; }
        var c = _g.color; c.a = _a0 * (1f - k); _g.color = c;
    }
}

// =====================================================================================
// 3) ARRASTAR — itens flutuando (gravidade baixa) para a caixa, depois apertar o botão.
//    Coleta de pedras (botão VOLTAR sempre ativo; nota = pedras na caixa) e braço robótico (1 caixa, PEGAR).
// =====================================================================================
public class MinigameLixo : Minigame
{
    public override Mecanica Mecanica => Mecanica.Lixo;

    readonly Txt _botao, _alvoRotulo;
    readonly int _qtd;
    readonly bool _botaoSempre, _pedras;
    class Peca { public RectTransform rt; public Vector2 vel; public bool dentro; }
    List<Peca> _pecas;
    Peca _segurando;
    RectTransform _alvo, _botaoRt, _botaoFaceRt;
    FormaUI _botaoFace;
    Text _botaoTexto, _contador;
    bool? _botaoAtivo;

    const float Meio = 55f;                                     // meia-largura da peça: 110 de lado (mínimo 88)
    static readonly Vector2 PosAlvo = new Vector2(0f, 30f), TamAlvo = new Vector2(240f, 200f);
    const float XMax = 325f, YMin = -140f, YMax = 225f;        // a peça inteira fica dentro da área, acima do botão
    const float VelMin = 100f, VelMax = 165f;                   // 120 a 200 na área antiga, mais larga
    static readonly Vector2 PosBotao = new Vector2(0f, -228f), TamBotao = new Vector2(360f, 88f);

    public MinigameLixo(int qtd, Txt botao, Txt alvoRotulo, bool pedras, bool botaoSempreAtivo)
    {
        _qtd = qtd; _botao = botao; _alvoRotulo = alvoRotulo; _pedras = pedras; _botaoSempre = botaoSempreAtivo;
    }

    int Dentro() => _pecas == null ? 0 : _pecas.FindAll(x => x.dentro).Count;
    protected override float Progresso() => Dentro() / (float)_qtd;
    protected override bool SairConclui => _botaoSempre;

    protected override void Construir()
    {
        _segurando = null;
        var cor = UiTema.CorDaTarefa(tarefa);
        if (_pedras)
        {
            // Chão de regolito com crateras rasas.
            Chanfro(Vector2.zero, TamanhoArea, UiTema.Chao, UiTema.ChanfroPainel);
            Poligono(new Vector2(-250f, -120f), new Vector2(170f, 60f), UiTema.ChaoClaro, 9, 0.2f, 11);
            Poligono(new Vector2(270f, 175f), new Vector2(120f, 46f), UiTema.ChaoClaro, 8, 0.2f, 12);
            Poligono(new Vector2(230f, -100f), new Vector2(90f, 34f), UiTema.ChaoClaro, 7, 0.2f, 13);
            Poligono(new Vector2(-280f, 200f), new Vector2(80f, 30f), UiTema.ChaoClaro, 7, 0.2f, 14);
            // Caixa de amostras chanfrada, com a tira laranja.
            _alvo = Placa(PosAlvo, TamAlvo, UiTema.AcoClaro, UiTema.ChanfroBotao, null, "Caixa");
            Chanfro(Vector2.zero, TamAlvo - new Vector2(28f, 28f), UiTema.Vazio, 6f, _alvo);
            Chanfro(new Vector2(0, TamAlvo.y / 2f - 4f), new Vector2(TamAlvo.x - 2f * UiTema.ChanfroBotao, 8f), cor, 0f, _alvo);
        }
        else
        {
            // Espaço, com a garra descendo do braço e a área de pegar tracejada.
            Chanfro(Vector2.zero, TamanhoArea, UiTema.Pagina, UiTema.ChanfroPainel);
            var rnd = new System.Random(7);
            for (int i = 0; i < 26; i++)
            {
                float d = 3f + (float)rnd.NextDouble() * 4f;
                var c = UiTema.Apoio; c.a = 0.35f + (float)rnd.NextDouble() * 0.5f;
                Circulo(new Vector2((float)rnd.NextDouble() * 720f - 360f, (float)rnd.NextDouble() * 520f - 260f), d, c);
            }
            Chanfro(new Vector2(0, 230f), new Vector2(26f, 120f), UiTema.AcoClaro, 0f);
            Chanfro(new Vector2(0, 168f), new Vector2(120f, 22f), UiTema.AcoClaro, 4f);
            var dedoE = Forma(FormaUI.Modo.Triangulo, new Vector2(-50f, 142f), new Vector2(30f, 36f), UiTema.AcoClaro); dedoE.rotacao = 180f;
            var dedoD = Forma(FormaUI.Modo.Triangulo, new Vector2(50f, 142f), new Vector2(30f, 36f), UiTema.AcoClaro); dedoD.rotacao = 180f;
            _alvo = No("Alvo", PosAlvo, TamAlvo);
            var borda = Forma(FormaUI.Modo.Moldura, Vector2.zero, TamAlvo, cor, _alvo);
            borda.espessura = 4f; borda.larguraFaixa = 16f; borda.periodo = 26f;
        }
        Rotulo(_alvoRotulo.ToString(), Papel.Rotulo, PosAlvo + new Vector2(0, -TamAlvo.y / 2f - 22f), new Vector2(320, 34), UiTema.Branco);
        _contador = Rotulo("", Papel.Numero, PosAlvo + new Vector2(TamAlvo.x / 2f + 70f, 0f), new Vector2(120, 44), UiTema.Branco);

        // Botão de terminar (VOLTAR ou PEGAR), com corpo e sombra.
        _botaoRt = No("Botao", PosBotao, TamBotao);
        Chanfro(new Vector2(0, -UiTema.Sombra_Y), TamBotao, UiTema.Sombra, UiTema.ChanfroBotao, _botaoRt);
        _botaoFace = Chanfro(Vector2.zero, TamBotao, UiTema.Branco, UiTema.ChanfroBotao, _botaoRt);
        _botaoFaceRt = _botaoFace.rectTransform;
        _botaoTexto = Rotulo(_botao.ToString(), Papel.TituloPainel, Vector2.zero, TamBotao, UiTema.Tinta, _botaoFaceRt);

        _pecas = new List<Peca>();
        for (int i = 0; i < _qtd; i++)
        {
            Vector2 pos = new Vector2(i % 2 == 0 ? Random.Range(-XMax, -200f) : Random.Range(200f, XMax), Random.Range(-110f, 200f));
            var rt = No(_pedras ? "Pedra" : "Caixa", pos, Vector2.one * Meio * 2f);
            if (_pedras)
            {
                int s = Random.Range(1, 9999);
                Poligono(new Vector2(0, -6f), new Vector2(104f, 96f), UiTema.Sombra, 7, 0.25f, s, rt);
                Poligono(Vector2.zero, new Vector2(104f, 96f), UiTema.Pedra, 7, 0.25f, s, rt);
                Poligono(new Vector2(-12f, 14f), new Vector2(52f, 44f), UiTema.PedraClara, 5, 0.2f, s + 1, rt);
            }
            else
            {
                Chanfro(new Vector2(0, -6f), new Vector2(104f, 96f), UiTema.Sombra, 8f, rt);
                Chanfro(Vector2.zero, new Vector2(104f, 96f), cor, 8f, rt);
                Chanfro(new Vector2(0, -26f), new Vector2(104f, 12f), Escuro(cor, 0.75f), 0f, rt);
                Icone("comida", new Vector2(0, 8f), 56f, rt);
            }
            _pecas.Add(new Peca { rt = rt, vel = Random.insideUnitCircle.normalized * Random.Range(VelMin, VelMax) });
        }
        AtualizarContador();
        PintarBotao();
    }

    void AtualizarContador() => _contador.text = Textos.XDeY.F(Dentro(), _qtd).ToString();

    void PintarBotao()
    {
        bool ativo = _botaoSempre || (_pecas != null && _pecas.TrueForAll(x => x.dentro));
        if (_botaoAtivo == ativo) return;
        _botaoAtivo = ativo;
        _botaoFace.color = ativo ? UiTema.Branco : UiTema.DesativadoFace;
        _botaoTexto.color = ativo ? UiTema.Tinta : UiTema.Discreto;
    }

    protected override void PontosDemo(out Vector2 de, out Vector2 para)
    {
        de = new Vector2(-260f, 120f);
        if (_pecas != null) foreach (var p in _pecas) if (!p.dentro) { de = p.rt.anchoredPosition; break; }
        para = PosAlvo;
    }

    protected override void Atualizar()
    {
        Vector2 p = PosLocal;
        if (Ponteiro.Down && _segurando == null)
            foreach (var pc in _pecas)
                if (!pc.dentro && Mathf.Abs(p.x - pc.rt.anchoredPosition.x) < Meio && Mathf.Abs(p.y - pc.rt.anchoredPosition.y) < Meio)
                {
                    _segurando = pc; pc.rt.SetAsLastSibling(); pc.rt.localScale = Vector3.one * 1.06f; Sons.Toque(); break;
                }

        foreach (var pc in _pecas)
        {
            if (pc.dentro) continue;
            if (pc == _segurando) { pc.rt.anchoredPosition = p; continue; }
            Vector2 pos = pc.rt.anchoredPosition + pc.vel * Time.deltaTime;
            if (pos.x > XMax || pos.x < -XMax) { pc.vel.x = -pc.vel.x; pos.x = Mathf.Clamp(pos.x, -XMax, XMax); }
            if (pos.y > YMax || pos.y < YMin) { pc.vel.y = -pc.vel.y; pos.y = Mathf.Clamp(pos.y, YMin, YMax); }
            pc.rt.anchoredPosition = pos;
        }

        if (Ponteiro.Up && _segurando != null)
        {
            Vector2 c = _segurando.rt.anchoredPosition - PosAlvo;
            _segurando.rt.localScale = Vector3.one;
            if (Mathf.Abs(c.x) < TamAlvo.x / 2f + 10f && Mathf.Abs(c.y) < TamAlvo.y / 2f + 10f)
            {
                _segurando.dentro = true;
                int idx = Dentro() - 1;
                _segurando.rt.anchoredPosition = PosAlvo + (_qtd == 1 ? Vector2.zero : new Vector2((idx % 2 - 0.5f) * 96f, (0.5f - idx / 2) * 80f));
                _segurando.rt.localScale = Vector3.one * 0.78f;
                Pulso(_alvo);
                Sons.Acerto();
                AtualizarContador();
                Pulso(_contador);
            }
            else _segurando.vel = Random.insideUnitCircle.normalized * Random.Range(VelMin, VelMax);
            _segurando = null;
        }

        PintarBotao();
        if (_botaoAtivo == true && Ponteiro.Down && _segurando == null && Sobre(_botaoRt))
        {
            _botaoFaceRt.anchoredPosition = new Vector2(0, -UiTema.Sombra_Y); // afunda
            Terminar(Progresso());
        }
    }
}

// =====================================================================================
// 4) CANOS 3x3 — girar os canos até ligar o tanque mostarda (água usada) ao azul (água limpa). Reciclagem de água.
//    Resolvido: nota de 80 a 100% pelo tempo. Sem resolver: trecho molhado ÷ caminho.
// =====================================================================================
public class MinigameCanos : Minigame
{
    public override Mecanica Mecanica => Mecanica.Canos;

    const float Tam = 130f, Passo = 140f, Grossura = 38f; // peças de 130 (mínimo de 88)
    // direções: 0=N 1=E 2=S 3=W
    static readonly Vector2Int[] Delta = { new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0) };

    class Tile { public bool reto; public int rot; public RectTransform rt, giro; public readonly List<FormaUI> canos = new List<FormaUI>(); public bool molhado; }
    Tile[,] _g;
    int _linIni, _linFim, _tamCaminho = 1, _molhados;

    int Conexoes(Tile t)
    {
        int baseMask = t.reto ? (1 << 1 | 1 << 3) : (1 << 0 | 1 << 1);
        int m = 0;
        for (int d = 0; d < 4; d++) if ((baseMask & (1 << d)) != 0) m |= 1 << ((d + t.rot) % 4);
        return m;
    }

    protected override float Progresso() => Mathf.Clamp01(_molhados / (float)_tamCaminho) * 0.79f;
    protected override float ProgressoVisivel => Mathf.Clamp01(_molhados / (float)_tamCaminho);

    static Vector2 PosDe(int c, int r) => new Vector2((c - 1) * Passo, -(r - 1) * Passo);

    protected override void Construir()
    {
        _g = new Tile[3, 3]; // [coluna, linha]
        _linIni = Random.Range(0, 3); _linFim = Random.Range(0, 3);
        int linMeio = Random.Range(0, 3);

        // Caminho garantido: (ini,0) → (ini,1) → vertical até meio,1 → (meio,2) → vertical até fim,2
        var caminho = new List<Vector2Int> { new Vector2Int(0, _linIni), new Vector2Int(1, _linIni) };
        int l = _linIni;
        while (l != linMeio) { l += l < linMeio ? 1 : -1; caminho.Add(new Vector2Int(1, l)); }
        caminho.Add(new Vector2Int(2, linMeio));
        l = linMeio;
        while (l != _linFim) { l += l < _linFim ? 1 : -1; caminho.Add(new Vector2Int(2, l)); }
        _tamCaminho = caminho.Count;

        for (int c = 0; c < 3; c++) for (int r = 0; r < 3; r++)
            _g[c, r] = new Tile { reto = Random.value < 0.5f, rot = Random.Range(0, 4) };

        for (int i = 0; i < caminho.Count; i++)
        {
            int entrada = i == 0 ? 3 : DirDe(caminho[i], caminho[i - 1]);
            int saida = i == caminho.Count - 1 ? 1 : DirDe(caminho[i], caminho[i + 1]);
            var t = _g[caminho[i].x, caminho[i].y];
            t.reto = (entrada + 2) % 4 == saida;
            int alvo = (1 << entrada) | (1 << saida);
            for (int rot = 0; rot < 4; rot++) { t.rot = rot; if (Conexoes(t) == alvo) break; }
            t.rot = (t.rot + Random.Range(1, 4)) % 4; // embaralha (nunca já resolvido por esse tile)
        }

        // Quadro de aço com as placas chanfradas.
        Chanfro(Vector2.zero, new Vector2(3 * Passo + 24f, 3 * Passo + 24f), UiTema.Vazio, UiTema.ChanfroPainel);
        for (int c = 0; c < 3; c++) for (int r = 0; r < 3; r++)
        {
            var t = _g[c, r];
            t.rt = No("Peca", PosDe(c, r), new Vector2(Tam, Tam));
            Chanfro(Vector2.zero, new Vector2(Tam, Tam), UiTema.AcoClaro, 8f, t.rt);
            foreach (var q in new[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) })
                Circulo(q * (Tam / 2f - 14f), 9f, UiTema.Aco, t.rt);
            t.giro = No("Giro", Vector2.zero, new Vector2(Tam, Tam), t.rt);
            // Canos na orientação de base (rot 0): reto = leste-oeste; curva = norte e leste.
            if (t.reto) t.canos.Add(Chanfro(Vector2.zero, new Vector2(Tam, Grossura), UiTema.Apoio, 0f, t.giro));
            else
            {
                t.canos.Add(Chanfro(new Vector2(0, Tam / 4f), new Vector2(Grossura, Tam / 2f + Grossura / 2f), UiTema.Apoio, 0f, t.giro));
                t.canos.Add(Chanfro(new Vector2(Tam / 4f, 0), new Vector2(Tam / 2f + Grossura / 2f, Grossura), UiTema.Apoio, 0f, t.giro));
            }
            t.canos.Add(Circulo(Vector2.zero, Grossura + 12f, UiTema.Apoio, t.giro));
            t.giro.localRotation = Quaternion.Euler(0, 0, -90f * t.rot);
        }

        // Tanques: água usada (mostarda) à esquerda, água limpa (azul) à direita, cada um com cano até o quadro.
        float xTanque = 1.5f * Passo + 12f + 60f;
        Tanque(new Vector2(-xTanque, PosDe(0, _linIni).y), UiTema.AguaUsada, Idioma.L("Água usada", "Used water"), true);
        Tanque(new Vector2(xTanque, PosDe(2, _linFim).y), UiTema.Ar, Idioma.L("Água limpa", "Clean water"), false);
        Colorir(false);
    }

    void Tanque(Vector2 pos, Color cor, string rotulo, bool esquerda)
    {
        float lado = esquerda ? 1f : -1f;
        Chanfro(pos + new Vector2(lado * 62f, 0), new Vector2(40f, Grossura), cor, 0f);
        var placa = Placa(pos, new Vector2(110f, 130f), cor, UiTema.ChanfroBotao, null, "Tanque");
        Chanfro(new Vector2(0, -36f), new Vector2(110f, 10f), Escuro(cor, 0.7f), 0f, placa);
        Circulo(new Vector2(0, 12f), 64f, UiTema.Aco, placa);
        Icone("mod_ar", new Vector2(0, 12f), 48f, placa);
        Rotulo(rotulo, Papel.Rotulo, pos + new Vector2(0, 92f), new Vector2(170, 56), UiTema.Branco);
    }

    static int DirDe(Vector2Int de, Vector2Int para)
    {
        var d = para - de;
        for (int i = 0; i < 4; i++) if (Delta[i] == d) return i;
        return 1;
    }

    // Percorre desde o tanque de água usada; devolve os tiles molhados e se chegou ao tanque de água limpa.
    bool Percorrer(List<Tile> molhados)
    {
        var pos = new Vector2Int(0, _linIni);
        int entrada = 3;
        for (int passo = 0; passo < 12; passo++)
        {
            var t = _g[pos.x, pos.y];
            int m = Conexoes(t);
            if ((m & (1 << entrada)) == 0) return false;
            molhados.Add(t);
            int saida = -1;
            for (int d = 0; d < 4; d++) if (d != entrada && (m & (1 << d)) != 0) saida = d;
            var prox = pos + Delta[saida];
            if (prox.x < 0 || prox.x > 2 || prox.y < 0 || prox.y > 2)
                return pos.x == 2 && pos.y == _linFim && saida == 1;
            pos = prox; entrada = (saida + 2) % 4;
        }
        return false;
    }

    /// <summary>Trecho molhado fica ciano; o resto, seco. Peça que acabou de molhar dá pulso e som.</summary>
    bool Colorir(bool retorno)
    {
        var molhados = new List<Tile>();
        bool ok = Percorrer(molhados);
        int antes = _molhados;
        _molhados = molhados.Count;
        foreach (var t in _g)
        {
            bool m = molhados.Contains(t);
            foreach (var f in t.canos) f.color = m ? UiTema.Ar : UiTema.Apoio;
            if (retorno && m && !t.molhado) Pulso(t.rt);
            t.molhado = m;
        }
        if (retorno && _molhados > antes) Sons.Acerto();
        return ok;
    }

    protected override void PontosDemo(out Vector2 de, out Vector2 para) { de = PosDe(0, _linIni); para = de; }

    protected override void Atualizar()
    {
        // Peça gira em 120 ms (90 graus); com movimento reduzido, salta.
        float passoGiro = UiTema.MovimentoReduzido ? 360f : 750f * Time.deltaTime;
        foreach (var t in _g)
        {
            float atual = t.giro.localEulerAngles.z, alvo = -90f * t.rot;
            if (Mathf.Abs(Mathf.DeltaAngle(atual, alvo)) > 0.01f)
                t.giro.localRotation = Quaternion.Euler(0, 0, Mathf.MoveTowardsAngle(atual, alvo, passoGiro));
        }

        if (!Ponteiro.Down) return;
        foreach (var t in _g)
            if (Sobre(t.rt))
            {
                t.rot = (t.rot + 1) % 4;
                Sons.Toque();
                if (Colorir(true)) Terminar(Mathf.Max(0.8f, 1f - Tempo / duracao));
                return;
            }
    }
}

// =====================================================================================
// 5) DISJUNTORES — ligar o sistema pedido sem estourar a carga (desligue outro!).
//    Estufa e antena (comunicação). Resolvido: 100% até 1/3 do tempo, 70% até 2/3, depois 40%.
//    Sobrecarga: a alavanca volta e a faixa do limite fica amarela, com "Sobrecarga". Sem vermelho e sem tremor.
// =====================================================================================
public enum ModoDisjuntor { Estufa, Antena }

public class MinigameDisjuntores : Minigame
{
    public override Mecanica Mecanica => Mecanica.Disjuntores;

    static readonly Txt[] Nomes =
    {
        new Txt("Estufa", "Greenhouse"), new Txt("Filtros", "Filters"),
        new Txt("Comunicação", "Comms"), new Txt("Escudo", "Shield"),
    };
    static readonly string[] Icones = { "mod_estufa", "mod_ar", "terra", "mod_abrigo" };
    readonly int[] _consumo = { 30, 25, 20, 50 };
    readonly bool[] _ligada = new bool[4];
    readonly int _alvo;
    const int Capacidade = 80, Escala = 100; // a barra vai até 100; de 80 a 100 é a faixa hachurada do limite

    const float Passo = 172f, YTrilho = -10f, AltoTrilho = 200f, Curso = AltoTrilho / 2f - 34f;
    const float LarguraBarra = 640f, YBarra = 214f;
    readonly RectTransform[] _colunas = new RectTransform[4];
    readonly RectTransform[] _alavancas = new RectTransform[4];
    readonly FormaUI[] _leds = new FormaUI[4];
    RectTransform _cargaFill;
    FormaUI _limite;
    Text _cargaTexto, _aviso;
    int _arrastando = -1;
    float _sobrecargaAte = -1f;
    bool _resolvido;

    public MinigameDisjuntores(ModoDisjuntor modo)
    {
        if (modo == ModoDisjuntor.Estufa) { _alvo = 0; _ligada[1] = true; _ligada[2] = true; _ligada[3] = true; _consumo[3] = 30; }
        else { _alvo = 2; _consumo[2] = 55; _ligada[0] = true; _ligada[1] = true; }
    }

    int Carga() { int s = 0; for (int i = 0; i < 4; i++) if (_ligada[i]) s += _consumo[i]; return s; }
    protected override float Progresso() => _resolvido ? 1f : 0f;
    /// <summary>A barra anda conforme a carga é liberada para o sistema pedido (sem mostrar a nota de corte).</summary>
    protected override float ProgressoVisivel => _resolvido || _ligada[_alvo] ? 1f
        : Mathf.Clamp01((Capacidade - Carga()) / (float)_consumo[_alvo]) * 0.9f;

    static Color CorDoSistema(int i) => i == 0 ? UiTema.Estufa : i == 3 ? UiTema.Abrigo : UiTema.Ar;

    protected override void Construir()
    {
        _arrastando = -1;
        // Barra de carga, com a faixa hachurada do limite (80 de 100).
        _cargaTexto = Rotulo("", Papel.Rotulo, new Vector2(-LarguraBarra / 2f + 160f, YBarra + 42f), new Vector2(320, 34), UiTema.Branco, null, TextAnchor.MiddleLeft);
        Chanfro(new Vector2(0, YBarra), new Vector2(LarguraBarra + 14f, 50f), UiTema.AcoClaro, 6f);
        float xLimite = -LarguraBarra / 2f + LarguraBarra * Capacidade / Escala;
        _limite = Forma(FormaUI.Modo.Hachurada, new Vector2((xLimite + LarguraBarra / 2f) / 2f, YBarra), new Vector2(LarguraBarra / 2f - xLimite, 36f), UiTema.Apoio);
        _limite.periodo = 14f; _limite.larguraFaixa = 5f;
        _cargaFill = No("Carga", new Vector2(-LarguraBarra / 2f, YBarra), new Vector2(0, 36f));
        _cargaFill.pivot = new Vector2(0f, 0.5f);
        var fill = FormaUI.Criar(_cargaFill, FormaUI.Modo.Chanfrado, UiTema.Branco, "Face"); fill.chanfro = 4f;
        Chanfro(new Vector2(xLimite, YBarra), new Vector2(4f, 58f), UiTema.Branco, 0f);
        _aviso = Rotulo(Textos.Sobrecarga.ToString(), Papel.Rotulo, new Vector2(LarguraBarra / 2f - 90f, YBarra + 42f), new Vector2(180, 34), UiTema.Energia, null, TextAnchor.MiddleRight);
        _aviso.gameObject.SetActive(false);

        // Quadro elétrico.
        Placa(new Vector2(0, -72f), new Vector2(4 * Passo + 24f, 410f), UiTema.AcoClaro, UiTema.ChanfroPainel);
        for (int i = 0; i < 4; i++)
        {
            float x = (i - 1.5f) * Passo;
            _colunas[i] = No("Coluna", new Vector2(x, YTrilho), new Vector2(150f, 260f)); // área de toque (mínimo 88)
            Chanfro(new Vector2(x, YTrilho), new Vector2(64f, AltoTrilho), UiTema.Vazio, 6f);
            Chanfro(new Vector2(x, YTrilho), new Vector2(10f, AltoTrilho - 40f), UiTema.Sombra, 0f);
            _leds[i] = Circulo(new Vector2(x, YTrilho + AltoTrilho / 2f + 26f), 30f, UiTema.Vazio);
            _alavancas[i] = No("Alavanca", new Vector2(x, YAlavanca(i)), new Vector2(116f, 62f));
            Chanfro(new Vector2(0, -UiTema.Sombra_Y), new Vector2(116f, 58f), UiTema.Sombra, 8f, _alavancas[i]);
            Chanfro(Vector2.zero, new Vector2(116f, 58f), UiTema.Branco, 8f, _alavancas[i]);
            Chanfro(new Vector2(0, 6f), new Vector2(72f, 6f), UiTema.BrancoSombra, 0f, _alavancas[i]);
            Chanfro(new Vector2(0, -6f), new Vector2(72f, 6f), UiTema.BrancoSombra, 0f, _alavancas[i]);
            // Etiqueta com ícone, nome e consumo.
            float yEt = YTrilho - AltoTrilho / 2f - 76f;
            Chanfro(new Vector2(x, yEt), new Vector2(160f, 116f), UiTema.Aco, 6f);
            Icone(Icones[i], new Vector2(x, yEt + 32f), 40f);
            Rotulo(Nomes[i].ToString(), Papel.Rotulo, new Vector2(x, yEt - 4f), new Vector2(158, 30), UiTema.Branco);
            Rotulo(_consumo[i].ToString(), Papel.Numero, new Vector2(x, yEt - 36f), new Vector2(150, 36), UiTema.Apoio);
            if (i == _alvo)
            {
                var tag = Chanfro(new Vector2(x, YTrilho + AltoTrilho / 2f + 70f), new Vector2(170f, 40f), CorDoSistema(i), 6f);
                Rotulo(Textos.LigarEste.ToString(), Papel.Rotulo, Vector2.zero, new Vector2(170, 40), UiTema.Tinta, tag.rectTransform);
            }
            PintarLed(i);
        }
        Animar();
    }

    float YAlavanca(int i) => YTrilho + (_ligada[i] ? Curso : -Curso);
    void PintarLed(int i) => _leds[i].color = _ligada[i] ? CorDoSistema(i) : UiTema.Vazio;

    protected override void PontosDemo(out Vector2 de, out Vector2 para)
    {
        float x = (_alvo - 1.5f) * Passo;
        de = new Vector2(x, YTrilho - Curso); para = new Vector2(x, YTrilho + Curso);
    }

    protected override void Atualizar()
    {
        if (Ponteiro.Down && _arrastando < 0)
            for (int i = 0; i < 4; i++) if (Sobre(_colunas[i]) || Sobre(_alavancas[i])) { _arrastando = i; Sons.Toque(); break; }
        if (_arrastando >= 0)
        {
            float y = Mathf.Clamp(PosLocal.y, YTrilho - Curso, YTrilho + Curso);
            var p = _alavancas[_arrastando].anchoredPosition; p.y = y; _alavancas[_arrastando].anchoredPosition = p;
            if (Ponteiro.Up)
            {
                int i = _arrastando; _arrastando = -1;
                Soltar(i, y > YTrilho);
            }
        }
        Animar();
    }

    public int Alvo => _alvo;
    public bool Ligada(int i) => _ligada[i];
    public bool EmSobrecarga => Time.unscaledTime < _sobrecargaAte;

    /// <summary>Soltar a alavanca <paramref name="i"/> em cima (<paramref name="quer"/> ligar) ou embaixo. Pública para a sonda.</summary>
    public void Soltar(int i, bool quer)
    {
        if (Terminando) return;
        if (quer && !_ligada[i] && Carga() + _consumo[i] > Capacidade)
        {
            _sobrecargaAte = Time.unscaledTime + 1.2f; // a alavanca volta sozinha (em Animar) e o limite fica amarelo
            Sons.Erro();
        }
        else if (quer != _ligada[i])
        {
            _ligada[i] = quer;
            PintarLed(i);
            if (quer) { Pulso(_leds[i]); Sons.Acerto(); } else Sons.Toque();
        }
        Animar();
        if (_ligada[_alvo] && Carga() <= Capacidade)
        {
            _resolvido = true;
            float f = Tempo / duracao;
            Terminar(f <= 1f / 3f ? 1f : f <= 2f / 3f ? 0.7f : 0.4f);
        }
    }

    void Animar()
    {

        // Alavancas soltas voltam para a posição do estado (ligada em cima, desligada embaixo).
        for (int i = 0; i < 4; i++)
        {
            if (i == _arrastando) continue;
            var p = _alavancas[i].anchoredPosition;
            float alvo = YAlavanca(i);
            p.y = UiTema.MovimentoReduzido ? alvo : Mathf.MoveTowards(p.y, alvo, 1100f * Time.deltaTime);
            _alavancas[i].anchoredPosition = p;
        }

        int carga = Carga();
        _cargaFill.sizeDelta = new Vector2(LarguraBarra * Mathf.Clamp01(carga / (float)Escala), 36f);
        bool sobrecarga = Time.unscaledTime < _sobrecargaAte;
        _limite.color = sobrecarga ? UiTema.Energia : UiTema.Apoio;
        _aviso.gameObject.SetActive(sobrecarga);
        _cargaTexto.text = UiTema.Caixa(Textos.Carga.F(carga, Capacidade).ToString());
    }
}

// =====================================================================================
// 6) SEGURAR — manter o botão pressionado para selar o vazamento. Nota = progresso.
//    O anel enche enquanto segura; o vapor que sai do furo diminui com o progresso.
// =====================================================================================
public class MinigameSegurar : Minigame
{
    public override Mecanica Mecanica => Mecanica.Segurar;

    static readonly Vector2 PosBotao = new Vector2(0f, -110f), PosFuro = new Vector2(10f, 150f);
    float _prog;
    int _marco;
    FormaUI _anel;
    RectTransform _botao;
    class Particula { public FormaUI f; public float fase; public Vector2 dir; }
    readonly List<Particula> _vapor = new List<Particula>();

    protected override float Progresso() => _prog;

    protected override void Construir()
    {
        _prog = 0f; _marco = 0; _vapor.Clear();
        // Parede do módulo, em placas rebitadas, com o furo.
        Placa(new Vector2(0, 168f), new Vector2(720f, 220f), UiTema.AcoClaro, UiTema.ChanfroPainel);
        foreach (float x in new[] { -240f, 240f }) Chanfro(new Vector2(x, 168f), new Vector2(4f, 208f), UiTema.Aco, 0f);
        for (int i = 0; i < 6; i++)
        {
            float x = -330f + i * 132f;
            Circulo(new Vector2(x, 262f), 10f, UiTema.Aco);
            Circulo(new Vector2(x, 74f), 10f, UiTema.Aco);
        }
        Poligono(PosFuro, new Vector2(84f, 44f), UiTema.Tinta, 6, 0.45f, 7);
        for (int i = 0; i < 14; i++)
        {
            var c = UiTema.Branco; c.a = 0f;
            var f = Circulo(PosFuro, 30f, c);
            _vapor.Add(new Particula { f = f, fase = i / 14f, dir = DirecaoVapor() });
        }
        // Botão redondo grande com o anel de progresso.
        var trilho = Forma(FormaUI.Modo.Anel, PosBotao, new Vector2(260f, 260f), UiTema.Vazio); trilho.espessura = 24f;
        _anel = Forma(FormaUI.Modo.Anel, PosBotao, new Vector2(260f, 260f), UiTema.Ar); _anel.espessura = 24f; _anel.fracao = 0f;
        _botao = No("Botao", PosBotao, new Vector2(190f, 190f));
        Circulo(new Vector2(0, -UiTema.Sombra_Y), 190f, UiTema.Sombra, _botao);
        Circulo(Vector2.zero, 190f, UiTema.Branco, _botao);
        Icone("tarefa_remendo", Vector2.zero, 92f, _botao);
        AnimarVapor(0f);
    }

    static Vector2 DirecaoVapor()
    {
        float a = Random.Range(-70f, 70f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(a), Mathf.Cos(a) * 0.6f - 0.15f);
    }

    void AnimarVapor(float dt)
    {
        float forca = 1f - _prog;
        foreach (var p in _vapor)
        {
            p.fase += dt * 1.3f;
            if (p.fase >= 1f) { p.fase -= 1f; p.dir = DirecaoVapor(); }
            var rt = p.f.rectTransform;
            rt.anchoredPosition = PosFuro + p.dir * (16f + 170f * p.fase);
            float tam = 22f + 44f * p.fase;
            rt.sizeDelta = new Vector2(tam, tam);
            var c = p.f.color; c.a = (1f - p.fase) * 0.4f * forca; p.f.color = c;
        }
    }

    protected override void PontosDemo(out Vector2 de, out Vector2 para) { de = PosBotao; para = de; }

    protected override void Atualizar()
    {
        bool segura = Ponteiro.Held;
        if (Ponteiro.Down) Sons.Toque();
        _prog = Mathf.Clamp01(_prog + (segura ? 1f : -0.6f) * Time.deltaTime / 4f);
        _anel.fracao = _prog; _anel.Redesenhar();
        _botao.localScale = Vector3.one * (segura ? 0.95f : 1f);
        AnimarVapor(Time.deltaTime);
        int marco = Mathf.FloorToInt(_prog * 4f);
        if (marco > _marco) { Sons.Acerto(); Pulso(_anel); }
        _marco = marco;
        if (_prog >= 1f) Vencer();
    }
}
