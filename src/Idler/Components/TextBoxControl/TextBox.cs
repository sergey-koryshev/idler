namespace Idler.Components
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using FontWeights = System.Windows.FontWeights;

    public class TextBox: System.Windows.Controls.TextBox
    {
        public static readonly DependencyProperty RightControlProperty =
            DependencyProperty.Register("RightControl", typeof(FrameworkElement), typeof(TextBox), new PropertyMetadata(null));

        public FrameworkElement RightControl
        {
            get => (FrameworkElement)GetValue(RightControlProperty);
            set
            {
                SetValue(RightControlProperty, value);
            }
        }

        public static readonly DependencyProperty SuggestionTextProperty =
            DependencyProperty.Register(nameof(SuggestionText), typeof(string), typeof(TextBox), new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnSuggestionTextChanged));

        public string SuggestionText
        {
            get => (string)GetValue(SuggestionTextProperty);
            set
            {
                SetValue(SuggestionTextProperty, value);
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

        private SuggestionAdorner _suggestionAdorner;
        private AdornerLayer _adornerLayer;

        private void UpdateSuggestionVisual()
        {
            if (_adornerLayer == null)
            {
                _adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (_adornerLayer == null) return;
            }

            // Remove old one
            if (_suggestionAdorner != null)
            {
                _adornerLayer.Remove(_suggestionAdorner);
                _suggestionAdorner = null;
            }

            if (string.IsNullOrEmpty(SuggestionText) || !IsFocused || CaretIndex != Text.Length)
                return;

            _suggestionAdorner = new SuggestionAdorner(this, SuggestionText);
            _adornerLayer.Add(_suggestionAdorner);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateSuggestionVisual();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateSuggestionVisual();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateSuggestionVisual();
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateSuggestionVisual();
        }

        // SuggestionText property changed callback
        private static void OnSuggestionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb) tb.UpdateSuggestionVisual();
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape && _suggestionAdorner != null)
            {
                if (_suggestionAdorner != null)
                {
                    _adornerLayer.Remove(_suggestionAdorner);
                    _suggestionAdorner = null;
                }

                InvalidateVisual();
                e.Handled = true;
            }
            else if (e.Key == Key.Right && _suggestionAdorner != null)
            {
                if (CaretIndex == Text.Length) // only at the end
                {
                    // Accept suggestion
                    var currentText = Text;
                    Text = currentText + SuggestionText;
                    CaretIndex = Text.Length;

                    if (_suggestionAdorner != null)
                    {
                        _adornerLayer.Remove(_suggestionAdorner);
                        _suggestionAdorner = null;
                    }

                    e.Handled = true;
                }
            }
        }
    }

    public class SuggestionAdorner : Adorner
    {
        private readonly string _suggestion;
        private readonly Typeface _typeface;
        private readonly Brush _brush;

        public SuggestionAdorner(UIElement adornedElement, string suggestion)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            SnapsToDevicePixels = true;
            _suggestion = suggestion ?? "";

            var tb = (TextBox)AdornedElement;
            if (tb == null) throw new Exception();

           

            _suggestion = suggestion ?? "";
            _typeface = new Typeface(
                tb.FontFamily,
                tb.FontStyle,
                tb.FontWeight,
                tb.FontStretch);

            _brush = Brushes.Gray;  // or #FF888888, etc.
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (string.IsNullOrEmpty(_suggestion)) return;

            var textBox = (TextBox)AdornedElement;
            if (!textBox.IsFocused || textBox.CaretIndex != textBox.Text.Length)
                return;

            string prefix = textBox.Text ?? "";

            var suggestionFormatted = new FormattedText(
                _suggestion,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                _typeface,
                textBox.FontSize,
                _brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var caretRect = textBox.GetRectFromCharacterIndex(textBox.Text.Length, true); // trailing = true

            double x = caretRect.Left
                     + textBox.Padding.Left
                     + textBox.BorderThickness.Left;

            // Position right after real text
            //double x = prefixFormatted.WidthIncludingTrailingWhitespace + textBox.Padding.Left + textBox.BorderThickness.Left + 2;
            double y = (textBox.ActualHeight - suggestionFormatted.Height) / 2;

            suggestionFormatted.MaxTextWidth = Math.Max(0, textBox.ViewportWidth - caretRect.Left - 4);
            suggestionFormatted.Trimming = TextTrimming.CharacterEllipsis;
            suggestionFormatted.MaxLineCount = 1;

            // Optional: respect vertical alignment better
            // y -= textBox.Padding.Top;  // adjust if needed

            drawingContext.DrawText(suggestionFormatted, new Point(x, y));
        }
    }
}
