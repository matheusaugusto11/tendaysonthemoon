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
            if (!ehGame) return;
            bool mudou = false;
            // (jogador e cenário de teste NÃO são mais recriados automaticamente; use o menu Tools se precisar)
            if (Object.FindObjectsOfType<GameManager>(true).Length == 0)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                Undo.RegisterCreatedObjectUndo(gm, "Criar GameManager");
                mudou = true;
            }
            if (CriarEstacoes()) mudou = true;
            if (ConfigurarMobile()) mudou = true;
            // (portas da estufa antiga desativadas: a base nova usa o componente Porta)
            if (!mudou) return;
            EditorSceneManager.SaveScene(cena, "Assets/Scenes/Game.unity");
            Debug.Log("[SetupCenaMovimento] Cena Game atualizada e salva (Player/GameManager).");
        };
    }

    static readonly (string id, string nome, Color cor)[] Estacoes =
    {
        ("estufa", "Estufa", new Color(0.3f, 0.9f, 0.3f)),
        ("reciclador", "Reciclador de Água", new Color(0.3f, 0.6f, 1f)),
        ("lixo", "Compactador de Lixo", new Color(0.7f, 0.5f, 0.3f)),
        ("filtro", "Filtro de Ar", new Color(0.8f, 0.8f, 0.9f)),
        ("esteira", "Esteira", new Color(1f, 0.5f, 0.5f)),
        ("painel", "Painel Solar", new Color(1f, 0.9f, 0.2f)),
        ("disjuntores", "Quadro de Disjuntores", new Color(1f, 0.6f, 0.1f)),
        ("vazamento", "Vazamento", new Color(1f, 0.2f, 0.2f)),
        ("antena", "Antena", new Color(0.8f, 0.4f, 1f)),
        ("backup", "Backup de Dados", new Color(0.2f, 1f, 0.9f)),
        ("airlock", "Airlock", new Color(0.9f, 0.3f, 0.3f)),
        ("cama", "Cama", new Color(0.9f, 0.9f, 1f)),
    };

    // Cria só as estações que ainda não existem (mantém as que você moveu).
    static bool CriarEstacoes()
    {
        var existentes = new System.Collections.Generic.HashSet<string>();
        foreach (var e in Object.FindObjectsOfType<Estacao>(true)) existentes.Add(e.id);
        var pai = GameObject.Find("Estacoes");
        bool criou = false;
        for (int i = 0; i < Estacoes.Length; i++)
        {
            if (existentes.Contains(Estacoes[i].id)) continue;
            if (pai == null) { pai = new GameObject("Estacoes"); Undo.RegisterCreatedObjectUndo(pai, "Criar estações"); }
            float ang = i * Mathf.PI * 2f / Estacoes.Length;
            var go = new GameObject("Estacao_" + Estacoes[i].id);
            go.transform.SetParent(pai.transform, false);
            go.transform.position = new Vector3(Mathf.Cos(ang) * 16f, 0, Mathf.Sin(ang) * 16f);
            var e = go.AddComponent<Estacao>();
            e.id = Estacoes[i].id;
            e.nome = Estacoes[i].nome;
            e.cor = Estacoes[i].cor;
            e.Montar();
            criou = true;
        }
        return criou;
    }

    // Tela em paisagem (celular/tablet). Roda uma vez; só mexe se estiver diferente.
    static bool ConfigurarMobile()
    {
        if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.AutoRotation
            && !PlayerSettings.allowedAutorotateToPortrait && !PlayerSettings.allowedAutorotateToPortraitUpsideDown
            && PlayerSettings.allowedAutorotateToLandscapeLeft && PlayerSettings.allowedAutorotateToLandscapeRight)
            return false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        Debug.Log("[SetupCenaMovimento] Orientação fixada em paisagem (Landscape).");
        return true;
    }

    // Põe PortasAutomaticas nas estufas (instâncias do FBX) e liga os clips *_Abrir do modelo.
    static bool ConfigurarPortas()
    {
        bool mudou = false;
        foreach (var t in Object.FindObjectsOfType<Transform>())
        {
            var go = t.gameObject;
            if (!go.name.ToLower().Contains("estufa") || go.name.StartsWith("Estacao")) continue;
            if (go.transform.parent != null && go.transform.parent.name.ToLower().Contains("estufa")) continue;
            var fonte = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (fonte == null) continue;
            string caminho = AssetDatabase.GetAssetPath(fonte);
            if (!caminho.ToLower().EndsWith(".fbx")) continue;

            if (MoverEstacaoParaDentro(go)) mudou = true;
            var portas = go.GetComponent<PortasAutomaticas>();
            if (portas == null) { portas = Undo.AddComponent<PortasAutomaticas>(go); mudou = true; }
            if (portas.somAbrir == null)
            {
                var som = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/doorOpening.mp3");
                if (som != null)
                {
                    portas.somAbrir = som;
                    EditorUtility.SetDirty(portas);
                    Debug.Log("[SetupCenaMovimento] Som de porta (doorOpening) ligado à estufa.");
                    mudou = true;
                }
            }
        }
        return mudou;
    }

    // Põe a estação "estufa" dentro do modelo da estufa (só se ela ainda estiver fora).
    static bool MoverEstacaoParaDentro(GameObject modelo)
    {
        var rs = modelo.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return false;
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        foreach (var e in Object.FindObjectsOfType<Estacao>(true))
        {
            if (e.id != "estufa") continue;
            Vector3 p = e.transform.position;
            bool dentro = p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;
            if (dentro) return false;
            Undo.RecordObject(e.transform, "Mover estação estufa");
            e.transform.position = new Vector3(b.center.x, b.min.y, b.center.z);
            Debug.Log("[SetupCenaMovimento] Estação Estufa movida para dentro do modelo da estufa.");
            return true;
        }
        return false;
    }

    // Remove objetos raiz duplicados (mesmo nome e posição) e personagens extras com Movimento.
    static bool RemoverDuplicados()
    {
        bool removeu = false;
        var vistos = new System.Collections.Generic.HashSet<string>();
        foreach (var go in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            string chave = go.name.TrimStart('_') + go.transform.position.ToString("F2");
            if (go.name == "Main Camera" || go.name == "Directional Light" || go.name == "Global Volume") chave = go.name;
            if (!vistos.Add(chave))
            {
                Debug.Log("[SetupCenaMovimento] Removendo duplicado: " + go.name);
                Undo.DestroyObjectImmediate(go);
                removeu = true;
            }
        }
        var movs = Object.FindObjectsOfType<Movimento>();
        for (int i = 1; i < movs.Length; i++)
        {
            Debug.Log("[SetupCenaMovimento] Removendo personagem extra: " + movs[i].name);
            Undo.DestroyObjectImmediate(movs[i].gameObject);
            removeu = true;
        }
        return removeu;
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
