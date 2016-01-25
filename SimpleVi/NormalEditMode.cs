using System;
using System.Collections.Generic;
using Mono.TextEditor;
using SimpleVi;
using MonoDevelop.SourceEditor;
using System.Text.RegularExpressions;

namespace SimpleVi
{
    public class NormalEditMode : EditMode
    {
        private Dictionary<uint, Func<int, char[], bool>> _commands;

        ViEditMode Vi { get; set; }

        private string _countString;
        private uint? _command;
        private List<char> _commandArgs;

        private int Count {
            get {
                try {
                    return Convert.ToInt32(_countString);
                } catch (FormatException) {
                    return 0;
                }
            }
        }

        public NormalEditMode(ViEditMode _editMode)
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

            Vi = _editMode;
        }

        public void Activate()
        {
            _countString = "";
        }

        private void CaretToLineStart()
        {
            Data.Caret.Column = 1;

            while (Char.IsWhiteSpace(Data.Text[Data.Caret.Offset]))
            {
                if (Data.Text[Data.Caret.Offset] == '\r' || Data.Text[Data.Caret.Offset] == '\n')
                {
                    Data.Caret.Offset--;
                    break;
                }

                Data.Caret.Offset++;
            }
        }

        private bool LineStart(int count, char[] args)
        {
            CaretMoveActions.LineStart(Data);
            return true;
        }

        private bool Append(int count, char[] args)
        {
            Right(1, new char[]{});
            Vi.SetMode(ViMode.Insert);
            return true;
        }

        private bool AppendEnd(int count, char[] args)
        {
            LineEnd(1, new char[]{});
            Vi.SetMode(ViMode.Insert);
            return true;
        }

        private bool Delete(int count, char[] args)
        {
            if (args.Length == 0)
                return false;

            if (args[0] == 'd')
            {
                Data.SetSelectLines(Data.Caret.Line, Data.Caret.Line + (Math.Max(count, 1) - 1));
                ClipboardActions.Cut(Data);
                CaretToLineStart();
            }
            else if (args[0] == 'w')
            {
                int wordLength = CalculateWordLength(Data.Text, Data.Caret.Offset);
                Data.SetSelection(Data.Caret.Offset, Data.Caret.Offset + wordLength);
                ClipboardActions.Cut(Data);
            }
            else if (args[0] == '$')
            {
                // this might need some work but it's ok for now
                int eol = 0;
                for (int i = Data.Caret.Offset; i < Data.Text.Length; i++)
                {
                    if (Data.Text[i] == '\r' || Data.Text[i] == '\n')
                    {
                        eol = i;
                        break;
                    }
                }

                if (eol > 0)
                {
                    Data.SetSelection(Data.Caret.Offset, eol);
                    ClipboardActions.Cut(Data);
                }
            }

            return true;
        }

        private bool Go(int count, char[] args)
        {
            if (count == 0)
            {
                Data.Caret.Offset = Data.Text.Length;
            }
            else
            {
                // this throws an exception if you go beyond the document
                // do we need to count the lines first?
                Data.Caret.Line = count;
            }

            return true;
        }

        private bool Join(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                // not exactly up to spec but good enough for normal use for now
                LineEnd(1, new char[]{ });
                Data.InsertAtCaret(" ");
                Delete(1, new char[]{ 'w' }); // shouldn't work like this (beyond line)
                Data.Caret.Offset--;
            }
            return true;
        }

        private bool Down(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Down(Data);
            }

            return true;
        }

        private bool Left(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                if (DocumentLocation.MinColumn < Data.Caret.Column)
                    CaretMoveActions.Left(Data);
            }

            return true;
        }

        private bool Insert(int count, char[] args)
        {
            Vi.SetMode(ViMode.Insert);
            return true;
        }

        private bool InsertStart(int count, char[] args)
        {
            CaretToLineStart();
            Vi.SetMode(ViMode.Insert);
            return true;
        }

        private bool Up(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Up(Data);
            }

            return true;
        }

        private bool Right(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                CaretMoveActions.Right(Data);
            }

            return true;
        }

        private bool OpenBelow(int count, char[] args)
        {
            AppendEnd(1, new char[]{});
            // using a keypress instead of injecting EOL to get auto-indent
            Vi.BaseKeypress(Gdk.Key.Return, '\r', Gdk.ModifierType.None);
            return true;
        }

        private bool OpenAbove(int count, char[] args)
        {
            if (Data.Caret.Line == 1)
            {
                Data.Caret.Offset = 0;
                Data.InsertAtCaret(Data.EolMarker);
                Data.Caret.Offset = 0;
                Insert(1, new char[]{});
            }
            else
            {
                Up(1, new char[]{});
                OpenBelow(1, new char[]{});
            }

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
                int oldOffset = Data.Caret.Offset;
                LineEnd(1, new char[]{ });
                Data.Caret.Offset++;
                Data.InsertAtCaret(text);
                Data.Caret.Offset = oldOffset;
                Down(1, new char[]{});
                CaretToLineStart();
            }
            else
            {
                Data.Caret.Offset++;
                Data.InsertAtCaret(text);
                Data.Caret.Offset--;
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
                if (Data.Caret.Line == 1)
                {
                    Data.Caret.Offset = 0;
                    Data.InsertAtCaret(text);
                    Data.Caret.Offset = 0;
                    CaretToLineStart(); // if indentation before pasted text
                }
                else
                {
                    Up(1, new char[]{});
                    LineEnd(1, new char[]{ });
                    Data.Caret.Offset++;
                    int oldOffset = Data.Caret.Offset;
                    Data.InsertAtCaret(text);
                    Data.Caret.Offset = oldOffset;
                    CaretToLineStart();
                }
            }
            else
            {
                Data.InsertAtCaret(text);
                Data.Caret.Offset--;
            }

            return true;
        }

        private bool Replace(int count, char[] args)
        {
            if (args.Length < 1)
                return false;

            if (Char.IsControl(args[0]))
                return true;

            Data.SetSelection(Data.Caret.Offset, Data.Caret.Offset + 1);
            Data.DeleteSelectedText();
            Data.InsertAtCaret(Char.ToString(args[0]));
            Data.Caret.Offset--;
            return true;
        }

        private bool Undo(int count, char[] args)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                Vi.Document.GetContent<SourceEditorView>().Undo();
            }
            return true;
        }

        private bool VisualLine(int count, char[] args)
        {
            Vi.SetMode(ViMode.Visual);
            Data.SetSelectLines(Data.Caret.Line, Data.Caret.Line);
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

            Data.SetSelectLines(Data.Caret.Line, Data.Caret.Line);
            for (int i = 0; i < count; i++)
            {
                RunAction(MiscActions.RemoveIndentSelection);
            }
            Data.ClearSelection();
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

            Data.SetSelectLines(Data.Caret.Line, Data.Caret.Line);
            for (int i = 0; i < count; i++)
            {
                RunAction(MiscActions.IndentSelection);
            }
            Data.ClearSelection();
            return true;
        }

        private bool Word(int count, char[] args)
        {
            Data.Caret.Offset += CalculateWordLength(Data.Text, Data.Caret.Offset);
            return true;
        }

        private bool DeleteCharacter(int count, char[] args)
        {
            count = Math.Max(1, count);
            Data.SetSelection(Data.Caret.Offset, Data.Caret.Offset + count);
            ClipboardActions.Cut(Data);
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
                Data.SetSelectLines(Data.Caret.Line, Data.Caret.Line + (count - 1));
                ClipboardActions.Copy(Data);
                Data.ClearSelection();
            }

            return true;
        }

        private bool LineEnd(int count, char[] args)
        {
            CaretMoveActions.LineEnd(Data);
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

        protected override void HandleKeypress(Gdk.Key key, uint unicodeKey, Gdk.ModifierType modifier)
        {
            // reset count 
            if (
                (modifier == 0 && key == Gdk.Key.Escape) ||
                (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.c))
            {
                _countString = "";
                _command = null;
                _commandArgs.Clear();
                return;
            }

            if (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.f)
            {
                Vi.BaseKeypress(Gdk.Key.Page_Down, ' ', Gdk.ModifierType.None);
                return;
            }

            if (modifier == Gdk.ModifierType.ControlMask && key == Gdk.Key.b)
            {
                Vi.BaseKeypress(Gdk.Key.Page_Up, ' ', Gdk.ModifierType.None);
                return;
            }

            if (modifier == 0)
            {
                if (key == Gdk.Key.Page_Down || key == Gdk.Key.Page_Up)
                {
                    Vi.BaseKeypress(key, unicodeKey, modifier);
                    return;
                }

                // remap some function keys to Vi commands
                if (key == Gdk.Key.Home)
                    unicodeKey = '0';
                else if (key == Gdk.Key.End)
                    unicodeKey = '$';
                else if (key == Gdk.Key.Left)
                    unicodeKey = 'h';
                else if (key == Gdk.Key.Right)
                    unicodeKey = 'l';
                else if (key == Gdk.Key.Up)
                    unicodeKey = 'k';
                else if (key == Gdk.Key.Down)
                    unicodeKey = 'j';
                else if (key == Gdk.Key.Delete)
                    unicodeKey = 'x';
                else if (key == Gdk.Key.Insert)
                    unicodeKey = 'i';
                else if (key == Gdk.Key.BackSpace)
                    unicodeKey = 'h';

                if (_command == null)
                {
                    // build repeat buffer
                    if ((_countString.Length > 0 || unicodeKey > '0') && unicodeKey >= '0' && unicodeKey <= '9')
                    {
                        _countString += Char.ToString((char)unicodeKey);
                        return;
                    }

                    _command = unicodeKey;
                }
                else
                {
                    _commandArgs.Add((char)unicodeKey);
                }

                if (!_commands.ContainsKey((uint)_command))
                    return;

                if (_commands[(uint)_command](Count, _commandArgs.ToArray()))
                {
                    // succeeded, reset everything
                    _countString = "";
                    _command = null;
                    _commandArgs.Clear();
                }
            }

            // never let the cursor sit on an EOL
            while (Vi.Mode == ViMode.Normal && ViEditMode.IsEol(Data.Document.GetCharAt(Data.Caret.Offset)) && DocumentLocation.MinColumn < Data.Caret.Column)
                CaretMoveActions.Left(Data);
        }


        #endregion
    }
}

