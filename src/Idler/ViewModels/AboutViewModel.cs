namespace Idler.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Input;
    using Idler.Commands;

    public class AboutViewModel : BaseViewModel
    {
        private string copyright;
        private ICommand openGitHubUrlCommand;
        private string version;

        public string Copyright
        {
            get { return copyright; }
            set
            {
                copyright = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand OpenGitHubUrlCommand
        {
            get => openGitHubUrlCommand;
            set
            {
                openGitHubUrlCommand = value;
                this.OnPropertyChanged();
            }
        }

        public string Version
        {
            get => version;
            set
            {
                version = value;
                this.OnPropertyChanged();
            }
        }

        public AboutViewModel()
        {
            this.OpenGitHubUrlCommand = new OpenUrlCommand();
            this.Copyright = $"Copyright © 2020-{DateTime.Now.Year}  Sergey Koryshev";
#if DEBUG
            this.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
#else
            this.Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
#endif
        }
    }
}
