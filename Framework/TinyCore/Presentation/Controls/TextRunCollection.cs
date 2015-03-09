////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class TextRunCollection : ICollection
    {
        private TextFlow _textFlow;
        private ArrayList _textRuns;

        internal TextRunCollection(TextFlow textFlow)
        {
            this._textFlow = textFlow;
            _textRuns = new ArrayList();
        }

        public int Count
        {
            get
            {
                return _textRuns.Count;
            }
        }

        public int Add(string text, Font font, Color foreColor)
        {
            return Add(new TextRun(text, font, foreColor));
        }

        public int Add(TextRun textRun)
        {
            if (textRun == null)
            {
                throw new ArgumentNullException("textRun");
            }

            int result = _textRuns.Add(textRun);
            _textFlow.InvalidateMeasure();
            return result;
        }

        public void Clear()
        {
            _textRuns.Clear();
            _textFlow.InvalidateMeasure();
        }

        public bool Contains(TextRun run)
        {
            return _textRuns.Contains(run);
        }

        public int IndexOf(TextRun run)
        {
            return _textRuns.IndexOf(run);
        }

        public void Insert(int index, TextRun run)
        {
            _textRuns.Insert(index, run);
            _textFlow.InvalidateMeasure();
        }

        public void Remove(TextRun run)
        {
            _textRuns.Remove(run);
            _textFlow.InvalidateMeasure();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _textRuns.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            _textRuns.RemoveAt(index);

            _textFlow.InvalidateMeasure();
        }

        public TextRun this[int index]
        {
            get
            {
                return (TextRun)_textRuns[index];
            }

            set
            {
                _textRuns[index] = value;
                _textFlow.InvalidateMeasure();
            }
        }

        #region ICollection Members

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public void CopyTo(Array array, int index)
        {
            _textRuns.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _textRuns.GetEnumerator();
        }

        #endregion
    }
}


