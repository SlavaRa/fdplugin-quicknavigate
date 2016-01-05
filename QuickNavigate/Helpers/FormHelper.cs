using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using PluginCore;
using PluginCore.Helpers;
using static ASCompletion.PluginUI;

namespace QuickNavigate.Helpers
{
    class FormHelper
    {
        [NotNull]
        public static ImageList GetTreeIcons()
        {
            ImageList result = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = ScaleHelper.Scale(new Size(16, 16))
            };
            result.Images.AddRange(new[]
            {
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("FilePlain.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("FolderClosed.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("FolderOpen.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("CheckAS.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("QuickBuild.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Package.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Interface.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Intrinsic.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Class.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Variable.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("VariableProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("VariablePrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("VariableStatic.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("VariableStaticProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("VariableStaticPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Const.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("ConstProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("ConstPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Const.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("ConstProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("ConstPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Method.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("MethodProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("MethodPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("MethodStatic.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("MethodStaticProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("MethodStaticPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Property.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("PropertyProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("PropertyPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("PropertyStatic.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("PropertyStaticProtected.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("PropertyStaticPrivate.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Template.png"))),
                PluginBase.MainForm.ImageSetAdjust(Image.FromStream(GetStream("Declaration.png")))
            });
            return result;
        }
    }
}