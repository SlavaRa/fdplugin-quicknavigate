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
        public TypeNode(ClassModel model, int icon)
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
            ImageIndex = icon;
            SelectedImageIndex = icon;
            if (inFileNotNull)
            {
                Match match = Regex.Match(model.InFile.FileName, @"\S*.swc", RegexOptions.Compiled);
                if (match.Success) Module = Path.GetFileName(match.Value);
            }
        }
    }

    /// <summary>
    /// </summary>
    class ClassNode : TreeNode
    {
        public readonly ClassModel Model;

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="imageIndex"></param>
        /// <param name="selectedImageIndex"></param>
        public ClassNode(ClassModel model, int imageIndex, int selectedImageIndex) : base(model.Type, imageIndex, selectedImageIndex)
        {
            Model = model;
            Name = model.Name;
            Tag = "enabled";
        }
    }
}