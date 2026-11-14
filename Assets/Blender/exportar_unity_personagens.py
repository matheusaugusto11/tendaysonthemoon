"""Gera personagens.json (metadados para o Unity) a partir de personagens.blend. Os FBX saem de personagens.py.
Run: blender -b Assets/Blender/personagens.blend -P Assets/Blender/exportar_unity_personagens.py
"""
import bpy, os, json
HERE = os.path.dirname(os.path.abspath(bpy.data.filepath))
OUT = os.path.normpath(os.path.join(HERE, "..", "Models", "Personagens"))
CLIPS = {"Astronauta": [("Idle", 48, True), ("Caminhar", 24, True), ("Correr", 16, True)], "Robo": [("Idle", 48, True), ("Rodar", 24, True), ("Acelerar", 16, True)]}
HUMANOID = {"Hips": "Hips", "Spine": "Spine", "Chest": "Chest", "Neck": "Neck", "Head": "Head", "Shoulder_L": "LeftShoulder", "UpperArm_L": "LeftUpperArm",
            "LowerArm_L": "LeftLowerArm", "Hand_L": "LeftHand", "Shoulder_R": "RightShoulder", "UpperArm_R": "RightUpperArm", "LowerArm_R": "RightLowerArm", "Hand_R": "RightHand",
            "UpperLeg_L": "LeftUpperLeg", "LowerLeg_L": "LeftLowerLeg", "Foot_L": "LeftFoot", "Toes_L": "LeftToes",
            "UpperLeg_R": "RightUpperLeg", "LowerLeg_R": "RightLowerLeg", "Foot_R": "RightFoot", "Toes_R": "RightToes"}
res = {"observacao": "Frente do personagem = -Y no Blender (+Z no Unity). Escala em metros, pivô na base dos pés. Pose de repouso em A (nao T). Clipes: quadro 1..N+1, 24 fps; o ultimo quadro repete o primeiro (se o loop engasgar, recortar o ultimo quadro).",
       "personagens": []}
for ao in sorted([o for o in bpy.data.objects if o.type == 'ARMATURE'], key=lambda o: o.location.x):
    me = [c for c in ao.children if c.type == 'MESH'][0]
    kind = "Robo" if ao.name.startswith("Robo") else "Astronauta"
    name = me.name
    zs = [(me.matrix_world @ v.co).z for v in me.data.vertices]
    mats = []
    for m in me.data.materials:
        b = m.node_tree.nodes["Principled BSDF"]
        es = b.inputs["Emission Strength"].default_value
        mats.append({"nome": m.name, "cor_base": [round(c, 3) for c in b.inputs["Base Color"].default_value[:3]], "metalico": round(b.inputs["Metallic"].default_value, 2),
                     "rugosidade": round(b.inputs["Roughness"].default_value, 2), "emissao_cor": [round(c, 3) for c in b.inputs["Emission Color"].default_value[:3]] if es > 0 else None, "emissao_intensidade": round(es, 2)})
    bones = [{"osso": b.name, "pai": b.parent.name if b.parent else None, "humanoid": HUMANOID.get(b.name) if kind == "Astronauta" else None} for b in ao.data.bones]
    res["personagens"].append({"nome": name, "arquivo": f"{name}.fbx", "tipo": kind, "altura_m": round(max(zs) - min(zs), 3), "triangulos": sum(len(p.vertices) - 2 for p in me.data.polygons),
                               "ossos": bones, "clipes": [{"nome": n, "quadros": q, "fps": 24, "duracao_s": round(q / 24, 3), "loop": lp} for n, q, lp in CLIPS[kind]], "materiais": mats})
json.dump(res, open(os.path.join(OUT, "personagens.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
print("OK", [(p["nome"], p["altura_m"], p["triangulos"]) for p in res["personagens"]])
