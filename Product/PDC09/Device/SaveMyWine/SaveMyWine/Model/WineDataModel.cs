using System;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;

//--//

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public enum RadioState
    {
        Disconnected,
        Initializing,
        InitializationFailed,
        Initialized,
        Connected,
    }

    //--//

    public class WineDataModel
    {
        #region constants
        
        public const double MinimumTemperatureAllowed = 48.0; 
        public const double MaximumTemperatureAllowed = 80.0;
        public const double MinimumHumidityAllowed = 40.0;
        public const double MaximumHumidityAllowed = 70.0;

        #endregion

        #region radio state
        //--//

        internal class RadioStateTracker
        {
            public RadioStateTracker(WineDataModel model)
            {
                _model = model;
                _state = RadioState.Disconnected;
            }

            public RadioState State
            {
                get { return _state; }
                set
                {
                    bool stateChanged = false;

                    if (_state != value)
                    {
                        _state = value;

                        stateChanged = true;
                    }

                    if (stateChanged && _model.OnRadioStateChanged != null)
                    {
                        _model.OnRadioStateChanged(null, new RadioStateChangedEvent(_state));
                    }
                }
            }

            //--//

            private RadioState _state;
            private WineDataModel _model;
        }

        #endregion radio state

        #region events

        public class TimedEvent : EventArgs
        {
            public DateTime TimeOccurred;

            public TimedEvent()
            {
                TimeOccurred = DateTime.Now;
            }
        }

        public class RadioStateChangedEvent : TimedEvent
        {
            public RadioState State;

            public RadioStateChangedEvent(RadioState state)
            {
                State = state;
            }
        }

        public class SensorDataChangedEvent : TimedEvent
        {
            public double Previous;
            public double Last;
            public bool IsNewMinimum;
            public bool IsNewMaximum;

            public SensorDataChangedEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
            {
                Previous = previous;
                Last = last;
                IsNewMinimum = isNewMinimum;
                IsNewMaximum = isNewMaximum;
            }
        }

        public class TemperatureChangedEvent : SensorDataChangedEvent
        {
            public TemperatureChangedEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }

        public class HumidityChangedEvent : SensorDataChangedEvent
        {
            public HumidityChangedEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }

        public class TemperatureThresholdChangedEvent : SensorDataChangedEvent
        {
            public TemperatureThresholdChangedEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }

        public class HumidityThresholdChangedEvent : SensorDataChangedEvent
        {
            public HumidityThresholdChangedEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }

        public class TemperatureThresholdExceededEvent : SensorDataChangedEvent
        {
            public TemperatureThresholdExceededEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }
        public class HumidityThresholdExceededEvent : SensorDataChangedEvent
        {
            public HumidityThresholdExceededEvent(double previous, double last, bool isNewMinimum, bool isNewMaximum)
                : base(previous, last, isNewMinimum, isNewMaximum)
            {
            }
        }

        #endregion

        //--//

        #region delegates

        public delegate void RadioStateChangedEventHandler(object sender, RadioStateChangedEvent e);
        public delegate void TemperatureChangedEventHandler(object sender, TemperatureChangedEvent e);
        public delegate void HumidityChangedEventHandler(object sender, HumidityChangedEvent e);
        public delegate void TemperatureThresholdExceededEventHandler(object sender, TemperatureThresholdExceededEvent e);
        public delegate void HumidityThresholdExceededEventHandler(object sender, HumidityThresholdExceededEvent e);
        public delegate void TemperatureThresholdChangedEventHandler(object sender, TemperatureThresholdChangedEvent e);
        public delegate void HumidityThresholdChangedEventHandler(object sender, HumidityThresholdChangedEvent e);
        
        #endregion

        //--//

        #region state query and notifications

        public double CurrentTemperature
        {
            get
            {
                return _lastTemperature;
            }
        }
        public double CurrentHumidity
        {
            get
            {
                return _lastHumidity;
            }
        }

        public Range HistoricalTemperatureData
        {
            get { return HistoricalData.GetDataInstance().TemperatureRange; }
            set { HistoricalData.GetDataInstance().TemperatureRange = value; }
        }
        public Range HistoricalHumidityData
        {
            get { return HistoricalData.GetDataInstance().HumidityRange; }
            set { HistoricalData.GetDataInstance().HumidityRange = value; }
        }

        public Range TemperatureAlarmThresholdRange
        {
            get
            {
                return _temperatureAlarmThreshold;
            }
            set
            {
                if (OnTemperatureThresholdChanged != null)
                {
                    if (value.Maximum != _temperatureAlarmThreshold.Maximum)
                    {
                        OnTemperatureThresholdChanged(this, new TemperatureThresholdChangedEvent(_temperatureAlarmThreshold.Maximum, value.Maximum, false, true));
                    }
                    if (value.Minimum != _temperatureAlarmThreshold.Minimum)
                    {
                        OnTemperatureThresholdChanged(this, new TemperatureThresholdChangedEvent(_temperatureAlarmThreshold.Minimum, value.Minimum, true, false));
                    }
                }

                _temperatureAlarmThreshold = value;

                if (CurrentTemperature < value.Minimum || CurrentTemperature > value.Maximum)
                {
                    if (OnTemperatureThresholdExceeded != null)
                    {
                        OnTemperatureThresholdExceeded(this, new TemperatureThresholdExceededEvent(CurrentTemperature, CurrentTemperature, false, false));
                    }
                }
            }
        }

        public Range HumidityAlarmThresholdRange
        {
            get
            {
                return _humidityAlarmThreshold;
            }
            set
            {
                if (OnHumidityThresholdChanged != null)
                {
                    if (value.Maximum != _temperatureAlarmThreshold.Maximum)
                    {
                        OnHumidityThresholdChanged(this, new HumidityThresholdChangedEvent(_humidityAlarmThreshold.Maximum, value.Maximum, false, true));
                    }
                    if (value.Minimum != _temperatureAlarmThreshold.Minimum)
                    {
                        OnHumidityThresholdChanged(this, new HumidityThresholdChangedEvent(_humidityAlarmThreshold.Minimum, value.Minimum, true, false));
                    }
                }

                _humidityAlarmThreshold = value;

                if (CurrentHumidity < value.Minimum || CurrentHumidity > value.Maximum)
                {
                    if (OnHumidityThresholdExceeded != null)
                    {
                        OnHumidityThresholdExceeded(this, new HumidityThresholdExceededEvent(CurrentHumidity, CurrentHumidity, false, false));
                    }
                }
            }
        }

        public RadioState Radio
        {
            get
            {
                return _radioStateTracker.State;
            }
            set
            {
                _radioStateTracker.State = value;
            }
        }

        public event RadioStateChangedEventHandler OnRadioStateChanged;
        public event TemperatureChangedEventHandler OnTemperatureChanged;
        public event HumidityChangedEventHandler OnHumidityChanged;
        public event TemperatureThresholdExceededEventHandler OnTemperatureThresholdExceeded;
        public event HumidityThresholdExceededEventHandler OnHumidityThresholdExceeded;
        public event TemperatureThresholdChangedEventHandler OnTemperatureThresholdChanged;
        public event HumidityThresholdChangedEventHandler OnHumidityThresholdChanged;
        
        #endregion state query and notifications

        //--//

        #region state change

        public WineDataModel()
        {
            _radioStateTracker = new RadioStateTracker(this);
            _temperatureAlarmThreshold = new Range(MinimumTemperatureAllowed, MaximumTemperatureAllowed);
            _humidityAlarmThreshold = new Range(MinimumHumidityAllowed, MaximumHumidityAllowed);
            _lastTemperature = 60;
            _lastHumidity = 50;
        }

        public void UpdateTemperatureData(double temperature)
        {
            if (temperature > 0)
            {
                double lastTemperature = _lastTemperature;

                if (lastTemperature != temperature)
                {
                    _lastTemperature = temperature;
                }

                bool isNewMinimum = false;
                bool isNewMaximum = false;

                if (HistoricalTemperatureData.Minimum > temperature)
                {
                    HistoricalTemperatureData.Minimum = temperature;

                    isNewMinimum = true;
                }
                if (HistoricalTemperatureData.Maximum < temperature)
                {
                    HistoricalTemperatureData.Maximum = temperature;

                    isNewMaximum = true;
                }

                if (OnTemperatureChanged != null)
                {
                    OnTemperatureChanged(this, new TemperatureChangedEvent(lastTemperature, temperature, isNewMinimum, isNewMaximum)); ;
                }

                if (
                    (temperature < TemperatureAlarmThresholdRange.Minimum) ||
                    (temperature > TemperatureAlarmThresholdRange.Maximum)
                   )
                {
                    if (OnTemperatureThresholdExceeded != null)
                    {
                        OnTemperatureThresholdExceeded(this, new TemperatureThresholdExceededEvent(lastTemperature, temperature, isNewMinimum, isNewMaximum));
                    }
                }
            }
        }

        public void UpdateHumidityData(double humidity)
        {
            if (humidity > 0)
            {
                double lastHumidity = _lastHumidity;

                if (lastHumidity != humidity)
                {
                    _lastHumidity = humidity;
                }

                bool isNewMinimum = false;
                bool isNewMaximum = false;

                if (HistoricalHumidityData.Minimum > humidity)
                {
                    HistoricalHumidityData.Minimum = humidity;

                    isNewMinimum = true;
                }
                if (HistoricalHumidityData.Maximum < humidity)
                {
                    HistoricalHumidityData.Maximum = humidity;

                    isNewMaximum = true;
                }

                if (OnHumidityChanged != null)
                {
                    OnHumidityChanged(this, new HumidityChangedEvent(lastHumidity, humidity, isNewMinimum, isNewMaximum));
                }

                if (
                    (humidity < HumidityAlarmThresholdRange.Minimum) ||
                    (humidity > HumidityAlarmThresholdRange.Maximum)
                   )
                {
                    if (OnHumidityThresholdExceeded != null)
                    {
                        OnHumidityThresholdExceeded(this, new HumidityThresholdExceededEvent(lastHumidity, humidity, isNewMinimum, isNewMaximum));
                    }
                }
            }
        }

        public void Reset()
        {
            _lastTemperature = 60;
            _lastHumidity = 50;

            HistoricalData.GetDataInstance().Reset();

            if (OnTemperatureChanged != null)
            {
                OnTemperatureChanged(this, new TemperatureChangedEvent(_lastTemperature, _lastTemperature, true, true)); ;
            }
            if (OnHumidityChanged != null)
            {
                OnHumidityChanged(this, new HumidityChangedEvent(_lastHumidity, _lastHumidity, true, true));
            }
        }

        #endregion state change

        //--//

        #region private members

        private RadioStateTracker _radioStateTracker;
        private Range _temperatureAlarmThreshold;
        private Range _humidityAlarmThreshold;
        private double _lastTemperature;
        private double _lastHumidity;

        private object SyncObject = new object();
        
        #endregion private members
    }
}
