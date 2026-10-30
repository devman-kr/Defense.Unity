using System.Reflection;
using NUnit.Framework;

namespace Defense.Tests.EditMode
{
    /// <summary>
    /// 결정론 게이트가 도는지 보이는 스모크 — 테스트 어셈블리가 컴파일되고 Core 어셈블리를 참조해 불러온다.
    /// </summary>
    public class GateSmokeTests
    {
        /// <summary>Core 어셈블리가 이름 그대로 로드된다.</summary>
        [Test]
        public void CoreAssembly_IsLoadable()
        {
            Assembly core = Assembly.Load("Defense.Core");
            Assert.That(core.GetName().Name, Is.EqualTo("Defense.Core"));
        }
    }
}
