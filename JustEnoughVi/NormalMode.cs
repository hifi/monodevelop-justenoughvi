using System;
using System.Collections.Generic;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class NormalMode : ViMode
    {
        private readonly Dictionary<char, Func<int, char[], bool>> _commands;

        private string _countString;
        private readonly List<char> _commandBuf;

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        public NormalMode(TextEditor editor) : base(editor)
        {
            _commandBuf = new List<char>();
            _countString = "";

            _commands = new Dictionary<char, Func<int, char[], bool>>();

            _commands.Add('0', FirstColumn);
            _commands.Add('a', Append);
            _commands.Add('A', AppendEnd);
            _commands.Add('b', WordBack);
            _commands.Add('c', Change);
            _commands.Add('C', ChangeToEnd);
            _commands.Add('d', Delete);
            _commands.Add('D', DeleteToEnd);
            _commands.Add('g', Go);
            _commands.Add('G', GoToLine);
            _commands.Add('h', Left);
            _commands.Add('i', Insert);
            _commands.Add('I', InsertStart);
            _commands.Add('j', Down);
            _commands.Add('J', Join);
            _commands.Add('k', Up);
            _commands.Add('l', Right);
            _commands.Add('o', OpenBelow);
            _commands.Add('O', OpenAbove);
            _commands.Add('p', PasteAppend);
            _commands.Add('P', PasteInsert);
            _commands.Add('r', Replace);
            _commands.Add('u', Undo);
            _commands.Add('V', VisualLine);
            _commands.Add('w', Word);
            _commands.Add('x', DeleteCharacter);
            _commands.Add('y', Yank);
            _commands.Add('Y', YankLine);
            _commands.Add('$', LineEnd);
            _commands.Add('/', Find);
            _commands.Add('<', IndentRemove);
            _commands.Add('>', IndentAdd);
            _commands.Add('^', LineStart);
            _commands.Add('_', LineStart);
            _commands.Add('%', MatchingBrace);
        }

        protected void Reset()
        {
            _commandBuf.Clear();
            _countString = "";
        }

        private void CaretOffEol()
        {
            if (RequestedMode == Mode.Insert)
                return;

            var line = Editor.GetLine(Editor.CaretLine);
            if (line.EndOffset > line.Offset && Editor.CaretOffset == line.EndOffset)
                EditActions.MoveCaretLeft(Editor);
        }

        private bool FirstColumn(int count, char[] args)
        {
            Editor.CaretColumn = DocumentLocation.MinColumn;
            return true;
        }

        private bool Append(int count, char[] args)
        {
            EditActions.MoveCaretRight(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool AppendEnd(int count, char[] args)
        {
            EditActions.MoveCaretToLineEnd(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool WordBack(int count, char[] args)
        {
            Editor.CaretOffset = StringUtils.PreviousWordOffset(Editor.Text, Editor.CaretOffset);
            return true;
        }

        private bool Change(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'c')
            {
                EditActions.MoveCaretToLineStart(Editor);
                int start = Editor.CaretOffset;
                EditActions.MoveCaretToLineEnd(Editor);
                Editor.SetSelection(start, Editor.CaretOffset);
                EditActions.ClipboardCut(Editor);
                RequestedMode = Mode.Insert;
            }

            if (args[0] == '$')
            {
                ChangeToEnd(1, new char[] { });
                return true;
            }

            if (args[0] == 'w')
            {
                Editor.SetSelection(Editor.CaretOffset, StringUtils.NextWordOffset(Editor.Text, Editor.CaretOffset));
                EditActions.ClipboardCut(Editor);
                RequestedMode = Mode.Insert;
            }

            else if (args[0] == 'i')
            {
                if (args.Length < 2)
                    return false;

                if (args[1] == '"')
                {
                    if (Editor.Text[Editor.CaretOffset] != '"')
                        return true;

                    int offset = StringUtils.FindNextInLine(Editor.Text, Editor.CaretOffset, '"');
                    if (offset > 0)
                    {
                        EditActions.MoveCaretRight(Editor);
                        Editor.SetSelection(Editor.CaretOffset, offset);
                        EditActions.ClipboardCut(Editor);
                        RequestedMode = Mode.Insert;
                    }
                }

                if (args[1] == '(')
                {
                    if (Editor.Text[Editor.CaretOffset] != '(')
                        return true;

                    int offset = StringUtils.FindNextInLine(Editor.Text, Editor.CaretOffset, ')');
                    if (offset > 0)
                    {
                        EditActions.MoveCaretRight(Editor);
                        Editor.SetSelection(Editor.CaretOffset, offset);
                        EditActions.ClipboardCut(Editor);
                        RequestedMode = Mode.Insert;
                    }
                }
            }

            return true;
        }

        private bool ChangeToEnd(int count, char[] args)
        {
            int start = Editor.CaretOffset;
            EditActions.MoveCaretToLineEnd(Editor);
            Editor.SetSelection(start, Editor.CaretOffset);
            EditActions.ClipboardCut(Editor);
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
                if (Editor.CaretOffset == Editor.Text.Length)
                {
                    var line = Editor.GetLine(Editor.CaretLine);
                    if (line.Offset == line.EndOffset)
                    {
                        EditActions.Backspace(Editor);
                        return true;
                    }
                }

                SetSelectLines(Editor.CaretLine, Editor.CaretLine + count + (count > 0 ? -1 : 0));
                EditActions.ClipboardCut(Editor);
                EditActions.MoveCaretToLineStart(Editor);
            }
            else if (args[0] == 'w')
            {
                int wordOffset = StringUtils.NextWordOffset(Editor.Text, Editor.CaretOffset);
                Editor.SetSelection(Editor.CaretOffset, wordOffset);
                EditActions.ClipboardCut(Editor);
            }
            else if (args[0] == '$')
            {
                DeleteToEnd(1, new char[]{ });
            }

            return true;
        }

        private bool DeleteToEnd(int count, char[] args)
        {
            var line = Editor.GetLine(Editor.CaretLine);
            Editor.SetSelection(Editor.CaretOffset, line.EndOffset);
            EditActions.ClipboardCut(Editor);
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
                EditActions.MoveCaretToDocumentEnd(Editor);
            }
            else
            {
                Editor.CaretLine = count;
            }

            EditActions.MoveCaretToLineStart(Editor);

            return true;
        }

        private bool Join(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.JoinLines(Editor);
            }
            return true;
        }

        private bool Down(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.MoveCaretDown (Editor);
            }

            return true;
        }

        private bool Left(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                if (DocumentLocation.MinColumn < Editor.CaretColumn)
                    EditActions.MoveCaretLeft(Editor);
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
            EditActions.MoveCaretToLineStart(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool Up(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.MoveCaretUp(Editor);
            }

            return true;
        }

        private bool Right(int count, char[] args)
        {
            count = Math.Min(Math.Max(count, 1), Editor.GetLine(Editor.CaretLine).EndOffset - Editor.CaretOffset - 1);

            for (int i = 0; i < count; i++)
            {
                EditActions.MoveCaretRight(Editor);
            }

            return true;
        }

        private bool OpenBelow(int count, char[] args)
        {
            EditActions.InsertNewLineAtEnd(Editor);
            RequestedMode = Mode.Insert;
            return true;
        }

        private bool OpenAbove(int count, char[] args)
        {
            EditActions.MoveCaretUp(Editor);
            if (Editor.CaretLine == DocumentLocation.MinLine)
            {
                Editor.CaretColumn = 1;
                EditActions.InsertNewLine(Editor);
                EditActions.MoveCaretUp(Editor);
            }
            else
            {
                EditActions.InsertNewLineAtEnd(Editor);
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
                int oldOffset = Editor.CaretOffset;
                EditActions.MoveCaretToLineEnd(Editor);
                Editor.CaretOffset++;
                Editor.InsertAtCaret(text);
                Editor.CaretOffset = oldOffset;
                Down(1, new char[]{});
                EditActions.MoveCaretToLineStart(Editor);
            }
            else
            {
                Editor.CaretOffset++;
                Editor.InsertAtCaret(text);
                Editor.CaretOffset--;
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
                if (Editor.CaretLine == 1)
                {
                    Editor.CaretOffset = 0;
                    Editor.InsertAtCaret(text);
                    Editor.CaretOffset = 0;
                    EditActions.MoveCaretToLineStart(Editor);
                }
                else
                {
                    Up(1, new char[]{});
                    LineEnd(1, new char[]{ });
                    Editor.CaretOffset++;
                    int oldOffset = Editor.CaretOffset;
                    Editor.InsertAtCaret(text);
                    Editor.CaretOffset = oldOffset;
                    EditActions.MoveCaretToLineStart(Editor);
                }
            }
            else
            {
                Editor.InsertAtCaret(text);
                Editor.CaretOffset--;
            }

            return true;
        }

        private bool Replace(int count, char[] args)
        {
            if (args.Length < 1)
                return false;

            if (Char.IsControl(args[0]))
                return true;

            Editor.SetSelection(Editor.CaretOffset, Editor.CaretOffset + 1);
            EditActions.Delete(Editor);
            Editor.InsertAtCaret(Char.ToString(args[0]));
            Editor.CaretOffset--;
            return true;
        }

        private bool Undo(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.Undo(Editor);
            }
            Editor.ClearSelection();
            return true;
        }

        private bool VisualLine(int count, char[] args)
        {
            RequestedMode = Mode.Visual;
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

            SetSelectLines(Editor.CaretLine, Editor.CaretLine);

            for (int i = 0; i < count; i++)
            {
                EditActions.UnIndentSelection(Editor);
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

            SetSelectLines(Editor.CaretLine, Editor.CaretLine);

            for (int i = 0; i < count; i++)
            {
                EditActions.IndentSelection(Editor);
            }

            Editor.ClearSelection();
            return true;
        }

        private bool Word(int count, char[] args)
        {
            Editor.CaretOffset = StringUtils.NextWordOffset(Editor.Text, Editor.CaretOffset);
            return true;
        }

        private bool DeleteCharacter(int count, char[] args)
        {
            count = Math.Min(Math.Max(count, 1), Editor.GetLine(Editor.CaretLine).EndOffset - Editor.CaretOffset);
            if (count > 0)
            {
                Editor.SetSelection(Editor.CaretOffset, Editor.CaretOffset + count);
                EditActions.ClipboardCut(Editor);
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
                SetSelectLines(Editor.CaretLine, Editor.CaretLine);
                EditActions.ClipboardCopy(Editor);
                Editor.ClearSelection();
            }

            return true;
        }

        private bool LineEnd(int count, char[] args)
        {
            EditActions.MoveCaretToLineEnd(Editor);
            return true;
        }

        private bool LineStart(int count, char[] args)
        {
            EditActions.MoveCaretToLineStart(Editor);
            return true;
        }

        private bool MatchingBrace(int count, char[] args)
        {
            EditActions.GotoMatchingBrace(Editor);
            return true;
        }

        #region implemented abstract members of ViMode

        public override void Activate()
        {
            EditActions.SwitchCaretMode(Editor);
        }

        public override void Deactivate()
        {
            EditActions.SwitchCaretMode(Editor);
            Reset();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            if ((descriptor.ModifierKeys == ModifierKeys.Control && (descriptor.KeyChar == 'f' || descriptor.KeyChar == 'd')) ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageDown))
            {
                // This isn't quite right. Ctrl-f should be full page down, Ctrl-d should be half page down
                Editor.CaretLine += Math.Min(Editor.LineCount - Editor.CaretLine, 20);
                EditActions.MoveCaretToLineStart(Editor);
                Editor.CenterToCaret();
                return false;
            }

            if ((descriptor.ModifierKeys == ModifierKeys.Control && (descriptor.KeyChar == 'b' || descriptor.KeyChar == 'u')) ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageUp))
            {
                Editor.CaretLine -= Math.Min(Editor.CaretLine - 1, 20);
                EditActions.MoveCaretToLineStart(Editor);
                Editor.CenterToCaret();
                return false;
            }

            if (descriptor.ModifierKeys == 0)
            {
                char unicodeKey = descriptor.KeyChar;

                // remap some function keys to Vi commands
                if (descriptor.SpecialKey == SpecialKey.Home)
                    unicodeKey = '0';
                else if (descriptor.SpecialKey == SpecialKey.End)
                    unicodeKey = '$';
                else if (descriptor.SpecialKey == SpecialKey.Left)
                    unicodeKey = 'h';
                else if (descriptor.SpecialKey == SpecialKey.Right)
                    unicodeKey = 'l';
                else if (descriptor.SpecialKey == SpecialKey.Up)
                    unicodeKey = 'k';
                else if (descriptor.SpecialKey == SpecialKey.Down)
                    unicodeKey = 'j';
                else if (descriptor.SpecialKey == SpecialKey.Delete)
                    unicodeKey = 'x';
                //else if (descriptor.SpecialKey == SpecialKey.Insert)
                //    unicodeKey = 'i';
                else if (descriptor.SpecialKey == SpecialKey.BackSpace)
                    unicodeKey = 'h';

                // build repeat buffer
                if (_commandBuf.Count == 0 && (_countString.Length > 0 || unicodeKey > '0') && unicodeKey >= '0' && unicodeKey <= '9')
                {
                    _countString += Char.ToString((char)unicodeKey);
                    return false;
                }

                _commandBuf.Add(unicodeKey);

                if (!_commands.ContainsKey(_commandBuf[0]))
                {
                    _commandBuf.Clear();
                    return false;
                }

                CaretOffEol();

                if (_commands[_commandBuf[0]](Count, _commandBuf.GetRange(1, _commandBuf.Count - 1).ToArray()))
                {
                    Reset();
                }

                CaretOffEol();
            }

            return false;
        }

        #endregion
    }
}

