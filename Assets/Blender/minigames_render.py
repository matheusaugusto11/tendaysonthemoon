"""Renderiza os 16 fundos 3D dos minigames (no local do cenario onde cada um acontece) + ancoras em pixels.
Run: blender -b Assets/Blender/base_iluminada.blend -P Assets/Blender/minigames_render.py -- <pasta_saida> [--only=a,b]
Depois: python Assets/Blender/minigames_overlay.py <pasta_saida>   (desenha o HUD e a mecanica de cada minigame)
"""
import bpy, os, sys, math, json
from mathutils import Vector, Matrix
from bpy_extras.object_utils import world_to_camera_view

OUT = sys.argv[sys.argv.index("--") + 1]
only = [a for a in sys.argv if a.startswith("--only=")]; only = only[0].split("=")[1].split(",") if only else None
os.makedirs(OUT, exist_ok=True)
scene = bpy.context.scene
W, H = 1600, 900
GZ = -0.2; GH = 16.1; TUN_R1 = 15.85
def rad(a): return math.radians(a)
def Mmod(ang): return Matrix.Rotation(rad(ang) - math.pi / 2, 4, 'Z') @ Matrix.Translation((0, TUN_R1, 0))

# ---------------------------------------------------------------- presets de luz (dia / erupcao)
LIGHTS = [o for o in bpy.data.objects if o.type == 'LIGHT']
PRE = {"dia": dict(g=dict(int=.8, ext=0, emerg=0), sol=(4.2, (1, .96, .88), (68, 0, 38)), sky=(0, 0, .003)),
       "erupcao": dict(g=dict(int=.18, ext=.6, emerg=1.0), sol=(5.5, (1, .42, .15), (84, 0, 38)), sky=(.30, .03, .14))}
def preset(name):
    P = PRE[name]
    for o in LIGHTS:
        g = o.get("grupo")
        if g == 'sol':
            o.data.energy, o.data.color = P["sol"][0], P["sol"][1]; o.rotation_euler = tuple(rad(v) for v in P["sol"][2])
        elif g == 'terra': o.data.energy = .08
        else: o.data.energy = o["e0"] * P["g"].get(g, 1)
    if scene.world is None: scene.world = bpy.data.worlds.new("Espaco")
    scene.world.use_nodes = True
    bg = scene.world.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (*P["sky"], 1); bg.inputs["Strength"].default_value = 1
    for n in ("Lampada", "Lampada laranja", "Lampada roxa", "Lampada ciano", "Lampada amarela"):
        m = bpy.data.materials.get(n)
        if m:
            b = m.node_tree.nodes["Principled BSDF"]
            if name == "erupcao": b.inputs["Emission Color"].default_value = (1, .15, .08, 1); b.inputs["Emission Strength"].default_value = 4
            else: b.inputs["Emission Strength"].default_value = 3
preset("dia")

# ---------------------------------------------------------------- render
scene.render.engine = 'BLENDER_EEVEE'; scene.eevee.taa_render_samples = 24
try: scene.eevee.shadow_pool_size = '2048'
except Exception: pass
scene.view_settings.view_transform = 'Standard'; scene.view_settings.exposure = 0.0
scene.render.resolution_x, scene.render.resolution_y = W, H

# Terra (so previa)
em_m = bpy.data.materials.new("Terra planeta"); em_m.use_nodes = True
nt = em_m.node_tree
for n_ in list(nt.nodes): nt.nodes.remove(n_)
o_ = nt.nodes.new("ShaderNodeOutputMaterial"); e_ = nt.nodes.new("ShaderNodeEmission"); e_.inputs["Strength"].default_value = 1.6
nz = nt.nodes.new("ShaderNodeTexNoise"); nz.inputs["Scale"].default_value = 3.5; nz.inputs["Detail"].default_value = 6
rp = nt.nodes.new("ShaderNodeValToRGB"); rp.color_ramp.elements[0].position = .6; rp.color_ramp.elements[0].color = (.03, .15, .55, 1)
rp.color_ramp.elements[1].position = .7; rp.color_ramp.elements[1].color = (.9, .95, 1, 1)
tc = nt.nodes.new("ShaderNodeTexCoord")
nt.links.new(tc.outputs["Object"], nz.inputs["Vector"]); nt.links.new(nz.outputs["Fac"], rp.inputs["Fac"])
nt.links.new(rp.outputs["Color"], e_.inputs["Color"]); nt.links.new(e_.outputs["Emission"], o_.inputs["Surface"])
bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=9, location=(-95, 150, 70))
earth = bpy.context.object; earth.data.materials.append(em_m); bpy.ops.object.shade_smooth(); earth.visible_shadow = False

# ---------------------------------------------------------------- props de cena (so para os minigames)
def mkmat(name, color, emit=None, es=0, rough=.6, metal=0):
    m = bpy.data.materials.new(name); m.use_nodes = True; b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1); b.inputs["Roughness"].default_value = rough; b.inputs["Metallic"].default_value = metal
    if emit: b.inputs["Emission Color"].default_value = (*emit, 1); b.inputs["Emission Strength"].default_value = es
    return m
PM = dict(dark=mkmat("p_dark", (.1, .11, .13)), steel=mkmat("p_steel", (.6, .62, .65), metal=.4), white=mkmat("p_white", (.8, .8, .82)),
          belt=mkmat("p_belt", (.05, .05, .06)), screen=mkmat("p_screen", (.3, 1, .4), (.3, 1, .4), 2), orange=mkmat("p_or", (.95, .52, .1)),
          crater=mkmat("p_crater", (.12, .12, .14), rough=1), sample=mkmat("p_sample", (.3, 1, 1), (.3, 1, 1), 6), crystal=mkmat("p_cr", (1, .75, .25), (1, .75, .25), 4),
          rock_c=[mkmat(f"p_rock{i}", c, e, .9) for i, (c, e) in enumerate([((.4, .9, 1), (.4, .9, 1)), ((.6, 1, .4), (.6, 1, .4)), ((1, .6, .95), (1, .6, .95))])])
def _fin(o, m, Mx=None):
    o.data.materials.clear(); o.data.materials.append(m)
    for p in o.data.polygons: p.use_smooth = False
    if Mx is not None:
        bpy.context.view_layer.update(); o.matrix_world = Mx @ o.matrix_world
    return o
def box(c, s, m, Mx=None, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=c, rotation=rot); o = bpy.context.object; o.scale = s; return _fin(o, m, Mx)
def cyl(c, r, d, m, Mx=None, axis='Z', verts=10, r2=None):
    rot = {'Z': (0, 0, 0), 'X': (0, math.pi / 2, 0), 'Y': (math.pi / 2, 0, 0)}[axis]
    if r2 is None: bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d, location=c, rotation=rot)
    else: bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r, radius2=r2, depth=d, location=c, rotation=rot)
    return _fin(bpy.context.object, m, Mx)
def ico(c, s, m, Mx=None, sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, location=c); o = bpy.context.object; o.scale = s; return _fin(o, m, Mx)
def beam(p1, p2, w, m):
    p1, p2 = Vector(p1), Vector(p2); v = p2 - p1
    bpy.ops.mesh.primitive_cube_add(size=1, location=(p1 + p2) / 2); o = bpy.context.object; o.scale = (w, w, v.length)
    o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = v.to_track_quat('Z', 'Y'); return _fin(o, m)

# pedras brilhantes para o minigame de coleta
ROCKS = [(7, -49.5, 0), (15.5, -50.5, 1), (9.5, -57, 2)]
for x, y, k in ROCKS:
    ico((x, y, GZ + .45), (1.2, 1.05, .8), PM["rock_c"][k], sub=1)
# esteira no dormitorio (ao lado do pufe)
Md = Mmod(-150); TX, TY = 1.3, 6.0
box((TX, TY, .12), (1.0, 2.0, .24), PM["dark"], Md); box((TX, TY, .26), (.85, 1.8, .04), PM["belt"], Md)
for sx in (-1, 1): beam(Md @ Vector((TX + sx * .5, TY - .8, .25)), Md @ Vector((TX + sx * .5, TY - .8, 1.3)), .05, PM["steel"])
box((TX, TY - .85, 1.3), (1.1, .08, .06), PM["steel"], Md); box((TX, TY - .9, 1.55), (.5, .1, .3), PM["dark"], Md); box((TX, TY - .95, 1.55), (.42, .03, .22), PM["screen"], Md)
# braco robotico no patio de containers
def arm():
    b = Vector((33.0, 29.0, GZ))
    cyl(tuple(b + Vector((0, 0, .15))), 1.0, .3, PM["dark"], verts=10); cyl(tuple(b + Vector((0, 0, 1.0))), .5, 1.6, PM["white"], verts=10)
    j1 = b + Vector((0, 0, 1.9)); ico(tuple(j1), (.45, .45, .45), PM["steel"])
    j2 = j1 + Vector((1.3, -1.5, 2.1)); beam(j1, j2, .38, PM["white"]); ico(tuple(j2), (.35, .35, .35), PM["steel"])
    j3 = j2 + Vector((1.4, -1.4, -1.3)); beam(j2, j3, .30, PM["white"]); ico(tuple(j3), (.28, .28, .28), PM["steel"])
    for s in (-1, 1): beam(j3, j3 + Vector((s * .35, .0, -.55)) + Vector((.0, .0, 0)), .08, PM["orange"])
    box((j3.x + .0, j3.y + .0, GZ + .4), (1.1, 1.1, .8), PM["white"]); box((j3.x, j3.y, GZ + .4), (1.12, 1.12, .12), PM["orange"])
    return j3
CLAW = arm()
# pista do rover (terreno livre a leste da base) com rover de cena
RX, RY = 46.0, -14.0
Mr = Matrix.Translation((RX, RY, GZ)) @ Matrix.Rotation(rad(-90), 4, 'Z')   # +Y local -> +X
box((0, 0, .85), (2.0, 4.0, .6), PM["white"], Mr); box((0, 1.0, 1.4), (1.8, 1.8, .7), PM["white"], Mr)
box((0, 1.92, 1.45), (1.5, .05, .45), PM["screen"], Mr); box((0, -1.4, 1.35), (1.6, 1.2, .5), PM["steel"], Mr); box((0, 0, .95), (2.02, 4.02, .08), PM["orange"], Mr)
for yy in (-1.4, 0, 1.4):
    for sx in (-1, 1): cyl((sx * 1.2, yy, .45), .45, .4, PM["dark"], Mr, axis='X')
for cx, cy, r in ((55, -14.4, 1.7), (59.5, -11.6, 1.4), (62, -16.5, 1.6), (65, -13, 1.3)):
    cyl((cx, cy, GZ + .02), r, .04, PM["crater"], verts=14)
for sx, sy in ((52, -11.5), (57.5, -14), (61, -12.5), (64, -17)):
    ico((sx, sy, GZ + .45), (.4, .4, .4), PM["sample"])

# ---------------------------------------------------------------- tomadas
Ml = Mmod(30); Ma = Mmod(150); Me = Mmod(-30); Mz = Mmod(-90)
def L(Mx, v): return tuple(Mx @ Vector(v))
panel_c = Vector((-54.0, 0, GZ + 1.15))
def panel_corners():
    out = {}
    for nm, (sx, sy) in dict(tl=(-1, 1), tr=(1, 1), bl=(-1, -1), br=(1, -1)).items():
        v = Vector((sx * .87, sy * 1.5, .03)); v = Matrix.Rotation(rad(-22), 3, 'Y') @ v
        out["panel_" + nm] = tuple(panel_c + v)
    out["panel_c"] = tuple(panel_c); return out
def ring(c, r, n, z=0):   # pontos num arco
    return [(c[0] + math.cos(rad(a)) * r, c[1] + math.sin(rad(a)) * r, z) for a in n]
hub_wall = {f"furo{i}": (math.cos(rad(a)) * 6.3, math.sin(rad(a)) * 6.3, z) for i, (a, z) in enumerate([(118, 1.3), (62, 2.0), (135, 2.1), (47, 1.2)])}
SHOTS = {
 "amostra":   dict(pre="dia", cam=(3, -42.5, 1.8), tgt=(11, -56, .5), lens=24, anch={**{f"pedra{i}": (x, y, GZ + 1.3) for i, (x, y, k) in enumerate(ROCKS)}, "porta": L(Mz, (0, 12.5, 1.6))}),
 "painel":    dict(pre="dia", cam=(-62.5, -10.5, 3.2), tgt=(-54, 0, 1.3), lens=26, anch=panel_corners()),
 "vazamento": dict(pre="dia", cam=(0, -5.0, 1.7), tgt=(0, 3, 1.9), lens=16, anch=hub_wall),
 "antena":    dict(pre="dia", cam=(45, -9, 2.4), tgt=(36, -2, 3.4), lens=24, anch={"prato": (36, -1.3, GZ + 2.9), "terra": (-95, 150, 70)}),
 "estufa":    dict(pre="dia", cam=(0, GH + 1.0, 1.65), tgt=(0, GH + 22, 2.0), lens=20, anch={"p0": (-2.45, GH + 5.3, 1.25), "p1": (2.45, GH + 8.5, 1.25), "p2": (-2.45, GH + 11.7, 1.25)}),
 "falha":     dict(pre="dia", cam=(-58.3, -1.3, 1.9), tgt=(-54, 0, 1.15), lens=32, anch=panel_corners()),
 "agua":      dict(pre="dia", cam=L(Ma, (1.6, 1.0, 1.55)), tgt=L(Ma, (-1.8, 8, 1.6)), lens=18, anch={"cano": L(Ma, (-2.7, 5.6, 3.1)), "chao": L(Ma, (-2.0, 5.0, 0.3))}),
 "poeira":    dict(pre="dia", cam=(-58.2, 1.6, 2.7), tgt=(-54, 0, 1.15), lens=32, anch=panel_corners()),
 "micro":     dict(pre="dia", cam=L(Ml, (-0.4, 3.0, 1.55)), tgt=L(Ml, (-3.0, 5.0, 1.2)), lens=22, anch={"micro": L(Ml, (-2.9, 4.2, 1.25))}),
 "upload":    dict(pre="dia", cam=(-30, 14.5, 2.4), tgt=(-36, 22, 3.2), lens=28, anch={"prato": (-36, 22.5, GZ + 2.6), "terra": (-95, 150, 70)}),
 "filtros":   dict(pre="dia", cam=L(Me, (0.6, 1.2, 1.5)), tgt=L(Me, (-3.0, 6.0, 1.7)), lens=22, anch={f"fan{i}": L(Me, (-2.25, y, 2.3)) for i, y in enumerate((2.3, 4.7, 7.1))}),
 "exercicio": dict(pre="dia", cam=L(Md, (-0.8, 1.2, 1.7)), tgt=L(Md, (1.0, 6.0, 1.0)), lens=20, anch={"esteira": L(Md, (TX, TY, 0.5))}),
 "braco":     dict(pre="dia", cam=(27, 15, 4.0), tgt=(36, 27, 1.8), lens=26, anch={"garra": tuple(CLAW), "caixa": (CLAW.x, CLAW.y, GZ + .8)}),
 "rover":     dict(pre="dia", cam=(38.0, -14, 4.0), tgt=(58, -14, -.3), lens=26, anch={"rover": (RX, RY, GZ + .8), **{f"c{i}": (x, y, GZ) for i, (x, y) in enumerate(((55, -14.4), (59.5, -11.6), (62, -16.5), (65, -13)))}, **{f"s{i}": (x, y, GZ + .5) for i, (x, y) in enumerate(((52, -11.5), (57.5, -14), (61, -12.5), (64, -17)))}}),
 "co2":       dict(pre="dia", cam=L(Ma, (0.0, 4.2, 1.5)), tgt=L(Ma, (3.7, 8.6, 1.3)), lens=22, anch={"painel": L(Ma, (3.45, 8.6, 1.45))}),
 "treino":    dict(pre="erupcao", cam=(0, -5.0, 1.7), tgt=(0, 3, 1.9), lens=16, anch={}),
}
def shot(name, d):
    if only and name not in only: return
    preset(d["pre"])
    cd = bpy.data.cameras.new("c"); cd.lens = d["lens"]; cd.clip_end = 1000
    co = bpy.data.objects.new("c", cd); bpy.context.collection.objects.link(co)
    co.location = d["cam"]; co.rotation_mode = 'QUATERNION'; co.rotation_quaternion = (Vector(d["tgt"]) - Vector(d["cam"])).to_track_quat('-Z', 'Y')
    scene.camera = co; bpy.context.view_layer.update()
    scene.render.filepath = os.path.join(OUT, f"bg_{name}.png"); bpy.ops.render.render(write_still=True)
    pts = {}
    for k, v in d["anch"].items():
        p = world_to_camera_view(scene, co, Vector(v)); pts[k] = [round(p.x * W, 1), round((1 - p.y) * H, 1), round(p.z, 2)]
    json.dump(pts, open(os.path.join(OUT, f"anch_{name}.json"), "w"))
    bpy.data.objects.remove(co)
for n, d in SHOTS.items(): shot(n, d)
print("OK BG")
