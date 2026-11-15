using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Só diagnóstico do Editor: fixa a resolução da janela Game (ex.: 1920x1080), para que as capturas do Play saiam no
/// tamanho do referencial da interface. Pelo menu "Dez Dias/Game view 1920x1080" ou pelo arquivo Temp/pedido_resolucao.txt
/// (conteúdo "LARGURAxALTURA"). Usa a API interna da Unity por reflexão; se ela mudar, só avisa no Console.
/// </summary>
public static class ResolucaoGameView
{
    [MenuItem("Dez Dias/Game view 1920x1080")]
    public static void Menu() => Definir(1920, 1080);

    public static string Definir(int w, int h)
    {
        try
        {
            var asm = typeof(Editor).Assembly;
            var tSizes = asm.GetType("UnityEditor.GameViewSizes");
            var tSingleton = typeof(ScriptableSingleton<>).MakeGenericType(tSizes);
            var instancia = tSingleton.GetProperty("instance").GetValue(null);
            var grupo = tSizes.GetProperty("currentGroup").GetValue(instancia);
            var tGrupo = grupo.GetType();
            var tTamanho = asm.GetType("UnityEditor.GameViewSize");
            var tTipo = asm.GetType("UnityEditor.GameViewSizeType");
            string nome = $"{w}x{h} (diagnóstico)";

            int total = (int)tGrupo.GetMethod("GetTotalCount").Invoke(grupo, null);
            int indice = -1;
            var getTam = tGrupo.GetMethod("GetGameViewSize");
            for (int i = 0; i < total; i++)
            {
                var t = getTam.Invoke(grupo, new object[] { i });
                string n = (string)tTamanho.GetProperty("baseText").GetValue(t);
                if (n == nome) { indice = i; break; }
            }
            if (indice < 0)
            {
                var ctor = tTamanho.GetConstructor(new[] { tTipo, typeof(int), typeof(int), typeof(string) });
                var novo = ctor.Invoke(new object[] { Enum.Parse(tTipo, "FixedResolution"), w, h, nome });
                tGrupo.GetMethod("AddCustomSize").Invoke(grupo, new[] { novo });
                indice = (int)tGrupo.GetMethod("GetTotalCount").Invoke(grupo, null) - 1;
            }

            var tGameView = asm.GetType("UnityEditor.GameView");
            var janela = EditorWindow.GetWindow(tGameView, false, null, false);
            var sel = tGameView.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            sel.Invoke(janela, new object[] { indice, null });
            janela.Repaint();
            return $"Game view fixada em {w}x{h} (índice {indice}).";
        }
        catch (Exception e)
        {
            return "FALHOU: " + e.GetType().Name + ": " + e.Message;
        }
    }

    /// <summary>Chamado pelo VerificacaoRemota: lê o arquivo, aplica e grava o resultado em Temp/resolucao_resultado.txt.</summary>
    public static void Pedido(string arquivo)
    {
        string conteudo = File.ReadAllText(arquivo).Trim();
        File.Delete(arquivo);
        var partes = conteudo.ToLowerInvariant().Split('x');
        string r = partes.Length == 2 && int.TryParse(partes[0], out int w) && int.TryParse(partes[1], out int h)
            ? Definir(w, h) : "FALHOU: formato esperado LARGURAxALTURA, veio \"" + conteudo + "\"";
        File.WriteAllText("Temp/resolucao_resultado.txt", r);
    }
}
