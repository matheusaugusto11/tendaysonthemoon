# Formas e utilidades do gerador da Estação-Roda. Só bmesh e organização; nenhuma decisão de medida aqui
# (as medidas moram em config.json). Precisa do _comum.py ao lado.
import math
import bpy
import bmesh
from mathutils import Vector, Matrix, Euler
from _comum import triangulos


def lin(hex_srgb):
    """'#RRGGBB' (sRGB) -> tupla linear (o Principled e o Workbench esperam linear)."""
    h = hex_srgb.lstrip('#')
    c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    return tuple(((v + 0.055) / 1.055) ** 2.4 if v > 0.04045 else v / 12.92 for v in c)


# ------------------------------------------------------------------ organização

def colecao(nome, pai=None):
    c = bpy.data.collections.get(nome) or bpy.data.collections.new(nome)
    destino = pai or bpy.context.scene.collection
    if c.name not in destino.children:
        destino.children.link(c)
    return c


def material(nome, hex_srgb, rugosidade=0.6, metal=0.0):
    """Principled simples; diffuse_color igual à cor base (é o que o Workbench das prévias mostra)."""
    from _comum import material_novo
    m = bpy.data.materials.get(nome) or material_novo(nome)
    cor = lin(hex_srgb)
    p = m.node_tree.nodes['Principled BSDF']
    p.inputs['Base Color'].default_value = (*cor, 1)
    p.inputs['Roughness'].default_value = rugosidade
    p.inputs['Metallic'].default_value = metal
    m.diffuse_color = (*cor, 1)
    return m


# ------------------------------------------------------------------ primitivas (bmesh, sem N-gon)

def _objeto(nome, bm, col, mat, loc, rot, pai=None):
    bm.normal_update()
    me = bpy.data.meshes.new(nome)
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new(nome, me)
    col.objects.link(o)
    o.location = Vector(loc)
    o.rotation_euler = Euler([math.radians(a) for a in rot])
    if mat:
        o.data.materials.append(mat)
    if pai is not None:
        o.parent = pai
    return o


def caixa(nome, col, mat, tam, loc, rot=(0, 0, 0), pai=None):
    """Caixa de tam=(x,y,z) metros com o CENTRO em loc."""
    bm = bmesh.new()
    bm.loops.layers.uv.new('UVMap')
    bmesh.ops.create_cube(bm, size=1.0, calc_uvs=True)
    bmesh.ops.scale(bm, vec=Vector(tam), verts=bm.verts)
    return _objeto(nome, bm, col, mat, loc, rot, pai)


def cilindro(nome, col, mat, raio, altura, loc, lados=12, rot=(0, 0, 0), pai=None, escala=(1, 1, 1)):
    """Cilindro vertical com o CENTRO em loc; tampas em leque de triângulos (sem N-gon)."""
    bm = bmesh.new()
    bm.loops.layers.uv.new('UVMap')
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True, segments=lados, radius1=raio, radius2=raio, depth=altura, calc_uvs=True)
    if tuple(escala) != (1, 1, 1):
        bmesh.ops.scale(bm, vec=Vector(escala), verts=bm.verts)
    return _objeto(nome, bm, col, mat, loc, rot, pai)


def elipsoide(nome, col, mat, raios, loc, seg=(14, 10), rot=(0, 0, 0), pai=None):
    """Esfera UV escalada por raios=(rx,ry,rz). Os polos viram leque de triângulos."""
    bm = bmesh.new()
    bm.loops.layers.uv.new('UVMap')
    bmesh.ops.create_uvsphere(bm, u_segments=seg[0], v_segments=seg[1], radius=1.0, calc_uvs=True)
    bmesh.ops.scale(bm, vec=Vector(raios), verts=bm.verts)
    return _objeto(nome, bm, col, mat, loc, rot, pai)


# ------------------------------------------------------------------ câmeras

def camera(nome, col, pos, alvo, fov_vertical, ortografica=None):
    """Câmera com FOV vertical (Sensor Fit Vertical) olhando para alvo."""
    cam = bpy.data.cameras.new(nome)
    cam.sensor_fit = 'VERTICAL'
    cam.angle_y = math.radians(fov_vertical)
    if ortografica:
        cam.type = 'ORTHO'
        cam.ortho_scale = ortografica
    cam.clip_start = 0.05
    cam.clip_end = 400
    o = bpy.data.objects.new(nome, cam)
    col.objects.link(o)
    o.location = Vector(pos)
    d = Vector(alvo) - Vector(pos)
    if ortografica and abs(d.x) < 0.01 and abs(d.y) < 0.01:
        o.rotation_euler = (0, 0, 0)          # de cima para baixo, +Y para o alto da imagem
    else:
        o.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    return o


def tris_por_nome(objs, orcamento=None):
    """Lista (nome, triângulos) e o total."""
    linhas = [(o.name, triangulos(o)) for o in objs if o.type == 'MESH']
    return linhas, sum(t for _, t in linhas)


# ------------------------------------------------------------------ cascas e raízes

def vazio(nome, col, loc=(0, 0, 0), rot_graus=(0, 0, 0), tamanho=1.0):
    """Empty simples (raiz de asset ou ponto de jogo)."""
    o = bpy.data.objects.new(nome, None)
    o.empty_display_type = 'PLAIN_AXES'
    o.empty_display_size = tamanho
    col.objects.link(o)
    o.location = Vector(loc)
    o.rotation_euler = Euler([math.radians(a) for a in rot_graus])
    return o


def casca_aberta(nome, col, mat, raio_ext, comprimento, lados, loc, rot=(0, 0, 0), espessura=0.15, raio2=None, pai=None, escala=(1, 1, 1)):
    """Cilindro/tronco de cone SEM tampas, com modificador Solidify para dentro (parede de `espessura`).
    Normais para fora; Solidify com offset -1 mantém o raio externo."""
    bm = bmesh.new()
    bm.loops.layers.uv.new('UVMap')
    bmesh.ops.create_cone(bm, cap_ends=False, segments=lados, radius1=raio_ext, radius2=raio2 if raio2 else raio_ext,
                          depth=comprimento, calc_uvs=True)
    if tuple(escala) != (1, 1, 1):
        bmesh.ops.scale(bm, vec=Vector(escala), verts=bm.verts)
    o = _objeto(nome, bm, col, mat, loc, rot, pai)
    m = o.modifiers.new('Casca', 'SOLIDIFY')
    m.thickness = espessura
    m.offset = -1.0
    return o


def mundo_de(raiz, v):
    """Ponto local de uma raiz -> mundo (usa a matriz da raiz)."""
    bpy.context.view_layer.update()
    return raiz.matrix_world @ Vector(v)
