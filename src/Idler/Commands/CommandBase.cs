namespace Idler.Commands
{
    using System;
    using System.Windows.Input;

    public abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// An optional parameter that can be used to influence the command's execution logic.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the command can execute; otherwise, <see langword="false"/>. The command cannot
        /// execute if the category change was triggered programmatically.
        /// </returns>
        public virtual bool CanExecute(object parameter) => true;

        public abstract void Execute(object parameter);

        protected void OnCanExecutedChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
