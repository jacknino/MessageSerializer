using System.Security.Cryptography;

namespace MessageSerializer
{
    public class CalculatorAuthenticationSha512 : CalculatorAuthenticationHashBase<SHA512>
    {
        protected override SHA512 CreateHashAlgorithm()
        {
            return SHA512.Create();
        }
    }
}
