using DezDias;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mão fantasma que repete o gesto de um minijogo (tarefas 7.7 e 7.8 da mudança interface-e-minijogos): arrastar,
/// esfregar, tocar no ritmo, segurar, tocar para girar ou arrastar para ligar. Tem dois usos:
/// - <b>com cenário</b>: a ilustração do cartão de primeira vez, que desenha os próprios objetos (pedra e caixa, cano...);
/// - <b>sem cenário</b>: por cima do minijogo de verdade, nos pontos que ele indica (Dia 1 com tutorial e o botão "?").
/// Com movimento reduzido a mão não anda: fica parada no ponto do gesto, com o objeto já no estado final.
/// </summary>
public class DemoGesto : MonoBehaviour
{
    public Mecanica Mecanica { get; private set; }
    public bool ComCenario { get; private set; }
    /// <summary>Ciclos completos do gesto mostrados até agora.</summary>
    public int Ciclos => Mathf.FloorToInt((Time.unscaledTime - _t0) / _ciclo);

    Vector2 _de, _para;
    float _t0, _ciclo, _alfaProp;
    RectTransform _mao, _prop;
    Image _maoImg;
    FormaUI _propForma, _anel, _onda;
    Graphic[] _propPartes;
    RectTransform _canoGiro;

    public static float CicloDe(Mecanica m) => m == Mecanica.Esteira || m == Mecanica.Canos ? 1.0f : 1.5f;

    /// <summary>Texto do gesto, para a legenda do cartão.</summary>
    public static Txt Legenda(Mecanica m) => m switch
    {
        Mecanica.Lixo => Textos.GestoArrastar, Mecanica.Raspadinha => Textos.GestoEsfregar, Mecanica.Esteira => Textos.GestoRitmo,
        Mecanica.Segurar => Textos.GestoSegurar, Mecanica.Canos => Textos.GestoGirar, _ => Textos.GestoLigar,
    };

    /// <summary>
    /// Cria a demonstração dentro de <paramref name="pai"/> (coordenadas locais com o centro em 0,0). Com cenário, os
    /// pontos <paramref name="de"/> e <paramref name="para"/> são ignorados e a ilustração usa os seus.
    /// </summary>
    public static DemoGesto Criar(Transform pai, Mecanica mec, Vector2 de, Vector2 para, bool cenario, Color corSistema)
    {
        var go = new GameObject("DemoGesto", typeof(RectTransform), typeof(DemoGesto), typeof(CanvasGroup));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var cg = go.GetComponent<CanvasGroup>(); cg.blocksRaycasts = false; cg.interactable = false;
        var d = go.GetComponent<DemoGesto>();
        d.Montar(mec, de, para, cenario, corSistema);
        return d;
    }

    static RectTransform No(Transform pai, string nome, Vector2 pos, Vector2 tam)
    {
        var rt = new GameObject(nome, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam; rt.anchoredPosition = pos;
        return rt;
    }

    static FormaUI F(Transform pai, FormaUI.Modo modo, Vector2 pos, Vector2 tam, Color cor, string nome = "Forma")
    {
        var f = FormaUI.Criar(pai, modo, cor, nome);
        var rt = f.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam; rt.anchoredPosition = pos;
        return f;
    }

    static FormaUI Chanfro(Transform pai, Vector2 pos, Vector2 tam, Color cor, float ch = UiTema.ChanfroBotao)
    {
        var f = F(pai, FormaUI.Modo.Chanfrado, pos, tam, cor, "Chanfro");
        f.chanfro = ch;
        return f;
    }

    void Montar(Mecanica mec, Vector2 de, Vector2 para, bool cenario, Color cor)
    {
        Mecanica = mec; ComCenario = cenario; _ciclo = CicloDe(mec); _t0 = Time.unscaledTime;
        _alfaProp = cenario ? 1f : 0.55f;
        if (cenario) PontosDoCenario(mec, out de, out para);
        _de = de; _para = para;
        var raiz = transform;
        if (cenario) MontarCenario(mec, cor);

        // Objeto que a mão leva (pedra, alavanca) ou o anel do toque.
        switch (mec)
        {
            case Mecanica.Lixo:
            {
                _prop = No(raiz, "Pedra", de, new Vector2(96, 96));
                var sombra = F(_prop, FormaUI.Modo.Poligono, new Vector2(0, -5), new Vector2(96, 90), UiTema.Sombra, "Sombra");
                sombra.lados = 7; sombra.irregularidade = 0.25f; sombra.semente = 3;
                _propForma = F(_prop, FormaUI.Modo.Poligono, Vector2.zero, new Vector2(96, 90), new Color(0.62f, 0.6f, 0.66f), "Face");
                _propForma.lados = 7; _propForma.irregularidade = 0.25f; _propForma.semente = 3;
                var faceta = F(_prop, FormaUI.Modo.Poligono, new Vector2(-10, 12), new Vector2(46, 40), new Color(0.74f, 0.72f, 0.78f), "Faceta");
                faceta.lados = 5; faceta.irregularidade = 0.2f; faceta.semente = 9;
                _propPartes = new Graphic[] { sombra, _propForma, faceta };
                break;
            }
            case Mecanica.Disjuntores:
            {
                _prop = No(raiz, "Alavanca", de, new Vector2(110, 60));
                var s = Chanfro(_prop, new Vector2(0, -5), new Vector2(110, 56), UiTema.Sombra);
                _propForma = Chanfro(_prop, Vector2.zero, new Vector2(110, 56), UiTema.Branco);
                var g = Chanfro(_prop, Vector2.zero, new Vector2(70, 8), UiTema.BrancoSombra, 0);
                _propPartes = new Graphic[] { s, _propForma, g };
                break;
            }
            case Mecanica.Segurar:
                _anel = F(raiz, FormaUI.Modo.Anel, de, cenario ? new Vector2(210, 210) : new Vector2(260, 260), cor, "Anel");
                _anel.espessura = cenario ? 20 : 24; _anel.fracao = 0f;
                _propPartes = new Graphic[] { _anel };
                break;
            case Mecanica.Esteira: case Mecanica.Canos:
                _onda = F(raiz, FormaUI.Modo.Anel, de, new Vector2(60, 60), UiTema.Branco, "Toque");
                _onda.espessura = 6;
                _propPartes = new Graphic[] { _onda };
                break;
        }
        if (_propPartes != null) foreach (var p in _propPartes) { var c = p.color; c.a *= _alfaProp; p.color = c; }

        _mao = No(raiz, "Mao", de, new Vector2(96, 96));
        _mao.pivot = new Vector2(0.47f, 0.9f); // a ponta do dedo fica no ponto do gesto
        _maoImg = _mao.gameObject.AddComponent<Image>();
        _maoImg.sprite = UiTema.Icone("mao"); _maoImg.preserveAspect = true; _maoImg.raycastTarget = false;
        _maoImg.color = new Color(1, 1, 1, 0.92f);
        Atualizar(0f);
    }

    /// <summary>Pontos do gesto na ilustração do cartão (caixa de cerca de 520 por 240).</summary>
    static void PontosDoCenario(Mecanica m, out Vector2 de, out Vector2 para)
    {
        switch (m)
        {
            case Mecanica.Lixo: de = new Vector2(-150, -10); para = new Vector2(150, -10); break;
            case Mecanica.Raspadinha: de = new Vector2(-140, 50); para = new Vector2(140, -50); break;
            case Mecanica.Esteira: de = new Vector2(0, -20); para = de; break;
            case Mecanica.Segurar: de = Vector2.zero; para = de; break;
            case Mecanica.Canos: de = Vector2.zero; para = de; break;
            default: de = new Vector2(0, -50); para = new Vector2(0, 50); break;
        }
    }

    FormaUI _mancha;
    RectTransform _ponteiroEsteira;

    void MontarCenario(Mecanica m, Color cor)
    {
        var raiz = transform;
        switch (m)
        {
            case Mecanica.Lixo:
            {
                var caixa = Chanfro(raiz, _para, new Vector2(170, 140), UiTema.AcoClaro);
                var tira = Chanfro(raiz, _para + new Vector2(0, 64), new Vector2(150, 8), UiTema.Lab, 0);
                var borda = F(raiz, FormaUI.Modo.Moldura, _para, new Vector2(130, 100), UiTema.Apoio, "Alvo");
                borda.espessura = 3; borda.larguraFaixa = 12; borda.periodo = 20;
                _ = caixa; _ = tira;
                break;
            }
            case Mecanica.Raspadinha:
            {
                Chanfro(raiz, Vector2.zero, new Vector2(380, 200), UiTema.AcoClaro);
                Chanfro(raiz, Vector2.zero, new Vector2(360, 180), new Color32(0x2B, 0x5C, 0x9E, 0xFF), 4);
                _mancha = F(raiz, FormaUI.Modo.Poligono, new Vector2(0, 0), new Vector2(260, 150), new Color32(0x8C, 0x81, 0x70, 0xFF), "Mancha");
                _mancha.lados = 9; _mancha.irregularidade = 0.35f; _mancha.semente = 5;
                break;
            }
            case Mecanica.Esteira:
            {
                Chanfro(raiz, new Vector2(0, 30), new Vector2(440, 70), UiTema.AcoClaro);
                Chanfro(raiz, new Vector2(60, 30), new Vector2(66, 70), UiTema.Estufa, 4);
                _ponteiroEsteira = No(raiz, "Ponteiro", new Vector2(-200, 82), new Vector2(34, 30));
                var tri = F(_ponteiroEsteira, FormaUI.Modo.Triangulo, Vector2.zero, new Vector2(34, 30), UiTema.Branco, "Tri");
                tri.rotacao = 180f;
                break;
            }
            case Mecanica.Segurar:
            {
                var trilho = F(raiz, FormaUI.Modo.Anel, Vector2.zero, new Vector2(210, 210), UiTema.AcoClaro, "Trilho");
                trilho.espessura = 20;
                var botao = F(raiz, FormaUI.Modo.Poligono, Vector2.zero, new Vector2(146, 146), UiTema.Branco, "Botao");
                botao.lados = 40; botao.irregularidade = 0f;
                break;
            }
            case Mecanica.Canos:
            {
                Chanfro(raiz, new Vector2(-100, 0), new Vector2(70, 36), UiTema.AguaUsada, 0);
                Chanfro(raiz, new Vector2(0, 90), new Vector2(36, 50), UiTema.Ar, 0);
                Chanfro(raiz, Vector2.zero, new Vector2(130, 130), UiTema.AcoClaro);
                _canoGiro = No(raiz, "Giro", Vector2.zero, new Vector2(130, 130));
                Chanfro(_canoGiro, new Vector2(0, 28), new Vector2(36, 74), UiTema.Apoio, 0);
                Chanfro(_canoGiro, new Vector2(28, 0), new Vector2(74, 36), UiTema.Apoio, 0);
                break;
            }
            default: // Disjuntores
            {
                Chanfro(raiz, Vector2.zero, new Vector2(150, 230), UiTema.AcoClaro);
                Chanfro(raiz, new Vector2(0, -6), new Vector2(64, 170), UiTema.Vazio);
                var led = F(raiz, FormaUI.Modo.Poligono, new Vector2(0, 98), new Vector2(26, 26), cor, "Led");
                led.lados = 20; led.irregularidade = 0f;
                break;
            }
        }
    }

    static float Suave(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    void Update() => Atualizar(Time.unscaledTime - _t0);

    void Atualizar(float tempo)
    {
        bool parado = UiTema.MovimentoReduzido;
        float k = parado ? 0.5f : (tempo % _ciclo) / _ciclo;
        Vector2 mao = _de;
        bool aperta = false;
        float alfaMao = 0.92f;
        switch (Mecanica)
        {
            case Mecanica.Lixo: case Mecanica.Disjuntores:
            {
                if (parado) { mao = _para; aperta = true; if (_prop) _prop.anchoredPosition = _para; break; }
                Vector2 entrada = _de + new Vector2(60, -50);
                if (k < 0.15f) { mao = Vector2.Lerp(entrada, _de, Suave(k / 0.15f)); alfaMao *= k / 0.15f; }
                else if (k < 0.25f) { mao = _de; aperta = true; }
                else if (k < 0.75f) { mao = Vector2.Lerp(_de, _para, Suave((k - 0.25f) / 0.5f)); aperta = true; }
                else if (k < 0.85f) mao = _para;
                else { mao = _para; alfaMao *= 1f - (k - 0.85f) / 0.15f; }
                if (_prop)
                {
                    _prop.anchoredPosition = k < 0.25f ? _de : k < 0.75f ? mao : _para;
                    float a = Mecanica == Mecanica.Lixo && k > 0.85f ? 1f - (k - 0.85f) / 0.15f : 1f;
                    foreach (var p in _propPartes) { var c = p.color; c.a = a * _alfaProp * (p == _propPartes[0] ? 0.9f : 1f); p.color = c; }
                }
                break;
            }
            case Mecanica.Raspadinha:
            {
                aperta = k > 0.08f && k < 0.92f;
                float onda = Mathf.PingPong(k * 4f, 1f);
                mao = new Vector2(Mathf.Lerp(_de.x, _para.x, onda), Mathf.Lerp(_de.y, _para.y, k));
                if (parado) mao = (_de + _para) * 0.5f;
                if (_mancha) { var c = _mancha.color; c.a = parado ? 0.35f : Mathf.Lerp(1f, 0.15f, k); _mancha.color = c; }
                break;
            }
            case Mecanica.Esteira:
            {
                float fase = (k * 2f) % 1f; // dois toques por ciclo
                aperta = !parado && fase > 0.45f && fase < 0.6f;
                mao = _de + (aperta ? Vector2.zero : new Vector2(0, -14));
                OndaDoToque(parado ? 0.3f : (fase - 0.45f) / 0.55f);
                if (_ponteiroEsteira)
                {
                    float x = parado ? 60f : Mathf.Lerp(-200f, 200f, Mathf.PingPong(tempo * 0.9f, 1f));
                    _ponteiroEsteira.anchoredPosition = new Vector2(x, 82);
                }
                break;
            }
            case Mecanica.Segurar:
            {
                if (k < 0.1f) { mao = _de + new Vector2(50, -50) * (1f - k / 0.1f); alfaMao *= k / 0.1f; }
                else { mao = _de; aperta = k < 0.9f; }
                if (_anel) { _anel.fracao = parado ? 0.7f : Mathf.Clamp01((k - 0.1f) / 0.8f); _anel.Redesenhar(); }
                break;
            }
            case Mecanica.Canos:
            {
                float fase = (k * 2f) % 1f;
                aperta = !parado && fase > 0.3f && fase < 0.45f;
                mao = _de + (aperta ? Vector2.zero : new Vector2(0, -14));
                OndaDoToque(parado ? 0.3f : (fase - 0.3f) / 0.7f);
                if (_canoGiro)
                {
                    int giros = Mathf.FloorToInt(tempo / _ciclo * 2f + 0.55f);
                    float alvo = -90f * giros;
                    float atual = _canoGiro.localEulerAngles.z;
                    if (parado) alvo = -90f;
                    float novo = Mathf.MoveTowardsAngle(atual, alvo, 750f * Time.unscaledDeltaTime); // 90 graus em 120 ms
                    _canoGiro.localRotation = Quaternion.Euler(0, 0, novo);
                }
                break;
            }
        }
        _mao.anchoredPosition = mao;
        _mao.localScale = Vector3.one * (aperta ? 0.88f : 1f);
        var cm = _maoImg.color; cm.a = alfaMao; _maoImg.color = cm;
    }

    void OndaDoToque(float k)
    {
        if (_onda == null) return;
        bool vis = k >= 0f && k <= 1f;
        _onda.gameObject.SetActive(vis);
        if (!vis) return;
        float tam = Mathf.Lerp(50f, 150f, k);
        _onda.rectTransform.sizeDelta = new Vector2(tam, tam);
        var c = _onda.color; c.a = (1f - k) * 0.9f * (ComCenario ? 1f : 0.8f); _onda.color = c;
    }
}
