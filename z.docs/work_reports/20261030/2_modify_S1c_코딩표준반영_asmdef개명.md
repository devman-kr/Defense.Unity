---
id: 20261030-p00-s1c-coding-standard-apply
title: P00 세션 S1c 코딩 표준 반영 — 사본 2 · CLAUDE.md 「코딩」 · asmdef Defense.Core → Devman.Core
summary: Planning 에서 Unity 판으로 고친 C# 코딩 표준과 .editorconfig 를 저장소에 사본으로 넣고, CLAUDE.md 를 템플릿 정본과 같게 했다. 결정 D-07 ⑤ 대로 Core asmdef 와 루트 네임스페이스를 Devman.Core 로 바꾸고 IVT · 테스트 참조를 맞춘 뒤 batchmode EditMode 게이트를 통과했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-30
updated: 2026-10-30
author: devman-kr
team: dev
work_unit: P00
work_type: modify
tags: [defense, 코딩표준, asmdef, 환경]
commits: [aeccdae, 6130ea4]
related: [1_create_저장소_골격.md]
---

# P00 세션 S1c — 코딩 표준 반영 · asmdef 개명

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-30 (Mac M5 16 GB, Claude Fable 5.1) |
| 근거 | Planning `P00_환경_체계_로깅/02_계획/계획서_20261030.md` 4절 S1c · 결정 D-06 · D-07 · 검토서 `01_검토/검토서_20261030_코딩표준.md` |
| 위험 등급 | 중 — asmdef 이름 · 네임스페이스 개명(`BUILD`). 고위험 경로(`ProjectSettings/` · `Packages/` · 저장 · 재화 · 빌드 프로필)는 건드리지 않음 |
| 증거 ① 게이트 | `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log`(에디터 닫힘) → 종료 0. 개명 뒤 `total="1" passed="1" failed="0"` · 14초, 센서 고침 뒤 `total="2" passed="2" failed="0"` · 410초(그중 403초는 키체인 대화상자 대기 — 6절). 두 번 다 `grep -cE 'warning CS\|error CS' Logs/gate-editmode.log` → 0. 로그에 `Devman.Core.dll` · `Defense.Tests.EditMode.dll` 컴파일 흔적 |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤, 커밋 `aeccdae` · `6130ea4`). `git ls-files \| grep -cE '^(Library\|Temp\|Logs\|obj\|Builds\|UserSettings)/'` → 0. 바뀐 `Assets/` 파일 4개 모두 `.meta` 짝 있음 |
| 증거 ③ 캡처 | 없음 — 코드 · 문서만. 확인하지 않은 것은 6절 |

## 1. 한 줄 결론

코딩 표준이 저장소 안에 있고(`z.docs/dev/csharp_coding_standards.md` · `.editorconfig`), `CLAUDE.md` 가 그것을 가리키며, Core 어셈블리는 게임 이름이 없는 `Devman.Core` 다. 게이트는 그대로 통과한다.

## 2. 시작 상태(실측)

- S2 결과 `4dfe06d` — Core asmdef `Defense.Core`(rootNamespace 같음), 테스트 asmdef `Defense.Tests.EditMode` 가 그것을 참조, `AssemblyInfo.cs` 의 IVT 가 `Defense.Tests.EditMode`, 스모크 테스트가 `Assembly.Load("Defense.Core")`.
- `CLAUDE.md` 「코딩」은 S2 시점 템플릿(`Defense.Core.*` · 이름 · 하드코딩 · 성능 · 주석 줄). 표준 · `.editorconfig` 사본 없음.
- Unity 에디터 · Blender 꺼짐(G-01). 다른 Claude 세션 유휴 20분 이상, 작업 트리 깨끗.

## 3. 만든 것 · 바꾼 것

| 파일 | 변경 | 근거 |
|---|---|---|
| `z.docs/dev/csharp_coding_standards.md` | 신규 — Planning 정본 `_템플릿/Unity_csharp_coding_standards.md` 사본(diff 0) | D-06 ① |
| `.editorconfig` | 신규 — 정본 `_템플릿/Unity_.editorconfig` 사본(diff 0). IDE 용, Unity 컴파일러는 읽지 않음 | D-06 ⑦ |
| `CLAUDE.md` | 템플릿 `_템플릿/Unity_CLAUDE.md` 와 같게(diff 0) — 「코딩」은 폴더 · asmdef · 자주 어기는 금지 + 표준 참조, 「검토 기준」 4줄 → 표준 참조 1줄, 「참조 경로」에 표준 · `.editorconfig` | 검토서 4.4 분담 |
| `Assets/_Project/Core/Devman.Core.asmdef`(+`.meta`) | `Defense.Core.asmdef` 에서 `git mv`, `name` · `rootNamespace` 를 `Devman.Core` 로. 다른 키는 그대로 | D-07 ⑤ |
| `Assets/_Project/Core/AssemblyInfo.cs` | IVT 를 `Devman.Core.Tests.EditMode` 로 — Core 의 internal 은 Core 자기 테스트 어셈블리에만, 게임 이름 없음. 그 어셈블리는 Core 에 첫 타입이 생기는 `DEF-M01` 에서 만든다(없는 어셈블리를 가리키는 IVT 는 무해) | 표준 §15.5 |
| `Assets/_Project/Tests/EditMode/Defense.Tests.EditMode.asmdef` | `references` 의 `Defense.Core` → `Devman.Core` | — |
| `Assets/_Project/Tests/EditMode/GateSmokeTests.cs` | `Assembly.Load("Devman.Core")` · 단언 이름 | — |

하지 않은 것(계획 범위 밖, 권장대로 `DEF-M01` 로): `Assets/_Project/Defense/`(asmdef `Defense.Game`) · `Core/Tests/EditMode/`(`Devman.Core.Tests.EditMode`) · `Tests/PlayMode/` 생성, 템플릿 잔재(`Assets/Scenes/SampleScene` · `Assets/TutorialInfo/` · 루트 `InputSystem_Actions.inputactions`) 정리.

## 4. 결정론 게이트 실측

- 에디터 닫힘(batchmode) 경로만 — 열린 에디터 경로(`unity command`)는 이번에 쓰지 않았다.
- asmdef 이름이 바뀌어 Bee 가 `Devman.Core.dll` 을 새로 만들었고 `Library/` 는 그대로(재생성 없음). 14초(S2 실측 약 20초보다 짧음 — 캐시).
- 빌드는 돌리지 않았다 — 빌드 설정 · 플랫폼 변경 없음(`CLAUDE.md` 「결정론 게이트」 통과 기준).

## 5. 코드 리뷰

위험 등급: 중(asmdef · 네임스페이스 개명 — `BUILD`, 고위험 경로 없음). 결정론 게이트: 컴파일 오류 0 · 새 경고 0 · EditMode 1 → 2 통과, 실패 증가 0 · PlayMode 없음(런 흐름 변경 아님) · 빌드 없음(빌드 설정 변경 아님).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `4dfe06d..aeccdae` | 코드 5파일(asmdef 2 · `.cs` 2 · `.editorconfig`) +~90/−8, 문서 2 | 세션 센서(A sonnet) | 1 · 41.9만 · 5분 | 6 | 2 | 0 | 0 | 1 | 3 | 0 |

원본 · 처리표: `../z.docs/Defense.Planning/P00_환경_체계_로깅/03_작업기록/코드리뷰_20261030_S1c/`(처리.md 6행). 2라운드 없음 — 수정 커밋 `6130ea4` 가 장부 2행 · 테스트 1개뿐이고 게이트를 통과했다(S2 선례).

사람이 볼 행(blocking · important 이면서 new, 미해결):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P00.S1c.UNI.01 | question · important · new | CONTRACT | 장부 누락 — 표준 · `.editorconfig` 의 출처 · 라이선스 · AI 생성 표기 없음(G-02) | `ASSETS.md` | 고침 | `6130ea4` — 「파일」 2행. POCU 표준의 이용 조건은 확인하지 않음(6절) |

나머지: UNI.02(nit — IVT 이름 고정 테스트, 고침 `6130ea4`) · UNI.03(thought — 플레이어 빌드 미검증, 게이트 기준 충족 · 다음 빌드 때) · UNI.04(thought — IVT 대상 변경 사유는 3절 표) · UNI.05(범위 밖 — 과거 기록의 옛 이름) · UNI.06(thought — 보고서 순서). 판정 반론 없음(「틀림 · 의도」 0). 종료 조건 충족.

## 6. 확인하지 않은 것 · 추정

- `.editorconfig` 가 VS Code C# · Rider 에서 의도대로 동작하는지(명명 규칙은 Planning S1b 2라운드 센서가 .NET SDK 10 Roslyn 으로만 실측). 열린 에디터 경로의 게이트 · `csharp-lsp` 의 정의 이동은 P00 NOTES 4번 ① 그대로.
- 없는 어셈블리를 가리키는 `InternalsVisibleTo` 가 무해하다는 것은 C# 컴파일러 동작(세션 지식) — 이번 게이트가 경고 0 으로 통과한 것이 유일한 실측이다.
- `Library/Bee/PlayerScriptAssemblies/` 에 옛 `Defense.Core.dll` 이 남아 있다(센서 UNI.03) — 커밋 대상 아니고 다음 빌드가 다시 만든다. Android-Dev · Web 빌드로 개명 뒤 플레이어 컴파일을 확인하지 않았다.
- 둘째 게이트 실행의 410초는 **macOS 키체인 접근 대화상자**를 기다린 시간이다(사용자 질문으로 확인 ✔). `securityd` 로그: 19:28:33 「displaying keychain prompt for `~/.unity/bin/unity`」 → 19:35:16 「user approved 'allow'」, 에디터 시작 19:35:16, 결과 19:35:23 — 실제 게이트는 7초. 첫 실행(19:17:46 → 19:18:00)도 같은 대화상자가 떴고 14초 안에 답했다. `unity` CLI 가 저장된 로그인 토큰을 키체인에서 읽을 때마다(Hub `broker-audit` 「stored-token-valid」) 뜨며, 「항상 허용」을 누르기 전까지는 실행마다 뜬다. 게이트 시간 측정은 이 응답 시간을 뺀다.
- POCU C# 코딩 표준(`docs.popekim.com`)의 이용 조건은 확인하지 않았다. 이 저장소의 표준은 규칙 체계만 그 계열이고 문장은 새로 썼다(`ASSETS.md`).

## 7. 다음

- `DEF-M01`(P01 B): `Defense.Game` asmdef · `Devman.Core.Tests.EditMode` 어셈블리 · 템플릿 잔재 정리 · 코어 소속 결정(웨이브 디렉터 · 드래프트 · 스탯 — P00 D-07 비고).
- Planning: S1c 완료 기록(계획서 8절 완료 기준 · 현황판 · NOTES).
