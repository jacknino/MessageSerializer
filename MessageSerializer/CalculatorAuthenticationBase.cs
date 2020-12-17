namespace MessageSerializer
{
    public abstract class CalculatorAuthenticationBase<TAuthenticationResultType> : CalculatorBase<TAuthenticationResultType>
    {
        // We need to specify the type that the authentication result is (ushort, ulong, byte[20], etc.)
        // We need a Calculate that takes an array of bytes or a series of arrays of bytes and computes the authentication value and returns it
        // We need a Verify that takes an array of bytes or a series of arrays of bytes and verifies the authentication value with a field that matches the type
        //public abstract TAuthenticationResultType Compute(params byte[][] arrays);
        //public abstract bool Verify(TAuthenticationResultType receivedValue, TAuthenticationResultType computedValue);

        public static TAuthenticationResultType Compute<TAuthenticationClass>(params byte[][] arrays) where TAuthenticationClass : CalculatorAuthenticationBase<TAuthenticationResultType>, new()
        {
            TAuthenticationClass authenticator = new TAuthenticationClass();
            return authenticator.Calculate(arrays);
        }

        public static bool Verify<TAuthenticationClass>(TAuthenticationResultType receivedValue, TAuthenticationResultType computedValue) where TAuthenticationClass : CalculatorAuthenticationBase<TAuthenticationResultType>, new()
        {
            TAuthenticationClass authenticator = new TAuthenticationClass();
            DeserializeStatus status = new DeserializeStatus();
            return authenticator.Verify(status, receivedValue, computedValue);
        }
    }
}
