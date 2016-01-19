using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ASCompletion;
using ASCompletion.Context;
using JetBrains.Annotations;
using PluginCore;

namespace QuickNavigate.Helpers
{
    class FormHelper
    {
        public static bool IsFileOpened([NotNull] string fileName)
        {
            return PluginBase.MainForm.Documents.Any(it => it.FileName == fileName);
        }

        [NotNull]
        public static List<string> FilterOpenedFiles([NotNull] ICollection<string> fileNames)
        {
            var result = (from doc in PluginBase.MainForm.Documents
                          let fileName = doc.FileName
                          where fileNames.Contains(fileName)
                          select fileName).ToList();
            return result;
        }

        public static void Navigate([NotNull] string fileName, [NotNull] TreeNode node)
        {
            ModelsExplorer.Instance.OpenFile(fileName);
            Navigate(node);
        }

        public static void Navigate([NotNull] TreeNode node) => ASContext.Context.OnSelectOutlineNode(node);
    }
}