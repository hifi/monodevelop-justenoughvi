using System;
using System.Collections.Generic;
using Mono.TextEditor;
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
                var count = 0;
                int.TryParse(_countString, out count);
                return count;   
            }
        }

        public NormalMode(TextEditorData editor) : base(editor)
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

            var line = Editor.GetLine(Editor.Caret.Line);
            if (line.EndOffset > line.Offset && Editor.Caret.Offset == line.EndOffset)
                CaretMoveActions.Left(Editor);
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
                
//                EditActions.JoinLines(Editor);
            }
            return true;
        }

        private bool Down(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Down(Editor);
            }

            return true;
        }

        private bool Left(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                if (Mono.TextEditor.DocumentLocation.MinColumn < Editor.Caret.Column)
                    CaretMoveActions.Left(Editor);
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

        private bool Up(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Up(Editor);
            }

            return true;
        }

        private bool Right(int count, char[] args)
        {
            count = Math.Min(Math.Max(count, 1), Editor.GetLine(Editor.Caret.Line).EndOffset - Editor.Caret.Offset - 1);

            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Right(Editor);
            }

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
                Down(1, new char[]{});
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
                    Up(1, new char[]{});
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
            return true;
        }

        private bool MatchingBrace(int count, char[] args)
        {
            MiscActions.GotoMatchingBracket(Editor);
            return true;
        }

        #region implemented abstract members of ViMode

        public override void Activate()
        {
            MiscActions.SwitchCaretMode(Editor);
        }

        public override void Deactivate()
        {
            MiscActions.SwitchCaretMode(Editor);
            Reset();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            if ((descriptor.ModifierKeys == ModifierKeys.Control && (descriptor.KeyChar == 'f' || descriptor.KeyChar == 'd')) ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageDown))
            {
                // This isn't quite right. Ctrl-f should be full page down, Ctrl-d should be half page down
                Editor.Caret.Line += Math.Min(Editor.LineCount - Editor.Caret.Line, 20);
                CaretMoveActions.LineStart(Editor);
                Editor.CenterToCaret();
                return false;
            }

            if ((descriptor.ModifierKeys == ModifierKeys.Control && (descriptor.KeyChar == 'b' || descriptor.KeyChar == 'u')) ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageUp))
            {
                Editor.Caret.Line -= Math.Min(Editor.Caret.Line - 1, 20);
                CaretMoveActions.LineStart(Editor);
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

