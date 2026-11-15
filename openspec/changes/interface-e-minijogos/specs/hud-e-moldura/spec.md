# Spec Delta

## Purpose

Define como a interface emoldura a cena 3D durante a missão: o HUD no topo, o botão principal
fixo, o painel lateral, a faixa do deslocamento e a pausa.

## ADDED Requirements

### Requirement: A interface emoldura a cena
Durante o deslocamento e o minijogo, a interface SHALL ocupar só o topo (HUD), o canto inferior
direito (botão principal) e, quando houver painel, a faixa direita da tela. O centro e a metade
esquerda SHALL ficar livres para a cena 3D.

#### Scenario: Andando pela base
- **WHEN** o jogador anda até a estação da tarefa
- **THEN** o personagem e o caminho à frente ficam visíveis, sem painel ou faixa escura por cima do centro

### Requirement: Botão principal fixo
Em toda tela da missão, a ação que avança SHALL ficar no mesmo lugar (canto inferior direito), com o
mesmo tamanho e estilo: Continuar, Começar o dia, Próximo dia, Pular, Ver relatório e a ação junto
da estação. Com os controles de toque ativos, o botão SHALL ficar acima do joystick da câmera.

#### Scenario: Do Boletim ao Resultado
- **WHEN** o jogador passa por Boletim, Distribuição e Resultado
- **THEN** o botão que avança está sempre no mesmo canto e com o mesmo tamanho

#### Scenario: Perto da estação no celular
- **WHEN** o jogador chega à estação com os controles de toque na tela
- **THEN** o botão de ação aparece no canto inferior direito, sem cobrir o joystick da câmera

### Requirement: HUD com medidores por forma
O HUD SHALL mostrar o dia, o Oxigênio, a Radiação, a Bateria, a Ciência e o relógio do dia em todas
as fases da missão, menos no Relatório. Cada medidor SHALL ter ícone, valor numérico e forma
própria: Oxigênio em 10 segmentos, Radiação em barra com ponteiro e Bateria em 4 células.

#### Scenario: Leitura rápida
- **WHEN** o Oxigênio está em 6 de 10
- **THEN** 6 dos 10 segmentos estão cheios e o número 6 aparece junto do ícone

### Requirement: Zona de alerta nos medidores
Cada medidor de Oxigênio e de Radiação SHALL ter zona de alerta marcada mesmo antes de alcançada:
Oxigênio em 3 ou menos e Radiação em 7 ou mais. Quando o valor entra na zona, o medidor SHALL mudar
para o vermelho de alerta e mostrar o triângulo de alerta, sem piscar.

#### Scenario: Radiação alta
- **WHEN** a Radiação chega a 7
- **THEN** a barra fica vermelha, o triângulo de alerta aparece junto dela, e o restante do HUD não muda de cor

#### Scenario: Fora da zona
- **WHEN** a Radiação está em 4
- **THEN** a barra usa a cor normal e a faixa hachurada da zona de alerta continua visível no fim da barra

### Requirement: Medidor anda junto com a mudança
Quando um valor do HUD muda, o medidor SHALL andar até o novo valor em vez de saltar, e uma variação
flutuante (ícone, ▲ ou ▼ e número) SHALL aparecer junto do medidor por pouco mais de 1 segundo.

#### Scenario: Custo de sair da base
- **WHEN** o jogador sai para a coleta de pedras e paga 2 de Oxigênio
- **THEN** o medidor de Oxigênio desce dois segmentos e aparece "▼ 2" junto dele

### Requirement: Relógio do dia sem pressão vermelha
O relógio do dia SHALL mostrar o ícone de relógio e o tempo em minutos e segundos. No último minuto
ele SHALL mudar para a cor da energia, sem vermelho, sem pulso e sem som.

#### Scenario: Último minuto
- **WHEN** faltam 45 segundos no relógio do dia
- **THEN** o relógio aparece em amarelo, parado, sem piscar

### Requirement: Faixa do deslocamento
Durante o deslocamento, uma faixa estreita no topo, logo abaixo do HUD, SHALL dizer o destino com o
ícone da tarefa ou do console. A seta guia e o destino SHALL usar a cor do módulo de destino.

#### Scenario: Indo para a Estufa
- **WHEN** o jogador sai do Boletim num dia de tarefa na Estufa
- **THEN** a faixa mostra o ícone da tarefa e "Leve a tripulação até a Estufa", e a seta usa o verde da Estufa

### Requirement: Pausa como cartão central
A Pausa SHALL ser a única tela que cobre o centro: um cartão central sobre a cena escurecida, com
"Continuar" como botão principal e as ações secundárias abaixo. Voltar ao menu SHALL pedir
confirmação em tom de pergunta.

#### Scenario: Reiniciar pela pausa
- **WHEN** o jogador escolhe reiniciar na Pausa
- **THEN** o jogo pergunta "A missão atual será perdida. Voltar ao início?" com as opções de voltar e de confirmar
