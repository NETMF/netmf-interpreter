using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls
{
    public class ListBoxItem : ContentControl
    {
        public bool IsSelected
        {
            get
            {
                return (_listBox != null && _listBox.SelectedItem == this);
            }
        }

        public bool IsSelectable
        {
            get
            {
                return _isSelectable;
            }

            set
            {
                VerifyAccess();

                if (_isSelectable != value)
                {
                    _isSelectable = value;
                    if (!value && IsSelected)
                    {
                        _listBox.SelectedIndex = -1;
                    }
                }
            }
        }

        protected internal virtual void OnIsSelectedChanged(bool isSelected)
        {
        }

        internal void SetListBox(ListBox listbox)
        {
            this._listBox = listbox;
            if (IsSelected && !IsSelectable)
            {
                _listBox.SelectedIndex = -1;
            }
        }

        private bool _isSelectable = true;
        private ListBox _listBox;
    }
}


