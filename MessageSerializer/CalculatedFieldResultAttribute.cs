using System;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace MessageSerializer
{
    public class CalculatedFieldResultAttributeDefaults : CalculatedFieldAttributeDefaults
    {
        public Type Calculator { get; protected set; }
        public int Priority { get; protected set; }
        public Position DefaultStart { get; protected set; }
        public Position DefaultEnd { get; protected set; }
        public bool Verify { get; protected set; }

        public CalculatedFieldResultAttributeDefaults()
        {
            // Note:
            // Calculator = null
            // Priority = 0
            // Verify = false
            DefaultStart = Position.StartOfMessage;
            DefaultEnd = Position.EndOfMessage;
        }
    }

    public class CalculatedFieldResultAttribute : CalculatedFieldAttribute
    {
        protected Type _calculator;
        protected int _priority;
        protected Position _defaultStart;
        protected Position _defaultEnd;
        protected bool _verify;

        public CalculatedFieldResultAttribute()
            : this(new CalculatedFieldResultAttributeDefaults())
        {

        }

        public CalculatedFieldResultAttribute(Type calculator)
            : this(new CalculatedFieldResultAttributeDefaults())
        {
            // If you are creating a ResultAttribute you must have a Calculator
            _calculator = calculator;
        }

        public CalculatedFieldResultAttribute(CalculatedFieldResultAttributeDefaults defaults)
            : base(defaults)
        {
            _calculator = defaults.Calculator;
            _priority = defaults.Priority;
            _defaultStart = defaults.DefaultStart;
            _defaultEnd = defaults.DefaultEnd;
            _verify = defaults.Verify;
        }

        [XmlIgnore]
        public Type Calculator
        {
            get { return _calculator; }
            set
            {
                _calculator = value;
                IsCalculatorSpecified = GetVariableSpecified();
            }
        }

        public string CalculatorAssemblyQualifiedName
        {
            get => Calculator?.AssemblyQualifiedName;
            set => Calculator = Type.GetType(value);
        }

        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                IsPrioritySpecified = GetVariableSpecified();
            }
        }

        public Position DefaultStart
        {
            get { return _defaultStart; }
            set
            {
                _defaultStart = value;
                IsDefaultStartSpecified = GetVariableSpecified();
            }
        }

        public Position DefaultEnd
        {
            get { return _defaultEnd; }
            set
            {
                _defaultEnd = value;
                IsDefaultEndSpecified = GetVariableSpecified();
            }
        }

        public bool Verify
        {
            get { return _verify; }
            set
            {
                _verify = value;
                IsVerifySpecified = GetVariableSpecified();
            }
        }

        // Because the CalculatorBase class depends on whatever the type of the field is
        // for what the various functions will take as arguments and return and the attribute
        // can't figure out what the type of its associated property is on its own we'll
        // allow it a chance to change the type here.
        public virtual Type GetActualCalculatedType(PropertyInfo propertyInfo)
        {
            if (Calculator != null && Calculator.IsGenericTypeDefinition)
            {
                // If IsGenericTypeDefinition is true then it means the type
                // was defined as typeof(CalculatorBase<>) instead of something like typeof(CalculatorBase<ushort>)
                // If that is the case we will attempt to add the PropertyType as the generic definition
                Type[] arguments = Calculator.GetGenericArguments();
                if (arguments.Length == 1)
                    return Calculator.MakeGenericType(new Type[] { propertyInfo.PropertyType });
            }

            return Calculator;
        }

        // See the notes in MessagePropertyAttribute for why all this is here
        // It's basically to make serializing to XML as minimal as possible
        [XmlIgnore]
        public bool IsCalculatorSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsPrioritySpecified { get; protected set; }
        [XmlIgnore]
        public bool IsDefaultStartSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsDefaultEndSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsVerifySpecified { get; protected set; }

        public bool ShouldSerializeCalculatorAssemblyQualifiedName() {  return IsCalculatorSpecified; }
        public bool ShouldSerializePriority() {  return IsPrioritySpecified; }
        public bool ShouldSerializeDefaultStart() { return IsDefaultStartSpecified; }
        public bool ShouldSerializeDefaultEnd() { return IsDefaultEndSpecified; }
        public bool ShouldSerializeVerify() { return IsVerifySpecified; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{GetType().Name}->");
            sb.Append($"Name: {Name}");
            sb.Append($", Calculator: {Calculator?.PrintableType()}");
            sb.Append($", Priority: {Priority}");
            sb.Append($", DefaultStart: {DefaultStart}");
            sb.Append($", DefaultEnd: {DefaultEnd}");
            if (IsExcludeSpecified)
                sb.Append($", Exclude: {Exclude}");
            if (IsStartSpecified)
                sb.Append($", Start: {Start}");
            if (IsEndSpecified)
                sb.Append($", End: {End}");
            if (!string.IsNullOrWhiteSpace(AddedBy))
                sb.Append($", AddedBy: {AddedBy}");
            if (IsVerifySpecified)
                sb.Append($", Verify: {Verify}");

            return sb.ToString();
        }
    }
}
