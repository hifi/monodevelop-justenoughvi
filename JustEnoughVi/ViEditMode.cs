using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.SourceEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JustEnoughVi
{
    public enum ViMode
    {
        Normal,
        Insert,
        Visual
    }

    public class ViEditMode : Mono.TextEditor.EditMode
    {
        private EditMode _baseMode; 

        private NormalEditMode _normalMode;
        private InsertEditMode _insertMode;
        private VisualEditMode _visualMode;

        private BaseEditMode _currentMode;
        private BaseEditMode _requestedMode;

        public ViEditMode(Document doc)
        {
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

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            // generic mode escape handler
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                _currentMode.InternalDeactivate((ExtensibleTextEditor)Editor, Data);
                _currentMode = _requestedMode = _normalMode;
                _currentMode.InternalActivate((ExtensibleTextEditor)Editor, Data);
                return;
            }

            _currentMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);

            if (_requestedMode != _currentMode)
            {
                _currentMode.InternalDeactivate((ExtensibleTextEditor)Editor, Data);
                _requestedMode.InternalActivate((ExtensibleTextEditor)Editor, Data);
                _currentMode = _requestedMode;
            }
        }

        #endregion
    }
}

