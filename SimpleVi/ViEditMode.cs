using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.SourceEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleVi
{
    public enum ViMode
    {
        Normal,
        Insert,
        Visual
    }

    public class ViEditMode : Mono.TextEditor.EditMode
    {
        private Document _doc;
        private EditMode _baseMode; 
        private TextEditorData _data;
        private string _countString;

        private NormalEditMode _normalMode;

        public int VisualStart { get; set; }
        public int VisualEnd { get; set; }

        public ViMode Mode {get; set; }

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        new public Document Document {
            get { return _doc; }
        }

        public EditMode BaseMode { get { return _baseMode; } }

        public ViEditMode(Document doc)
        {
            _doc = doc;
            _countString = "";

            var textEditor = doc.GetContent<SourceEditorView>().TextEditor;
            _baseMode = textEditor.CurrentMode;
            _data = _doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();
            _normalMode = new NormalEditMode(this);

            SetMode(ViMode.Normal);
        }

        public void SetMode(ViMode newMode)
        {
            if (newMode == ViMode.Normal)
            {
                _data.Caret.Mode = CaretMode.Block;

                if (Mode == ViMode.Insert)
                {
                    CaretMoveActions.Left(_data);
                }
            }
            else if (newMode == ViMode.Insert)
            {
                _data.Caret.Mode = CaretMode.Insert;
            }
            else if (newMode == ViMode.Visual)
            {
                _data.Caret.Mode = CaretMode.Block;
            }

            Mode = newMode;

            // reset count string on mode switch for now
            _countString = "";
        }

        internal static bool IsEol(char c)
        {
            return (c == '\r' || c == '\n');
        }

        private bool InsertKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                SetMode(ViMode.Normal);
                return true;
            }

            return false;
        }

        private bool VisualKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                Data.ClearSelection();
                SetMode(ViMode.Normal);
                return true;
            }

            if (modifier == 0)
            {
                // build repeat buffer
                if (unicodeKey >= '0' && unicodeKey <= '9')
                {
                    _countString += Char.ToString((char)unicodeKey);
                    return true;
                }

                if (unicodeKey == 'j' || unicodeKey == 'k')
                {
                    if (unicodeKey == 'j')
                    {
                        VisualEnd++;
                    }
                    else if (unicodeKey == 'k')
                    {
                        VisualEnd--;
                    }

                    int start = VisualStart;
                    int end = VisualEnd;

                    if (end < start)
                    {
                        end--;
                        start++;
                    }

                    Data.SetSelectLines(start, end);
                }

                if (unicodeKey == 'd')
                {
                    ClipboardActions.Cut(_data);
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == 'y' || unicodeKey == 'Y')
                {
                    ClipboardActions.Copy(_data);
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == '<')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.RemoveIndentSelection);
                    }
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }

                if (unicodeKey == '>')
                {
                    var count = Math.Max(1, Count);
                    for (int i = 0; i < count; i++)
                    {
                        RunAction(MiscActions.IndentSelection);
                    }
                    Data.ClearSelection();
                    SetMode(ViMode.Normal);
                }
            }

            return true;
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (Data != null)
                _data = Data;

            bool handled = false;

            if (Mode == ViMode.Normal)
            {
                _normalMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
                handled = true;
            }
            else if (Mode == ViMode.Insert)
                handled = InsertKeypress(key, unicodeKey, modifier);
            else if (Mode == ViMode.Visual)
                handled = VisualKeypress(key, unicodeKey, modifier);

            if (!handled)
                BaseKeypress(key, unicodeKey, modifier);
        }

        public void BaseKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _baseMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        #endregion
    }
}

