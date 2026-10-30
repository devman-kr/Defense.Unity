---
id: 20261030-p00-s2-defense-unity-repo-skeleton
title: P00 세션 S2 Defense.Unity 생성 · 결정론 게이트 실측 · 빌드 스모크
summary: Unity 6000.3.27f1 「Universal 3D」 템플릿으로 저장소를 만들고(템플릿 생성물은 첫 커밋으로 분리), Unity CLI 로 에디터 닫힘(batchmode) · 열림(unity command) 두 경로의 게이트 명령을 실측해 CLAUDE.md 에 적었다. PlayerSettings · 빌드 프로필을 정하고 Android 개발 APK · WebGL 을 한 번씩 빌드했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-30
updated: 2026-10-30
author: devman-kr
team: dev
work_unit: P00
work_type: create
tags: [defense, 환경, 게이트, 빌드]
commits: [e46ecab, 3d4abc4, 4ec4f97, 4fdea24, a89d77b, 59f9a07]
related: []
---

# P00 세션 S2 — Defense.Unity 생성 · 게이트 실측 · 빌드 스모크

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-30 (Mac M5 16 GB, Claude Opus 5.5) |
| 근거 | Planning `P00_환경_체계_로깅/02_계획/계획서_20261030.md` 4절 S2 · 결정 D-05 |
| 위험 등급 | 고 — `ProjectSettings/` · `Packages/manifest.json` · 빌드 프로필 첫 설정(가이드 5절). 승인 지점 셋(패키지 · PlayerSettings · 빌드)은 그 직전에 사용자 승인 |
| 증거 ① 게이트 | `unity test . --mode EditMode …` → 종료 0, `total="1" passed="1" failed="0"`, `grep -E 'warning CS\|error CS'` 0줄 |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤). `git ls-files \| grep -cE '^(Library\|Temp\|Logs\|obj\|Builds\|UserSettings)/'` → 0 |
| 증거 ③ 캡처 | `z.captures/20261030/web_first3.png`(WebGL 첫 화면 — SampleScene 하늘 · 지평선, 「Lantern Rush!」) · 확인 안 한 것은 9절 |

## 1. 한 줄 결론

`Defense.Unity` 가 생겼고 게이트 · 빌드가 CLI 로 돈다. 컴파일 · EditMode 는 에디터 닫힘 약 20초 · 열림 0.3초, Android 개발 APK 42.7 MB(240초), WebGL 14 MB(377초, 전환 재임포트 포함). 함정 셋(Web 프로필의 PlayerSettings 사본 · `build run` 의 대상 결정 · 빌드마다 URP 에셋 재저장)을 찾아 `CLAUDE.md` 에 적었다.

## 2. 시작 상태(실측)

- Unity 6000.3.27f1(arm64) + Android · WebGL 모듈 · Unity CLI 1.0.0-beta.13 · Claude Code `unity` 플러그인 0.1.6-beta. Unity 로그인 · 라이선스(Personal, Assigned) 활성.
- 에디터 · Blender 꺼짐(G-01). `Defense.Unity` 폴더 없음.

## 3. 만든 것 · 바꾼 것

| 커밋 | 내용 |
|---|---|
| `e46ecab` | `unity projects new Defense.Unity --template com.unity.template.urp-blank`(72초) 생성물 그대로 + `.gitignore`(github/gitignore `Unity.gitignore` `301e18631acc` + `.DS_Store` · `.vscode/`). 61파일. Force Text · Visible Meta Files 는 템플릿 기본값 그대로 맞음 |
| `3d4abc4` | `CLAUDE.md`(Planning `_템플릿/Unity_CLAUDE.md` + 게이트 실측 · 빌드 프로필 검토 기준) · `README.md` · `ASSETS.md` · `LICENSE`(MIT) · `.gitattributes`(LF · 바이너리, LFS 없음) · `Defense.Core` asmdef + `AssemblyInfo.cs`(InternalsVisibleTo) · `Defense.Tests.EditMode` asmdef + `GateSmokeTests` · `com.unity.pipeline` 0.9.1-exp.1 · PlayerSettings · 빌드 프로필 `Android-Dev` · `Web` · 에디터 자동 변환(PC URP 에셋 12→13 · 머티리얼 버전 9→10) |
| `4ec4f97` | Android 첫 빌드 때 에디터가 다시 저장한 Mobile URP 에셋 · 전역 설정 · 볼륨 프로필 · Android 배칭 기본 항목 |
| `4fdea24` | WebGL 첫 빌드 때 에디터가 다시 저장한 PC URP 에셋 · 전역 설정 · WebGL 배칭 기본 항목 |
| `a89d77b` | 이 보고서 · `CLAUDE.md` 빌드 줄(`--target` 필수 · 시간 · 빌드 뒤 `git status`) |
| `59f9a07` | 세션 센서 지적 고침(8절) |
| 이 커밋 | 이 보고서 8절 · 9절 |

PlayerSettings(사용자 승인 2026-10-30): 회사 `DefaultCompany` → `devmankr` · 제품명 `Defense.Unity` → `Lantern Rush!` · 번들 ID(Android · iOS · Standalone) → `com.devmankr.defense`(D-05). 버전 0.1.0 · Android IL2CPP · ARM64 · 최소 API 25 · WebGL Brotli 는 템플릿 값 그대로.

## 4. 결정론 게이트 실측

| 경로 | 명령 | 결과 |
|---|---|---|
| 닫힘 · 통과 | `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` | 종료 0 · 1/1 통과 · 18.3초 / 19.8초 · 경고 · 오류 줄 0 |
| 닫힘 · 실패 탐지 | 같은 명령, 미정의 기호 + 쓰지 않는 변수가 든 임시 파일을 넣고 | 종료 6 · `TEST_RUN_ERROR` · 로그에 `error CS0103` · `warning CS0219`. 임시 파일은 지움 |
| 닫힘 · 잠금 | 에디터가 연 상태에서 같은 명령 | 시작 전 거부 — 종료 6 · `already open in a running Editor (PID …)` |
| 열림 · 연결 | 창 없는 에디터(`Unity -batchmode -projectPath … -logFile …`, `-quit` 없음) + `unity command` | 바로 연결 · 명령 166개 |
| 열림 · 컴파일 | `U script recompile` · `U console status` | `up_to_date` · `compilationFailed: false` · 오류 · 경고 0 |
| 열림 · EditMode | `U test run --mode editor --result-only` | 1/1 통과 · 0.34초 |
| 열림 · 플레이 모드 | `U editor.playmode start` / `stop` | 2초 안에 `playing` · `stopped` |
| 열림 · 종료 | `U script eval --code 'UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(0); return "exit scheduled";'` | 4초 안에 종료 |

`U` = `unity --no-banner command --project-path "$PWD" --caller plugin --skill unity-cli`. 전역 옵션을 `command` 뒤에 두면 다음 낱말을 값으로 먹어 명령이 어긋났다(`editor status` → `status` 만 남음).

## 5. 빌드 스모크

| 플랫폼 | 경로 | 명령 | 결과 |
|---|---|---|---|
| Android 개발 APK | 열림 | `U build run --target Android --profileName Android-Dev --outputPath …/z.builds/debug/defense_20261030_3d4abc4_android-dev.apk --options '["Development"]' --confirm true` | 성공 · 240초(첫 IL2CPP) · 42,735,200 바이트 · 경고 · 오류 0. `aapt2 dump badging`: `com.devmankr.defense` · `Lantern Rush!` · debuggable · arm64-v8a · targetSdk 36 |
| WebGL | 닫힘 | `unity build . --profile Web --output-path ../z.builds/webgl/defense_20261030_4ec4f97_web --log-file Logs/build-web.log` | 성공 · 377초(Android → WebGL 전환 재임포트 포함) · 14 MB(`.wasm.br` 8.5 MB · `.data.br` 4.4 MB) · 출처 기록 `unity-build.provenance.json`(revision `4ec4f97` · dirty — 미커밋 문서). 로컬 서버(Brotli 헤더) + 헤드리스 Chrome(SwiftShader)으로 실제 25초 기다려 첫 화면 확인 |

`com.unity.pipeline` 런타임 포함 여부: 런타임 asmdef 는 `UNITY_EDITOR` 또는 `ENABLE_RUNTIME_PIPELINE(_DEV)` 일 때만 컴파일된다(`defineConstraints`). APK 의 IL2CPP `global-metadata.dat` 에서 `Unity.Pipeline` 0건 · `Defense.Core` 1건 · 빌드 보고서 산출 파일 663개 중 pipeline 관련 0개 — 들어가지 않았다.

## 6. 찾은 함정

1. **`build create_profile --platform Web` 이 PlayerSettings 전체 사본(944줄)을 프로필에 넣는다.** 그대로 두면 `ProjectSettings/` 를 바꿔도 Web 빌드에 반영되지 않는다. 내부 `RemovePlayerSettings(true)` 로는 지워지지 않았고, `SerializedObject` 로 `m_PlayerSettingsYaml.m_Settings` 를 비우자 재임포트 뒤에도 `HasSerializedPlayerSettings() == false`. Android 프로필은 사본 없이 만들어졌다. → `CLAUDE.md` 「검토 기준」 한 줄.
2. **`build run --profileName` 만 주면 프로필 플랫폼이 아니라 활성 대상(macOS)으로 빌드된다.** 패키지 `BuildCommand.cs` 가 대상을 프로필 적용 전에 `EditorUserBuildSettings.activeBuildTarget` 로 정하고, 개발 빌드 여부도 `options` 에서만 받는다. 첫 시도가 `.app` 을 만들어 지웠다. → `--target` · `--options` 필수로 `CLAUDE.md` 에.
3. 프로필 `Android-Dev` 는 `create_profile` 로 만들면 개발 빌드가 꺼져 있어 내부 `platformBuildProfile.development = true` 로 켰다(에디터 닫힘 `unity build --profile` 경로가 이 값을 쓴다).
4. 템플릿(URP 17.1) 에셋을 17.3 에디터가 처음 열 때 · 플랫폼마다 처음 빌드할 때 다시 저장한다(셰이더 프리필터 · 배칭 항목) — 열 때는 `3d4abc4` 에, 빌드 때는 `4ec4f97` · `4fdea24` 로 따로 커밋했다. → `CLAUDE.md` 「결정론 게이트」에 「빌드 뒤 `git status`, 자동 변환은 따로 커밋」. batchmode 라 「URP Material upgrade」 대화상자는 취소 기록만 남고 머티리얼 버전은 10 으로 올라갔다(Assets 에 머티리얼 0개).

## 7. 도구 확인

- `csharp-ls --diagnose`(`Defense.Unity` 에서) — `Defense.Unity.slnx` 를 찾아 로드 끝. 이 세션의 Claude Code LSP 는 프로젝트가 생기기 전에 떠서 문서 심볼 · 작업 공간 심볼만 되고 정의 이동 · 호버는 안 됐다 → 다음 세션에서 확인.
- External Script Editor 는 이미 VS Code(사용자 EditorPrefs). `.csproj` · `.slnx` · `.vscode/` 는 git 밖.

## 8. 코드 리뷰

위험 등급: 고(`ProjectSettings/` · `Packages/` · 빌드 프로필). D-05 대로 템플릿 생성물(`e46ecab`)은 범위에서 빼고 세션 센서 1 — 설정 차이는 사람이 직접 읽는다. 결정론 게이트: 컴파일 오류 0 · 새 경고 0 · EditMode 1 → 1 통과(고친 뒤 재실행, 스크립트가 바뀐 실행).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `e46ecab..a89d77b` | 16파일 +315/−32 | 세션 센서(A sonnet) + 판정 반론(D sonnet) | 2 · 105만 · 약 20분 | 16 | 9 | 0 | 1 | 1 | 5 | 0 |

원본 · 처리표: `../z.docs/Defense.Planning/P00_환경_체계_로깅/03_작업기록/코드리뷰_20261030_S2/`(처리.md 16행).

사람이 볼 행(blocking · important 이면서 new, 미해결, 데이터 손상 · 비밀값):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P00.S2.UNI.01 | question · important · new | BUILD | 닫힘 경로 Android 빌드 미실측 · 프로필 `m_BuildType: 2` 와 `m_Development: 1` 조합 | `Assets/Settings/Build Profiles/Android-Dev.asset:33,53` | 고침 | `59f9a07` — `CLAUDE.md` 에 미실측 · 확인 방법 표시 |
| P00.S2.UNI.02 | issue · important · new | BUILD | 승인 지점 변경을 기능 변경과 한 커밋(`3d4abc4`)에 | `3d4abc4` | 의도 | 계획서 7절 #6 · D-05 · 반론 수락 |
| P00.S2.UNI.03 | suggestion · important · new | BUILD | 플레이어에 명령 실행 서버를 넣는 정의 기호에 대한 검토 기준 없음 | `ProjectSettings/ProjectSettings.asset:838` | 고침 | `59f9a07` — `CLAUDE.md` 「검토 기준」 |
| P00.S2.UNI.12 | suggestion · nit · pre-existing | BUILD | 서명 자료 ignore 없음(비밀값 예방) | `.gitignore` | 고침 | `59f9a07` |

판정 반론: 의도 1 → 수락 1 · 반박 0. 범위 밖 1(pre-existing — H-02). 처리 안 함 5(nit 3 · plausible 2). 고침이 ignore 줄 · 주석 · 문서뿐이라 2라운드 없음. 종료 조건 충족.

사람이 직접 읽을 설정 차이: `git diff e46ecab..59f9a07 -- Packages/manifest.json ProjectSettings/ProjectSettings.asset` — manifest 1항목(`com.unity.pipeline`) · PlayerSettings 회사 · 제품명 · 번들 ID 3곳 · 플랫폼별 배칭 항목 2개(빌드 때 에디터가 더함).

## 9. 확인하지 않은 것 · 추정

- 실기기 설치 · 실행(APK) — 사용자 실행 요청 범위.
- PlayMode 게이트 명령 — PlayMode 테스트가 없어 `DEF-M01` 에서 실측.
- 에디터 닫힘 경로의 Android 빌드(`unity build --profile Android-Dev`)와 열림 경로의 WebGL 빌드 — 각 플랫폼은 한 경로로만 빌드했다.
- WebGL 을 실제 GPU 브라우저에서 — 헤드리스 SwiftShader 에서는 콘솔에 숨은 셰이더 3개(`Hidden/CoreSRP/CoreCopy` · `Hidden/Universal Render Pipeline/StencilDitherMaskSeed` · `Hidden/Universal/HDRDebugView`) 「not supported on this GPU」 오류가 찍혔다. 화면은 정상.
- 「WebGL 기본 품질은 Mobile 인데 PC 에셋이 바뀐 까닭」은 센서가 소스로 답했다(△ `P00.S2.UNI.07` — PC 단계가 WebGL 을 제외하지 않아 둘 다 빌드에 들어간다). 정리는 H-02.
- 닫힘 경로 Android 빌드(`unity build --profile Android-Dev`)가 debuggable APK 를 내는지(`P00.S2.UNI.01`).
- Claude Code 세션에서 `csharp-lsp` 의 정의 이동 · 호버(7절).
- Unity 플러그인 상시 컨텍스트 비용(`/context`) — 세션이 직접 볼 수 없다.

## 10. 다음

- Planning: `_템플릿/Unity_CLAUDE.md` 를 이 저장소 `CLAUDE.md` 와 같게(S4 흡수분) · P01 계획서 2차 착수 가능.
- C# 코딩 표준 Unity 판은 다른 세션이 정리 중 — 받으면 `z.docs/dev/csharp_coding_standards.md` 로 넣고 `CLAUDE.md` 「코딩」에서 가리킨다.
