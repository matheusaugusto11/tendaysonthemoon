# Motor de modelagem da produção (Etapa 5): bmesh puro, quads garantidos, material por face.
# Nenhuma medida mora aqui; as peças (pecas.py, estrutura.py) passam os números vindos de config.json.
#
# Convenções:
#  - revolucao(): o perfil é uma lista de pontos (r, h) percorrida em sentido ANTI-HORÁRIO no plano (r, h)
#    (o sólido fica à esquerda de quem anda); assim as normais saem para fora. r = 0 vira um vértice só (leque).
#  - varredura(): perfil (u, v) percorrido em sentido HORÁRIO, quadro (u, v, T) destro; normais para fora.
#  - Tudo é construído em faces; sem N-gon. Aresta com ângulo > 30° fica "dura" e as outras suaves
#    (o Blender 4.1+ respeita aresta dura sem modificador).
import math
import bpy
import bmesh
from mathutils import Vector, Matrix, Euler

EPS = 1e-6


def _rot(rot_graus):
    return Euler([math.radians(a) for a in rot_graus]).to_matrix().to_4x4()


class Peca:
    """Acumula geometria num bmesh. mats = lista de materiais; cada face guarda o índice do seu material."""

    def __init__(self, nome, col, mats=()):
        self.nome = nome
        self.col = col
        self.mats = list(mats)
        self.bm = bmesh.new()
        self.bm.loops.layers.uv.new('UVMap')

    # -------------------------------------------------------------- utilidades
    def slot(self, m):
        if m is None:
            return 0
        if m not in self.mats:
            self.mats.append(m)
        return self.mats.index(m)

    def vert(self, p):
        return self.bm.verts.new(Vector(p))

    def face(self, vs, mat=None, ref=None):
        """Cria a face; se ref (Vector) for dado, vira a face para que a normal fique do lado de ref."""
        try:
            f = self.bm.faces.new(vs)
        except ValueError:
            return None
        f.material_index = self.slot(mat)
        if ref is not None:
            f.normal_update()
            if f.normal.dot(Vector(ref)) < 0:
                f.normal_flip()
        return f

    def _aplicar(self, verts_novos, matriz):
        bmesh.ops.transform(self.bm, matrix=matriz, verts=verts_novos)

    # -------------------------------------------------------------- caixa
    def caixa(self, tam, loc=(0, 0, 0), mat=None, rot=(0, 0, 0), bisel=0.0, sem=(), mats_face=None):
        """Caixa tam=(x,y,z) com o centro em loc. bisel: chanfro de 1 segmento em todas as arestas (gera 8 triângulos
        nos cantos). sem: faces a omitir ('-z','+z','-y','+y','-x','+x'), para o que fica escondido/encostado."""
        n0 = set(self.bm.verts)
        r = bmesh.ops.create_cube(self.bm, size=1.0)
        vs = [v for v in self.bm.verts if v not in n0]
        bmesh.ops.scale(self.bm, vec=Vector(tam), verts=vs)
        if bisel > 0:
            es = list({e for v in vs for e in v.link_edges})
            bmesh.ops.bevel(self.bm, geom=es, offset=bisel, offset_type='OFFSET', segments=1, profile=0.5, affect='EDGES')
            vs = [v for v in self.bm.verts if v not in n0]
        self.bm.normal_update()                       # normais no quadro local da caixa, antes de girar
        faces = {f for v in vs for f in v.link_faces}
        for f in faces:
            f.material_index = self.slot(mat)
        if mats_face:
            for chave, m in mats_face.items():
                for f in faces:
                    if f.normal.dot(_dir(chave)) > 0.99:
                        f.material_index = self.slot(m)
        if sem:
            apagar = {f for f in faces if any(f.normal.dot(_dir(ch)) > 0.99 for ch in sem)}
            bmesh.ops.delete(self.bm, geom=list(apagar), context='FACES')           # FACES também leva as arestas que ficariam soltas
            vs = [v for v in vs if v.is_valid]
        m4 = Matrix.Translation(Vector(loc)) @ _rot(rot)
        bmesh.ops.transform(self.bm, matrix=m4, verts=vs)
        return vs

    # -------------------------------------------------------------- revolução
    def revolucao(self, perfil, lados, mats, eixo='Z', loc=(0, 0, 0), rot=(0, 0, 0), fase=0.0, escala=(1, 1, 1)):
        """Gira o perfil [(r, h), ...] em volta do eixo. mats: um material ou lista com um por segmento (None = pula)."""
        n = len(perfil)
        if not isinstance(mats, (list, tuple)):
            mats = [mats] * (n - 1)
        aneis = []
        novos = []
        for (r, h) in perfil:
            if abs(r) < EPS:
                v = self.vert((0, 0, h))
                novos.append(v)
                aneis.append([v] * lados)
            else:
                anel = []
                for i in range(lados):
                    a = fase + 2 * math.pi * i / lados
                    v = self.vert((r * math.cos(a), r * math.sin(a), h))
                    anel.append(v)
                    novos.append(v)
                aneis.append(anel)
        for s in range(n - 1):
            if mats[s] is False:
                continue
            for i in range(lados):
                j = (i + 1) % lados
                a0, a1, b1, b0 = aneis[s][i], aneis[s][j], aneis[s + 1][j], aneis[s + 1][i]
                vs = [a0, a1, b1, b0]
                uniq = []
                for v in vs:
                    if v not in uniq:
                        uniq.append(v)
                if len(uniq) < 3:
                    continue
                self.face(uniq, mats[s])
        m4 = Matrix.Translation(Vector(loc)) @ _rot(rot) @ _eixo(eixo) @ Matrix.Diagonal((*escala, 1))
        bmesh.ops.transform(self.bm, matrix=m4, verts=list(set(novos)))
        return list(set(novos))

    def cilindro(self, raio, alt, loc=(0, 0, 0), lados=8, mat=None, eixo='Z', rot=(0, 0, 0), tampas=(True, True), fase=0.0):
        """Cilindro com o centro em loc; tampas em leque de triângulos (só em peça pequena)."""
        h = alt / 2
        perfil = ([(0, -h)] if tampas[0] else []) + [(raio, -h), (raio, h)] + ([(0, h)] if tampas[1] else [])
        mats = [mat] * (len(perfil) - 1)
        return self.revolucao(perfil, lados, mats, eixo, loc, rot, fase)

    # -------------------------------------------------------------- varredura
    def varredura(self, perfil, estacoes, mats, fechar=False, tampas=False):
        """Estação = (posição, u, v) com u × v = T (sentido do caminho). Perfil (u, v) HORÁRIO e FECHADO se fechar=True."""
        if not isinstance(mats, (list, tuple)):
            mats = [mats] * (len(perfil))
        aneis = []
        for (pos, u, v) in estacoes:
            pos, u, v = Vector(pos), Vector(u), Vector(v)
            aneis.append([self.vert(pos + p[0] * u + p[1] * v) for p in perfil])
        n = len(perfil)
        for s in range(len(estacoes) - 1):
            for j in range(n if fechar else n - 1):
                k = (j + 1) % n
                self.face([aneis[s][j], aneis[s + 1][j], aneis[s + 1][k], aneis[s][k]], mats[j])
        return aneis

    # -------------------------------------------------------------- acabamento
    def suavizar(self, angulo=30):
        """Faces suaves; aresta com ângulo maior que `angulo` fica dura (hard surface sem N-gon e sem loop de apoio)."""
        lim = math.radians(angulo)
        self.bm.normal_update()
        for f in self.bm.faces:
            f.smooth = True
        for e in self.bm.edges:
            if len(e.link_faces) == 2:
                if e.calc_face_angle(0) > lim:
                    e.smooth = False
            else:
                e.smooth = False

    def limpar(self, dist=1e-4):
        bmesh.ops.remove_doubles(self.bm, verts=self.bm.verts, dist=dist)
        # quads degenerados que viraram 2 vértices viram nada
        mortas = [f for f in self.bm.faces if len(f.verts) < 3 or f.calc_area() < 1e-9]
        if mortas:
            bmesh.ops.delete(self.bm, geom=mortas, context='FACES')
        soltos = [v for v in self.bm.verts if not v.link_edges]
        if soltos:
            bmesh.ops.delete(self.bm, geom=soltos, context='VERTS')

    def objeto(self, loc=(0, 0, 0), rot=(0, 0, 0), pai=None, suave=True, angulo=30):
        self.limpar()
        if suave:
            self.suavizar(angulo)
        self.bm.normal_update()
        me = bpy.data.meshes.new(self.nome)
        self.bm.to_mesh(me)
        self.bm.free()
        for m in self.mats:
            me.materials.append(m)
        o = bpy.data.objects.new(self.nome, me)
        self.col.objects.link(o)
        o.location = Vector(loc)
        o.rotation_euler = Euler([math.radians(a) for a in rot])
        if pai is not None:
            o.parent = pai
        return o


def _dir(chave):
    s = 1 if chave[0] == '+' else -1
    return Vector({'x': (s, 0, 0), 'y': (0, s, 0), 'z': (0, 0, s)}[chave[1]])


def _eixo(eixo):
    """Perfil construído em torno de Z; leva o eixo Z para o eixo pedido."""
    if eixo == 'Z':
        return Matrix.Identity(4)
    if eixo == 'Y':
        return Matrix.Rotation(math.radians(-90), 4, 'X')     # z -> +y
    if eixo == 'X':
        return Matrix.Rotation(math.radians(90), 4, 'Y')      # z -> +x
    raise ValueError(eixo)


def poligono_regular(lados, raio, fase=0.0):
    return [(raio * math.cos(fase + 2 * math.pi * i / lados), raio * math.sin(fase + 2 * math.pi * i / lados)) for i in range(lados)]
