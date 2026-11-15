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
                m = new MinigameLixo(T("Pedra", "Rock"), 4, T("VOLTAR", "BACK"), T("CAIXA DE AMOSTRAS", "SAMPLE BOX"), new Color(0.62f, 0.6f, 0.66f), botaoSempreAtivo: true);
                break;
            case Tarefas.Braco:
                m = new MinigameLixo(T("Caixa", "Box"), 1, T("PEGAR", "GRAB"), T("GARRA", "CLAW"), new Color(0.9f, 0.62f, 0.26f), botaoSempreAtivo: false);
                break;
            case Tarefas.Poeira:
                m = new MinigameRaspadinha(new Color(0.17f, 0.36f, 0.62f));
                break;
            case Tarefas.Filtros:
                m = new MinigameRaspadinha(new Color(0.75f, 0.9f, 1f));
                break;
            case Tarefas.Agua:
                m = new MinigameCanos();
                break;
            case Tarefas.Upload:
                m = new MinigameEsteira(3, errarDesfaz: false, rotulo: T("PACOTES ENVIADOS", "PACKETS SENT"));
                break;
            case Tarefas.Exercicio:
                m = new MinigameEsteira(5, errarDesfaz: true, rotulo: T("PASSOS NO RITMO", "STEPS IN RHYTHM"));
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
        m.duracao = limite;
        return m;
    }
}

// =====================================================================================
// 1) ESTEIRA — ritmo: apertar no momento certo (ESPAÇO / A / toque) na zona verde.
//    Exercício (5 acertos, errar desfaz) e transmissão de dados (3 pacotes, errar não desfaz).
// =====================================================================================
public class MinigameEsteira : Minigame
{
    const float Largura = 1000f;
    readonly int _alvo;
    readonly bool _errarDesfaz;
    readonly Txt _rotulo;
    int _acertos;
    float _x, _dir = 1f, _zonaX;
    const float ZonaLarg = 150f, Vel = 650f;
    RectTransform _cursor, _zona;
    Image[] _pontos;

    public MinigameEsteira(int acertos, bool errarDesfaz, Txt rotulo)
    {
        _alvo = acertos; _errarDesfaz = errarDesfaz; _rotulo = rotulo;
    }

    protected override float Progresso() => _acertos / (float)_alvo;

    protected override void Construir()
    {
        _acertos = 0; _x = -Largura / 2f; _dir = 1f;
        Retangulo(new Vector2(0, 0), new Vector2(Largura + 20, 90), new Color(0.15f, 0.2f, 0.3f));
        _zona = Retangulo(Vector2.zero, new Vector2(ZonaLarg, 90), new Color(0.3f, 0.85f, 0.4f)).rectTransform;
        _cursor = Retangulo(Vector2.zero, new Vector2(14, 130), Color.white).rectTransform;
        _pontos = new Image[_alvo];
        for (int i = 0; i < _alvo; i++)
            _pontos[i] = Retangulo(new Vector2((i - (_alvo - 1) / 2f) * 80f, -140), new Vector2(50, 50), new Color(1, 1, 1, 0.2f));
        Rotulo(_rotulo.ToString(), 28, new Vector2(0, -200), new Vector2(900, 40));
        NovaZona();
    }

    void NovaZona()
    {
        _zonaX = Random.Range(-Largura / 2f + ZonaLarg, Largura / 2f - ZonaLarg);
        _zona.anchoredPosition = new Vector2(_zonaX, 0);
    }

    protected override void Atualizar()
    {
        _x += _dir * Vel * Time.deltaTime;
        if (_x > Largura / 2f) { _x = Largura / 2f; _dir = -1f; }
        if (_x < -Largura / 2f) { _x = -Largura / 2f; _dir = 1f; }
        _cursor.anchoredPosition = new Vector2(_x, 0);

        if (Input.GetKeyDown(KeyCode.Space) || Ponteiro.Down)
        {
            if (Mathf.Abs(_x - _zonaX) <= ZonaLarg / 2f)
            {
                _pontos[_acertos].color = new Color(0.3f, 0.9f, 0.4f);
                _acertos++;
                if (_acertos >= _alvo) { Vencer(); return; }
                NovaZona();
            }
            else if (_errarDesfaz && _acertos > 0)
            {
                _acertos--;
                _pontos[_acertos].color = new Color(1, 1, 1, 0.2f);
            }
        }
    }
}

// =====================================================================================
// 2) RASPADINHA — esfregar a poeira (painéis solares / filtros de ar). Nota = fração limpa.
// =====================================================================================
public class MinigameRaspadinha : Minigame
{
    const int Cols = 10, Lins = 4;
    const float Celula = 130f, Raio = 105f;
    readonly Color _fundo;
    float[] _sujeira;
    Image[] _cels;
    Vector2[] _centros;

    public MinigameRaspadinha(Color fundoLimpo) { _fundo = fundoLimpo; }

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
        _sujeira = new float[n]; _cels = new Image[n]; _centros = new Vector2[n];
        Retangulo(Vector2.zero, new Vector2(Cols * Celula + 10, Lins * Celula + 10), _fundo); // superfície limpa
        for (int i = 0; i < n; i++)
        {
            int c = i % Cols, l = i / Cols;
            _centros[i] = new Vector2((c - (Cols - 1) / 2f) * Celula, (l - (Lins - 1) / 2f) * Celula);
            _sujeira[i] = 1f;
            _cels[i] = Retangulo(_centros[i], new Vector2(Celula - 2, Celula - 2), new Color(0.35f, 0.33f, 0.3f, 1f));
        }
    }

    protected override void Atualizar()
    {
        if (!Ponteiro.Held) return;
        Vector2 p = PosLocal;
        float limpa = Ponteiro.Movimento / 350f; // só esfregar (mover) limpa
        int sujas = 0;
        for (int i = 0; i < _sujeira.Length; i++)
        {
            if (_sujeira[i] > 0f && (p - _centros[i]).magnitude < Raio)
            {
                _sujeira[i] = Mathf.Max(0f, _sujeira[i] - limpa);
                var c = _cels[i].color; c.a = _sujeira[i]; _cels[i].color = c;
            }
            if (_sujeira[i] > 0.08f) sujas++;
        }
        if (sujas == 0) Vencer();
    }
}

// =====================================================================================
// 3) ARRASTAR — itens flutuando (gravidade baixa) para a caixa, depois apertar o botão.
//    Coleta de pedras (botão VOLTAR sempre ativo; nota = pedras na caixa) e braço robótico (1 caixa, PEGAR).
// =====================================================================================
public class MinigameLixo : Minigame
{
    readonly Txt _item, _botao, _caixaRotulo;
    readonly int _qtd;
    readonly bool _botaoSempre;
    readonly Color _cor;
    class Peca { public RectTransform rt; public Vector2 vel; public bool dentro; }
    List<Peca> _pecas;
    Peca _segurando;
    RectTransform _caixa, _botaoRt;
    Image _botaoImg;
    const float Meio = 55f; // meia-largura da peça

    public MinigameLixo(Txt item, int qtd, Txt botao, Txt caixaRotulo, Color cor, bool botaoSempreAtivo)
    {
        _item = item; _qtd = qtd; _botao = botao; _caixaRotulo = caixaRotulo; _cor = cor; _botaoSempre = botaoSempreAtivo;
    }

    int Dentro() => _pecas == null ? 0 : _pecas.FindAll(x => x.dentro).Count;
    protected override float Progresso() => Dentro() / (float)_qtd;
    protected override bool SairConclui => _botaoSempre;

    protected override void Construir()
    {
        _segurando = null;
        _caixa = Retangulo(new Vector2(0, 0), new Vector2(280, 280), new Color(0.2f, 0.5f, 0.7f, 0.9f)).rectTransform;
        Rotulo(_caixaRotulo.ToString(), 26, new Vector2(0, 110), new Vector2(270, 50));
        _botaoRt = Retangulo(new Vector2(0, -215), new Vector2(380, 80), new Color(0.35f, 0.35f, 0.4f)).rectTransform;
        _botaoImg = _botaoRt.GetComponent<Image>();
        Rotulo(_botao.ToString(), 40, new Vector2(0, -215), new Vector2(380, 80));

        _pecas = new List<Peca>();
        for (int i = 0; i < _qtd; i++)
        {
            Vector2 pos = new Vector2(i % 2 == 0 ? Random.Range(-600f, -250f) : Random.Range(250f, 600f), Random.Range(-100f, 180f));
            var im = Retangulo(pos, Vector2.one * Meio * 2f, _cor);
            Rotulo(_item.ToString(), 22, Vector2.zero, new Vector2(Meio * 2f, 40), im.transform);
            _pecas.Add(new Peca { rt = im.rectTransform, vel = Random.insideUnitCircle.normalized * Random.Range(120f, 200f) });
        }
    }

    protected override void Atualizar()
    {
        Vector2 p = PosLocal;
        if (Ponteiro.Down && _segurando == null)
            foreach (var pc in _pecas)
                if (!pc.dentro && (Mathf.Abs(p.x - pc.rt.anchoredPosition.x) < Meio && Mathf.Abs(p.y - pc.rt.anchoredPosition.y) < Meio))
                { _segurando = pc; break; }

        foreach (var pc in _pecas)
        {
            if (pc.dentro) continue;
            if (pc == _segurando) { pc.rt.anchoredPosition = p; continue; }
            Vector2 pos = pc.rt.anchoredPosition + pc.vel * Time.deltaTime;
            if (pos.x > 650 || pos.x < -650) { pc.vel.x = -pc.vel.x; pos.x = Mathf.Clamp(pos.x, -650, 650); }
            if (pos.y > 215 || pos.y < -215) { pc.vel.y = -pc.vel.y; pos.y = Mathf.Clamp(pos.y, -215, 215); }
            pc.rt.anchoredPosition = pos;
        }

        if (Ponteiro.Up && _segurando != null)
        {
            Vector2 c = _segurando.rt.anchoredPosition;
            if (Mathf.Abs(c.x) < 140 && Mathf.Abs(c.y) < 140)
            {
                _segurando.dentro = true;
                int idx = Dentro() - 1;
                _segurando.rt.anchoredPosition = new Vector2((idx % 2 - 0.5f) * 120f, (idx / 2 - 0.5f) * 120f);
                _segurando.rt.sizeDelta = Vector2.one * 90f;
            }
            else _segurando.vel = Random.insideUnitCircle.normalized * Random.Range(120f, 200f);
            _segurando = null;
        }

        bool todos = _pecas.TrueForAll(x => x.dentro);
        bool ativo = todos || _botaoSempre;
        _botaoImg.color = ativo ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.35f, 0.35f, 0.4f);
        if (ativo && Ponteiro.Down && _segurando == null && Sobre(_botaoRt)) Terminar(Progresso());
    }
}

// =====================================================================================
// 4) CANOS 3x3 — girar os canos até ligar o tanque amarelo ao azul. Reciclagem de água.
//    Resolvido: nota de 80 a 100% pelo tempo. Sem resolver: trecho molhado ÷ caminho.
// =====================================================================================
public class MinigameCanos : Minigame
{
    const float Tam = 150f;
    // direções: 0=N 1=E 2=S 3=W
    static readonly Vector2Int[] Delta = { new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0) };

    class Tile { public bool reto; public int rot; public RectTransform rt; public Image[] barras = new Image[4]; }
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

        for (int c = 0; c < 3; c++) for (int r = 0; r < 3; r++)
        {
            Vector2 pos = new Vector2((c - 1) * (Tam + 12), -(r - 1) * (Tam + 12));
            var t = _g[c, r];
            t.rt = Retangulo(pos, new Vector2(Tam, Tam), new Color(0.12f, 0.17f, 0.25f)).rectTransform;
            for (int d = 0; d < 4; d++)
            {
                Vector2 off = d == 0 ? new Vector2(0, Tam / 4) : d == 1 ? new Vector2(Tam / 4, 0) : d == 2 ? new Vector2(0, -Tam / 4) : new Vector2(-Tam / 4, 0);
                Vector2 sz = d % 2 == 0 ? new Vector2(34, Tam / 2 + 17) : new Vector2(Tam / 2 + 17, 34);
                t.barras[d] = Retangulo(off, sz, Color.gray, t.rt);
            }
            Atualizar(t);
        }
        float yIni = -(_linIni - 1) * (Tam + 12), yFim = -(_linFim - 1) * (Tam + 12);
        Retangulo(new Vector2(-370, yIni), new Vector2(150, 110), new Color(1f, 0.85f, 0.2f));
        Rotulo(Idioma.L("ÁGUA\nUSADA", "USED\nWATER"), 22, new Vector2(-370, yIni), new Vector2(150, 80));
        Retangulo(new Vector2(370, yFim), new Vector2(150, 110), new Color(0.3f, 0.6f, 1f));
        Rotulo(Idioma.L("ÁGUA\nLIMPA", "CLEAN\nWATER"), 22, new Vector2(370, yFim), new Vector2(150, 80));
        Colorir();
    }

    static int DirDe(Vector2Int de, Vector2Int para)
    {
        var d = para - de;
        for (int i = 0; i < 4; i++) if (Delta[i] == d) return i;
        return 1;
    }

    void Atualizar(Tile t)
    {
        int m = Conexoes(t);
        for (int d = 0; d < 4; d++) t.barras[d].gameObject.SetActive((m & (1 << d)) != 0);
    }

    // Percorre desde o tanque amarelo; devolve os tiles molhados e se chegou ao tanque azul.
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

    bool Colorir()
    {
        foreach (var t in _g) foreach (var b in t.barras) b.color = Color.gray;
        var molhados = new List<Tile>();
        bool ok = Percorrer(molhados);
        _molhados = molhados.Count;
        foreach (var t in molhados) foreach (var b in t.barras) b.color = ok ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.85f, 0.2f);
        return ok;
    }

    protected override void Atualizar()
    {
        if (!Ponteiro.Down) return;
        foreach (var t in _g)
            if (Sobre(t.rt))
            {
                t.rot = (t.rot + 1) % 4;
                Atualizar(t);
                if (Colorir()) Terminar(Mathf.Max(0.8f, 1f - Tempo / duracao));
                return;
            }
    }
}

// =====================================================================================
// 5) DISJUNTORES — ligar o sistema pedido sem estourar a carga (desligue outro!).
//    Estufa e antena (comunicação). Resolvido: 100% até 1/3 do tempo, 70% até 2/3, depois 40%.
// =====================================================================================
public enum ModoDisjuntor { Estufa, Antena }

public class MinigameDisjuntores : Minigame
{
    static readonly Txt[] Nomes =
    {
        new Txt("Estufa", "Greenhouse"), new Txt("Filtros", "Filters"),
        new Txt("Comunicação", "Comms"), new Txt("Escudo", "Shield"),
    };
    readonly int[] _consumo = { 30, 25, 20, 50 };
    readonly bool[] _ligada = new bool[4];
    readonly int _alvo;
    const int Capacidade = 80;

    readonly RectTransform[] _trilhos = new RectTransform[4];
    readonly RectTransform[] _botoes = new RectTransform[4];
    readonly Image[] _luz = new Image[4];
    RectTransform _cargaBarra;
    Image _cargaImg;
    Text _cargaTexto;
    int _arrastando = -1;
    float _flashAte;
    bool _resolvido;
    const float Alto = 260f, Meio = Alto / 2f - 34f;

    public MinigameDisjuntores(ModoDisjuntor modo)
    {
        if (modo == ModoDisjuntor.Estufa) { _alvo = 0; _ligada[1] = true; _ligada[2] = true; _ligada[3] = true; _consumo[3] = 30; }
        else { _alvo = 2; _consumo[2] = 55; _ligada[0] = true; _ligada[1] = true; }
    }

    int Carga() { int s = 0; for (int i = 0; i < 4; i++) if (_ligada[i]) s += _consumo[i]; return s; }
    protected override float Progresso() => _resolvido ? 1f : 0f;

    protected override void Construir()
    {
        _arrastando = -1;
        Retangulo(new Vector2(0, 200), new Vector2(1000, 44), new Color(0, 0, 0, 0.5f));
        var fill = Retangulo(Vector2.zero, new Vector2(992, 36), new Color(0.4f, 0.85f, 0.4f), Area);
        _cargaImg = fill; _cargaBarra = fill.rectTransform;
        _cargaBarra.pivot = new Vector2(0, 0.5f);
        _cargaBarra.anchoredPosition = new Vector2(-496, 200);
        _cargaTexto = Rotulo("", 30, new Vector2(0, 200), new Vector2(900, 44));

        for (int i = 0; i < 4; i++)
        {
            float x = (i - 1.5f) * 300f;
            _trilhos[i] = Retangulo(new Vector2(x, -30), new Vector2(90, Alto), new Color(0.15f, 0.2f, 0.3f)).rectTransform;
            _botoes[i] = Retangulo(new Vector2(x, 0), new Vector2(120, 68), new Color(0.85f, 0.85f, 0.9f)).rectTransform;
            _luz[i] = Retangulo(new Vector2(x, -190), new Vector2(40, 40), Color.gray);
            Rotulo($"{Nomes[i]}\n{_consumo[i]}", 28, new Vector2(x, -225), new Vector2(240, 70));
            if (i == _alvo) Rotulo(Idioma.L("LIGAR ESTE", "SWITCH ON"), 26, new Vector2(x, 130), new Vector2(240, 40));
            PosicionarBotao(i);
        }
        Atualizar();
    }

    float YBotao(int i) => -30f + (_ligada[i] ? Meio : -Meio);
    void PosicionarBotao(int i)
    {
        var p = _botoes[i].anchoredPosition; p.y = YBotao(i); _botoes[i].anchoredPosition = p;
        _luz[i].color = _ligada[i] ? new Color(0.4f, 1f, 0.4f) : new Color(0.4f, 0.4f, 0.4f);
    }

    protected override void Atualizar()
    {
        if (Ponteiro.Down)
            for (int i = 0; i < 4; i++) if (Sobre(_botoes[i]) || Sobre(_trilhos[i])) { _arrastando = i; break; }
        if (_arrastando >= 0)
        {
            float y = Mathf.Clamp(PosLocal.y, -30f - Meio, -30f + Meio);
            var p = _botoes[_arrastando].anchoredPosition; p.y = y; _botoes[_arrastando].anchoredPosition = p;
            if (Ponteiro.Up)
            {
                int i = _arrastando; _arrastando = -1;
                bool quer = y > -30f;
                if (quer && !_ligada[i] && Carga() + _consumo[i] > Capacidade) { _flashAte = Time.time + 0.6f; UI.Mensagem(Idioma.L("Sobrecarga!", "Overload!")); }
                else
                {
                    if (quer != _ligada[i]) UI.Mensagem("");
                    _ligada[i] = quer;
                }
                PosicionarBotao(i);
                if (_ligada[_alvo] && Carga() <= Capacidade)
                {
                    _resolvido = true;
                    float f = Tempo / duracao;
                    Terminar(f <= 1f / 3f ? 1f : f <= 2f / 3f ? 0.7f : 0.4f);
                }
            }
        }

        int carga = Carga();
        _cargaBarra.sizeDelta = new Vector2(992f * Mathf.Clamp01(carga / (float)Capacidade), 36f);
        _cargaImg.color = Time.time < _flashAte ? new Color(1f, 0.2f, 0.2f)
            : (carga > Capacidade * 0.85f ? new Color(1f, 0.7f, 0.2f) : new Color(0.4f, 0.85f, 0.4f));
        _cargaTexto.text = Idioma.L($"CARGA {carga} / {Capacidade}", $"LOAD {carga} / {Capacidade}");
    }
}

// =====================================================================================
// 6) SEGURAR — manter o botão pressionado para selar o vazamento. Nota = progresso.
// =====================================================================================
public class MinigameSegurar : Minigame
{
    float _prog;
    RectTransform _barra;

    protected override float Progresso() => _prog;

    protected override void Construir()
    {
        _prog = 0f;
        Retangulo(Vector2.zero, new Vector2(700, 60), new Color(0, 0, 0, 0.5f));
        var f = Retangulo(Vector2.zero, new Vector2(1, 50), new Color(0.4f, 0.85f, 1f));
        _barra = f.rectTransform; _barra.pivot = new Vector2(0, 0.5f); _barra.anchoredPosition = new Vector2(-345, 0);
        Rotulo("Tssshhhh...", 50, new Vector2(0, 120), new Vector2(800, 80));
    }

    protected override void Atualizar()
    {
        _prog = Mathf.Clamp01(_prog + (Ponteiro.Held ? 1f : -0.6f) * Time.deltaTime / 4f);
        _barra.sizeDelta = new Vector2(690f * _prog, 50f);
        if (_prog >= 1f) Vencer();
    }
}
