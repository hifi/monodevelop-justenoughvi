using System;
using Mono.TextEditor;
using MonoDevelop.SourceEditor;

namespace JustEnoughVi
{
    public abstract class BaseEditMode : EditMode
    {
        protected ViEditMode Vi { get; set; }

        public BaseEditMode(ViEditMode vi)
        {
            Vi = vi;
        }

        public abstract void InternalActivate(ExtensibleTextEditor editor, TextEditorData data);
        public abstract void InternalDeactivate(ExtensibleTextEditor editor, TextEditorData data);
    }
}

