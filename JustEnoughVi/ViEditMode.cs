using Mono.TextEditor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.SourceEditor;

namespace JustEnoughVi
{
    public enum ViMode
    {
        Normal,
        Insert,
        Visual
    }

    public class ViEditMode : EditMode
    {
        private readonly EditMode _baseMode;

        private readonly NormalEditMode _normalMode;
        private readonly InsertEditMode _insertMode;
        private readonly VisualEditMode _visualMode;

        private BaseEditMode _currentMode;
        private BaseEditMode _requestedMode;

        public ViEditMode(Document doc)
        {
            var baseEditor = doc.GetContent<SourceEditorView>().TextEditor;
            var baseData = doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();

            _baseMode = baseEditor.CurrentMode;

            _normalMode = new NormalEditMode(this);
            _insertMode = new InsertEditMode(this);
            _visualMode = new VisualEditMode(this);

            // start in normal mode
            _currentMode = _requestedMode = _normalMode;
            _currentMode.InternalActivate(baseEditor, baseData);
        }

        internal void SetMode(ViMode newMode)
        {
            if (newMode == ViMode.Normal)
                _requestedMode = _normalMode;
            else if (newMode == ViMode.Insert)
                _requestedMode = _insertMode;
            else if (newMode == ViMode.Visual)
                _requestedMode = _visualMode;
        }

        internal void BaseKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            _baseMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);
        }

        #region implemented abstract members of EditMode

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            // generic mode escape handler
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.bracketleft) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                _currentMode.InternalDeactivate(Editor, Data);
                _currentMode = _requestedMode = _normalMode;
                _currentMode.InternalActivate(Editor, Data);
                return;
            }

            _currentMode.InternalHandleKeypress(Editor, Data, key, unicodeKey, modifier);

            if (_requestedMode != _currentMode)
            {
                _currentMode.InternalDeactivate(Editor, Data);
                _requestedMode.InternalActivate(Editor, Data);
                _currentMode = _requestedMode;
            }
        }

        #endregion
    }
}

