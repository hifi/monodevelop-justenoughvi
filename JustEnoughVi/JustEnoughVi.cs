using Mono.TextEditor;
using MonoDevelop.Ide.Editor.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JustEnoughVi
{
    public abstract class Command
    {
        public Command(string keybind)
        {
            KeyBind = keybind;
        }
        public string KeyBind { get; }
        public Motion Motion { get; set; }
        public int Count { get; set; } = 1;
        public bool NeedsMotion { get; set; } = true;
        public TextEditorData Editor { get; set; }

        public abstract CommandResult Run();

        public CommandResult RunOuter()
        {
            CommandResult res = null;//  TODO: this is wrong
            for (int i = 0; i < Count; i++)
            {
                if (Motion != null)
                {
                    Motion.StartOffset = Motion.EndOffset = Editor.Caret.Offset;
                    Motion.Run(Editor);
                }
                res = Run();
            }
            return res;
        }

        public bool IsSatisfied()
        {
            return !NeedsMotion || NeedsMotion && Motion != null;
        }
    }

    public class CommandResult
    {
        public CommandResult(bool finished) : this(finished, Mode.None)
        {

        }

        public CommandResult(bool finished, Mode requestedMode)
        {
            Finished = finished;
            RequestedMode = requestedMode;
        }

        public bool Finished { get; private set; }
        public Mode RequestedMode { get; private set; }
    }

    public enum Mode
    {
        None,
        Normal,
        Insert,
        Visual,
        VisualLine
    }

    public struct KeyBinding
    {
        public KeyBinding(Type modeType, string keybind)
        {
            ViMode = modeType;
            KeyBind = keybind;
        }

        public Type ViMode { get; private set; }
        public string KeyBind { get; private set; }

        //public override bool Equals(object other)
        //{
        //    var otherKey = (KeyBinding)other;
        //    return otherKey.ViMode == ViMode && otherKey.KeyBind == KeyBind;
        //}

        //public override int GetHashCode()
        //{
        //    return ViMode.GetHashCode() * 17 + KeyBind.GetHashCode() * 23;
        //}

        public override string ToString()
        {
            return $"[KeyBinding: ViMode={ViMode.Name}, KeyBind={KeyBind}]";
        }
    }

    public class JustEnoughVi : TextEditorExtension
    {
        private NormalMode _normalMode;
        private InsertMode _insertMode;
        private VisualMode _visualMode;

        private ViMode _requestedMode;

        public ViMode CurrentMode { get; private set; }

        // TODO: not sure about storing instances here.... maybe types would be better
        private Dictionary<KeyBinding, Command> _commands = new Dictionary<KeyBinding, Command>();
        private Dictionary<KeyBinding, Motion> _motions = new Dictionary<KeyBinding, Motion>();

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

            // start in normal mode
            CurrentMode = _requestedMode = _normalMode;
            CurrentMode.InternalActivate();
            InitializeMode(textEditorData, typeof(NormalMode));
            InitializeMode(textEditorData, typeof(InsertMode));
            InitializeMode(textEditorData, typeof(VisualMode));
            InitializeMotions();
        }

        private void InitializeMode(TextEditorData textEditorData, Type modeType)
        {
            var commandTypes = modeType.GetNestedTypes()
                                       .Where(t => typeof(Command).IsAssignableFrom(t));

            foreach (var commandType in commandTypes)
            {
                var command = Activator.CreateInstance(commandType) as Command;
                command.Editor = textEditorData;
                _commands.Add(new KeyBinding(modeType, command.KeyBind), command);
            }
        }

        private void InitializeMotions()
        {
            var motionTypes = typeof(Motion).Assembly.GetTypes()
                                            .Where(t => t != typeof(Motion) && typeof(Motion).IsAssignableFrom(t));
            foreach (var motionType in motionTypes)
            {
                var motion = Activator.CreateInstance(motionType) as Motion;

                _motions.Add(motion.KeyBinding, motion);
            }
        }
        string _keybuffer = "";
        Command _currentCommand = null;
        int _currentCount = 1;
        public Command KeyParser(KeyDescriptor descriptor)
        {
            var key = descriptor.KeyChar.ToString();
            var keybind = new KeyBinding(CurrentMode.GetType(), key);
            if (_currentCommand == null && _commands.ContainsKey(keybind))
            {
                var command = _commands[keybind];
                if (command.IsSatisfied())
                {
                    
                    command.Count = _currentCount;
                    return command;
                }
                //return null;
                _currentCommand = command;
            }

            else if (_currentCommand != null && _motions.ContainsKey(keybind))
            {
                _currentCommand.Motion = _motions[keybind];
                if (_currentCommand.IsSatisfied())
                {
                    _currentCommand.Count = _currentCount;
                    return _currentCommand;
                }
            }

            if (descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
            {
                //todo - handle multiple digit counts
                _currentCount = int.Parse(key);
            }
            //else
            //{
            _keybuffer += key;
            return null;
            //}
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            //if (CurrentMode is InsertMode)
            //{
            //    return true; // for now
            //}

            var command = KeyParser(descriptor);
            if (command != null && _keybuffer.Length <4) //TODO:stupid hack to break 
            {
                var result = command.RunOuter();
                SetNewMode(result.RequestedMode);
                _keybuffer = "";
                _currentCount = 1;
                _currentCommand = null;
                return !result.Finished && base.KeyPress(descriptor);
            }
            // old parsing method.. shouldn't get here once all commands are implemented
            if (_keybuffer.Length >= 3)
            {
                
                _keybuffer = "";
                _currentCount = 1;
                _currentCommand = null;
            }
            //else
            //{
            //    return CurrentMode is InsertMode && base.KeyPress(descriptor);
            //}


            //return base.KeyPress(descriptor);
            //var keybind = new KeyBinding(CurrentMode.GetType(), descriptor.KeyChar.ToString());
            //if (_commands.ContainsKey(keybind))
            //{
            //    var result = _commands[keybind].RunOuter();
            //    SetNewMode(result.RequestedMode);
            //    return result.Finished && base.KeyPress(descriptor);
            //}

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

            var pass = CurrentMode.KeyPress(descriptor);

            var newMode = CurrentMode.RequestedMode;
            SetNewMode(newMode);

            return pass && base.KeyPress(descriptor);
        }

        void SetNewMode(Mode newMode)
        {
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

            if (_requestedMode != CurrentMode)
            {
                CurrentMode.RequestedMode = Mode.None;
                CurrentMode.InternalDeactivate();
                _requestedMode.InternalActivate();
                CurrentMode = _requestedMode;
            }
        }
    }
}

