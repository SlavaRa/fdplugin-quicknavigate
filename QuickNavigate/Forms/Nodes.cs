// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Model;
using JetBrains.Annotations;
using PluginCore.Managers;

namespace QuickNavigate.Forms
{
    public class NodeFactory
    {
        public static TreeNode CreateTreeNode(FileModel inFile, bool isHaxe, MemberModel it)
        {
            var flags = it.Flags;
            var icon = PluginUI.GetIcon(flags, it.Access);
            var constrDecl = isHaxe && (flags & FlagType.Constructor) > 0 ? "new" : it.FullName;
            var node = new MemberNode(it.ToString(), icon, icon)
            {
                InFile = inFile,
                Tag = $"{constrDecl}@{it.LineFrom}"
            };
            return node;
        }

        public static TreeNode CreateTreeNode(ClassModel classModel)
        {
            return new ClassNode(classModel, PluginUI.GetIcon(classModel.Flags, classModel.Access));
        }
    }

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
            InFile = model.InFile ?? FileModel.Ignore;
            Package = InFile.Package;
            IsPrivate = (model.Access & Visibility.Private) > 0;
            Text = Name;
            Tag = "class";
            In = Package;
            if (IsPrivate)
            {
                In = Path.GetFileNameWithoutExtension(InFile.FileName);
                if (!string.IsNullOrEmpty(Package)) In = $"{Package}.{In}";
            }
            else if (InFile.Context.Features.hasModules)
            {
                var module = InFile.Module;
                if (!string.IsNullOrEmpty(module) && module != Name) In = !string.IsNullOrEmpty(In) ? $"{In}.{module}" : module;
            }
            ImageIndex = imageIndex;
            SelectedImageIndex = selectedImageIndex;
            if (InFile == FileModel.Ignore) return;
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