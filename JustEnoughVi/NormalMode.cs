using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class FirstColumnCommand : Command
    {
        public FirstColumnCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.FirstColumn(Editor);
        }
    }

    public class AppendCommand : Command
    {
        public AppendCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            CaretMoveActions.Right(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class ChangeLineCommand : Command
    {
        public ChangeLineCommand(TextEditorData editor) : base(editor) { }

        protected override void Run()
        {
            Motion.LineStart(Editor);
            int start = Editor.Caret.Offset;
            Motion.LineEnd(Editor);
            Editor.SetSelection(start, Editor.Caret.Offset);
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Insert;
        }
    }

    public class FindCommand : Command
    {
        public FindCommand(TextEditorData editor) : base(editor)
        {
            TakeArgument = true;
        }

        protected override void Run()
        {
            var offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, Argument);
            if (offset > -1)
                Editor.Caret.Offset = offset;
        }
    }

    public class NormalMode : ViMode
    {
        public NormalMode(TextEditorData editor) : base(editor)
        {
            // normal mode commands
            CommandMap.Add("0", new FirstColumnCommand(editor));
            CommandMap.Add("a", new AppendCommand(editor));
            CommandMap.Add("cc", new ChangeLineCommand(editor));
            CommandMap.Add("f", new FindCommand(editor));

            // normal mode keys
            KeyMap.Add("A", AppendEnd);
            KeyMap.Add("b", WordBack);
            KeyMap.Add("c", Change);
            KeyMap.Add("C", ChangeToEnd);
            KeyMap.Add("d", Delete);
            KeyMap.Add("D", DeleteToEnd);
            KeyMap.Add("F", FindPrevious);
            KeyMap.Add("g", Go);
            KeyMap.Add("G", GoToLine);
            KeyMap.Add("i", Insert);
            KeyMap.Add("I", InsertStart);
            KeyMap.Add("J", Join);
            KeyMap.Add("n", SearchNext);
            KeyMap.Add("N", SearchPrevious);
            KeyMap.Add("o", OpenBelow);
            KeyMap.Add("O", OpenAbove);
            KeyMap.Add("p", PasteAppend);
            KeyMap.Add("P", PasteInsert);
            KeyMap.Add("r", Replace);
            KeyMap.Add("u", Undo);
            KeyMap.Add("v", Visual);
            KeyMap.Add("V", VisualLine);
            KeyMap.Add("w", Word);
            KeyMap.Add("x", DeleteCharacter);
            KeyMap.Add("y", Yank);
            KeyMap.Add("Y", YankLine);
            KeyMap.Add("z", Recenter);
            KeyMap.Add("/", Search);
            KeyMap.Add("<", IndentRemove);
            KeyMap.Add(">", IndentAdd);
            KeyMap.Add("%", MatchingBrace);

            // remaps
            KeyMap.Add("Delete", DeleteCharacter);
        }

        private bool AppendEnd(int count, char[] args)
        {
            CaretMoveActions.LineEnd(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool WordBack(int count, char[] args)
        {
            Editor.Caret.Offset = StringUtils.PreviousWordOffset(Editor.Text, Editor.Caret.Offset);
            return true;
        }

        private bool Change(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == '$')
            {
                ChangeToEnd(1, new char[] { });
                return true;
            }

            if (args[0] == 'w')
            {
                Editor.SetSelection(Editor.Caret.Offset, StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset));
                ClipboardActions.Cut(Editor);
                RequestedMode = Mode.Insert;
            }

            else if (args[0] == 'i')
            {
                if (args.Length < 2)
                    return false;

                if (args[1] == '"')
                {
                    if (Editor.Text[Editor.Caret.Offset] != '"')
                        return true;

                    int offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, '"');
                    if (offset > 0)
                    {
                        CaretMoveActions.Right(Editor);
                        Editor.SetSelection(Editor.Caret.Offset, offset);
                        ClipboardActions.Cut(Editor);
                        RequestedMode = Mode.Insert;
                    }
                }

                if (args[1] == '(')
                {
                    if (Editor.Text[Editor.Caret.Offset] != '(')
                        return true;

                    int offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, ')');
                    if (offset > 0)
                    {
                        CaretMoveActions.Right(Editor);
                        Editor.SetSelection(Editor.Caret.Offset, offset);
                        ClipboardActions.Cut(Editor);
                        RequestedMode = Mode.Insert;
                    }
                }
            }

            return true;
        }

        private bool ChangeToEnd(int count, char[] args)
        {
            int start = Editor.Caret.Offset;
            CaretMoveActions.LineEnd(Editor);
            Editor.SetSelection(start, Editor.Caret.Offset);
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool Delete(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'd')
            {
                // hack for last line, it doesn't actually cut the line though
                if (Editor.Caret.Offset == Editor.Text.Length)
                {
                    var line = Editor.GetLine(Editor.Caret.Line);
                    if (line.Offset == line.EndOffset)
                    {
                        DeleteActions.Backspace(Editor);
                        return true;
                    }
                }

                SetSelectLines(Editor.Caret.Line, Editor.Caret.Line + count + (count > 0 ? -1 : 0));
                ClipboardActions.Cut(Editor);
                Motion.LineStart(Editor);
            }
            else if (args[0] == 'w')
            {
                int wordOffset = StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset);
                Editor.SetSelection(Editor.Caret.Offset, wordOffset);
                ClipboardActions.Cut(Editor);
            }
            else if (args[0] == '$')
            {
                DeleteToEnd(1, new char[] { });
            }

            return true;
        }

        private bool DeleteToEnd(int count, char[] args)
        {
            var line = Editor.GetLine(Editor.Caret.Line);
            Editor.SetSelection(Editor.Caret.Offset, line.EndOffset);
            ClipboardActions.Cut(Editor);
            return true;
        }

        private bool Find(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            var offset = StringUtils.FindNextInLine(Editor.Text, Editor.Caret.Offset, args[0]);
            if (offset > -1)
                Editor.Caret.Offset = offset;
            return true;
        }

        private bool FindPrevious(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            var offset = StringUtils.FindPreviousInLine(Editor.Text, Editor.Caret.Offset, args[0]);
            if (offset > -1)
                Editor.Caret.Offset = offset;
            return true;
        }

        private bool Go(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'g')
            {
                GoToLine(Math.Max(1, count), new char[] { });
            }

            if (args[0] == 'd')
            {
                Dispatch("MonoDevelop.Refactoring.RefactoryCommands.GotoDeclaration");
                return true;
            }

            if (args[0] == 't')
            {
                Dispatch(WindowCommands.NextDocument);
                return true;
            }

            if (args[0] == 'T')
            {
                Dispatch(WindowCommands.PrevDocument);
                return true;
            }

            return true;
        }

        private bool GoToLine(int count, char[] args)
        {
            if (count == 0)
            {
                CaretMoveActions.ToDocumentEnd(Editor);
            }
            else
            {
                Editor.Caret.Line = count;
            }

            Motion.LineStart(Editor);
            return true;
        }

        private bool Join(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.LineEnd(Editor);
                int selectStart = Editor.Caret.Offset;

                while (Char.IsWhiteSpace(Editor.Text[Editor.Caret.Offset]))
                    Editor.Caret.Offset++;

                Editor.SetSelection(selectStart, Editor.Caret.Offset);
                Editor.DeleteSelectedText();
                Editor.InsertAtCaret(" ");
                Editor.Caret.Offset--;
            }
            return true;
        }

        private bool Insert(int count, char[] args)
        {
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool InsertStart(int count, char[] args)
        {
            Motion.LineStart(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool OpenBelow(int count, char[] args)
        {
            MiscActions.InsertNewLineAtEnd(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool OpenAbove(int count, char[] args)
        {
            if (Editor.Caret.Line == DocumentLocation.MinLine)
            {
                Editor.Caret.Column = 1;
                MiscActions.InsertNewLine(Editor);
                CaretMoveActions.Up(Editor);
            }
            else
            {
                CaretMoveActions.Up(Editor);
                MiscActions.InsertNewLineAtEnd(Editor);
            }
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool PasteAppend(int count, char[] args)
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return true;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[] { '\r', '\n' }) > 0)
            {
                int oldOffset = Editor.Caret.Offset;
                CaretMoveActions.LineEnd(Editor);
                Editor.Caret.Offset++;
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset = oldOffset;
                Motion.Down(Editor);
                Motion.LineStart(Editor);
            }
            else
            {
                Editor.Caret.Offset++;
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset--;
            }

            return true;
        }

        private bool PasteInsert(int count, char[] args)
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return true;

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

            return true;
        }

        private bool Replace(int count, char[] args)
        {
            if (args.Length < 1)
                return false;

            if (Char.IsControl(args[0]))
                return true;

            Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + 1);
            DeleteActions.Delete(Editor);
            Editor.InsertAtCaret(Char.ToString(args[0]));
            Editor.Caret.Offset--;
            return true;
        }

        private bool Undo(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                MiscActions.Undo(Editor);
            }
            Editor.ClearSelection();
            return true;
        }

        private bool Visual(int count, char[] args)
        {
            RequestedMode = Mode.Visual;
            return true;
        }

        private bool VisualLine(int count, char[] args)
        {
            RequestedMode = Mode.VisualLine;
            return true;
        }

        private bool Search(int count, char[] args)
        {
            Dispatch(SearchCommands.Find);
            return true;
        }

        private bool SearchNext(int count, char[] args)
        {
            Dispatch(SearchCommands.FindNext);
            return true;
        }

        private bool SearchPrevious(int count, char[] args)
        {
            Dispatch(SearchCommands.FindPrevious);
            return true;
        }

        private bool IndentRemove(int count, char[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == '<')
                    count = 1;
                else
                    return true;
            }

            if (count < 1)
                return false;

            SetSelectLines(Editor.Caret.Line, Editor.Caret.Line);

            for (int i = 0; i < count; i++)
            {
                MiscActions.RemoveIndentSelection(Editor);
            }

            Editor.ClearSelection();
            return true;
        }

        private bool IndentAdd(int count, char[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == '>')
                    count = 1;
                else
                    return true;
            }

            if (count < 1)
                return false;

            SetSelectLines(Editor.Caret.Line, Editor.Caret.Line);

            for (int i = 0; i < count; i++)
            {
                MiscActions.IndentSelection(Editor);
            }

            Editor.ClearSelection();
            return true;
        }

        private bool Word(int count, char[] args)
        {
            Editor.Caret.Offset = StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset);
            return true;
        }

        private bool DeleteCharacter(int count, char[] args)
        {
            count = Math.Min(Math.Max(count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset);
            if (count > 0)
            {
                Editor.SetSelection(Editor.Caret.Offset, Editor.Caret.Offset + count);
                ClipboardActions.Cut(Editor);
            }
            return true;
        }

        private bool Yank(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'y')
                YankLine(1, new char[] { });

            return true;
        }

        private bool YankLine(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                SetSelectLines(Editor.Caret.Line, Editor.Caret.Line);
                ClipboardActions.Copy(Editor);
                Editor.ClearSelection();
            }

            return true;
        }

        private bool Recenter(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'z')
                Dispatch(TextEditorCommands.RecenterEditor);

            return true;
        }

        private bool MatchingBrace(int count, char[] args)
        {
            MiscActions.GotoMatchingBracket(Editor);
            return true;
        }

        private bool Dispatch(object command)
        {
            return MonoDevelop.Ide.IdeApp.CommandService.DispatchCommand(command);
        }

        #region implemented abstract members of ViMode

        protected override void Activate()
        {
            Editor.Caret.Mode = CaretMode.Block;
        }

        protected override void Deactivate()
        {
            Editor.Caret.Mode = CaretMode.Insert;
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return base.KeyPress(descriptor);
        }

        #endregion
    }
}

