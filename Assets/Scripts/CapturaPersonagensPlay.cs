using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Só para validação: dentro do Play na cena PersonagensTeste (e só se existir Temp/captura_play.flag),
/// força Idle/Caminhar/Correr pelo Animator Controller real e salva PNGs em Docs/capturas-personagens.
/// </summary>
public class CapturaPersonagensPlay : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (SceneManager.GetActiveScene().name != "PersonagensTeste") return;
        string flag = Path.Combine(Application.dataPath, "../Temp/captura_play.flag");
        if (!File.Exists(flag)) return;
        new GameObject("CapturaPersonagensPlay").AddComponent<CapturaPersonagensPlay>();
    }

    IEnumerator Start()
    {
        string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string pasta = Path.Combine(raiz, "Docs/capturas-personagens");
        Directory.CreateDirectory(pasta);
        foreach (var pt in FindObjectsOfType<PersonagemTeste>()) pt.enabled = false; // nada de input durante a captura

        var cam = Camera.main;
        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var vistas = new[]
        {
            ("frente_3-4", new Vector3(5f, 2.6f, 7.5f)), ("frente", new Vector3(0f, 1.6f, 8.5f)), ("costas", new Vector3(0f, 1.6f, -7.5f)),
        };
        var estados = new[] { ("idle", 0f), ("caminhar", 1.5f), ("correr", 4.5f) };
        foreach (var (estado, vel) in estados)
        {
            foreach (var an in FindObjectsOfType<AnimadorPersonagem>()) an.SetVelocidade(vel);
            yield return new WaitForSeconds(1.4f);            // deixa a suavização e a blend tree assentarem
            foreach (var (nome, pos) in vistas)
            {
                cam.transform.position = pos; cam.transform.LookAt(new Vector3(0, 1.0f, 0));
                cam.targetTexture = rt;
                cam.Render();
                var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(pasta, $"{estado}_{nome}.png"), tex.EncodeToPNG());
                Destroy(tex);
                yield return null;
            }
            cam.targetTexture = null;
        }
        // relatório do que o Animator realmente está tocando
        var rel = "";
        foreach (var an in FindObjectsOfType<AnimadorPersonagem>())
        {
            var a = an.GetComponent<Animator>();
            rel += $"{an.name}: isHuman={a.isHuman} controller={(a.runtimeAnimatorController != null ? a.runtimeAnimatorController.name : "null")} speed={a.speed:0.00} Velocidade={a.GetFloat("Velocidade"):0.00}\n";
        }
        File.WriteAllText(Path.Combine(raiz, "Temp/captura_play_pronta.txt"), rel);
    }
}
