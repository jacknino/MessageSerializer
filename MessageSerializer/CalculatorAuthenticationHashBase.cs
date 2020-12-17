using System;
using System.Linq;
using System.Security.Cryptography;

namespace MessageSerializer
{
    public abstract class CalculatorAuthenticationHashBase<THashAlgorithm> : CalculatorAuthenticationBase<byte[]> where THashAlgorithm : HashAlgorithm
    {
        public override byte[] Calculate(params byte[][] arrays)
        {
            byte[] combinedArray = ArrayOps.Combine(arrays);

            using (THashAlgorithm hashAlgorithm = CreateHashAlgorithm())
            {
                byte[] bytes = hashAlgorithm.ComputeHash(combinedArray);
                return bytes;
            }
        }

        public override bool Verify(DeserializeStatus status, byte[] receivedValue, byte[] computedValue)
        {
            bool returnValue;
            if (receivedValue == null && computedValue == null)
                returnValue = true;
            else if (receivedValue == null || computedValue == null)
                returnValue = false;
            else
                returnValue = receivedValue.SequenceEqual(computedValue);

            if (!returnValue)
                AddFailedMessage(status, receivedValue, computedValue);

            return returnValue;
        }

        protected override string ToString(byte[] value)
        {
            return BitConverter.ToString(value);
        }

        protected abstract THashAlgorithm CreateHashAlgorithm();
    }
}
