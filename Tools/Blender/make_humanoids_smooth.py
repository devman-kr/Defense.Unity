# SPDX-License-Identifier: GPL-3.0-or-later
# 이 폴더(Tools/Blender/)의 스크립트는 Blender Python API 를 쓰므로 GPL-3.0-or-later 다(LICENSE). 만든 모델(FBX · PNG)은 영향이 없다.
# P04 S2 — 치비 인간형 둘(해골 궁수 · 좀비)의 매끈 판: 같은 뼈대 14(Generic) · 거친 메시에 자동 가중치 · 서브디비전 2단계 · 동작 각 3
# 도우미는 make_enemies_smooth.py 에서 복사했다(S1 과 같은 이유 — 그 파일의 산출을 바꾸지 않으려고). 팔레트는 S1 과 같은 4x4 표 — 새 셀 없음.
# 실행: Blender -b --factory-startup -noaudio --python-exit-code 1 --python make_humanoids_smooth.py -- <출력 폴더> [S1 매끈 판 FBX 폴더]
import bpy, bmesh, sys, os, json, time, math, hashlib
from mathutils import Vector, Matrix

argv = sys.argv[sys.argv.index("--") + 1:]
out = argv[0]
s1_dir = argv[1] if len(argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "30_Assets", "Models", "P04_Smooth")
os.makedirs(out, exist_ok=True)
naive_dir = os.path.join(out, "naive")
os.makedirs(naive_dir, exist_ok=True)
res = {"blender": bpy.app.version_string, "steps": {}, "models": {}}
T0 = time.time()

SUBDIV_LEVELS = 2          # S1 과 같은 단계
EDGE_CREASE = 0.75         # 신발 바닥 · 활 끝 테두리(S1 의 탑 테두리 · 날개 외곽과 같은 값)
EDGE_CREASE_ANGLE = math.radians(50)
LIMB_SIDES = 8             # 팔다리 관의 둘레 정점 — 이웃 면 사이 45° 라 크리스 각(50°)에 걸리지 않는다


def step(name, t0):
    res["steps"][name] = round(time.time() - t0, 2)


bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.fps = 24

# ---------------------------------------------------------------- 팔레트(4x4) — make_enemies_smooth.py 와 같은 표. Unity 머티리얼 셋이 이 한 장을 공유하므로 셀을 바꾸지 않는다
PAL = [
    (0.46, 0.82, 0.40, 1), (0.26, 0.58, 0.30, 1), (0.98, 0.98, 0.95, 1), (0.10, 0.10, 0.14, 1),   # 0 슬라임 1 슬라임 진함 2 눈흰자 3 눈동자
    (0.96, 0.56, 0.62, 1), (0.48, 0.36, 0.68, 1), (0.30, 0.22, 0.46, 1), (0.95, 0.92, 0.80, 1),   # 4 볼 5 박쥐 몸 6 박쥐 날개 7 박쥐 귀 안
    (0.74, 0.74, 0.76, 1), (0.52, 0.52, 0.56, 1), (0.92, 0.50, 0.24, 1), (1.00, 0.86, 0.40, 1),   # 8 돌 밝음 9 돌 어두움 10 지붕 11 등불
    (0.55, 0.78, 0.36, 1), (0.40, 0.62, 0.28, 1), (0.55, 0.38, 0.24, 1), (1.00, 1.00, 1.00, 1),   # 12 잔디 13 잔디 진함 14 나무 15 흰색
]
C_SLIME, C_SLIME_DK, C_EYE, C_PUPIL, C_CHEEK, C_BAT, C_WING, C_EAR, C_STONE, C_STONE_DK, C_ROOF, C_LAMP, C_GRASS, C_GRASS_DK, C_WOOD, C_WHITE = range(16)
# 인간형이 빌려 쓰는 셀 — 뼈 = 눈흰자(따뜻한 흰색) · 눈 = 눈동자 · 활 = 나무 · 시위 = 흰색 · 좀비 피부 = 잔디 진함 · 셔츠 = 박쥐 몸 · 바지 = 나무
C_BONE, C_DOT, C_BOW, C_STRING = C_EYE, C_PUPIL, C_WOOD, C_WHITE
C_SKIN, C_SHIRT, C_PANTS = C_GRASS_DK, C_BAT, C_WOOD
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


def md5(path):
    with open(path, "rb") as f:
        return hashlib.md5(f.read()).hexdigest()


res["palette_md5"] = {"this": md5(pal_path), "s1_repo": md5(os.path.join(s1_dir, "Palette.png")) if os.path.exists(os.path.join(s1_dir, "Palette.png")) else None}


def cell_uv(cell):
    return ((cell % 4) + 0.5) / 4.0, ((cell // 4) + 0.5) / 4.0


def set_cells(obj, cell_of):
    """면마다 UV 를 팔레트 셀 중심 한 점으로 — cell_of 는 셀 번호 하나 또는 (면 중심, 면 번호) → 셀 번호 함수."""
    me = obj.data
    if not me.uv_layers:  # from_pydata 메시는 UV 층이 없다 — bmesh 가 만들면 이름이 Float2 라 join 때 UVMap 과 합쳐지지 않는다
        me.uv_layers.new(name="UVMap")
    bm = bmesh.new()
    bm.from_mesh(me)
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        cell = cell_of if isinstance(cell_of, int) else cell_of(obj.matrix_world @ f.calc_center_median(), f.index)
        u, v = cell_uv(cell)
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


def smooth(obj):
    active(obj)
    bpy.ops.object.shade_smooth()


def sphere(name, r, loc, cell, seg=10, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=r, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=True, scale=True)
    smooth(o)
    set_cells(o, cell)
    return o


def box(name, size, loc, cell):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = size
    bpy.ops.object.transform_apply(location=True, scale=True)
    smooth(o)
    set_cells(o, cell)
    return o


def torus(name, major, minor, loc, cell, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=12, minor_segments=4, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=True, scale=True)
    smooth(o)
    set_cells(o, cell)
    return o


def tube(name, pts, radii, cell_of, ref=(1, 0, 0), sides=LIMB_SIDES, ring_offsets=None):
    """경로(점 목록)를 따라 원 고리를 이어 붙인 관 — 양 끝은 n각 뚜껑. 팔다리는 관절 둘레에 고리를 촘촘히 넣어 자동 가중치의 접힘을 시험한다.
    ring_offsets[i] 가 있으면 i 번째 고리의 정점을 경로 방향으로 번갈아 ± 그만큼 밀어 톱니 테두리(찢어진 옷단)를 만든다."""
    ref = Vector(ref)
    verts, faces = [], []
    n = len(pts)
    for i, p in enumerate(pts):
        p = Vector(p)
        t = (Vector(pts[min(i + 1, n - 1)]) - Vector(pts[max(i - 1, 0)])).normalized()
        u = (ref - ref.dot(t) * t).normalized()
        w = t.cross(u)
        zig = ring_offsets[i] if ring_offsets else 0.0
        for k in range(sides):
            a = 2 * math.pi * k / sides
            verts.append(p + radii[i] * (math.cos(a) * u + math.sin(a) * w) + t * (zig if k % 2 == 0 else -zig))
    for i in range(n - 1):
        for k in range(sides):
            a, b = i * sides + k, i * sides + (k + 1) % sides
            faces.append([a, b, b + sides, a + sides])
    faces.append(list(reversed(range(sides))))
    faces.append([(n - 1) * sides + k for k in range(sides)])
    me = bpy.data.meshes.new(name)
    me.from_pydata([tuple(v) for v in verts], [], faces)
    me.validate()
    o = bpy.data.objects.new(name, me)
    scene.collection.objects.link(o)
    smooth(o)
    set_cells(o, cell_of)
    return o


def crease_edges(obj, pred):
    """pred(edge) 가 참인 모서리에 크리스 — 서브디비전 뒤에도 그 테두리가 남는다."""
    me = obj.data
    bm = bmesh.new()
    bm.from_mesh(me)
    layer = bm.edges.layers.float.get("crease_edge") or bm.edges.layers.float.new("crease_edge")
    n = 0
    for e in bm.edges:
        if pred(e):
            e[layer] = EDGE_CREASE
            n += 1
    bm.to_mesh(me)
    bm.free()
    return n


def sharp(e):
    return len(e.link_faces) == 2 and e.calc_face_angle(0.0) > EDGE_CREASE_ANGLE


def join(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    j = bpy.context.active_object
    j.name = name
    j.data.name = name
    return j


def tris(me):
    return sum(len(p.vertices) - 2 for p in me.polygons)


def add_subsurf(obj):
    """뼈대 모디파이어 뒤에 붙인다(S1 발견 1) — 자세는 거친 메시에서 굽히고 그 결과를 나눈다."""
    m = obj.modifiers.new("Subsurf", 'SUBSURF')
    m.levels = SUBDIV_LEVELS
    m.render_levels = SUBDIV_LEVELS
    return m


# ---------------------------------------------------------------- 1. 뼈대 — 두 모델이 같은 구성 · 같은 이름(Root · Hips · Spine · Head · 팔 3 × 2 · 다리 2 × 2 = 14)
# Blender 좌표: Z 위 · 앞(얼굴)은 -Y · 캐릭터의 왼쪽은 +X. 이름 끝 L = 캐릭터 왼쪽.
BONE_ORDER = ["Root", "Hips", "Spine", "Head",
              "UpperArmL", "ForeArmL", "HandL", "UpperArmR", "ForeArmR", "HandR",
              "ThighL", "ShinL", "ThighR", "ShinR"]


def build_rig(name, d):
    bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
    arm = bpy.context.active_object
    arm.name = name
    arm.data.name = name
    eb = arm.data.edit_bones
    root = eb[0]
    root.name = "Root"
    root.head, root.tail = (0, 0, 0), (0, 0, 0.2)
    root.use_deform = False  # 발 사이 바닥에 있어 자동 가중치가 정강이 · 발을 끌어가지 않게
    bones = {"Root": root}

    def bone(nm, head, tail, parent):
        b = eb.new(nm)
        b.head, b.tail = head, tail
        b.parent = bones[parent]
        bones[nm] = b
        return b

    bone("Hips", (0, 0, d["pelvis_z"]), (0, 0, d["spine_z"]), "Root")
    bone("Spine", (0, 0, d["spine_z"]), (0, 0, d["neck_z"]), "Hips")
    bone("Head", (0, 0, d["neck_z"]), (0, 0, d["head_top"]), "Spine")
    for sfx, sx in (("L", 1), ("R", -1)):
        x = sx * d["shoulder_x"]
        bone("UpperArm" + sfx, (x, 0, d["shoulder_z"]), (x, 0, d["elbow_z"]), "Spine")
        bone("ForeArm" + sfx, (x, 0, d["elbow_z"]), (x, 0, d["wrist_z"]), "UpperArm" + sfx)
        bone("Hand" + sfx, (x, 0, d["wrist_z"]), (x, 0, d["hand_end_z"]), "ForeArm" + sfx)
        hx = sx * d["hip_x"]
        bone("Thigh" + sfx, (hx, 0, d["hip_z"]), (hx, 0, d["knee_z"]), "Hips")
        bone("Shin" + sfx, (hx, 0, d["knee_z"]), (hx, 0, d["ankle_z"]), "Thigh" + sfx)
    for b in eb:
        b.use_connect = False
    bpy.ops.object.mode_set(mode='OBJECT')
    return arm


def limb_points(top, bottom, joint, x, y=0.0):
    """세로 관의 고리 높이 — 관절 둘레(±0.02 · ±0.045)에 고리를 몰아 넣는다."""
    zs = sorted({round(z, 4) for z in (top, (top + joint) / 2 + 0.03, joint + 0.045, joint + 0.02, joint, joint - 0.02, joint - 0.045, (joint + bottom) / 2 - 0.02, bottom)}, reverse=True)
    return [(x, y, z) for z in zs]


def nearest_bone(arm, p):
    best, best_d = None, 1e9
    for b in arm.data.bones:
        if not b.use_deform:
            continue
        h, t = b.head_local, b.tail_local
        seg = t - h
        s = max(0.0, min(1.0, (p - h).dot(seg) / seg.length_squared))
        dd = (p - (h + seg * s)).length
        if dd < best_d:
            best, best_d = b.name, dd
    return best


def skin_and_subsurf(body, arm, extra=()):
    """거친 메시에 자동 가중치(뼈 열 — bone heat). 가중치 합이 0 으로 남은 정점은 떨어진 조각(섬) 단위로 묶어
    섬 중심에서 가장 가까운 뼈에 1.0 으로 메우고 수를 남긴다 — 정점마다 가까운 뼈를 고르면 갈비 바깥쪽이 위팔에 붙는다(실측)."""
    bpy.ops.object.select_all(action='DESELECT')
    body.select_set(True)
    arm.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    me = body.data
    unweighted = {v.index for v in me.vertices if sum(g.weight for g in v.groups) <= 0.0}
    links = {i: [] for i in unweighted}
    for e in me.edges:
        a, b = e.vertices
        if a in unweighted and b in unweighted:
            links[a].append(b)
            links[b].append(a)
    filled, seen = {}, set()
    for start in sorted(unweighted):
        if start in seen:
            continue
        island, stack = [], [start]
        seen.add(start)
        while stack:
            i = stack.pop()
            island.append(i)
            for j in links[i]:
                if j not in seen:
                    seen.add(j)
                    stack.append(j)
        center = sum((me.vertices[i].co for i in island), Vector()) / len(island)
        best = nearest_bone(arm, center)
        vg = body.vertex_groups.get(best) or body.vertex_groups.new(name=best)
        vg.add(island, 1.0, 'REPLACE')
        filled.setdefault(best, []).append(len(island))
    per_bone = {g.name: sum(1 for v in body.data.vertices for gg in v.groups if gg.group == g.index and gg.weight > 0.0) for g in body.vertex_groups}
    add_subsurf(body)
    for o in extra:
        add_subsurf(o)
    return {"method": "ARMATURE_AUTO", "verts": len(body.data.vertices), "unweighted_filled": len(unweighted),
            "filled_islands_by_bone": filled, "verts_per_group": per_bone}


# ---------------------------------------------------------------- 2. 해골 궁수 — 흰 뼈 · 갈비 셋 · 큰 머리 · 큰 손발 · 점 눈 둘 · 활(별도 메시, 왼손 뼈 100 %)
ARCHER = dict(shoulder_x=0.27, shoulder_z=0.96, elbow_z=0.76, wrist_z=0.56, hand_end_z=0.44, hand_z=0.50,
              hip_x=0.11, hip_z=0.64, knee_z=0.38, ankle_z=0.10, pelvis_z=0.56, spine_z=0.68, neck_z=1.00, head_top=1.62,
              arm_r=0.048, leg_r=0.05, hand_r=0.095)
BOW_INSET = 0.075          # 활 평면을 손 중심에서 몸 쪽(-X)으로 — 시위가 아래팔을 지나가지 않게(아래팔 반지름 0.048)
BOW_HALF = 0.36            # 활 끝까지(쉼 자세에서 Y 축 — 팔을 앞으로 뻗으면 세로가 된다)
BOW_BRACE = 0.14           # 손잡이에서 시위까지(쉼 자세 +Z = 쏠 때 몸 쪽)


def build_archer():
    d = ARCHER
    parts = [
        sphere("Skull", 0.31, (0, 0, 1.30), C_BONE, seg=12, rings=8, scale=(1, 0.95, 1)),
        tube("SpineColumn", [(0, 0, 1.02), (0, 0, 0.88), (0, 0, 0.74), (0, 0, 0.60)], [0.045] * 4, C_BONE),
        sphere("Pelvis", 0.15, (0, 0, 0.62), C_BONE, seg=10, rings=6, scale=(1.2, 0.8, 0.55)),
    ]
    for z, r in ((0.93, 0.17), (0.85, 0.16), (0.77, 0.13)):
        parts.append(torus("Rib", r, 0.025, (0, 0, z), C_BONE, scale=(1, 0.8, 1)))
    for sx in (-1, 1):
        parts.append(sphere("EyeDot", 0.065, (sx * 0.11, -0.275, 1.28), C_DOT, seg=8, rings=6, scale=(1, 0.45, 1.15)))
        x = sx * d["shoulder_x"]
        parts.append(tube("Arm", limb_points(0.98, d["wrist_z"], d["elbow_z"], x), [d["arm_r"]] * 9, C_BONE))
        parts.append(sphere("Hand", d["hand_r"], (x, 0, d["hand_z"]), C_BONE, seg=10, rings=6))
        hx = sx * d["hip_x"]
        parts.append(tube("Leg", limb_points(0.66, d["ankle_z"] - 0.03, d["knee_z"], hx), [d["leg_r"]] * 9, C_BONE))
        foot = box("Foot", (0.13, 0.21, 0.09), (hx, -0.05, 0.055), C_BONE)
        crease_edges(foot, lambda e: all(v.co.z < 0.02 for v in e.verts))  # 신발 바닥 테두리
        parts.append(foot)
    body = join(parts, "SkeletonArcher")
    arm = build_rig("SkeletonArcherRig", d)
    body.parent = arm

    # 활 — 손잡이를 가운데로 한 포물선 활대 + 곧은 시위. 쉼 자세(팔이 아래)에서 활대는 Y 축, 팔을 앞으로 뻗으면 세로가 된다
    gx, gz = d["shoulder_x"] - BOW_INSET, d["hand_z"]
    arc = [(gx, y, gz + BOW_BRACE * (y / BOW_HALF) ** 2) for y in [BOW_HALF * (i / 4.0 - 1.0) for i in range(9)]]
    limb = tube("BowLimb", arc, [0.012, 0.016, 0.02, 0.022, 0.024, 0.022, 0.02, 0.016, 0.012], C_BOW)
    string = tube("BowString", [arc[0], (gx, 0.0, gz + BOW_BRACE), arc[-1]], [0.006] * 3, C_STRING)
    bow = join([limb, string], "SkeletonArcherBow")
    crease_edges(bow, sharp)  # 활대 · 시위 끝 뚜껑 테두리 — 서브디비전이 끝을 끌어당겨 활이 짧아지지 않게
    bow.parent = arm
    bow.modifiers.new("Armature", 'ARMATURE').object = arm
    bow.vertex_groups.new(name="HandL").add([v.index for v in bow.data.vertices], 1.0, 'REPLACE')
    weights = skin_and_subsurf(body, arm, extra=(bow,))
    return body, bow, arm, weights


# ---------------------------------------------------------------- 3. 좀비 — 초록 피부 · 찢어진 셔츠(톱니 옷단 · 구멍) · 바지 · 큰 손발 · 점 눈 둘
ZOMBIE = dict(shoulder_x=0.32, shoulder_z=0.97, elbow_z=0.77, wrist_z=0.57, hand_end_z=0.45, hand_z=0.51,
              hip_x=0.12, hip_z=0.64, knee_z=0.38, ankle_z=0.10, pelvis_z=0.56, spine_z=0.68, neck_z=1.00, head_top=1.63,
              arm_r=0.06, leg_r=0.066, hand_r=0.10)
SHIRT_HEM_Z = 0.66         # 셔츠 아랫단(이 아래는 바지)
SLEEVE_Z = 0.84            # 소매 끝(이 위는 셔츠)
SHORTS_Z = 0.42            # 바짓단(이 아래는 맨 정강이)


def build_zombie():
    d = ZOMBIE
    torso_z = [1.03, 0.98, 0.88, 0.78, SHIRT_HEM_Z + 0.03, SHIRT_HEM_Z, SHIRT_HEM_Z - 0.03, 0.58]
    torso_r = [0.13, 0.21, 0.235, 0.235, 0.225, 0.22, 0.21, 0.17]
    holes = {2 * LIMB_SIDES + 1, 3 * LIMB_SIDES + 5}  # 셔츠 구멍 — 고리 사이 면 둘을 피부색으로

    def torso_cell(c, i):
        if i in holes:
            return C_SKIN
        return C_SHIRT if c.z > SHIRT_HEM_Z else C_PANTS

    zig = [0.0] * len(torso_z)
    zig[torso_z.index(SHIRT_HEM_Z)] = 0.018  # 옷단 고리를 톱니로
    parts = [
        sphere("Head", 0.32, (0, 0, 1.31), C_SKIN, seg=12, rings=8),
        tube("Torso", [(0, 0, z) for z in torso_z], torso_r, torso_cell, ring_offsets=zig),
    ]
    for sx in (-1, 1):
        parts.append(sphere("EyeDot", 0.045, (sx * 0.11, -0.295, 1.34), C_DOT, seg=8, rings=6, scale=(1, 0.5, 1.1)))
        x = sx * d["shoulder_x"]
        parts.append(tube("Arm", limb_points(0.99, d["wrist_z"], d["elbow_z"], x), [d["arm_r"]] * 9,
                          lambda c, i: C_SHIRT if c.z > SLEEVE_Z else C_SKIN))
        parts.append(sphere("Hand", d["hand_r"], (x, 0, d["hand_z"]), C_SKIN, seg=10, rings=6))
        hx = sx * d["hip_x"]
        parts.append(tube("Leg", limb_points(0.66, d["ankle_z"] - 0.03, d["knee_z"], hx), [d["leg_r"]] * 9,
                          lambda c, i: C_PANTS if c.z > SHORTS_Z else C_SKIN))
        foot = box("Foot", (0.15, 0.23, 0.09), (hx, -0.05, 0.055), C_SKIN)
        crease_edges(foot, lambda e: all(v.co.z < 0.02 for v in e.verts))
        parts.append(foot)
    body = join(parts, "Zombie")
    arm = build_rig("ZombieRig", d)
    body.parent = arm
    weights = skin_and_subsurf(body, arm)
    return body, arm, weights


t0 = time.time()
archer, bow, archer_arm, archer_weights = build_archer()
zombie, zombie_arm, zombie_weights = build_zombie()
res["models"]["skeleton_archer"] = {"weights": archer_weights, "bow_weight": "HandL 1.0 (vertex group, Armature modifier) — 정점 %d" % len(bow.data.vertices)}
res["models"]["zombie"] = {"weights": zombie_weights}
res["modifier_order"] = {o.name: [m.type for m in o.modifiers] for o in (archer, bow, zombie)}
step("models", t0)

# ---------------------------------------------------------------- 4. 동작 — 자세를 「뼈 방향 · 손목 목표 · 발 목표」로 적고, 부모부터 차례로 포즈 행렬을 맞춰 쿼터니언 키를 넣는다
t0 = time.time()
FWD, UP = Vector((0, -1, 0)), Vector((0, 0, 1))


def limb(fwd_deg, out_deg=0.0, side=1):
    """아래를 향한 팔다리를 앞으로 fwd_deg 만큼 들고(음수면 뒤로) 바깥(side +1 = +X)으로 out_deg 벌린 방향."""
    a = math.radians(fwd_deg)
    v = Vector((0, -math.sin(a), -math.cos(a)))
    return Matrix.Rotation(-math.radians(out_deg) * side, 3, 'Y') @ v


def carried(arm, name):
    """부모의 지금 포즈에 실려 간 쉼 자세 행렬(아마추어 공간) — 이 뼈의 회전이 0 일 때의 행렬."""
    b = arm.data.bones[name]
    if b.parent is None:
        return b.matrix_local.copy()
    return arm.pose.bones[b.parent.name].matrix @ b.parent.matrix_local.inverted() @ b.matrix_local


def set_bone(arm, name, rot3, head):
    arm.pose.bones[name].matrix = Matrix.Translation(head) @ rot3.to_4x4()
    bpy.context.view_layer.update()


def aim(arm, name, direction):
    m = carried(arm, name)
    rot = m.to_3x3()
    q = rot.col[1].normalized().rotation_difference(direction.normalized())  # 뼈 방향은 뼈 기준 Y 축
    set_bone(arm, name, q.to_matrix() @ rot, m.translation)


IK_STATS = {"clamped": 0, "solved": 0}


def two_bone(arm, upper, lower, target, pole):
    """두 뼈 IK — 위 뼈 머리에서 target(아래 뼈 꼬리)까지, 굽는 쪽은 pole 방향. 닿지 않으면 쭉 펴고 수를 센다."""
    h = carried(arm, upper).translation
    l1, l2 = arm.data.bones[upper].length, arm.data.bones[lower].length
    e = target - h
    d = e.length
    if d > 0.999 * (l1 + l2):
        IK_STATS["clamped"] += 1
        IK_STATS.setdefault("clamped_by_bone", {})
        IK_STATS["clamped_by_bone"][upper] = IK_STATS["clamped_by_bone"].get(upper, 0) + 1
        d = 0.999 * (l1 + l2)
    IK_STATS["solved"] += 1
    e.normalize()
    ca = max(-1.0, min(1.0, (l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)))
    alpha = math.acos(ca)
    p = (pole - pole.dot(e) * e).normalized()
    upper_dir = math.cos(alpha) * e + math.sin(alpha) * p
    aim(arm, upper, upper_dir)
    knee = h + l1 * upper_dir
    aim(arm, lower, (h + e * d) - knee)


def apply_pose(arm, spec):
    """spec: 뼈 이름 → {rot: [(축, 도)], off: Vector, dir: Vector, ik: (아래 뼈, 목표 또는 함수, pole)} — BONE_ORDER(부모 먼저)로 적용."""
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
        pb.scale = (1, 1, 1)
    bpy.context.view_layer.update()
    done = set()
    for name in BONE_ORDER:
        s = spec.get(name)
        if not s or name in done:
            continue
        if "ik" in s:
            lower, target, pole = s["ik"]
            target = target(arm) if callable(target) else target
            two_bone(arm, name, lower, Vector(target), Vector(pole))
            done.add(lower)
            continue
        m = carried(arm, name)
        rot, head = m.to_3x3(), m.translation.copy()
        for axis, deg in s.get("rot", ()):
            rot = Matrix.Rotation(math.radians(deg), 3, axis) @ rot
        if "dir" in s:
            rot = rot.col[1].normalized().rotation_difference(s["dir"].normalized()).to_matrix() @ rot
        set_bone(arm, name, rot, head + s.get("off", Vector()))


def key_pose(arm, frame, prev):
    for name in BONE_ORDER:
        pb = arm.pose.bones[name]
        q = pb.rotation_quaternion.copy()
        if name in prev and prev[name].dot(q) < 0.0:  # 이웃 키와 같은 반구 — 성분 보간이 먼 길로 돌지 않게
            q.negate()
            pb.rotation_quaternion = q
        prev[name] = q
        pb.keyframe_insert(data_path="rotation_quaternion", frame=frame)
        if name in ("Root", "Hips"):
            pb.keyframe_insert(data_path="location", frame=frame)


def reset_pose(arm):
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
        pb.scale = (1, 1, 1)


def make_action(arm, name, frames):
    """frames: [(프레임, spec)] — 키 프레임마다 자세를 맞추고 14 뼈 모두에 키."""
    arm.animation_data_create()
    arm.animation_data.action = None
    prev = {}
    for f, spec in frames:
        apply_pose(arm, spec)
        key_pose(arm, f, prev)
    act = arm.animation_data.action
    act.name = name
    act.use_fake_user = True
    arm.animation_data.action = None
    reset_pose(arm)
    return act


def assign(arm, act):
    reset_pose(arm)
    arm.animation_data_create()
    arm.animation_data.action = act
    try:
        if act is not None and act.slots and arm.animation_data.action_slot is None:
            arm.animation_data.action_slot = act.slots[0]
    except AttributeError:
        pass


def ankle(d, side):
    return Vector((side * d["hip_x"], 0, d["ankle_z"]))


def planted(d, spec, feet=None):
    """다리를 IK 로 — 발목 목표(기본은 쉼 자세 자리). 엉덩이가 내려가면 무릎이 굽어 발이 바닥에 남는다."""
    feet = feet or {}
    for sfx, sx in (("L", 1), ("R", -1)):
        spec["Thigh" + sfx] = {"ik": ("Shin" + sfx, feet.get(sfx, ankle(d, sx)), FWD + Vector((sx * 0.15, 0, 0)))}
    return spec


def walk_foot(d, side, pp, stride, lift):
    """제자리 걷기 발목 경로 — 디딤(pp 0~0.5)은 앞에서 뒤로 미끄러지고, 흔듦(0.5~1)은 들려서 앞으로."""
    a = ankle(d, side)
    if pp < 0.5:
        return a + Vector((0, -stride + 2 * stride * (pp / 0.5), 0))
    q = (pp - 0.5) / 0.5
    return a + Vector((0, stride * math.cos(math.pi * q), lift * math.sin(math.pi * q)))


# --- 해골 궁수
A = ARCHER


def archer_idle(b):
    """숨: b 0 → 1 에 엉덩이가 1.5 cm 내려가고 가슴이 펴지며 고개가 끄덕인다. 활 팔은 앞으로 반쯤 들고 있다."""
    return planted(A, {
        "Hips": {"off": Vector((0, 0, -0.015 * b))},
        "Spine": {"rot": [('X', -1.5 * b)]},
        "Head": {"rot": [('X', 3 * b)]},
        "UpperArmL": {"dir": limb(15 + 3 * b, 8, 1)}, "ForeArmL": {"dir": limb(60 + 4 * b, 5, 1)},
        "UpperArmR": {"dir": limb(5 + 2 * b, 10, -1)}, "ForeArmR": {"dir": limb(18 + 4 * b, 8, -1)},
    })


def archer_walk(p):
    sw = math.cos(2 * math.pi * p)  # +1: 왼다리 앞 · 오른팔 앞
    spec = {
        "Hips": {"off": Vector((0, 0, -0.012 - 0.012 * math.cos(4 * math.pi * p))), "rot": [('Z', 5 * sw), ('Y', 2 * math.sin(2 * math.pi * p))]},
        "Spine": {"rot": [('Z', -4 * sw)]},
        "Head": {"rot": [('X', 2 * math.cos(4 * math.pi * p))]},
        "UpperArmL": {"dir": limb(15 - 8 * sw, 8, 1)}, "ForeArmL": {"dir": limb(55 - 8 * sw, 5, 1)},
        "UpperArmR": {"dir": limb(5 + 20 * sw, 8, -1)}, "ForeArmR": {"dir": limb(30 + 20 * sw, 6, -1)},
    }
    return planted(A, spec, {"L": walk_foot(A, 1, p % 1.0, 0.10, 0.08), "R": walk_foot(A, -1, (p + 0.5) % 1.0, 0.10, 0.08)})


def bow_point(arm, rest_point):
    """쉼 자세에서 활 위의 한 점이 지금 왼손 포즈에서 어디 있는가(활은 왼손 뼈에 100 %)."""
    b = arm.data.bones["HandL"]
    return arm.pose.bones["HandL"].matrix @ b.matrix_local.inverted() @ Vector(rest_point)


BOW_GRIP = Vector((ARCHER["shoulder_x"] - BOW_INSET, 0, ARCHER["hand_z"]))
BOW_STRING_MID = BOW_GRIP + Vector((0, 0, BOW_BRACE))
RIGHT_ELBOW_POLE = (-1.0, 0.6, -0.2)   # 시위를 당기는 팔꿈치는 바깥 · 뒤로


def string_target(draw, extra=(0, 0, 0)):
    """오른 손목 목표 = 시위 가운데에서 활 → 몸 방향으로 draw 만큼 + extra."""
    def f(arm):
        grip, mid = bow_point(arm, BOW_GRIP), bow_point(arm, BOW_STRING_MID)
        back = (mid - grip).normalized()
        return mid + back * draw + Vector(extra)
    return f


def left_bow_target(arm):
    """활 팔 손목 목표 — 왼어깨에서 앞 · 몸 가운데 쪽으로 0.36(팔 길이 0.40 — 팔꿈치가 조금 굽는다).
    활이 몸 가운데 앞에 와야 짧은 치비 오른팔(어깨에서 손목 0.40)이 시위에 닿는다."""
    shoulder = carried(arm, "UpperArmL").translation
    return shoulder + 0.36 * Vector((-0.55, -1.0, 0.10)).normalized()


def archer_shoot(stage):
    """stage: ready · raise · nock · draw · hold · release · follow."""
    if stage == "ready":
        return archer_idle(0)
    turn = {"raise": 0.6, "nock": 1.0, "draw": 1.0, "hold": 1.0, "release": 1.0, "follow": 0.8}[stage]
    spec = {
        "Hips": {"rot": [('Z', -5 * turn)], "off": Vector((0, 0, -0.01 * turn))},
        "Spine": {"rot": [('Z', -10 * turn), ('X', -2 * turn)]},
        "Head": {"rot": [('Z', 12 * turn)]},
        "UpperArmL": {"ik": ("ForeArmL", left_bow_target, (1.0, 0.2, -0.6))},
        # 손 뼈를 과녁 쪽으로 꺾는다 — 손 뼈를 아래팔에 맞춰 두면 화살 방향이 아래팔을 따라 시위가 왼어깨 쪽으로 당겨져 오른손이 닿지 않는다(첫 실행 IK 펴짐)
        "HandL": {"dir": FWD + Vector((0, 0, 0.05))},
    }
    right = {"raise": string_target(0.0, (-0.04, 0.04, -0.06)), "nock": string_target(0.0, (-0.02, 0.0, -0.02)),
             "draw": string_target(0.07, (-0.02, 0.0, -0.02)), "hold": string_target(0.075, (-0.02, 0.0, -0.015)),
             "release": string_target(0.075, (-0.12, 0.10, 0.06)), "follow": string_target(0.05, (-0.14, 0.12, -0.04))}[stage]
    spec["UpperArmR"] = {"ik": ("ForeArmR", right, RIGHT_ELBOW_POLE)}
    return planted(A, spec)


# --- 좀비
Z = ZOMBIE


def zombie_arms(fl, fr, bend=6):
    return {"UpperArmL": {"dir": limb(fl, 6, 1)}, "ForeArmL": {"dir": limb(fl + bend, 3, 1)},
            "UpperArmR": {"dir": limb(fr, 6, -1)}, "ForeArmR": {"dir": limb(fr + bend, 3, -1)}}


def zombie_idle(b):
    """b 0 → 1: 앞으로 숙인 몸이 좌우로 흔들리고 고개가 꺾이며 팔이 번갈아 오르내린다."""
    s = 1 - 2 * b
    spec = {
        "Hips": {"off": Vector((0, 0, -0.012 * b)), "rot": [('Y', 3 * s)]},
        "Spine": {"rot": [('X', 12 + 2 * b), ('Y', -2 * s)]},
        "Head": {"rot": [('Y', 10 + 4 * b), ('X', 6)]},
    }
    spec.update(zombie_arms(80 + 4 * b, 84 - 4 * b))
    return planted(Z, spec)


def zombie_walk(p):
    """절뚝 — 왼다리는 보통 걸음, 오른다리는 끌듯 짧고 낮다. 오른다리로 디딜 때 몸이 더 내려가고 오른쪽으로 기운다."""
    dip = max(0.0, -math.sin(2 * math.pi * p))  # 오른발 디딤(p 0.5~1)
    spec = {
        "Hips": {"off": Vector((0, 0, -0.01 - 0.025 * dip)), "rot": [('Y', 6 * math.sin(2 * math.pi * p)), ('Z', 4 * math.cos(2 * math.pi * p))]},
        "Spine": {"rot": [('X', 14), ('Y', -4 * math.sin(2 * math.pi * p))]},
        "Head": {"rot": [('Y', 10), ('X', 6 + 3 * dip)]},
    }
    spec.update(zombie_arms(78 + 4 * math.sin(4 * math.pi * p), 82 - 4 * math.sin(4 * math.pi * p)))
    return planted(Z, spec, {"L": walk_foot(Z, 1, p % 1.0, 0.10, 0.08), "R": walk_foot(Z, -1, (p + 0.5) % 1.0, 0.06, 0.015)})


def zombie_attack(stage):
    """ready → raise(팔을 머리 위로, 팔꿈치 굽힘) → swing(내리치며 숙이고 무릎 굽힘)."""
    if stage == "ready":
        return zombie_idle(0)
    if stage in ("raise", "top"):
        k = 1.0 if stage == "top" else 0.0
        spec = {"Spine": {"rot": [('X', -6 - 2 * k)]}, "Head": {"rot": [('X', -5), ('Y', 8)]}}
        spec.update(zombie_arms(160 + 5 * k, 160 + 5 * k, bend=40 + 5 * k))
        return planted(Z, spec)
    spec = {"Hips": {"off": Vector((0, 0, -0.04))}, "Spine": {"rot": [('X', 28)]}, "Head": {"rot": [('X', 10), ('Y', 8)]}}
    spec.update(zombie_arms(55, 55, bend=8))
    return planted(Z, spec)


archer_acts = {
    "Idle": make_action(archer_arm, "Idle", [(1, archer_idle(0)), (13, archer_idle(1)), (25, archer_idle(0))]),
    "Walk": make_action(archer_arm, "Walk", [(f, archer_walk((f - 1) / 24.0)) for f in range(1, 26, 3)]),
    "Shoot": make_action(archer_arm, "Shoot", [(1, archer_shoot("ready")), (6, archer_shoot("raise")), (10, archer_shoot("nock")),
                                               (16, archer_shoot("draw")), (20, archer_shoot("hold")), (21, archer_shoot("release")),
                                               (24, archer_shoot("follow")), (31, archer_shoot("ready"))]),
}
zombie_acts = {
    "Idle": make_action(zombie_arm, "Idle", [(1, zombie_idle(0)), (13, zombie_idle(1)), (25, zombie_idle(0))]),
    "Walk": make_action(zombie_arm, "Walk", [(f, zombie_walk((f - 1) / 32.0)) for f in range(1, 34, 4)]),
    "Attack": make_action(zombie_arm, "Attack", [(1, zombie_attack("ready")), (7, zombie_attack("raise")), (10, zombie_attack("top")),
                                                 (13, zombie_attack("swing")), (21, zombie_attack("ready"))]),
}
res["actions"] = {a.name: [round(x, 1) for x in a.frame_range] for a in bpy.data.actions}
res["ik"] = dict(IK_STATS)
step("animation", t0)

# ---------------------------------------------------------------- 5. 실측 — 관절 접힘 각(최대) · 그때 메시 두께, Walk 첫 · 끝 프레임, 활이 손을 따라가는가
t0 = time.time()
JOINTS = {"ElbowL": ("UpperArmL", "ForeArmL"), "ElbowR": ("UpperArmR", "ForeArmR"), "KneeL": ("ThighL", "ShinL"), "KneeR": ("ThighR", "ShinR")}


def bone_dir(arm, name):
    return arm.pose.bones[name].matrix.to_3x3().col[1].normalized()


def fold_deg(arm, upper, lower):
    return math.degrees(bone_dir(arm, upper).angle(bone_dir(arm, lower)))


def evaluated(obj):
    dg = bpy.context.evaluated_depsgraph_get()
    ev = obj.evaluated_get(dg)
    m = ev.to_mesh(preserve_all_data_layers=True, depsgraph=dg)
    return ev, m


def ring_indices(body, arm, upper, lower, radius):
    """쉼 자세 평가 메시(서브디비전 2단계)에서 관절 둘레 고리 — 관절 축 위치 ±0.025 · 축에서 반지름 1.6 배 안 · 두 뼈 가중치 합 > 0.5."""
    assign(arm, None)
    scene.frame_set(1)
    ev, m = evaluated(body)
    j = arm.data.bones[lower].head_local
    axis = (arm.data.bones[upper].tail_local - arm.data.bones[upper].head_local).normalized()
    gi = {g.name: g.index for g in body.vertex_groups}
    idx = []
    for v in m.vertices:
        rel = v.co - j
        along = rel.dot(axis)
        radial = (rel - along * axis).length
        w = sum(g.weight for g in v.groups if g.group in (gi.get(upper), gi.get(lower)))
        if abs(along) < 0.025 and radial < radius * 1.6 and w > 0.5:
            idx.append(v.index)
    rest = [m.vertices[i].co.copy() for i in idx]
    ev.to_mesh_clear()
    if not rest:
        return idx, 0.0
    xs, ys = [p.x for p in rest], [p.y for p in rest]
    return idx, round(((max(xs) - min(xs)) + (max(ys) - min(ys))) / 2, 4)


def fold_thickness(body, arm, act, frame, upper, lower, idx):
    """접힌 프레임에서 고리 정점을 관절 안쪽 이등분 방향으로 투영한 폭 — 쉼 자세 지름과 비교한다(찌그러지면 작아진다)."""
    assign(arm, act)
    scene.frame_set(frame)
    u, l = bone_dir(arm, upper), bone_dir(arm, lower)
    j = arm.matrix_world @ arm.pose.bones[lower].head
    b = (l - u)
    if b.length < 1e-4:
        return None
    b.normalize()
    ev, m = evaluated(body)
    proj = [(ev.matrix_world @ m.vertices[i].co - j).dot(b) for i in idx]
    ev.to_mesh_clear()
    return round(max(proj) - min(proj), 4) if proj else None


def measure(key, body, arm, acts, d):
    r = {"joints": {}}
    for jn, (up, lo) in JOINTS.items():
        best = (-1.0, None, None)
        for an, act in acts.items():
            assign(arm, act)
            f0, f1 = (int(x) for x in act.frame_range)
            for f in range(f0, f1 + 1):
                scene.frame_set(f)
                a = fold_deg(arm, up, lo)
                if a > best[0]:
                    best = (a, an, f)
        radius = d["arm_r"] if jn.startswith("Elbow") else d["leg_r"]
        idx, rest_t = ring_indices(body, arm, up, lo, radius)
        fold_t = fold_thickness(body, arm, acts[best[1]], best[2], up, lo, idx)
        r["joints"][jn] = {"max_fold_deg": round(best[0], 1), "action": best[1], "frame": best[2], "ring_verts": len(idx),
                           "thickness_rest_m": rest_t, "thickness_fold_m": fold_t,
                           "ratio": round(fold_t / rest_t, 3) if fold_t and rest_t else None}
    walk = acts["Walk"]
    f0, f1 = (int(x) for x in walk.frame_range)
    mats = []
    for f in (f0, f1):
        assign(arm, walk)
        scene.frame_set(f)
        mats.append([pb.matrix.copy() for pb in arm.pose.bones])
    r["walk_first_last_max_diff"] = round(max(abs(a[i][j] - b[i][j]) for a, b in zip(*mats) for i in range(4) for j in range(4)), 6)
    res["models"][key].update(r)


measure("skeleton_archer", archer, archer_arm, archer_acts, ARCHER)
measure("zombie", zombie, zombie_arm, zombie_acts, ZOMBIE)


def bow_follow(frames):
    """Shoot 프레임마다 활 정점 중심과 왼손 뼈 머리 사이 거리(활이 손에 붙어 있으면 같다)."""
    out_d = {}
    for f in frames:
        assign(archer_arm, archer_acts["Shoot"])
        scene.frame_set(f)
        ev, m = evaluated(bow)
        c = sum((ev.matrix_world @ v.co for v in m.vertices), Vector()) / len(m.vertices)
        ev.to_mesh_clear()
        h = archer_arm.matrix_world @ archer_arm.pose.bones["HandL"].head
        out_d[f] = round((c - h).length, 4)
    return out_d


res["models"]["skeleton_archer"]["bow_centroid_to_handL_m"] = bow_follow((1, 6, 16, 21, 31))
for a in (archer_arm, zombie_arm):
    assign(a, None)
scene.frame_set(1)
step("measure", t0)

# ---------------------------------------------------------------- 6. 단계별 수 — 서브디비전 0 · 1 · 2 의 삼각형 · 정점(쉼 자세)
t0 = time.time()


def uv_off_center(me):
    worst = 0.0
    for dd in me.uv_layers.active.data:
        u, v = dd.uv
        cu, cv = (math.floor(u * 4) + 0.5) / 4, (math.floor(v * 4) + 0.5) / 4
        worst = max(worst, abs(u - cu), abs(v - cv))
    return round(worst, 6)


def level_counts(o):
    sub = o.modifiers["Subsurf"]
    levels = {}
    for lv in (0, 1, 2):
        sub.levels = lv
        ev, m = evaluated(o)
        levels[lv] = {"tris": tris(m), "verts": len(m.vertices), "uv_off_center": uv_off_center(m)}
        ev.to_mesh_clear()
    sub.levels = SUBDIV_LEVELS
    return levels


res["models"]["skeleton_archer"]["levels"] = level_counts(archer)
res["models"]["skeleton_archer"]["bow_levels"] = level_counts(bow)
res["models"]["zombie"]["levels"] = level_counts(zombie)
for key, arm in (("skeleton_archer", archer_arm), ("zombie", zombie_arm)):
    res["models"][key]["bones"] = [b.name for b in arm.data.bones]
    res["models"][key]["deform_bones"] = sum(1 for b in arm.data.bones if b.use_deform)
step("counts", t0)

# ---------------------------------------------------------------- 7. 저장 · FBX — 먼저 그대로(naive) 내보내 클립 이름 함정을 남기고, 모델 표지 키를 더해 다시(정본)
t0 = time.time()
layout = {archer_arm: (1.2, 0, 0), zombie_arm: (2.8, 0, 0)}
FBX_OPTIONS = dict(use_selection=True, object_types={'ARMATURE', 'MESH'}, use_mesh_modifiers=True, use_mesh_modifiers_render=True,
                   use_subsurf=False, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                   bake_space_transform=True, add_leaf_bones=False, mesh_smooth_type='FACE',
                   path_mode='STRIP', bake_anim=True, bake_anim_use_all_actions=True,
                   bake_anim_use_nla_strips=False, bake_anim_simplify_factor=0.0)
res["fbx_options"] = {k: (sorted(v) if isinstance(v, set) else v) for k, v in FBX_OPTIONS.items()}
EXPORTS = (("SkeletonArcher", [archer, bow, archer_arm], archer_arm, archer_acts), ("Zombie", [zombie, zombie_arm], zombie_arm, zombie_acts))


def export_all(folder):
    sizes = {}
    for name, objs, _, _ in EXPORTS:
        bpy.ops.object.select_all(action='DESELECT')
        for o in objs:
            o.select_set(True)
        p = os.path.join(folder, f"{name}.fbx")
        bpy.ops.export_scene.fbx(filepath=p, **FBX_OPTIONS)
        sizes[os.path.basename(p)] = os.path.getsize(p)
    return sizes


res["fbx_bytes_naive"] = export_all(naive_dir)
# 「모든 동작」 내보내기는 동작의 F-Curve 경로가 그 오브젝트에서 풀리면 넣는다 — 두 뼈대의 뼈 이름이 같아 상대 모델의 동작까지 들어간다.
# 동작마다 모델 이름 사용자 속성(아마추어 오브젝트의 ["SkeletonArcherRig"] 등)에 키를 하나 넣으면 다른 아마추어에서는 경로가 풀리지 않아 빠진다.
# 사용자 속성은 FBX 에 쓰지 않는다(use_custom_props 끔 — 기본).
for _, _, arm, acts in EXPORTS:
    arm[arm.name] = 0.0
    for act in acts.values():
        assign(arm, act)
        arm.keyframe_insert(data_path=f'["{arm.name}"]', frame=act.frame_range[0])
    assign(arm, None)
res["fbx_bytes"] = export_all(out)
for o, loc in layout.items():
    o.location = loc
# .blend 는 표지 키를 넣은 뒤에 저장한다 — 그 전 상태로 저장하면 .blend 에서 다시 내보낼 때 상대 모델 동작이 섞인다
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "p04_humanoids_smooth.blend"))
scene.frame_set(1)
step("export", t0)

# ---------------------------------------------------------------- 8. 미리보기 렌더(EEVEE) — 라인업(S1 매끈 판 셋 + 인간형 둘) · 동작 자세 6 · 궁수 근접
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

# S1 매끈 판을 저장소 FBX 에서 가져온다(스크립트 산출을 다시 만들지 않고 실제로 들어간 판을 쓴다)
s1_objs = []
for nm, loc in (("Slime", (-3.6, 0, 0)), ("Bat", (-2.2, 0, 1.2)), ("Tower", (-0.6, 0, 0))):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=os.path.join(s1_dir, f"{nm}.fbx"))
    new = [o for o in bpy.data.objects if o not in before]
    for o in new:
        if o.type == 'ARMATURE' and o.animation_data:
            o.animation_data.action = None
            for pb in o.pose.bones:
                pb.location, pb.scale = (0, 0, 0), (1, 1, 1)
                pb.rotation_quaternion, pb.rotation_euler = (1, 0, 0, 0), (0, 0, 0)
        if o.parent is None:
            o.location = Vector(loc)
    s1_objs += new


def look(cam_loc, target):
    cam.location = cam_loc
    cam.rotation_euler = (Vector(target) - Vector(cam_loc)).to_track_quat('-Z', 'Y').to_euler()


def render(path, w, h):
    scene.render.resolution_x, scene.render.resolution_y = w, h
    scene.render.filepath = path
    r0 = time.time()
    bpy.ops.render.render(write_still=True)
    return round(time.time() - r0, 2) if os.path.exists(path) else "파일 없음"


HUMAN = (archer, bow, archer_arm, zombie, zombie_arm)
ALL = HUMAN + tuple(s1_objs)


def show_only(keep):
    for o in ALL:
        o.hide_render = o not in keep


renders = {}
show_only(ALL)
look((0.0, -12.0, 3.4), (0.0, 0, 1.0))
renders["render_lineup.png"] = render(os.path.join(out, "render_lineup.png"), 1280, 600)
poses = [("archer", archer_arm, (archer, bow, archer_arm), archer_acts["Walk"], 7), ("zombie", zombie_arm, (zombie, zombie_arm), zombie_acts["Walk"], 9),
         ("archer", archer_arm, (archer, bow, archer_arm), archer_acts["Shoot"], 10), ("archer", archer_arm, (archer, bow, archer_arm), archer_acts["Shoot"], 16),
         ("zombie", zombie_arm, (zombie, zombie_arm), zombie_acts["Attack"], 10), ("zombie", zombie_arm, (zombie, zombie_arm), zombie_acts["Attack"], 13)]
for who, arm, keep, act, f in poses:
    show_only(keep)
    assign(arm, act)
    scene.frame_set(f)
    c = arm.location
    look((c.x + 2.6, c.y - 3.4, c.z + 1.9), (c.x, c.y, c.z + 0.85))
    nm = f"render_{who}_{act.name}_f{f:02d}.png"
    renders[nm] = render(os.path.join(out, nm), 540, 540)
    assign(arm, None)
# 궁수 근접 — 얼굴 · 활(당긴 자세)
show_only((archer, bow, archer_arm))
assign(archer_arm, archer_acts["Shoot"])
scene.frame_set(16)
c = archer_arm.location
look((c.x - 1.5, c.y - 1.6, c.z + 1.45), (c.x, c.y - 0.2, c.z + 1.0))  # 오른쪽 앞에서 — 활(왼손)이 얼굴을 가리지 않게
renders["render_close_archer_draw.png"] = render(os.path.join(out, "render_close_archer_draw.png"), 640, 640)
assign(archer_arm, None)
scene.frame_set(1)
show_only(ALL)
res["renders_sec"] = renders
step("render", t0)

# ---------------------------------------------------------------- 9. FBX 다시 읽기 — 뼈 · 메시 · 동작(정본과 naive), Walk 첫 · 끝 같음, 활이 손을 따라가는가
t0 = time.time()


def reimport(path, check_anim):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=path)
    meshes = [o for o in bpy.data.objects if o.type == 'MESH']
    arms = [o for o in bpy.data.objects if o.type == 'ARMATURE']
    r = {"armatures": len(arms), "bones": [b.name for a in bpy.data.armatures for b in a.bones],
         "actions": {a.name: [round(x, 1) for x in a.frame_range] for a in bpy.data.actions},
         "meshes": {o.name: {"verts": len(o.data.vertices), "tris": tris(o.data), "uv_off_center": uv_off_center(o.data),
                             "groups_with_weight": sum(1 for g in o.vertex_groups if any(gg.group == g.index and gg.weight > 0 for v in o.data.vertices for gg in v.groups))}
                    for o in meshes}}
    if not check_anim:
        return r
    arm = arms[0]
    arm.animation_data_create()

    def use(act):
        arm.animation_data.action = act
        try:
            if act.slots and arm.animation_data.action_slot is None:
                arm.animation_data.action_slot = act.slots[0]
        except AttributeError:
            pass

    walk = next(a for a in bpy.data.actions if a.name.split("|")[-1].startswith("Walk"))
    use(walk)
    f0, f1 = (int(round(x)) for x in walk.frame_range)
    mats = []
    for f in (f0, f1):
        bpy.context.scene.frame_set(f)
        mats.append([pb.matrix.copy() for pb in arm.pose.bones])
    r["walk_first_last_max_diff"] = round(max(abs(a[i][j] - b[i][j]) for a, b in zip(*mats) for i in range(4) for j in range(4)), 6)
    bow_obj = next((o for o in meshes if o.name.endswith("Bow")), None)
    if bow_obj is not None:
        shoot = next(a for a in bpy.data.actions if a.name.split("|")[-1].startswith("Shoot"))
        use(shoot)
        dists = {}
        for f in (1, 6, 16, 21, 31):
            bpy.context.scene.frame_set(f)
            dg = bpy.context.evaluated_depsgraph_get()
            ev = bow_obj.evaluated_get(dg)
            m = ev.to_mesh()
            c = sum((ev.matrix_world @ v.co for v in m.vertices), Vector()) / len(m.vertices)
            ev.to_mesh_clear()
            dists[f] = round((c - arm.matrix_world @ arm.pose.bones["HandL"].head).length, 4)
        r["bow_centroid_to_handL_m"] = dists
    return r


res["reimport"] = {n: reimport(os.path.join(out, f"{n}.fbx"), True) for n in ("SkeletonArcher", "Zombie")}
res["reimport_naive"] = {n: reimport(os.path.join(naive_dir, f"{n}.fbx"), False) for n in ("SkeletonArcher", "Zombie")}
step("reimport", t0)
res["total_sec"] = round(time.time() - T0, 2)
print("P04_RESULT " + json.dumps(res, ensure_ascii=False))
