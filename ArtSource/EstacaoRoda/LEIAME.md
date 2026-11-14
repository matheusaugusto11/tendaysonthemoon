# Estação-Roda

**O que é:** a base lunar do jogo *Dez Dias na Lua*: hub central, 5 corredores, 5 módulos, eclusa, área de apoio, terreno e dois
personagens (astronauta e Bip). O jogador anda por dentro de tudo. Estilo low-poly estruturado e realista, nada futurista.
**Plataforma e motor:** WebGL, Unity 2022.3.62f3 URP 14.0.12. Prioridade de aparelho: **PC, depois celular, depois tablet**.
**Perfil:** `perfil.json` (orçamento do mais fraco da lista, para rodar nos três).
**Blender:** gerado no **5.2.1** (`"blender": "5.2"` em `fonte/config.json`); o `.blend` abre no **5.3**, que é a versão do usuário. O 5.3 da Store trava ao
renderizar prévias em segundo plano nesta máquina, por isso não é usado para gerar. **Criado em:** 14/11/2026.

## Andamento

- **Etapa atual:** 5 de 6, Produção. Lote **K (kit comum, R00)**, processo **Modelagem**, aguardando confirmação. Depois do kit: L3 (corredor, hub, eclusa), L4 (módulos), apoio e terreno, montagem. Os personagens (L1) estão em espera: o cenário vem primeiro.
- **Aprovado:**
  - Etapa 4 (blockout do cenário, `Montagem.blend`) em 14/11/2026, incluindo o refinamento G (Estufa: paredes a z 3,2, cumeeira a z 5,0).
  - Etapa 1 (escopo), 2 (referências) e 3 (plano): documento `Docs/02-Arte/MODELAGEM-BLENDER.md`. Em 14/11/2026 o usuário aprovou os
    refinamentos de arquitetura A a F e as duas divergências de referência, e respondeu: Blender 5.3, construção por script,
    processo técnico em detalhe, atlas do Bip e das telas por script, aparelho PC > celular > tablet. O plano em geral segue
    sem objeção; **a aprovação do blockout confirma também o plano.**
- **Em aberto:**
  - Refinamento G (precisa de ok): paredes da Estufa a z 3,2 e cumeeira a z 5,0 (eram 2,6 e 4,4), porque a verga da escotilha chega a z 2,88.
  - Proporção do capacete do astronauta (Ø 0,46 ou 0,42) → só bloqueia a modelagem dos personagens, hoje em espera.

## Escopo

Ver `Docs/02-Arte/MODELAGEM-BLENDER.md`, seção 2.

## Referências

24 imagens em `ref/` (índice em `ref/INDICE.md`). Princípios: blocos chanfrados e poucos vértices; branco, aço escuro e um acento por peça;
detalhe grande e legível; interior com faixa escura no pé da parede; terreno facetado.

## Plano

- **Intenção:** a base se entende de longe pela silhueta e pela cor, e convence de perto como estrutura de verdade.
- **Orçamento:** 120 mil triângulos no quadro (cerca de 107 mil previstos), 12 mil por objeto, 4 materiais por objeto (justificado: estados em
  tempo de execução), paleta de 256², UV0 colapsada no atlas, sem lightmap.
- **Superfície:** atlas de paleta; acento, luz e vidro em materiais próprios.
- **Movimento:** astronauta Humanoid (20 ossos), Bip Generic (11 ossos), prato e escotilhas por animação de objeto.
- **Estrutura e entrega:** um `.blend` por grupo, FBX por asset `_v001`; arquitetura em `MODELAGEM-BLENDER.md`, seção 4.6.
- **Processo técnico:** `MODELAGEM-BLENDER.md`, seção 4.9 · o usuário acompanha em detalhe: **sim**.

## Processos da produção

| Processo | Confirmado em | Observação |
| --- | --- | --- |
| C · Blockout do cenário | 14/11/2026 | `Montagem.blend`, folhas em `renders/C_cameras` |
| K · Kit comum: modelagem | | `Kit_Comum.blend`, folhas em `renders/K_cameras` e `renders/K_arame` |
| L1 · Blockout dos personagens (em espera) | | `Personagens.blend`, folhas em `renders/L1_*` |

## Orçamento × atual

| Item | Orçamento final | Blockout atual |
| --- | --- | --- |
| Astronauta (cada) | 4.500 triângulos | ≈ 510 a 545 |
| Bip | 2.500 | 528 |
| Cenário inteiro | ≈ 107.000 triângulos | 6.120 (215 malhas, blockout) |
| Kit comum (18 peças, 21 objetos) | 1.980 (soma das receitas) | 1.712 |
| Auditor | 0 erros | cenário: 0 erros, 45 avisos (UV provisória do blockout); personagens: 0 erros, 19 avisos |

## Pastas

| Pasta | O que tem |
| --- | --- |
| `Kit_Comum.blend` | kit R00; `fonte/construir.py --lote K` (peças em `fonte/pecas.py`, motor em `fonte/modelar.py`) |
| `Montagem.blend` | cenário; gerado por `fonte/construir.py --lote C` (padrão) |
| `Personagens.blend` | gerado por `fonte/construir.py --lote L5` (blockout dos personagens). Não edite à mão sem avisar: rodar o gerador refaz por cima |
| `fonte/` | `construir.py`, `kit.py`, `config.json`, `_comum.py` |
| `ref/` | imagens de referência |
| `tex/` | texturas finais (atlas de paleta, telas, rosto do Bip) |
| `export/` | FBX por asset |
| `renders/` | prévias, folhas e auditorias |

## Como refazer

```bash
BLENDER_VERSAO=5.2 rodar_blender.sh fonte/construir.py -- --cfg fonte/config.json --lote C        # cenário (L5 gera os personagens)
BLENDER_VERSAO=5.2 rodar_blender.sh auditar.py Montagem.blend -- --perfil perfil.json --saida renders/auditoria_C.json
BLENDER_VERSAO=5.2 rodar_blender.sh previa.py Montagem.blend -- --cameras todas --folha --saida <caminho absoluto>/renders/C_cameras
```

Use caminho **absoluto** na saída das prévias: o executor roda o Blender em outra pasta.
