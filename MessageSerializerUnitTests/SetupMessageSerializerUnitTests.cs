using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [SetUpFixture]
    public class SetupMessageSerializerUnitTests
    {
        [SetUp]
        public void Setup()
        {
            SerializerClassGeneration.WriteCodeAndDebugInfoToDisk = true;
        }
    }
}
