# Blockout do cenário da Estação-Roda: primitivas em escala real, na posição final (lei arquitetônica da seção 4.6
# de Docs/02-Arte/MODELAGEM-BLENDER.md). Cada asset é um grupo de blocos filhos de uma raiz ROOT_*.
# Convenções: z para cima; frente do asset para -Y; +Y é o fundo da cena; ângulo θ no sentido horário a partir de +Y;
# posição = (R sen θ, R cos θ); rotação Z = -θ. Pivô do módulo = centro da escotilha de ligação, no chão.
import math
import bpy
from mathutils import Vector
import kit

NOMES = {'air': 'Ar_e_Agua', 'green': 'Estufa', 'shelter': 'Abrigo', 'lab': 'Laboratorio', 'battery': 'Bateria'}
ACENTO = {'air': '#4DD0E1', 'green': '#6BD425', 'shelter': '#C77DFF', 'lab': '#FF9F43', 'battery': '#FFD23F'}


class Cena:
    """Materiais, coleção e configuração compartilhados pelos blocos."""

    def __init__(self, cfg, raiz_col):
        self.cfg = cfg
        self.A = cfg['arquitetura']
        self.K = cfg['cenario']
        self.col = raiz_col
        self.M = {n: kit.material(f'MAT_Blk_{n}', h, rugosidade=0.7) for n, h in {
            'branco': '#E6E4DF', 'cinza': '#BFC0C4', 'escuro': '#2E323B', 'concreto': '#8E8C95', 'chao': '#6E6B79',
            'vidro': '#5FB3C4', 'solar': '#2B5C9E', 'rocha': '#85828F', 'boneco': '#E8B923', 'marcador': '#FF3B6B'}.items()}
        for k, h in ACENTO.items():
            self.M[k] = kit.material(f'MAT_Blk_{k}', h, rugosidade=0.55)
        self.piso = self.A['piso_z']
        self.eixo = self.A['eixo_escotilha_z']
        self.t = self.K['parede']

    # --------------------------------------------------------------- utilidades
    def sub(self, nome):
        return kit.colecao(nome, self.col)

    def raiz(self, nome, col, theta, r):
        """Empty na posição polar (R sen θ, R cos θ) com rotação Z = -θ."""
        a = math.radians(theta)
        return kit.vazio(f'ROOT_{nome}', col, (r * math.sin(a), r * math.cos(a), 0), (0, 0, -theta), tamanho=1.5)

    def boneco(self, nome, col, pai, loc):
        return kit.caixa(f'SM_Blk_Escala_Boneco_{nome}', col, self.M['boneco'], (0.50, 0.30, 1.80), (loc[0], loc[1], loc[2] + 0.9), pai=pai)

    def marcador(self, nome, col, pai, loc, direcao_graus_local):
        """Disco fino de Ø 2,2 (o vão da escotilha) colado na parede, com o eixo na direção pedida (graus a partir de +Y, horário)."""
        return kit.cilindro(f'SM_Blk_Marcador_{nome}', col, self.M['marcador'], self.A['escotilha_vao'] / 2, 0.06, loc,
                            lados=16, rot=(90, 0, 180 - direcao_graus_local), pai=pai)

    def sala(self, nome, col, pai, ext, topo_z, frente=True, piso=True, mat=None, teto=True):
        """Sala retangular oca: piso (z 0..piso), 4 paredes, teto. Origem na face da frente (y=0), centro em x. Corpo para +Y.
        A parede da frente tem uma abertura de 2,2 de largura até z = eixo + 1,1 (a escotilha)."""
        X, Y = ext
        t = self.t
        m = mat or self.M['branco']
        z0 = self.piso
        if piso:
            kit.caixa(f'SM_Blk_{nome}_Piso', col, self.M['concreto'], (X, Y, z0), (0, Y / 2, z0 / 2), pai=pai)
        h = topo_z - z0 - 0.30
        zc = z0 + h / 2
        vao = self.A['escotilha_vao']
        lintel_z = self.eixo + vao / 2
        if frente:
            larg = (X - vao) / 2
            kit.caixa(f'SM_Blk_{nome}_Frente_L', col, m, (larg, t, h), (-(vao / 2 + larg / 2), t / 2, zc), pai=pai)
            kit.caixa(f'SM_Blk_{nome}_Frente_R', col, m, (larg, t, h), ((vao / 2 + larg / 2), t / 2, zc), pai=pai)
            kit.caixa(f'SM_Blk_{nome}_Frente_Verga', col, m, (vao, t, topo_z - 0.30 - lintel_z), (0, t / 2, (topo_z - 0.30 + lintel_z) / 2), pai=pai)
        kit.caixa(f'SM_Blk_{nome}_Fundo', col, m, (X, t, h), (0, Y - t / 2, zc), pai=pai)
        kit.caixa(f'SM_Blk_{nome}_Lado_L', col, m, (t, Y - 2 * t, h), (-(X / 2 - t / 2), Y / 2, zc), pai=pai)
        kit.caixa(f'SM_Blk_{nome}_Lado_R', col, m, (t, Y - 2 * t, h), ((X / 2 - t / 2), Y / 2, zc), pai=pai)
        if teto:
            kit.caixa(f'SM_Blk_{nome}_Teto', col, m, (X, Y, 0.30), (0, Y / 2, topo_z - 0.15), pai=pai)

    # --------------------------------------------------------------- estrutura
    def corredor(self, k):
        c = self.A['corredor']
        col = self.sub('Corredores')
        r = self.raiz(f'Corredor_{NOMES[k]}', col, self.A['angulos'][k], self.A['r_modulo'])
        L = c['comprimento']
        kit.casca_aberta(f'SM_Blk_Corredor_{NOMES[k]}_Tubo', col, self.M['branco'], (c['interno'] + 2 * c['parede']) / 2, L, 12,
                         (0, -L / 2, self.eixo), rot=(90, 0, 0), espessura=c['parede'], pai=r)
        kit.caixa(f'SM_Blk_Corredor_{NOMES[k]}_Piso', col, self.M['escuro'], (c['piso_largura'], L, 0.05), (0, -L / 2, self.piso - 0.025), pai=r)
        kit.caixa(f'SM_Blk_Corredor_{NOMES[k]}_Faixa', col, self.M[k], (0.30, L - 0.4, 0.06), (0, -L / 2, self.eixo + c['interno'] / 2 + 0.02), pai=r)
        for i in range(3):
            for s, lado in ((1, 'L'), (-1, 'R')):      # apoios nas laterais, por fora do tubo (o interior fica livre)
                kit.caixa(f'SM_Blk_Corredor_{NOMES[k]}_Apoio_{lado}{i + 1}', col, self.M['escuro'], (0.35, 0.40, 0.60),
                          (s * 1.25, -(1.5 + i * 3.5), 0.30), pai=r)
        self.marcador(f'Corredor_{NOMES[k]}_Modulo', col, r, (0, 0.03, self.eixo), 180)
        self.marcador(f'Corredor_{NOMES[k]}_Hub', col, r, (0, -L - 0.03, self.eixo), 0)
        return r

    def hub(self):
        h, A = self.K['hub'], self.A['hub']
        col = self.sub('Hub')
        r = kit.vazio('ROOT_Hub', col, (0, 0, 0), (0, 0, 0), tamanho=2.0)
        ap_ext, ap_int = A['apotema_externo'], A['apotema_interno']
        circ_ext = ap_ext / math.cos(math.radians(18))
        kit.cilindro('SM_Blk_Hub_Plaza', col, self.M['concreto'], A['plaza_raio'], self.piso, (0, 0, self.piso / 2), lados=24, pai=r)
        esp = h['parede_hub']
        kit.casca_aberta('SM_Blk_Hub_Casco', col, self.M['branco'], circ_ext, h['teto_z_base'] - self.piso, 10,
                         (0, 0, (h['teto_z_base'] + self.piso) / 2), espessura=esp, pai=r)
        zc = (h['teto_z_base'] + h['teto_z_topo']) / 2
        kit.casca_aberta('SM_Blk_Hub_Teto', col, self.M['cinza'], circ_ext, h['teto_z_topo'] - h['teto_z_base'], 10, (0, 0, zc),
                         espessura=h['teto_espessura'], raio2=h['teto_topo_circunraio'], pai=r)
        kit.cilindro('SM_Blk_Hub_Claraboia', col, self.M['vidro'], h['claraboia_raio'], 0.2, (0, 0, h['teto_z_topo'] + 0.1), lados=5, pai=r)
        kit.cilindro('SM_Blk_Hub_Console', col, self.M['escuro'], h['console_raio'], h['console_altura'], (0, 0, self.piso + h['console_altura'] / 2), lados=20, pai=r)
        self.boneco('Hub', col, r, (1.3, -3.3, self.piso))
        for nome, ang in self.A['angulos'].items():
            a = math.radians(ang)
            cor = nome if nome in ACENTO else 'cinza'
            self.marcador_hub(nome, col, r, ang, ap_ext)
        return r

    def marcador_hub(self, nome, col, r, ang, ap):
        a = math.radians(ang)
        loc = ((ap + 0.03) * math.sin(a), (ap + 0.03) * math.cos(a), self.eixo)
        self.marcador(f'Hub_{nome}', col, r, loc, ang)
        ai = self.A['hub']['apotema_interno'] - 0.03
        self.marcador(f'Hub_{nome}_Interno', col, r, (ai * math.sin(a), ai * math.cos(a), self.eixo), ang)

    def eclusa(self):
        e = self.K['eclusa']['ext']
        X, Y, Z = e
        col = self.sub('Eclusa')
        r = self.raiz('Eclusa', col, self.A['angulos']['eclusa'], self.A['r_eclusa'])
        self.sala('Eclusa', col, r, (X, Y), self.piso + 2.6 + 0.30, piso=False)
        kit.caixa('SM_Blk_Eclusa_Escotilha_Externa', col, self.M['marcador'], (self.A['escotilha_vao'], 0.06, self.A['escotilha_vao']), (0, Y + 0.03, self.eixo), pai=r)
        self.boneco('Eclusa', col, r, (0, Y / 2, self.piso))
        return r

    # --------------------------------------------------------------- módulos
    def ar(self):
        k, cfg = 'air', self.K['ar']
        col = self.sub('Mod_Ar_e_Agua')
        r = self.raiz('Mod_Ar_e_Agua', col, self.A['angulos'][k], self.A['r_modulo'])
        X, Y = cfg['ext']
        self.sala('Ar', col, r, (X, Y), cfg['topo_z'])
        d = cfg['tanque_diam']
        for i, x in enumerate((-2.6, 0.0, 2.6)):
            kit.cilindro(f'SM_Blk_Ar_Tanque_{i + 1}', col, self.M[k], d / 2, cfg['tanque_topo_z'] - self.piso, (x, Y / 2, (cfg['tanque_topo_z'] + self.piso) / 2), lados=16, pai=r)
        kit.caixa('SM_Blk_Ar_Mezanino', col, self.M['escuro'], (X - 2 * self.t, 1.0, 0.15), (0, Y - self.t - 0.5, cfg['mezanino_z']), pai=r)
        kit.cilindro('SM_Blk_Ar_Tanque_Horizontal', col, self.M['cinza'], 1.05, 4.6, (-(X / 2 + 1.7), Y / 2, self.piso + 1.05), lados=16, rot=(90, 0, 0), pai=r)
        kit.caixa('SM_Blk_Ar_Ponte_Tubos', col, self.M[k], (6.0, 0.3, 0.3), (0, Y / 2, 6.4), pai=r)
        kit.cilindro('SM_Blk_Ar_Chamine', col, self.M['cinza'], 0.25, 1.7, (-3.0, Y - 1.0, cfg['topo_z'] + 0.85), lados=8, pai=r)
        self.boneco('Ar', col, r, (0, 1.6, self.piso))
        return r

    def estufa(self):
        k, cfg = 'green', self.K['estufa']
        col = self.sub('Mod_Estufa')
        r = self.raiz('Mod_Estufa', col, self.A['angulos'][k], self.A['r_modulo'])
        X, Y = cfg['ext']
        zt = cfg['parede_z_topo']
        # paredes e piso (sem o teto plano): a cobertura é de duas águas
        self.sala('Estufa', col, r, (X, Y), zt + 0.30, mat=self.M['branco'], teto=False)
        alfa = math.degrees(math.atan2(cfg['cumeeira_z'] - zt, Y / 2))
        meio = Y / 2
        comp = math.hypot(Y / 2, cfg['cumeeira_z'] - zt)
        for s, lado in ((1, 'A'), (-1, 'B')):
            kit.caixa(f'SM_Blk_Estufa_Agua_{lado}', col, self.M['vidro'], (X, comp, 0.08),
                      (0, meio + s * (Y / 4), (zt + cfg['cumeeira_z']) / 2), rot=(-s * alfa, 0, 0), pai=r)
        kit.caixa('SM_Blk_Estufa_Cumeeira', col, self.M['escuro'], (X, 0.3, 0.25), (0, meio, cfg['cumeeira_z']), pai=r)
        cb = cfg['canteiro']
        yb = Y / 2 + cfg['corredor_central'] / 2 + cb[1] / 2
        for i, x in enumerate((-3.6, 0.0, 3.6)):
            kit.caixa(f'SM_Blk_Estufa_Canteiro_A{i + 1}', col, self.M[k], cb, (x, Y / 2 - (yb - Y / 2), self.piso + cb[2] / 2), pai=r)
            kit.caixa(f'SM_Blk_Estufa_Canteiro_B{i + 1}', col, self.M[k], cb, (x, yb, self.piso + cb[2] / 2), pai=r)
        self.boneco('Estufa', col, r, (0, 1.6, self.piso))
        return r

    def abrigo(self):
        k, cfg = 'shelter', self.K['abrigo']
        col = self.sub('Mod_Abrigo')
        r = self.raiz('Mod_Abrigo', col, self.A['angulos'][k], self.A['r_modulo'])
        X, Y = cfg['ext']
        crown = cfg['coroa_z'] - self.piso
        kit.caixa('SM_Blk_Abrigo_Piso', col, self.M['concreto'], (X, Y, self.piso), (0, Y / 2, self.piso / 2), pai=r)
        # abóbada: tubo de 12 lados com eixo em Y, achatado em Z, só acima do piso (o resto fica sob o piso)
        kit.casca_aberta('SM_Blk_Abrigo_Abobada', col, self.M['branco'], X / 2, Y, 12, (0, Y / 2, self.piso), rot=(90, 0, 0),
                         espessura=self.t, escala=(1, crown / (X / 2), 1), pai=r)
        kit.caixa('SM_Blk_Abrigo_Parede_Fundo', col, self.M['branco'], (X, self.t, crown), (0, Y - self.t / 2, self.piso + crown / 2), pai=r)
        kit.elipsoide('SM_Blk_Abrigo_Berma', col, self.M['rocha'], (X / 2 + 1.0, Y / 2 + 1.2, cfg['berma_z'] - self.piso), (0, Y / 2 + 0.6, self.piso), seg=(12, 6), pai=r)
        for s, lado in ((1, 'L'), (-1, 'R')):
            kit.caixa(f'SM_Blk_Abrigo_Gabiao_{lado}', col, self.M['concreto'], (1.0, 2.6, 1.8), (s * (X / 2 + 0.2), 0.8, 1.3), rot=(0, 0, -s * 20), pai=r)
        self.boneco('Abrigo', col, r, (0, 2.5, self.piso))
        return r

    def lab(self):
        k, cfg = 'lab', self.K['lab']
        col = self.sub('Mod_Laboratorio')
        r = self.raiz('Mod_Laboratorio', col, self.A['angulos'][k], self.A['r_modulo'])
        X, Y = cfg['ext']
        self.sala('Lab', col, r, (X, Y), cfg['topo_z'])
        kit.caixa('SM_Blk_Lab_Faixa', col, self.M[k], (X + 0.04, Y + 0.04, 0.55), (0, Y / 2, 1.0), pai=r)
        kit.caixa('SM_Blk_Lab_HVAC', col, self.M['cinza'], (1.8, 0.9, 1.5), (-2.2, Y / 2, cfg['topo_z'] + 0.75), pai=r)
        kit.cilindro('SM_Blk_Lab_Mastro', col, self.M['escuro'], 0.08, cfg['equip_z'] - cfg['topo_z'], (2.4, Y - 1.2, (cfg['equip_z'] + cfg['topo_z']) / 2), lados=8, pai=r)
        kit.elipsoide('SM_Blk_Lab_Prato', col, self.M['cinza'], (1.3, 1.3, 0.45), (1.2, Y / 2, cfg['topo_z'] + 1.0), seg=(12, 6), pai=r)
        kit.cilindro('SM_Blk_Lab_Tanque', col, self.M[k], 0.6, 2.4, (-(X / 2 + 1.0), Y - 1.5, self.piso + 1.2), lados=12, pai=r)
        kit.caixa('SM_Blk_Lab_Rampa', col, self.M['escuro'], (2.6, 1.9, 0.15), ((X / 2 + 1.4), 1.8, 0.45), rot=(0, 0, 0), pai=r)
        self.boneco('Lab', col, r, (0, 1.6, self.piso))
        return r

    def bateria(self):
        k, cfg = 'battery', self.K['bateria']
        col = self.sub('Mod_Bateria')
        r = self.raiz('Mod_Bateria', col, self.A['angulos'][k], self.A['r_modulo'])
        cx, cy, cz = cfg['controle_ext']
        kit.caixa('SM_Blk_Bateria_Laje', col, self.M['concreto'], (15.4, 13.4, self.piso), (0, 6.7, self.piso / 2), pai=r)
        self.sala('Bateria_Controle', col, r, (cx, cy), self.piso + 2.6 + 0.30, piso=False)
        w, l, h = cfg['conteiner']
        for i, (x, y) in enumerate(((-cfg['lado_x'], l / 2), (cfg['lado_x'], l / 2), (0.0, cfg['traseiro_y'] + l / 2))):
            kit.caixa(f'SM_Blk_Bateria_Conteiner_C{i + 1}', col, self.M['branco'], (w, l, h), (x, y, self.piso + h / 2), pai=r)
            kit.caixa(f'SM_Blk_Bateria_Faixa_C{i + 1}', col, self.M[k], (w + 0.04, l + 0.04, 0.35), (x, y, self.piso + 1.0), pai=r)
        tr = cfg['transformador']
        kit.caixa('SM_Blk_Bateria_Transformador', col, self.M['cinza'], tr, (cfg['transformador_x'], 2.0, self.piso + tr[2] / 2), pai=r)
        kit.cilindro('SM_Blk_Bateria_Poste', col, self.M['escuro'], 0.08, 3.2, (-4.9, -0.8, self.piso + 1.6), lados=8, pai=r)
        self.boneco('Bateria', col, r, (0, 2.0, self.piso))
        return r

    # --------------------------------------------------------------- apoio e terreno
    def apoio(self):
        a = self.K['apoio']
        col = self.sub('Apoio')
        r = kit.vazio('ROOT_Apoio', col, (0, 0, 0), (0, 0, 0), tamanho=2.0)
        pn = a['solar_painel']
        for g, (gx, gy) in (('A', a['solar_a']), ('B', a['solar_b'])):
            for i in range(2):
                for j in range(5):
                    kit.caixa(f'SM_Blk_Solar_{g}_{i}{j}', col, self.M['solar'], pn, (gx + j * a['solar_passo'], gy + i * 5.0, 1.55), rot=(-28, 0, 0), pai=r)
                    kit.cilindro(f'SM_Blk_Solar_Poste_{g}_{i}{j}', col, self.M['cinza'], 0.12, 1.3, (gx + j * a['solar_passo'], gy + i * 5.0, 0.65), lados=8, pai=r)
        px, py = a['pouso']
        kit.cilindro('SM_Blk_Pouso_Plataforma', col, self.M['concreto'], a['pouso_raio'], 0.35, (px, py, 0.175), lados=8, pai=r)
        kit.caixa('SM_Blk_Pouso_Modulo_Carga', col, self.M['cinza'], (3.2, 3.2, 2.2), (px, py, 0.35 + 1.3 + 1.1), pai=r)
        for i, (sx, sy) in enumerate(((1.7, 1.7), (-1.7, 1.7), (1.7, -1.7), (-1.7, -1.7))):
            kit.caixa(f'SM_Blk_Pouso_Perna_{i + 1}', col, self.M['escuro'], (0.18, 0.18, 1.6), (px + sx, py + sy, 0.35 + 0.8), pai=r)
        dx, dy = a['prato']
        kit.caixa('SM_Blk_Prato_Base', col, self.M['concreto'], (2.4, 2.4, 1.4), (dx, dy, 0.7), pai=r)
        kit.elipsoide('SM_Blk_Prato_Disco', col, self.M['cinza'], (3.4, 3.4, 1.0), (dx, dy, 3.6), seg=(14, 6), rot=(-30, 0, 0), pai=r)
        rx, ry = a['rover']
        kit.caixa('SM_Blk_Rover_Chassi', col, self.M['branco'], (3.6, 2.2, 1.3), (rx, ry, 0.55 + 0.65), pai=r)
        for i, x in enumerate((-1.3, 0, 1.3)):
            for s, lado in ((1, 'L'), (-1, 'R')):
                kit.cilindro(f'SM_Blk_Rover_Roda_{lado}{i + 1}', col, self.M['escuro'], 0.55, 0.5, (rx + x, ry + s * 1.25, 0.55), lados=10, rot=(90, 0, 0), pai=r)
        for i, (cx, cy) in enumerate(a['conteineres']):
            kit.caixa(f'SM_Blk_Conteiner_Suprimento_{i + 1}', col, self.M['cinza'], (2.44, 6.1, 2.6), (cx, cy, 1.3), pai=r)
        return r

    def terreno(self):
        t = self.K['terreno']
        col = self.sub('Terreno')
        r = kit.vazio('ROOT_Terreno', col, (0, 0, 0), (0, 0, 0), tamanho=3.0)
        kit.cilindro('SM_Blk_Terreno_Nucleo', col, self.M['chao'], t['raio'], 0.3, (0, 0, -0.15), lados=48, pai=r)
        kit.casca_aberta('SM_Blk_Terreno_Muralha', col, self.M['rocha'], t['muralha_raio'], t['muralha_altura'], 24, (0, 0, t['muralha_altura'] / 2 - 2.0),
                         espessura=6.0, pai=r)
        return r
