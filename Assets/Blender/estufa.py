"""Estufa (greenhouse) - low-poly lunar base interior for "Ten Days on the Moon".
Run:  blender -b -P estufa.py
Outputs next to this script: estufa.blend, estufa.glb, estufa_render.png
Units: meters. +Y = down the length of the greenhouse, Z up. Door at the far end.
"""
import bpy, math, random, os
from mathutils import Vector

OUT = os.path.dirname(os.path.abspath(__file__))
random.seed(7)

# ---------------------------------------------------------------- scene reset
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'

W = 4.0          # half width
L = 22.0         # length
WALL_H = 3.0
PEAK = 6.5
BAY = 3.2
Y0 = 0.5

# ---------------------------------------------------------------- materials
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
)

# ---------------------------------------------------------------- helpers
def assign(o, m):
    o.data.materials.clear()
    o.data.materials.append(m)
    return o

def box(name, center, size, m):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center)
    o = bpy.context.object
    o.name = name
    o.scale = size
    return assign(o, m)

def beam(p1, p2, w, d, m, name="Viga"):
    p1, p2 = Vector(p1), Vector(p2)
    v = p2 - p1
    bpy.ops.mesh.primitive_cube_add(size=1, location=(p1 + p2) / 2)
    o = bpy.context.object
    o.name = name
    o.scale = (w, d, v.length)
    o.rotation_mode = 'QUATERNION'
    o.rotation_quaternion = v.to_track_quat('Z', 'Y')
    return assign(o, m)

def poly(name, verts, faces, m):
    me = bpy.data.meshes.new(name)
    me.from_pydata(verts, [], faces)
    me.update()
    o = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(o)
    return assign(o, m)

def ico(loc, scale, m, rot=(0, 0, 0), name="Ico"):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, location=loc, rotation=rot)
    o = bpy.context.object
    o.scale = scale
    o.name = name
    return assign(o, m)

def cone(loc, r1, r2, h, m, rot=(0, 0, 0), verts=5, name="Cone"):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=h, location=loc, rotation=rot)
    o = bpy.context.object
    o.name = name
    return assign(o, m)

def octagon(name, center, w, h, thick, m):
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=1, depth=1)
    o = bpy.context.object
    o.rotation_euler = (0, 0, math.pi / 8)
    bpy.ops.object.transform_apply(rotation=True)
    o.rotation_euler = (math.pi / 2, 0, 0)
    o.scale = (w, h, thick)
    o.location = center
    o.name = name
    return assign(o, m)

# ---------------------------------------------------------------- shell
box("Piso", (0, L / 2, -0.1), (2 * W, L, 0.2), M['floor'])
# faixa central de piso (corredor) - leve contraste
box("Corredor", (0, L / 2, 0.003), (2.3, L, 0.01), mat("Corredor", (0.66, 0.67, 0.69), 0.9))

for s in (-1, 1):
    box("Parede baixa", (s * (W + 0.1), L / 2, 0.7), (0.2, L, 1.4), M['dark'])
    box("Parede alta", (s * (W + 0.1), L / 2, 2.2), (0.2, L, 1.6), M['white'])
    box("Friso", (s * (W - 0.01), L / 2, 1.4), (0.04, L, 0.05), M['darksteel'])
# paredes das extremidades (fundo e entrada): lajes fechadas + oitao solido + porta atravessando a parede
T = 0.3
def prism(name, y1, y2, m):
    v = [(-W - .2, y1, WALL_H), (W + .2, y1, WALL_H), (0, y1, PEAK + .05),
         (-W - .2, y2, WALL_H), (W + .2, y2, WALL_H), (0, y2, PEAK + .05)]
    f = [(0, 2, 1), (3, 4, 5), (0, 1, 4, 3), (1, 2, 5, 4), (2, 0, 3, 5)]
    return poly(name, v, f, m)

DOORS = []   # (nome, folha, angulo_aberto)

def bool_cut(obj, cutter):
    mod = obj.modifiers.new("Corte", 'BOOLEAN')
    mod.operation = 'DIFFERENCE'
    mod.object = cutter
    mod.solver = 'EXACT'
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier="Corte")

def leaf_mesh(name, w, h, thick, side, m):
    """Meia folha octogonal; origem na dobradica (borda externa). side=+1 direita, -1 esquerda."""
    c, s1 = math.cos(math.radians(22.5)), math.sin(math.radians(22.5))
    c2, s2 = math.cos(math.radians(67.5)), math.sin(math.radians(67.5))
    hx = w * c
    pts = [(0, -h * s2), (w * c2, -h * s2), (hx, -h * s1), (hx, h * s1), (w * c2, h * s2), (0, h * s2)]
    pts = [((x - hx) * (1 if side > 0 else -1), z) for x, z in pts]
    t = thick / 2
    n = len(pts)
    v = [(x, -t, z) for x, z in pts] + [(x, t, z) for x, z in pts]
    f = [tuple(range(n))[::-1], tuple(range(n, 2 * n))]
    if side < 0:
        f = [f[1], f[0]]
    for i in range(n):
        j = (i + 1) % n
        f.append((i, j, j + n, i + n) if side > 0 else (i, i + n, j + n, j))
    cx = -side * (hx - .16)      # macaneta junto ao encontro das folhas
    base = len(v)
    for dx in (-.025, .025):
        for dy in (-t - .05, t + .05):
            for dz in (-.2, .1):
                v.append((cx + dx, dy, dz))
    f += [tuple(base + i for i in q) for q in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]]
    return poly(name, v, f, m)

def end_wall(yi, d, label):
    """yi = face interna da parede, d = +1 se o interior fica em +Y (entrada), -1 se em -Y (fundo)."""
    yc = yi - d * T / 2
    y1, y2 = sorted((yi, yi - d * T))
    low = box("Parede fim baixa", (0, yc, 0.7), (2 * W + 0.4, T, 1.4), M['dark'])
    high = box("Parede fim alta", (0, yc, 2.2), (2 * W + 0.4, T, 1.6), M['white'])
    prism("Oitao", y1, y2, M['white'])
    cut_wall = octagon("c1", (0, yc, 1.5), 1.2, 1.5, T + .4, M['white'])
    frame = octagon("Porta moldura", (0, yc, 1.5), 1.2, 1.5, T + .08, M['green_trim'])
    cut_in = octagon("c2", (0, yc, 1.5), 1.0, 1.3, T + .5, M['white'])
    bool_cut(low, cut_wall); bool_cut(high, cut_wall); bool_cut(frame, cut_in)
    bpy.data.objects.remove(cut_wall); bpy.data.objects.remove(cut_in)
    w, h = .99, 1.29
    hx = w * math.cos(math.radians(22.5))
    for side, nm in ((1, "D"), (-1, "E")):
        lf = leaf_mesh(f"Porta_{label}_{nm}", w, h, .1, side, M['door'])
        lf.location = (side * hx, yc, 1.5)
        DOORS.append((label, lf, math.radians(105) * (-d if side > 0 else d)))

end_wall(L, -1, "Fundo")
end_wall(0, 1, "Entrada")

# ---------------------------------------------------------------- trusses
def zigzag(a_pts, b_pts, w, d, m):
    n = len(a_pts) - 1
    for k in range(n):
        if k % 2 == 0:
            beam(a_pts[k], b_pts[k + 1], w, d, m, "Diagonal")
        else:
            beam(b_pts[k], a_pts[k + 1], w, d, m, "Diagonal")

def lerp(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))

def truss(y):
    for s in (-1, 1):
        # coluna
        co = [(s * 3.92, y, z) for z in (0, WALL_H)]
        ci = [(s * 3.5, y, z) for z in (0, WALL_H)]
        beam(co[0], co[1], .09, .12, M['steel'], "Coluna")
        beam(ci[0], ci[1], .09, .12, M['steel'], "Coluna")
        zigzag([lerp(co[0], co[1], k / 4) for k in range(5)], [lerp(ci[0], ci[1], k / 4) for k in range(5)], .05, .07, M['steel'])
        # empena (banzo externo / interno)
        ao, po = (s * 3.92, y, WALL_H), (0, y, PEAK)
        ai, pi_ = (s * 3.5, y, WALL_H), (0, y, PEAK - .42)
        beam(ao, po, .1, .13, M['steel'], "Banzo")
        beam(ai, pi_, .09, .12, M['steel'], "Banzo")
        n = 7
        zigzag([lerp(ao, po, k / n) for k in range(n + 1)], [lerp(ai, pi_, k / n) for k in range(n + 1)], .05, .07, M['steel'])
    # tirante
    z = 5.25
    xo = 3.92 * (1 - (z - WALL_H) / (PEAK - WALL_H))
    beam((-xo, y, z), (xo, y, z), .1, .12, M['steel'], "Tirante")
    beam((0, y, z), (0, y, PEAK - .2), .08, .1, M['steel'], "Pendural")

ys = [Y0 + BAY * i for i in range(7)]
for y in ys:
    truss(y)

# longitudinais
beam((0, 0, PEAK), (0, L, PEAK), .16, .2, M['steel'], "Cumeeira")
for s in (-1, 1):
    beam((s * 3.92, 0, WALL_H), (s * 3.92, L, WALL_H), .16, .16, M['steel'], "Beiral")
    beam((s * 1.96, 0, (WALL_H + PEAK) / 2 + .05), (s * 1.96, L, (WALL_H + PEAK) / 2 + .05), .1, .12, M['steel'], "Terca")

# vidro do telhado (com espessura, estende ate as paredes das pontas)
glass = []
for s_ in (-1, 1):
    g = poly("Vidro", [(s_ * 4.2, -T, WALL_H - .01), (s_ * 4.2, L + T, WALL_H - .01), (0, L + T, PEAK + .03), (0, -T, PEAK + .03)],
             [(0, 1, 2, 3)] if s_ > 0 else [(0, 3, 2, 1)], M['glass'])
    mod = g.modifiers.new("Esp", 'SOLIDIFY')
    mod.thickness = 0.05
    bpy.context.view_layer.objects.active = g
    bpy.ops.object.modifier_apply(modifier="Esp")
    g.visible_shadow = False
    glass.append(g)
# claraboias abertas no lado esquerdo (vidro quase sem tinta) para entrar luz
# (a luz do sol atravessa o vidro porque ele nao projeta sombra)

# ---------------------------------------------------------------- luzes de LED verdes
for i in range(6):
    yc = Y0 + BAY * i + BAY / 2
    for s in (-1, 1):
        box("LED", (s * (W - 0.05), yc, 2.3), (0.05, 1.9, 0.09), M['led'])

# ---------------------------------------------------------------- canteiros e plantas
def lettuce(x, y, z):
    n = random.randint(7, 9)
    base = random.choice([M['leaf1'], M['leaf2'], M['leaf3']])
    for k in range(n):
        a = 2 * math.pi * k / n + random.uniform(-.2, .2)
        r = random.uniform(.06, .12)
        tilt = random.uniform(.5, .9)
        ico((x + math.cos(a) * r, y + math.sin(a) * r, z + .1 + .02 * (k % 2)), (.16, .12, .05),
            random.choice([base, base, M['leaf2']]), rot=(math.sin(a) * tilt, -math.cos(a) * tilt, a), name="Alface")
    ico((x, y, z + .18), (.1, .1, .08), M['leaf2'], name="Alface")

def wheat(x, y, z):
    for k in range(random.randint(3, 5)):
        dx, dy = random.uniform(-.14, .14), random.uniform(-.14, .14)
        h = random.uniform(.55, .85)
        cone((x + dx, y + dy, z + h / 2), .018, .008, h, M['stalk'], verts=4, name="Haste")
        ico((x + dx, y + dy, z + h + .05), (.04, .04, .15), M['wheat'], name="Espiga")
        for q in range(2):
            a = random.uniform(0, 6.28)
            cone((x + dx + math.cos(a) * .05, y + dy + math.sin(a) * .05, z + .22), .03, .004, .45, M['leaf1'],
                 rot=(math.sin(a) * .45, -math.cos(a) * .45, 0), verts=3, name="Folha")

def bushplant(x, y, z):
    for k in range(random.randint(9, 12)):
        a = 2 * math.pi * k / 10 + random.uniform(-.3, .3)
        t = random.uniform(.7, 1.1)
        h = random.uniform(.14, .38)
        r = random.uniform(.06, .17)
        ico((x + math.cos(a) * r, y + math.sin(a) * r, z + h), (.17, .07, .025), random.choice([M['bush'], M['leaf3'], M['leaf1']]),
            rot=(math.sin(a) * t, -math.cos(a) * t, a), name="Folhagem")
    cone((x, y, z + .12), .03, .015, .24, M['stalk'], verts=4, name="Haste")

PLANTS = [lettuce, wheat, bushplant, lettuce, bushplant, wheat]

def planter(cx, cy, kind, size=(2.15, 2.8)):
    sx, sy = size
    h = 0.7
    box("Canteiro", (cx, cy, h / 2 - .02), (sx, sy, h), M['white'])
    box("Canteiro base", (cx, cy, .06), (sx + .06, sy + .06, .12), M['darksteel'])
    for dx in (-1, 1):                              # suportes de canto
        for dy in (-1, 1):
            box("Canto", (cx + dx * sx / 2, cy + dy * sy / 2, h - .05), (.12, .12, .16), M['darksteel'])
    box("Borda", (cx, cy, h + .005), (sx + .04, sy + .04, .04), M['steel'])
    soil = box("Terra", (cx, cy, h + .03), (sx - .16, sy - .16, .06), M['soil'])
    # relevo da terra
    box("Terra montes", (cx, cy, h + .07), (sx - .5, sy - .5, .04), M['soil2'])
    cols, rows = 3, 4
    for i in range(cols):
        for j in range(rows):
            px = cx + (i - (cols - 1) / 2) * (sx - .5) / (cols - 1) * 0.95 + random.uniform(-.07, .07)
            py = cy + (j - (rows - 1) / 2) * (sy - .55) / (rows - 1) + random.uniform(-.07, .07)
            fn = kind if random.random() < .75 else random.choice(PLANTS)
            fn(px, py, h + .09)

for s in (-1, 1):
    for i in range(6):
        yc = Y0 + BAY * i + BAY / 2
        planter(s * 2.42, yc, PLANTS[(i + (0 if s < 0 else 2)) % len(PLANTS)])


# ---------------------------------------------------------------- animacao das portas (Abrir: frames 1 -> 40)
for label, lf, ang in DOORS:
    lf.rotation_euler = (0, 0, 0)
    lf.keyframe_insert("rotation_euler", frame=1)
    lf.rotation_euler = (0, 0, ang)
    lf.keyframe_insert("rotation_euler", frame=40)
    act = lf.animation_data.action
    act.name = f"{lf.name}_Abrir"
    tr = lf.animation_data.nla_tracks.new()
    tr.name = f"Porta_{label}_Abrir"
    tr.strips.new(act.name, 1, act)
    lf.animation_data.action = None
    lf.rotation_euler = (0, 0, 0)
scene.frame_start, scene.frame_end = 1, 40

# ---------------------------------------------------------------- flat shading no resto (visual low-poly)
for o in bpy.data.objects:
    if o.type == 'MESH':
        for p in o.data.polygons:
            p.use_smooth = False

# ---------------------------------------------------------------- junta por material e exporta SO o cenario
bpy.ops.object.select_all(action='DESELECT')
groups = {}
for o in bpy.data.objects:
    if o.type == 'MESH' and not o.name.startswith("Porta_") and o.data.materials:
        groups.setdefault(o.data.materials[0].name, []).append(o)
for name, objs in groups.items():
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    bpy.context.object.name = "Estufa_" + name

bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
    if o.type == 'MESH':
        o.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "estufa.blend"))
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, "estufa.glb"), export_format='GLB', use_selection=True,
                          export_lights=False, export_cameras=False, export_animations=True,
                          export_animation_mode='NLA_TRACKS')
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "estufa.fbx"), use_selection=True, object_types={'MESH'},
                         apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', embed_textures=False,
                         bake_anim=True, bake_anim_use_nla_strips=True, bake_anim_use_all_actions=False,
                         bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE')
print("OK")

# ======== daqui para baixo: so previa (nao vai para o .blend nem para o .glb) ========
# ---------------------------------------------------------------- exterior (visivel pelo vidro)
bpy.ops.mesh.primitive_plane_add(size=400, location=(0, 60, -0.3))
assign(bpy.context.object, M['moon']).name = "Solo lunar"

# Terra
earth_m = bpy.data.materials.new("Terra planeta")
earth_m.use_nodes = True
nt = earth_m.node_tree
for n in list(nt.nodes):
    nt.nodes.remove(n)
out = nt.nodes.new("ShaderNodeOutputMaterial")
em = nt.nodes.new("ShaderNodeEmission")
em.inputs["Strength"].default_value = 1.6
noise = nt.nodes.new("ShaderNodeTexNoise")
noise.inputs["Scale"].default_value = 3.5
noise.inputs["Detail"].default_value = 6
ramp = nt.nodes.new("ShaderNodeValToRGB")
ramp.color_ramp.elements[0].position = 0.60
ramp.color_ramp.elements[0].color = (0.03, 0.15, 0.55, 1)
ramp.color_ramp.elements[1].position = 0.70
ramp.color_ramp.elements[1].color = (0.9, 0.95, 1, 1)
tc = nt.nodes.new("ShaderNodeTexCoord")
nt.links.new(tc.outputs["Object"], noise.inputs["Vector"])
nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
nt.links.new(ramp.outputs["Color"], em.inputs["Color"])
nt.links.new(em.outputs["Emission"], out.inputs["Surface"])
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=2.2, location=(-7, 30, 17))
ob = bpy.context.object
ob.name = "Terra (ceu)"
assign(ob, earth_m)
bpy.ops.object.shade_smooth()
ob.visible_shadow = False

# ---------------------------------------------------------------- luzes
sun = bpy.data.lights.new("Sol", 'SUN')
sun.energy = 4.0
sun.color = (1.0, 0.96, 0.88)
sun.angle = math.radians(0.6)
so = bpy.data.objects.new("Sol", sun)
bpy.context.collection.objects.link(so)
so.rotation_euler = (math.radians(50), 0, math.radians(52))

fill = bpy.data.lights.new("Luz ambiente", 'AREA')
fill.shape = 'RECTANGLE'
fill.size, fill.size_y = 5.0, 18.0
fill.energy = 330
fill.color = (0.8, 1.0, 0.85)
fo = bpy.data.objects.new("Luz ambiente", fill)
bpy.context.collection.objects.link(fo)
fo.location = (0, L / 2, 5.3)
fo.rotation_euler = (0, 0, 0)

world = bpy.data.worlds.new("Espaco")
scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes["Background"]
bg.inputs["Color"].default_value = (0.0, 0.0, 0.003, 1)
bg.inputs["Strength"].default_value = 1.0

# ---------------------------------------------------------------- camera
cam_d = bpy.data.cameras.new("Camera")
cam_d.lens = 20
cam = bpy.data.objects.new("Camera", cam_d)
bpy.context.collection.objects.link(cam)
cam.location = (0.1, 0.2, 1.65)
dirv = Vector((0, L, 2.2)) - Vector(cam.location)
cam.rotation_mode = 'QUATERNION'
cam.rotation_quaternion = dirv.to_track_quat('-Z', 'Y')
scene.camera = cam

# ---------------------------------------------------------------- render
scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_x = 1600
scene.render.resolution_y = 1100
scene.eevee.taa_render_samples = 48
try:
    scene.eevee.shadow_pool_size = '1024'
except Exception as e:
    print('pool', e)
scene.view_settings.view_transform = 'Standard'
scene.view_settings.exposure = -0.2
scene.render.filepath = os.path.join(OUT, "estufa_render.png")
bpy.ops.render.render(write_still=True)

# vista externa (verificar se a casca esta fechada)
for o in bpy.data.objects:
    if o.name == "Solo lunar":
        o.location.z = -0.3
cam2d = bpy.data.cameras.new("CamExterna"); cam2d.lens = 24
cam2 = bpy.data.objects.new("CamExterna", cam2d); bpy.context.collection.objects.link(cam2)
cam2.location = (9, L + 12, 6)
cam2.rotation_mode = 'QUATERNION'
cam2.rotation_quaternion = (Vector((0, L - 1, 2.5)) - Vector(cam2.location)).to_track_quat('-Z', 'Y')
scene.camera = cam2
scene.render.resolution_x, scene.render.resolution_y = 1000, 700
scene.render.filepath = os.path.join(OUT, "estufa_externa.png")
bpy.ops.render.render(write_still=True)
scene.camera = cam


# previa com as portas abertas
for label, lf, ang in DOORS:
    for tr in lf.animation_data.nla_tracks:
        tr.mute = True
    lf.rotation_euler = (0, 0, ang)
scene.render.resolution_x, scene.render.resolution_y = 1600, 1100
scene.render.filepath = os.path.join(OUT, "estufa_porta_aberta.png")
bpy.ops.render.render(write_still=True)
