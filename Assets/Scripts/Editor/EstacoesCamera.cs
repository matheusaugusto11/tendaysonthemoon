using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Ponto de câmera das estações (tarefa 7.2 da mudança interface-e-minijogos, design D8).
/// Temp/pedido_estacoes_dump.txt → Temp/estacoes.txt: cada Estacao da cena, se está ativa, onde fica e a distância até
/// a primeira parede em 16 direções, na altura da câmera.
/// Temp/pedido_pontos_camera.txt → cria (ou refaz) o filho "PontoCamera" das 8 estações de tarefa ativas, na direção
/// mais livre e mais perto do hub, e salva Game.unity. Um ponto marcado como manual (nome "PontoCamera_manual") fica.
/// Menu: "Dez Dias/Pontos de câmera das estações".
/// </summary>
[InitializeOnLoad]
public static class EstacoesCamera
{
    public static readonly string[] Ids = { "airlock", "estufa", "reciclador", "filtro", "backup", "esteira", "vazamento", "disjuntores" };
    const float Distancia = 3.2f, Altura = 1.7f, RaioEsfera = 0.35f;
    static readonly Vector3 Hub = new Vector3(2f, 0f, 2f);
    static double _proxima;

    static EstacoesCamera() { EditorApplication.update += Sondar; }

    static void Sondar()
    {
        if (EditorApplication.timeSinceStartup < _proxima) return;
        _proxima = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists("Temp/pedido_estacoes_dump.txt")) { File.Delete("Temp/pedido_estacoes_dump.txt"); Dump(); }
        if (File.Exists("Temp/pedido_pontos_camera.txt")) { File.Delete("Temp/pedido_pontos_camera.txt"); Gerar(); Dump(); }
    }

    static void AbrirGame()
    {
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Game.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
    }

    static string Caminho(Transform t) => t.parent == null ? t.name : Caminho(t.parent) + "/" + t.name;

    static void Dump()
    {
        AbrirGame();
        var sb = new StringBuilder();
        foreach (var e in Object.FindObjectsOfType<Estacao>(true))
        {
            var p = e.transform.position;
            sb.AppendLine($"{(e.gameObject.activeInHierarchy ? "ON " : "off")} {e.id,-12} {Caminho(e.transform)}  pos=({p.x:0.00},{p.y:0.00},{p.z:0.00}) semMaquina={e.semMaquina} raio={e.raio}");
            if (e.pontoCamera != null)
            {
                var c = e.pontoCamera.position;
                sb.AppendLine($"     pontoCamera {e.pontoCamera.name} ({c.x:0.00},{c.y:0.00},{c.z:0.00}) a {Vector3.Distance(c, p):0.0} m");
            }
            if (!e.gameObject.activeInHierarchy) continue;
            var linha = new StringBuilder("     paredes:");
            for (int i = 0; i < 16; i++)
            {
                float ang = i * 22.5f;
                Vector3 dir = Quaternion.Euler(0, ang, 0) * Vector3.forward;
                Vector3 o = new Vector3(p.x, p.y + Altura, p.z);
                linha.Append(Physics.Raycast(o, dir, out var hit, 8f, ~0, QueryTriggerInteraction.Ignore) ? $" {ang:0}°={hit.distance:0.0}" : $" {ang:0}°=livre");
            }
            sb.AppendLine(linha.ToString());
            // Equipamento perto da estação (os maiores primeiro), para escolher o que o close enquadra.
            var perto = new System.Collections.Generic.List<Renderer>();
            foreach (var r in Object.FindObjectsOfType<Renderer>())
            {
                if (r.transform.IsChildOf(e.transform) || r.GetComponentInParent<Movimento>() != null) continue;
                var b = r.bounds;
                Vector3 c = b.center; c.y = p.y;
                if (Vector3.Distance(c, p) > 7f || b.size.magnitude > 9f || b.size.magnitude < 0.4f) continue;
                perto.Add(r);
            }
            perto.Sort((x, y) => y.bounds.size.magnitude.CompareTo(x.bounds.size.magnitude));
            for (int k = 0; k < Mathf.Min(14, perto.Count); k++)
            {
                var b = perto[k].bounds;
                sb.AppendLine($"     obj {Caminho(perto[k].transform)}  centro=({b.center.x:0.0},{b.center.y:0.0},{b.center.z:0.0}) tam=({b.size.x:0.0},{b.size.y:0.0},{b.size.z:0.0})");
            }
        }
        File.WriteAllText("Temp/estacoes.txt", sb.ToString());
        Debug.Log("[EstacoesCamera] Temp/estacoes.txt");
    }

    /// <summary>Distância livre de <paramref name="o"/> na direção <paramref name="dir"/>, com uma esfera do tamanho da câmera.</summary>
    static float Livre(Vector3 o, Vector3 dir, float max)
        => Physics.SphereCast(o, RaioEsfera, dir, out var hit, max, ~0, QueryTriggerInteraction.Ignore) ? hit.distance : max;

    /// <summary>A câmera em <paramref name="cam"/> vê o ponto <paramref name="alvo"/> sem nada no caminho (como confere a sonda) e não está dentro de nada.</summary>
    static bool Visao(Vector3 cam, Vector3 alvo)
    {
        if (Physics.OverlapSphere(cam, 0.3f, ~0, QueryTriggerInteraction.Ignore).Length > 0) return false;
        float d = Vector3.Distance(cam, alvo);
        return !Physics.Raycast(cam, (alvo - cam).normalized, d - 0.6f, ~0, QueryTriggerInteraction.Ignore)
            && !Physics.Raycast(alvo + (cam - alvo).normalized * 0.6f, (cam - alvo).normalized, d - 0.6f, ~0, QueryTriggerInteraction.Ignore);
    }

    static float DistanciaAoSegmento(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
        return Vector3.Distance(p, a + ab * t);
    }

    [MenuItem("Dez Dias/Pontos de câmera das estações")]
    public static void Gerar()
    {
        AbrirGame();
        var sb = new StringBuilder();
        foreach (var e in Object.FindObjectsOfType<Estacao>(false))
        {
            if (System.Array.IndexOf(Ids, e.id) < 0) continue;
            var antigo = e.transform.Find("PontoCamera");
            if (e.transform.Find("PontoCamera_manual") != null) { sb.AppendLine($"{e.id}: ponto manual, mantido"); continue; }
            if (antigo != null) Object.DestroyImmediate(antigo.gameObject);

            Vector3 p = e.transform.position;
            Vector3 alvo = p + Vector3.up * 1.1f;
            Vector3 paraHub = Hub - p; paraHub.y = 0f;
            if (paraHub.sqrMagnitude < 0.01f) paraHub = Vector3.back;
            paraHub.Normalize();
            // Procura a direção com a câmera a até 3,2 m sem parede no caminho nem na visão até a estação, e longe do
            // corpo provisório das outras estações (que não tem colisor); entre as boas, a mais longe e voltada ao hub.
            var outras = new System.Collections.Generic.List<Vector3>();
            foreach (var o in Object.FindObjectsOfType<Estacao>(false)) if (o != e && !o.semMaquina) outras.Add(o.transform.position);
            float melhorNota = float.MinValue; Vector3 melhor = Vector3.zero; float melhorDist = 0f;
            for (int i = 0; i < 48; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * 7.5f, 0) * Vector3.forward;
                Vector3 subida = (dir * Distancia + Vector3.up * (Altura - 1.1f)).normalized;
                float livre = Livre(alvo, subida, Distancia + 0.6f);
                float dist = Mathf.Min(Distancia, livre - 0.5f);
                if (dist < 1.6f) continue;
                Vector3 cam = alvo + subida * dist;
                if (!Visao(cam, alvo)) continue;
                bool tapa = false;
                foreach (var o in outras) if (DistanciaAoSegmento(new Vector3(o.x, alvo.y, o.z), cam, alvo) < 1.6f) tapa = true;
                if (tapa) continue;
                float nota = dist * 2f + Vector3.Dot(dir, paraHub) * 1.5f;
                if (nota > melhorNota) { melhorNota = nota; melhor = subida; melhorDist = dist; }
            }
            if (melhorNota == float.MinValue) { sb.AppendLine($"{e.id}: nenhuma direção livre, fica no padrão"); continue; }
            var go = new GameObject("PontoCamera");
            go.transform.SetParent(e.transform, false);
            go.transform.position = alvo + melhor * melhorDist;
            go.transform.rotation = Quaternion.LookRotation(alvo - go.transform.position, Vector3.up);
            e.pontoCamera = go.transform;
            EditorUtility.SetDirty(e);
            var c = go.transform.position;
            sb.AppendLine($"{e.id}: ponto em ({c.x:0.00},{c.y:0.00},{c.z:0.00}), a {melhorDist:0.0} m");
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        File.WriteAllText("Temp/pontos_camera.txt", sb.ToString());
        Debug.Log("[EstacoesCamera] " + sb);
    }
}
