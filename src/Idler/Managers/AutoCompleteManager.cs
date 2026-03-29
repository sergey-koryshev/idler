namespace Idler.Managers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Idler.Helpers.DB;

    public class AutoCompleteManager
    {
        public async Task<string> GetSuggestion(string text, CancellationToken cancellationToken)
        {
            var recentDescriptions = await DataBaseFunctions.GetRecentDescriptionsByPrefix(text, 1, cancellationToken);
            var topSuggestion = recentDescriptions.FirstOrDefault();

            if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(topSuggestion) || !topSuggestion.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return topSuggestion.Substring(text.Length);
        }
    }
}
