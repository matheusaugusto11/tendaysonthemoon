using System.Collections;
using System.IO;
using System.Text;
using DezDias;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Só diagnóstico: com Temp/probe_jogo.flag, percorre o início do jogo no Play e escreve Temp/probe_jogo.txt (+ PNGs em Docs/capturas-seta).</summary>
public class ProbeJogo : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (!File.Exists(Path.Combine(Application.dataPath, "../Temp/probe_jogo.flag"))) return;
        new GameObject("ProbeJogo").AddComponent<ProbeJogo>();
    }

    readonly StringBuilder _sb = new StringBuilder();
    void Log(string s)
    {
        _sb.AppendLine($"[{Time.realtimeSinceStartup:0.0}s] {s}");
        File.WriteAllText(Path.Combine(Application.dataPath, "../Temp/probe_progresso.txt"), _sb.ToString()); // grava a cada passo
    }
    void OnEnable() { Application.logMessageReceived += Capturar; }
    void OnDisable() { Application.logMessageReceived -= Capturar; }
    void Capturar(string msg, string stack, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert)
            _sb.AppendLine($"  >> {t}: {msg}\n     {stack.Split('\n')[0]}");
    }

    string Estado()
    {
        var gm = GameManager.Instancia;
        var jog = FindObjectOfType<Movimento>();
        return $"fase={(gm != null ? gm.Fase.ToString() : "NULL")} jogador={(jog != null ? jog.transform.position.ToString("F1") : "NULL")}";
    }

    void Foto(string nome, string raiz, Vector3 camPos, Vector3 alvo)
    {
        var cam = Camera.main; if (cam == null) return;
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var pos = cam.transform.position; var rot = cam.transform.rotation;
        cam.transform.position = camPos; cam.transform.LookAt(alvo);
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
        var pasta = Path.Combine(raiz, "Docs/capturas-camera"); Directory.CreateDirectory(pasta);
        File.WriteAllBytes(Path.Combine(pasta, nome + ".png"), tex.EncodeToPNG());
        Destroy(tex); Destroy(rt);
        cam.transform.position = pos; cam.transform.rotation = rot;
    }

    IEnumerator Start()
    {
        string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        yield return new WaitForSecondsRealtime(2f);
        var gm = GameManager.Instancia;
        Log("Início: " + Estado());
        if (gm == null) { Terminar(raiz); yield break; }
        gm.Comecar(Modo.Cadete); yield return new WaitForSecondsRealtime(0.5f);
        Log("após Comecar(): " + Estado());
        for (int i = 0; i < 6 && gm.Fase != Fase.AteTarefa; i++)
        {
            if (gm.Fase == Fase.ComoJogar) gm.PularSlides();
            else if (gm.Fase == Fase.Boletim) gm.SairDoBoletim();
            yield return new WaitForSecondsRealtime(0.4f);
            Log($"passo {i}: " + Estado());
        }
        yield return new WaitForSecondsRealtime(1.5f);
        var jog = FindObjectOfType<Movimento>();
        var cam = Camera.main;
        var c3 = CameraTerceiraPessoa.Instancia;
        var cc = jog.GetComponent<CharacterController>();
        Log($"Câmera nova presente: {(c3 != null)} near={cam.nearClipPlane:0.00} zonas={ZonaCamera.Todas.Count}");

        // Casos críticos: (nome, posição dos pés, giro da câmera)
        Vector3 uLab = new Vector3(-0.866f, 0, -0.5f);
        var casos = new (string, Vector3, float)[]
        {
            ("hub_centro", new Vector3(0, 0, 1f), 0f),
            ("tunel_lab_camera_p_parede", uLab * 11.6f, -30f),
            ("sala_lab_centro", new Vector3(-19.36f, 0, -11.18f), -120f),
            ("sala_lab_costas_na_parede", new Vector3(-19.36f, 0, -11.18f) + uLab * 5.2f, 60f),
            ("estufa_corredor", new Vector3(0, 0, -20f), 180f),
            ("exterior", new Vector3(0, 0, 36f), 180f),
        };
        foreach (var (nome, pe, giro) in casos)
        {
            cc.enabled = false; jog.transform.position = pe + Vector3.up * 1.15f; jog.transform.rotation = Quaternion.Euler(0, giro, 0); cc.enabled = true;
            jog.DefinirGiro(giro); c3.Cortar();
            yield return new WaitForSecondsRealtime(1.6f);
            Vector3 cp = cam.transform.position;
            int dentro = 0; string em = "";
            foreach (var col in Physics.OverlapSphere(cp, 0.06f, ~0, QueryTriggerInteraction.Ignore))
                if (!col.transform.IsChildOf(jog.transform)) { dentro++; em += col.name + " "; }
            Log($"  {nome}: zona={c3.ZonaAtual} distancia={c3.DistanciaAtual:0.00}m fov={cam.fieldOfView:0} camera={cp:F1} DENTRO_DE_COLISOR={(dentro > 0 ? "SIM " + em : "não")}");
            Foto("cam_" + nome, raiz, cp, cp + cam.transform.forward); // mesma pose da câmera
        }

        // Atravessar do hub para o túnel do laboratório andando (porta abre sozinha)
        cc.enabled = false; jog.transform.position = uLab * 4.5f + Vector3.up * 1.15f; jog.transform.rotation = Quaternion.Euler(0, -120f, 0); cc.enabled = true;
        jog.DefinirGiro(-120f); c3.Cortar();
        yield return new WaitForSecondsRealtime(0.8f);
        jog.entradaDepuracao = new Vector2(0, 1);
        int violacoes = 0; float maxSalto = 0f; Vector3 ant = cam.transform.position;
        for (float t = 0; t < 4.5f; t += 0.25f)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            Vector3 cp = cam.transform.position;
            bool dentro = false;
            foreach (var col in Physics.OverlapSphere(cp, 0.06f, ~0, QueryTriggerInteraction.Ignore))
                if (!col.transform.IsChildOf(jog.transform)) dentro = true;
            if (dentro) violacoes++;
            maxSalto = Mathf.Max(maxSalto, (cp - ant).magnitude); ant = cp;
            Log($"  andando t={t + 0.25f:0.00}s pé={jog.transform.position:F1} zona={c3.ZonaAtual} dist={c3.DistanciaAtual:0.00} dentro={(dentro ? "SIM" : "não")}");
        }
        jog.entradaDepuracao = Vector2.zero;
        Log($"Travessia: {violacoes} amostras com a câmera dentro de colisor; maior deslocamento da câmera em 0,25 s = {maxSalto:0.00} m");
        Terminar(raiz);
    }

    void Terminar(string raiz) { File.WriteAllText(Path.Combine(raiz, "Temp/probe_jogo.txt"), _sb.ToString()); }
}
