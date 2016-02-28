using Mono.TextEditor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public enum Mode
    {
        None,
        Normal,
        Insert,
        Visual,
        VisualLine,
        Replace
    }

    public class JustEnoughVi : TextEditorExtension
    {
        private NormalMode _normalMode;
        private InsertMode _insertMode;
        private VisualMode _visualMode;
        private ReplaceMode _replaceMode;

        private ViMode _requestedMode;

        public ViMode CurrentMode { get; private set; }

        protected override void Initialize()
        {
            var textEditorData = Editor.GetContent<ITextEditorDataProvider>().GetTextEditorData();
            Initialize(textEditorData);
        }

        public void Initialize(TextEditorData textEditorData)
        {
            _normalMode = new NormalMode(textEditorData);
            _insertMode = new InsertMode(textEditorData);
            _visualMode = new VisualMode(textEditorData);
            _replaceMode = new ReplaceMode(textEditorData);

            // start in normal mode
            CurrentMode = _requestedMode = _normalMode;
            CurrentMode.InternalActivate();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            // generic mode escape handler
            if (
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.Escape) ||
                (descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == '[') ||
                (descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == 'c'))
            {
                CurrentMode.InternalDeactivate();
                CurrentMode = _requestedMode = _normalMode;
                CurrentMode.InternalActivate();
                return false;
            }

            var pass = CurrentMode.KeyPress (descriptor);

            var newMode = CurrentMode.RequestedMode;
            if (newMode == Mode.Normal)
                _requestedMode = _normalMode;
            else if (newMode == Mode.Insert)
                _requestedMode = _insertMode;
            else if (newMode == Mode.Visual)
            {
                _visualMode.Select = SelectMode.Normal;
                _requestedMode = _visualMode;
            }
            else if (newMode == Mode.VisualLine)
            {
                _visualMode.Select = SelectMode.Line;
                _requestedMode = _visualMode;
            }
            else if (newMode == Mode.Replace)
                _requestedMode = _replaceMode;

            if (_requestedMode != CurrentMode)
            {
                CurrentMode.RequestedMode = Mode.None;
                CurrentMode.InternalDeactivate();
                _requestedMode.InternalActivate();
                CurrentMode = _requestedMode;
            }

            return pass && base.KeyPress(descriptor);
        }
    }
}

