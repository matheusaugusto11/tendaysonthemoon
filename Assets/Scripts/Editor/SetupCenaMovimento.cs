using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SetupCenaMovimento
{
    // Ao abrir/recompilar no Editor, monta o cenário uma única vez na cena "Game".
    static SetupCenaMovimento()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var cena = EditorSceneManager.GetActiveScene();
            Debug.Log("[SetupCenaMovimento] cena ativa: '" + cena.name + "' path='" + cena.path + "'");
            bool ehGame = cena.name == "Game" || (string.IsNullOrEmpty(cena.path) && GameObject.Find("Main Camera") != null);
            if (!ehGame || GameObject.Find("Player") != null) return;
            Criar();
            EditorSceneManager.SaveScene(cena, "Assets/Scenes/Game.unity");
            Debug.Log("[SetupCenaMovimento] Cenário criado e cena Game salva.");
        };
    }

    [MenuItem("Tools/Criar Cenário de Movimento")]
    static void Criar()
    {
        Random.InitState(42);
        var chao = GameObject.CreatePrimitive(PrimitiveType.Plane);
        chao.name = "Chao";
        chao.transform.localScale = new Vector3(5, 1, 5);
        Undo.RegisterCreatedObjectUndo(chao, "Criar chão");

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1.1f, 0);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();
        player.AddComponent<Movimento>();
        // Um "nariz" para ver a direção em que o personagem olha.
        var nariz = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nariz.name = "Frente";
        Object.DestroyImmediate(nariz.GetComponent<BoxCollider>());
        nariz.transform.SetParent(player.transform, false);
        nariz.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
        nariz.transform.localScale = Vector3.one * 0.3f;
        Undo.RegisterCreatedObjectUndo(player, "Criar player");

        // Objetos que dá para empurrar (Rigidbody).
        for (int i = 0; i < 6; i++)
            Fisico(PrimitiveType.Cube, "CaixaEmpurravel" + i, new Vector3(-6 + i * 0.4f, 0.5f + i * 1.01f * 0, 6), Vector3.one, 1f);
        // Pilha de caixas para derrubar.
        for (int y = 0; y < 4; y++)
            Fisico(PrimitiveType.Cube, "Pilha" + y, new Vector3(6, 0.5f + y, 6), Vector3.one, 1f);
        // Esferas e uma caixa pesada.
        for (int i = 0; i < 4; i++)
            Fisico(PrimitiveType.Sphere, "Bola" + i, new Vector3(-3 + i * 1.5f, 0.5f, 10), Vector3.one, 0.5f);
        Fisico(PrimitiveType.Cube, "CaixaPesada", new Vector3(0, 0.75f, 12), Vector3.one * 1.5f, 15f);

        // Rampa e degraus (estáticos) para testar subida.
        var rampa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rampa.name = "Rampa";
        rampa.transform.position = new Vector3(-10, 0.9f, -6);
        rampa.transform.rotation = Quaternion.Euler(-20, 0, 0);
        rampa.transform.localScale = new Vector3(4, 0.4f, 8);
        Undo.RegisterCreatedObjectUndo(rampa, "Criar rampa");
        for (int i = 0; i < 5; i++)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.name = "Degrau" + i;
            d.transform.position = new Vector3(10, 0.1f + i * 0.2f, -6 + i * 0.8f);
            d.transform.localScale = new Vector3(4, 0.2f + i * 0.4f, 0.8f);
            Undo.RegisterCreatedObjectUndo(d, "Criar degrau");
        }
        // Paredes/pilares fixos para colisão.
        for (int i = 0; i < 6; i++)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            c.name = "Pilar" + i;
            c.transform.position = new Vector3(Random.Range(-20f, 20f), 1.5f, Random.Range(-20f, 20f));
            c.transform.localScale = new Vector3(1, 1.5f, 1);
            Undo.RegisterCreatedObjectUndo(c, "Criar pilar");
        }
        Selection.activeGameObject = player;
    }

    static void Fisico(PrimitiveType tipo, string nome, Vector3 pos, Vector3 escala, float massa)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nome;
        go.transform.position = pos;
        go.transform.localScale = escala;
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = massa;
        Undo.RegisterCreatedObjectUndo(go, "Criar " + nome);
    }
}
