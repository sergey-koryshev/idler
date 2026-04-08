namespace Idler.Components.PopupDialogHostControl
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents closable dialog.
    /// </summary>
    public interface IClosableDialog
    {
        /// <summary>
        /// Performs asynchronous operations when a dialog is closing.
        /// </summary>
        /// <remarks>Ensure you handled all exceptions within the method.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task OnDialogClosing();
    }
}
