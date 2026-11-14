# Kit comum (R00): peças reutilizadas em todos os módulos. Medidas em config.json["kit"]; nada de número solto aqui
# além de proporções de detalhe (espessuras de ripa, folga de parafuso) que não são arquitetura.
# Origem de cada peça = ponto de montagem. Frente = -Y. Tudo em quads (triângulo só em tampa pequena).
import math
from mathutils import Vector
import bpy
import kit as K
from modelar import Peca


# ------------------------------------------------------------------ materiais da produção

def materiais(cfg):
    """MAT_Pt_<nn>_<Nome> (atlas de paleta), MAT_Accent_*, MAT_Glow_*, MAT_Glass. Ficam em dict pelas chaves usadas nas peças."""
    M = {}
    for (nn, nome, hexa, rug, met) in cfg['paleta']:
        M[f'pt{nn}'] = K.material(f'MAT_Pt_{nn}_{nome}', hexa, rug, met)
    for k, hexa in cfg['acentos'].items():
        M[f'acc_{k}'] = K.material(f'MAT_Accent_{k}', hexa, 0.55, 0.0)
    for k, hexa in cfg['glow'].items():
        m = K.material(f'MAT_Glow_{k}', hexa, 0.5, 0.0)
        p = m.node_tree.nodes['Principled BSDF']
        p.inputs['Emission Color'].default_value = (*K.lin(hexa), 1)
        p.inputs['Emission Strength'].default_value = 2.0
        M[f'glow_{k}'] = m
    g = K.material('MAT_Glass', cfg['vidro']['cor'], 0.1, 0.0)
    p = g.node_tree.nodes['Principled BSDF']
    p.inputs['Alpha'].default_value = cfg['vidro']['alfa']
    g.diffuse_color = (*K.lin(cfg['vidro']['cor']), cfg['vidro']['alfa'])
    try:
        g.surface_render_method = 'BLENDED'
    except Exception:
        pass
    g.use_backface_culling = False
    M['vidro'] = g
    return M


def _tubo_x(P, raio, x0, x1, y, z, lados, mat, tampas=(True, True)):
    P.cilindro(raio, x1 - x0, ((x0 + x1) / 2, y, z), lados, mat, eixo='X', tampas=tampas)


# ------------------------------------------------------------------ peças

def escotilha(col, M, c):
    """Anel de 12 lados (perfil girado), vão Ø2,2, moldura 0,25, prof. 0,30. Acento nas faixas internas; 4 parafusos por face."""
    r0 = c['vao'] / 2
    r1 = r0 + c['quadro']
    rm = r0 + c['quadro'] * 0.6
    hp = c['prof'] / 2            # meia profundidade
    hb = hp - 0.10                # faixa externa mais baixa
    P = Peca('SM_Kit_Escotilha', col)
    perfil = [(r0, -hp), (rm, -hp), (rm, -hb), (r1, -hb), (r1, hb), (rm, hb), (rm, hp), (r0, hp), (r0, -hp)]
    # acento só nas faixas internas (as duas bandas elevadas); degrau e faixa externa em branco; parede do vão em aço
    mats = [M['acc_Neutral'], M['pt01'], M['pt01'], M['pt01'], M['pt01'], M['pt01'], M['acc_Neutral'], M['pt04']]
    P.revolucao(perfil, c['lados'], mats, eixo='Y')
    rp = (rm + r1) / 2
    for k in range(c['parafusos']):
        a = math.radians(45 + 90 * k)
        for s in (-1, 1):
            P.cilindro(0.035, 0.03, (rp * math.cos(a), s * (hb + 0.015), -rp * math.sin(a)), 4, M['pt05'], eixo='Y',
                       tampas=(s < 0, s > 0))
    return [P.objeto()]


def escotilha_porta(col, M, c, e):
    """Disco de 12 lados + roda de 6 raios na face -Y. Origem na dobradiça (borda esquerda)."""
    r = c['diam'] / 2
    h = c['espessura'] / 2
    P = Peca('SM_Kit_Escotilha_Porta', col)
    off = (r, 0, 0)
    P.revolucao([(0, -h), (r, -h), (r, h), (0, h)], c['lados'], [M['pt01']] * 3, 'Y', off)
    rim = [(0.42, -h - 0.06), (0.50, -h - 0.06), (0.50, -h), (0.42, -h), (0.42, -h - 0.06)]
    P.revolucao(rim, c['lados'], [M['pt05'], M['pt05'], False, M['pt05']], 'Y', off)
    for k in range(c['raios']):
        a = 2 * math.pi * k / c['raios']
        rr = 0.25
        P.caixa((0.36, 0.05, 0.05), (off[0] + rr * math.cos(a), -h - 0.03, -rr * math.sin(a)), M['pt05'],
                rot=(0, math.degrees(a), 0), sem=('-x', '+x'))
    P.cilindro(0.09, 0.07, (off[0], -h - 0.035, 0), 6, M['pt05'], eixo='Y', tampas=(True, False))
    return [P.objeto()]


def corrimao(col, M, c):
    """1 m ao longo de +X, origem no pé do 1º montante. Tubo Ø0,05 com o eixo a 1,0 m do piso."""
    P = Peca('SM_Kit_Corrimao_1m', col)
    L, H, m = c['comprimento'], c['altura'], c['montante']
    for x in (m / 2, L - m / 2):
        P.caixa((m, m, H - c['tubo_raio']), (x, 0, (H - c['tubo_raio']) / 2), M['pt04'], sem=('+z', '-z'))
    _tubo_x(P, c['tubo_raio'], 0, L, 0, H, c['lados'], M['pt04'])
    return [P.objeto()]


def escada_vertical(col, M, c):
    """2 trilhos e 8 degraus a cada 0,30. Origem no pé, centrada; sobe em +Z."""
    P = Peca('SM_Kit_Escada_Vertical', col)
    t, w, hh = c['trilho'], c['largura'], c['altura']
    xr = w / 2 - t / 2
    for s in (-1, 1):
        P.caixa((t, t, hh), (s * xr, 0, hh / 2), M['pt04'])
    n = int(round(hh / c['degrau_passo']))
    for i in range(n):
        P.caixa((w - 2 * t, 0.03, 0.03), (0, 0, c['degrau_passo'] / 2 + i * c['degrau_passo']), M['pt04'], sem=('-x', '+x'))
    return [P.objeto()]


def escada_degrau(col, M, c, corr):
    """Degraus de 0,20 × 0,25, largura 1,0, sobe para +Y. Só faces visíveis (sem face interna); corrimão de um lado."""
    n, hr, run, w = c['degraus'], c['altura'], c['profundidade'], c['largura']
    P = Peca('SM_Kit_Escada_Degrau', col)
    cache = {}

    def V(x, y, z):
        k = (round(x, 4), round(y, 4), round(z, 4))
        if k not in cache:
            cache[k] = P.vert((x, y, z))
        return cache[k]
    xl, xr = -w / 2, w / 2
    yn = n * run
    mat, mat2 = M['pt04'], M['pt05']
    for j in range(n):                                   # coluna j: de y_j a y_j+1
        y0, y1 = j * run, (j + 1) * run
        zt = (j + 1) * hr
        P.face([V(xl, y0, zt), V(xr, y0, zt), V(xr, y1, zt), V(xl, y1, zt)], mat, ref=(0, 0, 1))              # piso do degrau
        P.face([V(xl, y0, j * hr), V(xr, y0, j * hr), V(xr, y0, zt), V(xl, y0, zt)], mat2, ref=(0, -1, 0))   # espelho
        P.face([V(xl, y0, 0), V(xr, y0, 0), V(xr, y1, 0), V(xl, y1, 0)], mat, ref=(0, 0, -1))                # fundo
        for i in range(j + 1):                           # laterais em grade: sem T-junction, sem N-gon
            za, zb = i * hr, (i + 1) * hr
            for sx, ref in ((xl, (-1, 0, 0)), (xr, (1, 0, 0))):
                P.face([V(sx, y0, za), V(sx, y1, za), V(sx, y1, zb), V(sx, y0, zb)], mat, ref=ref)
    for i in range(n):                                   # costas
        P.face([V(xl, yn, i * hr), V(xr, yn, i * hr), V(xr, yn, (i + 1) * hr), V(xl, yn, (i + 1) * hr)], mat, ref=(0, 1, 0))
    # corrimão inclinado no lado esquerdo, paralelo à linha dos degraus
    xc = xl + 0.05
    ya, yb = run / 2, (n - 0.5) * run
    za, zb = hr + corr['altura'], n * hr + corr['altura'] - 0.0
    for y, ztr in ((ya, hr), (yb, n * hr)):
        P.caixa((corr['montante'], corr['montante'], corr['altura'] - corr['tubo_raio']), (xc, y, ztr + (corr['altura'] - corr['tubo_raio']) / 2),
                M['pt04'], sem=('+z', '-z'))
    L = math.hypot(yb - ya, zb - za)
    ang = -math.degrees(math.atan2(yb - ya, zb - za))
    P.cilindro(corr['tubo_raio'], L, (xc, (ya + yb) / 2, (za + zb) / 2), corr['lados'], M['pt04'], rot=(ang, 0, 0))
    return [P.objeto()]


def grade_piso(col, M, c):
    """Chapa 1×1: borda escura de 0,05 e 3 ripas claras no fundo. Origem no centro da face de cima."""
    s, hh, b = c['lado'], c['altura'], c['borda']
    P = Peca('SM_Kit_Grade_Piso_1x1', col)
    ho, hi = s / 2, s / 2 - b
    zt, zb, zf = 0.0, -hh, -0.02

    def anel(z, h):
        return [P.vert((x, y, z)) for (x, y) in ((-h, -h), (h, -h), (h, h), (-h, h))]
    ob, ot, it, fl = anel(zb, ho), anel(zt, ho), anel(zt, hi), anel(zf, hi)
    P.face(ob[::-1], M['pt05'], ref=(0, 0, -1))
    for i in range(4):
        j = (i + 1) % 4
        P.face([ob[i], ob[j], ot[j], ot[i]], M['pt05'], ref=((ot[i].co + ot[j].co) / 2 - Vector((0, 0, 0))))
        P.face([ot[i], ot[j], it[j], it[i]], M['pt05'], ref=(0, 0, 1))
        P.face([it[i], it[j], fl[j], fl[i]], M['pt05'], ref=-((it[i].co + it[j].co) / 2))
    P.face(fl, M['pt04'], ref=(0, 0, 1))
    for y in (-0.3, 0.0, 0.3):
        P.caixa((s - 2 * b - 0.004, 0.06, zt - zf), (0, y, (zt + zf) / 2), M['pt04'])      # caixa fechada: nenhuma aresta compartilhada com o fundo
    return [P.objeto()]


def _perfil_calha(w, h, t):
    """U aberto, sentido horário no plano (u, v)."""
    return [(-w / 2, 0), (-w / 2, h), (-w / 2 + t, h), (-w / 2 + t, t), (w / 2 - t, t), (w / 2 - t, h), (w / 2, h), (w / 2, 0), (-w / 2, 0)]


def _hex_horario(r, cu, cv, n=6):
    return [(cu + r * math.cos(-2 * math.pi * i / n), cv + r * math.sin(-2 * math.pi * i / n)) for i in range(n)] + \
           [(cu + r, cv)]


def bandeja_cabos(col, M, c, curva=False):
    """Calha U de 0,40 × 0,08 com 2 cabos (Ø0,06, 6 lados). Reta de 1 m ao longo de +X, ou curva de 90° (2 trechos)."""
    nome = 'SM_Kit_Bandeja_Cabos_Curva' if curva else 'SM_Kit_Bandeja_Cabos_1m'
    P = Peca(nome, col)
    w, h, t, r = c['largura'], c['altura'], c['parede'], c['cabo_raio']
    if curva:
        R, n = c['curva_raio'], 2
        est = []
        for k in range(n + 1):
            th = math.radians(90 * k / n)
            T = Vector((math.cos(th), math.sin(th), 0))
            est.append((Vector((R * math.sin(th), R * (1 - math.cos(th)), 0)), Vector((0, 0, 1)).cross(T), Vector((0, 0, 1))))
    else:
        L = c['comprimento']
        est = [(Vector((0, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))), (Vector((L, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))]
    P.varredura(_perfil_calha(w, h, t), est, [M['pt05']] * 8)
    for cu in (-0.08, 0.08):
        P.varredura(_hex_horario(r, cu, t + r, 6), est, [M['pt06']] * 6)
    return [P.objeto()]


def valvula(col, M, c):
    """Roda de 8 raios na ponta de um tubo, saindo da parede para -Y. Acento nos raios."""
    r = c['diam'] / 2
    P = Peca('SM_Kit_Valvula', col)
    P.cilindro(0.05, 0.20, (0, -0.10, 0), 8, M['pt04'], eixo='Y', tampas=(False, False))
    yw = -0.215
    P.cilindro(0.07, 0.05, (0, yw - 0.01, 0), 8, M['pt05'], eixo='Y', tampas=(True, False))
    rim = [(r - 0.04, yw - 0.015), (r, yw - 0.015), (r, yw + 0.015), (r - 0.04, yw + 0.015), (r - 0.04, yw - 0.015)]
    P.revolucao(rim, c['raios'], [M['acc_Neutral'], M['acc_Neutral'], False, M['acc_Neutral']], 'Y')       # sem a face de trás (fica colada no raio)
    for k in range(c['raios']):
        a = 2 * math.pi * k / c['raios']
        rr = (0.07 + r - 0.04) / 2
        P.caixa((r - 0.04 - 0.07, 0.03, 0.03), (rr * math.cos(a), yw, -rr * math.sin(a)), M['acc_Neutral'],
                rot=(0, math.degrees(a), 0), sem=('-x', '+x', '+y'))
    return [P.objeto()]


def poste_luz(col, M, c):
    """Mastro de 8 lados com base alargada, cabeça em caixa e placa de luz separada (_Glow). Origem no pé."""
    H = c['altura']
    ch = c['cabeca']
    P = Peca('SM_Kit_Poste_Luz', col)
    rm = c['mastro_raio']
    zc = H - ch[2] / 2
    perfil = [(0, 0), (0.14, 0), (0.14, 0.05), (rm, 0.12), (rm, zc - ch[2] / 2)]
    P.revolucao(perfil, c['lados'], [M['pt05'], M['pt05'], M['pt05'], M['pt04']], 'Z')
    P.caixa(tuple(ch), (0, 0, zc), M['pt01'])
    G = Peca('SM_Kit_Poste_Luz_Glow', col)
    G.caixa((ch[0] - 0.06, ch[1] - 0.06, 0.01), (0, 0, zc - ch[2] / 2 - 0.005), M['glow_Neutral'])
    return [P.objeto(), G.objeto()]


def respiro(col, M, c):
    """Caixa 0,6 × 0,6 com 6 venezianas inclinadas na frente (-Y). Origem no encosto da parede."""
    s, pr, n = c['lado'], c['prof'], c['venezianas']
    P = Peca('SM_Kit_Respiro', col)
    P.caixa((s, pr, s), (0, -pr / 2, 0), M['pt02'], sem=('+y',))
    for i in range(n):
        z = -s / 2 + s * (i + 0.5) / n
        P.caixa((s - 0.1, 0.02, s / n * 0.7), (0, -pr - 0.005, z), M['pt05'], rot=(25, 0, 0), sem=('-x', '+x'))
    return [P.objeto()]


def janela_redonda(col, M, c):
    """Aro de 8 lados + vidro. Origem no centro do aro, no plano da parede; sobressai para -Y."""
    r = c['diam'] / 2
    P = Peca('SM_Kit_Janela_Redonda', col)
    pr = c['prof']
    ri = r - c['aro']
    P.revolucao([(ri, -pr), (r, -pr), (r, 0), (ri, 0), (ri, -pr)], c['lados'], [M['pt04']] * 4, 'Y')
    G = Peca('SM_Kit_Janela_Redonda_Vidro', col)
    G.revolucao([(0, 0), (ri, 0)], c['lados'], [M['vidro']], 'Y')
    return [P.objeto(), G.objeto(loc=(0, -pr * 0.4, 0))]


def janela_retangular(col, M, c):
    """Moldura de aço 1,5 × 1,0 (aro 0,10, prof. 0,10) e vidro. Origem no centro, no plano da parede; sobressai para -Y."""
    w, h = c['tam']
    a, pr = c['aro'], c['prof']
    P = Peca('SM_Kit_Janela_Retangular', col)

    def ret(hw, hh, y):
        return [P.vert((x, y, z)) for (x, z) in ((-hw, -hh), (hw, -hh), (hw, hh), (-hw, hh))]
    of, if_, ib, ob = ret(w / 2, h / 2, -pr), ret(w / 2 - a, h / 2 - a, -pr), ret(w / 2 - a, h / 2 - a, 0), ret(w / 2, h / 2, 0)
    for i in range(4):
        j = (i + 1) % 4
        mid_o = (of[i].co + of[j].co) / 2
        P.face([of[i], of[j], if_[j], if_[i]], M['pt04'], ref=(0, -1, 0))
        P.face([ob[i], ob[j], of[j], of[i]], M['pt04'], ref=Vector((mid_o.x, 0, mid_o.z)))
        P.face([if_[i], if_[j], ib[j], ib[i]], M['pt05'], ref=-Vector((mid_o.x, 0, mid_o.z)))
        P.face([ib[i], ib[j], ob[j], ob[i]], M['pt04'], ref=(0, 1, 0))
    G = Peca('SM_Kit_Janela_Retangular_Vidro', col)
    hw, hh = w / 2 - a, h / 2 - a
    G.face([G.vert((-hw, 0, -hh)), G.vert((hw, 0, -hh)), G.vert((hw, 0, hh)), G.vert((-hw, 0, hh))], M['vidro'], ref=(0, -1, 0))
    return [P.objeto(), G.objeto(loc=(0, -pr * 0.4, 0))]


def armario(col, M, c):
    """Caixa chanfrada 0,9 × 0,5 × 2,0 com 2 portas e 4 venezianas na frente. Origem no centro da base."""
    x, y, z = c['tam']
    P = Peca('SM_Kit_Armario', col)
    P.caixa((x, y, z), (0, 0, z / 2), M['pt02'], bisel=c['bisel'])
    for s in (-1, 1):
        P.caixa((x / 2 - 0.05, 0.02, z - 0.15), (s * (x / 4), -y / 2 - 0.01, z / 2), M['pt01'], sem=('+y',))
        for zz in (1.72, 1.62):
            P.caixa((0.3, 0.015, 0.05), (s * (x / 4), -y / 2 - 0.028, zz), M['pt05'], rot=(25, 0, 0), sem=('-x', '+x'))
    return [P.objeto()]


def bancada(col, M, c):
    """Tampo chanfrado, bloco de 3 gavetas à esquerda e painel de pé à direita. 2,0 × 0,8 × 0,9. Origem no centro da base."""
    x, y, z = c['tam']
    tp = c['tampo']
    P = Peca('SM_Kit_Bancada', col)
    P.caixa((x, y, tp), (0, 0, z - tp / 2), M['pt02'], bisel=c['bisel'])
    hb = z - tp
    P.caixa((0.8, y - 0.1, hb), (-x / 2 + 0.45, 0, hb / 2), M['pt01'], sem=('+z', '-z'))
    for i in range(3):
        P.caixa((0.74, 0.02, hb / 3 - 0.05), (-x / 2 + 0.45, -(y - 0.1) / 2 - 0.01, hb * (i + 0.5) / 3), M['pt04'], sem=('+y',))
    P.caixa((0.06, y - 0.1, hb), (x / 2 - 0.08, 0, hb / 2), M['pt05'], sem=('+z', '-z'))
    return [P.objeto()]


def caixa(col, M, c, nome, s):
    """Caixa chanfrada de s de largura (0,8 s de fundo, 0,7 s de altura) com 2 cintas. Origem no centro da base."""
    d, hh = s * 0.8, s * 0.7
    P = Peca(f'SM_Kit_Caixa_{nome}', col)
    P.caixa((s, d, hh), (0, 0, hh / 2), M['pt02'], bisel=c['bisel'])
    for x in (-0.28 * s, 0.28 * s):
        P.caixa((c['cinta'], d + 0.02, hh + 0.02), (x, 0, hh / 2), M['pt05'])
    return [P.objeto()]


# ------------------------------------------------------------------ montagem do kit

def construir_kit(cfg, col, M):
    """Cria todas as peças do R00 e devolve a lista na ordem do plano. Todas nascem na origem; o chamador espalha."""
    k = cfg['kit']
    pecas = [
        escotilha(col, M, k['escotilha']),
        escotilha_porta(col, M, k['porta'], k['escotilha']),
        corrimao(col, M, k['corrimao']),
        escada_vertical(col, M, k['escada_vertical']),
        escada_degrau(col, M, k['escada_degrau'], k['corrimao']),
        grade_piso(col, M, k['grade']),
        bandeja_cabos(col, M, k['bandeja']),
        bandeja_cabos(col, M, k['bandeja'], curva=True),
        valvula(col, M, k['valvula']),
        poste_luz(col, M, k['poste']),
        respiro(col, M, k['respiro']),
        janela_redonda(col, M, k['janela_redonda']),
        janela_retangular(col, M, k['janela_retangular']),
        armario(col, M, k['armario']),
        bancada(col, M, k['bancada']),
        caixa(col, M, k['caixa'], 'P', k['caixa']['P']),
        caixa(col, M, k['caixa'], 'M', k['caixa']['M']),
        caixa(col, M, k['caixa'], 'G', k['caixa']['G']),
    ]
    return pecas
