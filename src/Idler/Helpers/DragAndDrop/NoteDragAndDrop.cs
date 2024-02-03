using Idler.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Idler.Helpers.DragAndDrop
{
    public class NoteDragAndDrop
    {
        public static string DragSourceLabel = "DragSource";

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(NoteDragAndDrop), new FrameworkPropertyMetadata(default(bool), OnPropChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty DataContextProperty = DependencyProperty.RegisterAttached(
        "DataContext", typeof(object), typeof(NoteDragAndDrop), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = false,
        });

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetDataContext(DependencyObject element, bool value)
        {
            element.SetValue(DataContextProperty, value);
        }

        public static bool GetDataContext(DependencyObject element)
        {
            return (bool)element.GetValue(DataContextProperty);
        }

        private static void OnPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement fe))
            {
                throw new InvalidOperationException();
            }

            if ((bool)e.NewValue)
            {
                fe.AllowDrop = true;
                fe.Drop += OnDrop;
                fe.PreviewMouseMove += OnPreviewMouseMove;
            }
            else
            {
                fe.AllowDrop = false;
                fe.Drop -= OnDrop;
                fe.PreviewMouseMove -= OnPreviewMouseMove;
            }
        }

        private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem draggedItem && e.LeftButton == MouseButtonState.Pressed && e.Source is FrameworkElement dragSource && dragSource.Tag?.ToString() == DragSourceLabel)
            {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var targetItem = ((FrameworkElement)sender).DataContext as ShiftNote;
            var droppedItem = e.Data.GetData(typeof(ShiftNote)) as ShiftNote;

            if (targetItem == null || droppedItem == null)
            {
                return;
            }

            var dataContext = ((FrameworkElement)sender).GetValue(DataContextProperty) as IDragAndDrop<ShiftNote>;

            if (dataContext == null)
            {
                return;
            }

            dataContext.OnElementDropped(sender, droppedItem, targetItem);
        }
    }
}
