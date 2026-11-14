# Gera os .blend da Estação-Roda por lote. É a fonte da verdade: para mudar uma medida, mude config.json e rode de novo.
#   BLENDER_VERSAO=5.2 rodar_blender.sh fonte/construir.py -- --cfg fonte/config.json --lote L1
# Lote C  = cenário (blockout montado: Montagem.blend). Primeiro na ordem de trabalho, por decisão do usuário.
# Lote L5 = personagens (astronauta e Bip): blockout pronto, em espera.
import sys, os, math
AQUI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, AQUI)
import bpy
from _comum import args, log, etapa
import kit
import cenario
import pecas

CFG = args()
RAIZ = os.path.abspath(os.path.join(AQUI, '..'))
C = CFG['cores']


def cena_base():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    sc = bpy.context.scene
    sc.unit_settings.system = 'METRIC'
    sc.unit_settings.scale_length = 1.0
    sc.render.fps = CFG.get('fps', 30)
    return sc


# ------------------------------------------------------------------ L1: personagens (blockout)

def astronauta(nome, x, acento, col, M, extras):
    """Blockout do astronauta em caixas e uma esfera. Frente para -Y, pés em z = 0, 1,8 m (sem antena)."""
    a = CFG['astronauta']
    P = []

    def parte(sufixo, tipo, mat, *a_, **k):
        o = tipo(f'SM_Blk_{nome}_{sufixo}', col, mat, *a_, **k)
        o.location.x += x
        P.append(o)
        return o

    qx = a['quadril_x']
    bota, canela, coxa, pelve = a['bota'], a['canela'], a['coxa'], a['pelve']
    z_canela = bota[2] + canela[2] / 2
    z_coxa = bota[2] + canela[2] + coxa[2] / 2
    z_pelve_topo = bota[2] + canela[2] + coxa[2] + 0.04          # 0,89
    z_pelve = z_pelve_topo - pelve[2] / 2
    tor = a['torso']
    z_torso = z_pelve_topo + tor[2] / 2 - 0.02
    z_torso_topo = z_torso + tor[2] / 2
    z_ombro = z_torso_topo - 0.08
    hd = a['capacete_diametro']
    z_capacete = a['altura'] - hd / 2

    for lado, s in (('L', 1), ('R', -1)):
        parte(f'Bota_{lado}', kit.caixa, M['escuro'], bota, (s * qx, -0.03, bota[2] / 2))
        parte(f'Canela_{lado}', kit.caixa, M['branco'], canela, (s * qx, 0, z_canela))
        parte(f'Coxa_{lado}', kit.caixa, M['branco'], coxa, (s * qx, 0, z_coxa))
        sup, inf, mao = a['braco_sup'], a['braco_inf'], a['mao']
        z_sup = z_ombro - sup[2] / 2
        z_inf = z_ombro - sup[2] - inf[2] / 2
        z_mao = z_ombro - sup[2] - inf[2] - mao[2] / 2
        parte(f'BracoSup_{lado}', kit.caixa, M['branco'], sup, (s * a['ombro_x'], 0, z_sup))
        parte(f'BracoInf_{lado}', kit.caixa, M['branco'], inf, (s * a['ombro_x'], 0, z_inf))
        parte(f'Mao_{lado}', kit.caixa, M['cinza'], mao, (s * a['ombro_x'], 0, z_mao))
        parte(f'Acento_Ombro_{lado}', kit.caixa, M[nome], (0.02, 0.12, 0.10), (s * (a['ombro_x'] + sup[0] / 2 + 0.01), 0, z_ombro - 0.10))
    parte('Pelve', kit.caixa, M['cinza'], pelve, (0, 0, z_pelve))
    parte('Torso', kit.caixa, M['branco'], tor, (0, 0, z_torso))
    parte('Peitoral', kit.caixa, M['cinza'], (0.24, 0.02, 0.16), (0, -tor[1] / 2 - 0.01, z_torso + 0.05))
    parte('Mochila', kit.caixa, M['cinza'], a['mochila'], (0, tor[1] / 2 + a['mochila'][1] / 2, z_torso - 0.02))
    parte('Pescoco_Anel', kit.caixa, M[nome], a['pescoco_anel'], (0, 0, z_torso_topo + a['pescoco_anel'][2] / 2))
    parte('Capacete', kit.elipsoide, M['branco'], (hd / 2, hd / 2, hd / 2), (0, 0, z_capacete), seg=(12, 8))
    parte('Viseira', kit.elipsoide, M['dourado'], (hd * 0.38, hd * 0.28, hd * 0.30), (0, -hd * 0.30, z_capacete + 0.01), seg=(10, 6))
    extras(parte, M, z_torso, z_capacete, z_ombro, a)
    return P


def acessorios_lia(parte, M, z_torso, z_capacete, z_ombro, a):
    parte('Cinto', kit.caixa, M['escuro'], (0.46, 0.30, 0.05), (0, 0, 0.93))
    parte('Chave_Inglesa', kit.caixa, M['cinza'], (0.06, 0.05, 0.50), (0.17, 0.36, z_torso), rot=(0, 12, 0))
    for s, l in ((1, 'L'), (-1, 'R')):
        parte(f'Lampada_{l}', kit.caixa, M['claro'], (0.05, 0.08, 0.06), (s * 0.235, -0.07, z_capacete + 0.02))


def acessorios_teo(parte, M, z_torso, z_capacete, z_ombro, a):
    parte('Maleta_Amostras', kit.caixa, M['escuro'], (0.26, 0.14, 0.20), (-0.30, 0.02, 0.84))


def acessorios_nina(parte, M, z_torso, z_capacete, z_ombro, a):
    parte('Mochila_Medica', kit.caixa, M['Nina'], (0.30, 0.08, 0.30), (0, 0.45, z_torso - 0.02))
    parte('Antena_Cruz', kit.cilindro, M['cinza'], 0.012, 0.22, (0, 0, z_capacete + 0.34), lados=6)


def acessorios_kai(parte, M, z_torso, z_capacete, z_ombro, a):
    parte('Tablet', kit.caixa, M['escuro'], (0.04, 0.20, 0.28), (-0.36, -0.02, z_ombro - 0.44))
    parte('Antena_Prato', kit.cilindro, M['cinza'], 0.012, 0.22, (0.10, 0, z_capacete + 0.34), lados=6)


def bip(nome, x, col, M):
    b = CFG['bip']
    P = []

    def parte(sufixo, tipo, mat, *a_, **k):
        o = tipo(f'SM_Blk_{nome}_{sufixo}', col, mat, *a_, **k)
        o.location.x += x
        P.append(o)
        return o

    rx, ry, rz = b['corpo_raios']
    z_c = b['esteira'][2] - 0.09 + rz                      # corpo apoiado logo acima das esteiras
    parte('Corpo', kit.elipsoide, M['branco'], (rx, ry, rz), (0, 0, z_c), seg=(14, 10))
    for s, l in ((1, 'L'), (-1, 'R')):
        parte(f'Esteira_{l}', kit.caixa, M['borracha'], b['esteira'], (s * b['esteira_x'], 0, b['esteira'][2] / 2))
    z_tela = z_c + 0.235
    parte('Tela', kit.caixa, M['tela'], (b['tela'][0], b['tela'][1], b['tela'][2]), (0, -0.25, z_tela))
    parte('Banda', kit.elipsoide, M['bip_banda'], (rx + 0.01, ry + 0.01, 0.035), (0, 0, z_c - 0.12), seg=(14, 4))
    z_ombro = z_c + 0.08
    for s, l in ((1, 'L'), (-1, 'R')):
        sup, inf, gr = b['braco_sup'], b['braco_inf'], b['garra']
        parte(f'BracoSup_{l}', kit.caixa, M['cinza'], sup, (s * 0.49, 0, z_ombro - sup[2] / 2))
        parte(f'BracoInf_{l}', kit.caixa, M['cinza'], inf, (s * 0.49, 0, z_ombro - sup[2] - inf[2] / 2))
        parte(f'Garra_{l}', kit.caixa, M['escuro'], gr, (s * 0.49, 0, z_ombro - sup[2] - inf[2] - gr[2] / 2))
        parte(f'Ombro_{l}', kit.caixa, M['cinza'], (0.20, 0.06, 0.06), (s * 0.40, 0, z_ombro))
    topo = z_c + rz
    ar, ah = b['antena']
    parte('Antena', kit.cilindro, M['cinza'], ar / 2, ah, (0, 0, topo + ah / 2), lados=6)
    parte('Antena_Ponta', kit.elipsoide, M['claro'], (0.03, 0.03, 0.03), (0, 0, topo + ah + 0.01), seg=(6, 4))
    return P


def lote_L5():
    etapa('cena')
    sc = cena_base()
    blk = kit.colecao('01 Blockout')
    M = {k: kit.material(f'MAT_Blk_{k}', C[k]) for k in ('branco', 'cinza', 'escuro', 'dourado', 'borracha', 'chao', 'bip_banda', 'tela')}
    for k in ('Lia', 'Teo', 'Nina', 'Kai'):
        M[k] = kit.material(f'MAT_Blk_{k}', C[k])
    M['claro'] = kit.material('MAT_Blk_claro', '#FFF3C4')

    etapa('escala')
    L = CFG['lineup']['x']
    cubo = kit.caixa('SM_Blk_Escala_Cubo_1m', blk, M['chao'], (1, 1, 1), (CFG['lineup']['cubo_x'], 0, 0.5))
    px = CFG['lineup']['porta_x']
    kit.caixa('SM_Blk_Escala_Porta_Batente_L', blk, M['chao'], (0.10, 0.10, 2.10), (px - 0.6, 0, 1.05))
    kit.caixa('SM_Blk_Escala_Porta_Batente_R', blk, M['chao'], (0.10, 0.10, 2.10), (px + 0.6, 0, 1.05))
    kit.caixa('SM_Blk_Escala_Porta_Verga', blk, M['chao'], (1.30, 0.10, 0.10), (px, 0, 2.05))
    kit.caixa('SM_Blk_Chao', blk, M['chao'], (12.0, 4.0, 0.04), (-0.6, 0, -0.02))

    etapa('astronautas')
    todos = []
    for nome, extras in (('Lia', acessorios_lia), ('Teo', acessorios_teo), ('Nina', acessorios_nina), ('Kai', acessorios_kai)):
        partes = astronauta(nome, L[nome], nome, blk, M, extras)
        todos += partes
        zmax = max(o.location.z + o.dimensions.z / 2 for o in partes if o.name.split('_')[3] in ('Capacete',))
        log(f'{nome}: {len(partes)} blocos, topo do capacete a {zmax:.3f} m (alvo {CFG["astronauta"]["altura"]:.2f})')

    etapa('bip')
    bpartes = bip('Bip', L['Bip'], blk, M)
    todos += bpartes
    topo = max(o.location.z + o.dimensions.z / 2 for o in bpartes)
    log(f'Bip: {len(bpartes)} blocos, altura total {topo:.3f} m (corpo + antena)')

    etapa('cameras')
    ca = CFG['cameras']
    d, inc = ca['andar']['distancia'], math.radians(ca['andar']['inclinacao'])
    al = (L['Lia'], 0, 1.0)
    kit.camera('CAM_01_Andar', blk, (al[0], al[1] + d * math.cos(inc), al[2] + d * math.sin(inc)), al, ca['andar']['fov'])
    at = (L['Teo'], 0, 1.1)
    kit.camera('CAM_03_Close', blk, (at[0] - 1.5, -2.3, 0.55), at, ca['close']['fov'])
    kit.camera('CAM_05_Elenco', blk, ca['elenco']['pos'], ca['elenco']['alvo'], ca['elenco']['fov'])

    etapa('orcamento')
    linhas, total = kit.tris_por_nome(todos)
    por = {}
    for nome, t in linhas:
        por.setdefault(nome.split('_')[2], 0)
        por[nome.split('_')[2]] += t
    for k, t in por.items():
        orc = 4500 if k != 'Bip' else 2500
        log(f'  {k}: {t} triângulos de blockout (orçamento final {orc})')
    log(f'total do blockout: {total} triângulos')

    saida = os.path.join(RAIZ, 'Personagens.blend')
    bpy.ops.wm.save_as_mainfile(filepath=saida)
    log(f'salvo: {saida}')


def lote_C():
    etapa('cena')
    cena_base()
    blk = kit.colecao('01 Blockout')
    S = cenario.Cena(CFG, blk)
    A = CFG['arquitetura']
    etapa('terreno e estrutura')
    S.terreno()
    S.hub()
    S.eclusa()
    corredores = {}
    for k in ('air', 'green', 'battery', 'shelter', 'lab'):
        corredores[k] = S.corredor(k)
    S.boneco('Corredor_Ar', corredores['air'].children[0].users_collection[0], corredores['air'], (0, -5.0, S.piso))
    etapa('modulos')
    raizes = {'air': S.ar(), 'green': S.estufa(), 'battery': S.bateria(), 'shelter': S.abrigo(), 'lab': S.lab()}
    etapa('apoio')
    S.apoio()

    etapa('cameras')
    ca = CFG['cameras']
    d, inc = ca['andar']['distancia'], math.radians(ca['andar']['inclinacao'])
    alvo = kit.mundo_de(corredores['air'], (0, -5.0, S.piso + 0.9))[:]
    kit.camera('CAM_01_Andar', blk, (alvo[0], alvo[1] - d * math.cos(inc), alvo[2] + d * math.sin(inc)), alvo, ca['andar']['fov'])
    dm, im = ca['mapa']['distancia'], math.radians(ca['mapa']['inclinacao'])
    kit.camera('CAM_02_Mapa', blk, (0, -dm * math.cos(im), dm * math.sin(im)), (0, 0, 0), ca['mapa']['fov'])
    kit.camera('CAM_03_Geral', blk, ca['geral']['pos'], ca['geral']['alvo'], ca['geral']['fov'])
    kit.camera('CAM_04_Interior_Hub', blk, ca['interior_hub']['pos'], ca['interior_hub']['alvo'], ca['interior_hub']['fov'])
    g = raizes['green']
    kit.camera('CAM_06_Topo', blk, (0.001, 0, ca['topo']['altura']), (0, 0, 0), 30, ortografica=ca['topo']['escala'])
    kit.camera('CAM_05_Interior_Estufa', blk, kit.mundo_de(g, (-5.0, 3.6, 2.1)), kit.mundo_de(g, (3.0, 3.6, 1.0)), 60)

    etapa('orcamento')
    total = 0
    for sub in blk.children:
        tris = sum(kit.tris_por_nome([o for o in sub.objects])[1:])
        total += tris
        log(f'  {sub.name}: {tris} triângulos de blockout, {len([o for o in sub.objects if o.type == "MESH"])} blocos')
    log(f'total do blockout: {total} triângulos')
    saida = os.path.join(RAIZ, 'Montagem.blend')
    bpy.ops.wm.save_as_mainfile(filepath=saida)
    log(f'salvo: {saida}')


def _limites_x(objs):
    xs = []
    for o in objs:
        for v in o.data.vertices:
            xs.append((o.matrix_world @ v.co).x)
    return min(xs), max(xs)


def lote_K():
    """R00 Kit comum: modelagem real das peças (Kit_Comum.blend). Cada peça na origem de montagem; aqui só se espalham para conferir."""
    import json
    from mathutils import Vector
    etapa('cena')
    cena_base()
    col = kit.colecao('02 Kit')
    teste = kit.colecao('_Teste')
    M = pecas.materiais(CFG)
    etapa('pecas')
    todas = pecas.construir_kit(CFG, col, M)
    por_nome = {o.name: o for grupo in todas for o in grupo}
    lay = CFG['layout_kit']
    etapa('layout')
    cursor = 0.0
    cams = []
    for gi, nomes in enumerate(lay['grupos']):
        x0 = cursor
        cursor += 1.2                                     # espaço do boneco de escala
        for nome in nomes:
            partes = [por_nome[nome]] + [o for n, o in por_nome.items() if n.startswith(nome + '_') and n not in nomes]
            lo, hi = _limites_x(partes)
            dz = lay['z'].get(nome, 0.0)
            dx = cursor - lo
            for o in partes:
                o.location = Vector((o.location.x + dx, o.location.y, o.location.z + dz))
            cursor += (hi - lo) + 0.9
        kit.caixa(f'SM_Teste_Boneco_{"ABCD"[gi]}', teste, kit.material('MAT_Pt_37_Amarelo_Seguranca', '#E8B923'),
                  (0.5, 0.3, 1.8), (x0 + 0.4, 0, 0.9))
        cx, larg = (x0 + cursor) / 2, cursor - x0
        d = max(larg * 0.78, 7.0)
        alvo = (cx, 0, 1.3)
        kit.camera(f'CAM_0{gi + 1}_Kit_{"ABCD"[gi]}', teste, (cx + d * 0.30, -d * 0.95, 1.3 + d * 0.30), alvo, 40)
        cursor += 2.0
    etapa('orcamento')
    orc = CFG['orcamento_kit']
    total = 0
    for nome, o in sorted(por_nome.items()):
        base = [n for n in orc if nome == n or nome.startswith(n + '_')]
        if not base:
            continue
    for grupo in todas:
        tr = sum(kit.tris_por_nome(grupo)[1:])
        nome = grupo[0].name
        total += tr
        bud = orc.get(nome, 0)
        log(f'  {nome}: {tr} tris (receita: {bud}) {"ESTOURO" if tr > bud else ""}')
    log(f'total do kit: {total} triângulos, {len(por_nome)} objetos')
    saida = os.path.join(RAIZ, 'Kit_Comum.blend')
    bpy.ops.wm.save_as_mainfile(filepath=saida)
    log(f'salvo: {saida}')


LOTES = {'C': lote_C, 'L5': lote_L5, 'K': lote_K}

if __name__ == '__main__':
    lote = CFG.get('lote', 'C')
    if lote not in LOTES:
        raise SystemExit(f'lote {lote} ainda não tem blockout (só: {", ".join(LOTES)})')
    LOTES[lote]()
