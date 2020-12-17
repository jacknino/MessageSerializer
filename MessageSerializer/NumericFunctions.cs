using System;

namespace MessageSerializer
{
    public class NumericFunctions
    {
        public static bool IsIntegerType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    //case TypeCode.Decimal:
                    //case TypeCode.Double:
                    //case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPowerOfTwo(ulong value)
        {
            return (value != 0) && ((value & (value - 1)) == 0);
        }
    }
}
