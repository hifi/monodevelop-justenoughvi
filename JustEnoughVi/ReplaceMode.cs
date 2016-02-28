using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
namespace JustEnoughVi
{
    public class ReplaceMode : ViMode
    {
        public ReplaceMode(TextEditorData editor) : base(editor)
        {
        }

        #region implemented abstract members of ViMode

        protected override void Activate()
        {
            Editor.Caret.Mode = CaretMode.Block;
        }

        protected override void Deactivate()
        {
            CaretMoveActions.Left(Editor);
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            return true;
        }

        #endregion    
    }
}

