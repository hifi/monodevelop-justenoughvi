using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public enum Mode
    {
        None,
        Normal,
        Insert,
        Visual
    }

    public class JustEnoughVi : TextEditorExtension
    {
        private NormalMode _normalMode;
        private InsertMode _insertMode;
        private VisualMode _visualMode;

        private ViMode _currentMode;
        private ViMode _requestedMode;

        public JustEnoughVi()
        {

        }

        protected override void Initialize()
        {
            _normalMode = new NormalMode(Editor);
            _insertMode = new InsertMode(Editor);
            _visualMode = new VisualMode(Editor);

            // start in normal mode
            _currentMode = _requestedMode = _normalMode;
            _currentMode.Activate();
        }

        public void Interrupt()
        {
            _currentMode.Deactivate();
            _currentMode = _requestedMode = _normalMode;
            _currentMode.Activate();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            // escape is always enabled
            if (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.Escape)
            {
                Interrupt();
                return false;
            }

            var pass = _currentMode.KeyPress(descriptor);

            var newMode = _currentMode.RequestedMode;
            if (newMode == Mode.Normal)
                _requestedMode = _normalMode;
            else if (newMode == Mode.Insert)
                _requestedMode = _insertMode;
            else if (newMode == Mode.Visual)
                _requestedMode = _visualMode;

            if (_requestedMode != _currentMode)
            {
                _currentMode.RequestedMode = Mode.None;
                _currentMode.Deactivate();
                _requestedMode.Activate();
                _currentMode = _requestedMode;
            }

            return pass && base.KeyPress(descriptor);
        }
    }
}

