---
id: 20261031-p00-trailer-remote-links
title: P00 트레일러 규칙(G-13) · README 개발 과정 링크(G-14)
summary: CLAUDE.md 트레일러 줄을 그 세션 모델 이름으로 적게 고치고(G-13), README 「개발 과정」에 Defense.Design · Planning · Logs · games-common 저장소 링크(G-14)를 넣었다. 마크다운 두 줄이라 Unity 게이트 대신 문서 검사를 증거로 썼다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P00
work_type: docs
commits: [4aa5df9, 5628a7d]
implements: [20261030-p00-plan-remaining-build]
---

# P00 트레일러 규칙(G-13) · README 개발 과정 링크(G-14) — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-30 · 2026-10-31 (Mac M5 16 GB, Claude Opus 5.5 — Planning P00 세션 10) |
| 근거 | 사용자 「전체 권장안으로 진행」(2026-10-30) → `_공통` G-13(트레일러는 그 세션 모델 이름) · G-14(계정 `devman-kr` · 저장소 이름은 폴더 이름 그대로 · `_공통` 은 `games-common`). Planning P00 계획서 추기 2차 · 3차 |
| 위험 등급 | 저 — 마크다운 두 파일, 각 한 줄. 코드 · 씬 · 설정 · 패키지 변경 없음 |
| 증거 ① 게이트 | 문서 검사(Games 루트에서 `python3 -I _공통/_도구/fm_lint.py --strict Defense _공통 CLAUDE.md`) → 오류 0. Unity 게이트(`unity test`)는 돌리지 않았다 — `Assets/` 밖 마크다운만 바뀌어 컴파일 · 테스트 입력이 같다(P02 S3 보고와 같은 판단) |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤). `cmp CLAUDE.md ../z.docs/Defense.Planning/_템플릿/Unity_CLAUDE.md` → 같음 |
| 증거 ③ 캡처 | 없음 — 문서만. 링크가 열리는지는 GitHub 저장소를 만든 뒤(P00 S5) 본다 |

## 1. 바꾼 것

| 커밋 | 파일:절 | 전 | 후 |
|---|---|---|---|
| `4aa5df9`(2026-10-30) | `CLAUDE.md` 「git 워크플로우」 트레일러 줄 | 트레일러는 `_공통` Q-01 결정대로. 결정 전: `Claude Fable 5.1` 고정 | 그 세션 모델 이름으로 `Co-Authored-By: Claude <모델> <noreply@anthropic.com>` 하나(G-13 — 예: `Claude Opus 5.5`). 정본 `_템플릿/Unity_CLAUDE.md` 도 같이 고쳤다(Planning) |
| `5628a7d`(2026-10-31) | `README.md` 「만든 방법」 끝 「개발 과정」 줄 | (채움 — `_공통` Q-02 · Q-03 결정 뒤 저장소 링크) | `Defense.Design` · `Defense.Planning` · `Defense.Logs` · `games-common` 링크(`https://github.com/devman-kr/<이름>`)와 저장소마다 담긴 것 한 마디. 골격 `_템플릿/Unity_README.md` 의 같은 줄은 G-14 주소 꼴 자리표시로 바꿨다(Planning) |

`README.md` 는 공개 저장소 첫 화면이라 프론트매터를 붙이지 않는다(`_공통/문서규약.md` 1절 예외).

## 2. 코드 리뷰

코드 변경이 없어(마크다운 두 줄) 세션 센서를 띄우지 않았다 — 루트 `CLAUDE.md` 「코드 리뷰」는 코드를 커밋한 세션이 대상이다. 위험 등급 저 — 사람이 볼 것은 기본 정보의 증거 3행.

## 3. 확인하지 않은 것 · 추정

- README 의 네 링크는 GitHub 저장소를 만들기 전에 넣었다 — 첫 푸시 뒤(P00 S5) 공개 화면에서 열리는지 본다.
- `4aa5df9` 이전 커밋의 트레일러 `Claude Fable 5.1` 은 고정 문구라 실제 모델과 다를 수 있고, 고치지 않는다(G-13).
- 이 저장소는 작성자 재작성(`_공통` G-16, 2026-10-31) 대상이 아니다 — 처음부터 noreply 라 해시가 그대로다.

**추기 2026-10-31(P00 종결)**: `z.docs/dev/csharp_coding_standards.md` 끝 「원문 · 결정 경위」 줄의 검토서 경로를 Planning `z.completed/P00_환경_체계_로깅/01_검토/검토서_20261030_코딩표준.md` 로 고쳤다 — P00 과제 폴더를 `z.completed/` 로 옮겨서다. 정본 `_템플릿/Unity_csharp_coding_standards.md` 를 같이 고쳐 `cmp` 같음. 이 추기와 같은 커밋이다. 문서 검사 오류 0.
