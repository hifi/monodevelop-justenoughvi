using Mono.TextEditor;

namespace JustEnoughVi
{
    public abstract class BaseEditMode : EditMode
    {
        protected ViEditMode Vi { get; set; }

        public BaseEditMode(ViEditMode vi)
        {
            Vi = vi;
        }

        public abstract void InternalActivate(TextEditor editor, TextEditorData data);
        public abstract void InternalDeactivate(TextEditor editor, TextEditorData data);
    }
}

