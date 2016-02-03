using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public abstract class Command
    {
        public bool TakeArgument { get; protected set; }
        public bool SecondaryCount { get; protected set; }
        public int MinCount { get; protected set; }
        protected Mode RequestedMode { private get; set; }

        protected TextEditorData Editor { get; private set; }
        protected int Count { get; private set; }
        protected char Argument { get; private set; }

        protected Command(TextEditorData editor)
        {
            TakeArgument = false;
            SecondaryCount = false;
            MinCount = 1;
            Editor = editor;
        }

        public Mode RunCommand(int count, char arg)
        {
            Count = Math.Max(MinCount, count);
            Argument = arg;
            RequestedMode = Mode.None;

            Run();

            return RequestedMode;
        }

        protected abstract void Run();
    }
}

