---
id: 20261030-unity-assets-ledger
title: ASSETS — 외부 파일 장부
summary: Defense.Unity 외부 파일 장부 — .gitignore · Unity 템플릿 산출물 · 코딩 표준 사본 · .editorconfig · Tools/Blender(GPL) · P03 · P04 샘플 모델 · 셀 셰이더와 추가 패키지 com.unity.pipeline · newtonsoft-json 의 출처 · 라이선스 · AI 생성 여부.
doc_type: register
status: final
lifecycle: living
created: 2026-10-30
updated: 2026-11-02
---
# ASSETS — 외부 파일 장부

이 저장소에 든 외부 파일과 `Packages/manifest.json` 으로 참조하는 추가 패키지의 출처 · 라이선스를 적는다. **장부에 없는 외부 파일은 커밋하지 않는다.** AI 생성물은 「AI 생성(도구 · 모델)」으로 적는다. 유료 에셋은 쓰지 않고, Unity Asset Store 「무료」 에셋은 재배포 금지라 커밋하지 않는다.

## 파일

| 파일 | 출처 | 라이선스 | 수정 | AI 생성 | 들어온 날 |
|---|---|---|---|---|---|
| `.gitignore` | [github/gitignore `Unity.gitignore`](https://github.com/github/gitignore/blob/main/Unity.gitignore) 커밋 `301e18631acc` | CC0-1.0 | 끝에 이 저장소 줄(`.DS_Store` · `.vscode/` · 서명 자료 · `com.unity.pipeline` 빌드 임시 에셋)을 덧붙임 | 아님 | 2026-10-30 |
| `Assets/Settings/`(빌드 프로필 제외) · `Assets/Scenes/SampleScene.unity` · `Assets/TutorialInfo/` · `Assets/Readme.asset` · `Assets/InputSystem_Actions.inputactions` | Unity 6000.3.27f1 프로젝트 템플릿 「Universal 3D」(`com.unity.template.urp-blank` 17.1.0)이 만든 파일 | [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)(템플릿 패키지 `LICENSE.md`) | 손으로 고친 값 없음. 17.3 에디터가 열 때 · 빌드할 때 다시 저장 — URP 에셋(`PC_RPAsset` · `Mobile_RPAsset`) 에셋 버전 12→13 · 새 필드 · 셰이더 프리필터, `UniversalRenderPipelineGlobalSettings` 설정 목록, `DefaultVolumeProfile` 새 필드와 스크립트가 없는 볼륨 컴포넌트 참조 2개(`OutlineVolumeComponent` · `OasisFogVolumeComponent`) 제거 | 아님 | 2026-10-30 |
| `z.docs/dev/csharp_coding_standards.md` | Planning 정본 `../z.docs/Defense.Planning/_템플릿/Unity_csharp_coding_standards.md` 의 사본. 규칙 체계는 EasyCheck.Server 의 C# 코딩 표준(사용자 작성, POCU C# 코딩 표준 계열 — `docs.popekim.com`)에서 왔고 Unity 판 문장은 새로 썼다(Planning P00 검토서 2026-10-30 · D-06 · D-07) | 이 저장소 `LICENSE`(MIT). 원문 문장을 그대로 옮긴 것 없음 — POCU 표준 자체의 이용 조건은 확인하지 않음(작업 보고 6절) | 정본과 diff 0 | AI 생성(Claude Code · Fable 5.1 — 사용자 결정 14 · 확인) | 2026-10-30 |
| `.editorconfig` | Planning 정본 `../z.docs/Defense.Planning/_템플릿/Unity_.editorconfig` 의 사본. 세션이 표준 §1 · §2 · §4 · §5 에 맞춰 작성 | 이 저장소 `LICENSE`(MIT) | 정본과 diff 0 | AI 생성(Claude Code · Fable 5.1) | 2026-10-30 |
| `Tools/Blender/make_enemies.py` | 이 저장소에서 작성(Planning P03 S0 — D-09 · `_공통` G-17). 창 없는 Blender 5.2.3 이 슬라임 · 박쥐 · 등불 탑을 팔레트 UV · 뼈대 · 동작과 함께 FBX 로 낸다 | GPL-3.0-or-later(`Tools/Blender/LICENSE` — Blender Python API 를 쓰는 스크립트는 공개하면 GPL. 만든 FBX · PNG 는 영향 없음) | — | AI 생성(Claude Code · Fable 5.1) | 2026-10-31 |
| `Tools/Blender/LICENSE` | [GNU GPL v3 원문](https://www.gnu.org/licenses/gpl-3.0.txt)(FSF) — SHA-256 `3972dc97…b36986` | 원문 자체의 조건(누구나 그대로 복제 · 배포 가능, 변경 금지) | 없음 | 아님 | 2026-10-31 |
| `Assets/30_Assets/Models/P03_CLI/`(FBX 6 · `palette.png` · `palette.mat` · `island.asset`) | 이 저장소 `Tools/Blender/make_enemies.py` 가 창 없는 Blender 5.2.3 으로 만든 샘플 모델(슬라임 · 박쥐 · 등불 탑, 변형 둘) · 팔레트. `palette.mat` · `island.asset` 은 에디터 스크립트 `Assets/10_Scripts/Editor/P03/` 가 만듦(Planning P03 S2 Opus 5.5 — `c70f6dc` · `3a1dd18`, 머지 `39266b4`) | 이 저장소 `LICENSE`(MIT) — GPL 스크립트의 산출물은 GPL 영향 없음(`Tools/Blender/README.md`) | — | AI 생성(Claude Code — 스크립트 Fable 5.1, 수정 · 실행 Opus 5.5) | 2026-11-01 |
| `Tools/Blender/make_enemies_smooth.py` | 이 저장소에서 작성(Planning P04 S1). `make_enemies.py` 의 도우미를 복사해 같은 모델에 서브디비전 · 스무스 셰이딩 · 크리스를 얹는다 | GPL-3.0-or-later(`Tools/Blender/LICENSE`) | — | AI 생성(Claude Code · Opus 5.5) | 2026-11-02 |
| `Assets/30_Assets/Models/P04_Smooth/`(FBX 3 · `Palette.png`) · `Assets/30_Assets/Materials/`(`PaletteLit` · `PaletteUnlit` · `PaletteCel`) · `Assets/30_Assets/Shaders/PaletteCel.shader` | FBX · 팔레트는 이 저장소 `Tools/Blender/make_enemies_smooth.py` 가 창 없는 Blender 5.2.3 으로 만든 매끈 판 샘플 모델. 머티리얼은 에디터 스크립트 `Assets/10_Scripts/Editor/P04/` 가 만듦. 셀 셰이더는 세션이 작성(URP 17.3 의 `ShadowCasterPass` · `DepthOnlyPass` · `UnlitDepthNormalsPass` 를 include 로 씀) | 이 저장소 `LICENSE`(MIT) — GPL 스크립트의 산출물은 GPL 영향 없음 | — | AI 생성(Claude Code · Opus 5.5) | 2026-11-02 |

## 추가 패키지

템플릿 기본 패키지 밖에서 더한 것과 그 때문에 새로 들어온 의존을 적는다. 파일은 저장소에 없고 Unity 가 레지스트리에서 받는다 — 저장소가 패키지를 재배포하지 않으므로 Unity 라이선스 패키지도 매니페스트로 참조한다(Planning P00 D-08).

| 패키지 | 판 | 출처 | 라이선스 | 왜 | 들어온 날 |
|---|---|---|---|---|---|
| `com.unity.pipeline` | 0.9.1-exp.1(실험판) | Unity 레지스트리 | Unity Terms of Service(패키지 `LICENSE.md`) | Unity CLI 가 열린 에디터를 다룬다(`unity command`). 에디터를 열면 127.0.0.1 에 토큰으로 보호된 명령 서버가 뜬다(이 저장소를 연 모든 에디터). 런타임 어셈블리(`Unity.Pipeline` · `IlInterpreter`)는 `ENABLE_RUNTIME_PIPELINE` · `ENABLE_RUNTIME_PIPELINE_DEV` 를 정의할 때만 플레이어 빌드에 들어간다(asmdef `defineConstraints` — `CLAUDE.md` 「검토 기준」이 막는다). 속성만 든 `Unity.Pipeline.Attributes` 는 조건이 없다 | 2026-10-30 |
| `com.unity.nuget.newtonsoft-json` | 3.2.2 | Unity 레지스트리 | Unity Companion License(패키지 `LICENSE.md`) | `com.unity.pipeline` 의 의존으로 새로 들어옴(`packages-lock.json` depth 1) | 2026-10-30 |
