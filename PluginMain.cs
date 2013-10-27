using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;

namespace QuickNavigatePlugin
{
	public class PluginMain : IPlugin
	{
        private const Int32 PLUGIN_API = 1;
        private const String PLUGIN_NAME = "QuickNavigatePlugin";
        private const String PLUGIN_GUID = "5e256956-8f0d-4f2b-9548-08673c0adefd";
        private const String PLUGIN_HELP = "www.flashdevelop.org/community/";
        private const String PLUGIN_AUTH = "Canab";
	    private const String SETTINGS_FILE = "Settings.fdb";
        private const String PLUGIN_DESC = "QuickNavigate plugin";

        private String settingFilename;
        private Settings settingObject;
	    private ControlClickManager controlClickManager;

        private List<String> projectFiles = new List<string>();

	    #region Required Properties

        public int Api
        {
            get { return PLUGIN_API; }
        }
        
        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
		{
			get { return PLUGIN_NAME; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
		{
			get { return PLUGIN_GUID; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
		{
			get { return PLUGIN_AUTH; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
		{
			get { return PLUGIN_DESC; }
		}

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
		{
			get { return PLUGIN_HELP; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return settingObject; }
        }
		
		#endregion
		
		#region Required Methods
		
		/// <summary>
		/// Initializes the plugin
		/// </summary>
		public void Initialize()
		{
            InitBasics();
            LoadSettings();
            AddEventHandlers();
            CreateMenuItems();

            if (settingObject.CtrlClickEnabled)
                controlClickManager = new ControlClickManager();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
            SaveSettings();
		}
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
		{
            if (e.Type == EventType.FileSwitch)
            {
                if (controlClickManager != null)
                    controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;
            }
            else if (e.Type == EventType.Command)
            {
            }
		}
		
		#endregion
        
        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, PLUGIN_NAME);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, SETTINGS_FILE);
        }

        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);
        }

        public void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");
            ToolStripMenuItem menuItem;
            System.Drawing.Image image;

            image = PluginBase.MainForm.FindImage("209");
            menuItem = new ToolStripMenuItem("Open Resource", image, ShowResourceForm, Keys.Control | Keys.R);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.OpenResource", menuItem);
            menu.DropDownItems.Add(menuItem);

            image = PluginBase.MainForm.FindImage("99|16|0|0");
            menuItem = new ToolStripMenuItem("Open Type", image, ShowTypeForm, Keys.Control | Keys.Y);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.OpenType", menuItem);
            menu.DropDownItems.Add(menuItem);

            image = PluginBase.MainForm.FindImage("315|16|0|0");
            menuItem = new ToolStripMenuItem("Quick Outline", image, ShowOutlineForm, Keys.Control | Keys.Shift | Keys.O);
            PluginBase.MainForm.RegisterShortcutItem("QuickNavigate.Outline", menuItem);
            menu.DropDownItems.Add(menuItem);
        }

        private void ShowResourceForm(object sender, EventArgs e)
	    {
            if (PluginBase.CurrentProject != null)
                new OpenResourceForm(this).ShowDialog();
	    }

        private void ShowTypeForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null)
                new OpenTypeForm(this).ShowDialog();
        }
        
        private void ShowOutlineForm(object sender, EventArgs e)
        {
            new QuickOutlineForm(this).ShowDialog();
        }
        
        public void LoadSettings()
        {
            if (File.Exists(settingFilename))
            {
                try
                {
                    settingObject = new Settings();
                    settingObject = (Settings) ObjectSerializer.Deserialize(settingFilename, settingObject);
                }
                catch
                {
                    settingObject = new Settings();
                    SaveSettings();
                }
            }
            else
            {
                settingObject = new Settings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            ObjectSerializer.Serialize(settingFilename, settingObject);
        }

        public List<string> GetProjectFiles()
        {
            if (!settingObject.ResourcesCaching || projectFiles.Count == 0)
                reloadProjectFiles();

            return projectFiles;
        }

        public void invalidateCache()
        {
            projectFiles.Clear();
        }

        private void reloadProjectFiles()
        {
            projectFiles.Clear();

            List<string> folders = GetProjectFolders();
            foreach (string folder in folders)
                if (Directory.Exists(folder))
                    projectFiles.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
        }

        public bool isFileOpened(String file)
        {
            foreach (ITabbedDocument doc in PluginBase.MainForm.Documents)
                if (doc.FileName == file)
                    return true;

            return false;
        }

        public List<String> GetProjectFolders()
        {
            String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
            List<string> folders = new List<string>();
            folders.Add(projectFolder);

            if (!settingObject.SearchExternalClassPath)
                return folders;

            foreach (string path in PluginBase.CurrentProject.SourcePaths)
            {
                if (Path.IsPathRooted(path))
                    folders.Add(path);
                else
                {
                    String folder = Path.GetFullPath(Path.Combine(projectFolder, path));
                    if (!folder.StartsWith(projectFolder))
                        folders.Add(folder);
                }
            }

            return folders;
        }

		#endregion

	}
	
}