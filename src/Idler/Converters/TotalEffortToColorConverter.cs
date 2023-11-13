using Idler.Extensions;
using Idler.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Idler.Converters
{
    public class TotalEffortToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? colorInt = null;

            if (values.Length == 2)
            {
                if (values[1] is Dictionary<DateTime, decimal> datesToHighlight)
                {
                    if (values[0] is DateTime date && datesToHighlight.ContainsKey(date))
                    {
                        decimal totalEffort = datesToHighlight[date];

                        if (totalEffort == 0)
                        {
                            colorInt = (int)TotalEffortType.None;
                        }
                        if (totalEffort > 0 && totalEffort < 8)
                        {
                            colorInt = (int)TotalEffortType.Parttime;
                        }
                        if (totalEffort == 8)
                        {
                            colorInt = (int)TotalEffortType.CompleteShift;
                        }
                        if (totalEffort > 8)
                        {
                            colorInt = (int)TotalEffortType.Overtime;
                        }
                    }
                }
            }

            return colorInt == null 
                ? Color.Empty.ToMediaColor() 
                : ((Color)new ColorConverter().ConvertFromString($"#{colorInt:X}")).ToMediaColor();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
