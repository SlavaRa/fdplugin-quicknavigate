using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ASCompletion.Model;

namespace QuickNavigate.Forms
{
    /// <summary>
    /// </summary>
    public class TypeNode : TreeNode
    {
        public readonly ClassModel Model;
        public new string Name;
        public string In;
        public string NameInLowercase;
        public string Package;
        public string Module;
        public bool IsPrivate;

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="icon"></param>
        public TypeNode(ClassModel model, int icon) : this(model, icon, icon)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="imageIndex"></param>
        /// <param name="selectedImageIndex"></param>
        public TypeNode(ClassModel model, int imageIndex, int selectedImageIndex)
        {
            Model = model;
            Name = model.Name;
            bool inFileNotNull = model.InFile != null;
            Package = inFileNotNull ? model.InFile.Package : string.Empty;
            IsPrivate = (model.Access & Visibility.Private) > 0;
            Text = Name;
            In = Package;
            if (!string.IsNullOrEmpty(Package))
            {
                if (IsPrivate)
                    In = string.Format("{0}.{1}", Package, Path.GetFileNameWithoutExtension(model.InFile.FileName));
            }
            else if (IsPrivate) In = Path.GetFileNameWithoutExtension(model.InFile.FileName);
            ImageIndex = imageIndex;
            SelectedImageIndex = selectedImageIndex;
            if (inFileNotNull)
            {
                Match match = Regex.Match(model.InFile.FileName, @"\S*.swc", RegexOptions.Compiled);
                if (match.Success) Module = Path.GetFileName(match.Value);
            }
        }
    }

    /// <summary>
    /// </summary>
    class ClassHierarchyNode : TypeNode
    {
        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="imageIndex"></param>
        /// <param name="selectedImageIndex"></param>
        public ClassHierarchyNode(ClassModel model, int imageIndex, int selectedImageIndex)
            : base(model, imageIndex, selectedImageIndex)
        {
            Tag = "enabled";
            Name = model.Name;
            Text = model.Type;
        }
    }
}