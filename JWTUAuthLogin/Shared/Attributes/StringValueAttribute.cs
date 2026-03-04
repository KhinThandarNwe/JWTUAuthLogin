namespace JWTUAuthLogin.Shared.Attributes
{
    public class StringValueAttribute : Attribute
    {
         public string Value { get; }

        public StringValueAttribute(string value)
        {
            Value = value;
        }
    }
}
