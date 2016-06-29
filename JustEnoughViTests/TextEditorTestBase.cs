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
        public void Test(string source, string keys, string expected, Type expectedMode)
        {
            var plugin = InitTest(source);

            ProcessKeys(keys, plugin);
            Check(plugin, expected, expectedMode);
        }

        public void Test(string source, Gdk.Key specialKey, string expected, Type expectedMode)
        {
            var plugin = InitTest(source);

            var descriptor = KeyDescriptor.FromGtk(specialKey, '\0', Gdk.ModifierType.None);
            plugin.KeyPress(descriptor);
            
            Check(plugin, expected, expectedMode);
        }

        private JustEnoughVi.JustEnoughVi InitTest(string source)
        {
            var options = new TextEditorOptions {
                TabsToSpaces = true,
            };
            var editor = Create(source, options);
            var plugin = new JustEnoughVi.JustEnoughVi();
            plugin.Initialize(editor);
            return plugin;
        }

        public void ProcessKeys(string keyPresses, JustEnoughVi.JustEnoughVi plugin)
        {
            foreach (var c in keyPresses)
            {
                // TODO: modifier parsing
                var descriptor = KeyDescriptor.FromGtk(Gdk.Key.a /* important? */, c, Gdk.ModifierType.None);
                plugin.KeyPress(descriptor);
            }
        }

        static TextEditorData Create(string content, ITextEditorOptions options = null)
        {
            var sb = new StringBuilder();
            int caretIndex = -1, selectionStart = -1, selectionEnd = -1;

            for (int i = 0; i < content.Length; i++)
            {
                var ch = content[i];
                switch (ch)
                {
                case '$':
                    caretIndex = sb.Length - 1;
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
                else
                {
                    editor.SetSelection(selectionStart, selectionEnd);
                    if (caretIndex < 0)
                        editor.Caret.Offset = selectionEnd;
                }
            }
            return editor;
        }

        static void Check(JustEnoughVi.JustEnoughVi plugin, string content, Type expectedMode)
        {
            var mode = plugin.CurrentMode;
            var editor = mode.Editor;
            var checkDocument = Create(content);
            if (checkDocument.Text != editor.Text)
            {
                Console.WriteLine("was:");
                Console.WriteLine(editor.Text);
                Console.WriteLine("expected:");
                Console.WriteLine(checkDocument.Text);
            }
            Assert.AreEqual(checkDocument.Text, editor.Text);
            Assert.That(mode, Is.TypeOf(expectedMode));
            var offsetFix = (mode is NormalMode) ? 0 : 1;
            if (checkDocument.Caret.Offset != 0)
            {
                Assert.AreEqual(checkDocument.Caret.Offset, mode.Editor.Caret.Offset - offsetFix, "Caret offset mismatch.");
            }
            if (editor.IsSomethingSelected || checkDocument.IsSomethingSelected)
                Assert.AreEqual(checkDocument.SelectionRange, editor.SelectionRange, "Selection mismatch.");
        }
    }
}
