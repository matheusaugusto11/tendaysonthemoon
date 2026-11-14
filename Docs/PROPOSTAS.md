# Propostas de jogo em VR

Três propostas em realidade virtual para o desafio descrito em [DESAFIO.md](DESAFIO.md).
As propostas anteriores, sem VR, estão em [PROPOSTAS-SEM-VR.md](PROPOSTAS-SEM-VR.md).

> **Sobre os números e fatos.** Valores de jogo (células, turnos, litros) são ponto de
> partida para balancear. Fatos reais citados (eventos solares, panoramas, atraso de sinal,
> regras de idade dos headsets, pacotes do Unity) vêm do meu conhecimento e precisam ser
> conferidos antes de virar decisão. Os datasets oficiais do desafio saem em 13/11.

Nomes são provisórios.

---

## 1. O conflito entre VR e o desafio

O enunciado pede **barreira de entrada baixa**: jogável no navegador, sem download e sem
conta, para alcançar os avaliadores e os estudantes. A maioria das pessoas não tem headset
VR, e um avaliador não vai instalar um APK no Quest por modo desenvolvedor.

Há também a questão da idade. O público é de 7 a 12 anos, e os fabricantes de headset
costumam recomendar idade mínima acima disso (a Meta, por exemplo, exige 10 anos e conta
gerenciada pelos pais). Isso precisa ser conferido, mas indica que parte do público-alvo
não deveria jogar de headset.

Por isso as três propostas seguem as mesmas regras de plataforma:

1. **WebXR.** O jogo roda no navegador. Quem abre o link no navegador de um headset entra
   em VR com um botão, sem instalar nada.
2. **Modo sem headset obrigatório.** O mesmo link funciona em computador (mouse) e celular
   (toque e giroscópio). O jogo precisa ser completo nesse modo, porque é nele que a
   maioria vai jogar e possivelmente a maioria dos avaliadores.
3. **Lógica separada da entrada.** As regras do jogo (recursos, turnos, eventos) ficam em
   um núcleo que não sabe se o jogador está de headset. VR e tela só mudam a forma de
   interagir. Isso permite testar sem headset e entregar o modo tela mesmo se o VR
   atrasar.

E as mesmas regras de conforto, pensadas para crianças:

- jogador parado, sentado ou em pé, sem andar com o analógico (zero enjoo);
- tudo ao alcance do braço, com interação por pegar, encaixar e apontar;
- sessões de 5 a 10 minutos;
- texto grande e pouco, com narração em áudio e ícones.

E as regras de design que vêm do desafio:

- no máximo três recursos centrais;
- nenhuma escolha sem custo;
- dado real da NASA com a fonte visível dentro do jogo;
- fracasso que ensina, sem game over punitivo.

---

## Proposta A: Mãos na Base

**Formato:** sobrevivência por turnos, com as mãos.
**Local:** módulo de uma base no polo sul da Lua.
**Idade ideal:** 10 a 12 de headset, 7 a 12 na tela.

### Conceito

A criança está dentro do módulo de comando da base, de frente para um painel em semicírculo
com cinco tomadas: ar e água, estufa, abrigo, laboratório e bateria. Toda manhã chega uma
caixa com **células de energia de verdade**, objetos que se pegam com a mão. São 6 em dia
de sol e 3 em dia de sombra. A criança precisa decidir onde encaixar cada uma.

O trade-off fica físico: com as mãos cheias, ela sente que não tem células para tudo.

### Como se joga

1. **Boletim da manhã.** Pela janela do módulo, a criança vê a luz do dia e o Sol. Um rádio
   anuncia a previsão de clima espacial.
2. **Encaixar energia.** Ela pega as células e encaixa nas tomadas. Cada módulo acende e
   mostra o que vai fazer naquele dia.
3. **O dia acontece.** Ao puxar uma alavanca, o dia passa: as plantas da estufa crescem
   diante dela, o laboratório produz amostras e os medidores se mexem.
4. **Resultado.** Um robô ajudante explica em uma frase o que mudou e por quê.

### Recursos centrais

Energia (as células), oxigênio e radiação. Comida e ciência como contadores secundários.

### Trade-offs

- O laboratório gera ciência, mas a célula sai do ar ou da estufa.
- A estufa consome energia por dias antes de dar comida e oxigênio.
- Ir para o abrigo protege da erupção, mas o dia de trabalho se perde.
- Guardar células na bateria salva os dias de sombra, mas atrasa o resto.
- O alerta de erupção nem sempre se confirma.

### Dados da NASA

- **DONKI:** as erupções são eventos solares reais sorteados do histórico. Depois de cada
  uma, um cartão na parede mostra data, tipo e intensidade.
- **OSDR:** experimentos reais de cultivo no espaço dão base ao tempo da estufa e aos
  cartões sobre radiação no corpo.
- **Artemis:** iluminação do polo sul lunar define os dias de luz e de sombra.

### Modo sem headset

Mesma cena com câmera fixa. A criança arrasta as células com o mouse ou o dedo.

### Versão mínima para o hackathon

Um cenário, cinco tomadas, 10 turnos, cinco eventos, lista embutida de erupções reais e
relatório final.

### Riscos

- Balanceamento: fácil surgir uma estratégia sempre vencedora.
- O VR acrescenta pouco à estratégia em si; o ganho é tornar o trade-off tátil.

---

## Proposta B: Janela para Marte

**Formato:** narrativa de decisões dentro de panoramas reais.
**Local:** Marte.
**Idade ideal:** 7 a 12; a mais leve para headset e para celular.

### Conceito

A criança está **dentro de fotografias reais de Marte**: panoramas 360° tirados pelos
rovers da NASA. Cada sol começa em um lugar real do planeta. Um dilema aparece em cartões
flutuantes, e ela escolhe apontando e apertando o gatilho, ou tocando na tela.

É a proposta em que o VR mais entrega com menos esforço: estar em pé dentro de uma foto
real de Marte já é a experiência.

### Como se joga

1. **Chegada.** O panorama do sol aparece. O robô ajudante aponta algo na paisagem: uma
   rocha, uma nuvem de poeira, uma cratera.
2. **Dilema.** Dois ou três cartões, cada um com o custo mostrado em ícones antes da escolha.
3. **Consequência.** Medidores mudam e algumas escolhas voltam a cobrar sols depois.

O baralho tem cerca de 30 dilemas e cada partida usa 12.

### Mecânica de assinatura: o rádio da Terra

No painel há um rádio. A criança pode perguntar ao controle da missão, mas a mensagem
leva de 3 a 22 minutos para chegar, conforme a posição dos planetas, e o mesmo para voltar.
No jogo, a resposta só chega no sol seguinte. Ela ouve o chiado e a espera; em dilema
urgente, esperar piora a situação. É a única proposta que coloca o atraso de comunicação,
um dos objetivos do desafio, como mecânica central.

### Recursos centrais

Ar e água, energia e saúde da tripulação. A comida entra como arco da história.

### Exemplos de dilema

- **Tempestade de poeira no horizonte** (visível no panorama). Limpar os painéis gasta ar
  em uma saída; esperar derruba a energia por vários sols.
- **Alerta de radiação.** Abrigo perde o sol de trabalho; continuar custa saúde.
- **Rocha rara a 2 km.** Ir gasta ar e energia; ficar perde a descoberta.

### Dados da NASA

- **Missões de superfície de Marte:** panoramas reais dos rovers como cenário, e medições
  reais de temperatura e pressão no boletim de cada sol. Conferir a licença de cada imagem;
  imagens da NASA costumam ser de domínio público.
- **DONKI:** o alerta de radiação é um evento solar real. Um candidato é a erupção de
  setembro de 2017, registrada na superfície de Marte pelo rover Curiosity.
- **OSDR:** efeitos na saúde da tripulação com base em pesquisa real com astronautas.

### Modo sem headset

No celular, o panorama acompanha o movimento do aparelho (giroscópio). No computador,
arrasta-se para olhar em volta. Funciona bem em sala de aula com projetor.

### Versão mínima para o hackathon

Doze panoramas, doze dilemas fixos, três medidores, o rádio da Terra e dois finais.

### Riscos

- Interação limitada: em VR, a criança mais olha e escolhe do que manipula.
- Panoramas grandes pesam no carregamento WebGL; precisam ser comprimidos.
- Carga de leitura: mitigar com narração e ícones.

---

## Proposta C: Muro de Água

**Formato:** gerenciamento físico de recurso em tempo real, com pausa.
**Local:** abrigo de uma base lunar.
**Idade ideal:** 9 a 12 de headset, 7 a 12 na tela.

### Conceito

A proposta se apoia em um trade-off real e pouco conhecido: **a água protege contra
radiação**, e por isso há estudos de habitats com paredes de água. Na base, a mesma água
serve para beber, para gerar oxigênio e para regar a estufa.

A criança está no abrigo, cercada por prateleiras vazias na parede. Ela pega bolsas de água
do tanque e as empilha nas prateleiras para montar o escudo. Cada bolsa na parede é uma
bolsa a menos para a tripulação usar.

### Como se joga

1. **Calmaria.** A água está no tanque, alimentando ar e estufa. Os medidores sobem.
2. **Alerta.** Uma linha do tempo no teto mostra uma erupção chegando, com contagem
   regressiva. O jogo pode ser pausado a qualquer momento.
3. **Montar o escudo.** A criança pega bolsas e preenche as prateleiras. Quanto mais
   cheia a parede, menor a dose. Ela vê o tanque esvaziando.
4. **Passou a tempestade.** Ela devolve as bolsas ao tanque, mas parte da água se perdeu
   no processo, e a estufa ficou sem rega durante o evento.

### Recursos centrais

- **Água:** a mesma reserva vira escudo ou vida.
- **Oxigênio:** depende da água no tanque.
- **Radiação:** depende da água na parede.

### Trade-offs

- Escudo cheio protege a tripulação, mas seca o ar e a estufa.
- Montar o escudo cedo demais desperdiça água à toa; tarde demais expõe a tripulação.
- Erupções fracas não justificam a parede inteira: é preciso ler a intensidade.

### Dados da NASA

- **DONKI:** sequência real de eventos solares, com o modo "Tempestades famosas" (por
  exemplo, as grandes tempestades de maio de 2024) embutido e, se sobrar tempo, um modo
  com os eventos dos últimos 30 dias. O jogo sempre indica se o dado é arquivado ou recente.
- **OSDR:** faixas do medidor de radiação explicadas com dosimetria real de astronautas.
- **Estudos de blindagem da NASA:** base para a proteção da água. Precisa ser confirmado
  na lista oficial de recursos.

### Modo sem headset

Visão fixa da parede; a criança clica nas prateleiras para mover bolsas.

### Versão mínima para o hackathon

Uma sala, um tanque, 12 prateleiras, dois cenários de tempestade embutidos e relatório.

### Riscos

- Tempo real com crianças pode estressar: a pausa e os avisos antecipados são obrigatórios.
- Movimento repetitivo de pegar e empilhar pode cansar no headset; limitar a 8 a 12 bolsas.
- O dado que liga quantidade de água a redução de dose precisa de fonte confiável.

---

## Comparação

| Critério | A: Mãos na Base | B: Janela para Marte | C: Muro de Água |
| --- | --- | --- | --- |
| Formato | Turnos | Narrativa | Tempo real com pausa |
| Local | Lua | Marte | Lua |
| O que o VR acrescenta | Trade-off tátil (pegar células) | Estar dentro de fotos reais de Marte | Construir o escudo com as mãos |
| Uso de dados reais | Erupções embutidas | Panoramas reais, a fonte mais visível | Tempestades reais na mecânica |
| Atraso de comunicação | Fora | Mecânica central | Fora |
| Modo sem headset | Bom | Muito bom (celular) | Bom |
| Conforto em VR | Alto | Muito alto | Médio |
| Esforço de arte 3D | Médio | Baixo | Médio |
| Esforço de interação VR | Médio | Baixo | Alto |
| Risco principal | Balanceamento | Pouca interação | Fonte do dado de blindagem e cansaço |

## Recomendação

**Proposta A, com a água da C como evento.** A tem o ritmo mais seguro para crianças
(turnos, sem pressa), funciona igualmente bem com e sem headset e deixa o trade-off central
literalmente nas mãos de quem joga. O trade-off da água como escudo é forte o bastante para
entrar na A como evento de erupção ("use água para reforçar o abrigo?") sem virar um
sistema a mais.

Se a equipe for pequena ou ninguém tiver experiência com interação em VR, a **B** é a mais
barata e a que mais impressiona com menos esforço, além de ser a única que trata o atraso de
comunicação.

---

## O que confirmar antes de começar

- [ ] **WebXR no Unity 2022.3.** O Unity não exporta WebXR nativamente. O caminho mais
      conhecido é o pacote comunitário WebXR Export (De-Panther). Testar logo no primeiro
      dia se ele funciona nesta versão e conferir que a licença é open source (seção 7.4 do
      [DESAFIO.md](DESAFIO.md)). Se não funcionar, avaliar um motor web (three.js, Babylon.js
      ou A-Frame) ou um build nativo para Quest com o modo tela no navegador.
- [ ] **Headset para testar.** Qual a equipe tem? O navegador dele suporta WebXR?
- [ ] **Idade mínima do headset** usado no teste e na demonstração.
- [ ] **Vídeo de demonstração.** Não pode mostrar nome, voz ou rosto reconhecível de menores
      de 18 anos. Se crianças forem testar o jogo, filmar só a tela ou a visão do headset.
- [ ] **Licença das imagens** (panoramas e fotos da NASA) e de qualquer asset 3D usado.
