using DezDias;
using UnityEngine;

/// <summary>
/// Sons de interface (design D10 da mudança interface-e-minijogos): clipes gerados em código por <see cref="SomSintetico"/>,
/// tocados num AudioSource próprio. Ligado ou mudo fica em PlayerPrefs (tdm_som, 1 = ligado). No WebGL o áudio só começa
/// depois do primeiro gesto, o que não pesa: todo som sai de um toque ou de um clique.
/// </summary>
public static class SonsUI
{
    const string ChaveSom = "tdm_som";
    const float Volume = 0.6f;

    static AudioSource _fonte;
    static AudioClip _toque, _acerto, _concluido, _naoDeu, _painel, _alarme;

    /// <summary>Quantos sons foram pedidos com o som ligado (diagnóstico).</summary>
    public static int Tocados { get; private set; }
    /// <summary>Nome do último som tocado (diagnóstico).</summary>
    public static string Ultimo { get; private set; } = "";

    public static bool Ligado
    {
        get { try { return PlayerPrefs.GetInt(ChaveSom, 1) == 1; } catch { return true; } }
        set { try { PlayerPrefs.SetInt(ChaveSom, value ? 1 : 0); PlayerPrefs.Save(); } catch { } }
    }

    public static void Toque() => Tocar(ref _toque, "toque", SomSintetico.Toque);
    public static void Acerto() => Tocar(ref _acerto, "acerto", SomSintetico.Acerto);
    public static void Concluido() => Tocar(ref _concluido, "concluido", SomSintetico.Concluido);
    /// <summary>Fim sem sucesso: o toque de "feito", mais grave e suave. Nunca um som de erro (GDD 4.2, decisão 25).</summary>
    public static void NaoDeu() => Tocar(ref _naoDeu, "naodeu", SomSintetico.NaoDeu);
    public static void Painel() => Tocar(ref _painel, "painel", SomSintetico.Painel);
    /// <summary>Alarme de partículas do Sol (evento do dia).</summary>
    public static void Alarme() => Tocar(ref _alarme, "alarme", SomSintetico.Alarme);

    static void Tocar(ref AudioClip clip, string nome, System.Func<float[]> gerar)
    {
        if (!Ligado) return;
        if (clip == null)
        {
            var dados = gerar();
            clip = AudioClip.Create("ui_" + nome, dados.Length, 1, SomSintetico.Taxa, false);
            clip.SetData(dados, 0);
        }
        Tocados++; Ultimo = nome;
        if (_fonte == null)
        {
            var go = new GameObject("SonsUI") { hideFlags = HideFlags.HideAndDontSave };
            Object.DontDestroyOnLoad(go);
            _fonte = go.AddComponent<AudioSource>();
            _fonte.playOnAwake = false; _fonte.spatialBlend = 0f; _fonte.ignoreListenerPause = true;
        }
        _fonte.PlayOneShot(clip, Volume);
    }

    // O Editor pode entrar no Play sem recarregar o domínio: zera o estado estático.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar()
    {
        _fonte = null; _toque = _acerto = _concluido = _naoDeu = _painel = _alarme = null;
        Tocados = 0; Ultimo = "";
    }
}
