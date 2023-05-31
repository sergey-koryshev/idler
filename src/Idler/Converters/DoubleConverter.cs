using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Idler.Converters
{
    internal class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format("{0:0.##}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringifiedValue = value.ToString();

            if (string.IsNullOrWhiteSpace(stringifiedValue))
            {
                return new ValidationResult(false, "Value cannot be empty");
            }

            stringifiedValue = stringifiedValue.Replace(',', '.');

            if (stringifiedValue.StartsWith("."))
            {
                stringifiedValue = string.Concat("0", stringifiedValue);
            }

            if (stringifiedValue.EndsWith("."))
            {
                return new ValidationResult(false, "Value cannot end with '.'");
            }

            try
            {
                return decimal.Parse(stringifiedValue, CultureInfo.InvariantCulture);
            }
            catch
            {
                return new ValidationResult(false, "Value cannot be parsed");
            }
        }
    }
}
