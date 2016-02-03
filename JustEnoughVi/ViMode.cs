using System;
using System.Collections.Generic;
using Mono.TextEditor;
//using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class UpCommand : Command
    {
        public UpCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Motion.Up(Editor);
        }
    }

    public class DownCommand : Command
    {
        public DownCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Motion.Down(Editor);
        }
    }

    public class LeftCommand : Command
    {
        public LeftCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Motion.Left(Editor);
        }
    }

    public class RightCommand : Command
    {
        public RightCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
                Motion.Right(Editor);
        }
    }

    public class LineStartCommand : Command
    {
        public LineStartCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineStart(Editor);
        }
    }

    public class LineEndCommand : Command
    {
        public LineEndCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineEnd(Editor);
        }
    }

    public class PageUpCommand : Command
    {
        public PageUpCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Editor.Caret.Line -= Math.Min(Editor.Caret.Line - 1, 20);
            Motion.LineStart(Editor);
            Editor.CenterToCaret();
        }
    }

    public class PageDownCommand : Command
    {
        public PageDownCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Editor.Caret.Line += Math.Min(Editor.LineCount - Editor.Caret.Line, 20);
            Motion.LineStart(Editor);
            Editor.CenterToCaret();
        }
    }

    public abstract class ViMode
    {
        public TextEditorData Editor { get; set; }
        public Mode RequestedMode { get; internal set; }
        protected Dictionary<string , Func<int, char[], bool>> KeyMap { get; private set; }
        protected Dictionary<string , Command> CommandMap { get; private set; }

        private readonly List<char> _commandBuf;
        private int _count;
        private bool _countReset;
        private Command _command;
        private string _buf;

        protected ViMode(TextEditorData editor)
        {
            Editor = editor;
            RequestedMode = Mode.None;
            KeyMap = new Dictionary<string, Func<int, char[], bool>>();
            CommandMap = new Dictionary<string, Command>();

            _commandBuf = new List<char>();
            _buf = "";

            // standard motion keys
            CommandMap.Add("k", new UpCommand(editor));
            CommandMap.Add("j", new DownCommand(editor));
            CommandMap.Add("h", new LeftCommand(editor));
            CommandMap.Add("l", new RightCommand(editor));
            CommandMap.Add("^", new LineStartCommand(editor));
            CommandMap.Add("_", new LineStartCommand(editor));
            CommandMap.Add("$", new LineEndCommand(editor));
            CommandMap.Add("^b", new PageUpCommand(editor));
            CommandMap.Add("^f", new PageDownCommand(editor));
            CommandMap.Add("^d", new PageDownCommand(editor));
            CommandMap.Add("^u", new PageUpCommand(editor));

            // remaps
            /*
            KeyMap.Add("Home", MotionFirstColumn);
            KeyMap.Add("End", MotionLineEnd);
            KeyMap.Add("Left", MotionLeft);
            KeyMap.Add("Right", MotionRight);
            KeyMap.Add("Up", MotionUp);
            KeyMap.Add("Down", MotionDown);
            KeyMap.Add("BackSpace", MotionLeft);
            */
        }

        public void InternalActivate()
        {
            Reset();
            Activate();
        }

        public void InternalDeactivate()
        {
            Deactivate();
            Reset();
        }

        protected abstract void Activate();
        protected abstract void Deactivate();

        private void Reset()
        {
            _commandBuf.Clear();
            _count = 0;
            _countReset = false;
            _buf = "";
        }

        private void CaretOffEol()
        {
            if (RequestedMode == Mode.Insert)
                return;

            var line = Editor.GetLine(Editor.Caret.Line);
            if (line.EndOffset > line.Offset && Editor.Caret.Offset == line.EndOffset)
                CaretMoveActions.Left(Editor);
        }

        private bool CommandKeyPress(KeyDescriptor descriptor)
        {
            // build repeat buffer
            if (_command == null && (_count > 0 || descriptor.KeyChar > '0') && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
            {
                _count = (_count * 10) + (descriptor.KeyChar - 48);
                return false;
            }
            // secondary run if command supports secondary count
            else if (_command != null && _command.SecondaryCount && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
            {
                if (!_countReset)
                {
                    _count = 0;
                    _countReset = true;
                }

                _count = (_count * 10) + (descriptor.KeyChar - 48);
                return false;
            }

            _buf += Char.ToString(descriptor.KeyChar);

            if (_command == null)
            {

                if (descriptor.ModifierKeys == ModifierKeys.Control)
                    _buf = "^" + _buf;

                if (!CommandMap.ContainsKey(_buf))
                {
                    if (_buf.Length > 2)
                    {
                        _count = 0;
                        _buf = "";
                        Reset();
                    }
                    return true;
                }

                _command = CommandMap[_buf];
                _buf = "";
                if (_command.TakeArgument)
                    return false;
            }

            CaretOffEol();

            RequestedMode = _command.RunCommand(_count, (char)(_buf.Length > 0 ? _buf[0] : 0));
            _command = null;
            Reset();

            CaretOffEol();
            return false;
        }

        public virtual bool KeyPress(KeyDescriptor descriptor)
        {
            // glue this here for now
            if (!CommandKeyPress(descriptor))
                return false;

            string command;

            if (descriptor.SpecialKey > 0)
            {
                command = descriptor.SpecialKey.ToString();
                _commandBuf.Clear();
                _count = 0;
            }
            else
            {
                // build repeat buffer
                if (_commandBuf.Count == 0 && (_count > 0 || descriptor.KeyChar > '0') && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
                {
                    _count = (_count * 10) + (descriptor.KeyChar - 48);
                    return false;
                }
                // secondary run if command is waiting for arguments
                else if (_commandBuf.Count == 1 && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
                {
                    if (!_countReset)
                    {
                        _count = 0;
                        _countReset = true;
                    }

                    _count = (_count * 10) + (descriptor.KeyChar - 48);
                    return false;
                }

                _commandBuf.Add(descriptor.KeyChar);
                command = Char.ToString(_commandBuf[0]);
            }

            if (descriptor.ModifierKeys == ModifierKeys.Control)
                command = "^" + command;

            if (!KeyMap.ContainsKey(command))
            {
                Reset();
                return false;
            }

            CaretOffEol();

            if (KeyMap[command](_count, (_commandBuf.Count > 1 ? _commandBuf.GetRange(1, _commandBuf.Count - 1).ToArray() : new char[] { })))
            {
                Reset();
            }

            CaretOffEol();

            return false;
        }

        // TODO: move this somewhere else? extend TextEditor?
        protected void SetSelectLines(int start, int end)
        {
            start = Math.Min(start, Editor.LineCount);
            end = Math.Min(end, Editor.LineCount);

            var startLine = start > end ? Editor.GetLine(end) : Editor.GetLine(start);
            var endLine = start > end ? Editor.GetLine(start) : Editor.GetLine(end);

            Editor.SetSelection(startLine.Offset, endLine.EndOffsetIncludingDelimiter);
        }
    }
}

