# Prompt para a sessão do Unity: importar os personagens e configurar animação

Cole o texto abaixo na outra sessão (projeto Unity na raiz deste repositório). Se a sessão do cenário (`Docs/PROMPT-UNITY-BASE.md`) já tiver rodado, use a cena `Base` para testar.

---

Você vai importar no Unity os personagens 3D do jogo "Ten Days on the Moon" e configurar o Animator. Os modelos foram feitos no Blender por outra sessão e já estão exportados. **Não edite `Assets/Blender/`** (fontes) e não regenere os modelos.

## Projeto
Unity 2022.3.62f3, URP 14.0.12. Leia `Docs/DESAFIO.md` só se precisar de contexto do jogo.

## Arquivos prontos em `Assets/Models/Personagens/`
- `Astronauta_Engenharia.fbx` (ciano), `Astronauta_Botanica.fbx` (verde), `Astronauta_Medicina.fbx` (roxo), `Astronauta_Ciencia.fbx` (laranja): ~1,9 m, ~1.300 a 1.600 triângulos, malha única skinnada, 20 ossos.
- `Robo_Assistente.fbx`: ~1,37 m, ~1.600 triângulos, esteiras no lugar de pernas.
- `personagens.json`: por personagem, altura, triângulos, hierarquia de ossos (com o nome do osso Humanoid correspondente), clipes (nome, quadros, fps, loop) e materiais (cor base, metálico, rugosidade, emissão).

Referências visuais em `Assets/Blender/`: `personagens_equipe_frente.png`, `personagens_equipe_3-4.png`, `personagens_equipe_costas.png`, `personagens_robo_*.png`, `personagens_astro_medicina.png`. Animações em GIF: `animacoes_idle.gif`, `animacoes_caminhar_*.gif`, `animacoes_correr_*.gif`.

## Características dos modelos
- Unidades em metros; pivô na base dos pés. **A frente do personagem é −Y no Blender, que deve virar +Z (forward) no Unity.** Se o personagem importar de costas ou espelhado, pare e relate.
- Pose de repouso em **A** (braços ligeiramente abertos), não em T.
- Astronautas usam nomes de ossos de humanoide: `Hips, Spine, Chest, Neck, Head, Shoulder_L/R, UpperArm_L/R, LowerArm_L/R, Hand_L/R, UpperLeg_L/R, LowerLeg_L/R, Foot_L/R, Toes_L/R`. O `_L` fica no lado +X. Lado "L" do personagem é a esquerda dele.
- O robô tem ossos próprios (`Root, Body, Head, Antenna, UpperArm_L/R, LowerArm_L/R, Hand_L/R, Tread_L/R`) e deve ser **Generic**, não Humanoid.
- Os acessórios por função fazem parte da malha: Engenharia (cinto de ferramentas, cabo), Botânica (pote com muda, na mão direita), Medicina (maleta no peito), Ciência (scanner na mão direita). Não são objetos separados.

## Clipes (em cada FBX; 24 fps; todos em loop)
- Astronautas: `Idle` (48 quadros), `Caminhar` (24), `Correr` (16).
- Robô: `Idle` (48), `Rodar` (24, movimento normal), `Acelerar` (16, movimento rápido).
O último quadro de cada ciclo repete o primeiro. Se o loop engasgar no Unity, recorte o último quadro ou ative "Loop Pose". Os clipes são só de rotação, mais deslocamento vertical dos quadris em `Correr`. **Não há deslocamento de raiz (sem root motion)**: o personagem anda parado no lugar e o movimento vem do script.

## Tarefas
1. **Importar** os 5 FBX: Scale Factor correto (1 unidade = 1 m; confira com um cubo de 1 m), Rig: **Humanoid** nos astronautas (Avatar criado a partir do modelo; verifique o mapeamento de ossos pelo `personagens.json` e corrija manualmente se o auto-mapeamento falhar; a pose A pode exigir ajuste em "Muscles & Settings" ou "Enforce T-Pose", tente sem alterar o modelo) e **Generic** no robô. Animation: importe os clipes com os nomes originais, Loop Time ligado, e `Bake Into Pose` em Rotation, Y e XZ (sem root motion).
2. **Materiais URP**: crie materiais URP/Lit em `Assets/Materials/Personagens/` a partir do `personagens.json` (cor base, metálico, rugosidade). O `Visor dourado` é metálico e brilhante. Materiais com emissão (`Robo olho`, `Tela verde`) ligam Emission com a cor e a intensidade do JSON. Os astronautas **compartilham** materiais de traje (branco, cinza, luva) e só diferem no `Acento <cor>`; mantenha assim para economizar draw calls. Remapeie os FBX para eles.
3. **Prefabs**: um prefab por personagem em `Assets/Prefabs/Personagens/`, com Animator, `CapsuleCollider` (altura ~1,9 m, raio ~0,3 m, centro na metade; no robô, ~1,3 m e raio 0,35 m) e `CharacterController` ou `Rigidbody` conforme o padrão do projeto. Sombras: lance e receba.
4. **Animator Controller** para astronautas, com parâmetro `Velocidade` (float, em m/s): `Idle` abaixo de ~0,2, `Caminhar` entre ~0,2 e ~3,5 e `Correr` acima, com blend tree 1D e transições suaves. Compartilhe o mesmo controller entre os 4 astronautas (Humanoid permite). Faça um controller equivalente para o robô (`Idle`, `Rodar`, `Acelerar`). Ajuste a velocidade de reprodução para os pés não deslizarem: velocidade de passada de referência `Caminhar` ≈ 1,5 m/s, `Correr` ≈ 4,5 m/s. Ajuste olhando e relate o valor final.
5. **Script de teste**: um `PersonagemTeste` simples (WASD ou setas movem; Shift corre) que alimenta o parâmetro `Velocidade`. Gire o personagem na direção do movimento e deslize pela cena. Só para validar, sem lógica de jogo.
6. **Cena de validação** `Assets/Scenes/PersonagensTeste.unity` com os 5 prefabs lado a lado num chão simples, câmera de 3/4 e uma luz direcional.

## Entregáveis e verificação
- Cena abrindo sem erros ou avisos, com os 5 personagens animando `Idle`, `Caminhar` e `Correr` (ou os equivalentes do robô).
- Capturas do Game view dos personagens parados, caminhando e correndo, comparáveis às imagens de referência.
- Confirme: de frente certo, joelhos dobrando para trás da canela (como num humano), mãos junto ao corpo, loops sem pulo.
- Resumo curto: o que foi criado, valores de velocidade escolhidos, divergências do Blender (eixos, mapeamento Humanoid).
- Se algo no FBX estiver errado (escala, malha espelhada, ossos mal mapeados), pare e relate em vez de contornar.

## Cuidados
- O Unity também importa os `.blend` e `.png` de `Assets/Blender/` se estiverem em `Assets/`. **Não apague nem mova esses arquivos sem avisar o usuário**; proponha movê-los para fora de `Assets/` e espere a confirmação.
- Não faça commit sem o usuário pedir.
