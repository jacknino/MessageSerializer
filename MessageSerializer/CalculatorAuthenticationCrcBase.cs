namespace MessageSerializer
{
    public abstract class CalculatorAuthenticationCrcBase<T> : CalculatorAuthenticationBase<T>
    {
        public override T Calculate(params byte[][] arrays)
        {
            T crc = SetInitialValue();
            foreach (byte[] array in arrays)
            {
                crc = AddByteArrayToAuthenticationValue(crc, array);
            }

            return GetFinalValue(crc);
        }

        protected virtual T AddByteArrayToAuthenticationValue(T crc, byte[] sourceArray, int startIndex = 0, int count = -1)
        {
            if (count == -1)
                count = sourceArray.Length - startIndex;

            for (int index = startIndex; index < startIndex + count; index++)
            {
                crc = AddByteToCrc(crc, sourceArray[index]);
            }

            return crc;
        }

        protected virtual T SetInitialValue()
        {
            return default(T);
        }

        protected virtual T GetFinalValue(T crc)
        {
            return crc;
        }

        protected abstract T AddByteToCrc(T crc, byte valueToAdd);
    }
}
