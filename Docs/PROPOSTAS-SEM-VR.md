# Propostas de jogo

Três propostas para o desafio descrito em [DESAFIO-2026-11-14.md](DESAFIO-2026-11-14.md). Cada uma usa um dos
três formatos que o enunciado sugere (sobrevivência com recursos, narrativa de decisões e
painel de controle), então são caminhos realmente diferentes, não variações do mesmo jogo.

Todas respeitam as mesmas regras de base:

- público de 7 a 12 anos, com texto curto e termos explicados na hora;
- no máximo três recursos centrais na tela;
- nenhuma escolha sem custo;
- dados reais da NASA com a fonte visível dentro do jogo;
- fracasso que ensina, sem tela de "game over" punitiva;
- partida curta (5 a 12 minutos), jogável no navegador via build WebGL.

> **Sobre os números.** Valores de jogo (células de energia, turnos, pontos) são ponto de
> partida para balancear, não dados reais. Fatos reais citados (eventos solares, atraso de
> sinal, medições em Marte) vêm do meu conhecimento e precisam ser conferidos nos datasets
> oficiais. A lista de recursos saiu em 13/11/2026 (ver [DESAFIO-2026-11-14.md](DESAFIO-2026-11-14.md), seção 6.1)
> e o que já foi conferido está em [DADOS-NASA-2026-11-14.md](DADOS-NASA-2026-11-14.md).

Escrito em 10/11/2026, antes da decisão de 14/11/2026 pelo plano "Dez Dias na Lua"
(sobrevivência por turnos no polo sul da Lua, próxima da Proposta A). Nomes são provisórios.

---

## Proposta A: Noite na Base

**Formato:** sobrevivência por turnos com gerenciamento de recursos.
**Local:** polo sul da Lua, região alvo do programa Artemis.
**Idade ideal:** toda a faixa de 7 a 12.

### Conceito

A criança é a comandante júnior de uma base com quatro astronautas. A missão dura 10 dias.
Todo dia a base gera uma quantidade limitada de energia, e a criança decide para onde ela
vai. Não dá para ligar tudo.

### Como se joga

Cada dia é um turno com quatro passos:

1. **Boletim da manhã.** Mostra quanta luz solar a base vai receber hoje e a previsão de
   clima espacial ("Sol calmo" ou "alerta de erupção").
2. **Distribuir energia.** A criança arrasta células de energia para os módulos. Em um dia
   normal são 6 células; em dia de sombra, 3.
3. **O dia acontece.** Animação curta mostrando cada módulo funcionando ou parado.
4. **Resultado.** Os medidores mudam e aparece uma frase explicando o porquê.

### Módulos

| Módulo | O que faz quando recebe energia | O que acontece sem energia |
| --- | --- | --- |
| Ar e água | Mantém o oxigênio da base | A reserva de oxigênio cai |
| Estufa | Faz as plantas crescerem: comida em alguns dias e um pouco de oxigênio | As plantas param e podem morrer |
| Abrigo | Protege a tripulação da radiação | A tripulação fica exposta se houver erupção |
| Laboratório | Gera estrelas de ciência, o objetivo da missão | Nenhuma descoberta no dia |
| Bateria | Guarda células para os dias de sombra | Nada sobra para depois |

### Recursos centrais

- **Energia:** o orçamento do dia.
- **Oxigênio:** a reserva da base.
- **Radiação:** a dose acumulada pela tripulação.

Comida e ciência aparecem como contadores secundários.

### Trade-offs que a criança vive

- Ligar o laboratório dá estrelas, mas tira energia do ar ou da estufa.
- A estufa custa energia por vários dias antes de devolver alguma coisa.
- Mandar todos para o abrigo protege da erupção, mas ninguém trabalha naquele dia.
- Guardar energia na bateria salva os dias de sombra, mas atrasa a ciência de hoje.
- O alerta de erupção nem sempre se confirma: proteger-se "à toa" também tem custo.

### Eventos

Erupção solar, período de sombra, vazamento de ar, falha em painel solar e descoberta
científica (uma chance de ganhar estrelas extras gastando oxigênio em uma saída externa).
A ordem muda a cada partida.

### Dados da NASA

- **DONKI:** as erupções do jogo são eventos solares reais. Cada partida sorteia um trecho
  real do histórico. Depois do evento, um cartão mostra: "Isso aconteceu de verdade em
  [data]: erupção classe [X]".
- **OSDR:** experimentos de cultivo de plantas no espaço servem de base para o tempo de
  crescimento da estufa e para um cartão explicando o efeito da radiação no corpo.
- **Artemis:** dados de iluminação do polo sul lunar definem os dias de luz e de sombra.

### Fim da partida

- **Sucesso:** 10 dias completos. Nota de 1 a 3 estrelas pela ciência feita e pela saúde
  da tripulação.
- **Missão interrompida:** se o oxigênio acaba ou a radiação passa do limite, a tripulação
  volta para casa em segurança. O relatório mostra em que dia a situação virou e sugere
  outra estratégia.

### O que a criança aprende

Energia é limitada e tem que ser dividida; o Sol produz radiação perigosa em momentos
difíceis de prever; plantas dão comida e ar, mas cobram energia e tempo.

### Versão mínima para o hackathon

Uma tela, cinco módulos, 10 turnos, cinco eventos, lista embutida de eventos reais do
DONKI e relatório final.

### Riscos

- Balanceamento: fácil cair em uma estratégia sempre vencedora. Precisa de testes.
- O atraso de comunicação fica de fora, porque na Lua ele é de pouco mais de um segundo.

---

## Proposta B: Sol a Sol

**Formato:** narrativa de decisões com baralho de dilemas.
**Local:** posto em Marte.
**Idade ideal:** 8 a 12 (exige leitura; com narração em áudio atende também os de 7).

### Conceito

A criança comanda uma missão de 12 sols em Marte, com três astronautas e um robô
ajudante que explica os termos. Cada sol traz um dilema ilustrado com duas ou três
saídas. O custo de cada saída aparece em ícones **antes** da escolha.

### Como se joga

1. **Abertura do sol.** Foto real de Marte e o clima do dia.
2. **Dilema.** Uma carta com a situação e as opções, cada uma com seus custos.
3. **Consequência.** Os medidores mudam e a história anda. Algumas escolhas voltam a
   cobrar o preço sols depois.

O baralho tem cerca de 30 cartas e cada partida usa 12, então duas partidas nunca são
iguais.

### Mecânica de assinatura: perguntar à Terra

Em qualquer dilema a criança pode pedir ajuda ao controle da missão. Só que o sinal
demora: de 3 a 22 minutos para ir, dependendo da posição dos planetas, e o mesmo para
voltar. No jogo, a resposta só chega no sol seguinte. Em dilema urgente, esperar piora a
situação. A criança aprende que em Marte a tripulação precisa decidir sozinha.

### Recursos centrais

- **Ar e água:** suporte à vida.
- **Energia:** vem dos painéis solares.
- **Saúde da tripulação:** cai com radiação e com fome.

A comida entra como um arco da história: plantar cedo custa água e energia, não plantar
deixa a tripulação sem comida no fim.

### Exemplos de dilema

- **Tempestade de poeira chegando.** Limpar os painéis agora gasta ar em uma saída
  externa; esperar faz a energia cair por vários sols.
- **Alerta de radiação.** Ir para o abrigo perde um sol de trabalho; continuar a coleta
  rende ciência e custa saúde.
- **Estufa com problema.** Consertar gasta energia hoje; ignorar custa comida daqui a
  cinco sols.
- **Rocha rara a 2 km.** Ir buscar gasta ar e energia; ficar perde a descoberta.

### Dados da NASA

- **Missões de superfície de Marte:** fotos reais dos rovers como cenário e medições reais
  de temperatura e pressão no boletim de cada sol.
- **DONKI:** o alerta de radiação é um evento solar real. Um candidato é a erupção de
  setembro de 2017, que foi medida na superfície de Marte pelo rover Curiosity.
- **OSDR:** os efeitos na saúde da tripulação seguem achados reais de pesquisa com
  astronautas, citados em cartões curtos.

### Fim da partida

Três ou quatro finais conforme o estado da missão, do "missão completa" ao "retorno
antecipado". O relatório mostra as três decisões que mais pesaram e convida a tentar
outro caminho.

### O que a criança aprende

Toda decisão tem custo, às vezes só visível dias depois; em Marte não dá para esperar
resposta da Terra; poeira e radiação são ameaças reais do planeta.

### Versão mínima para o hackathon

Doze cartas fixas, três medidores, a mecânica de perguntar à Terra, dois finais e
ilustrações simples sobre fotos reais.

### Riscos

- Carga de leitura para os mais novos. Mitigação: frases curtas, ícones e narração.
- Virar um quiz com resposta certa. Mitigação: nenhuma opção pode ser a melhor em tudo.
- Menos "mão na massa" que as outras propostas: a criança escolhe, não constrói.

---

## Proposta C: Central de Missão

**Formato:** painel de controle em tempo real com pausa.
**Local:** base lunar comandada a partir da sala de controle.
**Idade ideal:** 10 a 12, e uso em sala de aula.

### Conceito

A criança é a diretora de voo júnior. Na tela está o painel da base: três medidores
grandes, uma linha do tempo do Sol e três controles deslizantes de energia. O clima
espacial que atinge a base **é o clima espacial real**, puxado do DONKI.

### Modos de jogo

- **Sol de hoje.** O jogo busca os eventos solares reais dos últimos 30 dias e os toca
  comprimidos em uma partida de cerca de 6 minutos. Cada semana o desafio é outro.
- **Tempestades famosas.** Cenários com eventos históricos embutidos, como as grandes
  tempestades solares de maio de 2024. Funciona sem internet e garante uma partida
  movimentada mesmo quando o Sol real está calmo.

O jogo sempre informa em qual modo está, para nunca apresentar dado arquivado como ao vivo.

### Como se joga

O tempo corre sozinho e pode ser pausado a qualquer momento. A criança move três
controles que dividem 100% da energia entre suporte à vida, escudo e estufa. Alertas
aparecem na linha do tempo com antecedência: há tempo para reagir, mas não para tudo.

### Recursos centrais

- **Energia:** os 100% a dividir.
- **Oxigênio:** depende do suporte à vida e da estufa.
- **Radiação:** sobe quando há evento solar e o escudo está fraco.

### Trade-offs que a criança vive

- Subir o escudo antes da erupção tira energia do ar; subir tarde demais expõe a tripulação.
- A estufa precisa de energia constante: cortar por pouco tempo já atrasa a colheita.
- Manter tudo "no meio" parece seguro, mas não segura uma erupção forte.

### Dados da NASA

- **DONKI:** fonte principal e central da mecânica. Cada alerta do jogo mostra data,
  tipo e intensidade do evento real.
- **OSDR:** o medidor de radiação tem faixas explicadas com base em dosimetria real de
  astronautas.
- **Artemis:** condições do local da base.

### Fim da partida

Relatório com gráfico simples da partida: onde a energia esteve em cada momento e como os
medidores reagiram. Lado a lado, o que a criança fez e o que o Sol fez de verdade.

### O que a criança aprende

O clima espacial existe e muda todo dia; proteger-se tem custo; ler um aviso com
antecedência e agir cedo vale mais que reagir no susto.

### Versão mínima para o hackathon

Modo "Tempestades famosas" com dois cenários embutidos, três medidores, três controles e
relatório final. O modo ao vivo entra só se sobrar tempo.

### Riscos

- Painel pode parecer frio para crianças de 7 a 9 anos.
- Depende de rede: limite de chamadas da API e possíveis bloqueios no navegador.
- É a proposta com mais trabalho de interface.

---

## Comparação

| Critério | A: Noite na Base | B: Sol a Sol | C: Central de Missão |
| --- | --- | --- | --- |
| Formato | Sobrevivência por turnos | Narrativa de decisões | Painel em tempo real |
| Local | Lua | Marte | Lua |
| Idade ideal | 7 a 12 | 8 a 12 | 10 a 12 |
| Clareza do trade-off | Alta | Alta | Média |
| Uso de dados reais | Embutido, bem visível | Embutido, ambientação | Ao vivo, é a mecânica |
| Atraso de comunicação | Fora | Mecânica central | Fora |
| Rejogabilidade | Alta | Média | Alta |
| Esforço de arte | Médio | Médio | Baixo |
| Esforço de programação | Médio | Baixo | Alto |
| Risco principal | Balanceamento | Carga de leitura | Rede e apelo infantil |

## Recomendação

**Proposta A.** É a que melhor cobre a faixa inteira de 7 a 12 anos, deixa o trade-off
visível no gesto mais simples possível (arrastar energia para um módulo) e atende aos
pontos que o enunciado destaca: poucos recursos, nenhuma escolha gratuita, rejogabilidade
e dado real com fonte à mostra.

Duas ressalvas:

- Se a equipe for pequena ou o tempo apertar, a **B** é a mais barata de construir e é a
  única que trata o atraso de comunicação, que consta nos objetivos do desafio.
- O modo "Sol de hoje" da **C** pode virar uma melhoria da A: uma missão diária montada
  com os eventos solares reais mais recentes.
