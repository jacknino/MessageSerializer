using System;
using System.Collections;
using System.Collections.Generic;

namespace MessageSerializer
{
    public abstract class SerializerBase
    {
        public abstract byte[] Serialize(object objectToSerialize);
        public abstract object Deserialize(byte[] bytes, ref int currentArrayIndex, DeserializeStatus status);
        public abstract string ToString(object objectToPrint, int indentLevel, ToStringFormatProperties formatProperties);

        public virtual DeserializeResults<T> Deserialize<T>(byte[] bytes, ref int currentArrayIndex, bool suppressExceptions) where T : class, IMessageSerializable
        {
            DeserializeResults<T> results = new DeserializeResults<T>();
            try
            {
                results.Object = (T)Deserialize(bytes, ref currentArrayIndex, results.Status);
            }
            catch (Exception ex)
            {
                if (suppressExceptions)
                {
                    results.Status.Exception = ex;
                }
                else
                {
                    throw;
                }
            }

            return results;
        }

        // Called by the generated class when verifying a calculated field
        protected virtual TResultType Deserialize<TResultType>(TypeSerializerBase<TResultType> serializer, byte[] bytes, ref int currentArrayIndex, int length, DeserializeStatus status, params List<byte[]>[] arrays)
        {
            int startArrayIndex = currentArrayIndex;
            TResultType result = serializer.Deserialize(bytes, ref currentArrayIndex, length, ref status);
            int endArrayIndex = currentArrayIndex;

            byte[] array = ArrayOps.GetSubArray(bytes, startArrayIndex, endArrayIndex - startArrayIndex);
            foreach(List<byte[]> currentArray in arrays)
                currentArray.Add(array);

            return result;
        }

        // Called by the generated class when verifying a calculated field
        protected virtual TListType DeserializeList<TListType, TResultType>(TypeSerializerBase<TResultType> serializer, byte[] bytes, ref int currentArrayIndex, int length, DeserializeStatus status, params List<byte[]>[] arrays)
            where TListType : IList, IEnumerable<TResultType>, new()
        {
            int startArrayIndex = currentArrayIndex;
            TListType result = serializer.DeserializeList<TListType>(bytes, ref currentArrayIndex, length, ref status);
            int endArrayIndex = currentArrayIndex;

            byte[] array = ArrayOps.GetSubArray(bytes, startArrayIndex, endArrayIndex - startArrayIndex);
            foreach (List<byte[]> currentArray in arrays)
                currentArray.Add(array);

            return result;
        }

        // Called by the generated class when creating a calculator
        protected TCalculatorType CreateCalculator<TCalculatorType, TResultType>(string name, MessageSerializedClassInfo classInfo)
            where TCalculatorType : CalculatorBase<TResultType>, new()
        {
            TCalculatorType calculator = new TCalculatorType();
            calculator.SetFieldInfo(name, classInfo);

            return calculator;
        }

        // Called by the generated classes when serializing individual fields
        protected virtual byte[] AddSerializeResultToMessageLength(byte[] serializedBytes, ref int messageLength, bool excludeFromLength)
        {
            if (!excludeFromLength)
                messageLength = messageLength + serializedBytes.Length;
            return serializedBytes;
        }

        // Called by the generated classes when need to figure out an authentication value
        protected virtual TResultType CalculateAuthenticationValue<TAuthenticationType, TResultType>(params byte[][] arrays) where TAuthenticationType : CalculatorAuthenticationBase<TResultType>, new()
        {
            TAuthenticationType authenticationClass = new TAuthenticationType();
            return authenticationClass.Calculate(arrays);
        }
    }
}
