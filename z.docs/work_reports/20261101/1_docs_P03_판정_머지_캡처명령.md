---
id: 20261101-p03-s3-judge-merge-docs
title: P03 S3 판정 — Opus 실행 브랜치 머지 · 보고 토큰 정정 · Blender README · ASSETS · CLAUDE.md 캡처 명령
summary: 판정 세션이 p03/exec-sonnet · p03/exec-opus 를 검산해(Planning P03 비교표) 사용자가 고른 Opus 브랜치를 main 에 --no-ff 머지(39266b4)하고, 머지 뒤 게이트 통과 · Opus 보고의 리뷰 토큰 정정 · Blender README · ASSETS 행 · CLAUDE.md 캡처 명령(D-07)을 더했다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-11-01
updated: 2026-11-01
work_unit: P03
work_type: docs
commits: [39266b4]
implements: [20261031-p03-handoff-s3-judge]
---

# P03 S3 판정 — Opus 실행 브랜치 머지 · 보고 토큰 정정 · Blender README · ASSETS · CLAUDE.md 캡처 명령 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-11-01 (Mac M5 16 GB, Claude Fable 5.1 — 세션 a3a90bb0, Planning P03 판정 세션 S3) |
| 근거 | Planning P03 지시서 S3(`20261031-p03-handoff-s3-judge`) · 비교표 `z.docs/Defense.Planning/z.completed/P03_CLI_환경테스트/03_작업기록/비교표_20261101.md` · 결정 D-11(사용자 선택 `p03/exec-opus`) |
| 위험 등급 | 저 — 머지(S2 가 세션 센서 2라운드를 거친 에디터 전용 스크립트 · 시험 씬 · 샘플 에셋) + 문서 5. 제품 런타임 코드 · `ProjectSettings/` · `Packages/` 변경 없음. 되돌리기: `git revert -m 1 39266b4` |
| 증거 ① 게이트 | 머지 직후 `main` 에서 닫힘 `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 9초 · `editmode-results.xml` `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS' Logs/gate-editmode.log` 0(주 체크아웃 `Library/` 에는 `Defense.Editor` 가 처음이라 이 실행에서 컴파일됐다). 머지 전 두 worktree 에서도 같은 명령 1회씩 통과(9초 · 7초 · 2/2 · 0줄) |
| 증거 ② 상태 | 이 보고 커밋 뒤 `git status --short` → 빈 출력. `git branch` 는 `main` 만(두 실행 브랜치는 번들 `../z.tmp/p03_results/<모델>/` 보관 뒤 삭제, worktree 제거) |
| 증거 ③ 캡처 | 머지된 씬의 캡처 · 동영상은 작업 공간 `../z.captures/20261031/opus/`(세로 · 가로 · `sample_48f_24fps.mp4` — Blender 로 다시 읽어 48프레임 ✔) · 비교용 세로 캡처 두 장은 Planning 비교표 `assets/`. 사용자 확인 「봤고 둘 다 괜찮다」(D-08). 확인 안 한 것은 3절 |

## 1. 바꾼 것

| 커밋 | 무엇 | 전 | 후 |
|---|---|---|---|
| `39266b4` | `merge: [P03] Opus 실행 브랜치 — 비교 판정`(`--no-ff`) | `main` = `763866b`(S0 끝) | S2 Opus 5.5 의 4 커밋(`a5a2a5c` · `c70f6dc` · `3a1dd18` · `9250f22`) — `Assets/00_Scenes/P03_CLI_Sample.unity` · `Assets/10_Scripts/Editor/`(asmdef `Defense.Editor` · `P03/` 3파일) · `Assets/30_Assets/Models/P03_CLI/`(FBX 6 · 팔레트 · `palette.mat` · `island.asset`) · `Tools/Blender/make_enemies.py` 수정 · S2 작업 보고 · 센서 기록 |
| 이 커밋 | `z.docs/work_reports/20261031/4_create_P03_샘플씬_opus.md` 8절 · `코드리뷰_opus/처리.md` 머리 | 리뷰 새 토큰 577,570 · 378,454(합 956,024 — `센서기록_추출.py` 가 한 응답의 여러 줄을 따로 더한 값) | 고유 응답 기준 245,918 · 161,141(합 407,059) + 정정 사유 한 문장. 센서 원본 · `summary.json` 은 그대로 |
| 같음 | `Tools/Blender/README.md` 표 | 「알려진 결함 둘 … P03 실행 세션이 고친다」 | S2 가 다듬은 판 — 날개 UV 층 `UVMap`, 회전축은 원근 착시라 렌더 시점만 변경, Blender 6.0 폐기 경고 |
| 같음 | `ASSETS.md` | P03 샘플 모델 행 없음 | `Assets/30_Assets/Models/P03_CLI/` 행(저장소 스크립트 산출 · MIT · AI 생성 — 스크립트 Fable 5.1, 수정 · 실행 Opus 5.5) · summary |
| 같음 | `CLAUDE.md` 「결정론 게이트」 에디터 열림 절(+ 정본 `../z.docs/Defense.Planning/_템플릿/Unity_CLAUDE.md`, `cmp` 같음) | 캡처 명령 없음 | 캡처 줄 하나(D-07) — `capture game_view --source camera` 명령 · 창 없는 에디터 조건(`-nographics` 없이) · `--save_path` 가 `Assets/` 기준인 함정 · 에디터 스크립트 렌더 경로 · 스킨 메시 플래그 · Blender MP4 · 기동 시간 |
| 같음 | 이 보고서 | — | `z.docs/work_reports/20261101/1_docs_P03_판정_머지_캡처명령.md` |

## 2. 검산 요약(비교표 1 · 2절의 요지)

두 브랜치 모두 성공 기준 8 · 게이트 통과 · 함정 둘 실측값 일치. Opus 는 43분 · 새 토큰 108만에 성공 기준 4 「머티리얼 하나」를 섬까지 지켰고, Sonnet 은 88분 · 242만에 섬 머티리얼을 따로 두었으나 녹화의 스킨 행렬 함정(`forceMatrixRecalculationPerRender`)을 찾아 고쳤다. 격리 감사(Logs 다이제스트)에서 두 세션 모두 다른 모델의 산출물에 접근하지 않았다. 보고와 실제가 다른 곳은 Opus 보고의 리뷰 토큰(2.3배)뿐이고 이 커밋이 정정했다.

## 3. 확인하지 않은 것 · 추정한 것

- 머지된 녹화 코드(`P03FrameRecorder`)는 스킨 행렬 플래그 없이도 Opus 세션에서 날개가 움직였다(픽셀 차 실측 △ · 판정 세션은 프레임 6 을 열어 봄). Sonnet 세션에서는 같은 구조에서 재현됐으므로 다른 씬 · 다른 에디터 상태에서 재현될 수 있다 — `DEF-M03` 녹화 도구는 플래그를 기본으로 켠다(`CLAUDE.md` 캡처 줄에 적음). 원인 차이는 조사하지 않았다.
- 두 세션의 시간 · 토큰은 세션이 자기 JSONL 을 집계한 값(△)이다. 판정 세션은 집계 방식(고유 응답 기준)이 서로 같음을 확인했고 숫자를 다시 세지는 않았다. 사용자의 `/cost` · 시작 · 끝 기록은 받지 못했다.
- MP4 는 Blender 로 다시 읽어 프레임 수 · 크기 · fps 만 확인했다. 플레이어에서 재생해 보지 않았다(두 세션도 같음).
- 두 worktree 게이트의 `warning CS` 0줄은 스크립트가 이미 컴파일된 `Library/` 에서 돌아 컴파일 줄이 없는 실행이다 — 경고 0 의 근거는 두 세션의 열린 에디터 `consoleWarnings 0`(△)과 머지 뒤 `main` 의 새 컴파일 실행(✔)이다.
- 센서는 띄우지 않았다 — 이 세션의 변경은 머지(코드는 S2 세션 센서 2라운드를 거침)와 문서뿐이고 코드 변경 0줄이다. 지시서 S3 도 센서를 두지 않았다.
- `Tools/Blender/README.md` 의 「Blender 6.0 폐기 경고」는 두 세션의 실행 로그(DeprecationWarning 2줄)에서 옮겼다(✔ Opus 다이제스트 573~574행).

## 4. 지시서와 다른 점

- 지시서 S3 는 진 브랜치만 삭제하라고 했으나 이긴 브랜치 `p03/exec-opus` 도 머지 뒤 삭제했다(커밋은 `main` 에 있고 번들도 보관) — worktree 를 지우려면 어차피 브랜치를 정리해야 했다.
- 사용자가 계획서 7절 (b) 격리 · (d) 복원 절차를 건너뛰어 두 브랜치가 worktree 로 그대로 있었다 — 번들은 판정 세션이 만들었다(`763866b..`).
- 이긴 쪽(Opus)의 캡처 README 도 Planning `03_작업기록/실행_opus/` 에 사본을 두었다(git 밖에만 있는 것을 남기려고).

## 5. 다음 — `DEF-M03` 에 넘긴 것

Planning P01 NOTES 4번 `DEF-M03` 메모를 두 보고의 「발견」 합집합으로 갱신했다(비교표 5절). 이 저장소에서 볼 것: `Assets/10_Scripts/Editor/P03/`(가져오기 설정 · 씬 빌더 · 프레임 녹화) · `Tools/Blender/make_enemies.py` · `CLAUDE.md` 캡처 줄. 샘플 모델을 본편에 쓸지는 `DEF-M03` 결정 → 기획서.
