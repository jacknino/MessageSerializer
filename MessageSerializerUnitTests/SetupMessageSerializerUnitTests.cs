using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [SetUpFixture]
    public class SetupMessageSerializerUnitTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SerializerClassGeneration.WriteCodeAndDebugInfoToDisk = true;
        }
    }
}
