namespace Idler.Components
{
    using System.Windows;

    public class TextBox : System.Windows.Controls.TextBox
    {
        public static readonly DependencyProperty RightControlProperty =
            DependencyProperty.Register("RightControl", typeof(FrameworkElement), typeof(TextBox), new PropertyMetadata(null));

        public FrameworkElement RightControl
        {
            get => (FrameworkElement)GetValue(RightControlProperty);
            set {
                SetValue(RightControlProperty, value);

            }
        }

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        // Drag-and-Drop event handlers are overridden
        // since standard component has built-in logic
        // to handle Drag-and-Drop events and
        // it interferes with another drag operations 
        protected override void OnDragEnter(DragEventArgs e) { }

        protected override void OnDragOver(DragEventArgs e) { }

        protected override void OnDragLeave(DragEventArgs e) { }
    }
}
