namespace Idler
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using Idler.Extensions;
    using Idler.Helpers.DB;
    using Idler.Helpers.MVVM;
    using Idler.Helpers.Notifications;
    using Microsoft.Toolkit.Uwp.Notifications;

    /// <summary>
    /// Represents a single row from table Shift
    /// </summary>
    public class Shift : UpdatableObject
    {
        private readonly NotificationsManager notificationsManager;

        private ObservableCollection<ShiftNote> notes = new ObservableCollection<ShiftNote>();
        private DateTime selectedDate;
        private DispatcherTimer reminder;
        private bool ignorefirstReminder = true;

        /// <summary>
        /// Gets/sets collection of Shift Notes
        /// </summary>
        public ObservableCollection<ShiftNote> Notes
        {
            get => this.notes;
            set
            {
                this.notes = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedDate
        {
            get => selectedDate;
            set
            {
                selectedDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets total effort
        /// </summary>
        public decimal TotalEffort
        {
            get
            {
                return Notes.Sum(x => x.Effort);
            }
        }

        /// <summary>
        /// Returns true if reminder is enabled
        /// </summary>
        public bool IsReminderEnabled
        {
            get => Properties.Settings.Default.ReminderInterval.Ticks > 0 &&
                Properties.Settings.Default.IsReminderEnabled;
            set
            {
                Properties.Settings.Default.IsReminderEnabled = value;
                Properties.Settings.Default.Save();
                this.notificationsManager?.ShowInfo($"Reminders are {(value == true ? "enabled" : "disabled")}.");
            }
        }

        public int TotalErrorsCount
        {
            get
            {
                return Notes.Sum(x => x.ErrorsCount);
            }
        }

        public Shift()
        {
            this.Notes.CollectionChanged += NotesCollectionChangedHandler;
            Properties.Settings.Default.SettingsSaving += OnSettignsSaving;
            this.InitializeReminer();
            this.OnPropertyChanged(nameof(this.IsReminderEnabled), true);
            this.notificationsManager = NotificationsManager.GetInstance();
        }

        private void NotesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Changed != true)
            {
                this.Changed = true;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ShiftNote newShiftNote in e.NewItems)
                    {
                        newShiftNote.PropertyChanged += ShiftNotePropertyChangedHandler;
                    }
                    break;
            }

            OnPropertyChanged(nameof(this.TotalEffort));
            OnPropertyChanged(nameof(this.TotalErrorsCount));
        }

        /// <summary>
        /// Handler for event "PropertyChanged" of class ShiftNote
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShiftNotePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ShiftNote.Effort):
                    OnPropertyChanged(nameof(this.TotalEffort));
                    break;
                case nameof(ShiftNote.ErrorsCount):
                    OnPropertyChanged(nameof(this.TotalErrorsCount));
                    break;
                case nameof(ShiftNote.Changed):
                    if (this.Changed != true)
                    {
                        this.Changed = ((ShiftNote)sender).Changed;
                    }
                    break;
            }
        }

        /// <summary>
        /// Retrieves values from DataBase
        /// </summary>
        protected override async Task RefreshInternalAsync()
        {
            this.Notes.Clear();

            int[] shiftNoteIds = await Task.Run(async () => await ShiftNote.GetNotesByDate(this.SelectedDate));

            foreach (int shiftNoteId in shiftNoteIds)
            {
                ShiftNote newNote = new ShiftNote(this.Notes);
                this.Notes.Add(newNote);
                newNote.Id = shiftNoteId;
                await newNote.RefreshAsync();
            }
        }

        /// <summary>
        /// Saves all changes in properties in DataBase
        /// </summary>
        protected override async Task UpdateInternalAsync()
        {
            foreach (ShiftNote shiftNote in this.Notes)
            {
                if (shiftNote.Changed == true)
                {
                    await shiftNote.UpdateAsync();
                }
            }

            int[] originShiftIDs = await ShiftNote.GetNotesByDate(this.SelectedDate);

            int[] diff = originShiftIDs.Except(from shiftNote in this.Notes select (int)shiftNote.Id).ToArray();

            foreach (int shiftNoteIdToDelete in diff)
            {
                await ShiftNote.RemoveShiftNoteByShiftNoteId(shiftNoteIdToDelete);
            }
        }

        /// <summary>
        /// Adds new Shift Note to list
        /// </summary>
        /// <param name="shiftNote"></param>
        public void AddNewShiftNote(ShiftNote shiftNote)
        {
            this.Notes.Add(shiftNote);
        }

        private void OnSettignsSaving(object sender, CancelEventArgs e)
        {
            this.reminder.Interval = Properties.Settings.Default.ReminderInterval;
            if (this.IsReminderEnabled)
            {
                this.reminder.Start();
            }
            else
            {
                this.reminder.Stop();
            }
            this.OnPropertyChanged(nameof(this.IsReminderEnabled));
        }

        private void InitializeReminer()
        {
            this.reminder = new DispatcherTimer();
            this.reminder.Tick += OnReminderActivated;
            this.reminder.Interval = Properties.Settings.Default.ReminderInterval;
            if (this.IsReminderEnabled)
            {
                this.reminder.Start();
            }
            else
            {
                this.reminder.Stop();
            }
        }

        private void OnReminderActivated(object sender, EventArgs e)
        {
            if (this.ignorefirstReminder)
            {
                this.ignorefirstReminder = false;
                return;
            }

            var dailyWorkload = Properties.Settings.Default.DailyWorkLoad;
            var today = DateTime.Now.Date;

            var totalEffortRetrieveTask = this.SelectedDate.Date == today.Date
                ? Task.Run(() => this.TotalEffort)
                : DataBaseFunctions.GetTotalEffort(today);

            totalEffortRetrieveTask.SafeAsyncCall(totalEffort =>
            {
                if (dailyWorkload > 0 && totalEffort >= dailyWorkload)
                {
                    return;
                }

                new ToastContentBuilder()
                    .AddArgument("action", "remindFillReport")
                    .AddAppLogoOverride(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/reminder-icon.png")))
                    .AddText("Idler Reminder")
                    .AddText("Hey! Just remind you to fill your current work progress.")
                    .Show();
            }, errorCallback: ex => Trace.TraceError($"Error has occurred while retrieving total effort for '{today.Date:d}': {ex.Message}"));
        }

        public void ResetReminder()
        {
            if (this.IsReminderEnabled)
            {
                this.reminder.Stop();
                this.reminder.Start();
            }
        }        
    }
}
