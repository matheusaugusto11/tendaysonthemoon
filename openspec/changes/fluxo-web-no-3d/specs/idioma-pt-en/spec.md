# Spec Delta

## Purpose

Garante que o jogo inteiro possa ser lido em português e em inglês. A regra eliminatória da
submissão exige inglês, e o público brasileiro precisa do português.

## ADDED Requirements

### Requirement: Todo texto nos dois idiomas
Todo texto que o jogador vê SHALL existir em português e em inglês. Isso inclui telas, botões, tarefas, minijogos, dicas, cartões de erupção e créditos.

#### Scenario: Partida em inglês
- **WHEN** o jogador joga uma missão inteira em inglês
- **THEN** nenhum texto de interface aparece em português

### Requirement: Idioma inicial
O idioma inicial SHALL ser português quando o idioma do navegador ou do sistema começa com "pt". Nos outros casos, SHALL ser inglês.

#### Scenario: Navegador em inglês
- **WHEN** a build WebGL abre num navegador configurado em inglês
- **THEN** a tela inicial aparece em inglês

### Requirement: Troca de idioma a qualquer momento
Um botão de idioma SHALL ficar visível na tela inicial e no HUD. Trocar o idioma SHALL atualizar a tela atual na hora, sem perder o estado da partida.

#### Scenario: Trocar no meio da distribuição
- **WHEN** o jogador troca o idioma com 3 células já colocadas
- **THEN** a tela de distribuição é redesenhada no outro idioma, com as mesmas 3 células nos mesmos módulos

### Requirement: Datas no idioma escolhido
As datas das erupções SHALL aparecer por extenso no idioma ativo, sem mudar o dia por causa do fuso horário.

#### Scenario: Data da erupção
- **WHEN** o cartão da erupção de 2017-09-06 aparece em inglês
- **THEN** a data aparece como "September 6, 2017"
