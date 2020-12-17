using System;
using System.Text;
using System.Xml.Serialization;

namespace MessageSerializer
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageClassAttribute : Attribute
    {
        protected Endiannesses _endianness;

        public bool DefaultExcludeProperty { get; set; }
        public bool PutInheritedPropertiesLast { get; set; }

        public Endiannesses Endianness
        {
            get { return _endianness; }
            set
            {
                _endianness = value;
                EndiannessExplicitlySpecified = true;
            }
        }

        [XmlIgnore]
        public bool EndiannessExplicitlySpecified { get; protected set; }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int indentLevel)
        {
            StringBuilder sb = new StringBuilder();
            sb.ConditionalJoin("Endianness".GetNameValuePair(EndiannessExplicitlySpecified, Endianness));
            sb.ConditionalJoin("DefaultExcludeProperty".GetNameValuePair(DefaultExcludeProperty, DefaultExcludeProperty));
            sb.ConditionalJoin("PutInheritedPropertiesLast".GetNameValuePair(PutInheritedPropertiesLast, PutInheritedPropertiesLast));

            return sb.ToString();
        }
    }
}
