using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;

using System.Runtime.CompilerServices;

using Microsoft.SPOT.Samples.SaveMyWine;

namespace Microsoft.SPOT.Samples.SaveMyWine.Controls
{
    public class Regulator : ContentControl
    {
        int _position;
        int _minimumPosition;
        int _maximumPosition;

        Text _positionText;
        Image _arrowUp;
        Image _arrowDown;
        bool _upSelected;
        bool _downSelected;

        DispatcherTimer _timer;

        //--//

        public Regulator(int defaultValue)
            : this(defaultValue, 0, 100)
        {
        }

        public Regulator(int defaultValue, int minimum, int maximum)
        {
            _position = defaultValue;
            _minimumPosition = minimum;
            _maximumPosition = maximum;

            StackPanel basePanel = new StackPanel(Orientation.Vertical);
            basePanel.SetMargin(2, 2, 2, 2);

            _arrowUp = new Image(Resources.GetBitmap(Resources.BitmapResources.UpArrow));
            _arrowDown = new Image(Resources.GetBitmap(Resources.BitmapResources.DnArrow));

            _positionText = new Text(Resources.GetFont(Resources.FontResources.nina48), _position.ToString());

            basePanel.Children.Add(_arrowUp);
            basePanel.Children.Add(_positionText);
            basePanel.Children.Add(_arrowDown);

            Child = basePanel;

            _timer = new DispatcherTimer( this.Dispatcher );
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, 200 );
            _timer.Tick += new EventHandler( this.OnTick );
        }

        public int Position
        {
            get
            {
                return _position;
            }
            set
            {
                int trimmedValue = value;

                // check low
                trimmedValue = trimmedValue <= _minimumPosition ? _minimumPosition : trimmedValue;
                // check high
                trimmedValue = trimmedValue >= _maximumPosition ? _maximumPosition : trimmedValue;

                _position = trimmedValue;

                _positionText.TextContent = _position.ToString();
            }
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            int xUp, yUp, xDn, yDn;

            e.GetPosition(_arrowUp, 0, out xUp, out yUp);
            e.GetPosition(_arrowDown, 0, out xDn, out yDn);

            lock (this)
            {
                _upSelected = false;
                _downSelected = false;

                if (xUp < _arrowUp.ActualWidth && yUp < _arrowUp.ActualHeight)
                {
                    _arrowUp.Bitmap = Resources.GetBitmap(Resources.BitmapResources.UpArrowOn);
                    _upSelected = true;
                }
                else if (xDn < _arrowDown.ActualWidth && yDn < _arrowDown.ActualHeight)
                {
                    _arrowDown.Bitmap = Resources.GetBitmap(Resources.BitmapResources.DnArrowOn);
                    _downSelected = true;
                }

                _timer.Start();
            }
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            bool upSelected;
            bool downSelected;
            
            _timer.Stop();

            lock(this)
            {
                upSelected = _upSelected;
                downSelected = _downSelected;

                if (upSelected)
                {
                    _arrowUp.Bitmap = Resources.GetBitmap(Resources.BitmapResources.UpArrow);
                    _upSelected = false;
                }
                else if (downSelected)
                {
                    _arrowDown.Bitmap = Resources.GetBitmap(Resources.BitmapResources.DnArrow);
                    _downSelected = false;
                }
            }

            if (upSelected) Position += 1;
            else if (downSelected) Position -= 1;
        }

        private void OnTick(object sender, EventArgs e)
        {
            bool upSelected;
            bool downSelected;

            lock(this)
            {
                upSelected = _upSelected;
                downSelected = _downSelected;
            }

            if (upSelected) Position += 1;
            else if (downSelected) Position -= 1;
        }
    }
}