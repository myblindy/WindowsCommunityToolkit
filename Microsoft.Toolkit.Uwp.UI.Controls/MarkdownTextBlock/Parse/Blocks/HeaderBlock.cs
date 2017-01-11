﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Helpers;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Parse.Elements
{
    /// <summary>
    /// Represents a heading.
    /// </summary>
    public class HeaderBlock : MarkdownBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderBlock"/> class.
        /// </summary>
        public HeaderBlock()
            : base(MarkdownBlockType.Header)
        {
        }

        private int _headerLevel;

        /// <summary>
        /// Gets or sets the header level (1-6).  1 is the most important header, 6 is the least important.
        /// </summary>
        public int HeaderLevel
        {
            get
            {
                return this._headerLevel;
            }

            set
            {
                if (value < 1 || value > 6)
                {
                    throw new ArgumentOutOfRangeException("HeaderLevel", "The header level must be between 1 and 6 (inclusive).");
                }

                this._headerLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the contents of the block.
        /// </summary>
        public IList<MarkdownInline> Inlines { get; set; }

        /// <summary>
        /// Parses a header that starts with a hash.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location of the first hash character. </param>
        /// <param name="end"> The location of the end of the line. </param>
        /// <returns> A parsed header block, or <c>null</c> if this is not a header. </returns>
        internal static HeaderBlock ParseHashPrefixedHeader(string markdown, int start, int end)
        {
            // This type of header starts with one or more '#' characters, followed by the header
            // text, optionally followed by any number of hash characters.
            var result = new HeaderBlock();

            // Figure out how many consecutive hash characters there are.
            int pos = start;
            while (pos < end && markdown[pos] == '#' && pos - start < 6)
            {
                pos++;
            }

            result.HeaderLevel = pos - start;
            if (result.HeaderLevel == 0)
            {
                return null;
            }

            // Ignore any hashes at the end of the line.
            while (pos < end && markdown[end - 1] == '#')
            {
                end--;
            }

            // Parse the inline content.
            result.Inlines = Common.ParseInlineChildren(markdown, pos, end);
            return result;
        }

        /// <summary>
        /// Parses a two-line header.
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="firstLineStart"> The location of the start of the first line. </param>
        /// <param name="firstLineEnd"> The location of the end of the first line. </param>
        /// <param name="secondLineStart"> The location of the start of the second line. </param>
        /// <param name="secondLineEnd"> The location of the end of the second line. </param>
        /// <returns> A parsed header block, or <c>null</c> if this is not a header. </returns>
        internal static HeaderBlock ParseUnderlineStyleHeader(string markdown, int firstLineStart, int firstLineEnd, int secondLineStart, int secondLineEnd)
        {
            // This type of header starts with some text on the first line, followed by one or more
            // underline characters ('=' or '-') on the second line.
            // The second line can have whitespace after the underline characters, but not before
            // or between each character.

            // Check the second line is valid.
            if (secondLineEnd <= secondLineStart)
            {
                return null;
            }

            // Figure out what the underline character is ('=' or '-').
            char underlineChar = markdown[secondLineStart];
            if (underlineChar != '=' && underlineChar != '-')
            {
                return null;
            }

            // Read past consecutive underline characters.
            int pos = secondLineStart + 1;
            for (; pos < secondLineEnd; pos++)
            {
                char c = markdown[pos];
                if (c != underlineChar)
                {
                    break;
                }

                pos++;
            }

            // The rest of the line must be whitespace.
            for (; pos < secondLineEnd; pos++)
            {
                char c = markdown[pos];
                if (c != ' ' && c != '\t')
                {
                    return null;
                }

                pos++;
            }

            var result = new HeaderBlock();
            result.HeaderLevel = underlineChar == '=' ? 1 : 2;

            // Parse the inline content.
            result.Inlines = Common.ParseInlineChildren(markdown, firstLineStart, firstLineEnd);
            return result;
        }

        /// <summary>
        /// Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            if (Inlines == null)
            {
                return base.ToString();
            }

            return string.Join(string.Empty, Inlines);
        }
    }
}
