# Art bible: Estação-Roda, low-poly estruturado

Versão 3, 14/11/2026, **aguardando aprovação do usuário**. Substitui a v2 (massinha). A versão visual,
com a maquete 3D, está no artifact "Estação-Roda Low-Poly"; este arquivo é a referência escrita para
agentes e para a modelagem no Blender.

Mantido do que já estava aprovado: hub com cinco corredores, os cinco módulos, as cores da interface,
as câmeras por fase e o Blender. Só o estilo mudou.

## 1. Estilo em uma frase

**Low-poly limpo com engenharia plausível.** Uma base feita de peças reais (tanques, túneis, treliças,
painéis, escotilhas) em faces grandes e bordas chanfradas, com luz dura de Sol baixo, sobre terreno
facetado e céu preto. Nada futurista.

## 2. Referências e o que tiramos delas

O visual das três imagens enviadas lembra um jogo de exploração low-poly (Astroneer). Servem só de
inspiração; nenhum asset delas entra no jogo.

| Referência | O que pegamos | O que trocamos |
| --- | --- | --- |
| 1, malha de hub com corredores | Hub radial, fileiras de painéis solares, tanques cilíndricos, faces grandes e poucos vértices | Seis corredores viram cinco, cada um com a cor do módulo |
| 2, exterior | Blocos chanfrados em branco, preto e um acento; tiras de luz; plataforma com escada e corrimão; tanques; vidro azulado com moldura escura; prato de antena | Amarelo único vira cinco acentos; nave e árvores alienígenas saem; Marte laranja vira Lua cinza |
| 3, interior da estufa | Estrutura de aço aparente, janelas altas, canteiros em fileiras, plantas low-poly, luz dura com sombras | Cultivo em estágios; luz de cultivo sinaliza o estado |

## 3. As seis regras

1. **Estrutura real.** Cada módulo é feito de peças reconhecíveis.
2. **Low-poly limpo.** Faces grandes e planas, bordas chanfradas, sem ruído. Metal com arestas vivas;
   terreno e rochas facetados.
3. **Cor em blocos.** Corpo branco e cinza, estrutura em aço escuro, um acento por módulo em faixas,
   portas e tiras de luz.
4. **Detalhe médio e legível.** Escadas, escotilhas, antenas, canteiros. Nada microscópico.
5. **Luz real e dura.** Sol baixo, sombra definida, preenchimento frio. Tiras e janelas emissivas.
6. **Nada futurista.** Sem naves, hologramas ou objetos flutuando.

## 4. Estrutura de cada peça

| Peça | Equivalente real | Componentes principais | Acento |
| --- | --- | --- | --- |
| Hub | Nó central pressurizado com portas radiais | Tambor de 12 m, teto facetado com 5 janelas, 5 placas de porta coloridas, corrimão, mesa de controle | Neutro |
| Corredor | Túnel pressurizado | Tubo com anéis de reforço, trilho de cabos, vestíbulo e escotilha, tira de luz | Cor do módulo |
| Ar e água | Planta de suporte à vida | 3 tanques verticais com cintas, tanque horizontal, passarela e escada, tubulação com válvulas, armário de controle, chaminé | `#4DD0E1` |
| Estufa | Estufa de pesquisa pressurizada (a conferir o exemplo real) | Salão com telhado de vidro de duas águas, quadros de aço em A, canteiros, luz de cultivo | `#6BD425` |
| Abrigo | Habitat coberto de regolito (conceito de estudo da NASA, F10) | Montículo de regolito, portal de concreto com porta blindada, gabiões, respiros | `#C77DFF` |
| Laboratório | Módulo pré-fabricado tipo contêiner | Caixa chanfrada, janelas, rampa com corrimão, climatização e instrumentos no teto, prato | `#FF9F43` |
| Bateria | Banco de baterias em contêineres | 3 contêineres com venezianas, ventiladores e barra de LED, transformador, postes de luz | `#FFD23F` |
| Apoio | Campo solar, pouso, comunicação, rover | Painéis em suportes inclinados, plataforma de pouso e módulo de carga, prato, rover de seis rodas | Neutro |

Os módulos ficam a 19 m do centro, a 72° entre si (Ar e água 0°, Estufa 72°, Bateria 144°, Abrigo 216°,
Laboratório 288°). Corredor de 10 m de comprimento do hub (raio 6) até o módulo. Astronauta com 1,8 m.
Trajeto a 8 m/s: hub até a porta 2,0 s; pior caso entre módulos 3,9 s. Nenhum trajeto acima de 5 s.

## 4.1 Interiores (o jogador anda por dentro)

Regras de todo interior: paredes brancas com juntas escuras e faixa escura no pé da parede; piso de
placas cinza com corredor livre de 2 m no centro; tira de luz no teto na cor de acento; janelas ou vigias
com Sol duro; pé-direito de 3 m (2,8 m no abrigo); escala humana de 1,8 m.

| Ambiente | Função no jogo | Layout e medidas | Componentes | Interação |
| --- | --- | --- | --- | --- |
| Hub (centro de controle) | Partida; células e fala do Bip | Sala circular de raio 5,5 m; 2,6 m, 4,3 m no centro | Mesa em anel com bandeja de 6 células, 5 escotilhas coloridas, janelas no teto, armários, corrimão | Bandeja; Bip |
| Corredor | Deslocamento (≤ 5 s) | Tubo de 10 m, 3,0 m de diâmetro | Anéis, grade de piso, trilho de cabos, corrimãos, tira de luz, vigias | Travessia |
| Ar e água (casa de máquinas) | Oxigênio; vazamento | 7×5×3 m, dois níveis | Bases dos tanques, tubulação com válvulas, bombas, filtros, passarela, armário | Painel de energia; parede de vazamento |
| Estufa | Regar e colher | 11,6×6,6 m, cumeeira a 3,4 m | Canteiros, quadros de aço em A, vidro, luz de cultivo | Painel de energia; 6 vasos |
| Abrigo | Proteção da erupção | 8×6×2,8 m, enterrado | Parede grossa, 4 beliches, tanques de água de blindagem, estação médica, dosímetro, porta blindada | Painel de energia; reunião |
| Laboratório | Descobertas; Expedição | 7×4,4×3 m | Bancadas, espectrômetro, freezer, câmara de luvas, monitores, eclusa de amostras | Painel de energia; bancada |
| Bateria | Guardar células (0 a 3) | 5×4×2,8 m | Racks com barras de LED, corredor central, resfriamento, bandejas de cabos, console | Painel de energia; barras de LED |
| Eclusa | Saída externa | 3×2,5×2,6 m | Armários de trajes, banco, escotilha redonda com roda, luzes de status | Sair e voltar |

Pontos de interação: **painel de energia** (2 encaixes redondos na cor do módulo, um por ambiente) e
**parede de vazamento** (6 pontos suspeitos: junta, vedação de janela, flange, escotilha, painel de
rebites, respiro; um solta vapor). Câmera dentro: andar atrás e um pouco acima do astronauta, a 2,5 m,
FOV 45; tarefa em close baixo, FOV 35; paredes que atrapalham ficam transparentes ou recuam (definir
com o Diretor de Câmera).

Prompts de interior (11, no artifact): hub, corredor, Ar e água, estufa, abrigo, laboratório, bateria,
eclusa, painel de energia (3 estados), parede de vazamento e câmera de jogo.

## 5. Estados de luz (tabela 3.3 do GDD)

| Estado | Células | Visual |
| --- | --- | --- |
| Desligado | 0 | Acento dessaturado, tiras escuras, sem leque de luz, parado |
| Ligado | 1 | Tiras acesas, 1 encaixe aceso, leque de luz no chão, movimento lento |
| Turbo | 2 | Tudo aceso, 2 encaixes, leque forte, movimento rápido |

Ocioso: Ar e água (vapor na chaminé), Estufa (plantas balançam e crescem em 5 estágios), Laboratório
(prato gira), Bateria (um contêiner aceso por célula guardada, 0 a 3), Hub (três células orbitam).
Estado do mundo: poeira no painel (0 a 6), vazamento, falha de painel, fome.

## 6. Cor e material

| Uso | Cor | Material |
| --- | --- | --- |
| Acentos | `#4DD0E1`, `#6BD425`, `#C77DFF`, `#FF9F43`, `#FFD23F` | Tinta fosca e tira emissiva |
| Pintura branca | `#E6E4DF` | Metal pintado |
| Cinza claro | `#BFC0C4` | Metal pintado |
| Aço escuro | `#2E323B` | Aço, mais metálico |
| Concreto | `#9A98A0` | Fosco |
| Vidro | `#5FB3C4` | Transparente a 35%, moldura escura |
| Painel solar | `#2B5C9E` | Metálico, liso |
| Regolito | `#9B98A3` claro, `#6E6B79` sombra | Fosco, facetado |
| Luz quente | `#FFF3C4` | Emissivo |
| Céu | `#05060C` | Preto, Terra pequena e baixa |

Tripulação: traje EVA branco com viseira dourada e acento na faixa do capacete, na placa do ombro e na
faixa da mochila (Lia ciano, Téo verde, Nina violeta, Kai laranja). Bip: robô de esteiras, corpo branco,
tela com olhos ciano, faixa `#8FB4FF`.

## 7. Luz e câmera

| Luz | Valor |
| --- | --- |
| Sol | Quente `#FFE2B8`, 12° acima do horizonte, da esquerda, sombra definida |
| Preenchimento | Frio azulado `#7A86C0`, fraco |
| Contorno | Fino, azul frio, por trás |
| Luzes do mundo | Tiras, janelas e leques de luz no chão em cor de acento, emissivas |

> **Licença artística.** No polo sul o Sol fica a no máximo 1,5° do horizonte (NASA F2, F3, F5), e a
> sombra seria cerca de 38 vezes o objeto. O jogo usa 12° (cerca de 5 vezes). É decisão visual, não dado.

| Fase | Câmera | FOV | Movimento |
| --- | --- | --- | --- |
| 1 Boletim | Revelação do espaço até o hub | 40 → 50 | Descida de 4 s, pulável |
| 2 Distribuir | Mapa: base inteira e bandeja visíveis, 62° de inclinação | 45 | Parada |
| 3 Tarefas | Close baixo em três quartos | 35 | Blend de 1,2 s |
| 4 O dia acontece | Voo em volta da estação | 50 | Órbita de 8 a 10 s, pulável |
| 5 Resultado | Recuo ao hub, Bip em primeiro plano | 45 | 2 s |
| Acerto | Golpe de zoom | −8 | 0,25 s |

Imagem nítida, sem profundidade de campo. Cinemachine ainda não está instalado (Timeline 1.7.7 já está).

## 8. Receitas de Blender

Padrão: 1 unidade = 1 m, escala aplicada, pivô no centro da base, frente para −Y, Z para cima, nomes
`SM_` e `SK_`, FBX com Apply Transform, cor pela UV em um atlas de paleta de 256², tiras de luz em
material emissivo separado. Acabamento: Bevel de 1 segmento, Weighted Normal com arestas vivas acima de
30°, terreno e rochas em Shade Flat, juntas de painel por Inset ou normal map pequeno.

| Asset | Primitivas | Modificadores | Triângulos |
| --- | --- | --- | --- |
| Hub | Cilindro de 24 lados, tronco de cone de 10 lados, 5 faixas de janela, 5 placas de porta, mesa de caixas | Bevel 0,03 m; Weighted Normal | 8.000 |
| Corredor | Cilindro de 12 lados, anéis (Array ×5), trilho de cabos, aro de escotilha | Array; Bevel; instância ×5 | 1.500 |
| Ar e água | Tanques (cilindro de 14 lados com calota), tubos, passarela e escada, armário | Bevel; Array | 8.000 |
| Estufa | Salão, quadros em A (Array), vidros, canteiros (Array), plantas (icosfera nível 0) | Bevel; Solidify nos vidros | 10.000 |
| Abrigo | Meia esfera ou cilindro, portal e porta, gabiões | Bevel; Displace leve; Shade Flat | 5.000 |
| Laboratório | Caixa chanfrada, janelas, rampa, equipamentos de teto, prato | Bevel; Array nos corrimãos | 8.000 |
| Bateria | Contêineres com venezianas (Array), transformador, isoladores | Bevel; Array | 8.000 |
| Astronauta | Caixas chanfradas, esfera de 10×8 (capacete), calota dourada (viseira) | Bevel; Weighted Normal | 4.500 |
| Bip | Caixa chanfrada, 2 esteiras, cúpula de 10 lados, tela, antena | Bevel | 2.500 |
| Terreno | Grade de 64×64 | Displace, Decimate 0,5, Shade Flat, cor por vértice | 10.000 |
| Rochas e espigões | Icosfera nível 1 ou cilindro de 6 a 8 lados | Displace, Decimate, Shade Flat; 6 variantes | 300 cada |

Orçamento: total visível até 120.000 triângulos (a maquete do artifact tem cerca de 41.000), 4 materiais
(pintura, aço, vidro, emissivo), uma direcional com sombra, build até 30 MB comprimido. Tudo **a medir
no WebGL**.

## 9. Assets e portões

Ordem: personagens, props, cenário. Nada entra no Blender sem prancha aprovada com 3 imagens.

| Asset | Prioridade |
| --- | --- |
| `SK_Astronaut` (Lia, Téo, Nina, Kai), `SK_Bip` | P0 |
| `SM_Hub`, `SM_Corridor` (+ vestíbulo), `SM_DoorPlate` ×5 | P0 |
| `SM_Mod_Air`, `_Green`, `_Shelter`, `_Lab`, `_Battery` | P0 |
| `SM_Ground`, `SM_Rock` ×6, `SM_Spire` ×6, `SM_SolarPanel` | P0 |
| `SM_EnergyCell`, `SM_Dock` | P0 |
| `SM_Planter`, `SM_Crop` (5 estágios), `SM_Tool` ×4, VFX | P1 |
| `SM_Lander`, `SM_Dish`, `SM_Rover`, `SM_Crate` | P2 |

Portões: 0 estilo · 1 pranchas dos personagens · 2 pranchas dos props · 3 pranchas do cenário ·
4 blockout e greybox · 5 produção e animação · 6 revisão e medição no WebGL.

## 10. Prompts do Gemini

O kit completo (mensagem de estilo, 14 prompts de prancha, protocolo e checklist de 12 itens) está no
artifact, com botão de copiar. Protocolo: um chat por grupo; **anexar as três imagens de referência** com
a mensagem de estilo; gerar primeiro a cena geral; uma imagem por mensagem; anexar a melhor imagem
anterior; aceitar só o que passar no checklist.

**Mensagem de estilo (resumo):** clean low-poly 3D game art with realistic space-habitat engineering, like
the attached references; chamfered boxes and low-sided cylinders; white painted panels with dark seams,
dark steel frames, handrails, ladders, hatches, tanks; one accent color per module in bands, doors and
strip lights; teal glass with dark frames; faceted low-poly terrain; not futuristic; hard low Sun about 12°
from the left; black sky with a small blue Earth; 35mm three-quarter view; avoid clay, vector, outlines,
photorealism, spaceships, neon, haze, noise texture.

## 11. Perguntas abertas

1. Aprova o estilo "low-poly estruturado e realista" no lugar da massinha?
2. Aprova astronautas com traje EVA e viseira dourada (sem rosto), com a expressão no Bip e nos retratos
   da interface?
3. Aprova o Bip como robô de esteiras em vez de esfera flutuante?
4. Aprova o corpo branco com a cor da interface como acento de cada módulo?
5. Aprova o Sol a 12° e o Cinemachine? Qual é o prazo real?
