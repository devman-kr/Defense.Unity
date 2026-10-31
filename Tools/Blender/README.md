---
id: 20261031-unity-tools-blender-readme
title: Tools/Blender — 창 없는 Blender 로 로우폴리 모델을 만드는 스크립트
summary: Defense.Unity 의 bpy 스크립트 폴더(GPL-3.0-or-later) — make_enemies.py 가 슬라임 · 박쥐 · 등불 탑을 팔레트 UV · 뼈대 · 동작과 함께 FBX 로 내보내고 미리보기를 렌더한다. 실행 명령과 라이선스 경계를 적는다.
doc_type: guide
status: final
lifecycle: living
created: 2026-10-31
updated: 2026-10-31
---
# Tools/Blender — 창 없는 Blender 로 로우폴리 모델을 만드는 스크립트

이 게임의 3D 모델 일부는 Blender 를 창 없이 띄워 `bpy` 스크립트로 만든다(`_공통` G-17 · Planning P03 D-09). 스크립트는 모델 · 팔레트 UV · 뼈대 · 동작 · FBX 내보내기 · 미리보기 렌더를 한 번에 하고, 결과를 숫자(JSON)와 그림으로 남긴다.

## 라이선스

**이 폴더만 GPL-3.0-or-later** 다(`LICENSE`). Blender Python API 를 쓰는 스크립트는 공개하면 GPL 이기 때문이다(Blender FAQ). 스크립트가 만든 모델(FBX · `.blend` · PNG)은 GPL 의 영향을 받지 않고 이 저장소의 다른 파일과 같은 조건(루트 `LICENSE` — MIT · `ASSETS.md`)이다.

## 실행

```
Blender -b --factory-startup -noaudio --python-exit-code 1 --python Tools/Blender/make_enemies.py -- <출력 폴더>
```

- `Blender` 는 `/Applications/Blender.app/Contents/MacOS/Blender`(Blender 5.2.3 LTS 에서 확인). 창 없이 5초 안팎, 메모리 0.4 GB.
- 결과 JSON 은 표준 출력의 `P03_RESULT` 줄. 출력 폴더에 `*.fbx`(모델마다 `bake_space_transform` 끔 · 켬 두 변형) · `palette.png` · `render_*.png` · `.blend`.
- Unity 에디터와 같이 띄우지 않는다(한 기기 G-01).

## 스크립트

| 파일 | 만드는 것 | 비고 |
|---|---|---|
| `make_enemies.py` | 슬라임(뼈 2 · Idle · Hop · Hit · Die) · 박쥐(뼈 3 · Flap · Hit) · 등불 탑(정적) · 팔레트 4×4 | Planning P03 의 시작판(2026-10-31) — 알려진 결함 둘(박쥐 날개 팔레트 셀 · 날갯짓 회전축)은 P03 실행 세션이 고친다 |
