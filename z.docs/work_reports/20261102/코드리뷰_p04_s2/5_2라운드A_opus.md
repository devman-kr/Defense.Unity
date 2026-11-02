---
id: 20261102-ac646265-final
title: 코드리뷰 — 에이전트 ac646265 최종 보고 원문
summary: 「P04 S2 센서 2라운드」 에이전트(claude-opus-5-5 · 도구 호출 15)가 돌려준 최종 보고 원문 — 출처 SubagentHandback.
doc_type: review
status: final
lifecycle: static
created: 2026-11-02
updated: 2026-11-02
work_unit: P04
---
세션 모델: Opus 5.5(지시 기준과 같다).

| # | 종류 | 심각도 | 기원 | 유형 | 파일:줄 | 실패 시나리오(입력 → 잘못된 결과) | 확신 |
|---|---|---|---|---|---|---|---|
| 1 | issue | important | new | 산출물 재현성 · 공개 저장소 로컬 경로 | `Tools/Blender/make_humanoids_smooth.py:888-892` · `Assets/30_Assets/Models/P04_Smooth/SkeletonArcher.fbx` · `Zombie.fbx` | 저장 순서를 바꿔 스크립트 산출 FBX 바이트가 달라졌는데(`z.tmp/p04_out/s2r/SkeletonArcher.fbx` 970524 B, 커밋된 것 970828 B) `1eb9fc8` 은 FBX 를 다시 커밋하지 않았다(이 FBX 의 마지막 변경은 `4130cd2`). 그래서 스크립트를 다시 돌리면 저장소 사본과 다른 파일이 나온다. 커밋된 두 FBX 에는 아직 `/Users/devman/_Projects/_Personal/Games/Defense/z.tmp/p04_out/s2/p04_humanoids_smooth.blend` 절대 경로가 들어 있다(`git show 1eb9fc8:… \| strings` 로 확인). 공개 저장소에 계정 이름이 든 로컬 경로가 그대로 남는다 | confirmed |
| 2 | question | nit | pre-existing | 상태 불일치(⑧ 부분 해소) | `Tools/Blender/make_humanoids_smooth.py:856` · `:889-892` | `.blend` 는 아마추어를 배치 위치 `(1.2,0,0)` · `(2.8,0,0)` 에 둔 채 저장된다. 정본 FBX 는 원점에서 내보낸다(배치는 `export_all(out)` 뒤에 적용). 그래서 그 `.blend` 를 열어 같은 옵션으로 다시 내보내면 동작 섞임은 사라지지만 루트가 1.2 m · 2.8 m 어긋난 FBX 가 나온다. 옛 순서(배치 → 저장 → 원점 복귀)에도 같았다 | confirmed |
| 3 | nitpick | nit | new | 문서 주석 붙는 대상 | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:289-290` · `:310` | `JointProbe` 를 옮기면서 `Clips` 위에 있던 `/// <summary>모델 넷의 클립 …</summary>` 와 `class Clips` 사이에 끼워 넣었다. 그래서 그 요약이 `JointProbe` 에 붙고(IDE 에서 `JointProbe` 위에 커서를 올리면 「모델 넷의 클립」이 뜬다) `Clips` 는 요약을 잃었다. 둘 다 private 라 표준 §(문서 주석 1 — public 만 필수) 위반은 아니다 | confirmed |
| 4 | nitpick | nit | pre-existing | 리터럴 중복(⑩ 과 같은 종류) | `Assets/10_Scripts/Editor/P04/P04HumanoidCapture.cs:61-62` | 출력 파일 이름 `P04S2_Close_ArcherShoot24_Lit.png` · `P04S2_Close_ZombieWalk25_Lit.png` 에 프레임 번호가 리터럴로 들어 있다. `ARCHER_MAX_ELBOW_FRAME` · `ZOMBIE_MAX_KNEE_FRAME` 을 Blender 실측에 맞춰 고치면 실제로 찍힌 프레임과 파일 이름이 어긋난다 | confirmed |
| 5 | thought | nit | out-of-scope | 공개 저장소 로컬 경로 | `Tools/Blender/make_humanoids_smooth.py:858-862`(FBX_OPTIONS `path_mode='STRIP'`) · `Assets/30_Assets/Models/P04_Smooth/*.fbx` | 1번을 고쳐 다시 내보내도 텍스처 절대 경로는 남는다. 다시 낸 `s2r/SkeletonArcher.fbx` 에도 `/Users/devman/…/s2r/Palette.png` 가 들어 있고, S1 `Slime.fbx` 에는 `/Users/devman/…/p04_out/Palette.png` 와 `p04_enemies_smooth.blend` 경로가 있다. 그러니 `.blend` 경로만 빠지는 것이고 로컬 경로 노출은 그대로다 | confirmed |

확인하지 않은 것 · 추정한 것
- 지적 ⑩ · ⑪ · ⑫ · ⑬ 은 해소됐다고 본다. ⑩: 상수 넷의 값이 `s2r/p04_result.json` 의 `joints` 와 맞다(궁수 ElbowR Shoot 24 · KneeL Walk 19, 좀비 ElbowL Attack 10 · KneeL Walk 25). ⑪: `"Archer"` · `"Zombie"` 를 `Find` 하는 리터럴이 더 남아 있지 않다(`git grep`). ⑫: §3 멤버 순서 ⑩ 중첩 타입이 클래스 끝에 있다. ⑬: 카메라 월드 계산은 `CreateRing` 이 대상의 local 변환을 정한 뒤에 일어나고, `SetParent(parent, false)` 다음에 `SetPositionAndRotation` 를 부르니 순서도 맞다.
- ⑬ 은 지금 씬에서 결과가 같다고 추정한다. `P04Humanoids` 루트와 묶음이 원점 · 회전 없음이라 옛 local 계산과 같은 값이 나오고, 씬 `.unity` 를 다시 만들지 않은 것도 결과에 영향이 없다고 본다. 씬을 다시 만들어 값을 대조하지는 않았다.
- ⑧ 의 저장 위치가 렌더 단계(8절) 전이라 `.blend` 에 월드 · 카메라 · 해 · S1 가져온 모델이 들어가지 않는다는 것은 코드 순서로만 확인했다. 저장 시점의 상태는 활성 동작 없음(`assign(arm, None)`) · 프레임 1(6절 끝 `frame_set(1)` 뒤로 프레임을 바꾸는 호출이 없다)로 옛 저장과 같다고 추정한다. 내보내기 도구가 현재 프레임을 되돌리는지는 확인하지 않았다.
- 1번의 「300 B 안팎 감소」는 `z.tmp/p04_out/s2r/` 파일 크기로만 대조했다. Zombie.fbx 다시 낸 판의 크기와 바이트 비교는 하지 않았다.
- 실행 · 컴파일 · 테스트는 하지 않았다(읽기만).