using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestEndiannessRequiresReversal
    {
        struct CurrentRequiredExpectedResult
        {
            public Endiannesses Current { get; set; }
            public Endiannesses Required { get; set; }
            public bool RequiresReversal { get; set; }
        }

        private void Test(CurrentRequiredExpectedResult parameters)
        {
            bool actualResult = ArrayOps.EndiannessRequiresReversal(parameters.Current, parameters.Required);
            Assert.That(actualResult, Is.EqualTo(parameters.RequiresReversal), $"Current: {parameters.Current}, Required: {parameters.Required}, Expected: {parameters.RequiresReversal}, Actual: {actualResult}");
        }

        [Test]
        public void Test()
        {
            // Current   | Required | BitConverter.IsLittleEndian | EndiannessRequiresReversal
            // System    | System   | N/A                         | false
            // System    | Little   | true                        | false
            // System    | Little   | false                       | true
            // System    | Big      | true                        | true
            // System    | Big      | false                       | false
            // Little    | System   | true                        | false
            // Little    | System   | false                       | true
            // Little    | Little   | N/A                         | false
            // Little    | Big      | N/A                         | true
            // Big       | System   | true                        | true
            // Big       | System   | false                       | false
            // Big       | Little   | N/A                         | true
            // Big       | Big      | N/A                         | false

            bool systemIsLittleEndian = ArrayOps.SystemEndiannessIsLittleEndian();
            List<CurrentRequiredExpectedResult> testScenarios = new List<CurrentRequiredExpectedResult>()
            {
                new CurrentRequiredExpectedResult() { Current = Endiannesses.System, Required = Endiannesses.System, RequiresReversal = false },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.System, Required = Endiannesses.Little, RequiresReversal = !systemIsLittleEndian },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.System, Required = Endiannesses.Big, RequiresReversal = systemIsLittleEndian },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Little, Required = Endiannesses.System, RequiresReversal = !systemIsLittleEndian },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Little, Required = Endiannesses.Little, RequiresReversal = false },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Little, Required = Endiannesses.Big, RequiresReversal = true },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Big, Required = Endiannesses.System, RequiresReversal = systemIsLittleEndian },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Big, Required = Endiannesses.Little, RequiresReversal = true },
                new CurrentRequiredExpectedResult() { Current = Endiannesses.Big, Required = Endiannesses.Big, RequiresReversal = false }
            };

            foreach(CurrentRequiredExpectedResult parameters in testScenarios)
                Test(parameters);
        }
    }
}
