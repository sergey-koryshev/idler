using Idler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                var dependObj = obj;

                do
                {
                    dependObj = dependObj.GetParent();

                    if (dependObj is T)
                    {
                        return dependObj as T;
                    }
                }
                while (dependObj != null);
            }

            return null;
        }

        public static DependencyObject GetParent(this DependencyObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is ContentElement)
            {
                var parent = ContentOperations.GetParent(obj as ContentElement);

                if (parent != null)
                {
                    return parent;
                }

                if (obj is FrameworkContentElement)
                {
                    return (obj as FrameworkContentElement).Parent;
                }

                return null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        public static DependencyObject FindChild(this DependencyObject obj, string name)
        {
            if (obj == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var frameworkElement = child as FrameworkElement;

                if (frameworkElement != null && frameworkElement.Name == name)
                {
                    return child;
                }
                else
                {
                    var interimResult = child.FindChild(name);

                    if (interimResult != null)
                    {
                        return interimResult;
                    }
                }
            }

            return null;
        }
    }
}
