---
id: 20261102-aece9c12-final
title: 코드리뷰 — 에이전트 aece9c12 최종 보고 원문
summary: 「P04 S1 세션 센서 리뷰」 에이전트(claude-opus-5-5 · 도구 호출 32)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
세션 모델: Opus 5.5(지시서 기준과 같다).

| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | question | important | new | 지시서 안의 두 조건이 서로 맞지 않아, 기존 P03 녹화 결과가 바뀐다 | `Assets/10_Scripts/Editor/P03/P03FrameRecorder.cs:25-29, 44-48` | 기존 5인자 `Record(outDir, …)` 도 새 오버로드를 거치므로 `forceMatrixRecalculationPerRender = true` 가 켜진다. 그래서 P03 녹화를 다시 돌리면 프레임 1~47 이 예전 산출과 달라진다(배경 사실). 수정 허용 영역은 「P03 씬 · 녹화 산출이 같아야 한다」라고 하고, 성공 기준 6 은 이 플래그를 켜라고 한다. 보고의 「지시서와 다른 점」에 사유(예전 산출이 늦은 자세였다)를 적지 않으면 계약 위반으로 읽힌다 | confirmed |
| 2 | suggestion | important | new | 셰이딩 비교에 다른 변수가 섞인다(조명 · SSAO 수신이 셰이더마다 다르다) | `Assets/30_Assets/Shaders/PaletteCel.shader:46-49, 1-2, 149` | 셀 패스에는 `_SCREEN_SPACE_OCCLUSION` · `_ADDITIONAL_LIGHTS` 키워드가 없다. 에디터 품질(`QualitySettings m_CurrentQuality: 1` = PC)의 `PC_Renderer` 는 SSAO 를 켜 두었다(`AfterOpaque: 0`, `Source: 1`). 그래서 Lit 은 등불 점광과 SSAO 를 받고, URP Unlit 도 SSAO 를 곱하지만(`UnlitForwardPass.hlsl:129-132`) 셀만 둘 다 받지 않는다. 캡처 6장에서 셀과 Lit · Unlit 의 차이가 일부 셰이딩이 아니라 AO · 등불 때문인데, 사용자는 그것을 셰이딩 차이로 보고 고르게 된다. 셰이더 머리 주석은 추가 조명 제외만 적고 SSAO 미수신은 적지 않는다(149줄 주석은 오히려 SSAO 가 동작하는 것처럼 읽힌다) | confirmed(키워드 · 설정) / plausible(캡처에서 보이는 크기) |
| 3 | thought | important | new | 성공 기준 3(b) 의 확인 지표가 실패할 수 없다 | `Assets/10_Scripts/Editor/P04/P04ModelImport.cs:135` | Unity `Mesh.normals.Length` 는 노멀이 있으면 항상 `vertexCount` 다. 「노멀 수 = 정점 수」는 정점이 갈라져도 참이다. 「이웃 면이 노멀을 공유한다」의 증거는 Unity 정점 수 = Blender 정점 수(슬라임 4206)라는 대조뿐이다. 보고가 `normals=` 줄을 증거로 들면 근거가 비어 있다 | confirmed |
| 4 | nitpick | nit | new | 이제 public 인 메서드가 null 을 돌려주는데 이름에 `OrNull` 이 없다(§6.1) | `Assets/10_Scripts/Editor/P03/P03FrameRecorder.cs:99-110` · 호출처 `P03FrameRecorder.cs:37-38` · `P04/P04Capture.cs:32-34, 99` | `GetRoot` 가 private 에서 public 으로 바뀌었고, 루트가 없으면 Assert 뒤 `null` 을 돌려준다. 호출처는 모두 null 검사 없이 `root.Find` · `target.Find` 를 부른다. 루트 이름을 바꾸거나 잘못 넘기면 Assert 다음에 NullReferenceException 이 난다 | confirmed |
| 5 | nitpick | nit | new | 일반화한 오버로드가 받는 입력이 넓어졌는데, 그 입력의 실패가 원인 없는 예외로 끝난다 | `Assets/10_Scripts/Editor/P03/P03FrameRecorder.cs:41-43, 56, 71` | 새 9인자 `Record` 에 클립 이름을 벗기지 않은 모델(`P04ModelImport.Apply` 를 돌리기 전 — 클립 이름이 `SlimeRig|Idle`)이나 Flap · Idle · Hop 이 없는 모델 경로를 넘기면 `batClips[FLAP_CLIP]` 에서 원인 메시지 없는 `KeyNotFoundException` 이 난다(P03 전용일 때는 입력이 고정이었다) | confirmed |
| 6 | nitpick | nit | new | 셰이딩 이름 문자열 리터럴이 세 파일에 흩어져 있다(§1.3 취지) | `P04/P04ModelImport.cs:64-65` · `P04/P04Capture.cs:26, 45` · `P04/P04SampleSceneBuilder.cs:77` | `"Lit"` · `"Unlit"` · `"Cel"` 을 따로 쓴다. 한 곳에서 오타가 나면(`"cel"`) Assert 하나만 찍히고, 조용히 Lit 머티리얼로 찍혀 파일 이름과 셰이딩이 어긋난다 | confirmed |
| 7 | suggestion | nit | new | 구도 상수를 복사해 두어 두 빌더가 어긋날 수 있다 | `Assets/10_Scripts/Editor/P04/P04SampleSceneBuilder.cs:30-69` | 「P03SampleSceneBuilder 와 같은 값」 상수 20개가 복사본이다(지금은 값이 모두 같음을 확인했다). P03 쪽 구도를 고치면 P04 비교 씬이 말없이 다른 구도가 되고, 「P03 샘플 씬 구도 그대로」(성공 기준 5)가 깨진다 | confirmed |
| 8 | thought | nit | new | 크리스 각 기준이 의도하지 않은 부품에도 걸린다 | `Tools/Blender/make_enemies_smooth.py:16, 161, 269, 276` | 탑 `Tip`(UV 구, seg=6 · rings=4)은 같은 띠의 이웃 사각면 사이 각이 약 55° 로 `EDGE_CREASE_ANGLE` 50° 보다 커서, 세로 모서리 6개에 크리스 0.75 가 걸린다. 그러면 꼭지 구슬에 세로 능선이 남는다. 도우미 docstring 이 든 대상(테두리 · 각기둥 · 날개)에는 구가 없다. 계산으로만 확인했고 렌더는 보지 않았다 | plausible |
| 9 | thought | nit | new | 「같은 형태」와 크기가 달라질 수 있다 — 서브디비전이 작은 구를 줄인다 | `Tools/Blender/make_enemies_smooth.py:180-182, 216-219, 269` | Catmull-Clark 의 극한 곡면은 제어 메시 안쪽에 생긴다. 분할이 적은 구(눈동자 seg6/rings4 · 볼 · Tip)일수록 많이 줄어, 매끈 판의 눈동자 · 볼 · 꼭지가 로우폴리보다 작고 몸에 묻힐 수 있다. 보정이 없고, 라인업 비교에서 「매끈함」과 「크기 차이」가 섞인다 | plausible |
| 10 | nitpick | nit | new | 그림자 페이드가 빠졌다 | `Assets/30_Assets/Shaders/PaletteCel.shader:97` | `GetMainLight(shadowCoord)` 는 `MainLightRealtimeShadow` 만 쓰고 그림자 페이드를 하지 않는다(`RealtimeLights.hlsl:102-107`). Lit 은 `GetMainLight(shadowCoord, positionWS, shadowMask)` 로 페이드한다. 그림자 거리 50 m 경계 근처에서 셀만 그림자가 뚝 끊긴다. 지금 캡처 구도(카메라 거리 19 m)에서는 드러나지 않는다 | confirmed |
| 11 | thought | nit | new | 캡처 품질이 목표 기기 품질과 다르다 | `ProjectSettings/QualitySettings.asset:7, 119-133` · `Assets/Settings/Mobile_RPAsset.asset:58, 66` | 에디터 캡처는 PC 품질(SSAO · 소프트 그림자 · 캐스케이드 4)로 렌더된다. 목표인 Android 기본 품질 0 = Mobile 은 SSAO 없음 · 소프트 그림자 끔 · 캐스케이드 1 이다. 사용자가 고르는 그림은 모바일에서 실제로 보일 모습과 다르다 | plausible |
| 12 | thought | nit | new | 고위험 경로 해당 여부 판단 | `Assets/10_Scripts/Editor/P04/P04SampleSceneBuilder.cs` | `Editor/*Build*` 보다 한 단계 깊다(`Editor/P04/`). diff 의 `.cs` 에 `BuildPipeline` · `EditorBuildSettings` · `PlayerSettings` · `BuildProfile` · `EditorUserBuildSettings` 사용이 0건이다(grep). 씬을 만드는 빌더일 뿐 빌드 설정을 건드리지 않으므로 해당 없음으로 본다. diff 파일 목록 전부가 수정 허용 영역 안이고 `ProjectSettings/` · `Packages/` · `Assets/Settings/` 변경은 0이다 | confirmed |

## 확인하지 않은 것 · 추정한 것
- FBX · PNG 바이너리, 씬(`.unity`), `.md`(README · ASSETS · 보고)는 지시대로 읽지 않았다. 캡처 · 렌더 그림도 보지 않았다. 그래서 #2 · #8 · #9 · #11 이 화면에서 얼마나 보이는지는 추정이다.
- #2 · #11 은 캡처 때 에디터의 활성 품질이 `m_CurrentQuality: 1`(PC)이라는 가정에 기댄다. 활성 빌드 대상이 Android 였으면 Mobile 품질이라 SSAO 가 없고, 이때 #2 는 등불 점광 차이만 남는다.
- `P04Capture.MeasureHop` 의 시간 환산 `(frame - 1) / hop.frameRate` 는 「Unity 클립 프레임 0 = Blender 프레임 1」을 가정한다. `.meta` 의 Hop `firstFrame 0 · lastFrame 23` 과 스크립트의 Hop 키 범위 1~24 가 맞아 보여 지적하지 않았다. FBX take 의 시작 시각을 직접 읽지는 않았다.
- Blender FBX 내보내기가 Armature 모디파이어를 끄지 않고 뼈대를 REST 로 둔 채 Subsurf 를 적용해 정점 그룹을 남긴다는 것은 Blender 5.2 `io_scene_fbx/export_fbx_bin.py:2702-2722` 에서 확인했다. 실제 FBX 안의 가중치는 보지 않았다(배경 사실과 `P04_RESULT.reimport` 를 믿었다).
- 비편집 가능 메시(`isReadable: 0`)에서 `Mesh.uv` · `BakeMesh` 가 에디터에서 동작하는지는 실행으로 확인하지 않았다. 세션이 돌린 결과가 있다고 보고 지적하지 않았다.
- 셀 셰이더의 `_CASTING_PUNCTUAL_LIGHT_SHADOW` 변형은 그림자를 드리우는 점광이 씬에 없어 이번 세션에서 컴파일되지 않았을 수 있다. include 원본(`ShadowCasterPass.hlsl`)을 읽어 필요한 선언이 다 있음을 확인했으므로 컴파일 오류 가능성은 낮다고 추정한다.
- 셀 셰이더에 `_LIGHT_LAYERS` · `LOD_FADE_CROSSFADE` · DOTS 인스턴싱 · Meta · MotionVectors 패스가 없다. 지금 설정(GPU Resident Drawer 끔 · 베이크 없음)에서는 영향이 없다고 보고 표에 넣지 않았다.
- 1 · 2 · 3 각도(정확성 · 파일 사이 · 누락)는 diff 범위의 `.cs` 4 · `.shader` 1 · `.mat` 3 · `.py` 1 을 `8e8f3ad` 시점 전문으로 읽었다. `make_enemies.py` 와의 차이는 `diff` 로 끝까지 봤다(도우미 복사 · 함수화 외에 동작 변경 없음). 성공 기준 1~6 가운데 코드 대응이 없는 항목은 찾지 못했다.
