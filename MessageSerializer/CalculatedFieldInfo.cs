using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageSerializer
{
    public class CalculatedFieldInfo
    {
        public string Name { get; protected set; }
        public MessageSerializedPropertyInfo CalculatorResultPropertyInfo { get; protected set; }
        public CalculatedFieldResultAttribute CalculatorResultAttribute { get; protected set; }
        public int CalculatedResultIndex { get; protected set; }
        public Type CalculatorType { get; protected set; }
        public SortedSet<int> IncludedPropertyIndexes { get; protected set; }

        public CalculatedFieldInfo(string name, List<MessageSerializedPropertyInfo> properties)
        {
            Name = name;

            CalculatorResultPropertyInfo = GetCalculatorPropertyInfo(properties, out var calculatedResultIndex);
            CalculatedResultIndex = calculatedResultIndex;
            CalculatorResultAttribute = GetApplicableCalculatedFieldAttribute<CalculatedFieldResultAttribute>(CalculatorResultPropertyInfo);
            CalculatorType = CalculatorResultAttribute.GetActualCalculatedType(CalculatorResultPropertyInfo.PropertyInfo);

            int startIndex = GetIndex(CalculatorResultPropertyInfo, properties, "Start", (calculatedFieldAttribute) => calculatedFieldAttribute.Start, (calculatedFieldResultAttribute) => calculatedFieldResultAttribute.DefaultStart);
            int endIndex = GetIndex(CalculatorResultPropertyInfo, properties, "End", (calculatedFieldAttribute) => calculatedFieldAttribute.End, (calculatedFieldResultAttribute) => calculatedFieldResultAttribute.DefaultEnd);
            IncludedPropertyIndexes = GetIncludedPropertyIndexes(startIndex, endIndex, properties);
        }

        public IEnumerable<int> GetAssociatedBlobLengthFieldIndexes(List<MessageSerializedPropertyInfo> fullPropertyList)
        {
            return IncludedPropertyIndexes.Where(includedPropertyIndex => fullPropertyList[includedPropertyIndex].MessagePropertyAttribute.BlobType == BlobTypes.Length);
        }

        protected MessageSerializedPropertyInfo GetCalculatorPropertyInfo(List<MessageSerializedPropertyInfo> properties, out int calculatedFieldIndex)
        {
            MessageSerializedPropertyInfo calculatorPropertyInfo = null;
            calculatedFieldIndex = 0;

            for (int currentIndex = 0; currentIndex < properties.Count; ++currentIndex)
            {
                MessageSerializedPropertyInfo propertyInfo = properties[currentIndex];
                CalculatedFieldResultAttribute calculatedFieldResultAttribute = GetApplicableCalculatedFieldAttribute<CalculatedFieldResultAttribute>(propertyInfo);
                if (calculatedFieldResultAttribute != null && calculatedFieldResultAttribute.Calculator != null)
                {
                    if (calculatorPropertyInfo != null)
                        throw new Exception($"For calculated field {Name}, property {propertyInfo.PropertyInfo.Name} has a calculator type defined of {calculatedFieldResultAttribute.Calculator.FullName} but the the class already has a calculator type defined on property {calculatorPropertyInfo.PropertyInfo.Name}");

                    Type actualCalculatorType = calculatedFieldResultAttribute.GetActualCalculatedType(propertyInfo.PropertyInfo);
                    Type expectedCalculatorType = typeof(CalculatorBase<>).MakeGenericType(new Type[] { propertyInfo.PropertyInfo.PropertyType });
                    if (!CheckIsType(actualCalculatorType, expectedCalculatorType, false))
                        throw new Exception($"For calculated field {Name}, property {propertyInfo.PropertyInfo.Name}, a calculator type was defined of {actualCalculatorType.FullName} but it is not of type {expectedCalculatorType.FullName}");

                    calculatorPropertyInfo = propertyInfo;
                    calculatedFieldIndex = currentIndex;
                }
            }

            if (calculatorPropertyInfo == null)
                throw new Exception($"Calculated field {Name} does not have any property attributes that define a calculator for the class");

            return calculatorPropertyInfo;
        }

        //protected int GetIndex(MessageSerializedPropertyInfo calculatorPropertyInfo, List<MessageSerializedPropertyInfo> properties, string fieldName, Func<CalculatedFieldAttribute, bool, Position> getPositionFunction)
        protected int GetIndex(
            MessageSerializedPropertyInfo calculatorPropertyInfo, 
            List<MessageSerializedPropertyInfo> properties, 
            string fieldName, 
            Func<CalculatedFieldAttribute, Position> getPositionFunction,
            Func<CalculatedFieldResultAttribute, Position> getDefaultPositionFunction)
        {
            // Find the class that has the value defined and then get the index from that
            // If there isn't a class with the value defined use the default
            MessageSerializedPropertyInfo valuePropertyInfo = null;
            int indexToUse = -1;
            int defaultIndex = -1;
            for (int currentIndex = 0; currentIndex < properties.Count; ++currentIndex)
            {
                MessageSerializedPropertyInfo propertyInfo = properties[currentIndex];
                CalculatedFieldAttribute calculatedFieldAttribute = GetApplicableCalculatedFieldAttribute<CalculatedFieldAttribute>(propertyInfo);
                Position position = calculatedFieldAttribute == null ? Position.Unspecified : getPositionFunction(calculatedFieldAttribute);
                if (calculatedFieldAttribute != null && position != Position.Unspecified)
                {
                    if (valuePropertyInfo != null)
                        throw new Exception($"For calculated field {Name}, property {propertyInfo.PropertyInfo.Name} specified {fieldName}, but {fieldName} was already specified on {valuePropertyInfo.PropertyInfo.Name}");

                    valuePropertyInfo = propertyInfo;
                    indexToUse = GetIndexFromPosition(position, currentIndex, properties.Count);
                }

                if (propertyInfo == calculatorPropertyInfo)
                    defaultIndex = GetIndexFromPosition(getDefaultPositionFunction(calculatedFieldAttribute as CalculatedFieldResultAttribute), currentIndex, properties.Count);
            }

            return valuePropertyInfo == null ? defaultIndex : indexToUse;
        }

        protected int GetIndexFromPosition(Position position, int currentIndex, int numberOfIndexes)
        {
            int index = -1;
            switch (position)
            {
            case Position.StartOfMessage:
                index = 0;
                break;

            case Position.ThisField:
                index = currentIndex;
                break;

            case Position.NextField:
                index = currentIndex + 1;
                break;

            case Position.PreviousField:
                index = currentIndex - 1;
                break;

            case Position.EndOfMessage:
                index = numberOfIndexes - 1;
                break;
            }

            if (index < 0 || index >= numberOfIndexes)
                throw new Exception($"Calculated index for {position} of {index} based on current index of {currentIndex} and {numberOfIndexes} indexes is not valid");

            return index;
        }

        protected T GetApplicableCalculatedFieldAttribute<T>(MessageSerializedPropertyInfo propertyInfo) where T : CalculatedFieldAttribute
        {
            foreach (CalculatedFieldAttribute calculatedFieldAttribute in propertyInfo.CalculatedFieldAttributes)
            {
                if (calculatedFieldAttribute is T && calculatedFieldAttribute.Name == Name)
                    return calculatedFieldAttribute as T;
            }

            return null;
        }


        // TODO: Implement this somewhere else
        protected bool CheckIsType(Type typeToCheck, Type expectedType, bool checkGeneric)
        {
            for (Type currentType = typeToCheck; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
            {
                Type underlyingTypeToCheck = currentType.IsGenericType && checkGeneric ? currentType.GetGenericTypeDefinition() : currentType;
                if (expectedType == underlyingTypeToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        protected SortedSet<int> GetIncludedPropertyIndexes(int startIndex, int endIndex, List<MessageSerializedPropertyInfo> properties)
        {
            var includedPropertyIndexes = new SortedSet<int>();
            for (int index = startIndex; index <= endIndex; ++index)
            {
                MessageSerializedPropertyInfo propertyInfo = properties[index];
                CalculatedFieldAttribute calculatedFieldAttribute = GetApplicableCalculatedFieldAttribute<CalculatedFieldAttribute>(propertyInfo);
                if (calculatedFieldAttribute == null || !calculatedFieldAttribute.Exclude)
                    includedPropertyIndexes.Add(index);
            }

            return includedPropertyIndexes;
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int indentLevel)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indentLevel++.GetIndent() + $"On {CalculatorResultPropertyInfo.PropertyInfo.Name}: {CalculatorResultAttribute}");
            sb.AppendLine(indentLevel.GetIndent() + $"Included Field Indexes {string.Join(", ", IncludedPropertyIndexes.Select(item => item.ToString()))}");

            return sb.ToString();
        }
    }
}
