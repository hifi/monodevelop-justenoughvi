// 
// TextEditorTestBase.cs
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Core.Text;
using JustEnoughVi;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughViTests
{
    public class TextEditorTestBase
    {
        static bool firstRun = true;

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            if (firstRun)
            {
                Gtk.Application.Init();
                var x = GuiUnit.TestRunner.ExitCode;// hack to get GuiUnit into the AppDomain
                firstRun = false;
            }
        }

        [TestFixtureTearDown]
        public virtual void TearDown()
        {
        }

        public void ProcessKeys(string keyPresses, ViMode mode)
        {
            foreach (var c in keyPresses)
            {
                // TODO: modifier parsing
                var descriptor = KeyDescriptor.FromGtk(Gdk.Key.a /* important? */, c, Gdk.ModifierType.None);
                mode.KeyPress(descriptor);
            }
        }

        public static TextEditor Create(string content, MonoDevelop.Ide.Editor.ITextEditorOptions options = null)
        {

            var sb = new StringBuilder();
            int caretIndex = -1, selectionStart = -1, selectionEnd = -1;
            var foldSegments = new List<FoldSegment>();
            var foldStack = new Stack<FoldSegment>();

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
                        //if (next == '[') {
                        //    var segment = new FoldSegment (data.Document, "...", sb.Length, 0, FoldingType.None);
                        //    segment.IsFolded = false;
                        //    foldStack.Push (segment);
                        //    i++;
                        //    break;
                        //}
                    }
                    goto default;
                //case '+':
                //if (i + 1 < content.Length) {
                //    var next = content [i + 1];
                //    if (next == '[') {
                //        var segment = new FoldSegment (data.Document, "...", sb.Length, 0, FoldingType.None);
                //        segment.IsFolded = true;
                //        foldStack.Push (segment);
                //        i++;
                //        break;
                //    }
                //}
                //goto default;
                //case ']':
                //if (foldStack.Count > 0) {
                //    FoldSegment segment = foldStack.Pop ();
                //    segment.Length = sb.Length - segment.Offset;
                //    foldSegments.Add (segment);
                //    break;
                //}
                //goto default;
                default:
                    sb.Append(ch);
                    break;
                }
            }
            var doc = TextEditorFactory.CreateNewReadonlyDocument(new StringTextSource(sb.ToString()), "test.cs", "text/csharp");
            var data = MonoDevelop.Ide.Editor.TextEditorFactory.CreateNewEditor(doc);
            //var data = new TextEditorData ();
            if (options != null)
                data.Options = options;
            //data.Text = sb.ToString ();

            if (caretIndex >= 0)
                data.CaretOffset = caretIndex;
            if (selectionStart >= 0)
            {
                if (caretIndex == selectionStart)
                {
                    data.SetSelection(selectionEnd, selectionStart);
                }
                else {
                    data.SetSelection(selectionStart, selectionEnd);
                    if (caretIndex < 0)
                        data.CaretOffset = selectionEnd;
                }
            }
            //if (foldSegments.Count > 0)
            //    data.Document.UpdateFoldSegments (foldSegments);
            return data;
        }

        public static void Check(TextEditor data, string content)
        {
            var checkDocument = Create(content);
            if (checkDocument.Text != data.Text)
            {
                Console.WriteLine("was:");
                Console.WriteLine(data.Text);
                Console.WriteLine("expected:");
                Console.WriteLine(checkDocument.Text);
            }
            Assert.AreEqual(checkDocument.Text, data.Text);
            Assert.AreEqual(checkDocument.CaretOffset, data.CaretOffset, "Caret offset mismatch.");
            if (data.IsSomethingSelected || checkDocument.IsSomethingSelected)
                Assert.AreEqual(checkDocument.SelectionRange, data.SelectionRange, "Selection mismatch.");
            //if (checkDocument.Document.HasFoldSegments || data.Document.HasFoldSegments) {
            //    var list1 = new List<FoldSegment> (checkDocument.Document.FoldSegments);
            //    var list2 = new List<FoldSegment> (data.Document.FoldSegments);
            //    Assert.AreEqual (list1.Count, list2.Count, "Fold segment count mismatch.");
            //    for (int i = 0; i < list1.Count; i++) {
            //        Assert.AreEqual (list1 [i].Segment, list2 [i].Segment, "Fold " + i + " segment mismatch.");
            //        Assert.AreEqual (list1 [i].IsFolded, list2 [i].IsFolded, "Fold " + i + " isFolded mismatch.");
            //    }
            //}
        }
    }
}
