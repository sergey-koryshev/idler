namespace Idler.Components
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;

    /// <summary>
    /// Represents a custom popup control with arrow positioning and content templating support.
    /// Inherits from <see cref="System.Windows.Controls.Primitives.Popup"/> and provides additional
    /// features such as arrow alignment relative to the placement target and a <see cref="PopupContent"/> property
    /// for templated content.
    /// </summary>
    public class Popup : System.Windows.Controls.Primitives.Popup
    {
        /// <summary>
        /// Reference to the arrow element within the popup template.
        /// </summary>
        private FrameworkElement arrow;

        /// <summary>
        /// Reference to the wrapper element assigned to child of the popup.
        /// </summary>
        private ContentPresenter wrapperContent;

        /// <summary>
        /// Identifies the <see cref="PopupContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(
                nameof(PopupContent),
                typeof(UIElement),
                typeof(Popup),
                new PropertyMetadata(null, OnPopupContentPropertyChanged));

        /// <summary>
        /// Gets or sets the content to display within the popup.
        /// </summary>
        public UIElement PopupContent
        {
            get => (UIElement)GetValue(PopupContentProperty);
            set => SetValue(PopupContentProperty, value);
        }

        /// <summary>
        /// Initializes static members of the <see cref="Popup"/> class.
        /// Sets the default style key for the control.
        /// </summary>
        static Popup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Popup), new FrameworkPropertyMetadata(typeof(Popup)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Popup"/> class.
        /// </summary>
        public Popup()
        {
            this.InitializeWrapperElements();
            this.Opened += this.Popup_Opened;
        }

        /// <summary>
        /// Handles the Opened event to adjust the popup's position and arrow alignment.
        /// </summary>
        /// <param name="sender">The popup instance.</param>
        /// <param name="e">Event arguments.</param>
        private void Popup_Opened(object sender, System.EventArgs e)
        {
            if (this.arrow == null)
            {
                return;
            }

            Popup popup = sender as Popup;
            FrameworkElement target = popup.PlacementTarget as FrameworkElement;
            FrameworkElement popupChild = popup.Child as FrameworkElement;

            if (popup != null && target != null && popupChild != null)
            {
                // To calculate proper position, we use popupChild size instead of Popup itself because if Popup doesn't have size specified it's NaN.
                popup.SetValue(HorizontalOffsetProperty, (target.ActualWidth / 2.0) - (popupChild.ActualWidth / 2.0));

                Point popupPositionRelativeToTarget = popupChild.TranslatePoint(new Point(0, 0), target);
                double realHorizontalOffset = popupPositionRelativeToTarget.X;

                AdjustArrowPosition(realHorizontalOffset, popupPositionRelativeToTarget.Y <= 0 ? popupChild.ActualHeight - this.arrow.ActualHeight : 0, target, popupPositionRelativeToTarget.Y <= 0);
            }
        }

        /// <summary>
        /// Adjusts the arrow's position and orientation based on the popup's placement.
        /// </summary>
        /// <param name="horizontalOffset">The horizontal offset for the arrow.</param>
        /// <param name="verticalOffset">The vertical offset for the arrow.</param>
        /// <param name="target">The target element the popup is placed relative to.</param>
        /// <param name="isTop">Indicates if the arrow should point upwards.</param>
        private void AdjustArrowPosition(double horizontalOffset, double verticalOffset, FrameworkElement target, bool isTop)
        {
            if (this.arrow != null)
            {
                var transform = new RotateTransform();
                var translate = new TranslateTransform();
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(transform);
                transformGroup.Children.Add(translate);
                transform.Angle = isTop ? 0 : 180;
                translate.X = Math.Abs(horizontalOffset) + (target.ActualWidth / 2) - (this.arrow.ActualWidth / 2);
                translate.Y = verticalOffset;
                this.arrow.SetValue(RenderTransformProperty, transformGroup);
            }
        }

        /// <summary>
        /// Handles changes to the <see cref="PopupContent"/> property to update it in child's wrapper.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnPopupContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Popup popup && popup.wrapperContent != null)
            {
                popup.wrapperContent.Content = e.NewValue as UIElement;
            }
        }

        /// <summary>
        /// Initializes wrapper with arrow and sets to the child property of the <see cref="Popup"/>.
        /// </summary>
        private void InitializeWrapperElements()
        {
            this.arrow = new Polygon
            {
                Name = "PART_Arrow",
                Points = new PointCollection(new Point[]
                {
                    new Point(0,0),
                    new Point(12,0),
                    new Point(6,6)
                }),
                Fill = Brushes.White,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            this.wrapperContent = new ContentPresenter
            {
                Content = this.PopupContent
            };

            var contentBorder = new Border
            {
                Name = "PART_Content",
                Background = Brushes.White,
                SnapsToDevicePixels = true,
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(2, 6, 2, 6),
                Child = this.wrapperContent
            };

            var grid = new Grid
            {
                SnapsToDevicePixels = true,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 2,
                    ShadowDepth = 1,
                    Opacity = 0.2,
                    Color = Colors.Black
                }
            };

            grid.Children.Add(this.arrow);
            grid.Children.Add(contentBorder);

            this.Child = grid;
        }
    }
}
