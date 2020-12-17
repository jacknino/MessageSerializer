using System;
using System.Text;

namespace MessageSerializer
{
    public class TypeSerializerString : TypeSerializerBase<string>
    {
        public TypeSerializerString(MessageSerializedPropertyInfo propertyInfo) 
            : base(propertyInfo)
        {
        }

        public override byte[] Serialize(string value)
        {
            return GetByteArrayFromString(value);
        }

        public override string Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            // TODO: In SerializerClassGeneration need to figure out the fieldLengthExpression
            if (length == -1)
                length = GetLength();

            string returnValue = GetStringFromByteArray(bytes, currentArrayIndex, length);
            currentArrayIndex += length;
            return returnValue;
        }

        protected byte[] GetByteArrayFromString(string source)
        {
            // What we want to do:
            // ArrayLength > 0 and variableLength == false, conversion should be for all bytes up to arrayLength.
            //    If prePad, add enough padCharacters at front to make arrayLength, otherwise will just have nulls at the end
            // ArrayLength == 0 and variableLength == false - this is an exception
            // ArrayLength > 0 and variableLength == true, ArrayLength is now the max characters that can be converted.
            // ArrayLength == 0 and variableLength == true, just convert all the characters
            // ArrayLength == 0 and variableLength == true and MinLength > 0 - make sure the length is at least MaxLength long
            // ArrayLength == 0 and variableLength == true and MaxLength > 0 - make sure the length is no longer than MaxLength long
            // Exceptions:
            //   If MinLength and MaxLength > 0 but MinLength > MaxLength
            bool variableLength = _propertyInfo.MessagePropertyAttribute.VariableLength;
            int outputLength = source.Length;
            if (variableLength)
            {
                int minLength = _propertyInfo.MessagePropertyAttribute.MinLength;
                int maxLength = _propertyInfo.MessagePropertyAttribute.MaxLength;
                if (minLength > 0 && maxLength > 0 && minLength > maxLength)
                    throw new Exception($"For the {_propertyInfo.PropertyInfo.Name} property, tried to convert {source} to a byte array but MinLength was {minLength} and MaxLength was {maxLength} which is not valid");

                if (minLength > 0 && minLength > outputLength)
                    outputLength = minLength;

                if (maxLength > 0 && maxLength < outputLength)
                    outputLength = maxLength;
            }
            else
            {
                if (_propertyInfo.MessagePropertyAttribute.Length == 0)
                    throw new Exception($"For the {_propertyInfo.PropertyInfo.Name} property, tried to convert {source} to a byte array but arrayLength was 0 and variableLength was false so the value can't be handled");

                outputLength = _propertyInfo.MessagePropertyAttribute.Length;
            }

            //if (variableLength && (arrayLength == 0 || source.Length <= arrayLength))
            //{
            //    return Encoding.ASCII.GetBytes(source);
            //}

            if (_propertyInfo.MessagePropertyAttribute.Prepad && source.Length < outputLength)
                source = new string(_propertyInfo.MessagePropertyAttribute.PrepadCharacter, outputLength - source.Length) + source;

            byte[] resultArray = new byte[outputLength];
            Encoding.ASCII.GetBytes(source, 0, Math.Min(outputLength, source.Length), resultArray, 0);
            return resultArray;
        }

        protected string GetStringFromByteArray(byte[] sourceArray, int startIndex = 0, int count = -1)
        {
            // if count == -1 we want to go from startIndex to the end of the array
            if (count == -1)
                count = sourceArray.Length - startIndex;

            // NOTE: C# strings can happily contain null characters
            // Often when they are displayed (like in a UI) the display will stop at the null
            // This can cause confusing problems because two strings that look the same aren't
            // So we need to make sure that we stop at the null if there is one
            int nullIndex = Array.IndexOf<byte>(sourceArray, 0, startIndex, count);
            if (nullIndex != -1)
            {
                // So if we have a 20 byte array and we are looking at startIndex 10 and count of 10
                // if we get say 17 back from this, this means that rather than count being 10 we only
                // want it to be 7 (elements 10-16).  The nullIndex returned is the index from 0 of the
                // the array, not the index from startIndex.  So (nullIndex - startIndex) gives us how
                // how many characters we want to exclude the nulls
                count = nullIndex - startIndex;
            }

            return Encoding.ASCII.GetString(sourceArray, startIndex, count);
        }
    }
}
