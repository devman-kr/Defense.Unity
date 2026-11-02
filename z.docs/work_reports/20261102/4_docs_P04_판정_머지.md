---
id: 20261102-p04-s4-judge-merge-docs
title: P04 S4 판정 — p04/smooth 머지 · 시험 렌더러 정리 · CLAUDE.md 폴더 체계(Shaders · Rendering)
summary: 판정 세션이 S1 · S2 · S3 보고를 산출물로 검산하고, 사용자 결정(시험 렌더러 삭제 · 볼륨 프로필 · FBX 경로 · 미결 6 은 DEF-M03)을 반영한 정리 커밋 뒤 p04/smooth 를 main 에 --no-ff 머지(86431e0)하고 게이트를 통과시켰으며 CLAUDE.md 폴더 체계에 Shaders · Rendering 을 더했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
work_type: docs
commits: [356995d, 86431e0]
---

# P04 S4 판정 — `p04/smooth` 머지 · 시험 렌더러 정리 · `CLAUDE.md` 폴더 체계 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-11-02 저녁(Mac M5 16 GB, Claude Fable 5.1 — 세션 a3a90bb0, Planning P04 판정 세션 S4) |
| 근거 | Planning P04 계획서 `20261102-p04-plan-smooth-models` 3절 5번 · 7절 (d) · 결정기록 D-08~D-11(사용자 답 2026-11-02 — 「결과 매우 만족」 · 시험 렌더러 삭제 · 볼륨 프로필 · FBX 경로 · 미결 6 은 `DEF-M03`) |
| 위험 등급 | 저 — 머지(세 세션이 각각 세션 센서 2라운드를 거친 에디터 전용 스크립트 · 시험 씬 · 샘플 에셋 · 셰이더 · 머티리얼) + 정리 커밋(`Assets/Settings/` 를 템플릿으로 되돌림) + 문서 2. 제품 런타임 코드 · `ProjectSettings/` · `Packages/` 변경 없음. 되돌리기: `git revert -m 1 86431e0` |
| 증거 ① 게이트 | 머지 직후 `main` 에서 닫힘 `unity test <저장소> --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 8초 · `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS'` 0(스크립트 재컴파일 줄 25 — 바뀐 실행) · `Shader error` 0 |
| 증거 ② 상태 | 게이트 뒤 `git status --short` → 빈 출력(`QualitySettings.asset` 재저장 없음). `git diff --stat 43dc1c7 HEAD -- Assets/Settings ProjectSettings Packages` → 빈 출력 |
| 증거 ③ 캡처 | 머지된 결과의 캡처 · 동영상은 작업 공간 `../z.captures/20261102/p04/`(S1 · S2) · `p04/toon/`(S3 — 전 8 · 외곽선 24 · 타일 76 · 후 8 · MP4 · 대조 시트 16). 판정 세션이 연 것은 대조 시트 2 · 4(세로). 사용자 확인 「결과 매우 만족」(D-08). 최종 전후 시트 사본은 Planning `03_작업기록/assets/최종_세로_전후_20261102.png` |

## 1. 바꾼 것

| 커밋 | 파일 | 무엇 |
|---|---|---|
| `356995d`(`p04/smooth`) | `Assets/Settings/Toon_Renderer.asset` · `Toon_PC_Renderer.asset`(+ `.meta`) 삭제 · `Mobile_RPAsset.asset` · `PC_RPAsset.asset` 렌더러 목록 1줄씩 원복 · `ASSETS.md` 렌더러 행 삭제 | 외곽선 B(후처리 엣지) 비교용 시험 렌더러 — 사용자가 A(외피)를 골라 쓰이지 않으므로 머지 전에 지움(D-09). `Assets/Settings/` 는 `43dc1c7` 과 같다. `ToonEdgeDetect` 셰이더 · 머티리얼 · 캡처 코드의 B 경로는 남김 — 다시 찍으려면 S3 보고 9절의 `script eval` 코드로 렌더러를 다시 만든다 |
| `86431e0`(머지) | 91 파일 +12,511/−12 | S1 `d8992c9`~`9fcc860` · S2 `ff8be0d`~`489489c` · S3 `672db58`~`1f7fac0` · 정리 `356995d` |
| (이 보고 커밋) | `CLAUDE.md`(정본 Planning `_템플릿/Unity_CLAUDE.md` 같이) · 이 보고 | 폴더 체계 `Assets/30_Assets/` 에 `Shaders/`(코드로 쓴 셰이더) · `Rendering/`(램프 텍스처 · 볼륨 프로필) 추가 — S1 · S3 가 만든 폴더를 체계에 등재 |

## 2. 검산 — 보고의 주장을 산출물로(✔ 직접 확인)

| 주장(보고) | 확인 |
|---|---|
| S3: `Assets/Settings/` 변경은 렌더러 둘 + 목록 1줄씩, 기본 인덱스 0 불변 | ✔ `git diff --stat 43dc1c7..1f7fac0 -- Assets/Settings` 6 파일 +199, 목록 줄만. 정리 커밋 뒤 0 |
| S3: `ProjectSettings/` · `Packages/` 변경 없음 | ✔ diff 빈 출력 |
| S3: `PaletteCel.shader` · Lit/Unlit/Cel 머티리얼은 S3 범위에서 불변 | ✔ `git diff --stat 489489c..1f7fac0` 빈 출력 |
| S3: `PaletteToon.mat` 키워드 5 · 값 | ✔ `_RAMP_TEXTURE` `_RIM` `_SELF_SHADOW_OFF` `_SHADOW_BANDS` `_SPECULAR` · `_MidLevel 0.6` · `_StepSoftness 0.02` · `_ShadowSaturation 0.3` · `_OutlineWidth 2` · `_ShadowTint (0.58, 0.62, 0.78)` |
| S1 · S2: `.meta` 짝 | ✔ 새 파일 68 중 짝 없는 것 0(짝 없이 나온 셋은 기존 파일 수정 — `P03FrameRecorder.cs` · RP 에셋 둘) |
| S2: 클립 이름 — `.001` 이 벗겨진다 | ✔ `.meta` 클립 `Idle` `Shoot` `Walk` / `Attack` `Idle` `Walk` = `p04_result.json` 재읽기 이름에서 `.001` 뺀 것 |
| S2: 커밋 FBX 에 절대 경로 문자열 | ✔ P04 FBX 5 · P03 FBX 6 모두 `strings \| grep -c /Users/` 4 — `.blend` 경로 1 · 팔레트 경로 3. 처리는 D-10 |
| S3: 캡처 수 — 전 8 · 외곽선 24 · 타일 60 · 후 8 · 프레임 48 · MP4 493,492 B | ✔ 전 8 · 외곽선 24 · 후 8 · 프레임 48 · MP4 493,492 B. **타일은 76**(19벌 × 4 장면 — `T00` 기준 + 손잡이 13 에 값 2개짜리 5) — 보고의 60 은 오기, 내용은 같다 |
| S3: 캡처에 외곽선 · 블롭 · 양자화 그림자가 보인다 | ✔ `Sheet4_Final_Portrait.png` · `Sheet2_Outline_Portrait.png` 을 열어 봄 — 후 Mobile 에서 S1 의 그림자 톱니 · 자기 그림자 얼룩이 사라지고 외곽선 · 블롭이 보인다. B 는 섬 옆면 선 · 팔다리 뭉침 |
| S1 · S2 · S3 게이트 통과 | ✔ 머지 뒤 `main` 에서 다시 1회(기본 정보 ①) |

확인 안 한 것: S1 · S2 캡처(8 + 5)는 사용자가 S1 · S2 때 봤고 판정 세션은 다시 열지 않았다 · S2 의 Blender 실측 수치(접힘 각 · 두께)는 `p04_result.json` 을 다시 열지 않았다(△) · 세 보고의 센서 처리표 행은 셌을 뿐 지적마다 다시 판정하지 않았다 · S3 의 기법 출처 URL(△)은 열지 않았다.

## 3. 확인하지 않은 것 · 추정한 것

- 머지 뒤 게이트는 닫힌 `unity test` 1회 — 창 없는 에디터를 띄워 셰이더 4종 · 비교 씬을 다시 캡처하지는 않았다(세 세션이 각각 열림 · 닫힘 게이트와 캡처를 했고, 머지는 충돌 없는 fast-forward 내용).
- 시험 렌더러를 지운 뒤 `P04ToonCapture` 의 렌더러 인덱스 1 로 B 캡처를 호출하면 URP 가 기본 렌더러로 돌아간다(경고) — 돌려 보지 않았다(추정).
- 볼륨 프로필(템플릿 `SampleSceneProfile` — `Mobile_RPAsset` · `PC_RPAsset` · 템플릿 `SampleScene.unity` 참조) · FBX 절대 경로 · 아트 방향 미결 6 은 사용자 결정으로 `DEF-M03` 에 넘겼다(Planning P01 NOTES 4번).

## 4. 코드 리뷰

위험 등급 저(머지 · 문서) — 코드 변경 없음. 세 세션의 센서 기록(S1 17행 · S2 19행 · S3 31행 — 모두 Opus 5.5 센서, S3 는 판정 반론 D)은 `z.docs/work_reports/20261102/코드리뷰_p04_s{1,2,3}/` 이고 정본은 Planning `z.completed/P04_매끈_모델_시험/03_작업기록/코드리뷰_20261102_S{1,2,3}/`. S4 센서는 띄우지 않았다(코드 diff 0 — 정리 커밋은 에셋 삭제 · 장부 한 줄).
