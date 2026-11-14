using UnityEngine;

/// <summary>
/// Uma máquina/ponto de interação no mundo 3D (Estufa, Reciclador, Cama...).
/// Mostra marcador flutuante e nome quando a tarefa está ativa; some quando concluída.
/// </summary>
public class Estacao : MonoBehaviour
{
    public string id;               // ex.: "estufa" (liga com DayData.estacoes)
    public string nome;             // ex.: "Estufa"
    public Color cor = Color.cyan;
    public float raio = 3.2f;       // distância para poder interagir
    public float custoEnergia = 10f;
    public float custoTempo = 15f;  // segundos de jogo gastos

    public bool Ativa { get; private set; }
    public bool Concluida { get; private set; }

    Transform _marcador;
    TextMesh _rotulo;
    Renderer[] _corpos;
    float _fase;

    void Awake()
    {
        Montar();
        Colorir();
        Definir(false, false);
    }

    /// <summary>Cria a parte visual (idempotente). Também chamado pelo editor para ficar salvo na cena.</summary>
    public void Montar()
    {
        if (transform.Find("Visual") != null) { Achar(); return; }
        var visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        var baseGo = Prim(PrimitiveType.Cylinder, "Base", visual, new Vector3(0, 0.05f, 0), new Vector3(2.6f, 0.05f, 2.6f));
        var coluna = Prim(PrimitiveType.Cube, "Maquina", visual, new Vector3(0, 0.9f, 0), new Vector3(0.9f, 1.7f, 0.9f));
        var marc = Prim(PrimitiveType.Cube, "Marcador", visual, new Vector3(0, 3.2f, 0), Vector3.one * 0.6f);
        marc.transform.localRotation = Quaternion.Euler(45, 45, 0);

        var rot = new GameObject("Rotulo");
        rot.transform.SetParent(visual, false);
        rot.transform.localPosition = new Vector3(0, 2.4f, 0);
        var tm = rot.AddComponent<TextMesh>();
        tm.text = nome;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.08f;
        tm.color = Color.white;
        Achar();
    }

    void Achar()
    {
        var v = transform.Find("Visual");
        _marcador = v.Find("Marcador");
        var r = v.Find("Rotulo");
        _rotulo = r.GetComponent<TextMesh>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            _rotulo.font = font;
            r.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        _rotulo.text = nome;
        _corpos = new[]
        {
            v.Find("Base").GetComponent<Renderer>(),
            v.Find("Maquina").GetComponent<Renderer>(),
            _marcador.GetComponent<Renderer>(),
        };
    }

    static GameObject Prim(PrimitiveType t, string n, Transform pai, Vector3 pos, Vector3 escala)
    {
        var go = GameObject.CreatePrimitive(t);
        go.name = n;
        Object.DestroyImmediate(go.GetComponent<Collider>()); // não bloqueia o personagem
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        return go;
    }

    void Colorir() => Pintar(Concluida ? new Color(0.3f, 0.8f, 0.4f) : cor);

    void Pintar(Color c)
    {
        foreach (var r in _corpos)
        {
            var m = new Material(r.sharedMaterial);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            r.material = m;
        }
    }

    public void Definir(bool ativa, bool concluida)
    {
        Ativa = ativa;
        Concluida = concluida;
        if (_corpos == null) return;
        Pintar(concluida ? new Color(0.3f, 0.8f, 0.4f) : (ativa ? cor : new Color(0.35f, 0.35f, 0.4f)));
        _marcador.gameObject.SetActive(ativa && !concluida);
        _rotulo.gameObject.SetActive(ativa);
    }

    void Update()
    {
        if (_marcador == null) return;
        _fase += Time.deltaTime;
        if (_marcador.gameObject.activeSelf)
        {
            _marcador.localPosition = new Vector3(0, 3.2f + Mathf.Sin(_fase * 3f) * 0.25f, 0);
            _marcador.Rotate(0, 90f * Time.deltaTime, 0, Space.World);
        }
        var cam = Camera.main;
        if (cam && _rotulo.gameObject.activeSelf)
            _rotulo.transform.rotation = Quaternion.LookRotation(_rotulo.transform.position - cam.transform.position);
    }

    /// <summary>Regras da noite para esta estação: (comida se feita, oxigênio perdido se ignorada, motivo).</summary>
    public static (float comidaSeFeita, float o2SeIgnorada, string motivo) Regras(string id)
    {
        switch (id)
        {
            case "estufa": return (6f, 0f, "Estufa desligada: sem comida nova");
            case "filtro": return (0f, 8f, "Filtros entupidos: oxigênio caiu");
            case "lixo": return (0f, 5f, "Lixo acumulado: saúde da base caiu");
            case "esteira": return (0f, 3f, "Sem exercício: saúde caiu");
            case "reciclador": return (0f, 4f, "Água sem reciclar: saúde caiu");
            default: return (0f, 0f, "");
        }
    }

    /// <summary>Custo (energia, oxigênio) ao concluir a estação, e um aviso curto para o botão.</summary>
    public static (float energia, float oxigenio, string aviso) Custos(string id)
    {
        switch (id)
        {
            case "painel": return (15f, 40f, "Fora da base: -40% O2");
            case "antena": return (60f, 0f, "Gasta muita energia");
            case "disjuntores": return (20f, 0f, "");
            case "vazamento": return (5f, 0f, "");
            case "airlock": return (0f, 0f, "");
            default: return (10f, 0f, "");
        }
    }

    public float DistanciaAte(Vector3 p)
    {
        Vector3 d = transform.position - p; d.y = 0;
        return d.magnitude;
    }
}
