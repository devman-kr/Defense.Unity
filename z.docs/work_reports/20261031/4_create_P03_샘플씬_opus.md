---
id: 20261031-p03-s2-sample-scene-opus
title: P03 S2(Opus 5.5) — Blender CLI 결함 수정 · Unity CLI 샘플 씬 · 캡처 2 · 48프레임 MP4
summary: make_enemies.py 의 날개 UV 층(Float2) 결함을 고치고 날갯짓은 대칭(날개 끝 0.444/0.444 m)임을 실측해 렌더 시점만 바꿨으며, _bst FBX 로 P03_CLI_Sample.unity 를 세워 CLI 캡처 2장 · 48프레임 MP4 를 냈다.
doc_type: work_report
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P03
work_type: create
commits: [a5a2a5c, c70f6dc, 3a1dd18]
implements: [20261031-p03-handoff-s1s2-exec]
---

# P03 S2(Opus 5.5) — Blender CLI 결함 수정 · Unity CLI 샘플 씬 · 캡처 2 · 48프레임 MP4 — Defense.Unity

## 기본 정보

| 항목 | 내용 |
|------|------|
| 작업일 | 2026-10-31 22:07~(KST) · Mac M5 16 GB · Claude Opus 5.5(세션 `3aa03a8d`) · effort high |
| 근거 | Planning P03 지시서 `20261031-p03-handoff-s1s2-exec`(S1 · S2 공통 본문) — 세션 S2 · worktree `z.tmp/wt/p03-opus` · 브랜치 `p03/exec-opus` · 시작 `763866b` |
| 위험 등급 | 저 — 에디터 전용 스크립트(asmdef `Defense.Editor`, `includePlatforms: Editor`) · 시험 씬 · 샘플 에셋 · `Tools/Blender/make_enemies.py`. `ProjectSettings/` · `Packages/` · 저장 · 재화 변경 없음 |
| 증거 ① 게이트 | 커밋마다 두 번(`c70f6dc` 전 · `3a1dd18` 전, 같은 결과). 열린 에디터: `U script recompile` → `recompile_status` `up_to_date` · `compilationFailed: false` → `U console status` `consoleErrors: 0` · `consoleWarnings: 0` → `U test run --mode editor` `Total 2 · Passed 2 · Failed 0`. 닫힌 뒤: `unity test . --mode EditMode --output Logs/editmode-results.xml --timeout 900 -- -logFile Logs/gate-editmode.log` → 종료 0 · 6.6초(`3a1dd18` 전) · `result="Passed" total="2" passed="2" failed="0"` · `grep -cE 'warning CS\|error CS' Logs/gate-editmode.log` 0(스크립트가 열린 에디터에서 이미 컴파일돼 이 실행에는 컴파일 줄이 없다 — 경고 근거는 열린 에디터의 `consoleWarnings: 0`) |
| 증거 ② 상태 | `git status --short` → 빈 출력(이 보고서를 담은 커밋 뒤). 커밋 전에는 이 보고서 · `코드리뷰_opus/` 만 미추적이었고 `ProjectSettings/` · `Packages/` · `Library/` · `Temp/` · `Logs/` 변경 없음 |
| 증거 ③ 캡처 | `z.captures/20261031/opus/` — `capture_portrait_1080x1920.png` · `capture_landscape_1920x1080.png` · `sample_48f_24fps.mp4`(48프레임) · `frames/` · `renders/` · README 대응표. 확인 페이지 https://claude.ai/artifact/NFzcAeFWes4766BELBTYb6(비공개). 확인 안 한 것은 6절 |

## 1. 바꾼 것

| 커밋 | 무엇 | 전 | 후 |
|---|---|---|---|
| `a5a2a5c` | `Tools/Blender/make_enemies.py` — 날개 색 | `set_cell` 이 UV 층 없는 `from_pydata` 메시(날개)에 bmesh `uv.verify()` 로 층을 만들면 이름이 `Float2` 라 `join` 때 몸의 `UVMap` 과 합쳐지지 않고, 합친 `UVMap` 의 날개 루프가 (0,0) = 셀 0(슬라임 초록) | UV 층이 없으면 `uv_layers.new(name="UVMap")` 을 먼저 — 날개 루프 UV (0.625, 0.375) = 셀 6(어두운 보라). 합친 메시의 UV 층은 `UVMap` 하나 |
| 같음 | 같은 파일 — 날갯짓 | 박쥐 자세 렌더가 앞 오른쪽 위 사선 카메라(+2.2, −2.6, +1.7) — 대칭 날갯짓이 원근 때문에 한쪽만 오르내리는 것처럼 보였다(3절) | 박쥐 자세 렌더만 정면 카메라(0, −2.6, +0.7). 동작(회전 축 · 각도)은 바꾸지 않음. `extent(..., tips=True)` 로 날개 끝 높이 `tip_z_l` · `tip_z_r` 를 `P03_RESULT` 에 남김 |
| `c70f6dc` | `Assets/30_Assets/Models/P03_CLI/` | 없음 | FBX 여섯(`slime` · `bat` · `tower` 와 `_bst`) · `palette.png`(Point · 밉맵 끔 · 비압축 · Clamp) · `palette.mat`(URP Lit, `_BaseMap` = 팔레트, 모델 셋이 공유) · `island.mat`(URP Lit 단색 — 원판 프리미티브용, `3a1dd18` 에서 지움) |
| 같음 | `Assets/10_Scripts/Editor/`(asmdef `Defense.Editor` 신설) · `Editor/P03/` | 없음 | `P03ModelImport`(텍스처 · 공유 머티리얼 · FBX 머티리얼 `Palette` → `palette.mat` 연결 · `clipAnimations` 벗긴 이름 · Idle/Hop/Flap `loopTime`) · `P03SampleSceneBuilder`(씬을 처음부터 만든다) · `P03FrameRecorder`(클립 샘플링 → 카메라 → RenderTexture → PNG) |
| 같음 | `Assets/00_Scenes/P03_CLI_Sample.unity` | 없음 | 빌더로 만든 씬(손 YAML 편집 없음) — 2절 |
| `3a1dd18` | 섬 머티리얼(센서 지적 `P03.S2.UNI.02`) | 섬만 `island.mat`(단색) — 씬 머티리얼 둘 | 실린더 프리미티브 메시를 복사해 UV 를 전부 팔레트 셀 12 중심 (0.125, 0.875)으로 모은 `island.asset` + `palette.mat`. `island.mat` 삭제 — 씬 렌더러의 머티리얼 집합이 `palette.mat` 하나(스크립트로 확인). 빌더 주석 · 요약 바로잡음(`UNI.12`). 씬을 다시 만들고 캡처 2 · 프레임 48 · MP4 를 이 판으로 다시 냈다 |

## 2. 성공 기준 대조

| # | 기준 | 결과 | 근거 |
|---|---|---|---|
| 1 | Blender 다듬기 | 됨 | 종료 0 · 3.6초. 렌더를 열어 봄: 날개 셀 6 · f01 두 날개 위 · f06 두 날개 아래. 슬라임 렌더 4장은 이전(`z.captures/20261031/render_*.png`)과 픽셀 최대 차이 0, 라인업은 다른 픽셀 2,496개가 전부 날개 영역(x 331~559 · y 195~248) — 탑 · 슬라임 그대로. `P03_RESULT` 는 아래 |
| 2 | 가져오기 | 됨 | 7파일이 `Assets/30_Assets/Models/P03_CLI/`. `ModelImporter.clipAnimations` 를 코드로 — 스크립트 출력 `slime_bst: Die · Hit · Hop(loop) · Idle(loop)` · `bat_bst: Flap(loop) · Hit`, 서브 에셋 클립 이름도 같음(`isLooping` 일치). `asset get_import_settings` 는 모델의 클립 항목을 돌려주지 않아(텍스처 항목만 확인 — `filterMode Point` · `mipmapEnabled False` · `Uncompressed`) 스크립트 출력으로 확인 |
| 3 | 함정 둘 실측값 표 | 됨 | 3절 |
| 4 | 샘플 씬 | 됨 | `scene get_hierarchy` — 트랜스폼 72: `P03Sample` 아래 `Island` · `Tower` · `Slimes`(`Slime01`~`08`, 반지름 3.4 / 4.6 m 번갈아 · 탑을 봄) · `Bats`(`Bat01`~`04`, 반지름 2.4 m · 높이 1.35 / 1.65 m · 접선 방향) · `Sun`(방향광) · `LampLight`(점광, y 2.0 m) · `PortraitCamera`(원근 FOV 60° · 피치 58° · 거리 19 m). 나머지 59는 모델 자식(메시 · 뼈대 · 뼈). 머티리얼: 모델 셋과 섬이 `palette.mat` 하나(`3a1dd18` — 그 전 `c70f6dc` 판은 섬만 `island.mat` 이었다) |
| 5 | 정지 캡처 2 | 됨(CLI) | `capture game_view --source camera` 가 창 없는 에디터에서 그림을 냈다 — 대체 경로 쓰지 않음. 열어 봄: 섬 · 탑 · 슬라임 8 · 박쥐 4. 저장 경로는 4절 「발견」 1 |
| 6 | 녹화 | 됨 | 48프레임 720×1280 → Blender VSE(`FFMPEG` · `MPEG4` · `H264`, 뷰 변환 `Standard`) → `sample_48f_24fps.mp4` 179,917 B(`3a1dd18` 판). Blender 로 다시 읽어 `movieclip.frame_duration` 48 · `movie strip frame_final_duration` 48 · 24 fps(두 판 모두). 첫 판에서 12번째 프레임을 뽑아 원본 PNG 와 비교 — 평균 RGB 차 (−0.52, 0.08, 0.06) |
| 7 | 게이트 · 보고 · 센서 · 커밋 | 됨 | 기본 정보 증거 ① · 커밋 `a5a2a5c`(Blender) · `c70f6dc`(Unity) · `3a1dd18`(센서 지적 수정) · 이 보고서와 센서 기록을 담은 커밋 · 8절 |
| 8 | 확인 페이지 | 됨 | https://claude.ai/artifact/NFzcAeFWes4766BELBTYb6 — 렌더 · 캡처 2 · 동영상 · 프레임 3 · 함정 표 · 명령과 시간 |

`P03_RESULT`(고친 스크립트, `z.tmp/p03_out_opus`):

```
{"blender": "5.2.3 LTS", "steps": {"slime_model": 0.01, "bat_model": 0.01, "tower_model": 0.0, "animation": 0.0, "export": 0.06, "render": 3.0, "reimport": 0.05}, "models": {"slime": {"mesh": {"verts": 250, "faces": 288}, "bones": ["SlimeRoot", "SlimeTop"]}, "bat": {"mesh": {"verts": 214, "faces": 234}, "bones": ["BatRoot", "WingL", "WingR"], "groups": {"BatRoot": 190, "WingR": 12, "WingL": 12}}, "tower": {"mesh": {"verts": 109, "faces": 87}}}, "actions": {"Die": [1.0, 18.0], "Flap": [1.0, 12.0], "Hit": [1.0, 12.0], "Hit.001": [1.0, 10.0], "Hop": [1.0, 24.0], "Idle": [1.0, 24.0]}, "deform": {"slime_hop_f1": {"max_z": 0.825, "min_z": 0.025, "max_x": 0.5}, "slime_hop_f13": {"max_z": 1.375, "min_z": 0.575, "max_x": 0.5}, "slime_die_f18": {"max_z": 0.049, "min_z": 0.001, "max_x": 0.85}, "bat_flap_f1": {"max_z": 0.467, "min_z": -0.19, "max_x": 0.671, "tip_z_l": 0.444, "tip_z_r": 0.444}, "bat_flap_f6": {"max_z": 0.287, "min_z": -0.309, "max_x": 0.745, "tip_z_l": -0.278, "tip_z_r": -0.278}}, "fbx_bytes": {"slime.fbx": 115580, "slime_bst.fbx": 115564, "bat.fbx": 82876, "bat_bst.fbx": 82844, "tower.fbx": 21180, "tower_bst.fbx": 21132}, "renders_sec": {"lineup.png": 0.32, "render_slime_Idle_f12.png": 0.32, "render_slime_Hop_f08.png": 0.42, "render_slime_Hit_f03.png": 0.54, "render_slime_Die_f18.png": 0.53, "render_bat_Flap_f01.png": 0.44, "render_bat_Flap_f06.png": 0.43}, "reimport_slime": {"meshes": 1, "armatures": 1, "bones": ["SlimeRoot", "SlimeTop"], "actions": {"SlimeRig|SlimeRig|Die": [1.0, 18.0], "SlimeRig|SlimeRig|Hit": [1.0, 12.0], "SlimeRig|SlimeRig|Hop": [1.0, 24.0], "SlimeRig|SlimeRig|Idle": [1.0, 24.0]}}, "total_sec": 3.17}
```

(원문 파일 `z.captures/20261031/opus/renders/p03_result.json.txt`.)

## 3. 함정 둘 실측값

**(a) 가져온 직후 클립 이름**(벗기기 전 — Unity `ModelImporter.importedTakeInfos` · `defaultClipAnimations` · 서브 에셋 이름이 모두 같다)

| 모델 | 가져온 직후 | 벗긴 뒤(`clipAnimations`) |
|---|---|---|
| `slime` · `slime_bst` | `SlimeRig\|Die` · `SlimeRig\|Hit` · `SlimeRig\|Hop` · `SlimeRig\|Idle` | `Die` · `Hit` · `Hop`(loop) · `Idle`(loop) |
| `bat` · `bat_bst` | `BatRig\|Flap` · `BatRig\|Hit.001` | `Flap`(loop) · `Hit` |
| `tower` · `tower_bst` | 테이크 없음 | — |
| 참고 — Blender 가 자기 FBX 를 다시 읽음 | `SlimeRig\|SlimeRig\|Idle` 꼴(`P03_RESULT.reimport_slime`) | — |

`Hit.001`: Blender 동작은 파일 전체에서 이름이 하나라, 슬라임 `Hit` 뒤에 만든 박쥐 `Hit` 이 `Hit.001` 로 내보내졌다(`P03_RESULT.actions`). `P03ModelImport.StripClipName` 이 `|` 앞과 숫자 꼬리 `.NNN` 을 함께 벗긴다.

**(b) 두 변형의 루트 오브젝트 회전 · 스케일 · 뼈 수**(가져온 모델 에셋의 `localEulerAngles` · `localScale`, 뼈 수는 `SkinnedMeshRenderer.bones.Length` = 뼈대 자식 수)

| 모델 | FBX 루트 | 메시 오브젝트 | 뼈대 오브젝트 | 스케일(전부) | 뼈 |
|---|---|---|---|---|---|
| `slime` | 0° | `Slime` X 270° | `SlimeRig` X 270° | 1 | 2 |
| `slime_bst` | 0° | `Slime` 0° | `SlimeRig` X 270° | 1 | 2 |
| `bat` | 0° | `Bat` X 270° | `BatRig` X 270° | 1 | 3 |
| `bat_bst` | 0° | `Bat` 0° | `BatRig` X 270° | 1 | 3 |
| `tower` | X 270.0198°(루트가 메시) | — | — | 1 | 0 |
| `tower_bst` | 0°(루트가 메시) | — | — | 1 | 0 |

임포터는 여섯 모두 `animationType Generic` · `useFileScale True` · `fileScale 1` · `globalScale 1` · `bakeAxisConversion False`. `bake_space_transform` 은 메시 · 정적 모델의 X 270° 만 없애고 **뼈대 오브젝트의 X 270° 는 두 변형 모두 남는다.** 두 변형의 굽힘 결과는 같고 Blender 값과 일치한다 — `SampleAnimation` + `BakeMesh` 월드 좌표로 박쥐 Flap 날개 끝 y f1 0.444 / 0.444 · f6 −0.278 / −0.278, 슬라임 Hop f13 최고 1.375 · 최저 0.575(Blender `deform` 과 같은 값).

**(c) 씬에 쓴 변형**: 셋 다 `_bst`. 기본 변형의 탑은 축 보정 X 270.0198° 가 루트 자체에 있어 씬에서 루트 회전을 정하면 보정이 덮여 탑이 눕고, 0.0198° 어긋남도 남는다. 슬라임 · 박쥐는 루트가 0° 라 어느 변형이든 루트를 돌려도 되고 굽힘도 같지만, 세 모델을 「루트 · 메시 0°」 한 규칙으로 맞추려고 `_bst` 로 통일했다.

## 4. 걸린 시간

벽시계(명령 앞뒤 `time.time()`), 2026-10-31 · Mac M5 16 GB · Blender 5.2.3 · Unity 6000.3.27f1 · Unity CLI 1.0.0-beta.13. 키체인 대화상자는 뜨지 않았다.

| 단계 | 명령 | 초 |
|---|---|---|
| Blender 실행(고친 스크립트) | `Blender -b --factory-startup -noaudio --python-exit-code 1 --python Tools/Blender/make_enemies.py -- z.tmp/p03_out_opus` | 3.6(스크립트 안 3.17 — 렌더 3.0) |
| 참고 — 원본 스크립트 기준 실행 | 같은 명령, 스크래치패드 출력 | 4.6 |
| 에디터 기동 | `Unity -batchmode -projectPath <worktree> -logFile Logs/editor-headless.log` → `U editor status` ready. 클론한 `Library/` 로 바로 열림(지우기 없음) | 12.3 |
| 가져오기 | `U asset import --source <z.tmp/p03_out_opus/…> --path 30_Assets/Models/P03_CLI/…` × 7 | 0.5 |
| 가져오기 설정 | `U script eval --code 'return Defense.Editor.P03.P03ModelImport.Apply();'` | 0.7 |
| 씬 | `U script eval --code 'return Defense.Editor.P03.P03SampleSceneBuilder.Build();'`(카메라를 고친 둘째 판) | 0.6 |
| 캡처 | `U capture game_view --source camera --camera PortraitCamera --width 1080 --height 1920 --save_path Temp/P03/…`(가로는 1920×1080) | 0.3 + 0.2 |
| 프레임 | `U script eval --code 'return Defense.Editor.P03.P03FrameRecorder.Record("<z.captures/…/frames>", 48, 24, 720, 1280);'` | 2.0 |
| 에디터 끄기 | `U script eval --code '…EditorApplication.Exit(0)…'` → `pgrep` 없음 | 1.1 |
| 닫힘 게이트 | `unity test . --mode EditMode …` | 7.5 |
| 인코딩 | `Blender -b … --python encode_mp4.py -- frames sample_48f_24fps.mp4 24`(인코딩 0.38 · 다시 읽기 0.02) | 1.2 |
| 다시 — 에디터 기동(둘째) | 같은 명령, `Library/` 가 이미 데워진 상태 | 6.2 |
| 다시 — 씬 · 캡처 · 프레임 | `Build()` · 캡처 2 · `Record(…)` (`3a1dd18` 판) | 0.5 · 0.3 + 0.2 · 1.6 |
| 다시 — 끄기 · 닫힘 게이트 · 인코딩 | 위와 같은 명령 | 1.2 · 6.6 · 1.2 |

세션 전체: 첫 명령 22:07:45 → 커밋 ② 22:23:49(16분) → 센서 1라운드 12분 26초(에이전트 실측) → 수정 커밋 `3a1dd18`. 단계 시간의 합보다 세션이 긴 것은 진단(날개 UV · 투영 계산) · 실측 스크립트 · 화면 확인 · 문서 때문이다.

## 5. 지시서와 다른 점

- **날갯짓 회전축 결함은 재현되지 않았다** — 회전 계산은 고치지 않고 박쥐 렌더 시점만 바꿨다. 근거: 기준 `.blend` 를 다시 열어 Flap 을 평가하면 두 날개 끝이 f1 z +0.467(몸 기준) · f6 −0.278 로 대칭이고 뼈 회전은 `WingL` · `WingR` 모두 로컬 X(= 월드 Y 축, 몸 앞뒤) 0.6 → −0.5 rad. 이전 렌더의 날개 끝 화면 좌표(왼쪽 날개 위로 가파름 · 오른쪽 거의 수평)는 이 대칭 위치를 사선 카메라로 투영한 값과 픽셀 단위로 맞았다(f01 오른쪽 끝 계산 (337, 269) · 그림 (355, 272), 왼쪽 계산 (151, 215) · 그림 (150, 215)). Unity 에서도 같은 값(3절 (b)).
- `P03_RESULT.deform` 의 박쥐 두 행에 `tip_z_l` · `tip_z_r` 두 키를 더했다(성공 기준 1 의 「위아래로」를 숫자로 확인하려고).
- 캡처 저장: 지시서는 「안 되면 `Temp/` 에 찍어 옮긴다」였으나 CLI 는 `Temp/P03/…` 도 `Assets/Temp/P03/…` 로 저장한다(4절 발견 1). 그곳에 찍어 SHA-256 대조 뒤 옮기고 `AssetDatabase.DeleteAsset("Assets/Temp")` 로 지웠다 — 커밋에 없음.
- 머티리얼: 첫 판(`c70f6dc`)은 섬에 `island.mat`(단색)을 따로 뒀다가, 세션 센서가 성공 기준 4 와 다르다고 짚어 `3a1dd18` 에서 섬도 `palette.mat` 을 쓰게 고쳤다(잔디 셀 UV 메시 `island.asset` 추가 — 수정 허용 영역 `Models/P03_CLI/**` 안).
- 파일 이름은 지시서대로 소문자(`slime.fbx` · `palette.png`)이고, 같은 폴더에 세션이 만든 에셋도 맞춰 `palette.mat` · `island.asset` — 코딩 표준 §17(PascalCase)과 다르다(센서 `UNI.10`, 처리 안 함).
- 「슬라임은 Idle · Hop 을 섞고」는 슬라임마다 번갈아(짝수 Idle · 홀수 Hop) 맡기고 위상을 `클립 길이 × i / 8` 로 어긋나게 했다. 박쥐 위상은 `Flap 길이 × i / 4`. 프레임 크기 720×1280 은 세션이 정했다.
- 리뷰어는 `Explore` 에이전트(Edit · Write 도구 없음)에 모델 `opus` 로 띄웠다 — 쓰기 권한을 주지 않으려고.
- 캡처 README(`z.captures/20261031/opus/README.md`)는 git 밖 캡처 폴더에 두고 저장소 커밋에는 넣지 않았다(같은 id 사본을 두 곳에 만들지 않으려고).
- 성공 기준 4 의 「프리미티브」: 섬은 실린더 프리미티브의 UV 를 바꾼 사본 메시(`island.asset`)다 — 「프리미티브」와 「머티리얼 하나 공유」를 함께 맞추려면 사본이 필요하다고 해석했다(센서 `UNI.25`). 내장 메시 그대로여야 한다면 섬에 머티리얼을 하나 더 두는 첫 판(`c70f6dc`)으로 돌아가야 한다.

## 6. 확인하지 않은 것 · 추정한 것

- 정지 캡처 2장은 에디터 상태 그대로라 슬라임 · 박쥐가 바인드 자세다. 플레이 모드에서는 Animator 컨트롤러가 없어 움직이지 않는다(플레이 모드는 범위 밖, 확인 안 함).
- MP4 는 Blender 로 다시 읽은 프레임 수 · 한 프레임 색만 봤다 — 브라우저 · QuickTime 재생은 확인 페이지에 올렸을 뿐 세션이 재생해 보지 않았다.
- 프레임 사이 박쥐 날갯짓은 영역별 픽셀 차로 확인했다(예: `Bat03` 영역 프레임 0 ↔ 5 에서 1,710 px). 위상 때문에 0 ↔ 5 차이가 작은 박쥐(`Bat02` · `Bat04` 10 · 11 px)는 계산상 두 시각 모두 날개가 중간 높이다. 샘플링 직후 렌더에 자세가 반영되는지는 따로 시험했다(위 → 아래 1,167 px · 다시 렌더 0 px).
- `tower.fbx` 루트가 270° 가 아니라 270.0198° 인 원인은 조사하지 않았다(FBX 쪽 값인지 Unity 변환 오차인지 모름).
- URP 에서 `Camera.Render()` 는 경고 없이 돌았다. Unity 6 가 권하는 `RenderPipeline.SubmitRenderRequest` 와 결과가 같은지는 비교하지 않았다.
- 콘솔: 마지막 녹화 전 `U console clear` 로 비웠다. 지운 오류는 세션이 낸 것 셋 — 없는 명령 `asset refresh` · 캡처 경로 거부 둘 · 녹화 반환 문자열이 다시 연 씬의 파괴된 `Transform` 을 읽은 예외(고친 뒤 `c70f6dc` 에 들어감). 게이트의 `consoleErrors 0` 은 비운 뒤 값이다.
- `EditorSceneManager.NewScene` 이 `ProjectSettings/SceneTemplateSettings.json`(추적 안 됨)을 만들었다 — 수정 금지 영역이라 에디터를 끈 뒤 지웠다. 세션 시작 때 `git status` 는 비어 있었다.
- 격리: 상위 날짜 폴더 `z.captures/20261031/` 를 `ls` 로 나열하다 다른 모델 폴더 이름(`sonnet`)이 출력에 나왔다 — 그 폴더는 열지 않았다. 같은 명령으로 날짜 폴더 `README.md` 머리 12줄을 읽었다. Planning 은 지시서 밖으로 `_템플릿/리뷰어_지시.md` · `코드리뷰_가이드.md`(2 · 3절과 리뷰어 지시에 넣을 고위험 경로 표 때문에 **5절도**, 처리 때 6 · 8절) · `_도구/센서기록_추출.py` 머리를 읽었다. `프롬프트_작성_가이드.md` 는 읽지 않았다 — 리뷰어 지시 템플릿 A 를 그대로 채웠다.

## 7. 발견 — `DEF-M03` 에 넘길 것

1. **CLI 캡처 경로**: `capture game_view --save_path` 는 `Assets/` 기준이다 — `Temp/x.png` · 프로젝트 안 절대 경로 모두 `Assets/Temp/…` 로 저장되고(Unity 가 `.meta` 를 만들고 가져온다), 프로젝트 밖 절대 경로는 400 `outside the project root`, `..` 는 `must not contain '..'`. 파이프라인은 「`Assets/Temp/` 에 찍기 → 옮기기 → `AssetDatabase.DeleteAsset`」 이나 에디터 스크립트 렌더(`P03FrameRecorder.RenderToPng`, 절대 경로 가능)로.
2. `capture game_view --source camera` 는 창 없는(`-batchmode`, `-nographics` 없음) 에디터에서 그림을 낸다 — 1080×1920 한 장 0.3초. 에디터 기동 12.3초 포함 Unity 쪽 전체가 30초 안쪽.
3. **Blender 동작 이름은 파일 전체에서 하나**라 모델이 같은 동작 이름을 쓰면 `.001` 이 붙어 나간다(`BatRig|Hit.001`). 모델별 `.blend` · 실행을 나누거나 가져오기 코드가 꼬리를 벗긴다(지금은 후자).
4. **`bake_space_transform` 은 뼈대 오브젝트의 X 270° 를 없애지 않는다.** 정적 모델은 `_bst` 가 루트 0° 라 필수에 가깝고(기본 변형 탑 루트 270.0198°), 스킨 모델은 굽힘이 같아 어느 쪽이든 된다. 기본안: 전부 `_bst` 하나만 내보내기.
5. 팔레트 텍스처는 Point · 밉맵 끔 · 비압축이어야 한다 — 4×4 팔레트는 밉맵 · 블록 압축(4×4 블록)이 셀 색을 섞는다.
6. Blender: `from_pydata` 로 만든 메시는 `join` 전에 `uv_layers.new(name="UVMap")` 이 필요하다(bmesh 가 만든 층은 `Float2`). VSE 로 MP4 를 낼 때는 `view_transform = 'Standard'` — 기본 AgX 는 PNG 색을 바꾼다(인코딩 스크립트에 반영, 비교는 6절).
7. `EditorSceneManager.NewScene` 이 `ProjectSettings/SceneTemplateSettings.json` 을 만든다 — 커밋할지 `.gitignore` 할지 정해야 한다.
8. 에디터 스크립트 이름 `*Builder*` 는 고위험 경로 패턴 `Assets/10_Scripts/Editor/*Build*` 에 걸릴 수 있다(지금은 `Editor/P03/` 아래라 한 단계 깊다). 패턴을 빌드 스크립트로 좁히거나 이름 규약을 정해야 한다.
9. 범위 밖이라 고치지 않은 것: `Tools/Blender/README.md` 표의 「알려진 결함 둘 … P03 실행 세션이 고친다」가 이제 낡았다 · `ASSETS.md` 에 `Assets/30_Assets/Models/P03_CLI/*`(저장소 스크립트가 만든 모델 — AI 생성 여부 표기) 행이 없다. 머지(S3) 때 둘 다 손볼 것.
10. 씬을 빌더로 처음부터 다시 만들면 fileID 가 모두 새로 매겨져, 섬 하나를 바꾼 `3a1dd18` 의 씬 diff 가 1,430줄이었다. 씬을 빌더로 관리한다면 「빌더 코드가 정본, 씬 diff 는 보지 않는다」로 정하거나, 기존 씬을 열어 바뀐 오브젝트만 고치는 빌더로.
11. 정적 모델 · 프리미티브에 팔레트를 쓰려면 UV 를 셀 중심으로 모은 메시가 필요하다(이번 섬은 `island.asset`) — 프리미티브용 머티리얼을 따로 만들지 않는 규칙이면 이 단계가 파이프라인에 들어간다.
12. 더 나은 방법(한 문장): 씬을 CLI `gameobject create` 를 수십 번 부르기보다 에디터 스크립트 빌더 하나를 `script eval` 로 부르는 편이 재현 · 리뷰가 쉬웠다(씬 0.5초).

## 8. 코드 리뷰

위험 등급: 저(지시서). 결정론 게이트: 컴파일 오류 0 · 경고 0(열린 에디터 `consoleWarnings 0`) · EditMode 2 → 2 통과, 실패 0(열림 · 닫힘 모두, `c70f6dc` 전 · `3a1dd18` 전).

| 저장소 | 범위 | 크기 | 구성 | 에이전트 · 새 토큰 · 시간 | 지적 | 고침 | 틀림 | 의도 | 범위 밖 | 처리 안 함 | 미해결 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity | `763866b..c70f6dc` | 14파일(코드 5 · 머티리얼 YAML 2 · 이진 7) +700/−3 | 세션 센서(A opus — `Explore`, 읽기 전용 도구) | 1 · 57.8만 · 12분 26초 | 22 | 3 | 0 | 0 | 2 | 17 | 0 |
| Unity 2라운드 | `c70f6dc..3a1dd18` | 3파일 +220/−157 | 세션 센서(A opus) | 1 · 37.8만 · 6분 28초 | 5 | 0 | 0 | 0 | 0 | 5 | 0 |

원본 · 처리표: `z.docs/work_reports/20261031/코드리뷰_opus/`(처리.md 27행) — 지시서대로 worktree 안(Planning 에 쓰지 않음).

사람이 볼 행(blocking · important 이면서 new, 미해결):

| 식별자 | 종류 · 심각도 · 기원 | 영역 | 유형 | 파일:줄 | 처리 | 근거 · 커밋 · 반론 |
|---|---|---|---|---|---|---|
| P03.S2.UNI.01 | issue · blocking · new(plausible) | CONTRACT | 모델 루트에 `SampleAnimation` — 클립에 루트 곡선이 있으면 녹화에서 배치를 덮어씀 | P03FrameRecorder.cs:44,49 | 처리 안 함(plausible) | 녹화 프레임 0 · 5 · 24 · MP4 12번째 프레임을 열어 12마리가 제자리 — 시나리오 미발생. `GetCurveBindings` 는 보지 않음 |
| P03.S2.UNI.02 | question · important · new | CONTRACT | 섬만 별도 `island.mat` — 성공 기준 4 「머티리얼 하나 공유」와 다름 | P03SampleSceneBuilder.cs:24,76-85 | 고침 | `3a1dd18` |
| P03.S2.UNI.03 | question · important · new(plausible) | CONTRACT | 「날갯짓 회전축」 결함을 동작이 아니라 렌더 시점 변경으로 처리 | make_enemies.py:407-410 | 처리 안 함(plausible) | 사용자 확인 — 5절 근거 · 확인 페이지의 이전 / 고침 렌더 |

판정 반론: 틀림 0 · 의도 0 → 생략. 범위 밖 2(`UNI.21` README 문구 · `UNI.22` `StripClipName` 테스트 — H 후보 하나). 처리 안 함 22 — plausible 7(1라운드 5 · 2라운드 2) · nit 10(1라운드 7 · 2라운드 3) · 상한 밖 5. 종료 조건 충족, 3라운드 없음.
