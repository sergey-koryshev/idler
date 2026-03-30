namespace Idler.ViewModels
{
    using System.Threading.Tasks;
    using Idler.Components.PopupDialogHostControl;

    public abstract class BaseDialogViewModel : BaseViewModel, IClosableDialog
    {
        public virtual Task CloseDailog()
        {
            return Task.CompletedTask;
        }
    }
}
