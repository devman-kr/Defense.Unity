---
id: 20261031-p03-s0-folder-layout
title: P03 S0 준비 — 번호 폴더 체계(D-09) · Tools/Blender 시작 스크립트(GPL) · 규칙 문서
summary: Assets/_Project/Core · Tests 를 Assets/10_Scripts/ 아래로 옮기고 00_Scenes · 20_Animations · 30_Assets 를 신설해(D-09) 규칙 문서 · ASSETS.md 를 맞추고 Tools/Blender/ 에 make_enemies.py 와 GPL LICENSE 를 넣었다. 게이트 2/2.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P03
work_type: refactor
commits: [7f88bef, 1f87826]
implements: [20261031-p03-plan-cli-pipeline]
---

# P03 S0 준비 — 번호 폴더 체계(D-09) · Tools/Blender 시작 스크립트(GPL) · 규칙 문서 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-31 (Mac M5 16 GB, Claude Fable 5.1 — Planning P03 계획 세션 a3a90bb0) |
| 근거 | Planning P03 [계획서](../../../../z.docs/Defense.Planning/P03_CLI_환경테스트/02_계획/계획서_20261031.md) S0 · 결정기록 D-09(번호 폴더 · `Tools/Blender/`) · D-10(S0 는 계획 세션이) · `_공통` G-17(bpy 스크립트 폴더는 GPL). 계획 제시 뒤 사용자 승인 「승인 — 센서는 생략」(2026-10-31) |
| 위험 등급 | 저 — 파일 이동(git mv, 코드 내용 변경 없음) · 마크다운 · `Assets/` 밖 Python 스크립트 · 라이선스 원문. 고위험 경로(`ProjectSettings/` · `Packages/` · 저장 · 재화) 변경 없음 |
| 증거 ① 게이트 | 에디터 닫힘 경로 `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 10.3초 · `grep -E 'warning CS\|error CS' Logs/gate-editmode.log` 0줄 · `editmode-results.xml` `result="Passed" total="2" passed="2" failed="0"`(이동 뒤 실행 — 스크립트가 옮겨져 컴파일이 다시 돌았다). 문서 검사 `fm_lint.py --strict Defense _공통 CLAUDE.md` 오류 0 |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서 커밋 뒤). `cmp CLAUDE.md ../z.docs/Defense.Planning/_템플릿/Unity_CLAUDE.md` · `cmp z.docs/dev/csharp_coding_standards.md …/Unity_csharp_coding_standards.md` → 같음 |
| 증거 ③ 캡처 | 없음 — 화면이 바뀌는 작업이 아니다 |

## 1. 바꾼 것

| 커밋 | 무엇 | 전 | 후 |
|---|---|---|---|
| `7f88bef` | 폴더 이동(git mv, `.meta` 짝) | `Assets/_Project/Core/`(`Devman.Core` asmdef · `AssemblyInfo.cs`) · `Assets/_Project/Tests/EditMode/`(`Defense.Tests.EditMode` asmdef · `GateSmokeTests.cs`) · `Assets/_Project.meta` | `Assets/10_Scripts/Core/` · `Assets/10_Scripts/Tests/EditMode/`. `_Project.meta` 삭제. 사용자가 Finder 로 만든 빈 폴더 「00. Scenes · 10. Scripts · 20. Animations · 30. Assets」 → `00_Scenes` · `10_Scripts` · `20_Animations` · `30_Assets`(공백 · 점 제거 — D-09). 게이트 실행(batchmode)이 만든 폴더 `.meta` 넷을 커밋 — Unity 는 `.meta` 가 있으면 빈 폴더를 다시 만든다 |
| `1f87826` | `CLAUDE.md` 「코딩」 폴더 줄 | `Assets/_Project/…` 일곱 폴더 · `Assets/ThirdParty/` | 번호 폴더 네 개와 하위 배치(`10_Scripts/Core · Defense · Editor · Tests`, `30_Assets/Models · Prefabs · Textures · Materials · Audio · Data · ThirdParty`) · `Settings/` 그대로 · 공백 · 점 금지 · `Tools/Blender/` GPL. 정본 `_템플릿/Unity_CLAUDE.md` 도 같이(Planning) |
| 같음 | `README.md` | 「구조」 블록 `Assets/_Project/…` · 「직접 빌드하기」 `Assets/_Project/Scenes/Main.unity` · 「라이선스」 MIT 한 줄 | 번호 폴더 블록(+ `Tools/Blender/` 줄) · `Assets/00_Scenes/Main.unity` · `Tools/Blender/` 만 GPL-3.0-or-later 한 문장 |
| 같음 | `z.docs/dev/csharp_coding_standards.md` | §8 3번 `Assets/_Project/Data/<영역>/` · §17 기준 폴더 `Assets/_Project/` · §17 표의 `Data/` · `Defense/` · `Scenes/` · `Art/` · `Audio/` | `Assets/30_Assets/Data/<영역>/` · 기준 폴더는 번호 폴더 넷 · 표를 `30_Assets/Data` · `30_Assets/Prefabs` · `00_Scenes` · `30_Assets/Models`(머티리얼 · 텍스처는 모델 옆, 공용은 `Materials/` · `Textures/`) · `20_Animations` · `30_Assets/Audio` 로. 정본 `_템플릿/Unity_csharp_coding_standards.md` 도 같이 |
| 같음 | `Tools/Blender/`(신설) | 없음 | `make_enemies.py`(P03 사전 확인판 + SPDX 머리 두 줄 — 슬라임 · 박쥐 · 등불 탑 · 팔레트 · FBX 두 변형 · 렌더 · JSON. 알려진 결함 둘: 박쥐 날개 팔레트 셀 · 날갯짓 회전축 — P03 실행 세션이 고친다) · `LICENSE`(GNU GPL v3 원문, gnu.org, SHA-256 `3972dc97…b36986`) · `README.md`(실행 명령 · 라이선스 경계) |
| 같음 | `ASSETS.md` | 파일 4행 | + `Tools/Blender/make_enemies.py`(AI 생성 · GPL-3.0-or-later) · `Tools/Blender/LICENSE`(원문) 두 행 · summary |

건드리지 않은 것: 템플릿 잔재 `Assets/Scenes/SampleScene.unity`(`EditorBuildSettings` 가 이 경로를 가리킨다) · `TutorialInfo/` · `Readme.asset` · `Settings/` · `ProjectSettings/` · `Packages/` · 활성 플랫폼. 정리는 P01 NOTES 4번 `DEF-M01`.

## 2. 검증

- 이동 뒤 게이트(기본 정보 ①) — asmdef 참조는 이름으로 묶여 경로 이동에 영향이 없음을 테스트 2/2 로 확인.
- `git show --stat 7f88bef` 가 이동 11 · 삭제 1 · 추가 4(`.meta`)이고 내용 변경(`M`)이 없다. git 은 `_Project.meta → 00_Scenes.meta` 를 내용이 비슷한 이름 바꿈으로 보였는데(폴더 `.meta` 는 GUID 만 다르다) 실제로는 삭제 + 새 파일이다 — 의미 없음.
- 번호 폴더가 `Assets/` 안에서 어떻게 보이는지는 에디터 화면으로 보지 않았다(창 없는 실행만).

## 3. 코드 리뷰

세션 센서를 띄우지 않았다 — 사용자 승인 「승인 — 센서는 생략」(2026-10-31). 사유: C# 변경이 없고(이동만, 게이트가 증명) `Tools/Blender/make_enemies.py` 는 P03 실행 세션 둘이 고치는 시작판이라 그때의 세션 센서가 본다. 위험 등급 저 — 사람이 볼 것은 기본 정보의 증거 3행.

## 4. 확인하지 않은 것 · 추정

- `bake_space_transform` 켠 FBX(`*_bst.fbx`)가 Unity 에서 루트 회전을 없애는지 — 스크립트가 두 변형을 내지만 Unity 로 가져오지 않았다(P03 실행 세션의 함정 실측 항목).
- GPL 경계 — 「Blender Python API 를 쓰는 스크립트를 공개하면 GPL」은 Blender FAQ 에 기대는 `_공통` G-17 의 판단이고, 이 세션이 법적으로 다시 확인하지 않았다. 생성물(FBX · PNG)이 영향을 받지 않는다는 것도 같은 출처.
- `Assets/Settings/` 를 번호 폴더 밖에 둔 것은 템플릿 생성물이라서다 — 뒤에 `40_Settings` 로 옮길지는 `DEF-M01` 정리 때.
