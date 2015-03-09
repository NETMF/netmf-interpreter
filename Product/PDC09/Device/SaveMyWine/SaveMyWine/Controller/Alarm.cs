using System;
using System.Runtime.CompilerServices;
using System.Threading;
//using ChipworkX.Hardware;
using Microsoft.SPOT.Hardware;

using Microsoft.SPOT;

//--//

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public class Alarm
    {
        private readonly int c_TemperatureExceeded_Period = 100; // ms
        private readonly int c_HumidityExceeded_Period = 300; // ms
        private readonly int c_Iterations = 100;

        //--//

        private Timer _soundAlarm;
        private OutputPort _soundPin;
        private int _count;
        private bool _sounding;

        //--//

        public Alarm( Cpu.Pin pin, WineDataModel model )
        {
            _soundPin = new OutputPort( pin, false );

            _soundAlarm = new Timer( this.PulseAlarm, null, Timeout.Infinite, Timeout.Infinite );

            model.OnHumidityThresholdExceeded += new WineDataModel.HumidityThresholdExceededEventHandler( this.HumidityThresoldExceeded );
            model.OnTemperatureThresholdExceeded += new WineDataModel.TemperatureThresholdExceededEventHandler( this.TemperatureThresoldExceeded );
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Sound( int period )
        {
            if(!_sounding)
            {
                _sounding = true;
                _count = 0;
                _soundAlarm.Change( 10, period );
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            _sounding = false;
            _soundPin.Write( false );
            _soundAlarm.Change( Timeout.Infinite, Timeout.Infinite );
        }

        //--//

        private void TemperatureThresoldExceeded(object sender, WineDataModel.TemperatureThresholdExceededEvent e)
        {
            Sound( c_TemperatureExceeded_Period );
        }

        private void HumidityThresoldExceeded(object sender, WineDataModel.HumidityThresholdExceededEvent e)
        {
            Sound( c_HumidityExceeded_Period );
        }

        private void PulseAlarm( object state )
        {
            if(_count++ > c_Iterations)
            {
                Stop();
            }
            else
            {
                if(_count % 2 == 1)
                {
                    _soundPin.Write( true );
                }
                else
                {
                    _soundPin.Write( false );
                }
            }
        }
    }
}
