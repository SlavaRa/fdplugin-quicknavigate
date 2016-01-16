using System;
using System.Collections.Generic;
using ASCompletion.Model;
using QuickNavigate.Forms;

namespace QuickNavigate.Collections
{
    public class SmartMemberComparer : IComparer<MemberModel>
    {
        readonly string search;

        public SmartMemberComparer(string search)
        {
            if (!string.IsNullOrEmpty(search)) search = search.ToLower();
            this.search = search;
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

        int GetPriority(string name)
        {
            if (true) name = name.ToLower();
            if (name == search) return -100;
            if (name.StartsWith(search)) return -90;
            return 0;
        }
    }

    public class NodeNameComparer : IComparer<TypeNode>
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
            return StringComparer.Ordinal.Compare(x.Name.ToLower(), y.Name.ToLower());
        }
    }

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
            return x.Package.Length == y.Package.Length
                 ? StringComparer.Ordinal.Compare(x.Package, y.Package)
                 : x.Package.Length.CompareTo(y.Package.Length);
        }
    }

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
        public int Compare(TypeNode x, TypeNode y) => StringComparer.OrdinalIgnoreCase.Compare($"{x.Name}{x.Package}", $"{y.Name}{y.Package}");
    }

    public static class TypeExplorerNodeComparer
    {
        public static NodeNameComparer NameIgnoreCase = new NodeNameComparer();
        public static NodePackageComparer Package = new NodePackageComparer();
        public static NodeNamePackageComparer NamePackageIgnoreCase = new NodeNamePackageComparer();
    }
}