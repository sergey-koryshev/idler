using Idler.Properties;
namespace Idler.Commands
{
    using System;
    using Microsoft.Win32;

    internal class LaunchOpenDialogCommand : CommandBase
    {
        private readonly string filter;
        private readonly Action<OpenFileDialog> callback;

        public LaunchOpenDialogCommand(string filter, Action<OpenFileDialog> callback)
        {
            this.filter = filter;
            this.callback = callback;
        }

        public override void Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = this.filter
            };

            if (dialog.ShowDialog() == true)
            {
                this.callback?.Invoke(dialog);
            }
        }
    }
}
