namespace MessageSerializer
{
    public class CalculatedLengthResultAttributeDefaults : CalculatedFieldResultAttributeDefaults
    {
        public CalculatedLengthResultAttributeDefaults()
        {
            Name = "Length";
            DefaultStart = Position.NextField;
            Calculator = typeof(CalculatorLength<>);
            Priority = 1000;
            Verify = false;
        }
    }

    public class CalculatedLengthAttribute : CalculatedFieldAttribute
    {
        public CalculatedLengthAttribute()
            : base(new CalculatedLengthResultAttributeDefaults())
        {
        }
    }

    public class CalculatedLengthResultAttribute : CalculatedFieldResultAttribute
    {
        public CalculatedLengthResultAttribute()
            : base(new CalculatedLengthResultAttributeDefaults())
        {

        }
    }
}
