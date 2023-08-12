using Idler.Components.PopupDialogControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Idler.Components
{
    public class PopupDialog : Control
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PopupDialog), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(PopupDialog), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(Buttons), typeof(PopupDialog), new PropertyMetadata(Buttons.OkCancel));

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(Result), typeof(PopupDialog), new PropertyMetadata(Result.None));

        public static readonly DependencyProperty OkCommandProperty =
            DependencyProperty.Register("OkCommand", typeof(ICommand), typeof(PopupDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(PopupDialog), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public Buttons Buttons
        {
            get { return (Buttons)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public Result Result
        {
            get { return (Result)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public ICommand OkCommand
        {
            get { return (ICommand)GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        static PopupDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupDialog), new FrameworkPropertyMetadata(typeof(PopupDialog)));
        }

        public PopupDialog()
        {
            this.OkCommand = new OkCommand(this);
            this.CancelCommand = new CancelCommand(this);
        }
    }
}
