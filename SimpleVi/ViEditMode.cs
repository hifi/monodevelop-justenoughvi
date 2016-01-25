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

        private NormalEditMode _normalMode;
        private InsertEditMode _insertMode;
        private VisualEditMode _visualMode;

        private BaseEditMode _currentMode;
        private BaseEditMode _requestedMode;

        // is this available from the editor?
        new public Document Document {
            get { return _doc; }
        }

        public ViEditMode(Document doc)
        {
            _doc = doc;

            var editor = doc.GetContent<SourceEditorView>().TextEditor;
            _baseMode = editor.CurrentMode;
            var data = doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();

            _normalMode = new NormalEditMode(this);
            _insertMode = new InsertEditMode(this);
            _visualMode = new VisualEditMode(this);

            // start in normal mode
            _currentMode = _requestedMode = _normalMode;
            _currentMode.InternalActivate(editor, data);
        }

        public void SetMode(ViMode newMode)
        {
            if (newMode == ViMode.Normal)
                _requestedMode = _normalMode;
            else if (newMode == ViMode.Insert)
                _requestedMode = _insertMode;
            else if (newMode == ViMode.Visual)
                _requestedMode = _visualMode;
        }

        public void BaseKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _baseMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        internal static bool IsEol(char c)
        {
            return (c == '\r' || c == '\n');
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _currentMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);

            // where the heck this should be?
            if (_requestedMode == _normalMode)
            {
                if (_currentMode == _normalMode)
                {
                    while (ViEditMode.IsEol(Data.Document.GetCharAt(Data.Caret.Offset)) && DocumentLocation.MinColumn < Data.Caret.Column)
                        CaretMoveActions.Left(Data);
                }

                if (_currentMode == _insertMode)
                {
                    CaretMoveActions.Left(Data);
                }
            }

            if (_requestedMode != _currentMode)
            {
                _requestedMode.InternalActivate((ExtensibleTextEditor)Editor, Data);
                _currentMode = _requestedMode;
            }
        }

        #endregion
    }
}

