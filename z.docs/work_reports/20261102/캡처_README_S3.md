---
id: 20261102-p04-captures-s3-toon-unity-copy
title: 캡처 README 사본 — P04 S3(Opus 5.5) 카툰 렌더링 승인 지점 ①~④
summary: P04 S3 의 산출 대응표 — 「전」 PaletteCel 과 「후」 PaletteToon 을 5종 세로 · 라인업 · 근접 2 × Mobile · PC 로 찍은 캡처, 외곽선 없음 · A · B 대조, 손잡이 13개 타일, 최종 조합 48프레임 MP4, 전역 영향 비교 캡처, 대조 시트 16장이며 git 사본은 없다.
doc_type: register
status: final
lifecycle: living
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
# 2026-11-02 캡처 — P04 S3(Opus 5.5) 카툰 렌더링

원본은 작업 공간 `z.captures/20261102/p04/toon/README.md`(git 밖) — 이 파일은 커밋용 사본이다.

세션 f7fb53cd, 브랜치 `p04/smooth`(worktree `z.tmp/wt/p04`, 시작 `489489c`). 캡처 · 프레임 · MP4 는 커밋하지 않는다(지시서 「산출물」). 모든 캡처는 비교 씬 `Assets/00_Scenes/P04_Smooth_Sample.unity` 를 에디터 스크립트 `P04ToonCapture`(카메라 → RenderTexture → PNG, 스킨 행렬 렌더마다 재계산)로 찍었고 품질 단계는 찍는 동안만 바꿨다. 파일 이름 `{기법}_{품질}_{장면}` — 장면은 `Portrait`(세로 1080×1920 — 매끈 판 슬라임 8 · 박쥐 4 · 탑 + 궁수 4 · 좀비 4) · `Lineup5`(1920×1080 — 매끈 판 다섯) · `CloseArcher` · `CloseSlime`(근접 1080×1080). 아래 「내용」은 세션이 열어 본 관찰이다 — 판정은 사용자.

| 원본 | 내용 | git 사본 |
|---|---|---|
| `before/Before_Cel_{Mobile,PC}_*.png`(8) | 「전」 — S1 셀 셰이더 `PaletteCel`. Mobile 은 그림자 가장자리 톱니 · 궁수 골반 자기 그림자 얼룩, PC 는 SSAO 가 슬라임 얼굴에 흐린 그늘 | 없음 |
| `outline/O0_ToonNoOutline_*` · `OA_Hull_*` · `OB_Edge_*`(각 8) | 승인 지점 ② — `PaletteToon`(양자화 · 채도 0.3) 외곽선 없음 · A 뒤집은 외피(2‰) · B 후처리 엣지(시험 렌더러 인덱스 1). B 는 가는 팔다리가 검게 뭉침. 사용자가 A 를 고름 | 없음 |
| `tiles/T00_Base_Mobile_*` · `T01`~`T13_*_Mobile_*`(Mobile 4장씩) | 승인 지점 ③ 손잡이 타일 — 기준 `PaletteToon.mat` 에서 한 값만 바꿈(단 수 · 경계 폭 · 그림자 색조 · 어두운 단 채도 · 양자화 · 블롭 · 자기 그림자 · 림 · 스페큘러 · 조명 고정 · 램프 · 톤매핑 · 외곽선 두께). `T11_Ramp` 는 머티리얼에 램프가 안 걸린 첫 판을 고친 뒤 다시 찍은 것 | 없음 |
| `final/After_Toon_{Mobile,PC}_*.png`(8) | 승인 지점 ④ 「후」 — 최종 `PaletteToon.mat` + 블롭 그림자(캐릭터 그림자 드리우기 끔) | 없음 |
| `frames_final_mobile/frame_000.png` ~ `frame_047.png` | 최종 조합 · Mobile 품질 48프레임 720×1280(24 fps — 슬라임 Idle · Hop · 박쥐 Flap · 궁수 Shoot · Walk · 좀비 Walk · Attack). 프레임 0 · 12 · 24 MD5 서로 다름 | 없음 |
| `Final_Toon_Mobile_48f_24fps.mp4` | 위 프레임을 Blender 5.2.3 내장 ffmpeg(VSE · H264 · `Standard`)로 묶은 것 493,492 B. 다시 읽어 48프레임 · 24 fps(인코더 스크립트는 세션 스크래치패드 — 저장소 밖) | 없음 |
| `Sheet0_Before_{Portrait,Close,Lineup}.png` | 대조 시트 0 — 「전」 Mobile · PC | 없음 |
| `Sheet2_Outline_{Portrait,CloseArcher,CloseSlime,Lineup5}.png` | 대조 시트 2 — 없음 · A · B × Mobile · PC | 없음 |
| `Sheet3_Tiles_{Lineup5,CloseSlime,CloseArcher,Portrait}.png` | 대조 시트 3 — 손잡이 한 줄 · 값 한 칸(Mobile) | 없음 |
| `Sheet4_Final_{Portrait,CloseArcher,CloseSlime,Lineup5,Frames}.png` | 대조 시트 4 — 전 · 후 × Mobile · PC, 녹화 프레임 0 · 12 · 24 · 36 | 없음 |
| `global/{before,before2,after,after2,reverted}/P03_q{0,1}.png` · `P04Lit_q{0,1}.png` · `global/diff_P04Lit_q0.png` | `Assets/Settings/` 변경 전역 영향 확인 — 기본 렌더러로 P03 씬 · S1 비교 씬(Lit) 540×960. before = before2(재현성), after 는 P03 둘 · P04 PC 가 바이트까지 같고 P04 Mobile 만 5픽셀 1/255 다름. RP 에셋을 원본으로 되돌린 `reverted` 도 after 와 같은 해시 — 목록 변경이 원인이 아님 | 없음 |
| `probe/` | 진단 — 씬을 Single 로 열기 전에 만든 머티리얼 사본이 자홍색으로 그려진 재현(`sheet2.png`), 후처리 켬 × RT 색공간 시험, 램프 수정 확인(`ramp.png`) | 없음 |
| `tiles_r2/` · `final_r2/` | 센서 수정(`a8f430b`) 뒤 에디터를 다시 띄워 같은 타일 · 최종 조합을 다시 찍은 것 — 근접 장면은 첫 판과 바이트가 같고 라인업 · 세로는 장마다 3~475 픽셀(최대 차 24~31/255) 다름. 라벨은 「③ 직전 기준」. 승인에 쓴 것은 첫 판 | 없음 |

톤매핑 줄(`T12_PostOnly` · `T12_Neutral`)은 근거가 되지 못한다 — 두 RP 에셋의 볼륨 프로필이 템플릿 `SampleSceneProfile`(톤매핑 Neutral · 비네트 · 블룸)이라 후처리를 켜기만 해도 그 효과가 걸리고, 첫 판의 Neutral 볼륨은 꺼진 채였다(작업 보고 8절 · 센서 `P04.S3.UNI.01` · `UNI.19`).
