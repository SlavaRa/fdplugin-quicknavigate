using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ASCompletion.Model;
using JetBrains.Annotations;

namespace QuickNavigate.Forms
{
    public class TypeNode : TreeNode
    {
        public ClassModel Model;
        public new string Name;
        public string In;
        public string NameInLowercase;
        public string Package;
        public string Module;
        public bool IsPrivate;

        public TypeNode([NotNull] ClassModel model, int icon) : this(model, icon, icon)
        {
        }

        public TypeNode([NotNull] ClassModel model, int imageIndex, int selectedImageIndex)
        {
            Model = model;
            Name = model.Name;
            var inFile = model.InFile;
            Package = inFile != null ? inFile.Package : string.Empty;
            IsPrivate = (model.Access & Visibility.Private) > 0;
            Text = Name;
            Tag = "class";
            In = Package;
            if (!string.IsNullOrEmpty(Package))
            {
                if (IsPrivate) In = $"{Package}.{Path.GetFileNameWithoutExtension(inFile.FileName)}";
            }
            else if (IsPrivate) In = Path.GetFileNameWithoutExtension(inFile.FileName);
            ImageIndex = imageIndex;
            SelectedImageIndex = selectedImageIndex;
            if (inFile == null) return;
            var match = Regex.Match(inFile.FileName, @"\S*.swc", RegexOptions.Compiled);
            if (match.Success) Module = Path.GetFileName(match.Value);
        }
    }

    public class MemberNode : TreeNode
    {
        public FileModel InFile;

        public MemberNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex)
        {
        }
    }

    class ClassHierarchyNode : TypeNode
    {
        public ClassHierarchyNode(ClassModel model, int imageIndex, int selectedImageIndex)
            : base(model, imageIndex, selectedImageIndex)
        {
            Text = model.Type;
        }

        public bool Enabled = true;
    }
}