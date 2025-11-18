using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Utilities
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            // 1. Get the FieldInfo for the specific enum value
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());

            if (field == null)
            {
                return enumValue.ToString();
            }

            // 2. Look for the DescriptionAttribute on that field
            DescriptionAttribute[] attributes = (DescriptionAttribute[])field.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );

            // 3. Return the Description if found, otherwise return the enum member name
            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
    }
}
