using System;
using Mono.TextEditor;

namespace JustEnoughVi
{
    public struct CommandRange
    {
        public CommandRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; }
        public int End { get; }

        public int Length => End - Start;

        public static readonly CommandRange Empty = new CommandRange(-1, -1);

        // TODO: should override Equals, GetHashCode and implement IEquatable

        public static bool operator ==(CommandRange x, CommandRange y)
        {
            return x.Start == y.Start && x.End == y.End;
        }

        public static bool operator !=(CommandRange x, CommandRange y)
        {
            return !(x == y);
        }
    }

}
