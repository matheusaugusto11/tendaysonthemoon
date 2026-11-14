using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Depois que o pacote Input System estiver no manifest, muda "Active Input Handling" para "Both"
/// (teclado/mouse antigos + controle novo) e oferece reiniciar o Editor para valer.
/// </summary>
[InitializeOnLoad]
public static class ConfigurarInputSystem
{
    static ConfigurarInputSystem()
    {
        EditorApplication.delayCall += Verificar;
    }

    static void Verificar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        string manifest = File.ReadAllText("Packages/manifest.json");
        if (!manifest.Contains("com.unity.inputsystem")) return;

        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop == null || prop.intValue == 2) return;

        prop.intValue = 2; // 2 = Both
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("[ConfigurarInputSystem] Active Input Handling = Both. Reinicie o Unity para ativar o controle.");

        if (EditorUtility.DisplayDialog("Controle de Xbox",
            "O Input System foi ativado (Active Input Handling = Both).\nÉ preciso reiniciar o Unity para o controle funcionar.\n\nReiniciar agora? (a cena é salva antes)",
            "Reiniciar agora", "Depois"))
        {
            EditorSceneManager.SaveOpenScenes();
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }
    }
}
