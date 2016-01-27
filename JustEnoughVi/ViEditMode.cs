using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public enum ViMode
    {
        None,
        Normal,
        Insert,
        Visual
    }

    public class ViEditMode : TextEditorExtension
    {
        private NormalEditMode _normalMode;
        private InsertEditMode _insertMode;
        private VisualEditMode _visualMode;

        private BaseEditMode _currentMode;
        private BaseEditMode _requestedMode;

        public ViEditMode()
        {

        }

        protected override void Initialize()
        {
            _normalMode = new NormalEditMode(Editor);
            _insertMode = new InsertEditMode(Editor);
            _visualMode = new VisualEditMode(Editor);

            // start in normal mode
            _currentMode = _requestedMode = _normalMode;
            _currentMode.Activate();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            // generic mode escape handler
            if (
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.Escape) ||
                (descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == '[') ||
                (descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == 'c'))
            {
                _currentMode.Deactivate();
                _currentMode = _requestedMode = _normalMode;
                _currentMode.Activate();
                return false;
            }

            var pass = _currentMode.KeyPress (descriptor);

            var newMode = _currentMode.RequestedMode;
            if (newMode == ViMode.Normal)
                _requestedMode = _normalMode;
            else if (newMode == ViMode.Insert)
                _requestedMode = _insertMode;
            else if (newMode == ViMode.Visual)
                _requestedMode = _visualMode;

            if (_requestedMode != _currentMode)
            {
                _currentMode.RequestedMode = ViMode.None;
                _currentMode.Deactivate();
                _requestedMode.Activate();
                _currentMode = _requestedMode;
            }

            return pass && base.KeyPress(descriptor);
        }
    }
}

