using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Só diagnóstico: com Temp/probe_jogo.flag, percorre o fluxo do jogo no Play e escreve Temp/probe_jogo.txt.</summary>
public class ProbeJogo : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (!File.Exists(Path.Combine(Application.dataPath, "../Temp/probe_jogo.flag"))) return;
        new GameObject("ProbeJogo").AddComponent<ProbeJogo>();
    }

    readonly StringBuilder _sb = new StringBuilder();
    void Log(string s) => _sb.AppendLine($"[{Time.realtimeSinceStartup:0.0}s] {s}");

    void OnEnable() { Application.logMessageReceived += Capturar; }
    void OnDisable() { Application.logMessageReceived -= Capturar; }
    void Capturar(string msg, string stack, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert || t == LogType.Warning)
            _sb.AppendLine($"  >> {t}: {msg}\n     {stack.Split('\n')[0]}");
    }

    string Estado()
    {
        var gm = GameManager.Instancia;
        var jog = FindObjectOfType<Movimento>();
        var cam = Camera.main;
        string s = $"cena={SceneManager.GetActiveScene().name} gm={(gm != null ? gm.Estado.ToString() : "NULL")} dia={(gm != null ? gm.DiaAtual : -1)} timeScale={Time.timeScale}";
        s += $" | jogador={(jog != null ? jog.transform.position.ToString("F1") : "NULL")} ativo={(jog != null && jog.enabled)}";
        s += $" | camera={(cam != null ? cam.transform.position.ToString("F1") : "NULL")} eventSystem={(FindObjectOfType<EventSystem>() != null)}";
        int canvases = 0, ativos = 0; foreach (var c in FindObjectsOfType<Canvas>(true)) { canvases++; if (c.gameObject.activeInHierarchy) ativos++; }
        s += $" | canvases={ativos}/{canvases}";
        return s;
    }

    IEnumerator Start()
    {
        string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        yield return new WaitForSeconds(2f);
        Log("Início: " + Estado());
        var gm = GameManager.Instancia;
        if (gm == null) { Log("GameManager.Instancia é NULL: o jogo não inicia."); Terminar(raiz); yield break; }
        gm.Jogar(); yield return new WaitForSeconds(0.5f); Log("após Jogar(): " + Estado());
        gm.ComecarJogo(); yield return new WaitForSeconds(0.5f); Log("após ComecarJogo(): " + Estado());
        gm.ComecarAcao(); yield return new WaitForSeconds(1f); Log("após ComecarAcao(): " + Estado());
        var jog = FindObjectOfType<Movimento>(); var p0 = jog != null ? jog.transform.position : Vector3.zero;
        yield return new WaitForSeconds(3f);
        Log("3s depois na fase de Ação: " + Estado() + $" | tempoRestante={gm.TempoRestante:0.0}");
        Terminar(raiz);
    }

    void Terminar(string raiz)
    {
        File.WriteAllText(Path.Combine(raiz, "Temp/probe_jogo.txt"), _sb.ToString());
    }
}
