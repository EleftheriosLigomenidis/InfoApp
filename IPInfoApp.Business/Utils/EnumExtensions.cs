using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IPInfoApp.Business.Utils
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description of the enumeration
        /// </summary>
        /// <param name="enumValue">The enum value</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetEnumDescription(this Enum enumValue)
        {
            FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
            if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            throw new ArgumentException("Item not found.", nameof(enumValue));
        }
    }
}
