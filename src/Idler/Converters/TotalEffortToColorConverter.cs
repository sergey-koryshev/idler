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

                        var color = TotalEffortType.None;

                        if (totalEffort == 0)
                        {
                            color = TotalEffortType.None;
                        }
                        if (totalEffort > 0 && totalEffort < Properties.Settings.Default.DailyWorkLoad)
                        {
                            color = TotalEffortType.Parttime;
                        }
                        if (totalEffort == Properties.Settings.Default.DailyWorkLoad)
                        {
                            color = TotalEffortType.CompleteShift;
                        }
                        if (totalEffort > Properties.Settings.Default.DailyWorkLoad)
                        {
                            color = Properties.Settings.Default.IsOvertimeHighlighted 
                                ? TotalEffortType.Overtime 
                                : TotalEffortType.CompleteShift;
                        }

                        colorInt = date == DateTime.Today ? color.GetDarkerColor() : (int)color;
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
