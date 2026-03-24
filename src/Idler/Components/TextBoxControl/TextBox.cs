namespace Idler.Components
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using Idler.Components.TextBoxControl;

    public class TextBox : System.Windows.Controls.TextBox
    {
        private SuggestingTextAdorner suggestingTextAdorner;
        private AdornerLayer adornerLayer;

        public static readonly DependencyProperty RightControlProperty =
            DependencyProperty.Register(nameof(RightControl), typeof(FrameworkElement), typeof(TextBox), new PropertyMetadata(null));

        public FrameworkElement RightControl
        {
            get => (FrameworkElement)GetValue(RightControlProperty);
            set
            {
                SetValue(RightControlProperty, value);
            }
        }

        public static readonly DependencyProperty SuggestingTextProperty =
            DependencyProperty.Register(nameof(SuggestingText), typeof(string), typeof(TextBox), new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnSuggestionTextChanged));

        public string SuggestingText
        {
            get => (string)GetValue(SuggestingTextProperty);
            set
            {
                SetValue(SuggestingTextProperty, value);
            }
        }

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.UpdateSuggestingTextVisual();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            this.UpdateSuggestingTextVisual();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            this.UpdateSuggestingTextVisual();
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.UpdateSuggestingTextVisual();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape && this.suggestingTextAdorner != null)
            {
                this.ClearSuggestingTextAdorner();
                this.InvalidateVisual();
                e.Handled = true;
            }
            else if (e.Key == Key.Right && this.suggestingTextAdorner != null)
            {
                if (this.CaretIndex == this.Text.Length)
                {
                    var currentText = this.Text;
                    this.Text = currentText + this.SuggestingText;
                    this.CaretIndex = this.Text.Length;
                    this.ClearSuggestingTextAdorner();
                    e.Handled = true;
                }
            }
        }

        // Drag-and-Drop event handlers are overridden
        // since standard component has built-in logic
        // to handle Drag-and-Drop events and
        // it interferes with another drag operations
        protected override void OnDragEnter(DragEventArgs e) { }

        protected override void OnDragOver(DragEventArgs e) { }

        protected override void OnDragLeave(DragEventArgs e) { }

        private static void OnSuggestionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.UpdateSuggestingTextVisual();
            }
        }

        private void UpdateSuggestingTextVisual()
        {
            this.ClearSuggestingTextAdorner();
            
            if (this.adornerLayer == null)
            {
                this.adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (this.adornerLayer == null)
                {
                    return;
                }

                // sometimes adorner layer is unloaded so we have to recreate it again
                // it happens, for example, after waking PC up or just after Lock Screen.
                this.adornerLayer.Unloaded += (s, e) =>
                {
                    this.adornerLayer = AdornerLayer.GetAdornerLayer(this);
                };
            }

            if (string.IsNullOrEmpty(this.SuggestingText) || string.IsNullOrEmpty(this.Text) || !this.IsFocused)
            {
                return;
            }

            this.suggestingTextAdorner = new SuggestingTextAdorner(this);
            this.adornerLayer.Add(suggestingTextAdorner);
        }

        private void ClearSuggestingTextAdorner()
        {
            if (this.suggestingTextAdorner != null)
            {
                this.adornerLayer?.Remove(this.suggestingTextAdorner);
                this.suggestingTextAdorner = null;
            }
        }
    }
}
