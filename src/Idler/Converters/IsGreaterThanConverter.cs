namespace Idler.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    internal class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int valueA && parameter is int valueB)
            {
                return valueA > valueB;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
