---
id: 20261031-p02-s3-frontmatter-migration
title: P02 S3 문서 프론트매터 이행 — 문서 3건 · 작업 보고 줄 · 보고서 3건 바로잡음
summary: 문서규약(G-18) 블록을 ASSETS.md · CLAUDE.md · csharp_coding_standards.md 앞에만 붙이고(본문 바이트 그대로 · 정본과 cmp 같음), 20261030 작업 보고서 3건의 프론트매터를 규약에 맞추고, CLAUDE.md 작업 보고 줄이 규약을 가리키게 했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P02
work_type: docs
commits: [26584fb]
implements: [20261030-p02-plan-frontmatter]
---

# P02 S3 문서 프론트매터 이행 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-31 (Mac M5 16 GB, Claude Opus 5.5) |
| 근거 | 사용자 지시 「모든 마크다운 문서에 프론트매터」 → `_공통` G-18 · 문서규약 `../../_공통/문서규약.md`. Planning P02 계획서 S3, 승인 ②(표본 10건 · 저장소별 건수를 보고 2026-10-31 승인) |
| 위험 등급 | 저 — 마크다운 6파일. 코드 · 씬 · 설정 · 패키지 변경 없음 |
| 증거 ① 게이트 | 문서규약 엄격 검사(Games 루트에서 `python3 -I _공통/_도구/fm_lint.py --strict Defense/Defense.Unity`) → 오류 0 · 경고 0. Unity 게이트(`unity test`)는 돌리지 않았다 — `Assets/` 밖 마크다운만 바뀌어 컴파일 · 테스트 입력이 같다 |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤). `cmp` 로 `CLAUDE.md` · `z.docs/dev/csharp_coding_standards.md` 가 Planning 정본(`_템플릿/Unity_CLAUDE.md` · `_템플릿/Unity_csharp_coding_standards.md`)과 같음 |
| 증거 ③ 캡처 | 없음 — 문서만 |

## 1. 바꾼 것

| 파일 | 바뀐 것 |
|---|---|
| `ASSETS.md` | 맨 앞에 블록 10줄(`doc_type: register` · `lifecycle: living`). 본문 그대로 |
| `CLAUDE.md` | 맨 앞에 정본과 같은 블록. 「작업 후」 작업 보고 줄: 「프론트매터 `work_unit` 은 과제 ID(`P01`)」 → 「프론트매터는 문서규약(G-18)대로 — `doc_type: work_report` · `work_unit` 은 과제 ID · `work_type` 은 verb · `commits` 는 보고하는 커밋」. 정본을 고치고 복사했다 |
| `z.docs/dev/csharp_coding_standards.md` | 맨 앞에 정본과 같은 블록. 본문 그대로 |
| `z.docs/work_reports/20261030/1~3` | 이미 있던 프론트매터를 규약에 맞춤 — 1: summary 를 200자 이하로 · `tags` 를 영역 코드(`BUILD` · `TEST`)로. 2: `tags` 를 `BUILD` 로. 2 · 3: `related` 를 파일 이름에서 문서 id 로. 3: 맞는 영역 코드가 없어 `tags` 를 뺐다 |
| `README.md` | 바꾸지 않음 — 공개 저장소 첫 화면은 규약 1절 예외 |

## 2. 검증

- 이행 도구는 `_공통/_도구/fm_migrate.py apply` 다. 원본 바이트 앞에 블록만 붙이고, 쓰기 전후로 원본과 대조한다. 끝난 뒤 따로 세 파일을 「블록 + `HEAD` 판」과 대조했다(3/3 같음).
- `git show --numstat 26584fb` 결과:
  - `ASSETS.md` · `z.docs/dev/csharp_coding_standards.md` 는 +10/−0 이다.
  - `CLAUDE.md` 는 +11/−1 이다(블록과 바꾼 한 줄).
  - 보고서 셋은 +2/−2 · +2/−2 · +1/−2 이다(프론트매터 줄만).
- 세 게임 전체 이행(125건)은 Planning P02 `03_작업기록/작업색인.md` 에 저장소별 커밋으로 적었다.

## 3. 코드 리뷰

이 저장소에는 코드 변경이 없어(마크다운 6파일) 세션 센서를 띄우지 않았다. 이 세션이 커밋한 코드(`_공통/_도구` 의 `fm_lint.py` · `fm_migrate.py` · 로그 훅)는 S2 에서 세션 센서와 판정 반론을 거쳤다. 기록은 Planning `P02_문서_프론트매터/03_작업기록/코드리뷰_20261030_S2/` 에 있다. 위험 등급이 저이므로 사람이 볼 것은 기본 정보의 증거 3행이다.

## 4. 확인하지 않은 것 · 추정

- GitHub 이 `CLAUDE.md` · `ASSETS.md` 맨 앞의 프론트매터를 표로 그리는지 — 공개(P00 S5) 뒤 화면으로 본다.
- 프론트매터가 붙은 `CLAUDE.md` 를 새 세션이 그대로 따르는지 — 이 세션에서 Claude Code 가 이 보고서를 쓴 직후 `CLAUDE.md` 를 다시 읽어 넣었는데(2026-10-31), 바꾼 작업 보고 줄은 있고 맨 앞 블록은 빠져 있었다. 하네스가 블록을 떼고 본문만 지시로 넣는다 ✔(한 번 관찰). 새 세션을 시작할 때도 같은지는 P02 S4 뒤에 본다.
