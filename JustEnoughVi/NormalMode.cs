using System;
using System.Collections.Generic;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class NormalMode : ViMode
    {
        public NormalMode(TextEditorData editor) : base(editor)
        {
            // standard motion keys
            KeyMap.Add("k", MotionUp);
            KeyMap.Add("j", MotionDown);
            KeyMap.Add("h", MotionLeft);
            KeyMap.Add("l", MotionRight);

            KeyMap.Add("^b", MotionPageUp);
            KeyMap.Add("^f", MotionPageDown);
            KeyMap.Add("^d", MotionPageUp);
            KeyMap.Add("^u", MotionPageDown);

            // normal mode keys
            KeyMap.Add("0", FirstColumn);
            KeyMap.Add("a", Append);
            KeyMap.Add("A", AppendEnd);
            KeyMap.Add("b", WordBack);
            KeyMap.Add("c", Change);
            KeyMap.Add("C", ChangeToEnd);
            KeyMap.Add("d", Delete);
            KeyMap.Add("D", DeleteToEnd);
            KeyMap.Add("g", Go);
            KeyMap.Add("G", GoToLine);
            KeyMap.Add("i", Insert);
            KeyMap.Add("I", InsertStart);
            KeyMap.Add("J", Join);
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
            KeyMap.Add("$", LineEnd);
            KeyMap.Add("/", Find);
            KeyMap.Add("<", IndentRemove);
            KeyMap.Add(">", IndentAdd);
            KeyMap.Add("^", LineStart);
            KeyMap.Add("_", LineStart);
            KeyMap.Add("%", MatchingBrace);
        }

        private bool FirstColumn(int count, char[] args)
        {
            Editor.Caret.Column = Mono.TextEditor.DocumentLocation.MinColumn;
            return true;
        }

        private bool Append(int count, char[] args)
        {
            CaretMoveActions.Right(Editor);
            RequestedMode = Mode.Insert;
            return true;
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

            if (args[0] == 'c')
            {
                CaretMoveActions.LineStart(Editor);
                int start = Editor.Caret.Offset;
                CaretMoveActions.LineEnd(Editor);
                Editor.SetSelection(start, Editor.Caret.Offset);
                ClipboardActions.Cut(Editor);
                RequestedMode = Mode.Insert;
            }

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
                CaretMoveActions.LineStart(Editor);
            }
            else if (args[0] == 'w')
            {
                int wordOffset = StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset);
                Editor.SetSelection(Editor.Caret.Offset, wordOffset);
                ClipboardActions.Cut(Editor);
            }
            else if (args[0] == '$')
            {
                DeleteToEnd(1, new char[]{ });
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

        private bool Go(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'g')
            {
                GoToLine(Math.Max(1, count), new char[]{ });
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
            CaretMoveActions.LineStart(Editor);

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
            CaretMoveActions.Left(Editor);
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
            CaretMoveActions.Up(Editor);
            if (Editor.Caret.Line == Mono.TextEditor.DocumentLocation.MinLine)
            {
                Editor.Caret.Column = 1;
                MiscActions.InsertNewLine(Editor);
                CaretMoveActions.Up(Editor);
            }
            else
            {
                MiscActions.InsertNewLineAtEnd(Editor);
            }
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool PasteAppend(int count, char[] args)
        {
            // can the clipboard content be pulled without Gtk?
            var clipboard = Gtk.Clipboard.Get(Mono.TextEditor.ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return true;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[]{ '\r', '\n' }) > 0)
            {
                int oldOffset = Editor.Caret.Offset;
                CaretMoveActions.LineEnd(Editor);
                Editor.Caret.Offset++;
                Editor.InsertAtCaret(text);
                Editor.Caret.Offset = oldOffset;
                MotionDown();
                CaretMoveActions.LineStart(Editor);
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
            var clipboard = Gtk.Clipboard.Get(Mono.TextEditor.ClipboardActions.CopyOperation.CLIPBOARD_ATOM);

            if (!clipboard.WaitIsTextAvailable())
                return true;

            string text = clipboard.WaitForText();

            if (text.IndexOfAny(new char[]{ '\r', '\n' }) > 0)
            {
                if (Editor.Caret.Line == 1)
                {
                    Editor.Caret.Offset = 0;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = 0;
                    CaretMoveActions.LineStart(Editor);
                }
                else
                {
                    MotionUp();
                    LineEnd(1, new char[]{ });
                    Editor.Caret.Offset++;
                    int oldOffset = Editor.Caret.Offset;
                    Editor.InsertAtCaret(text);
                    Editor.Caret.Offset = oldOffset;
                    CaretMoveActions.LineStart(Editor);
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

        private bool Find(int count, char[] args)
        {
            MonoDevelop.Ide.IdeApp.CommandService.DispatchCommand(MonoDevelop.Ide.Commands.SearchCommands.Find);
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
                YankLine(1, new char[]{});

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

        private bool LineEnd(int count, char[] args)
        {
            CaretMoveActions.LineEnd(Editor);
            return true;
        }

        private bool LineStart(int count, char[] args)
        {
            CaretMoveActions.LineStart(Editor);
            Editor.Caret.Offset = StringUtils.NextWordOffset(Editor.Text, Editor.Caret.Offset);
            return true;
        }

        private bool MatchingBrace(int count, char[] args)
        {
            MiscActions.GotoMatchingBracket(Editor);
            return true;
        }

        #region implemented abstract members of ViMode

        protected override void Activate()
        {
            MiscActions.SwitchCaretMode(Editor);
        }

        protected override void Deactivate()
        {
            MiscActions.SwitchCaretMode(Editor);
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return base.KeyPress(descriptor);
        }

        #endregion
    }
}

