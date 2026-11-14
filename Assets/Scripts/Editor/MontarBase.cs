using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Importa a base lunar (Assets/Models/Base) para a cena Game seguindo Docs/PROMPT-UNITY-BASE.md:
/// materiais URP, FBX remapeados, luzes calibradas, portas, colisores simples, controlador de iluminação.
/// Menu: Tools/Base Lunar. Também roda ao detectar Temp/pedido_montar_base.txt.
/// </summary>
[InitializeOnLoad]
public static class MontarBase
{
    const string Dir = "Assets/Models/Base/";
    const string MatDir = "Assets/Materials/Base";
    const string Cena = "Assets/Scenes/Game.unity";

    static MontarBase()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists("Temp/pedido_montar_base.txt")) { File.Delete("Temp/pedido_montar_base.txt"); Montar(); }
            if (File.Exists("Temp/pedido_capturas.txt")) { File.Delete("Temp/pedido_capturas.txt"); Capturar(); }
        };
    }

    // ---------------- JSON ----------------
    [Serializable] class LuzJ { public string nome, tipo, grupo; public float energia_blender_W, raio_m, cone_graus; public float[] cor_rgb; public bool sombra; }
    [Serializable] class LuzesJ { public LuzJ[] luzes; }
    [Serializable] class MatJ { public string nome; public float[] cor_base; public float metalico, rugosidade, emissao_intensidade, alpha; public float[] emissao_cor; public bool transparente; }
    [Serializable] class MatsJ { public MatJ[] materiais; }
    [Serializable] class PortaJ { public string objeto, porta, folha, clipe_fbx; }
    [Serializable] class PortasJ { public PortaJ[] portas; }

    static T Ler<T>(string arq) => JsonUtility.FromJson<T>(File.ReadAllText(Dir + arq));

    // ---------------- Materiais ----------------
    static Dictionary<string, Material> CriarMateriais()
    {
        Directory.CreateDirectory(MatDir);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var res = new Dictionary<string, Material>();
        foreach (var j in Ler<MatsJ>("base_materiais.json").materiais)
        {
            string caminho = $"{MatDir}/{j.nome}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            bool novo = m == null;
            if (novo) m = new Material(shader);
            m.shader = shader;
            // Cores do Blender estão em linear; o Unity espera sRGB no campo de cor → .gamma.
            var cor = new Color(j.cor_base[0], j.cor_base[1], j.cor_base[2], j.transparente ? j.alpha : 1f);
            m.SetColor("_BaseColor", new Color(Mathf.LinearToGammaSpace(cor.r), Mathf.LinearToGammaSpace(cor.g), Mathf.LinearToGammaSpace(cor.b), cor.a));
            m.SetFloat("_Metallic", j.metalico);
            m.SetFloat("_Smoothness", 1f - j.rugosidade);
            if (j.emissao_cor != null && j.emissao_cor.Length == 3 && j.emissao_intensidade > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(j.emissao_cor[0], j.emissao_cor[1], j.emissao_cor[2]) * j.emissao_intensidade);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else { m.DisableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Color.black); }
            if (j.transparente)
            {
                m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)RenderQueue.Transparent;
            }
            if (novo) AssetDatabase.CreateAsset(m, caminho); else EditorUtility.SetDirty(m);
            res[j.nome] = m;
        }
        AssetDatabase.SaveAssets();
        return res;
    }

    static void ConfigurarFbx(string caminho, Dictionary<string, Material> mats, bool animacao)
    {
        var imp = (ModelImporter)AssetImporter.GetAtPath(caminho);
        imp.importLights = true;
        imp.importCameras = false;
        imp.addCollider = false;
        imp.animationType = animacao ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
        imp.importAnimation = animacao;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Material>())
            if (mats.TryGetValue(obj.name, out var novo))
                imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), obj.name), novo);
        imp.SaveAndReimport();
    }

    // ---------------- Montagem ----------------
    [MenuItem("Tools/Base Lunar/Montar na cena Game")]
    public static void Montar()
    {
        var relatorio = new List<string>();
        var mats = CriarMateriais();
        ConfigurarFbx(Dir + "base_cenario.fbx", mats, false);
        ConfigurarFbx(Dir + "base_portas.fbx", mats, true);
        relatorio.Add($"Materiais URP/Lit criados: {mats.Count} em {MatDir}");

        var cena = EditorSceneManager.GetActiveScene();
        if (cena.path != Cena) cena = EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);

        // refaz do zero se já existir
        var antigo = GameObject.Find("Base");
        if (antigo != null) UnityEngine.Object.DestroyImmediate(antigo);

        // objetos de teste antigos: desligados (não apagados)
        DesligarTeste("Chao"); DesligarTeste("estufa (3)");
        var velhoCenario = GameObject.Find("Cenario");
        if (velhoCenario != null) { velhoCenario.name = "Cenario (teste antigo)"; velhoCenario.SetActive(false); relatorio.Add("Objetos de teste antigos (Cenario, Chao, estufa (3)) foram DESATIVADOS, não apagados."); }
        var dirLuzVelha = GameObject.Find("Directional Light");
        if (dirLuzVelha != null) dirLuzVelha.SetActive(false);

        var raiz = new GameObject("Base");
        var gCenario = Filho(raiz, "Cenario"); var gPortas = Filho(raiz, "Portas"); var gLuzes = Filho(raiz, "Luzes"); var gAmb = Filho(raiz, "Ambiente");

        // --- cenário
        var instC = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Dir + "base_cenario.fbx"));
        PrefabUtility.UnpackPrefabInstance(instC, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        var filhos = new List<Transform>(); foreach (Transform t in instC.transform) filhos.Add(t);
        var luzesJ = Ler<LuzesJ>("base_luzes.json").luzes.ToDictionary(l => l.nome);
        int nLuzes = 0, nMeshes = 0, nColisores = 0;
        Light sol = null, terra = null;
        foreach (var t in filhos)
        {
            var luz = t.GetComponent<Light>();
            if (luz != null)
            {
                t.SetParent(gLuzes.transform, true);
                if (luzesJ.TryGetValue(t.name, out var j))
                {
                    var lg = t.gameObject.AddComponent<LightGroup>();
                    Calibrar(luz, j, lg);
                    if (j.grupo == "sol") sol = luz;
                    if (j.grupo == "terra") terra = luz;
                }
                nLuzes++;
            }
            else
            {
                t.SetParent(gCenario.transform, true); nMeshes++;
                if (DeveTerColisor(t.name)) { var mf = t.GetComponent<MeshFilter>(); if (mf != null) { t.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh; nColisores++; } }
            }
            t.gameObject.isStatic = luz == null;
        }
        UnityEngine.Object.DestroyImmediate(instC);
        relatorio.Add($"Cenário: {nMeshes} malhas, {nLuzes} luzes. Colisores MeshCollider (estáticos): {nColisores}.");

        // --- portas
        var instP = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Dir + "base_portas.fbx"), gPortas.transform);
        instP.name = "base_portas";
        var clipesAsset = AssetDatabase.LoadAllAssetsAtPath(Dir + "base_portas.fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToDictionary(c => c.name);
        var som = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/doorOpening.mp3");
        int nPortas = 0, nFolhas = 0;
        foreach (var g in Ler<PortasJ>("base_portas.json").portas.GroupBy(p => p.porta))
        {
            var folhas = new List<Transform>(); var clips = new List<AnimationClip>();
            foreach (var p in g)
            {
                var f = instP.transform.Find(p.objeto);
                if (f == null) { Debug.LogWarning("[MontarBase] folha não achada: " + p.objeto); continue; }
                folhas.Add(f);
                if (clipesAsset.TryGetValue(p.clipe_fbx, out var clip)) clips.Add(clip);
                if (f.GetComponent<Collider>() == null) f.gameObject.AddComponent<BoxCollider>();
                nFolhas++;
            }
            var go = Filho(gPortas, "Porta_" + g.Key);
            var porta = go.AddComponent<Porta>();
            porta.idPorta = g.Key; porta.raiz = instP; porta.folhas = folhas.ToArray(); porta.clips = clips.ToArray(); porta.somAbrir = som;
            nPortas++;
        }
        relatorio.Add($"Portas: {nPortas} componentes Porta ({nFolhas} folhas, BoxCollider em cada folha). Abrem sozinhas a 3,2 m e tocam doorOpening.");

        // --- ambiente: controlador, sol, Terra no céu
        var ctrl = gAmb.AddComponent<BaseLightingController>();
        ctrl.presetsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(Dir + "iluminacao_presets.json");
        ctrl.raizCenario = gCenario.transform; ctrl.sol = sol; ctrl.terra = terra;
        RenderSettings.sun = sol;
        CriarTerraNoCeu(gAmb, terra, mats);

        // --- jogador e estações
        Physics.SyncTransforms();
        PosicionarJogoNaBase(relatorio);

        ctrl.AplicarImediato(PresetLuz.Dia);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        File.WriteAllText("Temp/relatorio_montar_base.txt", string.Join("\n", relatorio));
        Debug.Log("[MontarBase] OK:\n" + string.Join("\n", relatorio));
    }

    static void DesligarTeste(string nome) { var g = GameObject.Find(nome); if (g != null) g.SetActive(false); }
    static GameObject Filho(GameObject pai, string nome) { var g = new GameObject(nome); g.transform.SetParent(pai.transform, false); return g; }

    // Só o que o personagem pisa/encosta: pisos, paredes e estruturas. Decoração e emissivos ficam sem colisor.
    static readonly string[] ComColisor =
    {
        "Base_Piso", "Base_Solo lunar", "Base_Painel branco", "Base_Painel escuro", "Base_Estrutura", "Base_Metal escuro",
        "Base_Laranja", "Base_Ciano", "Base_Roxo", "Base_Amarelo", "Base_Corredor", "Base_Grade",
    };
    static bool DeveTerColisor(string nome) => ComColisor.Contains(nome);

    // Ponto de partida do prompt: W/100 (pontos) e W/150 (spots). Ajustado olhando as imagens de referência.
    const float DivisorPonto = 35f, DivisorSpot = 60f;

    static void Calibrar(Light l, LuzJ j, LightGroup lg)
    {
        lg.grupo = j.grupo;
        if (j.tipo == "SUN")
        {
            l.type = LightType.Directional;
            l.shadows = j.grupo == "sol" ? LightShadows.Soft : LightShadows.None;
            lg.energiaBase = j.energia_blender_W;
            return;
        }
        var cor = new Color(j.cor_rgb[0], j.cor_rgb[1], j.cor_rgb[2]);
        l.color = new Color(Mathf.LinearToGammaSpace(cor.r), Mathf.LinearToGammaSpace(cor.g), Mathf.LinearToGammaSpace(cor.b));
        if (j.tipo == "SPOT")
        {
            l.type = LightType.Spot;
            lg.energiaBase = j.energia_blender_W / DivisorSpot;
            l.range = 14f;
            l.spotAngle = j.cone_graus > 0 ? j.cone_graus : 75f;
            l.innerSpotAngle = l.spotAngle * 0.6f;
            l.shadows = LightShadows.Soft;
        }
        else
        {
            l.type = LightType.Point;
            lg.energiaBase = j.energia_blender_W / DivisorPonto;
            l.range = 10f;
            // sombras de ponto são caras: só nas luzes principais
            l.shadows = (j.nome == "Hub cupula") ? LightShadows.Soft : LightShadows.None;
        }
        l.intensity = lg.energiaBase;
        l.lightmapBakeType = LightmapBakeType.Realtime;
    }

    static void CriarTerraNoCeu(GameObject pai, Light terraLuz, Dictionary<string, Material> mats)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "TerraNoCeu.mat");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.1f, 0.3f, 0.7f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.25f, 0.55f, 1f) * 1.4f);
            AssetDatabase.CreateAsset(mat, MatDir + "TerraNoCeu.mat");
        }
        var esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esfera.name = "Terra (no ceu)";
        UnityEngine.Object.DestroyImmediate(esfera.GetComponent<Collider>());
        esfera.transform.SetParent(pai.transform, false);
        // direção: oposta à "Luz da Terra" (que aponta da Terra para a base)
        Vector3 dir = terraLuz != null ? -terraLuz.transform.forward : new Vector3(0.68f, 0.47f, -0.57f);
        esfera.transform.position = dir.normalized * 600f;
        esfera.transform.localScale = Vector3.one * 80f;
        var r = esfera.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
    }

    // Coordenadas Unity (x = -blender x, z = -blender y). Módulos: centros a ~22 m do hub.
    static readonly Dictionary<string, Vector3> Modulos = new Dictionary<string, Vector3>
    {
        { "Estufa", new Vector3(0, 0, -27.1f) }, { "Laboratorio", new Vector3(-19.36f, 0, -11.18f) },
        { "Energia", new Vector3(-19.36f, 0, 11.18f) }, { "Eclusa", new Vector3(0, 0, 22.35f) },
        { "Dormitorio", new Vector3(19.36f, 0, 11.18f) }, { "Agua", new Vector3(19.36f, 0, -11.18f) },
    };

    static Vector3 Slot(string modulo, float radial, float lateral)
    {
        var c = Modulos[modulo]; var u = c.normalized; var t = new Vector3(-u.z, 0, u.x);
        return c + u * radial + t * lateral;
    }

    static void PosicionarJogoNaBase(List<string> relatorio)
    {
        var posicoes = new Dictionary<string, Vector3>
        {
            { "estufa", new Vector3(-3.4f, 0, -31f) },
            { "reciclador", Slot("Agua", 0, 0) },
            { "lixo", Slot("Agua", 3f, 2.5f) },
            { "disjuntores", Slot("Energia", 0, 0) },
            { "filtro", Slot("Energia", -2.5f, -2.5f) },
            { "backup", Slot("Laboratorio", 0, 0) },
            { "esteira", Slot("Laboratorio", 3f, 2.5f) },
            { "cama", Slot("Dormitorio", -1f, 1.5f) },
            { "airlock", new Vector3(0, 0, 25.5f) },
            { "vazamento", new Vector3(3.2f, 0, 3.2f) },
        };
        foreach (var e in UnityEngine.Object.FindObjectsOfType<Estacao>())
        {
            if (posicoes.TryGetValue(e.id, out var p)) e.transform.position = p;
            else if (e.id == "painel") e.transform.position = NoChao(new Vector3(44f, 0, 0));
            else if (e.id == "antena") e.transform.position = NoChao(new Vector3(-36f, 0, 2f));
        }
        var jog = UnityEngine.Object.FindObjectOfType<Movimento>();
        if (jog != null)
        {
            jog.transform.position = new Vector3(0, 1.15f, 3.5f);
            var cc = jog.GetComponent<CharacterController>();
            if (cc != null) { cc.stepOffset = 0.4f; cc.slopeLimit = 50f; }
        }
        relatorio.Add("Jogador no hub (0, 1.15, 3.5); StepOffset 0,4 m (soleira de 0,3 m). Estações redistribuídas pelos módulos.");
    }

    static Vector3 NoChao(Vector3 p)
    {
        if (Physics.Raycast(new Vector3(p.x, 30f, p.z), Vector3.down, out var hit, 60f)) p.y = hit.point.y;
        return p;
    }

    // ---------------- Capturas ----------------
    static readonly (string nome, Vector3 pos, Vector3 alvo, float fov)[] Vistas =
    {
        ("aerea", new Vector3(0, 110, 52), new Vector3(2, 0, 2), 50),
        ("hub", new Vector3(0f, 1.8f, 5.6f), new Vector3(0f, 1.9f, -3f), 75),
        ("laboratorio", new Vector3(-15.6f, 2.1f, -9.4f), new Vector3(-22f, 1.2f, -13f), 70),
        ("estufa", new Vector3(0, 2.0f, -17.2f), new Vector3(0, 1.4f, -32f), 70),
        ("eclusa_externa", new Vector3(0, 2.6f, 40f), new Vector3(0, 2f, 28.3f), 65),
    };

    [MenuItem("Tools/Base Lunar/Capturar vistas (Dia, Erupcao, Noite)")]
    public static void Capturar()
    {
        var ctrl = UnityEngine.Object.FindObjectOfType<BaseLightingController>();
        if (ctrl == null) { Debug.LogError("[MontarBase] BaseLightingController não achado."); return; }
        string pasta = "Docs/capturas-base";
        Directory.CreateDirectory(pasta);
        // esconde jogador e estações (não fazem parte do cenário de referência)
        var escondidos = new List<GameObject>();
        foreach (var n in new[] { "Player", "Estacoes" }) { var o = GameObject.Find(n); if (o != null && o.activeSelf) { o.SetActive(false); escondidos.Add(o); } }
        var go = new GameObject("CapCam"); var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox; cam.allowHDR = true; cam.nearClipPlane = 0.1f; cam.farClipPlane = 1500f;
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;
        foreach (PresetLuz p in Enum.GetValues(typeof(PresetLuz)))
        {
            ctrl.AplicarImediato(p);
            foreach (var v in Vistas)
            {
                cam.transform.position = v.pos; cam.transform.LookAt(v.alvo); cam.fieldOfView = v.fov;
                cam.Render();
                var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
                File.WriteAllBytes($"{pasta}/{p.ToString().ToLower()}_{v.nome}.png", tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
        ctrl.AplicarImediato(PresetLuz.Dia);
        foreach (var o in escondidos) o.SetActive(true);
        cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
        Debug.Log("[MontarBase] capturas em " + pasta);
    }
}
