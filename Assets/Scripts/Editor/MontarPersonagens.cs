using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// Importa os personagens (Docs/PROMPT-UNITY-PERSONAGENS.md): materiais URP, rig Humanoid/Generic, clipes em loop,
/// controllers, prefabs, cena de validação e o astronauta no jogador da cena Game.
/// Menu: Tools/Personagens. Também roda ao detectar Temp/pedido_personagens.txt.
/// </summary>
[InitializeOnLoad]
public static class MontarPersonagens
{
    const string Dir = "Assets/Models/Personagens/";
    const string MatDir = "Assets/Materials/Personagens";
    const string PrefabDir = "Assets/Prefabs/Personagens";
    const string AnimDir = "Assets/Animators";
    static readonly string[] Astronautas = { "Astronauta_Engenharia", "Astronauta_Botanica", "Astronauta_Medicina", "Astronauta_Ciencia" };
    const string Robo = "Robo_Assistente";

    static double _proxima;
    static MontarPersonagens() { EditorApplication.update += Sondar; }
    static void Sondar()
    {
        if (EditorApplication.timeSinceStartup < _proxima) return;
        _proxima = EditorApplication.timeSinceStartup + 1.0;
        File.WriteAllText("Temp/estado_editor.txt", $"isPlaying={EditorApplication.isPlaying} will={EditorApplication.isPlayingOrWillChangePlaymode} compiling={EditorApplication.isCompiling} t={EditorApplication.timeSinceStartup:0}");
        // utilitários de teste do jogo: entrar/sair do Play por arquivo
        if (EditorApplication.isPlaying && File.Exists("Temp/pedido_parar_play.txt")) { File.Delete("Temp/pedido_parar_play.txt"); EditorApplication.isPlaying = false; return; }
        if (!EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating && File.Exists("Temp/pedido_play_jogo.txt"))
        {
            File.Delete("Temp/pedido_play_jogo.txt");
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Game.unity") EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true; return;
        }
        // captura dentro do Play: encerra o Play quando o relatório aparecer e reabre a cena do jogo
        // (só quando a captura foi pedida por mim: o flag existe; um Play seu nunca é interrompido)
        if (EditorApplication.isPlaying && File.Exists("Temp/captura_play.flag") && File.Exists("Temp/captura_play_pronta.txt"))
        { File.Delete("Temp/captura_play.flag"); File.Delete("Temp/captura_play_pronta.txt"); EditorApplication.isPlaying = false; return; }
        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && SessionState.GetBool("reabrirGame", false))
        {
            SessionState.SetBool("reabrirGame", false);
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
        }
        if (!EditorApplication.isPlaying && File.Exists("Temp/pedido_play_cap.txt") && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            File.Delete("Temp/pedido_play_cap.txt");
            Directory.CreateDirectory("Docs/capturas-personagens");
            foreach (var f in Directory.GetFiles("Docs/capturas-personagens", "*.png")) File.Delete(f);
            File.Delete("Temp/captura_play_pronta.txt");
            File.WriteAllText("Temp/captura_play.flag", "1");
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/PersonagensTeste.unity", OpenSceneMode.Single);
            SessionState.SetBool("reabrirGame", true);
            EditorApplication.isPlaying = true;
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("Temp/pedido_personagens.txt")) { File.Delete("Temp/pedido_personagens.txt"); Montar(); }
        if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists("Temp/pedido_seta_cena.txt")) { File.Delete("Temp/pedido_seta_cena.txt"); ColocarSetaNaCena(); }
        if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists("Temp/pedido_cap_seta.txt")) { File.Delete("Temp/pedido_cap_seta.txt"); CapturarSeta(); }
        if (File.Exists("Temp/pedido_diag_personagens.txt")) { File.Delete("Temp/pedido_diag_personagens.txt"); DiagnosticoClipes(); }
        if (File.Exists("Temp/pedido_cap_personagens.txt")) { File.Delete("Temp/pedido_cap_personagens.txt"); CapturarPersonagens(); }
    }

    // ---------------- JSON ----------------
    [Serializable] class MatJ { public string nome; public float[] cor_base, emissao_cor; public float metalico, rugosidade, emissao_intensidade; }
    [Serializable] class OssoJ { public string osso, pai, humanoid; }
    [Serializable] class PersJ { public string nome, arquivo, tipo; public float altura_m; public OssoJ[] ossos; public MatJ[] materiais; }
    [Serializable] class PersonagensJ { public PersJ[] personagens; }

    static PersonagensJ Dados() => JsonUtility.FromJson<PersonagensJ>(File.ReadAllText(Dir + "personagens.json"));

    // ---------------- Materiais ----------------
    static Dictionary<string, Material> CriarMateriais(PersonagensJ d)
    {
        Directory.CreateDirectory(MatDir);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var res = new Dictionary<string, Material>();
        foreach (var m in d.personagens.SelectMany(p => p.materiais))
        {
            if (res.ContainsKey(m.nome)) continue; // traje compartilhado: um só material
            string caminho = $"{MatDir}/{m.nome}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            bool novo = mat == null;
            if (novo) mat = new Material(shader);
            mat.shader = shader;
            mat.SetColor("_BaseColor", new Color(Mathf.LinearToGammaSpace(m.cor_base[0]), Mathf.LinearToGammaSpace(m.cor_base[1]), Mathf.LinearToGammaSpace(m.cor_base[2])));
            mat.SetFloat("_Metallic", m.metalico);
            mat.SetFloat("_Smoothness", 1f - m.rugosidade);
            if (m.emissao_intensidade > 0f && m.emissao_cor != null && m.emissao_cor.Length == 3)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(m.emissao_cor[0], m.emissao_cor[1], m.emissao_cor[2]) * m.emissao_intensidade);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else { mat.DisableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.black); }
            if (novo) AssetDatabase.CreateAsset(mat, caminho); else EditorUtility.SetDirty(mat);
            res[m.nome] = mat;
        }
        AssetDatabase.SaveAssets();
        return res;
    }

    // ---------------- Importação ----------------
    static string NomeHumano(string h) => string.Concat(h.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

    static string ConfigurarFbx(string nome, bool humanoid, Dictionary<string, Material> mats, PersJ pj)
    {
        string caminho = Dir + nome + ".fbx";
        var imp = (ModelImporter)AssetImporter.GetAtPath(caminho);
        imp.globalScale = 1f; imp.useFileScale = true;
        imp.importCameras = false; imp.importLights = false;
        imp.animationType = humanoid ? ModelImporterAnimationType.Human : ModelImporterAnimationType.Generic;
        imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        imp.importAnimation = true;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Material>())
            if (mats.TryGetValue(obj.name, out var novo)) imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), obj.name), novo);
        imp.SaveAndReimport();

        // clipes: nomes originais, Loop Time, sem root motion (bake into pose)
        var clips = imp.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.loopTime = true; c.loopPose = true;
            c.lockRootRotation = true; c.lockRootHeightY = true; c.lockRootPositionXZ = true;
            c.keepOriginalOrientation = true; c.keepOriginalPositionY = true; c.keepOriginalPositionXZ = true;
        }
        imp.clipAnimations = clips;
        imp.SaveAndReimport();

        var info = AssetDatabase.LoadAllAssetsAtPath(caminho);
        var avatar = info.OfType<Avatar>().FirstOrDefault();
        string clipsTxt = string.Join(", ", info.OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).Select(c => $"{c.name}({c.length:0.00}s)"));
        string status = $"{nome}: {(humanoid ? "Humanoid" : "Generic")} avatarValido={(avatar != null && avatar.isValid)} humano={(avatar != null && avatar.isHuman)} clipes=[{clipsTxt}]";
        if (humanoid && (avatar == null || !avatar.isHuman)) status += "  ** AUTO-MAPEAMENTO FALHOU **";
        return status;
    }

    // ---------------- Controllers ----------------
    static AnimatorController CriarController(string nome, string clipParado, string clipAndar, string clipCorrer, string fbxCaminho, float passada1, float passada2)
    {
        Directory.CreateDirectory(AnimDir);
        string caminho = $"{AnimDir}/{nome}.controller";
        AssetDatabase.DeleteAsset(caminho);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(caminho);
        ctrl.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
        // clipes extraídos para .anim próprios: o controller não depende de nenhum FBX (evita referência circular com os prefabs)
        Directory.CreateDirectory(AnimDir + "/Clipes");
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var origem in AssetDatabase.LoadAllAssetsAtPath(fbxCaminho).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")))
        {
            string destino = $"{AnimDir}/Clipes/{nome}_{origem.name}.anim";
            AssetDatabase.DeleteAsset(destino);
            var copia = UnityEngine.Object.Instantiate(origem); copia.name = origem.name;
            AssetDatabase.CreateAsset(copia, destino);
            clips[origem.name] = copia;
        }

        var arvore = new BlendTree { name = "Locomocao", blendType = BlendTreeType.Simple1D, blendParameter = "Velocidade", useAutomaticThresholds = false };
        arvore.AddChild(clips[clipParado], 0f);
        arvore.AddChild(clips[clipAndar], passada1);   // referência de passada: caminhar
        arvore.AddChild(clips[clipCorrer], passada2);  // referência de passada: correr
        AssetDatabase.AddObjectToAsset(arvore, ctrl);

        var sm = ctrl.layers[0].stateMachine;
        var estado = sm.AddState("Locomocao");
        estado.motion = arvore;
        sm.defaultState = estado;
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        return ctrl;
    }

    // ---------------- Prefabs ----------------
    static GameObject CriarPrefab(string nome, bool robo, RuntimeAnimatorController ctrl, PersJ pj)
    {
        Directory.CreateDirectory(PrefabDir);
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(Dir + nome + ".fbx");
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        inst.name = nome;
        var anim = inst.GetComponent<Animator>() ?? inst.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion = false;
        var cap = inst.AddComponent<CapsuleCollider>();
        float h = robo ? 1.3f : 1.9f, r = robo ? 0.35f : 0.3f;
        cap.height = h; cap.radius = r; cap.center = new Vector3(0, h / 2f, 0);
        inst.AddComponent<AnimadorPersonagem>().controller = ctrl;
        foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            smr.receiveShadows = true;
            smr.updateWhenOffscreen = false;
        }
        string caminho = $"{PrefabDir}/{nome}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(inst, caminho);
        UnityEngine.Object.DestroyImmediate(inst);
        return prefab;
    }

    // ---------------- Principal ----------------
    [MenuItem("Tools/Personagens/Importar e configurar")]
    public static void Montar()
    {
        var rel = new List<string>();
        var d = Dados();
        var mats = CriarMateriais(d);
        rel.Add($"Materiais URP/Lit: {mats.Count} em {MatDir} (traje compartilhado; só 'Acento <cor>' difere).");

        foreach (var p in d.personagens)
        {
            bool humano = p.tipo == "Astronauta";
            rel.Add(ConfigurarFbx(Path.GetFileNameWithoutExtension(p.arquivo), humano, mats, p));
        }

        // controllers: recria e RECARREGA do disco (a referência em memória fica obsoleta após o DeleteAsset/Create)
        CriarController("Astronauta", "Idle", "Caminhar", "Correr", Dir + Astronautas[0] + ".fbx", 1.5f, 4.5f);
        CriarController("Robo", "Idle", "Rodar", "Acelerar", Dir + Robo + ".fbx", 1.5f, 4.5f);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        var ctrlAstro = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimDir + "/Astronauta.controller");
        var ctrlRobo = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimDir + "/Robo.controller");
        if (ctrlAstro == null || ctrlRobo == null) { Debug.LogError("[MontarPersonagens] controller não carregou do disco."); return; }
        rel.Add("Controllers: Assets/Animators/Astronauta.controller (compartilhado pelos 4) e Robo.controller. Blend tree 1D 'Velocidade': 0=Idle, 1,5=Caminhar/Rodar, 4,5=Correr/Acelerar.");

        var prefabs = new Dictionary<string, GameObject>();
        foreach (var p in d.personagens)
        {
            string n = Path.GetFileNameWithoutExtension(p.arquivo);
            prefabs[n] = CriarPrefab(n, n == Robo, n == Robo ? ctrlRobo : ctrlAstro, p);
        }
        rel.Add($"Prefabs: {prefabs.Count} em {PrefabDir} (Animator, CapsuleCollider, sombras ligadas, AnimadorPersonagem).");

        CriarCenaTeste(prefabs);
        rel.Add("Cena de validação: Assets/Scenes/PersonagensTeste.unity (5 personagens, WASD/Shift movem todos).");

        ColocarNoJogo(prefabs[Astronautas[0]], rel);

        File.WriteAllText("Temp/relatorio_personagens.txt", string.Join("\n", rel));
        Debug.Log("[MontarPersonagens] OK:\n" + string.Join("\n", rel));
    }

    // ---------------- Cena de validação ----------------
    static void CriarCenaTeste(Dictionary<string, GameObject> prefabs)
    {
        string caminho = "Assets/Scenes/PersonagensTeste.unity";
        var anterior = SceneManager.GetActiveScene();
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(cena);

        var chao = GameObject.CreatePrimitive(PrimitiveType.Plane); chao.name = "Chao"; chao.transform.localScale = new Vector3(3, 1, 2);
        var luz = new GameObject("Luz direcional"); var l = luz.AddComponent<Light>();
        l.type = LightType.Directional; l.intensity = 1.2f; l.shadows = LightShadows.Soft;
        luz.transform.rotation = Quaternion.Euler(45, 160, 0); // vem da frente (+Z), como a câmera
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.52f);
        var camGo = new GameObject("Camera"); camGo.tag = "MainCamera"; var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(5.5f, 3.0f, 8f); camGo.transform.LookAt(new Vector3(0, 1.1f, 0f)); // 3/4 pela frente (+Z)

        var ordem = Astronautas.Concat(new[] { Robo }).ToArray();
        for (int i = 0; i < ordem.Length; i++)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[ordem[i]]);
            inst.transform.position = new Vector3((i - 2) * 2.2f, 0, 0);
            inst.AddComponent<PersonagemTeste>();
        }
        EditorSceneManager.SaveScene(cena, caminho);
        SceneManager.SetActiveScene(anterior);
        EditorSceneManager.CloseScene(cena, true);
    }

    // ---------------- No jogo ----------------
    static void ColocarNoJogo(GameObject astronautaPrefab, List<string> rel)
    {
        var jogador = UnityEngine.Object.FindObjectOfType<Movimento>();
        if (jogador == null) { rel.Add("Jogador (Movimento) não encontrado na cena ativa; astronauta não foi colocado."); return; }
        var cena = jogador.gameObject.scene;
        // remove astronauta anterior, se houver
        var antigo = jogador.transform.Find("Astronauta"); if (antigo != null) UnityEngine.Object.DestroyImmediate(antigo.gameObject);

        // esconde a cápsula de teste (não apaga)
        var mr = jogador.GetComponent<MeshRenderer>(); if (mr != null) mr.enabled = false;
        var frente = jogador.transform.Find("Frente"); if (frente != null) frente.gameObject.SetActive(false);

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(astronautaPrefab, jogador.transform);
        inst.name = "Astronauta";
        var cc = jogador.GetComponent<CharacterController>();
        float baseY = cc != null ? cc.center.y - cc.height / 2f : -1f;     // pés na base do CharacterController
        inst.transform.localPosition = new Vector3(0, baseY, 0);
        inst.transform.localRotation = Quaternion.identity;
        var capsula = inst.GetComponent<CapsuleCollider>(); if (capsula != null) UnityEngine.Object.DestroyImmediate(capsula); // o jogador já tem CharacterController

        // velocidades compatíveis com as animações (passadas de 1,5 e 4,5 m/s)
        jogador.velocidade = 2.0f; jogador.velocidadeCorrida = 4.5f;
        EditorUtility.SetDirty(jogador);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        rel.Add($"Jogo: astronauta de Engenharia colocado no _Player (pés em y={baseY:0.00} do CharacterController); cápsula de teste desligada. Velocidades do Movimento: andar 2,0 e correr 4,5 m/s.");
    }

    /// <summary>Põe o componente SetaGuia3D direto no _Player (campos editáveis no Inspector dele) e monta o desenho num filho.</summary>
    [MenuItem("Tools/Personagens/Colocar seta guia no jogador")]
    public static void ColocarSetaNaCena()
    {
        var jog = UnityEngine.Object.FindObjectOfType<Movimento>();
        if (jog == null) { Debug.LogError("[ColocarSeta] jogador (Movimento) não encontrado na cena ativa."); return; }

        // remove versões antigas: componente solto em outros objetos (inclusive o filho "SetaGuia3D" da versão anterior)
        foreach (var velha in UnityEngine.Object.FindObjectsOfType<SetaGuia3D>(true))
            if (velha.gameObject != jog.gameObject) UnityEngine.Object.DestroyImmediate(velha.gameObject);

        var seta = jog.GetComponent<SetaGuia3D>();
        if (seta == null) seta = Undo.AddComponent<SetaGuia3D>(jog.gameObject);
        seta.Montar(); // refaz o desenho com o raio atual (filho "SetaGuia3D" do _Player)
        EditorUtility.SetDirty(seta);
        EditorSceneManager.MarkSceneDirty(jog.gameObject.scene);
        EditorSceneManager.SaveScene(jog.gameObject.scene);
        Debug.Log("[ColocarSeta] Componente SetaGuia3D no objeto " + jog.name + " (alturaDosPes e raio no Inspector do " + jog.name + ").");
    }

    /// <summary>Renderiza a seta guia 3D em volta do jogador (sem precisar do Play) em Docs/capturas-seta.</summary>
    static void CapturarSeta()
    {
        string pasta = "Docs/capturas-seta"; Directory.CreateDirectory(pasta);
        var jog = UnityEngine.Object.FindObjectOfType<Movimento>();
        if (jog == null) { Debug.LogError("[CapturarSeta] jogador não achado"); return; }
        // seta TEMPORÁRIA: a da cena (filha do jogador) não é tocada
        var goSeta = new GameObject("SetaGuia3D_captura");
        var seta = goSeta.AddComponent<SetaGuia3D>();
        seta.Montar(); seta.PrepararParaCaptura();
        bool jaExistia = false;
        var go = new GameObject("CapCamSeta"); var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox; cam.nearClipPlane = 0.05f; cam.farClipPlane = 500f; cam.fieldOfView = 55f;
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;
        try
        {
            var p = jog.transform.position;
            var destino = new Vector3(p.x + 6f, 0, p.z + 12f);
            foreach (var (nome, cor) in new[] { ("padrao", new Color(1f, 0.82f, 0.2f)), ("ajustada", new Color(0.3f, 0.9f, 1f)) })
            {
                if (nome == "ajustada")
                {   // valores bem diferentes, só para provar que os ajustes do Inspector valem ao vivo
                    seta.tamanhoSeta = 1.9f; seta.espessuraSeta = 2.5f; seta.raio = 2.3f;
                    seta.raioAro = 2.3f; seta.quantidadeMarcas = 44; seta.larguraMarca = 0.2f; seta.comprimentoMarca = 0.22f;
                    seta.brilhoAro = 3f; seta.alturaDosPes = 0.25f;
                }
                seta.Atualizar(jog.transform, destino, cor);
                var dirD = new Vector3(destino.x - p.x, 0, destino.z - p.z).normalized;
                var lado = new Vector3(dirD.z, 0, -dirD.x);
                foreach (var (vista, pos) in new[] { ("de-frente", p + dirD * 3.8f + Vector3.up * 2.3f), ("de-lado", p + lado * 3.4f - dirD * 0.8f + Vector3.up * 2.2f) })
                {
                    cam.transform.position = pos; cam.transform.LookAt(p + Vector3.down * 0.5f);
                    cam.Render();
                    var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                    RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
                    File.WriteAllBytes($"{pasta}/seta_{nome}_{vista}.png", tex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }
        finally
        {
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(go);
            if (seta != null && !jaExistia) UnityEngine.Object.DestroyImmediate(seta.gameObject);
            else if (seta != null) seta.Esconder();
        }
    }

    static void DiagnosticoClipes()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var nome in new[] { "Astronauta_Engenharia", "Robo_Assistente" })
        {
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(Dir + nome + ".fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")))
            {
                var b = AnimationUtility.GetCurveBindings(clip);
                sb.AppendLine($"{nome}/{clip.name}: len={clip.length:0.00} humano={clip.isHumanMotion} legacy={clip.legacy} curvas={b.Length}");
                foreach (var bind in b.Where(x => x.propertyName.Contains("Leg") || x.propertyName.Contains("Upper") || x.path.Contains("UpperLeg") || x.propertyName.Contains("Front")).Take(14))
                {
                    var c = AnimationUtility.GetEditorCurve(clip, bind);
                    float mn = float.MaxValue, mx = float.MinValue; foreach (var k in c.keys) { mn = Mathf.Min(mn, k.value); mx = Mathf.Max(mx, k.value); }
                    sb.AppendLine($"    {bind.path}|{bind.propertyName}: keys={c.length} min={mn:0.000} max={mx:0.000}");
                }
            }
        }
        var ctrlAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimDir + "/Astronauta.controller");
        sb.AppendLine($"CONTROLLER asset: {(ctrlAsset != null ? ctrlAsset.name : "NULL (nao carrega)")} clips={(ctrlAsset != null ? ctrlAsset.animationClips.Length : -1)}");
        foreach (var n in Astronautas.Concat(new[] { Robo }))
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{n}.prefab");
            var an = pf != null ? pf.GetComponent<Animator>() : null;
            sb.AppendLine($"PREFAB {n}: animator={(an != null)} controller={(an != null && an.runtimeAnimatorController != null ? an.runtimeAnimatorController.name : "NULL")} avatar={(an != null && an.avatar != null ? an.avatar.name : "null")}");
        }
        File.WriteAllText("Temp/diag_personagens.txt", sb.ToString());
    }

    // ---------------- Capturas ----------------
    [MenuItem("Tools/Personagens/Capturar personagens parados e andando")]
    public static void CapturarPersonagens()
    {
        string pasta = "Docs/capturas-personagens"; Directory.CreateDirectory(pasta);
        var anterior = SceneManager.GetActiveScene();
        // esconde a cena do jogo durante a captura (restaura no finally)
        var ocultos = anterior.GetRootGameObjects().Where(g => g.activeSelf).ToList();
        foreach (var g in ocultos) g.SetActive(false);
        Scene cena = default;
        try {
        cena = EditorSceneManager.OpenScene("Assets/Scenes/PersonagensTeste.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(cena);
        var cam = GameObject.Find("Camera").GetComponent<Camera>();
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;
        var ptes = UnityEngine.Object.FindObjectsOfType<PersonagemTeste>();
        var vistas = new[] { ("frente_3-4", new Vector3(5f, 2.6f, 7.5f)), ("frente", new Vector3(0f, 1.6f, 8.5f)), ("costas", new Vector3(0f, 1.6f, -7.5f)) };
        // amostra os clipes direto (o Animator não avalia fora do Play): (estado, clipe astronauta, clipe robô, tempo)
        var estados = new[] { ("idle", "Idle", "Idle", 0.8f), ("caminhar", "Caminhar", "Rodar", 0.25f), ("correr", "Correr", "Acelerar", 0.17f) };
        foreach (var (estado, clipeAstro, clipeRobo, t) in estados)
        {
            var grafos = new List<PlayableGraph>();
            try
            {
                foreach (var an in UnityEngine.Object.FindObjectsOfType<AnimadorPersonagem>())
                {
                    bool robo = an.name.StartsWith("Robo");
                    var clip = AssetDatabase.LoadAllAssetsAtPath(Dir + an.name + ".fbx").OfType<AnimationClip>().FirstOrDefault(c => c.name == (robo ? clipeRobo : clipeAstro));
                    var animator = an.GetComponent<Animator>();
                    var g = PlayableGraph.Create("Cap_" + an.name);
                    g.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    var saida = AnimationPlayableOutput.Create(g, "Anim", animator);
                    var cp = AnimationClipPlayable.Create(g, clip);
                    cp.SetTime(t); cp.SetSpeed(0);
                    saida.SetSourcePlayable(cp);
                    g.Evaluate(0f);
                    grafos.Add(g);
                }
                foreach (var (nome, pos) in vistas)
                {
                    cam.transform.position = pos; cam.transform.LookAt(new Vector3(0, 1.0f, 0));
                    cam.Render();
                    var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                    RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
                    File.WriteAllBytes($"{pasta}/{estado}_{nome}.png", tex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
            finally { foreach (var g in grafos) if (g.IsValid()) g.Destroy(); }
        }
        cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(rt);
        Debug.Log("[MontarPersonagens] capturas em " + pasta);
        }
        finally
        {
            SceneManager.SetActiveScene(anterior);
            if (cena.IsValid()) EditorSceneManager.CloseScene(cena, true);
            foreach (var g in ocultos) if (g != null) g.SetActive(true);
        }
    }
}
