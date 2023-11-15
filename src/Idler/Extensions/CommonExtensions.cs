using Idler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Extensions
{
    public static class CommonExtensions
    {
        public static int? GetDarkerColor(this TotalEffortType value)
        {
            Type enumType = value.GetType();
            FieldInfo[] enumValues = enumType.GetFields();
            FieldInfo foundValue = enumValues
                .Where(field => field.IsLiteral && field.GetValue(null).Equals(value))
                .FirstOrDefault();

            if (foundValue == null)
            {
                return null;
            }

            var attribute = (DarkerColorAttribute)foundValue.GetCustomAttribute(typeof(DarkerColorAttribute));

            return attribute?.Color;
        }
    }
}
