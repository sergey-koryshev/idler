namespace Idler.Components
{
    using Idler.Extensions;
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;

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
        /// Identifies the <see cref="PopupContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(
                nameof(PopupContent),
                typeof(UIElement),
                typeof(Popup),
                new PropertyMetadata(null));

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
            DependencyPropertyDescriptor.FromProperty(Popup.ChildProperty, typeof(Popup)).AddValueChanged(this, OnChildPropertyChanged);
            this.Opened += Popup_Opened;
        }

        /// <summary>
        /// Handles the Opened event to adjust the popup's position and arrow alignment.
        /// </summary>
        /// <param name="sender">The popup instance.</param>
        /// <param name="e">Event arguments.</param>
        private void Popup_Opened(object sender, System.EventArgs e)
        {
            Popup popup = sender as Popup;
            FrameworkElement target = popup.PlacementTarget as FrameworkElement;
            FrameworkElement popupChild = popup.Child as FrameworkElement;

            if (popup != null && target != null && popupChild != null)
            {
                popup.SetValue(HorizontalOffsetProperty, (target.ActualWidth / 2.0) - (popup.Width / 2.0));

                Point popupPositionRelativeToTarget = popupChild.TranslatePoint(new Point(0, 0), target);
                double realHorizontalOffset = popupPositionRelativeToTarget.X;

                AdjustArrowPosition(realHorizontalOffset, popupPositionRelativeToTarget.Y <= 0 ? popup.Height - this.arrow.ActualHeight : 0, target, popupPositionRelativeToTarget.Y <= 0);
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
        /// Handles changes to the Child property and locates the arrow element in the visual tree.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnChildPropertyChanged(object sender, EventArgs e)
        {
            this.arrow = this.Child?.FindChild("PART_Arrow") as FrameworkElement;
        }
    }
}
