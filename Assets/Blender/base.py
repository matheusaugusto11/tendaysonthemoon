"""Base lunar completa - "Ten Days on the Moon".
Hub central + 6 tuneis + Estufa, Laboratorio, Dormitorio, Agua, Energia e Eclusa + exterior (terreno, paineis solares, antena...).
Precisa de estufa.blend (gerado por estufa.py) na mesma pasta.
Run:  blender -b -P base.py
Saidas: base.blend, base.glb, base.fbx (so o cenario) + previas base_*.png (camera/luz/Terra so existem nas previas)
Unidades em metros. Hub no (0,0). Portas: Estufa +Y(90 graus), Lab 30, Energia -30, Eclusa -90, Dormitorio -150, Agua 150.
"""
import bpy, math, random, os
from mathutils import Vector, Matrix, noise

OUT = os.path.dirname(os.path.abspath(__file__))
random.seed(11)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
GZ = -0.2          # altura do solo
T = 0.3            # espessura das paredes dos modulos

# ================================================================ materiais
def mat(name, color, rough=0.7, metal=0.0, emit=None, emit_str=0.0, alpha=1.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1)
    b.inputs["Roughness"].default_value = rough
    b.inputs["Metallic"].default_value = metal
    if emit:
        b.inputs["Emission Color"].default_value = (*emit, 1)
        b.inputs["Emission Strength"].default_value = emit_str
    if alpha < 1:
        b.inputs["Alpha"].default_value = alpha
        m.surface_render_method = 'BLENDED'
    return m

def glow(name, c, s=6.0):
    return mat(name, c, 0.4, emit=c, emit_str=s)

# mesmos nomes/valores da estufa (para unificar os materiais ao importar)
M = dict(
    white=mat("Painel branco", (0.80, 0.79, 0.76), 0.85),
    dark=mat("Painel escuro", (0.07, 0.07, 0.08), 0.9),
    floor=mat("Piso", (0.55, 0.56, 0.58), 0.9),
    steel=mat("Estrutura", (0.62, 0.64, 0.66), 0.55, 0.4),
    darksteel=mat("Metal escuro", (0.12, 0.13, 0.15), 0.6, 0.3),
    glass=mat("Vidro", (0.04, 0.42, 0.38), 0.1, 0.0, emit=(0.0, 0.55, 0.45), emit_str=0.55, alpha=0.45),
    green_trim=mat("Borda verde", (0.25, 0.75, 0.12), 0.5, emit=(0.3, 1, 0.1), emit_str=0.6),
    door=mat("Porta", (0.7, 0.7, 0.7), 0.6, 0.2),
    soil=mat("Terra", (0.09, 0.055, 0.04), 1.0),
    soil2=mat("Terra clara", (0.22, 0.13, 0.09), 1.0),
    led=mat("Luz verde", (0.4, 1, 0.3), 0.3, emit=(0.4, 1, 0.25), emit_str=9),
    leaf1=mat("Folha 1", (0.30, 0.68, 0.22), 0.8),
    leaf2=mat("Folha 2", (0.45, 0.78, 0.30), 0.8),
    leaf3=mat("Folha 3", (0.20, 0.50, 0.20), 0.8),
    bush=mat("Folhagem", (0.11, 0.35, 0.12), 0.8),
    stalk=mat("Haste", (0.45, 0.6, 0.18), 0.8),
    wheat=mat("Espiga", (0.95, 0.60, 0.10), 0.7),
    moon=mat("Solo lunar", (0.35, 0.35, 0.37), 1.0),
    # novos
    grate=mat("Grade", (0.16, 0.17, 0.19), 0.7, 0.4),
    rock=mat("Rocha", (0.30, 0.30, 0.32), 1.0),
    rock2=mat("Rocha clara", (0.50, 0.50, 0.52), 1.0),
    orange=mat("Laranja", (0.95, 0.52, 0.10), 0.6),
    purple=mat("Roxo", (0.55, 0.28, 0.85), 0.6),
    cyan=mat("Ciano", (0.15, 0.75, 0.85), 0.6),
    yellow=mat("Amarelo", (0.95, 0.75, 0.10), 0.6),
    green_door=mat("Verde", (0.35, 0.78, 0.18), 0.6),
    red=mat("Vermelho", (0.85, 0.15, 0.10), 0.6),
    grey_door=mat("Cinza porta", (0.55, 0.57, 0.60), 0.5, 0.3),
    screen=glow("Tela", (0.25, 0.85, 0.8), 1.2),
    screen_o=glow("Tela amarela", (1.0, 0.8, 0.25), 3.0),
    screen_g=glow("Tela verde", (0.3, 1.0, 0.4), 3.0),
    lamp=glow("Lampada", (1.0, 0.92, 0.78), 8.0),
    lamp_o=glow("Lampada laranja", (1.0, 0.6, 0.2), 8.0),
    lamp_p=glow("Lampada roxa", (0.7, 0.4, 1.0), 8.0),
    lamp_c=glow("Lampada ciano", (0.3, 0.9, 1.0), 8.0),
    lamp_y=glow("Lampada amarela", (1.0, 0.85, 0.2), 8.0),
    tank=mat("Tanque", (0.85, 0.86, 0.88), 0.4, 0.2),
    solar=mat("Painel solar", (0.10, 0.25, 0.55), 0.25, 0.6),
    barrel=mat("Barril", (0.2, 0.7, 0.85), 0.5),
    mattress=mat("Colchao", (0.16, 0.16, 0.19), 0.9),
    visor=mat("Visor", (0.85, 0.62, 0.15), 0.1, 0.9),
    wood=mat("Madeira", (0.45, 0.3, 0.15), 0.9),
    jar1=mat("Pote 1", (0.8, 0.2, 0.55), 0.5), jar2=mat("Pote 2", (0.3, 0.7, 0.3), 0.5), jar3=mat("Pote 3", (0.9, 0.7, 0.2), 0.5),
)

# ================================================================ helpers
def assign(o, m):
    o.data.materials.clear(); o.data.materials.append(m); return o

def box(name, c, size, m):
    bpy.ops.mesh.primitive_cube_add(size=1, location=c)
    o = bpy.context.object; o.name = name; o.scale = size
    return assign(o, m)

def beam(p1, p2, w, d, m, name="Viga"):
    p1, p2 = Vector(p1), Vector(p2); v = p2 - p1
    bpy.ops.mesh.primitive_cube_add(size=1, location=(p1 + p2) / 2)
    o = bpy.context.object; o.name = name; o.scale = (w, d, v.length)
    o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = v.to_track_quat('Z', 'Y')
    return assign(o, m)

def poly(name, verts, faces, m):
    me = bpy.data.meshes.new(name); me.from_pydata(verts, [], faces); me.update()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    return assign(o, m)

def ico(loc, scale, m, rot=(0, 0, 0), name="Ico", sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, location=loc, rotation=rot)
    o = bpy.context.object; o.scale = scale; o.name = name
    return assign(o, m)

def cyl(loc, r, depth, m, axis='Z', verts=12, name="Cil", r2=None):
    rot = {'Z': (0, 0, 0), 'X': (0, math.pi / 2, 0), 'Y': (math.pi / 2, 0, 0)}[axis]
    if r2 is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc, rotation=rot)
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r, radius2=r2, depth=depth, location=loc, rotation=rot)
    o = bpy.context.object; o.name = name
    return assign(o, m)

def octagon(name, center, w, h, thick, m):
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=1, depth=1)
    o = bpy.context.object
    o.rotation_euler = (0, 0, math.pi / 8)
    bpy.ops.object.transform_apply(rotation=True)
    o.rotation_euler = (math.pi / 2, 0, 0)
    o.scale = (w, h, thick); o.location = center; o.name = name
    return assign(o, m)

def bool_cut(obj, cutter):
    mod = obj.modifiers.new("Corte", 'BOOLEAN')
    mod.operation = 'DIFFERENCE'; mod.object = cutter; mod.solver = 'EXACT'
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier="Corte")

def tube_mesh(name, ro0, ro1, ri0, ri1, t0, t1, n, m, axis='Z', cx=0, cy=0, cz=0, rot0=0.0):
    """Tubo/anel oco e fechado. Raio pode variar entre as pontas (cone oco). axis Z, Y ou X."""
    def P(r, a, t):
        c, s = math.cos(a) * r, math.sin(a) * r
        if axis == 'Z': return (cx + c, cy + s, t)
        if axis == 'Y': return (cx + c, t, cz + s)
        return (t, cy + c, cz + s)
    ang = [rot0 + 2 * math.pi * i / n for i in range(n)]
    v = ([P(ro0, a, t0) for a in ang] + [P(ro1, a, t1) for a in ang] + [P(ri0, a, t0) for a in ang] + [P(ri1, a, t1) for a in ang])
    O0, O1, I0, I1 = 0, n, 2 * n, 3 * n
    f = []
    for i in range(n):
        j = (i + 1) % n
        f += [(O0 + i, O0 + j, O1 + j, O1 + i), (I0 + i, I1 + i, I1 + j, I0 + j),
              (O1 + i, O1 + j, I1 + j, I1 + i), (O0 + i, I0 + i, I0 + j, O0 + j)]
    if axis == 'Y':
        f = [tuple(reversed(q)) for q in f]
    return poly(name, v, f, m)

def place(objs, Mx):
    bpy.context.view_layer.update()
    for o in objs:
        o.matrix_world = Mx @ o.matrix_world

def new_since(before):
    return [o for o in bpy.data.objects if o.name not in before]

def Mtr(x=0, y=0, z=0, yaw=0.0):
    return Matrix.Translation((x, y, z)) @ Matrix.Rotation(yaw, 4, 'Z')

DOORS = []   # (rotulo, folha, angulo)

def leaf_mesh(name, w, h, thick, side, m):
    c, s1 = math.cos(math.radians(22.5)), math.sin(math.radians(22.5))
    c2, s2 = math.cos(math.radians(67.5)), math.sin(math.radians(67.5))
    hx = w * c
    pts = [(0, -h * s2), (w * c2, -h * s2), (hx, -h * s1), (hx, h * s1), (w * c2, h * s2), (0, h * s2)]
    pts = [((x - hx) * (1 if side > 0 else -1), z) for x, z in pts]
    t = thick / 2; n = len(pts)
    v = [(x, -t, z) for x, z in pts] + [(x, t, z) for x, z in pts]
    f = [tuple(range(n))[::-1], tuple(range(n, 2 * n))]
    if side < 0: f = [f[1], f[0]]
    for i in range(n):
        j = (i + 1) % n
        f.append((i, j, j + n, i + n) if side > 0 else (i, i + n, j + n, j))
    cx = -side * (hx - .16); base = len(v)
    for dx in (-.025, .025):
        for dy in (-t - .05, t + .05):
            for dz in (-.2, .1):
                v.append((cx + dx, dy, dz))
    f += [tuple(base + i for i in q) for q in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]]
    return poly(name, v, f, m)

def door_unit(hosts, Mx, yc, thick, d, label, frame_m, leaf_m):
    """Porta octogonal articulada. Quadro local: parede ao longo de X, espessura em Y, centro em yc, +Y para fora.
    d = +1 se o interior fica em +Y local, -1 se em -Y. Mx leva o quadro local para o mundo."""
    cut_wall = octagon("c1", (0, yc, 1.5), 1.2, 1.5, thick + .4, M['white'])
    frame = octagon("Porta moldura", (0, yc, 1.5), 1.2, 1.5, thick + .08, frame_m)
    cut_in = octagon("c2", (0, yc, 1.5), 1.0, 1.3, thick + .5, M['white'])
    leaves = []
    w, h = .99, 1.29
    hx = w * math.cos(math.radians(22.5))
    for side, nm in ((1, "D"), (-1, "E")):
        lf = leaf_mesh(f"Porta_{label}_{nm}", w, h, .1, side, leaf_m)
        lf.location = (side * hx, yc, 1.5)
        leaves.append((lf, math.radians(105) * (-d if side > 0 else d)))
    place([cut_wall, frame, cut_in] + [l for l, _ in leaves], Mx)
    for hst in hosts: bool_cut(hst, cut_wall)
    bool_cut(frame, cut_in)
    bpy.data.objects.remove(cut_wall); bpy.data.objects.remove(cut_in)
    for lf, ang in leaves:
        DOORS.append((label, lf, ang))

# ================================================================ importa a estufa pronta
GH_Y = 15.8 + 0.3            # y da face interna da parede de entrada da estufa
with bpy.data.libraries.load(os.path.join(OUT, "estufa.blend"), link=False) as (src, dst):
    dst.objects = list(src.objects)
for o in dst.objects:
    bpy.context.collection.objects.link(o)
    if o.name.startswith("Porta_"):
        o.name = o.name.replace("Porta_", "Porta_Estufa_")
        o.location.y += GH_Y
        if o.animation_data:
            for tr in o.animation_data.nla_tracks:
                tr.name = tr.name.replace("Porta_", "Porta_Estufa_")
    else:
        o.location.y += GH_Y
    for slot in o.material_slots:
        base = slot.material.name.rsplit('.', 1)[0] if slot.material and slot.material.name[-4] == '.' else None
        if base and base in bpy.data.materials:
            slot.material = bpy.data.materials[base]
bpy.context.view_layer.update()

# ================================================================ HUB
HR_IN, HR_OUT, HH = 6.4, 6.8, 3.4
HUB_ROT = math.radians(7.5)
APO = math.cos(math.radians(7.5))           # apotema do poligono de 24 lados
PORTS = [  # angulo, nome, material porta, lampada
    (90, "Estufa", M['green_door'], M['lamp']),
    (30, "Laboratorio", M['orange'], M['lamp_o']),
    (-30, "Energia", M['yellow'], M['lamp_y']),
    (-90, "Eclusa", M['grey_door'], M['lamp']),
    (-150, "Dormitorio", M['purple'], M['lamp_p']),
    (150, "Agua", M['cyan'], M['lamp_c']),
]
LIGHTS = {}     # chave -> lista de (loc mundo, size_x, size_y, cor, energia, forma)
SHOTS = []

box_floor = cyl((0, 0, GZ / 2 - 0.0), 6.9, 0.2, M['floor'], verts=24, name="Piso hub"); box_floor.location.z = -0.1
cyl((0, 0, -0.45), 7.1, 0.5, M['darksteel'], verts=24, name="Base hub")
ring_dark = tube_mesh("Hub parede baixa", HR_OUT, HR_OUT, HR_IN, HR_IN, 0, 1.4, 24, M['dark'], rot0=HUB_ROT)
ring_white = tube_mesh("Hub parede alta", HR_OUT, HR_OUT, HR_IN, HR_IN, 1.4, HH, 24, M['white'], rot0=HUB_ROT)
tube_mesh("Hub friso", HR_OUT + .03, HR_OUT + .03, HR_OUT - .1, HR_OUT - .1, 1.35, 1.45, 24, M['darksteel'], rot0=HUB_ROT)
tube_mesh("Hub faixa", HR_OUT + .02, HR_OUT + .02, HR_OUT - .1, HR_OUT - .1, 2.9, 3.0, 24, M['steel'], rot0=HUB_ROT)
# teto: faixa branca inclinada + vidro + tampa
tube_mesh("Hub teto", HR_OUT, 5.6, HR_OUT - .3, 5.35, HH, 4.2, 24, M['white'], rot0=HUB_ROT)
g = tube_mesh("Hub vidro", 5.6, 3.4, 5.52, 3.32, 4.2, 5.6, 24, M['glass'], rot0=HUB_ROT)
cyl((0, 0, 5.72), 3.5, 0.25, M['white'], verts=24, name="Hub tampa")
for k in range(0, 24, 3):
    a = HUB_ROT + 2 * math.pi * k / 24
    beam((5.6 * math.cos(a), 5.6 * math.sin(a), 4.2), (3.45 * math.cos(a), 3.45 * math.sin(a), 5.62), .1, .12, M['steel'], "Montante")
tube_mesh("Hub aro vidro", 5.7, 5.7, 5.45, 5.45, 4.12, 4.28, 24, M['darksteel'], rot0=HUB_ROT)

# portas do hub
for ang, nm, dm, lm in PORTS:
    a = math.radians(ang)
    yc = (HR_OUT + HR_IN) / 2 * APO
    door_unit([ring_dark, ring_white], Mtr(0, 0, 0, a - math.pi / 2), yc, 0.5, -1, f"Hub_{nm}", M['darksteel'], dm)
    # faixa colorida no piso, do console ate a porta
    for r in (3.1, 4.0, 4.9):
        bx = box(f"Trilha {nm}", (math.cos(a) * r, math.sin(a) * r, 0.01), (.35, 0.8, .02), dm)
        bx.rotation_euler = (0, 0, a + math.pi / 2)
    box(f"Seta {nm}", (math.cos(a) * 5.6, math.sin(a) * 5.6, 0.01), (.6, .6, .02), dm).rotation_euler = (0, 0, a + math.pi / 4)

# console central
tube_mesh("Anel console", 2.6, 2.6, 2.35, 2.35, 0, .12, 24, M['darksteel'])
cyl((0, 0, .3), 1.9, .6, M['dark'], verts=12, name="Console base")
cyl((0, 0, .66), 2.0, .14, M['white'], verts=12, name="Console tampo")
for k in range(-3, 4):
    a = math.radians(90 + k * 22)
    bx = box("Tela console", (math.cos(a) * 1.55, math.sin(a) * 1.55 + 0.0, 1.35), (.9, .08, .55), M['screen'])
    bx.rotation_euler = (0, 0, a + math.pi / 2)
    bx2 = box("Moldura tela", (math.cos(a) * 1.6, math.sin(a) * 1.6, 1.35), (.98, .05, .63), M['darksteel'])
    bx2.rotation_euler = (0, 0, a + math.pi / 2)
box("Vitrine", (0, -.5, 1.0), (1.2, .7, .42), M['glass'])
for k in range(5):
    cyl((-.45 + k * .225, -.5, 1.0), .08, .3, M['lamp_y'], verts=8, name="Cristal")
for sx in (-1, 1):
    box("Banco console", (sx * 1.3, -1.2, .4), (.9, .6, .3), M['white'])
for k in range(12):
    a = HUB_ROT + 2 * math.pi * (k + .5) / 12
    bx = box("Luz teto", (math.cos(a) * 5.5, math.sin(a) * 5.5, 3.36), (1.6, .12, .06), M['lamp'])
    bx.rotation_euler = (0, 0, a + math.pi / 2)
for a in (110, 130, 70, 50, -10, 190, 250, 290):
    ar = math.radians(a)
    box("Cilindro de gas", (math.cos(ar) * 6.1, math.sin(ar) * 6.1, .9), (.45, .45, 1.6), M['tank']).rotation_euler = (0, 0, ar)
cyl((0, 0, 5.58), 3.0, .04, glow("Cupula luz", (1.0, 0.95, 0.85), 1.1), verts=24, name="Hub luz cupula")
LIGHTS['hub'] = [((0, 0, 5.0), 9, 9, (1, 1, 1), 420, 'RECTANGLE'), ((0, 0, .4), 9, 9, (.9, .95, 1), 180, 'RECTANGLE', True)]

# ================================================================ TUNEIS
TUN_R0, TUN_R1 = 6.6, 15.85
def tunnel(ang, nm, lm):
    a = math.radians(ang); before = {o.name for o in bpy.data.objects}
    L = TUN_R1 - TUN_R0; cz = 1.5
    sh = tube_mesh("Tunel casco", 1.9, 1.9, 1.72, 1.72, TUN_R0, TUN_R1, 12, M['white'], axis='Y', cz=cz, rot0=math.radians(15))
    for yy in [TUN_R0 + 1.0 + 1.55 * i for i in range(int((L - 1) // 1.55))]:
        tube_mesh("Costela", 2.0, 2.0, 1.86, 1.86, yy, yy + .18, 12, M['darksteel'], axis='Y', cz=cz, rot0=math.radians(15))
    box("Piso grade", (0, (TUN_R0 + TUN_R1) / 2, -0.06), (1.9, L, .12), M['grate'])
    box("Luz tunel", (0, (TUN_R0 + TUN_R1) / 2, 3.15), (.14, L - 1.6, .05), lm)
    box("Faixa tunel", (0, (TUN_R0 + TUN_R1) / 2, 0.01), (.1, L, .01), lm)
    for sx in (-1, 1):
        beam((sx * 1.5, TUN_R0 + .6, 1.05), (sx * 1.5, TUN_R1 - .6, 1.05), .05, .05, M['darksteel'], "Corrimao")
        for yy in range(int(TUN_R0 + 2), int(TUN_R1 - 1), 3):
            beam((sx * 1.5, yy, 1.05), (sx * 1.62, yy, .0), .04, .04, M['darksteel'], "Poste")
    for sx in (-1, 1):                        # janelas redondas
        for yy in (TUN_R0 + 3.2, TUN_R0 + 6.5):
            cut = cyl((sx * 1.75, yy, 1.8), .3, 1.0, M['white'], axis='X', verts=10, name="cj")
            bool_cut(sh, cut); bpy.data.objects.remove(cut)
            cyl((sx * 1.76, yy, 1.8), .3, .04, M['glass'], axis='X', verts=10, name="Janela vidro")
            tube_mesh("Janela aro", .4, .4, .27, .27, min(sx * 1.62, sx * 1.86), max(sx * 1.62, sx * 1.86), 10, M['steel'], axis='X', cy=yy, cz=1.8)
    place(new_since(before), Mtr(0, 0, 0, a - math.pi / 2))
    LIGHTS['tunel_' + nm] = [((math.cos(a) * 11, math.sin(a) * 11, 3.0), 1.0, 7.0, (.8, 1, 1), 150, 'RECTANGLE')]

for ang, nm, dm, lm in PORTS:
    tunnel(ang, nm, lm)

# ================================================================ MODULOS (salas retangulares)
def room(nm, ang, accent, lamp, door_m, content, length=12.0, width=8.0, exit_door=None):
    before = {o.name for o in bpy.data.objects}
    hw = width / 2; ih = 3.4; ys = T; ye = T + length
    lowF = box("Parede frente b", (0, T / 2, .7), (width + .6, T, 1.4), M['dark'])
    highF = box("Parede frente a", (0, T / 2, 2.4), (width + .6, T, 2.0), M['white'])
    lowB = box("Parede fundo b", (0, ye + T / 2, .7), (width + .6, T, 1.4), M['dark'])
    highB = box("Parede fundo a", (0, ye + T / 2, 2.4), (width + .6, T, 2.0), M['white'])
    for s in (-1, 1):
        box("Parede lado b", (s * (hw + T / 2), (T + ye + T) / 2, .7), (T, length + 2 * T, 1.4), M['dark'])
        box("Parede lado a", (s * (hw + T / 2), (T + ye + T) / 2, 2.4), (T, length + 2 * T, 2.0), M['white'])
        box("Faixa lado", (s * (hw - .01), (ys + ye) / 2, 1.4), (.03, length, .45), accent)
        box("Faixa ext", (s * (hw + T + .01), (T + ye + T) / 2, 1.4), (.02, length + 2 * T, .5), accent)
        box("Friso", (s * (hw - .02), (ys + ye) / 2, 3.35), (.05, length, .06), M['darksteel'])
    box("Faixa fundo", (0, ye - .01, 1.4), (width, .03, .45), accent)
    box("Piso", (0, (T + ye + T) / 2, -.1), (width + 2 * T, length + 2 * T, .2), M['floor'])
    box("Base", (0, (T + ye + T) / 2, -.35), (width + 2 * T + .4, length + 2 * T + .4, .3), M['darksteel'])
    for i in range(5):
        box("Piso junta", (0, ys + 2 + i * 2.2, .005), (width, .03, .01), M['darksteel'])
    # teto com claraboia central
    zc = ih + .1
    box("Teto frente", (0, (ys + 4.5) / 2 + 0.0, zc), (width + 2 * T, 4.5 - ys + T, .2), M['white'])
    box("Teto fundo", (0, (7.5 + ye + T) / 2, zc), (width + 2 * T, ye + T - 7.5, .2), M['white'])
    for s in (-1, 1):
        box("Teto lado", (s * (hw + T - 1.6) , 6.0, zc), (3.2 + (T if True else 0) - .0, 3.0, .2), M['white'])
    gl = box("Claraboia", (0, 6.0, zc), (2.9, 3.0, .05), M['glass']); gl.visible_shadow = False
    for yy in (4.5, 7.5):
        box("Moldura clar", (0, yy, zc + .02), (3.0, .08, .22), M['steel'])
    for xx in (-1.5, 1.5):
        box("Moldura clar", (xx, 6.0, zc + .02), (.08, 3.0, .22), M['steel'])
    for yy in (2.2, 4.4, 8.4, 10.6):
        box("Luz teto", (0, yy, ih - .03), (1.6, .14, .05), lamp)
    # porta de entrada
    door_unit([lowF, highF], Matrix.Identity(4), T / 2, T, +1, f"{nm}_Entrada", M['darksteel'], door_m)
    if exit_door:
        door_unit([lowB, highB], Matrix.Identity(4), ye + T / 2, T, -1, f"{nm}_Saida", M['darksteel'], exit_door)
    content()
    objs = new_since(before)
    Mx = Mtr(0, 0, 0, math.radians(ang) - math.pi / 2) @ Matrix.Translation((0, TUN_R1, 0))
    place(objs, Mx)
    c_ = Mx @ Vector((0, 6.3, 3.2))
    LIGHTS[nm] = [((c_.x, c_.y, 3.2), width - 1.0, length - 1.0, (1, 1, 1), 260, 'RECTANGLE'), ((c_.x, c_.y, .5), width - 1.0, length - 1.0, (.9, .95, 1), 140, 'RECTANGLE', True)]
    return Mx

# ---------- Laboratorio (laranja)
def lab_content():
    jars = [M['jar1'], M['jar2'], M['jar3']]
    for s in (-1, 1):
        box("Bancada", (s * 3.0, 6.6, .85), (1.2, 8.0, .08), M['white'])
        for yy in (2.8, 6.6, 10.4):
            box("Bancada gabinete", (s * 3.1, yy, .42), (1.0, 2.0, .82), M['steel'])
        box("Prateleira", (s * 3.75, 6.6, 2.15), (.4, 9.0, .05), M['white'])
        for k in range(14):
            cyl((s * 3.7, 2.4 + k * .6 + random.uniform(-.1, .1), 2.3), .08, .22, random.choice(jars), verts=6, name="Pote")
        for yy in (3.6, 6.6, 9.6):
            box("Monitor lab", (s * 3.9, yy, 1.55), (.06, 1.0, .6), M['screen'])
            box("Moldura mon", (s * 3.93, yy, 1.55), (.04, 1.08, .68), M['darksteel'])
    # microscopio
    box("Micro base", (-2.9, 4.2, .93), (.4, .3, .06), M['steel'])
    cyl((-2.9, 4.25, 1.15), .05, .45, M['steel'], verts=6, name="Micro braco")
    cyl((-2.9, 4.1, 1.28), .06, .3, M['darksteel'], axis='Y', verts=6, name="Micro tubo")
    box("Equip", (-2.8, 5.4, 1.0), (.5, .4, .25), M['white'])
    # glovebox
    box("Glovebox", (3.0, 9.3, 1.0), (1.4, 1.6, 1.9), M['steel'])
    box("Glovebox vidro", (2.3, 9.3, 1.45), (.04, 1.2, .8), M['glass']).visible_shadow = False
    box("Glovebox luz", (2.25, 9.3, 1.9), (.06, 1.3, .06), M['lamp'])
    # freezer
    box("Freezer", (-2.9, 9.6, .5), (1.0, 1.2, 1.0), M['white'])
    box("Freezer tampa", (-2.9, 9.6, 1.02), (1.05, 1.25, .06), M['steel'])
    # hatch decor
    octagon("Escotilha fundo", (0, 12.3 - .02, 1.5), .75, .95, .06, M['orange'])
    octagon("Escotilha fundo2", (0, 12.3 - .06, 1.5), .6, .8, .06, M['door'])

# ---------- Dormitorio (roxo)
def dorm_content():
    for k in range(3):
        for lvl in range(2):
            y = 2.5 + k * 2.4; z = .35 + lvl * 1.05
            box("Beliche base", (-3.0, y, z), (1.4, 2.2, .1), M['steel'])
            box("Beliche topo", (-3.0, y, z + .85), (1.4, 2.2, .1), M['steel'])
            box("Beliche fundo", (-3.65, y, z + .42), (.1, 2.2, .85), M['steel'])
            for yy in (y - 1.05, y + 1.05):
                box("Beliche lateral", (-3.0, yy, z + .42), (1.4, .1, .85), M['steel'])
            box("Colchao", (-3.0, y, z + .12), (1.2, 2.0, .12), M['mattress'])
    for k in range(6):
        for lvl in range(2):
            cyl((3.3, 2.0 + k * .62, .5 + lvl * 1.0), .27, .9, M['barrel'], verts=8, name="Barril")
            cyl((3.3, 2.0 + k * .62, .5 + lvl * 1.0), .285, .1, M['darksteel'], verts=8, name="Cinta barril")
    for sx in (3.3,):
        box("Prateleira barris", (sx, 5.0, 1.52), (.7, 4.0, .05), M['steel'])
    cyl((0, 5.5, .2), .75, .4, M['white'], verts=10, name="Pufe")
    cyl((0, 5.5, .41), .3, .02, M['mattress'], verts=10, name="Pufe centro")
    box("Monitor vital", (0, 12.28, 2.5), (1.2, .05, .6), M['screen_g'])
    box("Kit medico", (1.7, 12.27, 1.3), (.5, .08, .5), M['white'])
    box("Cruz", (1.7, 12.22, 1.3), (.3, .02, .08), M['red']); box("Cruz", (1.7, 12.22, 1.3), (.08, .02, .3), M['red'])
    box("Armario", (-1.4, 12.0, .45), (1.2, .5, .9), M['steel'])
    octagon("Escotilha fundo", (0, 12.3 - .02, 1.5), .75, .95, .06, M['purple'])
    octagon("Escotilha fundo2", (0, 12.3 - .06, 1.5), .6, .8, .06, M['door'])

# ---------- Agua / Engenharia (ciano)
def water_content():
    for k, yy in enumerate((3.0, 5.6, 8.2)):
        cyl((-2.7, yy, 1.7), 1.0, 3.2, M['tank'], verts=10, name="Tanque")
        cyl((-2.7, yy, 1.7), 1.03, .45, M['cyan'], verts=10, name="Faixa tanque")
        for dz in (-.75, .75):
            cyl((-2.7, yy, 1.7 + dz), 1.04, .12, M['darksteel'], verts=10, name="Cinta tanque")
        cyl((-1.55, yy, 1.1), .18, .06, M['cyan'], axis='X', verts=8, name="Valvula")
        cyl((-1.7, yy, .5), .1, 1.0, M['steel'], axis='X', verts=6, name="Cano")
    cyl((-1.3, 5.6, .5), .13, 7.0, M['steel'], axis='Y', verts=8, name="Cano longo")
    cyl((-1.3, 5.6, .85), .1, 7.0, M['steel'], axis='Y', verts=8, name="Cano longo")
    for yy in (3.0, 6.0):
        box("Estrado bomba", (2.9, yy, .15), (1.5, 1.6, .3), M['darksteel'])
        box("Bomba", (2.9, yy, .6), (1.0, .8, .6), M['steel'])
        cyl((2.9, yy + .5, .65), .25, .4, M['white'], axis='Y', verts=8, name="Bomba motor")
        cyl((3.3, yy, 1.2), .4, 1.0, M['tank'], verts=8, name="Reservatorio")
    box("Painel controle", (3.7, 8.6, 1.05), (.5, 1.1, 2.0), M['steel'])
    box("Tela controle", (3.42, 8.6, 1.45), (.05, .8, .5), M['screen'])
    box("Painel botoes", (3.42, 8.6, 1.0), (.05, .8, .2), M['darksteel'])
    # passarela
    box("Passarela", (0, 11.0, 2.0), (7.6, 2.3, .1), M['grate'])
    for sx in (-3.8, 3.8):
        beam((sx, 9.9, 1.0 + .0), (sx, 9.9, 3.0 - 1.0), .06, .06, M['steel'])
    for xx in (-3.5, 0, 3.5):
        for yy in (9.9, 12.0):
            beam((xx, yy, 0), (xx, yy, 2.0), .1, .1, M['darksteel'], "Perna passarela")
    beam((-3.8, 9.88, 2.9), (3.8, 9.88, 2.9), .05, .05, M['steel'], "Corrimao")
    beam((-3.8, 9.88, 2.45), (3.8, 9.88, 2.45), .05, .05, M['steel'], "Corrimao")
    for xx in (-1.0, -0.5):
        pass
    for yy in range(8, 10):
        pass
    for k in range(8):
        beam((-.4, 9.0 + .0, .25 + k * .25), (.4, 9.0 + (k * .0), .25 + k * .25), .04, .04, M['steel'], "Degrau")
    beam((-.4, 9.1, 0), (-.4, 9.9, 2.0), .05, .05, M['steel'], "Escada"); beam((.4, 9.1, 0), (.4, 9.9, 2.0), .05, .05, M['steel'], "Escada")
    for xx in (-3.6, -1.2, 1.2, 3.6):
        box("Painel vent", (xx, 12.27, 2.8), (.5, .05, .4), M['darksteel'])
    for xx in (-3.5, 3.5):
        box("Luz ciano parede", (xx, 12.25, 1.3), (.06, .06, 1.6), M['lamp_c'])

# ---------- Energia (amarelo)
def energy_content():
    for s in (-1, 1):
        for k in range(4):
            y = 2.3 + k * 2.4
            box("Rack", (s * 3.0, y, 1.3), (1.4, 1.8, 2.5), M['white'])
            for q in range(5):
                box("Persiana", (s * 2.28, y, .55 + q * .4), (.04, 1.4, .08), M['steel'])
            box("Rack luz", (s * 2.27, y + .85, 1.5), (.04, .06, 1.6), M['lamp_y'])
            cyl((s * 2.28, y, 2.3), .26, .05, M['darksteel'], axis='X', verts=8, name="Ventilador")
    box("Linha amarela", (-1.3, 6.6, .01), (.2, 11.6, .01), M['yellow'])
    box("Linha amarela", (1.3, 6.6, .01), (.2, 11.6, .01), M['yellow'])
    box("Grade central", (0, 6.6, .005), (2.4, 11.6, .01), M['grate'])
    box("Painel bat", (0, 12.2, 2.0), (1.6, .12, 1.2), M['white'])
    for k in range(3):
        box("Barra bateria", (-.5 + k * .5, 12.12, 2.0), (.3, .04, .9), M['lamp_y'])
    box("Console energia", (0, 11.7, .55), (1.7, .6, 1.0), M['steel'])
    box("Console topo", (0, 11.6, 1.07), (1.8, .5, .06), M['darksteel'])

# ---------- Eclusa
def astronaut(x, y, z, face=1):
    def b(dx, dy, dz, sx, sy, sz, m): return box("Astro", (x + dx, y + dy, z + dz), (sx, sy, sz), m)
    b(0, 0, 1.0, .5, .3, .6, M['white']); b(0, 0, .55, .46, .28, .3, M['white'])
    for sy_ in (-.12, .12):
        b(0, sy_, .22, .18, .18, .44, M['white']); b(0.04 * face, sy_, .02, .26, .2, .08, M['darksteel'])
        b(0, sy_ * 2.6, 1.0, .14, .14, .55, M['white'])
    b(-.28 * face, 0, 1.0, .12, .4, .5, M['steel'])
    ico((x, y, 1.5), (.22, .2, .22), M['white'], name="Capacete", sub=1)
    ico((x + .1 * face, y, 1.5), (.14, .16, .16), M['visor'], name="Visor", sub=1)

def airlock_content():
    for k, yy in enumerate((2.6, 4.9, 7.2)):
        box("Nicho fundo", (-3.88, yy, 1.45), (.1, 1.5, 2.8), M['darksteel'])
        for sy_ in (-.8, .8):
            box("Nicho lateral", (-3.5, yy + sy_, 1.45), (.8, .1, 2.9), M['steel'])
        box("Nicho topo", (-3.5, yy, 2.95), (.8, 1.7, .1), M['steel'])
        astronaut(-3.45, yy, 0.05)
    box("Banco", (3.5, 4.0, .45), (.7, 2.2, .1), M['steel']); box("Banco apoio", (3.7, 4.0, .22), (.3, 1.8, .4), M['darksteel'])
    box("Suporte ferr", (3.88, 7.0, 1.4), (.06, 1.8, .15), M['steel'])
    cyl((3.82, 6.5, 1.0), .03, 1.1, M['wood'], verts=5, name="Cabo vassoura")
    box("Vassoura", (3.82, 6.5, .35), (.12, .3, .25), M['wood'])
    for k, yy in enumerate((7.0, 7.3, 7.6)):
        box("Chave", (3.82, yy, 1.1), (.04, .05, .5), M['darksteel'])
    for k, m in enumerate((M['lamp_g'] if 'lamp_g' in M else M['led'], M['lamp_o'], M['led'], M['lamp_o'], M['led'])):
        box("Indicador", (-1.0 + k * .5, 12.2, 2.9), (.15, .05, .1), m)
    box("Tapete saida", (0, 11.2, .01), (2.6, 1.8, .02), M['dark'])
    for sx in (-1.2, 1.2):
        box("Faixa piso", (sx, 8.0, .012), (.2, 8.5, .01), M['yellow'])

def exterior_pad_eclusa(Mx):
    before = {o.name for o in bpy.data.objects}
    box("Plataforma saida", (0, 12.3 + T + 2.2, -.05), (4.0, 4.4, .2), M['steel'])
    for k in range(4):
        box("Degrau", (0, 12.3 + T + 4.6 + k * .45, -.12 - k * .0), (4.0, .45, .12), M['grate']).location.z = -.1 - (k) * .03
    for s in (-1, 1):
        beam((s * 1.9, 12.6, .0), (s * 1.9, 12.6, 1.0), .06, .06, M['darksteel'])
        beam((s * 1.9, 12.6 + 4.2, .0), (s * 1.9, 12.6 + 4.2, 1.0), .06, .06, M['darksteel'])
        beam((s * 1.9, 12.6, 1.0), (s * 1.9, 12.6 + 4.2, 1.0), .05, .05, M['darksteel'], "Corrimao")
    place(new_since(before), Mx)

MODS = {}
MODS['Laboratorio'] = room("Laboratorio", 30, M['orange'], M['lamp_o'], M['orange'], lab_content)
MODS['Dormitorio'] = room("Dormitorio", -150, M['purple'], M['lamp_p'], M['purple'], dorm_content)
MODS['Agua'] = room("Agua", 150, M['cyan'], M['lamp_c'], M['cyan'], water_content)
MODS['Energia'] = room("Energia", -30, M['yellow'], M['lamp_y'], M['yellow'], energy_content)
MODS['Eclusa'] = room("Eclusa", -90, M['steel'], M['lamp'], M['grey_door'], airlock_content, exit_door=M['grey_door'])
exterior_pad_eclusa(MODS['Eclusa'])

# detalhes externos dos modulos
def roof_props():
    # lab: antena + ar condicionado ; energia: caixas ; dormitorio: tubos
    Mx = MODS['Laboratorio']
    before = {o.name for o in bpy.data.objects}
    box("Condicionador", (2.2, 3.0, 3.9), (1.8, 1.2, .8), M['white']); cyl((2.9, 3.0, 3.9), .35, .1, M['darksteel'], axis='X', verts=10, name="Ventoinha")
    for k in range(3):
        beam((-2.2 + [0, .4, -.4][k] * 0.5, 10.0, 3.5), (-2.2, 10.0, 5.0), .06, .06, M['darksteel'], "Tripe")
    cyl((-2.2, 10.0, 5.1), .5, .1, M['white'], axis='Y', verts=10, name="Antena prato")
    box("Farol", (-2.2, 10.0, 5.35), (.12, .12, .15), M['lamp_o'])
    place(new_since(before), Mx)
    Mx = MODS['Agua']; before = {o.name for o in bpy.data.objects}
    cyl((5.6, 6.0, 1.3), 1.0, 5.0, M['tank'], axis='Y', verts=10, name="Cilindro externo"); cyl((5.6, 6.0, 1.3), 1.03, .3, M['cyan'], axis='Y', verts=10, name="Faixa cilindro")
    place(new_since(before), Mx)
roof_props()

# ================================================================ animacao das portas
for label, lf, ang in DOORS + [(None, o, None) for o in ()]:
    z0 = lf.rotation_euler.z
    lf.keyframe_insert("rotation_euler", frame=1)
    lf.rotation_euler.z = z0 + ang
    lf.keyframe_insert("rotation_euler", frame=40)
    act = lf.animation_data.action
    act.name = f"{lf.name}_Abrir"
    tr = lf.animation_data.nla_tracks.new(); tr.name = f"Porta_{label}_Abrir"
    tr.strips.new(act.name, 1, act)
    lf.animation_data.action = None
    lf.rotation_euler.z = z0
scene.frame_start, scene.frame_end = 1, 40

# ================================================================ EXTERIOR
# terreno
def smooth(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a))); return t * t * (3 - 2 * t)
def height(x, y):
    r = math.hypot(x, y)
    h = GZ
    h += 2.2 * noise.noise(Vector((x / 28, y / 28, 3.1))) * smooth(55, 95, r)
    h += 0.5 * noise.noise(Vector((x / 9, y / 9, 7.7))) * smooth(60, 100, r)
    h += smooth(105, 140, r) * (30 + 6 * noise.noise(Vector((x / 22, y / 22, 1.3))))
    return h
N, S = 150, 190.0
verts = [(-S + 2 * S * i / N, -S + 2 * S * j / N) for j in range(N + 1) for i in range(N + 1)]
vz = [(x, y, height(x, y)) for x, y in verts]
faces = [(j * (N + 1) + i, j * (N + 1) + i + 1, (j + 1) * (N + 1) + i + 1, (j + 1) * (N + 1) + i) for j in range(N) for i in range(N)]
poly("Terreno", vz, faces, M['moon'])

# areas ocupadas
def occupied(x, y, pad=0):
    r = math.hypot(x, y)
    if r < 9 + pad: return True
    for ang, *_ in PORTS:
        a = math.radians(ang); ca, sa = math.cos(a), math.sin(a)
        al = x * ca + y * sa; la = -x * sa + y * ca
        if 5 < al < 32 and abs(la) < 6.5 + pad: return True
    if abs(x) < 6 + pad and 14 < y < 40 + pad: return True
    for cx, cy, rr in OCC:
        if math.hypot(x - cx, y - cy) < rr + pad: return True
    return False
OCC = [(-47, 0, 14), (36, -2, 7), (30, -30, 9), (0, -44, 7), (-22, -42, 8), (38, 24, 6), (-36, 22, 6)]

def rock(x, y, s, m, name="Rocha"):
    o = ico((x, y, height(x, y) + s * .2), (s * random.uniform(.9, 1.4), s * random.uniform(.9, 1.3), s * random.uniform(.55, .9)), m, rot=(random.random(), random.random(), random.random() * 6), name=name, sub=1)
    for v in o.data.vertices:
        v.co += Vector((random.uniform(-.18, .18), random.uniform(-.18, .18), random.uniform(-.15, .15)))
    return o

cnt = 0
while cnt < 130:
    x, y = random.uniform(-100, 100), random.uniform(-100, 100)
    if occupied(x, y, 3) or math.hypot(x, y) > 105: continue
    rock(x, y, random.choice([.4, .6, .9, 1.4, 2.4, 3.8]) * (1 if math.hypot(x, y) < 60 else 1.7), random.choice([M['rock'], M['rock2']])); cnt += 1

def spire(x, y, h, w):
    z0 = height(x, y) - .5
    o = cyl((x, y, z0 + h / 2), w, h, M['rock2'], verts=6, name="Pico", r2=w * .25)
    for v in o.data.vertices:
        v.co += Vector((random.uniform(-.25, .25) * w, random.uniform(-.25, .25) * w, random.uniform(-.1, .1) * h * (v.co.z > 0)))
    return o
for _ in range(14):
    for tries in range(40):
        a = random.uniform(0, 6.28); r = random.uniform(70, 100); x, y = r * math.cos(a), r * math.sin(a)
        if not occupied(x, y): break
    spire(x, y, random.uniform(6, 16), random.uniform(1.4, 2.8))
    for k in range(3):
        spire(x + random.uniform(-3, 3), y + random.uniform(-3, 3), random.uniform(2, 7), random.uniform(.8, 1.6))

# campo solar
for ix in range(4):
    for iy in range(7):
        x, y = -54 + ix * 4.3, -14.4 + iy * 4.8
        z = GZ
        beam((x - .6, y, z), (x - .6, y, z + .8), .09, .09, M['darksteel'], "Perna solar")
        beam((x + .6, y, z), (x + .6, y, z + 1.3), .09, .09, M['darksteel'], "Perna solar")
        p = box("Painel solar", (x, y, z + 1.15), (1.7, 3.0, .06), M['solar'])
        p.rotation_euler = (0, math.radians(-22), 0)
        for k in (-1, 1):
            q = box("Moldura solar", (x, y + k * .75, z + 1.17), (1.74, .04, .08), M['steel']); q.rotation_euler = (0, math.radians(-22), 0)
        q = box("Moldura solar", (x, y, z + 1.17), (.04, 3.04, .08), M['steel']); q.rotation_euler = (0, math.radians(-22), 0)
# cabos do campo solar ate o hub
for k, yy in enumerate((-.5, 0, .5)):
    box("Cabo", (-30, yy, GZ + .12), (47, .12, .12), M['darksteel'])
box("Calha cabos", (-30, 0, GZ + .05), (47, 1.6, .06), M['steel'])
for xx in range(-52, -8, 6):
    box("Braçadeira", (xx, 0, GZ + .13), (.25, 1.5, .18), M['steel'])

# prato de antena
def dish(x, y, s=1.0):
    z = GZ
    cyl((x, y, z + .2), 1.2 * s, .4, M['white'], verts=10, name="Antena base")
    cyl((x, y, z + 1.2), .35 * s, 2.0, M['steel'], verts=8, name="Antena torre")
    d = cyl((x, y + .6 * s, z + 2.6 * s), 2.4 * s, .5 * s, M['white'], axis='Y', verts=12, name="Prato", r2=.15 * s)
    d.rotation_euler = (math.radians(-65), 0, 0) if False else (math.radians(-45) + math.pi / 2, 0, 0)
    beam((x, y + .6 * s, z + 2.6 * s), (x, y + 1.7 * s, z + 3.6 * s), .08, .08, M['darksteel'], "Alimentador")
dish(36, -2); dish(-36, 22, .8)

# lander
def lander(x, y):
    z = GZ
    box("Pad lander", (x, y, z + .1), (14, 12, .2), M['darksteel'])
    for k in range(-1, 2):
        box("Pad faixa", (x, y + k * 3, z + .205), (13, .25, .02), M['steel'])
    box("Pad borda", (x, y, z + .21), (14.2, 12.2, .03), M['white']).scale.z = .02
    cyl((x, y, z + 2.5), 1.6, 2.2, M['white'], verts=8, name="Lander corpo")
    cyl((x, y, z + 3.8), 1.2, .6, M['steel'], verts=8, name="Lander topo", r2=.7)
    cyl((x, y + 1.0, z + 2.6), .45, .1, M['glass'], axis='Y', verts=8, name="Janela lander")
    for k in range(4):
        a = math.pi / 4 + k * math.pi / 2
        beam((x + math.cos(a) * 1.4, y + math.sin(a) * 1.4, z + 1.7), (x + math.cos(a) * 3.4, y + math.sin(a) * 3.4, z + .25), .12, .12, M['darksteel'], "Perna lander")
        box("Pe lander", (x + math.cos(a) * 3.5, y + math.sin(a) * 3.5, z + .25), (.7, .7, .08), M['darksteel'])
lander(30, -30)

# containers
def container(x, y, z, rot=0):
    c = box("Container", (x, y, z + .6), (2.4, 1.2, 1.2), M['white']); c.rotation_euler = (0, 0, rot)
    for s in (-1, 1):
        for sy_ in (-1, 1):
            q = box("Container canto", (x + math.cos(rot) * s * 1.2 - math.sin(rot) * sy_ * .6, y + math.sin(rot) * s * 1.2 + math.cos(rot) * sy_ * .6, z + .6), (.1, .1, 1.24), M['darksteel'])
    q = box("Container faixa", (x, y, z + .6), (2.42, 1.22, .22), M['yellow']); q.rotation_euler = (0, 0, rot)
for i, (dx, dy, dz) in enumerate([(0, 0, 0), (2.6, 0, 0), (0, 1.3, 0), (2.6, 1.3, 0), (1.3, .65, 1.2)]):
    container(34 + dx, 24 + dy, GZ + dz, .3) if False else container(36 + dx, 24 + dy, GZ + dz)
container(-40, 22, GZ); container(-40, 23.3, GZ)

# rover
def wheel(x, y, z):
    cyl((x, y, z), .45, .4, M['darksteel'], axis='X', verts=10, name="Roda"); cyl((x + (.22 if x > 0 else -.22), y, z), .22, .06, M['steel'], axis='X', verts=8, name="Cubo roda")
def rover(x, y, z, yaw=0):
    before = {o.name for o in bpy.data.objects}
    box("Rover chassi", (0, 0, .85), (2.0, 4.0, .6), M['white'])
    box("Rover cabine", (0, 1.0, 1.4), (1.8, 1.8, .7), M['white'])
    box("Rover para-brisa", (0, 1.92, 1.45), (1.5, .05, .45), M['glass'])
    box("Rover tras", (0, -1.4, 1.35), (1.6, 1.2, .5), M['steel'])
    box("Rover faixa", (0, 0, .95), (2.02, 4.02, .08), M['orange'])
    box("Rover luz", (-.6, 2.02, .9), (.2, .05, .1), M['lamp']); box("Rover luz", (.6, 2.02, .9), (.2, .05, .1), M['lamp'])
    for yy in (-1.4, 0, 1.4):
        wheel(-1.2, yy, .45); wheel(1.2, yy, .45)
    beam((.6, -1.4, 1.6), (.6, -1.4, 2.4), .03, .03, M['darksteel'], "Antena rover")
    place(new_since(before), Mtr(x, y, z, yaw))
rover(8, -38, GZ, math.radians(30))

# ============================ flat shading
for o in bpy.data.objects:
    if o.type == 'MESH':
        for p in o.data.polygons: p.use_smooth = False
        if o.data.materials and o.data.materials[0] and o.data.materials[0].name in ("Vidro",): o.visible_shadow = False

# ================================================================ junta por material + exporta so o cenario
bpy.ops.object.select_all(action='DESELECT')
groups = {}
for o in bpy.data.objects:
    if o.type == 'MESH' and not o.name.startswith("Porta_") and o.data.materials:
        groups.setdefault(o.data.materials[0].name, []).append(o)
for name, objs in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    bpy.context.object.name = "Base_" + name
for o in bpy.data.objects:
    if o.type == 'MESH' and o.data.materials and o.data.materials[0].name == "Vidro": o.visible_shadow = False
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
    if o.type == 'MESH': o.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "base.blend"))
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, "base.glb"), export_format='GLB', use_selection=True,
                          export_lights=False, export_cameras=False, export_animations=True, export_animation_mode='NLA_TRACKS')
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "base.fbx"), use_selection=True, object_types={'MESH'},
                         apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', bake_anim=True, bake_anim_use_nla_strips=True,
                         bake_anim_use_all_actions=False, bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE')
print("OK EXPORT")

# ======== daqui para baixo: so previas (nao vao para o .blend/.glb/.fbx) ========
import sys
only = [a for a in sys.argv if a.startswith("--only=")]
only = only[0].split("=")[1].split(",") if only else None

scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_x, scene.render.resolution_y = 1100, 720
scene.eevee.taa_render_samples = 24
try: scene.eevee.shadow_pool_size = '1024'
except Exception: pass
scene.view_settings.view_transform = 'Standard'
scene.view_settings.exposure = -0.2
world = bpy.data.worlds.new("Espaco"); scene.world = world; world.use_nodes = True
world.node_tree.nodes["Background"].inputs["Color"].default_value = (0, 0, 0.003, 1)

earth_m = bpy.data.materials.new("Terra planeta"); earth_m.use_nodes = True
nt = earth_m.node_tree
for n_ in list(nt.nodes): nt.nodes.remove(n_)
out = nt.nodes.new("ShaderNodeOutputMaterial"); em = nt.nodes.new("ShaderNodeEmission"); em.inputs["Strength"].default_value = 1.6
noi = nt.nodes.new("ShaderNodeTexNoise"); noi.inputs["Scale"].default_value = 3.5; noi.inputs["Detail"].default_value = 6
rp = nt.nodes.new("ShaderNodeValToRGB"); rp.color_ramp.elements[0].position = .6; rp.color_ramp.elements[0].color = (.03, .15, .55, 1)
rp.color_ramp.elements[1].position = .7; rp.color_ramp.elements[1].color = (.9, .95, 1, 1)
tc = nt.nodes.new("ShaderNodeTexCoord")
nt.links.new(tc.outputs["Object"], noi.inputs["Vector"]); nt.links.new(noi.outputs["Fac"], rp.inputs["Fac"])
nt.links.new(rp.outputs["Color"], em.inputs["Color"]); nt.links.new(em.outputs["Emission"], out.inputs["Surface"])
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=7, location=(-80, 190, 95))
assign(bpy.context.object, earth_m); bpy.ops.object.shade_smooth(); bpy.context.object.visible_shadow = False

sun = bpy.data.lights.new("Sol", 'SUN'); sun.energy = 4.2; sun.color = (1, .96, .88); sun.angle = math.radians(.6)
so = bpy.data.objects.new("Sol", sun); bpy.context.collection.objects.link(so)
so.rotation_euler = (math.radians(68), 0, math.radians(38))

def render_shot(name, cam_pos, target, lens=20, light_keys=(), res=None):
    if only and name not in only: return
    cd = bpy.data.cameras.new("c"); cd.lens = lens; cd.clip_end = 1000
    co = bpy.data.objects.new("c", cd); bpy.context.collection.objects.link(co)
    co.location = cam_pos; co.rotation_mode = 'QUATERNION'
    co.rotation_quaternion = (Vector(target) - Vector(cam_pos)).to_track_quat('-Z', 'Y')
    scene.camera = co
    made = []
    for key in light_keys:
        for item in LIGHTS[key]:
            loc, sx, sy, col, en, shape = item[:6]
            ld = bpy.data.lights.new("f", 'AREA'); ld.shape = shape; ld.size = sx; ld.size_y = sy; ld.energy = en; ld.color = col
            lo = bpy.data.objects.new("f", ld); bpy.context.collection.objects.link(lo); lo.location = loc
            if len(item) > 6: lo.rotation_euler = (math.pi, 0, 0)
            made.append((lo, ld))
    scene.render.resolution_x, scene.render.resolution_y = res or (1100, 720)
    scene.render.filepath = os.path.join(OUT, f"base_{name}.png")
    bpy.ops.render.render(write_still=True)
    for lo, ld in made: bpy.data.objects.remove(lo); bpy.data.lights.remove(ld)
    bpy.data.objects.remove(co)

def W(Mx, v): return Mx @ Vector(v)
render_shot("aerea", (-52, -62, 38), (0, 6, 2), 24, res=(1400, 800))
render_shot("aerea2", (50, -48, 26), (0, 2, 2), 24, res=(1400, 800))
render_shot("hub", (0, -5.6, 1.7), (0, 3, 1.7), 16, ("hub",))
for nm, ang in (("Laboratorio", 30), ("Dormitorio", -150), ("Agua", 150), ("Energia", -30), ("Eclusa", -90)):
    Mx = MODS[nm]
    render_shot(f"{nm.lower()}", W(Mx, (0, T + .8, 1.65)), W(Mx, (0, 12, 1.5)), 16, (nm,))
Mx = MODS['Eclusa']
render_shot("eclusa_lado", W(Mx, (3.0, 11.0, 1.6)), W(Mx, (-3.6, 4.5, 1.2)), 16, ("Eclusa",))
a = math.radians(150)
render_shot("tunel", (math.cos(a) * 7.2, math.sin(a) * 7.2, 1.6), (math.cos(a) * 16, math.sin(a) * 16, 1.5), 20, ("tunel_Agua",))
for o in DOORS:
    for tr in o[1].animation_data.nla_tracks: tr.mute = True
    o[1].rotation_euler.z += o[2]
render_shot("hub_portas_abertas", (0, -5.6, 1.7), (0, 3, 1.7), 16, ("hub",))
print("OK PREVIAS")
