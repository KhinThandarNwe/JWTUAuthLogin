using JWTUAuthLogin.Shared.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace JWTUAuthLogin.Shared.Extensions
{
    public static class EnumExtenctions
    {
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[]? attributes =
                fi.GetCustomAttributes(typeof(DescriptionAttribute), false)
                as DescriptionAttribute[];

            return attributes != null && attributes.Length > 0
                ? attributes.First().Description
                : value.ToString();
        }
        public static string? GetStringValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var stringValueAttribute = fieldInfo.GetCustomAttributes<StringValueAttribute>(false);
            return stringValueAttribute.ToString();
        }
    }
}
