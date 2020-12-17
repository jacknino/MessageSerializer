using System.Security.Cryptography;

namespace MessageSerializer
{
    public class CalculatorAuthenticationSha256 : CalculatorAuthenticationHashBase<SHA256>
    {
        protected override SHA256 CreateHashAlgorithm()
        {
            return SHA256.Create();
        }
    }
}
