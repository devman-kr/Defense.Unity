---
id: 20261102-p04-s1-smooth-shading
title: P04 S1(Opus 5.5) — 매끈 판 슬라임 · 박쥐 · 등불 탑 · 함정 실측 · 셰이딩 3종 캡처 · 녹화
summary: 서브디비전 2단계 매끈 판을 만들어 스킨 가중치 · 노멀 · 팔레트 UV · 삼각형 수를 실측하고(탄젠트가 정점을 가르는 함정 발견), URP Lit · Unlit · 손으로 쓴 셀 셰이더로 로우폴리 판과 나란히 캡처 8장 · 48프레임 MP4 를 냈다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
work_type: create
commits: [d8992c9, 8e8f3ad, e84c677]
implements: [20261102-p04-handoff-s1-smooth-shading]
---

# P04 S1(Opus 5.5) — 매끈 판 · 함정 실측 · 셰이딩 3종 캡처 · 녹화 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-11-02 13:54~(KST) · Mac M5 16 GB · Claude Opus 5.5(세션 `94cbfae6`) |
| 근거 | Planning P04 지시서 `20261102-p04-handoff-s1-smooth-shading` — worktree `z.tmp/wt/p04` · 브랜치 `p04/smooth` · 시작 `43dc1c7` |
| 위험 등급 | 저 — 에디터 전용 스크립트(asmdef `Defense.Editor`) · 시험 씬 · 샘플 에셋 · 셰이더 · 머티리얼 · `Tools/Blender/` 스크립트. `ProjectSettings/` · `Packages/` · `Assets/Settings/` · 저장 · 재화 변경 없음 |
| 증거 ① 게이트 | 두 번 같은 결과(`8e8f3ad` 전 · `e84c677` 전). 열린 에디터: `U script recompile` → `recompile_status` `up_to_date` · `compilationFailed: false` → `U console status` `consoleErrors: 0` · `consoleWarnings: 0`(셰이더 포함 — `ShaderUtil.ShaderHasError` false · 메시지 0) → `U test run --mode editor` `Total 2 · Passed 2 · Failed 0`. 닫힌 뒤: `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 7초 / 6초 · `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS'` 0(스크립트가 열린 에디터에서 이미 컴파일돼 이 실행에는 컴파일 줄이 없다 — 경고 근거는 열린 에디터의 `consoleWarnings: 0`) |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서를 담은 커밋 뒤). `ProjectSettings/SceneTemplateSettings.json`(씬을 새로 만들 때 생김)은 지웠고 `ProjectSettings/` · `Packages/` · `Assets/Settings/` · `Library/` · `Temp/` · `Logs/` 는 커밋에 없다 |
| 증거 ③ 캡처 | `z.captures/20261102/p04/` — `P04_{Smooth,Low}_{Lit,Unlit,Cel}.png`(6) · `P04_Lineup_Lit.png` · `P04_Close_SlimeFace_Smooth_Lit.png` · `SmoothLit_48f_24fps.mp4`(다시 읽어 48프레임) · `frames_smooth_lit/` · `renders/`(Blender 10 · `p04_result.json`) · README 대응표(커밋용 사본 `z.docs/work_reports/20261102/캡처_README.md`). 확인 페이지 https://claude.ai/artifact/ACa4iS6yjvZY3WAn8QtGkj(비공개). 확인 안 한 것은 7절 |

## 1. 결론

**완료** — 성공 기준 7 중 7, 미지수 5 중 5 실측. 서브디비전 2단계 매끈 판은 스킨 가중치 · 뼈 · 동작이 그대로 남고(Hop 13 꼭대기 Blender = FBX = Unity 1.366 m), 팔레트 UV 는 번지지 않으며, 삼각형은 캐릭터 기준 로우폴리의 약 17.6배다. 새로 찾은 함정 하나 — Unity 기본 가져오기(탄젠트 MikkTSpace)가 셀 중심 UV 때문에 정점을 68% 가른다 → `importTangents = None` 으로 Blender 와 같은 정점 수. 셀 셰이더는 등불 점광을 받지 않는다(Lit 과의 비교에 남는 차이). 지시서 안의 충돌 하나(P03 녹화 산출 불변 ↔ 스킨 플래그 켜기)는 성공 기준 6 을 따랐고 P03 녹화가 바뀐다(8절). 리뷰 미해결 0 · 리뷰하지 못한 범위 없음. 브랜치는 로컬에만 있다(푸시 안 함).

## 2. 바꾼 것

| 커밋 | 파일 | 무엇 |
|---|---|---|
| `d8992c9` | `Tools/Blender/make_enemies_smooth.py`(새) · `Tools/Blender/README.md`(표 행) | `make_enemies.py` 의 도우미를 복사(공통 모듈로 뽑지 않음 — 그 파일 산출을 건드리지 않으려고)해 같은 형태 · 뼈 · 가중치 · 동작 · 팔레트에 Subsurf 2단계(Armature 뒤) · `shade_smooth`. 박쥐 귀 · 탑 지붕 꼭짓점은 정점 크리스 1.0, 날개 외곽 · 탑 테두리는 모서리 크리스 0.75(이웃 면 사이 각 > 50°). 비교용 로우폴리 판을 같이 만들어 라인업 렌더. FBX 는 `_bst` 하나 → `Slime.fbx` · `Bat.fbx` · `Tower.fbx` · `Palette.png`. `P04_RESULT` JSON |
| `8e8f3ad` | `Assets/30_Assets/Models/P04_Smooth/`(FBX 3 · `Palette.png`) | 가져오기: 노멀 `Import` · **탄젠트 `None`**(3절 b) · 머티리얼 `Palette` → `PaletteLit` 리맵 · 클립 이름 벗기기 · Idle/Hop/Flap `loopTime` · 팔레트 Point · 밉맵 끔 · 비압축 · Clamp |
| 같음 · `e84c677` | `Assets/30_Assets/Shaders/PaletteCel.shader`(새 폴더) | URP 17.3 손으로 쓴 셀 — 주 조명 N·L × 그림자 감쇠를 `smoothstep` 두 개로 3단(어두운 · 중간 `_MidLevel` · 밝은) 램프, 어두운 단은 `_ShadowTint`, 앰비언트 `SampleSH` × 0.35, 안개. 그림자 받음(`_MAIN_LIGHT_SHADOWS*` · `_SHADOWS_SOFT*`). `e84c677`(센서 지적 수정)에서 SSAO 수신(`_SCREEN_SPACE_OCCLUSION`)과 그림자 거리 페이드(`GetMainLight(shadowCoord, positionWS, shadowMask)`)를 더했다. 추가 조명(등불 점광)은 받지 않음. 그림자 캐스터 · 깊이 · 깊이 노멀 패스는 URP 의 `ShadowCasterPass.hlsl` · `DepthOnlyPass.hlsl` · `UnlitDepthNormalsPass.hlsl` include. SRP Batcher 용 `UnityPerMaterial` 버퍼를 `HLSLINCLUDE` 에 하나 |
| 같음 | `Assets/30_Assets/Materials/`(새 폴더) `PaletteLit.mat` · `PaletteUnlit.mat` · `PaletteCel.mat` | 셰이딩마다 머티리얼 하나, 셋이 같은 `Palette.png`. Lit 은 P03 과 같은 `_Smoothness` 0.1 |
| 같음 | `Assets/10_Scripts/Editor/P04/` `P04ModelImport.cs` · `P04SampleSceneBuilder.cs` · `P04Capture.cs`(새) | 가져오기 · 머티리얼 셋 · 메시 수 출력 / 비교 씬(루트 셋 — `P04Smooth` · `P04Low` · `P04Lineup`, P03 구도 값 그대로) · 셰이딩 바꾸기 / 캡처 8 · Hop 높이 · UV 이탈 측정 |
| 같음 | `Assets/10_Scripts/Editor/P03/P03FrameRecorder.cs` | `Record` 를 씬 · 루트 · 모델 경로 인자 판으로 일반화(옛 서명은 P03 값으로 넘김), `GetRoot(scene, rootName)` 공개, 녹화 전 `forceMatrixRecalculationPerRender = true` |
| 같음 | `Assets/00_Scenes/P04_Smooth_Sample.unity` | 빌더로 만든 씬(손 YAML 편집 없음) — 저장 상태는 `P04Smooth` 만 켜짐 · 모든 렌더러 `PaletteLit` |
| 같음 | `ASSETS.md` | P04 스크립트 행 · P04 모델 · 머티리얼 · 셰이더 행 |

## 3. 검증 결과

### 성공 기준 대조

| # | 기준 | 결과 | 근거 |
|---|---|---|---|
| 1 | Blender 매끈 판 스크립트 | 됨 | 지시서 명령 그대로 종료 0 · 5.4초 · 0.55 GB. 렌더 10장을 열어 봄 — 매끈 판 슬라임 · 박쥐 · 탑에 면 경계 없음, 색은 셀 그대로, 지붕 · 귀는 크리스로 뾰족함 유지. `make_enemies.py` 는 고치지 않았다(도우미 복사 — 다시 돌려 비교할 필요 없음) |
| 2 | 가져오기 | 됨 | `P04ModelImport.Apply()` 출력 — `Slime.fbx tris=8384 verts=4206 normals=4206 bones=2 bindposes=2` · `Bat.fbx tris=6880 verts=3458 normals=3458 bones=3 bindposes=3` · `Tower.fbx tris=2848 verts=1442 normals=1442 bones=0`. `P04_RESULT` 2단계와 삼각형 · 정점 · 뼈 수가 같다. 클립 `SlimeRig\|Hop -> Hop (loop)` · `BatRig\|Hit.001 -> Hit` 등. `.meta` 에 `normalImportMode: 0`(Import) · `tangentImportMode: 2`(None) |
| 3 | 함정 실측 표 | 됨 | 4절 |
| 4 | 셰이딩 3종 | 됨(점광 제외) | 머티리얼 셋 · 셀 셰이더 컴파일 오류 · 경고 0. 그림자 받음 들어감. 렌더러 기능(`Assets/Settings/`)을 요구하지 않음 — SSAO(렌더러에 이미 있음, 깊이 노멀 소스)용 `DepthNormals` 패스를 셰이더 쪽에 둠. 추가 조명은 범위를 작게 하려고 뺐다 |
| 5 | 캡처 8 | 됨 | `P04Capture.CaptureAll` — 세로 1080×1920 6 · 라인업 1920×1080 · 근접 1080×1080. 전부 열어 봄(5절) |
| 6 | 녹화 | 됨 | 48프레임 720×1280 → `SmoothLit_48f_24fps.mp4` 216,338 B. Blender 로 다시 읽어 `movieclip.frame_duration` 48 · strip 48 · 24 fps. 프레임 0 · 12 · 24 의 MD5 가 서로 다르고 frame_012 를 열어 자세 차이 확인 |
| 7 | 게이트 · 보고 · 센서 · 커밋 · 확인 페이지 | 됨 | 기본 정보 증거 ① · 커밋 셋(10절 표) · 확인 페이지 URL |

### `P04_RESULT` 요지(원문 `z.captures/20261102/p04/renders/p04_result.json`)

- 모디파이어 순서 `Slime` · `Bat` = `[ARMATURE, SUBSURF]`, `Tower` = `[SUBSURF]` · 탑 크리스 모서리 116.
- FBX 다시 읽기(Blender): 슬라임 정점 4206 · 삼각형 8384 · 정점 그룹 `SlimeRoot` · `SlimeTop`(가중치 > 0 정점 각 3845) · 동작 4, 박쥐 3458 · 6880 · `BatRoot` 3166 · `WingL` 146 · `WingR` 146, 탑 1442 · 2848.
- Hop 높이(m): 매끈 Blender f1 0.816 · f13 **1.366** / FBX 다시 읽기 f13 1.366 / 로우폴리 f13 1.375(= P03).
- 박쥐 Flap 6 날개 끝: 매끈 `max_x` 0.727 · `tip_z` −0.279/−0.279, 로우폴리 0.745 · −0.278/−0.278.
- FBX 크기: `Slime.fbx` 268,396 B · `Bat.fbx` 204,652 B · `Tower.fbx` 67,692 B.

## 4. 함정 실측 표 · 삼각형 수 표

| 함정 | 실측 | 근거 |
|---|---|---|
| (a) 서브디비전을 적용해 내보낸 FBX 에 스킨 가중치 · 뼈가 남는가 | **남는다.** 슬라임 Hop 프레임 13 꼭대기: Blender 매끈 1.366 m = Blender 로 다시 읽은 FBX 1.366 = **Unity 1.366**(`P04Capture.MeasureHop` — 클립 샘플링 → `BakeMesh` → 최대 y). 로우폴리: Blender 1.375 = Unity 1.375(P03 `extent` 와 같음). 차이 9 mm 는 Catmull-Clark 수축 | FBX 내보내기 코드(`io_scene_fbx/export_fbx_bin.py` 2704~2750): `use_mesh_modifiers` 면 Armature 모디파이어는 끄지 않고 뼈대를 `REST` 로 둔 채 평가하고 `preserve_all_data_layers=True` 로 메시를 만든다 — Subsurf 가 정점 그룹을 보간해 넘긴다. 순서는 Armature → Subsurf(나중 추가) |
| (b) 노멀 — 면 경계 · 꼭짓점마다 하나 | **노멀은 공유된다, 그러나 기본 가져오기에서 정점이 갈라졌다.** 처음 가져오기(탄젠트 `CalculateMikk` 기본): 슬라임 정점 7054 · 박쥐 6132 · 탑 1721(Blender 4206 · 3458 · 1442). 같은 위치 정점 묶음 1542(슬라임)에서 노멀 차이 0 · UV 차이 0 · **탄젠트 차이 1542**. 면 UV 가 셀 중심 한 점이라 UV 기울기가 0 → MikkTSpace 탄젠트가 면마다 달라 Unity 가 정점을 가른다. `importTangents = None` 뒤 4206 · 3458 · 1442(탑의 같은 위치 2 묶음은 Blender 원본에서 겹친 꼭짓점 — 노멀 · UV 도 다름). 공유의 증거는 **Unity 정점 수 = Blender 정점 수**와 같은 위치 묶음 0 이다 — 지시서의 「`Mesh.normals` 수 = 정점 수」는 Unity 에서 정점이 갈라져도 늘 참이라 증거가 되지 못한다(센서 `P04.S1.UNI.03`). 수동 재계산 없음. 캡처 6장에 면 경계 없음 | 진단 스크립트(`U script eval` — 위치로 묶어 노멀 각 0.5° · UV · 탄젠트 차이를 셈). 노멀 맵을 쓰지 않는 팔레트 셰이딩에서는 탄젠트가 쓸모없다 |
| (c) 팔레트 UV — 서브디비전 뒤 셀 경계 번짐 | **번지지 않는다.** UV 가 셀 중심에서 벗어난 최대 거리 0: Blender 0 · 1 · 2단계 모두 0.0, FBX 다시 읽기 0.0, Unity(슬라임 매끈 · 로우폴리) 0.000000. 근접 캡처(`P04_Close_SlimeFace_Smooth_Lit.png` · Blender `render_close_slime_smooth.png`)에서 눈 흰자 · 눈동자 · 볼 · 몸 경계가 셀 색 그대로 | Subsurf `uv_smooth` 기본 `PRESERVE_BOUNDARIES`. 부품마다 모든 루프 UV 가 같은 점이라 어떤 보간도 같은 점 — 부품 사이는 기하가 떨어져 있어 섞이지 않는다 |
| (d) 삼각형 수 | 아래 표 | 단계별 수는 Blender 가 평가한 메시(쉼 자세) |
| (e) Blender 5.2 API · FBX 옵션 | 스무스: `bpy.ops.object.shade_smooth()`(인자 `keep_sharp_edges` 만, 이 판은 기본). `shade_smooth_by_angle(angle, keep_sharp_edges)` · `shade_auto_smooth` 도 있으나 Subsurf 결과가 이미 곡면이라 안 씀. Subsurf: `obj.modifiers.new("Subsurf", 'SUBSURF')` · `levels = render_levels = 2` · `uv_smooth` 기본. 크리스: 모서리 `bm.edges.layers.float.new("crease_edge")`, 정점 `me.attributes.new("crease_vert", 'FLOAT', 'POINT')`(4.0 이후 속성 이름 — Subsurf `use_creases` 기본 켬). FBX: `use_mesh_modifiers=True` · `use_mesh_modifiers_render=True` · `use_subsurf=False`(켜면 마지막 Subsurf 를 끄고 FBX 서브디비전 정보만 넣음) · `mesh_smooth_type='FACE'` · `bake_space_transform=True` · `add_leaf_bones=False` · `bake_anim_use_all_actions=True` | `P04_RESULT.fbx_options` · 조사 스크립트 출력 |

| 모델 | 0단계(= 로우폴리) | 1단계 | 2단계(내보낸 판) | 1단계 ÷ 0 | 2단계 ÷ 0 | 2단계 × 300 | 정점 0 → 1 → 2 |
|---|---|---|---|---|---|---|---|
| 슬라임 | 472 | 2,096 | 8,384 | 4.44 | 17.76 | 2,515,200 | 250 → 1,062 → 4,206 |
| 박쥐 | 392 | 1,720 | 6,880 | 4.39 | 17.55 | 2,064,000 | 214 → 878 → 3,458 |
| 등불 탑 | 182 | 712 | 2,848 | 3.91 | 15.65 | —(적 아님) | 109 → 374 → 1,442 |

「2단계 × 300」은 기획서 5절 동시 생존 적 상한을 곱한 숫자만 — 판단은 사용자. 측정 2026-11-02 · Blender 5.2.3 · Unity 6000.3.27f1(가져온 수 같음).

## 5. 셰이딩 비교 메모 · 레퍼런스 관찰

셰이딩 비교(세션이 캡처를 열어 본 차이 — 판정은 사용자):

- **Lit**: 매끈 판은 슬라임 표면에 면 경계가 없고 탑이 둥근 원기둥 · 부드러운 원뿔 지붕으로 읽힌다. 로우폴리 판은 같은 조명에서 슬라임 면이 격자처럼 보이고 탑이 8각. 두 판의 차이가 가장 크게 보이는 셰이딩. 부드러운 방향 그림자 · 등불 점광 · SSAO(눈 둘레 그늘)가 있다.
- **Unlit**: 명암 · 그림자가 없어 두 판이 거의 같다 — 차이는 실루엣(8각 탑 · 슬라임 외곽의 꺾임)뿐이고 매끈 슬라임은 평평한 원판처럼 보인다. 슬라임 둘레의 흐린 어두운 고리는 SSAO 로 추정.
- **셀**: 매끈 판은 밝은 단 · 어두운 단이 깔끔한 두 덩어리로 갈린다. 로우폴리 판은 면마다 단이 달라 슬라임이 얼룩진다. 바닥 그림자 가장자리에 중간 단 고리가 생긴다(부드러운 그림자 감쇠가 중간 단을 지남 — `_MidLevel` 1 로 두 단이면 없어질 것으로 추정, 확인 안 함). 점광을 받지 않아 탑 둘레가 Lit 보다 평평하다. 셀 캡처 2장은 SSAO 수신을 더한 `e84c677` 판으로 다시 찍었다 — 처음 판과 바이트는 다르나 눈으로는 그늘이 조금 짙어진 정도.
- 세 셰이딩 모두 에디터 활성 품질 PC(SSAO · 소프트 그림자 · 캐스케이드 4)로 찍었다. 목표인 Android 기본 품질(Mobile — SSAO 없음 · 소프트 그림자 끔 · 캐스케이드 1)에서는 그림자 가장자리가 더 딱딱하고 AO 그늘이 없을 것이다(추정 — 센서 `P04.S1.UNI.11`).

레퍼런스 관찰(App Store 공개 스크린샷 — 피자 레디 2 · 버거 플리즈 2 · XP 히어로 3장, iTunes 검색 API 로 주소를 얻어 스크래치패드에 받아 열어 보고 지움. 54초):

- 면의 매끈함: 각진 면이 보이지 않는다. 소품(피자 · 병 · 차 · 기계)은 모서리를 둥글린 덩어리, XP 히어로 선인장은 세로 골이 있는 둥근 기둥.
- 비례: 피자 · 버거 캐릭터는 화면에서 작은 사람(머리가 크고 모자 · 머리 색으로 읽힘), XP 히어로 적(고블린 · 토끼)은 둥근 몸에 머리가 크다.
- 눈 · 입: XP 히어로 토끼는 빨간 점 눈. 피자 · 버거는 화면 크기에서 얼굴이 거의 안 보인다.
- 색: 면 단위 단색 원색 · 파스텔. XP 히어로 바닥은 넓은 영역 그라데이션(비네트)과 무늬 조각.
- 그림자: 바닥에 방향 그림자가 있고 가장자리가 부드럽다. 셰이딩은 언릿이 아니라 방향광 명암이 분명한 부드러운 그라데이션(눈에 띄는 셀 단계 없음).
- 외곽선: 피자 · 버거 없음. XP 히어로 적은 얇은 어두운 외곽선이 있어 보이고 보스는 빨간 외곽선(상태 효과로 추정).
- 바닥 · 배경: 피자 · 버거는 단색 바닥(줄눈 약하게) + 잔디, XP 히어로는 바이옴별 단색 바닥 + 소품.
- **스타일 보드 3절 [추정]과 다른 점**: 보드는 「로우폴리 메시 · 거의 언릿 · 플랫 셰이딩 · 그림자 단순」이라 적었으나, 본 화면은 매끈(둥근 모서리) · 방향광 라이팅 · 부드러운 방향 그림자에 가깝다. 보드 4절 「외곽선 없음」 방침과 달리 XP 히어로 적에는 외곽선이 있어 보인다. 보드는 고치지 않았다(사용자 결정).

## 6. 단계별 걸린 시간

| 단계 | 초 | 비고 |
|---|---|---|
| Blender(매끈 판 · 렌더 10 · FBX · 다시 읽기) | 5.4 | 최종 실행 wall, 스크립트 안 `total_sec` 4.9(렌더 4.6). 첫 실행 6.0(크리스 없는 판 — 귀 · 지붕 · 날개가 뭉개져 고침) |
| 레퍼런스 보기 | 54 | 검색 · 받기 · 열기 · 지우기 |
| 에디터 기동 | 10.1 | `[Project] Loading completed in 10.116 seconds`(클론 `Library/`, 첫 새로 고침 8.1). 기동 직후 시작한 대기 루프가 두 번째 확인에서 `ready` |
| 가져오기 | < 1 × 2 | `Apply()` 두 번(탄젠트 진단 전 · 후), 사이 재컴파일 2 |
| 씬 | < 1 | `Build()` |
| 셰이더 | — | 기동 때 컴파일 — 따로 재지 않음 |
| 캡처 8 | 3 | `CaptureAll` 한 번 |
| 프레임 48 | 2 | `Record` |
| P03 녹화 대조(덤) | 2 + 비교 | 7절 |
| 에디터 세션 합 | 175 | 대기 루프 시작부터 `Exit(0)` 까지 |
| 셰이더 수정 뒤 에디터 2회차 | 약 40 | 기동 5(클론이 아닌 같은 `Library/` 다시 열기) · 셰이더 확인 · 캡처 8 다시 · 게이트 · 끄기 |
| 인코딩 · 다시 읽기 | 2 | Blender 내장 ffmpeg, 인코딩만 0.36 |
| 닫힌 게이트 | 7 | `unity test` |

## 7. 확인하지 않은 것 · 추정한 것

- **P03 녹화 산출이 바뀌었다.** 계약은 「P03 씬 · 녹화 산출이 같아야 한다」인데, 지시서가 켜라고 한 `forceMatrixRecalculationPerRender` 때문에 P03 녹화를 다시 떠 보니 2026-10-31 `z.captures/20261031/opus/frames/` 와 프레임 0 만 같고 1~47 이 다르다(frame_012 다른 픽셀 50,320 · 최대 차 229). 옛 n 과 새 n−1 도 같지 않아 단순한 한 프레임 지연은 아니다. 옛 frame_012 를 열어 보면 슬라임 자세가 새 녹화와 다르다 — 플래그 없는 옛 녹화가 스킨 행렬을 렌더마다 갱신하지 않은 판(P03 발견)이라고 **추정**하고, 플래그를 끈 판을 다시 떠 대조하지는 않았다. P03 씬 파일은 그대로다.
- 셀 셰이더의 그림자 고리가 `_MidLevel` 1(두 단)에서 사라지는지, 모바일 품질(`Mobile_RPAsset` — 소프트 그림자 꺼짐 · 캐스케이드 1)에서 같은 모양인지 보지 않았다. 활성 품질은 PC(`m_CurrentQuality: 1`, Forward+).
- 셀 셰이더는 Forward+ 의 추가 조명 루프(`_CLUSTER_LIGHT_LOOP`) · 라이트맵 · 라이트 레이어 · 데칼 · LOD 크로스페이드 키워드를 넣지 않았다 — 지금 씬에는 쓰이지 않아 확인 안 함. SRP Batcher 호환은 버퍼 하나로 맞췄다고 보고 인스펙터 표시는 확인 안 함.
- 박쥐 날개는 크리스를 줘도 로우폴리보다 2.4% 짧다(0.727 / 0.745 m). 녹화 · 캡처 화면에서 눈에 띄는 차이는 못 봤다.
- 레퍼런스 외곽선은 600×1299 스크린샷에서 「있어 보인다」 수준 — 셰이더 외곽선인지 텍스처인지 모른다. 레퍼런스가 셀인지 Lambert 인지도 화면만으로는 추정.
- 「2단계 × 300」의 성능(드로콜 · 스킨 비용 · 기기 프레임)은 재지 않았다 — 숫자만.

## 8. 지시서와 다른 점 · 더 나은 방법 · 범위 밖 제안

- 다른 점(지시서 안의 충돌): 「수정 허용 영역」은 `Editor/P03/**` 변경에 「P03 씬 · 녹화 산출이 같아야 한다」를 걸었고, 성공 기준 6 은 `P03FrameRecorder` 에서 `forceMatrixRecalculationPerRender` 를 켜라고 했다. 둘을 함께 지킬 수 없어 성공 기준 6 을 따랐다 — 옛 5인자 `Record` 도 새 판을 거쳐 플래그가 켜지므로 **P03 녹화를 다시 돌리면 프레임 1~47 이 2026-10-31 산출과 다르다**(7절). P03 씬 파일과 가져오기 설정은 그대로다(센서 `P04.S1.UNI.01`).
- 다른 점: 가져오기에 `importTangents = None` 을 더했다(지시서는 노멀만 적음) — 정점이 갈라지는 함정을 막는 설정이라 넣고 4절 (b)에 전후 수를 적었다. 셀 셰이더는 추가 조명을 받지 않는다(성공 기준 4 는 주 조명만 요구).
- 다른 점: 셰이딩 비교의 로우폴리 판은 P03 `*_bst.fbx` 를 그대로 쓰고 씬 인스턴스의 머티리얼만 셋 중 하나로 바꿨다(P03 에셋 · 가져오기 설정은 그대로).
- 다른 점: 캡처 날짜 폴더 `z.captures/20261102/README.md` 와 `z.captures/README.md` 폴더 표 행은 수정 허용 영역(`z.captures/<날짜>/p04/`) 밖이라 만들지 않았다 — S3 가 더할 것.
- 더 나은 방법: 셰이딩을 바꿀 때 씬을 셋 만들거나 모델마다 리맵하지 않고, 비교 씬 하나에 루트를 셋 두고 `ApplyMaterial` 로 씬 인스턴스만 바꾸니 캡처 8장이 한 호출 3초였다.
- 범위 밖 제안: ① 셀 셰이더 두 단 판(`_MidLevel` 1)과 추가 조명 루프 ② 레퍼런스가 라이팅 + 부드러운 그림자에 가까우니 Lit 에 램프 텍스처만 얹은 「부드러운 셀」 한 장 더 ③ 스타일 보드 3절을 이 관찰로 확정할지(사용자 결정).

## 9. 발견 — S2 · `DEF-M03` 에 넘길 것

1. **매끈 판 Blender 레시피**: 모델링 → 뼈대 · 가중치(거친 메시에) → `add_subsurf`(Armature 뒤, `levels = render_levels = 2`) → FBX `use_mesh_modifiers=True` · `use_subsurf=False`. 가중치는 Subsurf 가 보간해 남는다 — 따로 적용(`modifier_apply`)할 필요 없다.
2. **크리스가 필수인 곳**: 원뿔 꼭짓점(정점 크리스 1.0 — 없으면 귀가 혹, 지붕이 버섯이 되고 꼭지 구슬이 떠 있다), 얇은 판(날개 — 모서리 크리스 0.75, 없으면 13% 짧아짐), 원기둥 윗면 · 아랫면 테두리(0.75 — 각이 50° 넘는 모서리만). 인간형(S2)이면 손가락 끝 · 무기 날 · 신발 바닥이 같은 부류.
3. **Unity 가져오기 — 팔레트 모델은 탄젠트 끄기**: `ModelImporter.importTangents = ModelImporterTangents.None`. 셀 중심 UV 에서 MikkTSpace 가 정점을 가른다(슬라임 4206 → 7054, 68% 늘어남). P03 로우폴리 FBX 도 같은 조건이라 플랫 셰이딩 정점에 탄젠트 분할이 더해져 있을 것(확인 안 함).
4. **수(數)**: 서브디비전 1단계 ≈ 4.4배, 2단계 ≈ 17.6배(캐릭터). 슬라임 2단계 8,384 삼각형 · 박쥐 6,880.
5. **셀 셰이더 골격(URP 17.3)**: 주 패스는 `Lighting.hlsl` 의 `GetMainLight(TransformWorldToShadowCoord(positionWS))` · 키워드 `_MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN` · `_SHADOWS_SOFT*` · 안개. 부가 패스는 URP 파일을 include(알파 테스트가 없으면 머티리얼 변수 불필요): `ShadowCaster`(`_CASTING_PUNCTUAL_LIGHT_SHADOW`) · `DepthOnly` · `DepthNormals`(렌더러 SSAO 가 깊이 노멀을 쓴다). Forward+ 추가 조명 키워드는 17.x 에서 `_CLUSTER_LIGHT_LOOP`(옛 `_FORWARD_PLUS`).
6. `Shader.passCount` 는 URP Lit 도 1 을 돌려준다(서브셰이더 3 — 첫째 9 패스) — 패스 수 확인은 `GetPassCountInSubshader(0)`.
7. Unlit 에서는 매끈 · 로우폴리 차이가 실루엣뿐이라, 아트 방향이 언릿이면 서브디비전 비용(17배)이 화면에 거의 남지 않는다.
8. 레퍼런스 받기: iTunes 검색 API(`https://itunes.apple.com/search?term=<이름>&entity=software`)가 `screenshotUrls` 를 돌려준다. 주소 끝 크기(`392x696bb.jpg`)를 `600x1300bb.jpg` 로 바꾸면 큰 그림.
9. `fm_lint.py` 는 `z.tmp` 를 제외 폴더로 본다 — worktree(`z.tmp/wt/…`) 안 문서는 루트 구조를 흉내 낸 임시 폴더로 복사해 검사했다.

## 10. 코드 리뷰

위험 등급: 저(지시서). 결정론 게이트: 컴파일 오류 0 · 경고 0(열린 에디터 `consoleWarnings 0`, 셰이더 메시지 0) · EditMode 2 → 2 통과, 실패 0(열림 · 닫힘 모두, `8e8f3ad` 전 · `e84c677` 전).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `43dc1c7..8e8f3ad` | 13파일(코드 6 · 머티리얼 YAML 3 · 이진 4) +1,564/−8 | 세션 센서(A opus) | 1 · 51.0만(도구) / 21.1만(알림) · 7분 52초 | 12 | 4 | 0 | 0 | 0 | 8 | 0 |
| Unity 2라운드 | `8e8f3ad..e84c677` | 1파일 +11/−2 | 세션 센서(A opus) | 1 · 21.7만(도구) / 10.9만(알림) · 2분 39초 | 5 | 0 | 0 | 0 | 0 | 5 | 0 |

원본 · 처리표: `z.docs/work_reports/20261102/코드리뷰_p04_s1/`(처리.md 17행) — 지시서대로 worktree 안(S3 가 Planning 으로 옮긴다). 새 토큰의 「도구」 값은 `센서기록_추출.py` 산출로, P03 S3 가 찾은 중복 합산이 있을 수 있어 하네스 완료 알림의 `subagent_tokens` 를 나란히 적었다.

사람이 볼 행(blocking · important 이면서 new, 미해결):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P04.S1.UNI.01 | question · important · new | CONTRACT | 지시서 안의 두 조건(P03 녹화 불변 ↔ 스킨 플래그)이 맞지 않아 P03 녹화가 바뀐다 | P03FrameRecorder.cs:25-29,44-48 | 고침(보고) | 8절 · 7절에 사유 · 대조 수치 |
| P04.S1.UNI.02 | suggestion · important · new | CONTRACT | 셀만 SSAO · 점광을 받지 않아 셰이딩 비교에 다른 변수가 섞임 | PaletteCel.shader:46-49 | 고침 | `e84c677`(SSAO) — 점광 미수신은 남김(2 · 5 · 8절) |
| P04.S1.UNI.03 | thought · important · new | TEST | 「노멀 수 = 정점 수」는 실패할 수 없는 지표 | P04ModelImport.cs:135 | 고침(보고) | 4절 (b) 증거를 정점 수 대조 · 탄젠트 진단으로 |

판정 반론: 틀림 0 · 의도 0 → 생략. 처리 안 함 13 — plausible 5(1라운드 3 · 2라운드 2) · nit 8(1라운드 5 · 2라운드 3). 종료 조건 충족, 3라운드 없음.
