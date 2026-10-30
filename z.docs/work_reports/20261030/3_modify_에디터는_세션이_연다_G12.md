---
id: 20261030-p00-g12-editor-opened-by-session
title: P00 에디터는 세션이 연다(G-12) — CLAUDE.md 작업 중 · 게이트 열림 경로
summary: 공통 결정 G-12(Unity 에디터는 세션이 Unity CLI 로 창 없는 에디터를 띄워 다루고 일이 끝나면 끈다)에 맞춰 CLAUDE.md 의 승인 지점과 열림 경로 명령을 고쳤다. 띄우기 · EditMode · 끄기를 문서 명령 그대로 실측했고, S1c 가 반영한 코딩 표준이 정본과 같고 게이트를 통과함을 다시 확인했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-30
updated: 2026-10-30
author: devman-kr
team: dev
work_unit: P00
work_type: modify
commits: [e227f1f]
related: [20261030-p00-s1c-coding-standard-apply]
---

# P00 에디터는 세션이 연다(G-12)

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-30 (Mac M5 16 GB, Claude Opus 5.5) |
| 근거 | 사용자 지시 「에디터는 에이전트가 연다로 기본값을 수정」 → `_공통` G-12. 시험 근거는 Planning P00 D-08 ④(S2 한 세션) |
| 위험 등급 | 저 — `CLAUDE.md` 문서만. 코드 · 씬 · 설정 · 패키지 변경 없음 |
| 증거 ① 게이트 | 에디터 닫힘 `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · `total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS'` 0 · 11초(변경 전 상태 확인용). 열림 경로는 2절 |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤). `diff -q CLAUDE.md ../z.docs/Defense.Planning/_템플릿/Unity_CLAUDE.md` → 같음 |
| 증거 ③ 캡처 | 없음 — 문서만 |

## 1. 바꾼 것

| 파일:절 | 전 | 후 |
|---|---|---|
| `CLAUDE.md` 「작업 중」 | 에디터 재시작 · 패키지 추가 · … 승인 지점 | 에디터는 세션이 띄운다(G-12). 승인 지점은 **사용자가 연** 에디터의 종료 · 재시작 · 패키지 추가 · … |
| 「결정론 게이트」 닫힘 경로 제목 | batchmode — 기본 | batchmode — 에디터 작업이 없을 때의 기본 |
| 「결정론 게이트」 열림 경로 | 에디터: 사용자가 연 에디터, 또는 세션이 띄운 창 없는 에디터 | 에디터: 세션이 띄운 창 없는 에디터 + 띄우기 전 `pgrep -q -x Blender` · **끄기** 줄(`U script eval … EditorApplication.Exit(0)` · 꺼짐은 `pgrep -q` 참거짓으로만) · 사용자 GUI 에디터는 쓰되 끄지 않음 |

## 2. 열림 경로 실측(문서 명령 그대로, 2026-10-30)

- Blender 없음 확인 → 창 없는 에디터 기동 → `U editor status` 가 `ready` 까지 4초(`Library/` 캐시 있음).
- `U test run --mode editor --result-only` → `"Failed": 0`.
- `U script eval --code '… EditorApplication.Exit(0); return "exit";'` → `"result": "exit"`, 프로세스 사라짐까지 1초. 뒤 `git status` 에 이 문서 변경 말고 없음.

## 3. 코딩 표준 적용 상태(사용자 확인 요청)

S1c(`aeccdae` · `6130ea4` · `4d20f5b`)가 이미 반영했다. 이번에 다시 본 것: `z.docs/dev/csharp_coding_standards.md` · `.editorconfig` 가 Planning 정본과 diff 0, `CLAUDE.md` 「코딩」이 표준을 가리킴, Core asmdef 이름 `Devman.Core`, 게이트 EditMode 2/2 통과 · 경고 0. 새로 넣은 것은 없다. 남은 것(`Defense.Game` asmdef · `Devman.Core.Tests.EditMode` · 템플릿 잔재)은 S1c 보고 7절대로 `DEF-M01`.

## 4. 코드 리뷰

코드 변경이 없어(문서 `CLAUDE.md` 한 파일, +5/−3) 세션 센서를 띄우지 않았다 — 루트 `CLAUDE.md` 「코드 리뷰」는 코드를 커밋한 세션이 대상. 위험 등급 저 — 사람이 볼 것은 기본 정보의 증거 3행.

## 5. 확인하지 않은 것 · 추정

- 사용자가 GUI 에디터를 열어 둔 상태에서 세션이 그 에디터를 `unity command` 로 쓰는 경로는 이번에 실측하지 않았다(S2 · 이번 모두 창 없는 에디터만).
- `pgrep -x Blender` 의 프로세스 이름 `Blender` 는 macOS `Blender.app` 실행 파일 이름 기준(세션 지식) — Blender 를 띄운 상태에서 확인하지 않았다.
- 키체인 대화상자: 게이트 실행(19:45:57)과 `unity auth status`(19:47:31) 두 번 떴고, 사용자가 둘째에 「항상 허용」을 눌렀다. 셋째 실행(19:47:50)은 대화상자 없이 1초 — `securityd` 로그 `displaying keychain prompt` 새 줄 0 ✔.
