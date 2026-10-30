using System.Reflection;
using NUnit.Framework;

namespace Defense.Tests.EditMode
{
    /// <summary>
    /// 결정론 게이트가 도는지 보이는 스모크 — 테스트 어셈블리와 Core 어셈블리가 컴파일돼 에디터에 로드된다.
    /// asmdef 참조 · InternalsVisibleTo 이름은 검사하지 않는다(Core 에 아직 타입이 없다).
    /// </summary>
    public class GateSmokeTests
    {
        /// <summary>Core 어셈블리가 이름 그대로 로드된다.</summary>
        [Test]
        public void CoreAssembly_IsLoadable()
        {
            Assembly core = Assembly.Load("Devman.Core");
            Assert.That(core.GetName().Name, Is.EqualTo("Devman.Core"));
        }
    }
}
