using UnityEngine;

/// <summary>Os 10 dias do roteiro, criados em código quando nenhum DayData é atribuído ao GameManager.</summary>
public static class DiasPadrao
{
    static readonly string[][] Estacoes =
    {
        new[] { "estufa", "reciclador" },
        new string[0],                 // Dia 2: 2 sorteadas
        new[] { "esteira" },           // Dia 3: esteira + 1 sorteada
        new[] { "painel", "estufa", "lixo" },
        new[] { "disjuntores" },
        new string[0],                 // Dia 6: 3 sorteadas (consequências)
        new[] { "vazamento" },
        new[] { "antena", "disjuntores" },
        new[] { "painel" },            // Dia 9: painel + 2 sorteadas
        new[] { "backup" },
    };

    public static DayData[] Criar()
    {
        var dias = CriarBase();
        for (int i = 0; i < dias.Length; i++) dias[i].estacoes = Estacoes[i];
        dias[1].tarefasAleatorias = 2;
        dias[2].tarefasAleatorias = 1;
        dias[5].tarefasAleatorias = 3;
        dias[8].tarefasAleatorias = 2;
        return dias;
    }

    static DayData[] CriarBase()
    {
        return new[]
        {
            Dia(1, "O Básico", "O computador pede para ligar a base.",
                "Ligue a alavanca da Estufa. Use o Reciclador de Água.", "", false, 5),
            Dia(2, "A Rotina Chata", "Alerta de poeira nos filtros e lixo acumulado.",
                "Compactador de Lixo e Filtro de Ar. Pular as tarefas reduz a Saúde da Base.",
                "Regolito lunar é como vidro quebrado. Destrói circuitos se não for limpo!", false, 5),
            Dia(3, "O Fator Corpo", "Alarme médico.",
                "Exercício na Esteira. Sobra menos tempo para cuidar da Estufa.",
                "Astronautas perdem massa óssea sem gravidade. O exercício diário é obrigatório.", false, 5),
            Dia(4, "Sair da Base", "Bateria amarela: regolito cobriu os painéis solares externos.",
                "Vá ao Airlock e limpe os painéis. Isso gasta 40% do seu oxigênio!", "", false, 8),
            Dia(5, "A Crise", "Briefing em VERMELHO. Tempestade solar detectada!",
                "Prepare o Escudo no Quadro de Disjuntores quando o alarme tocar.",
                "DONKI (NASA): CME / Flare classe X detectada.", true, 5),
            Dia(6, "Consequências", "A tempestade passou, mas a base está um caos.",
                "Lixo acumulado, plantas quase mortas, filtros entupidos. Priorize.", "", false, 5),
            Dia(7, "O Micro-Meteorito", "Tudo parece normal... por enquanto.",
                "Fique atento a qualquer vazamento.", "", false, 5),
            Dia(8, "A Grande Transmissão", "Janela de Comunicação aberta.",
                "Envie os dados para a Terra. A antena gasta quase toda a energia do dia.", "", false, 5),
            Dia(9, "Empacotando", "Mensagem da Terra: nave de resgate amanhã. Deixe a base limpa.",
                "Ejete o lixo, recicle a última água e limpe os painéis.", "", false, 5),
            Dia(10, "O Resgate", "Hoje é o dia final.",
                "Arraste as maletas de pesquisa no Backup e entre no Airlock.", "", false, 0),
        };
    }

    static DayData Dia(int n, string titulo, string briefing, string tarefas, string nasa, bool vermelho, float perdaO2)
    {
        var d = ScriptableObject.CreateInstance<DayData>();
        d.numero = n;
        d.titulo = titulo;
        d.briefing = briefing;
        d.tarefas = tarefas;
        d.alertaNasa = nasa;
        d.alertaVermelho = vermelho;
        d.perdaOxigenioNoite = perdaO2;
        return d;
    }
}
