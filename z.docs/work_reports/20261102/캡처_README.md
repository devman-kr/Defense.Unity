---
id: 20261102-p04-captures-s1-unity-copy
title: 캡처 README 사본 — P04 S1 · S2(Opus 5.5) 매끈 판 · 셰이딩 비교 · 인간형
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

## S2 — 해골 궁수 · 좀비 매끈 인간형(세션 99a2b19d, 2026-11-02)

같은 브랜치 `p04/smooth`, 커밋 `ff8be0d`(Blender) · `4130cd2`(Unity). 셰이딩은 사용자 선택이 아직 없어 지시서 규칙대로 Lit(`PaletteLit`)만 찍었다. 렌더 · 캡처 · 프레임 · FBX 비교판은 커밋하지 않는다.

| 원본 | 내용 | git 사본 |
|---|---|---|
| `P04S2_Portrait_Lit.png` | 비교 씬 세로 1080×1920 — 매끈 판 루트(`P04Smooth` — 섬 · 탑 · 슬라임 8 · 박쥐 4)에 인간형 루트(`P04Humanoids`)를 켜서: 해골 궁수 4(슬라임 고리 바깥 반지름 5.6, 탑을 보고 Shoot 프레임 16 기준 위상 엇갈림) · 좀비 4(두 고리 사이 반지름 4.0, Walk). 궁수는 흰 해골 · 활이 위에서도 읽히고, 좀비는 초록 머리 · 보라 셔츠로 슬라임과 구별된다 | 없음 |
| `P04S2_Lineup5_Lit.png` | 라인업 1920×1080 — 매끈 판 다섯(슬라임 · 박쥐 · 등불 탑 · 해골 궁수 · 좀비, 인간형은 Idle 프레임 1). 키는 탑 아래 · 슬라임의 약 2배 | 없음 |
| `P04S2_Close_ArcherFaceBow_Lit.png` | 궁수 근접 1080×1080(오른쪽 앞) — Shoot 프레임 16(당김). 큰 머리 · 점 눈 둘 · 갈비 셋 · 활대(나무 셀) · 시위(흰 셀). 활이 왼손에 붙어 세로로 서고 오른손이 시위 쪽에 있다. 시위는 굽지 않는다(활 전체가 왼손 뼈 100 %) | 없음 |
| `P04S2_Close_ArcherShoot24_Lit.png` | 같은 카메라, Shoot 프레임 24(놓은 뒤) — 오른팔꿈치 최대 접힘 111.3° 자세. 팔꿈치 안쪽이 얇아지나(두께 비 0.663) 꺾여 찢어지거나 뒤집히지 않는다 | 없음 |
| `P04S2_Close_ZombieWalk25_Lit.png` | 좀비 옆 근접 1080×1080 — Walk 프레임 25, 왼무릎 최대 접힘 72.2°. 무릎이 둥글게 굽고(두께 비 0.856) 바짓단 경계가 셀 색 그대로. 앞에 이웃 슬라임이 걸린다 | 없음 |
| `frames_s2_smooth_lit/frame_000.png` ~ `frame_047.png` | 매끈 판 + 인간형 · Lit 48프레임 720×1280(24 fps 2초 — 슬라임 Idle · Hop · 박쥐 Flap · 궁수 Shoot · 좀비 Walk, 개체마다 위상이 다름). `P04HumanoidCapture.Record`(스킨 메시 24개 `forceMatrixRecalculationPerRender` 켬) | 없음 |
| `S2_SmoothLit_Humanoids_48f_24fps.mp4` | 위 프레임을 Blender 5.2.3 내장 ffmpeg(VSE · H264 · `view_transform = 'Standard'`)로 묶은 것 327,711 B. 다시 읽어 48프레임 · 24 fps · 720×1280 | 없음 |
| `renders_s2/render_lineup.png` | Blender EEVEE 1280×600 — 저장소 S1 매끈 판 FBX 셋(가져옴) + 해골 궁수 · 좀비(쉼 자세) | 없음 |
| `renders_s2/render_archer_Walk_f07.png` · `render_zombie_Walk.001_f09.png` · `render_archer_Shoot_f10.png` · `render_archer_Shoot_f16.png` · `render_zombie_Attack_f10.png` · `render_zombie_Attack_f13.png` | 동작 자세 540×540 — Walk 2 · Shoot 2(시위 걸기 · 당김) · Attack 2(머리 위 · 내리침). 관절이 찌그러지지 않고 활이 손을 따라간다 | 없음 |
| `renders_s2/render_close_archer_draw.png` | 궁수 근접 640×640 — Shoot 16 | 없음 |
| `renders_s2/p04_result.json` | `make_humanoids_smooth.py` 표준 출력 `P04_RESULT` — 뼈 · 가중치(자동 · 메운 섬) · 관절 접힘 각과 두께 · Walk 첫 · 끝 차이 · 활 추종 · 단계별 삼각형 · FBX 크기 · 다시 읽기(정본 · naive) | 없음 |
| `renders_s2/naive/SkeletonArcher.fbx` · `Zombie.fbx` | 모델 표지 키 없이 그대로 내보낸 비교판 — 각 FBX 에 두 모델의 동작 6개가 다 들어 있다(클립 이름 함정의 증거) | 없음 |
| `renders_s2/stale_skinning/P04S2_Close_ArcherFaceBow_Lit.png` · `P04S2_Close_ArcherShoot24_Lit.png` | 캡처 코드에 스킨 플래그를 켜기 전의 두 장 — 자세를 바꿔 찍었는데 MD5 `0c979002…` 로 바이트까지 같다(P03 발견 ④ 재현) | 없음 |
