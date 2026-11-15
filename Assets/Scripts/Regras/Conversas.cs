namespace DezDias
{
    /// <summary>Etapas do dia em que a Lia e o Bip conversam.</summary>
    public enum Momento { Boletim, AteTarefa, Escolha, MinijogoInfo, Concluida, AteHub, Distribuir, Resultado }

    /// <summary>
    /// Conversas da Lia (a engenheira, que anda pela base) e do Bip (o robô ajudante, que explica) em cada etapa de todos
    /// os dias. No Dia 1 com tutorial, as falas são as dicas do tutorial. Nos outros dias, reagem ao que está acontecendo
    /// (alerta de erupção, sombra, vazamento, oxigênio, tarefa de hoje, resultado). Mesmo formato da apresentação:
    /// robo = true é o Bip, false é a Lia. No máximo 3 falas por etapa, para não segurar a criança.
    /// Os fatos citados são de divulgação da NASA (gravidade da Lua, dia lunar, atraso do sinal, reciclagem de água na
    /// Estação Espacial, exercício diário dos astronautas, poeira lunar, silêncio no vácuo).
    /// </summary>
    public static class Conversas
    {
        static Txt T(string pt, string en) => new Txt(pt, en);
        static (bool, Txt) Lia(string pt, string en) => (false, T(pt, en));
        static (bool, Txt) Bip(string pt, string en) => (true, T(pt, en));

        const int O2Baixo = 3;

        /// <summary>Falas da etapa, em ordem. Nunca vazio.</summary>
        public static (bool robo, Txt fala)[] Para(Missao m, Momento mo, bool tutorial)
        {
            if (m == null || m.Hoje == null) return new[] { Lia("Vamos lá!", "Let's go!") };
            if (tutorial && m.Dia == 1)
            {
                var t = Tutorial(m, mo);
                if (t != null) return t;
            }
            switch (mo)
            {
                case Momento.Boletim: return Boletim(m);
                case Momento.AteTarefa: return AteTarefa(m);
                case Momento.Escolha: return Escolha(m);
                case Momento.MinijogoInfo: return new[] { Bip("Primeira vez nesta tarefa? Olha a mão mostrando o gesto.", "First time on this task? Watch the hand show the move.") };
                case Momento.Concluida: return Concluida(m);
                case Momento.AteHub: return AteHub(m);
                case Momento.Distribuir: return Distribuir(m);
                default: return Resultado(m);
            }
        }

        // ------------------------------------------------------------------ Dia 1 (tutorial)

        static (bool, Txt)[] Tutorial(Missao m, Momento mo)
        {
            switch (mo)
            {
                case Momento.Boletim:
                    return new[] {
                        Bip("Todo dia começa com o boletim. Ele mostra quanta energia a base vai ter e se o Sol está calmo.", "Every day starts with the briefing. It shows how much energy the base will have and whether the Sun is calm."),
                        Lia("Hoje está tudo tranquilo. Bom dia para aprender!", "All quiet today. A good day to learn!"),
                    };
                case Momento.AteTarefa:
                    return new[] {
                        (false, Textos.FalasAbertura[0]),
                        (false, Textos.FalasAbertura[1]),
                        (false, Textos.FalasAbertura[2]),
                        Bip("Leve a Lia até a estação marcada seguindo a seta no chão.", "Take Lia to the marked station by following the arrow on the floor."),
                    };
                case Momento.Escolha:
                    if (m.Hoje.Tarefa != Tarefas.Amostra) return null;
                    return new[] {
                        Bip("Toda tarefa tem um custo, escrito em cada botão.", "Every task has a cost, written on each button."),
                        Lia("Posso sair para buscar pedras ou ficar na base. As duas escolhas custam alguma coisa.", "I can go out for rocks or stay at base. Both choices cost something."),
                    };
                case Momento.MinijogoInfo:
                    if (m.Hoje.Tarefa != Tarefas.Amostra) return null;
                    return new[] {
                        Bip("A barra de tempo é o ar do traje.", "The time bar is the air in the suit."),
                        Lia("Eu arrasto as pedras para a caixa e aperto VOLTAR antes que ela acabe!", "I drag the rocks into the box and press BACK before it runs out!"),
                    };
                case Momento.Concluida:
                    return new[] {
                        Bip("Viu? Cada escolha muda os medidores lá em cima.", "See? Each choice changes the meters up top."),
                        Lia("Na Lua nada é de graça!", "On the Moon nothing is free!"),
                    };
                case Momento.AteHub:
                    return new[] {
                        Bip("Agora volte ao console de energia, no centro do hub.", "Now head back to the energy console, in the middle of the hub."),
                        Lia("É lá que a gente decide para onde vai a energia.", "That is where we decide where the energy goes."),
                    };
                case Momento.Distribuir:
                    return new[] {
                        Bip("Esta é a decisão mais importante do dia. Use + e - para mandar células a cada módulo.", "This is the most important decision of the day. Use + and - to send cells to each module."),
                        Lia("Ar e água com 2 células seguram o oxigênio. O resto é com você: comida, ciência ou guardar para depois?", "Air & water with 2 cells hold oxygen steady. The rest is up to you: food, science or saving for later?"),
                    };
                case Momento.Resultado:
                    return new[] {
                        Bip("Aqui você vê o que mudou e por quê.", "Here you see what changed and why."),
                        Lia("A partir de amanhã podem chegar alertas de erupção solar. Boa missão!", "From tomorrow on, solar flare alerts may arrive. Have a good mission!"),
                    };
            }
            return null;
        }

        // ------------------------------------------------------------------ Boletim

        static (bool, Txt)[] Boletim(Missao m)
        {
            var al = m.AlertaDe(m.Dia);
            if (al != null && al.Forte)
                return new[] {
                    Bip("Alerta forte! O Sol pode soltar uma erupção hoje. Se vier, o abrigo protege a tripulação.", "Strong alert! The Sun may flare today. If it does, the shelter protects the crew."),
                    Lia("Então nada de passeio longo lá fora hoje, né?", "So no long walks outside today, right?"),
                };
            if (al != null)
                return new[] {
                    Bip("Alerta fraco de erupção. Pode não ser nada... ou pode ser alguma coisa.", "Weak flare alert. It might be nothing... or it might be something."),
                    Lia("Ligo o abrigo ou arrisco? Você decide, comandante.", "Do I power the shelter or take the risk? Your call, commander."),
                };
            if (m.Vazamento)
                return new[] {
                    Lia("Ainda tem ar escapando pelo furo...", "Air is still escaping through the hole..."),
                    Bip("Cada dia com vazamento custa oxigênio. Consertar logo sai mais barato.", "Every day with a leak costs oxygen. Fixing it soon is cheaper."),
                };
            if (m.EhSombra(m.Dia))
                return new[] {
                    Lia("Hoje a base fica na sombra. Os painéis quase não pegam luz.", "Today the base is in shadow. The panels barely catch any light."),
                    Bip("É agora que a bateria guardada faz diferença.", "This is when the saved battery makes a difference."),
                };
            if (!m.Contato)
                return new[] { Bip("Sem contato com a Terra hoje. Vamos ter que decidir sozinhos.", "No contact with Earth today. We will have to decide on our own.") };
            if (m.O2 <= O2Baixo)
                return new[] {
                    Lia("O oxigênio está baixo. Hoje o ar vem primeiro.", "Oxygen is low. Today, air comes first."),
                    Bip("Sair da base gasta o ar do traje. Melhor ficar dentro.", "Going outside uses the suit's air. Better to stay in."),
                };
            switch (m.Dia)
            {
                case 2: return new[] { Bip("Na Lua, um dia inteiro, de um nascer do Sol até o outro, dura cerca de 29 dias da Terra.", "On the Moon, a full day, from one sunrise to the next, lasts about 29 Earth days."), Lia("Ainda bem que a gente conta os dias pelo relógio!", "Good thing we count days by the clock!") };
                case 3: return new[] { Lia("Dormi mal! Aqui eu peso só um sexto do que peso na Terra e fico quicando na cama.", "I slept badly! Here I weigh only one sixth of my weight on Earth, so I keep bouncing in bed.") };
                case 4: return new[] { Bip("Curiosidade: uma mensagem leva cerca de 1,3 segundo para ir da Lua até a Terra.", "Fun fact: a message takes about 1.3 seconds to travel from the Moon to Earth.") };
                case 5: return new[] { Lia("Metade da missão! Cinco dias na Lua, cinco pela frente.", "Halfway there! Five days on the Moon, five to go.") };
                case 6: return new[] { Bip("A poeira da Lua é fina e afiada: aqui não tem vento nem água para arredondar os grãos.", "Moon dust is fine and sharp: there is no wind or water here to smooth the grains.") };
                case 7: return new[] { Lia("Sabia que lá fora é silêncio total? Sem ar, o som não tem por onde ir.", "Did you know it is totally silent outside? Without air, sound has nothing to travel through.") };
                case 8: return new[] { Bip("Faltam três dias. As escolhas de agora pesam mais.", "Three days left. The choices you make now matter more.") };
                case 9: return new[] { Lia("Penúltimo dia! Amanhã é a reta final.", "Second to last day! Tomorrow is the home stretch.") };
                case 10: return new[] { Bip("Último dia da missão. Vamos fazer valer!", "Last day of the mission. Let's make it count!") };
                default: return new[] { Lia("Bom dia, comandante! Vamos ver o que o dia traz.", "Good morning, commander! Let's see what the day brings.") };
            }
        }

        // ------------------------------------------------------------------ Indo até a tarefa

        static (bool, Txt)[] AteTarefa(Missao m)
        {
            (bool, Txt)[] f;
            switch (m.Hoje.Tarefa)
            {
                case Tarefas.Amostra: f = new[] { Lia("Hoje tem pedra interessante lá fora!", "There are interesting rocks outside today!"), Bip("Lá fora o ar vem do traje. Quanto mais longe, mais oxigênio gasto.", "Outside, air comes from the suit. The farther you go, the more oxygen it costs.") }; break;
                case Tarefas.Poeira: f = new[] { Lia("Os painéis solares estão cobertos de poeira de novo.", "The solar panels are covered in dust again."), Bip("Painel sujo pega menos luz e gera menos energia.", "A dirty panel catches less light and makes less energy.") }; break;
                case Tarefas.Estufa: f = new[] { Lia("Vamos ver as plantas? Elas dão comida e um pouco de ar.", "Shall we check the plants? They give food and a little air."), Bip("Planta demora para crescer. Cuidar cedo rende mais depois.", "Plants take time to grow. Caring for them early pays off later.") }; break;
                case Tarefas.Agua: f = new[] { Lia("O reciclador de água está pedindo ajuda.", "The water recycler needs help."), Bip("Na Estação Espacial, quase toda a água é reaproveitada. Até o suor!", "On the Space Station, almost all the water is reused. Even sweat!") }; break;
                case Tarefas.Upload: f = new[] { Lia("Hora de mandar os dados da missão para a Terra.", "Time to send the mission data to Earth."), Bip("Pacote por pacote, no ritmo certo.", "Packet by packet, at the right rhythm.") }; break;
                case Tarefas.Filtros: f = new[] { Lia("Os filtros de ar estão ficando entupidos.", "The air filters are getting clogged."), Bip("Filtro limpo tira o gás carbônico que a gente solta ao respirar.", "A clean filter removes the carbon dioxide we breathe out.") }; break;
                case Tarefas.Exercicio: f = new[] { Lia("Hora da esteira! Com pouca gravidade, músculo e osso enfraquecem.", "Treadmill time! With little gravity, muscles and bones get weaker."), Bip("Os astronautas da Estação Espacial treinam cerca de duas horas por dia.", "Astronauts on the Space Station exercise about two hours a day.") }; break;
                case Tarefas.Braco: f = new[] { Lia("Tem carga chegando! Vamos usar o braço robótico.", "Cargo is arriving! Let's use the robotic arm."), Bip("Devagar e com cuidado: errar a garra custa tempo.", "Slowly and carefully: missing with the claw costs time.") }; break;
                case Tarefas.Vazamento: f = new[] { Bip("Alerta! Tem ar escapando por um furo na parede!", "Alert! Air is escaping through a hole in the wall!"), Lia("Vou pegar os remendos, rápido!", "I'll grab the patches, quick!") }; break;
                case Tarefas.Antena: f = new[] { Lia("A antena perdeu o sinal da Terra.", "The antenna lost Earth's signal."), Bip("Sem antena, sem notícias de casa. Vamos apontar de novo.", "No antenna, no news from home. Let's aim it again.") }; break;
                default: f = new[] { Lia("A tarefa de hoje está marcada com a seta. Vamos lá!", "Today's task is marked with the arrow. Let's go!") }; break;
            }
            var d = Tarefas.Def(m.Hoje.Tarefa);
            if (m.AlertaDe(m.Dia) != null && d != null && d.Fora)
                return new[] { f[0], f[f.Length - 1], Bip("E tem alerta de erupção hoje: se for sair, seja rápida.", "And there is a flare alert today: if you go out, be quick.") };
            return f;
        }

        // ------------------------------------------------------------------ Escolha

        static (bool, Txt)[] Escolha(Missao m)
        {
            var d = Tarefas.Def(m.Hoje.Tarefa);
            if (m.Hoje.Tarefa == Tarefas.Vazamento)
                return new[] { Lia("Não dá para esperar: o ar está indo embora!", "We can't wait: the air is leaking away!") };
            if (!Tarefas.PodeFazer(m))
                return new[] { Bip("Com tão pouco oxigênio, sair agora é perigoso.", "With so little oxygen, going out now is dangerous.") };
            if (d != null && d.Fora)
                return new[] { Lia("Lá fora eu respiro o ar do traje. Será que vale a pena hoje?", "Outside I breathe the suit's air. Is it worth it today?") };
            return new[] { Bip("Fazer a tarefa custa alguma coisa, mas deixar para depois também.", "Doing the task costs something, but leaving it for later does too.") };
        }

        // ------------------------------------------------------------------ Tarefa concluída

        static (bool, Txt)[] Concluida(Missao m)
        {
            if (m.Hoje.TempoEsgotou)
                return new[] { Lia("Ih, o tempo do dia acabou!", "Oops, the day ran out of time!") };
            switch (m.Dia % 4)
            {
                case 0: return new[] { Lia("Pronto! Uma coisa a menos.", "Done! One less thing to worry about."), Bip("Agora vamos ao console decidir a energia.", "Now let's go to the console and decide the energy.") };
                case 1: return new[] { Lia("Ufa, essa deu trabalho!", "Phew, that was hard work!"), Bip("Olhe os medidores lá em cima: veja o que mudou.", "Look at the meters up top: see what changed.") };
                case 2: return new[] { Bip("Tarefa resolvida. Próxima parada: o hub.", "Task handled. Next stop: the hub.") };
                default: return new[] { Lia("Feito! Bora voltar para o hub.", "Done! Let's head back to the hub.") };
            }
        }

        // ------------------------------------------------------------------ Voltando ao hub

        static (bool, Txt)[] AteHub(Missao m)
        {
            var al = m.AlertaDe(m.Dia);
            var lia = Lia("De volta ao hub!", "Back to the hub!");
            if (al != null && al.Forte) return new[] { lia, Bip("Não esqueça: alerta forte hoje. Pense no abrigo.", "Don't forget: strong alert today. Think about the shelter.") };
            if (m.Vazamento) return new[] { lia, Bip("O vazamento continua aberto. O conserto precisa de energia.", "The leak is still open. The repair needs energy.") };
            if (m.EhSombra(m.Dia)) return new[] { lia, Bip("Pouca luz hoje: cada célula conta.", "Little light today: every cell counts.") };
            if (m.O2 <= O2Baixo + 1) return new[] { lia, Bip("O oxigênio pede pelo menos 2 células no ar e água.", "Oxygen needs at least 2 cells on air & water.") };
            return new[] { lia, Bip("Hora de dividir as células do dia.", "Time to share out today's cells.") };
        }

        // ------------------------------------------------------------------ Distribuir

        static (bool, Txt)[] Distribuir(Missao m)
        {
            var al = m.AlertaDe(m.Dia);
            if (m.O2 <= O2Baixo) return new[] { Lia("Primeiro o ar! Sem oxigênio, nada mais importa.", "Air first! Without oxygen, nothing else matters.") };
            if (al != null && al.Forte) return new[] { Bip("Com alerta forte, o abrigo com 2 células protege a tripulação.", "With a strong alert, the shelter with 2 cells protects the crew.") };
            if (al != null) return new[] { Lia("Alerta fraco... arrisco a ciência ou ligo o abrigo?", "Weak alert... do I risk the science or power the shelter?") };
            if (m.Vazamento) return new[] { Bip("O conserto precisa de 2 células para fechar o vazamento.", "The repair needs 2 cells to seal the leak.") };
            if (m.EhSombra(m.Dia)) return new[] { Bip("Dia de sombra: a bateria guardada pode salvar o dia.", "Shadow day: the saved battery can save the day.") };
            return new[] { Lia("Não dá para ligar tudo. O que é mais importante hoje?", "We can't power everything. What matters most today?") };
        }

        // ------------------------------------------------------------------ Resultado

        static (bool, Txt)[] Resultado(Missao m)
        {
            var r = m.Hoje.Res;
            if (m.Fim == Fim.Completa)
                return new[] { Lia("Conseguimos! Dez dias na Lua!", "We did it! Ten days on the Moon!"), Bip("Bip-bip! Missão cumprida, comandante!", "Beep-beep! Mission accomplished, commander!") };
            if (m.Fim != Fim.Nenhum)
                return new[] { Bip("A missão acabou antes da hora, mas todo mundo volta para casa em segurança.", "The mission ended early, but everyone goes home safely."), Lia("Da próxima vez a gente tenta outro caminho!", "Next time we'll try another way!") };
            if (r != null && r.Erupcao != null)
                return r.DRad > 0
                    ? new[] { Lia("Essa erupção pegou a gente. A radiação subiu.", "That flare caught us. Radiation went up."), Bip("Ela aconteceu de verdade: o cartão mostra a data registrada pela NASA.", "It really happened: the card shows the date NASA recorded.") }
                    : new[] { Bip("A erupção veio, e a radiação não subiu!", "The flare came, and radiation didn't go up!"), Lia("Boa decisão, comandante!", "Good call, commander!") };
            if (r != null && r.FalsoAlarme)
                return new[] { Lia("Alarme falso! O Sol ficou quietinho.", "False alarm! The Sun stayed quiet."), Bip("Às vezes acontece. Prevenir também é uma escolha.", "It happens. Playing it safe is also a choice.") };
            if (r != null && r.Colheita)
                return new[] { Lia("Colheita na estufa! Comida fresquinha!", "Harvest in the greenhouse! Fresh food!") };
            return new[] { Lia("Mais um dia na Lua!", "Another day on the Moon!"), Bip("Amanhã tem mais.", "More tomorrow.") };
        }
    }
}
