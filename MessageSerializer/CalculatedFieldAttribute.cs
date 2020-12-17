using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace MessageSerializer
{
    public enum Position
    {
        Unspecified = 0,
        StartOfMessage,
        ThisField,
        NextField,
        PreviousField,
        EndOfMessage
    }

    public class CalculatedFieldAttributeDefaults
    {
        public string Name { get; protected set; }
        public bool Exclude { get; protected set; }
        public Position Start { get; protected set; }
        public Position End { get; protected set; }

        public CalculatedFieldAttributeDefaults()
        {
            // Note: Not doing anything here basically sets the defaults as:
            // Name = ""
            // Exclude = false
            // Start = Position.Unspecified
            // End = Position.Unspecified
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CalculatedFieldAttribute : Attribute
    {
        protected string _name;
        protected bool _exclude;
        protected Position _start;
        protected Position _end;

        public CalculatedFieldAttribute()
            : this(new CalculatedFieldAttributeDefaults())
        {
        }

        public CalculatedFieldAttribute(CalculatedFieldAttributeDefaults defaults)
        {
            _name = defaults.Name;
            _exclude = defaults.Exclude;
            _start = defaults.Start;
            _end = defaults.End;
        }

        public bool Exclude
        {
            get { return _exclude; }
            set
            {
                _exclude = value;
                IsExcludeSpecified = GetVariableSpecified();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                IsNameSpecified = GetVariableSpecified();
            }
        }

        public Position Start
        {
            get { return _start; }
            set
            {
                _start = value;
                IsStartSpecified = GetVariableSpecified();
            }
        }

        public Position End
        {
            get { return _end; }
            set
            {
                _end = value;
                IsEndSpecified = GetVariableSpecified();
            }
        }

        [XmlIgnore]
        [DefaultValue("")]
        public string AddedBy { get; set; }

        // See the notes in MessagePropertyAttribute for why all this is here
        // It's basically to make serializing to XML as minimal as possible
        [XmlIgnore]
        public bool ApplyingRules { get; set; }

        protected bool GetVariableSpecified()
        {
            return !ApplyingRules;
        }

        [XmlIgnore]
        public bool IsNameSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsExcludeSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsStartSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsEndSpecified { get; protected set; }

        public bool ShouldSerializeExclude() { return IsExcludeSpecified; }
        public bool ShouldSerializeName() { return IsNameSpecified; }
        public bool ShouldSerializeStart() { return IsStartSpecified; }
        public bool ShouldSerializeEnd() { return IsEndSpecified; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{GetType().Name}->");
            sb.Append($"Name: {Name}");
            if (IsExcludeSpecified)
                sb.Append($", Exclude: {Exclude}");
            if (IsStartSpecified)
                sb.Append($", Start: {Start}");
            if (IsEndSpecified)
                sb.Append($", End: {End}");
            if (!string.IsNullOrWhiteSpace(AddedBy))
                sb.Append($", AddedBy: {AddedBy}");

            return sb.ToString();
        }
    }
}
