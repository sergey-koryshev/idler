namespace Idler.Helpers.DragAndDrop
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class DragAdorner : Adorner
    {
        private VisualBrush visualBrush;
        private Point location;
        private Size elementRenderSize;
        private Point offset;

        public DragAdorner(ListViewItem adornedElement, FrameworkElement elementToRender, Point offset) : base(adornedElement)
        {
            this.visualBrush = new VisualBrush(elementToRender) { Stretch = Stretch.None };
            this.visualBrush.Opacity = 0.7;
            this.elementRenderSize = elementToRender.RenderSize;
            this.offset = offset;
            this.IsHitTestVisible = false;
            this.AllowDrop = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var newLocation = this.location;
            newLocation.Y -= this.offset.Y;
            newLocation.X -= this.offset.X;
            drawingContext.DrawRectangle(this.visualBrush, null, new Rect(newLocation, this.elementRenderSize));
        }

        public void UpdatePosition(Point location)
        {
            this.location = location;
            this.InvalidateVisual();
        }
    }
}
