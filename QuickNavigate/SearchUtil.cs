using System.Collections.Generic;
using ASCompletion.Model;
using JetBrains.Annotations;

namespace QuickNavigate
{
    internal static class SearchUtil
    {
        [NotNull]
        public static List<string> FindAll([NotNull] List<string> source, [NotNull] string search)
        {
            var length = search.Length;
            var result = source.FindAll(it =>
            {
                var score = PluginCore.Controls.CompletionList.SmartMatch(it, search, length);
                return score > 0 && score < 6;
            });
            return result;
        }

        [NotNull]
        public static List<MemberModel> FindAll([NotNull] List<MemberModel> items, [NotNull] string search)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it =>
            {
                var score = PluginCore.Controls.CompletionList.SmartMatch(it.FullName, search, length);
                return score > 0 && score < 6;
            });
            return result;
        }
    }
}