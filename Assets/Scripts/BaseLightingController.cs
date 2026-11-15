using System;
using System.Collections.Generic;
using UnityEngine;

public enum PresetLuz { Dia, Erupcao, Noite }

[Serializable] class PMult { public float sol, terra, ext, emerg; public float @int; }
[Serializable] class PEmis { public float[] cor; public float intensidade; }
[Serializable] class PCen { public PMult multiplicadores; public float[] sol_cor, sol_rotacao_graus, cor_do_mundo; public PEmis emissivo_lampadas; }
[Serializable] class PCenarios { public PCen dia, erupcao, noite; }
[Serializable] class PArquivo { public PCenarios cenarios; }

/// <summary>
/// Aplica os cenários de iluminação de iluminacao_presets.json (Dia, Erupção, Noite) na base:
/// multiplicador por grupo de luz, cor e rotação do sol, cor do mundo e emissão das lâmpadas.
/// API: SetPreset(PresetLuz, transição opcional). Funciona também fora do Play (AplicarImediato).
/// </summary>
public class BaseLightingController : MonoBehaviour
{
    [Header("Dados")]
    public TextAsset presetsJson;
    public Transform raizCenario;      // meshes da base (para achar materiais emissivos)
    public Light sol, terra;           // luzes direcionais

    [Header("Calibração")]
    [Tooltip("Blender → Unity: intensidade do sol/terra = valor do preset × este fator")]
    public float fatorSol = 0.45f;
    [Tooltip("Luz da Terra (noite): mais fraca que o sol")] public float fatorTerra = 0.28f;
    [Tooltip("Luz ambiente mínima (por canal). Evita preto total se uma luz ficar de fora, ex.: no celular.")] public float pisoAmbiente = 0.035f;
    public float pulsoHz = 1.5f;
    public PresetLuz presetInicial = PresetLuz.Dia;
    public float transicaoPadrao = 1.5f;
    public Color tintCeuDia = new Color(0.5f, 0.5f, 0.5f);
    public Color tintCeuErupcao = new Color(0.85f, 0.3f, 0.3f);
    public Color tintCeuNoite = new Color(0.4f, 0.42f, 0.5f);

    [Header("Orçamento de luzes")]
    [Tooltip("Máximo de luzes pontuais/spot ligadas ao mesmo tempo (as mais próximas da câmera). Na WebGL e no celular o URP só " +
             "aceita 32 luzes adicionais visíveis e descarta o resto, o que faz luzes apagarem e acenderem ao andar (ex.: corredor da estufa).")]
    public int maxLuzesAtivas = 24;
    [Tooltip("Velocidade do fade quando uma luz entra ou sai do orçamento (1/segundos)")] public float velocidadeFade = 4f;

    class Estado
    {
        public float sol, terra, @int, ext, emerg;
        public Color solCor, mundo, tintCeu;
        public Quaternion solRot;
        public bool emisPadrao; public Color emisCor; public float emisInt;
    }

    class Emissivo { public Renderer r; public int slot; public Color unidade; public float intensidadeBase; public bool lampada; }

    [NonSerialized] PArquivo _dados;
    readonly List<LightGroup> _luzes = new List<LightGroup>();
    float[] _alvo, _fade;                              // intensidade do preset e fator do orçamento, por luz de _luzes
    readonly List<int> _candidatas = new List<int>();
    float[] _dist;
    Comparison<int> _porDistancia;
    Camera _cam;
    readonly List<Emissivo> _emissivos = new List<Emissivo>();
    Estado _atual, _de, _para;
    float _t = 1f, _duracao;
    Material _skyClone;
    MaterialPropertyBlock _mpb;

    public PresetLuz PresetAtual { get; private set; }

    void Awake() { Preparar(); }
    void Start()
    {
        if (Application.isPlaying) SetPreset(presetInicial, 0f);
    }

    void Preparar()
    {
        if (_dados == null && presetsJson != null)
        {
            // O JSON usa "cor": "padrao" (texto) ou [r,g,b]; JsonUtility só lê o segundo formato.
            string txt = presetsJson.text.Replace("\"cor\": \"padrao\"", "\"cor\": []");
            _dados = JsonUtility.FromJson<PArquivo>(txt);
        }
        _luzes.Clear();
        _luzes.AddRange(transform.root.GetComponentsInChildren<LightGroup>(true));
        _alvo = null; GarantirVetores();
        ColetarEmissivos();
        if (Application.isPlaying && _skyClone == null && RenderSettings.skybox != null)
        {
            _skyClone = new Material(RenderSettings.skybox);
            CeuEstrelado.Aplicar(_skyClone);
            RenderSettings.skybox = _skyClone;
        }
    }

    void ColetarEmissivos()
    {
        _emissivos.Clear();
        if (raizCenario == null) return;
        foreach (var r in raizCenario.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                bool lamp = m.name.StartsWith("Lampada");
                bool outro = m.name.StartsWith("Tela") || m.name.StartsWith("Luz") || m.name.StartsWith("Cupula luz");
                if (!lamp && !outro) continue;
                var c = m.GetColor("_EmissionColor");
                float max = Mathf.Max(c.r, c.g, c.b);
                if (max <= 0.0001f) continue;
                _emissivos.Add(new Emissivo { r = r, slot = i, unidade = c / max, intensidadeBase = max, lampada = lamp });
            }
        }
    }

    Estado Montar(PresetLuz p)
    {
        PCen c = p == PresetLuz.Dia ? _dados.cenarios.dia : p == PresetLuz.Erupcao ? _dados.cenarios.erupcao : _dados.cenarios.noite;
        var e = new Estado
        {
            sol = c.multiplicadores.sol, terra = c.multiplicadores.terra, @int = c.multiplicadores.@int,
            ext = c.multiplicadores.ext, emerg = c.multiplicadores.emerg,
            solCor = new Color(c.sol_cor[0], c.sol_cor[1], c.sol_cor[2]),
            mundo = new Color(c.cor_do_mundo[0], c.cor_do_mundo[1], c.cor_do_mundo[2]),
            solRot = BlenderParaUnity(c.sol_rotacao_graus[0], c.sol_rotacao_graus[1], c.sol_rotacao_graus[2]),
            emisInt = c.emissivo_lampadas.intensidade,
            tintCeu = p == PresetLuz.Dia ? tintCeuDia : p == PresetLuz.Erupcao ? tintCeuErupcao : tintCeuNoite,
        };
        var cor = c.emissivo_lampadas.cor;
        e.emisPadrao = cor == null || cor.Length < 3;
        if (!e.emisPadrao) e.emisCor = new Color(cor[0], cor[1], cor[2]);
        return e;
    }

    /// <summary>Euler XYZ do Blender (sol aponta para -Z local) → rotação do Unity (x,y,z) → (-x, z, -y).</summary>
    static Quaternion BlenderParaUnity(float rx, float ry, float rz)
    {
        // Composição do Blender (Rz·Ry·Rx) aplicada ao vetor "para baixo" (-Z) do sol.
        Vector3 d = RotBlender(rx, ry, rz).MultiplyVector(new Vector3(0, 0, -1));
        Vector3 du = new Vector3(-d.x, d.z, -d.y);
        return Quaternion.LookRotation(du.normalized);
    }

    static Matrix4x4 RotBlender(float rx, float ry, float rz)
    {
        float a = rx * Mathf.Deg2Rad, b = ry * Mathf.Deg2Rad, c = rz * Mathf.Deg2Rad;
        var Rx = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, Mathf.Cos(a), Mathf.Sin(a), 0), new Vector4(0, -Mathf.Sin(a), Mathf.Cos(a), 0), new Vector4(0, 0, 0, 1));
        var Ry = new Matrix4x4(new Vector4(Mathf.Cos(b), 0, -Mathf.Sin(b), 0), new Vector4(0, 1, 0, 0), new Vector4(Mathf.Sin(b), 0, Mathf.Cos(b), 0), new Vector4(0, 0, 0, 1));
        var Rz = new Matrix4x4(new Vector4(Mathf.Cos(c), Mathf.Sin(c), 0, 0), new Vector4(-Mathf.Sin(c), Mathf.Cos(c), 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
        return Rz * Ry * Rx;
    }

    static Estado Mistura(Estado a, Estado b, float t)
    {
        return new Estado
        {
            sol = Mathf.Lerp(a.sol, b.sol, t), terra = Mathf.Lerp(a.terra, b.terra, t), @int = Mathf.Lerp(a.@int, b.@int, t),
            ext = Mathf.Lerp(a.ext, b.ext, t), emerg = Mathf.Lerp(a.emerg, b.emerg, t),
            solCor = Color.Lerp(a.solCor, b.solCor, t), mundo = Color.Lerp(a.mundo, b.mundo, t), tintCeu = Color.Lerp(a.tintCeu, b.tintCeu, t),
            solRot = Quaternion.Slerp(a.solRot, b.solRot, t),
            emisPadrao = t < 0.5f ? a.emisPadrao : b.emisPadrao,
            emisCor = Color.Lerp(a.emisPadrao ? Color.white : a.emisCor, b.emisPadrao ? Color.white : b.emisCor, t),
            emisInt = Mathf.Lerp(a.emisInt, b.emisInt, t),
        };
    }

    /// <summary>Troca de cenário, com transição suave (segundos). 0 = imediato.</summary>
    public void SetPreset(PresetLuz p, float transicao = -1f)
    {
        if (_dados == null || _dados.cenarios == null || _dados.cenarios.dia == null || _dados.cenarios.dia.sol_cor == null || _dados.cenarios.dia.sol_cor.Length < 3)
        {
            _dados = null;
            Preparar();
        }
        if (_dados == null) { Debug.LogWarning("[BaseLightingController] presetsJson não atribuído."); return; }
        if (transicao < 0f) transicao = transicaoPadrao;
        if (Application.isPlaying) Alarme(p == PresetLuz.Erupcao, PresetAtual == PresetLuz.Erupcao);
        PresetAtual = p;
        _para = Montar(p);
        if (_atual == null || transicao <= 0f || !Application.isPlaying)
        {
            _atual = _para; _t = 1f; Aplicar(_atual);
            return;
        }
        _de = _atual; _t = 0f; _duracao = transicao;
    }

    /// <summary>Aplica imediatamente (usado fora do Play, ex.: capturas do editor).</summary>
    public void AplicarImediato(PresetLuz p) { SetPreset(p, 0f); }

    [Header("Alarme do evento climático (Resources/Sons/Alarme)")]
    [Tooltip("Quantas vezes o alarme toca quando as luzes vermelhas ligam. 0 = enquanto elas estiverem ligadas.")]
    [Min(0)] public int repeticoesAlarme = 3;
    [Range(0f, 1f)] public float volumeAlarme = 0.5f;

    AudioSource _alarme;
    float _fimAlarme;

    /// <summary>Liga o alarme quando entra o cenário de Erupção (luzes vermelhas) e desliga quando ele sai.</summary>
    void Alarme(bool ligar, bool jaEstava)
    {
        if (!ligar) { if (_alarme) _alarme.Stop(); return; }
        if (jaEstava && _alarme && _alarme.isPlaying) return;
        if (!SonsUI.Ligado) return;
        var clip = SonsUI.Arquivo("Alarme");
        if (!clip) return;
        if (!_alarme)
        {
            _alarme = gameObject.AddComponent<AudioSource>();
            _alarme.playOnAwake = false; _alarme.loop = true; _alarme.spatialBlend = 0f;
        }
        _alarme.clip = clip;
        _alarme.volume = volumeAlarme;
        _alarme.Play();
        _fimAlarme = repeticoesAlarme > 0 ? Time.time + clip.length * repeticoesAlarme : float.MaxValue;
    }

    void Update()
    {
        if (_alarme && _alarme.isPlaying && Time.time >= _fimAlarme) _alarme.Stop();
        if (_atual == null) return;
        if (_t < 1f)
        {
            _t = Mathf.Min(1f, _t + Time.deltaTime / Mathf.Max(0.01f, _duracao));
            _atual = Mistura(_de, _para, Mathf.SmoothStep(0f, 1f, _t));
            Aplicar(_atual);
        }
        else if (_atual.emerg > 0.001f) AplicarEmergencia(_atual); // pulsa a cada frame
    }

    // Depois do Movimento e do CameraFoco (LateUpdate), com a câmera já no lugar deste quadro.
    void LateUpdate()
    {
        if (_atual == null || !Application.isPlaying) return;
        Orcamento(Time.unscaledDeltaTime);
    }

    /// <summary>
    /// Liga só as <see cref="maxLuzesAtivas"/> luzes (int/ext/emerg com intensidade) mais próximas da câmera, com fade.
    /// As paredes não bloqueiam a culling de luz do Unity: do corredor da estufa a câmera "vê" 35 a 52 esferas de luz,
    /// e acima de 32 (WebGL/celular) o URP descarta luzes de forma instável entre quadros.
    /// </summary>
    void Orcamento(float dt)
    {
        if (_alvo == null || _alvo.Length != _luzes.Count) return;
        if (_cam == null) _cam = Camera.main;
        Vector3 c = _cam != null ? _cam.transform.position : Vector3.zero;
        _candidatas.Clear();
        for (int i = 0; i < _luzes.Count; i++)
        {
            var g = _luzes[i];
            if (g == null || g.Luz.type == LightType.Directional || _alvo[i] <= 0.001f) continue;
            // distância ao centro (não à borda do alcance: dentro do hub muitas esferas empatariam em 0 e a ordem pularia)
            _dist[i] = (c - g.transform.position).sqrMagnitude;
            _candidatas.Add(i);
        }
        if (_porDistancia == null) _porDistancia = (a, b) => { int r = _dist[a].CompareTo(_dist[b]); return r != 0 ? r : a.CompareTo(b); };
        _candidatas.Sort(_porDistancia);
        float passo = velocidadeFade * dt;
        for (int k = 0; k < _candidatas.Count; k++)
        {
            int i = _candidatas[k];
            _fade[i] = Mathf.MoveTowards(_fade[i], k < maxLuzesAtivas ? 1f : 0f, passo);
        }
        for (int i = 0; i < _luzes.Count; i++)
        {
            var g = _luzes[i];
            if (g == null || g.Luz.type == LightType.Directional) continue;
            float inten = _alvo[i] * _fade[i];
            g.Luz.intensity = inten;
            g.Luz.enabled = inten > 0.001f;
        }
    }

    void Aplicar(Estado e)
    {
        if (sol != null) { sol.intensity = e.sol * fatorSol; sol.color = e.solCor.gamma; sol.transform.rotation = e.solRot; }
        if (terra != null) terra.intensity = e.terra * fatorTerra;
        GarantirVetores();
        for (int i = 0; i < _luzes.Count; i++)
        {
            var g = _luzes[i];
            if (g == null) continue;
            switch (g.grupo)
            {
                case "int": _alvo[i] = g.energiaBase * e.@int; break;
                case "ext": _alvo[i] = g.energiaBase * e.ext; break;
                case "emerg": continue; // tratado em AplicarEmergencia
                default: g.Luz.enabled = g.Luz.intensity > 0.001f; continue; // sol/terra: intensidade já definida acima
            }
            g.Luz.intensity = _alvo[i] * Fade(i);
            g.Luz.enabled = g.Luz.intensity > 0.001f;
        }
        AplicarEmergencia(e);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        var amb = e.mundo.gamma;
        RenderSettings.ambientLight = new Color(Mathf.Max(amb.r, pisoAmbiente), Mathf.Max(amb.g, pisoAmbiente), Mathf.Max(amb.b, pisoAmbiente));
        if (_skyClone != null && _skyClone.HasProperty("_Tint")) _skyClone.SetColor("_Tint", e.tintCeu);

        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        foreach (var em in _emissivos)
        {
            if (em.r == null) continue;
            Color cor; float inten;
            if (em.lampada) { cor = e.emisPadrao ? em.unidade : e.emisCor; inten = e.emisInt; }
            else { cor = em.unidade; inten = em.intensidadeBase * (e.emisInt / 3f); }
            em.r.GetPropertyBlock(_mpb, em.slot);
            _mpb.SetColor("_EmissionColor", cor * inten);
            em.r.SetPropertyBlock(_mpb, em.slot);
        }
    }

    void AplicarEmergencia(Estado e)
    {
        float pulso = Application.isPlaying ? 0.25f + 0.75f * (0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * pulsoHz * Time.time)) : 1f;
        for (int i = 0; i < _luzes.Count; i++)
        {
            var g = _luzes[i];
            if (g == null || g.grupo != "emerg") continue;
            _alvo[i] = e.emerg > 0.001f ? g.energiaBase * e.emerg * pulso : 0f;
            g.Luz.intensity = _alvo[i] * Fade(i);
            g.Luz.enabled = g.Luz.intensity > 0.001f;
        }
    }

    void GarantirVetores()
    {
        if (_alvo != null && _alvo.Length == _luzes.Count) return;
        _alvo = new float[_luzes.Count]; _fade = new float[_luzes.Count]; _dist = new float[_luzes.Count];
        for (int i = 0; i < _fade.Length; i++) _fade[i] = 1f;
    }

    // Fora do Play não há orçamento: tudo aceso (capturas do editor).
    float Fade(int i) => Application.isPlaying && _fade != null && i < _fade.Length ? _fade[i] : 1f;

    // Testes no editor (menu de contexto do componente)
    [ContextMenu("Preset: Dia")] void TesteDia() => AplicarImediato(PresetLuz.Dia);
    [ContextMenu("Preset: Erupção")] void TesteErupcao() => AplicarImediato(PresetLuz.Erupcao);
    [ContextMenu("Preset: Noite")] void TesteNoite() => AplicarImediato(PresetLuz.Noite);
}
