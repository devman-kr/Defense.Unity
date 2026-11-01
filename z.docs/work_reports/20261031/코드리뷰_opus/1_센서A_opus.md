---
id: 20261031-ae9c8cc0-final
title: 코드리뷰 — 에이전트 ae9c8cc0 최종 보고 원문
summary: 「P03 세션 센서 리뷰(읽기 전용)」 에이전트(claude-opus-5-5 · 도구 호출 27)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-10-31
updated: 2026-10-31
work_unit: P03
---
| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | issue | blocking | new | 모델 루트에 `SampleAnimation` 을 직접 걸어, 클립에 루트 변환 곡선이 있으면 씬 배치를 덮어씀 | `Assets/10_Scripts/Editor/P03/P03FrameRecorder.cs:44,49`(배치는 `P03SampleSceneBuilder.cs:107,123,168-169`) | `make_enemies.py:351-355` 는 `bake_anim_simplify_factor=0.0`(모든 곡선에 키 강제) · `bake_anim_use_all_actions` 로 내보낸다. 그러면 뼈대 오브젝트(SlimeRig · BatRig) 자체의 위치 · 회전 · 크기 곡선도 테이크에 들어간다. FBX 루트 노드가 하나뿐이라 Unity 는 그 노드를 프리팹 루트로 합친다(「루트 X 270°」 함정과 같은 원인). 그 곡선이 경로 `""` 로 남아 있으면 `clip.SampleAnimation(인스턴스 루트, t)` 가 매 프레임 루트 localPosition 을 (0,0,0) 으로, 회전을 내보낼 때 값으로 덮는다 → 녹화(성공 기준 6)에서 슬라임 8 · 박쥐 4 가 섬 중앙(탑 안)에 겹친다. 정지 캡처(기준 5)는 샘플링을 안 거쳐 정상이므로 놓치기 쉽다. 확인 방법: `AnimationUtility.GetCurveBindings(clip)` 에 path `""` 항목이 있는지 보거나 `frame_000.png` 를 연다 | plausible |
| 2 | question | important | new | 성공 기준 4 「URP Lit 하나에 팔레트 공유」와 다른 두 번째 머티리얼 | `P03SampleSceneBuilder.cs:24,76-85` · `Assets/30_Assets/Models/P03_CLI/island.mat` | 섬에는 팔레트를 쓰지 않는 별도 URP Lit `island.mat`(텍스처 없음, `_BaseColor` 단색)을 새로 만든다 → 씬 머티리얼이 둘이다. 기준 4 를 글자대로 읽으면 미달이다. 계약 「모델마다 만들지 않는다」는 모델 대상이라 위반이 아닐 수도 있다. 다만 팔레트 셀 12(잔디)를 쓰지 않은 이유가 코드에도 커밋에도 없다 | confirmed |
| 3 | question | important | new | 알려진 결함 「날갯짓 회전축」을 동작 데이터가 아니라 렌더 시점 변경으로 처리 | `Tools/Blender/make_enemies.py:407-410`(회전 키 `299-305` 는 blame `1f878261` 그대로) · `Tools/Blender/README.md:33` | README(763866b)는 결함을 「날갯짓 회전축」이라 적었다. 그런데 diff 는 회전 키를 그대로 두고 박쥐 렌더만 정면 카메라로 바꿨다 → FBX 의 Flap 데이터는 시작 커밋과 같다. 대칭의 근거는 커밋 `a5a2a5c` 메시지의 날개 끝 높이 z(f01 0.444/0.444 · f06 -0.278/-0.278)이고, 내 계산(뼈 roll 0 이면 월드 Y 축 회전 = 뼈 X 축 순수 회전)과도 맞는다. 다만 `tip_z_l/r` 은 z 만 보므로 축이 Y-Z 사이로 기운 경우는 구별하지 못한다. 기획 측이 본 결함이 정말 원근 착시였는지는 f01/f06 렌더로 사람이 확인해야 한다 | plausible |
| 4 | issue | nit | new | 파일 사이 암묵 순서 의존, 가정에 단언 없음(§6.9 · §1.3 이름 상수 한 곳) | `P03FrameRecorder.cs:16-18,34,42,57` ↔ `P03ModelImport.cs:22,90` | `Record` 는 클립 이름 `Idle` · `Hop` · `Flap` 을 자기 상수로 들고 사전에서 바로 꺼낸다 → `P03ModelImport.Apply()` 를 먼저 안 돌렸거나(이름이 `BatRig\|Flap`) 두 파일 중 한쪽 이름만 바뀌면 설명 없는 `KeyNotFoundException` 이 난다 | confirmed |
| 5 | issue | nit | new | 파일 쓰기 예외 경로에서 정리 없음(§6.5 파일 경계) | `P03FrameRecorder.cs:63-82`(77) · `37-52` | `File.WriteAllBytes` 가 실패하면(권한 · 디스크) `camera.targetTexture` · `RenderTexture.active` 가 복원되지 않고 임시 RT · Texture2D 가 남는다. 끝의 씬 다시 열기(58)도 돌지 않아 샘플링한 자세가 열린 씬에 더티로 남는다 → 뒤이은 `scene save` 가 그 자세를 씬 파일에 저장한다 | confirmed |
| 6 | issue | nit | new | 꼬리 벗기기 뒤 이름 충돌을 검사하지 않음 | `P03ModelImport.cs:38-49,88-93` · `P03FrameRecorder.cs:105` | 한 FBX 에 `Rig\|Hit` 과 `Rig\|Hit.001` 이 같이 들어오면(all-actions 내보내기는 뼈 경로가 맞는 동작을 전부 넣는다) 둘 다 `Hit` 이 되어 클립 이름이 겹치고, `LoadClips` 는 말없이 마지막 것을 쓴다. `Attack.2` 처럼 일부러 붙인 숫자 꼬리도 잘린다. 지금 메타에는 겹침이 없다 | plausible |
| 7 | issue | nit | new | 입력 검증 없음 | `P03FrameRecorder.cs:25,39,65` | `fps` 가 0 이면 `time` 이 무한대가 되고 `Mathf.Repeat` 가 NaN 을 내 자세가 깨진 프레임이 나온다. `width` · `height` 가 0 이면 RT 생성이 실패한다. 호출 값이 고정이면 영향 없음 | confirmed |
| 8 | nit | nit | new | 색을 코드에 둠(§8.1), Blender PAL 표와 원본이 둘 | `P03SampleSceneBuilder.cs:48-52` ↔ `make_enemies.py:27-28` | Blender 쪽 PAL 셀 11 · 12 를 바꾸면 섬과 점광 색만 옛 값으로 남는다. 둘을 맞추는 수단은 주석뿐이다 | confirmed |
| 9 | nit | nit | new | 패키지 내부 셰이더 경로 문자열이 두 곳에 중복(§8.6), 빌더 쪽은 null 단언도 없음 | `P03ModelImport.cs:17,69-71` · `P03SampleSceneBuilder.cs:25,79` | URP 패키지가 `Shaders/Lit.shader` 위치를 바꾸면 두 곳 다 `LoadAssetAtPath<Shader>` 가 null 을 받는다. ModelImport 는 단언 뒤 `new Material(null)` 에서 예외, 빌더는 단언 없이 바로 예외 | confirmed |
| 10 | nit | nit | new | 에셋 이름이 소문자(§17 PascalCase) | `Assets/30_Assets/Models/P03_CLI/palette.mat` · `island.mat`(`P03ModelImport.cs:14` · `P03SampleSceneBuilder.cs:24`) | FBX · `palette.png` 이름은 지시서와 기존 스크립트가 정했으므로 제외한다. 이 diff 가 새로 정한 `.mat` 두 이름만 해당한다. `island.mat` 은 모델이 아니라 프리미티브용이라 §17 표상 공용 `Materials/` 자리다 | confirmed |
| 11 | question | nit | new | 이름 기반 탐색이 §7.3 / CLAUDE.md 「`Find*`」 금지(지시서: 에디터 스크립트에도 같은 금지)에 걸리는지 | `P03FrameRecorder.cs:29-31,86-91` | `Transform.Find`(이름)와 루트 이름 비교로 카메라 · 무리를 찾는다 → 씬에서 이름이 바뀌면 null → NRE. §7.3 목록(`GameObject.Find` 등)에는 없지만 CLAUDE.md 줄임말은 `Find*` 라 판정이 필요하다 | confirmed |
| 12 | nit | nit | new | 주석이 실제 방향 · 동작과 다름 | `P03SampleSceneBuilder.cs:121`(요약 `10`) | `Cross(up, outward)` 는 위에서 볼 때(+X 오른쪽, +Z 위) 시계 방향 접선이다. 씬의 Bat01 은 위치 (0.92, 1.35, 2.22), 회전 y 0.831 · w 0.556 → 앞 방향 (0.924, 0, -0.383) 이라 시계 방향이다. 주석은 「시계 반대」라고 한다. 요약의 「탑 둘레를 도는」 박쥐는 녹화에서도 제자리다(Flap 만 샘플링) | confirmed |
| 13 | question | nit | new | 성공 기준 4 「세로 9:16」을 맞추는 곳이 캡처 크기뿐 | `P03SampleSceneBuilder.cs:147-160` | 카메라에 비율 고정이 없어 씬 자체는 9:16 이 아니고, 출력 크기(CLI `--width/--height`, `Record` 720×1280)가 비율을 정한다. 가로 1920×1080 캡처도 같은 카메라(세로 FOV 60 고정)라 구도가 다르다. 의도라면 문제없다 | confirmed |
| 14 | question | nit | new | `_bst` 변형을 고른 이유가 코드에 없음 | `P03SampleSceneBuilder.cs:11,20-23,169` | 요약에는 「`_bst` 를 쓴다」만 있다. 기본 변형으로 바꾸면 `InstantiateModel` 의 localRotation 덮어쓰기(169)가 루트 X 270° 를 지워 모델이 눕는다 → 다음 사람이 변형을 바꿀 때 막힐 곳이다(기준 3(c) 사유는 보고에만 있을 것) | plausible |
| 15 | nit | nit | new | 공개 범위가 필요보다 넓음(§3.3) | `P03ModelImport.cs:14,38` · `P03SampleSceneBuilder.cs:15-21` | `PALETTE_MATERIAL_PATH` 는 클래스 안에서만, `StripClipName` 과 빌더 상수는 같은 어셈블리 안에서만 쓰인다. public 이 필요한 것은 `script eval` 진입점(`Apply` · `Build` · `Record`)뿐이다 | confirmed |
| 16 | nit | nit | new | 반환값 변수 이름(§5.2 `retval`) | `P03ModelImport.cs:27` · `P03FrameRecorder.cs:99` | 반환값 변수가 `report` · `clips` 다. 표준은 `retval` 을 메서드 시작에 둔다 | confirmed |
| 17 | nit | nit | new | 단언 메시지에 `[태그]` 없음(§13.2) | `P03ModelImport.cs:54,70,84` · `P03SampleSceneBuilder.cs:70,165` · `P03FrameRecorder.cs:93` | 콘솔에서 출처별로 거를 수 없다 | confirmed |
| 18 | thought | nit | new | 네임스페이스 `Defense.Editor` 가 `UnityEditor.Editor` 를 가림 | `Assets/10_Scripts/Editor/Defense.Editor.asmdef:3` · `P03*.cs` 의 `namespace Defense.Editor.P03` | 나중에 `Defense.Editor.*` 안에 `class XInspector : Editor` 를 쓰면 `Editor` 가 네임스페이스로 해석되어 CS0118 이 난다(`UnityEditor.Editor` 로 적어야 함). asmdef 이름은 지시서가 정한 것 | plausible |
| 19 | thought | nit | new | 수정 허용 글롭 밖에 폴더 `.meta` 추가 | `Assets/10_Scripts/Editor.meta` · `Assets/30_Assets/Models.meta` | `Editor/**` · `Models/P03_CLI/**` 의 부모 폴더 meta 다. 새 폴더라 짝으로 꼭 필요하다 — 범위 위반으로 볼지 판정만 하면 된다 | confirmed |
| 20 | thought | nit | new | 요청에 없던 진단 출력 추가 | `Tools/Blender/make_enemies.py:312,320-322,330` | `P03_RESULT` 에 `tip_z_l/r` 키가 늘었다. 성공 기준 1 검증용이라 범위 안으로 볼 수 있다. 이 키를 읽는 소비자는 없다 | confirmed |
| 21 | thought | nit | out-of-scope | README 의 「알려진 결함 둘 … 고친다」 문구가 남음 | `Tools/Blender/README.md:33` | 결함을 처리한 뒤에도 README 는 미해결로 적고 있다. README 는 수정 허용 영역 밖이라 이 diff 가 고칠 수 없다 — 머지 때 누가 고칠지 정해야 한다 | confirmed |
| 22 | thought | nit | out-of-scope | 순수 로직 `StripClipName` 에 EditMode 테스트 없음(§15.1) | `P03ModelImport.cs:38-49` | `Tests/` 가 읽기 전용이고 테스트 요청도 없어 이 diff 범위 밖이다. `Defense.Tests.EditMode` 는 `Defense.Editor` 를 참조하지 않는다. #6 의 충돌 · 과잉 절단은 테스트가 있으면 잡힌다 | confirmed |

### 확인하지 않은 것 · 추정한 것
- **#1 은 추정이다.** FBX(이진)를 열지 않았고, 녹화 프레임이 있는 `z.captures/` 도 격리 조건이라 열지 않았다. 「뼈대 오브젝트 곡선이 테이크에 들어간다」는 Blender FBX 내보내기의 알려진 동작(simplify 0 이면 키를 강제하고, all-actions 는 뼈대 오브젝트를 포함)에서, 「Unity 가 단일 루트 노드를 프리팹 루트로 합치고 그 곡선이 path `""` 로 남는다」는 Unity 동작 지식에서 나온 추론이다. Unity 가 상수이면서 기본 자세와 같은 루트 곡선을 가져올 때 버린다면 #1 은 성립하지 않는다.
- **#3 의 대칭 수치는 커밋 메시지의 값을 그대로 썼다.** 렌더 PNG 는 열지 않았다. 축 계산은 날개 뼈 roll 이 0 이라고 가정했다.
- **`capture game_view --source camera` 의 카메라 선택은 확인했다.** 근거는 저장소 폴더 안 `Library/PackageCache/com.unity.pipeline@363b8bb9e91d/Editor/Commands/Capture/CaptureCommands.cs:169-186` 이다. `Camera.main` 이 없으면 첫 번째로 켜진 카메라를 쓰므로, 태그가 Untagged 인 `PortraitCamera` 로도 동작한다 — 지적하지 않았다.
- **성공 기준 2 는 메타로 확인했다.** `*.fbx.meta`(diff 대상 밖이지만 맥락으로 읽음)의 클립 이름 · `takeName` · `loopTime` 과 머티리얼 재지정(Palette → palette.mat), `palette.png.meta` 의 Point · 밉맵 끔 · 비압축(플랫폼 덮어쓰기 없음)이 기준과 맞는다. 씬 YAML 에서는 슬라임 · 박쥐 위치와 회전 값만 대조했다.
- **기준 3(b)(루트 회전 · 스케일 · 뼈 수)를 내는 코드는 diff 에 없다.** CLI 로 쟀을 것으로 추정하며, 확인하지 않았다.
- **세로 구도는 계산만 했다.** 세로 9:16 · FOV 60 · 거리 19 m 이면 목표 깊이에서 가로 반폭이 약 6.2 m 라, 반지름 6.5 m 인 섬의 좌우 끝이 조금 잘릴 것으로 계산된다. 그림은 보지 않았다.
- **아무것도 실행하지 않았다.** 컴파일 · 테스트 · 에디터를 돌리지 않았으므로 「경고 0 · EditMode 2/2」는 검증하지 않았다. MSAA RT 에서의 `ReadPixels`, 창 없는 에디터에서 URP `camera.Render()` 가 동작하는지도 배경 설명을 믿었다.
- **작업 보고서는 읽지 않았다.** HEAD 에 아직 보고 커밋이 없다. 따라서 #2 · #14 의 사유가 보고에 적혀 있는지는 모른다.