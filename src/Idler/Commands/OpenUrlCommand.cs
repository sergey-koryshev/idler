namespace Idler.Commands
{
    using System.Diagnostics;

    public class OpenUrlCommand : CommandBase
    {
        private readonly string url;

        public OpenUrlCommand() : this(null) { }

        public OpenUrlCommand(string url)
        {
            this.url = url;
        }

        public override void Execute(object parameter)
        {
            var targetUrl = this.url ?? parameter?.ToString();

            if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                Process.Start(new ProcessStartInfo(targetUrl));
            }
        }
    }
}
