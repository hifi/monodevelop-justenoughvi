using System;
using System.Text;
using JustEnoughVi;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor.Extension;
using NUnit.Framework;

namespace JustEnoughViTests
{
    public class TextEditorTestBase
    {
        static int x = GuiUnit.TestRunner.ExitCode;// hack to get GuiUnit into the AppDomain

        public void ProcessKeys(string keyPresses, ViMode mode)
        {
            foreach (var c in keyPresses)
            {
                // TODO: modifier parsing
                var descriptor = KeyDescriptor.FromGtk(Gdk.Key.a /* important? */, c, Gdk.ModifierType.None);
                mode.KeyPress(descriptor);
            }
        }

        public static TextEditorData Create(string content, ITextEditorOptions options = null)
        {
            var sb = new StringBuilder();
            int caretIndex = -1, selectionStart = -1, selectionEnd = -1;

            for (int i = 0; i < content.Length; i++)
            {
                var ch = content[i];
                switch (ch)
                {
                case '$':
                    caretIndex = sb.Length;
                    break;
                case '<':
                    if (i + 1 < content.Length)
                    {
                        if (content[i + 1] == '-')
                        {
                            selectionStart = sb.Length;
                            i++;
                            break;
                        }
                    }
                    goto default;
                case '-':
                    if (i + 1 < content.Length)
                    {
                        var next = content[i + 1];
                        if (next == '>')
                        {
                            selectionEnd = sb.Length;
                            i++;
                            break;
                        }
                    }
                    goto default;
                default:
                    sb.Append(ch);
                    break;
                }
            }
            var editor = new TextEditorData();
            editor.Text = sb.ToString();
            if (options != null)
                editor.Options = options;

            if (caretIndex >= 0)
                editor.Caret.Offset = caretIndex;
            if (selectionStart >= 0)
            {
                if (caretIndex == selectionStart)
                {
                    editor.SetSelection(selectionEnd, selectionStart);
                }
                else {
                    editor.SetSelection(selectionStart, selectionEnd);
                    if (caretIndex < 0)
                        editor.Caret.Offset = selectionEnd;
                }
            }
            return editor;
        }

        public static void Check(TextEditorData editor, string content)
        {
            var checkDocument = Create(content);
            if (checkDocument.Text != editor.Text)
            {
                Console.WriteLine("was:");
                Console.WriteLine(editor.Text);
                Console.WriteLine("expected:");
                Console.WriteLine(checkDocument.Text);
            }
            Assert.AreEqual(checkDocument.Text, editor.Text);
            Assert.AreEqual(checkDocument.Caret.Offset, editor.Caret.Offset, "Caret offset mismatch.");
            if (editor.IsSomethingSelected || checkDocument.IsSomethingSelected)
                Assert.AreEqual(checkDocument.SelectionRange, editor.SelectionRange, "Selection mismatch.");
        }
    }
}
