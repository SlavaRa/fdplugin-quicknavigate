using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ASCompletion.Model;
using JetBrains.Annotations;

namespace QuickNavigate.Forms
{
    public class MemberNode : TreeNode
    {
        public FileModel InFile;

        public MemberNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex)
        {
        }
    }

    public class ClassNode : TreeNode
    {
        public ClassModel Model;
        public FileModel InFile;
        public new string Name;
        public string In;
        public string NameInLowercase;
        public string Package;
        public string Module;
        public bool IsPrivate;

        public ClassNode([NotNull] ClassModel model, int icon) : this(model, icon, icon)
        {
        }

        public ClassNode([NotNull] ClassModel model, int imageIndex, int selectedImageIndex)
        {
            Model = model;
            Name = model.Name;
            InFile = model.InFile;
            Package = InFile != null ? InFile.Package : string.Empty;
            IsPrivate = (model.Access & Visibility.Private) > 0;
            Text = Name;
            Tag = "class";
            In = Package;
            if (!string.IsNullOrEmpty(Package))
            {
                if (IsPrivate) In = $"{Package}.{Path.GetFileNameWithoutExtension(InFile.FileName)}";
            }
            else if (IsPrivate) In = Path.GetFileNameWithoutExtension(InFile.FileName);
            ImageIndex = imageIndex;
            SelectedImageIndex = selectedImageIndex;
            if (InFile == null) return;
            var match = Regex.Match(InFile.FileName, @"\S*.swc", RegexOptions.Compiled);
            if (match.Success) Module = Path.GetFileName(match.Value);
        }
    }

    class ClassHierarchyNode : ClassNode
    {
        public ClassHierarchyNode([NotNull] ClassModel model, int imageIndex, int selectedImageIndex)
            : base(model, imageIndex, selectedImageIndex)
        {
            Text = model.Type;
        }

        public bool Enabled = true;
    }
}