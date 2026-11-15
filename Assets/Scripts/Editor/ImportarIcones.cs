using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ícones da interface (Assets/Resources/Icones, design D5 da mudança interface-e-minijogos): importa como
/// Sprite sem mipmap, 128 px, sem compressão com perda, e escreve Temp/relatorio_icones.txt com o que importou.
/// </summary>
public class ImportarIcones : AssetPostprocessor
{
    const string Pasta = "Assets/Resources/Icones/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Pasta)) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Bilinear;
        ti.maxTextureSize = 128;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
    }

    static void OnPostprocessAllAssets(string[] importados, string[] apagados, string[] movidos, string[] origens)
    {
        var sb = new StringBuilder();
        foreach (var p in importados)
        {
            if (!p.StartsWith(Pasta) || !p.EndsWith(".png")) continue;
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            sb.AppendLine(s != null ? $"OK {Path.GetFileNameWithoutExtension(p)} {s.rect.width}x{s.rect.height}" : $"SEM_SPRITE {p}");
        }
        if (sb.Length > 0) File.AppendAllText("Temp/relatorio_icones.txt", sb.ToString());
    }
}
