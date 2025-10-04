namespace Idler.Commands
{
    using System.ComponentModel;

    public class RefreshViewCommand : CommandBase
    {
        private readonly ICollectionView view;

        public RefreshViewCommand(ICollectionView view)
        {
            this.view = view;
        }

        public override void Execute(object parameter)
        {
            this.view.Refresh();
        }
    }
}
