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

        public static List<string> Matches(List<string> source, string search, string separator, int limit, bool wholeWord, bool matchCase)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(search)) return result;
            bool noCase = !matchCase;
            bool searchHasSeparator = search.Contains(separator);
            foreach (string item in source)
            {
                string type = item.Contains(separator) ? item.Substring(item.LastIndexOf(separator) + 1) : item;
                string itemName = searchHasSeparator ? item : type;
                if (SimpleSearchMatch(itemName, search, wholeWord, noCase) || AdvancedSearchMatch(type, search, noCase))
                {
                    result.Add(item);
                    if (limit > 0 && result.Count >= limit) break;
                }
            }
            return result;
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
            List<string> result = new List<string>();
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
                result.Add(part);
                i = j;
            }
            return result;
        }
    }
}