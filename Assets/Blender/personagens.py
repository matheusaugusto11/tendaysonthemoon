"""Personagens - "Ten Days on the Moon": 4 astronautas (Engenharia/ciano, Botanica/verde, Medicina/roxo, Ciencia/laranja) + robo assistente.
Low-poly, esqueleto proprio, malha unica skinnada por personagem, animacoes Idle e Caminhar (Robo: Idle e Rodar).
Run:  blender -b -P personagens.py
Saidas: personagens.blend, ../Models/Personagens/<nome>.fbx, personagens_*.png (previas)
Frente do personagem = -Y do Blender (vira +Z no Unity). Altura do astronauta ~1.8 m.
"""
import bpy, math, os, sys
from mathutils import Vector, Matrix

HERE = os.path.dirname(os.path.abspath(__file__))
OUTFBX = os.path.normpath(os.path.join(HERE, "..", "Models", "Personagens"))
os.makedirs(OUTFBX, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 24

# ------------------------------------------------------------ materiais
def mat(name, color, rough=.6, metal=0.0, emit=None, es=0.0):
    m = bpy.data.materials.new(name); m.use_nodes = True
    b = m.node_tree.nodes["Principled BSDF"]
    b.inputs["Base Color"].default_value = (*color, 1); b.inputs["Roughness"].default_value = rough; b.inputs["Metallic"].default_value = metal
    if emit:
        b.inputs["Emission Color"].default_value = (*emit, 1); b.inputs["Emission Strength"].default_value = es
    return m
MT = dict(
    branco=mat("Traje branco", (.80, .80, .82), .75), cinza=mat("Traje cinza", (.20, .23, .28), .7), claro=mat("Traje cinza claro", (.55, .57, .60), .6, .2),
    visor=mat("Visor dourado", (.80, .55, .12), .30, .6), luva=mat("Luva", (.13, .14, .17), .8), couro=mat("Couro", (.42, .27, .13), .85),
    ciano=mat("Acento ciano", (.15, .75, .85), .55), verde=mat("Acento verde", (.35, .78, .18), .55), roxo=mat("Acento roxo", (.62, .38, .92), .55), laranja=mat("Acento laranja", (.95, .52, .10), .55),
    glifo=mat("Glifo branco", (.95, .95, .95), .6), planta=mat("Planta", (.30, .70, .25), .8), vidro=mat("Vidro pote", (.7, .9, .85), .1),
    tela=mat("Tela verde", (.3, 1, .4), .4, emit=(.3, 1, .4), es=2.0), cruz=mat("Cruz vermelha", (.85, .12, .1), .6),
    r_corpo=mat("Robo corpo", (.78, .80, .86), .6), r_faixa=mat("Robo faixa", (.45, .52, .95), .55), r_escuro=mat("Robo escuro", (.10, .11, .14), .6),
    r_esteira=mat("Robo esteira", (.07, .07, .08), .9), r_metal=mat("Robo metal", (.45, .47, .52), .5, .5), r_olho=mat("Robo olho", (.2, .95, 1), .3, emit=(.2, .95, 1), es=6.0),
)

# ------------------------------------------------------------ helpers
PARTS = []     # (obj, osso)
SMOOTH_ANGLE = math.radians(50)   # arestas acima disso continuam vivas; o resto fica arredondado
def _fin(o, name, m, bone):
    o.name = name
    o.data.materials.clear(); o.data.materials.append(m)
    bpy.ops.object.select_all(action='DESELECT'); o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.object.shade_smooth_by_angle(angle=SMOOTH_ANGLE)
    PARTS.append((o, bone)); return o

def bevel(o, w=.012, seg=2):
    mod = o.modifiers.new("b", 'BEVEL'); mod.width = w; mod.segments = seg; mod.limit_method = 'ANGLE'
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="b")

def box(name, c, size, m, bone, bev=0.0, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=c, rotation=rot)
    o = bpy.context.object; o.scale = size
    if bev: bpy.ops.object.transform_apply(scale=True); bevel(o, bev)
    return _fin(o, name, m, bone)

def ico(name, c, scale, m, bone, sub=1, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=max(sub, 2), location=c, rotation=rot)
    o = bpy.context.object; o.scale = scale
    return _fin(o, name, m, bone)

def uvs(name, c, scale, m, bone, seg=12, rings=8):
    seg, rings = max(seg, 20), max(rings, 12)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, location=c)
    o = bpy.context.object; o.scale = scale
    return _fin(o, name, m, bone)

def cyl(name, c, r, d, m, bone, axis='Z', verts=8, scale=(1, 1, 1)):
    verts = max(verts, 12)
    rot = {'Z': (0, 0, 0), 'X': (0, math.pi / 2, 0), 'Y': (math.pi / 2, 0, 0)}[axis]
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d, location=c, rotation=rot)
    o = bpy.context.object; o.scale = scale
    return _fin(o, name, m, bone)

def limb(name, p1, p2, r1, r2, m, bone, verts=6):
    verts = max(verts, 10)
    p1, p2 = Vector(p1), Vector(p2); v = p2 - p1
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=v.length, location=(p1 + p2) / 2)
    o = bpy.context.object; o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = v.to_track_quat('Z', 'Y')
    return _fin(o, name, m, bone)

def torus(name, c, R, r, m, bone, rot=(0, 0, 0), seg=10):
    seg = max(seg, 16)
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg, minor_segments=5, location=c, rotation=rot)
    return _fin(bpy.context.object, name, m, bone)

def finalize(prefix, bones, origin):
    """junta as pecas numa malha skinnada, cria armadura e liga."""
    objs = []
    for o, bone in PARTS:
        bpy.context.view_layer.objects.active = o
        vg = o.vertex_groups.new(name=bone); vg.add(list(range(len(o.data.vertices))), 1.0, 'REPLACE')
        objs.append(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    mesh = bpy.context.object; mesh.name = prefix; mesh.data.name = prefix
    PARTS.clear()
    arm = bpy.data.armatures.new(prefix + "_Armature"); ao = bpy.data.objects.new(prefix + "_Rig", arm)
    bpy.context.collection.objects.link(ao)
    bpy.context.view_layer.objects.active = ao; ao.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    eb = {}
    for name, head, tail, parent in bones:
        b = arm.edit_bones.new(name); b.head = Vector(head); b.tail = Vector(tail); eb[name] = b
        if parent: b.parent = eb[parent]; b.use_connect = False
    bpy.ops.object.mode_set(mode='OBJECT')
    mesh.parent = ao
    mod = mesh.modifiers.new("Armature", 'ARMATURE'); mod.object = ao
    ao.location = origin
    return ao, mesh

# ------------------------------------------------------------ astronauta
ROLES = {
    "Engenharia": dict(acc="ciano", tools=True),
    "Botanica":   dict(acc="verde", plant=True),
    "Medicina":   dict(acc="roxo", medkit=True),
    "Ciencia":    dict(acc="laranja", scanner=True),
}
def build_astronaut(role, ox):
    spec = ROLES[role]; A = MT[spec["acc"]]; W, C, CL = MT["branco"], MT["cinza"], MT["claro"]
    # offsets em X por personagem; tudo construido em (0,0,0) e movido depois pela armadura
    for s in (1, -1):
        sx = s; side = "L" if s > 0 else "R"
        # pernas
        ico(f"Quadril_{side}", (sx * .10, 0, .955), (.112, .112, .112), W, f"UpperLeg_{side}", 2)
        limb(f"Coxa_{side}", (sx * .10, 0, .95), (sx * .10, 0, .53), .105, .088, W, f"UpperLeg_{side}")
        limb(f"Canela_{side}", (sx * .10, 0, .53), (sx * .10, 0, .15), .088, .066, W, f"LowerLeg_{side}")
        box(f"Joelheira_{side}", (sx * .10, -.075, .53), (.15, .10, .15), C, f"LowerLeg_{side}", .04)
        cyl(f"Tornozelo_{side}", (sx * .10, 0, .17), .082, .05, C, f"LowerLeg_{side}", verts=8)
        box(f"Bota_{side}", (sx * .10, -.06, .06), (.15, .30, .12), W, f"Foot_{side}", .045)
        box(f"Sola_{side}", (sx * .10, -.06, .015), (.16, .31, .03), C, f"Foot_{side}", .012)
        # braco (pose em A)
        S = Vector((sx * .25, 0, 1.46)); d1 = Vector((sx * .259, 0, -.966)); E = S + d1 * .30
        d2 = Vector((sx * .14, 0, -.99)).normalized(); Wr = E + d2 * .27
        ico(f"Ombreira_{side}", (sx * .27, 0, 1.49), (.125, .12, .105), A, f"UpperArm_{side}", 1)
        limb(f"Braco_{side}", S, E, .075, .063, W, f"UpperArm_{side}")
        ico(f"Cotovelo_{side}", E, (.074, .074, .074), C, f"LowerArm_{side}", 2)
        limb(f"Antebraco_{side}", E, Wr, .063, .05, W, f"LowerArm_{side}")
        cyl(f"Pulso_{side}", Wr - d2 * .03, .058, .05, C, f"LowerArm_{side}", verts=8)
        box(f"Luva_{side}", Wr + d2 * .08, (.085, .10, .15), MT["luva"], f"Hand_{side}", .035)
    # tronco
    box("Pelve", (0, 0, .98), (.34, .22, .20), W, "Hips", .07)
    box("Cinto", (0, 0, 1.09), (.375, .255, .075), C, "Hips", .025)
    box("Torso", (0, 0, 1.31), (.40, .25, .42), W, "Chest", .09)
    box("Peitoral", (0, -.14, 1.36), (.17, .05, .14), CL, "Chest", .02)
    for k, bx in enumerate((-.04, .04)):
        box("Botao", (bx, -.168, 1.39), (.03, .012, .03), A, "Chest")
    box("Visor info", (0, -.168, 1.33), (.10, .012, .025), MT["luva"], "Chest")
    for sx in (-1, 1):
        box("Alca", (sx * .13, -.128, 1.35), (.045, .01, .44), C, "Chest", rot=(0, math.radians(8) * sx, 0))
    box("Gola", (0, 0, 1.55), (.20, .17, .05), C, "Neck", .022)
    # capacete
    uvs("Capacete", (0, 0, 1.69), (.205, .215, .215), W, "Head", 24, 16)
    uvs("Visor", (0, -.115, 1.695), (.155, .105, .15), MT["visor"], "Head", 24, 16)
    box("Faixa capacete", (0, 0, 1.90), (.055, .26, .03), A, "Head", .01)
    cyl("Colar capacete", (0, 0, 1.585), .165, .04, C, "Head", verts=10)
    # mochila (PLSS)
    box("Mochila", (0, .20, 1.33), (.34, .16, .46), W, "Chest", .07)
    box("Mochila acento", (.19, .20, 1.33), (.045, .17, .40), A, "Chest", .015)
    box("Mochila placa", (0, .285, 1.33), (.20, .015, .30), CL, "Chest", .006)
    box("Mochila valvula", (0, .30, 1.38), (.04, .03, .16), C, "Chest")
    # glifo na ombreira esquerda
    gp = Vector((.27, -.115, 1.49))
    if role == "Engenharia":
        box("Glifo chave", gp, (.016, .01, .085), MT["glifo"], "UpperArm_L", rot=(0, math.radians(40), 0)); torus("Glifo chave anel", gp + Vector((.03, 0, .035)), .018, .006, MT["glifo"], "UpperArm_L", (math.pi / 2, 0, 0), 6)
    elif role == "Botanica":
        ico("Glifo folha", gp, (.04, .008, .028), MT["glifo"], "UpperArm_L", 1, (0, math.radians(35), 0)); box("Glifo haste", gp + Vector((-.02, 0, -.03)), (.008, .008, .05), MT["glifo"], "UpperArm_L", rot=(0, math.radians(35), 0))
    elif role == "Medicina":
        box("Glifo cruz a", gp, (.07, .01, .022), MT["glifo"], "UpperArm_L"); box("Glifo cruz b", gp, (.022, .01, .07), MT["glifo"], "UpperArm_L")
    else:
        for k in range(3):
            box("Glifo atomo", gp, (.085, .008, .012), MT["glifo"], "UpperArm_L", rot=(0, math.radians(60 * k), 0))
        ico("Glifo nucleo", gp, (.014, .014, .014), MT["glifo"], "UpperArm_L", 1)
    # acessorios por funcao
    if spec.get("tools"):
        box("Cinto ferramentas", (0, 0, 1.03), (.37, .245, .045), MT["couro"], "Hips")
        for px, py in ((.17, -.06), (-.17, -.06), (.0, -.14)):
            box("Bolsa", (px, py, 1.0), (.075, .055, .10), MT["couro"], "Hips", .008)
        for k in range(3):
            box("Ferramenta", (-.17 + k * .02 - .02, -.09, 1.07), (.012, .012, .08), C, "Hips")
        torus("Cabo", (.20, -.01, .93), .10, .018, C, "UpperLeg_L", (0, math.pi / 2, 0), 10)
        torus("Cabo", (.205, -.01, .93 - .0), .08, .016, C, "UpperLeg_L", (0, math.pi / 2, 0), 10)
    if spec.get("plant"):
        hp = Vector((-.40, -.10, .93))
        box("Pote moldura", hp, (.15, .10, .17), C, "Hand_R", .01); box("Pote vidro", hp + Vector((0, -.04, 0)), (.12, .02, .14), MT["vidro"], "Hand_R")
        limb("Muda caule", hp + Vector((0, 0, -.04)), hp + Vector((0, 0, .06)), .01, .006, MT["planta"], "Hand_R", 4)
        for k in range(4):
            a = k * math.pi / 2
            ico("Muda folha", hp + Vector((math.cos(a) * .03, math.sin(a) * .03, .07)), (.035, .02, .012), MT["planta"], "Hand_R", 1, (0.5 * math.sin(a), -.5 * math.cos(a), a))
    if spec.get("medkit"):
        box("Maleta", (0, -.14, 1.13), (.20, .07, .14), A, "Chest", .03)
        box("Maleta cruz a", (0, -.178, 1.13), (.09, .01, .026), MT["glifo"], "Chest"); box("Maleta cruz b", (0, -.178, 1.13), (.026, .01, .09), MT["glifo"], "Chest")
        box("Maleta alca", (0, -.14, 1.215), (.07, .03, .02), C, "Chest")
    if spec.get("scanner"):
        sp = Vector((-.40, -.12, .93))
        box("Scanner", sp, (.07, .17, .055), CL, "Hand_R", .01); box("Scanner tela", sp + Vector((0, -.03, .03)), (.045, .09, .008), MT["tela"], "Hand_R")
        for k in range(3): cyl("Scanner botao", sp + Vector((-.02 + k * .02, .06, .03)), .008, .014, A, "Hand_R", verts=6)
        limb("Scanner antena", sp + Vector((0, -.085, 0)), sp + Vector((0, -.15, 0)), .01, .006, C, "Hand_R", 4)
    bones = [("Hips", (0, 0, .98), (0, 0, 1.10), None), ("Spine", (0, 0, 1.10), (0, 0, 1.30), "Hips"), ("Chest", (0, 0, 1.30), (0, 0, 1.50), "Spine"),
             ("Neck", (0, 0, 1.50), (0, 0, 1.60), "Chest"), ("Head", (0, 0, 1.60), (0, 0, 1.88), "Neck")]
    for sx, side in ((1, "L"), (-1, "R")):
        S = Vector((sx * .25, 0, 1.46)); E = S + Vector((sx * .259, 0, -.966)) * .30
        d2 = Vector((sx * .14, 0, -.99)).normalized(); Wr = E + d2 * .27
        bones += [(f"Shoulder_{side}", (sx * .08, 0, 1.46), tuple(S), "Chest"), (f"UpperArm_{side}", tuple(S), tuple(E), f"Shoulder_{side}"),
                  (f"LowerArm_{side}", tuple(E), tuple(Wr), f"UpperArm_{side}"), (f"Hand_{side}", tuple(Wr), tuple(Wr + d2 * .16), f"LowerArm_{side}"),
                  (f"UpperLeg_{side}", (sx * .10, 0, .95), (sx * .10, 0, .53), "Hips"), (f"LowerLeg_{side}", (sx * .10, 0, .53), (sx * .10, 0, .13), f"UpperLeg_{side}"),
                  (f"Foot_{side}", (sx * .10, 0, .13), (sx * .10, -.12, .04), f"LowerLeg_{side}"), (f"Toes_{side}", (sx * .10, -.12, .04), (sx * .10, -.24, .04), f"Foot_{side}")]
    return finalize(f"Astronauta_{role}", bones, (ox, 0, 0))

# ------------------------------------------------------------ robo
def build_robot(ox):
    B, F, D, E, Mt, T, Eye = MT["r_corpo"], MT["r_faixa"], MT["r_escuro"], MT["r_esteira"], MT["r_metal"], MT["r_esteira"], MT["r_olho"]
    for s, side in ((1, "L"), (-1, "R")):
        sx = s
        box(f"Esteira corpo_{side}", (sx * .24, 0, .12), (.15, .36, .20), E, f"Tread_{side}", .06)
        for yy in (-.19, .19):
            cyl(f"Esteira roda_{side}", (sx * .24, yy, .12), .12, .15, E, f"Tread_{side}", axis='X', verts=10)
            cyl(f"Esteira cubo_{side}", (sx * .24 + sx * .085, yy, .12), .065, .03, Mt, f"Tread_{side}", axis='X', verts=6)
        cyl(f"Esteira cubo_{side}", (sx * .24 + sx * .085, 0, .12), .04, .03, Mt, f"Tread_{side}", axis='X', verts=6)
        for k in range(7):
            box(f"Esteira dente_{side}", (sx * .24, -.22 + k * .073, .245 if k % 1 == 0 else .245), (.16, .04, .02), E, f"Tread_{side}")
            box(f"Esteira dente_{side}", (sx * .24, -.22 + k * .073, -.005 + .0), (.16, .04, .02), E, f"Tread_{side}")
        box(f"Eixo_{side}", (sx * .15, 0, .14), (.10, .08, .06), Mt, "Body")
        # braco
        S = Vector((sx * .30, 0, .80)); Ee = Vector((sx * .335, 0, .62)); Wr = Vector((sx * .345, -.02, .44))
        ico(f"Ombro_{side}", S, (.065, .065, .065), B, f"UpperArm_{side}", 1)
        box(f"Braco_{side}", (S + Ee) / 2, (.06, .07, .20), B, f"UpperArm_{side}", .026, rot=(0, math.radians(-8) * sx, 0))
        ico(f"Cotovelo_{side}", Ee, (.05, .05, .05), Mt, f"LowerArm_{side}", 1)
        box(f"Antebraco_{side}", (Ee + Wr) / 2, (.06, .07, .19), B, f"LowerArm_{side}", .026, rot=(0, math.radians(-2) * sx, 0))
        box(f"Pulso_{side}", Wr, (.055, .06, .04), Mt, f"Hand_{side}")
        for k in (-1, 1):
            box(f"Garra_{side}", Wr + Vector((0, k * .028, -.06)), (.02, .02, .08), Mt, f"Hand_{side}", rot=(math.radians(k * -12), 0, 0))
    uvs("Corpo", (0, 0, .60), (.27, .25, .36), B, "Body", 28, 18)
    cyl("Faixa azul", (0, 0, .60), 1.0, .085, F, "Body", verts=28, scale=(.284, .264, 1))
    cyl("Painel linha", (0, 0, .42), 1.0, .012, D, "Body", verts=28, scale=(.248, .228, 1))
    cyl("Painel linha", (0, 0, .78), 1.0, .012, D, "Body", verts=28, scale=(.240, .220, 1))
    box("Painel frente", (0, -.255, .60), (.012, .01, .34), D, "Body")
    cyl("Pescoco", (0, 0, .94), .12, .04, D, "Head", verts=10)
    uvs("Cabeca", (0, 0, 1.04), (.20, .19, .17), B, "Head", 24, 14)
    box("Rosto", (0, -.15, 1.03), (.25, .06, .15), D, "Head", .06)
    for sx in (-1, 1):
        box("Olho", (sx * .06, -.185, 1.04), (.045, .012, .06), Eye, "Head", .008)
    cyl("Antena haste", (0, 0, 1.24), .012, .16, D, "Antenna", verts=5)
    ico("Antena ponta", (0, 0, 1.33), (.022, .022, .022), Eye, "Antenna", 1)
    bones = [("Root", (0, 0, 0), (0, 0, .15), None), ("Body", (0, 0, .30), (0, 0, .90), "Root"), ("Head", (0, 0, .90), (0, 0, 1.22), "Body"), ("Antenna", (0, 0, 1.18), (0, 0, 1.34), "Head")]
    for sx, side in ((1, "L"), (-1, "R")):
        S = (sx * .30, 0, .80); Ee = (sx * .335, 0, .62); Wr = (sx * .345, -.02, .44)
        bones += [(f"UpperArm_{side}", S, Ee, "Body"), (f"LowerArm_{side}", Ee, Wr, f"UpperArm_{side}"), (f"Hand_{side}", Wr, (Wr[0], Wr[1], Wr[2] - .12), f"LowerArm_{side}"),
                  (f"Tread_{side}", (sx * .24, 0, .02), (sx * .24, 0, .24), "Root")]
    return finalize("Robo_Assistente", bones, (ox, 0, 0))

# ------------------------------------------------------------ animacao
def drive(ao, bone, axis, deg):
    pb = ao.pose.bones[bone]; B3 = pb.bone.matrix_local.to_3x3()
    R = Matrix.Rotation(math.radians(deg), 3, axis)
    pb.rotation_mode = 'QUATERNION'; pb.rotation_quaternion = (B3.inverted() @ R @ B3).to_quaternion()

def make_action(ao, name, frames, fn, bones):
    """fn(f, t) -> dict {osso: [(eixo, graus), ...]}  com t em 0..1"""
    ao.animation_data_create()
    act = bpy.data.actions.new(name); ao.animation_data.action = act
    bpy.context.view_layer.objects.active = ao
    for f in range(0, frames + 1):
        t = f / frames
        pose = fn(f, t)
        for b in bones:
            pb = ao.pose.bones[b]; pb.rotation_mode = 'QUATERNION'; pb.rotation_quaternion = (1, 0, 0, 0)
            if b in pose:
                q = Matrix.Identity(3)
                for axis, deg in pose[b]: q = Matrix.Rotation(math.radians(deg), 3, axis) @ q
                B3 = pb.bone.matrix_local.to_3x3(); pb.rotation_quaternion = (B3.inverted() @ q @ B3).to_quaternion()
            pb.keyframe_insert("rotation_quaternion", frame=f + 1)
        if '_y' in pose:
            hb = ao.pose.bones["Hips"]; hb.location = hb.bone.matrix_local.to_3x3().inverted() @ Vector((0, 0, pose['_y']))
            hb.keyframe_insert("location", frame=f + 1)
    tr = ao.animation_data.nla_tracks.new(); tr.name = name
    tr.strips.new(name, 1, act)
    ao.animation_data.action = None
    for pb in ao.pose.bones: pb.rotation_quaternion = (1, 0, 0, 0)
    if 'Hips' in ao.pose.bones: ao.pose.bones['Hips'].location = (0, 0, 0)

S2 = lambda t, k=1, ph=0: math.sin(2 * math.pi * (t * k + ph))
def astro_idle(f, t):
    return {"Chest": [('X', 1.2 * S2(t))], "Head": [('Z', 4 * S2(t, 1, .25))], "Spine": [('X', -.6 * S2(t))],
            "UpperArm_L": [('Y', 1.5 * S2(t, 1, .1))], "UpperArm_R": [('Y', -1.5 * S2(t, 1, .1))]}
def astro_walk(f, t):
    p = S2(t); q = S2(t, 1, .25)
    kl = max(0, -S2(t, 1, .25)) * 40; kr = max(0, S2(t, 1, .25)) * 40
    return {"UpperLeg_L": [('X', 28 * p)], "UpperLeg_R": [('X', -28 * p)], "LowerLeg_L": [('X', kl)], "LowerLeg_R": [('X', kr)],
            "Foot_L": [('X', 10 * p)], "Foot_R": [('X', -10 * p)],
            "UpperArm_L": [('X', -22 * p)], "UpperArm_R": [('X', 22 * p)], "LowerArm_L": [('X', -12 - 8 * p)], "LowerArm_R": [('X', -12 + 8 * p)],
            "Hips": [('Z', 4 * p)], "Chest": [('Z', -6 * p), ('X', 2)], "Head": [('Z', 3 * p)]}
def astro_run(f, t):
    p = S2(t); c = math.cos(2 * math.pi * t)
    kl = 12 + max(0, -c) * 85; kr = 12 + max(0, c) * 85
    return {"UpperLeg_L": [('X', 46 * p)], "UpperLeg_R": [('X', -46 * p)], "LowerLeg_L": [('X', kl)], "LowerLeg_R": [('X', kr)],
            "Foot_L": [('X', 14 * p - 6)], "Foot_R": [('X', -14 * p - 6)],
            "UpperArm_L": [('X', -50 * p)], "UpperArm_R": [('X', 50 * p)], "LowerArm_L": [('X', -85 - 12 * p)], "LowerArm_R": [('X', -85 + 12 * p)],
            "Hips": [('Z', 7 * p)], "Spine": [('X', 6)], "Chest": [('Z', -10 * p), ('X', 4)], "Head": [('Z', 4 * p), ('X', -8)],
            "_y": 0.025 * (1 - math.cos(4 * math.pi * t))}
def robot_fast(f, t):
    return {"Body": [('X', 9 + 1.5 * S2(t, 2))], "Head": [('X', 5), ('Z', 4 * S2(t))], "Antenna": [('X', -28 + 8 * S2(t, 2, .2))],
            "UpperArm_L": [('Y', 10 + 5 * S2(t, 2))], "UpperArm_R": [('Y', -10 - 5 * S2(t, 2))]}
def robot_idle(f, t):
    return {"Head": [('Z', 14 * S2(t))], "Antenna": [('X', 10 * S2(t, 2)), ('Y', 8 * S2(t, 2, .25))], "Body": [('X', .8 * S2(t, 2))],
            "UpperArm_L": [('Y', 3 * S2(t, 1, .1))], "UpperArm_R": [('Y', -3 * S2(t, 1, .1))]}
def robot_roll(f, t):
    return {"Body": [('X', 5 + 1.5 * S2(t, 2))], "Head": [('X', 4), ('Z', 5 * S2(t))], "Antenna": [('X', 18 * S2(t, 2, .2))],
            "UpperArm_L": [('Y', 6 * S2(t, 2))], "UpperArm_R": [('Y', -6 * S2(t, 2))], "Tread_L": [('X', 0)], "Tread_R": [('X', 0)]}

# ------------------------------------------------------------ montar a equipe
CHARS = []
xs = {"Engenharia": -2.4, "Botanica": -0.8, "Medicina": 0.8, "Ciencia": 2.4}
for role, x in xs.items():
    ao, me = build_astronaut(role, x)
    names = [b.name for b in ao.pose.bones]
    make_action(ao, "Idle", 48, astro_idle, names); make_action(ao, "Caminhar", 24, astro_walk, names); make_action(ao, "Correr", 16, astro_run, names)
    CHARS.append((f"Astronauta_{role}", ao, me))
ao, me = build_robot(4.0)
names = [b.name for b in ao.pose.bones]
make_action(ao, "Idle", 48, robot_idle, names); make_action(ao, "Rodar", 24, robot_roll, names); make_action(ao, "Acelerar", 16, robot_fast, names)
CHARS.append(("Robo_Assistente", ao, me))
scene.frame_start, scene.frame_end = 1, 48

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(HERE, "personagens.blend"))

# ------------------------------------------------------------ exportar FBX individuais
for name, ao, me in CHARS:
    ox = ao.location.x
    ao.location.x = 0
    bpy.ops.object.select_all(action='DESELECT'); ao.select_set(True); me.select_set(True)
    bpy.context.view_layer.objects.active = ao
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUTFBX, f"{name}.fbx"), use_selection=True, object_types={'ARMATURE', 'MESH'},
                             apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', add_leaf_bones=False, bake_anim=True,
                             bake_anim_use_nla_strips=True, bake_anim_use_all_actions=False, bake_anim_simplify_factor=0.5,
                             mesh_smooth_type='OFF', primary_bone_axis='Y', secondary_bone_axis='X')
    ao.location.x = ox
print("OK EXPORT")

# ------------------------------------------------------------ previas
for _n, _ao, _me in CHARS:
    _ao.animation_data_clear()      # previas na pose de repouso (A); o .blend e os FBX ja foram salvos
    for _pb in _ao.pose.bones: _pb.rotation_quaternion = (1, 0, 0, 0)
bpy.context.view_layer.update()
only = [a for a in sys.argv if a.startswith("--only=")]
only = only[0].split("=")[1].split(",") if only else None
scene.render.engine = 'BLENDER_EEVEE'; scene.eevee.taa_render_samples = 32
scene.view_settings.view_transform = 'Standard'; scene.view_settings.exposure = -0.15
world = bpy.data.worlds.new("Estudio"); scene.world = world; world.use_nodes = True
world.node_tree.nodes["Background"].inputs["Color"].default_value = (.72, .74, .78, 1); world.node_tree.nodes["Background"].inputs["Strength"].default_value = .35
bpy.ops.mesh.primitive_plane_add(size=60, location=(0, 0, -.001))
fl = bpy.context.object; fl.data.materials.append(mat("Chao estudio", (.62, .64, .68), .9))
def light(name, loc, energy, size, tgt, color=(1, 1, 1)):
    ld = bpy.data.lights.new(name, 'AREA'); ld.energy = energy; ld.size = size; ld.color = color
    o = bpy.data.objects.new(name, ld); bpy.context.collection.objects.link(o); o.location = loc
    o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = (Vector(tgt) - Vector(loc)).to_track_quat('-Z', 'Y')
light("key", (-3, -6, 5), 330, 4, (0, 0, 1)); light("fill", (6, -5, 2.5), 110, 4, (0, 0, 1)); light("rim", (0, 6, 4), 220, 4, (0, 0, 1))
def shot(name, cam, tgt, lens, res):
    if only and name not in only: return
    cd = bpy.data.cameras.new("c"); cd.lens = lens; co = bpy.data.objects.new("c", cd); bpy.context.collection.objects.link(co)
    co.location = cam; co.rotation_mode = 'QUATERNION'; co.rotation_quaternion = (Vector(tgt) - Vector(cam)).to_track_quat('-Z', 'Y')
    scene.camera = co; scene.render.resolution_x, scene.render.resolution_y = res
    scene.render.filepath = os.path.join(HERE, f"personagens_{name}.png"); bpy.ops.render.render(write_still=True); bpy.data.objects.remove(co)
shot("equipe_frente", (.8, -12.5, 1.15), (.8, 0, .95), 50, (1700, 700))
shot("equipe_3-4", (9, -9, 2.4), (.8, 0, .9), 45, (1700, 760))
shot("equipe_costas", (.8, 12.5, 1.15), (.8, 0, .95), 50, (1700, 700))
shot("robo_frente", (4.0, -3.2, 1.0), (4.0, 0, .68), 50, (700, 800))
shot("robo_3-4", (5.5, -2.6, 1.2), (4.0, 0, .68), 50, (700, 800))
shot("robo_lado", (7.0, 0, 1.0), (4.0, 0, .68), 50, (700, 800))
shot("astro_medicina", (2.4, -3.4, 1.4), (.8, 0, .95), 55, (700, 900))
print("OK PREVIAS")
