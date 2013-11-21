using System;
using System.Collections.Generic;

namespace QuickNavigatePlugin
{
    class SearchUtil
    {
        public static List<string> getMatchedItems(List<string> source, string searchText, string pathSeparator, int limit) 
        {
            return getMatchedItems(source, searchText, pathSeparator, limit, false);
        }

        public static List<string> getMatchedItems(List<string> source, string searchText, string pathSeparator, int limit, bool wholeWord)
        {
            return getMatchedItems(source, searchText, pathSeparator, limit, false, false);
        }

        public static List<string> getMatchedItems(List<string> source, string searchText, string pathSeparator, int limit, bool wholeWord, bool matchCase)
        {
            List<string> matchedItems = new List<string>();
            int i = 0;

            foreach (string item in source)
            {
                string itemName = item.Substring(item.LastIndexOf(pathSeparator) + 1);

                if (itemName.Length < searchText.Length) continue;

                if (SimpleSearchMatch(itemName, searchText) || AdvancedSearchMatch(itemName, searchText))
                {
                    matchedItems.Add(item);
                    if (limit > 0 && i++ > limit) break;
                }
            }

            return matchedItems;
        }

        private static bool SimpleSearchMatch(string item, string search)
        {
            return SimpleSearchMatch(item, search, false, false);
        }

        private static bool SimpleSearchMatch(string item, string search, bool wholeWord, bool matchCase)
        {
            if(!matchCase)
            {
                item = item.ToLower();
                search = search.ToLower();
            }

            if (!wholeWord) return item.IndexOf(search) != -1;
            else return item.StartsWith(search);
        }

        private static bool AdvancedSearchMatch(string item, string searchText)
        {
            List<string> parts = GetParts(item);

            if (parts.Count == 0)
                return false;
            
            int partNum = 0;
            
            char[] search = searchText.ToLower().ToCharArray();
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

                if (pi == 0)
                    break;

                partNum++;
            }

            return si == sl;
        }

        private static List<string> GetParts(string item)
        {
            List<string> parts = new List<string>();

            char[] chars = item.ToCharArray();
            int i = 0;
            int length = chars.Length;

            while (i < length)
            {
                while (i < length && !char.IsLetter(chars[i]))
                    i++;

                if (i == length)
                    break;

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
                            if (char.IsUpper(current) || char.IsDigit(current))
                                part += chars[j++];
                            else break;
                        }
                    }
                }

                parts.Add(part.ToLower());

                i = j;
            }

            return parts;
        }

    }
}