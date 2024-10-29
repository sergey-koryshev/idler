namespace Idler.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    internal class SumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double sum = 0;
            double startedValue = parameter is double test ? test : 0;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double value)
                {
                    sum += value == 0 && i == 0 ? startedValue : value;
                }
            }

            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
