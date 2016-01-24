using System;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.SourceEditor;

namespace SimpleVi
{
    public class SimpleViExtension : TextEditorExtension
    {
        public SimpleViExtension()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var textEditor = Document.GetContent<SourceEditorView>().TextEditor;
            textEditor.CurrentMode = new ViEditMode(Document);

            Console.WriteLine("SimpleVi Init for " + Document.Name);
        }
    }
}

