---
id: 20261102-aa713242-final
title: 코드리뷰 — 에이전트 aa713242 최종 보고 원문
summary: 「P04 S3 센서 2라운드」 에이전트(claude-opus-5-5 · 도구 호출 32)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | issue | important | pre-existing | 정확성(타일 라벨 · UNI.01 효과) | P04ToonTiles.cs:178-183 · Assets/Settings/Mobile_RPAsset.asset:96(PC_RPAsset.asset:96 도 같음) · Assets/Settings/SampleSceneProfile.asset:105-110 | 두 RP 에셋의 `m_VolumeProfile` 이 `SampleSceneProfile`(guid 10fc4df2…)을 가리킨다. 이 프로필은 Tonemapping `mode: 1`(Neutral) · Vignette 강도 0.2 · Bloom 강도 0.25(문턱 1)를 덮어쓴다. 그래서 카메라 후처리만 켜도 RP 에셋 기본 프로필 때문에 Neutral · 비네트 · 블룸이 걸리고, `T12_PostOnly` 칸의 「NONE (POST ON)」 라벨은 사실과 다르다. 여기에 `ToonVolume_Neutral` 을 더해도 바뀌는 것이 없으니 UNI.01 을 고친 뒤에도 Neutral 칸이 PostOnly 칸과 바이트까지 같은 것과 들어맞는다(보고 3_create_P04_카툰렌더링.md:193). 보고 :194 의 「후처리를 켜면 회색 막」도 같은 원인일 수 있다. 보고 :194 는 「기본 볼륨 프로필의 켜진 값은 전부 중립」이라고 적었는데, 확인한 대상은 전역 `DefaultVolumeProfile`(mode 0)이고 RP 에셋별 프로필은 보지 않은 것으로 보인다 | plausible |
| 2 | nitpick | nit | new | 주석(UNI.04 덜 고침) | P04ToonTiles.cs:115 | 주석이 아직 「기준 칸의 라벨 — 그 손잡이에서 PaletteToon.mat 의 지금 값」이다. 이번 수정으로 기준은 `ApplyBaseline` 고정값이 되었으므로, 주석이 지금 코드와 반대로 말한다 | confirmed |
| 3 | suggestion | nit | new | 정확성(UNI.04 부분 해소) | P04ToonTiles.cs:144-158 · :177 | `ApplyBaseline` 이 고정하는 것은 값 5개와 키워드 6개뿐이다. `_LowStep` · `_HighStep` · `_ShadowStep` · `_RimThreshold` · `_SpecularThreshold` · `_FixedLightDirection` · `_OutlineColor` · `_AmbientStrength` 와 `_RampMap` 내용은 지금 머티리얼을 따른다. 그래서 이 가운데 하나를 머티리얼에서 바꾸거나 램프를 다른 값으로 다시 구우면 「③ 직전 기준」 라벨이 다시 그림과 어긋난다. 특히 T11 「ON (SAME CURVE)」는 그때 머티리얼 값으로 구운 PNG 와 C# 고정값(0.6 · 0.02)이 같아야만 맞다 | plausible |
| 4 | nitpick | nit | new | 중복 상수(UNI.02 와 같은 꼴) | P04ToonTiles.cs:146-150 · P04ToonAssets.cs:100-104 | 기준값 0.6 · 0.02 · (0.58, 0.62, 0.78) · 0.3 · 2 가 `ApplyBaseline` 과 `ApplyApprovedDefaults` 두 곳에 같은 리터럴로 있다. 한쪽만 고치면 「③ 직전」과 「③ 선택」이 같은 값이라는 전제가 말없이 깨진다 | confirmed |
| 5 | thought | nit | new | 정본 동기화(UNI.02) | P04ToonAssets.cs:46-58 · :97-115 | 램프의 정본을 머티리얼 값으로 바꿨지만, 굽는 일은 `CreateRampAndBlob` 을 손으로 부를 때만 일어난다. `ApplyApprovedDefaults` 는 `_RAMP_TEXTURE` 를 켜기만 하고 다시 굽지 않는다. 최종 머티리얼은 램프가 켜져 있으므로 인스펙터에서 `_MidLevel` · `_LowStep` · `_HighStep` 을 바꿔도 그림이 바뀌지 않고 PNG 와 값이 어긋난다. 요약 주석에 「다시 굽는다」고만 적혀 있고 막는 장치는 없다 | plausible |
| 6 | issue | nit | new | 예외 경로(UNI.06 · UNI.07 덜 고침) | P04ToonCapture.cs:91-97 · :125-128 | `Record` 는 사본을 `finally` 에서 해제하지만 `CaptureQuality` 는 끝줄에서만 해제한다. `Render` · `PrepareScene` 등이 예외를 던지면 사본 Material 이 해제되지 않는다. `CaptureSet` 의 `finally` 는 `used` 를 볼 수 없다 | confirmed |
| 7 | nitpick | nit | new | 주석 사실 | PaletteToon.shader:106-107 · P04ToonCapture.cs:34 | 주석은 「0~7 은 TagManager 에 Light Layer 0~7」이라고 하지만, ProjectSettings/TagManager.asset `m_RenderingLayers` 의 0번은 `Default` 이고 1~7번이 `Light Layer 1~7` 이다 | confirmed |
| 8 | issue | nit | pre-existing | 셰이더 수치(UNI.08 과 대칭) | PaletteToon.shader:180 | 위 끝만 1 로 잘랐다. 아래 끝 `_ShadowStep - _StepSoftness` 는 속성 범위(0.01~0.99 · 0.001~0.2) 안에서도 음수가 된다. 예: `_ShadowStep` 0.01 · `_StepSoftness` 0.2 → smoothstep(-0.19, 0.21, 0) ≈ 0.46. 이러면 그림자 한가운데(감쇠 0)도 어두운 단이 되지 않고 중간 밝기로 남는다 | confirmed |
| 9 | nitpick | nit | pre-existing | 표준 §6.9 | P04ToonCapture.cs:136-143 | `CaptureSet` 에는 있는 `Debug.Assert(material != null, …)` 가 `Record` 에는 없다. null 이 들어오면 `new Material(null)` 또는 `ApplyMaterial` 에서 원인 메시지 없이 예외가 나고, 이어 `finally` 가 씬을 다시 연다 | confirmed |
| 10 | nitpick | nit | pre-existing | 공유 배열(UNI.16 과 같은 꼴) | P04ToonTiles.cs:19 · :55 · :75 | 정적 `MOBILE_ONLY` 배열 하나를 Options 여러 개의 `Qualities` 에 같이 넣는다. UNI.16 이 `Options` 기본값에서 없앤 공유 배열이 호출 쪽에 그대로 남아 있다. 지금은 아무도 원소를 바꾸지 않는다 | confirmed |
| 11 | thought | nit | new | 예외 가림 | P04ToonCapture.cs:74-78 · :182-190 | `finally` 안의 `OpenScene` 이 던지면 try 에서 난 원래 예외를 덮는다. 예: try 의 `OpenScene` 이 경로 문제로 실패하면 finally 에서 같은 경로로 또 실패하고, 콘솔에는 두 번째 예외만 남는다 | plausible |
| 12 | nitpick | nit | new | 표준 §19.2 | P04ToonTiles.cs:67-74 | 새로 넣은 래핑 람다(기준 적용 뒤 손잡이 변경)에 역할 한 줄 주석이 없다. 메서드 요약에 의도가 있으니 그것으로 갈음한다고 볼 수도 있다 | plausible |
| 13 | nitpick | nit | pre-existing | 표준 §3 멤버 순서 | P04ContactSheet.cs:127 | 이번 커밋에서 손댄 `GLYPHS`(정적 필드)가 메서드 뒤에 있다. §3 은 정적 필드가 메서드보다 앞이다. 이번 diff 는 `new[]` 를 `new string[]` 으로만 바꿨고, 글리프 내용은 바뀌지 않았다(정규화 diff 로 확인) | confirmed |

확인하지 않은 것 · 추정한 것
- 1번의 원인 연결(RP 에셋 `m_VolumeProfile` 이 창 없는 에디터의 `Camera.Render` 경로에도 적용되어 PostOnly 칸에 Neutral · 비네트 · 블룸이 걸린다)은 추정이다. 근거는 에셋 값(Mobile/PC RP 에셋 :96 → SampleSceneProfile Tonemapping mode 1 · Vignette 0.2 · Bloom 0.25)과 「Neutral 볼륨을 켜도 바이트까지 같음」이라는 관찰이 맞아떨어진다는 것뿐이다. URP 17.3 의 VolumeManager 가 RP 에셋별 프로필을 기본값 층으로 쓰는 코드 경로는 읽지 않았다. 캡처도 돌려 보지 않았다(읽기 전용).
- `renderingLayerMask` 비트 8(256): TagManager `m_RenderingLayers` 에 8번 칸이 이름 없이 있는 것은 확인했다. 정의되지 않은(이름 없는) 레이어 비트를 엔진이 렌더러 값에서 걸러내는지는 확인하지 않았다. URP 쪽 `GetMeshRenderingLayer()` 는 마스킹 없이 `asuint(unity_RenderingLayer.x)` 를 돌려준다(ShaderVariablesFunctions.hlsl:608-611 ✔).
- `SafeNormalize` 는 Core Common.hlsl:1553 · :1559 에 float3 · half3 오버로드가 있다(✔). 키워드 조합별로 셰이더를 실제 컴파일해 오류 · 경고를 확인하지는 않았다. `shadowMask` 는 두 분기 모두에서 선언되고 `_RIM` · `_SPECULAR` 블록에서만 쓰여, 읽은 바로는 모든 조합에서 선언 누락은 없다.
- AddGlobalVolume 을 루트마다 둔 것: 활성 루트가 둘(매끈 판 · 인간형)이면 같은 프로필 · 같은 우선순위의 전역 볼륨이 둘이 된다. 블렌딩 결과는 하나일 때와 같을 것으로 추정한다(실측 안 함).
- ③ 타일 · 시트 산출물(z.captures)이 이 수정 뒤 새 라벨(「BASE = BASELINE BEFORE CHOICE 3」)로 다시 만들어졌는지, `ToonRamp.png` 를 다시 구웠는지는 보지 않았다. 다시 굽는 PNG 는 diff 에 없는데, 기준값과 지금 머티리얼 값이 같으므로 내용도 같을 것으로 추정한다.
- 캡처 README(z.docs/work_reports/20261102/캡처_README_S3.md:22)의 「기준 `PaletteToon.mat` 에서 한 값만 바꿈」은 ③ 촬영 당시에는 사실이었다. 수정된 코드 기준 서술과는 다르다. `*.md` 는 리뷰 범위 밖이라 표에 넣지 않았다.