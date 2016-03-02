using System;
using System.Collections.Generic;

namespace JustEnoughVi
{
    public class StringUtils
    {
        private StringUtils() { }

        // FIXME: make configurable
        static readonly List<char> nonWordChars = new List<char>(new char[]{ '(', ')', '[', ']', '{', '}', '<', '>', ';', ':', ',', '.', '"', '\'' });

        public static int NextWordOffset(string searchText, int offset)
        {
            if (offset >= searchText.Length)
                return searchText.Length;

            int endOffset = offset;

            if (nonWordChars.Contains(searchText[offset]))
            {
                while (endOffset < searchText.Length && nonWordChars.Contains(searchText[endOffset]))
                    endOffset++;
            }
            else
            {
                while (endOffset < searchText.Length && !Char.IsWhiteSpace(searchText[endOffset]) && !nonWordChars.Contains(searchText[endOffset]))
                    endOffset++;
            }

            if (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset]))
            {
                while (endOffset < searchText.Length && (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset])))
                    endOffset++;
            }

            return endOffset;
        }

        public static int WordEndOffset(string searchText, int offset)
        {
            if (offset >= searchText.Length)
                return searchText.Length;

            int endOffset = offset;

            if (nonWordChars.Contains(searchText[offset]))
            {
                if (searchText.Length > offset + 1 && !nonWordChars.Contains(searchText[offset + 1]))
                    endOffset = StringUtils.NextWordOffset(searchText, offset);
                else
                {
                    while (endOffset < searchText.Length && nonWordChars.Contains(searchText[endOffset]))
                        endOffset++;
                }
            }
            else if (Char.IsWhiteSpace(searchText[offset]))
                endOffset = StringUtils.NextWordOffset(searchText, offset) + 1;
            else if (searchText.Length > offset + 1 && (Char.IsWhiteSpace(searchText[offset + 1]) || nonWordChars.Contains(searchText[offset + 1])))
                endOffset = StringUtils.NextWordOffset(searchText, offset) + 1;
            while (endOffset < searchText.Length && !Char.IsWhiteSpace(searchText[endOffset]) && !nonWordChars.Contains(searchText[endOffset]))
                endOffset++;
            return --endOffset;
        }

        public static int PreviousWordOffset(string searchText, int offset)
        {
            int endOffset = offset - 1;

            if (endOffset <= 0)
                return 0;

            if (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset]))
            {
                while (endOffset > 0 && (Char.IsWhiteSpace(searchText[endOffset]) || Char.IsControl(searchText[endOffset])))
                    endOffset--;
            }

            if (nonWordChars.Contains(searchText[endOffset]))
            {
                while (endOffset > 0 && nonWordChars.Contains(searchText[endOffset]))
                    endOffset--;
            }
            else
            {
                while (endOffset > 0 && !Char.IsWhiteSpace(searchText[endOffset]) && !nonWordChars.Contains(searchText[endOffset]))
                    endOffset--;
            }

            return endOffset + 1;
        }

        public static int FindNextInLine(string searchText, int offset, char c)
        {
            do
            {
                offset++;
                if (offset == searchText.Length || Char.IsControl(searchText[offset]))
                    return -1;
            } while (searchText[offset] != c);

            return offset;
        }

        public static int FindPreviousInLine(string searchText, int offset, char c)
        {
            do
            {
                offset--;
                if (offset == 0 || Char.IsControl(searchText[offset]))
                    return -1;
            } while (searchText[offset] != c);

            return offset;
        }
    }
}


