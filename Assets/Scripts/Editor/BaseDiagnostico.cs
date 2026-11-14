using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>Escreve em Temp/base_diagnostico.txt o que há dentro dos FBX da base (nomes, materiais, bounds, luzes, portas).</summary>
[InitializeOnLoad]
public static class BaseDiagnostico
{
    const string Pedido = "Temp/pedido_diagnostico.txt";
    static BaseDiagnostico()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(Pedido)) return;
            File.Delete(Pedido);
            Rodar();
        };
    }

    static void Rodar()
    {
        var sb = new StringBuilder();
        foreach (var caminho in new[] { "Assets/Models/Base/base_cenario.fbx", "Assets/Models/Base/base_portas.fbx" })
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (go == null) { sb.AppendLine("NAO ACHEI " + caminho); continue; }
            sb.AppendLine("=== " + caminho);
            var imp = (ModelImporter)AssetImporter.GetAtPath(caminho);
            sb.AppendLine($"scale {imp.globalScale} useFileScale {imp.useFileScale} lights {imp.importLights} anim {imp.animationType} clips {imp.clipAnimations.Length}/{imp.defaultClipAnimations.Length}");
            var rs = go.GetComponentsInChildren<Renderer>(true);
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            sb.AppendLine($"renderers {rs.Length} bounds center {b.center} size {b.size}");
            sb.AppendLine($"root rot {go.transform.rotation.eulerAngles} scale {go.transform.localScale}");
            var luzes = go.GetComponentsInChildren<Light>(true);
            sb.AppendLine($"luzes {luzes.Length}");
            foreach (var l in luzes.Take(6)) sb.AppendLine($"  luz {l.name} {l.type} I={l.intensity} range={l.range} pos={l.transform.position} rot={l.transform.rotation.eulerAngles}");
            var mats = rs.SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct().OrderBy(n => n);
            sb.AppendLine("materiais: " + string.Join(" | ", mats));
            sb.AppendLine("filhos diretos:");
            foreach (Transform t in go.transform)
            {
                var r = t.GetComponent<Renderer>();
                sb.AppendLine($"  {t.name} pos={t.position} rot={t.rotation.eulerAngles} {(r ? "mat=" + string.Join(",", r.sharedMaterials.Select(m => m ? m.name : "null")) + " size=" + r.bounds.size : "")}");
            }
            var clips = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
            sb.AppendLine($"clips {clips.Length}: " + string.Join(", ", clips.Take(5).Select(c => $"{c.name}({c.length:0.00}s legacy={c.legacy})")));
        }
        File.WriteAllText("Temp/base_diagnostico.txt", sb.ToString());
        Debug.Log("[BaseDiagnostico] escrito Temp/base_diagnostico.txt");
    }
}
