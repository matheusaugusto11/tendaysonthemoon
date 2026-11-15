using System.Runtime.InteropServices;
using DezDias;
using UnityEngine;

/// <summary>Ponte com o navegador na build WebGL (Plugins/WebGL/DezDias.jslib). No editor usa os equivalentes do Unity.</summary>
public static class Navegador
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int DD_PrefereMenosMovimento();
    [DllImport("__Internal")] static extern int DD_LinguaNavegadorPt();
    [DllImport("__Internal")] static extern int DD_TelaDeToque();
    [DllImport("__Internal")] static extern void DD_AbrirLinkNoProximoClique(string url);
#endif

    /// <summary>O navegador pede movimento reduzido (prefers-reduced-motion).</summary>
    public static bool PrefereMenosMovimento
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { return DD_PrefereMenosMovimento() == 1; } catch { return false; }
#else
            return false;
#endif
        }
    }

    /// <summary>Na WebGL, a tela principal é de toque e não há mouse (pega o iPad, que se apresenta como Mac).</summary>
    public static bool TelaDeToque
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { return DD_TelaDeToque() == 1; } catch { return false; }
#else
            return false;
#endif
        }
    }

    /// <summary>Idioma inicial: o do navegador na WebGL; senão o do sistema.</summary>
    public static Lingua LinguaInicial()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            int pt = DD_LinguaNavegadorPt();
            if (pt >= 0) return pt == 1 ? Lingua.Pt : Lingua.En;
        }
        catch { }
#endif
        return Application.systemLanguage == SystemLanguage.Portuguese ? Lingua.Pt : Lingua.En;
    }

    /// <summary>Endereço da página tem "minigame" (abre o menu de teste dos minijogos).</summary>
    public static bool PediuMenuMinijogos()
    {
        var url = Application.absoluteURL ?? "";
        return url.ToLowerInvariant().Contains("minigame");
    }

    /// <summary>Prepara a abertura do link no pointer down; na WebGL ele abre no soltar do clique (gesto do usuário).</summary>
    public static void PrepararLink(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DD_AbrirLinkNoProximoClique(url);
#endif
    }

    /// <summary>Abertura direta (editor e outras plataformas). Na WebGL o PrepararLink já cuidou.</summary>
    public static void AbrirLinkDireto(string url)
    {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        Application.OpenURL(url);
#endif
    }
}
