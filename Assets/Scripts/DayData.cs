using System;
using UnityEngine;

/// <summary>Dados de um dia do jogo. O level designer pode criar assets (Create > Dez Dias > Dia) ou usar os padrões de DiasPadrao.</summary>
[CreateAssetMenu(menuName = "Dez Dias/Dia", fileName = "Dia")]
public class DayData : ScriptableObject
{
    public int numero = 1;
    public string titulo;
    [TextArea(2, 6)] public string briefing;
    [TextArea(1, 4)] public string tarefas;
    [TextArea(1, 3)] public string alertaNasa;
    public bool alertaVermelho;
    [Tooltip("ids das Estacao FIXAS do dia, ex.: estufa, reciclador")] public string[] estacoes;
    [Tooltip("quantas tarefas sorteadas somar às fixas")] public int tarefasAleatorias;
    public EventoDia[] eventos;
    [Tooltip("de onde sortear (vazio = padrão: lixo, filtro, esteira, estufa, reciclador)")] public string[] poolAleatorio;

    [Header("Tempo e recursos")]
    public float duracaoSegundos = 300f;   // fase de ação (máx. 5 min)
    public float energiaInicial = 100f;    // bateria que o dia começa
    public float perdaOxigenioNoite = 5f;  // consumo base da noite
    public float perdaComidaNoite = 1f;
}

/// <summary>Evento programado dentro do dia (alarme, vazamento...). Ativa uma estação urgente.</summary>
[Serializable]
public class EventoDia
{
    [Tooltip("segundos de ação decorridos até disparar")] public float segundo = 120f;
    [Tooltip("id da Estacao que resolve o evento, ex.: disjuntores")] public string estacao;
    public string titulo = "ALARME!";
    [TextArea(1, 3)] public string texto;
    [Tooltip("segundos para resolver (0 = sem limite)")] public float limite = 60f;
    public float penalEnergia = 30f;
    public float penalOxigenio = 10f;
    [Tooltip("oxigênio perdido por segundo enquanto o evento está ativo")] public float drenoOxigenioPorSeg = 0f;
}
