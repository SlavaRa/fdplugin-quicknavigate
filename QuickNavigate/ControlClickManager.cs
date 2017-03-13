// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ASCompletion.Completion;
using ASCompletion.Context;
using PluginCore;
using ScintillaNet;
using ScintillaNet.Configuration;
using ScintillaNet.Enums;
using Keys = System.Windows.Forms.Keys;

namespace QuickNavigate
{
    class ControlClickManager : IDisposable
    {
        const int CLICK_AREA = 4; //pixels
        ScintillaControl sci;
        Word currentWord;
        Timer timer;
        readonly POINT clickedPoint = new POINT();

        #region MouseHook definitions

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        int hHook;
        const int WH_MOUSE = 7;

        HookProc safeHookProc;

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        public ControlClickManager()
        {
            timer = new Timer {Interval = 10};
            timer.Tick += GoToDeclaration;
        }

        public void Dispose()
        {
            if (timer == null) return;
            timer.Dispose();
            timer = null;
        }

        void GoToDeclaration(object sender, EventArgs e)
        {
            timer.Stop();
            SetCurrentWord(null);
            ASComplete.DeclarationLookup(sci);
        }

        public ScintillaControl Sci
        {
            set
            {
                if (hHook == 0)
                {
                    safeHookProc = MouseHookProc;
                    #pragma warning disable 618,612
                    hHook = SetWindowsHookEx(WH_MOUSE, safeHookProc, (IntPtr)0, AppDomain.GetCurrentThreadId());
                    #pragma warning restore 618,612
                }
                sci = value;
            }
        }

        int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && sci != null)
            {
                MouseHookStruct hookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                if (wParam == (IntPtr) 513) //mouseDown
                {
                    clickedPoint.x = hookStruct.pt.x;
                    clickedPoint.y = hookStruct.pt.y;
                }
                if (Control.ModifierKeys ==  Keys.Control)
                {
                    if (wParam == (IntPtr) 514) //mouseUp
                    {
                        if (currentWord != null && !timer.Enabled) timer.Start();
                    }
                    else
                    {
                        if ((Control.MouseButtons & MouseButtons.Left) > 0)
                        {
                            int dx = Math.Abs(clickedPoint.x - hookStruct.pt.x);
                            int dy = Math.Abs(clickedPoint.y - hookStruct.pt.y);
                            if (currentWord != null && dx > CLICK_AREA || dy > CLICK_AREA)
                                SetCurrentWord(null);
                        }
                        else
                        {
                            Point globalPoint = new Point(hookStruct.pt.x, hookStruct.pt.y);
                            Point localPoint = sci.PointToClient(globalPoint);
                            ProcessMouseMove(localPoint);
                        }
                    }
                }
                else if (currentWord != null) SetCurrentWord(null);
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        void ProcessMouseMove(Point point)
        {
            int position = sci.PositionFromPointClose(point.X, point.Y);
            if (position < 0) SetCurrentWord(null);
            else if (ASContext.Context.IsFileValid)
            {
                Word word = new Word
                {
                    StartPos = sci.WordStartPosition(position, true),
                    EndPos = sci.WordEndPosition(position, true)
                };
                ASResult expr = ASComplete.GetExpressionType(sci, word.EndPos);
                if (expr.IsNull())
                {
                    string overrideKey = ASContext.Context.Features.overrideKey;
                    if (expr.Context == null || !expr.Context.BeforeBody || string.IsNullOrEmpty(overrideKey) || sci.GetWordFromPosition(position) != overrideKey)
                        word = null;
                }
                SetCurrentWord(word);
            }
        }

        void SetCurrentWord(Word word)
        {
            if (Word.Equals(word, currentWord)) return;
            if (currentWord != null) UnHighlight(currentWord);
            currentWord = word;
            if (currentWord != null) Highlight(currentWord);
        }

        void UnHighlight(Word word)
        {
            sci.CursorType = -1;
            int mask = 1 << sci.StyleBits;
            sci.StartStyling(word.StartPos, mask);
            sci.SetStyling(word.EndPos - word.StartPos, 0);
        }

        void Highlight(Word word)
        {
            sci.CursorType = 8;
            int mask = 1 << sci.StyleBits;
            Language language = PluginBase.MainForm.SciConfig.GetLanguage(sci.ConfigurationLanguage);
            sci.SetIndicStyle(0, (int)IndicatorStyle.RoundBox);
            sci.SetIndicFore(0, language.editorstyle.HighlightBackColor);
            sci.StartStyling(word.StartPos, mask);
            sci.SetStyling(word.EndPos - word.StartPos, mask);
        }
    }

    class Word
    {
        public static bool Equals(Word word1, Word word2)
        {
            if (word1 == null && word2 == null) return true;
            if (word1 == null || word2 == null) return false;
            return word1.StartPos == word2.StartPos
                && word1.EndPos == word2.EndPos;
        }

        public int StartPos;
        public int EndPos;
    }
}