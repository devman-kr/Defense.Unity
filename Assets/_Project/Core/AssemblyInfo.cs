using System.Runtime.CompilerServices;

// Core 의 internal 은 Core 자기 테스트 어셈블리에만 연다(코딩 표준 §15.5). 게임 이름이 들어가지 않아 다음 게임이 그대로 복사한다(G-07).
// 어셈블리는 Core 에 첫 타입이 생기는 DEF-M01 에서 만든다 — 없는 어셈블리를 가리키는 IVT 는 무해하다.
[assembly: InternalsVisibleTo("Devman.Core.Tests.EditMode")]
