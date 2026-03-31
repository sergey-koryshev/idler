namespace Idler.ViewModels
{
    using System.Threading.Tasks;
    using Idler.Components.PopupDialogHostControl;

    public abstract class BaseDialogViewModel : BaseViewModel, IClosableDialog
    {
        public abstract Task CloseDailog();
    }
}
