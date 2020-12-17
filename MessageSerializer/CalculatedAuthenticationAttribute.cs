using System;

namespace MessageSerializer
{
    public class CalculatedAuthenticationResultAttributeDefaults : CalculatedFieldResultAttributeDefaults
    {
        public CalculatedAuthenticationResultAttributeDefaults()
        {
            Name = "Authentication";
            DefaultStart = Position.StartOfMessage;
            DefaultEnd = Position.PreviousField;
            Priority = 2000;
            Verify = true;
        }
    }

    public class CalculatedAuthenticationAttribute : CalculatedFieldAttribute
    {
        public CalculatedAuthenticationAttribute()
            : base(new CalculatedAuthenticationResultAttributeDefaults())
        {
        }
    }

    public class CalculatedAuthenticationResultAttribute : CalculatedFieldResultAttribute
    {
        public CalculatedAuthenticationResultAttribute()
            : base(new CalculatedAuthenticationResultAttributeDefaults())
        {
        }

        public CalculatedAuthenticationResultAttribute(Type calculatorType)
            : base(new CalculatedAuthenticationResultAttributeDefaults())
        {
            Calculator = calculatorType;
        }
    }
}
