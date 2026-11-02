---
id: 20261102-p04-captures-s1-unity-copy
title: 캡처 README 사본 — P04 S1(Opus 5.5) 매끈 판 · 셰이딩 비교
summary: P04 S1 의 산출 대응표 — 셰이딩 3 × 판 2 세로 캡처 6장 · 라인업 · 슬라임 얼굴 근접, 매끈 판 Lit 48프레임 PNG 와 MP4(다시 읽어 48프레임), Blender 매끈 판 렌더 10장 · P04_RESULT JSON 이며 git 사본은 없다.
doc_type: register
status: final
lifecycle: living
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
# 2026-11-02 캡처 — P04 S1(Opus 5.5) 매끈 판 · 셰이딩 비교

원본은 작업 공간 `z.captures/20261102/p04/README.md`(git 밖) — 이 파일은 커밋용 사본이다.

세션 94cbfae6, 브랜치 `p04/smooth`(worktree `z.tmp/wt/p04`). 렌더 · 캡처 · 프레임은 커밋하지 않는다(지시서 「산출물」). 판정은 사용자가 한다 — 아래 「내용」은 세션이 열어 본 관찰이다.

| 원본 | 내용 | git 사본 |
|---|---|---|
| `P04_Smooth_Lit.png` · `P04_Low_Lit.png` | P04 비교 씬(`Assets/00_Scenes/P04_Smooth_Sample.unity`) 세로 1080×1920, P03 샘플 씬 구도(섬 · 탑 · 슬라임 8 · 박쥐 4), URP Lit(`PaletteLit`). 매끈 판은 슬라임 면이 보이지 않고 탑이 둥근 원기둥 · 부드러운 원뿔 지붕, 로우폴리 판은 슬라임 면 경계 · 8각 탑이 보인다. 둘 다 부드러운 방향 그림자 · 등불 점광 | 없음 |
| `P04_Smooth_Unlit.png` · `P04_Low_Unlit.png` | 같은 구도, URP Unlit(`PaletteUnlit`). 명암 · 그림자가 없어 두 판이 거의 같다 — 차이는 실루엣(8각 탑 · 슬라임 외곽의 꺾임)뿐 | 없음 |
| `P04_Smooth_Cel.png` · `P04_Low_Cel.png` | 같은 구도, 셀 셰이더(`PaletteCel` — 주 조명 3단 램프 · 그림자 받음, 점광 안 받음). 매끈 판은 밝은 단 · 어두운 단 두 덩어리로 깔끔히 갈리고, 로우폴리 판은 면마다 단이 달라 얼룩진다. 그림자 가장자리에 중간 단 고리가 보인다. 셀 셰이더에 SSAO 수신 · 그림자 페이드를 더한 `e84c677` 판으로 8장 모두 다시 찍었다(Lit · Unlit 은 같은 코드, 셀 2장만 바뀜 — 처음 판은 `z.tmp/p04_out/cel_before_fix/`) | 없음 |
| `P04_Lineup_Lit.png` | 라인업 1920×1080, Lit — 왼쪽 로우폴리 슬라임 · 박쥐 · 탑, 오른쪽 매끈 판 셋 | 없음 |
| `P04_Close_SlimeFace_Smooth_Lit.png` | 매끈 슬라임 얼굴 근접 1080×1080, Lit — 눈 흰자 · 눈동자 · 볼과 몸의 경계가 셀 색 그대로(번짐 없음), 면 경계 없음, SSAO 그늘이 눈 둘레에 | 없음 |
| `frames_smooth_lit/frame_000.png` ~ `frame_047.png` | 매끈 판 · Lit 48프레임 720×1280(24 fps 2초 — 슬라임 Idle · Hop 번갈아 위상 다르게, 박쥐 Flap). `P03FrameRecorder.Record`(씬 · 루트 · 모델 인자판, `forceMatrixRecalculationPerRender` 켬) | 없음 |
| `SmoothLit_48f_24fps.mp4` | 위 프레임을 Blender 5.2.3 내장 ffmpeg(VSE · H264 · `view_transform = 'Standard'`)로 묶은 것 216,338 B. 인코더는 `../../20261031/opus/encode_mp4.py` 를 그대로 썼고 다시 읽어 48프레임 · 24 fps · 720×1280 | 없음 |
| `renders/render_lineup.png` | Blender EEVEE 1280×540 — 왼쪽 로우폴리 3 · 오른쪽 매끈 3 | 없음 |
| `renders/render_smooth_*.png`(6) | 매끈 판 자세 — 슬라임 Idle 12 · Hop 8 · Hit 3 · Die 18, 박쥐 Flap 1 · 6. 480×480 | 없음 |
| `renders/render_close_slime_smooth.png` · `render_close_slime_low.png` · `render_close_tower_smooth.png` | 근접 — 슬라임 얼굴 매끈 · 로우폴리(같은 구도 640×640), 매끈 탑 540×720 | 없음 |
| `renders/p04_result.json` | `make_enemies_smooth.py` 표준 출력 `P04_RESULT` — 단계별 삼각형 · 정점 · UV 셀 중심 이탈, Hop 13 높이, FBX 크기 · 다시 읽기 | 없음 |
