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

    public class ListViewDragAndDrop
    {
        private static readonly Cursor GrabHandCursor = new Cursor(new MemoryStream(Properties.Resources.CloseHandCursor));
        private static readonly Cursor OpenHandCursor = new Cursor(new MemoryStream(Properties.Resources.OpenHandCursor));

        public static readonly string DragSourceLabel = "DragSource";
        public static readonly string DraggableElementFormat = "DraggableElement";
        public static readonly string DragAdornerName = "AdornerElement";
        public static readonly int PlaceholderHeight = 15;

        public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
        "IsDragOver", typeof(bool), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(default(bool))
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(default(bool), OnIsEnabledPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty IsDragAdornerProperty = DependencyProperty.RegisterAttached(
        "IsDragAdorner", typeof(bool), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(default(bool), OnIsDragAdornerPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty DragAdornerProperty = DependencyProperty.RegisterAttached(
        "DragAdorner", typeof(DragAdorner), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty DraggingElementProperty = DependencyProperty.RegisterAttached(
        "DraggingElement", typeof(FrameworkElement), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.RegisterAttached(
        "TargetElement", typeof(FrameworkElement), typeof(ListViewDragAndDrop), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = false,
        });

        public static void SetIsDragOver(DependencyObject element, bool value)
        {
            element.SetValue(IsDragOverProperty, value);
        }

        public static bool GetIsDragOver(DependencyObject element)
        {
            return (bool)element.GetValue(IsDragOverProperty);
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsDragAdorner(DependencyObject element, bool value)
        {
            element.SetValue(IsDragAdornerProperty, value);
        }

        public static bool GetIsDragAdorner(DependencyObject element)
        {
            return (bool)element.GetValue(IsDragAdornerProperty);
        }

        public static void SetDragAdorner(DependencyObject element, DragAdorner value)
        {
            element.SetValue(DragAdornerProperty, value);
        }

        public static DragAdorner GetDragAdorner(DependencyObject element)
        {
            return (DragAdorner)element.GetValue(DragAdornerProperty);
        }

        public static void SetDraggingElement(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(DraggingElementProperty, value);
        }

        public static FrameworkElement GetDraggingElement(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(DraggingElementProperty);
        }

        public static void SetTargetElement(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(TargetElementProperty, value);
        }

        public static FrameworkElement GetTargetElement(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(TargetElementProperty);
        }

        private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (e.NewValue != e.OldValue)
                {
                    if ((bool)e.NewValue)
                    {
                        element.AllowDrop = true;
                        element.DragEnter += OnDragEnter;
                        element.DragOver += OnDragOver;
                        element.DragLeave += OnDragLeave;
                        element.GiveFeedback += OnGiveFeedback;
                        element.Drop += OnDrop;
                        element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                    }
                    else
                    {
                        element.AllowDrop = false;
                        element.DragEnter -= OnDragOver;
                        element.DragOver -= OnDragOver;
                        element.DragLeave -= OnDragLeave;
                        element.GiveFeedback -= OnGiveFeedback;
                        element.Drop -= OnDrop;
                        element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                    }
                }
            }
        }

        private static void OnDragEnter(object sender, DragEventArgs e)
        {
            var d = sender as DependencyObject;
            d.SetValue(IsDragOverProperty, true);
            var targetItem = (e.OriginalSource as FrameworkElement).FindAncestor<ListViewItem>();

            if (targetItem != null)
            {
                var currentTargetItem = d.GetValue(TargetElementProperty) as FrameworkElement;

                if (currentTargetItem != null && currentTargetItem != targetItem)
                {
                    ResetContext(currentTargetItem.DataContext as IDraggableItem);
                }

                d.SetValue(TargetElementProperty, targetItem);
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var targetItemContext = (e.OriginalSource as FrameworkElement)?.DataContext as IDraggableItem;
            var droppedItemContext = e.Data.GetData(DraggableElementFormat) as IDraggableItem;
            var dragAndDropHandlerContext = (sender as FrameworkElement)?.DataContext as IDragAndDrop;

            if (targetItemContext != null && droppedItemContext != null && dragAndDropHandlerContext != null)
            {
                dragAndDropHandlerContext.OnElementDropped(droppedItemContext, targetItemContext);
            }

            ResetContext(targetItemContext);
            ResetContext(droppedItemContext);
        }

        private static void OnIsDragAdornerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.Name = DragAdornerName;
                }
                else
                {
                    element.Name = string.Empty;
                }
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
                Mouse.SetCursor(Cursors.Hand);
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
            var d = sender as DependencyObject;
            var adorner = d.GetValue(DragAdornerProperty) as DragAdorner;

            if (adorner != null)
            {
                if (adorner.Visibility != Visibility.Collapsed)
                {
                    adorner.Visibility = Visibility.Collapsed;
                }
            }

            var currentTargetItem = d.GetValue(TargetElementProperty) as FrameworkElement;

            if (currentTargetItem != null)
            {
                ResetContext(currentTargetItem.DataContext as IDraggableItem);
                d.SetValue(TargetElementProperty, null);
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            var d = sender as DependencyObject;
            var adorner = d.GetValue(DragAdornerProperty) as DragAdorner;
            d.SetValue(IsDragOverProperty, true);

            if (adorner != null)
            {
                if (adorner.Visibility != Visibility.Visible)
                {
                    adorner.Visibility = Visibility.Visible;
                }

                adorner.UpdatePosition(e.GetPosition(d.GetValue(DraggingElementProperty) as FrameworkElement));
            }

            var currentTargetItem = d.GetValue(TargetElementProperty) as FrameworkElement;

            if (currentTargetItem != null)
            {
                var context = currentTargetItem.DataContext as IDraggableItem;
                var targetElementPosition = e.GetPosition(currentTargetItem);
                var offset = context.DragOverPlaceholderPosition == Models.DragOverPlaceholderPosition.Top ? PlaceholderHeight : context.DragOverPlaceholderPosition == Models.DragOverPlaceholderPosition.Bottom ? -PlaceholderHeight : 0;
                var direction = targetElementPosition.Y < (currentTargetItem.ActualHeight + offset) / 2 ? Models.DragOverPlaceholderPosition.Top : Models.DragOverPlaceholderPosition.Bottom;
                context.DragOverPlaceholderPosition = direction;
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView container && e.OriginalSource is FrameworkElement dragArea && dragArea.Tag?.ToString() == DragSourceLabel && dragArea.DataContext is IDraggableItem)
            {
                Mouse.SetCursor(Cursors.Hand);
                var draggingItem = dragArea.FindAncestor<ListViewItem>();
                var elementToRenderInAdorner = draggingItem.FindChild(DragAdornerName) as FrameworkElement;
                AdornerLayer adornerLayer = null;
                DragAdorner adorner = null;
                container.SetValue(DraggingElementProperty, draggingItem);
                draggingItem.Visibility = Visibility.Collapsed;

                if (elementToRenderInAdorner != null)
                {
                    adornerLayer = AdornerLayer.GetAdornerLayer(container);
                    adorner = new DragAdorner(draggingItem, elementToRenderInAdorner, e.GetPosition(draggingItem));
                    adornerLayer.Add(adorner);
                    container.SetValue(DragAdornerProperty, adorner);
                }

                DragDrop.DoDragDrop(draggingItem, new DataObject(DraggableElementFormat, draggingItem.DataContext), DragDropEffects.Move);

                if (adorner != null)
                {
                    adornerLayer.Remove(adorner);
                    container.SetValue(DragAdornerProperty, null);
                }

                Mouse.SetCursor(OpenHandCursor);
                draggingItem.Visibility = Visibility.Visible;
                ResetContext(draggingItem.DataContext as IDraggableItem);
                var currentTargetItem = container.GetValue(TargetElementProperty) as FrameworkElement;
                ResetContext(currentTargetItem?.DataContext as IDraggableItem);
                container.SetValue(DraggingElementProperty, null);
                container.SetValue(TargetElementProperty, null);
            }
        }

        private static void ResetContext(IDraggableItem item)
        {
            if (item != null)
            {
                item.DragOverPlaceholderPosition = Models.DragOverPlaceholderPosition.None;
            }
        }
    }
}
