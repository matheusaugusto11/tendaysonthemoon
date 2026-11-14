"""Iluminacao da base lunar - "Ten Days on the Moon".
Abre base.blend (gerado por base.py), adiciona o sistema de luz e exporta:
  base_iluminada.blend / .glb / .fbx   -> cenario + luzes (todas ligadas na intensidade maxima 'e0'; cada luz tem extras: grupo, e0)
  iluminacao_presets.json              -> multiplicadores por grupo para os cenarios Dia, Erupcao e Noite
  iluminacao_<cenario>_<vista>.png     -> previas (Terra/cameras so existem nas previas)
Run:  blender -b -P iluminacao.py            (opcional: -- --only=dia_hub,noite_aerea)
Grupos de luz: sol, terra, int (interior), ext (postes/faróis), emerg (emergencia, vermelhas)
"""
import bpy, math, os, sys, json
from mathutils import Vector, Matrix

OUT = os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT, "base.blend"))
scene = bpy.context.scene
GZ = -0.2
TUN_R1 = 15.85
only = [a for a in sys.argv if a.startswith("--only=")]
only = only[0].split("=")[1].split(",") if only else None

def rad(a): return math.radians(a)
def Mtr(x=0, y=0, z=0, yaw=0.0): return Matrix.Translation((x, y, z)) @ Matrix.Rotation(yaw, 4, 'Z')
def Mmod(ang): return Mtr(0, 0, 0, rad(ang) - math.pi / 2) @ Matrix.Translation((0, TUN_R1, 0))

# ---------------------------------------------------------------- colecao de luzes
col = bpy.data.collections.new("Iluminacao")
scene.collection.children.link(col)
LIGHTS = []

def mk_light(name, kind, loc, energy, color, grupo, shadow=True, radius=.25, spot=None, target=None, size=None):
    ld = bpy.data.lights.new(name, kind)
    ld.energy = energy; ld.color = color
    if kind in ('POINT', 'SPOT'): ld.shadow_soft_size = radius
    if kind == 'SPOT':
        ld.spot_size = rad(spot or 70); ld.spot_blend = .5
    ld.use_shadow = shadow
    o = bpy.data.objects.new(name, ld)
    col.objects.link(o)
    o.location = loc
    if target is not None:
        o.rotation_mode = 'QUATERNION'
        o.rotation_quaternion = (Vector(target) - Vector(loc)).to_track_quat('-Z', 'Y')
    o["grupo"] = grupo; o["e0"] = float(energy)
    LIGHTS.append(o)
    return o

# ---------------------------------------------------------------- materiais (para os postes)
def mat(name, color, rough=.6, metal=.3, emit=None, es=0):
    m = bpy.data.materials.get(name)
    if m: return m
    m = bpy.data.materials.new(name); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1); b.inputs["Roughness"].default_value = rough; b.inputs["Metallic"].default_value = metal
    if emit:
        b.inputs["Emission Color"].default_value = (*emit, 1); b.inputs["Emission Strength"].default_value = es
    return m
M_DARK = mat("Metal escuro", (.12, .13, .15)); M_STEEL = mat("Estrutura", (.62, .64, .66), .55, .4)
M_LAMP = mat("Lampada", (1.0, .92, .78), .4, 0, (1.0, .92, .78), 8)
M_LAMPR = mat("Lampada vermelha", (1, .12, .08), .4, 0, (1, .12, .08), 6)

def _obj(name, mesh_op, m, **kw):
    mesh_op(**kw)
    o = bpy.context.object; o.name = name
    o.data.materials.clear(); o.data.materials.append(m)
    return o
def box(name, c, size, m):
    o = _obj(name, bpy.ops.mesh.primitive_cube_add, m, size=1, location=c); o.scale = size; return o
def beam(p1, p2, w, m, name="Poste"):
    p1, p2 = Vector(p1), Vector(p2); v = p2 - p1
    o = _obj(name, bpy.ops.mesh.primitive_cube_add, m, size=1, location=(p1 + p2) / 2)
    o.scale = (w, w, v.length); o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = v.to_track_quat('Z', 'Y'); return o

def lamp_post(label, base, height, aim, energy=900, spot=75):
    bx, by, bz = base; hx, hy, hz = bx, by, bz + height
    box(f"Poste base {label}", (bx, by, bz + .1), (.5, .5, .2), M_DARK)
    beam((bx, by, bz), (hx, hy, hz), .12, M_STEEL, f"Poste {label}")
    d = Vector((aim[0] - bx, aim[1] - by, 0)); d = d.normalized() if d.length else Vector((0, 1, 0))
    head = (hx + d.x * .45, hy + d.y * .45, hz)
    beam((hx, hy, hz), head, .08, M_STEEL, f"Poste braco {label}")
    h = box(f"Poste farol {label}", head, (.5, .5, .16), M_LAMP)
    h.rotation_euler = (0, 0, math.atan2(d.y, d.x))
    mk_light(f"Farol {label}", 'SPOT', (head[0], head[1], head[2] - .1), energy, (1, .93, .8), 'ext', True, .15, spot, (aim[0], aim[1], GZ))

# ================================================================ LUZES
# --- Sol / Terra
sun = mk_light("Sol", 'SUN', (0, 0, 50), 4.2, (1, .96, .88), 'sol'); sun.data.angle = rad(.6); sun.rotation_euler = (rad(68), 0, rad(38))
terra = mk_light("Luz da Terra", 'SUN', (0, 0, 50), .35, (.55, .7, 1.0), 'terra', False); terra.data.angle = rad(2); terra.rotation_euler = (rad(62), 0, rad(-130))

# --- Hub
for k in range(6):
    a = rad(k * 60 + 30)
    mk_light(f"Hub luz {k}", 'POINT', (math.cos(a) * 4.6, math.sin(a) * 4.6, 3.1), 110, (1, .97, .92), 'int', True, .3)
mk_light("Hub console", 'POINT', (0, 0, 1.9), 90, (.5, 1, 1), 'int', True, .3)
mk_light("Hub cupula", 'POINT', (0, 0, 5.2), 120, (1, .97, .9), 'int', True, .4)
for k in range(6):
    a = rad(k * 60)
    mk_light(f"Hub emergencia {k}", 'POINT', (math.cos(a) * 5.8, math.sin(a) * 5.8, 3.0), 110, (1, .1, .05), 'emerg', True, .25)

# --- Tuneis
PORTS = [(90, "Estufa"), (30, "Laboratorio"), (-30, "Energia"), (-90, "Eclusa"), (-150, "Dormitorio"), (150, "Agua")]
for ang, nm in PORTS:
    a = rad(ang)
    for r in (8.8, 11.2, 13.6):
        mk_light(f"Tunel {nm} {r}", 'POINT', (math.cos(a) * r, math.sin(a) * r, 3.0), 55, (.8, .95, 1), 'int', True, .2)
    mk_light(f"Tunel emergencia {nm}", 'POINT', (math.cos(a) * 11.2, math.sin(a) * 11.2, 2.9), 40, (1, .1, .05), 'emerg', True, .2)

# --- Modulos (cor do acento)
ACC = {"Laboratorio": (30, (1, .75, .45)), "Dormitorio": (-150, (.8, .6, 1)), "Agua": (150, (.6, .95, 1)),
       "Energia": (-30, (1, .9, .5)), "Eclusa": (-90, (1, 1, 1))}
for nm, (ang, color) in ACC.items():
    Mx = Mmod(ang)
    for k, y in enumerate((3.0, 6.5, 10.0)):
        p = Mx @ Vector((0, y, 3.1))
        mk_light(f"{nm} luz {k}", 'POINT', (p.x, p.y, 3.1), 150, color, 'int', True, .3)
    p = Mx @ Vector((0, 6.5, 3.0))
    mk_light(f"{nm} emergencia", 'POINT', (p.x, p.y, 3.0), 90, (1, .1, .05), 'emerg', True, .25)
    p = Mx @ Vector((0, 0.9, 2.6))
    # luz sobre a porta de entrada (indicador)
    mk_light(f"{nm} porta", 'POINT', (p.x, p.y, 2.6), 25, color, 'int', True, .1)

# --- Estufa (verde)
GH = 16.1
for k, y in enumerate((3.5, 8.5, 13.5, 18.5)):
    for s in (-1, 1):
        mk_light(f"Estufa luz {k}{'e' if s < 0 else 'd'}", 'POINT', (s * 2.4, GH + y, 2.6), 160, (.55, 1, .5), 'int', True, .3)
mk_light("Estufa emergencia", 'POINT', (0, GH + 11, 3.0), 120, (1, .1, .05), 'emerg', True, .3)

# --- Exterior: postes de luz
lamp_post("hub E", (2.8, 0, 5.85), 2.6, (14, 0, GZ)); lamp_post("hub W", (-2.8, 0, 5.85), 2.6, (-14, 0, GZ))
Me = Mmod(-90)
for sx in (-1, 1):
    p = Me @ Vector((sx * 1.9, 12.7, 0)); q = Me @ Vector((sx * 1.9, 16.8, 0)); aim = Me @ Vector((0, 20, 0))
    lamp_post(f"eclusa a{sx}", (p.x, p.y, 0), 3.2, (aim.x, aim.y, GZ), 700)
    lamp_post(f"eclusa b{sx}", (q.x, q.y, 0), 3.2, (aim.x, aim.y, GZ), 700)
lamp_post("pouso 1", (23.5, -36, GZ), 4.5, (30, -30), 1100); lamp_post("pouso 2", (36.5, -24, GZ), 4.5, (30, -30), 1100)
lamp_post("solar 1", (-42, -18, GZ), 4.0, (-50, -4), 900); lamp_post("solar 2", (-42, 18, GZ), 4.0, (-50, 4), 900)
lamp_post("containers", (40.5, 21, GZ), 3.5, (37.5, 25), 600)
mk_light("Eclusa saida", 'POINT', ((Me @ Vector((0, 12.9, 2.9))).x, (Me @ Vector((0, 12.9, 2.9))).y, 2.9), 110, (1, .85, .6), 'ext', True, .2)

# --- Rover: farois
rv = Mtr(8, -38, GZ, rad(30))
for sx in (-.6, .6):
    p = rv @ Vector((sx, 2.1, .95)); t = rv @ Vector((sx, 12, .2))
    mk_light(f"Rover farol {sx}", 'SPOT', (p.x, p.y, p.z), 500, (1, .96, .85), 'ext', True, .08, 45, (t.x, t.y, t.z))
for sx in (-1.0, 1.0):
    p = rv @ Vector((sx, -2.1, 1.0)); t = rv @ Vector((sx, -9, .2))
    mk_light(f"Rover lanterna {sx}", 'SPOT', (p.x, p.y, p.z), 60, (1, .1, .05), 'emerg', True, .06, 60, (t.x, t.y, t.z))

# luzes de aviso nas posicoes das lampadas de emergencia (geometria)
# ================================================================ presets
PRESETS = {
    "dia":     dict(grupos=dict(sol=4.2, terra=.08, int=.8, ext=0.0, emerg=0.0), sol_cor=(1, .96, .88), sol_rot=(68, 0, 38), sol_ang=.6,
                    mundo=((0, 0, .003), 1.0), lampadas=((1, .92, .78), 3.0), lampada_cor=None),
    "erupcao": dict(grupos=dict(sol=5.5, terra=0.0, int=.18, ext=.6, emerg=1.0), sol_cor=(1, .42, .15), sol_rot=(84, 0, 38), sol_ang=3.0,
                    mundo=((.30, .03, .14), 1.0), lampadas=((1, .15, .08), 4.0), lampada_cor=(1, .15, .08)),
    "noite":   dict(grupos=dict(sol=0.0, terra=1.0, int=1.0, ext=1.0, emerg=0.0), sol_cor=(1, .96, .88), sol_rot=(68, 0, 38), sol_ang=.6,
                    mundo=((0, .008, .03), 1.0), lampadas=((1, .92, .78), 10.0), lampada_cor=None),
}
EMIT_MATS = ["Lampada", "Lampada laranja", "Lampada roxa", "Lampada ciano", "Lampada amarela", "Luz verde", "Cupula luz"]
BASE_EMIT = {}
for n in EMIT_MATS:
    m = bpy.data.materials.get(n)
    if m:
        b = m.node_tree.nodes["Principled BSDF"]
        BASE_EMIT[n] = (tuple(b.inputs["Emission Color"].default_value[:3]), b.inputs["Emission Strength"].default_value)

def set_world(color, strength):
    if scene.world is None:
        scene.world = bpy.data.worlds.new("Espaco")
    w = scene.world; w.use_nodes = True
    bg = w.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (*color, 1); bg.inputs["Strength"].default_value = strength

def apply_preset(name):
    P = PRESETS[name]
    for o in LIGHTS:
        mult = P["grupos"].get(o["grupo"], 1.0)
        if o["grupo"] == 'sol':
            o.data.energy = P["grupos"]["sol"]; o.data.color = P["sol_cor"]; o.rotation_euler = tuple(rad(v) for v in P["sol_rot"]); o.data.angle = rad(P["sol_ang"])
        elif o["grupo"] == 'terra':
            o.data.energy = P["grupos"]["terra"]
        else:
            o.data.energy = o["e0"] * mult
    set_world(*P["mundo"])
    for n, (c, s) in BASE_EMIT.items():
        b = bpy.data.materials[n].node_tree.nodes["Principled BSDF"]
        em = P["lampada_cor"] if (P["lampada_cor"] and n.startswith("Lampada")) else c
        sc = P["lampadas"][1] if n.startswith("Lampada") else (0.25 if n == "Cupula luz" else s * (1.5 if name == "noite" else 1.0))
        b.inputs["Emission Color"].default_value = (*em, 1); b.inputs["Emission Strength"].default_value = sc

# estado exportado: todas as luzes no maximo, material padrao
for o in LIGHTS:
    if o["grupo"] not in ("sol", "terra"): o.data.energy = o["e0"]
export_state = {}
json.dump({"descricao": "energia_final = e0 * multiplicador do grupo (sol/terra: valor absoluto). Cor/intensidade do sol e emissivos por cenario abaixo.",
           "cenarios": {k: {"multiplicadores": v["grupos"], "sol_cor": v["sol_cor"], "sol_rotacao_graus": v["sol_rot"], "sol_angulo_graus": v["sol_ang"],
                           "cor_do_mundo": v["mundo"][0], "emissivo_lampadas": {"cor": v["lampada_cor"] or "padrao", "intensidade": v["lampadas"][1]}}
                        for k, v in PRESETS.items()}},
          open(os.path.join(OUT, "iluminacao_presets.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)

apply_preset("dia")
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.data.objects:
    if o.type in ('MESH', 'LIGHT'): o.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "base_iluminada.blend"))
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, "base_iluminada.glb"), export_format='GLB', use_selection=True,
                          export_lights=True, export_cameras=False, export_animations=True, export_animation_mode='NLA_TRACKS', export_extras=True)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "base_iluminada.fbx"), use_selection=True, object_types={'MESH', 'LIGHT'},
                         apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', bake_anim=True, bake_anim_use_nla_strips=True,
                         bake_anim_use_all_actions=False, bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE')
print("OK EXPORT")

# ======== so previas daqui para baixo ========
scene.render.engine = 'BLENDER_EEVEE'
scene.eevee.taa_render_samples = 24
try: scene.eevee.shadow_pool_size = '2048'
except Exception: pass
scene.view_settings.view_transform = 'Standard'; scene.view_settings.exposure = 0.0

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
eo = bpy.context.object; eo.data.materials.append(earth_m); bpy.ops.object.shade_smooth(); eo.visible_shadow = False

def shot(preset, name, cam, target, lens, res=(1200, 760)):
    key = f"{preset}_{name}"
    if only and key not in only: return
    apply_preset(preset)
    cd = bpy.data.cameras.new("c"); cd.lens = lens; cd.clip_end = 1000
    co = bpy.data.objects.new("c", cd); bpy.context.collection.objects.link(co)
    co.location = cam; co.rotation_mode = 'QUATERNION'; co.rotation_quaternion = (Vector(target) - Vector(cam)).to_track_quat('-Z', 'Y')
    scene.camera = co
    scene.render.resolution_x, scene.render.resolution_y = res
    scene.render.filepath = os.path.join(OUT, f"iluminacao_{key}.png")
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(co)

Mlab = Mmod(30); Mag = Mmod(150)
V = {
    "aerea": ((-52, -62, 38), (0, 6, 2), 24, (1400, 800)),
    "hub": ((0, -5.6, 1.7), (0, 3, 1.7), 16, (1200, 760)),
    "laboratorio": ((Mlab @ Vector((0, 1.1, 1.65))).to_tuple(), (Mlab @ Vector((0, 12, 1.5))).to_tuple(), 16, (1200, 760)),
    "eclusa_externa": ((14, -44, 5), (0, -30, 1.8), 22, (1400, 800)),
    "estufa": ((0, GH + .8, 1.65), (0, GH + 22, 2.2), 20, (1200, 760)),
}
for preset in ("dia", "erupcao", "noite"):
    for nm, (c, t, l, r) in V.items():
        shot(preset, nm, c, t, l, r)
print("OK PREVIAS")
