using System;
using System.Collections.Generic;
using ASCompletion.Model;
using QuickNavigate.Forms;

namespace QuickNavigate.Collections
{
    /// <summary>
    /// </summary>
    public class SmartMemberComparer : IComparer<MemberModel>
    {
        readonly string Search;
        readonly bool NoCase;

        /// <summary>
        /// </summary>
        /// <param name="search"></param>
        /// <param name="noCase"></param>
        public SmartMemberComparer(string search, bool noCase)
        {
            if (noCase && !string.IsNullOrEmpty(search)) search = search.ToLower();
            Search = search;
            NoCase = noCase;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// 
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(MemberModel x, MemberModel y)
        {
            int cmp = GetPriority(x.Name).CompareTo(GetPriority(y.Name));
            return cmp != 0 ? cmp : StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetPriority(string name)
        {
            if (NoCase) name = name.ToLower();
            if (name == Search) return -100;
            if (name.StartsWith(Search)) return -90;
            return 0;
        }
    }

    /// <summary>
    /// </summary>
    public class NodeNameComparer : IComparer<TypeNode>
    {
        /// <summary>
        /// </summary>
        public readonly bool IgnoreCase;

        /// <summary>
        /// </summary>
        /// <param name="search"></param>
        public NodeNameComparer() : this(false)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ignoreCase"></param>
        public NodeNameComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// 
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(TypeNode x, TypeNode y)
        {
            string xName = x.Name;
            string yName = y.Name;
            if (IgnoreCase)
            {
                xName = xName.ToLower();
                yName = yName.ToLower();
            }
            return StringComparer.Ordinal.Compare(xName, yName);
        }
    }

    /// <summary>
    /// </summary>
    public class NodePackageComparer : IComparer<TypeNode>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// 
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(TypeNode x, TypeNode y)
        {
            if (x.Package.Length == y.Package.Length) return StringComparer.Ordinal.Compare(x.Package, y.Package);
            return x.Package.Length.CompareTo(y.Package.Length);
        }
    }

    /// <summary>
    /// </summary>
    public class NodeNamePackageComparer : IComparer<TypeNode>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// 
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(TypeNode x, TypeNode y)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(x.Name + x.Package, y.Name + y.Package);
        }
    }

    /// <summary>
    /// </summary>
    public static class TypeExplorerNodeComparer
    {
        /// <summary>
        /// </summary>
        public static NodeNameComparer NameIgnoreCase = new NodeNameComparer(true);

        /// <summary>
        /// </summary>
        public static NodePackageComparer Package = new NodePackageComparer();

        /// <summary>
        /// </summary>
        public static NodeNamePackageComparer NamePackageIgnoreCase = new NodeNamePackageComparer();
    }
}