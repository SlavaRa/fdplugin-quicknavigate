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
        private const int PLUGIN_API = 1;
        private const string PLUGIN_NAME = "QuickNavigatePlugin";
        private const string PLUGIN_GUID = "5e256956-8f0d-4f2b-9548-08673c0adefd";
        private const string PLUGIN_HELP = "www.flashdevelop.org/community/";
        private const string PLUGIN_AUTH = "Canab";
	    private const string SETTINGS_FILE = "Settings.fdb";
        private const string PLUGIN_DESC = "QuickNavigate plugin";

        private string settingFilename;
        private Settings settingObject;
	    private ControlClickManager controlClickManager;

        private List<string> projectFiles = new List<string>();

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
            switch (e.Type)
            {
                case EventType.FileSwitch:
                    if (controlClickManager != null) controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;
                    break;
            }
		}
		
		#endregion
        
        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, PLUGIN_NAME);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, SETTINGS_FILE);
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        private void ShowResourceForm(object sender, EventArgs e)
	    {
            if (PluginBase.CurrentProject != null) new OpenResourceForm(this).ShowDialog();
	    }

        /// <summary>
        /// 
        /// </summary>
        private void ShowTypeForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null) new OpenTypeForm(this).ShowDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowOutlineForm(object sender, EventArgs e)
        {
            if (PluginBase.CurrentProject != null) new QuickOutlineForm(this).ShowDialog();
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
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

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(settingFilename, settingObject);
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> GetProjectFiles()
        {
            if (!settingObject.ResourcesCaching || projectFiles.Count == 0)
                reloadProjectFiles();

            return projectFiles;
        }

        /// <summary>
        /// 
        /// </summary>
        public void invalidateCache()
        {
            projectFiles.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        private void reloadProjectFiles()
        {
            projectFiles.Clear();

            List<string> folders = GetProjectFolders();
            foreach (string folder in folders)
                if (Directory.Exists(folder))
                    projectFiles.AddRange(Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isFileOpened(string file)
        {
            foreach (ITabbedDocument doc in PluginBase.MainForm.Documents)
                if (doc.FileName == file)
                    return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> GetProjectFolders()
        {
            List<string> folders = new List<string>();
            IProject project = PluginBase.CurrentProject;
            if (project == null) return folders;

            string projectFolder = Path.GetDirectoryName(project.ProjectPath);
            folders.Add(projectFolder);

            if (!settingObject.SearchExternalClassPath) return folders;

            foreach (string path in project.SourcePaths)
            {
                if (Path.IsPathRooted(path)) folders.Add(path);
                else
                {
                    string folder = Path.GetFullPath(Path.Combine(projectFolder, path));
                    if (!folder.StartsWith(projectFolder)) folders.Add(folder);
                }
            }

            return folders;
        }

		#endregion

	}
	
}