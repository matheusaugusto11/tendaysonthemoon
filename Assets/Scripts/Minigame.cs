using System;
using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base de todo minijogo 2D. Regras comuns (spec tarefas-e-minijogos): limite de tempo; termina sempre com nota de 0 a 1,
/// sem recomeçar ao falhar; no fim do tempo a nota é o progresso alcançado; SAIR termina com o progresso atual.
/// Molde (design D9 da mudança interface-e-minijogos): o painel lateral, montado pela UIJogo, tem título, instrução,
/// relógio, a Area de 760 por 560, a barra de progresso (lida de <see cref="ProgressoVisivel"/> a cada quadro) e "?" e
/// "Sair". Cada minijogo só desenha a Area em Construir(), com FormaUI, e dá retorno a cada ação (Pulso, Flutua e Sons).
/// O fim mostra a faixa "Ótimo!", "Bom!" ou "Não deu desta vez" com a nota e o efeito, e só então o jogo segue.
/// </summary>
public abstract class Minigame
{
    /// <summary>Tamanho da área do gesto no referencial 1920 por 1080.</summary>
    public static readonly Vector2 TamanhoArea = new Vector2(760f, 560f);
    /// <summary>Quanto tempo a faixa de resultado fica no painel antes de o jogo seguir.</summary>
    public const float DuracaoFaixa = 1.6f;
    /// <summary>Mão fantasma do Dia 1: segundos de demonstração antes de o relógio começar.</summary>
    public const float DuracaoDemo = 3f;

    public Txt titulo = new Txt("Minijogo", "Minigame");
    public Txt instrucao = Txt.Nada;
    public float duracao = 15f;
    public string tarefa;
    /// <summary>Segundos de mão fantasma antes do relógio (0 = começa na hora).</summary>
    public float demo;

    public abstract Mecanica Mecanica { get; }

    protected UIJogo UI { get; private set; }
    protected RectTransform Area { get; private set; }

    /// <summary>Botão que recebe a seleção inicial do controle (opcional).</summary>
    public Button PrimeiroBotao { get; protected set; }

    Func<ResultadoMinijogo, Txt> _aoResultado;
    Action _aoFechar;
    float _t, _fechaEm = -1f, _demoT;
    bool _terminou;
    ResultadoMinijogo _res;
    DemoGesto _demo;

    /// <summary>Segundos de jogo (o relógio do minijogo; não conta a demonstração).</summary>
    public float Tempo => _t;
    /// <summary>A mão fantasma está mostrando o gesto e o relógio ainda não começou.</summary>
    public bool EmDemonstracao => demo > 0f && _demoT < demo && _fechaEm < 0f && !_terminou;
    /// <summary>Faixa mostrada no fim (null enquanto o minijogo corre).</summary>
    public FaixaNota? Faixa { get; private set; }
    public bool Terminando => _fechaEm >= 0f || _terminou;
    public ResultadoMinijogo Resultado => _res;
    /// <summary>Demonstração na tela agora (a do Dia 1 ou a do botão "?").</summary>
    public DemoGesto DemoAtual => _demo;

    /// <param name="aoResultado">Aplica o resultado e devolve o efeito, para a faixa mostrar.</param>
    /// <param name="aoFechar">Chamado quando a faixa sai: o jogo segue.</param>
    public void Abrir(UIJogo ui, RectTransform area, Func<ResultadoMinijogo, Txt> aoResultado, Action aoFechar)
    {
        UI = ui; Area = area; _aoResultado = aoResultado; _aoFechar = aoFechar;
        _terminou = false; _fechaEm = -1f; _t = 0f; _demoT = 0f; Faixa = null;
        Ponteiro.Iniciar();
        UI.LimparArea();
        Construir();
        UI.Tempo(1f, duracao);
        UI.Progresso(ProgressoVisivel, true);
        if (demo > 0f) MostrarDemo();
    }

    protected abstract void Construir();
    protected virtual void Atualizar() { }

    /// <summary>Progresso de 0 a 1 do objetivo, usado quando o tempo acaba ou o jogador sai.</summary>
    protected abstract float Progresso();

    /// <summary>O que a barra de progresso mostra: o mesmo progresso, mas contínuo (sem degraus nem nota de corte).</summary>
    protected virtual float ProgressoVisivel => Progresso();

    /// <summary>Sair conta como objetivo concluído? (só a coleta de pedras: sair = voltar para a porta)</summary>
    protected virtual bool SairConclui => false;

    /// <summary>Onde a mão fantasma faz o gesto, em coordenadas da Area.</summary>
    protected virtual void PontosDemo(out Vector2 de, out Vector2 para) { de = Vector2.zero; para = Vector2.zero; }

    /// <summary>Mostra a mão fantasma por cima da Area (Dia 1 com tutorial, ou o botão "?").</summary>
    public void MostrarDemo()
    {
        if (Area == null || Terminando) return;
        if (_demo != null) UnityEngine.Object.Destroy(_demo.gameObject);
        PontosDemo(out var de, out var para);
        _demo = DemoGesto.Criar(Area, Mecanica, de, para, false, UiTema.CorDaTarefa(tarefa));
        _demo.transform.SetAsLastSibling();
        _demoAte = Time.unscaledTime + DuracaoDemo;
    }

    float _demoAte;

    void EsconderDemo()
    {
        if (_demo != null) UnityEngine.Object.Destroy(_demo.gameObject);
        _demo = null;
    }

    /// <summary>Chamado pelo GameManager todo frame (fora da pausa).</summary>
    public void Tick()
    {
        if (_terminou) return;
        Ponteiro.Atualizar();
        UI.Cursor(Ponteiro.ModoCursor, Ponteiro.Pos);
        float dt = Time.deltaTime;

        if (_fechaEm >= 0f)
        {
            _fechaEm -= dt;
            if (_fechaEm <= 0f) { _terminou = true; _aoFechar?.Invoke(); }
            return;
        }

        // Dia 1 com tutorial: a mão fantasma mostra o gesto e só depois o relógio começa. Toques no meio não contam.
        if (EmDemonstracao)
        {
            _demoT += dt;
            if (_demoT >= demo) { EsconderDemo(); Ponteiro.Iniciar(); }
            return;
        }
        if (_demo != null && Time.unscaledTime >= _demoAte) EsconderDemo();

        _t += dt;
        UI.Tempo(Mathf.Clamp01(1f - _t / duracao), Mathf.Max(0f, duracao - _t));
        if (_t >= duracao) { Encerrar(Progresso(), false); return; }
        Atualizar();
        if (!Terminando) UI.Progresso(ProgressoVisivel, false);
    }

    /// <summary>Objetivo cumprido, com a nota calculada pelo minigame.</summary>
    protected void Terminar(float nota) => Encerrar(nota, true);

    /// <summary>Objetivo cumprido com nota máxima.</summary>
    protected void Vencer() => Terminar(1f);

    void Encerrar(float nota, bool concluiu)
    {
        if (_terminou || _fechaEm >= 0f) return;
        EsconderDemo();
        _res = new ResultadoMinijogo(Mathf.Clamp01(nota), Mathf.Min(_t, duracao), concluiu);
        var efeito = _aoResultado != null ? _aoResultado(_res) : Txt.Nada;
        var faixa = Apresentacao.FaixaDaNota(_res.Nota);
        Faixa = faixa;
        UI.Progresso(ProgressoVisivel, false);
        UI.FaixaResultado(faixa, _res.Nota, efeito);
        if (faixa == FaixaNota.NaoDeu) SonsUI.NaoDeu(); else SonsUI.Concluido();
        _fechaEm = DuracaoFaixa;
    }

    /// <summary>SAIR (botão ou B): termina na hora com o progresso atual. Nunca é desistência grátis.</summary>
    public void Cancelar()
    {
        if (_terminou || _fechaEm >= 0f) return;
        Encerrar(Progresso(), SairConclui);
    }

    // ---------- Retorno a cada ação (design D9) ----------

    /// <summary>Escala de 1 para 1,08 e de volta, em 120 ms.</summary>
    protected static void Pulso(Component alvo) { if (alvo != null) AnimUI.Pulso(alvo.transform); }

    /// <summary>Texto que sobe e some, na posição da Area.</summary>
    protected void Flutua(string texto, Vector2 pos, Color? cor = null) => AnimUI.Flutua(Area, texto, pos, cor ?? UiTema.Branco);

    /// <summary>Sons curtos do minijogo. "Erro" é só o som grave e suave do "não deu", nunca um som de erro.</summary>
    protected static class Sons
    {
        public static void Acerto() => SonsUI.Acerto();
        public static void Toque() => SonsUI.Toque();
        public static void Erro() => SonsUI.NaoDeu();
    }

    // ---------- Desenho (FormaUI, ícones e texto, sempre de UiTema) ----------

    protected RectTransform No(string nome, Vector2 pos, Vector2 tam, Transform pai = null)
    {
        var rt = new GameObject(nome, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(pai != null ? pai : Area, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam; rt.anchoredPosition = pos;
        return rt;
    }

    /// <summary>Forma vetorial centrada em <paramref name="pos"/>.</summary>
    protected FormaUI Forma(FormaUI.Modo modo, Vector2 pos, Vector2 tam, Color cor, Transform pai = null, string nome = "Forma")
    {
        var f = FormaUI.Criar(pai != null ? pai : Area, modo, cor, nome);
        var rt = f.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam; rt.anchoredPosition = pos;
        return f;
    }

    protected FormaUI Chanfro(Vector2 pos, Vector2 tam, Color cor, float chanfro = UiTema.ChanfroBotao, Transform pai = null, string cantos = "SE ID")
    {
        var f = Forma(FormaUI.Modo.Chanfrado, pos, tam, cor, pai, "Chanfro");
        f.chanfro = chanfro;
        f.cantoSE = cantos.Contains("SE"); f.cantoSD = cantos.Contains("SD"); f.cantoIE = cantos.Contains("IE"); f.cantoID = cantos.Contains("ID");
        return f;
    }

    /// <summary>Placa chanfrada com sombra dura (6 abaixo): um nó com a sombra e a face. Devolve o nó; a face é o último filho.</summary>
    protected RectTransform Placa(Vector2 pos, Vector2 tam, Color face, float chanfro = UiTema.ChanfroBotao, Transform pai = null, string nome = "Placa")
    {
        var no = No(nome, pos, tam, pai);
        Chanfro(new Vector2(0, -UiTema.Sombra_Y), tam, UiTema.Sombra, chanfro, no);
        Chanfro(Vector2.zero, tam, face, chanfro, no);
        return no;
    }

    /// <summary>Polígono facetado (pedra, mancha), com semente para a forma.</summary>
    protected FormaUI Poligono(Vector2 pos, Vector2 tam, Color cor, int lados, float irregularidade, int semente, Transform pai = null, float rotacao = 0f)
    {
        var f = Forma(FormaUI.Modo.Poligono, pos, tam, cor, pai, "Poligono");
        f.lados = lados; f.irregularidade = irregularidade; f.semente = semente; f.rotacao = rotacao;
        return f;
    }

    /// <summary>A mesma cor, mais escura (sem mexer no alfa).</summary>
    protected static Color Escuro(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, c.a);

    protected FormaUI Circulo(Vector2 pos, float diametro, Color cor, Transform pai = null)
        => Poligono(pos, new Vector2(diametro, diametro), cor, 32, 0f, 1, pai);

    protected Image Icone(string nome, Vector2 pos, float tam, Transform pai = null)
    {
        var no = No("Icone", pos, new Vector2(tam, tam), pai);
        var im = no.gameObject.AddComponent<Image>();
        im.sprite = UiTema.Icone(nome); im.enabled = im.sprite != null; im.preserveAspect = true; im.raycastTarget = false;
        return im;
    }

    protected Text Rotulo(string txt, Papel papel, Vector2 pos, Vector2 area, Color? cor = null, Transform pai = null, TextAnchor alin = TextAnchor.MiddleCenter)
    {
        var no = No("Texto", pos, area, pai);
        var t = no.gameObject.AddComponent<Text>();
        t.font = UiTema.Fonte(papel, txt); t.fontSize = UiTema.Tamanho(papel);
        t.text = papel == Papel.Rotulo ? UiTema.Caixa(txt) : txt;
        t.color = cor ?? UiTema.Branco; t.alignment = alin; t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    /// <summary>Posição do ponteiro em coordenadas locais da área do minigame (centro = 0,0).</summary>
    protected Vector2 PosLocal
    {
        get
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Area, Ponteiro.Pos, null, out var l);
            return l;
        }
    }

    /// <summary>O ponteiro está sobre este retângulo?</summary>
    protected static bool Sobre(RectTransform rt) => RectTransformUtility.RectangleContainsScreenPoint(rt, Ponteiro.Pos, null);
}
