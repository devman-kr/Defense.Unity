using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Defense.Tests.EditMode
{
    /// <summary>
    /// 결정론 게이트가 도는지 보이는 스모크 — 테스트 어셈블리와 Core 어셈블리가 컴파일돼 에디터에 로드되고,
    /// Core 의 InternalsVisibleTo 가 코딩 표준 §15.5 의 이름을 가리킨다. asmdef 참조는 검사하지 않는다(Core 에 아직 타입이 없다).
    /// </summary>
    public class GateSmokeTests
    {
        private const string CORE_ASSEMBLY = "Devman.Core";
        private const string CORE_TEST_ASSEMBLY = "Devman.Core.Tests.EditMode";

        /// <summary>Core 어셈블리가 이름 그대로 로드된다.</summary>
        [Test]
        public void CoreAssembly_IsLoadable()
        {
            Assembly core = Assembly.Load(CORE_ASSEMBLY);
            Assert.That(core.GetName().Name, Is.EqualTo(CORE_ASSEMBLY));
        }

        /// <summary>Core 의 internal 은 Core 자기 테스트 어셈블리에만 열린다 — 게임 이름이 든 어셈블리에 열리면 복사(G-07)가 깨진다.</summary>
        [Test]
        public void CoreAssembly_OpensInternalsOnlyToOwnTestAssembly()
        {
            Assembly core = Assembly.Load(CORE_ASSEMBLY);
            object[] attributes = core.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false);
            Assert.That(attributes.Length, Is.EqualTo(1));
            InternalsVisibleToAttribute ivt = (InternalsVisibleToAttribute)attributes[0];
            Assert.That(ivt.AssemblyName, Is.EqualTo(CORE_TEST_ASSEMBLY));
        }
    }
}
