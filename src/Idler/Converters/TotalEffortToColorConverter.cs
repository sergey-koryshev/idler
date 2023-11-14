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
            if (Properties.Settings.Default.DailyWorkLoad == 0 || !Properties.Settings.Default.IsHighlightingEnabled)
            {
                return Color.Empty.ToMediaColor();
            }

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
                        if (totalEffort > 0 && totalEffort < Properties.Settings.Default.DailyWorkLoad)
                        {
                            colorInt = (int)TotalEffortType.Parttime;
                        }
                        if (totalEffort == Properties.Settings.Default.DailyWorkLoad)
                        {
                            colorInt = (int)TotalEffortType.CompleteShift;
                        }
                        if (totalEffort > Properties.Settings.Default.DailyWorkLoad)
                        {
                            colorInt = Properties.Settings.Default.IsOvertimeHighlighted 
                                ? (int)TotalEffortType.Overtime 
                                : (int)TotalEffortType.CompleteShift;
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
