---
id: 20261031-a4b45a08-final
title: 코드리뷰 — 에이전트 a4b45a08 최종 보고 원문
summary: 「P03 2라운드 센서(수정 커밋)」 에이전트(claude-opus-5-5 · 도구 호출 26)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P03
---
| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | question | nit | new | 덮어쓰기 경로가 더티 표시를 `CopySerialized` 에 맡김(검증되지 않은 경로) | Assets/10_Scripts/Editor/P03/P03SampleSceneBuilder.cs:104-113 | `island.asset` 이 이미 있는 상태에서 `Build()` 를 다시 돌린다(예: 셀 번호나 UV 규칙을 바꾼 뒤). 이때 `EditorUtility.CopySerialized` 가 대상에 더티를 걸지 않으면 112행 `SaveAssets` 가 디스크에 쓰지 않는다. 그러면 씬은 메모리 값으로 렌더되고, 커밋되는 `island.asset` 은 옛 값으로 남는다. 같은 폴더 `P03ModelImport.cs:74-77` 은 `SetDirty` 를 부른 뒤 `SaveAssets` 를 부른다. `island.asset` 은 이 커밋에서 처음 생겼으므로 이번 실행은 107-108행 생성 경로만 탔을 가능성이 크다. 보고에도 재실행 기록이 없다 | plausible |
| 2 | suggestion | nit | new | 팔레트 배치 지식의 세 번째 사본 | Assets/10_Scripts/Editor/P03/P03SampleSceneBuilder.cs:27-28,94-96 ↔ Tools/Blender/make_enemies.py:23-30,58 · P03ModelImport.cs:55 | Blender PAL 표의 셀 순서를 바꾸거나 팔레트를 8×8 로 키운다. 그러면 섬이 단언이나 오류 없이 다른 셀의 색으로 렌더된다. `PALETTE_SIZE` 는 `palette.png` 의 `width` 로 얻을 수 있다. 셀 번호 12 는 Blender 의 `C_GRASS` 와 손으로 맞춰야 한다. 1라운드 UNI.08 은 색 상수를 짚었고, 이 커밋에서 같은 이중 원본이 셀 번호 · 크기 상수로 형태만 바뀌었다 | confirmed |
| 3 | thought | nit | new | 성공 기준 4 「프리미티브」의 해석 | Assets/10_Scripts/Editor/P03/P03SampleSceneBuilder.cs:76,82-83 · Assets/00_Scenes/P03_CLI_Sample.unity:1333 | 씬의 `Island` MeshFilter 는 이제 내장 Cylinder 가 아니라 `island.asset`(GUID `d1fd583f…`, 실린더 사본)을 가리킨다. 검산(S3)이 「프리미티브」를 내장 메시로 읽으면 기준 4 를 다르게 판정할 수 있다. 「프리미티브」와 「머티리얼 하나 공유」를 함께 만족하려면 UV 를 바꾼 사본이 필요하다는 해석이 맞는지는 사람이 정할 일이다 | confirmed |
| 4 | question | nit | pre-existing | 고위험 경로 패턴 해당 여부 | Assets/10_Scripts/Editor/P03/P03SampleSceneBuilder.cs(경로) | 패턴 `Assets/10_Scripts/Editor/*Build*` 를 `*` 가 `/` 를 넘는 글롭(fnmatch 류)으로 대조하면 이 파일이 걸린다. 그러면 위험 등급과 리뷰 구성이 달라진다. gitignore 식 해석이면 걸리지 않는다. 내용은 빌드 스크립트가 아니라 씬 빌더다. 작업 보고 7절 8번도 같은 점을 적었다 | plausible |
| 5 | thought | nit | pre-existing | 씬을 통째로 다시 만들어 diff 를 읽을 수 없음 | Assets/00_Scenes/P03_CLI_Sample.unity(+715/−715) | 이 수정으로 의미가 바뀐 것은 섬의 메시 참조(1333행)와 머티리얼 GUID 둘뿐이다. 그런데 `Build()` 가 씬을 처음부터 다시 만들어 fileID 가 전부 바뀐다. 그래서 씬 YAML diff 에서 의미 있는 변경을 사람이 가려낼 수 없다 | confirmed |

## 확인하지 않은 것 · 추정한 것
- **`CopySerialized` · `CreateAsset` 동작**: `CopySerialized` 가 대상에 `SetDirty` 를 거는지는 Unity 소스나 문서로 확인하지 않았다(1행이 plausible 인 이유). `AssetDatabase.CreateAsset` 이 그 자리에서 디스크에 쓴다는 것은 Unity 동작 지식에 기댄 추정이다.
- **UV 규약**: 계산식 `(12 % 4 + 0.5) / 4 = 0.125` · `(12 / 4 + 0.5) / 4 = 0.875` 는 `make_enemies.py:58` 의 `set_cell` 식과 같다. `island.asset` 의 `_typelessdata` 를 풀어 보니 정점 88개(stride 56, UV0 오프셋 40)의 UV0 가 전부 (0.125, 0.875) 다.
- **PNG 행 순서 · 섬 색**: 「Blender `img.pixels` 첫 행 = 아래 → PNG → Unity v=0 아래」라는 행 순서는 추정이다. 모델 셋이 같은 팔레트로 맞게 보인다는 보고에 기댔다. 캡처를 열어 섬 색을 보지는 않았다(`z.captures` 격리).
- **섬 색이 옛 머티리얼과 같은지**:
  - 확인한 것: `palette.mat` 의 `_BaseColor` 흰색 · `_Smoothness` 0.1 · 셰이더 GUID `933532a4…`(URP Lit으로 추정). `palette.png` 는 sRGB · Point · 밉맵 끔이고, 플랫폼별 압축 덮어쓰기는 모두 `overridden: 0` 이다.
  - 비교하지 않은 것: 옛 `island.mat` 의 나머지 속성.
- **지운 `island.mat` 의 참조**:
  - 옛 GUID `d3af3376…` 는 `3a1dd18` 트리 어디에서도 참조되지 않는다.
  - 씬의 머티리얼 참조는 `palette.mat` 하나뿐이다. 세 `_bst` FBX 의 remap 도 `palette.mat` 을 가리킨다.
  - 빌더에서 지운 `LIT_SHADER_PATH` · `GRASS_*` 를 쓰는 다른 곳은 없다.
- **작업 보고 최종본**: 미추적 작업 보고(`4_create_P03_샘플씬_opus.md`)는 리뷰 도중 바뀌었다. 첫 읽기에서는 5절 112-113행이 `island.mat` 을 지금도 있는 것처럼 적고 있었다. 다시 읽을 때는 117-118행으로 고쳐져 있었다. 최종본은 확인하지 못했다.
- **읽은 파일(격리)**: 같은 폴더의 `코드리뷰_opus/처리.md` 는 저장소 폴더 안이라 열었다. 1라운드 지적과 겹치지 않게 하려는 목적이었다. 거기서 클래스 요약에 더한 `_bst` 사유 줄이 UNI.14 대응이고, UNI.09 도 이 커밋으로 닫혔다는 것을 확인했다.
- **UNI.12 방향 계산**:
  - `Cross(up, (sin a, 0, cos a)) = (cos a, 0, −sin a)` 다. 각 0 이면 위치가 +Z 이고 접선이 +X 다. 요 0 · 피치 58° 카메라의 화면에서 이것은 시계 방향이므로 새 주석이 맞다.
  - 「제자리」는 `P03FrameRecorder` 가 Flap 만 샘플링한다는 코드로만 판단했다. 클립에 루트 곡선이 있는지(UNI.01)는 보지 않았다.
- **고위험 경로**: `ProjectSettings/` · `Packages/` · `**/Save*` · `Assets/30_Assets/Data/` 는 바뀌지 않았다(diff stat 5파일, 모두 수정 허용 영역 안). `*Build*` 패턴은 4행에 적었다.
- **실행**: 컴파일 · 에디터 실행 · 테스트는 돌리지 않았다(지시).