namespace Idler.Commands
{
    using Idler.ViewModels;

    public class ResumeAutoCategorization : CommandBase
    {
        private AddNoteViewModel addNoteViewModel;

        public ResumeAutoCategorization(AddNoteViewModel addNoteViewModel)
        {
            this.addNoteViewModel = addNoteViewModel;
            this.addNoteViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddNoteViewModel.CategoryChangedProgrammatically))
                {
                    this.OnCanExecutedChanged();
                }
            };
        }

        /// <summary>
        /// Executes the command to initiate the auto-categorization process for the current note description.
        /// </summary>
        /// <remarks>
        /// This method resets the programmatic category change state and starts the
        /// auto-categorization process for the note description managed by the associated <see cref="AddNoteViewModel"/>.
        /// </remarks>
        /// <param name="parameter">An optional parameter that is not used by this implementation.</param>
        public override void Execute(object parameter)
        {
            this.addNoteViewModel.CategoryChangedProgrammatically = null;
            this.addNoteViewModel.StartAutoCategorizationDebounceProcess();
        }

        public override bool CanExecute(object parameter)
        {
            return this.addNoteViewModel.CategoryChangedProgrammatically == false;
        }
    }
}
