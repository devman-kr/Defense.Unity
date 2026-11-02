# SPDX-License-Identifier: GPL-3.0-or-later
# 이 폴더(Tools/Blender/)의 스크립트는 Blender Python API 를 쓰므로 GPL-3.0-or-later 다(LICENSE). 만든 모델(FBX · PNG)은 영향이 없다.
# P04 — make_enemies.py 의 슬라임 · 박쥐 · 등불 탑을 매끈 판으로: 같은 뼈대 · 같은 동작 · 같은 팔레트 셀 + 서브디비전 · 스무스 셰이딩
# 도우미는 make_enemies.py 에서 복사했다(그 파일의 산출을 바꾸지 않으려고 공통 모듈로 뽑지 않았다). 비교 렌더용으로 로우폴리 판도 같이 만든다.
# 실행: Blender -b --factory-startup -noaudio --python-exit-code 1 --python make_enemies_smooth.py -- <출력 폴더>
import bpy, bmesh, sys, os, json, time, math
from mathutils import Vector, Matrix

out = sys.argv[sys.argv.index("--") + 1]
os.makedirs(out, exist_ok=True)
res = {"blender": bpy.app.version_string, "steps": {}, "models": {}}
T0 = time.time()

SUBDIV_LEVELS = 2          # 캐릭터 · 탑 모두 2단계(Catmull-Clark)
EDGE_CREASE = 0.75         # 탑 윗면 · 아랫면 테두리와 날개 외곽 — 1 이면 날카롭고 0 이면 녹아 윤곽이 사라진다
EDGE_CREASE_ANGLE = math.radians(50)  # 이웃 면 사이 각이 이보다 큰 모서리만(원기둥 옆면끼리 45° 는 둥글게)
APEX_CREASE = 1.0          # 원뿔 꼭짓점(박쥐 귀 · 탑 지붕) — 없으면 서브디비전이 꼭짓점을 끌어내려 귀는 혹, 지붕은 버섯이 된다


def step(name, t0):
    res["steps"][name] = round(time.time() - t0, 2)


bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 24

# ---------------------------------------------------------------- 팔레트(4x4) — make_enemies.py 와 같은 표. 셀 번호 = x + 4*y, (0,0) 이 왼쪽 아래
PAL = [
    (0.46, 0.82, 0.40, 1), (0.26, 0.58, 0.30, 1), (0.98, 0.98, 0.95, 1), (0.10, 0.10, 0.14, 1),   # 0 슬라임 1 슬라임 진함 2 눈흰자 3 눈동자
    (0.96, 0.56, 0.62, 1), (0.48, 0.36, 0.68, 1), (0.30, 0.22, 0.46, 1), (0.95, 0.92, 0.80, 1),   # 4 볼 5 박쥐 몸 6 박쥐 날개 7 박쥐 귀 안
    (0.74, 0.74, 0.76, 1), (0.52, 0.52, 0.56, 1), (0.92, 0.50, 0.24, 1), (1.00, 0.86, 0.40, 1),   # 8 돌 밝음 9 돌 어두움 10 지붕 11 등불
    (0.55, 0.78, 0.36, 1), (0.40, 0.62, 0.28, 1), (0.55, 0.38, 0.24, 1), (1.00, 1.00, 1.00, 1),   # 12 잔디 13 잔디 진함 14 나무 15 흰색
]
C_SLIME, C_SLIME_DK, C_EYE, C_PUPIL, C_CHEEK, C_BAT, C_WING, C_EAR, C_STONE, C_STONE_DK, C_ROOF, C_LAMP, C_GRASS, C_GRASS_DK, C_WOOD, C_WHITE = range(16)
img = bpy.data.images.new("Palette", width=4, height=4)
img.pixels = [c for col in PAL for c in col]
pal_path = os.path.join(out, "Palette.png")
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
    if not me.uv_layers:  # from_pydata 메시는 UV 층이 없다 — bmesh 가 만들면 이름이 Float2 라 join 때 UVMap 과 합쳐지지 않는다
        me.uv_layers.new(name="UVMap")
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


def shade(smooth):
    if smooth:
        bpy.ops.object.shade_smooth()
    else:
        bpy.ops.object.shade_flat()


def sphere(name, r, loc, cell, smooth, seg=10, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(scale=True)
    shade(smooth)
    set_cell(o, cell)
    return o


def cone(name, r1, r2, depth, loc, cell, smooth, rot=(0, 0, 0), verts=8):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    o = bpy.context.active_object
    o.name = name
    shade(smooth)
    set_cell(o, cell)
    if smooth and r2 == 0.0:
        crease_apex(o)
    return o


def crease_apex(obj):
    """원뿔 꼭짓점(로컬 z 최대 정점)에 정점 크리스 — join 전, 오브젝트 회전이 메시에 들어가기 전에 부른다."""
    me = obj.data
    attr = me.attributes.get("crease_vert") or me.attributes.new("crease_vert", 'FLOAT', 'POINT')
    apex = max(me.vertices, key=lambda v: v.co.z)
    attr.data[apex.index].value = APEX_CREASE


def cylinder(name, r, depth, loc, cell, smooth, verts=8):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    shade(smooth)
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


def tris(me):
    return sum(len(p.vertices) - 2 for p in me.polygons)


def add_subsurf(obj):
    """뼈대 모디파이어 뒤에 붙인다 — 자세는 거친 메시에서 굽히고 그 결과를 나눈다(FBX 내보내기는 뼈대를 REST 로 두고 적용)."""
    m = obj.modifiers.new("Subsurf", 'SUBSURF')
    m.levels = SUBDIV_LEVELS
    m.render_levels = SUBDIV_LEVELS
    return m


def crease_sharp_edges(obj):
    """이웃 면 사이 각이 큰 모서리(윗면 · 아랫면 테두리 · 각기둥 모서리 · 날개 외곽)에 크리스 — 서브디비전 뒤에도 윤곽이 남고 모서리만 둥글다."""
    me = obj.data
    bm = bmesh.new()
    bm.from_mesh(me)
    layer = bm.edges.layers.float.get("crease_edge") or bm.edges.layers.float.new("crease_edge")
    n = 0
    for e in bm.edges:
        if len(e.link_faces) == 2 and e.calc_face_angle(0.0) > EDGE_CREASE_ANGLE:
            e[layer] = EDGE_CREASE
            n += 1
    bm.to_mesh(me)
    bm.free()
    return n


# ---------------------------------------------------------------- 1. 슬라임 — 둥근 몸 · 큰 눈 둘 · 볼 둘(make_enemies.py 와 같은 형태 · 뼈 · 가중치)
def build_slime(suffix, smooth):
    body = sphere("SlimeBody" + suffix, 0.5, (0, 0, 0), C_SLIME, smooth, seg=12, rings=8)
    for v in body.data.vertices:
        z = v.co.z
        v.co.z = max(0.0, 0.40 + (z * 0.85 if z > 0 else z * 0.75))
        if z < 0:  # 아래쪽을 살짝 넓혀 물방울 꼴
            v.co.x *= 1.0 + 0.12 * (-z / 0.5)
            v.co.y *= 1.0 + 0.12 * (-z / 0.5)
    parts = [body]
    for sx in (-1, 1):
        parts.append(sphere("Eye", 0.10, (sx * 0.17, -0.40, 0.50), C_EYE, smooth, seg=8, rings=6))
        parts.append(sphere("Pupil", 0.05, (sx * 0.17, -0.48, 0.51), C_PUPIL, smooth, seg=6, rings=4))
        parts.append(sphere("Cheek", 0.07, (sx * 0.31, -0.40, 0.34), C_CHEEK, smooth, seg=6, rings=4, scale=(1, 0.5, 0.7)))
    slime = join(parts, "Slime" + suffix)
    bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
    arm = bpy.context.active_object
    arm.name = "SlimeRig" + suffix
    eb = arm.data.edit_bones
    root = eb[0]
    root.name = "SlimeRoot"
    root.head, root.tail = (0, 0, 0), (0, 0, 0.35)
    top = eb.new("SlimeTop")
    top.head, top.tail = (0, 0, 0.35), (0, 0, 0.85)
    top.parent = root
    bpy.ops.object.mode_set(mode='OBJECT')
    slime.parent = arm
    slime.modifiers.new("Armature", 'ARMATURE').object = arm
    vr, vt = slime.vertex_groups.new(name="SlimeRoot"), slime.vertex_groups.new(name="SlimeTop")
    for v in slime.data.vertices:
        w = min(max((v.co.z - 0.25) / 0.45, 0.0), 1.0)
        vr.add([v.index], 1.0 - w, 'REPLACE')
        vt.add([v.index], w, 'REPLACE')
    if smooth:
        add_subsurf(slime)
    return slime, arm


# ---------------------------------------------------------------- 2. 박쥐 — 몸 · 귀 둘 · 눈 둘 · 날개 둘(6각 판 + 두께)
def build_bat(suffix, smooth):
    bbody = sphere("BatBody" + suffix, 0.22, (0, 0, 0), C_BAT, smooth, seg=10, rings=6)
    group_all(bbody, "BatRoot")
    bparts = [bbody]
    for sx in (-1, 1):
        e = cone("Ear", 0.07, 0.0, 0.17, (sx * 0.12, 0, 0.24), C_BAT, smooth, rot=(0, sx * 0.45, 0), verts=6)
        group_all(e, "BatRoot")
        bparts.append(e)
        ey = sphere("BatEye", 0.06, (sx * 0.09, -0.19, 0.05), C_EYE, smooth, seg=8, rings=6)
        group_all(ey, "BatRoot")
        bparts.append(ey)
        pu = sphere("BatPupil", 0.03, (sx * 0.09, -0.245, 0.05), C_PUPIL, smooth, seg=6, rings=4)
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
        shade(smooth)
        set_cell(wo, C_WING)
        if smooth:  # 얇은 판은 서브디비전이 외곽을 안으로 끌어 날개가 짧아진다
            crease_sharp_edges(wo)
        group_all(wo, "WingL" if sx > 0 else "WingR")
        bparts.append(wo)
    bat = join(bparts, "Bat" + suffix)
    bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
    arm = bpy.context.active_object
    arm.name = "BatRig" + suffix
    eb = arm.data.edit_bones
    broot = eb[0]
    broot.name = "BatRoot"
    broot.head, broot.tail = (0, 0, -0.25), (0, 0, 0.0)
    for nm, sx in (("WingL", 1), ("WingR", -1)):
        w = eb.new(nm)
        w.head, w.tail = (sx * 0.14, 0, 0.02), (sx * 0.80, 0, 0.02)
        w.parent = broot
    bpy.ops.object.mode_set(mode='OBJECT')
    bat.parent = arm
    bat.modifiers.new("Armature", 'ARMATURE').object = arm
    if smooth:
        add_subsurf(bat)
    return bat, arm


# ---------------------------------------------------------------- 3. 등불 탑(정적) — 돌 기둥 · 턱 · 등불집 · 지붕 · 꼭지
def build_tower(suffix, smooth):
    tparts = [
        cylinder("Base", 0.62, 1.6, (0, 0, 0.8), C_STONE, smooth, verts=8),
        cylinder("Ledge", 0.74, 0.16, (0, 0, 1.68), C_STONE_DK, smooth, verts=8),
        cylinder("Lamp", 0.34, 0.5, (0, 0, 2.0), C_LAMP, smooth, verts=8),
        cone("Roof", 0.66, 0.0, 0.7, (0, 0, 2.6), C_ROOF, smooth, verts=8),
        sphere("Tip", 0.09, (0, 0, 2.98), C_LAMP, smooth, seg=6, rings=4),
    ]
    for sx, sy in ((1, 0), (-1, 0), (0, 1), (0, -1)):  # 등불집 기둥 4
        tparts.append(cylinder("Post", 0.05, 0.56, (sx * 0.3, sy * 0.3, 2.0), C_WOOD, smooth, verts=4))
    tower = join(tparts, "Tower" + suffix)
    creased = 0
    if smooth:
        creased = crease_sharp_edges(tower)
        add_subsurf(tower)
    return tower, creased


t0 = time.time()
slime, slime_arm = build_slime("", True)
bat, bat_arm = build_bat("", True)
tower, tower_creased = build_tower("", True)
slime_lo, slime_arm_lo = build_slime("Low", False)
bat_lo, bat_arm_lo = build_bat("Low", False)
tower_lo, _ = build_tower("Low", False)
res["tower_creased_edges"] = tower_creased
res["modifier_order"] = {o.name: [m.type for m in o.modifiers] for o in (slime, bat, tower)}
step("models", t0)

# ---------------------------------------------------------------- 4. 동작 — 슬라임 Idle · Hop · Hit · Die, 박쥐 Flap · Hit(매끈 판 뼈대에 만들고 로우폴리 판은 같은 동작을 빌린다)
t0 = time.time()


def local_axis_for_world(arm, bone_name, world_axis):
    """포즈 뼈 회전은 뼈 기준 공간이다 — 월드 축을 뼈 공간으로 옮긴다."""
    m = (arm.matrix_world @ arm.data.bones[bone_name].matrix_local).to_3x3()
    return (m.inverted() @ Vector(world_axis)).normalized()


def rot_about_world(arm, bone_name, world_axis, angle):
    return Matrix.Rotation(angle, 3, local_axis_for_world(arm, bone_name, world_axis)).to_euler('XYZ')


def reset_pose(arm):
    for pb in arm.pose.bones:
        pb.location = (0, 0, 0)
        pb.rotation_euler = (0, 0, 0)
        pb.scale = (1, 1, 1)


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


def assign(arm, act):
    reset_pose(arm)
    arm.animation_data_create()
    for pb in arm.pose.bones:
        pb.rotation_mode = 'XYZ'
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


def extent(arm, mesh_obj, act, frame, tips=False):
    assign(arm, act)
    scene.frame_set(frame)
    dg = bpy.context.evaluated_depsgraph_get()
    ev = mesh_obj.evaluated_get(dg)
    m = ev.to_mesh()
    ps = [ev.matrix_world @ v.co for v in m.vertices]
    r = {"max_z": round(max(p.z for p in ps), 3), "min_z": round(min(p.z for p in ps), 3), "max_x": round(max(p.x for p in ps), 3)}
    if tips:  # 날개 끝(x 최대 · 최소 정점)의 높이 — 몸을 축으로 대칭이면 두 값이 같다
        r["tip_z_l"] = round(max(ps, key=lambda p: p.x).z, 3)
        r["tip_z_r"] = round(min(ps, key=lambda p: p.x).z, 3)
    ev.to_mesh_clear()
    return r


res["deform"] = {
    "smooth_slime_hop_f1": extent(slime_arm, slime, slime_acts["Hop"], 1), "smooth_slime_hop_f13": extent(slime_arm, slime, slime_acts["Hop"], 13),
    "smooth_slime_die_f18": extent(slime_arm, slime, slime_acts["Die"], 18),
    "smooth_bat_flap_f1": extent(bat_arm, bat, bat_acts["Flap"], 1, tips=True), "smooth_bat_flap_f6": extent(bat_arm, bat, bat_acts["Flap"], 6, tips=True),
    "low_slime_hop_f1": extent(slime_arm_lo, slime_lo, slime_acts["Hop"], 1), "low_slime_hop_f13": extent(slime_arm_lo, slime_lo, slime_acts["Hop"], 13),
    "low_bat_flap_f6": extent(bat_arm_lo, bat_lo, bat_acts["Flap"], 6, tips=True),
}
for a in (slime_arm, bat_arm, slime_arm_lo, bat_arm_lo):
    assign(a, None)
scene.frame_set(1)
step("animation", t0)

# ---------------------------------------------------------------- 5. 단계별 수 — 서브디비전 0 · 1 · 2 의 삼각형 · 정점(뼈대는 쉼 자세)
t0 = time.time()


def uv_off_center(me):
    """UV 가 셀 중심에서 가장 멀리 벗어난 거리(텍스처 공간, 셀 폭 0.25). 0 이면 서브디비전이 UV 를 흐트러뜨리지 않았다."""
    worst = 0.0
    for d in me.uv_layers.active.data:
        u, v = d.uv
        cu, cv = (math.floor(u * 4) + 0.5) / 4, (math.floor(v * 4) + 0.5) / 4
        worst = max(worst, abs(u - cu), abs(v - cv))
    return round(worst, 6)


for key, o in (("slime", slime), ("bat", bat), ("tower", tower)):
    sub = o.modifiers["Subsurf"]
    levels = {}
    for lv in (0, 1, 2):
        sub.levels = lv
        dg = bpy.context.evaluated_depsgraph_get()
        ev = o.evaluated_get(dg)
        m = ev.to_mesh()
        levels[lv] = {"tris": tris(m), "verts": len(m.vertices), "uv_off_center": uv_off_center(m)}
        if lv == SUBDIV_LEVELS:
            levels[lv]["vertex_groups"] = [g.name for g in o.vertex_groups]
            levels[lv]["smooth_faces"] = sum(p.use_smooth for p in m.polygons)
            levels[lv]["faces"] = len(m.polygons)
        ev.to_mesh_clear()
    sub.levels = SUBDIV_LEVELS
    res["models"][key] = {"levels": levels}
res["models"]["slime"]["bones"] = [b.name for b in slime_arm.data.bones]
res["models"]["bat"]["bones"] = [b.name for b in bat_arm.data.bones]
res["models"]["tower"]["bones"] = []
res["lowpoly"] = {k: {"tris": tris(o.data), "verts": len(o.data.vertices)} for k, o in (("slime", slime_lo), ("bat", bat_lo), ("tower", tower_lo))}
step("counts", t0)

# ---------------------------------------------------------------- 6. 저장 · FBX(매끈 판만 · bake_space_transform 켬 하나 — P03 발견 ④)
t0 = time.time()
layout = {slime_arm: (1.0, 0, 0), bat_arm: (2.4, 0, 1.2), tower: (4.0, 0, 0),
          slime_arm_lo: (-4.0, 0, 0), bat_arm_lo: (-2.6, 0, 1.2), tower_lo: (-1.0, 0, 0)}
for o, loc in layout.items():
    o.location = loc
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "p04_enemies_smooth.blend"))
for o in layout:  # 내보내기는 원점에서
    o.location = (0, 0, 0)
FBX_OPTIONS = dict(use_selection=True, object_types={'ARMATURE', 'MESH'}, use_mesh_modifiers=True, use_mesh_modifiers_render=True,
                   use_subsurf=False, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                   bake_space_transform=True, add_leaf_bones=False, mesh_smooth_type='FACE',
                   path_mode='STRIP', bake_anim=True, bake_anim_use_all_actions=True,
                   bake_anim_use_nla_strips=False, bake_anim_simplify_factor=0.0)
res["fbx_options"] = {k: (sorted(v) if isinstance(v, set) else v) for k, v in FBX_OPTIONS.items()}
fbx_out = {}
for name, objs in (("Slime", [slime, slime_arm]), ("Bat", [bat, bat_arm]), ("Tower", [tower])):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    p = os.path.join(out, f"{name}.fbx")
    bpy.ops.export_scene.fbx(filepath=p, **FBX_OPTIONS)
    fbx_out[os.path.basename(p)] = os.path.getsize(p)
res["fbx_bytes"] = fbx_out
for o, loc in layout.items():
    o.location = loc
step("export", t0)

# ---------------------------------------------------------------- 7. 미리보기 렌더(EEVEE) — 라인업(로우폴리 3 왼쪽 · 매끈 3 오른쪽) · 매끈 자세 · 근접
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


ALL = (slime_arm, bat_arm, tower, slime, bat, slime_arm_lo, bat_arm_lo, tower_lo, slime_lo, bat_lo)


def show_only(keep):
    for o in ALL:
        o.hide_render = o not in keep


renders = {}
show_only(ALL)
look((0.0, -14.5, 5.6), (0.0, 0, 1.1))
renders["render_lineup.png"] = render(os.path.join(out, "render_lineup.png"), 1280, 540)
poses = [("slime", slime_arm, slime, slime_acts["Idle"], 12), ("slime", slime_arm, slime, slime_acts["Hop"], 8),
         ("slime", slime_arm, slime, slime_acts["Hit"], 3), ("slime", slime_arm, slime, slime_acts["Die"], 18),
         ("bat", bat_arm, bat, bat_acts["Flap"], 1), ("bat", bat_arm, bat, bat_acts["Flap"], 6)]
for who, arm, me_obj, act, f in poses:
    show_only((arm, me_obj))
    assign(arm, act)
    scene.frame_set(f)
    c = arm.location
    if who == "bat":  # 날갯짓은 정면에서 — 사선 시점은 원근 때문에 대칭 날갯짓이 한쪽만 오르내리는 것처럼 보인다
        look((c.x, c.y - 2.6, c.z + 0.7), (c.x, c.y, c.z + 0.05))
    else:
        look((c.x + 2.2, c.y - 2.6, c.z + 1.7), (c.x, c.y, c.z + 0.45))
    nm = f"render_smooth_{who}_{act.name}_f{f:02d}.png"
    renders[nm] = render(os.path.join(out, nm), 480, 480)
    assign(arm, None)
scene.frame_set(1)
# 근접 — 슬라임 얼굴(매끈 · 로우폴리 같은 구도), 탑(매끈)
for nm, keep, c in (("render_close_slime_smooth.png", (slime_arm, slime), slime_arm.location),
                    ("render_close_slime_low.png", (slime_arm_lo, slime_lo), slime_arm_lo.location)):
    show_only(keep)
    look((c.x + 0.35, c.y - 1.35, c.z + 0.75), (c.x, c.y - 0.3, c.z + 0.45))
    renders[nm] = render(os.path.join(out, nm), 640, 640)
show_only((tower,))
c = tower.location
look((c.x + 3.0, c.y - 4.6, c.z + 3.2), (c.x, c.y, c.z + 1.6))
renders["render_close_tower_smooth.png"] = render(os.path.join(out, "render_close_tower_smooth.png"), 540, 720)
show_only(ALL)
res["renders_sec"] = renders
step("render", t0)

# ---------------------------------------------------------------- 8. FBX 다시 읽기 — 스킨 가중치 · 뼈 · 동작이 남았는지, Hop 13 꼭대기 높이(내보낸 메시 기준)
t0 = time.time()
reimport = {}
for name in ("Slime", "Bat", "Tower"):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=os.path.join(out, f"{name}.fbx"))
    meshes = [o for o in bpy.data.objects if o.type == 'MESH']
    arms = [o for o in bpy.data.objects if o.type == 'ARMATURE']
    me = meshes[0].data
    r = {"meshes": len(meshes), "armatures": len(arms), "verts": len(me.vertices), "tris": tris(me),
         "smooth_faces": sum(p.use_smooth for p in me.polygons), "faces": len(me.polygons),
         "uv_off_center": uv_off_center(me),
         "bones": [b.name for a in bpy.data.armatures for b in a.bones],
         "vertex_groups": {g.name: sum(1 for v in me.vertices for gg in v.groups if gg.group == g.index and gg.weight > 0) for g in meshes[0].vertex_groups},
         "actions": {a.name: [round(x, 1) for x in a.frame_range] for a in bpy.data.actions}}
    if name == "Slime":
        hop = next(a for a in bpy.data.actions if a.name.endswith("|Hop"))
        arm = arms[0]
        arm.animation_data_create()
        arm.animation_data.action = hop
        try:
            if hop.slots and arm.animation_data.action_slot is None:
                arm.animation_data.action_slot = hop.slots[0]
        except AttributeError:
            pass
        out_z = {}
        for f in (1, 13):
            bpy.context.scene.frame_set(f)
            dg = bpy.context.evaluated_depsgraph_get()
            ev = meshes[0].evaluated_get(dg)
            m = ev.to_mesh()
            ps = [ev.matrix_world @ v.co for v in m.vertices]
            out_z[f"hop_f{f}_max_z"] = round(max(p.z for p in ps), 3)
            out_z[f"hop_f{f}_min_z"] = round(min(p.z for p in ps), 3)
            ev.to_mesh_clear()
        r["deform"] = out_z
    reimport[name] = r
res["reimport"] = reimport
step("reimport", t0)
res["total_sec"] = round(time.time() - T0, 2)
print("P04_RESULT " + json.dumps(res, ensure_ascii=False))
