# Utilidades dos scripts da skill. Tudo aqui roda DENTRO do Blender (bpy), chamado pelo rodar_blender.sh:
#   blender -b [cena.blend] --factory-startup --python-exit-code 1 --python <script>.py -- --log x.txt --chave valor
# O launcher da Microsoft Store não repassa o console, por isso o log vai para arquivo (e também para o stdout).
import sys, os, json, time

_T0 = time.time()
_LOG = None


def args():
    """Argumentos depois de '--': '--chave valor' vira {'chave': 'valor'}, '--flag' sozinho vira True.
    '--cfg arquivo.json' mescla um JSON (os da linha de comando ganham). Números viram int/float."""
    bruto = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    d, i = {}, 0
    while i < len(bruto):
        k = bruto[i]
        if not k.startswith('--'):
            d.setdefault('_', []).append(k)
            i += 1
            continue
        k = k[2:].replace('-', '_')
        if i + 1 < len(bruto) and not bruto[i + 1].startswith('--'):
            d[k] = _numero(bruto[i + 1])
            i += 2
        else:
            d[k] = True
            i += 1
    if 'cfg' in d:
        # utf-8-sig: JSON escrito pelo PowerShell 5.1 vem com BOM e o json.load puro recusa
        with open(d['cfg'], encoding='utf-8-sig') as f:
            base = json.load(f)
        base.update(d)
        d = base
    global _LOG
    if d.get('log') and _LOG is None:
        os.makedirs(os.path.dirname(os.path.abspath(d['log'])), exist_ok=True)
        _LOG = open(d['log'], 'w', encoding='utf-8')
        sys.excepthook = _excecao_no_log
    # versão-alvo do .blend: a do executor (BLENDER_VERSAO) E a do config do projeto ("blender") têm de bater
    # com este Blender — senão um BLENDER_VERSAO=5.2 esquecido salvaria por cima um projeto combinado em 4.3
    for quero in (os.environ.get('BLENDER_VERSAO'), d.get('blender')):
        if quero:
            conferir_versao(quero)
    return d


def versao():
    import bpy
    return tuple(bpy.app.version[:2])


def conferir_versao(quero):
    """Recusa rodar em outra versão: um .blend salvo no 5.x não abre no 4.3 (e 'salvar como' não converte)."""
    import bpy
    try:
        q = tuple(int(x) for x in str(quero).split('.')[:2])
    except ValueError:
        raise RuntimeError(f'versão do Blender ainda não combinada ("blender": "{quero}" no config). Pergunte ao '
                           'usuário se o .blend é para o 4.3 ou o 5.2 e grave a resposta (references/versao-blender.md).') from None
    if versao() != q:
        raise RuntimeError(f'o projeto pede Blender {quero}, mas este é o {bpy.app.version_string}. '
                           f'Rode com BLENDER_VERSAO={quero} (rodar_blender.sh --listar mostra os instalados).')


# ------------------------------------------------------------------ compatibilidade 4.3 / 5.x

def material_novo(nome):
    """Material com nós nas duas versões: no 4.3 materials.new() nasce SEM node_tree (use_nodes = False);
    no 5.x é sempre com nós e use_nodes está obsoleto."""
    import bpy
    m = bpy.data.materials.new(nome)
    if m.node_tree is None:
        m.use_nodes = True
    return m


def motor_eevee(cena):
    """'BLENDER_EEVEE_NEXT' no 4.2–4.4, 'BLENDER_EEVEE' no 5.x."""
    for nome in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT'):
        try:
            cena.render.engine = nome
            return nome
        except TypeError:
            continue
    raise RuntimeError('Eevee não encontrado')


def fcurves(acao, dono=None):
    """F-curves de uma ação: no 4.3 é acao.fcurves; no 4.4+ (ações com slots) moram no channelbag do slot,
    e no 5.x acao.fcurves nem existe. dono = objeto que usa a ação (para achar o slot dele)."""
    if not hasattr(acao, 'slots'):
        return acao.fcurves
    from bpy_extras import anim_utils
    slot = dono.animation_data.action_slot if dono and dono.animation_data else None
    slot = slot or (acao.slots[0] if len(acao.slots) else None)
    cb = anim_utils.action_get_channelbag_for_slot(acao, slot) if slot else None
    return cb.fcurves if cb else []


def compositor(cena):
    """Compositor pronto nas duas APIs: devolve (árvore, nó Render Layers, nó de saída) já ligados.
    4.x: scene.use_nodes + scene.node_tree com Composite. 5.x: node group em scene.compositing_node_group
    com Group Output. Para inserir um efeito: ligue camadas -> efeito -> saida.inputs[0]."""
    import bpy
    if hasattr(cena, 'compositing_node_group'):
        ng = cena.compositing_node_group
        if ng is None:
            ng = bpy.data.node_groups.new('Compositor', 'CompositorNodeTree')
            ng.interface.new_socket('Image', in_out='OUTPUT', socket_type='NodeSocketColor')
            cena.compositing_node_group = ng
        camadas = next((n for n in ng.nodes if n.bl_idname == 'CompositorNodeRLayers'), None) or ng.nodes.new('CompositorNodeRLayers')
        saida = next((n for n in ng.nodes if n.bl_idname == 'NodeGroupOutput'), None) or ng.nodes.new('NodeGroupOutput')
    else:
        cena.use_nodes = True
        ng = cena.node_tree
        camadas = next((n for n in ng.nodes if n.bl_idname == 'CompositorNodeRLayers'), None) or ng.nodes.new('CompositorNodeRLayers')
        saida = next((n for n in ng.nodes if n.bl_idname == 'CompositorNodeComposite'), None) or ng.nodes.new('CompositorNodeComposite')
    if not saida.inputs[0].is_linked:
        ng.links.new(camadas.outputs['Image'], saida.inputs[0])
    return ng, camadas, saida


def _excecao_no_log(tipo, valor, tb):
    """O launcher da Store descarta o stderr: sem isto, o erro do script some e só sobra o código de saída."""
    import traceback
    log('ERRO\n' + ''.join(traceback.format_exception(tipo, valor, tb)).rstrip())
    sys.__excepthook__(tipo, valor, tb)


def _numero(s):
    for tipo in (int, float):
        try:
            return tipo(s)
        except ValueError:
            pass
    return s


def lista(v):
    """'a,b,c' ou ['a','b'] ou None -> lista de strings."""
    if v is None or v is True:
        return []
    if isinstance(v, (list, tuple)):
        return [str(x) for x in v]
    return [x.strip() for x in str(v).split(',') if x.strip()]


def log(*a):
    linha = ' '.join(str(x) for x in a)
    print(linha, flush=True)
    if _LOG:
        _LOG.write(linha + '\n')
        _LOG.flush()


def etapa(nome):
    log(f'[{time.time() - _T0:7.1f}s] {nome}')


def malhas(cena=None, so_visiveis=True):
    """Objetos de malha da cena (visíveis no view layer, se pedido)."""
    import bpy
    cena = cena or bpy.context.scene
    vl = bpy.context.view_layer
    return [o for o in cena.objects if o.type == 'MESH' and (not so_visiveis or o.visible_get(view_layer=vl))]


def triangulos(ob, avaliado=True):
    """Triângulos da malha; com avaliado=True conta depois dos modificadores (o que o export vai gerar)."""
    import bpy
    import numpy as np
    if avaliado:
        dg = bpy.context.evaluated_depsgraph_get()
        me = ob.evaluated_get(dg).data
    else:
        me = ob.data
    lt = np.empty(len(me.polygons), np.int32)
    me.polygons.foreach_get('loop_total', lt)
    return int((lt - 2).sum())


def salvar_json(caminho, dados):
    os.makedirs(os.path.dirname(os.path.abspath(caminho)), exist_ok=True)
    with open(caminho, 'w', encoding='utf-8') as f:
        json.dump(dados, f, indent=1, ensure_ascii=False)
