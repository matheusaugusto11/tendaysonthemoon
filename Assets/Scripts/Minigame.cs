using System;
using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base de todo minigame 2D. Regras comuns (spec tarefas-e-minijogos):
/// limite de tempo; termina sempre com nota de 0 a 1, sem recomeçar ao falhar;
/// no fim do tempo a nota é o progresso alcançado; SAIR termina com o progresso atual.
/// Cada minigame só preenche a "Area" em Construir().
/// </summary>
public abstract class Minigame
{
    public Txt titulo = new Txt("Minijogo", "Minigame");
    public Txt instrucao = Txt.Nada;
    public float duracao = 15f;

    protected UIJogo UI { get; private set; }
    protected RectTransform Area { get; private set; }

    /// <summary>Botão que recebe a seleção inicial do controle (opcional).</summary>
    public Button PrimeiroBotao { get; protected set; }

    Action<ResultadoMinijogo> _aoTerminar;
    float _t, _fechaEm = -1f;
    bool _terminou;
    ResultadoMinijogo _res;

    /// <summary>Segundos desde o início do minigame.</summary>
    public float Tempo => _t;

    public void Abrir(UIJogo ui, RectTransform area, Action<ResultadoMinijogo> aoTerminar)
    {
        UI = ui; Area = area; _aoTerminar = aoTerminar;
        _terminou = false; _fechaEm = -1f; _t = 0f;
        Ponteiro.Iniciar();
        UI.LimparArea();
        UI.Mensagem("");
        Construir();
        UI.Tempo(1f, duracao);
    }

    protected abstract void Construir();
    protected virtual void Atualizar() { }

    /// <summary>Progresso de 0 a 1 do objetivo, usado quando o tempo acaba ou o jogador sai.</summary>
    protected abstract float Progresso();

    /// <summary>Sair conta como objetivo concluído? (só a coleta de pedras: sair = voltar para a porta)</summary>
    protected virtual bool SairConclui => false;

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
            if (_fechaEm <= 0f) { _terminou = true; _aoTerminar?.Invoke(_res); }
            return;
        }

        _t += dt;
        UI.Tempo(Mathf.Clamp01(1f - _t / duracao), Mathf.Max(0f, duracao - _t));
        if (_t >= duracao) { Encerrar(Progresso(), false, Textos.TempoEsgotado); return; }
        Atualizar();
    }

    /// <summary>Objetivo cumprido, com a nota calculada pelo minigame.</summary>
    protected void Terminar(float nota) => Encerrar(nota, true, Textos.Concluido);

    /// <summary>Objetivo cumprido com nota máxima.</summary>
    protected void Vencer() => Terminar(1f);

    void Encerrar(float nota, bool concluiu, Txt msg)
    {
        if (_terminou || _fechaEm >= 0f) return;
        _res = new ResultadoMinijogo(Mathf.Clamp01(nota), Mathf.Min(_t, duracao), concluiu);
        UI.Mensagem(msg.ToString() + "  " + Textos.Nota.F(Mathf.RoundToInt(_res.Nota * 100f)));
        _fechaEm = 1.1f;
    }

    /// <summary>SAIR (botão ou B): termina na hora com o progresso atual. Nunca é desistência grátis.</summary>
    public void Cancelar()
    {
        if (_terminou || _fechaEm >= 0f) return;
        _res = new ResultadoMinijogo(Mathf.Clamp01(Progresso()), _t, SairConclui);
        _terminou = true;
        _aoTerminar?.Invoke(_res);
    }

    // ---------- Ajudas de desenho/entrada ----------

    protected Image Retangulo(Vector2 pos, Vector2 tam, Color cor, Transform pai = null)
        => UI.Retangulo(pai != null ? pai : Area, pos, tam, cor);

    protected Text Rotulo(string txt, int tamanho, Vector2 pos, Vector2 area, Transform pai = null)
    {
        var t = UI.Texto(pai != null ? pai : Area, txt, tamanho, new Vector2(0.5f, 0.5f), area);
        t.rectTransform.anchoredPosition = pos;
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
