---
id: 20261102-p04-s3-toon-rendering
title: P04 S3(Opus 5.5) — 카툰 렌더링 기법 탐구 · 승인 지점 넷 · PaletteToon 적용
summary: 실제 게임의 카툰 렌더링 기법 7 항목을 표로 정리하고 사용자와 승인 지점 넷에서 골라, 외곽선 A(뒤집은 외피) · 받는 그림자 양자화 · 캐릭터 자기 그림자 끔 · 림 · 스페큘러 · 램프 텍스처 · 블롭 그림자를 토글 셰이더 PaletteToon 으로 매끈 판 5종에 적용하고 두 품질 전후 캡처 · 48프레임 MP4 · 비용을 남겼다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
work_type: create
commits: [672db58, 6e1046e, c19bf48, 2f44f50, a8f430b]
implements: [20261102-p04-handoff-s3-toon-rendering]
---

# P04 S3(Opus 5.5) — 카툰 렌더링 기법 탐구 · 적용 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-11-02 15:1x~(KST) · Mac M5 16 GB · Claude Opus 5.5(세션 `f7fb53cd`) — 사용자가 지켜본 세션 |
| 근거 | Planning P04 지시서 `20261102-p04-handoff-s3-toon-rendering` — worktree `z.tmp/wt/p04` · 브랜치 `p04/smooth` · 시작 `489489c`(S2 끝, `{S2 끝 커밋}` 이 비어 `git log -1` 로 확인) |
| 위험 등급 | **중** — 승인 지점 ②에서 `Assets/Settings/` 를 바꿈(새 렌더러 둘 + RP 에셋 목록 1줄씩, 기본 인덱스 0 불변, 따로 커밋 `6e1046e`). 나머지는 셰이더 · 머티리얼 · 에디터 코드 · 렌더링 데이터. `ProjectSettings/` · `Packages/` · 씬 · 모델 변경 없음 |
| 증거 ① 게이트 | 열린 에디터(`672db58` · `c19bf48` 전 · `2f44f50` 뒤 · `a8f430b` 전): `U script recompile` → `recompile_status` `completed`/`up_to_date` · `compilationFailed: false` → `U console status` `consoleErrors: 0` · `consoleWarnings: 0`(셰이더 4종 `ShaderHasError` false · 메시지 0) → `U test run --mode editor` `Total 2 · Passed 2 · Failed 0`. 닫힌 뒤(`2f44f50` 뒤 · `a8f430b` 뒤 두 번): `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 7.6초 · `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS'` 0(스크립트는 열린 에디터에서 이미 컴파일됨 — 경고 근거는 열린 에디터의 `consoleWarnings: 0`) |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서를 담은 커밋 뒤). 품질 단계를 스크립트로 바꾸고 `SaveAssets` · 에디터 종료 때 `ProjectSettings/QualitySettings.asset` 이 직렬화 버전만 바뀌어 저장되는 일이 4번 있었고 매번 `git checkout` 으로 되돌렸다(`m_CurrentQuality: 1` 은 그대로) — 커밋에 없다 |
| 증거 ③ 캡처 | `z.captures/20261102/p04/toon/` — 「전」 8 · 외곽선 24 · 타일 60 · 「후」 8 · 프레임 48 · `Final_Toon_Mobile_48f_24fps.mp4`(다시 읽어 48) · 대조 시트 16 · 전역 영향 20 · 진단 `probe/`, 대응표 `README.md`(커밋용 사본 `z.docs/work_reports/20261102/캡처_README_S3.md`). 확인 페이지 https://claude.ai/artifact/9K9rXTau1jP3P9ZiACAuKW(비공개, 지점마다 갱신 — 4판). 확인 안 한 것은 8절 |

## 1. 결론

**완료** — 성공 기준 7 중 7, 승인 지점 ①~④ 모두 통과(사용자 선택 기록 5절), 미지수 ⑩~⑭ 다섯 모두 실측(⑪ 은 눈 확인). 사용자가 고른 조합: **외곽선 A 뒤집은 외피 2‰ + 3단 램프(경계 0.02) · 청보라 그림자 · 어두운 단 채도 0.3 · 받는 그림자 양자화 · 캐릭터 자기 그림자 끔 · 림 · 스페큘러 · 램프 텍스처, 조명 방향 고정 끔, 블롭 그림자, 톤매핑 None(후처리 끔)**. 외곽선 B(후처리 엣지)는 비교용 코드와 시험 렌더러(`Assets/Settings/` — 사용자 허락으로 남김)로 남았다. 새로 찾은 함정 셋 — ① 씬을 Single 로 다시 열기 전에 만든 메모리 머티리얼 사본은 자홍색(오류 셰이더)으로 그려진다 ② 같은 머티리얼을 섬과 캐릭터가 같이 쓰면 림 · 스페큘러 · 조명 고정이 섬에도 걸린다 → 렌더링 레이어 비트로 캐릭터만 ③ 두 RP 에셋의 볼륨 프로필이 템플릿 `SampleSceneProfile`(톤매핑 Neutral · 비네트 0.2 · 블룸 0.25)이라 카메라 후처리를 켜기만 해도 그 효과가 걸린다 — 승인 ③의 톤매핑 칸(「None · 후처리 켬」 · 「Neutral」)은 둘 다 사실상 이 프로필이었고 근거가 되지 못했다(센서 2라운드가 찾음, 사용자가 고른 「후처리 끔」에는 영향 없음). 리뷰: 1라운드 18 · 2라운드 13 — 고침 13(`a8f430b`) · 의도 1(반론 수락) · 범위 밖 1 · 처리 안 함 16, `blocking` 0 · 미해결 0(11절). 브랜치는 로컬에만 있다(푸시 안 함).

## 2. 바꾼 것

| 커밋 | 승인 지점 | 파일 | 무엇 |
|---|---|---|---|
| `672db58` | ② | `Assets/30_Assets/Shaders/PaletteToon.shader`(새) · `ToonEdgeDetect.shader`(새) · `Materials/PaletteToon.mat` · `ToonEdgeDetect.mat`(새) | `PaletteCel` 을 바탕으로 받는 그림자 양자화(`_SHADOW_BANDS`) · 어두운 단 채도(`_ShadowSaturation`) · 외피 외곽선 패스(`LightMode SRPDefaultUnlit` — `SetShaderPassEnabled` 로 켬 · 끔, 화면 높이 천분율 두께). B 는 Full Screen Pass 용 깊이 · 노멀 Roberts cross |
| 같음 | ② | `Assets/10_Scripts/Editor/P04/` `P04ToonCapture.cs` · `P04ContactSheet.cs` · `P04ToonAssets.cs`(새) · `P04HumanoidCapture.cs` | 5종 × 품질 캡처(품질 단계를 찍는 동안만 바꿈) · 비용 측정 · 대조 시트(5×7 비트맵 글꼴 라벨) · 머티리얼 생성. 인간형 자세 도우미를 `private` → `internal`(동작 변화 없음) |
| `6e1046e` | ② `Assets/Settings/` | `Toon_Renderer.asset` · `Toon_PC_Renderer.asset`(새) · `Mobile_RPAsset.asset` · `PC_RPAsset.asset`(목록 +1줄씩) | `Mobile_Renderer` · `PC_Renderer` 복사 + Full Screen Pass「ToonEdgeDetect」(injectionPoint 550 · requirements 3 = Depth \| Normal · fetchColorBuffer 1). 만든 코드는 9절 |
| `c19bf48` | ③ | `PaletteToon.shader` · `BlobShadow.shader`(새) · `Materials/BlobShadow.mat` · `Assets/30_Assets/Rendering/`(새 폴더 — `ToonRamp.png` · `ToonVolume_Neutral.asset`) · `PaletteToon.mat` · 에디터 `P04ToonTiles.cs`(새) 외 | 캐릭터 전용(렌더링 레이어 1) 자기 그림자 끔 · 조명 고정 · 림 · 스페큘러, 램프 텍스처(단계 함수와 같은 곡선), 블롭 그림자(곱하기 블렌드 쿼드), 톤매핑 볼륨, 타일 13 손잡이, 캡처 옵션(씬 준비 · 캐릭터 표시 · 블롭 · 볼륨), `PaletteToon.mat` = ③ 선택값 |
| `2f44f50` | ④ | `P04ToonCapture.cs` · `P04HumanoidCapture.cs` · `ASSETS.md` | 최종 조합 녹화(궁수 Shoot · Walk · 좀비 Walk · Attack 섞음) · 장부 행 둘 |
| `a8f430b` | 리뷰 | `P04ToonCapture.cs` · `P04ToonAssets.cs` · `P04ToonTiles.cs` · `P04ContactSheet.cs` · `PaletteToon.shader` | 센서 1라운드 지적 13건 수정(11절) — 볼륨 루트마다 · 램프 정본을 머티리얼 값으로 · 캐릭터 표시 렌더링 레이어 8 · 타일 기준 고정 · 예외 복원 · 셰이더 경계 · §5.6 등. 고친 뒤 다시 찍어 램프 PNG 바이트 같음 · 근접 장면 바이트 같음(8절) |
| (이 보고서 커밋) | — | `z.docs/work_reports/20261102/` 보고서 · 캡처 README 사본 · `코드리뷰_p04_s3/` | — |

## 3. 검증 결과

### 성공 기준 대조

| # | 기준 | 결과 | 근거 |
|---|---|---|---|
| 1 | 기법 표(승인 ①) | 됨 | 4절 — 7 항목 · 출처 · 비용 · 권장 |
| 2 | 토글 셰이더 · 머티리얼 | 됨 | `Defense/PaletteToon` — `shader_feature_local_fragment` 6(`_SHADOW_BANDS` · `_SELF_SHADOW_OFF` · `_FIXED_LIGHT_DIR` · `_RAMP_TEXTURE` · `_RIM` · `_SPECULAR`) + 외피 패스 켬 · 끔. `PaletteCel.shader` · 머티리얼 셋은 그대로(`git diff 489489c -- Assets/30_Assets/Shaders/PaletteCel.shader Assets/30_Assets/Materials/Palette{Cel,Lit,Unlit}.mat` 빈 출력). `PaletteToon.mat` 키워드 `_RAMP_TEXTURE,_RIM,_SELF_SHADOW_OFF,_SHADOW_BANDS,_SPECULAR` · `_MidLevel 0.6` · `_StepSoftness 0.02` · `_ShadowTint (0.58,0.62,0.78)` · `_ShadowSaturation 0.3` · `_OutlineWidth 2`. 오류 · 경고 0, 변형 수 6절 |
| 3 | 외곽선 | 됨 | A 가 5종에 적용(`PaletteToon.mat` 외피 패스 켬). B 코드(`ToonEdgeDetect`) · 시험 렌더러 · 캡처 `outline/OB_Edge_*` 남음 |
| 4 | 캡처 전후 · 대조 시트 4장 이상 | 됨 | 「전」 `before/` 8 · 「후」 `final/` 8(5종 세로 · 라인업 · 근접 2 × Mobile · PC), 대조 시트 16장(0 · 2 · 3 · 4). 전부 열어 봄 |
| 5 | 녹화 | 됨 | Mobile 품질 48프레임 720×1280 → `Final_Toon_Mobile_48f_24fps.mp4` 493,492 B, Blender 로 다시 읽어 `frame_duration` 48 · 24 fps. 인간형 포함(궁수 Shoot · Walk · 좀비 Walk · Attack), 스킨 행렬 렌더마다. 프레임 0 · 12 · 24 MD5 서로 다름(`49bb939a` · `34296d77` · `f2c532b0`) · 프레임 시트 열어 봄 |
| 6 | 비용 표 | 됨 | 6절 |
| 7 | 게이트 · 보고 · 센서 · 커밋 · 확인 페이지 | 됨 | 기본 정보 증거 ① · 커밋 넷 + 보고 · 11절 · 확인 페이지 |

### 전역 영향 없음의 증거(`Assets/Settings/` 변경)

기본 렌더러(인덱스 0)로 P03 씬(`P03_CLI_Sample`) · S1 비교 씬(`P04Smooth` 루트 · `PaletteLit`)을 두 품질에서 540×960 으로 바꾸기 전 · 뒤에 찍어 SHA-256 을 대조했다(`z.captures/20261102/p04/toon/global/`).

| 장면 | 전 | 전(재현) | 후 | 후(재현) | RP 목록 원복 |
|---|---|---|---|---|---|
| P03 · Mobile | `54b63ce2a7fc` | 같음 | 같음 | 같음 | 같음 |
| P03 · PC | `da997c27417e` | 같음 | 같음 | 같음 | — |
| P04 Lit · Mobile | `08fa428fcd54` | 같음 | `f81c825cb304` | `f81c825cb304` | `f81c825cb304` |
| P04 Lit · PC | `05f1eb5324fd` | 같음 | 같음 | 같음 | — |

P04 Lit · Mobile 한 장만 518,400 픽셀 중 5픽셀이 1/255 다르다(영역 x 138~317 · y 530~586). RP 에셋 둘을 원본으로 되돌려 찍어도 「후」와 같은 해시가 나와 **목록 변경이 원인이 아니다** — 변경 전 · 후 사이에 바뀐 에디터 상태(셰이더 변형 캐시 등)로 추정한다(확인 안 함).

## 4. 기법 표(승인 ①)

출처 표시: ✔ 세션이 직접 확인(URP 17.3 소스 · 실측) · △ 조사 에이전트가 열어 본 것(세션은 URL 목록만 확인) · [추정]. 「권장」은 ①에서 보인 권장, 사용자 선택은 5절.

### 4.1 조명 램프

| 기법 | 무엇 | 실제 게임 · 출처 | 넣는 법 | 모바일 비용 | 권장 · 근거 |
|---|---|---|---|---|---|
| 단계 램프(smoothstep 2~3단) | N·L 을 문턱 둘로 끊음 | Unity Toon Shader Step · Feather △ | 지금 `PaletteCel` | ALU 몇 개 | 켬 — 사용자가 마음에 든 모양 |
| 단 경계 폭 | 단 사이 번짐 폭 | UTS Feather △ | 속성 | 0 | ③ 타일 |
| 램프 텍스처(1D) | N·L 을 그린 곡선으로 | TF2 light warp texture △ | `_RAMP_TEXTURE` · 256×1 PNG | 텍스처 샘플 +1 · 변형 ×2 | 구현 · 기본 끔 — 같은 모양, 비용 차만 |
| 하프 램버트 | N·L×0.5+0.5 | TF2 scale · bias △ | — | 0 | 표만 — 문턱 이동과 같은 효과 |
| 어두운 단 채도 | 그늘을 회색으로 죽이지 않음 | GGXrd 그늘색 레이어 △ · 원신 그늘 농도 △ | 휘도 기준 채도 ×(1+s) | lerp 1 | 켬 · 값 ③ |
| 조명 방향 고정 | 캐릭터 명암을 해와 따로 | GGXrd 캐릭터 전용 광원 △ · UTS 가상 방향광 △ | `_FIXED_LIGHT_DIR` | 0 | ③ 타일 — 하루 조명(기획서 9절) |

### 4.2 그림자

| 기법 | 무엇 | 출처 | 넣는 법 | 비용 | 권장 · 근거 |
|---|---|---|---|---|---|
| 받는 그림자 양자화 | 감쇠를 N·L 과 따로 한 단 | UTS System Shadow Level △ · Hi-Fi Rush 선명한 경계 △ | `_SHADOW_BANDS` | 0 | 켬 — S1 「그림자 고리」 제거 |
| 그림자 색조 | 어두운 단 색 | 지금 `_ShadowTint` | 속성 | 0 | ③ 타일 |
| 캐릭터 자기 그림자 끔 | 바닥만 받음 | Unity 그림자 최적화 문서 △ | URP 포워드는 렌더러 「그림자 받기」를 머티리얼 키워드로만 처리 ✔(`Shadows.hlsl:13` `_RECEIVE_SHADOWS_OFF`) → 렌더링 레이어 비트(`GetMeshRenderingLayer()` ✔ `ShaderVariablesFunctions.hlsl:608`) | 0 | ③ 타일 |
| 블롭 그림자 | 발밑 원 쿼드 | Shadowgun — Unity 모바일 가이드 △ | `Defense/BlobShadow` · 캐릭터 그림자 드리우기 끔 | 그림자 패스 → 쿼드 | ③ 타일 |
| 하드 · 소프트 · 해상도 · 캐스케이드 · 바이어스 | 그림자 맵 품질 | Unity 문서 △ — 타일 GPU 에서 소프트 그림자 비용 큼 | RP 에셋 값(`Assets/Settings/`) | 캐스케이드마다 패스 +1 | 표 · 발견만 — 범위 밖 |
| 원신 CSM 8단 · Poisson · 캡슐 AO | 콘솔급 | miHoYo 발표 △ | — | 과함 | 해당 없음 |

### 4.3 외곽선

| 방식 | 두께 | 안쪽 선 | 비용 | 설정 변경 | 출처 | 권장 |
|---|---|---|---|---|---|---|
| (A) 뒤집은 외피 | 클립 공간 보정이면 일정(⑪) | 없음 — 떨어진 부품엔 생김 | 패스 +1 · 정점 ×2 · 드로 +1/렌더러 | 없음 | Guilty Gear 시리즈 △ · Ameye △ | 구현 · 채택 후보 |
| (B) 후처리 엣지(깊이 · 노멀 Roberts) | 화면 픽셀 단위 일정 | 있음 | 풀스크린 1 + DepthNormals 프리패스 | 새 렌더러 + 목록 1줄 | Roystan △ · `FullScreenPassRendererFeature.requirements` → `ConfigureInput` ✔(소스 103행) | 구현 · 비교용 |
| (C) Render Objects 레이어별 외피 | A 와 같음 | A 와 같음 | A + 셰이더 덮어쓰기는 SRP Batcher 불가 △ | 새 렌더러 | Unity 문서 △ | 표만 |

### 4.4 림 · 스페큘러 · 주변광

| 기법 | 출처 | 넣는 법 | 비용 | 권장 |
|---|---|---|---|---|
| 림(단계 프레넬, 빛 쪽만) | TF2 림 △ | `_RIM` | ALU | ③ 타일 — 탑다운에서 잘 안 보일 수 있음 [추정] |
| 툰 스페큘러(단계 하이라이트) | TF2 프레넬 퐁 △ · 출시 사례 출처 못 찾음 | `_SPECULAR` | ALU | ③ 타일(권장 끔) |
| 주변광(SH × 0.35) | 툰 SH 양자화 출처 못 찾음 | 지금 그대로 | 0 | 유지 |

### 4.5 색 · 후처리

| 기법 | 출처 | 넣는 법 | 비용 | 권장 |
|---|---|---|---|---|
| 톤매핑 None · Neutral | Unity 문서 △ | 볼륨 프로필 `ToonVolume_Neutral.asset` + 카메라 후처리(씬 인스턴스) — 렌더러 변경 없음 | 우버 패스 1 | ③ 타일 |
| 비네트 · 채도 | XP 히어로 바닥 비네트(S1 관찰) | 같은 볼륨 | 우버 패스 안 | 표만 |

### 4.6 그 밖의 기법(구현 안 함)

| 기법 | 맞나 | 이유 |
|---|---|---|
| 정점 색 AO(Blender 굽기) | 맞음 — `DEF-M03` 후보 | 실행 비용 0, Mobile 에 없는 SSAO 대신. GGXrd 정점 색 명암 △ |
| 얼굴 그림자 맵(SDF) | 불필요 | 점 눈 치비. 원신 · Hi-Fi Rush 는 따로 구현 △ |
| 디더링 페이드 | 나중에 | 가린 적 · 등장 퇴장. 그림자 패스도 잘라야 하는 함정 △ |
| 머티리얼 하나로 전부 | 이미 채택 | 4×4 팔레트 원칙 |

### 4.7 비용 재는 법

창 없는 에디터의 `Camera.Render()` 직후 `UnityStats` 와 `ProfilerRecorder`("Draw Calls Count" 등)는 둘 다 0 을 돌려줬다 ✔(프레임 끝에만 집계로 추정) → 드로는 「렌더러 × 실행되는 패스(그림자는 캐스케이드 수만큼)」 계산. 패스 `Shader.GetPassCountInSubshader(0)`(public), 변형 `ShaderUtil.GetVariantCount`(Unity 6 internal — 리플렉션) ✔.

출처 URL(△ — 에이전트가 열어 본 것): TF2 위키 https://wiki.teamfortress.com/wiki/Illustrative_Rendering_in_Team_Fortress_2 · GGXrd GDC https://gdcvault.com/play/1022031/GuiltyGearXrd-s-Art-Style-The · BlenderNation https://www.blendernation.com/2015/07/26/junya-c-motomura-behind-the-scenes-of-guilty-gear-xrd/ · ASW 강의 https://docswell.com/s/ASW_Academy/5LVY67-GG-Toonline-Eng · 원신 보도 https://www.famitsu.com/news/202107/23228135.html · miHoYo https://docswell.com/s/UnityJapan/KWRPQ5-210617-unity-dojo20211mihoyozhenzhongyi · Hi-Fi Rush https://80.lv/articles/the-making-of-hi-fi-rush-s-3d-toon-rendering-style · https://www.gcores.com/articles/181912 · UTS https://docs.unity3d.com/Packages/com.unity.toonshader@0.7/manual/ShadingStepAndFeather.html · https://docs.unity3d.com/Packages/com.unity.toonshader@0.7/manual/Metaverse.html · Full Screen Pass https://docs.unity.com/en-us/engine/6000.3/manual/post-processing-and-full-screen-effects/urp/custom-post-processing/renderer-feature-full-screen-pass · Render Objects https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/universal-render-pipeline/customizing-urp/urp-renderer-feature/renderer-feature-render-objects · Ameye https://ameye.dev/notes/rendering-outlines/ · Roystan https://roystan.net/articles/outline-shader/ · 그림자 최적화 https://docs.unity.com/en-us/engine/current/manual/lighting-overview/lighting/shadows-in-urp/shadows-optimization · Unity 모바일(2018) https://docs.unity.cn/2018.1/Documentation/Manual/MobileOptimizationGraphicsMethods.html · 톤매핑 https://docs.unity.com/en-us/engine/current/manual/post-processing-and-full-screen-effects/urp/effect-list/tonemapping · 디더 https://github.com/daniel-ilett/dither-transparency-urp. 외부 셰이더 코드는 열지도 옮기지도 않았다.

## 5. 승인 지점 기록

| 지점 · 시각(KST) | 보인 것 | 사용자가 고른 것 |
|---|---|---|
| ① 15:34 | 기법 표 7 항목 · 권장 조합 · 적용 순서 · 바꿀 파일 · `Settings` 표시 · 「전」 시트 3장(확인 페이지 1판) | 외곽선 A + B 구현(권장) · ③ 타일에 림 · 스페큘러 · 블롭 · 자기 그림자 끔 · 조명 고정 · 램프 전부 · 받는 그림자 양자화와 어두운 단 채도 처음부터 켬(권장) |
| ② 안 · `Settings` 허락 15:41 | 바뀔 줄 미리보기(새 렌더러 + 목록 1줄, 기본 인덱스 0) · PC 를 어떻게 둘지 | 렌더러 둘(권장 — `Toon_Renderer` + `Toon_PC_Renderer`, PC 는 SSAO · Forward+ 유지). 바꾼 뒤 `git diff --stat` 2 files +2 와 바뀐 줄을 페이지에 보임 |
| ② 15:46 | 대조 시트 2(없음 · A · B × Mobile · PC 4장) · 비교 표 · 전역 영향(2판) | **A 외피**(권장) · `Settings` 변경은 따로 커밋해 남김(권장) |
| ③ 16:01 | 타일 시트 4장(손잡이 13 × 값 2~3, Mobile) · 비용(3판). 톤매핑 줄 두 칸은 뒤에 근거가 못 됨으로 밝혀짐(8절 · UNI.01 · UNI.19) | 3단 · 0.02 · 청보라 · 0.3(권장) · 블롭 + 자기 그림자 끔(권장) · **림 · 스페큘러 · 램프 텍스처 켬**(권장은 모두 끔) |
| ④ 16:06 | 전후 시트 4장 + 프레임 시트 · MP4 · 「×300」 비교(4판) | 「확인」 |

## 6. 비용 표 · 미지수 ⑩~⑭ 실측

측정 2026-11-02 · Unity 6000.3.27f1 · URP 17.3.0 · 창 없는 에디터. 드로는 계산값(4.7). 세로 장면 = 렌더러 26(스킨 24) · 삼각형 291,952 · 정점 147,210.

| 기법 | 패스 | 드로(렌더러당, Mobile) | 정점 처리 | 풀스크린 | 변형 | 「×300」 슬라임(Mobile) |
|---|---|---|---|---|---|---|
| 「전」 `PaletteCel` | 4 | 2(포워드 · 그림자 캐스케이드 1) | ×2 | 0 | 전체 339 · 씬 47 | 드로 600 · 정점 2,523,600 |
| A 외피 | +1 | +1 | +1 배 | 0 | 0(패스 켬 · 끔) | 드로 +300 · 정점 +1,261,800 |
| B 엣지(비교) | 0(셰이더 1 · 변형 1) | +1 DepthNormals(Mobile) — PC 는 SSAO 가 이미 | +1 배(Mobile) | 1 | — | 드로 +300 · 정점 +1,261,800 + 풀스크린 1 |
| 양자화 · 채도 · 자기 그림자 끔 · 조명 고정 · 림 · 스페큘러 | 0 | 0 | 0 | 0 | 키워드마다 shader_feature ×2(가능 조합) | 0(프래그먼트 연산만) |
| 램프 텍스처 | 0 | 0 | 0 | 0 | ×2 | 0 — 텍스처 샘플 +1, smoothstep 2 대신 |
| 블롭 그림자 | 쿼드 셰이더 1 · 변형 2 | 그림자 −1(캐스케이드마다) · 쿼드 +1 | 그림자 패스 몫 −, 쿼드 +4 | 0 | — | 그림자 드로 −300 · 정점 −1,261,800 · 쿼드 +300 드로 · +1,200 정점 |
| 후처리 켬(톤매핑) | — | 0 | 0 | 우버 1 | — | 풀스크린 1 |
| **「후」 최종** | 5 | 3(포워드 · 외피 · 블롭) | ×2 + 4 | 0 | 전체 20,507 · 씬 88 | 드로 900 · 정점 2,524,800 |
| 「후」 해골 궁수(몸 + 활) | — | 5(전 4) | 14,222×2 + 4 | 0 | — | 드로 1,500(전 1,200) · 정점 8,534,400(전 8,533,200) |

| 미지수 | 실측 |
|---|---|
| ⑩ Full Screen Pass 가 깊이 · 노멀을 스스로 요청하나 | **요청한다.** `Mobile_RPAsset` 의 `m_RequireDepthTexture 0` 을 그대로 두고 기능의 `requirements` = Depth \| Normal 만으로 B 캡처에 노멀 기반 안쪽 선(눈 테두리 · 섬 옆면)이 나왔다. 소스 `FullScreenPassRendererFeature.cs:103` `ConfigureInput(requirements)` ✔. 셰이더에 `DepthNormals` 패스가 있어야 한다(`PaletteToon` 은 URP `UnlitDepthNormalsPass` include) |
| ⑪ 외피 두께의 원근 거리 균일성 | **눈으로 같다.** 세로 장면에서 섬 위쪽(먼) 박쥐와 아래쪽(가까운) 슬라임 · 섬 가장자리의 선을 같은 배율로 잘라 보면 둘 다 약 3~4 px(1080×1920, 2‰ = 3.84 px). 클립 공간에서 `w` 를 곱해 미는 구성상 화면 폭이 일정하다. 픽셀 계측은 안 함 |
| ⑫ Mobile 하드 그림자 아티팩트 | **톱니가 보인다.** 1024 · 캐스케이드 1 · 거리 50 에서 바닥 그림자 가장자리가 계단 · 박쥐 날개 그림자가 부스러짐(`Sheet0_Before_Portrait`), 궁수 골반 · 다리에 자기 그림자 얼룩(아크네로 보임). 피터팬(그림자 뜸)은 눈에 띄지 않음. 양자화는 얼룩을 또렷한 한 단으로 만들 뿐이라 자기 그림자 끔 · 블롭이 처방이 됐다. RP 값(거리 · 바이어스)은 범위 밖이라 바꿔 보지 않음 |
| ⑬ 램프 텍스처 vs 단계 함수 비용 | 같은 곡선이면 그림이 같다(`probe/ramp.png`). 차이는 텍스처 샘플 +1(256×1 R · 비압축 · Bilinear · Clamp — 196 B 수준 메모리) · smoothstep 2 제거 · 변형 ×2. 기기 GPU 시간은 범위 밖 — 잴 수 없었다 |
| ⑭ 키워드 조합 변형 수 | `PaletteToon` 가능한 전체 20,507(셀 339 — 키워드 6개 × 2^6 조합이 포워드 패스에 곱해짐) · 이 씬이 쓰는 것 88(셀 47). `shader_feature_local` 은 머티리얼이 쓰는 조합만 빌드에 넣으므로 머티리얼 하나면 늘어나는 것은 외피 패스 몫 정도. 빌드 실측(`IPreprocessShaders`)은 안 함 |

## 7. 단계별 걸린 시간

| 단계 | 시각 · 걸린 시간 | 비고 |
|---|---|---|
| 읽기 · 시작 절차 | ~15:20 | 지시서 · S1 · S2 보고 · 코드 · 설정 |
| 기법 조사(백그라운드 에이전트) | 6분 36초(병렬) | 도구 52회. 세션은 그동안 캡처 도구 · 「전」 캡처 |
| 에디터 기동 | 7초 | 15:21:39 → 15:21:46 `ready` |
| 「전」 캡처 8 + 시트 | 약 10초 | |
| 확인 페이지 1판 · 승인 ① | ~15:33 · 대기 약 1분 | |
| ② 셰이더 · A 캡처 · 자홍색 진단 | 15:35~15:40 | 진단 약 4분(사본 수명 · 시점 시험 5회) |
| `Settings` 허락 대기 | 약 1분 | |
| 기준 캡처 · 렌더러 생성 · B 캡처 · 원인 가르기 | 15:41~15:43 | 원복 시험 포함 |
| 승인 ② 대기 | 약 2분 | |
| 게이트 · 커밋 둘 | 15:48~15:49 | |
| ③ 구현 · 타일 26벌 · 수정 · 다시 찍기 | 15:49~15:58 | 타일 한 벌 12초. 블롭 순서 · 캐릭터 레이어 · 램프 수정으로 두 번 더 |
| 승인 ③ 대기 | 약 3분 | |
| ④ 최종 캡처 8 · 프레임 48 | 2초 | 16:02:43~45 |
| 에디터 끄기 · MP4 | 16:03 · 인코딩 1.1초 | |
| 승인 ④ 대기 | 약 2분 | |
| 닫힌 게이트 | 7.6초 | |
| 센서 · 처리 · 보고 | 16:11~ | 11절 |

## 8. 확인하지 않은 것 · 추정한 것

- **톤매핑 비교는 무효였다**: 승인 ③에 보인 `T12_Neutral` 칸은 볼륨 오브젝트가 꺼진 채 찍혀(캡처 도구가 찍을 루트만 켜면서 씬 최상위 볼륨을 끔 — 센서 `P04.S3.UNI.01`) 「후처리만 켬」 칸과 바이트까지 같았다. 볼륨을 루트마다 두게 고친(`a8f430b`) 뒤에도 바이트가 같았는데, 2라운드 센서가 원인을 짚었다(`P04.S3.UNI.19` — 세션이 에셋 값 확인): 두 RP 에셋의 `m_VolumeProfile` 이 템플릿 `SampleSceneProfile`(Tonemapping mode 1 = Neutral · Vignette 0.2 · Bloom 0.25 · MotionBlur 0.6)을 가리켜, 후처리를 켜기만 해도 이미 Neutral 이 걸린다. 그래서 「None · 후처리 켬」 칸은 라벨과 달리 Neutral + 비네트 + 블룸이었고, Neutral 볼륨을 더해도 바뀌는 것이 없었다. 사용자는 None · 후처리 끔을 골랐으므로 최종 조합은 영향이 없지만, ③의 톤매핑 칸은 근거가 되지 못한다. 처음에 「기본 볼륨 프로필은 중립」이라고 본 것은 전역 `DefaultVolumeProfile` 만 보고 RP 에셋별 프로필을 놓친 것이다. 렌더 경로가 이 프로필을 쓰는 코드는 읽지 않았다 — 값이 관찰과 맞는다는 것까지가 근거다.
- **후처리를 켜면 회색 막**: 같은 경로에서 카메라 후처리를 켜기만 해도 하늘색이 (0.62, 0.80, 0.93) → (0.58, 0.70, 0.77) 로 어두워지고 채도가 빠진다 — 위 `SampleSceneProfile` 의 Neutral 톤매핑 · 비네트로 설명된다(추정, 프로필을 빼고 다시 찍지 않음). 게임 뷰 · 기기에서의 모습은 보지 않았다.
- **에디터를 다시 띄운 뒤의 미세한 차이**: 수정 커밋 뒤 타일 · 최종 조합을 다시 찍어(`tiles_r2/` · `final_r2/`) 대조하니 근접 장면은 전부 바이트가 같고, 라인업 · 세로 장면은 장마다 3~475 픽셀(최대 차 24~31/255)이 달랐다(최종 세로는 1픽셀 · 4/255). 레이어 비트 변경 · 셰이더 경계 수정 뒤라 원인을 하나로 가르지 못했다 — 그림자 맵 경계의 에디터 상태 차이로 추정(3절의 5픽셀과 같은 부류). 승인에 쓴 시트는 첫 판(`tiles/` · `final/`)이다.
- 5픽셀 차이(3절)의 원인은 「목록 변경이 아님」까지만 확인 — 에디터 상태 차이는 추정.
- ⑪ 은 눈 확인(확대 자르기)이고 픽셀 폭 계측은 안 했다. 외피의 거리 균일성은 구성상 성질로 설명했다.
- 비용은 계산값이다 — 기기 프레임 시간 · GPU 시간 · 오버드로는 재지 않았다. 블롭 쿼드 300 의 인스턴싱 여부(SRP Batcher 와 GPU 인스턴싱은 함께 안 됨)도 확인 안 함.
- 캐릭터 표시(렌더링 레이어 1)는 캡처 때 씬 인스턴스에만 켰다 — 씬 · 프리팹에 저장된 값이 아니다. `DEF-M03` 에서 캐릭터 프리팹이 켜야 한다(10절).
- 블롭 크기는 렌더러 경계 상자의 가로 최대 × 1.1 — 걷거나 쏘는 자세에서 경계 상자가 커지면 원도 커진다(녹화에서 눈에 띄는 출렁임은 못 봄).
- 외곽선 B 의 PC 캡처는 `Toon_PC_Renderer`(SSAO 유지)로 찍었다 — 지시서의 「`Mobile_Renderer` 복제」 형태와 다르다(9절).
- 조사 에이전트의 출처(△)는 URL 목록과 요지만 옮겼고 세션이 각 페이지를 다시 열어 검산하지 않았다. 세션이 직접 확인한 것은 URP 소스 두 곳과 실측뿐이다.
- `ProjectSettings/QualitySettings.asset` 은 에디터가 직렬화 버전만 바꿔 저장하는 것을 매번 되돌렸다 — 다음 세션도 같은 일을 겪는다(10절).

## 9. 지시서와 다른 점 · 더 나은 방법 · 범위 밖 제안

- 다른 점(시작 전 알림): `{S2 끝 커밋}` 비어 있어 `489489c` 확인 · 브랜치에 원격 추적이 없어 `git pull --ff-only` 해당 없음 · `pgrep` 에 Unity Hub 만 잡힘.
- 다른 점(사용자 허락): 시험 렌더러를 하나(`Toon_Renderer`)가 아니라 둘(`Toon_PC_Renderer` 추가)로 — PC 품질의 B 비교가 SSAO · Forward+ 를 잃지 않게. 만든 코드(`script eval` 한 번):
  `AssetDatabase.CopyAsset(원본 렌더러, 새 렌더러)` → `ScriptableObject.CreateInstance<FullScreenPassRendererFeature>()`(`passMaterial` `ToonEdgeDetect.mat` · `injectionPoint` `BeforeRenderingPostProcessing` · `requirements` `Depth | Normal` · `fetchColorBuffer` true) → `AssetDatabase.AddObjectToAsset` → 직렬화 필드 `m_RendererFeatures` · `m_RendererFeatureMap`(로컬 id)에 한 칸씩(URP 에디터 `ScriptableRendererDataEditor.AddComponent` 와 같은 순서) → RP 에셋 `m_RendererDataList` 끝에 한 칸 → `SaveAssetIfDirty`.
- 다른 점: 캐릭터 전용 기법(자기 그림자 끔 · 조명 고정 · 림 · 스페큘러)을 렌더링 레이어 1 로 캐릭터에만 걸었다 — 지시서는 「토글」만 적었으나 섬 · 탑이 같은 머티리얼이라 림이 섬 윗면 전체를 밝히는 것을 타일에서 보고 고쳤다(머티리얼 하나 · 팔레트 원칙은 그대로).
- 다른 점: ③ 타일은 Mobile 품질만(지시서는 ③에 품질을 정하지 않음). 톤매핑 줄에 「후처리만 켬(None)」 칸을 더했다(Neutral 의 회색이 톤매핑 탓인지 가리려고).
- 다른 점: 볼륨 프로필 · 시험 렌더러는 URP 타입이 필요해 에디터 코드가 아니라 `script eval` 로 만들었다(에디터 asmdef 가 URP 를 참조하지 않음 — asmdef 변경은 범위 밖). 코드는 이 절과 커밋 메시지에 있다. 카메라 렌더러 인덱스 · 후처리 · 볼륨 컴포넌트는 직렬화 필드 · 타입 이름 문자열로 다룬다.
- 다른 점: `Assets/30_Assets/Rendering/` 은 폴더 체계(`CLAUDE.md` 「코딩」)에 없는 새 폴더다(지시서가 허용 · 「발견」에 적으라 함).
- 다른 점: `Assets/30_Assets/Materials/BlobShadow.mat` 은 「수정 허용 영역」의 Materials 목록(`PaletteToon.mat` + 외곽선 공용 머티리얼)에 이름이 없다. 지시서 「탐구 범위」 2 · 승인 ③ 손잡이 「블롭 vs 실시간」이 블롭 구현을 요구하고 셰이더를 쓰려면 머티리얼이 하나 필요해, 목록을 예를 든 것으로 읽고 Materials 폴더 관례(기법 공용 머티리얼 — `ToonEdgeDetect.mat` 과 같은 자리)를 따랐다. 모든 캐릭터가 같은 머티리얼 하나를 쓴다(모델별 머티리얼 아님). `Rendering/` 에 두는 길도 있었다(센서 `P04.S3.UNI.18` — 판정 반론 수락).
- 다른 점: 비교 씬은 고치지 않았다 — 렌더러 인덱스 · 블롭 · 캐릭터 표시는 캡처 때 씬 인스턴스에만. 최종 조합이 B 가 아니라 씬에 렌더러 인덱스를 저장할 이유가 없었다.
- 더 나은 방법: 손잡이 타일을 코드(`P04ToonTiles`)로 정의해 한 호출로 26벌 · 시트 4장을 12초에 다시 만들 수 있다 — 값을 바꿔 다시 볼 때 같은 표가 된다.
- 범위 밖 제안: ① Mobile RP 의 그림자 거리 50 → 25~30 시험(톱니 완화 — `Assets/Settings/` 값, 사용자 결정) ② 정점 색 AO 굽기(Blender) ③ 블롭 쿼드를 GPU 인스턴싱 머티리얼로 · 크기를 모델 상수로 ④ 후처리 회색 막의 원인 확인(게임 뷰 · 기기 빌드에서 같은지).

## 10. 발견 — `DEF-M03` 에 넘길 것

1. **셰이더 정답 경로**: `PaletteToon` = `PaletteCel` 골격(S1 보고 9절 5번) + `shader_feature_local_fragment` 토글 + 외피 패스 `LightMode SRPDefaultUnlit`(URP 포워드가 태그 없는 둘째 패스로 그림 — 변형 없이 `Material.SetShaderPassEnabled("SRPDefaultUnlit", on)`). 외피 두께는 클립 공간에서 `normalize(normalCS.xy) × 폭 × w`, x 는 화면 비율 보정 — 해상도 · 거리와 무관.
2. **머티리얼 하나 + 캐릭터 표시**: 섬 · 탑 · 캐릭터가 같은 팔레트 머티리얼을 쓰면 캐릭터 전용 효과는 렌더링 레이어 비트(`GetMeshRenderingLayer() & 2`)로 가른다. URP 포워드는 렌더러의 「그림자 받기」 체크를 무시한다(머티리얼 키워드 `_RECEIVE_SHADOWS_OFF` 만) — 캐릭터만 그림자를 안 받게 하려면 이 방법이 머티리얼을 나누지 않는 길. 캐릭터 프리팹의 `renderingLayerMask` 에 비트 1 을 켠다.
3. **블롭 그림자**: 캐릭터 `shadowCastingMode Off` + 발밑 곱하기 블렌드 쿼드(`Blend DstColor Zero` · `Queue Transparent-100` · 바닥 +1 cm). 적 300 의 스킨 메시 그림자 패스(드로 300+ · 정점 126만+)가 쿼드 300(정점 1,200)이 되고 Mobile 그림자 톱니 · 자기 그림자 얼룩도 사라진다. 탑 같은 정적 모델은 실시간 그림자 그대로.
4. **외곽선 B 를 쓰려면**: 내장 Full Screen Pass 기능 + `requirements = Depth | Normal` 이면 RP 에셋의 깊이 텍스처 설정을 바꾸지 않아도 된다. 셰이더마다 `DepthNormals` 패스 필수. 탑다운 작은 인간형의 가는 팔다리가 선에 덮이는 것이 이번 시험의 약점.
5. **에디터 캡처 함정**: ① 씬을 `OpenScene(Single)` 하기 전에 만든 메모리 머티리얼 사본은 오류 셰이더로 그려진다(`DontUnloadUnusedAsset` 도 소용없음) — 씬을 연 뒤 사본을 만든다 ② 씬 준비로 만든 오브젝트에 머티리얼 일괄 적용이 덮어쓰지 않게 순서 · 찍을 루트만 켜는 도우미가 씬 최상위에 둔 볼륨을 끄지 않게 루트 안에 ③ `AssetDatabase.SaveAssets` · 에디터 종료가 `QualitySettings.asset` 을 직렬화 버전(4 → 5)만 바꿔 저장한다 — 커밋 전 `git checkout` ④ 창 없는 `Camera.Render` 직후 `UnityStats` · `ProfilerRecorder` 는 0.
6. **램프 텍스처**: 같은 곡선이면 단계 함수와 그림이 같다 — 아티스트가 곡선을 그릴 일이 없으면 단계 함수가 텍스처 · 변형 하나씩 싸다. 가져오기 설정: sRGB 끔 · 밉맵 끔 · Clamp · 비압축. 만든 직후 `ImportAsset(ForceSynchronousImport)` 뒤에 읽어야 머티리얼에 걸린다(첫 실행에서 null).
7. **품질 차이**: Mobile(그림자 1024 · 캐스케이드 1 · 거리 50 · 소프트 끔 · SSAO 없음)에서 실시간 그림자 톱니가 분명하다 — 블롭이나 그림자 거리 단축이 첫 처방. PC 는 SSAO 가 슬라임 얼굴에 흐린 그늘을 얹는다(셀 · 툰 모양을 흐림).
8. **장부 · 폴더**: `Assets/30_Assets/Rendering/`(램프 · 볼륨) 폴더를 체계에 넣을지, 시험 렌더러 두 파일을 머지 때 지울지(S4).
9. **후처리 기본값(템플릿)**: `Mobile_RPAsset` · `PC_RPAsset` 의 `m_VolumeProfile` 이 템플릿 `SampleSceneProfile`(톤매핑 Neutral · 비네트 0.2 · 블룸 0.25 · 모션 블러 0.6)이다 — 카메라 후처리를 켜는 순간 팔레트 색이 Neutral 로 눌리고 가장자리가 어두워진다. 후처리를 쓸 거면 먼저 이 프로필을 게임 것으로 바꾸거나 비워야 한다(`Assets/Settings/` 변경 — 사용자 결정). 지금 카메라는 후처리 끔이라 영향 없음.
10. **렌더링 레이어**: TagManager `m_RenderingLayers` 의 0 은 Default, 1~7 은 Light Layer 1~7 로 이름이 붙어 있다. 캐릭터 표시는 이름 없는 8(값 256)을 썼다 — `DEF-M03` 에서 TagManager 에 「Character」 같은 이름을 붙이면(`ProjectSettings/` 변경) 셰이더 상수와 C# 상수 두 곳을 그 이름 기준으로 맞춘다.

## 11. 코드 리뷰

위험 등급: **중**(`Assets/Settings/` 변경 — 지시서). 결정론 게이트: 컴파일 오류 0 · 경고 0(열린 에디터 `consoleWarnings 0`, 셰이더 메시지 0) · EditMode 2 → 2 통과, 실패 0(열림 · 닫힘, `a8f430b` 뒤 포함).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `489489c..2f44f50` | 33파일 +1,963/−13(대상 셰이더 3 · 에디터 코드 6 · 머티리얼 3 · 렌더링 데이터 2 · `Settings` YAML 4) | 세션 센서(A opus) + 판정 반론(D opus) | 2 · 56.3만 + 10.6만(도구) / 22.7만 + 6.8만(알림) · 8분 15초 + 2분 1초 | 18 | 13 | 0 | 1 | 0 | 4 | 0 |
| Unity 2라운드 | `2f44f50..a8f430b` | 5파일 +235/−164 | 세션 센서(A opus) | 1 · 32.3만(도구) / 17.1만(알림) · 6분 13초 | 13 | 0 | 0 | 0 | 1 | 12 | 0 |

원본 · 처리표: `z.docs/work_reports/20261102/코드리뷰_p04_s3/`(처리.md 31행) — 지시서대로 worktree 안(S4 가 Planning 으로 옮긴다). 새 토큰의 「도구」 값은 `센서기록_추출.py` 산출, 「알림」은 하네스 완료 알림의 `subagent_tokens`.

사람이 볼 행(blocking · important 이면서 new, 그리고 미해결 — 위험 등급 중):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P04.S3.UNI.01 | issue · important · new | TEST | 씬 준비로 만든 볼륨을 ShowOnly 가 끔 | P04ToonCapture.cs:92,97,106,297-298 | 고침 | `a8f430b` — 루트마다 자식. 그래도 그림이 같던 원인은 UNI.19 |
| P04.S3.UNI.02 | suggestion · important · new | CONTRACT | 램프를 켜면 단계 값이 안 먹음(값 두 곳) | PaletteToon.shader:183-191 · P04ToonAssets.cs:28-32 | 고침 | `a8f430b` — 머티리얼 값으로 굽기(다시 구운 PNG 바이트 같음) |
| P04.S3.UNI.03 | suggestion · important · new | CONTRACT | 캐릭터 비트가 Light Layer 1 과 겹침 | PaletteToon.shader:107 · P04ToonCapture.cs:35 | 고침 | `a8f430b` — 렌더링 레이어 8 |
| P04.S3.UNI.04 | issue · important · new | TEST | 타일 기준 라벨이 머티리얼과 다름 | P04ToonTiles.cs:98,104-120 | 고침 | `a8f430b` — ③ 직전 고정값 |
| P04.S3.UNI.19 | issue · important · pre-existing | CONTRACT | RP 볼륨 프로필이 톤매핑 비교를 덮음 | Mobile_RPAsset.asset:96 · PC_RPAsset.asset:96 · SampleSceneProfile.asset | 범위 밖 | 템플릿 설정 — 보고 1 · 8 · 10절 서술 고침. 사용자 선택(후처리 끔)은 영향 없음. `Settings` 값 변경은 사용자 결정 |
| P04.S3.UNI.18 | question · nit · out-of-scope | BUILD | 허용 목록 밖 `BlobShadow.mat` | Materials/BlobShadow.mat | 의도 | 반론 D 수락(조건 — 9절 줄, 넣음) |

판정 반론: 의도 1 → 수락. 처리 안 함 16 — plausible 6(1라운드 2 · 2라운드 4) · nit 10(1라운드 2 · 2라운드 8). 종료 조건: blocking 0 · important 는 전부 고침 또는 사유(고침 4 · 범위 밖 1). 2라운드는 수정 없이 닫았고 3라운드 없음.
