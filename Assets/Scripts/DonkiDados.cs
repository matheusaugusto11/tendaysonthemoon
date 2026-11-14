using System;
using UnityEngine;

/// <summary>Um evento real da NASA DONKI (maio de 2024) associado a um dia do jogo.</summary>
[Serializable]
public class EventoNasa
{
    public int dia;
    public string id, data, nome, sigla, classe, flare, perigo, resumo, link;
    public int velocidade, kp, horasAteImpacto;

    /// <summary>Uma linha curta: "CME classe O · 1.561 km/s · flare X2.2 · Kp 9".</summary>
    public string Linha
    {
        get
        {
            var partes = new System.Collections.Generic.List<string> { $"{sigla} {classe}" };
            if (velocidade > 0) partes.Add($"{velocidade:N0} km/s".Replace(",", "."));
            if (!string.IsNullOrEmpty(flare)) partes.Add("flare " + flare);
            if (kp > 0) partes.Add("Kp " + kp);
            return string.Join("  ·  ", partes);
        }
    }

    /// <summary>Texto completo do card de alerta no briefing.</summary>
    public string Card
    {
        get
        {
            string impacto = horasAteImpacto > 0 ? $"\nChegada estimada: ~{horasAteImpacto} h" : "";
            return $"ALERTA NASA ({data})\n{nome}\n{Linha}{impacto}\nRisco: {perigo}\n{resumo}";
        }
    }
}

[Serializable]
class EventosNasaArquivo { public string creditos; public EventoNasa[] eventos; }

/// <summary>Carrega Resources/EventosNASA.json (dados reais do DONKI, já curados e em português).</summary>
public static class DonkiDados
{
    static EventosNasaArquivo _dados;

    static void Carregar()
    {
        if (_dados != null) return;
        var arq = Resources.Load<TextAsset>("EventosNASA");
        _dados = arq != null ? JsonUtility.FromJson<EventosNasaArquivo>(arq.text) : new EventosNasaArquivo();
        if (_dados == null) _dados = new EventosNasaArquivo();
        if (arq == null) Debug.LogWarning("[DonkiDados] Resources/EventosNASA.json não encontrado.");
    }

    public static string Creditos { get { Carregar(); return _dados.creditos ?? ""; } }

    /// <summary>Evento NASA do dia (1–10), ou null.</summary>
    public static EventoNasa Do(int dia)
    {
        Carregar();
        if (_dados.eventos == null) return null;
        foreach (var e in _dados.eventos) if (e.dia == dia) return e;
        return null;
    }
}
