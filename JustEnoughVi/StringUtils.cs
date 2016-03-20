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
                {
                    endOffset = StringUtils.NextWordOffset(searchText, offset);
                    if (nonWordChars.Contains(searchText[endOffset]))
                        return endOffset;
                }
                else
                {
                    while (endOffset < searchText.Length && nonWordChars.Contains(searchText[endOffset]))
                        endOffset++;
                }
            }
            else if (Char.IsWhiteSpace(searchText[offset]))
            {
                endOffset = StringUtils.NextWordOffset(searchText, offset);
                if (nonWordChars.Contains(searchText[endOffset]))
                    return endOffset;
            }
            else if (searchText.Length > offset + 1 && (Char.IsWhiteSpace(searchText[offset + 1]) || nonWordChars.Contains(searchText[offset + 1])))
            {
                endOffset = StringUtils.NextWordOffset(searchText, offset);
                if (nonWordChars.Contains(searchText[endOffset]))
                    return endOffset;
            }

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
                if (offset >= searchText.Length || searchText[offset] == '\n')
                    return -1;
            } while (searchText[offset] != c);

            return offset;
        }

        public static Tuple<int, int> FindMatchingOffsetsInLine(string searchText, int offset, char ch)
        {
            int firstOffset = offset;
            int secondOffset = offset;
            if (searchText[offset] != ch)
            {
                firstOffset = FindPreviousInLine(searchText, offset, ch);
                if (firstOffset < 0)
                    return Tuple.Create(-1, -1);

                secondOffset = FindNextInLine(searchText, offset, ch);
                if (secondOffset < 0)
                    return Tuple.Create(-1, -1);
            }
            else
            {
                int offs = offset - 1;
                int count = 0;
                while (offs >= 0 && searchText[offs] != '\n')
                {
                    if (searchText[offs] == ch)
                    {
                        if (count == 0)
                            firstOffset = offs;
                        count++;
                    }
                    offs--;
                }
                if (count % 2 != 0)
                    return Tuple.Create(firstOffset, offset);

                firstOffset = offset;
                secondOffset = FindNextInLine(searchText, offset, ch);
                if (secondOffset < 0)
                    return Tuple.Create(-1, -1);
            }
            return Tuple.Create(firstOffset, secondOffset);
        }

        public static Tuple<int, int> FindMatchingOffsets(string searchText, int offset, char start, char end)
        {
            int ignore = 0;
            int firstOffset = (searchText[offset] != end) ? offset : offset-1;
            while (firstOffset >= 0 && (ignore > 0 || searchText[firstOffset] != start))
            {
                if (searchText[firstOffset] == end)
                    ignore++;
                if (searchText[firstOffset] == start)
                    ignore--;
                firstOffset--;
            }
            if (firstOffset < 0)
                return Tuple.Create(-1, -1);

            int secondOffset = (firstOffset != offset) ? offset : offset + 1;
            while (secondOffset < searchText.Length && (ignore > 0 || searchText[secondOffset] != end))
            {
                if (searchText[secondOffset] == start)
                    ignore++;
                if (searchText[secondOffset] == end)
                    ignore--;
                secondOffset++;
            }
            if (secondOffset >= searchText.Length)
                return Tuple.Create(-1, -1);

            return Tuple.Create(firstOffset, secondOffset);
        }

        public static int FindLineEnd(string searchText, int offset)
        {
            while (offset < searchText.Length && !Char.IsControl(searchText[offset]))
                offset++;
            return offset;
        }

        public static int FindPreviousInLine(string searchText, int offset, char c)
        {
            do
            {
                offset--;
                if (offset < 0 || searchText[offset] == '\n')
                    return -1;
            } while (searchText[offset] != c);

            return offset;
        }
    }
}


