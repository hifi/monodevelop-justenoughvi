using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
using GLib;

namespace JustEnoughVi
{
    public enum SelectMode
    {
        Normal,
        Line
    }

    public class CutSelectionCommand : Command
    {
        public CutSelectionCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Normal;
        }
    }

    public class YankSelectionCommand : Command
    {
        public YankSelectionCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            var startOffset = Editor.SelectionAnchor;
            ClipboardActions.Copy(Editor);
            Editor.Caret.Offset = startOffset;
            RequestedMode = Mode.Normal;
        }
    }

    public class IndentSelectionCommand : Command
    {
        public IndentSelectionCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                MiscActions.IndentSelection(Editor);
            }

            RequestedMode = Mode.Normal;
        }
    }

    public class RemoveIndentSelectionCommand : Command
    {
        public RemoveIndentSelectionCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            for (int i = 0; i < Count; i++)
            {
                MiscActions.RemoveIndentSelection(Editor);
            }

            RequestedMode = Mode.Normal;
        }
    }


    public class ChangeSelectionCommand : Command
    {
        public ChangeSelectionCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Insert;        
        }
    }

    public class FindSelectionCommand : Command
    {
        readonly int findResultShift;

        public FindSelectionCommand(TextEditorData editor, int findResultShift) : base(editor)
        {
            TakeArgument = true;
            this.findResultShift = findResultShift;
        }

        protected override void Run()
        {
            var originalCaretPosition = Editor.Caret.Offset;
            for (int i = 0; i < Count; i++)
            {
                var offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, Argument);
                if (offset <= 0)
                    return;

                Editor.Caret.Offset = offset;
                Editor.SetSelection(originalCaretPosition, offset);
            }

            Editor.Caret.Offset += findResultShift;
        }
    }

    public class FindPreviousSelectionCommand : Command
    {
        readonly int findResultShift;
        public FindPreviousSelectionCommand(TextEditorData editor, int findResultShift) : base(editor)
        {
            TakeArgument = true;
            this.findResultShift = findResultShift;
        }

        protected override void Run()
        {
            var originalCaretPosition = Editor.Caret.Offset;
            for (int i = 0; i < Count; i++)
            {
                var offset = StringUtils.FindPreviousInLine(Editor.Text, Editor.Caret.Offset, Argument);
                if (offset <= 0)
                    return;

                Editor.Caret.Offset = offset;
                Editor.SetSelection(offset, originalCaretPosition);
            }
            Editor.Caret.Offset += findResultShift;
        }
    }

    public class PasteSelectionCommand : Command
    {

        public PasteSelectionCommand (TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[] { '\r', '\n' }) > 0)
            {
                if (Editor.Caret.Line == 1)
                {
                    Editor.Caret.Offset = 0;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = 0;
                    Motion.LineStart(Editor);
                }
                else
                {
                    Motion.Up(Editor);
                    Motion.LineEnd(Editor);
                    Editor.Caret.Offset++;
                    int oldOffset = Editor.Caret.Offset;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = oldOffset;
                    Motion.LineStart(Editor);
                }
            }
            else
            {
                Editor.InsertAtCaret(text);

                Editor.Caret.Offset--;
            }
            RequestedMode = Mode.Normal;
        }

    }

    public class VisualMode : ViMode
    {
        private int _startOffset;

        public SelectMode Select { get; set; }

        public VisualMode(TextEditorData editor) : base(editor)
        {
            // visual mode keys
            CommandMap.Add("d", new CutSelectionCommand(editor));
            CommandMap.Add("x", new CutSelectionCommand(editor));
            CommandMap.Add("y", new YankSelectionCommand(editor));
            CommandMap.Add("Y", new YankSelectionCommand(editor));
            CommandMap.Add(">", new IndentSelectionCommand(editor));
            CommandMap.Add("<", new RemoveIndentSelectionCommand(editor));
            CommandMap.Add("c", new ChangeSelectionCommand(editor));
            CommandMap.Add("w", new WordCommand(editor));
            CommandMap.Add("e", new WordEndCommand(editor));
            CommandMap.Add("b", new WordBackCommand(editor));
            CommandMap.Add("G", new GoToLineCommand(editor));
            CommandMap.Add("f", new FindSelectionCommand(editor, 0));
            CommandMap.Add("t", new FindSelectionCommand(editor, -1));
            CommandMap.Add("p", new PasteSelectionCommand(editor));
            CommandMap.Add("F", new FindPreviousSelectionCommand(editor, 0));
            CommandMap.Add("T", new FindPreviousSelectionCommand(editor, 1));

            // function key remaps
            SpecialKeyCommandMap.Add(SpecialKey.Delete, new CutSelectionCommand(editor));
        }

        #region implemented abstract members of ViMode

        private void UpdateSelection()
        {
            if (Select == SelectMode.Line)
            {
                var startLine = Editor.GetLineByOffset(_startOffset);
                var endLine = Editor.GetLineByOffset(Editor.Caret.Offset);
                Motion.SetSelectLines(Editor, startLine.LineNumber, endLine.LineNumber);
            }
            else
            {
                // Make sure we always select whole eol symbol
                DocumentLine line = Editor.GetLine(Editor.Caret.Line);
                int end;
                if (Editor.Caret.Offset >= line.EndOffset && Editor.Caret.Offset < line.EndOffsetIncludingDelimiter - 1)
                    end = line.EndOffsetIncludingDelimiter - 1;
                else
                    end = Editor.Caret.Offset;

                Editor.SetSelection(_startOffset + (Editor.Caret.Offset < _startOffset ? 1 : 0), end + (Editor.Caret.Offset >= _startOffset ? 1 : 0));
            }
        }

        protected override void Activate()
        {
            _startOffset = Editor.Caret.Offset;
            Editor.Caret.Mode = CaretMode.Block;
            UpdateSelection();
            AllowCaretOnEol = true;
        }

        protected override void Deactivate()
        {
            Editor.Caret.Mode = CaretMode.Insert;
            Editor.ClearSelection();
            AllowCaretOnEol = false;
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            var ret = base.KeyPress(descriptor);

            if (RequestedMode == Mode.None)
            {
                UpdateSelection();
            }

            return ret;
        }

        #endregion
    }
}

