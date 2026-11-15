using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Põe o astronauta novo (Assets/Models/Personagens/AstronautaNovo/astronauta.fbx, com os materiais da pasta) no jogador.
/// Usa o mesmo controller humanoide dos outros astronautas (Idle/Caminhar/Correr). O astronauta anterior fica desativado
/// como "Astronauta_anterior" dentro do _Player (nada é apagado).
/// Menu: Tools/Personagens/Aplicar astronauta novo no jogador. Também por Temp/pedido_astro_diag.txt e Temp/pedido_astro_aplicar.txt.
/// </summary>
[InitializeOnLoad]
public static class AstronautaNovo
{
    const string Fbx = "Assets/Models/Personagens/AstronautaNovo/astronauta.fbx";
    const string Prefab = "Assets/Prefabs/Personagens/Astronauta_Novo.prefab";
    const string Controller = "Assets/Animators/Astronauta.controller";
    const float AlturaAlvo = 1.9f; // mesma altura aproximada dos astronautas anteriores

    static double _proxima;
    static AstronautaNovo() { EditorApplication.update += Sondar; }

    static void Sondar()
    {
        if (EditorApplication.timeSinceStartup < _proxima) return;
        _proxima = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("Temp/pedido_astro_diag.txt")) { File.Delete("Temp/pedido_astro_diag.txt"); Diagnostico(); }
        if (File.Exists("Temp/pedido_astro_aplicar.txt")) { File.Delete("Temp/pedido_astro_aplicar.txt"); Aplicar(); }
    }

    static void Diagnostico()
    {
        var sb = new StringBuilder();
        var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
        sb.AppendLine($"importer: tipo={imp.animationType} avatarSetup={imp.avatarSetup} escala={imp.globalScale} useFileScale={imp.useFileScale} importAnim={imp.importAnimation}");
        var tudo = AssetDatabase.LoadAllAssetsAtPath(Fbx);
        var avatar = tudo.OfType<Avatar>().FirstOrDefault();
        sb.AppendLine($"avatar: {(avatar ? avatar.name : "nenhum")} valido={(avatar && avatar.isValid)} humano={(avatar && avatar.isHuman)}");
        sb.AppendLine("clipes: " + string.Join(", ", tudo.OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).Select(c => $"{c.name}({c.length:0.00}s)")));
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
        var inst = (GameObject)Object.Instantiate(go);
        var b = Limites(inst);
        sb.AppendLine($"limites: centro={b.center:F2} tamanho={b.size:F2}  raiz rot={go.transform.localEulerAngles:F0} escala={go.transform.localScale:F2}");
        foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
            sb.AppendLine($"  render {r.GetType().Name} '{r.name}' mats=[{string.Join(", ", r.sharedMaterials.Select(m => m ? m.name + "(" + m.shader.name + ")" : "null"))}]");
        sb.AppendLine("filhos: " + string.Join(", ", inst.GetComponentsInChildren<Transform>(true).Take(40).Select(t => t.name)));
        Object.DestroyImmediate(inst);
        File.WriteAllText("Temp/astro_diag.txt", sb.ToString());
        Debug.Log("[AstronautaNovo] diagnóstico em Temp/astro_diag.txt");
    }

    static Bounds Limites(GameObject g)
    {
        var rs = g.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(g.transform.position, Vector3.zero);
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    [MenuItem("Tools/Personagens/Aplicar astronauta novo no jogador")]
    public static void Aplicar()
    {
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(Controller);
        if (fbx == null || ctrl == null) { Debug.LogError($"[AstronautaNovo] faltando: fbx={(fbx != null)} controller={(ctrl != null)}"); return; }
        var avatar = AssetDatabase.LoadAllAssetsAtPath(Fbx).OfType<Avatar>().FirstOrDefault();

        // prefab: modelo + Animator (controller humanoide compartilhado) + AnimadorPersonagem, altura normalizada
        Directory.CreateDirectory(Path.GetDirectoryName(Prefab));
        var raiz = new GameObject("Astronauta_Novo");
        var modelo = (GameObject)PrefabUtility.InstantiatePrefab(fbx, raiz.transform);
        modelo.name = "Modelo";
        var b = Limites(modelo);
        float escala = b.size.y > 0.01f ? AlturaAlvo / b.size.y : 1f;
        if (Mathf.Abs(escala - 1f) < 0.05f) escala = 1f;
        modelo.transform.localScale = Vector3.one * escala;
        b = Limites(modelo);
        modelo.transform.localPosition = new Vector3(0, -b.min.y, 0); // pés em y=0

        var anim = modelo.GetComponent<Animator>() ?? modelo.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        if (avatar != null) anim.avatar = avatar;
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        var ap = modelo.GetComponent<AnimadorPersonagem>() ?? modelo.AddComponent<AnimadorPersonagem>();
        ap.controller = ctrl;
        foreach (var smr in modelo.GetComponentsInChildren<Renderer>(true))
        {
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            smr.receiveShadows = true;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, Prefab);
        Object.DestroyImmediate(raiz);

        // jogador: desativa o anterior (sem apagar) e põe o novo nos pés do CharacterController
        var cena = EditorSceneManager.GetActiveScene();
        var jogador = Object.FindObjectOfType<Movimento>();
        if (jogador == null) { Debug.LogError("[AstronautaNovo] jogador (Movimento) não encontrado na cena aberta."); return; }
        var novoVelho = jogador.transform.Find("Astronauta_Novo"); if (novoVelho != null) Object.DestroyImmediate(novoVelho.gameObject);
        var anterior = jogador.transform.Find("Astronauta");
        if (anterior != null) { anterior.name = "Astronauta_anterior"; anterior.gameObject.SetActive(false); }
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, jogador.transform);
        inst.name = "Astronauta_Novo";
        var cc = jogador.GetComponent<CharacterController>();
        float baseY = cc != null ? cc.center.y - cc.height / 2f : -1f;
        inst.transform.localPosition = new Vector3(0, baseY, 0);
        inst.transform.localRotation = Quaternion.identity;
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[AstronautaNovo] aplicado no {jogador.name}: escala {escala:0.000}, avatar={(avatar ? avatar.name + (avatar.isHuman ? " (humano)" : " (NÃO humano)") : "nenhum")}, anterior desativado={(anterior != null)}.");
    }
}
