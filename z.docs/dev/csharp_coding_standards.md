---
id: 20261030-unity-csharp-standards
title: C# 코딩 표준 — Unity
summary: Unity 6000.3(C# 9) 모바일 · 오프라인 게임용 C# 규칙으로 명명 · var 금지 · UnityEngine.Object 의 null 검사 · 풀 · 시간 · JsonUtility 저장 · 테스트 등 §0~§19 와 근거 표를 정하며 절 머리에 코드리뷰 영역 코드를 단다.
doc_type: guide
status: final
lifecycle: living
created: 2026-10-30
updated: 2026-10-31
---
# C# 코딩 표준 — Unity

Defense(Lantern Rush!)와 뒤이은 게임들의 Unity C# 규칙이다. EasyCheck.Server 의 C# 코딩 표준(POCU C# 표준 계열 — 원문은 Planning 커밋 `caf8ac8`)을 Unity 6 · 모바일 · 오프라인 · 공개 포트폴리오 전제로 고쳤다(Planning P00 검토서 2026-10-30 · 결정 D-06 · D-07). 정본은 Planning `_템플릿/Unity_csharp_coding_standards.md`, 사본은 `Defense.Unity/z.docs/dev/csharp_coding_standards.md` — 고칠 때는 정본을 고치고 복사한다.

**읽는 법**: 규칙은 번호 한 줄이라 「표준 §5.7」처럼 인용한다. Unity 전용 규칙에는 「왜」 한 줄이 붙고, 번호는 끝의 「근거」 표다. 절 머리의 대괄호는 코드리뷰 가이드 영역 코드(`SAVE` · `ECON` · `LIFE` · `TIME` · `PERF` · `INPUT` · `CONTRACT` · `TEST` · `UI` · `BUILD`)이고, 대괄호가 없는 절(기본 원칙 · 이름 · 클래스 구조 · 서식 · 에셋 이름 · 주석)도 리뷰가 보지만, 처리표의 영역 열은 지적이 닿는 동작 영역(예: §1.1 위반은 `BUILD`, §1.3 은 `CONTRACT`)으로 작성 세션이 매긴다. 폴더 · 어셈블리 이름과 위치 · 게이트 · 패키지 규칙은 저장소 `CLAUDE.md` 「코딩」 에만 있고 여기서는 가리키기만 한다. 「작성 세션이 더한 규칙」 표시는 결정 D-06 · D-07 에 없던 규칙이라 사용자 확인 대기라는 뜻이다. 이 표준은 규칙이지 게이트가 아니다 — 지키는지는 코드 리뷰가 본다. 언어는 C# 9.0(Unity 6000.3, 근거 1).

---

## 0. 기본 원칙

1. **가독성을 최우선으로 삼는다.** 코드가 그 자체로 문서여야 한다.
2. 합당한 이유가 없는 한 **`.editorconfig` 가 정한 IDE 자동 서식을 따른다.**
3. **일관성을 유지한다.** 기존 코드의 스타일을 존중한다.
4. **C# 일반 관례와 Unity 가 다를 때는 Unity 를 따른다** — 직렬화(§12) · null(§6) · 수명 주기(§7 · §9).
5. **접근점은 적을수록 좋다.** 싱글톤 · 정적 전역은 최소한으로 쓴다 — 무분별한 싱글톤은 의존을 숨기고 초기화 순서와 테스트 격리를 깨뜨려 구조가 망가지기 쉽다. 기본은 부트스트랩이 참조를 넘기는 것(§7.3).

---

## 1. 명명

| 대상 | 규칙 | 비고 |
|---|---|---|
| 클래스 · 구조체 · 열거형 | PascalCase | 비트 플래그는 `[Flags]` + 비트 시프트, 이름은 복수 |
| 인터페이스 | `I` + PascalCase | `IAdService` |
| private 필드 | `_camelCase` | `readonly` 도 같다. 인스펙터는 `_` 를 벗겨 보여 준다 |
| 상수(`const` · `static readonly`) | UPPER_SNAKE_CASE | Unity 관례는 `k_PascalCase` 이나 이 저장소는 UPPER_SNAKE(D-07) |
| 프로퍼티 | PascalCase | 자리는 §3 순서(필드 뒤) |
| 지역 변수 · 매개변수 | camelCase | |
| 메서드 | PascalCase, 동사 + 명사 | 코루틴은 동사 + `Routine`(`SpawnWaveRoutine`, 작성 세션이 더한 규칙) |
| bool 반환 메서드 · 프로퍼티 | `Is` · `Can` · `Has` · `Should` 접두 | |
| 이벤트 | 동사구 과거형 `WaveStarted` · `EnemyDied`, 발생 메서드 `OnWaveStarted` | C# `event Action<T>`(§7.5) |
| 줄임말 | 단독이면 대문자, 뒤에 단어가 오면 파스칼 · 카멜 | `OrderID` · `HttpCode` |
| 네임스페이스 | 코어 `Devman.Core.<영역>` · 이 게임 `Defense.Game.<영역>` | asmdef 이름 = 루트 네임스페이스. 목록은 `CLAUDE.md` 「코딩」 |
| 데이터 에셋 클래스 | `<이름>Def` · 곡선 `<이름>Curve` | 기획 데이터 표 K1~K6(§8) |
| 저장 타입 | `Save` 로 시작 — `SaveData` · `SaveSystem` | 코드리뷰 가이드 고위험 경로 `**/Save*` 가 이 이름으로 잡는다 |
| 시스템 · 서비스 | `<이름>System` · `I<이름>Service` | `WaveSystem` · `ISaveService` |

1. **코어 경계**: 다른 게임에서 수정 없이 쓰는 것(저장 · 광고 자리 · 플랫폼 · 풀 · 로그 래퍼 · 시간 배속 · 범용 유틸)은 `Devman.Core`, 이 게임의 규칙(웨이브 · 무기 · 드래프트 · 강화)은 `Defense.Game`. Core 코드에는 게임 이름이 들어가지 않는다(참조 금지는 `CLAUDE.md` 의 asmdef 규칙). 애매하면 Game 에 두고 두 번째 게임이 필요로 할 때 Core 로 올린다(작성 세션이 더한 규칙) — 개별 시스템의 소속은 `DEF-M01` asmdef 분할 때 정한다.
   — 왜: 코어는 다음 게임이 복사해 간다(G-07). 게임 이름이 들어 있으면 복사할 때마다 고쳐야 한다.
2. 반복문 인덱스 말고는 의미 있는 이름. getter · setter 메서드 대신 프로퍼티. `OrNull` 접미는 §6.1.
3. **경계의 이름**: 기획 데이터 표의 열 ↔ 데이터 에셋 필드는 `_` 와 대소문자를 빼면 같은 이름이고 단위는 접미(`Seconds` · `PerSecond` · `Percent` · `Meters`). 저장 DTO 의 public 필드는 camelCase — 그 이름이 JSON 키가 된다(§12.1). 태그 · 레이어 · 씬 · 애니메이터 파라미터 · 입력 액션 이름은 상수 클래스 하나(`GameIds`)에 — 문자열 리터럴을 흩뿌리지 않는다.

---

## 2. 파일 · 프로젝트 구조 [BUILD]

1. **클래스 하나 = 파일 하나**, 파일 이름 = 클래스 이름(대소문자까지). 한 파일에 클래스 여럿을 두지 않는다.
   — 왜: MonoBehaviour · ScriptableObject 파생은 클래스가 하나면 Unity 가 이름이 달라도 찾아 주지만, 여럿이면 파일 이름과 같은 클래스만 고르고 못 고르면 경고를 낸다(근거 7).
2. partial 은 `클래스명.세부.cs` 로 나누되 `클래스명.cs` 를 반드시 둔다. 드물게 쓴다.
   — 왜: MonoBehaviour 를 partial 로 나누면 파일 이름이 클래스 이름과 같은 파일만 컴포넌트로 붙는다(근거 7).
3. `using` 은 IDE 정렬 — `System` 계열 먼저, 나머지는 알파벳. 안 쓰는 `using` 은 지운다. 파일 범위 네임스페이스(C# 10)는 쓸 수 없다 — 블록으로.
4. 파일 헤더 블록 주석은 없다. 작성자 · 날짜 · 변경 이력은 git 과 작업 보고서에.
5. 폴더 · asmdef 구성은 `CLAUDE.md` 「코딩」 한 곳. 코드 폴더마다 asmdef 하나, 에디터 전용 코드는 `Editor/` + 에디터 asmdef(§16.1).
6. 설정값은 데이터 에셋에 — 게임 구성 상수는 `GameConfigDef`(§8), 사용자 설정은 저장 파일(§12.6). INI · XML · `Resources/` · `StreamingAssets/` 는 만들지 않는다(작성 세션이 더한 규칙 — D-07 비고).

---

## 3. 클래스 구조

멤버 순서: ① 상수 → ② 정적 필드 → ③ `[SerializeField]` 필드 → ④ 런타임 필드 → ⑤ 프로퍼티 → ⑥ 이벤트 → ⑦ **Unity 메시지**(실행 순서대로 `Awake` · `OnEnable` · `Start` · `FixedUpdate` · `Update` · `LateUpdate` · `OnDisable` · `OnDestroy`, 그 밖의 메시지는 그 뒤) → ⑧ public 메서드 → ⑨ private · protected 메서드 → ⑩ 중첩 타입.

1. 그룹 사이는 빈 줄 하나. 구분선 주석은 없다.
2. 연관 있는 멤버끼리 모은다. `#region` 은 쓰지 않는다 — 길어지면 클래스를 나눈다.
3. 상속을 의도하지 않는 클래스는 `sealed`(MonoBehaviour 포함). 밖에 보일 필요 없으면 `internal`(테스트는 §15.5).
4. ISP — 인터페이스는 좁고 분명하게. 실제 자원을 쥔 인터페이스만 `IDisposable`(§9.5).
5. 공통 골격 + 변주(적 행동 · 무기 발사)는 추상 기본 클래스에 골격, 하위 클래스에 변주(Template Method).

---

## 4. 서식

1. 띄어쓰기 4칸, 탭 없음, 줄 끝 LF, 한 줄 120자 안팎.
2. 중괄호는 **Allman**(새 줄). 한 줄 블록에도 중괄호. 예외: 단일 흐름 제어문(`return` · `continue` · `break`)은 생략 허용 — IDE 는 이 예외를 모르므로 `.editorconfig` 는 제안 수준으로 둔다.
3. 한 줄에 변수 하나. 값의 수직 정렬은 하지 않는다(자동 서식이 깨뜨린다).
4. 나머지는 `.editorconfig`(`Defense.Unity/.editorconfig`, 정본 Planning `_템플릿/Unity_.editorconfig`)가 정한다. Unity 컴파일러는 이 파일을 읽지 않는다 — IDE 용이고, 금지는 IDE 가 아니라 리뷰가 본다.

---

## 5. 변수 · C# 기능 [PERF — 5.7 핫패스만]

1. **`var` 금지.** 예외는 우변 `new` 로 타입이 드러날 때와 `IEnumerable` 체인뿐.
2. 변수 가리기 금지. 지역 변수는 쓰는 곳 가까이, 반환값 변수 `retval` 은 메서드 시작에.
3. 컨테이너는 `System.Collections.Generic`. 예외: 코루틴 반환형 `IEnumerator`(`System.Collections`).
   — 왜: Unity 코루틴은 비제네릭 `IEnumerator` 를 반환하는 메서드다(근거 5).
4. 디폴트 매개변수는 비트 패턴 0(`null` · `false` · `0`). 자주 쓰는 호출은 축약형 오버로딩.
5. **금지**: `using` 선언(C# 8) · `??` · `??=`(작성 세션이 더한 규칙) · 대상 형식 `new()`(C# 9) · 레코드 · `init` 세터 · `#nullable` 컨텍스트.
   — 왜(레코드 · `init`): Unity 6000.3 은 C# 9 이지만 `init` 세터를 지원하지 않고 레코드는 직렬화되지 않는다(근거 1). — 왜(`#nullable`): UnityEngine API 에 null 주석이 없어 경고만 늘고, Unity 의 가짜 null(§6.3)을 표현하지 못한다.
6. **필수**: `new` 뒤 명시적 타입. `out` 변수는 별도 줄에 선언(단순하면 인라인 허용).
7. **핫패스 금지**(매 프레임 · 적 · 투사체 단위로 도는 코드): LINQ · 박싱 · 문자열 결합 · 보간 · 클로저 · `params` · 인터페이스 타입으로 받은 컬렉션의 `foreach`(열거자가 박싱된다) · 새 배열 · `yield` 열거.
   — 왜: 매 프레임 할당은 GC 스파이크가 되어 모바일에서 프레임을 떨어뜨린다(§11.1 의 측정으로 확인한다).
8. 권장: `StringBuilder` 는 캐시해 재사용 · `CallerMemberName` 은 로그 래퍼(§13.2)에만 · `struct` 는 작고 불변인 값에만, 큰 구조체는 `in` 으로 전달.
9. 스레드 안전 컬렉션은 쓸 일이 없다 — 메인 스레드 게임(§14.5).

---

## 6. null · 예외 · 단언 [LIFE · SAVE]

1. public 메서드의 매개변수 · 반환값은 null 을 허용하지 않는 것을 추구한다. 허용하면 이름 뒤에 **`OrNull`**(`FindTargetOrNull`).
2. 순수 C# 타입에는 `?.` 허용, `??` 는 금지(§5.5).
3. **`UnityEngine.Object` 파생**(GameObject · Component · MonoBehaviour · ScriptableObject)은 **`== null` · `!= null` 만.** `?.` · `??` · `is null` · `is not null` · `ReferenceEquals`(작성 세션이 더한 규칙) 금지.
   — 왜: Unity 의 `==` 는 관리 참조뿐 아니라 네이티브 객체 포인터도 검사해 파괴된 객체를 null 로 본다(근거 2). `is null` 은 컴파일러가 사용자 정의 `==` 를 부르지 않도록 보장하고, `?.` · `??` 는 피연산자 자체가 null 인지만 본다(근거 3) — 파괴된 객체를 산 것으로 다룬다.
4. Unity 객체를 인터페이스 · `object` · 제약 없는 제네릭 `T` 변수로 들고 있으면 `== null` 도 Unity 의 `==` 가 아니다(연산자는 정적 타입으로 묶인다 — `where T : UnityEngine.Object` 면 Unity 의 `==` 다). 그런 참조의 수명은 넘겨준 쪽(부트스트랩)이 보장하고, 파괴 여부를 꼭 봐야 하면 `(service as UnityEngine.Object) == null` 로 본다.
5. 예외는 **경계에서만** 다룬다 — 저장 파일 읽기(§12.4) · 데이터 에셋 검증(`OnValidate` · EditMode 테스트) · 플랫폼 API(광고 · 파일). 안으로 들어온 데이터는 유효하다고 가정하고 던지지 않는다.
6. `catch (Exception)` 일괄 처리 금지 — 구체 타입. catch 하면 반드시 로그(§13). 정리 코드의 빈 catch 만 허용.
7. **MonoBehaviour 메시지에서 시작하는 호출 경로에서는 예외를 던지지 않는다**(Core 순수 로직이라도 `Update` 에서 불리면 같은 경로다).
   — 왜: 예외가 나면 그 메시지의 나머지가 실행되지 않은 채 다음 프레임이 돈다 — 게임이 멈추지 않고 상태만 어긋난다.
8. `switch` 의 `default:` 는 `Debug.Assert(false, …)` + `Debug.LogError` + 안전값. 빠진 열거값은 EditMode 테스트가 열거값을 전부 돌려 잡는다(§15.3). `NotImplementedException` 은 에디터 도구 · 테스트 보조 코드에만.
9. 코드가 세운 가정마다 `Debug.Assert`(`UnityEngine.Debug`). 단언 안에 부작용 있는 식을 쓰지 않고, 릴리스 동작을 단언에 기대지 않는다.
   — 왜: `[Conditional("UNITY_ASSERTIONS")]` 라 그 심볼이 없는 빌드에서는 호출 자체가 빠진다(근거 4).

---

## 7. MonoBehaviour [LIFE · PERF]

1. 인스펙터 노출은 **`[SerializeField] private`** 만. public 필드 금지(저장 DTO 는 §12.1 이 예외). `[field: SerializeField]` 는 쓰지 않는다.
   — 왜: 자동 프로퍼티의 직렬화 이름은 C# 컴파일러가 프로퍼티 이름으로 만든 백킹 필드 이름이라, 프로퍼티 이름을 바꾸면 값이 끊긴다(C# 쪽 — 근거 표 밖, 세션 지식).
2. 메시지의 역할: `Awake` = 자기 초기화 · 컴포넌트 캐시 / `Start` = 다른 객체 참조 · 부트스트랩이 넘긴 값 사용 / `OnEnable` · `OnDisable` = 구독 · 해제 **짝** / `OnDestroy` = 마지막 정리. 주입된 참조가 필요한 구독은 참조를 받은 뒤에만 — `OnEnable` 은 참조가 있을 때(`_system != null`)만 구독하고, `Init` 은 참조를 넣은 뒤 같은 구독을 한 번 한다(풀 재활성화 때는 `OnEnable` 이 다시 구독한다). 부트스트랩은 Script Execution Order 로 가장 먼저 `Awake` 가 돌게 한다(작성 세션이 더한 규칙).
3. **서비스 접근**: 씬의 부트스트랩 하나(`GameBootstrap`)가 시스템을 만들고 생성자 · `Init` 으로 참조를 넘긴다. `Instance` 싱글톤 · `FindObjectOfType` · `FindAnyObjectByType` · `GameObject.Find` · `SendMessage` 로 찾지 않는다. 정적 접근점은 순수 유틸(수학 · 상수)과 로그 래퍼(§13.2)뿐 — 꼭 필요하면 이유를 주석 한 줄로 남기고, 둘을 넘기면 설계를 다시 본다(§0 ⑤).
4. 바깥과 닿는 것(광고 · 플랫폼 · 파일 · 시간)은 인터페이스(`I<이름>Service`) + 모의 구현(`Mock<이름>Service`). 게임 코드는 인터페이스만 보고, 기획서 8절 모의 광고가 첫 예다.
5. 이벤트는 C# `event Action<T>`. `UnityEvent` 는 인스펙터에서 연결해야 하는 UI 에만.
6. **적 · 투사체 · 코인마다 `Update` 를 두지 않는다.** 시스템 하나(`EnemySystem`)가 배열 · 리스트를 돈다.
   — 왜: 개체마다 들어오는 Unity 메시지는 네이티브 → 관리 호출이라 개체 수에 비례해 비싸다(근거 11 — 2015년 측정, 지금 값은 §11.1 로 잰다).
7. `FixedUpdate` 는 물리만. 게임 로직은 `Update` + `Time.deltaTime`(§10.1).
8. `GetComponent` · `Camera.main` · `transform` 체인은 `Awake` 에서 캐시. 루프 안에서 부르지 않는다.
9. `[RequireComponent]` · `[DisallowMultipleComponent]` 로 전제를 선언하고, 필수 참조는 `Awake` 에서 `Debug.Assert`. 커스텀 어트리뷰트는 `sealed` + `[AttributeUsage]` 명시.
10. static 필드 · static 이벤트는 되도록 두지 않는다. 두면 씬 로드로는 초기화되지 않으므로 자리는 둘 — 앱 · 플레이 시작은 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, 런 재시작 · 씬 전환은 부트스트랩(§9.6)(작성 세션이 더한 규칙).
    — 왜: 도메인 리로드를 끈 플레이 모드에서는 static 값과 static 이벤트 구독자가 플레이 세션 사이에도 남고, Unity 가 권하는 초기화 자리가 그 어트리뷰트다(근거 9).

---

## 8. 데이터 에셋 · 계약 [CONTRACT · ECON]

1. 밸런스 수치 · 사용자 문구 · 색 · 곡선은 코드가 아니라 **데이터 에셋**(ScriptableObject `*Def` · `*Curve`)에. 코드에 박힌 수치는 지적 대상(ECON).
2. 기획 데이터 표(`Defense.Design/데이터/` — P01 S1 에서 만든다)의 열 = 에셋 필드(이름 · 단위 · 순서, §1.3). **표에 없는 수치는 코드에도 없다.** 표와 에셋이 다르면 CONTRACT. 재화 · 영구 강화 · 해금(K5)은 고위험 — 바꾸면 보고에 전후 값.
3. 클래스에 `[CreateAssetMenu(menuName = "Defense/<영역>/<이름>")]`, 에셋은 `Assets/_Project/Data/<영역>/`.
4. **런타임에 에셋을 바꾸지 않는다** — 읽기 전용. 런 중 변하는 값은 런타임 객체(POCO)로 복사해 쓴다.
   — 왜: 에디터에서는 플레이 모드에 쓴 값도 에셋에 저장돼 다음 플레이에 남고, 플레이어에서는 읽기만 되어 두 환경의 동작이 달라진다(근거 6).
5. 필드에 `[Min]` · `[Range]` · `[Tooltip]`. 검증은 `OnValidate` 와 EditMode 테스트(모든 `*Def` 를 로드해 범위 · 표 대조, §15.3).
6. 에셋 참조는 인스펙터로 연결한다. `Resources.Load` · 경로 문자열 금지.

---

## 9. 풀 · 수명 주기 [LIFE]

1. 런 중 `Instantiate` · `Destroy` 금지 — 풀에서 꺼내고 돌려준다. 풀은 엔진 내장 `UnityEngine.Pool.ObjectPool<T>` 를 쓰고(직접 만들지 않는다) 런 시작 전에 미리 채운다.
2. **꺼낼 때 상태를 전부 초기화**(체력 · 속도 · 타이머 · 구독 · 트레일 · 팔레트 UV). 돌려준 객체의 참조를 들고 있지 않는다 — 다음 프레임에 다른 적이 된다.
3. 돌려주기는 `SetActive(false)` — 그 객체의 코루틴은 이때 멈춘다. 다른 객체가 이 객체를 대상으로 띄운 코루틴은 띄운 쪽이 `StopCoroutine` 한다(§14.3).
4. 이벤트 구독은 `OnEnable` · `OnDisable` 짝(§7.2). 람다 구독은 해제할 수 없으니 메서드 그룹으로 구독한다.
5. `IDisposable` · 파이널라이저는 `NativeArray` · 파일 스트림처럼 실제 자원을 쥘 때만. MonoBehaviour 의 정리 지점은 `OnDisable` · `OnDestroy` 다.
6. 씬 전환 · 재시작의 초기화 순서(정적 상태 → 풀 → 시간 배율 → 시스템)는 부트스트랩 한 곳에.

---

## 10. 시간 [TIME]

1. 게임 로직은 **`Time.deltaTime`**(일시정지 · 배속 반영). UI 연출 · 메뉴만 `Time.unscaledDeltaTime`.
2. 배속 · 일시정지는 `Time.timeScale` 을 바꾸는 곳 하나(`GameClock`). 흩어 놓지 않는다.
3. 프레임 수에 기대는 로직 금지(「10 프레임 뒤」). 시간 · 거리로 쓴다.
4. 코루틴 대기 `WaitForSeconds` 는 캐시해 재사용. `WaitForSecondsRealtime` 은 UI 만.
5. 누적 타이머를 쓴다. `DateTime.Now` 는 저장 기록(§12)에만, 로직에는 쓰지 않는다(작성 세션이 더한 규칙 — D-07 비고).

---

## 11. 성능 [PERF]

1. **매 프레임 할당 0** 이 목표 — 프로파일러 GC Alloc 열로 확인한다. 금지 목록은 §5.7. 성능은 추측하지 않고 프로파일러 수치로 말한다.
2. 문자열은 UI 갱신 때만 만들고, 숫자 표시는 값이 바뀔 때만 갱신한다.
3. 물리 질의는 `NonAlloc` 계열 + 레이어 마스크. 콜라이더를 껐다 켜기보다 레이어로 거른다.
4. 개체별 색 · 변형은 팔레트 텍스처 UV(기획서 9절)나 머티리얼 변형 몇 개로. `renderer.material` 인스턴스화와 `MaterialPropertyBlock` 은 쓰지 않는다(검토서 4.1 과 반대 방향 — 작성 세션이 더한 규칙). 같은 메시 수백 개는 `Graphics.RenderMeshInstanced` 를 검토.
   — 왜: URP 의 SRP Batcher 는 `MaterialPropertyBlock` 을 붙인 렌더러를 배칭에서 뺀다(근거 8).
5. 목표는 적 300 · 중급 Android 60 fps(P01 NOTES — 측정 전 목표, 실측은 `DEF-M04`).

---

## 12. 저장 · 직렬화 [SAVE]

1. 저장 DTO 는 **`[Serializable]` 클래스 + public 필드(camelCase)**, 생성자 · 로직 없는 자료 클래스. 프로퍼티 · `Dictionary` · 다형성 · 다차원 배열 · 가변 배열(`int[][]`) · 중첩 컨테이너(`List<List<T>>`) 없음.
   — 왜: Unity 직렬화(JsonUtility · 인스펙터)는 필드만 보고 `Dictionary` · 다차원 · 가변 배열 · 중첩 컨테이너를 지원하지 않는다(근거 1). 어기면 컴파일은 되지만 값이 조용히 빠진다.
2. 라이브러리는 **`JsonUtility`**(패키지 0). `Dictionary` · 다형성이 필요해지면 보류 항목으로 `com.unity.nuget.newtonsoft-json` 을 재검토(D-06).
3. **포맷 버전 필드 `version` 필수.** 필드를 바꾸면 버전을 올리고 이전 버전 변환 함수 + 변환 테스트(§15.3). 기존 저장 파일을 못 읽게 되는 변경은 고위험.
4. JSON 한 파일(`Application.persistentDataPath`). 쓰기는 임시 파일에 쓴 뒤 바꿔치기, 이전 파일은 `.bak`. 읽기 실패(예외 · 미래 버전 · 깨짐)는 `.bak` → 기본값 순서로 복구하고 로그. 파일은 작으므로 읽기 · 쓰기는 동기(§14.1).
5. 저장 시점: 정산 · 강화 구매 · 해금 · 설정 변경 · 앱 백그라운드(`OnApplicationPause(true)`). 매 프레임 저장 금지. WebGL 의 저장 시점과 `persistentDataPath` 영속은 `DEF-M05` 에서 실측해 이 절에 적는다.
6. 저장 I/O 는 **`SaveSystem` 하나만** 호출한다. 다른 코드는 DTO 를 주고받을 뿐 파일을 모른다. `PlayerPrefs` 는 쓰지 않는다 — 사용자 설정까지 저장 파일에(기획서 6.1).
   — 왜: 한 파일이면 백업 · 복구 · 버전 관리가 한 곳이다.
7. 저장 데이터 변조 방지(암호화)는 범위 밖 — 오프라인 단일 플레이. 비밀값 · 서명 자료는 git 밖(루트 `CLAUDE.md`).

---

## 13. 로깅 [PERF]

1. 로그는 **상태 전이에만**: 런 시작 · 웨이브 · 보스 · 정산 · 저장 · 씬 전환 · 광고 자리. 매 프레임 · 적 · 투사체 · 풀 단위 로그 금지.
   — 왜: 로그마다 문자열이 만들어지고 스택 추적이 붙는데, 스택 추적 해석은 비용이 큰 작업이다(근거 10).
2. 메시지는 한국어, 앞에 `[태그]`(`[Wave]` · `[Save]`). 개발 전용 로그는 Core 의 얇은 정적 래퍼 `GameLog.Dev(...)`(`[Conditional("UNITY_EDITOR")]` · `[Conditional("DEVELOPMENT_BUILD")]`)로 — 릴리스에서는 호출과 인자 식이 함께 빠진다. 문자열 보간은 래퍼 인자에서만.
3. 레벨은 Unity 의 셋: `Log`(전이) · `LogWarning`(복구한 이상) · `LogError`(버그 — 게임은 계속). 예외는 `LogException(ex)`.
4. 메서드 진입 · 종료 로그, 클래스별 로그 상수는 쓰지 않는다. 릴리스 빌드의 스택 추적은 `None`(근거 10).
5. 비밀값 · 기기 식별자는 로그에 찍지 않는다.

---

## 14. 비동기 · 코루틴 [TIME · LIFE]

1. 프레임에 걸친 연출 · 대기는 **코루틴**. 저장 파일은 작아 동기로 읽고 쓴다(§12.4). SDK 콜백(광고)처럼 완료를 기다려야 하는 것은 Unity 6 **`Awaitable`** 로 감싼다(`AwaitableCompletionSource`). 우리가 `System.Threading.Tasks.Task` · UniTask 를 만들지 않는다(D-07). SDK 가 `Task` 를 주면 `await` 는 허용(작성 세션이 더한 예외).
   — 왜: `Awaitable` 은 Unity 가 플레이어 루프에 맞춰 제공하는 비동기 타입이고 패키지가 필요 없다(근거 5).
2. `async void` 금지 → `async Awaitable`(이벤트 핸들러만 예외).
3. 코루틴은 소유 GameObject 가 비활성화되거나 파괴되면 멈추지만 **`enabled = false` 로는 멈추지 않는다**(근거 5). 컴포넌트만 끄는 경우는 `OnDisable` 에서 `StopAllCoroutines()`. 다른 객체가 이 객체를 대상으로 띄운 코루틴은 띄운 쪽이 `StopCoroutine` 한다.
4. `await` 에는 `destroyCancellationToken` 을 넘긴다(작성 세션이 더한 규칙) — 취소되면 `OperationCanceledException` 으로 돌아오므로 그것만 잡아 조용히 끝낸다(§6.6 로그 규칙의 예외). 토큰이 없는 경로는 돌아온 뒤 Unity 객체를 만지기 전에 자신이 파괴됐는지(`this == null`) 본다. 씬 전환 뒤 돌아오는 결과는 버린다.
5. Unity API 는 메인 스레드에서만. 스레드 · 잡 시스템은 이 게임 범위 밖(필요하면 보류 항목으로).
6. `StartCoroutine("문자열")` 금지 — 메서드 참조로(작성 세션이 더한 규칙 — D-07 비고).

---

## 15. 테스트 [TEST]

1. 순수 로직(저장 변환 · 곡선 평가 · 스탯 계산 · 추첨 — Core 든 Game 이든)은 MonoBehaviour 에 의존하지 않는 POCO 로 두어 EditMode 로 검사한다.
   — 왜: EditMode 는 열린 에디터에서 약 0.3초 · batchmode 약 20초이고, PlayMode 는 씬이 필요하다(`CLAUDE.md` 「결정론 게이트」 실측).
2. 테스트 이름은 `메서드_상태_기대`(`Convert_V1Save_ProducesV2Fields`). 한 테스트에 한 가지 뜻.
3. 필수 테스트: 데이터 에셋 검증(모든 `*Def` 로드 · 범위 · 표 대조, §8.5) · 저장 포맷 변환(§12.3) · 열거값 전수(§6.8).
4. PlayMode 스모크(자동 선택 + 가속 한 판)는 런 흐름을 바꿨을 때.
5. 테스트 어셈블리는 코드 어셈블리마다 짝(이름 · 위치는 `CLAUDE.md` 「코딩」 — Core 의 것은 코어와 같이 복사된다). `internal` 은 각 어셈블리가 자기 테스트 어셈블리에만 `[assembly: InternalsVisibleTo]` 로 연다 — Core 의 IVT 에 게임 이름이 들어가지 않고(§1.1), 테스트 때문에 public 으로 올리지 않는다.
6. 새 동작에 맞춰 단언만 바꾼 테스트는 지적 대상. 실제 연결(wiring)도 테스트에 넣는다.

---

## 16. 에디터 코드 · 빌드 · 플랫폼 [BUILD]

1. 에디터 전용 코드(커스텀 인스펙터 · 메뉴 · 임포터)는 `Editor/` 폴더 + 에디터 asmdef(`includePlatforms: Editor`). `#if UNITY_EDITOR` 는 런타임 클래스 안의 보조 한두 줄에만.
2. 플랫폼 분기(`UNITY_ANDROID` · `UNITY_WEBGL` · `UNITY_IOS`)는 플랫폼 서비스 클래스 한 곳(`IPlatformService` 구현, §7.4). 게임 로직에 흩뿌리지 않는다.
3. 게이트(컴파일 · 테스트 · 빌드)와 명령은 `CLAUDE.md` 「결정론 게이트」 한 곳. `-warnaserror` 는 쓰지 않는다(패키지 경고까지 오류가 된다) — 대신 새 경고 0.
4. 패키지 · 외부 코드 · 라이선스 · 장부 규칙은 `CLAUDE.md` 「코딩」. `ProjectSettings/` · 빌드 프로필 변경은 기능 커밋과 분리한다.
5. 조건부 컴파일 블록에는 이유 주석.

---

## 17. 에셋 이름

접두 없이 PascalCase 하나. 종류는 폴더와 접미가 말한다. 띄어쓰기 · 한글 · 특수문자 없음(명령줄 · WebGL 경로). 변형은 `_접미` 하나만. 기준 폴더는 `Assets/_Project/`(목록은 `CLAUDE.md` 「코딩」).

| 종류 | 폴더 | 예 |
|---|---|---|
| 데이터 에셋 | `Data/<영역>/` | `Data/Weapons/Lantern.asset`(클래스 `WeaponDef`) · `Data/Curves/Night.asset` |
| 프리팹 | `Defense/<영역>/` | `EnemyGrunt.prefab` · 변형 `EnemyGrunt_Elite.prefab` |
| 씬 | `Scenes/` | `Boot.unity` · `Island.unity` · `Night.unity` |
| 모델 · 머티리얼 · 텍스처 | `Art/<영역>/` | `TowerBase.fbx` · `TowerBase.mat` · `Palette.png` — 같은 대상은 같은 이름 |
| 애니메이션 | `Art/<영역>/` | `EnemyGrunt_Walk.anim` |
| 오디오 | `Audio/Sfx/` · `Audio/Music/` | `Hit.wav` · `Night.ogg` |

1. 외부 에셋은 원본 이름을 유지하고 `ASSETS.md` 에 적는다(출처 추적).

---

## 18. UI · 입력 [UI · INPUT]

1. UI 프레임워크(uGUI 대 UI Toolkit)는 `DEF-M01` 전에 정한다 — 그때 이 절을 채운다(패널 상태 · 버튼 조건 · 연출 순서 · 세이프 영역 · 해상도 · 화면 방향). 지금은 규칙이 없다.
2. 지금 정해진 것: 입력은 Input System 패키지(Unity Universal 3D 템플릿 기본) · 세로 · 한 손(기획서 3절). 사용자 문구는 문자열 표(데이터 에셋)에서 — 코드 리터럴 금지.

---

## 19. 주석 · 문서

1. public 타입 · 메서드에 `<summary>`(한국어). `<param>` · `<returns>` 는 이름만으로 모를 때만. Unity 메시지 메서드 · 테스트 메서드는 생략.
2. 람다 · 코루틴 블록은 역할 한 줄. 「무엇」이 아니라 「왜」를 쓴다.
3. 인라인 변경 표식(`@id 날짜`) · 파일 헤더 이력은 없다(§2.4).
4. `// TODO(H-nn 또는 DEF-Mnn): …` — Planning 보류 항목 · 마일스톤 번호와 잇는다. 번호 없는 TODO 는 남기지 않는다.
5. 쓰지 말아야 할 메서드는 `[Obsolete("대신 X")]`.

---

## 근거

Unity 쪽은 Unity 6.3 매뉴얼 · 스크립트 레퍼런스, C# 쪽은 Microsoft Learn. 전부 2026-10-30 web-fetch 에이전트가 읽은 것(△)이고 사람이 직접 연 것은 없다.

| # | 사실 | 출처 |
|---|---|---|
| 1 | Unity 6000.3 은 C# 9.0, `init` 세터 미지원, 레코드는 직렬화 불가. 직렬화는 public 또는 `[SerializeField]` 필드만(static · const · readonly 제외), 프로퍼티 · 다차원 · 가변 배열 · `Dictionary` · 중첩 컨테이너 미지원. JsonUtility 는 필드만 | [C# compiler and language version reference](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/overview-of-dot-net-in-unity/csharp-compiler) · [Serialization rules](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/rules) · [JSON serialization](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/json-serialization) |
| 2 | `UnityEngine.Object` 의 `==` 는 관리 참조와 네이티브 객체 포인터를 함께 검사한다 | [Object.operator ==](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/object/op-equality) |
| 3 | `x is null` 은 컴파일러가 사용자 정의 `==` 를 부르지 않도록 보장한다. `a?.x` 는 `a` 가 null 이면 null, `??` 는 왼쪽이 null 일 때만 오른쪽 | [Patterns — constant pattern](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns) · [Member access — `?.`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators) · [`??`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator) |
| 4 | `Debug.Assert` 는 `[Conditional("UNITY_ASSERTIONS")]` — 심볼이 없으면 호출이 빠진다 | [Debug.Assert](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/debug/assert) |
| 5 | 코루틴은 `IEnumerator` 반환. GameObject 비활성화 · 파괴로 멈추고 `enabled = false` 로는 멈추지 않는다. 비동기는 Unity 자체 `Awaitable` | [Coroutines](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/coroutines-section/coroutines) · [Async/await support](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/programming-distribute-work-threads/async-await-support) |
| 6 | 에디터에서는 편집 · 플레이 모드 모두 ScriptableObject 에 저장할 수 있고, 플레이어에서는 읽기만 된다 | [ScriptableObject](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/get-started/fundamental-unity-types/class-scriptable-object) |
| 7 | 파일에 클래스가 여럿이면 파일 이름과 같은 클래스를 고르고, partial MonoBehaviour 는 파일 이름이 같은 파일만 컴포넌트가 되며, 못 고르면 경고 | [Naming scripts](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/get-started/naming-scripts) |
| 8 | 렌더러에 `MaterialPropertyBlock` 을 붙이면 SRP Batcher 와 호환되지 않는다. 같은 메시 다수는 `Graphics.RenderMeshInstanced` | [SRP Batcher incompatible](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/in-urp/reduce-draw-calls-urp/srpbatcher/srpbatcher-incompatible) |
| 9 | 도메인 리로드를 끄면 static 필드 값과 static 이벤트 구독자가 플레이 세션 사이에 남는다. 초기화는 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` | [Domain reloading](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/domain-reloading) |
| 10 | 콘솔 메시지 · 로그에 스택 추적이 붙고, 스택 추적 해석은 자원을 많이 쓰는 작업이다. 사용자 배포에는 끈다 | [Stack trace logging](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/editor-windows-views-reference/console-window/stack-trace) |
| 11 | 개체마다의 `Update` 는 네이티브 C++ 에서 관리 C# 으로 들어오는 호출이라 비용이 있다(2015년 Unity 5.2 측정 — 지금 값은 직접 잰다) | [10000 Update() calls](https://unity.com/blog/engine-platform/10000-update-calls)(Unity 블로그) |

원문(EasyCheck 판)과 절별 처리 · 결정 경위는 Planning `z.completed/P00_환경_체계_로깅/01_검토/검토서_20261030_코딩표준.md` · `결정기록.md` D-06 · D-07.
