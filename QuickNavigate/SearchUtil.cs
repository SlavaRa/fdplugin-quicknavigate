using PluginCore;
using System.Collections.Generic;

namespace QuickNavigate
{
    internal static class SearchUtil
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
            bool searchHasPathSeparator = search.Contains(pathSeparator);
            List<string> matches = new List<string>();
            foreach (string item in source)
            {
                string type = item.Contains(pathSeparator) ? item.Substring(item.LastIndexOf(pathSeparator) + 1) : item;
                string itemName = searchHasPathSeparator ? item : type;
                if (SimpleSearchMatch(itemName, search, wholeWord, noCase) || AdvancedSearchMatch(type, search, noCase))
                {
                    matches.Add(item);
                    if (--limit == 0) break;
                }
            }
            Sort(matches, search, pathSeparator, noCase);
            return matches;
        }

        public static string[] Sort(List<string> matches, string search, string pathSeparator, bool noCase)
        {
            if (noCase) search = search.ToLower();
            matches.Sort(delegate(string a, string b)
            {
                if (noCase) a = a.ToLower();
                string t1 = a.Contains(pathSeparator) ? a.Substring(a.LastIndexOf(pathSeparator) + 1) : a;
                if (a == search || t1 == search) return -1;
                string t2 = b.Contains(pathSeparator) ? b.Substring(b.LastIndexOf(pathSeparator) + 1) : b;
                if (b == search || t2 == search) return -1;
                return a.Length > b.Length ? 1 : 0;
            });
            return matches.ToArray();
        }

        internal static bool SimpleSearchMatch(string item, string search, bool wholeWord)
        {
            return SimpleSearchMatch(item, search, wholeWord, false);
        }

        internal static bool SimpleSearchMatch(string item, string search, bool wholeWord, bool noCase)
        {
            if (noCase)
            {
                item = item.ToLower();
                search = search.ToLower();
            }
            return wholeWord ? item.StartsWith(search) : item.Contains(search);
        }

        internal static bool AdvancedSearchMatch(string item, string searchText, bool noCase)
        {
            if (noCase) searchText = searchText.ToLower();
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

        internal static List<string> GetParts(string item, bool noCase)
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