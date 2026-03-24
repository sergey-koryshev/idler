namespace Idler.Components.TextBoxControl
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class SuggestingTextAdorner: Adorner
    {
        private const double iconSize = 9;
        private const double iconGap = 4;
        private const string iconResourceName = "RightArrowIcon";

        private readonly Brush brush;

        public SuggestingTextAdorner(TextBox textBox) : base(textBox)
        {
            this.IsHitTestVisible = false;
            this.SnapsToDevicePixels = true;
            this.brush = new SolidColorBrush(Color.FromRgb(146, 146, 146));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var textBox = this.AdornedElement as TextBox;

            if (textBox == null || string.IsNullOrEmpty(textBox.Text) || string.IsNullOrEmpty(textBox.SuggestingText) || !textBox.IsFocused )
            {
                return;
            }

            var typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);

            var dpi = VisualTreeHelper.GetDpi(textBox).PixelsPerDip;
            var culture = CultureInfo.CurrentUICulture;

            var formattedText = new FormattedText(
                textBox.Text,
                culture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                textBox.Foreground,
                dpi);

            double x = formattedText.WidthIncludingTrailingWhitespace + textBox.Padding.Left + textBox.BorderThickness.Left + 3;
            double y = (textBox.ActualHeight - formattedText.Height) / 2;
            var availableWidth = Math.Max(0, textBox.ActualWidth - textBox.BorderThickness.Right - textBox.Padding.Right - x);

            if (availableWidth == 0)
            {
                return;
            }

            var icon = Application.Current.Resources[iconResourceName] as DrawingImage;
            var maxTextWidth = icon == null ? availableWidth : Math.Max(0, availableWidth - (iconGap + iconSize));

            if (maxTextWidth == 0)
            {
                return;
            }

            var formattedSuggestingText = new FormattedText(
                textBox.SuggestingText,
                culture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                this.brush,
                dpi)
            {
                MaxLineCount = 1,
                MaxTextWidth = maxTextWidth,
                Trimming = TextTrimming.CharacterEllipsis
            };
            
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, textBox.ActualWidth, textBox.ActualHeight)));
            drawingContext.DrawText(formattedSuggestingText, new Point(x, y));

            if (icon != null)
            {
                double arrowX = x + formattedSuggestingText.WidthIncludingTrailingWhitespace + iconGap;
                double arrowY = (textBox.ActualHeight - formattedText.Height) / 2;
                drawingContext.DrawImage(icon, new Rect(arrowX, arrowY + iconSize / 2, iconSize, iconSize));
            }

            drawingContext.Pop();
        }
    }
}
