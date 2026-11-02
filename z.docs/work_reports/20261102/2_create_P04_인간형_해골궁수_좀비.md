---
id: 20261102-p04-s2-humanoids
title: P04 S2(Opus 5.5) — 해골 궁수 · 좀비 매끈 인간형 · 관절 접힘 · 소품 · 클립 이름 실측
summary: 같은 뼈대 14 · 자동 가중치 · 서브디비전 2단계로 치비 인간형 둘을 만들어 가져오고, 관절 접힘(팔꿈치 111° 에서 두께 0.66배) · 활 추종(프레임마다 0.0763 m) · 같은 뼈 이름의 「모든 동작」 내보내기 함정 · 삼각형 수(궁수 28,384 · 좀비 20,224)를 실측해 비교 씬에서 캡처 5장 · 48프레임 MP4 를 냈다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
work_type: create
commits: [ff8be0d, 4130cd2, 1eb9fc8]
implements: [20261102-p04-handoff-s2-humanoids]
---

# P04 S2(Opus 5.5) — 해골 궁수 · 좀비 매끈 인간형 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-11-02 14:25~(KST) · Mac M5 16 GB · Claude Opus 5.5(세션 `99a2b19d`) |
| 근거 | Planning P04 지시서 `20261102-p04-handoff-s2-humanoids`(초안 — 빈 자리 셋을 이 세션이 채움, 7절) — worktree `z.tmp/wt/p04` · 브랜치 `p04/smooth` · 시작 `9fcc860` |
| 위험 등급 | 저 — 에디터 전용 스크립트(asmdef `Defense.Editor`) · 시험 씬 · 샘플 FBX · `Tools/Blender/` 스크립트 · 장부 행. `ProjectSettings/` · `Packages/` · `Assets/Settings/` · 셰이더 · 머티리얼 · 저장 · 재화 변경 없음 |
| 증거 ① 게이트 | 열린 에디터(`4130cd2` 전): `U script recompile` → `recompile_status` `up_to_date` · `compilationFailed: false` → `U console status` `consoleErrors: 0` · `consoleWarnings: 0` → `U test run --mode editor` `Total 2 · Passed 2 · Failed 0`. 닫힌 뒤: `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 7초 · `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS'` 0(스크립트는 열린 에디터에서 이미 컴파일됨 — 경고 근거는 열린 에디터의 `consoleWarnings: 0`) |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서를 담은 커밋 뒤). `ProjectSettings/SceneTemplateSettings.json`(씬을 열고 저장할 때 생김)은 지웠고 `ProjectSettings/` · `Packages/` · `Assets/Settings/` · `Library/` · `Temp/` · `Logs/` 는 커밋에 없다 |
| 증거 ③ 캡처 | `z.captures/20261102/p04/` — `P04S2_Portrait_Lit.png` · `P04S2_Lineup5_Lit.png` · `P04S2_Close_ArcherFaceBow_Lit.png` · `P04S2_Close_ArcherShoot24_Lit.png` · `P04S2_Close_ZombieWalk25_Lit.png` · `S2_SmoothLit_Humanoids_48f_24fps.mp4`(다시 읽어 48프레임) · `frames_s2_smooth_lit/` · `renders_s2/`(Blender 8 · `p04_result.json` · `naive/` · `stale_skinning/`) · README S2 절(커밋용 사본 `z.docs/work_reports/20261102/캡처_README.md`). 확인 페이지 https://claude.ai/artifact/ESqBPViTcjuQybC1aWp6s4(비공개). 확인 안 한 것은 6절 |

## 1. 결론

**완료** — 성공 기준 5 중 5, 미지수 ⑥~⑨ 넷 모두 실측. 두 인간형은 같은 뼈대 14(Generic)에 거친 메시 자동 가중치 + 서브디비전 2단계로 Unity 까지 그대로 왔다 — 관절 접힘 각이 Blender 와 Unity 에서 같고(오른팔꿈치 111.3° = 111.3°), 활은 왼손 뼈 100 % 가중치로 프레임마다 손에서 0.0763 m 그대로, Walk 는 첫 · 끝 자세 차이 0. 새로 찾은 함정 셋 — ① 두 모델이 뼈 이름이 같으면 Blender 「모든 동작」 FBX 내보내기가 **상대 모델의 동작까지 넣는다**(FBX 마다 6개) → 동작마다 모델 표지 사용자 속성 키로 막았다 ② 자동 가중치(bone heat)는 떨어진 작은 조각(갈비 · 눈 · 발)에 가중치를 못 준다(궁수 정점 30 %) → 섬 단위로 가장 가까운 뼈에 메움 ③ 같은 에디터 프레임에서 자세를 바꿔 두 번째로 찍으면 스킨 메시가 앞 장 자세 그대로다(바이트까지 같음 — P03 발견 ④ 재현) → 캡처에도 스킨 플래그. 팔꿈치를 111° 접으면 안쪽 두께가 0.66배로 얇아진다(찢어짐 · 뒤집힘 없음). 셰이딩은 사용자 선택이 없어 Lit. 리뷰는 2라운드 19건 — 고침 5 · 미해결 0, important 넷은 plausible 3(보고 서술 보정 · 실측 첨부) · 범위 밖 1(커밋 FBX 에 이 Mac 의 절대 경로가 들어 있음 — S1 · P03 FBX 도 같아 사용자 결정으로 넘김). 브랜치는 로컬에만 있다(푸시 안 함).

## 2. 바꾼 것

| 커밋 | 파일 | 무엇 |
|---|---|---|
| `ff8be0d` | `Tools/Blender/make_humanoids_smooth.py`(새) · `Tools/Blender/README.md`(표 행) | `make_enemies_smooth.py` 의 도우미를 복사(S1 과 같은 이유 — 그 파일 산출 불변). 팔다리를 관절 둘레에 고리를 몰아 넣은 이어진 관으로 만들고(접힘 시험), 뼈대 14 · `ARMATURE_AUTO` · 남은 조각 섬 단위 메우기 · Subsurf 2단계(Armature 뒤). 자세는 「뼈 방향 · 두 뼈 IK(발목 · 손목 목표)」로 적어 부모부터 `pose_bone.matrix` 를 맞추고 쿼터니언 키(이웃 키 반구 맞춤). 활은 활대 + 시위 한 메시, 정점 그룹 `HandL` 1.0. 실측(접힘 각 · 두께 · Walk 첫 · 끝 · 활 추종 · 단계별 수) → FBX 를 그대로 한 번(`naive/`) · 표지 키를 더해 한 번 → 렌더 8(라인업은 저장소 S1 FBX 를 가져와 함께) → FBX 다시 읽기. `P04_RESULT` JSON |
| `4130cd2` | `Assets/30_Assets/Models/P04_Smooth/SkeletonArcher.fbx` · `Zombie.fbx`(+ `.meta`) | 가져오기: S1 과 같은 설정(노멀 `Import` · 탄젠트 `None` · `Palette` → `PaletteLit` 리맵 · 클립 이름 벗기기)에 리그 `Generic`, `Idle` · `Walk` `loopTime` |
| 같음 | `Assets/10_Scripts/Editor/P04/P04ModelImport.cs` | `ApplyHumanoids()` 추가(모델 경로 상수 둘 · 리그 · 루트 모션 노드 · 스킨 메시마다 수 출력), 반복 클립 목록에 `Walk`, 메시 수 출력을 `(label, mesh, boneCount)` 도우미로 분리. S1 `Apply()` 의 동작은 같다(S1 모델에 `Walk` 클립 없음) |
| 같음 | `Assets/10_Scripts/Editor/P04/P04HumanoidSceneBuilder.cs`(새) | 비교 씬에 꺼진 루트 둘 — `P04Humanoids`(궁수 4 반지름 5.6 · 좀비 4 반지름 4.0 · 근접 카메라 둘) · `P04Lineup5`(매끈 판 다섯). 있으면 지우고 다시 만든다. S1 캡처 · 녹화는 루트 셋만 켜고 끄므로 걸리지 않는다 |
| 같음 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs`(새) | 캡처 5(세로 · 근접 3 · 라인업) · 48프레임 녹화 · 측정(뼈대 회전 · 클립 · Walk 첫 · 끝 · 루트 이동 · 접힘 각 · 활 추종). 캡처 · 녹화 모두 스킨 메시 `forceMatrixRecalculationPerRender` 켬 |
| 같음 | `Assets/10_Scripts/Editor/P04/P04SampleSceneBuilder.cs` | 도우미 넷(`CreateIsland` · `CreateLights` · `AddCamera` · `InstantiateModel`) `private` → `internal` — 동작 변화 없음 |
| 같음 | `Assets/00_Scenes/P04_Smooth_Sample.unity` | 위 빌더로 루트 둘 추가(손 YAML 편집 없음). S1 루트 셋은 그대로(diff 삭제 2줄은 같은 줄이 다시 들어간 diff 정렬) |
| 같음 | `ASSETS.md` | 스크립트 행 · 인간형 FBX 행 |
| `1eb9fc8` | `P04HumanoidCapture.cs` · `P04HumanoidSceneBuilder.cs` · `make_humanoids_smooth.py` | 센서 지적 수정(9절) — 최대 접힘 프레임 모델별 상수 · 라인업 이름 상수 공유 · 중첩 타입 위치 · 근접 카메라 월드 좌표 · `.blend` 를 표지 키 뒤에 저장. 저장소 FBX 는 바꾸지 않음(6절) |

## 3. 검증 결과

### 성공 기준 대조

| # | 기준 | 결과 | 근거 |
|---|---|---|---|
| 1 | Blender 인간형 스크립트 | 됨 | `Blender -b --factory-startup -noaudio --python-exit-code 1 --python Tools/Blender/make_humanoids_smooth.py -- z.tmp/p04_out/s2` 종료 0 · 5.7초 · 최대 메모리 0.63 GB. 두 모델 뼈 14(변형 13 — `Root` 는 `use_deform` 끔). 가중치는 **자동(`ARMATURE_AUTO`)** — 미지수 ⑥ 이 자동 가중치의 접힘을 묻고, 이어진 관 팔다리에서 관절 둘레를 부드럽게 나누는 것이 자동의 장점이라서. 자동이 못 준 조각만 섬 단위로 메움(4절 a). 렌더 8장을 열어 봄 — 관절이 찌그러지지 않고 활이 손을 따라간다(4절) |
| 2 | 가져오기 | 됨 | `P04ModelImport.ApplyHumanoids()` 출력 — `SkeletonArcher tris=25568 verts=12810 bones=14` · `SkeletonArcherBow tris=2816 verts=1412 bones=14` · `Zombie tris=20224 verts=10328 bones=14`, `animationType=Generic motionNode=(none) importTangents=None`. 삼각형 수 · 뼈 수는 `P04_RESULT` 와 같다. 정점은 궁수 같음, 좀비만 +192(4절 d) |
| 3 | 함정 실측 표 | 됨 | 4절 |
| 4 | 샘플 씬 추가 · 캡처 · 녹화 | 됨 | `P04HumanoidSceneBuilder.Add()` → `humanoids=145 lineup5=51`(트랜스폼 수). 캡처 5장(세로 1 · 라인업 1 · 근접 3 — 지시서는 근접 1, 접힘 극단 둘을 더함) · 48프레임 720×1280 → `S2_SmoothLit_Humanoids_48f_24fps.mp4` 327,711 B, Blender 로 다시 읽어 `movieclip.frame_duration` 48 · strip 48 · 24 fps. 프레임 0 · 12 · 24 MD5 가 서로 다르고 frame_024 를 열어 자세 확인. 전부 열어 봄 |
| 5 | 게이트 · 보고 · 센서 · 커밋 · 확인 페이지 | 됨 | 기본 정보 증거 ① · 커밋 셋(9절) · 확인 페이지 URL |

### `P04_RESULT` 요지(원문 `z.captures/20261102/p04/renders_s2/p04_result.json`)

- 모디파이어 순서 `SkeletonArcher` · `SkeletonArcherBow` · `Zombie` 모두 `[ARMATURE, SUBSURF]`. 팔레트 PNG MD5 가 저장소 S1 `Palette.png` 와 같다(`8799258f…`) — 새 셀 없음.
- FBX 크기: `SkeletonArcher.fbx` 970,828 B · `Zombie.fbx` 771,052 B(그대로 낸 판 1,283,852 · 1,094,108 — 남의 동작 3개씩 더 들어서 크다).
- 다시 읽기(정본): 궁수 동작 `Idle` · `Shoot` · `Walk`, 좀비 `Attack` · `Idle.001` · `Walk.001`. 그대로 낸 판은 두 FBX 모두 동작 6.
- IK: 86번 풀고 20번 닿지 않음(허벅지 19 — 서 있는 다리가 곧게 펴진 경우, 오른 위팔 1 — Shoot 들기 프레임). 닿지 않으면 곧게 편다.

## 4. 함정 실측 표 · 삼각형 수 표

| 함정 | 실측 | 근거 |
|---|---|---|
| (a) 자동 가중치 + 서브디비전의 관절 접힘 | **찢어지거나 뒤집히지 않는다. 많이 접으면 안쪽이 얇아진다.** 접힘 각(위 뼈 · 아래 뼈 방향 사이, 최대)과 그 프레임의 관절 두께(서브디비전 2단계 메시에서 관절 축 ±2.5 cm 고리 정점을 안쪽 이등분 방향으로 투영한 폭 ÷ 쉼 자세 지름): 궁수 오른팔꿈치 **111.3°**(Shoot 24) 0.0574 / 0.0866 m = **0.663** · 왼팔꿈치 51.7°(Shoot 10) 0.952 · 무릎 61.7°(Walk 19 · 7) 0.918, 좀비 팔꿈치 45.1°(Attack 10) 0.955 · 왼무릎 **71.9°**(Walk 25) **0.856** · 오른무릎 48.7° 0.941. Unity 에서 같은 프레임을 샘플해 잰 각: 궁수 111.3° · 62.1°, 좀비 45.1° · 72.2° — 무릎의 0.3~0.4° 차이는 가져오기 기본 애니메이션 압축(`animationCompression` 1 = KeyframeReduction, 회전 오차 0.5°)으로 **추정**(센서 `P04.S2.UNI.05`, 압축을 끄고 다시 재지 않음). 근접 캡처 `P04S2_Close_ArcherShoot24_Lit.png` · `P04S2_Close_ZombieWalk25_Lit.png` 와 Blender 자세 렌더 6장에서 안쪽이 오므라들 뿐 면이 꺾이지 않음. **자동 가중치가 실패한 곳**: 떨어진 작은 조각 — 궁수 갈비 3 · 눈 2 · 발 2(정점 244 / 806 = 30 %), 좀비 발 2(16 / 642). 처음엔 정점마다 가장 가까운 뼈로 메웠더니 갈비 바깥쪽 정점 20개씩이 위팔 · 허벅지에 붙어(팔을 들면 갈비가 따라감) 섬 중심 기준으로 바꿨다 → 갈비는 `Spine`, 눈은 `Head`, 발은 `Shin`. 좀비 `Hips` 에는 자동이 정점을 하나도 주지 않았다(0) | `P04_RESULT.models.*.joints` · `weights.filled_islands_by_bone` · `P04HumanoidCapture.Measure()` 출력 |
| (b) 소품(활) 부착 | **따라온다.** 방식: 활은 별도 메시 오브젝트(부모 = 아마추어 오브젝트, 뼈 부모 아님) + Armature 모디파이어 + 정점 그룹 `HandL` 1.0(정점 96). Unity 에서는 두 번째 `SkinnedMeshRenderer`(`SkeletonArcherBow` — 뼈 14개 목록, 가중치는 `HandL` 만)가 된다. 활 정점 중심 ↔ 왼손 뼈 머리 거리: Blender 0.0763 m(Shoot 1 · 6 · 16 · 21 · 31) = FBX 다시 읽기 0.0763 = **Unity 0.0763**(`BakeMesh`). 시위는 활과 한 몸이라 당겨도 굽지 않는다. 화살 방향은 손 뼈 방향이라 쏠 때 왼손 뼈를 과녁 쪽으로 꺾어야 했다 — 손을 아래팔에 맞춰 두면 시위가 왼어깨 쪽으로 당겨져 치비 오른팔(어깨에서 손목 0.40 m)이 닿지 않는다(첫 실행에서 IK 가 펴짐 · 오른팔꿈치 최대 25°) | `P04_RESULT.models.skeleton_archer.bow_centroid_to_handL_m` · `reimport.SkeletonArcher` · `Measure()` |
| (c) 클립 이름 | **`.001` 은 붙고 벗겨진다. 더 큰 함정은 남의 동작이 들어오는 것.** Blender 동작 이름이 `Idle` · `Walk`(궁수) → `Idle.001` · `Walk.001`(좀비)이고, Unity 테이크 `ZombieRig\|Idle.001 -> Idle (loop)` · `ZombieRig\|Walk.001 -> Walk (loop)` — S1 의 `StripClipName` 으로 벗겨진다. 그러나 「모든 동작」 내보내기(`bake_anim_use_all_actions`)는 동작의 F-Curve 경로가 그 오브젝트에서 다 풀리면 넣으므로(`export_fbx_bin.py` 2522~2615 `find_validate_action_slot`) 뼈 이름이 같은 두 뼈대는 서로의 동작을 다 받는다 — 그대로 낸 FBX 는 둘 다 동작 6(`Attack` · `Idle` · `Idle.001` · `Shoot` · `Walk` · `Walk.001`). 이대로 Unity 에 넣으면 이름 벗기기 뒤 `Idle` · `Walk` 가 둘씩이 된다(추정 — 그대로 낸 판은 가져오지 않음). 막는 법: 동작마다 아마추어 오브젝트의 모델 이름 사용자 속성(`["SkeletonArcherRig"]`)에 키 하나 — 다른 아마추어에는 그 속성이 없어 경로가 풀리지 않는다. 사용자 속성은 `use_custom_props` 끔(기본)이라 FBX 에 안 쓴다 | `P04_RESULT.reimport` · `reimport_naive` · `ApplyHumanoids()` 클립 줄 |
| (d) 삼각형 수 · 「× 300」 | 아래 표. 좀비는 Unity 정점이 Blender 보다 192 많다 — 한 메시 안에서 셀이 바뀌는 경계(셔츠 옷단 1 · 셔츠 구멍 2 · 소매 2 · 바짓단 2)가 UV 이음매라 정점이 갈린다. 계산: 고리 8변 × 4(2단계) = 32 × 옷단 1 · 소매 2 · 바짓단 2 = 160 + 구멍 면 하나 테두리 16 × 2 = 192. 궁수는 조각마다 셀 하나라 갈리지 않음(12,810 = 12,810) | `ApplyHumanoids()` · `P04_RESULT.models.*.levels` |
| (e) Generic 가져오기의 루트 모션 · 루트 회전 | **P03 과 같다.** 뼈대 오브젝트(`SkeletonArcherRig` · `ZombieRig`)에 X 270° 가 남는다(`rigEuler=(270.0, 0.0, 0.0)` — P03 발견 ② 와 같은 현상). 굽힘 · 접힘 각은 Blender 와 같다. 루트 모션 없음 — 근거는 `motionNodeName` 이 비어 있다는 것 하나다(`Measure()` 의 「반 주기 뒤 모델 루트 이동 0.000000」은 클립에 모델 루트 경로 곡선이 없어 루트 모션이 있든 없든 0 이 나오는 값이라 증거가 못 된다 — 센서 `P04.S2.UNI.03`). 제자리 걷기라 엉덩이만 오르내린다(궁수 `Hips` y 0.559 / 0.537) | `ApplyHumanoids()` · `Measure()` |
| ⑨ Walk 반복이 FBX 에서 끊기지 않는가 | **자세는 이어진다. 이음매에서 멈칫하지도 않는다.** 첫 · 끝 자세 차이: Blender 0.0 · FBX 다시 읽기 0.0(뼈 포즈 행렬 성분 최대 차). Unity `Measure()` 의 0 은 반복 클립을 길이 시각에서 샘플한 값이라 0 초로 감겼다면 같은 시각끼리 비교한 것이 된다 — Unity 쪽 증거로는 약하다(센서 `P04.S2.UNI.04`). 이음매 속도(센서 `P04.S2.UNI.01` — 「끝 키 핸들이 수평이라 이음매에서 멈칫」 지적을 실측): 프레임 사이 뼈 회전량 최대값이 궁수 Walk 이음매 앞 3.32° · 뒤 3.17° 로 주기 가운데 같은 디딤 프레임(13)의 3.23° 와 같다(평균 5.74°) — 디딤 자세마다 느려지는 동작 모양이지 이음매만의 정지가 아니다. 좀비 Walk 는 이음매 앞 4.00° · 뒤 1.82°(가운데 디딤 17 근처 1.5° · 1.84°) | `P04_RESULT.walk_first_last_max_diff` · `reimport.*.walk_first_last_max_diff` · 이음매 측정 스크립트(스크래치패드, 저장된 `.blend` 를 읽음) |
| (f) 캡처의 스킨 행렬(덤) | 한 호출 안에서 자세를 바꿔 두 번째 근접을 찍었더니 첫 장과 **MD5 가 같다**(`0c979002…` — 프레임 16 과 24). 첫 장은 맞는 자세. `forceMatrixRecalculationPerRender` 를 캡처에도 켜자 둘째 장이 `374ab9f1…` 로 바뀌고 오른팔꿈치가 접힌 자세가 찍혔다. P03 Sonnet 세션이 본 「날개가 안 움직임」과 같은 원인으로 **추정** | `renders_s2/stale_skinning/` |

| 모델 | 0단계(거친 메시) | 1단계 | 2단계(내보낸 판) | 2단계 ÷ 0 | 2단계 × 300 | 정점 0 → 1 → 2(Blender) | Unity 정점 |
|---|---|---|---|---|---|---|---|
| 해골 궁수 본체 | 1,560 | 6,392 | 25,568 | 16.39 | — | 806 → 3,222 → 12,810 | 12,810 |
| 해골 궁수 활 | 184 | 704 | 2,816 | 15.30 | — | 96 → 356 → 1,412 | 1,412 |
| **해골 궁수 합** | 1,744 | 7,096 | **28,384** | 16.28 | **8,515,200** | — | 14,222 |
| **좀비** | 1,236 | 5,056 | **20,224** | 16.36 | **6,067,200** | 642 → 2,552 → 10,136 | 10,328 |
| (S1 참고) 슬라임 · 박쥐 | 472 · 392 | — | 8,384 · 6,880 | 17.76 · 17.55 | 2,515,200 · 2,064,000 | — | — |

인간형 한 마리가 슬라임의 2.4~3.4배다. 「× 300」은 기획서 5절 동시 생존 적 상한을 곱한 숫자만 — 판단은 사용자. 측정 2026-11-02 · Blender 5.2.3 · Unity 6000.3.27f1.

## 5. 단계별 걸린 시간

| 단계 | 초 | 비고 |
|---|---|---|
| Blender(모델 · 동작 · 실측 · FBX 둘 · 렌더 8 · 다시 읽기 4) | 5.7 | 최종 실행 wall, 스크립트 안 `total_sec` 5.25(렌더 4.22). 같은 스크립트를 네 번 돌렸다 — 1회차(활 방향 · 손발 크기 · 가중치 진단 전) · 2회차(섬 단위 메우기 전) · 3회차(근접 카메라 전) · 최종 |
| 에디터 기동 | 8 | 기동 직후 대기 루프가 세 번째 확인에서 `ready`(S1 이 쓴 `Library/` 그대로) |
| 가져오기 | 1 | `ApplyHumanoids()` |
| 씬 추가 | < 1 × 2 | `Add()` 두 번(좀비 근접 카메라 고친 뒤 다시) |
| 측정 | < 1 | `Measure()` |
| 캡처 5 | 1 × 2 | `CaptureAll` 두 번(스킨 플래그 전 · 후) |
| 프레임 48 | 1~2 × 2 | `Record` 두 번 |
| 에디터 세션 합 | 118 | 띄우기부터 `Exit(0)` 까지 |
| 인코딩 · 다시 읽기 | 2 | Blender 내장 ffmpeg, 인코딩만 0.33 |
| 닫힌 게이트 | 7 | `unity test` |

## 6. 확인하지 않은 것 · 추정한 것

- 그대로 낸 FBX(남의 동작 6개)는 Unity 에 가져오지 않았다 — 이름 벗기기 뒤 `Idle` · `Walk` 가 둘씩 되리라는 것은 `StripClipName` 을 읽은 추정이다(`P04HumanoidCapture` 의 클립 사전은 같은 이름이 둘이면 `Debug.Assert` 로 알린다).
- 자동 가중치(bone heat)가 갈비 · 눈 · 발 조각에서 왜 실패했는지 Blender 콘솔 경고 문구는 잡지 못했다(배경 실행의 보고 메시지가 표준 출력에 안 나옴) — 정점 그룹 합이 0 인 정점 수로만 셌다. 좀비 `Hips` 정점 0 의 영향은 Walk 의 엉덩이 기울기(±6°)가 허벅지 · 척추 가중치로만 몸통 아래에 전해지는 것 — 캡처에서 이상은 못 봤다.
- 두께 비는 「관절 축 ±2.5 cm 고리를 안쪽 이등분 방향으로 투영한 폭」이라는 이 세션의 정의다. 쉼 두께는 관 축에 수직인 폭(X · Y 평균)인데 접힘 두께의 투영 방향은 접힐수록 위 뼈 축과 겹쳐(111° 에서 sin 55.6° = 0.82) 고리의 축 방향 폭(±2.5 cm)이 최대 약 2 cm 섞일 수 있다 — 「0.663배」에는 단면이 얇아진 몫과 고리가 기운 몫이 함께 들어 있다(센서 `P04.S2.UNI.02`, 정점으로 다시 나눠 재지 않음). 부피 손실 · 캔디 래퍼(비틀림 수축)는 따로 재지 않았고, 비틀림이 큰 동작(팔 돌리기)은 없다.
- **커밋한 FBX 에 이 Mac 의 절대 경로가 들어 있다** — `strings` 로 보면 `.blend` 경로 하나와 팔레트 텍스처 경로 셋(`/Users/<사용자>/_Projects/…/z.tmp/p04_out/s2/…`). `path_mode='STRIP'` 이어도 텍스처의 절대 경로 문자열이 남는다. S1 의 `Slime.fbx` 등도 같다(4개). 수정 커밋 `1eb9fc8` 에서 `.blend` 저장을 내보내기 뒤로 옮겨 `.blend` 경로는 빠지지만(다시 돌린 FBX 3개), 저장소의 FBX 는 바꾸지 않았다(기하 · 동작은 같고 경로 문자열만 다름 — `P04_RESULT` 가 FBX 크기만 빼고 같음). 공개 저장소에 사용자 폴더 이름이 남는 문제의 처리는 사용자 · S3 결정이다(8절).
- 시위는 굽지 않는다 — 당긴 자세에서 오른손이 시위에서 떨어져 보인다. 시위 가운데를 오른손 뼈에 섞어 주는 방법은 Idle 에서 시위가 몸을 가로지르므로 하지 않았다.
- Unity 근접 캡처는 Lit 한 가지 · 에디터 활성 품질 PC 로만 찍었다(S1 과 같음). 모바일 품질 · 셀 · Unlit 의 인간형은 보지 않았다.
- 「× 300」의 성능(스킨 메시 300개의 CPU 스키닝 · 드로콜 · 기기 프레임)은 재지 않았다 — 숫자만.
- 비례: 키 1.62~1.63 m(슬라임 0.85 m 의 1.9배 · 탑 3.0 m 아래) · 머리 0.62~0.64 m(38~39 %)는 스크립트 치수로 계산한 값이고, 라인업 캡처에서 눈으로 확인했다.

## 7. 지시서와 다른 점 · 더 나은 방법 · 범위 밖 제안

- 다른 점(시작 전 알림): 지시서가 초안(`status: draft`)이라 빈 자리 셋을 채웠다 — ① 시작 커밋 = S1 끝 `9fcc860`(worktree HEAD · `git status` 빈 것 확인) ② 셰이딩 = 사용자 선택 기록이 없어 지시서 규칙대로 Lit ③ 참고 사실 = S1 보고 9절을 이 세션이 직접 읽어 씀. Planning 의 지시서 파일은 고치지 않았다(읽기 전용).
- 다른 점: 지시서는 「S1 의 공통 모듈」을 쓰라 했으나 S1 은 공통 모듈을 만들지 않았다(도우미 복사) — 같은 방식으로 복사했다. 세 스크립트가 같은 도우미(팔레트 · `set_cell` · `join` · `add_subsurf` · 크리스)를 들고 있다.
- 다른 점: 근접 캡처를 1장이 아니라 3장(얼굴 · 활 + 접힘 극단 둘 — 성공 기준 3 (a) 의 「극단 프레임의 캡처 확대」)으로 찍었다. 캡처 파일 이름 접두는 S1 과 섞이지 않게 `P04S2_`.
- 다른 점: 성공 기준 4 의 「스킨 플래그 켠 녹화기」는 `P03FrameRecorder.Record` 가 슬라임 · 박쥐 묶음만 알아서, 같은 규칙의 `P04HumanoidCapture.Record` 를 새로 썼다(P03 녹화기는 고치지 않음).
- 다른 점: 팔레트에 새 셀이 없어(4×4 가득 · 머티리얼 셋이 공유 — 계약) 기존 셀을 빌렸다 — 뼈 = 눈흰자 · 눈 = 눈동자 · 활 = 나무 · 시위 = 흰색 · 좀비 피부 = 잔디 진함 · 셔츠 = 박쥐 몸 · 바지 = 나무.
- 더 나은 방법: 자세를 오일러 각 키 대신 「뼈 방향 · 손목 · 발목 목표」로 적고 IK 로 풀어 `pose_bone.matrix` 를 맞추니 발이 바닥에 붙은 걷기 · 시위 잡기를 수치 몇 개로 적을 수 있었다 — 동작을 늘릴 때(`DEF-M03`) 같은 방식이 유지보수에 낫다.
- 범위 밖 제안: ① Blender 도우미를 `Tools/Blender/p04_common.py` 로 뽑기(세 스크립트 산출 불변 확인 포함) ② 시위를 별도 메시로 두고 가운데 정점을 오른손 뼈에 Shoot 동안만 따르게(제약 · 셰이프 키) ③ 인간형 셀 · Unlit 캡처 한 벌.

## 8. 발견 — `DEF-M03` 에 넘길 것(인간형 파이프라인의 정답 경로)

1. **뼈대**: 모델이 여럿이면 뼈 이름을 같게 두되(클립 공유 가능) **FBX 「모든 동작」 내보내기 전에 동작마다 모델 표지 사용자 속성 키를 넣는다** — `arm[arm.name] = 0.0; arm.keyframe_insert(data_path=f'["{arm.name}"]', frame=start)`. 안 하면 FBX 마다 모든 모델의 동작이 들어간다. 다른 길은 모델마다 Blender 파일을 따로 두는 것.
2. **가중치**: 팔다리는 관절 둘레에 고리(±2 · ±4.5 cm)를 몰아 넣은 이어진 관 + `ARMATURE_AUTO`. 떨어진 작은 조각(눈 · 갈비 · 발 · 장식)은 bone heat 가 못 주므로 **섬 단위로 가장 가까운 뼈에 1.0**(정점 단위로 메우면 큰 조각의 바깥 정점이 엉뚱한 뼈에 붙는다). 발 사이 바닥의 `Root` 는 `use_deform = False`.
3. **접힘 한계**: 자동 가중치 + Subsurf 2단계에서 팔꿈치 111° 두께 0.66배 · 무릎 72° 0.86배 — 100° 를 넘는 접힘은 눈에 띄게 얇아진다. 더 접는 동작이 필요하면 관절 고리를 더 넣거나 보조 뼈 · 교정 셰이프 키.
4. **소품**: 별도 메시 + Armature 모디파이어 + 손 뼈 정점 그룹 1.0 → Unity 에서 두 번째 `SkinnedMeshRenderer` 로 정확히 따라온다. 손 뼈 방향이 소품의 앞이므로 쥐는 자세에서 손목(손 뼈)을 따로 돌린다.
5. **동작 반복**: 키 프레임의 처음과 끝을 같은 자세로 두고 프레임 범위를 「주기 + 1」(24 프레임 주기면 1~25)로 — Blender · FBX · Unity 모두 첫 · 끝 차이 0, `loopTime` 클립 길이 1.0 s.
6. **Unity 가져오기**: S1 설정 그대로 + `animationType = Generic`. 뼈대 오브젝트 X 270° 는 남는다(P03 ②). 한 메시 안에서 팔레트 셀이 바뀌는 곳은 UV 이음매라 정점이 갈린다(좀비 +192, 1.9 %).
7. **캡처 · 녹화**: 한 호출 안에서 자세를 바꿔 여러 장 찍으면 스킨 메시마다 `forceMatrixRecalculationPerRender = true` — 안 켜면 둘째 장부터 앞 장 자세(바이트까지 같음).
8. **수(數)**: 치비 인간형 2단계 2만~2.8만 삼각형(거친 메시 1.2천~1.7천의 약 16배). 슬라임의 2.4~3.4배.
9. **FBX 의 로컬 경로**: Blender FBX 내보내기는 `.blend` 파일 경로(저장된 경우)와 텍스처 절대 경로를 문자열로 넣는다(`path_mode='STRIP'` 이어도). 공개 저장소에 넣을 FBX 는 내보내기 전에 저장하지 않거나 텍스처 경로를 상대로 두고, 커밋 전에 `strings <fbx> | grep Users/` 로 확인한다. P03 · S1 · S2 FBX 가 모두 해당 — 다시 내보낼지는 사용자 결정.

## 9. 코드 리뷰

위험 등급: 저(지시서). 결정론 게이트: 컴파일 오류 0 · 경고 0(열린 에디터 `consoleWarnings 0`) · EditMode 2 → 2 통과, 실패 0(열림 `4130cd2` 전 · 닫힘 `4130cd2` 전 · 닫힘 `1eb9fc8` 전 — 마지막은 스크립트가 다시 컴파일된 실행에서 `warning CS` 0줄).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `9fcc860..4130cd2` | 코드 5파일(bpy 1 · `.cs` 4) 약 +1,560 | 세션 센서(A opus) | 1 · 52.1만(도구) / 19.7만(알림) · 9분 48초 | 14 | 5 | 0 | 0 | 0 | 9 | 0 |
| Unity 2라운드 | `4130cd2..1eb9fc8` | 3파일 +41/−39 | 세션 센서(A opus) | 1 · 13.6만(도구) / 8.8만(알림) · 2분 11초 | 5 | 0 | 0 | 0 | 2 | 3 | 0 |

원본 · 처리표: `z.docs/work_reports/20261102/코드리뷰_p04_s2/`(처리.md 19행) — 지시서대로 worktree 안(S3 가 Planning 으로 옮긴다). 1라운드 important 넷(UNI.01~04)은 모두 plausible 이라 규칙상 처리 안 함이지만, 보고의 주장에 걸려 UNI.02 · 03 · 04 는 보고 서술(4절 (e) · ⑨ 행 · 6절)을 고쳤고 UNI.01 은 이음매 속도를 실측해 붙였다.

사람이 볼 행(blocking · important 이면서 new, 미해결 — 처리 안 함 · 범위 밖 포함):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P04.S2.UNI.01 | issue · important · new | TEST | 반복 이음매 속도 끊김(plausible) | make_humanoids_smooth.py:509,522-535 | 처리 안 함 | 실측 — 궁수 이음매 3.32° · 3.17° = 주기 가운데 디딤 3.23°, 정지 없음(4절 ⑨) |
| P04.S2.UNI.02 | question · important · new | TEST | 두께 정의와 주장 불일치(plausible) | make_humanoids_smooth.py:728-765 | 처리 안 함 | 6절에 정의의 한계 |
| P04.S2.UNI.03 | issue · important · new | TEST | 늘 0 인 값을 루트 모션 증거로(plausible) | P04HumanoidCapture.cs:177-182 | 처리 안 함 | 4절 (e) 근거를 `motionNodeName` 하나로 고침 |
| P04.S2.UNI.15 | issue · important · new | CONTRACT | 커밋 FBX ≠ 스크립트 산출 · 로컬 절대 경로 | make_humanoids_smooth.py:888-892 · FBX 2 | 범위 밖 | S1 · P03 FBX 에도 같고 다시 내보내도 텍스처 경로가 남음 — 사용자 · S3 결정(6절 · 8절 9번) |

판정 반론: 틀림 0 · 의도 0 → 생략. 종료 조건: blocking 0 · important 는 전부 고침 또는 사유(plausible 3 · 범위 밖 1). 3라운드 없음.
