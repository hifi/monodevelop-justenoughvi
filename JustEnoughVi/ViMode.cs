using System;
using System.Collections.Generic;
using Mono.TextEditor;
//using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public abstract class ViMode
    {
        public TextEditorData Editor { get; set; }
        public Mode RequestedMode { get; internal set; }
        protected Dictionary<string , Func<int, char[], bool>> KeyMap { get; private set; }

        private readonly List<char> _commandBuf;
        private string _countString;

        protected int Count {
            get {
                var count = 0;
                int.TryParse(_countString, out count);
                return count;
            }
        }

        protected ViMode(TextEditorData editor)
        {
            Editor = editor;
            RequestedMode = Mode.None;
            KeyMap = new Dictionary<string, Func<int, char[], bool>>();

            _commandBuf = new List<char>();
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
            _countString = "";
        }

        private void CaretOffEol()
        {
            if (RequestedMode == Mode.Insert)
                return;

            var line = Editor.GetLine(Editor.Caret.Line);
            if (line.EndOffset > line.Offset && Editor.Caret.Offset == line.EndOffset)
                CaretMoveActions.Left(Editor);
        }


        protected bool MotionDown(int count = 1, char[] args = null)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Down(Editor);
            }

            return true;
        }

        protected bool MotionLeft(int count = 1, char[] args = null)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                if (DocumentLocation.MinColumn < Editor.Caret.Column)
                    CaretMoveActions.Left(Editor);
            }

            return true;
        }

        protected bool MotionUp(int count = 1, char[] args = null)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Up(Editor);
            }

            return true;
        }

        protected bool MotionRight(int count = 1, char[] args = null)
        {
            count = Math.Min(Math.Max(count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset - 1);

            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Right(Editor);
            }

            return true;
        }

        protected bool MotionPageDown(int count = 1, char[] args = null)
        {
            Editor.Caret.Line += Math.Min(Editor.LineCount - Editor.Caret.Line, 20);
            CaretMoveActions.LineStart(Editor);
            Editor.CenterToCaret();
            return true;
        }

        protected bool MotionPageUp(int count = 1, char[] args = null)
        {
            Editor.Caret.Line -= Math.Min(Editor.Caret.Line - 1, 20);
            CaretMoveActions.LineStart(Editor);
            Editor.CenterToCaret();
            return true;
        }

        public virtual bool KeyPress(KeyDescriptor descriptor)
        {
            // remap some function keys to Vi keys
            if (descriptor.ModifierKeys == 0)
            {
                if (descriptor.SpecialKey == SpecialKey.Home)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.Key_0, '0', 0);
                else if (descriptor.SpecialKey == SpecialKey.End)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.Key_4, '$', 0);
                else if (descriptor.SpecialKey == SpecialKey.Left)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.h, 'h', 0);
                else if (descriptor.SpecialKey == SpecialKey.Right)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.l, 'l', 0);
                else if (descriptor.SpecialKey == SpecialKey.Up)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.k, 'k', 0);
                else if (descriptor.SpecialKey == SpecialKey.Down)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.j, 'j', 0);
                else if (descriptor.SpecialKey == SpecialKey.Delete)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.x, 'x', 0);
                else if (descriptor.SpecialKey == SpecialKey.BackSpace)
                    descriptor = KeyDescriptor.FromGtk(Gdk.Key.h, 'h', 0);
            }

            // build repeat buffer
            if (_commandBuf.Count == 0 && (_countString.Length > 0 || descriptor.KeyChar > '0') && descriptor.KeyChar >= '0' && descriptor.KeyChar <= '9')
            {
                _countString += Char.ToString(descriptor.KeyChar);
                return false;
            }

            _commandBuf.Add(descriptor.KeyChar);

            var command = Char.ToString(_commandBuf[0]);

            if (descriptor.ModifierKeys == ModifierKeys.Control)
                command = "^" + command;

            if (!KeyMap.ContainsKey(command))
            {
                Reset();
                return false;
            }

            CaretOffEol();

            if (KeyMap[command](Count, _commandBuf.GetRange(1, _commandBuf.Count - 1).ToArray()))
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

