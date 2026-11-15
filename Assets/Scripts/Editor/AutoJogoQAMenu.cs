using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Tools/QA: abre a cena Game, entra no Play e liga o auto jogar (AutoJogoQA). Só existe no Editor.
/// Também por Temp/qa_rodar.txt (conteúdo = pedido, ex.: "rapido" ou "completo;semente=123"), para rodar de fora do Unity.
/// </summary>
[InitializeOnLoad]
public static class AutoJogoQAMenu
{
    const string Cena = "Assets/Scenes/Game.unity";
    const string Gatilho = "Temp/qa_rodar.txt";
    static double _proxima;

    static AutoJogoQAMenu() { EditorApplication.update += Sondar; }

    static void Sondar()
    {
        if (EditorApplication.timeSinceStartup < _proxima) return;
        _proxima = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (!File.Exists(Gatilho)) return;
        var pedido = File.ReadAllText(Gatilho).Trim();
        File.Delete(Gatilho);
        Rodar(pedido == "" ? "rapido" : pedido);
    }

    [MenuItem("Tools/QA/Auto jogar: 3 missões completas (10 dias cada)")]
    static void Completo() => Rodar("completo");

    [MenuItem("Tools/QA/Auto jogar: 1 missão rápida")]
    static void Rapido() => Rodar("rapido");

    [MenuItem("Tools/QA/Auto jogar: 3 missões, sem sair do Play no fim")]
    static void SemSair() => Rodar("completo;parar=0");

    [MenuItem("Tools/QA/Abrir o último relatório")]
    static void AbrirUltimo()
    {
        var ultimo = Path.Combine(Application.dataPath, "..", "Temp", "QA", "ultimo.txt");
        if (!File.Exists(ultimo)) { EditorUtility.DisplayDialog("QA", "Ainda não há relatório. Rode o auto jogar primeiro.", "OK"); return; }
        EditorUtility.RevealInFinder(File.ReadAllText(ultimo).Trim());
    }

    static void Rodar(string pedido)
    {
        if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog("QA", "Saia do Play antes de iniciar o auto jogar.", "OK"); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (EditorSceneManager.GetActiveScene().path != Cena) EditorSceneManager.OpenScene(Cena);
        EditorPrefs.SetString(AutoJogoQA.ChavePedido, pedido);
        EditorApplication.isPlaying = true;
    }
}
