using System.Collections.Generic;

namespace MessageSerializer
{
    /// <summary>
    /// This class provides a set of defaults for how different class and fields should be serialized
    /// </summary>
    public class SerializationDefaults
    {
        public SerializationDefaults(List<ITypeSelector> typeSelectors = null, List<IPropertyRule> propertyRules = null)
        {
            Endianness = Endiannesses.System;

            TypeSelectors = typeSelectors ?? GetDefaultTypeSelectors();
            PropertyRules = propertyRules ?? GetDefaultPropertyRules();
        }

        public Endiannesses Endianness { get; set; }
        public List<ITypeSelector> TypeSelectors { get; set; }
        public List<IPropertyRule> PropertyRules { get; set; }

        protected List<ITypeSelector> GetDefaultTypeSelectors()
        {
            var typeSelectors = new List<ITypeSelector>();
            typeSelectors.Add(new TypeSelectorSpecifiedClass());
            typeSelectors.Add(new TypeSelectorSerializable());
            // Note that NumericBcd is put before numeric as otherwise Numeric would be chosen even if the field was supposed to be Bcd
            typeSelectors.Add(new TypeSelectorNumericBcd());
            typeSelectors.Add(new TypeSelectorEnum());
            typeSelectors.Add(new TypeSelectorNumeric());
            typeSelectors.Add(new TypeSelectorString());
            typeSelectors.Add(new TypeSelectorDateTime());
            typeSelectors.Add(new TypeSelectorByteArray());

            return typeSelectors;
        }

        protected List<IPropertyRule> GetDefaultPropertyRules()
        {
            var propertyRules = new List<IPropertyRule>();
            propertyRules.Add(new PropertyRuleEndianness());
            propertyRules.Add(new PropertyRuleLengthField());
            propertyRules.Add(new PropertyRuleBcd());
            propertyRules.Add(new PropertyRuleAuthenticationField());
            propertyRules.Add(new PropertyRuleDateTime());
            propertyRules.Add(new PropertyRuleLengths());
            propertyRules.Add(new PropertyRulePrepad());

            return propertyRules;
        }
}
}
