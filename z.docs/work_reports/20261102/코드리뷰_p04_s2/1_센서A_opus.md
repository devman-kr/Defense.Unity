---
id: 20261102-aadf6d56-final
title: 코드리뷰 — 에이전트 aadf6d56 최종 보고 원문
summary: 「P04 S2 세션 센서 리뷰」 에이전트(claude-opus-5-5 · 도구 호출 27)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | issue | important | new | 반복 클립 이음매에서 속도가 끊김 | `Tools/Blender/make_humanoids_smooth.py:509` · `:522-535` · `:693` · `:700` | Walk(궁수 1~25 · 좀비 1~33)와 Idle 은 키를 frame_range 양 끝에만 넣는다. F-Curve 에 Cycles 모디파이어도, 핸들 지정도 없다 → 외삽이 Constant 이면 Blender 자동(클램프) 핸들은 첫 키와 끝 키에서 수평으로 눕는다. 그러면 이음매에서 모든 채널의 속도가 0 이 되고, FBX 가 매 프레임 구운 값 그대로 Unity 에 간다 → 궁수는 1 초, 좀비는 1.33 초마다 걷는 발이 잠깐 멈칫한다. 「Walk 첫 · 끝 자세 차이 0」(`:793` · `P04HumanoidCapture.cs:168-175`)은 자세만 비교하고 속도는 보지 않아 이것을 못 잡는다 | plausible |
| 2 | question | important | new | 측정 정의가 「두께」 주장과 맞지 않음 | `Tools/Blender/make_humanoids_smooth.py:728-765` | 쉼 두께는 고리 정점의 X 폭과 Y 폭의 평균이다(관 축에 수직). 접힘 두께는 안쪽 이등분 방향 `b = l - u` 로 투영한 폭이다. 그런데 `b` 는 위 뼈 축과 sin(θ/2) 만큼 겹친다(111° 에서 0.82). 위쪽 고리 정점(j − s·u)과 아래쪽 고리 정점(j + s·l)은 둘 다 `b` 쪽으로 s·sin(θ/2) 만큼 밀린다 → 고리를 고를 때 쓴 축 방향 폭 ±0.025 m 가 투영 폭에 최대 약 0.02 m 섞인다(쉼 지름 0.087 m 대비). 그래서 「0.663 배」가 단면이 얇아진 것인지 고리가 기운 것인지 구분되지 않는다 | plausible |
| 3 | issue | important | new | 구조상 항상 0 이 나오는 측정을 루트 모션 증거로 씀 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:177-182` | `modelRootMoved` 는 `instance.transform.position` 이다. `motionNodeName` 이 비어 있어 클립 곡선은 자식 경로(`…Rig/Root/Hips` 등)에만 있고, 모델 루트(경로 "")를 움직이는 곡선이 없다 → `SampleAnimation` 은 루트 모션이 있든 없든 0.000000 을 낸다. 보고서 (e) 의 「반 주기 샘플 뒤 모델 루트 이동 0」은 증거가 되지 못한다. 남는 근거는 `motionNodeName` 이 빈 값이라는 것 하나다 | plausible |
| 4 | question | nit | new | 반복 클립을 길이 시각에서 샘플하면 0 초로 감길 수 있음 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:162-175` | `walk.SampleAnimation(instance, walk.length)` — Unity 가 loopTime 켠 클립의 길이 시각을 0 으로 감는다면, 첫 · 끝 비교는 같은 시각끼리의 비교라 항상 0 이다 → Unity 쪽 미지수 ⑨ 의 증거가 동어반복이 된다. `AnimationUtility.GetEditorCurve` 로 첫 키와 끝 키 값을 직접 비교하면 확실하다 | plausible |
| 5 | thought | nit | pre-existing | 애니메이션 압축이 기본값으로 걸려 있음 | `Assets/10_Scripts/Editor/P04/P04ModelImport.cs:135-155`(ApplyModel) · `Zombie.fbx.meta:31-33` | `animationCompression` 을 정하지 않아 `1`(KeyframeReduction, 회전 오차 0.5°)이 적용됐다 → Unity 무릎 각 62.1° · 72.2° 가 Blender 61.7° · 71.9° 와 0.3~0.4° 어긋난다(팔꿈치 111.3° 는 같다). 「Blender = Unity」를 확인하는 측정이라면 압축을 끄거나, 이 어긋남의 원인으로 적어야 한다 | confirmed |
| 6 | suggestion | nit | new | 가져오기 설정을 다른 참조에 쓰고 저장은 다시 얻은 참조로 함 | `Assets/10_Scripts/Editor/P04/P04ModelImport.cs:74-77` · `:137` | `ApplyHumanoids` 는 `GetAtPath` 로 얻은 importer 에 `animationType` 을 쓰고, `ApplyModel` 은 `GetAtPath` 를 다시 불러 `SaveAndReimport` 한다 — 두 번 모두 같은 인스턴스가 온다는 가정이다. 게다가 새 FBX 의 기본값도 Generic 이라, 출력의 `animationType=Generic` 만으로는 이 설정이 실제로 적용됐는지 구분되지 않는다 | plausible |
| 7 | thought | nit | new | IK 를 키 프레임에서만 풀고, 다리가 최대 길이에서 시작함 | `Tools/Blender/make_humanoids_smooth.py:457-461` · `:553-558` · `:669` · `:693` · `:700` | 쉼 자세에서 다리 길이(0.26 + 0.28 = 0.54)가 엉덩이~발목 거리와 정확히 같다 → 엉덩이가 조금만 올라가도 다리가 다 펴진다(허벅지 IK 19 회). 좀비 Walk 는 Hips Y 6° 기울임으로 엉덩이 한쪽이 약 0.0125 m 올라가 디딘 발이 뜰 수 있다. 키 사이(3 · 4 프레임)는 쿼터니언 보간이라 발이 바닥에서 미끄러지거나 바닥을 뚫을 수 있다. 발 높이를 프레임마다 재지는 않는다 | plausible |
| 8 | nitpick | nit | new | 저장한 .blend 가 정본 FBX 를 낸 상태와 다름 | `Tools/Blender/make_humanoids_smooth.py:859`, `:887-892` | `.blend` 는 모델 표지 키를 넣기 전에 저장된다 → 그 `.blend` 로 다시 내보내면 상대 모델 동작이 섞인 naive FBX 가 나온다. 산출 `.blend` 를 이어 쓰는 사람이 같은 함정에 다시 걸린다 | confirmed |
| 9 | thought | nit | new | Blender 가 실험적이라고 경고한 옵션 사용 | `Tools/Blender/make_humanoids_smooth.py:864` | `bake_space_transform=True` 는 Blender 5.2 툴팁에 "known to be broken with armatures/animations" 라고 적혀 있다(`io_scene_fbx/__init__.py:92`). 실제로 Unity 뼈대 오브젝트에 X 270° 가 남았다. 이번 검증은 접힘 각 프레임 넷과 활 거리 프레임 다섯뿐이다 — S1 · P03 과 같은 선택이지만 인간형 동작 전체에 대해서는 확인되지 않았다 | confirmed |
| 10 | suggestion | nit | new | Blender 실측 결과를 C# 에 손으로 옮긴 상수 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:32-35` · `:107-111` | 최대 접힘 프레임(24 · 25 · 19 · 10)을 `P04_RESULT` 에서 손으로 옮겼다. 19 와 10 은 리터럴이고, `WALK_MAX_KNEE_FRAME`(25)은 좀비 전용인데 이름은 공용이다 → Blender 동작을 고치면 C# 이 다른 프레임을 재고도 「같다」를 출력할 수 있다 | confirmed |
| 11 | nitpick | nit | new | 두 파일이 같은 오브젝트 이름 리터럴을 따로 적음 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:62-63` · `Assets/10_Scripts/Editor/P04/P04HumanoidSceneBuilder.cs:122` | 라인업5 자식 이름 "Archer" · "Zombie" 를 두 파일이 각자 적는다 → 빌더 쪽 이름을 바꾸면 `Find` 가 null 을 돌려 NRE 가 난다. 상수로 공유하는 것이 §1.3 취지에 맞다 | confirmed |
| 12 | nitpick | nit | new | 코딩 표준 §3 멤버 순서 위반 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:115-133` | 중첩 타입 `JointProbe` 가 메서드 사이에 있다. §3 ⑩ 은 중첩 타입을 클래스 끝에 둔다(`Clips` 는 끝에 있다) | confirmed |
| 13 | nitpick | nit | new | 좌표 공간을 섞어 씀 | `Assets/10_Scripts/Editor/P04/P04HumanoidSceneBuilder.cs:101-108` | `subject.localPosition` · `localRotation`(그룹 `Archers` · `Zombies` 기준)으로 계산한 위치를 `P04Humanoids` 루트 바로 아래 카메라의 localPosition 으로 쓴다 → 그룹이 항등 변환일 때만 맞는다. 그룹을 옮기거나 돌리면 카메라가 대상을 놓친다 | confirmed |
| 14 | thought | nit | new | 잠재 결함: 메우는 섬의 단위가 조각이 아니라 「가중치 없는 정점 덩어리」 | `Tools/Blender/make_humanoids_smooth.py:284-308` | 섬은 가중치 없는 정점끼리만 이은 연결 성분이다 → 한 조각의 일부만 bone heat 가 실패하면 그 일부만 가까운 뼈에 1.0 강체로 붙어, 가중치가 있는 이웃 정점과의 경계에서 찢어진다. 이번 실행에서는 메운 섬이 갈비 48 · 눈 42 · 발 8 정점으로 조각 전체였다(`z.captures/20261102/p04/renders_s2/p04_result.json`) | confirmed |

## 확인하지 않은 것 · 추정한 것
- #1 의 「첫 · 끝 자동 핸들이 수평으로 눕는다(외삽 Constant, Cycles 모디파이어 없음)」는 Blender C 소스(F-Curve 핸들 재계산)에 대한 기억이다. 이 기기에서 실제 F-Curve 핸들 값이나 Unity 클립 곡선을 열어 보지 않았다.
- #3 · #4 는 Unity `SampleAnimation` 의 동작에 대한 추정이다. 루트 경로 곡선이 있는지(클립 바인딩), loopTime 클립을 길이 시각에서 샘플할 때 0 초로 감기는지 확인하지 않았다.
- #2 는 수식으로 추론한 것이다. 정점 데이터로 투영 폭을 다시 계산해 보지 않았다. 좀비 팔꿈치 고리에 몸통 정점이 섞일 가능성도 의심했으나, 쉼 두께 0.1083 m(팔 지름 0.12 m)를 보고 해당 없다고 판단해 표에서 뺐다.
- 고위험 경로: 해당 없음으로 판단했다. `git diff --name-status 9fcc860..4130cd2` 결과 `ProjectSettings/` · `Packages/` · `**/Save*` · 빌드 프로필 변경이 없다. `Editor/P04/*Builder*` 는 씬을 만드는 빌더이고 빌드 스크립트가 아니다.
- 범위 · 계약: 바뀐 파일 14 개가 모두 수정 허용 영역 안이다(`ASSETS.md` · `Tools/Blender/README.md` · 씬 포함, 다만 md · 씬은 읽지 않았다). `P04SampleSceneBuilder` 의 private → internal 은 접근성만 바꿨고 동작은 그대로다. `LOOP_CLIP_NAMES` 에 넣은 `Walk` 는 S1 동작 이름(Idle · Hop · Hit · Die · Flap — `make_enemies_smooth.py:350-375`)에 없어 S1 `Apply()` 의 결과를 바꾸지 않는다. `AppendMeshStats` 를 나눈 뒤에도 S1 출력 형식은 같다.
- FBX 바이너리와 `.unity` 는 열지 않았다. 인간형 루트 둘이 꺼진 채 저장되는지는 코드(`P04HumanoidSceneBuilder.cs:72-78`)와, S1 캡처가 자기 루트 셋만 켜고 끈다는 점(`P04Capture.cs:94-97`)으로만 확인했다. 캡처 이미지 · MP4 는 보지 않았다.
- 실측값(접힘 각 · 두께 · IK 펴짐 횟수 · 팔레트 해시 · FBX 크기 970828 / 771052)은 `p04_result.json` 과 작업 보고서에서 읽었다. 다시 돌리지 않았다.
- 성공 기준 1~4 의 항목마다 대응하는 코드는 있다. 3(e) 루트 모션만 근거가 약하다(#3).