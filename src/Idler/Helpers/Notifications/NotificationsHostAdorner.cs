namespace Idler.Helpers.Notifications
{
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Idler.ViewModels;
    using Idler.Views;

    public class NotificationsHostAdorner : Adorner
    {
        private const double notificationXMargin = 5;
        private const double notificationTopMargin = 5;

        private readonly VisualCollection visuals;

        public NotificationsHostAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.visuals = new VisualCollection(this);
            this.UseLayoutRounding = true;
        }

        protected override int VisualChildrenCount => this.visuals.Count;        

        protected override Visual GetVisualChild(int index) => this.visuals[index];

        protected override Size MeasureOverride(Size constraint)
        {
            var finalMeazure = base.MeasureOverride(constraint);

            for (int i = 0; i < this.VisualChildrenCount; i++)
            {
                var uiElement = this.GetVisualChild(i) as UIElement;
                uiElement.Measure(new Size(finalMeazure.Width - notificationXMargin * 2, uiElement.DesiredSize.Height));
            }

            return finalMeazure;
        }

        protected override Size ArrangeOverride(Size size)
        {
            var finalSize = base.ArrangeOverride(size);

            double totalHeight = 0;
            double totalWidth = finalSize.Width - notificationXMargin * 2;

            for (int i = 0; i < this.VisualChildrenCount; i++)
            {
                totalHeight += notificationTopMargin;

                var uiElement = this.GetVisualChild(i) as UIElement;
                uiElement.Arrange(new Rect(new Point(notificationXMargin, totalHeight), new Size(totalWidth, uiElement.DesiredSize.Height)));

                totalHeight += uiElement.DesiredSize.Height;
            }

            return finalSize;
        }

        public void AddNotificationVisual(NotificationViewModel notificationViewModel)
        {
            notificationViewModel.VisualReference = new NotificationView { DataContext = notificationViewModel };
            this.visuals.Add(notificationViewModel.VisualReference);
            this.RearrangeVisuals();
        }

        public void RemoveNotificationVisual(NotificationViewModel notificationViewModel)
        {
            this.visuals.Remove(notificationViewModel.VisualReference);
            this.RearrangeVisuals();
        }

        private void RearrangeVisuals()
        {
            this.InvalidateMeasure();
            this.InvalidateArrange();
        }
    }
}
