using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Build WebGL para o itch.io. Menu: Dez Dias > Build WebGL. Linha de comando: -executeMethod BuildWebGL.Executar</summary>
public static class BuildWebGL
{
    const string Saida = "Builds/WebGL";

    [MenuItem("Dez Dias/Build WebGL")]
    public static void Executar()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;   // o itch.io serve sem depender de cabeçalhos
        PlayerSettings.WebGL.decompressionFallback = true;
        var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Game.unity" },
            locationPathName = Saida,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });
        if (r.summary.result == BuildResult.Succeeded) TelaCheia();
        Debug.Log($"Build WebGL: {r.summary.result}, {r.summary.totalSize / 1048576} MB, {r.summary.totalErrors} erros");
        if (Application.isBatchMode) EditorApplication.Exit(r.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    /// <summary>O template padrão fixa o canvas em 960x600 e corta a HUD. Aqui ele passa a ocupar todo o iframe do itch.io.</summary>
    static void TelaCheia()
    {
        var p = Saida + "/index.html";
        var s = System.IO.File.ReadAllText(p);
        if (s.Contains("#unity-footer{display:none}")) return;
        const string css = "<style>html,body{margin:0;height:100%;background:#000;overflow:hidden}#unity-container,#unity-container.unity-desktop{position:absolute;left:0;top:0;width:100%;height:100%;transform:none}#unity-canvas{width:100%!important;height:100%!important;background:#000}#unity-footer{display:none}</style>\n  </head>";
        System.IO.File.WriteAllText(p, s.Replace("</head>", css));
    }
}
