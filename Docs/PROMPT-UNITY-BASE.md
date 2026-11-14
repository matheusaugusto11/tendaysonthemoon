# Prompt para a sessão do Unity: importar a base lunar e configurar a iluminação

Cole o texto abaixo na outra sessão (projeto Unity na raiz deste repositório).

---

Você vai importar no Unity o cenário 3D da base lunar do jogo "Ten Days on the Moon" e configurar a iluminação. O cenário foi modelado no Blender por outra sessão e já está exportado. **Não edite nada em `Assets/Blender/`** (são as fontes do Blender) e não tente regenerar os modelos.

## Projeto
Unity 2022.3.62f3, URP 14.0.12 (`Packages/manifest.json`). Já existem `Assets/Scenes`, `Assets/Settings` e `Assets/Real Stars Skybox` (use esse skybox para o céu). Leia `Docs/DESAFIO.md` e `Docs/REGRAS-PARA-GANHAR.md` só se precisar de contexto do jogo.

## Arquivos prontos em `Assets/Models/Base/`
- `base_cenario.fbx`: toda a geometria estática (90 malhas, agrupadas por material, nomes `Base_<material>`, `Estufa_<material>`, `Poste_*`) **e as 90 luzes** (nomes iguais aos do JSON). Sem animação.
- `base_portas.fbx`: 28 folhas de porta (`Porta_<porta>_<D|E>`), cada uma com o **pivô já na dobradiça** e um clipe de abertura `Porta_..._Abrir` (quadros 1–40, 24 fps). As posições são em coordenadas do mundo: instancie esse prefab em (0,0,0), junto com o do cenário.
- `base_portas.json`: por folha, porta a que pertence, pivô, ângulo de abertura (±105°) e eixo de giro.
- `base_luzes.json`: por luz, tipo, grupo (`sol`, `terra`, `int`, `ext`, `emerg`), cor, energia em watts do Blender, raio, cone (spots), se projeta sombra.
- `base_materiais.json`: por material, cor base, metálico, rugosidade, emissão (cor e intensidade), transparência.
- `iluminacao_presets.json`: três cenários, **Dia**, **Erupção** e **Noite**, com multiplicadores por grupo de luz, cor e rotação do sol, cor do céu e emissão das lâmpadas.

Imagens de referência de como cada cenário deve parecer estão em `Assets/Blender/iluminacao_<dia|erupcao|noite>_<aerea|hub|laboratorio|eclusa_externa|estufa>.png`. Vistas gerais do cenário: `Assets/Blender/base_*.png`.

## Layout (para você se orientar e conferir)
Unidades em metros. Hub circular central de raio ~6,8 m, 6 túneis radiais de ~9 m, cada um terminando em um módulo: Estufa, Laboratório, Energia, Eclusa, Dormitório e Água. A Eclusa tem uma porta de saída para o solo. O exterior tem terreno, paredão de cratera, painéis solares, antenas, módulo de pouso e rover. O vão de cada porta tem ~2 m de largura e a soleira fica ~0,3 m acima do piso. No Blender o eixo Z é vertical; confira a orientação que o importador do Unity produzir e **não espelhe** nada. Use o que vier do FBX como verdade e valide pela escala: o hub deve ter ~13,6 m de diâmetro.

## Tarefas
1. **Importar** os dois FBX com escala correta (1 unidade = 1 m; teste com um cubo de 1 m ao lado). Ative `Import Lights` no `base_cenario.fbx`. Em `base_portas.fbx` mantenha as animações como clipes (Legacy ou Generic, o que for mais simples para disparar por script). Desative a geração de colisores automática; adicione colisores simples e baratos (BoxCollider/MeshCollider convexo) só onde precisar para um personagem andar: pisos, paredes, túneis. Depois me diga o que adicionou.
2. **Materiais URP**: crie materiais URP/Lit a partir de `base_materiais.json` (cor, metálico, rugosidade). Materiais com emissão: ative Emission com a cor e a intensidade (HDR). `Vidro` e `Painel solar` são transparentes (Surface Type Transparent para o vidro). Pela convenção, nomes que começam com `Lampada`, `Tela`, `Luz` e `Cupula luz` são emissivos controlados pela iluminação. Guarde os materiais em `Assets/Materials/Base/` e remapeie os dois FBX para eles.
3. **Cena**: monte `Assets/Scenes/Base.unity` (ou use a cena existente se houver uma de base) com os dois prefabs, o skybox de estrelas e uma esfera da Terra distante, azul e emissiva, num ponto fixo do céu. Mantenha a hierarquia organizada (Cenario, Portas, Luzes, Ambiente).
4. **Calibrar as luzes**: os watts do Blender não equivalem às unidades do Unity. Como ponto de partida, para luzes pontuais use intensidade ≈ W/100 e range 8–12 m; para spots, intensidade ≈ W/150, range ~14 m e cone de `base_luzes.json`; para o sol, a intensidade em torno de 1,5–2 com a cor do JSON. Ajuste olhando as imagens de referência. Interiores precisam ficar legíveis sem estourar. Sombras: só nos spots, no sol e em poucas luzes principais; desligue nas demais por desempenho. Evite que a luz de um ambiente vaze para fora das paredes (confira da vista aérea).
5. **Controlador de iluminação**: crie um script `BaseLightingController` que lê o grupo de cada luz (pelo nome, via `base_luzes.json`, ou marcando as luzes com um componente `LightGroup { grupo, energiaBase }`) e aplica os três cenários do `iluminacao_presets.json`: multiplicador por grupo, cor e rotação do sol, cor do céu/ambiente e emissão das lâmpadas (`Lampada*` ficam vermelhas na Erupção). API pública `SetPreset(Dia|Erupcao|Noite)` com transição suave opcional, e um menu de contexto no editor para testar. Na Erupção, faça as luzes do grupo `emerg` pulsarem (por exemplo, seno de 1–2 Hz).
6. **Portas**: um componente `Porta` por porta (combinando as folhas D e E pelo campo `porta` do JSON) com `Abrir()`, `Fechar()` e `Alternar()`. Pode tocar os clipes ou girar a folha em torno do pivô em Y pelo ângulo do JSON. Abra e feche com uma animação de cerca de 1,6 s. Não precisa de interação do jogador agora; deixe um teste por tecla no editor ou um botão de debug.
7. **Pós-processamento URP** (opcional, simples): Volume global com bloom leve e tonemapping, calibrado para os três cenários.

## Entregáveis e verificação
- Cena abrindo sem erros ou avisos de console, nos três cenários.
- Capturas do Game view de pelo menos: vista aérea, hub, um módulo e a estufa, em Dia, Erupção e Noite, comparáveis às imagens de referência.
- Resumo curto: o que foi criado, valores de calibração escolhidos e qualquer divergência do Blender (por exemplo, eixos).
- Se algo no FBX estiver errado (escala, faces invertidas, pivô de porta fora da dobradiça), pare e relate em vez de contornar com gambiarra.

## Cuidados
- O Unity também importará os arquivos de `Assets/Blender/` (.blend, .fbx, .glb, .png) se estiverem dentro de `Assets/`. Isso é lento e duplica assets. **Não apague nem mova esses arquivos sem avisar o usuário**; proponha movê-los para uma pasta fora de `Assets/` e espere a confirmação.
- Não faça commit sem o usuário pedir.
