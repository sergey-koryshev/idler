using Idler.Commands;
using Idler.Helpers.DB;
using Microsoft.Win32;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Idler.ViewModels
{
    public class ExportNotesViewModel : BaseViewModel
    {
		private DateTime dateTo;
        private DateTime dateFrom;
        private string pathToSave;
        private ICommand exportCommand;
        private ICommand getPathToSaveCommand;
        private bool isExporting;
        private IEnumerable<Models.ShiftNote> notes;
        private Task worker;
        private bool isFetching;
        private string error;
        private string resultMessage;
        private string excelTemplate;
        private bool isExcelTemplateUsed;

        public DateTime DateTo
        {
            get => dateTo;
            set 
            { 
                dateTo = value;
                this.OnPropertyChanged();
            }
        }

        public DateTime DateFrom 
        { 
            get => dateFrom; 
            set 
            {
                dateFrom = value;
                this.OnPropertyChanged();
            }
        }

        public string PathToSave 
        { 
            get => pathToSave;
            set
            {
                pathToSave = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ExportCommand 
        { 
            get => exportCommand;
            set
            {
                exportCommand = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand GetPathToSaveCommand 
        { 
            get => getPathToSaveCommand;
            set
            {
                getPathToSaveCommand = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsExporting 
        { 
            get => isExporting;
            set
            {
                isExporting = value;
                this.OnPropertyChanged();
            }
        }

        public IEnumerable<Models.ShiftNote> Notes 
        { 
            get => notes;
            set
            {
                notes = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsFetching 
        { 
            get => isFetching;
            set
            {
                isFetching = value;
                this.OnPropertyChanged();
            }
        }

        public string Error
        {
            get => error;
            set
            {
                error = value;
                this.OnPropertyChanged();
            }
        }

        public string ResultMessage
        {
            get => resultMessage;
            set
            {
                resultMessage = value;
                this.OnPropertyChanged();
            }
        }

        public string ExcelTemplate 
        { 
            get => excelTemplate;
            set
            {
                excelTemplate = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsExcelTemplateUsed 
        {
            get => isExcelTemplateUsed;
            set
            {
                isExcelTemplateUsed = value;
                this.OnPropertyChanged();
            }
        }

        public decimal? TotalEffort
        {
            get => this.Notes?.Sum(x => x.Effort);
        }

        public ExportNotesViewModel()
        {
            this.IsExcelTemplateUsed = Properties.Settings.Default.IsExcelTemplateUsed &&
                !string.IsNullOrWhiteSpace(Properties.Settings.Default.ExcelTemplate);
            this.PropertyChanged += PropertyChangedHandler;
            var currentDate = DateTime.Now;
            this.DateFrom = new DateTime(currentDate.Year, currentDate.Month, 1);
            this.DateTo = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            this.GetPathToSaveCommand = new RelayCommand(this.GetPathToSave);
            this.ExportCommand = new RelayCommand(
                this.ExportNotes,
                () => !string.IsNullOrWhiteSpace(this.PathToSave) 
                    && this.Notes?.Count() > 0 
                    && !this.IsExporting
                    && !this.IsFetching);
            this.ExcelTemplate = string.IsNullOrWhiteSpace(Properties.Settings.Default.ExcelTemplate)
                ? null : Properties.Settings.Default.ExcelTemplate;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.DateTo):
                case nameof(this.DateFrom):
                    if (this.DateFrom != default(DateTime) && this.DateTo != default(DateTime) && this.DateFrom <= this.DateTo) {
                        this.FetchNotes();
                    }
                    break;
                case nameof(this.IsExcelTemplateUsed):
                    Properties.Settings.Default.IsExcelTemplateUsed = this.IsExcelTemplateUsed;
                    Properties.Settings.Default.Save();
                    break;
            }
        }

        private void GetPathToSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Microsoft Excel (*.xlsx)|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                this.PathToSave = saveFileDialog.FileName;
            }
        }

        private void FetchNotes()
        {
            this.worker = Task.Run(async () =>
            {
                try
                {
                    this.SetResult(null, null);
                    this.IsFetching = true;
                    this.Notes =await DataBaseFunctions.GetNotesByDates(this.DateFrom, this.DateTo);
                    this.OnPropertyChanged(nameof(this.TotalEffort));
                }
                catch (Exception ex)
                {
                    this.SetResult("Fetching Failed!", ex.Message);
                }
                finally
                {
                    this.IsFetching = false;
                }
            });
        }

        private void ExportNotes()
        {
            this.worker = Task.Run(() =>
            {
                try
                {
                    this.IsExporting = true;
                    this.SetResult(null, null);

                    if (this.IsExcelTemplateUsed)
                    {
                        MiniExcel.SaveAsByTemplate(this.PathToSave, this.ExcelTemplate, new { notes = this.Notes });
                    }
                    else
                    {
                        MiniExcel.SaveAs(this.PathToSave, this.Notes);
                    }

                    this.SetResult("Export Completed!", null);

                }
                catch (Exception ex) 
                {
                    this.SetResult("Export Failed!", ex.Message);
                }
                finally
                {
                    this.IsExporting = false;
                }
            });
        }

        private void SetResult(string result, string error = null)
        {
            this.ResultMessage = result;
            this.Error = error;
        }
    }
}
