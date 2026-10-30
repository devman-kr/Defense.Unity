# Lantern Rush! (랜턴 러시!)

> 고르는 건 당신, 싸우는 건 탑.

잔디 섬 한가운데 등불 탑 하나. 밤이 되면 불빛을 노리고 사방에서 괴물이 몰려온다. 탑은 알아서 싸우고, 당신은 레벨업 카드를 고르고 위기의 순간에 낙뢰를 떨어뜨려 **새벽(30분)까지** 버틴다. 낮에는 번 골드로 탑을 강하게 하고 꾸민다. 한 판 10~30분, 세로 화면 한 손 조작의 3D 캐주얼 로그라이크 디펜스.

- **플레이(WebGL)**: (채움 — DEF-M05)
- **Android APK**: (채움 — Releases)
- **플레이 영상**: (채움)

![게임 화면](docs/images/gameplay.gif)

## 이렇게 논다

| 입력 | 하는 일 |
|---|---|
| 카드 탭 | 레벨업 때 카드 3장 중 하나 — 무기 · 패시브 · 진화 |
| 스킬 탭 | 30초마다 원하는 곳에 낙뢰 |
| 자동 선택 | 켜 두면 완전 방치 |
| 배속 ×2 | 30분 판을 15분에 |

한 판은 하룻밤이다. 등불이 꺼지거나 새벽이 오면 아침 — 「낮의 섬」에서 번 골드로 영구 강화 · 새 무기를 열고, 기록으로 탑과 섬을 꾸민 뒤 다시 「하루 보내기」.

## 기술 하이라이트

- **Unity 6 · URP**, Android · WebGL 빌드
- **데이터 주도 밸런스** — 무기 · 적 · 난이도 곡선이 전부 ScriptableObject. 코드 수정 없이 조정
- **적 300마리 · 60fps 목표** — 개체마다 `Update` 없이 시스템 하나가 배열로 처리, 오브젝트 풀, 공간 해시 표적 찾기 (측정값: (채움 — 기기 · 빌드 · 날짜))
- **코어 / 게임 분리** — `Core` 어셈블리는 게임을 모른다. 다음 게임(AirShooter)이 그대로 가져간다
- **테스트** — 난이도 곡선 · 카드 추첨 · 피해 계산 · 저장 변환 EditMode 테스트, 한 판 자동 진행 PlayMode 스모크
- **씬 전환 없는 하루 순환** — 낮(로비) · 저녁 · 밤(전투) · 새벽을 한 장면의 상태 기계와 조명 전환으로
- **모의 광고 자리** — `IAdService` 인터페이스로 보상형 광고 위치(골드 2배 · 부활 · 리롤)만 설계

## 구조

```
Assets/_Project/
  Core/      게임 무관 — 풀 · 상태 기계 · 스탯 · 드래프트 · 웨이브 디렉터 · 저장 · UI 틀
  Defense/   이 게임 — 탑 · 무기 · 적 · 낮의 섬 · 하루 순환
  Data/      밸런스 데이터(ScriptableObject)
  Tests/     EditMode · PlayMode
z.docs/      마일스톤 · 작업 보고서(기획서는 별도 기획 저장소)
```

## 직접 빌드하기

1. Unity 6000.3.27f1(`ProjectSettings/ProjectVersion.txt`) + Android · Web 빌드 모듈
2. 저장소를 Unity Hub 에서 열고 `Assets/_Project/Scenes/Main.unity` 실행 (채움 — DEF-M01)
3. 빌드: 에디터 `File > Build Profiles` 에서 `Android-Dev` · `Web` 프로필, 또는 Unity CLI 로 `unity build . --profile Web --output-path <폴더>`

## 만든 방법 — Claude 와 함께

이 게임은 기획 · 코드 · 리소스를 사람 한 명과 Claude(Anthropic) 가 함께 만들었다. 유료 에셋은 쓰지 않았다.

| 구분 | 비중 | 내용 |
|---|---|---|
| 직접 제작(사람 + Claude) | (채움) | 코드 전부 · 셰이더 · 이펙트 · UI · 프리미티브 모델 · Blender 모델 |
| CC0 · 오픈 라이선스 | (채움) | 3D 모델 · 효과음 · 폰트 — 전체 목록 [`ASSETS.md`](ASSETS.md) |
| AI 생성 | (채움) | 도구 · 모델 표기는 `ASSETS.md` |

개발 과정(기획 · 과제 · 결정 · 세션 로그): [Defense.Design](https://github.com/devman-kr/Defense.Design)(기획서 · 목업) · [Defense.Planning](https://github.com/devman-kr/Defense.Planning)(과제 · 결정 · 계획 · 코드 리뷰 기록) · [Defense.Logs](https://github.com/devman-kr/Defense.Logs)(세션마다 프롬프트 원문 · 다이제스트 — 훅이 쓴다) · 세 게임 공통 결정 [games-common](https://github.com/devman-kr/games-common)

## 라이선스

코드: [MIT](LICENSE). 외부 에셋 · Unity 템플릿 생성물은 각자의 라이선스를 따른다(`ASSETS.md`).
