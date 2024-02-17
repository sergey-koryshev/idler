namespace Idler.Components
{
    using System.Windows;

    public class TextBox : System.Windows.Controls.TextBox
    {
        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Handled = false;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Handled = false;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            e.Handled = false;
        }
    }
}
