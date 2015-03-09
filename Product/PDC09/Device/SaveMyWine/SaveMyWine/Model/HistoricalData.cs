using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    internal class HistoricalData
    {
        protected HistoricalData()
        {
            if ((_ewrTemperatureRange == null) || (_ewrTemperatureRange.Target == null))
            {
                _ewrTemperatureRange = ExtendedWeakReference.RecoverOrCreate( typeof( Range ), 0, ExtendedWeakReference.c_SurvivePowerdown );
                _ewrTemperatureRange.Priority = (int)ExtendedWeakReference.PriorityLevel.Important;
                _ewrTemperatureRange.PushBackIntoRecoverList();

                if (_ewrTemperatureRange.Target == null)
                {
                    _ewrTemperatureRange.Target = new Range(WineDataModel.MinimumTemperatureAllowed, WineDataModel.MaximumTemperatureAllowed);
                }
            }

            if((_ewrHumidityRange == null) || (_ewrHumidityRange.Target == null))
            {
                _ewrHumidityRange = ExtendedWeakReference.RecoverOrCreate(typeof(Range), 1, ExtendedWeakReference.c_SurvivePowerdown);
                _ewrHumidityRange.Priority = (int)ExtendedWeakReference.PriorityLevel.Important;
                _ewrHumidityRange.PushBackIntoRecoverList();

                if (_ewrHumidityRange.Target == null)
                {
                    _ewrHumidityRange.Target = new Range(WineDataModel.MinimumHumidityAllowed, WineDataModel.MaximumHumidityAllowed);
                }
            }
        }

        public static HistoricalData GetDataInstance()
        {
            if(_data == null)
            {
                lock(SyncObject)
                {
                    if(_data == null)
                    {
                        _data = new HistoricalData();
                    }
                }
            }

            return _data;
        }

        public Range TemperatureRange
        {
            get
            {
                Range range = (Range)_ewrTemperatureRange.Target;

                if (range == null)
                {
                    _data = null; _data = new HistoricalData();

                    range = (Range)_ewrTemperatureRange.Target;
                }

                return range;
            }
            set
            {
                _ewrTemperatureRange.Target = value;
            }
        }

        public Range HumidityRange
        {
            get
            {
                Range range = (Range)_ewrHumidityRange.Target;

                if (range == null)
                {
                    _data = null; _data = new HistoricalData();

                    range = (Range)_ewrHumidityRange.Target;
                }

                return range;
            }
            set
            {
                _ewrHumidityRange.Target = value;
            }
        }

        internal void Reset()
        {
            HistoricalData.GetDataInstance().TemperatureRange = new Range(WineDataModel.MinimumTemperatureAllowed, WineDataModel.MaximumTemperatureAllowed);
            HistoricalData.GetDataInstance().HumidityRange = new Range(WineDataModel.MinimumHumidityAllowed, WineDataModel.MaximumHumidityAllowed);
        }

        //--//

        private ExtendedWeakReference _ewrTemperatureRange;
        private ExtendedWeakReference _ewrHumidityRange;

        //--//
        
        private static HistoricalData _data;
        private static object SyncObject = new object(); 
    }
}