"""Exporta os arquivos para o Unity a partir de base_iluminada.blend.
Run:  blender -b Assets/Blender/base_iluminada.blend -P Assets/Blender/exportar_unity.py
Saida: Assets/Models/Base/  (base_cenario.fbx, base_portas.fbx, base_luzes.json, base_materiais.json, base_portas.json, iluminacao_presets.json)
"""
import bpy, os, json, math, shutil
HERE = os.path.dirname(os.path.abspath(bpy.data.filepath))
OUT = os.path.normpath(os.path.join(HERE, "..", "Models", "Base"))
os.makedirs(OUT, exist_ok=True)
scene = bpy.context.scene
scene.frame_set(1)

leaves = [o for o in bpy.data.objects if o.type == 'MESH' and o.name.startswith("Porta_")]
scenery = [o for o in bpy.data.objects if o.type == 'MESH' and not o.name.startswith("Porta_")]
lights = [o for o in bpy.data.objects if o.type == 'LIGHT']

def select(objs):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]

# ---- FBX 1: cenario estatico + luzes (sem animacao)
select(scenery + lights)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "base_cenario.fbx"), use_selection=True, object_types={'MESH', 'LIGHT'},
                         apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', bake_anim=False, mesh_smooth_type='FACE')
# ---- FBX 2: folhas das portas com animacao de abertura (um clipe por folha)
for o in leaves:
    if o.animation_data:
        for tr in o.animation_data.nla_tracks: tr.mute = False
select(leaves)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "base_portas.fbx"), use_selection=True, object_types={'MESH'},
                         apply_scale_options='FBX_SCALE_ALL', path_mode='COPY', bake_anim=True, bake_anim_use_nla_strips=True,
                         bake_anim_use_all_actions=False, bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE')

# ---- JSON: portas
portas = []
z1 = {o.name: o.rotation_euler.z for o in leaves}
scene.frame_set(40); bpy.context.view_layer.update()
z40 = {o.name: o.rotation_euler.z for o in leaves}
scene.frame_set(1); bpy.context.view_layer.update()
for o in sorted(leaves, key=lambda x: x.name):
    ang = math.degrees(z40[o.name] - z1[o.name])
    parts = o.name.split("_")        # Porta_<grupo>[_<sub>]_<D|E>
    portas.append({"objeto": o.name, "porta": "_".join(parts[1:-1]), "folha": parts[-1],
                   "pivo_dobradica_blender": [round(v, 4) for v in o.location], "rotacao_z_inicial_graus": round(math.degrees(z1[o.name]), 3),
                   "angulo_abertura_graus": round(ang, 2), "eixo_de_giro_blender": "Z", "quadros": [1, 40], "fps": scene.render.fps,
                   "clipe_fbx": f"{o.name}_Abrir"})
json.dump({"observacao": "o pivo do objeto no FBX ja e a dobradica. Eixo Z do Blender = eixo Y (vertical) no Unity. Sinal do angulo: positivo = anti-horario visto de cima no Blender.", "portas": portas},
          open(os.path.join(OUT, "base_portas.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)

# ---- JSON: luzes
L = []
for o in sorted(lights, key=lambda x: x.name):
    d = o.data
    e = {"nome": o.name, "tipo": d.type, "grupo": o.get("grupo"), "energia_blender_W": round(o.get("e0", d.energy), 2), "cor_rgb": [round(c, 3) for c in d.color],
         "posicao_blender": [round(v, 3) for v in o.matrix_world.translation], "sombra": bool(d.use_shadow)}
    if d.type in ('POINT', 'SPOT'): e["raio_m"] = round(d.shadow_soft_size, 3)
    if d.type == 'SPOT': e["cone_graus"] = round(math.degrees(d.spot_size), 1); e["suavizacao_cone"] = round(d.spot_blend, 2)
    if d.type == 'SUN': e["rotacao_blender_graus"] = [round(math.degrees(v), 1) for v in o.rotation_euler]
    L.append(e)
json.dump({"observacao": "As luzes tambem estao dentro de base_cenario.fbx com os MESMOS nomes. Grupos: sol, terra, int, ext, emerg. Energia final = energia_blender_W * multiplicador do grupo (ver iluminacao_presets.json). Watts do Blender NAO equivalem a unidades do Unity: recalibrar.", "luzes": L},
          open(os.path.join(OUT, "base_luzes.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)

# ---- JSON: materiais
used = {}
for o in scenery + leaves:
    for s in o.material_slots:
        if s.material: used[s.material.name] = s.material
M = []
for n, m in sorted(used.items()):
    b = m.node_tree.nodes.get("Principled BSDF")
    if not b: continue
    ec = b.inputs["Emission Color"].default_value; es = b.inputs["Emission Strength"].default_value
    M.append({"nome": n, "cor_base": [round(c, 3) for c in b.inputs["Base Color"].default_value[:3]], "metalico": round(b.inputs["Metallic"].default_value, 2),
              "rugosidade": round(b.inputs["Roughness"].default_value, 2), "emissao_cor": [round(c, 3) for c in ec[:3]] if es > 0 else None,
              "emissao_intensidade": round(es, 2) if es > 0 else 0, "alpha": round(b.inputs["Alpha"].default_value, 2), "transparente": b.inputs["Alpha"].default_value < 1})
json.dump({"materiais": M}, open(os.path.join(OUT, "base_materiais.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
shutil.copy(os.path.join(HERE, "iluminacao_presets.json"), os.path.join(OUT, "iluminacao_presets.json"))
print("OK", len(scenery), "malhas;", len(leaves), "folhas;", len(lights), "luzes;", len(M), "materiais")
