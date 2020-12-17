using System.Security.Cryptography;

namespace MessageSerializer
{
    public class CalculatorAuthenticationSha1 : CalculatorAuthenticationHashBase<SHA1>
    {
        protected override SHA1 CreateHashAlgorithm()
        {
            return SHA1.Create();
        }
    }
}
