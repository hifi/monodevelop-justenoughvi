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

        private NormalEditMode _normalMode;
        private InsertEditMode _insertMode;
        private VisualEditMode _visualMode;

        public ViMode Mode {get; set; }

        new public Document Document {
            get { return _doc; }
        }

        public EditMode BaseMode { get { return _baseMode; } }

        public ViEditMode(Document doc)
        {
            _doc = doc;

            var textEditor = doc.GetContent<SourceEditorView>().TextEditor;
            _baseMode = textEditor.CurrentMode;
            _data = _doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();
            _normalMode = new NormalEditMode(this);
            _insertMode = new InsertEditMode(this);
            _visualMode = new VisualEditMode(this);

            SetMode(ViMode.Normal);
        }

        public void SetMode(ViMode newMode)
        {
            if (newMode == ViMode.Normal)
            {
                _data.Caret.Mode = CaretMode.Block;
                _normalMode.Activate();

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
                _visualMode.VisualStart = Data.Caret.Line;
                _visualMode.VisualEnd = Data.Caret.Line;
                _visualMode.Activated();
            }

            Mode = newMode;
        }

        internal static bool IsEol(char c)
        {
            return (c == '\r' || c == '\n');
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            if (Mode == ViMode.Normal)
                _normalMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
            else if (Mode == ViMode.Insert)
                _insertMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
            else if (Mode == ViMode.Visual)
                _visualMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        public void BaseKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _baseMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        #endregion
    }
}

