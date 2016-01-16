using System.Collections.Generic;

namespace QuickNavigate
{
    internal static class SearchUtil
    {
        public static List<string> Matches(List<string> source, string search)
        {
            var length = search.Length;
            var result = source.FindAll(it =>
            {
                var score = PluginCore.Controls.CompletionList.SmartMatch(it, search, length);
                return score > 0 && score < 6;
            });
            return result;
        }
    }
}