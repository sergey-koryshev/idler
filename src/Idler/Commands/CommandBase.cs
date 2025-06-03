namespace Idler.Commands
{
    using System;
    using System.Windows.Input;

    public abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter) => true;

        public abstract void Execute(object parameter);

        protected void OnCanExecutedChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
