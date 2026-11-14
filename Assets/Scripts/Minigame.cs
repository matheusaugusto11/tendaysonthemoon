using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base de todo minigame 2D. Regras comuns: limite de 15 s; ao falhar ou estourar o tempo
/// o minigame recomeça do início. Cada minigame só preenche a "Area" em Construir().
/// </summary>
public abstract class Minigame
{
    public string titulo = "Minigame";
    public string instrucao = "";
    public float duracao = 15f;

    protected UIJogo UI { get; private set; }
    protected RectTransform Area { get; private set; }

    /// <summary>Botão que recebe a seleção inicial do controle (opcional).</summary>
    public Button PrimeiroBotao { get; protected set; }

    Action<bool> _aoTerminar;
    float _restante, _pausaMsg, _fechaEm = -1f;
    bool _terminou;
    bool _recomecar;

    public void Abrir(UIJogo ui, RectTransform area, Action<bool> aoTerminar)
    {
        UI = ui; Area = area; _aoTerminar = aoTerminar;
        _terminou = false; _fechaEm = -1f; _recomecar = false;
        Ponteiro.Iniciar();
        Comecar();
    }

    void Comecar()
    {
        UI.LimparArea();
        UI.Mensagem("");
        _restante = duracao;
        Construir();
        UI.Tempo(1f, duracao);
    }

    protected abstract void Construir();
    protected virtual void Atualizar() { }

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
            if (_fechaEm <= 0f) { _terminou = true; _aoTerminar?.Invoke(true); }
            return;
        }
        if (_recomecar)
        {
            _pausaMsg -= dt;
            if (_pausaMsg <= 0f) { _recomecar = false; Comecar(); }
            return;
        }

        _restante -= dt;
        UI.Tempo(Mathf.Clamp01(_restante / duracao), Mathf.Max(0f, _restante));
        if (_restante <= 0f) { Falhar("Tempo esgotado! Tente de novo."); return; }
        Atualizar();
    }

    /// <summary>Falhou: mostra a mensagem e recomeça do início.</summary>
    protected void Falhar(string msg)
    {
        if (_terminou || _recomecar || _fechaEm >= 0f) return;
        UI.Mensagem(msg);
        _recomecar = true;
        _pausaMsg = 1.4f;
    }

    /// <summary>Concluiu com sucesso: mostra "Concluído!" por um instante e fecha.</summary>
    protected void Vencer()
    {
        if (_terminou || _recomecar || _fechaEm >= 0f) return;
        UI.Mensagem("Concluído!");
        _fechaEm = 0.8f;
    }

    /// <summary>Atalho para minigames com botões de UI (provisório).</summary>
    protected void Terminar(bool sucesso) { if (sucesso) Vencer(); }

    /// <summary>Cancelar (botão SAIR / B): fecha sem concluir.</summary>
    public void Cancelar()
    {
        if (_terminou) return;
        _terminou = true;
        _aoTerminar?.Invoke(false);
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
