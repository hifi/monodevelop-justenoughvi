using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.SourceEditor;

namespace JustEnoughVi
{
    public class JustEnoughViExtension : TextEditorExtension
    {
        public JustEnoughViExtension()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var textEditor = Document.GetContent<SourceEditorView>().TextEditor;
            textEditor.CurrentMode = new ViEditMode(Document);
        }
    }
}

