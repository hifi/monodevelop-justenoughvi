using System;
using System.Collections.Generic;
using Mono.TextEditor;
using MonoDevelop.Ide.Commands;
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

    public class PreviousWindowCommand : Command
    {
        public PreviousWindowCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(WindowCommands.PrevDocument);
        }
    }

    public class NextWindowCommand : Command
    {
        public NextWindowCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Dispatch(WindowCommands.NextDocument);
        }
    }

    public abstract class ViMode
    {
        public TextEditorData Editor { get; set; }
        public Mode RequestedMode { get; internal set; }
        protected Dictionary<string, Command> CommandMap { get; private set; }
        protected Dictionary<SpecialKey, Command> SpecialKeyCommandMap { get; private set; }
        protected bool AllowCaretOnEol { get; set; } = false;

        private int _count;
        private Command _command;
        private string _buf;

        protected ViMode(TextEditorData editor)
        {
            Editor = editor;
            RequestedMode = Mode.None;
            CommandMap = new Dictionary<string, Command>();
            SpecialKeyCommandMap = new Dictionary<SpecialKey, Command>();
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
            CommandMap.Add("^wl", new NextWindowCommand(editor));
            CommandMap.Add("^wh", new PreviousWindowCommand(editor));

            // remaps
            SpecialKeyCommandMap.Add(SpecialKey.Home, new LineStartCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.End, new LineEndCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.Left, new LeftCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.Right, new RightCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.Up, new UpCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.Down, new DownCommand(editor));
            SpecialKeyCommandMap.Add(SpecialKey.BackSpace, new LeftCommand(editor));
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
            _count = 0;
            _buf = "";
        }

        private void CaretOffEol()
        {
            if (AllowCaretOnEol || RequestedMode == Mode.Insert)
                return;

            var line = Editor.GetLine(Editor.Caret.Line);
            if (line.EndOffset > line.Offset && Editor.Caret.Offset == line.EndOffset)
                CaretMoveActions.Left(Editor);
        }

        public virtual bool KeyPress(KeyDescriptor descriptor)
        {
            // build repeat buffer
            if (_command == null && (_count > 0 || descriptor.KeyChar > '0') && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
            {
                _count = (_count * 10) + (descriptor.KeyChar - 48);
                return false;
            }

            _buf += Char.ToString(descriptor.KeyChar);

            if (_command == null)
            {

                if (descriptor.ModifierKeys == ModifierKeys.Control)
                    _buf = "^" + _buf;

                if (!SpecialKeyCommandMap.TryGetValue(descriptor.SpecialKey, out _command))
                {
                    if (!CommandMap.ContainsKey(_buf))
                    {
                        foreach (var k in CommandMap.Keys)
                        {
                            if (k.StartsWith(_buf, StringComparison.Ordinal))
                                return false;
                        }

                        Reset();
                        return false;
                    }

                    _command = CommandMap[_buf];
                }

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

    }
}

