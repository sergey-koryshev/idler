namespace Idler.Helpers.DragAndDrop
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;

    using Idler.Extensions;
    using Idler.Interfaces;

    public class NoteDragAndDrop
    {
        private static ListViewItem DraggingElement;
        private static ListViewItem OverElement;
        private static DragAdorner DraggingElementAdorner;
        private static Cursor GrabHandCursor = new Cursor(new MemoryStream(Properties.Resources.CloseHandCursor));
        private static Cursor OpenHandCursor = new Cursor(new MemoryStream(Properties.Resources.OpenHandCursor));

        public static string DragSourceLabel = "DragSource";
        public static string DraggableElementFormat = "DraggableElement";
        public static string RenderElementName = "AdornerElement";
        public static int PlaceholderHeight = 15;

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(NoteDragAndDrop), new FrameworkPropertyMetadata(default(bool), OnIsEnabledChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
        "IsDragOver", typeof(bool), typeof(NoteDragAndDrop), new FrameworkPropertyMetadata(default(bool))
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

        public static void SetIsDragOver(DependencyObject element, bool value)
        {
            element.SetValue(IsDragOverProperty, value);
        }

        public static bool GetIsDragOver(DependencyObject element)
        {
            return (bool)element.GetValue(IsDragOverProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is FrameworkElement element))
            {
                throw new InvalidOperationException();
            }

            var parent = sender.FindAncestor<ListView>();

            if ((bool)e.NewValue)
            {
                element.AllowDrop = true;
                element.Drop += OnDrop;
                element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                element.DragOver += onElementDragOver;
                parent.AllowDrop = true;
                parent.DragEnter += onDragOver;
                parent.DragOver += onDragOver;
                parent.DragLeave += OnDragLeave;
                parent.GiveFeedback += OnGiveFeedback;
            }
        }

        private static void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            var dp = sender as DependencyObject;

            if ((bool)dp.GetValue(IsDragOverProperty) == false)
            {
                Mouse.SetCursor(Cursors.No);
            }
            else
            {
                Mouse.SetCursor(GrabHandCursor);
            }

            e.Handled = true;
        }

        private static void OnDragLeave(object sender, DragEventArgs e)
        {
            var dp = sender as DependencyObject;
            dp.SetValue(IsDragOverProperty, false);

            (sender as UIElement).Dispatcher.BeginInvoke(new Action(() =>
            {
                if ((bool)dp.GetValue(IsDragOverProperty) == false)
                {
                    OnRealDragLeave(sender, e);
                }
            }));
        }

        private static void OnRealDragLeave(object sender, DragEventArgs e)
        {
            if (DraggingElementAdorner != null)
            {
                if (DraggingElementAdorner.Visibility != Visibility.Collapsed)
                {
                    DraggingElementAdorner.Visibility = Visibility.Collapsed;
                }
            }

            if (OverElement != null)
            {
                ClearProperties(OverElement.DataContext as IDraggableListItem);
            } 
        }

        private static void onElementDragOver(object sender, DragEventArgs e)
        {
            var element = sender as ListViewItem;

            if (DraggingElement == element)
            {
                return;
            }

            if (OverElement != null && OverElement != element)
            {
                var oldContext = OverElement.DataContext as IDraggableListItem;
                ClearProperties(oldContext);
            }

            var context = element.DataContext as IDraggableListItem;
            var targetElementPosition = e.GetPosition(element);
            var offset = context.DragOverPlaceholderPosition == Models.DragOverPlaceholderPosition.Top ? PlaceholderHeight : context.DragOverPlaceholderPosition == Models.DragOverPlaceholderPosition.Bottom ? -PlaceholderHeight : 0;
            var direction = targetElementPosition.Y < (element.ActualHeight + offset) / 2 ? Models.DragOverPlaceholderPosition.Top : Models.DragOverPlaceholderPosition.Bottom;
            context.DragOverPlaceholderPosition = direction;
            OverElement = element;
            context.HasDragOver = true;
        }

        private static void onDragOver(object sender, DragEventArgs e)
        {
            ((DependencyObject)sender).SetValue(IsDragOverProperty, true);

            if (DraggingElementAdorner != null)
            {
                if (DraggingElementAdorner.Visibility != Visibility.Visible)
                {
                    DraggingElementAdorner.Visibility = Visibility.Visible;
                }

                DraggingElementAdorner.UpdatePosition(e.GetPosition(DraggingElement));
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem element && e.OriginalSource is FrameworkElement dragSource && dragSource.Tag?.ToString() == DragSourceLabel)
            {
                var context = element.DataContext as IDraggableListItem;
                AdornerLayer layer = null;
                var elementToRenderInAdorner = element.FindChild(RenderElementName) as FrameworkElement;

                if (elementToRenderInAdorner != null)
                {
                    layer = AdornerLayer.GetAdornerLayer(element);
                    layer.IsHitTestVisible = false;
                    DraggingElementAdorner = new DragAdorner(element, elementToRenderInAdorner, e.GetPosition(element));
                    layer?.Add(DraggingElementAdorner);
                }

                DraggingElement = element;

                element.Visibility = Visibility.Collapsed;
                Mouse.SetCursor(GrabHandCursor);

                var data = new DataObject(DraggableElementFormat, context);
                DragDrop.DoDragDrop(element, data, DragDropEffects.Move);

                Mouse.SetCursor(OpenHandCursor);

                if (elementToRenderInAdorner != null)
                {
                    layer?.Remove(DraggingElementAdorner);
                    DraggingElementAdorner = null;
                }

                element.Visibility = Visibility.Visible;

                ClearProperties(context);
                var targetItemContext = OverElement?.DataContext as IDraggableListItem;
                
                if (targetItemContext != null)
                {
                    ClearProperties(targetItemContext);
                }

                DraggingElement = null;
                OverElement = null;
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var targetItem = ((FrameworkElement)sender).DataContext as IDraggableListItem;
            var droppedItem = e.Data.GetData(DraggableElementFormat) as IDraggableListItem;
            var parent = ((FrameworkElement)sender).FindAncestor<ListView>();
            var dataContext = parent?.DataContext as IDragAndDrop;

            if (targetItem != null && droppedItem != null && parent != null && dataContext != null)
            {
                dataContext.OnElementDropped(droppedItem, targetItem, targetItem.DragOverPlaceholderPosition);
            }

            if (targetItem != null)
            {
                ClearProperties(targetItem);
            }

            if (droppedItem != null)
            {
                ClearProperties(droppedItem);
            }
        }

        private static void ClearProperties(IDraggableListItem item)
        {
            item.DragOverPlaceholderPosition = Models.DragOverPlaceholderPosition.None;
            item.HasDragOver = false;
        }
    }
}
