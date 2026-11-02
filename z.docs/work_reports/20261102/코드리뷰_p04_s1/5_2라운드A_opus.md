---
id: 20261102-ac4d37bd-final
title: 코드리뷰 — 에이전트 ac4d37bd 최종 보고 원문
summary: 「P04 S1 2라운드 센서」 에이전트(claude-opus-5-5 · 도구 호출 17)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
세션 모델: Opus 5.5(지시 기준과 같다).

| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | thought | nit | new | 정확성 · 이해 | `Assets/30_Assets/Shaders/PaletteCel.shader:101-106` | 3인자 `GetMainLight` 가 돌려주는 `shadowAttenuation` 은 `MixRealtimeAndBakedShadows` 의 `lerp(realtime, 1, shadowFade)` 결과다(`Shadows.hlsl:461,489`). 그런데 이 값이 곧바로 `lightAmount` 에 곱해진 뒤 `smoothstep(_LowStep±soft)` · `smoothstep(_HighStep±soft)` 로 끊긴다. 그래서 페이드 구간(`m_ShadowDistance 50` · `m_CascadeBorder 0.1078` 이면 약 44.6~50 m)의 그림자는 서서히 사라지지 않고, 감쇠가 0.05 와 0.5 를 지나는 거리에서 띠 경계 두 개로 끊긴다. 고치기 전에는 50 m 에서 끊기던 경계가 안쪽 두 단으로 옮겨질 뿐이다. 머리 주석의 「거리 페이드 포함」을 URP Lit 처럼 부드럽게 사라진다는 뜻으로 읽기 쉽다. 덧붙여 비교 씬은 카메라 거리 19 m · 섬 지름 13 m(`P04SampleSceneBuilder.cs:31,48`)라 화면이 약 26 m 안쪽뿐이므로, ② 수정은 이번 캡처 8장에 보이는 차이를 만들지 않는다. | plausible |
| 2 | nit | nit | new | 누락(주석) | `Assets/30_Assets/Shaders/PaletteCel.shader:3` | 「받지 않는 것」 목록에 주 조명 쿠키가 빠졌다. 새로 쓴 3인자 `GetMainLight` 에는 `_LIGHT_COOKIES` 분기가 있지만(`RealtimeLights.hlsl:114-117`) 이 패스는 그 키워드를 선언하지 않는다. URP Lit 은 선언한다(`Lit.shader` ForwardLit `#pragma multi_compile_fragment _ _LIGHT_COOKIES`). `PC_RPAsset` 은 `m_SupportsLightCookies: 1` 이다. 그래서 해에 쿠키를 달면 Lit 만 받는데, 주석을 믿은 다음 사람은 그 차이를 셀 쪽 결함으로 오인한다. 스페큘러 · 반사 프로브도 목록에 없다(셰이딩 모델 차이라 의도일 수 있다). 지금 빌더는 쿠키를 쓰지 않는다. | confirmed |
| 3 | nit | nit | new | 이해 | `Assets/30_Assets/Shaders/PaletteCel.shader:100-101` | 주석은 「URP Lit 과 같은 호출」이라고 하지만, Lit 은 `inputData.shadowMask`(`SAMPLE_SHADOWMASK` → 라이트맵이 없으면 `unity_ProbesOcclusion`)를 넘기고 이 셰이더는 상수 `half4(1,1,1,1)` 을 넘긴다. 지금은 `LIGHTMAP_ON` · `SHADOWS_SHADOWMASK` · `LIGHTMAP_SHADOW_MIXING` 을 선언하지 않아 `CALCULATE_BAKED_SHADOWS` 가 정의되지 않는다(`Shadows.hlsl:57-59`). 그래서 `bakedShadow` 는 1이고 결과는 같다. 다만 나중에 섀도마스크 키워드만 더하면 상수 1 때문에 구운 그림자가 조용히 빠진다. 상수가 「구운 그림자 없음」이라는 뜻이라는 설명이 없다. | confirmed |
| 4 | thought | nit | new | 이해 | `Assets/30_Assets/Shaders/PaletteCel.shader:50` | `_SCREEN_SPACE_OCCLUSION` 을 추가해 프래그먼트 변형 수가 2배가 됐다(주 조명 그림자 4 × 소프트 5 × SSAO 2 × 안개 × 인스턴싱). Mobile 렌더러에는 SSAO 기능이 없지만 PC 렌더러에 있어서 스트리핑되지 않는다. 결함은 아니고 변형 수를 기록할 때 참고할 값이다. | plausible |
| 5 | issue | nit | pre-existing | 파일 사이 | `Assets/30_Assets/Shaders/PaletteCel.shader:102` | `mainLight.distanceAttenuation` 을 곱하지 않는다. Lit 은 `distanceAttenuation * shadowAttenuation` 을 쓴다. 비클러스터 경로에서 이 값은 `unity_LightData.z`(`RealtimeLights.hlsl:92`)이고, 주 조명의 컬링 마스크가 그 레이어를 빼면 0이 된다. 그래서 해의 컬링 마스크에서 적 레이어를 빼면 Lit 의 적은 어두워지고 셀의 적은 그대로 밝다. 지금 빌더는 `cullingMask` 를 건드리지 않는다. | confirmed |

검산한 1라운드 수정 3건은 모두 맞게 고쳐졌다.
- ① SSAO: 키워드 선언이 Lit 과 같다(`multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION`). `GetNormalizedScreenSpaceUV(input.positionCS)` 에 프래그먼트 `SV_POSITION`(픽셀 좌표)을 넘기는 것도 Lit · Unlit 과 같다. `direct *= directAmbientOcclusion` 은 Lit 의 `light.color *= aoFactor.directAmbientOcclusion` 과 같다(`_ShadowTint` 항도 `mainLight.color` 에 곱해지는 항이기 때문이다). `ambient *= indirectAmbientOcclusion` 은 Lit 이 GI 에 곱하는 것과 같다. 에디터 품질은 `m_CurrentQuality: 1`(PC) → `PC_RPAsset` → `PC_Renderer`(SSAO 활성 · `AfterOpaque: 0` · `Source: 1` · `DirectLightingStrength: 0.25`)이므로 캡처에서 실제로 적용된다.
- ② `GetMainLight(shadowCoord, positionWS, shadowMask)` 는 `MainLightShadow` → `GetMainLightShadowFade` 를 거쳐 페이드를 한다(`Shadows.hlsl:477-495`). 페이드가 셀 램프에서 어떻게 보이는지는 위 1번.
- ③ 범위: 바뀐 파일은 셰이더 하나뿐이고 수정 허용 영역 안이다. CBUFFER · 다른 패스는 바뀌지 않았다.

## 확인하지 않은 것 · 추정한 것
- 게이트 결과(`ShaderHasError` false · 콘솔 0 · EditMode 2/2)는 다시 돌리지 않았다. 읽기 전용이라 에디터를 띄우지 않았다.
- 캡처를 열어 보지 않았다. SSAO 가 캡처에 실제로 보이는지, 1번의 페이드 구간이 화면 밖인지는 빌더 상수(카메라 19 m · 섬 13 m)와 RP 에셋 값으로 추정했다.
- 페이드 시작 거리(약 44.6 m)는 `m_CascadeBorder` 로 어림했고, URP C# 쪽 `_MainLightShadowParams.zw` 계산식은 읽지 않았다.
- `PC_Renderer` 의 `m_RenderingMode: 2` 를 Forward+ 로 보았다. 이 셰이더는 `_CLUSTER_LIGHT_LOOP` 를 선언하지 않는다. 그래도 주 조명 방향 · 색에는 영향이 없다고 판단했고, 실측하지 않았다.
- `_SCREEN_SPACE_OCCLUSION` 의 셰이더 변형 스트리핑 동작(4번)은 URP 스트리퍼 코드를 읽지 않고 추정했다.