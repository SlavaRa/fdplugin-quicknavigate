using PluginCore;
using System.Collections.Generic;

namespace QuickNavigatePlugin
{
    class SearchUtil
    {
        public static bool IsFileOpened(string fileName)
        {
            foreach (ITabbedDocument doc in PluginBase.MainForm.Documents)
            {
                if (doc.FileName == fileName) return true;
            }
            return false;
        }

        public static List<string> Matches(List<string> source, string search, string pathSeparator, int limit, bool wholeWord, bool matchCase)
        {
            bool noCase = !matchCase;
            if (noCase) search = search.ToLower();
            bool searchHasPathSeparator = search.Contains(pathSeparator);
            List<string> matches = new List<string>();
            foreach (string item in source)
            {
                string itemName = searchHasPathSeparator || !item.Contains(pathSeparator) ? item : item.Substring(item.LastIndexOf(pathSeparator) + 1);
                if (itemName.Length < search.Length) continue;
                if (noCase) itemName = itemName.ToLower();
                if (SimpleSearchMatch(itemName, search, wholeWord) || AdvancedSearchMatch(itemName, search, noCase))
                {
                    matches.Add(item);
                    if (--limit == 0) break;
                }
            }
            Sort(matches, search, pathSeparator, noCase);
            return matches;
        }

        public static void Sort(List<string> matches, string search, string pathSeparator, bool noCase)
        {
            if (matches == null || matches.Count <= 1 || string.IsNullOrEmpty(search)) return;
            if (noCase) search = search.ToLower();
            bool onlyType = !search.Contains(pathSeparator);
            matches.Sort(delegate(string s1, string s2)
            {
                if (s1 == s2) return 0;
                if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 1;
                if (noCase) s1 = s1.ToLower();
                if (s1 == search || (onlyType && s1.Contains(pathSeparator) && s1.Substring(s1.LastIndexOf(pathSeparator) + 1) == search)) return -1;
                return 0;
            });
        }

        private static bool SimpleSearchMatch(string item, string search, bool wholeWord)
        {
            return wholeWord ? item.StartsWith(search) : item.Contains(search);
        }

        private static bool AdvancedSearchMatch(string item, string searchText, bool noCase)
        {
            List<string> parts = GetParts(item, noCase);
            if (parts.Count == 0) return false;
            int partNum = 0;
            char[] search = searchText.ToCharArray();
            int si = 0;
            int sl = searchText.Length;
            while (si < sl && partNum < parts.Count)
            {
                char[] part = parts[partNum].ToCharArray();
                int pi = 0;
                int pl = part.Length;
                while (si < sl && pi < pl && search[si] == part[pi])
                {
                    si++;
                    pi++;
                }
                if (pi == 0) break;
                partNum++;
            }
            return si == sl;
        }

        private static List<string> GetParts(string item, bool noCase)
        {
            List<string> parts = new List<string>();
            char[] chars = item.ToCharArray();
            int i = 0;
            int length = chars.Length;
            while (i < length)
            {
                while (i < length && !char.IsLetter(chars[i]))
                    i++;
                if (i == length) break;
                string part = chars[i].ToString();
                var j = i + 1;
                if (j == length)
                {
                }
                else if (char.IsLower(chars[i + 1]))
                {
                    while (j < length && (char.IsLower(chars[j]) || char.IsDigit(chars[j])))
                        part += chars[j++];
                }
                else if (char.IsUpper(chars[i + 1]))
                {
                    while (j < length)
                    {
                        if (j + 1 < length)
                        {
                            var current = chars[j];
                            var next = chars[j + 1];
                            if ((char.IsUpper(current) || char.IsDigit(current)) && (char.IsUpper(next) || char.IsDigit(next)))
                                part += chars[j++];
                            else break;
                        }
                        else
                        {
                            var current = chars[j];
                            if (char.IsUpper(current) || char.IsDigit(current)) part += chars[j++];
                            else break;
                        }
                    }
                }
                if (noCase) part = part.ToLower();
                parts.Add(part);
                i = j;
            }
            return parts;
        }

    }
}