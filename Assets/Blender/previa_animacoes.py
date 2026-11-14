"""Renderiza as animacoes dos personagens (a partir de personagens.blend) em frames PNG na pasta dada e monta GIFs.
Run:  blender -b personagens.blend -P previa_animacoes.py -- <pasta_de_frames>
"""
import bpy, os, sys, math
from mathutils import Vector
FR = sys.argv[sys.argv.index("--") + 1]
HERE = os.path.dirname(os.path.abspath(bpy.data.filepath))
scene = bpy.context.scene
chars = [o for o in bpy.data.objects if o.type == 'ARMATURE']
for ao in chars: ao.animation_data_clear()

scene.render.engine = 'BLENDER_EEVEE'; scene.eevee.taa_render_samples = 16
scene.view_settings.view_transform = 'Standard'; scene.view_settings.exposure = -0.15
scene.render.resolution_x, scene.render.resolution_y = 1000, 440
world = bpy.data.worlds.new("Estudio"); scene.world = world; world.use_nodes = True
bg = world.node_tree.nodes["Background"]; bg.inputs["Color"].default_value = (.72, .74, .78, 1); bg.inputs["Strength"].default_value = .35
bpy.ops.mesh.primitive_plane_add(size=60, location=(0, 0, -.001))
m = bpy.data.materials.new("chao"); m.use_nodes = True; m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (.62, .64, .68, 1)
bpy.context.object.data.materials.append(m)
def light(loc, energy):
    ld = bpy.data.lights.new("l", 'AREA'); ld.energy = energy; ld.size = 4
    o = bpy.data.objects.new("l", ld); bpy.context.collection.objects.link(o); o.location = loc
    o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = (Vector((0, 0, 1)) - Vector(loc)).to_track_quat('-Z', 'Y')
light((-3, -6, 5), 330); light((6, -5, 2.5), 110); light((0, 6, 4), 220)
cd = bpy.data.cameras.new("c"); cd.lens = 50; cam = bpy.data.objects.new("c", cd); bpy.context.collection.objects.link(cam)
scene.camera = cam
def aim(pos, tgt):
    cam.location = pos; cam.rotation_mode = 'QUATERNION'; cam.rotation_quaternion = (Vector(tgt) - Vector(pos)).to_track_quat('-Z', 'Y')

def play(anim_astro, anim_robot, nframes, tag, view):
    for ao in chars:
        name = anim_robot if ao.name.startswith("Robo") else anim_astro
        ao.animation_data_create(); ao.animation_data.action = bpy.data.actions[f"{name}"] if f"{name}" in bpy.data.actions else None
    aim(*view)
    d = os.path.join(FR, tag); os.makedirs(d, exist_ok=True)
    for f in range(1, nframes + 1):
        scene.frame_set(f)
        scene.render.filepath = os.path.join(d, f"{f:03d}.png")
        bpy.ops.render.render(write_still=True)

# acoes: cada personagem tem acoes com o mesmo nome (Idle.xxx); procurar pelo dono
acts = {}
for a in bpy.data.actions: acts.setdefault(a.name.split(".")[0], []).append(a)
def assign(ao, base):
    cands = sorted(acts[base], key=lambda a: a.name)
    # a acao do personagem: guardada nas tiras NLA removidas -> usar a ordem de criacao (uma por personagem)
    return cands
order = [o for o in chars]   # ordem de criacao = ordem das acoes
idx = {o.name: i for i, o in enumerate(sorted(chars, key=lambda o: o.location.x))}
def play2(a_astro, a_robot, n, tag, view):
    for ao in chars:
        base = a_robot if ao.name.startswith("Robo") else a_astro
        lst = sorted(acts[base], key=lambda a: a.name)
        # as acoes foram criadas na ordem: astronautas (4) e robo; o robo so tem 1 de cada
        i = idx[ao.name] if not ao.name.startswith("Robo") else (len(lst) - 1)
        ao.animation_data_create(); ao.animation_data.action = lst[i]
    aim(*view)
    d = os.path.join(FR, tag); os.makedirs(d, exist_ok=True)
    for f in range(1, n + 1):
        scene.frame_set(f); scene.render.filepath = os.path.join(d, f"{f:03d}.png"); bpy.ops.render.render(write_still=True)

V34 = ((9, -9, 2.4), (.8, 0, .9)); VF = ((.8, -12.5, 1.15), (.8, 0, .95))
play2("Idle", "Idle", 48, "idle_34", V34)
play2("Caminhar", "Rodar", 24, "andar_34", V34)
play2("Caminhar", "Rodar", 24, "andar_frente", VF)
# corrida (lineup completo) antes de esconder os outros
for ao in chars:
    ao.hide_render = False
    for c in ao.children: c.hide_render = False
scene.render.resolution_x, scene.render.resolution_y = 1000, 440
play2("Correr", "Acelerar", 16, "correr_34", V34)
play2("Correr", "Acelerar", 16, "correr_frente", VF)
# lado: so a Medicina, para conferir joelhos
for ao in chars:
    keep = ao.name.startswith("Astronauta_Medicina")
    ao.hide_render = not keep
    for c in ao.children: c.hide_render = not keep
scene.render.resolution_x, scene.render.resolution_y = 520, 520
play2("Caminhar", "Rodar", 24, "andar_lado", ((9.0, 0, 1.0), (.8, 0, .9)))
play2("Correr", "Acelerar", 16, "correr_lado", ((9.0, 0, 1.0), (.8, 0, .9)))
print("OK FRAMES")
