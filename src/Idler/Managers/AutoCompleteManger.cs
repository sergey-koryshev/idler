namespace Idler.Managers
{
    using Idler.Helpers.DB;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class AutoCompleteManger
    {
        public async Task<string> GetSuggestion(string text)
        {
            var recentDescriptions = await DataBaseFunctions.GetRecentDescriptionsByPrefix(text, 5);
            var topSuggestion = recentDescriptions.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(topSuggestion) || !topSuggestion.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return topSuggestion.Substring(text.Length);
        }
    }
}
