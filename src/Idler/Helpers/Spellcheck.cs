namespace Idler.Helpers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Threading;

    public static class Spellcheck
    {
        private const int delayInterval = 300;
        private const string onlyOneModeMustBeEnabledErrorMessage = "Only 'IsEnabled' or 'IsEnabledOnFocus' can be enabled at the same time";

        private static InputLanguageManager CurrentLanguageManager => InputLanguageManager.Current;

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(Spellcheck), new FrameworkPropertyMetadata(default(bool), OnIsEnabledPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty IsEnabledOnFocusProperty = DependencyProperty.RegisterAttached(
        "IsEnabledOnFocus", typeof(bool), typeof(Spellcheck), new FrameworkPropertyMetadata(default(bool), OnIsEnabledOnFocusPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty ErrorsCountProperty = DependencyProperty.RegisterAttached(
        "ErrorsCount", typeof(int), typeof(Spellcheck), new FrameworkPropertyMetadata(default(int))
        {
            BindsTwoWayByDefault = false,
        });

        public static readonly DependencyProperty DelayTimerProperty = DependencyProperty.RegisterAttached(
        "DelayTimer", typeof(DispatcherTimer), typeof(Spellcheck), new FrameworkPropertyMetadata(null)
        {
            BindsTwoWayByDefault = false,
        });

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabledOnFocus(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledOnFocusProperty, value);
        }

        public static bool GetIsEnabledOnFocus(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledOnFocusProperty);
        }

        public static void SetErrorsCount(DependencyObject element, int value)
        {
            element.SetValue(ErrorsCountProperty, value);
        }

        public static int GetErrorsCount(DependencyObject element)
        {
            return (int)element.GetValue(ErrorsCountProperty);
        }

        public static void SetDelayTimer(DependencyObject element, DispatcherTimer value)
        {
            element.SetValue(DelayTimerProperty, value);
        }

        public static DispatcherTimer GetDelayTimer(DependencyObject element)
        {
            return (DispatcherTimer)element.GetValue(DelayTimerProperty);
        }

        private static void OnIsEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            if ((bool)textBox.GetValue(IsEnabledOnFocusProperty))
            {
                throw new InvalidOperationException(onlyOneModeMustBeEnabledErrorMessage);
            }

            HandleLanguageManager(textBox, e);

            if (e.NewValue != e.OldValue)
            {
                if ((bool)e.NewValue)
                {
                    textBox.SpellCheck.IsEnabled = true;
                    textBox.TextChanged += OnTextChanged;

                }
                else
                {
                    textBox.SpellCheck.IsEnabled = false;
                    textBox.TextChanged -= OnTextChanged;
                }
            }
        }

        private static void OnIsEnabledOnFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            if ((bool)textBox.GetValue(IsEnabledProperty))
            {
                throw new InvalidOperationException(onlyOneModeMustBeEnabledErrorMessage);
            }

            HandleLanguageManager(textBox, e);

            if (e.NewValue != e.OldValue)
            {
                if ((bool)e.NewValue)
                {
                    textBox.GotFocus += OnGotFocus;
                    textBox.LostFocus += OnLostFocus;
                    textBox.TextChanged += OnTextChanged;
                }
                else
                {
                    textBox.SpellCheck.IsEnabled = false;
                    textBox.GotFocus -= OnGotFocus;
                    textBox.LostFocus -= OnLostFocus;
                    textBox.TextChanged -= OnTextChanged;
                }
            }
        }

        private static void HandleLanguageManager(TextBox textBox, DependencyPropertyChangedEventArgs e)
        {
            InputLanguageEventHandler onInputLanguageChanged = (s, args) =>
            {
                textBox.Language = GetSpellCheckLanguage(args.NewLanguage.Name);
                textBox.SetValue(ErrorsCountProperty, CountErrors(textBox));
            };

            textBox.Language = GetSpellCheckLanguage(CurrentLanguageManager.CurrentInputLanguage.Name);

            if (e.NewValue != e.OldValue)
            {
                if ((bool)e.NewValue)
                {
                    CurrentLanguageManager.InputLanguageChanged += onInputLanguageChanged;
                }
                else
                {
                    CurrentLanguageManager.InputLanguageChanged -= onInputLanguageChanged;
                }
            }
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e) => ((TextBox)sender).SpellCheck.IsEnabled = true;

        private static void OnLostFocus(object sender, RoutedEventArgs e) => ((TextBox)sender).SpellCheck.IsEnabled = false;

        private static XmlLanguage GetSpellCheckLanguage(string ietfLanguageTag) => XmlLanguage.GetLanguage(ietfLanguageTag);

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            var delayTimer = textBox.GetValue(DelayTimerProperty) as DispatcherTimer;

            if (delayTimer != null)
            {
                delayTimer.Stop();
            }

            if (delayTimer == null)
            {
                delayTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(delayInterval), DispatcherPriority.Background, (s, args) =>
                {
                    textBox.SetValue(ErrorsCountProperty, CountErrors(textBox));
                    delayTimer.Stop();
                }, textBox.Dispatcher);

                textBox.SetValue(DelayTimerProperty, delayTimer);
            }

            delayTimer.Start();
        }

        private static int CountErrors(TextBox textBox)
        {
            if (textBox.Text.Length == 0)
            {
                return 0;
            }

            var targetTextBox = textBox.SpellCheck.IsEnabled ? textBox : GetPhantomTextBox(textBox);

            int errors = 0;

            for (int i = 0; i < targetTextBox.Text.Length; i++)
            {
                if (targetTextBox.GetSpellingError(i) != null)
                {
                    errors++;
                    i += targetTextBox.GetSpellingErrorLength(i);
                }
            }

            return errors;
        }

        private static TextBox GetPhantomTextBox(TextBox textBox)
        {
            var phantomTextBox = new TextBox()
            {
                Language = textBox.Language,
                Text = textBox.Text
            };

            phantomTextBox.SpellCheck.IsEnabled = true;

            return phantomTextBox;
        }
    }
}
