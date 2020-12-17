using System.Linq;

namespace MessageSerializer
{
    public abstract class CalculatorBase<TCalculatorResultType>
    {
        protected string _name;
        protected CalculatedFieldInfo _calculatedFieldInfo;

        // Originally the constructor was required to take name and classInfo but that meant
        // that the Calculator classes had to have a constructor with parameters, even
        // if they don't care about those values.  It also made it a pain for the unit
        // tests since they might not care what the calculator was attached to they just
        // want to make sure the value is calculated correctly.
        // So added the SetFieldInfo instead.

        public abstract TCalculatorResultType Calculate(params byte[][] arrays);

        public virtual bool Verify(DeserializeStatus status, TCalculatorResultType receivedValue, TCalculatorResultType computedValue)
        {
            bool success = receivedValue.Equals(computedValue);
            if (!success)
            {
                AddFailedMessage(status, receivedValue, computedValue);
            }

            return success;
        }

        public virtual bool Verify(DeserializeStatus status, TCalculatorResultType receivedValue, params byte[][] arrays)
        {
            TCalculatorResultType computedValue = Calculate(arrays);
            return Verify(status, receivedValue, computedValue);
        }

        public virtual void SetFieldInfo(string name, MessageSerializedClassInfo classInfo)
        {
            _name = name;
            _calculatedFieldInfo = GetMyCalculatedFieldInfo(classInfo);
        }

        protected virtual CalculatedFieldInfo GetMyCalculatedFieldInfo(MessageSerializedClassInfo classInfo)
        {
            return classInfo.CalculatedFields.Single(calculatedFieldInfo => calculatedFieldInfo.Name == _name);
        }

        protected virtual void AddFailedMessage(DeserializeStatus status, TCalculatorResultType receivedValue, TCalculatorResultType computedValue)
        {
            status.Messages.Add($"Verify failed on calculated field {_calculatedFieldInfo.CalculatorResultPropertyInfo.PropertyInfo.Name} of type {_name} using calculator {GetType().Name} with received value {ToString(receivedValue)} and computed value {ToString(computedValue)}");
        }

        protected virtual string ToString(TCalculatorResultType value)
        {
            return $"{value}";
        }
    }
}
