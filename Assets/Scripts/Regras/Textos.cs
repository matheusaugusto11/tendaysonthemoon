namespace DezDias
{
    /// <summary>
    /// Textos do jogo em PT e EN, copiados do protótipo web (web/index.html, chamadas L(pt,en)).
    /// Só caracteres do Latin-1 (a fonte do WebGL não tem emoji nem O₂ subscrito): ícones viram palavras.
    /// </summary>
    public static class Textos
    {
        static Txt T(string pt, string en) => new Txt(pt, en);

        // ---------- Início ----------
        public static readonly Txt Titulo = T("Dez Dias na Lua", "Ten Days on the Moon");
        public static readonly Txt IntroTexto = T("Você comanda uma base no polo sul da Lua, com quatro astronautas. A missão dura 10 dias.", "You command a base at the Moon's south pole, with four astronauts. The mission lasts 10 days.");
        public static readonly Txt IntroItem1 = T("Todo dia a base ganha poucas células de energia. Você escolhe para onde elas vão. Não dá para ligar tudo.", "Every day the base gets a few energy cells. You choose where they go. You cannot power everything.");
        public static readonly Txt IntroItem2 = T("Cuide do oxigênio, da radiação e da comida, e faça ciência para ganhar estrelas.", "Look after oxygen, radiation and food, and do science to earn stars.");
        public static readonly Txt IntroItem3 = T("Toda escolha tem um custo. As erupções solares do jogo aconteceram de verdade: são dados arquivados da NASA.", "Every choice has a cost. The solar flares in the game really happened: they are archived NASA data.");
        public static readonly Txt EscolhaMissao = T("Escolha sua missão", "Choose your mission");
        public static readonly Txt Cadete = T("Cadete", "Cadet");
        public static readonly Txt CadeteIdade = T("7 a 9 anos", "Ages 7 to 9");
        public static readonly Txt CadeteDesc = T("Mais ar e comida no começo. Menos erupções.", "More air and food at the start. Fewer flares.");
        public static readonly Txt Comandante = T("Comandante", "Commander");
        public static readonly Txt ComandanteIdade = T("10 a 12 anos", "Ages 10 to 12");
        public static readonly Txt ComandanteDesc = T("Mais alertas e dias de sombra mais duros.", "More alerts and harsher shadow days.");
        public static readonly Txt ComoJogar = T("Como jogar", "How to play");
        public static readonly Txt TutorialDia1 = T("Tutorial no Dia 1: {0}", "Day 1 tutorial: {0}");
        public static readonly Txt Ligado = T("ligado", "on");
        public static readonly Txt Desligado = T("desligado", "off");
        public static readonly Txt Creditos = T("Créditos e fontes", "Credits and sources");
        /// <summary>Rótulo do botão de idioma: mostra o idioma para onde ele troca.</summary>
        public static readonly Txt BotaoIdioma = T("English", "Português");
        public static readonly Txt Rodape = T("Arte e código gerados com IA · Protótipo para o NASA Space Apps Challenge 2026 · Não é um produto da NASA", "Art and code made with AI · Prototype for the NASA Space Apps Challenge 2026 · Not a NASA product");

        // ---------- Como jogar (slides) ----------
        public static readonly Txt[] SlideTitulo =
        {
            T("Sua missão", "Your mission"),
            T("A energia é pouca", "Energy is scarce"),
            T("Três coisas para cuidar", "Three things to look after"),
            T("Ande e resolva", "Walk and solve"),
            T("O Sol é de verdade", "The Sun is real"),
        };
        public static readonly Txt[] SlideTexto =
        {
            T("Você comanda uma base no polo sul da Lua, com quatro astronautas. A missão dura 10 dias.", "You command a base at the Moon's south pole, with four astronauts. The mission lasts 10 days."),
            T("Todo dia os painéis solares dão poucas células de energia. Você escolhe o que ligar: ar e água, estufa, abrigo, laboratório ou bateria. Não dá para ligar tudo!", "Every day the solar panels give a few energy cells. You choose what to power: air & water, greenhouse, shelter, lab or battery. You cannot power everything!"),
            T("O oxigênio não pode acabar. A radiação não pode encher. A comida não pode faltar. Se uma delas chegar ao limite, a tripulação volta para casa mais cedo, em segurança.", "Oxygen must not run out. Radiation must not fill up. Food must not run short. If one of them hits its limit, the crew goes home early, safely."),
            T("Todo dia tem uma tarefa. Leve o astronauta até a estação marcada seguindo a seta (setas, W A S D, controle ou o joystick na tela). Cada tarefa é um mini jogo, e toda tarefa tem um custo. Depois, volte ao console no centro do hub para distribuir a energia. Você tem 5 minutos por dia.", "Every day has a task. Take the astronaut to the marked station by following the arrow (arrow keys, W A S D, a gamepad or the on-screen joystick). Each task is a mini game, and every task has a cost. Then go back to the console in the middle of the hub to share out the energy. You have 5 minutes per day."),
            T("Às vezes chega um alerta de erupção solar, forte ou fraco. Ligar o abrigo protege da radiação, mas gasta energia. As erupções do jogo aconteceram de verdade: são dados arquivados do NASA DONKI.", "Sometimes a solar flare alert arrives, strong or weak. Powering the shelter protects from radiation, but uses energy. The flares in the game really happened: they are archived NASA DONKI data."),
        };
        public static readonly Txt Voltar = T("Voltar", "Back");
        public static readonly Txt Proximo = T("Próximo", "Next");
        public static readonly Txt Pular = T("Pular", "Skip");
        public static readonly Txt Comecar = T("Começar!", "Start!");

        // ---------- HUD e pausa ----------
        public static readonly Txt DiaDe = T("Dia {0} de {1}", "Day {0} of {1}");
        public static readonly Txt Oxigenio = T("Oxigênio", "Oxygen");
        public static readonly Txt Radiacao = T("Radiação", "Radiation");
        public static readonly Txt Bateria = T("Bateria", "Battery");
        public static readonly Txt Comida = T("Comida", "Food");
        public static readonly Txt Ciencia = T("Ciência", "Science");
        public static readonly Txt Pausa = T("PAUSA", "PAUSED");
        public static readonly Txt Continuar = T("Continuar", "Continue");
        public static readonly Txt ContinuarB = T("CONTINUAR (B)", "CONTINUE (B)");
        public static readonly Txt Reiniciar = T("REINICIAR", "RESTART");
        public static readonly Txt SomLigado = T("Som: ligado", "Sound: on");
        public static readonly Txt SomDesligado = T("Som: desligado", "Sound: off");
        public static readonly Txt ReiniciarPergunta = T("A missão atual será perdida. Voltar ao início?", "The current mission will be lost. Go back to the start?");
        public static readonly Txt SimVoltarAoInicio = T("Sim, voltar ao início", "Yes, back to the start");
        public static readonly Txt AjudaControle = T("A: confirmar   B: voltar   Start: pausa", "A: confirm   B: back   Start: pause");

        // ---------- Boletim ----------
        public static readonly Txt Boletim = T("Boletim da manhã", "Morning briefing");
        public static readonly Txt Parte1 = T("Parte 1 de 3 · Rotina da base", "Part 1 of 3 · Base routine");
        public static readonly Txt Parte2 = T("Parte 2 de 3 · Imprevistos do espaço", "Part 2 of 3 · Space surprises");
        public static readonly Txt Parte3 = T("Parte 3 de 3 · Preparar a volta para casa", "Part 3 of 3 · Getting ready to go home");
        public static readonly Txt LuzDeHoje = T("Luz de hoje", "Today's light");
        public static readonly Txt LuzSombra = T("o Sol ficou atrás de um morro. Os painéis rendem só {0} células.", "the Sun is behind a hill. The panels give only {0} cells.");
        public static readonly Txt LuzNormal = T("o Sol anda baixo pelo horizonte e os painéis rendem {0} células.", "the Sun travels low along the horizon and the panels give {0} cells.");
        public static readonly Txt NaBateria = T(" Há {0} na bateria.", " There are {0} in the battery.");
        public static readonly Txt AlertaForte = T("Alerta FORTE de erupção solar", "STRONG solar flare alert");
        public static readonly Txt AlertaForteTexto = T("É bem provável que a erupção venha hoje.", "The flare will very likely come today.");
        public static readonly Txt AlertaFraco = T("Alerta fraco de erupção solar", "Weak solar flare alert");
        public static readonly Txt AlertaFracoTexto = T("Pode ser que a erupção não venha.", "The flare may not come.");
        public static readonly Txt AbrigoCusta = T("As partículas podem chegar durante o trabalho: aí você decide se corre para o abrigo. À noite, ligar o Abrigo protege a base, mas custa energia.", "The particles may arrive while you work: then you decide whether to run to the shelter. At night, powering the Shelter protects the base, but costs energy.");
        public static readonly Txt CmeACaminho = T("Nuvem do Sol a caminho", "Cloud from the Sun on its way");
        public static readonly Txt CmeChegaNoDia = T("chega no Dia {0}.", "arrives on Day {0}.");
        public static readonly Txt CmePreparado = T(" O abrigo já está preparado: 1 célula no Abrigo vai bastar.", " The shelter is ready: 1 cell in the Shelter will be enough.");
        public static readonly Txt CmeNaoPreparado = T(" O abrigo não foi preparado: vai precisar de 2 células.", " The shelter was not prepared: it will need 2 cells.");
        public static readonly Txt CmeChegou = T("É a nuvem anunciada no Dia {0}.", "It is the cloud announced on Day {0}.");
        public static readonly Txt EnergiaDeHoje = T("Energia de hoje", "Today's energy");
        public static readonly Txt ClimaEspacial = T("Clima espacial", "Space weather");
        public static readonly Txt SolCalmo = T("Sol calmo hoje.", "the Sun is calm today.");
        public static readonly Txt VazamentoAberto = T("Vazamento aberto", "Open leak");
        public static readonly Txt VazamentoAbertoTexto = T("a base perde 1 de oxigênio por dia até o conserto (2 células).", "the base loses 1 oxygen a day until repaired (2 cells).");
        public static readonly Txt Tripulacao = T("Tripulação", "Crew");
        public static readonly Txt TripulacaoForte = T("em ótima forma depois do exercício. 1 célula a mais hoje.", "in great shape after exercising. 1 extra cell today.");
        public static readonly Txt TripulacaoFraca = T("mais fraca sem exercício. 1 célula a menos hoje.", "weaker without exercise. 1 cell fewer today.");
        public static readonly Txt SemContato = T("Sem contato com a Terra", "No contact with Earth");
        public static readonly Txt SemContatoTexto = T("hoje você decide sem a dica do controle da missão.", "today you decide without mission control's hint.");

        // ---------- Deslocamento e relógio ----------
        public static readonly Txt LeveAte = T("Leve o astronauta até: {0}", "Take the astronaut to: {0}");
        public static readonly Txt ConsoleHub = T("Console de energia, no centro do hub", "Energy console, in the middle of the hub");
        public static readonly Txt Console = T("Console de energia", "Energy console");
        public static readonly Txt BotaoIniciar = T("{0}\nINICIAR (A / E)", "{0}\nSTART (A / E)");
        public static readonly Txt BotaoDistribuir = T("DISTRIBUIR ENERGIA\n(A / E)", "SHARE OUT ENERGY\n(A / E)");
        public static readonly Txt TempoAcabou = T("O tempo do dia acabou!", "The day's time is up!");
        public static readonly Txt TempoAcabouTarefa = T("O tempo do dia acabou antes da tarefa. ", "The day's time ran out before the task. ");

        // ---------- Escolha e tarefa ----------
        public static readonly Txt RiscoErupcao = T("Hoje tem alerta de erupção. Quem sai da base fica sem a proteção do abrigo.", "There is a flare alert today. Anyone who goes outside is away from the shelter's protection.");
        public static readonly Txt RiscoContinuou = T("Você escolheu continuar no alarme. Quem sai da base pega mais radiação se as partículas vierem.", "You chose to keep working during the alarm. Anyone who goes outside takes more radiation if the particles come.");

        // ---------- Evento do dia e corrida até o abrigo ----------
        public static readonly Txt EventoDoDia = T("Evento do dia", "Event of the day");
        public static readonly Txt Abrigo = T("Abrigo", "Shelter");
        public static readonly Txt CorridaTitulo = T("Corra para o abrigo!", "Run to the shelter!");
        public static readonly Txt CorridaSegundos = T("{0} s", "{0} s");
        public static readonly Txt ChegouAbrigo = T("Vocês chegaram ao abrigo! A tripulação espera a tempestade passar.", "You made it to the shelter! The crew waits for the storm to pass.");
        public static readonly Txt AtrasouAbrigo = T("As partículas chegaram antes de vocês. Se vierem de verdade, metade da radiação do alarme vai contar.", "The particles got there before you. If they really come, half of the alarm's radiation will count.");
        public static readonly Txt Indisponivel = T("Indisponível", "Unavailable");
        public static readonly Txt OxigenioBaixo = T("Oxigênio baixo demais para sair.", "Oxygen is too low to go outside.");
        public static readonly Txt VoltarAoHub = T("Voltar ao hub e distribuir energia", "Back to the hub to share out energy");
        public static readonly Txt JogarDeNovo = T("Jogar de novo", "Play again");
        public static readonly Txt MenuMinijogos = T("Menu de minijogos", "Minigame menu");
        public static readonly Txt VoceSabia = T("Você sabia?", "Did you know?");
        public static readonly Txt Fontes = T("Fontes", "Sources");
        public static readonly Txt Fonte = T("Fonte", "Source");
        public static readonly Txt PrimeiraVez = T("Este aviso aparece só na primeira vez deste mini jogo. Depois, ele fica no botão ?.", "This card only shows the first time you play this mini game. After that, it lives under the ? button.");
        public static readonly Txt Sair = T("SAIR (B)", "EXIT (B)");
        public static readonly Txt Concluido = T("Concluído!", "Done!");
        public static readonly Txt TempoEsgotado = T("Tempo esgotado!", "Time's up!");
        public static readonly Txt Nota = T("Nota: {0}%", "Score: {0}%");
        // Faixa de resultado do minijogo (spec apresentacao-dos-minijogos, "Fim sem punição")
        public static readonly Txt Otimo = T("Ótimo!", "Great!");
        public static readonly Txt Bom = T("Bom!", "Good!");
        public static readonly Txt NaoDeu = T("Não deu desta vez", "Not this time");
        public static readonly Txt NotaCurta = T("Nota {0}%", "Score {0}%");
        public static readonly Txt Sobrecarga = T("Sobrecarga", "Overload");
        public static readonly Txt Carga = T("Carga {0} de {1}", "Load {0} of {1}");
        public static readonly Txt XDeY = T("{0} de {1}", "{0} of {1}");
        public static readonly Txt LigarEste = T("Ligar este", "Switch on");
        // Gesto de cada minijogo (cartão de primeira vez e mão fantasma)
        public static readonly Txt GestoArrastar = T("Arraste", "Drag");
        public static readonly Txt GestoEsfregar = T("Esfregue", "Scrub");
        public static readonly Txt GestoRitmo = T("Toque no ritmo", "Tap in rhythm");
        public static readonly Txt GestoSegurar = T("Segure", "Hold");
        public static readonly Txt GestoGirar = T("Toque para girar", "Tap to turn");
        public static readonly Txt GestoLigar = T("Arraste para ligar", "Drag to switch on");
        public static readonly Txt OlheComoFaz = T("Olhe como se faz", "Watch how it is done");

        // ---------- Distribuição ----------
        public static readonly Txt Distribuir = T("Distribuir energia", "Share out the energy");
        public static readonly Txt DistribuirTexto = T("Use + e - para mandar células a cada módulo. Não dá para ligar tudo.", "Use + and - to send cells to each module. You cannot power everything.");
        public static readonly Txt SugestaoCadete = T(" Já deixamos 2 no ar e água; você pode mudar.", " We already put 2 on air & water; you can change that.");
        public static readonly Txt CelulasLivres = T("Células livres: {0}", "Free cells: {0}");
        public static readonly Txt AlertaForteHoje = T("Alerta FORTE de erupção hoje", "STRONG flare alert today");
        public static readonly Txt AlertaFracoHoje = T("Alerta fraco de erupção hoje", "Weak flare alert today");
        public static readonly Txt DicaControle = T("Dica do controle da missão", "Mission control's hint");
        public static readonly Txt CelulasVaoSePerder = T("{0} célula(s) livres vão se perder se não forem usadas ou guardadas.", "{0} free cell(s) will be lost unless used or saved.");
        public static readonly Txt ComecarDia = T("Começar o dia", "Start the day");

        public static readonly Txt[] NomeModulo =
        {
            T("Ar e água", "Air & water"), T("Estufa", "Greenhouse"), T("Abrigo", "Shelter"),
            T("Laboratório", "Lab"), T("Bateria", "Battery"), T("Conserto", "Repair"),
        };
        public static readonly Txt DicaAr = T("2 células mantêm o oxigênio. 3 recuperam 1. Com menos, ele cai.", "2 cells keep oxygen steady. 3 win back 1. With fewer, it drops.");
        public static readonly Txt DicaEstufa = T("Faltam {0} células para colher comida e ar.", "{0} more cells until a harvest of food and air.");
        public static readonly Txt DicaAbrigo = T("Protege da radiação se uma erupção vier.", "Protects from radiation if a flare hits.");
        public static readonly Txt DicaLab = T("Cada célula rende 1 estrela de ciência.", "Each cell earns 1 science star.");
        public static readonly Txt DicaBateria = T("Guarda células para os dias de sombra.", "Saves cells for shadow days.");
        public static readonly Txt DicaConserto = T("Precisa de 2 células para fechar o vazamento.", "Needs 2 cells to seal the leak.");

        // Prévia (web: previewLines)
        public static readonly Txt PreviaSemContato = T("Sem contato com a Terra: sem previsão hoje.", "No contact with Earth: no forecast today.");
        public static readonly Txt PreviaO2Igual = T("O oxigênio se mantém.", "Oxygen holds steady.");
        public static readonly Txt PreviaO2Sobe = T("O oxigênio sobe {0}.", "Oxygen rises {0}.");
        public static readonly Txt PreviaO2Cai = T("O oxigênio cai {0}.", "Oxygen drops {0}.");
        public static readonly Txt PreviaAbrigo2 = T("Se a erupção vier, a tripulação fica protegida.", "If the flare hits, the crew is protected.");
        public static readonly Txt PreviaAbrigo1 = T("Se a erupção vier, o abrigo barra só metade da radiação.", "If the flare hits, the shelter blocks only half the radiation.");
        public static readonly Txt PreviaAbrigo0 = T("Se a erupção vier, a tripulação fica exposta.", "If the flare hits, the crew is exposed.");
        public static readonly Txt PreviaCiencia = T("Ciência: +{0}.", "Science: +{0}.");
        public static readonly Txt PreviaColheita = T("A estufa dá colheita hoje.", "The greenhouse gives a harvest today.");
        public static readonly Txt PreviaCresce = T("A estufa cresce. Faltarão {0} células.", "The greenhouse grows. {0} more cells to go.");
        public static readonly Txt PreviaComidaAcabando = T("A comida está acabando!", "Food is running out!");
        public static readonly Txt PreviaGuardadas = T("{0} célula(s) ficam guardadas para amanhã.", "{0} cell(s) stay saved for tomorrow.");

        // ---------- Resolução do dia (web: resolve) ----------
        public static readonly Txt ArTotal = T("Ar e água com força total: o oxigênio subiu 1.", "Air & water at full power: oxygen rose 1.");
        public static readonly Txt ArLigado = T("Ar e água ligados: o oxigênio se manteve.", "Air & water on: oxygen held steady.");
        public static readonly Txt ArPouca = T("Ar e água com pouca energia: o oxigênio caiu 1.", "Air & water low on power: oxygen dropped 1.");
        public static readonly Txt ArSem = T("Ar e água sem energia: o oxigênio caiu 2.", "Air & water unpowered: oxygen dropped 2.");
        public static readonly Txt VazamentoConsertado = T("Vazamento consertado! Custou 2 células.", "Leak sealed! It cost 2 cells.");
        public static readonly Txt VazamentoContinua = T("O vazamento continua: mais 1 de oxigênio perdido.", "The leak continues: 1 more oxygen lost.");
        public static readonly Txt Colheita = T("Colheita! A estufa deu comida para 4 dias e 1 de oxigênio.", "Harvest! The greenhouse gave 4 days of food and 1 oxygen.");
        public static readonly Txt MomentoColheita = T("A estufa deu comida e ar. Plantar cedo valeu a espera.", "The greenhouse gave food and air. Planting early paid off.");
        public static readonly Txt EstufaCresceu = T("A estufa cresceu. Faltam {0} células para colher.", "The greenhouse grew. {0} more cells until harvest.");
        public static readonly Txt LabRendeu = T("O laboratório rendeu {0} estrela(s) de ciência.", "The lab earned {0} science star(s).");
        public static readonly Txt ErupcaoProtegeu = T("A erupção veio e o abrigo protegeu quem estava dentro.", "The flare hit and the shelter protected everyone inside.");
        public static readonly Txt ErupcaoMeia = T("A erupção veio. O abrigo com meia energia barrou só parte: radiação +{0}.", "The flare hit. The half-powered shelter blocked only part: radiation +{0}.");
        public static readonly Txt ErupcaoSem = T("A erupção veio e o abrigo estava sem energia: radiação +{0}.", "The flare hit and the shelter had no power: radiation +{0}.");
        public static readonly Txt MomentoAbrigoFraco = T("Erupção solar com o abrigo fraco. A radiação subiu {0}.", "Solar flare with a weak shelter. Radiation rose {0}.");
        public static readonly Txt MomentoAbrigoLigado = T("Erupção solar, e o abrigo ligado protegeu todo mundo.", "Solar flare, and the powered shelter protected everyone.");
        public static readonly Txt FalsoComAbrigo = T("O alerta não se confirmou. As {0} célula(s) do abrigo não fizeram falta hoje, mas custaram.", "The alert did not come true. The {0} shelter cell(s) were not needed today, but they had a cost.");
        public static readonly Txt FalsoSemAbrigo = T("O alerta não se confirmou. Você arriscou e deu certo desta vez.", "The alert did not come true. You took the risk and it worked this time.");
        public static readonly Txt CelulasPerdidas = T("{0} célula(s) ficaram sem uso e se perderam.", "{0} cell(s) were left unused and lost.");
        public static readonly Txt MomentoO2Caiu = T("O oxigênio caiu {0} em um dia só.", "Oxygen dropped {0} in a single day.");
        public static readonly Txt MomentoSombra = T("Dia de sombra: os painéis renderam pouco.", "Shadow day: the panels gave little.");

        // ---------- O dia acontece ----------
        public static readonly Txt AnimComeco = T("O dia começou. A energia corre para os módulos...", "The day has begun. Energy flows to the modules...");
        public static readonly Txt AnimErupcaoExposta = T("A erupção chegou e pegou a base desprotegida!", "The flare arrived and caught the base unprotected!");
        public static readonly Txt AnimErupcaoSegurou = T("A erupção chegou, e o abrigo segurou!", "The flare arrived, and the shelter held!");
        public static readonly Txt AnimFalso = T("Ufa! O alerta não se confirmou.", "Phew! The alert did not come true.");
        public static readonly Txt AnimColheita = T("Dia de colheita na estufa!", "Harvest day in the greenhouse!");
        public static readonly Txt AnimSombra = T("Dia de sombra: a base vive do que guardou.", "Shadow day: the base lives on what it saved.");
        public static readonly Txt AnimTranquilo = T("Dia tranquilo na base.", "A quiet day at the base.");

        // ---------- Resultado ----------
        public static readonly Txt FimDoDia = T("Fim do dia {0}", "End of day {0}");
        public static readonly Txt Igual = T("igual", "same");
        public static readonly Txt DeAte = T("{0} para {1}", "{0} to {1}");
        public static readonly Txt PorQue = T("Por quê?", "Why?");
        public static readonly Txt AconteceuDeVerdade = T("Isso aconteceu de verdade", "This really happened");
        public static readonly Txt ErupcaoClasse = T("Erupção solar classe {0}", "Solar flare class {0}");
        public static readonly Txt ValeComo = T("No jogo ela vale como erupção {0}.", "In the game it counts as a {0} flare.");
        public static readonly Txt[] Intensidade = { T("fraca", "weak"), T("média", "medium"), T("forte", "strong") };
        public static readonly Txt FonteDonki = T("Fonte: NASA DONKI (CCMC). Dado arquivado, consultado em 13/11/2026. Não é dado ao vivo.", "Source: NASA DONKI (CCMC). Archived data, retrieved on 2026-11-13. Not live data.");
        public static readonly Txt FonteDonkiData = T("Fonte: NASA DONKI (CCMC). Dado arquivado, consultado em {0}. Não é dado ao vivo.", "Source: NASA DONKI (CCMC). Archived data, retrieved on {0}. Not live data.");
        public static readonly Txt SepMedido = T("Primeiro os satélites viram o clarão da erupção. Depois as partículas chegaram e foram medidas: {0}, às {1} (UTC).", "First, satellites saw the flare's flash. Then the particles arrived and were measured: {0}, at {1} (UTC).");
        public static readonly Txt VerRegistroSep = T("Ver o registro das partículas", "See the particle record");
        public static readonly Txt CmeCartao = T("Nuvem de plasma do Sol (CME) de {0}", "Coronal mass ejection (CME) of {0}");
        public static readonly Txt CmeCartaoTexto = T("Ela saiu do Sol a {0} km/s. No jogo, chega no Dia {1}.", "It left the Sun at {0} km/s. In the game, it arrives on Day {1}.");
        public static readonly Txt EtiquetaArquivado = T("Dado arquivado · não é ao vivo", "Archived data · not live");
        public static readonly Txt VerRegistro = T("Ver o registro", "See the record");
        public static readonly Txt ProximoDia = T("Próximo dia", "Next day");
        public static readonly Txt VerRelatorio = T("Ver o relatório da missão", "See the mission report");

        // ---------- Relatório (web: vEnd) ----------
        public static readonly Txt MissaoCompleta = T("Missão completa: 10 dias na Lua!", "Mission complete: 10 days on the Moon!");
        public static readonly Txt MissaoInterrompida = T("Missão interrompida no dia {0}", "Mission cut short on day {0}");
        public static readonly Txt SubCompleta = T("A tripulação está bem e volta para casa.", "The crew is well and heads home.");
        public static readonly Txt SubO2 = T("O oxigênio chegou ao limite. A tripulação voltou para casa em segurança antes da hora.", "Oxygen reached its limit. The crew went home safely ahead of time.");
        public static readonly Txt SubRad = T("A radiação chegou ao limite seguro. A tripulação voltou para casa para se cuidar.", "Radiation reached the safe limit. The crew went home to recover.");
        public static readonly Txt SubComida = T("A comida acabou. A tripulação voltou para casa em segurança antes da hora.", "Food ran out. The crew went home safely ahead of time.");
        public static readonly Txt DicaFimO2 = T("E se você mantivesse 2 células no ar e água e saísse menos da base?", "What if you kept 2 cells on air & water and went outside less often?");
        public static readonly Txt DicaFimRad = T("E se você ligasse o abrigo nos alertas fortes, mesmo perdendo ciência?", "What if you powered the shelter on strong alerts, even if it costs science?");
        public static readonly Txt DicaFimComida = T("E se você ligasse a estufa mais cedo? Planta demora para dar comida.", "What if you powered the greenhouse earlier? Plants take time to give food.");
        public static readonly Txt DicaFimRadAlta = T("A radiação ficou alta. E se o abrigo ficasse ligado nos alertas fortes?", "Radiation ended up high. What if the shelter stayed on during strong alerts?");
        public static readonly Txt DicaFim3Estrelas = T("Para 3 estrelas, faça {0} de ciência com pouca radiação. Vale arriscar nos alertas fracos?", "For 3 stars, reach {0} science with low radiation. Is it worth the risk on weak alerts?");
        public static readonly Txt DicaFimExcelente = T("Missão excelente! Será que dá certo de novo com outros eventos?", "Excellent mission! Will it work again with different events?");
        public static readonly Txt EventosMudam = T(" Os eventos mudam a cada partida.", " Events change every game.");
        public static readonly Txt OQueMaisPesou = T("O que mais pesou", "What mattered most");
        public static readonly Txt ParaProxima = T("Para a próxima missão", "For the next mission");
        public static readonly Txt DiaN = T("Dia {0}", "Day {0}");
        public static readonly Txt EstrelasDe3 = T("Nota: {0} de 3 estrelas", "Rating: {0} of 3 stars");
        public static readonly Txt GraficoTitulo = T("A missão dia a dia", "The mission day by day");

        // ---------- Créditos (web: vCredits) ----------
        public static readonly Txt DadosNasa = T("Dados e fatos da NASA", "NASA data and facts");
        public static readonly Txt NumeroDeJogo = T("O que é número de jogo", "What is a game value");
        public static readonly Txt NumeroDeJogoTexto = T("Células de energia, pontos de oxigênio, de radiação e de comida, estrelas e dias de sombra são valores de jogo, não medições. Os alertas fortes e fracos e os alarmes falsos também são regra de jogo, não previsões reais. A classe da erupção (C, M, X) é a publicada pelo DONKI; tratar C como fraca, M como média e X como forte é escolha nossa. Os custos e as chances dos eventos do dia, a contagem da corrida até o abrigo e os 2 dias até a nuvem do Sol chegar também são valores de jogo.", "Energy cells, oxygen, radiation and food points, stars and shadow days are game values, not measurements. Strong and weak alerts and false alarms are also game rules, not real forecasts. The flare class (C, M, X) is the one DONKI publishes; treating C as weak, M as medium and X as strong is our own choice. The costs and chances of the daily events, the countdown to the shelter and the 2 days for the cloud from the Sun to arrive are game values too.");
        public static readonly Txt UsoDeIA = T("Uso de IA", "Use of AI");
        public static readonly Txt UsoDeIATexto = T("O código e a arte deste jogo foram feitos com ajuda de IA. Este jogo não é um produto da NASA e não tem o endosso dela.", "The code and art of this game were made with AI assistance. This game is not a NASA product and is not endorsed by NASA.");
        public static readonly Txt Tipografia = T("Fontes de texto", "Typefaces");
        public static readonly Txt TipografiaTexto = T("Barlow Semi Condensed (The Barlow Project Authors) e Nunito (The Nunito Project Authors), usadas com a licença SIL Open Font License 1.1.", "Barlow Semi Condensed (The Barlow Project Authors) and Nunito (The Nunito Project Authors), used under the SIL Open Font License 1.1.");
        public static readonly Txt OutrosRecursos = T("Outros recursos", "Other resources");
        public static readonly Txt OutrosRecursosTexto = T("Motor Unity 2022.3. O céu de estrelas é gerado por código da equipe. Código e conteúdo originais sob licença Apache 2.0. Lista completa de referências no arquivo README do repositório.", "Unity 2022.3 engine. The star sky is generated by the team's code. Original code and content under the Apache 2.0 license. Full list of references in the repository README.");

        /// <summary>Fontes da NASA cujos fatos aparecem no jogo (nome, link, o que se usa).</summary>
        public static readonly (string nome, string url, Txt uso)[] FontesNasa =
        {
            ("NASA DONKI (CCMC), Space Weather Database Of Notifications, Knowledge, Information", "https://ccmc.gsfc.nasa.gov/tools/DONKI/",
                T("As 20 erupções solares do jogo, com data e classe reais (catálogo FLR, consultado em 13/11/2026), as partículas medidas depois delas (SEP) e as nuvens de plasma (CME), consultadas em 15/11/2026. Dado arquivado. A classe real decide com que força a radiação atinge a base.", "The game's 20 solar flares, with real dates and classes (FLR catalog, retrieved on 2026-11-13), the particles measured after them (SEP) and the plasma clouds (CME), retrieved on 2026-11-15. Archived data. The real class decides how hard radiation hits the base.")),
            ("NASA Science, 9 Phenomena NASA Astronauts Will Encounter at Moon's South Pole", "https://science.nasa.gov/solar-system/moon/phenomena-nasa-astronauts-will-encounter-at-moons-south-pole/",
                T("Sol baixo andando pelo horizonte, sombras longas e céu preto de dia.", "A low Sun travelling along the horizon, long shadows and a black daytime sky.")),
            ("NASA SVS 4944, Earth and Sun from the Moon's South Pole", "https://svs.gsfc.nasa.gov/4944",
                T("O Sol rente ao horizonte e a Terra vista do polo sul.", "The Sun skimming the horizon and Earth seen from the south pole.")),
            ("NASA Science, How Does Webb See Back in Time?", "https://science.nasa.gov/mission/webb/science-overview/science-explainers/how-does-webb-see-back-in-time/",
                T("A luz leva cerca de 1,3 segundo da Lua à Terra.", "Light takes about 1.3 seconds from the Moon to Earth.")),
            ("NASA Human Research Program, Hazard: Distance From Earth", "https://www.nasa.gov/hrp/hazard-distance-from-earth/",
                T("Atraso de comunicação de até 20 minutos em Marte.", "A communication delay of up to 20 minutes at Mars.")),
            ("NASA NTRS 20210022066, Long-Duration Lunar Safe Haven", "https://ntrs.nasa.gov/citations/20210022066",
                T("Estudo de conceito: abrigo coberto de regolito contra a radiação.", "Concept study: a regolith-covered shelter against radiation.")),
            ("NASA, Astronaut Exercise", "https://www.nasa.gov/missions/station/iss-research/astronaut-exercise/",
                T("Astronautas fazem até 10 horas de exercício por semana na Estação Espacial.", "Astronauts exercise for up to 10 hours a week on the Space Station.")),
            ("NASA Space Station blog, Canadarm2 captures Cygnus XL", "https://www.nasa.gov/blogs/spacestation/2026/04/13/canadarm2-reaches-out-and-captures-cygnus-xl-cargo-craft/",
                T("Captura de naves de carga com o braço robótico.", "Capturing cargo ships with the robotic arm.")),
            ("NASA blog, Station Crew Works Robotics, Research as Artemis II Launch Preps Continue", "https://www.nasa.gov/blogs/general-blog/2026/04/01/station-crew-works-robotics-research-as-artemis-ii-launch-preps-continue/",
                T("Treino de captura com o Canadarm2 a cada 60 dias.", "Capture practice with Canadarm2 every 60 days.")),
            ("NASA, NASA's Artemis Base Camp on the Moon Will Need Light, Water, Elevation", "https://www.nasa.gov/humans-in-space/nasas-artemis-base-camp-on-the-moon-will-need-light-water-elevation/",
                T("Evento da Terra: a base fica no lado voltado para a Terra para falar por rádio.", "Earth event: the base sits on the Earth-facing side so it can talk by radio.")),
            ("NASA, Shrinking Moon Causing Moonquakes and Faults Near Lunar South Pole", "https://www.nasa.gov/solar-system/moon/shrinking-moon-causing-moonquakes-and-faults-near-lunar-south-pole/",
                T("Evento do tremor: o tremor raso mais forte registrado pela Apollo foi perto do polo sul.", "Quake event: the strongest shallow moonquake recorded by Apollo was near the south pole.")),
            ("NASA Meteoroid Environment Office, Lunar Impact Monitoring", "https://www.nasa.gov/meteoroid-environment-office/lunar-impact-monitoring/",
                T("Evento do micrometeorito: a NASA observa os clarões de impactos de meteoroides na Lua.", "Micrometeorite event: NASA watches the flashes of meteoroid impacts on the Moon.")),
            ("NASA Science, Moon Dust", "https://science.nasa.gov/moon/moon-dust/",
                T("Evento da poeira: a poeira lunar gruda nos trajes e irritou os astronautas da Apollo 17.", "Dust event: Moon dust sticks to suits and made the Apollo 17 astronauts sore.")),
            ("NASA Science, 10 Cool Things NASA's Lunar Reconnaissance Orbiter is Teaching Us About the Moon", "https://science.nasa.gov/solar-system/moon/10-cool-things-nasas-lunar-reconnaissance-orbiter-is-teaching-us-about-the-moon/",
                T("Eventos do frio e do sono: -248 °C no fundo das crateras e mais de 200 dias de luz seguida no polo sul.", "Cold and sleep events: -248 °C at crater bottoms and more than 200 days of light in a row at the south pole.")),
            ("NASA Science, LCROSS", "https://science.nasa.gov/mission/lcross/",
                T("Evento do gelo: a missão LCROSS achou gelo de água numa cratera do polo sul.", "Ice event: the LCROSS mission found water ice in a south pole crater.")),
            ("NASA Human Research Program, Hazard: Isolation and Confinement", "https://www.nasa.gov/hrp/hazard-isolation-and-confinement/",
                T("Eventos do sono e da saudade: isolamento, perda de sono e desempenho da tripulação.", "Sleep and homesickness events: isolation, sleep loss and crew performance.")),
            ("NASA, Zinnias from Space! NASA Studies the Multiple Benefits of Gardening", "https://www.nasa.gov/humans-in-space/zinnias-from-space-nasa-studies-the-multiple-benefits-of-gardening/",
                T("Evento do fungo: as zínias da estufa Veggie pegaram mofo na Estação Espacial.", "Fungus event: the zinnias in the Veggie greenhouse got moldy on the Space Station.")),
            ("NASA OSDR, OSD-268, Microbiological and nutritional analysis of lettuce crops grown on the ISS", "https://osdr.nasa.gov/bio/repo/data/studies/OSD-268",
                T("Evento do fungo: estudo dos micróbios nas alfaces da mesma estufa.", "Fungus event: a study of the microbes on lettuce from the same greenhouse.")),
        };

        // ---------- Menu de minijogos ----------
        public static readonly Txt TesteMinijogos = T("Teste de minigames", "Minigame test");
        public static readonly Txt TesteMinijogosTexto = T("Escolha um minigame para jogar direto, sem passar pelo jogo. Cada partida começa com o oxigênio cheio.", "Pick a minigame to play it directly, without going through the game. Each run starts with full oxygen.");
        public static readonly Txt CadeteMaisTempo = T("Cadete (mais tempo)", "Cadet (more time)");

        // ---------- Tutorial do Dia 1 (web: TIPS) ----------
        public static readonly Txt ControleDaMissao = T("Controle da missão", "Mission control");
        public static readonly Txt DicaTutBoletim = T("Todo dia começa com o boletim. Ele mostra quanta energia a base vai ter e se o Sol está calmo. Hoje está tudo tranquilo.", "Every day starts with the briefing. It shows how much energy the base will have and whether the Sun is calm. All quiet today.");
        public static readonly Txt DicaTutAteTarefa = T("Leve o astronauta até a estação marcada: é lá que fica a tarefa de hoje. Siga a seta.", "Take the astronaut to the marked station: that is where today's task is. Follow the arrow.");
        public static readonly Txt DicaTutEscolha = T("Toda tarefa tem um custo, escrito em cada botão. Você pode sair para buscar pedras ou ficar na base: as duas escolhas custam alguma coisa.", "Every task has a cost, written on each button. You can go out for rocks or stay at base: both choices cost something.");
        public static readonly Txt DicaTutMinijogo = T("A barra de tempo é o ar do traje. Arraste as pedras para a caixa e aperte VOLTAR antes que ela acabe.", "The time bar is the air in the suit. Drag the rocks into the box and press BACK before it runs out.");
        public static readonly Txt DicaTutConcluida = T("Viu? Cada escolha muda os medidores lá em cima. Na Lua nada é de graça.", "See? Each choice changes the meters up top. On the Moon nothing is free.");
        public static readonly Txt DicaTutAteHub = T("Agora volte ao console de energia, no centro do hub.", "Now head back to the energy console, in the middle of the hub.");
        public static readonly Txt DicaTutDistribuir = T("Esta é a decisão mais importante do dia. Use + e - para mandar células a cada módulo. Ar e água com 2 células seguram o oxigênio. O resto é com você: comida, ciência ou guardar para depois?", "This is the most important decision of the day. Use + and - to send cells to each module. Air & water with 2 cells hold oxygen steady. The rest is up to you: food, science or saving for later?");
        public static readonly Txt DicaTutResultado = T("Aqui você vê o que mudou e por quê. A partir de amanhã podem chegar alertas de erupção solar. Boa missão!", "Here you see what changed and why. From tomorrow on, solar flare alerts may arrive. Have a good mission!");
    }
}
