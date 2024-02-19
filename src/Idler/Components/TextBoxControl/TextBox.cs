namespace Idler.Components
{
    using System.Windows;

    /// <summary>
    /// Represents custom TextBox with overridden Drag-and-Drop event handlers
    /// since standard component has built-in logic to handle Drag-and-Drop events and
    /// it interferes with another drag operations 
    /// </summary>
    public class TextBox : System.Windows.Controls.TextBox
    {
        protected override void OnDragEnter(DragEventArgs e) { }

        protected override void OnDragOver(DragEventArgs e) { }

        protected override void OnDragLeave(DragEventArgs e) { }
    }
}
