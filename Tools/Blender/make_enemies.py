# SPDX-License-Identifier: GPL-3.0-or-later
# 이 폴더(Tools/Blender/)의 스크립트는 Blender Python API 를 쓰므로 GPL-3.0-or-later 다(LICENSE). 만든 모델(FBX · PNG)은 영향이 없다.
# P03 — Blender CLI 로 샘플 에셋 만들기: 슬라임(뼈 2 · 동작 4) · 박쥐(뼈 3 · 동작 2) · 등불 탑(정적)
# 팔레트 텍스처 한 장(4x4) · 면마다 셀 UV · FBX 내보내기(두 변형) · EEVEE 미리보기 렌더 · 결과 JSON
# 실행: Blender -b --factory-startup -noaudio --python-exit-code 1 --python make_enemies.py -- <출력 폴더>
import bpy, bmesh, sys, os, json, time, math
from mathutils import Vector, Matrix

out = sys.argv[sys.argv.index("--") + 1]
os.makedirs(out, exist_ok=True)
res = {"blender": bpy.app.version_string, "steps": {}, "models": {}}
T0 = time.time()


def step(name, t0):
    res["steps"][name] = round(time.time() - t0, 2)


bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 24

# ---------------------------------------------------------------- 팔레트(4x4) — 셀 번호 = x + 4*y, (0,0) 이 왼쪽 아래
PAL = [
    (0.46, 0.82, 0.40, 1), (0.26, 0.58, 0.30, 1), (0.98, 0.98, 0.95, 1), (0.10, 0.10, 0.14, 1),   # 0 슬라임 1 슬라임 진함 2 눈흰자 3 눈동자
    (0.96, 0.56, 0.62, 1), (0.48, 0.36, 0.68, 1), (0.30, 0.22, 0.46, 1), (0.95, 0.92, 0.80, 1),   # 4 볼 5 박쥐 몸 6 박쥐 날개 7 박쥐 귀 안
    (0.74, 0.74, 0.76, 1), (0.52, 0.52, 0.56, 1), (0.92, 0.50, 0.24, 1), (1.00, 0.86, 0.40, 1),   # 8 돌 밝음 9 돌 어두움 10 지붕 11 등불
    (0.55, 0.78, 0.36, 1), (0.40, 0.62, 0.28, 1), (0.55, 0.38, 0.24, 1), (1.00, 1.00, 1.00, 1),   # 12 잔디 13 잔디 진함 14 나무 15 흰색
]
C_SLIME, C_SLIME_DK, C_EYE, C_PUPIL, C_CHEEK, C_BAT, C_WING, C_EAR, C_STONE, C_STONE_DK, C_ROOF, C_LAMP, C_GRASS, C_GRASS_DK, C_WOOD, C_WHITE = range(16)
img = bpy.data.images.new("Palette", width=4, height=4)
img.pixels = [c for col in PAL for c in col]
pal_path = os.path.join(out, "palette.png")
img.filepath_raw = pal_path
img.file_format = 'PNG'
img.save()
mat = bpy.data.materials.new("Palette")
mat.use_nodes = True
nt = mat.node_tree
bsdf = next(n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED')
tex = nt.nodes.new("ShaderNodeTexImage")
tex.image = img
tex.interpolation = 'Closest'
nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
bsdf.inputs["Roughness"].default_value = 0.9
if "Specular IOR Level" in bsdf.inputs:
    bsdf.inputs["Specular IOR Level"].default_value = 0.15


def set_cell(obj, cell):
    """오브젝트의 모든 면 UV 를 팔레트 셀 하나의 중심으로."""
    me = obj.data
    bm = bmesh.new()
    bm.from_mesh(me)
    uv = bm.loops.layers.uv.verify()
    u, v = ((cell % 4) + 0.5) / 4.0, ((cell // 4) + 0.5) / 4.0
    for f in bm.faces:
        for loop in f.loops:
            loop[uv].uv = (u, v)
    bm.to_mesh(me)
    bm.free()
    if not me.materials:
        me.materials.append(mat)


def active(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj


def sphere(name, r, loc, cell, seg=10, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(scale=True)
    bpy.ops.object.shade_flat()
    set_cell(o, cell)
    return o


def cone(name, r1, r2, depth, loc, cell, rot=(0, 0, 0), verts=8):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.shade_flat()
    set_cell(o, cell)
    return o


def cylinder(name, r, depth, loc, cell, verts=8):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.shade_flat()
    set_cell(o, cell)
    return o


def group_all(obj, name):
    vg = obj.vertex_groups.new(name=name)
    vg.add([v.index for v in obj.data.vertices], 1.0, 'REPLACE')


def join(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    j = bpy.context.active_object
    j.name = name
    return j


def mesh_stats(o):
    return {"verts": len(o.data.vertices), "faces": len(o.data.polygons)}


# ---------------------------------------------------------------- 1. 슬라임 — 둥근 몸 · 큰 눈 둘 · 볼 둘
t0 = time.time()
body = sphere("SlimeBody", 0.5, (0, 0, 0), C_SLIME, seg=12, rings=8)
for v in body.data.vertices:
    z = v.co.z
    v.co.z = max(0.0, 0.40 + (z * 0.85 if z > 0 else z * 0.75))
    if z < 0:  # 아래쪽을 살짝 넓혀 물방울 꼴
        v.co.x *= 1.0 + 0.12 * (-z / 0.5)
        v.co.y *= 1.0 + 0.12 * (-z / 0.5)
parts = [body]
for sx in (-1, 1):
    parts.append(sphere("Eye", 0.10, (sx * 0.17, -0.40, 0.50), C_EYE, seg=8, rings=6))
    parts.append(sphere("Pupil", 0.05, (sx * 0.17, -0.48, 0.51), C_PUPIL, seg=6, rings=4))
    parts.append(sphere("Cheek", 0.07, (sx * 0.31, -0.40, 0.34), C_CHEEK, seg=6, rings=4, scale=(1, 0.5, 0.7)))
slime = join(parts, "Slime")
# 뼈대: SlimeRoot(0~0.35) · SlimeTop(0.35~0.85). 가중치는 높이로(결정적)
bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
slime_arm = bpy.context.active_object
slime_arm.name = "SlimeRig"
eb = slime_arm.data.edit_bones
root = eb[0]
root.name = "SlimeRoot"
root.head, root.tail = (0, 0, 0), (0, 0, 0.35)
top = eb.new("SlimeTop")
top.head, top.tail = (0, 0, 0.35), (0, 0, 0.85)
top.parent = root
bpy.ops.object.mode_set(mode='OBJECT')
slime.parent = slime_arm
slime.modifiers.new("Armature", 'ARMATURE').object = slime_arm
vr, vt = slime.vertex_groups.new(name="SlimeRoot"), slime.vertex_groups.new(name="SlimeTop")
for v in slime.data.vertices:
    w = min(max((v.co.z - 0.25) / 0.45, 0.0), 1.0)
    vr.add([v.index], 1.0 - w, 'REPLACE')
    vt.add([v.index], w, 'REPLACE')
res["models"]["slime"] = {"mesh": mesh_stats(slime), "bones": [b.name for b in slime_arm.data.bones]}
step("slime_model", t0)

# ---------------------------------------------------------------- 2. 박쥐 — 몸 · 귀 둘 · 눈 둘 · 날개 둘(6각 판 + 두께)
t0 = time.time()
bbody = sphere("BatBody", 0.22, (0, 0, 0), C_BAT, seg=10, rings=6)
group_all(bbody, "BatRoot")
bparts = [bbody]
for sx in (-1, 1):
    e = cone("Ear", 0.07, 0.0, 0.17, (sx * 0.12, 0, 0.24), C_BAT, rot=(0, sx * 0.45, 0), verts=6)
    group_all(e, "BatRoot")
    bparts.append(e)
    ey = sphere("BatEye", 0.06, (sx * 0.09, -0.19, 0.05), C_EYE, seg=8, rings=6)
    group_all(ey, "BatRoot")
    bparts.append(ey)
    pu = sphere("BatPupil", 0.03, (sx * 0.09, -0.245, 0.05), C_PUPIL, seg=6, rings=4)
    group_all(pu, "BatRoot")
    bparts.append(pu)
    # 날개 — 앞 가장자리 곧고 뒤 가장자리 물결
    pts = [(0.14, -0.08, 0.02), (0.80, -0.05, 0.06), (0.70, 0.22, 0.0), (0.55, 0.12, 0.0), (0.40, 0.26, 0.0), (0.14, 0.10, 0.0)]
    pts = [(sx * x, y, z) for x, y, z in pts]
    if sx < 0:
        pts.reverse()
    wm = bpy.data.meshes.new("Wing")
    wm.from_pydata(pts, [], [list(range(len(pts)))])
    wo = bpy.data.objects.new("Wing", wm)
    scene.collection.objects.link(wo)
    active(wo)
    sol = wo.modifiers.new("Solidify", 'SOLIDIFY')
    sol.thickness = 0.03
    sol.offset = 0.0
    bpy.ops.object.modifier_apply(modifier="Solidify")
    bpy.ops.object.shade_flat()
    set_cell(wo, C_WING)
    group_all(wo, "WingL" if sx > 0 else "WingR")
    bparts.append(wo)
bat = join(bparts, "Bat")
bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
bat_arm = bpy.context.active_object
bat_arm.name = "BatRig"
eb = bat_arm.data.edit_bones
broot = eb[0]
broot.name = "BatRoot"
broot.head, broot.tail = (0, 0, -0.25), (0, 0, 0.0)
for nm, sx in (("WingL", 1), ("WingR", -1)):
    w = eb.new(nm)
    w.head, w.tail = (sx * 0.14, 0, 0.02), (sx * 0.80, 0, 0.02)
    w.parent = broot
bpy.ops.object.mode_set(mode='OBJECT')
bat.parent = bat_arm
bat.modifiers.new("Armature", 'ARMATURE').object = bat_arm
res["models"]["bat"] = {"mesh": mesh_stats(bat), "bones": [b.name for b in bat_arm.data.bones],
                        "groups": {g.name: sum(1 for v in bat.data.vertices for gg in v.groups if gg.group == g.index) for g in bat.vertex_groups}}
step("bat_model", t0)

# ---------------------------------------------------------------- 3. 등불 탑(정적) — 돌 기둥 · 턱 · 등불집 · 지붕 · 꼭지
t0 = time.time()
tparts = [
    cylinder("Base", 0.62, 1.6, (0, 0, 0.8), C_STONE, verts=8),
    cylinder("Ledge", 0.74, 0.16, (0, 0, 1.68), C_STONE_DK, verts=8),
    cylinder("Lamp", 0.34, 0.5, (0, 0, 2.0), C_LAMP, verts=8),
    cone("Roof", 0.66, 0.0, 0.7, (0, 0, 2.6), C_ROOF, verts=8),
    sphere("Tip", 0.09, (0, 0, 2.98), C_LAMP, seg=6, rings=4),
]
for sx, sy in ((1, 0), (-1, 0), (0, 1), (0, -1)):  # 등불집 기둥 4
    tparts.append(cylinder("Post", 0.05, 0.56, (sx * 0.3, sy * 0.3, 2.0), C_WOOD, verts=4))
tower = join(tparts, "Tower")
res["models"]["tower"] = {"mesh": mesh_stats(tower)}
step("tower_model", t0)

# ---------------------------------------------------------------- 4. 동작 — 슬라임 Idle · Hop · Hit · Die, 박쥐 Flap · Hit
t0 = time.time()


def local_axis_for_world(arm, bone_name, world_axis):
    """포즈 뼈 회전은 뼈 기준 공간이다 — 월드 축을 뼈 공간으로 옮긴다."""
    m = (arm.matrix_world @ arm.data.bones[bone_name].matrix_local).to_3x3()
    return (m.inverted() @ Vector(world_axis)).normalized()


def rot_about_world(arm, bone_name, world_axis, angle):
    return Matrix.Rotation(angle, 3, local_axis_for_world(arm, bone_name, world_axis)).to_euler('XYZ')


def make_action(arm, name, keys):
    arm.animation_data_create()
    for pb in arm.pose.bones:
        pb.rotation_mode = 'XYZ'
    arm.animation_data.action = None
    for frame, bone, prop, value in keys:
        pb = arm.pose.bones[bone]
        setattr(pb, prop, value)
        pb.keyframe_insert(data_path=prop, frame=frame)
    act = arm.animation_data.action
    act.name = name
    act.use_fake_user = True
    track = arm.animation_data.nla_tracks.new()
    track.name = name
    track.strips.new(name, int(act.frame_range[0]), act)
    track.mute = True
    arm.animation_data.action = None
    reset_pose(arm)
    return act


def reset_pose(arm):
    for pb in arm.pose.bones:
        pb.location = (0, 0, 0)
        pb.rotation_euler = (0, 0, 0)
        pb.scale = (1, 1, 1)


def assign(arm, act):
    reset_pose(arm)
    arm.animation_data.action = act
    try:
        if act is not None and act.slots and arm.animation_data.action_slot is None:
            arm.animation_data.action_slot = act.slots[0]
    except AttributeError:
        pass


S, Y = "scale", "location"  # 뼈 기준: Y 가 뼈 방향(= 위)
R = "rotation_euler"
slime_acts = {
    "Idle": make_action(slime_arm, "Idle", [
        (1, "SlimeRoot", S, (1, 1, 1)), (12, "SlimeRoot", S, (1.05, 0.93, 1.05)), (24, "SlimeRoot", S, (1, 1, 1)),
        (1, "SlimeTop", R, (0, 0, 0)), (6, "SlimeTop", R, (0.07, 0, 0)), (18, "SlimeTop", R, (-0.07, 0, 0)), (24, "SlimeTop", R, (0, 0, 0))]),
    "Hop": make_action(slime_arm, "Hop", [
        (1, "SlimeRoot", Y, (0, 0, 0)), (1, "SlimeRoot", S, (1, 1, 1)),
        (5, "SlimeRoot", Y, (0, 0, 0)), (5, "SlimeRoot", S, (1.15, 0.80, 1.15)),
        (8, "SlimeRoot", Y, (0, 0.25, 0)), (8, "SlimeRoot", S, (0.88, 1.22, 0.88)),
        (13, "SlimeRoot", Y, (0, 0.55, 0)), (13, "SlimeRoot", S, (1, 1, 1)),
        (18, "SlimeRoot", Y, (0, 0.2, 0)), (18, "SlimeRoot", S, (0.92, 1.15, 0.92)),
        (21, "SlimeRoot", Y, (0, 0, 0)), (21, "SlimeRoot", S, (1.2, 0.75, 1.2)),
        (24, "SlimeRoot", Y, (0, 0, 0)), (24, "SlimeRoot", S, (1, 1, 1))]),
    "Hit": make_action(slime_arm, "Hit", [
        (1, "SlimeRoot", S, (1, 1, 1)), (3, "SlimeRoot", S, (1.3, 0.65, 1.3)), (8, "SlimeRoot", S, (0.95, 1.08, 0.95)), (12, "SlimeRoot", S, (1, 1, 1)),
        (1, "SlimeTop", R, (0, 0, 0)), (3, "SlimeTop", R, (-0.35, 0, 0)), (12, "SlimeTop", R, (0, 0, 0))]),
    "Die": make_action(slime_arm, "Die", [
        (1, "SlimeRoot", S, (1, 1, 1)), (4, "SlimeRoot", S, (0.9, 1.15, 0.9)), (18, "SlimeRoot", S, (1.7, 0.06, 1.7))]),
}
up_l, dn_l = rot_about_world(bat_arm, "WingL", (0, 1, 0), -0.6), rot_about_world(bat_arm, "WingL", (0, 1, 0), 0.5)
up_r, dn_r = rot_about_world(bat_arm, "WingR", (0, 1, 0), 0.6), rot_about_world(bat_arm, "WingR", (0, 1, 0), -0.5)
bat_acts = {
    "Flap": make_action(bat_arm, "Flap", [
        (1, "WingL", R, up_l), (6, "WingL", R, dn_l), (12, "WingL", R, up_l),
        (1, "WingR", R, up_r), (6, "WingR", R, dn_r), (12, "WingR", R, up_r),
        (1, "BatRoot", Y, (0, 0.03, 0)), (6, "BatRoot", Y, (0, -0.03, 0)), (12, "BatRoot", Y, (0, 0.03, 0))]),
    "Hit": make_action(bat_arm, "Hit", [
        (1, "BatRoot", S, (1, 1, 1)), (3, "BatRoot", S, (1.3, 0.7, 1.3)), (10, "BatRoot", S, (1, 1, 1))]),
}
res["actions"] = {a.name: [round(x, 1) for x in a.frame_range] for a in bpy.data.actions}


def extent(arm, mesh_obj, act, frame):
    assign(arm, act)
    scene.frame_set(frame)
    dg = bpy.context.evaluated_depsgraph_get()
    ev = mesh_obj.evaluated_get(dg)
    m = ev.to_mesh()
    ps = [ev.matrix_world @ v.co for v in m.vertices]
    r = {"max_z": round(max(p.z for p in ps), 3), "min_z": round(min(p.z for p in ps), 3), "max_x": round(max(p.x for p in ps), 3)}
    ev.to_mesh_clear()
    return r


res["deform"] = {
    "slime_hop_f1": extent(slime_arm, slime, slime_acts["Hop"], 1), "slime_hop_f13": extent(slime_arm, slime, slime_acts["Hop"], 13),
    "slime_die_f18": extent(slime_arm, slime, slime_acts["Die"], 18),
    "bat_flap_f1": extent(bat_arm, bat, bat_acts["Flap"], 1), "bat_flap_f6": extent(bat_arm, bat, bat_acts["Flap"], 6),
}
assign(slime_arm, None)
assign(bat_arm, None)
step("animation", t0)

# ---------------------------------------------------------------- 5. 저장 · FBX(모델마다 · 두 변형: bake_space_transform 끔/켬)
t0 = time.time()
slime_arm.location = (-1.6, 0, 0)
bat_arm.location = (0, 0, 1.2)
tower.location = (2.2, 0, 0)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "p03_enemies.blend"))
for o in (slime_arm, bat_arm, tower):  # 내보내기는 원점에서
    o.location = (0, 0, 0)
fbx_out = {}
for name, objs in (("slime", [slime, slime_arm]), ("bat", [bat, bat_arm]), ("tower", [tower])):
    for suffix, bst in (("", False), ("_bst", True)):
        bpy.ops.object.select_all(action='DESELECT')
        for o in objs:
            o.select_set(True)
        p = os.path.join(out, f"{name}{suffix}.fbx")
        bpy.ops.export_scene.fbx(filepath=p, use_selection=True, object_types={'ARMATURE', 'MESH'},
                                 apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                                 bake_space_transform=bst, add_leaf_bones=False, mesh_smooth_type='FACE',
                                 path_mode='STRIP', bake_anim=True, bake_anim_use_all_actions=True,
                                 bake_anim_use_nla_strips=False, bake_anim_simplify_factor=0.0)
        fbx_out[os.path.basename(p)] = os.path.getsize(p)
res["fbx_bytes"] = fbx_out
slime_arm.location = (-1.6, 0, 0)
bat_arm.location = (0, 0, 1.2)
tower.location = (2.2, 0, 0)
step("export", t0)

# ---------------------------------------------------------------- 6. 미리보기 렌더(EEVEE) — 라인업 1장 · 자세 6장
t0 = time.time()
world = bpy.data.worlds.new("World")
world.use_nodes = True
bg = world.node_tree.nodes["Background"]
bg.inputs[0].default_value = (0.80, 0.88, 0.96, 1)
bg.inputs[1].default_value = 1.0
scene.world = world
cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
scene.collection.objects.link(cam)
scene.camera = cam
sun = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", 'SUN'))
sun.data.energy = 3.0
scene.collection.objects.link(sun)
sun.rotation_euler = (0.8, 0.3, 0.9)
engines = [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items]
scene.render.engine = next(e for e in engines if e.startswith("BLENDER_EEVEE"))
scene.render.image_settings.file_format = 'PNG'


def look(cam_loc, target):
    cam.location = cam_loc
    cam.rotation_euler = (Vector(target) - Vector(cam_loc)).to_track_quat('-Z', 'Y').to_euler()


def render(path, w, h):
    scene.render.resolution_x, scene.render.resolution_y = w, h
    scene.render.filepath = path
    r0 = time.time()
    bpy.ops.render.render(write_still=True)
    return round(time.time() - r0, 2) if os.path.exists(path) else "파일 없음"


renders = {}
look((4.6, -6.0, 3.6), (0.3, 0, 1.0))
renders["lineup.png"] = render(os.path.join(out, "render_lineup.png"), 960, 540)
poses = [("slime", slime_arm, slime_acts["Idle"], 12), ("slime", slime_arm, slime_acts["Hop"], 8), ("slime", slime_arm, slime_acts["Hit"], 3),
         ("slime", slime_arm, slime_acts["Die"], 18), ("bat", bat_arm, bat_acts["Flap"], 1), ("bat", bat_arm, bat_acts["Flap"], 6)]
for who, arm, act, f in poses:
    for o in (slime_arm, bat_arm, tower, slime, bat):
        o.hide_render = o not in (arm, slime if arm is slime_arm else bat)
    assign(arm, act)
    scene.frame_set(f)
    c = arm.location
    look((c.x + 2.2, c.y - 2.6, c.z + 1.7), (c.x, c.y, c.z + 0.45))
    nm = f"render_{who}_{act.name}_f{f:02d}.png"
    renders[nm] = render(os.path.join(out, nm), 480, 480)
    assign(arm, None)
for o in (slime_arm, bat_arm, tower, slime, bat):
    o.hide_render = False
res["renders_sec"] = renders
step("render", t0)

# ---------------------------------------------------------------- 7. FBX 다시 읽기 확인(기본 변형)
t0 = time.time()
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=os.path.join(out, "slime.fbx"))
res["reimport_slime"] = {"meshes": sum(o.type == 'MESH' for o in bpy.data.objects), "armatures": sum(o.type == 'ARMATURE' for o in bpy.data.objects),
                         "bones": [b.name for a in bpy.data.armatures for b in a.bones],
                         "actions": {a.name: [round(x, 1) for x in a.frame_range] for a in bpy.data.actions}}
step("reimport", t0)
res["total_sec"] = round(time.time() - T0, 2)
print("P03_RESULT " + json.dumps(res, ensure_ascii=False))
