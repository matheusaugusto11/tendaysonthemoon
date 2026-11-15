using System.Collections.Generic;
using DezDias;
using UnityEngine;

/// <summary>Papéis de texto da interface (design D3): cada um tem uma fonte, um tamanho e só isso.</summary>
public enum Papel { TituloTela, TituloPainel, Numero, Corpo, Rotulo }

/// <summary>
/// Todos os valores visuais da interface num lugar só (design D2 da mudança interface-e-minijogos): cores,
/// fontes, tamanhos, espaços, chanfros, tempos e ícones. Nenhuma tela escreve cor, tamanho ou tempo solto.
/// As cores em hex ficam em <see cref="Paleta"/> (assembly de regras, testado); aqui só viram Color.
/// </summary>
public static class UiTema
{
    static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

    // ---------- Cores ----------
    public static readonly Color Aco = C(Paleta.Aco), AcoClaro = C(Paleta.AcoClaro), Sombra = C(Paleta.Sombra),
        Pagina = C(Paleta.Pagina), Vazio = C(Paleta.Vazio);
    public static readonly Color Branco = C(Paleta.Branco), BrancoSombra = C(Paleta.BrancoSombra), Tinta = C(Paleta.Tinta);
    public static readonly Color Apoio = C(Paleta.Apoio), Discreto = C(Paleta.Discreto),
        DesativadoFace = C(Paleta.DesativadoFace), DesativadoSombra = C(Paleta.DesativadoSombra);
    public static readonly Color Ar = C(Paleta.Ar), Estufa = C(Paleta.Estufa), Abrigo = C(Paleta.Abrigo),
        Lab = C(Paleta.Lab), Energia = C(Paleta.Energia);
    public static readonly Color Alerta = C(Paleta.Alerta), Terra = C(Paleta.Terra), AguaUsada = C(Paleta.AguaUsada);

    /// <summary>Acento do módulo: o mesmo da cor do módulo na cena 3D. O Conserto (só com vazamento) é neutro: não há botão vermelho.</summary>
    public static Color CorDoModulo(Modulo m) => m switch
    {
        Modulo.Ar => Ar, Modulo.Estufa => Estufa, Modulo.Abrigo => Abrigo, Modulo.Lab => Lab, Modulo.Bateria => Energia, _ => Apoio,
    };

    /// <summary>Acento de um recurso lido de um texto de custo (ver <see cref="Apresentacao.PartesDoCusto(string)"/>).</summary>
    public static Color CorDoRecurso(Recurso r) => r switch
    {
        Recurso.Oxigenio => Ar, Recurso.Energia => Energia, Recurso.Bateria => Energia, Recurso.Estufa => Estufa,
        Recurso.Radiacao => Abrigo, Recurso.Ciencia => Branco, Recurso.Comida => Branco, _ => Apoio,
    };

    // ---------- Espaços, chanfros, alvos e tempos ----------
    public const int E4 = 4, E8 = 8, E12 = 12, E16 = 16, E24 = 24, E32 = 32, E48 = 48, E64 = 64;
    public const float ChanfroPainel = 14f, ChanfroBotao = 10f, ChanfroCelula = 5f;
    /// <summary>Deslocamento da sombra dura, para baixo, sem desfoque.</summary>
    public const float Sombra_Y = 6f;
    /// <summary>Alvo tocável mínimo no referencial 1920 por 1080, e espaço mínimo entre alvos.</summary>
    public const float AlvoMinimo = 88f, EspacoEntreAlvos = 12f;
    // Movimento (segundos). Nada de interface passa de 0,5 s.
    public const float TempoToque = 0.08f, TempoPainel = 0.25f, TempoMedidor = 0.6f, TempoFaixa = 0.3f, TempoMaximo = 0.5f;

    // ---------- Movimento reduzido ----------

    /// <summary>Só para o editor e os testes: faz de conta que o navegador pediu movimento reduzido.</summary>
    public static bool ForcarMovimentoReduzido { get; set; }

    static float _proximaLeitura;
    static bool _reduzidoDoNavegador;

    /// <summary>
    /// O navegador pede movimento reduzido (prefers-reduced-motion). Com ele, deslocamentos, escalas, órbitas e voos de
    /// câmera viram corte ou troca de opacidade, e tudo o que anda fica parado. Lido do navegador no máximo 1 vez por segundo.
    /// </summary>
    public static bool MovimentoReduzido
    {
        get
        {
            if (ForcarMovimentoReduzido) return true;
            if (Time.realtimeSinceStartup >= _proximaLeitura)
            {
                _proximaLeitura = Time.realtimeSinceStartup + 1f;
                _reduzidoDoNavegador = Navegador.PrefereMenosMovimento;
            }
            return _reduzidoDoNavegador;
        }
    }

    // ---------- Fontes ----------
    static Font _barlowBold, _barlowSemi, _nunito, _reserva;

    static Font Carregar(string nome)
    {
        var f = Resources.Load<Font>("Fontes/" + nome);
        if (f != null) return f;
        Debug.LogWarning($"[UiTema] Fonte Resources/Fontes/{nome} não encontrada: usando a fonte embutida da Unity.");
        if (_reserva == null) _reserva = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _reserva;
    }

    /// <summary>Barlow Semi Condensed Bold: título de tela e números de medidor.</summary>
    public static Font BarlowBold => _barlowBold != null ? _barlowBold : _barlowBold = Carregar("BarlowSemiCondensed-Bold");
    /// <summary>Barlow Semi Condensed SemiBold: título de painel, botão e rótulo.</summary>
    public static Font BarlowSemi => _barlowSemi != null ? _barlowSemi : _barlowSemi = Carregar("BarlowSemiCondensed-SemiBold");
    /// <summary>Nunito Bold: corpo, falas e cartões. Também tem o subscrito ₂ ("O₂"), que a Barlow não tem.</summary>
    public static Font Nunito => _nunito != null ? _nunito : _nunito = Carregar("Nunito-Bold");

    public static Font Fonte(Papel p) => p switch
    {
        Papel.TituloTela => BarlowBold, Papel.Numero => BarlowBold,
        Papel.TituloPainel => BarlowSemi, Papel.Rotulo => BarlowSemi,
        _ => Nunito,
    };

    /// <summary>
    /// Fonte do papel para um texto concreto. A Barlow não tem o subscrito ₂: no Editor a Unity completa com uma fonte do
    /// sistema, que não existe no WebGL e viraria quadrado. Todo texto com ₂ (como "O₂") usa a Nunito, que tem o glifo.
    /// </summary>
    public static Font Fonte(Papel p, string texto)
    {
        var f = Fonte(p);
        return f != Nunito && texto != null && texto.IndexOf('₂') >= 0 ? Nunito : f;
    }

    /// <summary>Tamanho em unidades do referencial 1920 por 1080. Nenhum texto abaixo de 24.</summary>
    public static int Tamanho(Papel p) => p switch
    {
        Papel.TituloTela => 60, Papel.TituloPainel => 40, Papel.Numero => 36, Papel.Corpo => 30, _ => 24,
    };

    /// <summary>Rótulos são em caixa alta (o Text da Unity não tem espaçamento entre letras, então só a caixa).</summary>
    public static string Caixa(string s) => string.IsNullOrEmpty(s) ? s : s.ToUpperInvariant();

    // ---------- Ícones ----------
    static readonly Dictionary<string, Sprite> _icones = new Dictionary<string, Sprite>();
    static readonly HashSet<string> _avisados = new HashSet<string>();

    /// <summary>Ícone de Assets/Resources/Icones (128 px). Devolve null, com um aviso só, se o nome não existir.</summary>
    public static Sprite Icone(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return null;
        if (_icones.TryGetValue(nome, out var s)) return s;
        s = Resources.Load<Sprite>("Icones/" + nome);
        if (s == null && _avisados.Add(nome)) Debug.LogWarning($"[UiTema] Ícone Resources/Icones/{nome} não encontrado.");
        _icones[nome] = s;
        return s;
    }

    /// <summary>Ícone de cada módulo de energia.</summary>
    public static string IconeDoModulo(Modulo m) => m switch
    {
        Modulo.Ar => "mod_ar", Modulo.Estufa => "mod_estufa", Modulo.Abrigo => "mod_abrigo", Modulo.Lab => "mod_lab",
        Modulo.Bateria => "mod_bateria", _ => "tarefa_remendo",
    };

    /// <summary>Ícone de cada recurso lido de um custo.</summary>
    public static string IconeDoRecurso(Recurso r) => r switch
    {
        Recurso.Oxigenio => "oxigenio", Recurso.Energia => "energia", Recurso.Ciencia => "estrela", Recurso.Estufa => "mod_estufa",
        Recurso.Comida => "comida", Recurso.Radiacao => "radiacao", Recurso.Bateria => "mod_bateria", _ => null,
    };

    /// <summary>
    /// Cor do módulo ou sistema de cada tarefa: a da tira de luz dos painéis, da faixa do deslocamento e da seta guia.
    /// O exercício é na esteira do Laboratório, por isso laranja.
    /// </summary>
    public static Color CorDaTarefa(string id)
    {
        switch (id)
        {
            case Tarefas.Estufa: return Estufa;
            case Tarefas.Agua: case Tarefas.Filtros: case Tarefas.Upload: case Tarefas.Antena: case Tarefas.Vazamento: return Ar;
            case Tarefas.Poeira: return Energia;
            default: return Lab; // coleta de pedras, exercício e braço robótico
        }
    }

    /// <summary>Ícone de cada tarefa do dia.</summary>
    public static string IconeDaTarefa(string id) => id switch
    {
        Tarefas.Amostra => "tarefa_expedicao", Tarefas.Poeira => "tarefa_poeira", Tarefas.Estufa => "tarefa_estufa",
        Tarefas.Agua => "mod_ar", Tarefas.Upload => "terra", Tarefas.Filtros => "tarefa_poeira",
        Tarefas.Exercicio => "check", Tarefas.Braco => "tarefa_expedicao", Tarefas.Vazamento => "tarefa_remendo",
        Tarefas.Antena => "terra", _ => "ajuda",
    };

    /// <summary>Estado da cache de fontes ao trocar de cena ou de Play (o Editor não recarrega os campos estáticos).</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reiniciar()
    {
        _barlowBold = _barlowSemi = _nunito = _reserva = null;
        ForcarMovimentoReduzido = false; _proximaLeitura = 0f; _reduzidoDoNavegador = false;
        _icones.Clear(); _avisados.Clear();
    }
}
