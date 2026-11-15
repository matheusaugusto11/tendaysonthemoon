using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

/// <summary>
/// Utilitário de verificação por arquivo (mesmo padrão de MontarBase/MontarPersonagens):
/// Temp/pedido_refresh.txt → AssetDatabase.Refresh();
/// Temp/pedido_testes.txt → roda os testes EditMode e escreve Temp/resultado_testes.txt;
/// erros de compilação vão para Temp/erros_compilacao.txt.
/// </summary>
[InitializeOnLoad]
public static class VerificacaoRemota
{
    static double _proxima;

    static VerificacaoRemota()
    {
        EditorApplication.update += Sondar;
        // Erros e exceções do Console (editor e Play) em Temp/console_erros.txt, para conferir de fora.
        Application.logMessageReceived += (msg, pilha, tipo) =>
        {
            if (tipo == LogType.Error || tipo == LogType.Exception || tipo == LogType.Assert)
                File.AppendAllText("Temp/console_erros.txt", $"[{System.DateTime.Now:HH:mm:ss}] {tipo}: {msg}\n{pilha}\n");
        };
        CompilationPipeline.assemblyCompilationFinished += (asm, msgs) =>
        {
            var sb = new StringBuilder();
            foreach (var m in msgs)
                if (m.type == CompilerMessageType.Error) sb.AppendLine($"{m.file}({m.line}): {m.message}");
            if (sb.Length > 0) File.AppendAllText("Temp/erros_compilacao.txt", $"== {Path.GetFileName(asm)}\n{sb}");
        };
        CompilationPipeline.compilationStarted += _ => { if (File.Exists("Temp/erros_compilacao.txt")) File.Delete("Temp/erros_compilacao.txt"); };
    }

    static void Sondar()
    {
        if (EditorApplication.timeSinceStartup < _proxima) return;
        _proxima = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("Temp/pedido_refresh.txt")) { File.Delete("Temp/pedido_refresh.txt"); AssetDatabase.Refresh(); return; }
        if (File.Exists("Temp/pedido_testes.txt")) { File.Delete("Temp/pedido_testes.txt"); RodarTestes(); }
        if (File.Exists("Temp/pedido_build_webgl.txt")) { File.Delete("Temp/pedido_build_webgl.txt"); BuildWebGL(); }
    }

    /// <summary>Build WebGL das cenas do Build Settings em Builds/WebGL (pasta ignorada pelo git). Resultado em Temp/resultado_build.txt.</summary>
    static void BuildWebGL()
    {
        var cenas = new System.Collections.Generic.List<string>();
        foreach (var c in EditorBuildSettings.scenes) if (c.enabled) cenas.Add(c.path);
        var opcoes = new BuildPlayerOptions
        {
            scenes = cenas.ToArray(),
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };
        var sb = new StringBuilder();
        try
        {
            var rel = UnityEditor.BuildPipeline.BuildPlayer(opcoes);
            var s = rel.summary;
            sb.AppendLine($"resultado={s.result} erros={s.totalErrors} avisos={s.totalWarnings} tamanho={s.totalSize / (1024f * 1024f):0.0} MB tempo={s.totalTime.TotalMinutes:0.0} min");
            foreach (var passo in rel.steps)
                foreach (var m in passo.messages)
                    if (m.type == LogType.Error || m.type == LogType.Exception) sb.AppendLine("ERRO " + m.content);
        }
        catch (System.Exception e) { sb.AppendLine("EXCECAO " + e); }
        File.WriteAllText("Temp/resultado_build.txt", sb.ToString());
    }

    static void RodarTestes()
    {
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new Coletor());
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
    }

    class Coletor : ICallbacks
    {
        readonly StringBuilder _sb = new StringBuilder();
        int _ok, _falhas;
        public void RunStarted(ITestAdaptor testsToRun) { _sb.Clear(); _ok = _falhas = 0; }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor r)
        {
            if (r.HasChildren) return;
            if (r.TestStatus == TestStatus.Passed) _ok++;
            else if (r.TestStatus == TestStatus.Failed) { _falhas++; _sb.AppendLine($"FALHOU {r.FullName}\n   {r.Message}"); }
        }
        public void RunFinished(ITestResultAdaptor r)
        {
            _sb.AppendLine($"{_ok} passaram, {_falhas} falharam ({r.Duration:0.0}s)");
            File.WriteAllText("Temp/resultado_testes.txt", _sb.ToString());
        }
    }
}
