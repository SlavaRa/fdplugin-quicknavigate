using ASCompletion.Completion;
using ASCompletion.Context;
using FlashDevelop;
using PluginCore;
using PluginCore.FRService;
using ScintillaNet;
using ScintillaNet.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuickNavigatePlugin
{
    class HighlightManager
    {
        private Timer timer;
        private string prenToken = string.Empty;

        public HighlightManager()
        {
            timer = new Timer();
            timer.Tick += TimerTick;
        }

        public int Interval {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

        internal void Start()
        {
            if (!timer.Enabled) timer.Start();
        }

        internal void Stop()
        {
            if (!timer.Enabled) return;
            timer.Stop();
            ScintillaControl Sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (ASContext.Context.IsFileValid && Sci != null) Sci.RemoveHighlights();
        }

        private void TimerTick(object sender, System.EventArgs e)
        {
            ScintillaControl Sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (!ASContext.Context.IsFileValid || Sci == null) return;
            string token = GetCurrentHighlightTarget(Sci);
            if (token == prenToken) return;
            prenToken = token;
            Sci.RemoveHighlights();
            List<SearchMatch> matches = GetResults(Sci, token);
            if (matches.Count > 0)
            {
                Language language = MainForm.Instance.SciConfig.GetLanguage(Sci.ConfigurationLanguage);
                Sci.AddHighlights(matches, language.editorstyle.HighlightBackColor);
            }
        }

        private string GetCurrentHighlightTarget(ScintillaControl Sci)
        {
            int position = Sci.WordEndPosition(Sci.CurrentPos, true);
            ASResult result = ASComplete.GetExpressionType(Sci, position);
            if (result.Member == null && result.Type == null) return string.Empty;
            return result.Type != null && result.Member == null ? result.Type.Name : result.Member.Name;
        }

        private List<SearchMatch> GetResults(ScintillaControl Sci, string text)
        {
            FRSearch search = new FRSearch(text);
            search.Filter = SearchFilter.None;
            search.NoCase = false;
            search.WholeWord = true;
            return search.Matches(Sci.Text);
        }
    }
}