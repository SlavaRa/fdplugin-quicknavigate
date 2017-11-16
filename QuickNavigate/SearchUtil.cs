// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using ASCompletion.Model;
using JetBrains.Annotations;

namespace QuickNavigate
{
    internal static class SearchUtil
    {
        [NotNull, ItemNotNull]
        public static List<string> FindAll([NotNull, ItemNotNull] List<string> items, [NotNull] string search)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it => IsMatch(it, search, length));
            return result;
        }

        [NotNull, ItemNotNull]
        public static List<MemberModel> FindAll([NotNull, ItemNotNull] List<MemberModel> items, [NotNull] string search)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it => IsMatch(it.FullName, search, length));
            return result;
        }

        [NotNull, ItemNotNull]
        public static List<MemberModel> FindAll([NotNull, ItemNotNull] List<MemberModel> items, [NotNull] string search, Func<MemberModel, bool> match)
        {
            var length = search.Length;
            if (length == 0) return items;
            var result = items.FindAll(it => IsMatch(it.FullName, search, length) || match(it));
            return result;
        }

        public static bool IsMatch([NotNull] string word, [NotNull] string search, int length)
        {
            var score = PluginCore.Controls.CompletionList.SmartMatch(word, search, length);
            return score > 0 && score < 6;
        }
    }
}