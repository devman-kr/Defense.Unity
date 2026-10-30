# Defense.Unity

Defense 게임의 Unity 6 · URP 프로젝트이자 공개 포트폴리오 저장소. 게임 기획 · 특징은 `../z.docs/Defense.Design/`(G-10)에만 있고 이 문서에는 적지 않는다. 작업 공간 규칙은 `../CLAUDE.md`(루트), 세 게임 공통은 `../../CLAUDE.md`, 과제 · 결정은 `../z.docs/Defense.Planning/`. 이 문서는 저장소 안 작업(코드 · 문서 · 커밋)의 규칙이고, 루트와 충돌하면 이 문서가 우선한다. 정본은 `../z.docs/Defense.Planning/_템플릿/Unity_CLAUDE.md` 이고, 고치면 정본도 같이 고친다.

## 작업 시작 전
- 작업 계획을 다음 형식으로 제시한 뒤 승인을 기다린다:
  - 목표:
  - 영향 파일:
  - 작업 단계(TodoWrite):
- Planning 지시서(`../z.docs/Defense.Planning/…/02_계획/지시서/`)로 시작한 세션은 지시서의 「성공 기준 · 위험 · 되돌리는 법」 · 「승인 지점」을 따른다 — 위험 등급이 저(문서 · 테스트 · 주석 · UI 문자열 · 데이터 에셋 수치 아님)면 「지시서와 다른 점」 한 줄로 알리고 바로 착수, 중 · 고는 위 형식대로 승인. 등급은 `../z.docs/Defense.Planning/코드리뷰_가이드.md` 5절.
- 설계와 다른 구현이 필요하면 먼저 `../z.docs/Defense.Design/기획서.md` 를 고치자고 제안한다. 코드가 기획서보다 앞서가지 않는다.

### 사용자와 논의가 필요한 사항 발견 시
- 권장안(프로젝트 도메인 · 일관성 · 안정성 기준)과 제언안(세션의 인사이트)을 같이 제안한다.

## 작업 중
- 명시적으로 지시된 내용 외에는 하지 않는다. 추가 작업이 필요하면 이유와 계획을 승인받는다.
- 읽기 전용 명령은 바로 실행한다. 파일 · 상태 · 외부 자원을 바꾸거나 네트워크로 보내는 명령은 확인을 받는다.
- 에디터는 세션이 띄운다(G-12 — 명령은 「결정론 게이트」의 에디터가 열려 있을 때). 에디터 조작(씬 · 프리팹 · 컴포넌트 · 플레이 모드 · 빌드)은 `unity-cli` 로 주 세션만 한다. 사용자가 연 에디터의 종료 · 재시작 · 패키지 추가 · `ProjectSettings/` 변경 · 플랫폼 전환은 승인 지점.

## 작업 후
- 작업 내용을 요약해 전달한다. 바꾸는 곳 밖의 원본 내용은 보존한다.
- `z.docs/work_reports/{yyyyMMdd}/{그날 순번}_{verb}_{제목}.md` 작업 보고서를 쓴다. 프론트매터 `work_unit` 은 과제 ID(`P01`).
- 모든 작업 후 커밋한다.

## git 워크플로우
- 커밋 메시지는 한글, `verb: [Pnn] 제목` — verb 는 `create` · `modify` · `fix` · `refactor` · `docs` · `delete` · `test`.
- 커밋 전에 `git diff` 와 `git status` 를 확인한다. `.meta` 가 짝으로 들어갔는지, `Library/` · `Temp/` · `Logs/` · `obj/` · `Builds/` · `UserSettings/` 가 안 들어갔는지 본다.
- 트레일러는 그 세션 모델 이름으로 `Co-Authored-By: Claude <모델> <noreply@anthropic.com>` 하나(`_공통` G-13 — 예: `Claude Opus 5.5`).
- 푸시는 루트 `CLAUDE.md` 「푸시」대로(허락 뒤 주 세션만).

## Agent Teams 병렬 구현
- 파일을 고치는 병렬 작업은 worktree 격리. 에이전트마다 폴더 영역을 겹치지 않게 나누고, **씬 · 프리팹 · `.asset` 은 한 에이전트만** 만진다.
- `main` 은 작업 중 바꾸지 않는다. 머지 방식은 사용자가 정한다(PR 검토 머지 권장 · 순차 머지 · 브랜치 삭제로 롤백).
- 서브에이전트는 코드 · 컴파일 · EditMode 테스트까지. 에디터 · 빌드는 주 세션.

## 코딩 (C# · Unity)
규칙의 정본은 **`z.docs/dev/csharp_coding_standards.md`**(C# 코딩 표준 — Unity, Planning 정본 `../z.docs/Defense.Planning/_템플릿/Unity_csharp_coding_standards.md`)다. 여기는 폴더 · 어셈블리 · 자주 어기는 금지만 두고 같은 규칙을 두 곳에 적지 않는다. 서식은 `.editorconfig`(정본 `../z.docs/Defense.Planning/_템플릿/Unity_.editorconfig`).
- 폴더: `Assets/_Project/Core/`(asmdef `Devman.Core` — 다른 게임에서 수정 없이 쓰는 것, 다음 게임이 복사해 간다 G-07) · `Assets/_Project/Defense/`(asmdef `Defense.Game` — 이 게임 규칙) · `Assets/_Project/Data/`(ScriptableObject 에셋) · `Assets/_Project/Art/` · `Assets/_Project/Scenes/` · `Assets/_Project/Audio/` · `Assets/ThirdParty/`(외부 에셋 — `ASSETS.md` 장부 필수). 테스트는 어셈블리마다 짝 — `Assets/_Project/Core/Tests/EditMode/`(asmdef `Devman.Core.Tests.EditMode`, 코어와 같이 복사) · `Assets/_Project/Tests/EditMode/`(`Defense.Tests.EditMode`) · `Assets/_Project/Tests/PlayMode/`(`Defense.Tests.PlayMode`). 에디터 코드는 `Editor/` + 에디터 asmdef.
- 어셈블리: 코드 폴더마다 asmdef, 이름 = 루트 네임스페이스. **Core 는 Game 을 참조하지 않는다.** 코어 · 게임 경계는 표준 §1.
- 자주 어기는 금지(표준 절): 코드에 박힌 밸런스 수치 · 문구(§8) · 런 중 `Instantiate` · `Destroy` · 개체마다 `Update` · 매 프레임 할당(§5 · §7 · §9) · `UnityEngine.Object` 에 `?.` · `??`(§6) · 저장 DTO 에 프로퍼티(§12) · 핫패스 `Debug.Log`(§13) · 싱글톤 · `Find*`(§7) · 파일 헤더 이력(§2).
- 외부 패키지는 MIT · Apache · CC0 등 재배포 가능한 것만, `Packages/manifest.json` 변경은 승인 지점이고 `ASSETS.md` 에 적는다. Asset Store 에셋은 커밋하지 않는다.

## 결정론 게이트 (커밋 전)
명령은 이 저장소 루트에서 실행한다. `unity` 는 Unity CLI(`~/.unity/bin`). 결과 파일 · 로그는 `Logs/`(git 밖). 시간은 2026-10-30 · Unity 6000.3.27f1 · Mac M5 실측.

**통과 기준**
- 컴파일 오류 0 · 새 경고 0.
- EditMode 테스트 전부 통과. 런 흐름을 바꿨으면 PlayMode 스모크(자동 선택 + 가속 한 판).
- 빌드 설정 · 플랫폼을 바꿨으면 대상 빌드 한 번(`Android-Dev` · `Web` 빌드 프로필).

**에디터가 닫혀 있을 때(batchmode — 에디터 작업이 없을 때의 기본)**
- 컴파일 + EditMode: `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` — 종료 코드 0 통과 · 8 테스트 실패 · 6 판정 없음(컴파일 오류 · 라이선스 · 시간 초과). 약 20초.
- 경고 · 오류 줄: `grep -E 'warning CS|error CS' Logs/gate-editmode.log` 가 0줄. 스크립트가 바뀐 실행에서만 컴파일 줄이 찍혀, 바뀐 것이 없으면 경고가 남아 있어도 0줄이다 — 커밋 전(스크립트를 고친 뒤) 실행에서 본다. 전체 경고 수가 필요하면 열린 에디터의 `console status`.
- PlayMode: 같은 명령에 `--mode PlayMode`(PlayMode 테스트가 생기는 `DEF-M01` 에서 실측).
- 빌드: `unity build . --profile Web --output-path ../z.builds/webgl/<이름> --log-file Logs/build-web.log` · `unity build . --profile Android-Dev --output-path ../z.builds/debug/<이름>.apk`(**미실측** — 프로필이 `m_Development: 1` · `m_BuildType: 2` 라 첫 실행 뒤 `aapt2 dump badging` 으로 `application-debuggable` 인지 본다). WebGL 377초(Android 에서 전환 재임포트 포함), Android 개발 APK 240초(첫 IL2CPP, 열림 경로). 산출물 장부는 `../z.builds/BUILDS.md`.
- 빌드 뒤 `git status` — 플랫폼을 바꿔 빌드하면 에디터가 URP 에셋(`Assets/Settings/*_RPAsset.asset` 셰이더 프리필터 · `UniversalRenderPipelineGlobalSettings.asset`)과 `ProjectSettings.asset`(플랫폼별 배칭 항목)을 다시 저장한다. 기능 커밋과 섞지 않고 「에디터 자동 변환」 커밋으로 따로 둔다.

**에디터가 열려 있을 때(`unity command`)**
- 이 프로젝트를 연 에디터가 있으면 batchmode 명령은 시작 전에 거부된다(종료 6, `already open in a running Editor`). 둘 중 하나만 쓴다.
- `unity` CLI 는 실행마다 **macOS 키체인 접근 대화상자**(저장된 로그인 토큰 읽기)를 띄울 수 있다 — 사용자가 「항상 허용」을 누르기 전까지. 사용자가 자리에 없으면 그만큼 기다리므로(2026-10-30 실측 403초), 게이트를 돌릴 때 대화상자를 알리고 시간 측정에서 뺀다.
- 줄임: `U() { unity --no-banner command --project-path "$PWD" --caller plugin --skill unity-cli "$@"; }`. 전역 옵션은 `command` 앞에 둔다 — 뒤에 두면 다음 낱말을 값으로 먹는다.
- 에디터: 세션이 띄운 창 없는 에디터(G-12). 띄우기 전에 `pgrep -q -x Blender` 로 Blender 가 없는지 본다(한 기기 G-01). `"/Applications/Unity/Hub/Editor/6000.3.27f1/Unity.app/Contents/MacOS/Unity" -batchmode -projectPath "$PWD" -logFile Logs/editor-headless.log` 를 `-quit` 없이 백그라운드로. 준비는 `U editor status --result-only` 의 `"status": "ready"`(창 없는 에디터는 `unity status` 목록에 나오지 않는다).
- 끄기: 일이 끝나면 `U script eval --code 'UnityEditor.EditorApplication.delayCall += () => UnityEditor.EditorApplication.Exit(0); return "exit";' --result-only`(약 4초). 꺼졌는지는 `pgrep -q -f 'Unity.app/Contents/MacOS/Unity'` 의 참거짓으로만 본다 — 에디터 명령줄은 출력하지 않는다(`-accessToken` 이 들어 있을 수 있다).
- 사용자가 GUI 에디터를 열어 두었으면(`unity status` 에 나온다) 그것을 쓰고, 끄거나 재시작하지 않는다(승인 지점).
- 컴파일: `U script recompile --result-only` → `U script recompile_status --result-only` 가 `completed` · `up_to_date` → `U console status --result-only` 의 `groundTruth` 가 `compilationFailed: false` · `consoleErrors: 0` · `consoleWarnings: 0`.
- EditMode: `U test run --mode editor --result-only` → `Summary.Failed: 0`. 약 0.3초. PlayMode 는 `--mode playmode`.
- 플레이 모드: `U editor.playmode start` → `U editor status --result-only` 의 `playMode: playing` → `U editor.playmode stop`.
- 빌드: `U build run --target Android --profileName Android-Dev --outputPath <절대 경로>/z.builds/debug/<이름>.apk --options '["Development"]' --confirm true` → `U build status --result-only` 가 `completed` · `result: Succeeded`. **`--target` 을 꼭 준다** — 대상은 프로필 적용 전에 활성 대상으로 정해져, 빠지면 `--profileName` 이 있어도 지금 대상(macOS 등)으로 빌드된다. 개발 빌드 여부도 프로필이 아니라 `--options` 가 정한다(pipeline 0.9.1 `BuildCommand.cs`).
- 명령 목록 · 인자는 `U --detail full --format json`(166개, 2026-10-30). 플레이어 설정은 `U settings.player get` · `set`.

## 검토 기준
코드 리뷰 센서가 diff 에 대조하는 이 저장소의 실패 유형. 반복 지적은 테스트 · 분석기로 내리고 여기서 지운다. 10줄을 넘기면 `z.docs/dev/review_criteria.md` 로.
- 코딩 표준 `z.docs/dev/csharp_coding_standards.md` 전체 — 특히 §12 저장(포맷 버전 · 변환 테스트) · §8 재화 수치(데이터 에셋 · 전후 값) · §10 시간 · §9 풀 · §6 null. 지적은 표준 절 번호로. 아래는 표준이 못 담는 이 저장소 고유 실패만.
- 빌드 프로필: `Assets/Settings/Build Profiles/*.asset` 이 PlayerSettings 사본(`m_PlayerSettingsYaml.m_Settings` 가 빈 배열이 아님)을 품지 않는다 — 품으면 `ProjectSettings/` 를 바꿔도 그 프로필 빌드에 반영되지 않는다.
- 플레이어 원격 실행 차단: `ProjectSettings.asset` 의 `scriptingDefineSymbols` 와 빌드 프로필 `m_ScriptingDefines` 에 `ENABLE_RUNTIME_PIPELINE` · `ENABLE_RUNTIME_PIPELINE_DEV` 를 넣지 않는다 — 넣으면 `com.unity.pipeline` 의 명령 실행 서버가 플레이어 빌드에 들어간다(패키지 `Documentation~/runtime-setup.md`).

## 참조 경로
- 기획서: `../z.docs/Defense.Design/기획서.md`(기획 저장소 — Planning X-02)
- 마일스톤: `z.docs/dev/milestone/NN_slug/milestone.md`(`DEF-Mnn`)
- 작업 보고서: `z.docs/work_reports/{yyyyMMdd}/*`
- 에셋 장부: `ASSETS.md`
- 코딩 표준: `z.docs/dev/csharp_coding_standards.md`(정본 `../z.docs/Defense.Planning/_템플릿/Unity_csharp_coding_standards.md`) · 서식 `.editorconfig`(정본 `../z.docs/Defense.Planning/_템플릿/Unity_.editorconfig`)
- Unity 버전: `ProjectSettings/ProjectVersion.txt`(6000.3.27f1)
