using System;
using System.Linq;

namespace MessageSerializer
{
    public class CalculatorLength<TCalculatedResultType> : CalculatorBase<TCalculatedResultType>
    {
        protected int _nonVaryingLengthPartOfMessageLength;

        //public CalculatorLength(string name, MessageSerializedClassInfo classInfo)
        //    : base(name, classInfo)
        //{
        //    _nonVaryingLengthPartOfMessageLength = 0;
        //    foreach (int propertyIndex in _calculatedFieldInfo.IncludedPropertyIndexes)
        //    {
        //        MessageSerializedPropertyInfo propertyInfo = classInfo.Properties[propertyIndex];
        //        if (!propertyInfo.IsVariableLength)
        //            _nonVaryingLengthPartOfMessageLength += propertyInfo.MessagePropertyAttribute.Length;
        //    }
        //}

        public int GetVaryingLengthFieldLength(int lengthFieldValue, params int[] otherLengths)
        {
            return lengthFieldValue - _nonVaryingLengthPartOfMessageLength - otherLengths.Sum();
        }

        public override void SetFieldInfo(string name, MessageSerializedClassInfo classInfo)
        {
            base.SetFieldInfo(name, classInfo);

            _nonVaryingLengthPartOfMessageLength = 0;
            foreach (int propertyIndex in _calculatedFieldInfo.IncludedPropertyIndexes)
            {
                MessageSerializedPropertyInfo propertyInfo = classInfo.Properties[propertyIndex];
                if (!propertyInfo.IsVariableLength)
                    _nonVaryingLengthPartOfMessageLength += propertyInfo.MessagePropertyAttribute.Length;
            }
        }

        public override TCalculatedResultType Calculate(params byte[][] arrays)
        {
            //int length = 0;
            //foreach (byte[] array in arrays)
            //{
            //    length += array.Length;
            //}

            int length = arrays.Sum(item => item.Length);

            // TODO: There has to be a better way to figure this out
            return (TCalculatedResultType)Convert.ChangeType(length, typeof(TCalculatedResultType));
        }
    }
}
