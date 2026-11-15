# Proposta: um evento com decisão em cada dia

## Por quê

Hoje o dia no Unity é uma sequência de rotina: Boletim, tarefa com minijogo, distribuição de
energia. Nenhum momento do dia pede ao jogador uma decisão difícil diante de um perigo. O alerta
de erupção aparece só como texto no Boletim, e a resposta a ele é um número de células no módulo
Abrigo, no meio da distribuição. O jogador não sente a tempestade nem escolhe "corro para o
abrigo ou termino o trabalho?". Por isso o dia parece desconexo, e o dado da NASA, só mais uma
etiqueta. As regras do desafio (`Docs/REGRAS-PARA-GANHAR.md` §2 e §3) pedem o contrário: toda
escolha com custo real, consequências ao longo da missão, rejogabilidade e o dado da NASA
mexendo na jogabilidade, ligando a decisão a uma consequência do mundo real. A entrega fecha
hoje, 15/11/2026, às 23h59. Por isso a mudança é enxuta: reaproveita as regras, o estado e as
telas que já existem e acrescenta um conjunto variado de eventos sorteados.

## O que muda

- **Um evento por dia, nos 10 dias.** Em algum momento do deslocamento, o dia é interrompido
  por um evento: toca um alarme, a luz muda e abre um cartão com 2 ou 3 escolhas. Cada escolha
  mostra o custo e mexe de verdade em Oxigênio, Radiação, células, ciência, comida ou no
  relógio do dia. Nenhuma opção sai de graça.
- **Tempestade de partículas solares (SEP) com corrida até o abrigo.** Nos dias de alerta, o
  evento é a chegada das partículas da erupção. O jogador escolhe entre correr até o abrigo
  (Dormitório) antes do fim da contagem, o que custa tempo do dia e pode custar a tarefa, e
  continuar o trabalho, o que custa radiação. Com alarme falso, quem correu perde tempo à toa.
  **BREAKING (regra):** essa escolha substitui a regra "astronauta fora da base: +2 de
  radiação". O Abrigo da distribuição continua protegendo a base no resto do dia.
- **Aviso de CME com consequência adiada.** Um evento anuncia uma nuvem de plasma real do
  DONKI a caminho, que chega 2 dias depois como um alerta forte. Preparar o abrigo hoje custa
  células e deixa a chegada mais barata. Deixar para depois não custa nada hoje.
- **Conjunto de eventos com fonte da NASA**, sorteado sem repetição na partida: tremor lunar,
  micrometeorito, poeira lunar na eclusa, frio da sombra, gelo numa cratera escura, Terra atrás
  do horizonte (sem contato), sono ruim no Sol que não se põe, saudade de casa e fungo na
  estufa. Somados a SEP e CME, são 11 tipos de evento. Cada cartão cita a fonte em texto. Fato
  sem fonte conferida fica marcado "a conferir" e não entra no sorteio até ser conferido.
- **Dados novos do DONKI:** eventos SEP medidos (não previsões de modelo) ligados às erupções
  da lista curada, e uma lista curada de CMEs com velocidade e link. Tudo rotulado como dado
  arquivado. O texto passa a explicar a cadeia real: a erupção é vista (classe X, M ou C), e as
  partículas chegam depois.
- **Resultado e relatório contam a decisão.** O resultado do dia ganha a linha do evento, e a
  decisão entra nos "momentos que mais pesaram" do relatório.
- **Créditos e fontes** passam a listar as novas fontes. `Docs/DADOS-NASA.md` ganha as seções
  de SEP, CME e fatos dos eventos.

## Capacidades

### Novas capacidades

- `eventos-do-dia`: o evento de cada dia, o sorteio sem repetição, o momento da interrupção, o
  cartão de decisão, os efeitos de cada escolha (imediatos e adiados), a corrida até o abrigo,
  o registro no resultado e no relatório, e o conjunto de eventos com fonte.

### Capacidades modificadas

As três vêm da mudança `fluxo-web-no-3d`, ainda não arquivada. Esta mudança deve ser arquivada
depois dela.

- `clima-espacial-donki`: o conjunto curado ganha os SEP ligados e a lista de CMEs. A regra do
  astronauta fora da base sai, e no lugar dela entra a exposição decidida no alarme. O cartão
  "Isso aconteceu de verdade" e os créditos incluem SEP e CME. A chegada de uma CME pode criar
  um alerta forte.
- `ciclo-da-missao`: o dia ganha a interrupção do evento entre o Boletim e a distribuição. O
  relógio fica parado no cartão do evento e perde tempo com a espera no abrigo. O Boletim
  mostra a CME a caminho, e o Dia 1 do tutorial tem um evento fixo com dica.
- `exploracao-na-base-3d`: o personagem anda na corrida até o abrigo, a seta guia aponta para o
  Dormitório durante o alarme e a luz de Erupção acende durante o alarme.

## Impacto

- **Regras (assembly `DezDias.Regras`):** novo `Eventos.cs` (tabela, sorteio e efeitos).
  Mudanças em `Missao.cs` (estado do evento, CME agendada, efeitos de "só hoje") e em
  `RegrasMissao.cs` (`NovaPartida` sorteia os eventos; `Resolver` troca a regra de "fora" pela
  exposição do alarme e inclui as linhas do evento). Textos novos em PT e EN em `Textos.cs` ou
  em `Eventos.cs`.
- **Unity:** `GameManager.cs` ganha as fases `Evento` e `CorridaAbrigo`, o gatilho do evento e
  o destino da seta para a estação `cama`. `UIJogo.cs` ganha o cartão do evento e a contagem da
  corrida, no estilo da `interface-e-minijogos`.
- **Dados:** `Assets/Resources/ErupcoesDONKI.json` ganha os campos do SEP ligado. Novo
  `Assets/Resources/CmesDONKI.json`. Respostas brutas do DONKI em `Docs/donki/`.
- **Testes EditMode:** sorteio dos eventos, efeitos de cada escolha, exposição no alarme e CME
  adiada. `ResolverTestes` é ajustado para a regra de "fora" que sai.
- **Fora desta mudança:** o protótipo web (`web/index.html`) não muda e passa a ter menos
  eventos que o Unity. Também ficam fora a escolha de um estudo específico da OSDR, se nenhum
  for conferido a tempo (os eventos de saúde usam as páginas de risco do NASA Human Research
  Program), animações novas de personagem, som novo além dos sons sintetizados existentes e
  qualquer mudança no visual dos minijogos.
- **Prazo:** a build da `interface-e-minijogos` (tarefa 6.2) é publicada antes de começar esta
  mudança e continua sendo a entrega se esta não ficar pronta a tempo.
