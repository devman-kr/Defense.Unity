---
id: 20261102-aba05216-final
title: 코드리뷰 — 에이전트 aba05216 최종 보고 원문
summary: 「P04 S3 세션 센서 리뷰」 에이전트(claude-opus-5-5 · 도구 호출 40)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
세션 모델: Opus 5.5(지시서 기준과 같음)

| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | issue | important | new | 씬 준비로 만든 루트를 ShowOnly 가 끔 — 톤매핑 볼륨이 적용되지 않음 | Assets/10_Scripts/Editor/P04/P04ToonCapture.cs:92,97,106,151 · :297-298 · P04HumanoidCapture.cs:225-230 · P04ToonTiles.cs:146-150 | `AddGlobalVolume` 이 `new GameObject("ToonVolume")` 를 씬 **루트**로 만들고(297-298), 그 뒤 `ShowOnly(scene, smooth, humanoids)`(97) · `ShowOnly(scene, lineup5)`(106)이 keep 에 없는 루트를 전부 `SetActive(false)` 한다 → Volume 이 OnDisable 로 빠져 T12_Neutral 이 T12_PostOnly(후처리만 켬)와 같은 그림이 된다. 승인 ③ 톤매핑 줄의 근거 그림이 실제로는 Neutral 을 비교하지 못했고, 보고 8절이 「Neutral 의 회색」을 후처리 경로 탓으로 본 해석도 이 원인을 빠뜨렸다. `Record`(151)도 같은 순서라 볼륨을 넘기면 똑같이 빠진다 | confirmed |
| 2 | suggestion | important | new | 램프 텍스처를 켜면 단계 손잡이가 동작하지 않음(값이 두 곳에 중복) | Assets/30_Assets/Shaders/PaletteToon.shader:183-191 · P04ToonAssets.cs:28-32,100-101,109 · PaletteToon.mat(`_UseRamp 1` · `_MidLevel 0.6`) | 사용자 선택은 「3단 · 경계 0.02」와 「램프 켬」이다. `_RAMP_TEXTURE` 가 켜지면 `_LowStep` · `_HighStep` · `_MidLevel` 은 쓰이지 않고, 곡선은 C# 상수 `RAMP_*` 로 구운 PNG 가 정한다. `_StepSoftness` 는 그림자 단 · 림 · litSide 에만 남는다 → DEF-M03 에서 머티리얼의 `_MidLevel` 을 1(2단)로 바꾸거나 경계 폭을 바꿔도 명암 단이 그대로다. 「3단 · 0.02」가 머티리얼 값 · C# 상수 · PNG 세 곳에 있다 | confirmed |
| 3 | suggestion | important | new | 캐릭터 표시에 「Light Layer 1」 비트를 다른 뜻으로 겹쳐 씀 | PaletteToon.shader:107 · P04ToonCapture.cs:35,241 · ProjectSettings/TagManager.asset:46 · Assets/Settings/Mobile_RPAsset.asset:77 · PC_RPAsset.asset:77 | 렌더링 레이어 비트 1(값 2)은 TagManager 에 `Light Layer 1` 로 이름이 붙어 있고 두 RP 에셋 모두 `m_SupportsLightLayers: 1` 이다. DEF-M03 에서 등불 점광 같은 조명에 Light Layer 1 을 쓰면 그 조명을 받는 렌더러가 전부 「캐릭터」로 바뀌어 림 · 스페큘러 · 자기 그림자 끔이 섬 · 탑에도 걸린다. 셰이더 `#define` 과 C# `const` 두 곳에만 있고 이름 붙은 레이어나 공용 상수(§1.3 `GameIds`)가 없다 | confirmed |
| 4 | issue | important | new | 기준 칸 라벨이 코드에 고정돼 지금 머티리얼과 다름 | Assets/10_Scripts/Editor/P04/P04ToonTiles.cs:98,104-120 · P04ToonAssets.cs:97-115 | `ApplyApprovedDefaults` 이후 `PaletteToon.mat` 은 림 · 스페큘러 · 램프 · 자기 그림자 끔이 켜져 있다. 그런데 `GetBaseCaption` 은 「RIM: OFF (BASE)」 「SELF SHADOW: RECEIVE (BASE)」 「RAMP TEX: OFF (BASE)」를 그대로 낸다. 시트 제목도 「BASE = CURRENT PALETTETOON.MAT」라고 적는다 → HEAD 에서 `CaptureAll` + `ComposeSheets` 를 다시 돌리면(S4 검산 등) 라벨이 그림과 반대인 시트가 나오고, T08 · T09 · T11 · T07 칸은 기준과 같은 그림이 된다 | confirmed |
| 5 | thought | nit | new | 고른 조합 가운데 씬 쪽 요소는 캡처 도구 안에만 있음 | P04ToonCapture.cs:88,234-246,252-288 · `Assets/00_Scenes/P04_Smooth_Sample.unity`(범위 안 변경 없음) | 캐릭터 렌더링 레이어 표시와 블롭 쿼드 · 캐릭터 그림자 끔은 캡처 때 씬 인스턴스에만 들어간다. 저장된 씬 · 프리팹으로 열면 `PaletteToon.mat` 의 림 · 스페큘러 · 자기 그림자 끔이 아무 렌더러에도 걸리지 않는다. 성공 기준 「DEF-M03 이 그대로 쓸 수 있다」는 부분 충족이다. 보고 8절에 적혀 있다 | confirmed |
| 6 | issue | nit | new | Record 가 Options 일부를 말없이 무시하고 사본을 해제하지 않음 | P04ToonCapture.cs:127,137-141,156-164 | `Record(..., options)` 에 `RendererIndex` · `IsPostProcessing` 을 주어도 `SetCameraRendering` 을 부르지 않아 기본 렌더러 · 후처리 끔으로 찍힌다 → 같은 Options 로 찍은 캡처와 MP4 가 다를 수 있다(이번 최종 조합은 후처리 끔 · 외곽선 A 라 영향 없음). `new Material(material)` 사본은 `DestroyImmediate` 하지 않는다(CaptureSet 110-113 과 다름) | confirmed |
| 7 | suggestion | nit | new | 예외 경로에서 품질 단계 · 씬이 복원되지 않음 | P04ToonCapture.cs:64-116 · 131-168 | `Find` 결과가 null 이거나 클립이 없어 루프 중간에 예외가 나면 `QualitySettings.SetQualityLevel(originalQuality)` 와 씬 다시 열기를 건너뛴다 → 활성 품질이 Mobile 0 으로 남아 `ProjectSettings/QualitySettings.asset` 이 바뀐 채 커밋될 위험이 있다(계약 「ProjectSettings 불변」). try/finally 가 없다 | confirmed |
| 8 | issue | nit | new | 경계값 — 그림자 단 문턱 + 폭이 1 을 넘음 | PaletteToon.shader:23,28,167,178 | `_ShadowStep` 은 0.99 까지, `_StepSoftness` 는 0.2 까지 허용한다. 예를 들어 0.9 + 0.2 에서는 `smoothstep(0.7, 1.1, 1.0) < 1` 이다 → `_SELF_SHADOW_OFF` 캐릭터나 그림자 없는 면도 shadowBand 가 1 보다 작아 어두운 단 쪽으로 섞인다. 지금 값(0.5 · 0.02)에서는 생기지 않는다 | confirmed |
| 9 | issue | nit | new | normalize(0) | PaletteToon.shader:34,172 | `_FixedLightDirection` 을 (0,0,0)으로 두고 `_FIXED_LIGHT_DIR` 를 켜면 `normalize` 가 NaN 이 되어 캐릭터 픽셀이 검정 또는 NaN 으로 나온다. 기본값은 0 이 아니다 | confirmed |
| 10 | issue | nit | new | 받는 그림자 양자화를 끄면 그림자 안에도 림 · 스페큘러가 남음 | PaletteToon.shader:179-182,199 | `_SHADOW_BANDS` 가 꺼지면 `shadowBand = 1` 이 되고, `litSide` 는 그림자를 뺀 `nDotL` 만 본다 → 다른 물체 그림자 안에 들어간 캐릭터에도 림 · 하이라이트가 그대로 그려진다(「빛 받는 쪽만」 주석과 다름). 지금 머티리얼은 양자화가 켜져 있다 | confirmed |
| 11 | issue | nit | new | [Toggle] 기본값 1 과 키워드가 어긋날 수 있음 | PaletteToon.shader:27 · P04ToonAssets.cs:40 | `new Material(Shader.Find("Defense/PaletteToon"))` 로 만든 머티리얼은 `_ShadowBands = 1` 이지만 `_SHADOW_BANDS` 키워드는 꺼져 있다(인스펙터를 열기 전까지). 그래서 인스펙터는 「켬」인데 그림은 양자화 없음이 된다. 이 diff 의 `CreateMaterials` 는 키워드를 직접 켜서 피했다 | plausible |
| 12 | issue | nit | new | 원근 전용 깊이 선형화 | Assets/30_Assets/Shaders/ToonEdgeDetect.shader:54-57 | 직교 카메라에서 `LinearEyeDepth(raw, _ZBufferParams)` 는 틀린 깊이를 준다 → 상대 깊이 차가 어긋나 외곽선 B 가 빠지거나 과해진다. 지금 카메라는 원근이다 | confirmed |
| 13 | issue | nit | new | 프리팹 인스턴스가 아니면 NRE | P04ToonCapture.cs:261,265-266 | 스킨 메시가 프리팹 인스턴스 밖에 있으면 `GetOutermostPrefabInstanceRoot` 가 null 을 돌려준다 → HashSet 에 null 이 들어가 265행 `character.GetComponentsInChildren` 에서 NRE 가 난다. 지금 씬은 `PrefabUtility.InstantiatePrefab` 으로 만들어져 걸리지 않는다 | plausible |
| 14 | nitpick | nit | new | §5.6 위반 — `new` 뒤 타입 생략(암시 형식 배열) | P04ToonCapture.cs:83,142,202 · P04ToonTiles.cs:129 · P04ContactSheet.cs:129-175 | `new[] { ... }` — 표준 §5.6 「`new` 뒤 명시적 타입」 | confirmed |
| 15 | nitpick | nit | new | §6.8 위반 — switch `default:` 에 `Debug.LogError` 가 없음 | P04ToonTiles.cs:121-123 | 모르는 손잡이 이름이 오면 `Debug.Assert(false)` 만 있고 LogError 가 없다 → 단언이 빠진 빌드 설정에서는 조용히 「BASE」를 낸다 | confirmed |
| 16 | nitpick | nit | new | §7.1 · §1.2 위반 — 공용 클래스의 public 가변 필드 | P04ToonCapture.cs:45-51 | `Options.Configure` · `PrepareScene` · `Qualities` · `RendererIndex` · `IsPostProcessing` 가 public 필드다. `Qualities` 의 기본값은 static readonly 배열 `BOTH_QUALITIES` 를 그대로 공유해, 받은 쪽이 원소를 바꾸면 이후 모든 Options 의 기본값이 바뀐다 | confirmed |
| 17 | nitpick | nit | new | 문서 주석 위치가 어긋남 | P04ToonCapture.cs:120-128,182 | `MeasureCost` 의 `<summary>`(120-123)가 `Record` 위에 붙어 `Record` 에는 summary 가 둘이고 `MeasureCost`(182)에는 없다 | confirmed |
| 18 | question | nit | out-of-scope | 수정 허용 영역 목록 밖 파일 | Assets/30_Assets/Materials/BlobShadow.mat | 지시서 「작업 위치」의 Materials 허용은 `PaletteToon.mat`(+ 외곽선 공용 머티리얼)뿐이다. 블롭 머티리얼은 목록에 없다(모델별 머티리얼은 아니므로 계약 위반은 아님). 허용으로 볼지 판단이 필요하다 | confirmed |

## 확인하지 않은 것 · 추정한 것
- 셰이더 컴파일 결과(오류 · 경고 0)와 변형 수는 다시 재지 않았다. HLSL 은 읽기로만 따졌다. `GetMeshRenderingLayer()`(`ShaderVariablesFunctions.hlsl:608`)와 `TransformWorldToShadowCoord` 의 화면 그림자 분기(`Shadows.hlsl:356-`)는 URP 17.3 소스로 확인했다.
- #1 은 코드 순서(PrepareScene → ShowOnly)와 `ShowOnly` 구현으로 판정했다. 실제 T12 타일 PNG 두 장이 픽셀 단위로 같은지는 열어 보지 않았다.
- #3 은 Light Layer 1 을 DEF-M03 이 쓸 경우의 위험이다. 지금 씬 조명의 렌더링 레이어 마스크는 확인하지 않았다.
- `Toon_PC_Renderer` 의 SSAO 하위 에셋에 `m_BlueNoise256Textures` · `m_Shader` 가 없는 것은 URP 17.3 에서 두 필드가 직렬화되지 않는 private 필드라서다(`ScreenSpaceAmbientOcclusion.cs:126-127`, 리소스는 GraphicsSettings 에서 받음). 결함이 아니라고 판단해 표에 넣지 않았다.
- `Assets/Settings/` 변경은 새 렌더러 둘 + RP 에셋 목록 1줄씩, `m_DefaultRendererIndex: 0` 그대로다. 새 렌더러는 원본 복제에 기능 1개와 에셋 버전 3 재직렬화만 더했다 ✔(diff 로 확인). 허락된 형태를 벗어난 곳은 찾지 못했다.
- `PaletteToon.mat` 의 값 · 키워드 · 램프 guid(`7e58156c…` = `ToonRamp.png.meta`) · 셰이더 guid · 팔레트 guid 는 사용자 선택과 짝이 맞다 ✔. `ToonRamp.png` 가져오기 설정(sRGB 끔 · Clamp · Bilinear · 비압축 · 밉맵 없음)도 확인했다 ✔.
- 깊이 프라이밍은 네 렌더러 모두 `m_DepthPrimingMode: 0` 이라 외피 패스가 ZTest Equal 에 막히는 경로는 없다고 보았다. 기기와 다른 플랫폼은 보지 않았다.
- `ShaderUtil.GetVariantCount` 리플렉션에 오버로드가 있어 `AmbiguousMatchException` 이 날 수 있는지는 UnityEditor 어셈블리를 열지 않아 확인하지 못했다. 세션이 숫자를 얻었으므로 지금은 동작한다고 추정한다.
- 블롭 쿼드 높이(루트 y + 0.01)가 섬 윗면과 맞는지, 라인업 루트의 바닥 높이는 확인하지 않았다.
