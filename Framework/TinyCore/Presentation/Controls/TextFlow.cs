////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class TextFlow : UIElement
    {
        public TextRunCollection TextRuns;

        internal class TextLine
        {
            public const int DefaultLineHeight = 10;

            public TextRun[] Runs;
            public int Baseline;
            public int Height;
            private int _width;

            public TextLine(ArrayList runs, int height, int baseline)
            {
                Runs = (TextRun[])runs.ToArray(typeof(TextRun));
                this.Baseline = baseline;
                this.Height = height;
            }

            // Empty line with specified height
            public TextLine(int height)
            {
                Runs = new TextRun[0];
                this.Height = height;
            }

            public int Width
            {
                get
                {
                    if (_width == 0)
                    {
                        int lineWidth = 0;
                        int width, height;
                        for (int i = Runs.Length - 1; i >= 0; i--)
                        {
                            Runs[i].GetSize(out width, out height);
                            lineWidth += width;
                        }

                        _width = lineWidth;
                    }

                    return _width;
                }
            }
        }

        internal ArrayList _lineCache;
        internal TextAlignment _alignment = TextAlignment.Left;
        internal int _currentLine;

        internal ScrollingStyle _scrollingStyle = ScrollingStyle.LineByLine;

        public TextFlow()
        {
            TextRuns = new TextRunCollection(this);
        }

        public ScrollingStyle ScrollingStyle
        {
            get
            {
                return _scrollingStyle;
            }

            set
            {
                VerifyAccess();

                if (value < ScrollingStyle.First || value > ScrollingStyle.Last)
                {
                    throw new ArgumentOutOfRangeException("ScrollingStyle", "Invalid Enum");
                }

                _scrollingStyle = value;
            }
        }

        public TextAlignment TextAlignment
        {
            get
            {
                return _alignment;
            }

            set
            {
                VerifyAccess();

                _alignment = value;
                Invalidate();
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredWidth = availableWidth;
            desiredHeight = availableHeight;

            if (availableWidth > 0)
            {
                _lineCache = SplitLines(availableWidth);

                // Compute total desired height
                //
                int totalHeight = 0;
                for (int lineNumber = _lineCache.Count - 1; lineNumber >= 0; --lineNumber)
                {
                    totalHeight += ((TextLine)_lineCache[lineNumber]).Height;
                }

                desiredHeight = totalHeight;
            }
        }

        internal bool LineScroll(bool up)
        {
            if (_lineCache == null) return false;

            if (up && _currentLine > 0)
            {
                _currentLine--;
                Invalidate();
                return true;
            }
            else if (!up && _currentLine < _lineCache.Count - 1)
            {
                _currentLine++;
                Invalidate();
                return true;
            }

            return false;
        }

        internal bool PageScroll(bool up)
        {
            if (_lineCache == null) return false;

            int lineNumber = _currentLine;
            int nLines = _lineCache.Count;
            int pageHeight = _renderHeight;
            int heightOfLines = 0;

            if (up)
            {
                // Determine first line of previous page
                //
                while (lineNumber > 0)
                {
                    lineNumber--;
                    TextLine line = (TextLine)_lineCache[lineNumber];
                    heightOfLines += line.Height;
                    if (heightOfLines > pageHeight)
                    {
                        lineNumber++;
                        break;
                    }
                }
            }
            else
            {
                // Determine first line of next page
                //
                while (lineNumber < nLines)
                {
                    TextLine line = (TextLine)_lineCache[lineNumber];
                    heightOfLines += line.Height;
                    if (heightOfLines > pageHeight)
                    {
                        break;
                    }

                    lineNumber++;
                }

                if (lineNumber == nLines) lineNumber = nLines - 1;
            }

            if (_currentLine != lineNumber)
            {
                _currentLine = lineNumber;
                Invalidate();
                return true;
            }
            else
            {
                return false;
            }
        }

        // Given an available width, takes the TextRuns and arranges them into
        // separate lines, breaking where possible at whitespace.
        //
        internal ArrayList SplitLines(int availableWidth)
        {
            Debug.Assert(availableWidth > 0);

            int lineWidth = 0;

            ArrayList remainingRuns = new ArrayList();
            for (int i = 0; i < TextRuns.Count; i++)
            {
                remainingRuns.Add(TextRuns[i]);
            }

            ArrayList lineCache = new ArrayList();
            ArrayList runsOnCurrentLine = new ArrayList();

            while (remainingRuns.Count > 0)
            {
                bool newLine = false;

                TextRun run = (TextRun)remainingRuns[0];
                remainingRuns.RemoveAt(0);

                if (run.IsEndOfLine)
                {
                    newLine = true;
                }
                else
                {
                    // Add run to end of current line
                    //
                    int runWidth, runHeight;
                    run.GetSize(out runWidth, out runHeight);
                    lineWidth += runWidth;
                    runsOnCurrentLine.Add(run);

                    // If the line length now extends beyond the available width, attempt to break the line
                    //
                    if (lineWidth > availableWidth)
                    {
                        bool onlyRunOnCurrentLine = (runsOnCurrentLine.Count == 1);

                        if (run.Text.Length > 1)
                        {
                            runsOnCurrentLine.Remove(run);

                            TextRun run1, run2;
                            if (run.Break(runWidth - (lineWidth - availableWidth), out run1, out run2, onlyRunOnCurrentLine))
                            {
                                // Break and put overflow on next line
                                //
                                if (run1 != null)
                                {
                                    runsOnCurrentLine.Add(run1);
                                }

                                if (run2 != null)
                                {
                                    remainingRuns.Insert(0, run2);
                                }
                            }
                            else if (!onlyRunOnCurrentLine)
                            {
                                // No break found - put it on its own line
                                //
                                remainingRuns.Insert(0, run);
                            }
                        }
                        else // run.Text.Length == 1
                        {
                            if (!onlyRunOnCurrentLine)
                            {
                                runsOnCurrentLine.Remove(run);
                                remainingRuns.Insert(0, run);
                            }
                        }

                        newLine = true;
                    }

                    if (lineWidth >= availableWidth || remainingRuns.Count == 0)
                    {
                        newLine = true;
                    }
                }

                // If we're done with this line, add it to the list
                //
                if (newLine)
                {
                    int lineHeight = 0;
                    int baseLine = 0;
                    int nRuns = runsOnCurrentLine.Count;
                    if (nRuns > 0)
                    {
                        // Compute line height & baseline
                        for (int i = 0; i < nRuns; i++)
                        {
                            Font font = ((TextRun)runsOnCurrentLine[i]).Font;
                            int h = font.Height + font.ExternalLeading;
                            if (h > lineHeight)
                            {
                                lineHeight = h;
                                baseLine = font.Ascent;
                            }
                        }

                        // Add line to cache
                        lineCache.Add(new TextLine(runsOnCurrentLine, lineHeight, baseLine));
                    }
                    else
                    {
                        // Empty line. Just borrow the height from the previous line, if any
                        lineHeight = (lineCache.Count) > 0 ?
                            ((TextLine)lineCache[lineCache.Count - 1]).Height :
                            TextLine.DefaultLineHeight;
                        lineCache.Add(new TextLine(lineHeight));
                    }

                    // Move onto next line
                    //
                    runsOnCurrentLine.Clear();
                    lineWidth = 0;
                }
            }

            return lineCache;
        }

        protected override void OnButtonDown(Microsoft.SPOT.Input.ButtonEventArgs e)
        {
            if (e.Button == Button.VK_UP || e.Button == Button.VK_DOWN)
            {
                bool isUp = (e.Button == Button.VK_UP);
                switch (_scrollingStyle)
                {
                    case ScrollingStyle.PageByPage:
                        e.Handled = PageScroll(isUp);
                        break;
                    case ScrollingStyle.LineByLine:
                        e.Handled = LineScroll(isUp);
                        break;
                    default:
                        Debug.Assert(false, "Unknown ScrollingStyle");
                        break;
                }
            }
        }

        public override void OnRender(Media.DrawingContext dc)
        {
            if (_lineCache == null || _lineCache.Count == 0)
            {
                return;
            }

            int nLines = _lineCache.Count;
            int top = 0;

            int width, height;
            GetRenderSize(out width, out height);

            // Draw each line of Text
            //
            int lineNumber = _currentLine;
            while (lineNumber < nLines)
            {
                TextLine line = (TextLine)_lineCache[lineNumber];
                if (top + line.Height > height)
                {
                    break;
                }

                TextRun[] runs = line.Runs;

                int x;
                switch (_alignment)
                {
                    case TextAlignment.Left:
                        x = 0;
                        break;

                    case TextAlignment.Center:
                        x = (width - line.Width) >> 1; // >> 1 is the same as div by 2
                        break;

                    case TextAlignment.Right:
                        x = width - line.Width;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                for (int i = 0; i < runs.Length; i++)
                {
                    TextRun run = runs[i];
                    int w, h;
                    run.GetSize(out w, out h);
                    int y = top + line.Baseline - run.Font.Ascent;
                    dc.DrawText(run.Text, run.Font, run.ForeColor, x, y);
                    x += w;
                }

                top += line.Height;
                lineNumber++;
            }
        }

        public int TopLine
        {
            get
            {
                return _currentLine;
            }

            set
            {
                VerifyAccess();

                Object temp = _lineCache[value]; // Easy way to make sure _lineCache is valid and value is within range

                _currentLine = value;
                Invalidate();
            }
        }

        public int LineCount
        {
            get
            {
                return _lineCache.Count; // if _lineCache is null, it'll throw a NullReferenceException
            }
        }
    }
}


