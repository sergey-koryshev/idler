using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Idler.Components
{
    public class PopUpWrapper : ContentControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PopUpWrapper), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(PopUpWrapper), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static PopUpWrapper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopUpWrapper), new FrameworkPropertyMetadata(typeof(PopUpWrapper)));
        }
    }
}
