using ASCompletion.Model;
using PluginCore;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuickNavigate.Controls
{
    public partial class HierarchyExplorer : Form
    {
        private readonly Settings settings;
        private readonly Dictionary<string, List<string>> extendsToTypes;
        private readonly Dictionary<string, List<ClassModel>> extendsToClasses;

        public HierarchyExplorer(Settings settings)
        {
            this.settings = settings;
            Font = PluginBase.Settings.ConsoleFont;
            InitializeComponent();
            if (settings.HierarchyExplorer.Width > MinimumSize.Width) Size = settings.HierarchyExplorer;
            (PluginBase.MainForm as FlashDevelop.MainForm).ThemeControls(this);
            extendsToClasses = GetAllProjectExtendsClasses();
            RefreshTree();
        }

        private Dictionary<string, List<ClassModel>> GetAllProjectExtendsClasses()
        {
            Dictionary<string, List<ClassModel>> result = new Dictionary<string, List<ClassModel>>();
            foreach (PathModel path in ASCompletion.Context.ASContext.Context.Classpath)
            {
                path.ForeachFile((aFile) =>
                {
                    foreach (ClassModel aClass in aFile.Classes)
                    {
                        string extendsType = aClass.ExtendsType;
                        if (!string.IsNullOrEmpty(extendsType))
                        {
                            if (!result.ContainsKey(extendsType)) result[extendsType] = new List<ClassModel>();
                            result[extendsType].Add(aClass);
                        }
                    }
                    return true;
                });
            }
            return result;
        }

        private void RefreshTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            FillTree();
            tree.EndUpdate();
            tree.ExpandAll();
        }

        private void FillTree()
        {
            ClassModel theClass = ASCompletion.Context.ASContext.Context.CurrentClass;
            if (theClass == null) return;
            foreach (string type in GetExtends(theClass)) tree.Nodes.Add(type);
            tree.SelectedNode = tree.Nodes.Add(theClass.Type);
            tree.SelectedNode.Name = theClass.Name;
            FillNode(tree.SelectedNode);
        }

        private List<string> GetExtends(ClassModel theClass)
        {
            List<string> result = new List<string>();
            ClassModel aClass = theClass.Extends;
            while (!aClass.IsVoid())
            {
                result.Add(aClass.Type);
                aClass = aClass.Extends;
            }
            result.Reverse();
            return result;
        }

        private void FillNode(TreeNode node)
        {
            if (!extendsToClasses.ContainsKey(node.Name)) return;
            foreach (ClassModel aClass in extendsToClasses[node.Name])
            {
                ClassModel extends = aClass.InFile.Context.ResolveType(aClass.ExtendsType, aClass.InFile);
                if (extends.Type == node.Text)
                {
                    TreeNode child = node.Nodes.Add(aClass.Type);
                    child.Name = aClass.Name;
                    FillNode(child);
                }
            }
        }

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        #endregion
    }
}