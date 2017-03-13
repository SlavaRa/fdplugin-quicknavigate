// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using ASCompletion.Model;
using JetBrains.Annotations;

namespace QuickNavigate
{
    internal static class SearchUtil
    {
        [NotNull]
        public static List<string> FindAll([NotNull] List<string> items, [NotNull] string search)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it => IsMatch(it, search, length));
            return result;
        }

        [NotNull]
        public static List<MemberModel> FindAll([NotNull] List<MemberModel> items, [NotNull] string search) => FindAll(items, search, false);

        [NotNull]
        public static List<MemberModel> FindAll([NotNull] List<MemberModel> items, [NotNull] string search, bool isHaxe)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it => IsMatch(it.FullName, search, length) 
                                             || ((it.Flags & FlagType.Constructor) != 0 
                                                 && (IsMatch("constructor", search, length) || (isHaxe && IsMatch("new", search, length)))));
            return result;
        }

        static bool IsMatch(string word, string search, int length)
        {
            var score = PluginCore.Controls.CompletionList.SmartMatch(word, search, length);
            return score > 0 && score < 6;
        }
    }
}