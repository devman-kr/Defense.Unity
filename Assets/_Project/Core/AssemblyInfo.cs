using System.Runtime.CompilerServices;

// 테스트 어셈블리가 Core 의 internal 멤버를 검사할 수 있게 연다.
[assembly: InternalsVisibleTo("Defense.Tests.EditMode")]
