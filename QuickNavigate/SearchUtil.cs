using PluginCore;
using System.Collections.Generic;

namespace QuickNavigate
{
    internal static class SearchUtil
    {
        public static List<string> Matches(List<string> source, string search, string separator, int limit, bool wholeWord, bool matchCase)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(search)) return result;
            bool noCase = !matchCase;
            bool searchHasSeparator = search.Contains(separator);
            bool firstCharIsUpper = char.IsUpper(search[0]);
            foreach (string item in source)
            {
                string type = item.Contains(separator) ? item.Substring(item.LastIndexOf(separator) + 1) : item;
                string itemName = searchHasSeparator ? item : type;
                string found = string.Empty;
                if (GetIsAbbreviation(search))
                {
                    if (AbbreviationSearchMatch(itemName, search)) found = item;
                }
                else if (!wholeWord && firstCharIsUpper && !searchHasSeparator)
                {
                    if (AdvancedSearchMatch(type, search, noCase)) found = item;
                }
                else if (SimpleSearchMatch(itemName, search, wholeWord, noCase)) found = item;
                if (!string.IsNullOrEmpty(found))
                {
                    result.Add(found);
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

        internal static bool GetIsAbbreviation(string s)
        {
            int length = s.Length;
            int i = 0;
            while (i < length)
            {
                char c = s[i++];
                if (!char.IsLetter(c) || char.IsLower(c)) return false;
            }
            return true;
        }

        internal static List<char> GetAbbreviation(string s)
        {
            List<char> result = new List<char>();
            int length = s.Length;
            int i = 0;
            while (i < length)
            {
                char c = s[i++];
                if(char.IsLetter(c) && char.IsUpper(c)) result.Add(c);
            }
            return result;
        }

        internal static bool AbbreviationSearchMatch(string item, string search)
        {
            List<char> abbreviation = GetAbbreviation(item);
            int abbreviationCount = abbreviation.Count;
            int searchLength = search.Length;
            if (abbreviationCount < searchLength) return false;
            int mathes = 0;
            for (int i = 0; i < abbreviationCount && mathes < searchLength; i++)
            {
                char c = search[mathes];
                if (abbreviation[i] == c) ++mathes;
                else 
                {
                    i = abbreviation.IndexOf(c, i);
                    if (i < 0) return false;
                    --i;
                    mathes = 0;
                }
            }
            return mathes == searchLength;
        }

        internal static bool AdvancedSearchMatch(string item, string search, bool noCase)
        {
            List<string> parts = GetParts(item);
            if (parts.Count == 0) return false;
            if (noCase) search = search.ToLower();
            int partNum = 0;
            int si = 0;
            int sl = search.Length;
            while (si < sl && partNum < parts.Count)
            {
                string part = parts[partNum];
                if (noCase) part = part.ToLower();
                int pi = 0;
                int pl = part.Length;
                while (si < sl && pi < pl && search[si] == part[pi])
                {
                    si++;
                    pi++;
                }
                partNum++;
            }
            return si == sl;
        }

        internal static List<string> GetParts(string item)
        {
            List<string> result = new List<string>();
            int i = 0;
            int length = item.Length;
            while (i < length)
            {
                while (i < length && !char.IsLetter(item[i])) i++;
                if (i == length) break;
                string part = item[i].ToString();
                var j = i + 1;
                if (j == length)
                {
                }
                else if (char.IsLower(item[j]))
                {
                    while (j < length && (char.IsLower(item[j]) || char.IsDigit(item[j])))
                        part += item[j++];
                }
                else if (char.IsUpper(item[j]))
                {
                    while (j < length)
                    {
                        if (j + 1 < length)
                        {
                            var current = item[j];
                            var next = item[j + 1];
                            if ((char.IsUpper(current) || char.IsDigit(current)) && (char.IsUpper(next) || char.IsDigit(next)))
                                part += item[j++];
                            else break;
                        }
                        else
                        {
                            var current = item[j];
                            if (char.IsUpper(current) || char.IsDigit(current)) part += item[j++];
                            else break;
                        }
                    }
                }
                result.Add(part);
                i = j;
            }
            return result;
        }
    }
}