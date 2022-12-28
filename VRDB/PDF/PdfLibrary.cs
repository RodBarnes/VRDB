using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace VRDB.PDF
{
    public static class PdfLibrary
    {
        /// <summary>
        /// This is a content agnostic extension method that expects a tabular PDF document and 
        /// returns a list of text lines found in the PDF.  It uses the provided array of header
        /// values to match and identify the X position of each column for parsing the data lines.
        /// </summary>
        /// <param name="filename">full path to the PDF file</param>
        /// <param name="headers">string array of header values</param>
        /// <returns>List of PdfTextLine objects</returns>
        public static List<PdfTextLine> ReadTextLines(this PdfDocument document, string[] headers)
        {
            var list = new List<PdfTextLine>();

            var buffer = new StringBuilder();
            var firstX = 0D;
            var lastY = 0D;
            var pageNo = 0;
            var colX = new double[headers.Length];
            var colPtr = -1;
            var captureX = 0;

            foreach (var page in document.GetPages())
            {
                pageNo++;
                int lineNo = 1;
                foreach (var letter in page.Letters)
                {
                    // This first section determines how to parse the following data lines
                    // It assumes the first line is a header that defines the column position locations
                    // for data elements in the other lines
                    if (firstX == 0)
                    {
                        // Capture the X of the first letter as the basis for the start of the line
                        firstX = letter.Location.X;
                    }

                    if (colPtr < headers.Length - 1)
                    {
                        // Progressively build the header value and check it against the header tokens
                        // capturing the index when there is a match
                        if (letter.Location.Y < lastY)
                        {
                            // If Y shifts up, assume wrapped data and append a space
                            buffer.Append(" ");
                        }
                        buffer.Append(letter.Value);
                        lastY = letter.Location.Y;
                        var check = buffer.ToString();
                        var idx = Array.IndexOf(headers, check);
                        if (idx != -1)
                        {
                            // Once the header value matches, we know the next letter is going to be the
                            // start of the next header field.  Set the pointer to the current field position
                            // and set up to capture the X of the next letter as the start of the next field
                            buffer.Clear();
                            colPtr = idx;
                            captureX = colPtr + 1;
                        }
                        else if (captureX > colPtr)
                        {
                            // Capture the X as the start of the next field position
                            // Then set the capture position to the same so we switch back to building
                            // the mext field value
                            colX[captureX] = letter.Location.X;
                            captureX = colPtr;
                        }
                    }
                    else
                    {
                        // Read through each successive data line, building each data field using the X positions
                        // captured from the header fields to identify when the next data field begins
                        // Upon a new line, so process the current data in the buffer before proceeding to next line
                        if (buffer.Length > 0)
                        {
                            if (letter.Location.X == firstX)
                            {
                                var line = buffer.ToString();
                                var delCnt = line.Split('|').Length;
                                if (delCnt == 1)
                                {
                                    // Name must be wrapped so append a space and continue building
                                    buffer.Append(" ");
                                }
                                else
                                {
                                    // Exclude header lines and other non-data lines
                                    if (delCnt == headers.Length && line.Split('|')[0] != headers[0])
                                    {
                                        // Add the line
                                        list.Add(new PdfTextLine(pageNo, lineNo++, buffer.ToString()));
                                    }
                                    // Reset the buffer
                                    buffer.Clear();
                                }
                            }
                            else if (letter.Location.Y < lastY)
                            {
                                // If Y shifts up, assume wrapped data and append a space
                                buffer.Append(" ");
                            }
                            else if (colX.Contains(letter.Location.X))
                            {
                                // Append a pipe delimiter before starting the next field
                                buffer.Append("|");
                            }
                        }
                        // Append the letter
                        buffer.Append(letter.Value);
                        lastY = letter.Location.Y;
                    }
                }
            }

            if (list.Count == 0)
            {
                throw new PdfDocumentFormatException($"No lines found in report or report header was incorrect.");
            }

            return list;
        }

        /// <summary>
        /// Provided just for analysis of PDF document content
        /// </summary>
        /// <param name="document"></param>
        /// <param name="filePath"></param>
        public static void WriteText(this PdfDocument document, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                var firstX = 0D;
                var pageNo = 0;
                var sb = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    pageNo++;
                    var lineNo = 1;
                    foreach (var letter in page.Letters)
                    {
                        if (firstX == 0)
                        {
                            // Get it the from the first letter
                            firstX = letter.Location.X;
                        }
                        else if (letter.Location.X == firstX)
                        {
                            // If back at start, write out the buffer and start a new one
                            writer.WriteLine($"[{pageNo:00},{lineNo++:00}]{sb}");
                            sb.Clear();
                        }
                        sb.Append($"{letter.Value}");
                    }
                }
            }
        }
    }
}


