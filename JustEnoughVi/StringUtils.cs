using System;
namespace JustEnoughVi
{
    public class StringUtils
    {
        private StringUtils() { }

        static bool IsCodePunctuation(char c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                case ';':
                case ':':
                case ',':
                case '.':
                case '"':
                case '\'':
                    return true;
            }

            return false;
        }

        public static int NextWordOffset(string searchText, int offset)
        {
            int endOffset = offset;

            if (IsCodePunctuation(searchText[offset]))
            {
                while (endOffset < searchText.Length && IsCodePunctuation(searchText[endOffset]))
                    endOffset++;
            }
            else
            {
                while (endOffset < searchText.Length && !Char.IsWhiteSpace(searchText[endOffset]) && !IsCodePunctuation(searchText[endOffset]))
                    endOffset++;
            }

            if (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset]))
            {
                while (endOffset < searchText.Length && (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset])))
                    endOffset++;
            }

            return endOffset;
        }

        public static int PreviousWordOffset(string searchText, int offset)
        {
            int endOffset = offset - 1;

            if (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset]))
            {
                while (endOffset > 0 && (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset])))
                    endOffset--;
            }

            if (IsCodePunctuation(searchText[endOffset]))
            {
                while (endOffset > 0 && IsCodePunctuation(searchText[endOffset]))
                    endOffset--;
            }
            else
            {
                while (endOffset > 0 && !Char.IsWhiteSpace(searchText[endOffset]) && !IsCodePunctuation(searchText[endOffset]))
                    endOffset--;
            }

            return endOffset + 1;
        }
    }
}

