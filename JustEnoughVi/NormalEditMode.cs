using System;
using System.Collections.Generic;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public class NormalEditMode : BaseEditMode
    {
        private readonly Dictionary<uint, Func<int, char[], bool>> _commands;

        private string _countString;
        private uint? _command;
        private readonly List<char> _commandArgs;

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        public NormalEditMode(TextEditor editor) : base(editor)
        {
            _command = null;
            _commandArgs = new List<char>();
            _countString = "";

            _commands = new Dictionary<uint, Func<int, char[], bool>>();

            _commands.Add('0', LineStart);
            _commands.Add('a', Append);
            _commands.Add('A', AppendEnd);
            _commands.Add('d', Delete);
            _commands.Add('G', Go);
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
        }

        public override void Activate()
        {
            EditActions.SwitchCaretMode(Editor);
        }

        public override void Deactivate()
        {
            EditActions.SwitchCaretMode(Editor);
            Reset();
        }

        protected void Reset()
        {
            _command = null;
            _commandArgs.Clear();
            _countString = "";
        }

        internal static bool IsEol(char c)
        {
            return (c == '\r' || c == '\n');
        }

        private void CaretToLineStart()
        {
            EditActions.MoveCaretToLineStart(Editor);
        }

        private void CaretOffEol()
        {
            if (Editor.CaretOffset >= Editor.Text.Length)
                Editor.CaretOffset = Editor.Text.Length - 1;

            while (NormalEditMode.IsEol (Editor.Text [Editor.CaretOffset]) && DocumentLocation.MinColumn < Editor.CaretColumn)
                EditActions.MoveCaretLeft (Editor);
        }

        private bool LineStart(int count, char[] args)
        {
            Editor.CaretColumn = DocumentLocation.MinColumn;
            return true;
        }

        private bool Append(int count, char[] args)
        {
            EditActions.MoveCaretRight(Editor);
            RequestedMode = ViMode.Insert;
            return true;
        }

        private bool AppendEnd(int count, char[] args)
        {
            EditActions.MoveCaretToLineEnd(Editor);
            RequestedMode = ViMode.Insert;
            return true;
        }

        private bool Delete(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'd')
            {
                Editor.CaretColumn = DocumentLocation.MinColumn;
                int startOffset = Editor.CaretOffset;
                EditActions.MoveCaretToLineEnd(Editor);
                int endOffset = Editor.CaretOffset + Editor.EolMarker.Length;
                Editor.SetSelection(startOffset, endOffset);
                EditActions.ClipboardCopy(Editor);
                Editor.ClearSelection();
                EditActions.DeleteCurrentLine(Editor);

                CaretToLineStart();
            }
            else if (args[0] == 'w')
            {
                int wordLength = CalculateWordLength(Editor.Text, Editor.CaretOffset);
                Editor.SetSelection(Editor.CaretOffset, Editor.CaretOffset + wordLength);
                EditActions.ClipboardCut(Editor);
            }
            else if (args[0] == '$')
            {
                // this might need some work but it's ok for now
                int eol = 0;
                for (int i = Editor.CaretOffset; i < Editor.Text.Length; i++)
                {
                    if (Editor.Text[i] == '\r' || Editor.Text[i] == '\n')
                    {
                        eol = i;
                        break;
                    }
                }

                if (eol > 0)
                {
                    Editor.SetSelection(Editor.CaretOffset, eol);
                    EditActions.ClipboardCut(Editor);
                }
            }

            return true;
        }

        private bool Go(int count, char[] args)
        {
            if (count == 0)
            {
                EditActions.MoveCaretToDocumentEnd(Editor);
            }
            else
            {
                Editor.CaretLine = count;
            }

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

            CaretOffEol();

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
            RequestedMode = ViMode.Insert;
            return true;
        }

        private bool InsertStart(int count, char[] args)
        {
            CaretToLineStart();
            RequestedMode = ViMode.Insert;
            return true;
        }

        private bool Up(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.MoveCaretUp(Editor);
            }

            CaretOffEol();

            return true;
        }

        private bool Right(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EditActions.MoveCaretRight(Editor);
            }

            CaretOffEol();

            return true;
        }

        private bool OpenBelow(int count, char[] args)
        {
            EditActions.InsertNewLineAtEnd(Editor);
            RequestedMode = ViMode.Insert;
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
            RequestedMode = ViMode.Insert;
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
                if (!text.EndsWith(Editor.EolMarker))
                    text += Editor.EolMarker;
                int oldOffset = Editor.CaretOffset;
                EditActions.MoveCaretToLineEnd(Editor);
                Editor.CaretOffset++;
                Editor.InsertAtCaret(text);
                Editor.CaretOffset = oldOffset;
                Down(1, new char[]{});
                CaretToLineStart();
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
                if (!text.EndsWith(Editor.EolMarker))
                    text += Editor.EolMarker;
                if (Editor.CaretLine == 1)
                {
                    Editor.CaretOffset = 0;
                    Editor.InsertAtCaret(text);
                    Editor.CaretOffset = 0;
                    CaretToLineStart(); // if indentation before pasted text
                }
                else
                {
                    Up(1, new char[]{});
                    LineEnd(1, new char[]{ });
                    Editor.CaretOffset++;
                    int oldOffset = Editor.CaretOffset;
                    Editor.InsertAtCaret(text);
                    Editor.CaretOffset = oldOffset;
                    CaretToLineStart();
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
            RequestedMode = ViMode.Visual;
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

            // FIXME: seriously
            int origOffset = Editor.CaretOffset;
            EditActions.MoveCaretToLineStart(Editor);
            int startOffset = Editor.CaretOffset;
            EditActions.MoveCaretToLineEnd(Editor);
            int endOffset = Editor.CaretOffset;
            Editor.CaretOffset = origOffset;

            Editor.SetSelection(startOffset, endOffset);

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

            // FIXME: seriously
            int origOffset = Editor.CaretOffset;
            EditActions.MoveCaretToLineStart(Editor);
            int startOffset = Editor.CaretOffset;
            EditActions.MoveCaretToLineEnd(Editor);
            int endOffset = Editor.CaretOffset;
            Editor.CaretOffset = origOffset;

            Editor.SetSelection(startOffset, endOffset);

            for (int i = 0; i < count; i++)
            {
                EditActions.IndentSelection(Editor);
            }

            Editor.ClearSelection();
            return true;
        }

        private bool Word(int count, char[] args)
        {
            Editor.CaretOffset += CalculateWordLength(Editor.Text, Editor.CaretOffset);
            return true;
        }

        private bool DeleteCharacter(int count, char[] args)
        {
            count = Math.Max(1, count);
            Editor.SetSelection(Editor.CaretOffset, Editor.CaretOffset + count);
            EditActions.ClipboardCut(Editor);
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
                int origOffset = Editor.CaretOffset;
                Editor.CaretColumn = DocumentLocation.MinColumn;
                int startOffset = Editor.CaretOffset;
                EditActions.MoveCaretToLineEnd(Editor);
                int endOffset = Editor.CaretOffset + Editor.EolMarker.Length;
                Editor.SetSelection(startOffset, endOffset);
                EditActions.ClipboardCopy(Editor);
                Editor.ClearSelection();
                Editor.CaretOffset = origOffset;
            }

            return true;
        }

        private bool LineEnd(int count, char[] args)
        {
            EditActions.MoveCaretToLineEnd(Editor);
            CaretOffEol();
            return true;
        }

        static bool IsCodePunctuation(char c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                case ';':
                case ':':
                case ',':
                case '.':
                case '"':
                case '\'':
                    return true;
            }

            return false;
        }

        static int CalculateWordLength(string searchText, int offset)
        {
            int endOffset = offset;

            if (IsCodePunctuation(searchText[offset]))
            {
                while (endOffset < searchText.Length && IsCodePunctuation(searchText[endOffset]))
                    endOffset++;
            }
            else
            {
                while (endOffset < searchText.Length && !Char.IsWhiteSpace(searchText[endOffset]) && !IsCodePunctuation(searchText[endOffset]))
                    endOffset++;
            }

            if (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset]))
            {
                while (endOffset < searchText.Length && (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset])))
                    endOffset++;
            }

            return endOffset - offset;
        }

        #region implemented abstract members of EditMode

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            if ((descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == 'f') ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageDown))
            {
                EditActions.ScrollPageDown(Editor);
                return false;
            }

            if ((descriptor.ModifierKeys == ModifierKeys.Control && descriptor.KeyChar == 'b') ||
                (descriptor.ModifierKeys == 0 && descriptor.SpecialKey == SpecialKey.PageUp))
            {
                EditActions.ScrollPageUp(Editor);
                return false;
            }

            if (descriptor.ModifierKeys == 0)
            {
                uint unicodeKey = descriptor.KeyChar;

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

                if (_command == null)
                {
                    // build repeat buffer
                    if ((_countString.Length > 0 || unicodeKey > '0') && unicodeKey >= '0' && unicodeKey <= '9')
                    {
                        _countString += Char.ToString((char)unicodeKey);
                        return false;
                    }

                    _command = unicodeKey;
                }
                else
                {
                    _commandArgs.Add((char)unicodeKey);
                }

                if (!_commands.ContainsKey((uint)_command))
                    return false;

                CaretOffEol();

                if (_commands[(uint)_command](Count, _commandArgs.ToArray()))
                {
                    Reset();
                }
            }

            return false;
        }

        #endregion
    }
}

