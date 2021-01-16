using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace MessageSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MessagePropertyAttribute : Attribute
    {
        protected bool _isBcd;
        protected string _format;
        // These are mainly for variable length fields
        // The field should be at least _minLength long (defaults to 0)
        // The field should not be more than _maxLength long (defaults to -1, meaning no limit)
        // _minimizeVariableLength is mainly for numeric fields.  If a numeric field is variable
        // length you could get a value like 0x00000012.  That value only needs one byte to be
        // represented but will take up 4 by default.  If _minimizeVariableLength is set it will
        // take up the minimum bytes necessary as defined by _minLength
        protected int _length;
        protected bool _variableLength;
        protected int _minLength;
        protected int _maxLength;
        protected bool _minimizeVariableLength;

        protected BlobTypes _blobType;
        protected Endiannesses _endianness;
        protected char _prepadCharacter;

        // Setting the default value makes it so the XmlSerializer won't output the property if it is set to the default
        [DefaultValue(false)]
        public bool Exclude { get; set; }

        public Endiannesses Endianness
        {
            get { return _endianness; }
            set
            {
                _endianness = value;
                IsEndiannessSpecified = GetVariableSpecified();
            }
        }

        // Setting the default value makes it so the XmlSerializer won't output the property if it is set to the default
        [DefaultValue(false)]
        public bool Prepad { get; set; }

        // TODO: Doesn't work correctly with rules because Prepad is being set
        public char PrepadCharacter
        {
            get { return _prepadCharacter; }
            set
            {
                _prepadCharacter = value;
                IsPrepadCharacterSpecified = GetVariableSpecified();

                // NOTE: We really only want to automatically specify Prepad to make things simpler for the user
                // Could probably get away with just having the PrepadCharacter setting
                Prepad = GetVariableSpecified();
            }
        }

        // For the Length field should be able to use one of the following:
        // Specified BlobDataName
        // Next property with BlobTypes.Data
        // Next property with name starting with BlobData
        public BlobTypes BlobType
        {
            get { return _blobType; }
            set
            {
                _blobType = value;
                IsBlobTypeSpecified = GetVariableSpecified();
            }
        }
        public string AssociatedBlobProperty { get; set; }

        public bool IsBcd
        {
            get { return _isBcd; }
            set
            {
                _isBcd = value;
                IsIsBcdSpecified = GetVariableSpecified();
            }
        }


        // For reading/writing XML using the XmlSerializer we can't output the Type
        // directly so that is what the AssemblyQualifiedName is used for
        [XmlIgnore]
        public Type TypeSerializerClass { get; set; }

        public string TypeSerializerClassAssemblyQualifiedName
        {
            get => TypeSerializerClass?.AssemblyQualifiedName;
            set => TypeSerializerClass = Type.GetType(value);
        }

        public int Length
        {
            get { return _length; }
            set
            {
                _length = value;
                IsLengthSpecified = GetVariableSpecified();

                // If the length has been specified then the implication is that the field is not variable length
                // However, we won't actually set VariableLength since the default is false and if for some crazy
                // reason it needs to be set to true that can still be done
                // TODO: This is a little weird since we actually don't want to say it is specified because now it will be output
                // when serialized which won't work
                //IsVariableLengthSpecified = true;
                IsVariableLengthSpecified = GetVariableSpecified();
            }
        }

        public bool VariableLength
        {
            get { return _variableLength; }
            set
            {
                _variableLength = value;
                IsVariableLengthSpecified = GetVariableSpecified();
            }
        }

        public int MinLength
        {
            get { return _minLength; }
            set
            {
                _minLength = value;
                IsMinLengthSpecified = GetVariableSpecified();
            }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = value;
                IsMaxLengthSpecified = GetVariableSpecified();
            }
        }

        public bool MinimizeVariableLength
        {
            get { return _minimizeVariableLength; }
            set
            {
                _minimizeVariableLength = value;
                IsMinimizeVariableLengthSpecified = GetVariableSpecified();
            }
        }

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                IsFormatSpecified = GetVariableSpecified();
            }
        }

        // When we are applying rules we don't actually want to set the values
        // indicating a variable has been specified because the rules are used
        // to apply defaults specifically when values aren't set.  This will
        // prevent them from serialized when being output to XML format and also
        // allows other rules to check if the values have been set as necessary.
        [XmlIgnore]
        public bool ApplyingRules { get; set; }

        [XmlIgnore]
        public string AppliedBy { get; set; }

        protected bool GetVariableSpecified()
        {
            return !ApplyingRules;
        }

        // Since these fields are calculated they don't need to be exported
        [XmlIgnore]
        public bool IsEndiannessSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsIsBcdSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsLengthSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsFormatSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsBlobTypeSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsVariableLengthSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsMinLengthSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsMaxLengthSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsMinimizeVariableLengthSpecified { get; protected set; }
        [XmlIgnore]
        public bool IsPrepadCharacterSpecified { get; protected set; }

        // This is not the greatest thing ever but it allows the XML Serializer
        // to not include the associated fields if they have not been explicitly
        // specified which would otherwise cause problems because the various rules
        // need to know if the values were actually specified or defaults are being used.
        // Also, it is possible to just name the above functions something like IsBcdSpecified
        // but then they would have to be public and then they would show up when
        // just trying to use the attribute on property which is not very good either.
        // In the IsBcdSpecified scenario, you can have a protected set for serializing out
        // but when you try to serialize in the serializer tries to set the WhateverSpecified
        // property if it is and thus fails when the property is protected.
        // See more notes in MessageSerializerClassInfo
        // https://stackoverflow.com/questions/37838640/shouldserialize-vs-specified-conditional-serialization-pattern/37842985
        public bool ShouldSerializeEndianness() { return IsEndiannessSpecified; }
        public bool ShouldSerializeIsBcd() { return IsIsBcdSpecified; }
        public bool ShouldSerializeLength() { return IsLengthSpecified; }
        public bool ShouldSerializeFormat() { return IsFormatSpecified; }
        public bool ShouldSerializeBlobType() { return IsBlobTypeSpecified; }
        public bool ShouldSerializeVariableLength() { return IsVariableLengthSpecified; }
        public bool ShouldSerializeMinLength() { return IsMinLengthSpecified; }
        public bool ShouldSerializeMaxLength() { return IsMaxLengthSpecified; }
        public bool ShouldSerializeMinimizeVariableLength() { return IsMinimizeVariableLengthSpecified; }
        public bool ShouldSerializePrepadCharacter() { return IsPrepadCharacterSpecified; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.ConditionalJoin("Exclude".GetNameValuePair(Exclude, Exclude));
            sb.ConditionalJoin("TypeSerializerClass".GetNameValuePair(TypeSerializerClass != null, TypeSerializerClass));
            sb.ConditionalJoin("Length".GetNameValuePair(IsLengthSpecified, Length));
            sb.ConditionalJoin("VariableLength".GetNameValuePair(IsVariableLengthSpecified, VariableLength));
            sb.ConditionalJoin("Endianness".GetNameValuePair(IsEndiannessSpecified, Endianness));
            sb.ConditionalJoin("IsBcd".GetNameValuePair(IsIsBcdSpecified, IsBcd));
            sb.ConditionalJoin("Format".GetNameValuePair(IsFormatSpecified, Format));
            sb.ConditionalJoin("BlobType".GetNameValuePair(IsBlobTypeSpecified, BlobType));
            sb.ConditionalJoin("AssociatedBlobProperty".GetNameValuePair(!string.IsNullOrWhiteSpace(AssociatedBlobProperty), AssociatedBlobProperty));
            sb.ConditionalJoin("MinLength".GetNameValuePair(IsMinLengthSpecified, MinLength));
            sb.ConditionalJoin("MaxLength".GetNameValuePair(IsMaxLengthSpecified, MaxLength));
            sb.ConditionalJoin("MinimizeVariableLength".GetNameValuePair(IsMinimizeVariableLengthSpecified, MinimizeVariableLength));
            sb.ConditionalJoin("Prepad".GetNameValuePair(Prepad, Prepad));
            sb.ConditionalJoin("PrepadCharacter".GetNameValuePair(IsPrepadCharacterSpecified, PrepadCharacter));
            sb.ConditionalJoin("AppliedBy".GetNameValuePair(!string.IsNullOrWhiteSpace(AppliedBy), AppliedBy));

            return sb.Length > 0 ? GetType().Name + ": " + sb : "";
        }

        //protected string GetValue(string name, bool isSpecified, object value)
        //{
        //    if (!isSpecified)
        //        return "";

        //    return $"{name}: {value}";
        //}
    }
}
